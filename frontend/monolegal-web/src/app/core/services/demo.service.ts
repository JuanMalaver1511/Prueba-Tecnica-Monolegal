import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../config/api.config';

@Injectable({ providedIn: 'root' })
export class DemoService {
  private readonly http = inject(HttpClient);

  reset(): Observable<void> {
    return this.http.post<void>(`${API_BASE_URL}/demo/reset`, {});
  }
}
