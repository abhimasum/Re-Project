import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LocalPreferences, LocalPreferencesService } from '../../core/local-preferences.service';

@Component({
  selector: 'app-local-preferences',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <h1>Local preferences</h1>
    <p class="muted">Stored in this browser only. They do not change the shared backend settings.</p>

    <form class="card" [formGroup]="form" (ngSubmit)="save()" style="max-width:520px">
      <label>
        <span>Station ID</span>
        <input formControlName="stationId" maxlength="20" />
      </label>
      <label>
        <span>Theme</span>
        <select formControlName="theme">
          <option value="dark">Dark</option>
          <option value="light">Light</option>
        </select>
      </label>
      <label>
        <span>Refresh interval (seconds)</span>
        <input type="number" formControlName="pollSeconds" min="2" max="300" />
      </label>
      <div style="display:flex;gap:8px">
        <button type="submit" [disabled]="form.invalid">Save</button>
        <button type="button" class="secondary" (click)="resetToDefaults()">Reset</button>
      </div>
      @if (saved()) { <div class="ok">Preferences saved.</div> }
    </form>
  `,
})
export class LocalPreferencesComponent {
  private readonly prefs = inject(LocalPreferencesService);
  private readonly fb = inject(FormBuilder);

  saved = signal(false);

  form = this.fb.nonNullable.group({
    stationId: [this.prefs.preferences().stationId, [Validators.required, Validators.maxLength(20)]],
    theme: [this.prefs.preferences().theme, Validators.required],
    pollSeconds: [this.prefs.preferences().pollSeconds, [Validators.required, Validators.min(2), Validators.max(300)]],
  });

  save(): void {
    if (this.form.invalid) return;
    this.prefs.save(this.form.getRawValue() as LocalPreferences);
    this.saved.set(true);
  }

  resetToDefaults(): void {
    this.prefs.reset();
    const p = this.prefs.preferences();
    this.form.reset({ stationId: p.stationId, theme: p.theme, pollSeconds: p.pollSeconds });
    this.saved.set(false);
  }
}
