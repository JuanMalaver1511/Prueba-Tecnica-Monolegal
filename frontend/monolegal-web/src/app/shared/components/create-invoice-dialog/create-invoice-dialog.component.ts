import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { Client } from '../../../core/models/client.model';
import { InvoiceStatus } from '../../../core/models/invoice.model';

@Component({
  selector: 'app-create-invoice-dialog',
  imports: [FormsModule],
  templateUrl: './create-invoice-dialog.component.html',
  styleUrl: './create-invoice-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateInvoiceDialogComponent {
  readonly open = input.required<boolean>();
  readonly busy = input.required<boolean>();
  readonly clients = input.required<Client[]>();

  readonly submit = output<{
    mode: 'newClient' | 'existingClient';
    client: {
      name: string;
      email: string;
      identification: string;
      isActive: boolean;
    };
    invoice: {
      clientId: string;
      number: string;
      description: string;
      amount: number;
      currency: string;
      issueDate: string;
      dueDate: string;
      status: InvoiceStatus;
    };
  }>();
  readonly cancel = output<void>();

  readonly mode = signal<'newClient' | 'existingClient'>('newClient');

  readonly clientName = signal('');
  readonly clientEmail = signal('');
  readonly clientIdNumber = signal('');
  readonly clientActive = signal(true);

  readonly existingClientId = signal('');

  readonly invoiceNumber = signal('');
  readonly invoiceDescription = signal('');
  readonly invoiceAmount = signal<number | null>(null);
  readonly invoiceCurrency = signal('COP');
  readonly invoiceIssueDate = signal(this.todayIso());
  readonly invoiceDueDate = signal(this.todayIso());
  readonly invoiceStatus = signal<InvoiceStatus>('primerrecordatorio');

  readonly statusOptions: InvoiceStatus[] = [
    'primerrecordatorio',
    'segundorecordatorio',
    'desactivado',
    'aldia',
    'pagada'
  ];

  readonly canSubmit = computed(() => {
    if (this.busy()) {
      return false;
    }

    if (!this.invoiceNumber().trim() || this.invoiceAmount() === null || this.invoiceAmount()! < 0) {
      return false;
    }

    if (this.mode() === 'existingClient') {
      return !!this.existingClientId().trim();
    }

    return !!this.clientName().trim() && !!this.clientEmail().trim();
  });

  onBackdrop(): void {
    if (!this.busy()) {
      this.cancel.emit();
    }
  }

  setMode(mode: 'newClient' | 'existingClient'): void {
    this.mode.set(mode);
  }

  reset(): void {
    this.mode.set('newClient');
    this.clientName.set('');
    this.clientEmail.set('');
    this.clientIdNumber.set('');
    this.clientActive.set(true);
    this.existingClientId.set('');
    this.invoiceNumber.set('');
    this.invoiceDescription.set('');
    this.invoiceAmount.set(null);
    this.invoiceCurrency.set('COP');
    this.invoiceIssueDate.set(this.todayIso());
    this.invoiceDueDate.set(this.todayIso());
    this.invoiceStatus.set('primerrecordatorio');
  }

  submitForm(): void {
    if (!this.canSubmit()) {
      return;
    }

    const invoice = {
      clientId: this.mode() === 'existingClient' ? this.existingClientId() : '',
      number: this.invoiceNumber().trim(),
      description: this.invoiceDescription().trim(),
      amount: Number(this.invoiceAmount()),
      currency: this.invoiceCurrency().trim().toUpperCase(),
      issueDate: this.invoiceIssueDate(),
      dueDate: this.invoiceDueDate(),
      status: this.invoiceStatus()
    };

    this.submit.emit({
      mode: this.mode(),
      client: {
        name: this.clientName().trim(),
        email: this.clientEmail().trim(),
        identification: this.clientIdNumber().trim(),
        isActive: this.clientActive()
      },
      invoice
    });
  }

  private todayIso(): string {
    const now = new Date();
    const offset = now.getTimezoneOffset() * 60000;
    return new Date(now.getTime() - offset).toISOString().slice(0, 10);
  }
}
