import { createServer } from 'node:http';
import { createReadStream, createWriteStream, existsSync, mkdirSync } from 'node:fs';
import { promises as fsPromises } from 'node:fs';
import path from 'node:path';
import { pipeline } from 'node:stream/promises';

const port = Number.parseInt(process.env.PORT ?? '8080', 10);
const dataRoot = path.resolve(process.env.DATA_ROOT ?? '/data');

mkdirSync(dataRoot, { recursive: true });

function sendJson(response, statusCode, payload) {
  response.writeHead(statusCode, { 'Content-Type': 'application/json' });
  response.end(JSON.stringify(payload));
}

function storagePathFromRequestPath(requestPath, prefix) {
  const encodedKey = requestPath.slice(prefix.length);
  const decodedKey = decodeURIComponent(encodedKey);
  const normalizedPath = path.normalize(decodedKey).replace(/^(\.\.(\/|\\|$))+/, '');
  const absolutePath = path.resolve(dataRoot, normalizedPath);

  if (!absolutePath.startsWith(dataRoot)) {
    return null;
  }

  return {
    absolutePath,
    storageKey: normalizedPath,
  };
}

const server = createServer(async (request, response) => {
  const requestUrl = new URL(request.url ?? '/', `http://${request.headers.host ?? 'localhost'}`);

  if (request.method === 'GET' && requestUrl.pathname === '/healthz') {
    return sendJson(response, 200, { status: 'ok' });
  }

  if (request.method === 'PUT' && requestUrl.pathname.startsWith('/upload/')) {
    const target = storagePathFromRequestPath(requestUrl.pathname, '/upload/');
    if (!target) {
      return sendJson(response, 400, { error: 'Invalid storage key.' });
    }

    await fsPromises.mkdir(path.dirname(target.absolutePath), { recursive: true });
    await pipeline(request, createWriteStream(target.absolutePath));

    return sendJson(response, 200, {
      status: 'stored',
      storageKey: target.storageKey,
    });
  }

  if (request.method === 'GET' && requestUrl.pathname.startsWith('/download/')) {
    const target = storagePathFromRequestPath(requestUrl.pathname, '/download/');
    if (!target) {
      return sendJson(response, 400, { error: 'Invalid storage key.' });
    }

    if (!existsSync(target.absolutePath)) {
      return sendJson(response, 404, { error: 'Object not found.' });
    }

    const headers = {
      'Content-Type': requestUrl.searchParams.get('contentType') || 'application/octet-stream',
      'Cache-Control': 'no-store',
    };

    const fileName = requestUrl.searchParams.get('fileName');
    if (fileName) {
      headers['Content-Disposition'] = `attachment; filename="${fileName.replaceAll('"', '')}"`;
    }

    response.writeHead(200, headers);
    await pipeline(createReadStream(target.absolutePath), response);
    return;
  }

  if (request.method === 'DELETE' && requestUrl.pathname.startsWith('/objects/')) {
    const target = storagePathFromRequestPath(requestUrl.pathname, '/objects/');
    if (!target) {
      return sendJson(response, 400, { error: 'Invalid storage key.' });
    }

    await fsPromises.rm(target.absolutePath, { force: true });
    return sendJson(response, 200, {
      status: 'deleted',
      storageKey: target.storageKey,
    });
  }

  return sendJson(response, 404, { error: 'Route not found.' });
});

server.listen(port, '0.0.0.0', () => {
  console.log(`object-storage-mock listening on ${port}`);
});
