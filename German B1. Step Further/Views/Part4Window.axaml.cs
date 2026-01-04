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
            
            // Enable window dragging from title bar
            if (titleBarRow != null)
            {
                titleBarRow.PointerPressed += TitleBar_PointerPressed;
            }
            
            // Connect all topic buttons
            ConnectTopicButtons();
            
            // Підписуємося на зміну сторінки для синхронізації
            NavigationService.PageChanged += OnPageChanged;
        }
        
        private void OnPageChanged(object? sender, PageChangedEventArgs e)
        {
            // Визначаємо яка тема відповідає поточним сторінкам
            // Частина 4: сторінки 147-164
            int leftPage = e.LeftPage;
            
            if (leftPage >= 147 && leftPage <= 164)
            {
                // Тема = (сторінка - 147) / 3 + 1
                // Частина 4 має рівно 6 тем (147-164).
                int topicNumber = (leftPage - 147) / 3 + 1;
                HighlightTopic(topicNumber);
            }
            else
            {
                // Сторінки не з частини 4 - прибираємо підсвічування
                ClearHighlight();
            }
        }
        
        private void HighlightTopic(int topicNumber)
        {
            if (_currentHighlightedTopic == topicNumber) return;
            
            // Прибираємо попереднє підсвічування
            ClearHighlight();
            
            // Підсвічуємо нову тему
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
            // Підключаємо обробники для всіх 6 тем Part 4
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
            if (sender is Button button && button.Tag is string tagString)
            {
                if (int.TryParse(tagString, out int topicNumber))
                {
                    // Передаємо Part=4 і номер теми
                    // MainWindow конвертує це в номер сторінки
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
