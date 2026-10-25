import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SettingsService } from '../../core/settings.service';
import { AppSetting } from '../../core/settings.models';

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <h1>Application settings</h1>
    <p class="muted">Shared settings stored in the backend. Every station and client reads these values.</p>

    <section class="card">
      <h2>Add a setting</h2>
      <form [formGroup]="createForm" (ngSubmit)="create()">
        <div style="display:grid;grid-template-columns:1fr 1fr 1fr auto;gap:12px;align-items:end">
          <label><span>Key</span><input formControlName="key" placeholder="api.timeoutSeconds" /></label>
          <label><span>Value</span><input formControlName="value" /></label>
          <label><span>Category</span><input formControlName="category" placeholder="general" /></label>
          <button type="submit" [disabled]="createForm.invalid || busy()">Add</button>
        </div>
      </form>
      @if (notice(); as n) { <div [class]="n.kind">{{ n.text }}</div> }
    </section>

    <section class="card">
      <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:8px">
        <h2 style="margin:0">Settings</h2>
        <select style="width:220px" [value]="categoryFilter()" (change)="setFilter($any($event.target).value)">
          <option value="">All categories</option>
          @for (c of categories(); track c) { <option [value]="c">{{ c }}</option> }
        </select>
      </div>

      @if (loading()) { <div class="muted">Loading…</div> }
      <table>
        <thead><tr><th>Key</th><th>Value</th><th>Category</th><th>Updated</th><th></th></tr></thead>
        <tbody>
          @for (s of settings(); track s.key) {
            <tr>
              <td><code>{{ s.key }}</code></td>
              <td>
                @if (editingKey() === s.key) {
                  <input [formControl]="editControl" style="max-width:280px" />
                } @else {
                  {{ s.value }}
                }
              </td>
              <td>{{ s.category }}</td>
              <td class="muted">{{ s.updatedAt | date:'short' }}</td>
              <td style="text-align:right">
                @if (editingKey() === s.key) {
                  <button (click)="saveEdit(s)" [disabled]="editControl.invalid || busy()">Save</button>
                  <button class="secondary" (click)="cancelEdit()">Cancel</button>
                } @else {
                  <button class="secondary" (click)="startEdit(s)">Edit</button>
                }
              </td>
            </tr>
          } @empty {
            @if (!loading()) { <tr><td colspan="5" class="muted">No settings yet.</td></tr> }
          }
        </tbody>
      </table>
    </section>
  `,
})
export class SettingsPageComponent implements OnInit {
  private readonly api = inject(SettingsService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  settings = signal<AppSetting[]>([]);
  categoryFilter = signal('');
  categories = computed(() => [...new Set(this.settings().map((s) => s.category))].sort());
  loading = signal(false);
  busy = signal(false);
  editingKey = signal<string | null>(null);
  notice = signal<{ kind: 'ok' | 'error'; text: string } | null>(null);

  createForm = this.fb.nonNullable.group({
    key: ['', [Validators.required, Validators.maxLength(100), Validators.pattern(/^[A-Za-z0-9._-]+$/)]],
    value: ['', [Validators.required, Validators.maxLength(2000)]],
    category: ['general', [Validators.required, Validators.maxLength(50)]],
  });

  editControl = this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(2000)]);

  ngOnInit(): void {
    this.load();
  }

  setFilter(category: string): void {
    this.categoryFilter.set(category);
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.list(this.categoryFilter() || undefined)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (list) => { this.settings.set(list); this.loading.set(false); },
        error: (err) => { this.loading.set(false); this.show('error', this.reason(err, 'Could not load settings.')); },
      });
  }

  create(): void {
    if (this.createForm.invalid) return;
    this.busy.set(true);
    this.api.create(this.createForm.getRawValue())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => { this.busy.set(false); this.createForm.reset({ category: 'general' }); this.show('ok', 'Setting added.'); this.load(); },
        error: (err) => { this.busy.set(false); this.show('error', this.reason(err, 'The setting could not be added.')); },
      });
  }

  startEdit(setting: AppSetting): void {
    this.editingKey.set(setting.key);
    this.editControl.setValue(setting.value);
  }

  cancelEdit(): void {
    this.editingKey.set(null);
  }

  saveEdit(setting: AppSetting): void {
    if (this.editControl.invalid) return;
    this.busy.set(true);
    this.api.update(setting.key, this.editControl.value)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => { this.busy.set(false); this.editingKey.set(null); this.show('ok', `${setting.key} updated.`); this.load(); },
        error: (err) => { this.busy.set(false); this.show('error', this.reason(err, 'The update failed.')); },
      });
  }

  private show(kind: 'ok' | 'error', text: string): void {
    this.notice.set({ kind, text });
  }

  /** ProblemDetails carries the reason in "detail"; fall back to the title, then to a generic message. */
  private reason(err: { error?: { detail?: string; title?: string } }, fallback: string): string {
    return err.error?.detail ?? err.error?.title ?? fallback;
  }
}
