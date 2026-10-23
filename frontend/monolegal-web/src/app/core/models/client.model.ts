export interface Client {
  id: string;
  name: string;
  email: string;
  identification: string;
  isActive: boolean;
}

export interface CreateClientRequest {
  name: string;
  email: string;
  identification: string;
  isActive: boolean;
}

export interface CreateClientWithInvoiceRequest {
  client: CreateClientRequest;
  invoice: CreateInvoiceRequest;
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

export type InvoiceStatus =
  | 'primerrecordatorio'
  | 'segundorecordatorio'
  | 'desactivado'
  | 'aldia'
  | 'pagada';
