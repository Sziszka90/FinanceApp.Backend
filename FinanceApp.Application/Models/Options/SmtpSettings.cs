namespace FinanceApp.Application.Models.Options;

public class SmtpSettings
{
  /// <summary>
  /// Represents the host setting for SMTP email sending.
  /// </summary>
  public string SmtpHost { get; set; } = string.Empty;

  /// <summary>
  /// Represents the port setting for SMTP email sending.
  /// </summary>
  public int SmtpPort { get; set; }

  /// <summary>
  /// Represents the user name for SMTP authentication.
  /// </summary>
  public string SmtpUser { get; set; } = string.Empty;

  /// <summary>
  /// Represents the password for SMTP authentication.
  /// </summary>
  public string SmtpPass { get; set; } = string.Empty;

  /// <summary>
  /// Represents the email address from which emails will be sent.
  /// </summary>
  public string FromEmail { get; set; } = string.Empty;
}
