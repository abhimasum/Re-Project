import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

/** Shape of an item the host (Razor/JS bridge) sends in. */
export interface PanelItem {
  id: string;
  defectDescription: string;
  status: string;
  assignedTo: string | null;
}

/** Event the host receives when a user clicks an action. */
export interface PanelAction {
  id: string;
  action: 'inspect' | 'approve' | 'complete';
}

@Component({
  selector: 'rework-panel',
  standalone: true,
  imports: [CommonModule],
  styleUrl: './rework-panel.component.css',
  template: `
    @if (visible().length === 0) {
      <div class="empty">No rework items.</div>
    }
    @for (item of visible(); track item.id) {
      <div class="row">
        <div class="desc">
          <div>{{ item.defectDescription }}</div>
          <div class="meta">{{ item.status }}@if (item.assignedTo) { · {{ item.assignedTo }} }</div>
        </div>
        @if (nextAction(item.status); as action) {
          <button type="button" (click)="act(item.id, action.key)">{{ action.label }}</button>
        }
      </div>
    }
  `,
})
export class ReworkPanelComponent {
  /** Set by the host as an element property: panel.items = [...] */
  @Input() set items(value: PanelItem[] | null | undefined) {
    this._items.set(value ?? []);
  }
  private readonly _items = signal<PanelItem[]>([]);
  readonly visible = this._items.asReadonly();

  /** Emitted as a CustomEvent named "actionRequested" on the host element. */
  @Output() actionRequested = new EventEmitter<PanelAction>();

  nextAction(status: string): { key: PanelAction['action']; label: string } | null {
    switch (status) {
      case 'Submitted': return { key: 'inspect', label: 'Inspect' };
      case 'UnderInspection': return { key: 'approve', label: 'Approve' };
      case 'Approved': return { key: 'complete', label: 'Mark complete' };
      default: return null;
    }
  }

  act(id: string, action: PanelAction['action']): void {
    this.actionRequested.emit({ id, action });
  }
}
