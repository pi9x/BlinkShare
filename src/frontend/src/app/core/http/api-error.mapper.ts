import { HttpErrorResponse } from '@angular/common/http';

import { AppError } from '../errors/app-error.model';

interface ProblemDetailsLike {
  title?: string;
  detail?: string;
  code?: string;
}

function mapStatusToKind(status?: number): AppError['kind'] {
  switch (status) {
    case 400:
      return 'validation';
    case 401:
      return 'unauthorized';
    case 403:
      return 'forbidden';
    case 404:
      return 'notFound';
    case 409:
      return 'conflict';
    case 410:
      return 'expired';
    case 429:
      return 'quotaExceeded';
    default:
      return 'unexpected';
  }
}

function mapCodeToKind(code?: string): AppError['kind'] {
  switch (code) {
    case 'general.validation':
    case 'share.invalid_text':
    case 'share.text_too_large':
    case 'share.file_too_large':
    case 'session.invalid_code':
      return 'validation';
    case 'auth.unauthorized':
    case 'auth.invalid_credentials':
      return 'unauthorized';
    case 'share.passcode_required':
    case 'share.invalid_passcode':
    case 'session.invalid_resume_token':
      return 'forbidden';
    case 'share.not_found':
    case 'session.not_found':
    case 'session.peer_not_found':
      return 'notFound';
    case 'share.expired':
    case 'session.reconnect_grace_elapsed':
      return 'expired';
    case 'quota.daily_bytes_exceeded':
      return 'quotaExceeded';
    default:
      return 'unexpected';
  }
}

export function toAppError(error: unknown): AppError {
  if (error instanceof HttpErrorResponse) {
    const payload = (error.error ?? {}) as ProblemDetailsLike;
    const code = payload.code ?? payload.title;
    return {
      kind: mapStatusToKind(error.status),
      code,
      status: error.status,
      message:
        payload.detail ??
        payload.title ??
        (error.status === 0
          ? 'The BlinkShare API is not reachable right now.'
          : 'The request could not be completed.'),
    };
  }

  if (error instanceof Error) {
    const [code, ...messageParts] = error.message.split('|');
    if (messageParts.length > 0) {
      return {
        kind: mapCodeToKind(code),
        code,
        message: messageParts.join('|'),
      };
    }

    return {
      kind: 'unexpected',
      message: error.message,
    };
  }

  return {
    kind: 'unexpected',
    message: 'An unexpected error occurred.',
  };
}
