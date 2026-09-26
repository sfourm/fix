import express, { Router } from 'express';
import type { FileService } from '../../../application/files/services/file.service.js';
import { organizationContext } from '../request-context.js';
import { idParam, parse, schemas } from '../validation.js';

/** Headers do envio: o corpo é o arquivo (binário); nome e tipo vêm à parte. */
export const FILE_NAME_HEADER = 'x-file-name';
export const FILE_CONTENT_TYPE_HEADER = 'x-file-content-type';

/** Intervalo do comentário de keep-alive do SSE (proxies derrubam conexões ociosas). */
const HEARTBEAT_MS = 25_000;

const TEMPLATE_NAMES: Record<string, string> = {
  Users: 'usuarios',
  Policies: 'politicas',
  Mandates: 'mandatos',
  Orders: 'boletas',
};

/** /api/files */
export function fileRoutes(files: FileService): Router {
  const router = Router();

  router.get('/summary', async (req, res) => {
    res.json(await files.summary({ context: organizationContext(req) }));
  });

  router.get('/templates/:kind', async (req, res) => {
    const { kind } = parse(schemas.fileKindParam, req.params);
    res.json(await files.template({ context: organizationContext(req), kind }));
  });

  router.get('/templates/:kind/csv', async (req, res) => {
    const { kind } = parse(schemas.fileKindParam, req.params);
    const csv = await files.templateCsv({ context: organizationContext(req), kind });
    res
      .type('text/csv; charset=utf-8')
      .attachment(`modelo-${TEMPLATE_NAMES[kind] ?? kind.toLowerCase()}.csv`)
      .send(csv);
  });

  /**
   * Progresso ao vivo (Server-Sent Events). O web lê com fetch (e não EventSource) para mandar o token no header
   * Authorization, como nas demais rotas.
   */
  router.get('/events', async (req, res) => {
    const { fileId } = parse(schemas.fileEventsQuery, req.query);
    const stop = await files.watch({ context: organizationContext(req), fileId }, (event) => {
      res.write(`event: progress\ndata: ${JSON.stringify(event)}\n\n`);
    });

    res.writeHead(200, {
      'Content-Type': 'text/event-stream; charset=utf-8',
      'Cache-Control': 'no-cache, no-transform',
      Connection: 'keep-alive',
      'X-Accel-Buffering': 'no',
    });
    res.write(': conectado\n\n');

    const heartbeat = setInterval(() => res.write(': ping\n\n'), HEARTBEAT_MS);
    req.on('close', () => {
      clearInterval(heartbeat);
      stop();
    });
  });

  router.get('/', async (req, res) => {
    const query = parse(schemas.filesQuery, req.query);
    res.json(await files.list({ context: organizationContext(req), ...query }));
  });

  // O corpo é o arquivo inteiro (até 20 MB, o limite do storage-service).
  router.post('/', express.raw({ type: () => true, limit: '20mb' }), async (req, res) => {
    const input = parse(schemas.uploadFile, {
      kind: req.query.kind,
      fileName: decodeHeader(req.header(FILE_NAME_HEADER)),
      contentType: req.header(FILE_CONTENT_TYPE_HEADER) || undefined,
    });
    const content = Buffer.isBuffer(req.body) ? req.body : Buffer.alloc(0);
    res.status(201).json(await files.upload({ context: organizationContext(req), ...input, content }));
  });

  router.get('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    res.json(await files.get({ context: organizationContext(req), id }));
  });

  router.get('/:id/lines', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const query = parse(schemas.fileLinesQuery, req.query);
    res.json(await files.listLines({ context: organizationContext(req), fileId: id, ...query }));
  });

  router.get('/:id/content', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const file = await files.content({ context: organizationContext(req), id });
    res.type(file.contentType || 'application/octet-stream').attachment(file.fileName).send(file.content);
  });

  router.get('/:id/download', async (req, res) => {
    const { id } = parse(idParam, req.params);
    res.json(await files.downloadUrl({ context: organizationContext(req), id }));
  });

  return router;
}

/** O nome vai URL-encoded no header (acentos e espaços). */
function decodeHeader(value: string | undefined): string | undefined {
  if (!value) {
    return undefined;
  }

  try {
    return decodeURIComponent(value);
  } catch {
    return value;
  }
}
