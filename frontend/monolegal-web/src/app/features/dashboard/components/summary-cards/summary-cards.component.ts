import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { InvoiceSummary } from '../../../../core/models/invoice.model';

@Component({
  selector: 'app-summary-cards',
  imports: [CurrencyPipe],
  templateUrl: './summary-cards.component.html',
  styleUrl: './summary-cards.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SummaryCardsComponent {
  readonly summary = input.required<InvoiceSummary>();

  readonly settledCount = computed(() =>
    this.summary()
      .byStatus.filter(status => status.status === 'aldia' || status.status === 'pagada')
      .reduce((total, status) => total + status.count, 0)
  );
}
