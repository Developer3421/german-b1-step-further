using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using German_B1._Step_Further.Services;

namespace German_B1._Step_Further.Views
{
    public partial class Part2Window : Window
    {
        private int _currentHighlightedTopic = -1;
        
        public Part2Window()
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
            
            // Subscribe to page change for synchronization
            NavigationService.PageChanged += OnPageChanged;
        }
        
        private void OnPageChanged(object? sender, PageChangedEventArgs e)
        {
            // Determine which topic corresponds to current pages
            // Part 2: pages 57-110
            int leftPage = e.LeftPage;
            
            if (leftPage >= 57 && leftPage <= 110)
            {
                // Topic = (page - 57) / 3 + 1
                int topicNumber = (leftPage - 57) / 3 + 1;
                HighlightTopic(topicNumber);
            }
            else
            {
                // Pages not from part 2 - clear highlighting
                ClearHighlight();
            }
        }
        
        private void HighlightTopic(int topicNumber)
        {
            if (_currentHighlightedTopic == topicNumber) return;
            
            // Clear previous highlighting
            ClearHighlight();
            
            // Highlight new topic
            var button = this.FindControl<Button>($"Topic2_{topicNumber}Button");
            if (button != null)
            {
                button.Background = new SolidColorBrush(Color.Parse("#FFCC80"));
                _currentHighlightedTopic = topicNumber;
            }
        }
        
        private void ClearHighlight()
        {
            if (_currentHighlightedTopic > 0)
            {
                var button = this.FindControl<Button>($"Topic2_{_currentHighlightedTopic}Button");
                if (button != null)
                {
                    button.Background = new SolidColorBrush(Color.Parse("#FFF3E0"));
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
            // Connect handlers for all 18 topics Part 2
            for (int i = 1; i <= 18; i++)
            {
                var button = this.FindControl<Button>($"Topic2_{i}Button");
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
                    // Pass Part=2 and topic number
                    // MainWindow converts this to page number
                    NavigationService.RequestNavigation(2, topicNumber);
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

