# ===== Frontend build =====
FROM node:20-alpine AS frontend-build
WORKDIR /app/frontend
COPY frontend/package*.json ./
RUN npm ci --no-audit --no-fund
COPY frontend/ ./
RUN npm run build

# ===== Backend build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src
COPY backend/*.csproj backend/
RUN dotnet restore backend/KitchenAfterSales.Api.csproj
COPY backend/ backend/
RUN dotnet publish backend/KitchenAfterSales.Api.csproj -c Release -o /out

# ===== Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
# Copy backend
COPY --from=backend-build /out ./backend
# Copy frontend to nginx html
RUN apt-get update && apt-get install -y nginx && rm -rf /var/lib/apt/lists/*
RUN mkdir -p /var/www/frontend
COPY --from=frontend-build /app/frontend/dist/kitchen-after-sales/browser/ /var/www/frontend/
# Nginx conf
COPY nginx.conf /etc/nginx/nginx.conf
# Expose ports
EXPOSE 80
# Prepare uploads folder and set permissions
RUN mkdir -p /app/uploads && chmod -R 777 /app/uploads
# Entrypoint: start backend and nginx
CMD nginx && ASPNETCORE_URLS=http://0.0.0.0:5000 dotnet /app/backend/KitchenAfterSales.Api.dll