#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

kubectl apply -f "${ROOT_DIR}/00-namespace.yaml"
kubectl apply -f "${ROOT_DIR}/01-configmap.yaml"
kubectl apply -f "${ROOT_DIR}/02-secret.yaml"
kubectl apply -f "${ROOT_DIR}/03-persistentvolumeclaims.yaml"
kubectl apply -f "${ROOT_DIR}/04-postgres.yaml"
kubectl apply -f "${ROOT_DIR}/05-redis.yaml"
kubectl apply -f "${ROOT_DIR}/06-object-storage.yaml"

kubectl rollout status deployment/postgres -n blinkshare --timeout=180s
kubectl rollout status deployment/redis -n blinkshare --timeout=180s
kubectl rollout status deployment/object-storage -n blinkshare --timeout=180s

kubectl delete job/object-storage-init -n blinkshare --ignore-not-found
kubectl apply -f "${ROOT_DIR}/06a-object-storage-init-job.yaml"
kubectl wait --for=condition=complete job/object-storage-init -n blinkshare --timeout=300s

kubectl delete job/migrate -n blinkshare --ignore-not-found
kubectl apply -f "${ROOT_DIR}/07-migrate-job.yaml"
kubectl wait --for=condition=complete job/migrate -n blinkshare --timeout=300s

kubectl apply -f "${ROOT_DIR}/08-api.yaml"
kubectl apply -f "${ROOT_DIR}/09-worker.yaml"
kubectl apply -f "${ROOT_DIR}/10-frontend.yaml"

kubectl rollout status deployment/api -n blinkshare --timeout=180s
kubectl rollout status deployment/worker -n blinkshare --timeout=180s
kubectl rollout status deployment/frontend -n blinkshare --timeout=180s

echo
echo "BlinkShare is deployed."
echo "Port-forward the frontend and Garage S3 endpoint with:"
echo "  kubectl -n blinkshare port-forward svc/frontend 8080:80"
echo "  kubectl -n blinkshare port-forward svc/object-storage 9000:9000"
