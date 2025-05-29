# Moclawr.Services.External

[![NuGet](https://img.shields.io/nuget/v/Moclawr.Services.External.svg)](https://www.nuget.org/packages/Moclawr.Services.External/)

## Overview

The Moclawr.Services.External package provides implementations for integrating with external services such as SMTP for email sending and SMS gateways. It simplifies the process of configuring and using these external communication services in your .NET applications.

## Features

- **SMTP Services**: Comprehensive email sending capabilities with support for:
  - Single and multiple recipients
  - CC and BCC functionality
  - Email attachments
  - HTML and plain text email bodies
  - Configurable SMTP settings

- **SMS Services**: Text message sending functionality with support for:
  - Single recipient messaging
  - Bulk messaging to multiple recipients
  - Configurable SMS gateway settings

- **Dependency Injection**: Easy integration with ASP.NET Core applications through extension methods

## Installation

Install the package via NuGet Package Manager:

```shell
dotnet add package Moclawr.Services.External
```

## Usage

### Registering Services

In your `Program.cs` or `Startup.cs`:

```csharp
using Services.External;

// Register external services
builder.Services.AddExternalServices(builder.Configuration);
```

### Email Service Configuration

Configure SMTP settings in `appsettings.json`:

```json
{
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "Your Application"
  }
}
```

### Using Email Service

```csharp
using Services.External.Email;

public class NotificationService
{
    private readonly IEmailService _emailService;

    public NotificationService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendWelcomeEmailAsync(string userEmail, string userName)
    {
        var emailRequest = new EmailRequest
        {
            To = [userEmail],
            Subject = "Welcome to Our Platform!",
            Body = $"<h1>Welcome {userName}!</h1><p>Thank you for joining our platform.</p>",
            IsHtml = true
        };

        await _emailService.SendEmailAsync(emailRequest);
    }

    public async Task SendEmailWithAttachmentAsync(string userEmail, byte[] pdfData)
    {
        var emailRequest = new EmailRequest
        {
            To = [userEmail],
            Subject = "Your Report",
            Body = "Please find your report attached.",
            Attachments = [
                new EmailAttachment
                {
                    FileName = "report.pdf",
                    Content = pdfData,
                    ContentType = "application/pdf"
                }
            ]
        };

        await _emailService.SendEmailAsync(emailRequest);
    }
}
```

### SMS Service Configuration

Configure SMS settings in `appsettings.json`:

```json
{
  "SmsSettings": {
    "Provider": "Twilio",
    "AccountSid": "your-account-sid",
    "AuthToken": "your-auth-token",
    "FromNumber": "+1234567890"
  }
}
```

### Using SMS Service

```csharp
using Services.External.Sms;

public class SmsNotificationService
{
    private readonly ISmsService _smsService;

    public SmsNotificationService(ISmsService smsService)
    {
        _smsService = smsService;
    }

    public async Task SendVerificationCodeAsync(string phoneNumber, string code)
    {
        var smsRequest = new SmsRequest
        {
            ToNumber = phoneNumber,
            Message = $"Your verification code is: {code}. Valid for 10 minutes."
        };

        await _smsService.SendSmsAsync(smsRequest);
    }

    public async Task SendBulkNotificationAsync(List<string> phoneNumbers, string message)
    {
        var bulkSmsRequest = new BulkSmsRequest
        {
            ToNumbers = phoneNumbers,
            Message = message
        };

        await _smsService.SendBulkSmsAsync(bulkSmsRequest);
    }
}
```

## Integration with Other Moclawr Packages

This package works seamlessly with other packages in the Moclawr ecosystem:

- **Moclawr.Core**: Uses configuration extensions and utilities for enhanced functionality
- **Moclawr.Shared**: Integrates with response models and exception handling
- **Moclawr.Host**: Perfect for dependency injection and service registration
- **Moclawr.MinimalAPI**: Use with endpoint handlers for sending notifications
- **Moclawr.Services.Caching**: Cache notification templates and settings
- **Moclawr.DotNetCore.CAP**: Trigger notifications through event-driven messaging

## Requirements

- .NET 9.0 or higher
- MailKit 4.8.0 or higher (for SMTP services)
- Twilio 7.6.0 or higher (for SMS services, optional)

## License

This package is licensed under the MIT License.
