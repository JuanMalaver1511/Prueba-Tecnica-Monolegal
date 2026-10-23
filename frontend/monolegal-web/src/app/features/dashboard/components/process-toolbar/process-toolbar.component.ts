import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-process-toolbar',
  templateUrl: './process-toolbar.component.html',
  styleUrl: './process-toolbar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProcessToolbarComponent {
  readonly processing = input.required<boolean>();
  readonly eligibleCount = input.required<number>();
  readonly invoicesCount = input.required<number>();
  readonly clientsCount = input.required<number>();
  readonly scopeLabel = input.required<string>();
  readonly process = output<void>();
}
