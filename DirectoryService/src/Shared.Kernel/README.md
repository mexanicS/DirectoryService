# MexanicS.SharedKernel

Shared error and pagination types for .NET services. `Result` and `UnitResult` are provided by the `CSharpFunctionalExtensions` dependency; this package does not redefine them.

The package targets .NET 10 and contains no ASP.NET Core or Entity Framework dependencies. The source repository is [DirectoryService](https://github.com/mexanicS/DirectoryService).

## Versions and releases

- **MAJOR**: an incompatible public API change, such as removing or renaming an error factory.
- **MINOR**: a backward-compatible public API addition.
- **PATCH**: a bug fix without a public API change.

Every stable release is tagged `vX.Y.Z` on the release commit. The release notes should link to that commit and describe the public changes. The `Version` in `Shared.Kernel.csproj` must match the intended stable tag. Pushes to `main` publish uniquely numbered preview packages; a `vX.Y.Z` tag publishes the stable package.

## Build and consume

Run `dotnet pack src/Shared.Kernel/Shared.Kernel.csproj --configuration Release` from the `DirectoryService` directory. This produces a `.nupkg` and a separate `.snupkg` with symbols.

The repository's `nuget.config` declares the GitHub Packages source without credentials. Set a `NuGetPackageSourceCredentials_github` environment variable in the form `Username=YOUR_GITHUB_USER;Password=YOUR_CLASSIC_PAT;ValidAuthenticationTypes=Basic` before restoring a private package locally. The token needs `read:packages`; a manual `dotnet nuget push` also needs `write:packages`. Do not commit credentials.

The source repository is public, so the package is published without an automatic repository link or inherited repository access. Create a repository secret named `GH_PACKAGES_PAT` containing a personal access token (classic) with `read:packages` and `write:packages`. The workflow uses that secret for every publication and checks that the package visibility is **private**. After the first publication, you can connect the repository in the package settings with **Inherit access from repository** disabled.
