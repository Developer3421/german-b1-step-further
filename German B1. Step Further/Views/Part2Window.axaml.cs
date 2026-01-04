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
            
            // Підписуємося на зміну сторінки для синхронізації
            NavigationService.PageChanged += OnPageChanged;
        }
        
        private void OnPageChanged(object? sender, PageChangedEventArgs e)
        {
            // Визначаємо яка тема відповідає поточним сторінкам
            // Частина 2: сторінки 57-110
            int leftPage = e.LeftPage;
            
            if (leftPage >= 57 && leftPage <= 110)
            {
                // Тема = (сторінка - 57) / 3 + 1
                int topicNumber = (leftPage - 57) / 3 + 1;
                HighlightTopic(topicNumber);
            }
            else
            {
                // Сторінки не з частини 2 - прибираємо підсвічування
                ClearHighlight();
            }
        }
        
        private void HighlightTopic(int topicNumber)
        {
            if (_currentHighlightedTopic == topicNumber) return;
            
            // Прибираємо попереднє підсвічування
            ClearHighlight();
            
            // Підсвічуємо нову тему
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
                    // Передаємо Part=2 і номер теми
                    // MainWindow конвертує це в номер сторінки
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

