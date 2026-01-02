using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using German_B1._Step_Further.Services;

namespace German_B1._Step_Further.Views
{
    public partial class Part4Window : Window
    {
        public Part4Window()
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
            // Підключаємо обробники для всіх 8 тем Part 4
            for (int i = 1; i <= 8; i++)
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
            if (sender is Button button && button.Tag is string)
            {
                // Part 1: 18 × 3 = 54 стор (стор. 3-56)
                // Part 2: 18 × 3 = 54 стор (стор. 57-110)
                // Part 3: 12 × 4 = 48 стор (стор. 111-158)
                // Part 4: 1 файл = 1 сторінка (стор. 159)
                // Оскільки Part 4 має тільки 1 файл, всі теми вказують на ту саму сторінку
                int part1Pages = 18 * 3; // 54
                int part2Pages = 18 * 3; // 54
                int part3Pages = 12 * 4; // 48
                int pageNumber = 2 + part1Pages + part2Pages + part3Pages + 1; // сторінка 159
                
                // Викликаємо навігацію для Part 4
                NavigationService.RequestNavigation(4, pageNumber);
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

