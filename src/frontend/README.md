# BlinkShare Frontend

Angular 21 frontend for BlinkShare, implemented in `/src/frontend` against the current backend slices in `/src/backend/BlinkShare.Api`.

## Stack

- Angular 21 standalone components
- Angular Router with route-level lazy loading
- HttpClient with bearer-token interceptor
- Signals for local feature state
- Tailwind CSS v4
- CodeMirror 6 for the editor surface

## Implemented surfaces

- `/`
  Main workspace for:
  - anonymous session create/join/resume
  - developer-oriented text editor with local draft persistence
  - stored text share creation
  - stored file share upload + complete flow
  - anonymous file-metadata relay
  - browser-local feed/history
- `/share/:code`
  Public share metadata, unlock, text read, and file download flow
- `/auth/login`
- `/auth/register`
- `/me`
  Authenticated account and quota summary

## Local development

1. Start the backend API from `src/backend/BlinkShare.Api`.
   Default development URL from the current launch settings is `http://localhost:5232`.
2. Start the frontend:

```bash
cd src/frontend
npm install
npm start
```

The Angular dev server uses `proxy.conf.json`, so `/api/*` and `/healthz` requests are proxied to `http://localhost:5232`.

## Docker Compose

The repo root now includes `docker-compose.yml` for the full local stack:

- frontend on `http://localhost:4200`
- API on `http://localhost:5232`
- PostgreSQL on `localhost:5432`
- Redis on `localhost:6379`
- object storage mock on `localhost:9000`

Run it from the repo root:

```bash
docker compose up --build
```

Notes:

- The `migrate` service runs EF Core migrations before the API and worker start.
- The object storage service is a local mock that makes BlinkShare's current development upload/download URLs usable in Docker.

## Verification

Production build passes with:

```bash
npm run build
```

The compiled output is written to `src/frontend/dist/blinkshare-frontend`.
