using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using German_B1._Step_Further.Services;

namespace German_B1._Step_Further.Views
{
    public partial class UserAgreementWindow : Window
    {
        public const string AgreementVersion = "1.0";

        private readonly bool _isInteractive;
        private UserAgreementLanguage _currentLanguage = UserAgreementLanguage.Ukrainian;

        public UserAgreementWindow(bool isInteractive = true)
        {
            _isInteractive = isInteractive;

            InitializeComponent();

            WireTitleBar();
            WireLanguageButtons();

            if (_isInteractive)
            {
                WireButtons();
            }
            else
            {
                // Info-only mode: hide Accept/Decline bar.
                var bar = this.FindControl<Grid>("ActionBar");
                if (bar != null)
                    bar.IsVisible = false;
            }

            ApplyLanguage(_currentLanguage);
        }

        private void WireTitleBar()
        {
            var closeButton = this.FindControl<Button>("CloseButton");
            var titleBarRow = this.FindControl<Grid>("TitleBarRow");

            if (closeButton != null)
            {
                // In startup mode closing acts like decline.
                closeButton.Click += (_, _) => Close(false);
            }

            if (titleBarRow != null)
                titleBarRow.PointerPressed += TitleBar_PointerPressed;
        }

        private void WireButtons()
        {
            var agreeButton = this.FindControl<Button>("AgreeButton");
            var declineButton = this.FindControl<Button>("DeclineButton");

            if (agreeButton != null)
                agreeButton.Click += AgreeButton_Click;

            if (declineButton != null)
                declineButton.Click += DeclineButton_Click;
        }

        private void WireLanguageButtons()
        {
            var uk = this.FindControl<Button>("LangUkButton");
            var de = this.FindControl<Button>("LangDeButton");
            var en = this.FindControl<Button>("LangEnButton");

            if (uk != null)
                uk.Click += (_, _) => SetLanguage(UserAgreementLanguage.Ukrainian);

            if (de != null)
                de.Click += (_, _) => SetLanguage(UserAgreementLanguage.German);

            if (en != null)
                en.Click += (_, _) => SetLanguage(UserAgreementLanguage.English);
        }

        private void SetLanguage(UserAgreementLanguage language)
        {
            _currentLanguage = language;
            ApplyLanguage(language);
        }

        private void ApplyLanguage(UserAgreementLanguage language)
        {
            var t = UserAgreementTextProvider.Get(language);

            Title = t.WindowTitle;

            var titleText = this.FindControl<TextBlock>("TitleText");
            if (titleText != null) titleText.Text = t.WindowTitle;

            var headerTitleText = this.FindControl<TextBlock>("HeaderTitleText");
            if (headerTitleText != null) headerTitleText.Text = t.HeaderTitle;

            var subtitleText = this.FindControl<TextBlock>("SubtitleText");
            if (subtitleText != null) subtitleText.Text = t.Subtitle;

            var bodyText = this.FindControl<TextBlock>("BodyText");
            if (bodyText != null) bodyText.Text = t.Body;

            var tipText = this.FindControl<TextBlock>("TipText");
            if (tipText != null) tipText.Text = t.Tip;

            if (_isInteractive)
            {
                var agreeButton = this.FindControl<Button>("AgreeButton");
                if (agreeButton != null) agreeButton.Content = new TextBlock { Text = t.AgreeButton, FontWeight = Avalonia.Media.FontWeight.SemiBold };

                var declineButton = this.FindControl<Button>("DeclineButton");
                if (declineButton != null) declineButton.Content = new TextBlock { Text = t.DeclineButton, FontWeight = Avalonia.Media.FontWeight.SemiBold };
            }
        }

        private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        private void AgreeButton_Click(object? sender, RoutedEventArgs e)
        {
            UserSettingsService.SetUserAgreementAccepted(AgreementVersion);
            Close(true);
        }

        private void DeclineButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
