# Akeng.MauiCrashReporter

**Akeng.MauiCrashReporter** is a lightweight crash diagnostics library designed specifically for **.NET MAUI applications**.

It helps developers capture, persist, classify, and inspect application crashes — especially crashes occurring in **Release builds** where debugging information can be difficult to retrieve.

The main objective is simple:

> **Make .NET MAUI Release crashes easier to diagnose without requiring developers to immediately rely on `adb logcat`, Xcode device logs, or an external crash reporting platform.**

A fatal crash is captured and persisted locally. On the next application startup, Akeng.MauiCrashReporter processes the crash and can automatically display a diagnostic report to the developer or tester.

---

# ✨ Features

Akeng.MauiCrashReporter provides:

* 🚨 Automatic fatal exception capture
* 💾 Local crash persistence
* 🔄 Crash recovery and processing on the next application startup
* 📱 Native .NET MAUI crash report popup
* 📋 Copy crash reports to the clipboard
* 📤 Share crash reports using the native share sheet
* 🧭 Application lifecycle breadcrumbs
* 🧩 Manual breadcrumbs
* 🏷️ Crash classification
* 📊 Crash severity detection
* 📱 Device and application information
* 🧵 Exception and stack trace information
* 🔁 Crash-loop detection support
* 🗃️ Local crash report history
* 🛡️ Failure-isolated architecture designed to never crash the host application
* ⚙️ Minimal setup

---

# 📦 Installation

Install the package from NuGet:

```bash
dotnet add package Akeng.MauiCrashReporter
```

Or search for:

```text
Akeng.MauiCrashReporter
```

using the NuGet Package Manager in Visual Studio or JetBrains Rider.

---

# 🚀 Quick Start

Configure the crash reporter in `MauiProgram.cs`.

```csharp
using AkengMauiCrashReporter.Extensions;

namespace MyMauiApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseAkengMauiCrashReporter();

        return builder.Build();
    }
}
```

That's it.

No manual initialization is required.

You do **not** need to:

* resolve the crash reporter from the dependency injection container;
* manually initialize the crash reporter;
* register global exception handlers yourself;
* manually process pending crashes during startup.

Akeng.MauiCrashReporter automatically integrates with the MAUI application lifecycle.

---

# ⚙️ Configuration

You can customize the crash reporter using the options callback.

```csharp
builder
    .UseMauiApp<App>()
    .UseAkengMauiCrashReporter(options =>
    {
        options.CaptureUnhandledExceptions = true;
        options.CaptureUnobservedTaskExceptions = true;

        options.CaptureLifecycleEvents = true;

        options.ShowCrashReportPopupOnNextStartup = true;

        options.AllowCopyCrashReport = true;
        options.AllowShareCrashReport = true;

        options.EnableDiagnostics = true;
    });
```

All configuration is optional.

The default configuration is designed to provide useful crash diagnostics with minimal setup.

---

# 🚨 Fatal Crash Handling

Fatal crashes are handled differently from normal exceptions.

When an unhandled exception occurs, Akeng.MauiCrashReporter attempts to synchronously write a minimal emergency crash record to local storage.

The fatal crash path is intentionally kept small.

It avoids operations such as:

* HTTP requests;
* database access;
* complex serialization pipelines;
* expensive device diagnostics;
* long-running asynchronous operations.

Conceptually:

```text
Unhandled Exception
        │
        ▼
GlobalExceptionHandler
        │
        ▼
CrashReporter
        │
        ▼
EmergencyCrashWriter
        │
        ▼
Emergency crash persisted locally
        │
        ▼
Application terminates
```

This design increases the probability that useful crash information survives even when the application is about to terminate.

---

# 🔄 Crash Recovery on Next Startup

When the application starts again, pending emergency crashes are automatically processed.

```text
Application starts
        │
        ▼
CrashReporter initialization
        │
        ▼
PendingCrashStore
        │
        ▼
Emergency crash found
        │
        ▼
CrashReportFactory
        │
        ▼
Full CrashReport created
        │
        ▼
CrashReportStore
        │
        ▼
Crash report available
```

Once the emergency crash has successfully been converted into a full crash report, the pending emergency record can safely be removed.

---

# 📱 Automatic Crash Report Popup

By default, Akeng.MauiCrashReporter can display the previous fatal crash when the application is launched again.

Example flow:

```text
Application running
        │
        ▼
Fatal exception
        │
        ▼
Application crashes
        │
        ▼
User restarts application
        │
        ▼
Previous crash detected
        │
        ▼
Crash report popup
```

The popup allows the user, tester, or developer to inspect the crash and optionally perform actions such as:

```text
Copy
Share
Close
```

---

# 📋 Copy Crash Report

When enabled:

```csharp
options.AllowCopyCrashReport = true;
```

the crash report can be copied to the clipboard.

This makes it easy for testers to send diagnostic information directly to developers.

For example:

```text
Application: MyMauiApp
Version: 1.4.2
Platform: Android
Severity: Fatal

Exception:
System.InvalidOperationException

Message:
Unable to load customer profile.

Stack Trace:
...
```

The exact report content depends on the information available when the crash is processed.

---

# 📤 Share Crash Report

Native sharing can be enabled with:

```csharp
options.AllowShareCrashReport = true;
```

Akeng.MauiCrashReporter uses the .NET MAUI sharing APIs to open the platform's native share interface.

The report can then be sent through applications available on the device, such as:

* email clients;
* messaging applications;
* note applications;
* collaboration tools;
* other compatible applications.

---

# 👁️ Reports Are Presented Only Once

Stored reports contain presentation state.

A crash report that has already been presented is marked accordingly.

Conceptually:

```text
HasBeenPresented = false
        │
        ▼
Popup displayed
        │
        ▼
User closes/copies/shares
        │
        ▼
HasBeenPresented = true
```

This prevents the same crash popup from appearing every time the application starts.

---

# 🧭 Breadcrumbs

Breadcrumbs provide contextual information about what happened before a crash.

For example:

```text
ApplicationStarted
ApplicationResumed
PageAppearing
NavigationStarted
NavigationCompleted
```

These events can help reconstruct the sequence of actions leading to an exception.

Lifecycle breadcrumbs can be enabled with:

```csharp
options.CaptureLifecycleEvents = true;
```

---

# 🧩 Manual Breadcrumbs

Applications can also record custom breadcrumbs.

For example:

```csharp
crashReporter.AddBreadcrumb(
    "Checkout started");
```

or depending on the overload used by your application:

```csharp
crashReporter.AddBreadcrumb(
    "Payment",
    "User started payment");
```

Manual breadcrumbs are useful for recording important business events such as:

```text
UserAuthenticated
CartLoaded
PaymentStarted
PaymentCompleted
ProfileUpdated
ApiRequestStarted
ApiRequestFailed
```

When a crash occurs, recent breadcrumbs can provide valuable context.

> The exact breadcrumb API depends on the public `ICrashReporter` API available in the package version you are using.

---

# 🛠️ Manual Exception Capture

Handled exceptions can also be reported manually through `ICrashReporter`.

Example:

```csharp
public class PaymentService
{
    private readonly ICrashReporter _crashReporter;

    public PaymentService(
        ICrashReporter crashReporter)
    {
        _crashReporter = crashReporter;
    }

    public async Task ProcessPaymentAsync()
    {
        try
        {
            // Payment logic
        }
        catch (Exception exception)
        {
            await _crashReporter.CaptureExceptionAsync(
                exception);

            throw;
        }
    }
}
```

This is useful when an exception is handled by the application but should still be recorded for diagnostic purposes.

> Refer to the package's current `ICrashReporter` API for the exact available overloads.

---

# 🏷️ Crash Classification

Akeng.MauiCrashReporter can analyze exceptions and attempt to classify them into useful diagnostic categories.

Examples may include:

* XAML
* Binding
* Resources
* Dependency Injection
* Navigation
* Network
* Database
* Serialization
* Reflection
* AOT
* Memory
* Threading

Classification is intended to help developers quickly understand the probable area responsible for a crash.

For example:

```text
Category:
DependencyInjection

Evidence:
Unable to resolve service for type ...
```

or:

```text
Category:
Xaml

Evidence:
XamlParseException
```

Classification should be treated as diagnostic assistance rather than a guarantee of the exact root cause.

---

# 📊 Crash Severity

Crash reports can contain severity information.

Typical severity levels include:

```text
Fatal
Error
Warning
NonFatal
```

Unhandled exceptions that terminate the application are generally considered:

```text
Fatal
```

Unobserved task exceptions or manually captured exceptions may be classified differently depending on the capture context.

---

# 🧵 Exception Information

Crash reports can contain diagnostic exception information such as:

```text
Exception Type
Message
Stack Trace
Inner Exception
Crash Origin
Timestamp
Severity
Classification
```

The amount of information available depends on the exception and the runtime environment.

---

# 📱 Device and Application Information

Crash reports may also include contextual information about the running application and device.

Examples include:

```text
Application name
Application version
Build number
Operating system
Platform
OS version
Device model
Runtime information
Session information
```

This can be especially useful when a crash only occurs on a particular device, operating system version, or application build.

---

# 🌍 Environment Diagnostics

Akeng.MauiCrashReporter can collect runtime/environment information useful when investigating Release-only problems.

For example:

```text
Debug / Release
Runtime
Architecture
AOT status
Trimming status
Platform
```

Some runtime properties cannot always be reliably determined.

For this reason, values such as AOT or trimming state may be reported as unknown when the runtime does not provide enough information.

For example:

```text
AOT: Unknown
Trimming: Unknown
```

This is intentional.

A **Release build does not automatically mean that trimming or AOT is enabled**.

---

# 💾 Local Storage

Crash information is stored inside the application's local data directory.

The internal structure follows a layout similar to:

```text
AppDataDirectory/
└── Akeng/
    └── MauiCrashReporter/
        ├── Pending/
        ├── Reports/
        └── Corrupted/
```

### Pending

Contains emergency crash information waiting to be processed.

### Reports

Contains processed crash reports.

### Corrupted

Can be used to isolate crash files that cannot be correctly deserialized or processed.

Applications should not depend directly on this internal directory structure because it may evolve in future versions.

---

# 🛡️ Safety Philosophy

One of the most important design rules of Akeng.MauiCrashReporter is:

> **The crash reporter must never become the reason the host application crashes.**

Internal crash-reporting operations are therefore isolated whenever possible.

For example:

```text
Crash reporting failure
        │
        ▼
Failure contained internally
        │
        ▼
Host application continues
```

This principle applies to areas such as:

* initialization;
* crash persistence;
* report processing;
* lifecycle tracking;
* popup presentation;
* clipboard operations;
* sharing;
* environment diagnostics.

A diagnostics library should not introduce additional instability into the application it is monitoring.

---

# 🔌 Dependency Injection

Akeng.MauiCrashReporter integrates with the standard .NET MAUI dependency injection container.

After calling:

```csharp
.UseAkengMauiCrashReporter()
```

the public crash reporter can be injected into application services.

Example:

```csharp
public class MyService
{
    private readonly ICrashReporter _crashReporter;

    public MyService(
        ICrashReporter crashReporter)
    {
        _crashReporter = crashReporter;
    }
}
```

There is no need to manually resolve the internal crash reporter during application startup.

---

# 🏗️ Internal Architecture

At a high level, Akeng.MauiCrashReporter follows this architecture:

```text
                    ┌─────────────────────┐
                    │   .NET MAUI App     │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │ Global Exception    │
                    │ Handler             │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │ CrashReporter       │
                    └───────┬─────────────┘
                            │
              ┌─────────────┴─────────────┐
              │                           │
              ▼                           ▼
    ┌───────────────────┐       ┌────────────────────┐
    │ Emergency Crash   │       │ Normal Exception   │
    │ Writer            │       │ Processing         │
    └─────────┬─────────┘       └─────────┬──────────┘
              │                           │
              ▼                           │
    ┌───────────────────┐                 │
    │ PendingCrashStore │                 │
    └─────────┬─────────┘                 │
              │                           │
              └────────────┬──────────────┘
                           ▼
                 ┌────────────────────┐
                 │ CrashReportFactory │
                 └─────────┬──────────┘
                           │
                           ▼
                 ┌────────────────────┐
                 │ CrashReportStore   │
                 └─────────┬──────────┘
                           │
                           ▼
                 ┌────────────────────┐
                 │ CrashReport        │
                 │ Presenter          │
                 └────────────────────┘
```

---

# 🔄 Initialization

Akeng.MauiCrashReporter uses the MAUI initialization infrastructure.

The application developer does not need to call an initialization method manually.

Conceptually:

```text
MauiAppBuilder.Build()
        │
        ▼
MAUI initialization
        │
        ▼
Akeng crash reporter initialization
        │
        ├── Global handlers installed
        │
        ├── Pending crashes processed
        │
        └── Diagnostics initialized
```

Disk-related initialization is designed not to block application startup unnecessarily.

---

# 📱 Platform Lifecycle Integration

Akeng.MauiCrashReporter uses platform-specific MAUI lifecycle integration when necessary.

For example, on Android, lifecycle events can be used to detect when the application becomes active and determine when it is appropriate to present a previous crash report.

Because this functionality requires platform-specific compilation, the NuGet package provides platform-specific assemblies.

---

# 🎯 Supported Platforms

Akeng.MauiCrashReporter is designed for .NET MAUI applications running on:

| Platform     | Support |
| ------------ | ------- |
| Android      | ✅       |
| iOS          | ✅       |
| Mac Catalyst | ✅       |
| Windows      | ✅       |

Platform behavior can differ because exception handling and application lifecycle behavior are controlled partly by the operating system.

---

# 🧩 Target Frameworks

The package provides platform-specific target frameworks.

For .NET 9, targets include variants such as:

```text
net9.0-android35.0
net9.0-ios
net9.0-maccatalyst
net9.0-windows10.0.19041.0
```

For .NET 10, targets include variants such as:

```text
net10.0-android36.0
net10.0-ios
net10.0-maccatalyst
net10.0-windows10.0.19041.0
```

Platform-specific target frameworks are important because Akeng.MauiCrashReporter integrates with native MAUI lifecycle and exception mechanisms.

---

# ⚠️ Important Limitations

Akeng.MauiCrashReporter significantly improves diagnostics for managed .NET MAUI crashes, but it cannot capture every possible application termination.

Some failures occur outside the managed .NET exception pipeline.

Examples may include:

* native process aborts;
* operating system process termination;
* Android native tombstones;
* low-memory process kills;
* native runtime crashes;
* some AOT/runtime failures;
* failures occurring before the crash reporter has been initialized;
* hardware or operating-system-level termination.

In those cases, platform tools may still be required.

For Android:

```bash
adb logcat
```

For iOS:

```text
Xcode Device Logs
Crash Logs
Diagnostic Reports
```

Akeng.MauiCrashReporter is therefore intended to **reduce the need for platform logs**, not claim that platform diagnostics will never be necessary.

---

# 🧪 Testing Crash Capture

During development, you can intentionally throw an exception to verify crash capture.

For example:

```csharp
private void OnFatalCrashClicked(
    object sender,
    EventArgs e)
{
    throw new InvalidOperationException(
        "TEST: Fatal crash from Akeng.MauiCrashReporter");
}
```

Expected behavior:

```text
1. Start application

2. Trigger the exception

3. Application terminates

4. Restart application

5. Akeng.MauiCrashReporter processes
   the previous crash

6. Crash report popup appears

7. Copy / Share / Close can be used

8. The same report is not presented again
   after it has been marked as presented
```

> Do not leave intentional crash code in production applications.

---

# 🧪 Release Testing

Crash reporting should also be tested in a Release build.

For Android, for example:

```bash
dotnet build -c Release -f net9.0-android
```

Testing only in Debug mode is not sufficient because runtime behavior can differ between Debug and Release builds.

Release testing is especially important when applications use:

* trimming;
* AOT;
* linker optimizations;
* platform-specific code;
* reflection;
* dependency injection;
* serialization.

---

# 🔐 Privacy

Crash reports can potentially contain application-specific information originating from:

* exception messages;
* stack traces;
* breadcrumbs;
* manually supplied metadata.

Application developers are responsible for ensuring that sensitive information is not intentionally added to crash reports or breadcrumbs.

Avoid recording information such as:

```text
Passwords
Authentication tokens
API keys
Credit card numbers
Personal secrets
Sensitive user data
```

Applications handling sensitive data should review crash-report content before enabling sharing workflows in production environments.

---

# 💡 Recommended Usage

Akeng.MauiCrashReporter is particularly useful for:

* QA builds;
* internal testing;
* beta applications;
* enterprise applications;
* field testing;
* Release-only crash investigation;
* applications deployed on physical devices;
* environments where developers do not have immediate access to device logs.

A tester can reproduce a crash, restart the application, and share the generated diagnostic report with the development team.

---

# 📌 Typical QA Workflow

```text
Tester installs Release build
            │
            ▼
Tester uses application
            │
            ▼
Application crashes
            │
            ▼
Tester restarts application
            │
            ▼
Crash report appears
            │
            ▼
Tester selects Share
            │
            ▼
Developer receives diagnostic report
            │
            ▼
Investigation begins
```

This is one of the primary workflows Akeng.MauiCrashReporter is designed to simplify.

---

# 🗺️ Roadmap

Potential future improvements include:

* Remote crash report delivery
* Custom crash report endpoints
* Crash report sanitization
* Advanced sensitive-data filtering
* Custom popup presentation
* Additional crash classifiers
* Improved crash-loop diagnostics
* Export to JSON
* Export to text
* Custom metadata
* User/session identifiers
* Advanced breadcrumb filtering
* Configurable report retention
* Automatic cleanup policies
* Optional integrations with external monitoring platforms

---

# 🤝 Contributing

Contributions, bug reports, feature requests, and suggestions are welcome.

- Repository: https://github.com/Belagol/Akeng.CountryPicker
- Issues: https://github.com/Belagol/Akeng.CountryPicker/issues
- Pull Requests: https://github.com/Belagol/Akeng.CountryPicker/pulls

When reporting an issue, please provide as much relevant information as possible, including:

```text
.NET version
.NET MAUI version
Platform
OS version
Device / emulator
Debug or Release
AOT enabled or disabled
Trimming enabled or disabled
Exception type
Relevant crash report
Steps to reproduce
```

Please remove sensitive information before publishing crash reports in public issues.

---

# 🐛 Reporting Issues

If you encounter a problem with Akeng.MauiCrashReporter, create an issue in the project's GitHub repository.

A useful issue should include:

1. The package version.
2. The target framework.
3. The affected platform.
4. Steps to reproduce.
5. Expected behavior.
6. Actual behavior.
7. Crash report or relevant diagnostic information.

---

# 📄 License

MIT License

---

# ❤️ Akeng

**Akeng.MauiCrashReporter** is part of the Akeng ecosystem.

Akeng builds software components and developer tools with a focus on practical, reusable solutions for modern application development.

---

# ⭐ Support the Project

If Akeng.MauiCrashReporter helps you diagnose difficult .NET MAUI crashes:

* ⭐ Star the repository
* 🐛 Report issues
* 💡 Suggest improvements
* 🔀 Contribute fixes
* 📣 Share the project with other .NET MAUI developers

Feedback from real-world .NET MAUI applications is especially valuable for improving crash detection and diagnostics.

---

## Akeng.MauiCrashReporter

**Understand the crash. Fix it faster.**

---

Made with ❤️ by **Dr. Ange Gabriel Belinga** using .NET MAUI