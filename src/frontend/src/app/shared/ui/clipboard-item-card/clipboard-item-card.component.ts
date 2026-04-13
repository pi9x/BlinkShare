import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

import { LocalClipboardItem } from '../../models/app.models';
import { formatBytes } from '../../utils/bytes';

@Component({
  selector: 'app-clipboard-item-card',
  standalone: true,
  imports: [DatePipe],
  template: `
    <article class="space-y-2">
      <div class="flex items-start justify-between gap-2">
        <div>
          <p class="text-xs font-bold uppercase tracking-[0.02em]" style="color: var(--muted-label);">
            <span [style.color]="item().direction === 'received' ? 'var(--success)' : 'var(--accent-warm)'">•</span>
            {{ item().direction }} · {{ item().kind === 'text' ? (item().language || 'plain') : 'file' }}
          </p>
        </div>

        <div class="flex flex-wrap items-center justify-end gap-2">
          <p class="text-xs font-semibold" style="color: var(--muted-label);">{{ item().createdAtUtc | date: 'shortTime' }}</p>
          @if (copied()) {
            <span class="text-xs font-semibold" style="color: var(--surface-strong);">Copied</span>
          }
          <button
            class="icon-button h-9 w-9"
            [class.icon-button-success]="copied()"
            type="button"
            (click)="copy.emit()"
            [attr.aria-label]="copied() ? 'Copied' : 'Copy'"
            [attr.title]="copied() ? 'Copied to clipboard' : 'Copy'"
          >
            @if (copied()) {
              <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                <path d="m5 12 5 5L20 7" />
              </svg>
            } @else {
              <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                <rect x="9" y="9" width="11" height="11" rx="2" />
                <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />
              </svg>
            }
          </button>
        </div>
      </div>

      @if (item().kind === 'text') {
        <pre class="max-h-40 overflow-auto p-3 text-xs leading-5 text-slate-100" style="border-radius: var(--radius-ui); background: var(--code-dark);">{{ item().text }}</pre>
      } @else {
        <div class="px-3 py-2 text-sm" style="border-radius: var(--radius-ui); background: var(--code-dark); color: #E2E8F0;">
          <p class="font-semibold text-white">{{ item().fileName }}</p>
          <p>{{ item().contentType || 'Unknown content type' }}</p>
          <p>{{ formatBytes(item().sizeBytes) }}</p>
        </div>
      }
    </article>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClipboardItemCardComponent {
  public readonly item = input.required<LocalClipboardItem>();
  public readonly copied = input(false);
  public readonly copy = output<void>();

  protected readonly formatBytes = formatBytes;
}
