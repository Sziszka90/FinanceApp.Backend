
var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject(
        "finance-app-backend",
        "../FinanceApp.Backend.Presentation.WebApi/FinanceApp.Backend.Presentation.WebApi.csproj")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

builder.AddExecutable(
    name: "finance-app-llmprocessor",
    command: "uv",
    args: new[]
    {
      "run", "--env-file", ".env.local", "--no-sync", "uvicorn",
      "main:app", "--host", "0.0.0.0", "--port", "8000"
    },
    workingDirectory: "../../FinanceApp.LLMProcessor")
    .WithHttpEndpoint(port: 8000, targetPort: 8000, name: "http", isProxied: false);

builder.AddExecutable(
    name: "finance-app-frontend",
    command: "npm",
    args: new[] { "run", "start", "--", "--host", "0.0.0.0", "--port", "4200" },
    workingDirectory: "../../FinanceApp.Frontend")
    .WithHttpEndpoint(port: 4200, targetPort: 4200, name: "http", isProxied: false);

builder.Build().Run();
