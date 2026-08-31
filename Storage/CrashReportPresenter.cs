using AkengMauiCrashReporter.Configuration;
using AkengMauiCrashReporter.Formatting;
using AkengMauiCrashReporter.Models;
using AkengMauiCrashReporter.Services;

namespace AkengMauiCrashReporter.Storage
{
    internal sealed class CrashReportPresenter
    {
        private readonly CrashReporter _crashReporter;
        private readonly ICrashReportStore _reportStore;
        private readonly CrashReportFormatter _formatter;
        private readonly MauiCrashReporterOptions _options;

        private int _presentationStarted;

        public CrashReportPresenter(ICrashReportStore reportStore, CrashReportFormatter formatter, MauiCrashReporterOptions options, CrashReporter crashReporter)
        {
            ArgumentNullException.ThrowIfNull(reportStore);
            ArgumentNullException.ThrowIfNull(formatter);
            ArgumentNullException.ThrowIfNull(options);

            _reportStore = reportStore;
            _formatter = formatter;
            _options = options;
            _crashReporter = crashReporter;
        }

        public async Task TryPresentAsync(
    CancellationToken cancellationToken = default)
        {
            try
            {
                await File.WriteAllTextAsync(
                    Path.Combine(
                        FileSystem.AppDataDirectory,
                        "presenter-called.txt"),
                    DateTime.UtcNow.ToString("O"),
                    cancellationToken);
            }
            catch
            {
            }

            if (!_options.ShowCrashReportPopupOnNextStartup)
                return;

            if (Interlocked.Exchange(
                    ref _presentationStarted,
                    1) == 1)
            {
                return;
            }

            try
            {
                await _crashReporter.InitializationCompleted;

                try
                {
                    await File.WriteAllTextAsync(
                        Path.Combine(
                            FileSystem.AppDataDirectory,
                            "presenter-initialization-completed.txt"),
                        DateTime.UtcNow.ToString("O"),
                        cancellationToken);
                }
                catch
                {
                }

                var storedReport =
                    await _reportStore
                        .GetLatestUnpresentedFatalReportAsync(
                            cancellationToken);

                try
                {
                    var diagnostic =
                        storedReport is null
                            ? "NO REPORT"
                            : $"REPORT FOUND: {storedReport.Id}";

                    await File.WriteAllTextAsync(
                        Path.Combine(
                            FileSystem.AppDataDirectory,
                            "presenter-report.txt"),
                        diagnostic,
                        cancellationToken);
                }
                catch
                {
                }

                if (storedReport is null)
                    return;

                await PresentAsync(
                    storedReport,
                    cancellationToken);
            }
            catch
            {
            }
            finally
            {
                Volatile.Write(
                    ref _presentationStarted,
                    0);
            }
        }

        //public async Task TryPresentAsync(CancellationToken cancellationToken = default)
        //{
        //    if (!_options.ShowCrashReportPopupOnNextStartup)
        //        return;

        //    // Evite qu'Android OnResume / iOS OnActivated
        //    // déclenchent plusieurs popups simultanément.
        //    if (Interlocked.Exchange(ref _presentationStarted, 1) == 1)
        //    {
        //        return;
        //    }

        //    try
        //    {
        //        await _crashReporter.InitializationCompleted;

        //        var storedReport = await _reportStore.GetLatestUnpresentedFatalReportAsync(cancellationToken);

        //        if (storedReport is null)
        //            return;

        //        await PresentAsync(storedReport, cancellationToken);
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        throw;
        //    }
        //    catch
        //    {
        //        // Le crash reporter ne doit jamais
        //        // faire planter l'application hôte.
        //    }
        //    finally
        //    {
        //        Volatile.Write(ref _presentationStarted, 0);
        //    }
        //}

        private async Task PresentAsync(StoredCrashReport storedReport, CancellationToken cancellationToken)
        {
            var page = GetCurrentPage();

            if (page is null)
                return;

            var report = storedReport.Report;

            var message = BuildSummary(report);

            var actions = new List<string>();

            if (_options.AllowCopyCrashReport)
            {
                actions.Add("Copier le rapport");
            }

            if (_options.AllowShareCrashReport)
            {
                actions.Add("Partager le rapport");
            }

            if (actions.Count == 0)
            {
                await page.DisplayAlert("Crash précédent détecté", message, "Fermer");

                await MarkAsPresentedSafelyAsync(storedReport.Id, cancellationToken);

                return;
            }

            var selectedAction = await page.DisplayActionSheet(message, "Fermer", null, actions.ToArray());

            switch (selectedAction)
            {
                case "Copier le rapport":
                    await CopyAsync(report);
                    break;

                case "Partager le rapport":
                    await ShareAsync(report);
                    break;
            }

            await MarkAsPresentedSafelyAsync(storedReport.Id, cancellationToken);
        }

        private async Task CopyAsync(CrashReport report)
        {
            try
            {
                var text = _formatter.Format(report, CrashReportFormat.Text);

                await Clipboard.Default.SetTextAsync(text);
            }
            catch
            {
                // Copy failure must not crash host app.
            }
        }

        private async Task ShareAsync(CrashReport report)
        {
            try
            {
                var text = _formatter.Format(report, CrashReportFormat.Text);

                await Share.Default.RequestAsync(new ShareTextRequest
                {
                    Title = "Akeng MAUI Crash Report",
                    Text = text
                });
            }
            catch
            {
                // Share failure must not crash host app.
            }
        }

        private async Task MarkAsPresentedSafelyAsync(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _reportStore.MarkAsPresentedAsync(id, cancellationToken);
            }
            catch
            {
                // Storage failure must not crash host app.
            }
        }

        private static string BuildSummary(CrashReport report)
        {
            var exceptionType = report.Exception?.Type ?? "Unknown exception";

            var message = report.Exception?.Message ?? "No exception message.";

            var category = report.Diagnostic?.Classification?.Category.ToString();

            var summary =
                $"""
                L'application s'est arrêtée anormalement lors de la session précédente.

                Exception:
                {exceptionType}

                Message:
                {message}
                """;

            if (!string.IsNullOrWhiteSpace(category))
            {
                summary +=
                    $"""

                    
                    Diagnostic:
                    {category}
                    """;
            }

            if (!string.IsNullOrWhiteSpace(report.Fingerprint))
            {
                summary +=
                    $"""

                    
                    Fingerprint:
                    {report.Fingerprint}
                    """;
            }

            return summary;
        }

        private static Page? GetCurrentPage()
        {
            try
            {
                var application = Application.Current;

                if (application is null)
                    return null;

                var window = application.Windows.FirstOrDefault();

                if (window?.Page is null)
                    return null;

                return GetVisiblePage(window.Page);
            }
            catch
            {
                return null;
            }
        }

        private static Page GetVisiblePage(Page page)
        {
            return page switch
            {
                NavigationPage navigationPage when navigationPage.CurrentPage is not null => GetVisiblePage(navigationPage.CurrentPage),
                TabbedPage tabbedPage when tabbedPage.CurrentPage is not null => GetVisiblePage(tabbedPage.CurrentPage),
                Shell shell when shell.CurrentPage is not null => GetVisiblePage(shell.CurrentPage),
                _ => page
            };
        }
    }
}
