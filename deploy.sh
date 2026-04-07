#!/usr/bin/env bash
set -e

IMAGE="johnsticks/sbinteligencia:latest"
CONTAINER="sbinteligencia"
DB_PASS="monitoreo"

echo "Pulling $IMAGE..."
docker pull $IMAGE

echo "Stopping old container (if exists)..."
docker stop $CONTAINER 2>/dev/null || true
docker rm   $CONTAINER 2>/dev/null || true

echo "Starting $CONTAINER..."
docker run -d \
  --name $CONTAINER \
  --restart unless-stopped \
  -p 8008:8008 \
  -e Auth__Mode=Cerberus \
  -e "cerberus__BaseUrl=http://172.17.0.1:8004" \
  -e "ConnectionStrings__MySqlBase=Server=172.17.0.1;Port=3306;User=simon;Password=${DB_PASS};Connection Timeout=5;" \
  -e "ConnectionStrings__MySqlAnalytics=Server=172.17.0.1;Port=3306;User=simon;Password=${DB_PASS};Connection Timeout=5;Database=SBInteligencia;" \
  $IMAGE

echo "Done. Container status:"
docker ps --filter name=$CONTAINER
