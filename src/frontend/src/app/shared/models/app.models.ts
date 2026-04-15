export enum AccountTier {
  Anonymous = 1,
  Free = 2,
  Premium = 3,
}

export enum ShareKind {
  Text = 1,
  File = 2,
}

export enum ShareStatus {
  Pending = 1,
  Ready = 2,
  Expired = 3,
  Deleted = 4,
}

export type EditorLanguage =
  | 'plaintext'
  | 'json'
  | 'javascript'
  | 'typescript'
  | 'jsx'
  | 'tsx'
  | 'html'
  | 'xml'
  | 'css'
  | 'markdown'
  | 'python'
  | 'sql'
  | 'yaml'
  | 'php';

export interface EditorLanguageOption {
  id: EditorLanguage;
  label: string;
}

export const EDITOR_LANGUAGES: EditorLanguageOption[] = [
  { id: 'plaintext', label: 'Plain text' },
  { id: 'json', label: 'JSON' },
  { id: 'javascript', label: 'JavaScript' },
  { id: 'typescript', label: 'TypeScript' },
  { id: 'jsx', label: 'JSX' },
  { id: 'tsx', label: 'TSX' },
  { id: 'html', label: 'HTML' },
  { id: 'xml', label: 'XML' },
  { id: 'css', label: 'CSS' },
  { id: 'markdown', label: 'Markdown' },
  { id: 'python', label: 'Python' },
  { id: 'sql', label: 'SQL' },
  { id: 'yaml', label: 'YAML' },
  { id: 'php', label: 'PHP' },
];

export interface CreateTextRequest {
  text: string | null;
}

export interface CreateTextResponse {
  shareId: string;
  code: string;
  expiresAtUtc: string;
}

export interface GetShareByCodeResponse {
  shareId: string;
  code: string;
  kind: ShareKind;
  status: ShareStatus;
  expiresAtUtc: string | null;
  hasPasscode: boolean;
  sizeBytes: number;
}

export interface ReadTextResponse {
  shareId: string;
  code: string;
  text: string;
  expiresAtUtc: string | null;
}

export interface UnlockRequest {
  passcode: string | null;
}

export interface UnlockResponse {
  shareId: string;
  code: string;
  unlockProof: string;
  unlockedUntilUtc: string;
}

export interface CreateFileUploadRequest {
  fileName: string | null;
  contentType: string | null;
  sizeBytes: number;
}

export interface CreateFileUploadResponse {
  shareId: string;
  code: string;
  storageKey: string;
  uploadUrl: string;
  expiresAtUtc: string;
}

export interface CompleteFileUploadResponse {
  shareId: string;
  code: string;
  status: ShareStatus;
}

export interface RequestDownloadResponse {
  shareId: string;
  code: string;
  downloadUrl: string;
  downloadCount: number;
}

export interface CreateOrJoinAnonymousSessionRequest {
  code: string | null;
}

export interface CreateOrJoinAnonymousSessionResponse {
  sessionId: string;
  code: string;
  peerId: string;
  resumeToken: string;
  peerCount: number;
  reconnectGraceSeconds: number;
}

export interface ResumeAnonymousSessionRequest {
  sessionId: string;
  peerId: string;
  resumeToken: string | null;
}

export interface ResumeAnonymousSessionResponse {
  sessionId: string;
  code: string;
  peerId: string;
  peerCount: number;
  reconnectedAtUtc: string;
}

export interface PublishAnonymousTextRequest {
  peerId: string;
  resumeToken: string | null;
  text: string | null;
}

export interface PublishAnonymousTextResponse {
  sessionId: string;
  peerId: string;
  publishedAtUtc: string;
  text: string;
}

export interface PublishAnonymousFileMetadataRequest {
  peerId: string;
  resumeToken: string | null;
  fileName: string | null;
  contentType: string | null;
  sizeBytes: number;
  shareCode?: string | null;
}

export interface PublishAnonymousFileMetadataResponse {
  sessionId: string;
  peerId: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  shareCode?: string | null;
  publishedAtUtc: string;
}

export interface RegisterRequest {
  email: string | null;
  password: string | null;
}

export interface LoginRequest {
  email: string | null;
  password: string | null;
}

export interface AuthSessionResponse {
  accountId: string;
  email: string;
  tier: AccountTier;
  sessionToken: string;
  sessionExpiresAtUtc: string;
}

export interface CurrentAccountResponse {
  accountId: string;
  email: string;
  tier: AccountTier;
  createdAtUtc: string;
}

export interface QuotaUsageResponse {
  tier: AccountTier;
  bytesUsedToday: number;
  bytesLimitToday: number;
  sharesCreatedToday: number;
  windowEndsAtUtc: string;
}

export interface HealthResponse {
  status: string;
  service?: string;
  timeUtc?: string;
}

export interface StoredAnonymousSession {
  sessionId: string;
  code: string;
  peerId: string;
  resumeToken: string;
  peerCount: number;
  reconnectGraceSeconds: number;
  restoredAtUtc?: string;
}

export interface EditorDraft {
  text: string;
  language: EditorLanguage;
  wrap: boolean;
  fullscreen: boolean;
}

export interface LocalClipboardItem {
  id: string;
  direction: 'sent' | 'received';
  kind: 'text' | 'file-metadata';
  text?: string;
  fileName?: string;
  contentType?: string;
  sizeBytes?: number;
  language?: EditorLanguage;
  createdAtUtc: string;
  sessionCode?: string;
  shareCode?: string;
}
