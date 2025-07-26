#!/bin/bash

# Script para desplegar el backend en Azure App Service con HTTPS automático
# Ejecutar: ./deploy-appservice-backend.sh

set -e  # Salir si hay algún error

echo "🚀 Iniciando despliegue del backend a Azure App Service..."

# Variables
RESOURCE_GROUP="coworking-reservation-rg"
LOCATION="West US 2"
SQL_SERVER="coworking-reservation-sql"
DB_NAME="CoworkingReservationDb"
APP_SERVICE_PLAN="coworking-reservation-plan"
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

print_status "Verificando login de Azure..."
if ! az account show &> /dev/null; then
    print_warning "No estás logueado en Azure. Iniciando login..."
    az login
fi

print_status "Eliminando recursos anteriores si existen..."
az group delete --name $RESOURCE_GROUP --yes --no-wait 2>/dev/null || true
print_status "Esperando que se eliminen los recursos anteriores..."
sleep 30

print_status "Creando grupo de recursos..."
az group create --name $RESOURCE_GROUP --location "$LOCATION" --output none

print_status "Creando servidor SQL..."
az sql server create \
    --name $SQL_SERVER \
    --resource-group $RESOURCE_GROUP \
    --location "$LOCATION" \
    --admin-user sqladmin \
    --admin-password "YourStrongPassword123!" \
    --output none

print_status "Creando base de datos SQL..."
az sql db create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER \
    --name $DB_NAME \
    --edition Basic \
    --capacity 5 \
    --output none

print_status "Configurando firewall de SQL Server..."
az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER \
    --name AllowAzureServices \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0 \
    --output none

print_status "Creando App Service Plan..."
az appservice plan create \
    --name $APP_SERVICE_PLAN \
    --resource-group $RESOURCE_GROUP \
    --location "$LOCATION" \
    --sku B1 \
    --is-linux \
    --output none

print_status "Creando App Service..."
az webapp create \
    --resource-group $RESOURCE_GROUP \
    --plan $APP_SERVICE_PLAN \
    --name $APP_SERVICE_NAME \
    --runtime "DOTNETCORE:8.0" \
    --deployment-local-git \
    --output none

print_status "Configurando variables de entorno..."
az webapp config appsettings set \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_NAME \
    --settings \
        ASPNETCORE_ENVIRONMENT=Production \
        ConnectionStrings__DefaultConnection="Server=$SQL_SERVER.database.windows.net;Database=$DB_NAME;User Id=sqladmin;Password=YourStrongPassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true" \
        JwtSettings__Key="YourSuperSecretJWTKeyHere123456789" \
        JwtSettings__Issuer="CoworkingReservationAPI" \
        JwtSettings__Audience="CoworkingReservationAPI" \
        JwtSettings__DurationInMinutes=60 \
        ImgBBSettings__ApiKey="3c74e91eee629405acaa7d13585fb61a" \
        ImgBBSettings__ApiUrl="https://api.imgbb.com/1/upload" \
        ImgBBSettings__ExpirationTime=0 \
    --output none

print_status "Configurando CORS para permitir el frontend..."
az webapp cors add \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_NAME \
    --allowed-origins "https://tesis-scaglia-signorelli.vercel.app" \
    --output none

print_status "Publicando la aplicación..."
cd CoworkingReservation.API
dotnet publish CoworkingReservation.API.csproj -c Release -o ./publish
cd ..

print_status "Creando archivo ZIP para el despliegue..."
cd CoworkingReservation.API/publish
zip -r ../publish.zip .
cd ../..

print_status "Desplegando ZIP a App Service..."
az webapp deployment source config-zip \
    --resource-group $RESOURCE_GROUP \
    --name $APP_SERVICE_NAME \
    --src ./CoworkingReservation.API/publish.zip \
    --output none

print_status "Obteniendo URL del App Service..."
APP_URL=$(az webapp show --resource-group $RESOURCE_GROUP --name $APP_SERVICE_NAME --query defaultHostName --output tsv)

echo ""
echo "🎉 ¡Despliegue del backend completado exitosamente!"
echo ""
echo "🌐 Tu API está disponible en:"
echo "   https://$APP_URL"
echo ""
echo "📊 Para monitorear:"
echo "   - Logs: az webapp log tail --resource-group $RESOURCE_GROUP --name $APP_SERVICE_NAME"
echo "   - Estado: az webapp show --resource-group $RESOURCE_GROUP --name $APP_SERVICE_NAME"
echo ""
echo "🔧 Para reiniciar:"
echo "   az webapp restart --resource-group $RESOURCE_GROUP --name $APP_SERVICE_NAME"
echo ""
echo "🗑️  Para eliminar recursos:"
echo "   az group delete --name $RESOURCE_GROUP --yes"
echo ""
echo "✅ HTTPS está habilitado automáticamente en App Service" 