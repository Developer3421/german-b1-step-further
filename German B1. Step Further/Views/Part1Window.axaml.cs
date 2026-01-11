using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using German_B1._Step_Further.Services;

namespace German_B1._Step_Further.Views
{
    public partial class Part1Window : Window
    {
        private int _currentHighlightedTopic = -1;
        
        public Part1Window()
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
            
            // Subscribe to page changes in main window
            NavigationService.PageChanged += OnPageChanged;
        }
        
        private void ConnectTopicButtons()
        {
            // Connect handlers for all 18 topics
            for (int i = 1; i <= 18; i++)
            {
                var button = this.FindControl<Button>($"Topic1_{i}Button");
                if (button != null)
                {
                    button.Click += TopicButton_Click;
                }
            }
        }
        
        /// <summary>
        /// Handle page changes in main window - highlight corresponding topic
        /// </summary>
        private void OnPageChanged(object? sender, PageChangedEventArgs e)
        {
            // Determine topic number by page number
            // Pages 3-5 = Topic 1, 6-8 = Topic 2, etc.
            int topicNumber = -1;
            
            if (e.LeftPage >= 3)
            {
                topicNumber = ((e.LeftPage - 3) / 3) + 1;
            }
            
            // If topic changed - update highlighting
            if (topicNumber != _currentHighlightedTopic && topicNumber >= 1 && topicNumber <= 18)
            {
                HighlightTopic(topicNumber);
            }
            else if (e.LeftPage < 3)
            {
                // Сторінки змісту - знімаємо підсвітку
                ClearHighlight();
            }
        }
        
        /// <summary>
        /// Підсвічує кнопку поточної теми
        /// </summary>
        private void HighlightTopic(int topicNumber)
        {
            // Знімаємо попередню підсвітку
            ClearHighlight();
            
            // Встановлюємо нову підсвітку
            var button = this.FindControl<Button>($"Topic1_{topicNumber}Button");
            if (button != null)
            {
                button.Background = new SolidColorBrush(Color.Parse("#FFE6D5C7"));
                button.BorderBrush = new SolidColorBrush(Color.Parse("#FF8A2BE2"));
                button.BorderThickness = new Avalonia.Thickness(2);
                _currentHighlightedTopic = topicNumber;
            }
        }
        
        /// <summary>
        /// Знімає підсвітку з усіх кнопок
        /// </summary>
        private void ClearHighlight()
        {
            if (_currentHighlightedTopic > 0)
            {
                var button = this.FindControl<Button>($"Topic1_{_currentHighlightedTopic}Button");
                if (button != null)
                {
                    button.Background = new SolidColorBrush(Color.Parse("#FFFAF0E6"));
                    button.BorderBrush = new SolidColorBrush(Color.Parse("#FFE8D5B7"));
                    button.BorderThickness = new Avalonia.Thickness(1);
                }
                _currentHighlightedTopic = -1;
            }
        }
        
        private void TopicButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tagString)
            {
                if (int.TryParse(tagString, out int topicNumber))
                {
                    // Викликаємо навігацію через сервіс
                    // Передаємо номер частини (1) та номер теми
                    NavigationService.RequestNavigation(1, topicNumber);
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
        
        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            // Відписуємося від події при закритті вікна
            NavigationService.PageChanged -= OnPageChanged;
        }
    }
}

