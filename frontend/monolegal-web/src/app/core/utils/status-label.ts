import { InvoiceStatus } from '../models/invoice.model';

export const STATUS_LABELS: Record<InvoiceStatus, string> = {
  primerrecordatorio: 'Primer recordatorio',
  segundorecordatorio: 'Segundo recordatorio',
  desactivado: 'Desactivado',
  aldia: 'Al día',
  pagada: 'Pagada'
};

export function statusLabel(status: InvoiceStatus): string {
  return STATUS_LABELS[status] ?? status;
}

export function errorMessage(error: unknown): string {
  if (typeof error === 'object' && error !== null) {
    const candidate = error as { error?: { detail?: string; title?: string }; message?: string };
    return candidate.error?.detail ?? candidate.error?.title ?? candidate.message ?? 'Error inesperado.';
  }

  return 'Error inesperado.';
}
