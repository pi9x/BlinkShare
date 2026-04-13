FROM node:22-alpine AS build
WORKDIR /app

COPY src/frontend/package.json src/frontend/package-lock.json ./
RUN npm ci

COPY src/frontend/ ./
RUN npm run build

FROM nginx:1.27-alpine AS final
WORKDIR /usr/share/nginx/html

COPY src/frontend/nginx/default.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/blinkshare-frontend/browser ./

EXPOSE 80
