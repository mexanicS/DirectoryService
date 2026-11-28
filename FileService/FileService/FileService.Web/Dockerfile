# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG GITHUB_USERNAME
WORKDIR /src

# Restore uses the same NuGet sources and shared build settings as the local build.
COPY nuget.config Directory.Build.props ./
COPY ["FileService.Web/FileService.Web.csproj", "FileService.Web/"]
COPY ["FileService.Core/FileService.Core.csproj", "FileService.Core/"]
COPY ["FileService.Domain/FileService.Domain.csproj", "FileService.Domain/"]
COPY ["FileService.Contracts/FileService.Contracts.csproj", "FileService.Contracts/"]
COPY ["FileService.Infrastructure.Postgres/FileService.Infrastructure.Postgres.csproj", "FileService.Infrastructure.Postgres/"]
COPY ["FileService.Infrastructure.S3/FileService.Infrastructure.S3.csproj", "FileService.Infrastructure.S3/"]

# The GitHub token is available only during this restore instruction.
RUN --mount=type=secret,id=github_packages_token,env=GITHUB_PACKAGES_TOKEN,required=true \
    NuGetPackageSourceCredentials_github="Username=$GITHUB_USERNAME;Password=$GITHUB_PACKAGES_TOKEN;ValidAuthenticationTypes=Basic" \
    dotnet restore "FileService.Web/FileService.Web.csproj" \
    --configfile /src/nuget.config -p:Configuration="$BUILD_CONFIGURATION"

COPY . .
WORKDIR /src/FileService.Web

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FileService.Web.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileService.Web.dll"]