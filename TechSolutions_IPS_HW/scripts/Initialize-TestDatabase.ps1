$env:ASPNETCORE_ENVIRONMENT = "Test"
dotnet ef database update --project .\TechSolutions_IPS_HW.csproj --startup-project .\TechSolutions_IPS_HW.csproj
