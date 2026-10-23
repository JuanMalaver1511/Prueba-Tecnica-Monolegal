export interface EmailPreview {
  invoiceId: string;
  invoiceNumber: string;
  clientName: string;
  toAddress: string;
  subject: string;
  htmlBody: string;
  plainTextBody: string;
  sentAtUtc: string | null;
  wasSent: boolean;
}
