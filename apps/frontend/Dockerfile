# Multi-stage Dockerfile para React Frontend
FROM node:18-alpine AS base
WORKDIR /app

# Instalar dependencias
FROM base AS deps
COPY package*.json ./
RUN npm ci --only=production && npm cache clean --force

# Imagen de desarrollo
FROM base AS development
COPY package*.json ./
RUN npm ci
COPY . .
EXPOSE 3000
ENV NODE_ENV=development
CMD ["npm", "run", "dev"]

# Build para producción
FROM base AS builder
COPY package*.json ./
RUN npm ci
COPY . .

# Build de la aplicación
RUN npm run build

# Imagen de producción con nginx
FROM nginx:alpine AS production
WORKDIR /usr/share/nginx/html

# Remover archivos default de nginx
RUN rm -rf ./*

# Copiar archivos construidos
COPY --from=builder /app/dist .
COPY nginx.conf /etc/nginx/nginx.conf

# Configurar nginx para SPA
RUN echo 'server { \
    listen 80; \
    server_name localhost; \
    root /usr/share/nginx/html; \
    index index.html; \
    location / { \
        try_files $uri $uri/ /index.html; \
    } \
    location /api { \
        proxy_pass http://weather-backend:80; \
        proxy_set_header Host $host; \
        proxy_set_header X-Real-IP $remote_addr; \
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for; \
        proxy_set_header X-Forwarded-Proto $scheme; \
    } \
}' > /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]