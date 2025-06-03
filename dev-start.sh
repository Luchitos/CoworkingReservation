#!/bin/bash

echo "🚀 Iniciando entorno de desarrollo..."

# 0. Asegurarse de que Docker Desktop esté abierto
echo "🐳 Abriendo Docker Desktop..."
open -a Docker

# 1. Esperar a que Docker esté operativo
echo "⏳ Esperando a que Docker esté listo..."
while ! docker info > /dev/null 2>&1; do
  sleep 1
done
echo "✅ Docker está activo."

# 2. Iniciar contenedor SQL Server
echo "📦 Iniciando contenedor Docker 'sql-coworking'..."
docker start sql-coworking

# 3. Backend .NET
echo "⚙️ Levantando backend .NET..."
cd CoworkingReservation.API || { echo "❌ No se encontró la carpeta CoworkingReservation.API"; exit 1; }
dotnet run &
BACKEND_PID=$!
cd ..

# 4. Frontend Angular
echo "🌐 Levantando frontend Angular..."
cd ../Tesis-Scaglia-Signorelli || { echo "❌ No se encontró la carpeta Tesis-Scaglia-Signorelli"; exit 1; }
npm start &
FRONTEND_PID=$!

# 5. Abrir URLs en navegador (Swagger + Frontend)
sleep 5
echo "🌍 Abriendo Swagger y Frontend en el navegador..."
open "http://localhost:5219/swagger/index.html"
open "http://localhost:4200/home"

# 6. Esperar a que ambos procesos terminen (Ctrl+C para cortar)
trap "kill $BACKEND_PID $FRONTEND_PID" EXIT
wait $BACKEND_PID $FRONTEND_PID
