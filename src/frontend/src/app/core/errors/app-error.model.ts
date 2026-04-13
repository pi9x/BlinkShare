export type AppErrorKind =
  | 'validation'
  | 'notFound'
  | 'expired'
  | 'quotaExceeded'
  | 'unauthorized'
  | 'forbidden'
  | 'conflict'
  | 'unexpected';

export interface AppError {
  kind: AppErrorKind;
  code?: string;
  message: string;
  status?: number;
}
