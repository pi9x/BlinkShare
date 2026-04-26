# BlinkShare Kubernetes Deployment

This folder contains plain Kubernetes manifests for deploying BlinkShare with:

- `postgres`
- `redis`
- `object-storage`
- `object-storage-init` job
- `migrate` job
- `api`
- `worker`
- `frontend`

All persistent storage claims use the predefined storage class `nfs-storage`.

## 1. Build and push images

The repo already includes Dockerfiles for all runtime components. Use the helper script to build and push them to Docker Hub:

```bash
export DOCKERHUB_USERNAME=pi9x
export IMAGE_TAG=0.1.4
./scripts/push-dockerhub.sh
```

If you want to build locally without pushing:

```bash
export DOCKERHUB_USERNAME=pi9x
export PUSH_MODE=load
./scripts/push-dockerhub.sh
```

The script publishes:

- `docker.io/<username>/blinkshare-api:<tag>`
- `docker.io/<username>/blinkshare-migrator:<tag>`
- `docker.io/<username>/blinkshare-worker:<tag>`
- `docker.io/<username>/blinkshare-frontend:<tag>`
- `docker.io/<username>/blinkshare-garage-bootstrap:<tag>`

## 2. Image names

The manifests already point to the `pi9x` Docker Hub namespace by default for BlinkShare images.
Object storage uses the official `dxflrs/garage:v2.2.0` image, and the one-time bootstrap job uses `blinkshare-garage-bootstrap`.

The checked-in `02-secret.yaml` contains placeholders only so these manifests can be published safely.
Before deploying to a real cluster, replace every `CHANGE_ME_*` value with private values or manage them with a secret manager such as SOPS, Sealed Secrets, or External Secrets Operator.
Do not commit local secret files; `.gitignore` excludes common local secret manifest names under `k8s/`.

## 3. Deploy

Apply everything in the correct order:

```bash
./k8s/deploy.sh
```

The placeholder values in `01-configmap.yaml`, `02-secret.yaml`, `06-object-storage.yaml`, and `06a-object-storage-init-job.yaml` must be replaced before the app can serve a real public domain.

The deploy script waits for:

- stateful dependencies to become ready
- the migration job to complete
- the application deployments to roll out

## 4. Access the app

Port-forward the frontend and Garage S3 service:

```bash
kubectl -n blinkshare port-forward svc/frontend 8080:80
kubectl -n blinkshare port-forward svc/object-storage 9000:9000
```

Then open:

```text
http://localhost:8080
```

## Notes

- The frontend proxies `/api/*` and `/hubs/*` to the in-cluster service named `api`, so no frontend env override is required.
- File upload and download flows use direct pre-signed Garage URLs, so the Garage S3 endpoint must also be reachable from the browser.
- Using NFS for PostgreSQL and Redis is convenient for simple environments, but it is not ideal for high-performance production workloads.
