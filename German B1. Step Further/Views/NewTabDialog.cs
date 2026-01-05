using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using German_B1._Step_Further.Services;

namespace German_B1._Step_Further.Views
{
    public class NewTabDialog : Window
    {
        private TextBox _pageNumberTextBox;
        public int ResultPage { get; private set; } = -1;

        private readonly int _minPage = BookNavigationMap.MinPage;
        private readonly int _maxPage = BookNavigationMap.MaxPage;

        public NewTabDialog()
        {
            Title = "Нова вкладка";
            Width = 350;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            CanResize = false;
            ShowInTaskbar = false;
            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
            ExtendClientAreaTitleBarHeightHint = -1;
            Background = new SolidColorBrush(Color.Parse("#1E1E2E"));

            var inputBackground = new SolidColorBrush(Color.Parse("#252536"));

            // Override theme resources to ensure consistent colors in all states (Focus, PointerOver)
            Resources["TextControlBackground"] = inputBackground;
            Resources["TextControlBackgroundPointerOver"] = inputBackground;
            Resources["TextControlBackgroundFocused"] = inputBackground;

            Resources["TextControlForeground"] = Brushes.Black;
            Resources["TextControlForegroundPointerOver"] = Brushes.White;
            Resources["TextControlForegroundFocused"] = Brushes.White;


            _pageNumberTextBox = new TextBox();
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
                Text = "Перейти на сторінку",
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

            // Enable dragging
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

            var label = new TextBlock
            {
                Text = $"Введіть номер сторінки ({_minPage}-{_maxPage}):",
                Foreground = new SolidColorBrush(Color.Parse("#CBD5E1")),
                FontSize = 14
            };

            _pageNumberTextBox = new TextBox
            {
                Watermark = "Номер сторінки",
                FontSize = 14,
                
                // Dark background with white text
                Background = new SolidColorBrush(Color.Parse("#252536")),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.Parse("#A78BFA")),
                BorderThickness = new Thickness(1.5),
                
                // Purple caret, dark purple selection with white text
                CaretBrush = new SolidColorBrush(Color.Parse("#A78BFA")),
                SelectionBrush = new SolidColorBrush(Color.Parse("#4A3F6B")),
                SelectionForegroundBrush = Brushes.White,

                Padding = new Thickness(12, 10),
                CornerRadius = new CornerRadius(6)
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 10,
                Margin = new Thickness(0, 8, 0, 0)
            };

            var cancelButton = new Button
            {
                Content = "Скасувати",
                Width = 100,
                Height = 36,
                Background = new SolidColorBrush(Color.Parse("#2D2D3D")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                CornerRadius = new CornerRadius(6)
            };
            cancelButton.Click += (_, _) => Close();

            var okButton = new Button
            {
                Content = "Відкрити",
                Width = 100,
                Height = 36,
                Background = new SolidColorBrush(Color.Parse("#A78BFA")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                CornerRadius = new CornerRadius(6)
            };
            okButton.Click += OkButton_Click;

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(okButton);

            contentStack.Children.Add(label);
            contentStack.Children.Add(_pageNumberTextBox);
            contentStack.Children.Add(buttonPanel);

            contentBorder.Child = contentStack;
            Grid.SetRow(contentBorder, 1);

            mainGrid.Children.Add(titleBar);
            mainGrid.Children.Add(contentBorder);

            Content = mainGrid;
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            if (int.TryParse(_pageNumberTextBox.Text, out int page) && page >= _minPage && page <= _maxPage)
            {
                ResultPage = BookNavigationMap.ClampToValidLeftPage(page);
                Close();
            }
        }
    }
}
