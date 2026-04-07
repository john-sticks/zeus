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
    Auth__Mode=Cerberus \
    cerberus__BaseUrl=http://172.17.0.1:8004 \
    ConnectionStrings__MySqlBase="Server=172.17.0.1;Port=3306;User=ariel;Connection Timeout=5;" \
    ConnectionStrings__MySqlAnalytics="Server=172.17.0.1;Port=3306;User=ariel;Connection Timeout=5;Database=SBInteligencia;"

EXPOSE 8008

ENTRYPOINT ["dotnet", "SBInteligencia.dll"]
