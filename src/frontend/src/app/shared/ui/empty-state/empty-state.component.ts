import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  template: `
    <div class="surface-panel rounded-[1.5rem] p-6 text-center">
      <p class="mb-2 text-sm font-semibold uppercase tracking-[0.18em] text-teal-700">
        {{ eyebrow() }}
      </p>
      <h3 class="mb-2 text-xl font-semibold tracking-tight text-slate-900">{{ title() }}</h3>
      <p class="mx-auto max-w-md text-sm leading-6 text-slate-600">{{ copy() }}</p>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmptyStateComponent {
  public readonly eyebrow = input('BlinkShare');
  public readonly title = input('Nothing here yet');
  public readonly copy = input('Start a session, create a share, or paste text to populate this view.');
}
