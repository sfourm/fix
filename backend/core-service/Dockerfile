# core-service (.NET 10, gRPC). Build a partir da raiz do repositório:
#   docker build -f backend/core-service/Dockerfile -t fix/core-service .
# O Fix.Presentation compila os contratos de ../../../../protos, por isso a árvore do repositório é preservada em /src.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY protos/ protos/
COPY backend/core-service/Directory.Build.props backend/core-service/Fix.slnx backend/core-service/
COPY backend/core-service/src/Fix.Domain/Fix.Domain.csproj backend/core-service/src/Fix.Domain/
COPY backend/core-service/src/Fix.Application/Fix.Application.csproj backend/core-service/src/Fix.Application/
COPY backend/core-service/src/Fix.Infrastructure/Fix.Infrastructure.csproj backend/core-service/src/Fix.Infrastructure/
COPY backend/core-service/src/Fix.Presentation/Fix.Presentation.csproj backend/core-service/src/Fix.Presentation/
RUN dotnet restore backend/core-service/src/Fix.Presentation/Fix.Presentation.csproj

COPY backend/core-service/src/ backend/core-service/src/
RUN dotnet publish backend/core-service/src/Fix.Presentation/Fix.Presentation.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5098 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_gcServer=0
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 5098
ENTRYPOINT ["dotnet", "Fix.Presentation.dll"]
