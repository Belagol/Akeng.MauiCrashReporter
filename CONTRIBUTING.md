# Contributing to Akeng.MauiCrashReporter

Thank you for your interest in contributing to Akeng.MauiCrashReporter.

Akeng.MauiCrashReporter is an open-source project focused on providing
reliable, performant, and developer-friendly crash diagnostics for .NET MAUI
applications.

Contributions of all kinds are welcome, including bug reports, documentation
improvements, tests, platform-specific fixes, diagnostic improvements, and
new features.

## Code of Conduct

By participating in this project, you agree to follow the project's
Code of Conduct.

Please read `CODE_OF_CONDUCT.md` before contributing.

## Ways to Contribute

You can contribute by:

- reporting bugs;
- suggesting improvements;
- improving documentation;
- adding or improving tests;
- improving crash classification rules;
- improving platform-specific crash handling;
- improving performance or reliability;
- reviewing pull requests;
- reproducing reported issues;
- contributing new features.

Not every contribution needs to contain code.

Good bug reports and reproducible test cases are extremely valuable.

## Reporting Bugs

Before opening a new issue, please check whether the problem has already been
reported.

When reporting a bug, include as much relevant information as possible.

Useful information includes:

- Akeng.MauiCrashReporter version;
- .NET version;
- .NET MAUI version;
- target framework;
- platform;
- operating system version;
- device or emulator;
- CPU architecture, when relevant;
- Debug or Release configuration;
- trimming configuration, when relevant;
- AOT configuration, when relevant;
- steps to reproduce;
- expected behavior;
- actual behavior;
- relevant exception information;
- crash report, when available.

Example:

```text
Akeng.MauiCrashReporter: 1.0.x
.NET: 9
.NET MAUI: 9.x
Target: net9.0-android35.0
Platform: Android
Architecture: arm64
Configuration: Release

Steps to reproduce:
1. Start the application.
2. Navigate to ...
3. Perform ...
4. Application crashes.

Expected:
...

Actual:
...
```

Please remove sensitive information before attaching crash reports.

## Security Vulnerabilities

Do not report security vulnerabilities through public GitHub Issues.

Please follow the process described in [`SECURITY.md`](SECURITY.md).

Security reports should be submitted privately whenever possible.

## Feature Requests

Feature requests are welcome.

Before proposing a major feature, please consider opening a GitHub Issue first so the idea can be discussed before significant implementation work begins.

A useful feature request should explain:

- the problem being solved;
- why the problem is relevant to .NET MAUI developers;
- the expected behavior;
- possible alternatives;
- platform implications;
- potential compatibility concerns.

The project favors features that remain consistent with its core goals:

- reliability;
- performance;
- simple integration;
- useful Release diagnostics;
- cross-platform .NET MAUI support;
- minimal impact on the host application.

## Development Setup

You will need a supported .NET SDK and the appropriate .NET MAUI workloads.

Clone the repository:

```bash
git clone https://github.com/Belagol/Akeng.MauiCrashReporter.git
cd Akeng.MauiCrashReporter
```

Restore dependencies:

```bash
dotnet restore
```

Build the project:

```bash
dotnet build
```

Platform-specific builds can also be performed.

For example:

```bash
dotnet build -c Release -f net9.0-android35.0
```

The exact supported target frameworks may change between releases. Check the project file and README for the current list.

## Building the NuGet Package

The package can be created using:

```bash
dotnet pack -c Release
```

The build should produce the main NuGet package:

```text
Akeng.MauiCrashReporter.<version>.nupkg
```

and, when symbol packaging is enabled:

```text
Akeng.MauiCrashReporter.<version>.snupkg
```

Official releases should be reproducible from the publicly available source code, except where signing infrastructure requires protected credentials.

## Source Link and Symbols

The project uses Source Link and publishes debugging symbols when applicable.

Changes should not intentionally break:

- Source Link information;
- symbol generation;
- deterministic builds;
- NuGet package metadata.

## Development Principles

Akeng.MauiCrashReporter follows several important design principles.

### 1. Never Crash the Host Application

A crash reporting library must not become a new source of crashes.

Failures inside the diagnostic pipeline should therefore be isolated whenever reasonably possible.

### 2. Keep the Fatal Crash Path Minimal

When an application is terminating because of an unhandled exception, there may be very little time available.

The fatal crash path should avoid unnecessary work such as:

- network requests;
- database operations;
- expensive diagnostics;
- long asynchronous workflows;
- unnecessary allocations.

Whenever possible, minimal crash information should be persisted first and enriched safely on the next application startup.

### 3. Keep Integration Simple

One of the primary goals of the project is simple integration into a .NET MAUI application.

Avoid introducing mandatory initialization steps unless there is a strong technical reason.

The preferred developer experience should remain close to:

```csharp
builder
    .UseMauiApp<App>()
    .UseAkengMauiCrashReporter();
```

### 4. Be Platform Aware

Android, iOS, Mac Catalyst, and Windows have different application lifecycle and crash behavior.

Do not assume that a solution tested on one platform automatically behaves correctly on another platform.

Platform-specific changes should be tested on the affected platform whenever possible.

### 5. Preserve Compatibility

Changes should consider supported:

- .NET versions;
- .NET MAUI versions;
- platform target versions;
- device architectures.

Be especially careful when changing Target Framework Monikers (TFMs).

A library targeting a newer platform version may become incompatible with applications targeting an older platform version.

### 6. Protect Diagnostic Data

Crash reports may contain sensitive application information.

New diagnostic features should follow data minimization principles and avoid collecting information that is not necessary for diagnosing crashes.

Do not intentionally collect credentials, tokens, secrets, or sensitive personal information.

## Crash Classification Rules

Changes to crash classification should be evidence-based.

Classification rules should avoid producing confident diagnoses from weak signals.

A classification rule should preferably consider multiple signals such as:

- exception type;
- exception message;
- stack trace;
- runtime context;
- platform;
- Release-specific behavior.

When adding a classification rule, include tests demonstrating both:

- cases that should match;
- cases that should not match.

False positives should be treated seriously.

Classification is diagnostic assistance and must not be presented as a guaranteed root cause unless the available evidence supports that conclusion.

## Testing

Bug fixes and new functionality should include tests whenever practical.

Tests should cover:

- expected behavior;
- failure behavior;
- edge cases;
- regression scenarios.

Platform-specific functionality may require integration testing on a physical device or emulator.

For crash-handling changes, consider testing:

```text
Application starts
        ↓
Crash occurs
        ↓
Crash information is persisted
        ↓
Application terminates
        ↓
Application restarts
        ↓
Pending crash is processed
        ↓
Crash report is available
        ↓
Report is presented when configured
```

Release-mode testing is particularly important because some .NET MAUI issues appear only with Release configuration, trimming, AOT, or platform-specific optimizations.

## Pull Requests

Keep pull requests focused.

A pull request should ideally address one bug, feature, or clearly related set of changes.

Before submitting a pull request:

1. Ensure the project builds.
2. Run the relevant tests.
3. Add tests for new behavior when practical.
4. Update documentation when behavior changes.
5. Verify public APIs carefully.
6. Avoid unrelated formatting changes.
7. Ensure no credentials or sensitive information are committed.

The pull request description should explain:

- what changed;
- why the change is needed;
- how it was tested;
- affected platforms;
- compatibility implications;
- related GitHub Issues.

## Public API Changes

Changes to public APIs require particular care because applications may depend on them.

Avoid breaking changes whenever possible.

Before changing or removing a public API, consider:

- backward compatibility;
- migration path;
- semantic versioning;
- documentation updates;
- impact on existing applications.

Breaking changes should normally be reserved for a major version unless there is a compelling reason otherwise.

## Coding Style

Follow the existing code style of the repository.

General expectations include:

- enable nullable reference types;
- use meaningful names;
- keep methods focused;
- avoid unnecessary complexity;
- prefer explicit and maintainable code;
- document non-obvious platform behavior;
- avoid swallowing exceptions unless failure isolation is intentional.

When exceptions are intentionally suppressed to protect the host application, the reason should be clear from the surrounding code or documentation.

## Documentation

Public functionality should be documented.

Documentation changes are welcome and can be submitted independently of code changes.

Examples should be:

- minimal;
- compilable when possible;
- consistent with the current public API;
- explicit about platform limitations.

## Dependencies

Avoid introducing new dependencies unless they provide clear value.

New dependencies should be evaluated for:

- license compatibility;
- maintenance status;
- security history;
- package size;
- performance impact;
- transitive dependencies;
- compatibility with supported target frameworks.

Dependencies must remain compatible with the project's open-source licensing requirements.

## Commit Messages

Use clear and descriptive commit messages.

Examples:

```text
Fix Android crash presentation after restart

Add tests for XAML crash classification

Improve fatal crash persistence

Document Source Link configuration
```

Avoid vague messages such as:

```text
fix
changes
update
```

## Licensing

By contributing to Akeng.MauiCrashReporter, you agree that your contributions may be distributed under the license used by the project.

The project currently uses the MIT License.

The project may adopt the .NET Foundation Contributor License Agreement (CLA) process if required as part of future .NET Foundation membership.

## .NET Foundation Alignment

Akeng.MauiCrashReporter aims to follow open-source development practices consistent with the .NET Foundation's project requirements.

This includes:

- transparent development;
- publicly accessible source code;
- reproducible builds;
- clear licensing;
- security-conscious development;
- public issue tracking;
- welcoming external contributions;
- publicly accessible documentation;
- community participation.

These practices should be preserved as the project evolves.

## Questions

If you are unsure whether a proposed contribution fits the project, open a GitHub Discussion or Issue before starting a large implementation.

Small fixes and documentation improvements can generally be submitted directly as pull requests.

Thank you for contributing to Akeng.MauiCrashReporter.