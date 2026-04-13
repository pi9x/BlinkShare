FROM node:22-alpine
WORKDIR /app

COPY tools/object-storage-mock/server.mjs ./server.mjs

EXPOSE 8080

CMD ["node", "server.mjs"]
