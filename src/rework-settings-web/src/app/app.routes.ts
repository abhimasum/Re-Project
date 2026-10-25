import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'settings' },
  {
    path: 'settings',
    loadComponent: () => import('./features/settings/settings-page.component').then((m) => m.SettingsPageComponent),
  },
  {
    path: 'preferences',
    loadComponent: () => import('./features/preferences/local-preferences.component').then((m) => m.LocalPreferencesComponent),
  },
  { path: '**', redirectTo: 'settings' },
];
