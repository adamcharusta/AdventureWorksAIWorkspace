# syntax=docker/dockerfile:1.7

###############################################################################
# Build stage — install deps, type-check, produce a static bundle.
###############################################################################
FROM node:22-alpine AS builder

WORKDIR /app

# Copy lockfile + .npmrc first so dependency layer caches independently of
# source changes. .npmrc holds `legacy-peer-deps=true` (jsx-a11y peer mismatch).
COPY package.json package-lock.json .npmrc ./

RUN --mount=type=cache,target=/root/.npm \
    npm ci

# Copy only what the production build needs (skips cypress/, tests, docs).
COPY index.html vite.config.ts tsconfig.json tsconfig.app.json tsconfig.node.json ./
COPY public/ ./public/
COPY src/ ./src/

RUN npm run build

###############################################################################
# Runtime stage — non-root nginx on port 8080 with envsubst-driven API proxy.
###############################################################################
FROM nginxinc/nginx-unprivileged:alpine AS runtime

# Default backend target — override at `docker run -e BACKEND_URL=...`.
ENV BACKEND_URL=http://api:8080

# Template gets envsubst'd at container start by the nginx-unprivileged entrypoint.
COPY --chown=nginx:nginx nginx.conf.template /etc/nginx/templates/default.conf.template

COPY --chown=nginx:nginx --from=builder /app/dist /usr/share/nginx/html

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:8080/ || exit 1
