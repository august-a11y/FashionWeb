import { Routes } from '@angular/router';

export const dashboard_Routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./dashboard.component').then(m => m.DashboardComponent),
    data: {
      title: ''
    }
  }
];

