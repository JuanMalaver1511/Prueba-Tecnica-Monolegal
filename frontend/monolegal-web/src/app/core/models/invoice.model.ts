export type InvoiceStatus =
  | 'primerrecordatorio'
  | 'segundorecordatorio'
  | 'desactivado'
  | 'aldia'
  | 'pagada';

export interface Invoice {
  id: string;
  clientId: string;
  clientName: string;
  number: string;
  description: string;
  amount: number;
  currency: string;
  issueDate: string;
  dueDate: string;
  status: InvoiceStatus;
  statusLabel: string;
  isReminderPending: boolean;
}

export interface InvoiceStatusSummary {
  status: InvoiceStatus;
  label: string;
  count: number;
  totalAmount: number;
}

export interface InvoiceSummary {
  totalInvoices: number;
  totalAmount: number;
  currency: string;
  pendingReminders: number;
  byStatus: InvoiceStatusSummary[];
}

export interface CreateInvoiceRequest {
  clientId: string;
  number: string;
  description: string;
  amount: number;
  currency: string;
  issueDate: string;
  dueDate: string;
  status: InvoiceStatus;
}
