export interface SessionJoinMessage {
  type: 'session.join';
  code?: string;
}

export interface SessionJoinedMessage {
  type: 'session.joined';
  sessionId: string;
  code: string;
  peerId: string;
}

export interface SessionResumeMessage {
  type: 'session.resume';
  sessionId: string;
  peerId: string;
}

export interface ContentTextPublishMessage {
  type: 'content.text.publish';
  sessionId: string;
  peerId: string;
  text: string;
}

export interface ContentTextReceivedMessage {
  type: 'content.text.received';
  sessionId: string;
  peerId: string;
  text: string;
  publishedAtUtc: string;
}

export interface ContentFileMetadataReceivedMessage {
  type: 'content.file-metadata.received';
  sessionId: string;
  peerId: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  shareCode?: string | null;
  publishedAtUtc: string;
}

export interface PeerConnectedMessage {
  type: 'peer.connected';
  sessionId: string;
  peerId: string;
  peerCount: number;
}

export type RealtimeIncomingMessage =
  | SessionJoinedMessage
  | ContentTextReceivedMessage
  | ContentFileMetadataReceivedMessage
  | PeerConnectedMessage;
