using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using German_B1._Step_Further.Views;

namespace German_B1._Step_Further
{
    public partial class MainWindow : Window
    {
        private int _currentPage = 1;
        private const int TotalPages = 2;

        public MainWindow()
        {
            InitializeComponent();
            
            // Connect window control buttons
            var minimizeButton = this.FindControl<Button>("MinimizeButton");
            var maximizeButton = this.FindControl<Button>("MaximizeButton");
            var closeButton = this.FindControl<Button>("CloseButton");
            var titleBarRow = this.FindControl<Grid>("TitleBarRow");
            
            // Connect navigation buttons
            var backButton = this.FindControl<Button>("BackButton");
            var forwardButton = this.FindControl<Button>("ForwardButton");
            
            // Connect part buttons
            var part1Button = this.FindControl<Button>("Part1Button");
            var part2Button = this.FindControl<Button>("Part2Button");
            var part3Button = this.FindControl<Button>("Part3Button");
            var part4Button = this.FindControl<Button>("Part4Button");

            if (minimizeButton != null)
                minimizeButton.Click += MinimizeButton_Click;
            if (maximizeButton != null)
                maximizeButton.Click += MaximizeButton_Click;
            if (closeButton != null)
                closeButton.Click += CloseButton_Click;
            
            if (backButton != null)
                backButton.Click += BackButton_Click;
            if (forwardButton != null)
                forwardButton.Click += ForwardButton_Click;
            
            if (part1Button != null)
                part1Button.Click += Part1Button_Click;
            if (part2Button != null)
                part2Button.Click += Part2Button_Click;
            if (part3Button != null)
                part3Button.Click += Part3Button_Click;
            if (part4Button != null)
                part4Button.Click += Part4Button_Click;
            
            // Enable window dragging from title bar
            if (titleBarRow != null)
            {
                titleBarRow.PointerPressed += TitleBar_PointerPressed;
            }
            
            // Load initial pages - Page 1 in left panel, Page 2 in right panel
            LoadPages();
        }
        
        private void Part1Button_Click(object? sender, RoutedEventArgs e)
        {
            var part1Window = new Part1Window();
            part1Window.Show();
        }
        
        private void Part2Button_Click(object? sender, RoutedEventArgs e)
        {
            var part2Window = new Part2Window();
            part2Window.Show();
        }
        
        private void Part3Button_Click(object? sender, RoutedEventArgs e)
        {
            var part3Window = new Part3Window();
            part3Window.Show();
        }
        
        private void Part4Button_Click(object? sender, RoutedEventArgs e)
        {
            var part4Window = new Part4Window();
            part4Window.Show();
        }

        private void LoadPages()
        {
            var leftPanel = this.FindControl<StackPanel>("LeftContentPanel");
            var rightPanel = this.FindControl<StackPanel>("RightContentPanel");
            
            if (leftPanel == null || rightPanel == null) return;
            
            leftPanel.Children.Clear();
            rightPanel.Children.Clear();
            
            // Load ContentsPage1 (Parts 1 & 2) in left panel
            leftPanel.Children.Add(new Views.ContentsPage1());
            
            // Load ContentsPage2 (Parts 3 & 4) in right panel
            rightPanel.Children.Add(new Views.ContentsPage2());
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                // Future: implement page navigation
            }
        }

        private void ForwardButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentPage < TotalPages)
            {
                _currentPage++;
                // Future: implement page navigation
            }
        }

        private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized 
                ? WindowState.Normal 
                : WindowState.Maximized;
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
