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

  private async request<T>(method: Method, path: string, body?: unknown, query?: Query): Promise<T> {
    const headers: Record<string, string> = { Accept: 'application/json' };
    const token = this.options.getToken();
    const organizationId = this.options.getOrganizationId();

    if (token) headers['Authorization'] = `Bearer ${token}`;
    if (organizationId) headers['X-Organization-Id'] = organizationId;
    if (body !== undefined) headers['Content-Type'] = 'application/json';

    let response: Response;
    try {
      response = await fetch(this.url(path, query), {
        method,
        headers,
        body: body === undefined ? undefined : JSON.stringify(body),
      });
    } catch {
      throw new ApiError(0, 'network', 'Não foi possível conectar ao servidor.');
    }

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

      if (response.status === 401 && token) {
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
