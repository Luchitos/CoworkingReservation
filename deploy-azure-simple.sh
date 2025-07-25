#!/bin/bash

# Script simplificado para desplegar en Azure con Container Registry
# Ejecutar: ./deploy-azure-simple.sh

set -e  # Salir si hay algún error

echo "🚀 Iniciando despliegue simplificado a Azure..."

# Variables
RESOURCE_GROUP="coworking-reservation-rg"
LOCATION="West US 2"
SQL_SERVER="coworking-reservation-sql"
DB_NAME="CoworkingReservationDb"
ACR_NAME="coworkingreservationacr"
CONTAINER_NAME="coworking-reservation-api"

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

print_status "Creando grupo de recursos..."
az group create --name $RESOURCE_GROUP --location "$LOCATION" --output none

print_status "Creando Azure Container Registry..."
az acr create \
    --resource-group $RESOURCE_GROUP \
    --name $ACR_NAME \
    --sku Basic \
    --admin-enabled true \
    --output none

print_status "Obteniendo credenciales de ACR..."
ACR_LOGIN_SERVER=$(az acr show --name $ACR_NAME --resource-group $RESOURCE_GROUP --query loginServer --output tsv)
ACR_USERNAME=$(az acr credential show --name $ACR_NAME --query username --output tsv)
ACR_PASSWORD=$(az acr credential show --name $ACR_NAME --query passwords[0].value --output tsv)

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

print_status "Construyendo imagen Docker para plataforma linux/amd64..."
docker build --platform linux/amd64 -t $ACR_LOGIN_SERVER/coworking-reservation-api:latest -f CoworkingReservation.API/Dockerfile .

print_status "Haciendo login en Azure Container Registry..."
az acr login --name $ACR_NAME

print_status "Subiendo imagen a Azure Container Registry..."
docker push $ACR_LOGIN_SERVER/coworking-reservation-api:latest

print_status "Creando Container Instance con imagen de ACR..."
az container create \
    --resource-group $RESOURCE_GROUP \
    --name $CONTAINER_NAME \
    --image $ACR_LOGIN_SERVER/coworking-reservation-api:latest \
    --registry-login-server $ACR_LOGIN_SERVER \
    --registry-username $ACR_USERNAME \
    --registry-password $ACR_PASSWORD \
    --os-type Linux \
    --cpu 1 \
    --memory 1.5 \
    --dns-name-label coworking-reservation-api \
    --ports 80 \
    --environment-variables \
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

print_status "Obteniendo URL del Container Instance..."
CONTAINER_IP=$(az container show --resource-group $RESOURCE_GROUP --name $CONTAINER_NAME --query ipAddress.fqdn --output tsv)

echo ""
echo "🎉 ¡Despliegue completado exitosamente!"
echo ""
echo "🌐 Tu API está disponible en:"
echo "   http://$CONTAINER_IP"
echo ""
echo "📊 Para monitorear:"
echo "   - Logs: az container logs --resource-group $RESOURCE_GROUP --name $CONTAINER_NAME"
echo "   - Estado: az container show --resource-group $RESOURCE_GROUP --name $CONTAINER_NAME"
echo ""
echo "🔧 Para reiniciar:"
echo "   az container restart --resource-group $RESOURCE_GROUP --name $CONTAINER_NAME"
echo ""
echo "🗑️  Para eliminar recursos:"
echo "   az group delete --name $RESOURCE_GROUP --yes" 