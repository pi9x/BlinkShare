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
      <div class="flex min-h-9 items-center justify-between gap-2">
        <div class="min-w-0 flex-1">
          <p class="truncate text-xs leading-none font-bold uppercase tracking-[0.02em]" style="color: var(--muted-label);">
            <span [style.color]="item().direction === 'received' ? 'var(--success)' : 'var(--accent-warm)'">•</span>
            {{ item().direction }} · {{ item().kind === 'text' ? (item().language || 'plain') : 'file' }}
          </p>
        </div>

        <div class="flex shrink-0 items-center justify-end gap-2">
          <p class="text-xs leading-none font-semibold" style="color: var(--muted-label);">{{ item().createdAtUtc | date: 'shortTime' }}</p>
          <button
            class="icon-button h-9 w-9"
            [class.icon-button-success]="item().kind === 'text' && copied()"
            type="button"
            (click)="item().kind === 'text' ? copy.emit() : download.emit()"
            [attr.aria-label]="item().kind === 'text' ? (copied() ? 'Copied' : 'Copy') : 'Download'"
            [attr.title]="item().kind === 'text' ? (copied() ? 'Copied to clipboard' : 'Copy') : 'Download file'"
          >
            @if (item().kind === 'text' && copied()) {
              <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                <path d="m5 12 5 5L20 7" />
              </svg>
            } @else if (item().kind === 'file-metadata') {
              <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
                <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
                <path d="M7 10 12 15 17 10" />
                <path d="M12 15V3" />
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
        <div class="flex items-start gap-2.5 px-3 py-2 text-sm" style="border-radius: var(--radius-ui); background: var(--code-dark); color: #E2E8F0;">
          <div class="mt-0.5 shrink-0 rounded-[0.35rem] border p-1.5" style="border-color: #2b4351; background: #15212a; color: #7a9baa;">
            <svg viewBox="0 0 24 24" class="h-4 w-4 fill-none stroke-current stroke-2">
              <path d="M14 2H7a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7z" />
              <path d="M14 2v5h5" />
            </svg>
          </div>
          <div class="min-w-0">
            <p class="truncate font-semibold text-white">{{ item().fileName || 'Unnamed file' }}</p>
            <p class="truncate">{{ item().contentType || 'Unknown content type' }} · {{ formatBytes(item().sizeBytes) }}</p>
          </div>
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
  public readonly download = output<void>();

  protected readonly formatBytes = formatBytes;
}
