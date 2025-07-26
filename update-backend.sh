#!/bin/bash

set -e

echo "🚀 Actualizando backend con capacidad dinámica..."

# Variables
RESOURCE_GROUP="coworking-reservation-rg"
APP_SERVICE_NAME="coworking-reservation-api"

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Función para imprimir con colores
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_status "Publicando la aplicación con capacidad dinámica..."
cd CoworkingReservation.API
dotnet publish CoworkingReservation.API.csproj -c Release -o ./publish
cd publish
zip -r ../publish.zip .
cd ../..

print_status "Desplegando ZIP a App Service..."
az webapp deployment source config-zip \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_NAME \
    --src ./CoworkingReservation.API/publish.zip

echo "🎉 ¡Backend actualizado exitosamente!"
echo ""
echo "✅ Cambios implementados:"
echo "   - Capacidad total dinámica basada en áreas"
echo "   - Actualización automática al crear/editar/eliminar áreas"
echo "   - Soporte para expansión de espacios"
echo ""
echo "🌐 URL del backend:"
az webapp show --resource-group $RESOURCE_GROUP --name $APP_SERVICE_NAME --query defaultHostName --output tsv 