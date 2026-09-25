# BFF (Node.js + TypeScript). Build a partir da raiz do repositório:
#   docker build -f frontend/bff/Dockerfile -t fix/bff .
# Os contratos gRPC (protos/) vão para /app/protos (PROTOS_DIR).

FROM node:22-alpine AS build
WORKDIR /app
COPY frontend/bff/package.json frontend/bff/package-lock.json ./
RUN npm ci
COPY frontend/bff/tsconfig.json ./
COPY frontend/bff/src ./src
RUN npm run build && npm prune --omit=dev

FROM node:22-alpine AS runtime
WORKDIR /app
ENV NODE_ENV=production \
    PORT=3000 \
    PROTOS_DIR=/app/protos
COPY --from=build /app/package.json ./
COPY --from=build /app/node_modules ./node_modules
COPY --from=build /app/dist ./dist
COPY protos ./protos
USER node
EXPOSE 3000
CMD ["node", "--import", "./dist/instrumentation.js", "dist/main.js"]
