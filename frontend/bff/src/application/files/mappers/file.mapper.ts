import type { FileKindSummaryDto } from '../dtos/file-kind-summary.dto.js';
import type { FileLineDto } from '../dtos/file-line.dto.js';
import type { FileTemplateDto } from '../dtos/file-template.dto.js';
import type { FileDto } from '../dtos/file.dto.js';
import type { FileKindSummaryResponse } from '../responses/file-kind-summary.response.js';
import type { FileLineResponse } from '../responses/file-line.response.js';
import type { FileTemplateResponse } from '../responses/file-template.response.js';
import type { FileResponse } from '../responses/file.response.js';

const inProgress = new Set(['Received', 'Processing']);

/** A URL interna (s3://) fica no BFF; o navegador baixa pelo link temporário. */
export const toFileResponse = ({ storageUrl: _storageUrl, ...file }: FileDto): FileResponse => ({
  ...file,
  progressPercent:
    file.totalLines === 0
      ? inProgress.has(file.status) ? 0 : 100
      : Math.floor((file.processedLines * 100) / file.totalLines),
});

export const toFileLineResponse = (dto: FileLineDto): FileLineResponse => ({ ...dto });

export const toFileKindSummaryResponse = (dto: FileKindSummaryDto): FileKindSummaryResponse => ({ ...dto });

export const toFileTemplateResponse = ({ exampleCsv: _exampleCsv, ...template }: FileTemplateDto): FileTemplateResponse => ({
  ...template,
});
