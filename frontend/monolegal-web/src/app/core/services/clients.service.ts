import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../config/api.config';
import { Client, CreateClientRequest, CreateClientWithInvoiceRequest } from '../models/client.model';
import { Invoice } from '../models/invoice.model';

@Injectable({ providedIn: 'root' })
export class ClientsService {
  private readonly http = inject(HttpClient);

  getClients(): Observable<Client[]> {
    return this.http.get<Client[]>(`${API_BASE_URL}/clients`);
  }

  createClient(request: CreateClientRequest): Observable<Client> {
    return this.http.post<Client>(`${API_BASE_URL}/clients`, request);
  }

  createClientWithInvoice(request: CreateClientWithInvoiceRequest): Observable<Invoice> {
    return this.http.post<Invoice>(`${API_BASE_URL}/clients/with-invoice`, request);
  }
}
