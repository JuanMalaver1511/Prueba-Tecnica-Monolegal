import { InvoiceStatus } from './invoice.model';

export interface ReminderAction {
  invoiceId: string;
  invoiceNumber: string;
  clientId: string;
  clientName: string;
  clientEmail: string;
  previousStatus: InvoiceStatus;
  newStatus: InvoiceStatus;
  emailSubject: string;
  succeeded: boolean;
  errorMessage: string | null;
}

export interface ReminderProcessingResult {
  clientsProcessed: number;
  invoicesEvaluated: number;
  invoicesSkipped: number;
  eligibleInvoices: number;
  emailsSent: number;
  invoicesUpdated: number;
  failures: number;
  actions: ReminderAction[];
}
