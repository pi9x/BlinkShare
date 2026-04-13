import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

import { LocalClipboardItem } from '../../models/app.models';
import { formatBytes } from '../../utils/bytes';

@Component({
  selector: 'app-clipboard-item-card',
  standalone: true,
  imports: [DatePipe],
  template: `
    <article class="surface-panel rounded-xl p-3">
      <div class="mb-2 flex items-start justify-between gap-2">
        <div>
          <p class="text-xs font-semibold uppercase tracking-[0.16em] text-slate-500">
            {{ item().direction }} · {{ item().kind === 'text' ? (item().language || 'plaintext') : 'file metadata' }}
          </p>
          <p class="mt-1 text-xs text-slate-500">{{ item().createdAtUtc | date: 'short' }}</p>
        </div>

        <div class="flex flex-wrap justify-end gap-2">
          <button class="icon-button h-9 w-9" type="button" (click)="copy.emit()" aria-label="Copy" title="Copy">
            <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
              <rect x="9" y="9" width="11" height="11" rx="2" />
              <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />
            </svg>
          </button>
        </div>
      </div>

      @if (item().kind === 'text') {
        <pre class="max-h-40 overflow-auto rounded-xl bg-slate-950 p-3 text-xs leading-5 text-slate-100">{{ item().text }}</pre>
      } @else {
        <div class="rounded-xl bg-white px-3 py-2 text-sm text-slate-700">
          <p class="font-semibold text-slate-900">{{ item().fileName }}</p>
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
  public readonly copy = output<void>();

  protected readonly formatBytes = formatBytes;
}
