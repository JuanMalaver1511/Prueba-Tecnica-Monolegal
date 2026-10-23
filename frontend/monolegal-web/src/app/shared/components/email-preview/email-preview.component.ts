import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

import { EmailPreview } from '../../../core/models/email-preview.model';

@Component({
  selector: 'app-email-preview',
  templateUrl: './email-preview.component.html',
  styleUrl: './email-preview.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EmailPreviewComponent {
  readonly preview = input<EmailPreview | null>(null);
  readonly loading = input.required<boolean>();
  readonly close = output<void>();

  onBackdrop(): void {
    this.close.emit();
  }
}
