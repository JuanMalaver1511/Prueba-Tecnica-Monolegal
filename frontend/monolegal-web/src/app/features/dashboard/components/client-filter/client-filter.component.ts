import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

import { Client } from '../../../../core/models/client.model';

@Component({
  selector: 'app-client-filter',
  templateUrl: './client-filter.component.html',
  styleUrl: './client-filter.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClientFilterComponent {
  readonly clients = input.required<Client[]>();
  readonly selected = input.required<string>();
  readonly selectedChange = output<string>();

  onChange(event: Event): void {
    this.selectedChange.emit((event.target as HTMLSelectElement).value);
  }
}
