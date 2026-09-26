# storage-service (.NET 10, gRPC). Build a partir da raiz do repositório:
#   docker build -f backend/storage-service/Dockerfile -t fix/storage-service .
# O Fix.Storage.Infrastructure compila os contratos de ../../../../protos, por isso a árvore do repositório é preservada em /src.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY protos/ protos/
COPY backend/storage-service/Directory.Build.props backend/storage-service/Directory.Build.targets backend/storage-service/Fix.Storage.slnx backend/storage-service/
COPY backend/storage-service/src/Fix.Storage.Domain/Fix.Storage.Domain.csproj backend/storage-service/src/Fix.Storage.Domain/
COPY backend/storage-service/src/Fix.Storage.Application/Fix.Storage.Application.csproj backend/storage-service/src/Fix.Storage.Application/
COPY backend/storage-service/src/Fix.Storage.Infrastructure/Fix.Storage.Infrastructure.csproj backend/storage-service/src/Fix.Storage.Infrastructure/
COPY backend/storage-service/src/Fix.Storage.Presentation/Fix.Storage.Presentation.csproj backend/storage-service/src/Fix.Storage.Presentation/
RUN dotnet restore backend/storage-service/src/Fix.Storage.Presentation/Fix.Storage.Presentation.csproj

COPY backend/storage-service/src/ backend/storage-service/src/
RUN dotnet publish backend/storage-service/src/Fix.Storage.Presentation/Fix.Storage.Presentation.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5099 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_gcServer=0
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 5099
ENTRYPOINT ["dotnet", "Fix.Storage.Presentation.dll"]
