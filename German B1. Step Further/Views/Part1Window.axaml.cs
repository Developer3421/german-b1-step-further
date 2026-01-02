using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using German_B1._Step_Further.Services;

namespace German_B1._Step_Further.Views
{
    public partial class Part1Window : Window
    {
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
        }
        
        private void ConnectTopicButtons()
        {
            // Підключаємо обробники для всіх 18 тем
            for (int i = 1; i <= 18; i++)
            {
                var button = this.FindControl<Button>($"Topic1_{i}Button");
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
                    // Сторінки 1-2 зайняті ContentsPage
                    // Part 1 починається з сторінки 3
                    // Кожна тема має 3 підтеми (3 .odt файли = 3 сторінки)
                    // Тема 1 = стор. 3-5, Тема 2 = стор. 6-8, тощо
                    int pagesPerTopic = 3;
                    int pageNumber = 2 + (topicNumber - 1) * pagesPerTopic + 1;
                    
                    // Викликаємо навігацію через сервіс
                    NavigationService.RequestNavigation(1, pageNumber);
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

