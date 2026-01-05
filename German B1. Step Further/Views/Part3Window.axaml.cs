using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using German_B1._Step_Further.Services;

namespace German_B1._Step_Further.Views
{
    public partial class Part3Window : Window
    {
        private int _currentHighlightedTopic = -1;

        public Part3Window()
        {
            InitializeComponent();

            // Connect window control buttons
            var closeButton = this.FindControl<Button>("CloseButton");
            var titleBarRow = this.FindControl<Grid>("TitleBarRow");

            if (closeButton != null)
                closeButton.Click += CloseButton_Click;

            // Enable window dragging from title bar
            if (titleBarRow != null)
            {
                titleBarRow.PointerPressed += TitleBar_PointerPressed;
            }

            // Connect all topic buttons
            ConnectTopicButtons();

            // Ensure page labels show absolute book pages (111-146)
            UpdatePageLabels();

            // Підписуємося на зміну сторінки для синхронізації
            NavigationService.PageChanged += OnPageChanged;
        }

        private void UpdatePageLabels()
        {
            // Each topic button contains a StackPanel with [title TextBlock, page-number TextBlock]
            for (int topicNumber = 1; topicNumber <= 12; topicNumber++)
            {
                var button = this.FindControl<Button>($"Topic3_{topicNumber}Button");
                if (button?.Content is StackPanel sp && sp.Children.Count >= 2 && sp.Children[1] is TextBlock pageTb)
                {
                    pageTb.Text = BookNavigationMap.GetTopicPageRangeLabel(3, topicNumber);
                }
            }
        }

        private void OnPageChanged(object? sender, PageChangedEventArgs e)
        {
            int leftPage = e.LeftPage;

            int topicNumber = BookNavigationMap.GetTopicNumberForLeftPage(3, leftPage);
            if (topicNumber > 0)
            {
                HighlightTopic(topicNumber);
            }
            else
            {
                ClearHighlight();
            }
        }

        private void HighlightTopic(int topicNumber)
        {
            if (_currentHighlightedTopic == topicNumber) return;

            // Прибираємо попереднє підсвічування
            ClearHighlight();

            // Підсвічуємо нову тему
            var button = this.FindControl<Button>($"Topic3_{topicNumber}Button");
            if (button != null)
            {
                button.Background = new SolidColorBrush(Color.Parse("#B3E5FC"));
                _currentHighlightedTopic = topicNumber;
            }
        }

        private void ClearHighlight()
        {
            if (_currentHighlightedTopic > 0)
            {
                var button = this.FindControl<Button>($"Topic3_{_currentHighlightedTopic}Button");
                if (button != null)
                {
                    button.Background = new SolidColorBrush(Color.Parse("#E3F2FD"));
                }
                _currentHighlightedTopic = -1;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            NavigationService.PageChanged -= OnPageChanged;
            base.OnClosed(e);
        }

        private void ConnectTopicButtons()
        {
            // Підключаємо обробники для всіх 12 тем Part 3
            for (int i = 1; i <= 12; i++)
            {
                var button = this.FindControl<Button>($"Topic3_{i}Button");
                if (button != null)
                {
                    button.Click += TopicButton_Click;
                }
            }
        }

        private void TopicButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tagString)
            {
                if (int.TryParse(tagString, out int topicNumber))
                {
                    // Передаємо Part=3 і номер теми
                    // MainWindow конвертує це в номер сторінки
                    NavigationService.RequestNavigation(3, topicNumber);
                }
            }
        }

        private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
