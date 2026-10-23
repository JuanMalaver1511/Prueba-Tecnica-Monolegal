import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

import { Invoice } from '../../../../core/models/invoice.model';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-invoice-table',
  imports: [CurrencyPipe, DatePipe, StatusBadgeComponent],
  templateUrl: './invoice-table.component.html',
  styleUrl: './invoice-table.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InvoiceTableComponent {
  readonly invoices = input.required<Invoice[]>();
  readonly previewInvoice = output<Invoice>();

  isOverdue(invoice: Invoice): boolean {
    return invoice.isReminderPending && new Date(invoice.dueDate) < new Date();
  }
}
