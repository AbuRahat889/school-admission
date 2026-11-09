# ---------- build ----------
FROM node:20-alpine AS build
WORKDIR /app

# Install deps first so this layer is cached when only source changes.
COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund

COPY . .
# Vite inlines VITE_* variables at build time, so the API URL is a build arg.
# It is the URL the *browser* uses, hence localhost:8080 (the backend's published port).
ARG VITE_API_URL=http://localhost:8080
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

# ---------- serve ----------
FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
