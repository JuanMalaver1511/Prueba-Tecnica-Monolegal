import { forkJoin, finalize } from 'rxjs';
import { Component, OnInit, computed, inject, signal, ViewChild } from '@angular/core';

import { Client } from '../../core/models/client.model';
import { EmailPreview } from '../../core/models/email-preview.model';
import { Invoice, InvoiceStatus, InvoiceSummary } from '../../core/models/invoice.model';
import { ReminderProcessingResult } from '../../core/models/reminder.model';
import { ClientsService } from '../../core/services/clients.service';
import { DemoService } from '../../core/services/demo.service';
import { InvoicesService } from '../../core/services/invoices.service';
import { RemindersService } from '../../core/services/reminders.service';
import { errorMessage } from '../../core/utils/status-label';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { CreateInvoiceDialogComponent } from '../../shared/components/create-invoice-dialog/create-invoice-dialog.component';
import { EmailPreviewComponent } from '../../shared/components/email-preview/email-preview.component';
import { ClientFilterComponent } from './components/client-filter/client-filter.component';
import { InvoiceTableComponent } from './components/invoice-table/invoice-table.component';
import { ProcessResultComponent } from './components/process-result/process-result.component';
import { ProcessToolbarComponent } from './components/process-toolbar/process-toolbar.component';
import { SummaryCardsComponent } from './components/summary-cards/summary-cards.component';

@Component({
  selector: 'app-dashboard',
  imports: [
    ConfirmDialogComponent,
    CreateInvoiceDialogComponent,
    ClientFilterComponent,
    InvoiceTableComponent,
    ProcessResultComponent,
    ProcessToolbarComponent,
    SummaryCardsComponent,
    EmailPreviewComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly clientsService = inject(ClientsService);
  private readonly invoicesService = inject(InvoicesService);
  private readonly remindersService = inject(RemindersService);
  private readonly demoService = inject(DemoService);

  @ViewChild(CreateInvoiceDialogComponent) createDialog?: CreateInvoiceDialogComponent;

  readonly clients = signal<Client[]>([]);
  readonly invoices = signal<Invoice[]>([]);
  readonly summary = signal<InvoiceSummary | null>(null);
  readonly selectedClientId = signal('all');
  readonly loading = signal(true);
  readonly processing = signal(false);
  readonly resetting = signal(false);
  readonly confirming = signal(false);
  readonly creating = signal(false);
  readonly createOpen = signal(false);
  readonly result = signal<ReminderProcessingResult | null>(null);
  readonly error = signal<string | null>(null);

  readonly previewOpen = signal(false);
  readonly previewLoading = signal(false);
  readonly preview = signal<EmailPreview | null>(null);

  readonly selectedClient = computed(() =>
    this.clients().find(client => client.id === this.selectedClientId()) ?? null
  );

  readonly scopeLabel = computed(() =>
    this.selectedClientId() === 'all'
      ? 'todos los clientes'
      : (this.selectedClient()?.name ?? 'el cliente seleccionado')
  );

  readonly clientsInScope = computed(() =>
    this.selectedClientId() === 'all' ? this.clients().length : this.selectedClient() ? 1 : 0
  );

  readonly eligibleCount = computed(() => this.summary()?.pendingReminders ?? 0);

  ngOnInit(): void {
    this.loadClients();
    this.loadInvoiceData();
  }

  onClientChange(clientId: string): void {
    this.selectedClientId.set(clientId);
    this.result.set(null);
    this.loadInvoiceData();
  }

  reload(): void {
    this.loadClients();
    this.loadInvoiceData();
  }

  openCreate(): void {
    this.error.set(null);
    this.createOpen.set(true);
    this.createDialog?.reset();
  }

  closeCreate(): void {
    if (this.creating()) {
      return;
    }

    this.createOpen.set(false);
  }

  submitCreate(event: {
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
  }): void {
    if (this.creating()) {
      return;
    }

    this.creating.set(true);
    this.error.set(null);

    if (event.mode === 'existingClient') {
      this.invoicesService
        .createInvoice(event.invoice)
        .pipe(finalize(() => this.creating.set(false)))
        .subscribe({
          next: () => {
            this.createOpen.set(false);
            this.selectedClientId.set(event.invoice.clientId);
            this.loadClients();
            this.loadInvoiceData();
          },
          error: error => {
            this.error.set(errorMessage(error));
          }
        });
      return;
    }

    this.clientsService
      .createClientWithInvoice({
        client: event.client,
        invoice: event.invoice
      })
      .pipe(finalize(() => this.creating.set(false)))
      .subscribe({
        next: invoice => {
          this.createOpen.set(false);
          this.selectedClientId.set(invoice.clientId);
          this.loadClients();
          this.loadInvoiceData();
        },
        error: error => {
          this.error.set(errorMessage(error));
        }
      });
  }

  openConfirm(): void {
    if (this.processing() || this.eligibleCount() === 0) {
      return;
    }

    this.error.set(null);
    this.confirming.set(true);
  }

  cancelConfirm(): void {
    if (this.processing()) {
      return;
    }

    this.confirming.set(false);
  }

  confirmProcess(): void {
    if (this.processing()) {
      return;
    }

    this.processing.set(true);
    this.error.set(null);

    const clientIds =
      this.selectedClientId() === 'all' ? [] : [this.selectedClientId()];

    this.remindersService
      .process(clientIds)
      .pipe(finalize(() => this.processing.set(false)))
      .subscribe({
        next: result => {
          this.confirming.set(false);
          this.result.set(result);
          this.loadInvoiceData();
        },
        error: error => {
          this.confirming.set(false);
          this.error.set(errorMessage(error));
        }
      });
  }

  resetDemo(): void {
    if (
      !window.confirm(
        'Se restaurarán los 3 clientes y las 12 facturas demo. ¿Deseas continuar?'
      )
    ) {
      return;
    }

    this.resetting.set(true);
    this.error.set(null);

    this.demoService
      .reset()
      .pipe(finalize(() => this.resetting.set(false)))
      .subscribe({
        next: () => {
          this.selectedClientId.set('all');
          this.result.set(null);
          this.loadClients();
          this.loadInvoiceData();
        },
        error: error => this.error.set(errorMessage(error))
      });
  }

  dismissResult(): void {
    this.result.set(null);
  }

  openEmailPreview(invoice: Invoice): void {
    this.preview.set(null);
    this.previewLoading.set(true);
    this.previewOpen.set(true);
    this.error.set(null);

    this.invoicesService
      .getEmailPreview(invoice.id)
      .pipe(finalize(() => this.previewLoading.set(false)))
      .subscribe({
        next: preview => this.preview.set(preview),
        error: error => {
          this.previewOpen.set(false);
          this.error.set(errorMessage(error));
        }
      });
  }

  closeEmailPreview(): void {
    this.previewOpen.set(false);
    this.preview.set(null);
  }

  private loadClients(): void {
    this.clientsService.getClients().subscribe({
      next: clients => this.clients.set(clients),
      error: error => this.error.set(errorMessage(error))
    });
  }

  private loadInvoiceData(): void {
    this.loading.set(true);

    const clientId =
      this.selectedClientId() === 'all' ? undefined : this.selectedClientId();

    forkJoin({
      invoices: this.invoicesService.getInvoices(clientId),
      summary: this.invoicesService.getSummary(clientId)
    })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: ({ invoices, summary }) => {
          this.invoices.set(invoices);
          this.summary.set(summary);
        },
        error: error => {
          this.invoices.set([]);
          this.summary.set(null);
          this.error.set(errorMessage(error));
        }
      });
  }
}
