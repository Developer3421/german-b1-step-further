using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using German_B1._Step_Further.Views;
using German_B1._Step_Further.Services;
using System;

namespace German_B1._Step_Further
{
    public partial class MainWindow : Window
    {
        // Відстеження поточної частини та сторінки
        private int _currentPart = 1;
        private int _currentPartPage = 1;

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
            
            // Connect instruments button
            var instrumentsButton = this.FindControl<Button>("InstrumentsButton");
            if (instrumentsButton != null)
                instrumentsButton.Click += InstrumentsButton_Click;
            
            // Connect new window button
            var newWindowButton = this.FindControl<Button>("NewWindowButton");
            if (newWindowButton != null)
                newWindowButton.Click += NewWindowButton_Click;
            
            // Enable window dragging from title bar
            if (titleBarRow != null)
            {
                titleBarRow.PointerPressed += TitleBar_PointerPressed;
            }
            
            // Підписуємося на події навігації
            NavigationService.NavigateToPage += OnNavigateToPage;
            
            // Load initial pages - Page 1 in left panel, Page 2 in right panel
            LoadPages();
        }
        
        private void OnNavigateToPage(object? sender, NavigationEventArgs e)
        {
            // Оновлюємо поточну частину та сторінку
            _currentPart = e.Part;
            _currentPartPage = e.PageNumber;
            
            // Завантажуємо відповідні сторінки
            LoadPartPages(e.Part, e.PageNumber);
        }
        
        private void LoadPartPages(int part, int pageNumber)
        {
            var leftPanel = this.FindControl<StackPanel>("LeftContentPanel");
            var rightPanel = this.FindControl<StackPanel>("RightContentPanel");

            if (leftPanel == null || rightPanel == null) return;

            leftPanel.Children.Clear();
            rightPanel.Children.Clear();

            // TODO: Тут буде логіка завантаження конкретних сторінок
            // Поки що показуємо номер частини та сторінки
            var leftText = new TextBlock
            {
                Text = $"Частина {part}\nСторінка {pageNumber}",
                FontSize = 24,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                TextAlignment = Avalonia.Media.TextAlignment.Center,
                Margin = new Avalonia.Thickness(20)
            };
            
            var rightText = new TextBlock
            {
                Text = $"Частина {part}\nСторінка {pageNumber + 1}",
                FontSize = 24,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                TextAlignment = Avalonia.Media.TextAlignment.Center,
                Margin = new Avalonia.Thickness(20)
            };
            
            leftPanel.Children.Add(leftText);
            rightPanel.Children.Add(rightText);
        }
        
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            
            // Відписуємося від подій при закритті
            NavigationService.NavigateToPage -= OnNavigateToPage;
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
        
        private void InstrumentsButton_Click(object? sender, RoutedEventArgs e)
        {
            var instrumentsWindow = new InstrumentsWindow();
            
            // В Avalonia Owner задається через Show(owner)
            instrumentsWindow.Show(this);
        }
        
        private void NewWindowButton_Click(object? sender, RoutedEventArgs e)
        {
            // Створюємо нове головне вікно
            var newMainWindow = new MainWindow();
            newMainWindow.Show();
        }

        private void LoadPages()
        {
            var leftPanel = this.FindControl<StackPanel>("LeftContentPanel");
            var rightPanel = this.FindControl<StackPanel>("RightContentPanel");

            if (leftPanel == null || rightPanel == null) return;

            leftPanel.Children.Clear();
            rightPanel.Children.Clear();

            // Load ContentsPage1 (Parts 1 & 2) in left panel
            leftPanel.Children.Add(new ContentsPage1());

            // Load ContentsPage2 (Parts 3 & 4) in right panel
            rightPanel.Children.Add(new ContentsPage2());
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentPartPage > 1)
            {
                _currentPartPage -= 2; // Повертаємось на 2 сторінки назад (пара сторінок)
                if (_currentPartPage < 1) _currentPartPage = 1;
                
                LoadPartPages(_currentPart, _currentPartPage);
            }
        }

        private void ForwardButton_Click(object? sender, RoutedEventArgs e)
        {
            // Визначаємо максимальну кількість сторінок для поточної частини
            int maxPages = GetMaxPagesForPart(_currentPart);
            
            if (_currentPartPage + 2 < maxPages)
            {
                _currentPartPage += 2; // Переходимо на 2 сторінки вперед (пара сторінок)
                LoadPartPages(_currentPart, _currentPartPage);
            }
        }
        
        private int GetMaxPagesForPart(int part)
        {
            // Кожна підтема = 1 .odt файл = 1 сторінка
            return part switch
            {
                1 => 18 * 3, // Part 1: 18 тем × 3 підтеми = 54 сторінки
                2 => 18 * 3, // Part 2: 18 тем × 3 підтеми = 54 сторінки
                3 => 12 * 4, // Part 3: 12 тем × ~4 підтеми = 48 сторінок
                4 => 1,      // Part 4: 1 файл = 1 сторінка
                _ => 2
            };
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
