#!/bin/bash

# Script para actualizar el container con HTTPS
# Ejecutar: ./update-container-https.sh

# Variables
RESOURCE_GROUP="coworking-reservation-rg"
CONTAINER_NAME="coworking-reservation-api"
ACR_NAME="coworkingreservationacr"

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_status "Verificando login de Azure..."
az account show &> /dev/null
if [ $? -ne 0 ]; then
    print_error "No has iniciado sesión en Azure. Ejecuta 'az login' y vuelve a intentarlo."
    exit 1
fi

print_status "Obteniendo credenciales de ACR..."
ACR_LOGIN_SERVER=$(az acr show --name $ACR_NAME --resource-group $RESOURCE_GROUP --query loginServer --output tsv)
ACR_USERNAME=$(az acr credential show --name $ACR_NAME --query username --output tsv)
ACR_PASSWORD=$(az acr credential show --name $ACR_NAME --query passwords[0].value --output tsv)

print_status "Construyendo nueva imagen Docker con HTTPS..."
docker build --platform linux/amd64 -t $ACR_LOGIN_SERVER/coworking-reservation-api:latest -f CoworkingReservation.API/Dockerfile .

print_status "Haciendo login en Azure Container Registry..."
az acr login --name $ACR_NAME

print_status "Subiendo imagen actualizada a Azure Container Registry..."
docker push $ACR_LOGIN_SERVER/coworking-reservation-api:latest

print_status "Eliminando container instance anterior..."
az container delete --resource-group $RESOURCE_GROUP --name $CONTAINER_NAME --yes --output none

print_status "Creando nuevo Container Instance con HTTPS..."
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
    --ports 80 443 \
    --environment-variables \
        ASPNETCORE_ENVIRONMENT=Production \
        ASPNETCORE_URLS="http://+:80;https://+:443" \
        ConnectionStrings__DefaultConnection="Server=coworking-reservation-sql.database.windows.net;Database=CoworkingReservationDb;User Id=sqladmin;Password=YourStrongPassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true" \
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

print_success "🎉 ¡Container actualizado exitosamente!"
print_info "🌐 Tu API está disponible en:"
print_info "   HTTPS: https://$CONTAINER_IP"
print_info "   HTTP:  http://$CONTAINER_IP"

print_info "📊 Para monitorear:"
print_info "   - Logs: az container logs --resource-group $RESOURCE_GROUP --name $CONTAINER_NAME"
print_info "   - Estado: az container show --resource-group $RESOURCE_GROUP --name $CONTAINER_NAME"

print_info "🔧 Para reiniciar:"
print_info "   az container restart --resource-group $RESOURCE_GROUP --name $CONTAINER_NAME" 