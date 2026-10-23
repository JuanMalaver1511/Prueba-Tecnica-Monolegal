import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    title: 'Monolegal | Recordatorios de cobranza',
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then(module => module.DashboardComponent)
  },
  { path: '**', redirectTo: '' }
];
