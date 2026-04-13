import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  ViewChild,
  effect,
  inject,
  input,
  output,
} from '@angular/core';
import { closeBrackets, closeBracketsKeymap } from '@codemirror/autocomplete';
import { css } from '@codemirror/lang-css';
import { html } from '@codemirror/lang-html';
import { javascript } from '@codemirror/lang-javascript';
import { json } from '@codemirror/lang-json';
import { markdown } from '@codemirror/lang-markdown';
import { php } from '@codemirror/lang-php';
import { python } from '@codemirror/lang-python';
import { sql } from '@codemirror/lang-sql';
import { xml } from '@codemirror/lang-xml';
import { yaml } from '@codemirror/lang-yaml';
import {
  bracketMatching,
  HighlightStyle,
  indentOnInput,
  syntaxHighlighting,
} from '@codemirror/language';
import { Compartment, EditorSelection, EditorState } from '@codemirror/state';
import {
  EditorView,
  drawSelection,
  dropCursor,
  highlightActiveLine,
  keymap,
  lineNumbers,
} from '@codemirror/view';
import { tags } from '@lezer/highlight';

import { EditorLanguage } from '../../models/app.models';

const blinkShareHighlightStyle = HighlightStyle.define([
  { tag: tags.propertyName, color: '#5FC8A8' },
  { tag: tags.string, color: '#F0A050' },
  { tag: tags.number, color: '#F0A050' },
  { tag: tags.bool, color: '#2EB88A' },
  { tag: tags.null, color: '#7A9BAA' },
  { tag: [tags.keyword, tags.operatorKeyword], color: '#C8DCEA' },
  { tag: [tags.punctuation, tags.bracket], color: '#7A9BAA' },
  { tag: tags.comment, color: '#446E7E' },
]);

@Component({
  selector: 'app-code-editor',
  standalone: true,
  imports: [CommonModule],
  template: ` <div #host class="editor-shell min-h-[18rem] w-full overflow-hidden"></div> `,
  styles: [
    `
      :host {
        display: block;
      }

      .editor-shell {
        border-radius: var(--radius-ui);
        overflow: hidden;
        background: #111920;
      }

      :host ::ng-deep .cm-editor {
        min-height: 18rem;
        background: transparent;
        color: #c8dcea;
        border-radius: var(--radius-ui);
      }

      :host ::ng-deep .cm-scroller {
        font-family:
          ui-monospace,
          "SFMono-Regular",
          Consolas,
          monospace;
        line-height: 1.6;
        padding: 1rem 0 4rem;
      }

      :host ::ng-deep .cm-gutters {
        border-right: 1px solid #1a2530;
        background: #111920;
        color: #446e7e;
      }

      :host ::ng-deep .cm-content,
      :host ::ng-deep .cm-gutter {
        min-height: 18rem;
      }

      :host ::ng-deep .cm-line {
        padding: 0 1rem;
      }

      :host ::ng-deep .cm-activeLine,
      :host ::ng-deep .cm-activeLineGutter {
        background: #15212a;
      }

      :host ::ng-deep .cm-focused {
        outline: none;
      }

      :host ::ng-deep .cm-cursor,
      :host ::ng-deep .cm-dropCursor {
        border-left-color: #c8dcea;
      }

      :host ::ng-deep .cm-selectionBackground {
        background: rgb(68 110 126 / 0.35) !important;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CodeEditorComponent {
  private readonly languageCompartment = new Compartment();
  private readonly readOnlyCompartment = new Compartment();
  private readonly editableCompartment = new Compartment();
  private readonly wrapCompartment = new Compartment();
  private readonly destroyRef = inject(DestroyRef);

  private view: EditorView | null = null;

  public readonly value = input('');
  public readonly language = input<EditorLanguage>('plaintext');
  public readonly readOnly = input(false);
  public readonly wrap = input(true);
  public readonly contentChanged = output<string>();

  @ViewChild('host', { static: true }) private readonly hostElement?: ElementRef<HTMLDivElement>;

  public constructor() {
    effect(() => {
      if (!this.view) {
        return;
      }

      const nextValue = this.value();
      const currentValue = this.view.state.doc.toString();
      if (nextValue !== currentValue) {
        this.view.dispatch({
          changes: {
            from: 0,
            to: currentValue.length,
            insert: nextValue,
          },
        });
      }
    });

    effect(() => {
      if (!this.view) {
        return;
      }

      this.view.dispatch({
        effects: this.languageCompartment.reconfigure(this.createLanguageExtension(this.language())),
      });
    });

    effect(() => {
      if (!this.view) {
        return;
      }

      this.view.dispatch({
        effects: [
          this.readOnlyCompartment.reconfigure(EditorState.readOnly.of(this.readOnly())),
          this.editableCompartment.reconfigure(EditorView.editable.of(!this.readOnly())),
        ],
      });
    });

    effect(() => {
      if (!this.view) {
        return;
      }

      this.view.dispatch({
        effects: this.wrapCompartment.reconfigure(this.wrap() ? EditorView.lineWrapping : []),
      });
    });
  }

  public ngAfterViewInit(): void {
    const host = this.hostElement?.nativeElement;
    if (!host) {
      return;
    }

    this.view = new EditorView({
      state: EditorState.create({
        doc: this.value(),
        extensions: [
          lineNumbers(),
          drawSelection(),
          dropCursor(),
          highlightActiveLine(),
          indentOnInput(),
          bracketMatching(),
          closeBrackets(),
          syntaxHighlighting(blinkShareHighlightStyle, { fallback: true }),
          this.languageCompartment.of(this.createLanguageExtension(this.language())),
          this.readOnlyCompartment.of(EditorState.readOnly.of(this.readOnly())),
          this.editableCompartment.of(EditorView.editable.of(!this.readOnly())),
          this.wrapCompartment.of(this.wrap() ? EditorView.lineWrapping : []),
          EditorState.allowMultipleSelections.of(true),
          EditorView.contentAttributes.of({
            tabindex: '0',
            spellcheck: 'false',
            autocorrect: 'off',
            autocapitalize: 'off',
          }),
          keymap.of([
            {
              key: 'Tab',
              run: (view) => {
                if (this.readOnly()) {
                  return false;
                }

                const changes = view.state.changeByRange((range) => ({
                  changes: { from: range.from, to: range.to, insert: '  ' },
                  range: EditorSelection.cursor(range.from + 2),
                }));

                view.dispatch(changes);
                return true;
              },
            },
            ...closeBracketsKeymap,
          ]),
          EditorView.theme({
            '&': {
              height: '100%',
              borderRadius: 'var(--radius-ui)',
              backgroundColor: '#111920',
            },
            '.cm-content': {
              caretColor: '#C8DCEA',
            },
          }),
          EditorView.updateListener.of((update) => {
            if (update.docChanged) {
              this.contentChanged.emit(update.state.doc.toString());
            }
          }),
        ],
      }),
      parent: host,
    });

    if (!this.readOnly()) {
      queueMicrotask(() => this.view?.focus());
    }

    this.destroyRef.onDestroy(() => {
      this.view?.destroy();
      this.view = null;
    });
  }

  private createLanguageExtension(language: EditorLanguage) {
    switch (language) {
      case 'javascript':
        return javascript();
      case 'typescript':
        return javascript({ typescript: true });
      case 'jsx':
        return javascript({ jsx: true });
      case 'tsx':
        return javascript({ typescript: true, jsx: true });
      case 'json':
        return json();
      case 'html':
        return html();
      case 'xml':
        return xml();
      case 'css':
        return css();
      case 'markdown':
        return markdown();
      case 'python':
        return python();
      case 'sql':
        return sql();
      case 'yaml':
        return yaml();
      case 'php':
        return php();
      default:
        return [];
    }
  }
}
