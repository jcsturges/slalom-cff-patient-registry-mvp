# ── Stage 1: Node build ───────────────────────────────────────────────────────
FROM node:20-alpine AS build
WORKDIR /app

# Build-time args for Vite (baked into JS bundle)
ARG VITE_API_URL=http://localhost:5000
ARG VITE_OKTA_ISSUER
ARG VITE_OKTA_CLIENT_ID
ARG VITE_OKTA_REDIRECT_URI=http://localhost:3000/login/callback
ENV VITE_API_URL=$VITE_API_URL
ENV VITE_OKTA_ISSUER=$VITE_OKTA_ISSUER
ENV VITE_OKTA_CLIENT_ID=$VITE_OKTA_CLIENT_ID
ENV VITE_OKTA_REDIRECT_URI=$VITE_OKTA_REDIRECT_URI

# Restore deps (layer-cached unless package-lock.json changes)
COPY package*.json ./
RUN npm ci --prefer-offline

# Build production bundle
COPY . .
RUN npm run build

# ── Stage 2: nginx serving ────────────────────────────────────────────────────
FROM nginx:1.27-alpine AS final

# Non-root user
RUN addgroup -S -g 1001 webapp \
 && adduser  -S -u 1001 -G webapp webapp

COPY --from=build /app/dist /usr/share/nginx/html

# nginx config: SPA fallback (serve index.html for all routes)
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Adjust permissions so non-root nginx works
RUN chown -R webapp:webapp /usr/share/nginx/html \
 && chown -R webapp:webapp /var/cache/nginx \
 && chown -R webapp:webapp /var/log/nginx \
 && touch /var/run/nginx.pid \
 && chown webapp:webapp /var/run/nginx.pid

EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD wget -qO /dev/null http://localhost:8080/health.txt || exit 1

USER webapp
CMD ["nginx", "-g", "daemon off;"]
