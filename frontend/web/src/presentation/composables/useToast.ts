import { reactive } from 'vue';

export interface Toast {
  id: number;
  kind: 'success' | 'error' | 'info';
  message: string;
}

const toasts = reactive<Toast[]>([]);
let nextId = 1;

function push(kind: Toast['kind'], message: string, timeoutMs = 4000) {
  const id = nextId++;
  toasts.push({ id, kind, message });
  setTimeout(() => dismiss(id), timeoutMs);
}

function dismiss(id: number) {
  const index = toasts.findIndex((t) => t.id === id);
  if (index >= 0) toasts.splice(index, 1);
}

export function useToast() {
  return {
    toasts,
    dismiss,
    success: (message: string) => push('success', message),
    error: (message: string) => push('error', message, 6000),
    info: (message: string) => push('info', message),
  };
}
