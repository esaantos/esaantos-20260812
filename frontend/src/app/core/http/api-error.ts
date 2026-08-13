import { HttpErrorResponse } from '@angular/common/http';

export function extractApiErrorMessage(err: unknown, fallback = 'Erro inesperado. Tente novamente.'): string {
  if (err instanceof HttpErrorResponse) {
    const body = err.error as { error?: string } | null;
    if (body?.error) {
      return body.error;
    }
    if (err.status === 0) {
      return 'Não foi possível conectar ao servidor.';
    }
  }
  return fallback;
}
