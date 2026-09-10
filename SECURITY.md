# Security Policy

Security is an important priority for Akeng.MauiCrashReporter.

Because the library is designed to capture and persist crash information from
.NET MAUI applications, special attention must be given to data handling,
local storage, diagnostics, and information that may appear in crash reports.

## Supported Versions

Security fixes are generally provided for the latest stable release of
Akeng.MauiCrashReporter.

| Version | Supported |
| --- | --- |
| Latest stable release | ✅ |
| Older releases | ⚠️ Best effort |
| Pre-release versions | ⚠️ Best effort |

Users are encouraged to upgrade to the latest stable version whenever possible.

## Reporting a Vulnerability

Please do not report security vulnerabilities through public GitHub Issues.

If you believe you have discovered a security vulnerability in
Akeng.MauiCrashReporter, please report it privately to the maintainers.

When reporting a vulnerability, include as much information as possible:

- affected package version;
- affected platform;
- .NET / .NET MAUI version;
- description of the vulnerability;
- steps required to reproduce it;
- potential impact;
- proof of concept, if available;
- suggested mitigation, if known.

Please avoid including real credentials, authentication tokens, API keys,
personal information, or other sensitive production data in the report.

## Response Process

The maintainers will make a reasonable effort to:

1. acknowledge receipt of a valid security report;
2. investigate and reproduce the issue;
3. evaluate its severity and impact;
4. prepare a fix or mitigation where appropriate;
5. coordinate disclosure when necessary;
6. publish a corrected package version as soon as reasonably possible.

Response times may vary depending on the complexity and severity of the issue.

## Crash Report Data

Akeng.MauiCrashReporter may collect diagnostic information such as:

- exception types;
- exception messages;
- stack traces;
- application information;
- device and runtime information;
- lifecycle breadcrumbs;
- manually supplied breadcrumbs or contextual information.

Applications integrating the package are responsible for ensuring that
sensitive information is not intentionally added to crash reports.

Developers should avoid recording:

- passwords;
- authentication tokens;
- API keys;
- session secrets;
- payment data;
- personal secrets;
- sensitive personal information.

Future versions of Akeng.MauiCrashReporter may provide additional sanitization
and redaction capabilities.

## Local Storage

Crash reports are persisted inside the application's local data directory.

The library should not intentionally expose crash reports outside the
application sandbox unless the user or application explicitly performs an
action such as copying or sharing a report.

## Dependency Security

Dependencies should be reviewed regularly for known vulnerabilities.

Where appropriate, dependency updates should be prioritized when they address
security issues.

## Security Design Principle

A core project principle is:

> Akeng.MauiCrashReporter must never introduce additional instability or
> become the cause of a crash in the host application.

Security-related failures inside the crash reporting pipeline should therefore
be isolated whenever reasonably possible.

## Responsible Disclosure

Please allow the maintainers reasonable time to investigate and address a
reported vulnerability before publishing technical details publicly.

Thank you for helping keep Akeng.MauiCrashReporter and its users secure.