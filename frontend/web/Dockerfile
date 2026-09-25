# Web (Vue 3 + Vite) servido por nginx. Build a partir da raiz do repositório:
#   docker build -f frontend/web/Dockerfile -t fix/web .
# A API é chamada em /api no mesmo origin (o Ingress encaminha /api para o BFF).

FROM node:22-alpine AS build
WORKDIR /app
COPY frontend/web/package.json frontend/web/package-lock.json ./
RUN npm ci
COPY frontend/web/ ./
RUN npm run build

FROM nginxinc/nginx-unprivileged:1.29-alpine AS runtime
COPY frontend/web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 8080
