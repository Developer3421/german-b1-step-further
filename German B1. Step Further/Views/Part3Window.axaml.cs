using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using German_B1._Step_Further.Services;

namespace German_B1._Step_Further.Views
{
    public partial class Part3Window : Window
    {
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
                    // Part 1: 18 тем × 3 підтеми = 54 сторінки (стор. 3-56)
                    // Part 2: 18 тем × 3 підтеми = 54 сторінки (стор. 57-110)
                    // Part 3 починається з сторінки 111
                    // Part 3 має 12 тем, кожна в середньому 3-4 підтеми, візьмемо 4
                    int part1Pages = 18 * 3; // 54
                    int part2Pages = 18 * 3; // 54
                    int pagesPerTopic = 4; // для Part 3
                    int pageNumber = 2 + part1Pages + part2Pages + (topicNumber - 1) * pagesPerTopic + 1;
                    
                    // Викликаємо навігацію для Part 3
                    NavigationService.RequestNavigation(3, pageNumber);
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

