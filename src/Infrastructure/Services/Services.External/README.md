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

### Configuration

Add the following configuration in your `appsettings.json`:

```json
{
  "SmtpConfiguration": {
    "Server": "smtp.example.com",
    "Port": 587,
    "Username": "your-username",
    "Password": "your-password",
    "EnableSsl": true,
    "SenderEmail": "sender@example.com",
    "SenderName": "Your Sender Name"
  },
  "SmsConfiguration": {
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret",
    "SenderName": "YourCompany"
  }
}
```

### Service Registration

Register the services in your `Program.cs` or `Startup.cs`:

```csharp
using Services.External;

// Add SMTP services
services.AddSmtpService(Configuration);

// Add SMS services
services.AddSmsService(Configuration);
```

### Sending Emails

```csharp
using Services.External.SmtpService;

public class NotificationService
{
    private readonly ISmtpServices _smtpServices;

    public NotificationService(ISmtpServices smtpServices)
    {
        _smtpServices = smtpServices;
    }

    public void SendWelcomeEmail(string userEmail, string userName)
    {
        string subject = "Welcome to our platform!";
        string body = $"<h1>Hello {userName}!</h1><p>Welcome to our platform.</p>";
        
        _smtpServices.SendEmail(userEmail, subject, body, isHtml: true);
    }
    
    public void SendReportEmail(string userEmail, string reportPath)
    {
        string subject = "Your report is ready";
        string body = "Please find your report attached.";
        var attachments = new List<string> { reportPath };
        
        _smtpServices.SendEmail(userEmail, subject, body, 
            new List<string>(), new List<string>(), attachments);
    }
}
```

### Sending SMS Messages

```csharp
using Services.External.SmsService;

public class AlertService
{
    private readonly ISmsServices _smsServices;

    public AlertService(ISmsServices smsServices)
    {
        _smsServices = smsServices;
    }

    public void SendVerificationCode(string phoneNumber, string code)
    {
        string message = $"Your verification code is: {code}";
        _smsServices.SendSms(phoneNumber, message);
    }
    
    public void SendBulkNotification(List<string> phoneNumbers, string message)
    {
        _smsServices.SendSms(phoneNumbers, message);
    }
}
```

## Integration with Other Moclawr Packages

This package works seamlessly with other packages in the Moclawr ecosystem:

- **Moclawr.Core**: Leverages configuration models and utility extensions
- **Moclawr.Shared**: Uses standardized response models for consistent error handling
- **Moclawr.Host**: Perfect companion for building complete API solutions with global exception handling
- **Moclawr.Services.Caching**: Cache external service responses to improve performance and reduce costs
- **Moclawr.MinimalAPI**: Integrates with endpoint handlers for email/SMS functionality in APIs
- **Moclawr.DotNetCore.CAP**: Use with event-driven messaging for asynchronous notifications

## Requirements

- .NET 9.0 or higher
- Microsoft.AspNetCore.App framework reference

## License

This package is licensed under the MIT License.
