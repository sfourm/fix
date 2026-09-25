import { reactive } from 'vue';

interface ConfirmState {
  open: boolean;
  title: string;
  message: string;
  confirmLabel: string;
  danger: boolean;
  resolve: ((value: boolean) => void) | null;
}

const state = reactive<ConfirmState>({
  open: false,
  title: '',
  message: '',
  confirmLabel: 'Confirmar',
  danger: false,
  resolve: null,
});

/** Diálogo de confirmação global (renderizado uma vez pelo ConfirmDialog em App.vue). */
export function useConfirm() {
  function confirm(options: { title: string; message: string; confirmLabel?: string; danger?: boolean }): Promise<boolean> {
    return new Promise((resolve) => {
      Object.assign(state, {
        open: true,
        title: options.title,
        message: options.message,
        confirmLabel: options.confirmLabel ?? 'Confirmar',
        danger: options.danger ?? false,
        resolve,
      });
    });
  }

  function settle(value: boolean) {
    state.resolve?.(value);
    Object.assign(state, { open: false, resolve: null });
  }

  return { state, confirm, settle };
}
