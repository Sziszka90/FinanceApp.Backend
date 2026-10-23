using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace FinanceApp.Backend.Presentation.WebApi.Extensions;

internal static class KeyVaultConfiguration
{
  public static IConfigurationBuilder AddFinanceAppKeyVault(this IConfigurationBuilder configuration)
  {
    var vaultUri = configuration.Build()["KeyVaultSettings:VaultUri"];
    if (string.IsNullOrWhiteSpace(vaultUri))
    {
      return configuration;
    }

    if (!Uri.TryCreate(vaultUri, UriKind.Absolute, out var parsedVaultUri) || parsedVaultUri.Scheme != Uri.UriSchemeHttps)
    {
      throw new InvalidOperationException("KeyVaultSettings:VaultUri must be a valid HTTPS Azure Key Vault URI.");
    }

    configuration.AddAzureKeyVault(
      parsedVaultUri,
      new DefaultAzureCredential(),
      new AzureKeyVaultConfigurationOptions
      {
        Manager = new FinanceAppKeyVaultSecretManager(),
        ReloadInterval = TimeSpan.FromHours(1)
      });

    return configuration;
  }
}

internal sealed class FinanceAppKeyVaultSecretManager : KeyVaultSecretManager
{
  private static readonly IReadOnlyDictionary<string, string> _secretConfigurationKeys =
    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
      ["cache-connection-string"] = "CacheSettings:ConnectionString",
      ["exchange-rate-api-app-id"] = "ExchangeRateSettings:AppId",
      ["finance-app-db-connection-string"] = "ConnectionStrings:MsSql",
      ["llm-processor-api-token"] = "LLMProcessorSettings:Token",
      ["openai-api-key"] = "OpenAISettings:ApiKey",
      ["rabbitmq-password"] = "RabbitMqSettings:Password",
      ["redis-password"] = "RedisSettings:Password",
      ["smtp-password"] = "SmtpSettings:SmtpPass",
      ["auth-secret-key"] = "AuthenticationSettings:SecretKey"
    };

  public override bool Load(SecretProperties secret)
  {
    return _secretConfigurationKeys.ContainsKey(secret.Name);
  }

  public override string GetKey(KeyVaultSecret secret)
  {
    return _secretConfigurationKeys[secret.Name];
  }
}
