using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using System;
using System.IO;

namespace German_B1._Step_Further.Views
{
    public class ComplaintWindow : Window
    {
        private TextBox _complaintTextBox;

        // Define brush constants for TextBox styling
        private static readonly IBrush DialogInputBackground = new SolidColorBrush(Color.Parse("#2D2D3D"));
        private static readonly IBrush DialogInputBorder = new SolidColorBrush(Color.Parse("#4A4A5A"));
        private static readonly IBrush DialogInputCaret = Brushes.White;
        private static readonly IBrush DialogInputSelection = new SolidColorBrush(Color.Parse("#A78BFA"));
        private static readonly IBrush DialogInputSelectionForeground = Brushes.White;


        public ComplaintWindow()
        {
            Title = "Скарга на контент створений Ai";
            Width = 500;
            Height = 620;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            CanResize = false;
            ShowInTaskbar = false;
            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
            ExtendClientAreaTitleBarHeightHint = -1;
            Background = new SolidColorBrush(Color.Parse("#1E1E2E"));

            // Override theme resources to ensure consistent colors in all states (Focus, PointerOver)
            Resources["TextControlBackground"] = DialogInputBackground;
            Resources["TextControlBackgroundPointerOver"] = DialogInputBackground;
            Resources["TextControlBackgroundFocused"] = DialogInputBackground;
            
            Resources["TextControlForeground"] = Brushes.White;
            Resources["TextControlForegroundPointerOver"] = Brushes.White;
            Resources["TextControlForegroundFocused"] = Brushes.White;


            _complaintTextBox = new TextBox();
            BuildUI();
        }

        private void BuildUI()
        {
            var mainGrid = new Grid
            {
                RowDefinitions = RowDefinitions.Parse("Auto,*")
            };

            // Title Bar
            var titleBar = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#181825")),
                Height = 40,
                CornerRadius = new CornerRadius(8, 8, 0, 0)
            };

            var titleBarContent = new Grid
            {
                ColumnDefinitions = ColumnDefinitions.Parse("*,Auto")
            };

            var titleText = new TextBlock
            {
                Text = "Скарга на контент створений Ai",
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeight.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(16, 0, 0, 0)
            };
            Grid.SetColumn(titleText, 0);

            var closeButton = new Button
            {
                Width = 40,
                Height = 40,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
            };
            closeButton.Click += (_, _) => Close();

            var closeIcon = new TextBlock
            {
                Text = "✕",
                Foreground = new SolidColorBrush(Color.Parse("#64748B")),
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            closeButton.Content = closeIcon;
            Grid.SetColumn(closeButton, 1);

            titleBarContent.Children.Add(titleText);
            titleBarContent.Children.Add(closeButton);
            titleBar.Child = titleBarContent;

            titleBar.PointerPressed += (_, e) =>
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                    BeginMoveDrag(e);
            };

            Grid.SetRow(titleBar, 0);

            // Content
            var contentBorder = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#1E1E2E")),
                Padding = new Thickness(24),
                CornerRadius = new CornerRadius(0, 0, 8, 8)
            };

            var contentStack = new StackPanel
            {
                Spacing = 16
            };

            // Info icon and header
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var iconBorder = new Border
            {
                Width = 48,
                Height = 48,
                CornerRadius = new CornerRadius(24),
                Background = new SolidColorBrush(Color.Parse("#A78BFA"))
            };

            var iconText = new TextBlock
            {
                Text = "✉",
                FontSize = 24,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconBorder.Child = iconText;

            headerPanel.Children.Add(iconBorder);

            // Description
            var descriptionText = new TextBlock
            {
                Text = "Якщо ви виявили некоректний, образливий або помилковий контент, створений штучним інтелектом, повідомте нам. Ви можете надіслати скаргу на електронну пошту або зберегти як текстовий файл.",
                Foreground = new SolidColorBrush(Color.Parse("#CBD5E1")),
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                LineHeight = 22
            };

            // Email section
            var emailSection = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#2D2D3D")),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16)
            };

            var emailStack = new StackPanel
            {
                Spacing = 8
            };

            var emailLabel = new TextBlock
            {
                Text = "Електронна пошта для скарг:",
                Foreground = new SolidColorBrush(Color.Parse("#94A3B8")),
                FontSize = 12
            };

            var emailText = new SelectableTextBlock
            {
                Text = "vetalebrowser01@gmail.com",
                Foreground = new SolidColorBrush(Color.Parse("#A78BFA")),
                FontSize = 16,
                FontWeight = FontWeight.SemiBold
            };

            emailStack.Children.Add(emailLabel);
            emailStack.Children.Add(emailText);
            emailSection.Child = emailStack;

            // TextBox label
            var textBoxLabel = new TextBlock
            {
                Text = "Опишіть проблему з AI контентом:",
                Foreground = new SolidColorBrush(Color.Parse("#CBD5E1")),
                FontSize = 14,
                Margin = new Thickness(0, 8, 0, 0)
            };

            // TextBox for complaint
            _complaintTextBox = new TextBox
            {
                Watermark = "Вкажіть, який контент викликав проблему...",
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Height = 120,
                FontSize = 14,

                // Improve contrast vs container background
                Background = DialogInputBackground,
                Foreground = Brushes.White,
                BorderBrush = DialogInputBorder,
                BorderThickness = new Thickness(1.5),

                // Ensure editing visuals are visible
                CaretBrush = DialogInputCaret,
                SelectionBrush = DialogInputSelection,
                SelectionForegroundBrush = DialogInputSelectionForeground,

                Padding = new Thickness(12, 10),
                CornerRadius = new CornerRadius(6)
            };

            // Buttons panel
            var buttonPanel = new StackPanel
            {
                Spacing = 10,
                Margin = new Thickness(0, 8, 0, 0)
            };

            // Copy email button
            var copyButton = new Button
            {
                Content = "Копіювати email",
                Height = 40,
                Background = new SolidColorBrush(Color.Parse("#A78BFA")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                CornerRadius = new CornerRadius(6)
            };
            copyButton.Click += async (_, _) =>
            {
                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync("vetalebrowser01@gmail.com");
                    if (copyButton.Content is string)
                    {
                        copyButton.Content = "✓ Скопійовано!";
                        await System.Threading.Tasks.Task.Delay(2000);
                        copyButton.Content = "Копіювати email";
                    }
                }
            };

            // Save as file button
            var saveButton = new Button
            {
                Content = "Зберегти як файл",
                Height = 40,
                Background = new SolidColorBrush(Color.Parse("#4CAF50")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                CornerRadius = new CornerRadius(6)
            };
            saveButton.Click += SaveButton_Click;

            // Close button
            var closeBtn = new Button
            {
                Content = "Закрити",
                Height = 40,
                Background = new SolidColorBrush(Color.Parse("#2D2D3D")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                CornerRadius = new CornerRadius(6)
            };
            closeBtn.Click += (_, _) => Close();

            buttonPanel.Children.Add(copyButton);
            buttonPanel.Children.Add(saveButton);
            buttonPanel.Children.Add(closeBtn);

            contentStack.Children.Add(headerPanel);
            contentStack.Children.Add(descriptionText);
            contentStack.Children.Add(emailSection);
            contentStack.Children.Add(textBoxLabel);
            contentStack.Children.Add(_complaintTextBox);
            contentStack.Children.Add(buttonPanel);

            contentBorder.Child = contentStack;
            Grid.SetRow(contentBorder, 1);

            mainGrid.Children.Add(titleBar);
            mainGrid.Children.Add(contentBorder);

            Content = mainGrid;
        }

        private async void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var text = _complaintTextBox.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var storageProvider = StorageProvider;
            if (storageProvider == null) return;

            var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Зберегти скаргу",
                SuggestedFileName = $"Скарга_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Текстовий файл") { Patterns = new[] { "*.txt" } }
                }
            });

            if (file != null)
            {
                var content = $"Скарга на AI контент\n" +
                              $"Дата: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                              $"-----------------------------------\n\n" +
                              $"{text}\n\n" +
                              $"-----------------------------------\n" +
                              $"Відправити на: vetalebrowser01@gmail.com";

                await using var stream = await file.OpenWriteAsync();
                await using var writer = new StreamWriter(stream);
                await writer.WriteAsync(content);

                if (sender is Button btn)
                {
                    btn.Content = "✓ Збережено!";
                    await System.Threading.Tasks.Task.Delay(2000);
                    btn.Content = "Зберегти як файл";
                }
            }
        }
    }
}

