# ============================================================
# Dockerfile — compila y corre el backend (Jascartec.Api) para
# desplegarlo en Railway (o cualquier hosting que use Docker).
# No afecta nada de la instalación local en USB — eso sigue
# siendo el .exe autocontenido de siempre.
# ============================================================

# ---------- Etapa 1: compilar ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos primero solo los .csproj para que Docker pueda reusar la capa
# de "restore" en los próximos builds si el código cambió pero las
# dependencias no (así cada despliegue es más rápido).
COPY backend/src/Jascartec.Domain/Jascartec.Domain.csproj src/Jascartec.Domain/
COPY backend/src/Jascartec.Application/Jascartec.Application.csproj src/Jascartec.Application/
COPY backend/src/Jascartec.Infrastructure/Jascartec.Infrastructure.csproj src/Jascartec.Infrastructure/
COPY backend/src/Jascartec.Api/Jascartec.Api.csproj src/Jascartec.Api/
RUN dotnet restore src/Jascartec.Api/Jascartec.Api.csproj

COPY backend/src/ src/
RUN dotnet publish src/Jascartec.Api/Jascartec.Api.csproj -c Release -o /app/publish --no-restore

# ---------- Etapa 2: imagen final, liviana (sin el SDK, solo el runtime) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
# Railway le pasa el puerto real en la variable $PORT; si no existe
# (por ejemplo, corriendo este Dockerfile en otro lado), usa 8080.
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet Jascartec.Api.dll"]
