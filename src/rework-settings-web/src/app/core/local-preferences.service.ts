import { Injectable, signal } from '@angular/core';

/** Settings that belong to this browser or terminal only, not to the shared backend. */
export interface LocalPreferences {
  stationId: string;
  theme: 'dark' | 'light';
  pollSeconds: number;
}

const STORAGE_KEY = 'rework.settings.local';
const DEFAULTS: LocalPreferences = { stationId: 'ST-01', theme: 'dark', pollSeconds: 5 };

@Injectable({ providedIn: 'root' })
export class LocalPreferencesService {
  readonly preferences = signal<LocalPreferences>(this.read());

  save(next: LocalPreferences): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
    this.preferences.set(next);
  }

  reset(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.preferences.set({ ...DEFAULTS });
  }

  private read(): LocalPreferences {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? { ...DEFAULTS, ...JSON.parse(raw) } : { ...DEFAULTS };
    } catch {
      // Corrupt storage should never block the settings screen.
      return { ...DEFAULTS };
    }
  }
}
