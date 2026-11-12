# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0.300-noble AS build
WORKDIR /source

ARG GITHUB_USERNAME

COPY nuget.config DirectoryService.Build.props ./
COPY src/DirectoryService.Domain/DirectoryService.Domain.csproj src/DirectoryService.Domain/
COPY src/DirectoryService.Contract/DirectoryService.Contract.csproj src/DirectoryService.Contract/
COPY src/DirectoryService.Application/DirectoryService.Application.csproj src/DirectoryService.Application/
COPY src/DirectoryService.Infrastructure/DirectoryService.Infrastructure.csproj src/DirectoryService.Infrastructure/
COPY src/DirectoryService.Presentation/DirectoryService.Presentation.csproj src/DirectoryService.Presentation/

RUN --mount=type=secret,id=github_packages_token,env=GITHUB_PACKAGES_TOKEN,required=true \
    NuGetPackageSourceCredentials_github="Username=${GITHUB_USERNAME};Password=${GITHUB_PACKAGES_TOKEN};ValidAuthenticationTypes=Basic" \
    dotnet restore src/DirectoryService.Presentation/DirectoryService.Presentation.csproj --configfile /source/nuget.config

COPY src/ ./src/

RUN dotnet publish src/DirectoryService.Presentation/DirectoryService.Presentation.csproj \
    --configuration Release --output /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0.8-noble AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish/ ./

USER $APP_UID
ENTRYPOINT ["dotnet", "DirectoryService.Presentation.dll"]
