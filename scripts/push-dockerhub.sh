#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

DOCKERHUB_USERNAME="${DOCKERHUB_USERNAME:-pi9x}"

IMAGE_TAG="${IMAGE_TAG:-0.1.4}"
PLATFORM="${PLATFORM:-linux/amd64}"
PUSH_MODE="${PUSH_MODE:-push}"

if [[ "${PUSH_MODE}" == "push" ]]; then
  OUTPUT_FLAG="--push"
elif [[ "${PUSH_MODE}" == "load" ]]; then
  OUTPUT_FLAG="--load"
else
  echo "Unsupported PUSH_MODE: ${PUSH_MODE}. Use 'push' or 'load'." >&2
  exit 1
fi

build_image() {
  local name="$1"
  local dockerfile="$2"
  local target="${3:-}"
  local image="docker.io/${DOCKERHUB_USERNAME}/blinkshare-${name}:${IMAGE_TAG}"

  echo "Building ${image}"

  local cmd=(
    docker buildx build
    --platform "${PLATFORM}"
    --file "${dockerfile}"
    --tag "${image}"
    "${OUTPUT_FLAG}"
  )

  if [[ -n "${target}" ]]; then
    cmd+=(--target "${target}")
  fi

  cmd+=("${ROOT_DIR}")
  "${cmd[@]}"
}

build_image "api" "${ROOT_DIR}/src/backend/BlinkShare.Api/Dockerfile"
build_image "migrator" "${ROOT_DIR}/src/backend/BlinkShare.Api/Dockerfile" "migrator"
build_image "worker" "${ROOT_DIR}/src/backend/BlinkShare.Worker/Dockerfile"
build_image "frontend" "${ROOT_DIR}/src/frontend/Dockerfile"
build_image "garage-bootstrap" "${ROOT_DIR}/tools/garage-bootstrap/Dockerfile"

echo
echo "Images are ready under docker.io/${DOCKERHUB_USERNAME}/blinkshare-*:${IMAGE_TAG}"
