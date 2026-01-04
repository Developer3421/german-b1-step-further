using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using German_B1._Step_Further.Services;
using German_B1._Step_Further.Models;
using System.Collections.Generic;
using System.Linq;
using German_B1._Step_Further.Views;

namespace German_B1._Step_Further
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // 1) Require agreement on first launch (stored locally in AppData).
                if (!UserSettingsService.IsUserAgreementAccepted())
                {
                    // We must have an owner window for a modal dialog. Create a tiny hidden owner.
                    var owner = new Avalonia.Controls.Window
                    {
                        Width = 1,
                        Height = 1,
                        ShowInTaskbar = false,
                        Opacity = 0,
                        CanResize = false
                    };

                    desktop.MainWindow = owner;
                    owner.Show();

                    var agreementWindow = new UserAgreementWindow(isInteractive: true);
                    var accepted = await agreementWindow.ShowDialog<bool>(owner);

                    owner.Close();

                    if (!accepted)
                    {
                        desktop.Shutdown();
                        return;
                    }
                }

                // 2) Existing logic: restore session(s) and show main window(s).
                var savedSessions = SessionDatabaseService.LoadSessionsForRestore();

                if (savedSessions.Count > 0)
                {
                    // Restore ONLY one main window (the first session).
                    var mainSession = savedSessions.FirstOrDefault();
                    desktop.MainWindow = new MainWindow(mainSession);

                    // Ignore any additional saved windows.
                    SessionDatabaseService.ClearRestoredSessions();
                }
                else
                {
                    desktop.MainWindow = new MainWindow();
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}