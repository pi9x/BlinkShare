#!/bin/sh
set -eu

cat >/usr/share/nginx/html/runtime-config.js <<EOF
window.__BLINKSHARE_RUNTIME_CONFIG__ = {
  apiBaseUrl: '${API_BASE_URL:-}',
  appName: '${APP_NAME:-BlinkShare}',
  anonymousMaxFileSizeBytes: ${ANONYMOUS_MAX_FILE_SIZE_BYTES:-524288},
  freeAccountMaxFileSizeBytes: ${FREE_ACCOUNT_MAX_FILE_SIZE_BYTES:-1048576},
};
EOF
