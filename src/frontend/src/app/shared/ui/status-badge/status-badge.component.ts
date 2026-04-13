import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  template: `
    <span
      class="inline-flex items-center gap-2 rounded-full border px-3 py-1 text-xs font-semibold uppercase tracking-[0.18em]"
      [class]="classes()"
    >
      <span class="h-2 w-2 rounded-full bg-current"></span>
      {{ label() }}
    </span>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusBadgeComponent {
  public readonly tone = input<'neutral' | 'success' | 'warning' | 'danger'>('neutral');
  public readonly label = input('Idle');

  protected readonly classes = computed(() => {
    switch (this.tone()) {
      case 'success':
        return 'border-teal-200 bg-teal-50 text-teal-800';
      case 'warning':
        return 'border-amber-200 bg-amber-50 text-amber-800';
      case 'danger':
        return 'border-rose-200 bg-rose-50 text-rose-800';
      default:
        return 'border-slate-200 bg-white text-slate-700';
    }
  });
}
