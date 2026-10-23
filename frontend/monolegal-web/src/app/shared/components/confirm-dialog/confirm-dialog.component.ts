import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

import { Client } from '../../../core/models/client.model';
import { Invoice } from '../../../core/models/invoice.model';
import { statusLabel } from '../../../core/utils/status-label';

interface RecipientGroup {
  client: Client;
  invoices: Invoice[];
}

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ConfirmDialogComponent {
  readonly open = input.required<boolean>();
  readonly busy = input.required<boolean>();
  readonly clients = input.required<Client[]>();
  readonly invoices = input.required<Invoice[]>();
  readonly scopeLabel = input.required<string>();
  readonly confirm = output<void>();
  readonly cancel = output<void>();

  readonly pendingInvoices = computed(() => this.invoices().filter(invoice => invoice.isReminderPending));

  readonly recipients = computed<RecipientGroup[]>(() => {
    const clientsById = new Map(this.clients().map(client => [client.id, client]));
    const groups = new Map<string, RecipientGroup>();

    for (const invoice of this.pendingInvoices()) {
      const client =
        clientsById.get(invoice.clientId) ??
        ({ id: invoice.clientId, name: invoice.clientName, email: '', identification: '', isActive: true } as Client);

      const group = groups.get(client.id) ?? { client, invoices: [] };
      group.invoices.push(invoice);
      groups.set(client.id, group);
    }

    return [...groups.values()].sort((a, b) => a.client.name.localeCompare(b.client.name));
  });

  readonly recipientsCount = computed(() => this.recipients().length);
  readonly reminderCount = computed(() => this.pendingInvoices().length);

  statusLabel = statusLabel;

  onBackdrop(): void {
    if (!this.busy()) {
      this.cancel.emit();
    }
  }
}
