
var builder = DistributedApplication.CreateBuilder(args);

var backend = builder.AddProject(
        "finance-app-backend",
        "../FinanceApp.Backend.Presentation.WebApi/FinanceApp.Backend.Presentation.WebApi.csproj");

builder.AddExecutable(
    name: "finance-app-llmprocessor",
    command: ".venv/bin/uvicorn",
    args: new[] { "main:app", "--host", "0.0.0.0", "--port", "8000" },
    workingDirectory: "../../FinanceApp.LLMProcessor"
);

builder.AddExecutable(
    name: "finance-app-frontend",
    command: "npm",
    args: new[] { "run", "start" },
    workingDirectory: "../../FinanceApp.Frontend"
);

builder.Build().Run();
