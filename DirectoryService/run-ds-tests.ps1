Write-Host "Start integration tests Directory Service..." -ForegroundColor Cyan


$testProjectPath = "./Tests/DirectoryService.IntegrationTests/DirectoryService.IntegrationTests.csproj" 

dotnet test $testProjectPath --filter "Category=Integration&Service=DirectoryService" --logger "console;verbosity=detailed"

Write-Host "Testing complete" -ForegroundColor Green
Pause