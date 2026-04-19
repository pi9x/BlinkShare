import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  template: `
    <span
      class="inline-flex items-center gap-1.5 rounded-full border px-2.5 py-1 text-[0.68rem] font-bold uppercase tracking-[0.08em] sm:gap-2 sm:px-3 sm:text-xs"
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
        return 'border-[#D4E3EB] bg-[#EEF8FB] text-[#1A7A8A]';
      case 'warning':
        return 'border-[#D4E3EB] bg-[#FFF6E7] text-[#C8A030]';
      case 'danger':
        return 'border-[#F3D4CF] bg-[#FEF1EF] text-[#C0402A]';
      default:
        return 'border-[#D4E3EB] bg-[#EEF8FB] text-[#7A9BAA]';
    }
  });
}
