using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using German_B1._Step_Further.Services;

namespace German_B1._Step_Further.Views
{
    public partial class Part4Window : Window
    {
        private int _currentHighlightedTopic = -1;

        public Part4Window()
        {
            InitializeComponent();

            // Connect window control buttons
            var closeButton = this.FindControl<Button>("CloseButton");
            var titleBarRow = this.FindControl<Grid>("TitleBarRow");

            if (closeButton != null)
                closeButton.Click += CloseButton_Click;

            // Enable window dragging from the title bar
            if (titleBarRow != null)
            {
                titleBarRow.PointerPressed += TitleBar_PointerPressed;
            }

            // Connect all topic buttons
            ConnectTopicButtons();

            // Ensure page labels show absolute book pages (147-164)
            UpdatePageLabels();

            // Subscribe to page change for synchronization
            NavigationService.PageChanged += OnPageChanged;
        }

        private void UpdatePageLabels()
        {
            // Each topic button contains a StackPanel with [title TextBlock, page-number TextBlock]
            for (int topicNumber = 1; topicNumber <= 6; topicNumber++)
            {
                var button = this.FindControl<Button>($"Topic4_{topicNumber}Button");
                if (button?.Content is StackPanel { Children: [_, TextBlock pageTb, ..] })
                {
                    pageTb.Text = BookNavigationMap.GetTopicPageRangeLabel(4, topicNumber);
                }
            }
        }

        private void OnPageChanged(object? sender, PageChangedEventArgs e)
        {
            int leftPage = e.LeftPage;

            int topicNumber = BookNavigationMap.GetTopicNumberForLeftPage(4, leftPage);
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

            // Clear previous highlighting
            ClearHighlight();

            // Highlight new topic
            var button = this.FindControl<Button>($"Topic4_{topicNumber}Button");
            if (button != null)
            {
                button.Background = new SolidColorBrush(Color.Parse("#CE93D8"));
                _currentHighlightedTopic = topicNumber;
            }
        }

        private void ClearHighlight()
        {
            if (_currentHighlightedTopic > 0)
            {
                var button = this.FindControl<Button>($"Topic4_{_currentHighlightedTopic}Button");
                if (button != null)
                {
                    button.Background = new SolidColorBrush(Color.Parse("#F3E5F5"));
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
            // Connect handlers for all 6 topics Part 4
            for (int i = 1; i <= 6; i++)
            {
                var button = this.FindControl<Button>($"Topic4_{i}Button");
                if (button != null)
                {
                    button.Click += TopicButton_Click;
                }
            }
        }

        private void TopicButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: string tagString })
            {
                if (int.TryParse(tagString, out int topicNumber))
                {
                    // Pass Part=4 and topic number
                    // MainWindow converts this to page number
                    NavigationService.RequestNavigation(4, topicNumber);
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
