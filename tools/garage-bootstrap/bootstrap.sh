#!/usr/bin/env bash
set -euo pipefail

CONFIG_FILE="${GARAGE_CONFIG_FILE:-/etc/garage/garage.toml}"
ACCESS_KEY_ID="${GARAGE_ACCESS_KEY_ID:?GARAGE_ACCESS_KEY_ID is required}"
SECRET_ACCESS_KEY="${GARAGE_SECRET_ACCESS_KEY:?GARAGE_SECRET_ACCESS_KEY is required}"
BUCKET_NAME="${GARAGE_BUCKET_NAME:?GARAGE_BUCKET_NAME is required}"
NODE_CAPACITY="${GARAGE_NODE_CAPACITY:-10G}"
S3_ENDPOINT_URL="${GARAGE_S3_ENDPOINT_URL:-http://object-storage:9000}"
CORS_ALLOWED_ORIGINS="${GARAGE_CORS_ALLOWED_ORIGINS:-*}"

garage_cmd() {
  /usr/local/bin/garage -c "${CONFIG_FILE}" "$@"
}

for _ in $(seq 1 60); do
  if garage_cmd status >/tmp/garage-status.txt 2>&1; then
    break
  fi
  sleep 2
done

if ! garage_cmd status >/tmp/garage-status.txt 2>&1; then
  cat /tmp/garage-status.txt >&2
  exit 1
fi

NODE_ID="$(grep -Eo '^[0-9a-f]+' /tmp/garage-status.txt | head -n 1)"
if [[ -z "${NODE_ID}" ]]; then
  cat /tmp/garage-status.txt >&2
  echo "Unable to determine Garage node ID." >&2
  exit 1
fi

if ! garage_cmd layout show 2>/tmp/garage-layout.err | grep -Eq '^[0-9a-f]{16}'; then
  garage_cmd layout assign "${NODE_ID}" -z dc1 -c "${NODE_CAPACITY}"
  garage_cmd layout apply --version 1
fi

if ! garage_cmd key list | grep -q "${ACCESS_KEY_ID}"; then
  garage_cmd key import -n blinkshare "${ACCESS_KEY_ID}" "${SECRET_ACCESS_KEY}" --yes
fi

if ! garage_cmd bucket list | grep -q "[[:space:]]${BUCKET_NAME}[[:space:]]*$"; then
  garage_cmd bucket create "${BUCKET_NAME}"
fi

garage_cmd bucket allow "${BUCKET_NAME}" --read --write --owner --key "${ACCESS_KEY_ID}"

cors_origins_json() {
  local origins="$1"
  local output=""
  local origin

  IFS=',' read -ra origin_list <<< "${origins}"
  for origin in "${origin_list[@]}"; do
    origin="${origin#"${origin%%[![:space:]]*}"}"
    origin="${origin%"${origin##*[![:space:]]}"}"
    if [[ -z "${origin}" ]]; then
      continue
    fi

    if [[ -n "${output}" ]]; then
      output+=", "
    fi
    output+="\"${origin}\""
  done

  printf '%s' "${output:-\"*\"}"
}

cat >/tmp/garage-cors.json <<EOF
{
  "CORSRules": [
    {
      "AllowedOrigins": [$(cors_origins_json "${CORS_ALLOWED_ORIGINS}")],
      "AllowedMethods": ["GET", "HEAD", "PUT"],
      "AllowedHeaders": ["*"],
      "ExposeHeaders": ["ETag"],
      "MaxAgeSeconds": 3000
    }
  ]
}
EOF

AWS_ACCESS_KEY_ID="${ACCESS_KEY_ID}" \
AWS_SECRET_ACCESS_KEY="${SECRET_ACCESS_KEY}" \
AWS_DEFAULT_REGION=garage \
aws configure set default.s3.addressing_style path

AWS_ACCESS_KEY_ID="${ACCESS_KEY_ID}" \
AWS_SECRET_ACCESS_KEY="${SECRET_ACCESS_KEY}" \
AWS_DEFAULT_REGION=garage \
aws --endpoint-url "${S3_ENDPOINT_URL}" \
  s3api put-bucket-cors \
  --bucket "${BUCKET_NAME}" \
  --cors-configuration file:///tmp/garage-cors.json
