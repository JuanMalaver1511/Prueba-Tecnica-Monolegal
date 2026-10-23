import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { InvoiceStatus } from '../../../core/models/invoice.model';

@Component({
  selector: 'app-status-badge',
  templateUrl: './status-badge.component.html',
  styleUrl: './status-badge.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StatusBadgeComponent {
  readonly status = input.required<InvoiceStatus>();
  readonly label = input.required<string>();

  readonly toneClass = computed(() => `badge--${this.status()}`);
}
