import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

import { ReminderProcessingResult } from '../../../../core/models/reminder.model';
import { statusLabel } from '../../../../core/utils/status-label';

@Component({
  selector: 'app-process-result',
  templateUrl: './process-result.component.html',
  styleUrl: './process-result.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProcessResultComponent {
  readonly result = input.required<ReminderProcessingResult>();
  readonly dismiss = output<void>();

  readonly hasFailures = computed(() => this.result().failures > 0);

  statusLabel = statusLabel;
}
