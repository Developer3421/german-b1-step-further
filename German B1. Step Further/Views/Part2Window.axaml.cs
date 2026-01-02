using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using German_B1._Step_Further.Services;

namespace German_B1._Step_Further.Views
{
    public partial class Part2Window : Window
    {
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
        }
        
        private void ConnectTopicButtons()
        {
            // Підключаємо обробники для всіх 18 тем Part 2
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
                    // Part 1: 18 тем × 3 підтеми = 54 сторінки (стор. 3-56)
                    // Part 2 починається з сторінки 57
                    int part1Pages = 18 * 3; // 54 сторінки
                    int pagesPerTopic = 3;
                    int pageNumber = 2 + part1Pages + (topicNumber - 1) * pagesPerTopic + 1;
                    
                    // Викликаємо навігацію для Part 2
                    NavigationService.RequestNavigation(2, pageNumber);
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

