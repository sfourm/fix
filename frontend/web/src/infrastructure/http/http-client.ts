import { ApiError, type ApiErrorCode } from './api-error';

type Method = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';
type Query = Record<string, string | number | undefined | null>;

export interface HttpClientOptions {
  baseUrl: string;
  /** Token de sessão emitido pelo BFF (header Authorization). */
  getToken: () => string | null;
  /** Organização atual (header X-Organization-Id). */
  getOrganizationId: () => string | null;
  onUnauthorized: () => void;
}

export class HttpClient {
  constructor(private readonly options: HttpClientOptions) {}

  get<T>(path: string, query?: Query): Promise<T> {
    return this.request<T>('GET', path, undefined, query);
  }

  post<T>(path: string, body?: unknown): Promise<T> {
    return this.request<T>('POST', path, body);
  }

  put<T>(path: string, body?: unknown): Promise<T> {
    return this.request<T>('PUT', path, body);
  }

  patch<T>(path: string, body?: unknown): Promise<T> {
    return this.request<T>('PATCH', path, body);
  }

  delete<T = void>(path: string): Promise<T> {
    return this.request<T>('DELETE', path);
  }

  /** Envia um arquivo como corpo binário; nome e tipo vão em headers próprios (X-File-Name, X-File-Content-Type). */
  async upload<T>(path: string, file: File, query?: Query): Promise<T> {
    const headers = this.baseHeaders();
    headers['Content-Type'] = 'application/octet-stream';
    headers['X-File-Name'] = encodeURIComponent(file.name);
    if (file.type) headers['X-File-Content-Type'] = file.type;

    return this.parse<T>(await this.send(this.url(path, query), { method: 'POST', headers, body: file }), headers);
  }

  /** Baixa um arquivo gerado pelo BFF (ex.: modelo CSV). */
  async blob(path: string): Promise<Blob> {
    const headers = this.baseHeaders();
    const response = await this.send(this.url(path), { method: 'GET', headers });
    if (!response.ok) {
      await this.parse(response, headers);
    }

    return response.blob();
  }

  /**
   * Server-Sent Events pelo fetch (o EventSource não envia o header Authorization). Resolve quando a conexão fecha;
   * para encerrar antes, cancele pelo signal.
   */
  async stream(
    path: string,
    query: Query,
    onEvent: (event: string, data: string) => void,
    signal: AbortSignal,
    onOpen?: () => void,
  ): Promise<void> {
    const headers = this.baseHeaders();
    headers['Accept'] = 'text/event-stream';
    const response = await this.send(this.url(path, query), { method: 'GET', headers, signal });
    if (!response.ok || !response.body) {
      await this.parse(response, headers);
      return;
    }

    onOpen?.();
    const reader = response.body.pipeThrough(new TextDecoderStream()).getReader();
    let buffer = '';
    for (;;) {
      const { value, done } = await reader.read();
      if (done) return;

      buffer += value.replace(/\r\n/g, '\n');
      let end: number;
      while ((end = buffer.indexOf('\n\n')) >= 0) {
        const block = buffer.slice(0, end);
        buffer = buffer.slice(end + 2);

        let event = 'message';
        const data: string[] = [];
        for (const line of block.split('\n')) {
          if (line.startsWith('event:')) event = line.slice(6).trim();
          else if (line.startsWith('data:')) data.push(line.slice(5).trimStart());
        }

        if (data.length) onEvent(event, data.join('\n'));
      }
    }
  }

  private async request<T>(method: Method, path: string, body?: unknown, query?: Query): Promise<T> {
    const headers = this.baseHeaders();
    if (body !== undefined) headers['Content-Type'] = 'application/json';

    const response = await this.send(this.url(path, query), {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
    });
    return this.parse<T>(response, headers);
  }

  private baseHeaders(): Record<string, string> {
    const headers: Record<string, string> = { Accept: 'application/json' };
    const token = this.options.getToken();
    const organizationId = this.options.getOrganizationId();

    if (token) headers['Authorization'] = `Bearer ${token}`;
    if (organizationId) headers['X-Organization-Id'] = organizationId;
    return headers;
  }

  private async send(url: string, init: RequestInit): Promise<Response> {
    try {
      return await fetch(url, init);
    } catch (error) {
      if (error instanceof DOMException && error.name === 'AbortError') throw error;
      throw new ApiError(0, 'network', 'Não foi possível conectar ao servidor.');
    }
  }

  /** Lê a resposta JSON; erro vira ApiError (401 com sessão encerra a sessão). */
  private async parse<T>(response: Response, headers: Record<string, string>): Promise<T> {
    if (response.status === 204) {
      return undefined as T;
    }

    const payload = await response.json().catch(() => null);

    if (!response.ok) {
      const error = new ApiError(
        response.status,
        (payload?.code as ApiErrorCode) ?? 'internal',
        payload?.message ?? `Erro ${response.status}.`,
        payload?.fields ?? {},
      );

      if (response.status === 401 && headers['Authorization']) {
        this.options.onUnauthorized();
      }

      throw error;
    }

    return payload as T;
  }

  private url(path: string, query?: Query): string {
    const search = new URLSearchParams();
    for (const [key, value] of Object.entries(query ?? {})) {
      if (value !== undefined && value !== null && value !== '') {
        search.set(key, String(value));
      }
    }

    const qs = search.toString();
    return `${this.options.baseUrl}${path}${qs ? `?${qs}` : ''}`;
  }
}
