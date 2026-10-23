import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../config/api.config';
import { EmailPreview } from '../models/email-preview.model';
import { Invoice, InvoiceSummary } from '../models/invoice.model';

@Injectable({ providedIn: 'root' })
export class InvoicesService {
  private readonly http = inject(HttpClient);

  getInvoices(clientId?: string): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(`${API_BASE_URL}/invoices`, {
      params: this.buildParams(clientId)
    });
  }

  getSummary(clientId?: string): Observable<InvoiceSummary> {
    return this.http.get<InvoiceSummary>(`${API_BASE_URL}/invoices/summary`, {
      params: this.buildParams(clientId)
    });
  }

  getEmailPreview(invoiceId: string): Observable<EmailPreview> {
    return this.http.get<EmailPreview>(`${API_BASE_URL}/invoices/${invoiceId}/email-preview`);
  }

  private buildParams(clientId?: string): HttpParams {
    return clientId ? new HttpParams().set('clientId', clientId) : new HttpParams();
  }
}
