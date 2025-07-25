# Usar la imagen oficial de .NET 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Usar la imagen de SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["CoworkingReservation.API/CoworkingReservation.API.csproj", "CoworkingReservation.API/"]
COPY ["CoworkingReservation.Application/CoworkingReservation.Application.csproj", "CoworkingReservation.Application/"]
COPY ["CoworkingReservation.Domain/CoworkingReservation.Domain.csproj", "CoworkingReservation.Domain/"]
COPY ["CoworkingReservation.Infrastructure/CoworkingReservation.Infrastructure.csproj", "CoworkingReservation.Infrastructure/"]

RUN dotnet restore "CoworkingReservation.API/CoworkingReservation.API.csproj"

# Copiar todo el código fuente
COPY . .
WORKDIR "/src/CoworkingReservation.API"

# Compilar la aplicación
RUN dotnet build "CoworkingReservation.API.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "CoworkingReservation.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Crear directorio para logs
RUN mkdir -p /app/logs

# Configurar variables de entorno por defecto
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "CoworkingReservation.API.dll"] 