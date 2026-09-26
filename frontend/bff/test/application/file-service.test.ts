import assert from 'node:assert/strict';
import { describe, it } from 'node:test';
import type { FileProgressDto } from '../../src/application/files/dtos/file-progress.dto.js';
import type { FileDto } from '../../src/application/files/dtos/file.dto.js';
import { toFileResponse } from '../../src/application/files/mappers/file.mapper.js';
import type { FileProgressListener, FileProgressPort } from '../../src/application/files/ports/file-progress.port.js';
import type { FileGateway } from '../../src/application/files/ports/file.gateway.js';
import { FileService } from '../../src/application/files/services/file.service.js';
import { AppError } from '../../src/cross-cutting/errors/app-error.js';

const ORG = 'org-1';
const context = { userId: 'user-1', organizationId: ORG };

class FakeProgress implements FileProgressPort {
  private listeners = new Set<FileProgressListener>();

  subscribe(listener: FileProgressListener): () => void {
    this.listeners.add(listener);
    return () => this.listeners.delete(listener);
  }

  emit(event: Partial<FileProgressDto>): void {
    for (const listener of this.listeners) {
      listener({ fileId: 'f1', organizationId: ORG, kind: 'Orders', ...event } as FileProgressDto);
    }
  }

  get count(): number {
    return this.listeners.size;
  }
}

// O usuário vê boletas e documentos, mas não usuários.
const gateway = {
  summary: async () => [
    { kind: 'Orders', total: 0, processing: 0, withErrors: 0, lastUploadedAt: null, canUpload: true },
    { kind: 'Documents', total: 0, processing: 0, withErrors: 0, lastUploadedAt: null, canUpload: true },
  ],
  get: async () => ({}) as FileDto,
} as unknown as FileGateway;

describe('acompanhamento ao vivo dos uploads', () => {
  it('repassa só eventos da organização e dos tipos que o usuário pode ver', async () => {
    const progress = new FakeProgress();
    const received: FileProgressDto[] = [];
    const stop = await new FileService(gateway, progress).watch({ context, fileId: null }, (e) => received.push(e));

    progress.emit({ fileId: 'a' });
    progress.emit({ fileId: 'b', organizationId: 'outra-org' });
    progress.emit({ fileId: 'c', kind: 'Users' });
    progress.emit({ fileId: 'd', kind: 'Documents' });

    assert.deepEqual(received.map((e) => e.fileId), ['a', 'd']);
    stop();
    assert.equal(progress.count, 0);
  });

  it('na tela de um arquivo, só os eventos dele', async () => {
    const progress = new FakeProgress();
    const received: string[] = [];
    await new FileService(gateway, progress).watch({ context, fileId: 'a' }, (e) => received.push(e.fileId));

    progress.emit({ fileId: 'a' });
    progress.emit({ fileId: 'b' });

    assert.deepEqual(received, ['a']);
  });

  it('sem RabbitMQ o acompanhamento responde indisponível', async () => {
    await assert.rejects(
      new FileService(gateway, null).watch({ context, fileId: null }, () => {}),
      (error: unknown) => error instanceof AppError && error.code === 'unavailable',
    );
  });
});

describe('resposta do arquivo', () => {
  const file = (patch: Partial<FileDto>): FileDto => ({
    id: 'f1',
    kind: 'Orders',
    status: 'Processing',
    fileName: 'boletas.csv',
    contentType: 'text/csv',
    sizeBytes: 10,
    organizationId: ORG,
    uploadedBy: 'user-1',
    uploadedAt: '2026-09-26T00:00:00Z',
    finishedAt: null,
    totalLines: 3,
    processedLines: 1,
    succeededLines: 1,
    failedLines: 0,
    error: null,
    storageUrl: 's3://fix-files/chave',
    ...patch,
  });

  it('calcula o percentual e não expõe a URL interna do S3', () => {
    const response = toFileResponse(file({}));
    assert.equal(response.progressPercent, 33);
    assert.equal('storageUrl' in response, false);
  });

  it('sem linhas: 0% enquanto recebido, 100% quando só armazenado', () => {
    assert.equal(toFileResponse(file({ status: 'Received', totalLines: 0, processedLines: 0 })).progressPercent, 0);
    assert.equal(toFileResponse(file({ status: 'Stored', totalLines: 0, processedLines: 0 })).progressPercent, 100);
  });
});
