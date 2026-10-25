import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <nav>
      <strong style="margin-right:auto;color:#38bdf8">Rework Settings</strong>
      <a routerLink="/settings" routerLinkActive="active">Shared settings</a>
      <a routerLink="/preferences" routerLinkActive="active">Local preferences</a>
    </nav>
    <main>
      <router-outlet />
    </main>
  `,
})
export class AppComponent {}
