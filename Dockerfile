# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY src/SBInteligencia/SBInteligencia.csproj src/SBInteligencia/
RUN dotnet restore src/SBInteligencia/SBInteligencia.csproj

COPY . .
RUN dotnet publish src/SBInteligencia/SBInteligencia.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0

ENV TZ=America/Argentina/Buenos_Aires
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8008 \
    DB_HOST=172.17.0.1 \
    DB_PORT=3306 \
    CERBERUS_HOST=172.17.0.1 \
    CERBERUS_PORT=8004

EXPOSE 8008

ENTRYPOINT ["dotnet", "SBInteligencia.dll"]
