import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../config/api.config';
import { ReminderProcessingResult } from '../models/reminder.model';

@Injectable({ providedIn: 'root' })
export class RemindersService {
  private readonly http = inject(HttpClient);

  process(clientIds: string[] = []): Observable<ReminderProcessingResult> {
    return this.http.post<ReminderProcessingResult>(`${API_BASE_URL}/reminders/process`, {
      clientIds
    });
  }
}
