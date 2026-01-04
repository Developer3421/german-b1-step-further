using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Layout;
using German_B1._Step_Further.Views;
using German_B1._Step_Further.Services;
using German_B1._Step_Further.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace German_B1._Step_Further
{
    public partial class MainWindow : Window
    {
        // Унікальний ідентифікатор вікна для збереження сесії
        public string WindowId { get; } = Guid.NewGuid().ToString();
        
        // Поточна ліва сторінка книжки (непарна: 1, 3, 5...)
        private int _currentLeftPage = 1;
        
        // ContentPageView для лівої та правої панелі
        private ContentPageView? _leftPageView;
        private ContentPageView? _rightPageView;
        
        // Максимальна кількість вкладок
        private const int MAX_TABS = 3;
        
        // Інформація про вкладки (індекс вкладки -> номер сторінки)
        private readonly List<int> _tabPages = new() { 1 };
        
        // Поточна активна вкладка
        private int _activeTabIndex = 0;
        
        // Для Drag and Drop
        private bool _isDraggingTab = false;
        private Border? _draggingTab = null;
        private Point _dragStartPoint;
        private int _draggingTabIndex = -1;
        
        // Для відновлення сесії
        private WindowSession? _restoredSession;

        public MainWindow() : this(null)
        {
        }
        
        public MainWindow(WindowSession? restoredSession)
        {
            _restoredSession = restoredSession;
            
            // Якщо відновлюємо сесію, використовуємо збережені вкладки
            if (_restoredSession != null && _restoredSession.TabPages.Count > 0)
            {
                _tabPages.Clear();
                _tabPages.AddRange(_restoredSession.TabPages);
                _activeTabIndex = _restoredSession.ActiveTabIndex;
            }
            
            InitializeComponent();
            
            // Реєструємо вікно в WindowManagerService
            WindowManagerService.RegisterWindow(this);
            WindowManagerService.TabTransferRequested += OnTabTransferRequested;
            
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
            
            // Connect add tab button
            var addTabButton = this.FindControl<Button>("PART_AddTabButton");
            if (addTabButton != null)
                addTabButton.Click += AddTabButton_Click;
            
            // Enable window dragging from title bar
            if (titleBarRow != null)
            {
                titleBarRow.PointerPressed += TitleBar_PointerPressed;
            }
            
            // Підписуємося на події навігації від Part1Window тощо
            NavigationService.NavigateToPage += OnNavigateToPage;
            
            // Ініціалізація при завантаженні вікна
            this.Opened += MainWindow_Opened;
        }
        
        /// <summary>
        /// Обробник відкриття вікна - ініціалізує вкладки після того як UI готовий
        /// </summary>
        private void MainWindow_Opened(object? sender, EventArgs e)
        {
            // Відновлюємо вкладки з сесії
            if (_restoredSession != null && _restoredSession.TabPages.Count > 1)
            {
                RestoreTabsFromSession();
            }
            else
            {
                // Завантажуємо початкові сторінки (1-2 = зміст)
                LoadBookPages(_tabPages[0]);
            }
            
            // Зберігаємо початкову сесію
            SaveCurrentSession();
        }
        
        /// <summary>
        /// Відновлює вкладки з сесії
        /// </summary>
        private void RestoreTabsFromSession()
        {
            var tabsContainer = this.FindControl<StackPanel>("TabsContainer");
            if (tabsContainer == null) return;
            
            // Видаляємо всі вкладки крім першої
            while (tabsContainer.Children.Count > 1)
            {
                tabsContainer.Children.RemoveAt(1);
            }
            
            // Оновлюємо першу вкладку
            UpdateTabTitle(0, _tabPages[0]);
            
            // Додаємо інші вкладки
            for (int i = 1; i < _tabPages.Count; i++)
            {
                var newTab = CreateTabBorder(i, _tabPages[i], true);
                tabsContainer.Children.Add(newTab);
            }
            
            // Активуємо збережену вкладку
            SetActiveTab(_activeTabIndex);
        }
        
        /// <summary>
        /// Знаходить вкладку за її індексом (по Tag)
        /// </summary>
        private Border? FindTabByIndex(int tabIndex)
        {
            var tabsContainer = this.FindControl<StackPanel>("TabsContainer");
            if (tabsContainer == null) return null;
            
            foreach (var child in tabsContainer.Children)
            {
                if (child is Border tab && tab.Tag is int idx && idx == tabIndex)
                {
                    return tab;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Оновлює заголовок вкладки за індексом
        /// </summary>
        private void UpdateTabTitle(int tabIndex, int pageNumber)
        {
            var tab = FindTabByIndex(tabIndex);
            if (tab == null) return;
            
            var grid = tab.Child as Grid;
            if (grid == null) return;
            
            foreach (var child in grid.Children)
            {
                if (child is TextBlock tb && tb.Classes.Contains("tab-title"))
                {
                    int partNumber = GetPartNumber(pageNumber);
                    int rightPage = pageNumber + 1;
                    tb.Text = $"Ч{partNumber} СТ{pageNumber}-{rightPage}";
                    break;
                }
            }
        }
        
        /// <summary>
        /// Обробник переміщення вкладки між вікнами
        /// </summary>
        private void OnTabTransferRequested(object? sender, TabTransferEventArgs e)
        {
            // Якщо це цільове вікно - приймаємо вкладку
            if (e.TargetWindow == this && e.SourceWindow != this)
            {
                AcceptTabFromOtherWindow(e.PageNumber);
            }
        }
        
        /// <summary>
        /// Обробка навігації від Part1Window/Part2Window/Part3Window/Part4Window
        /// </summary>
        private void OnNavigateToPage(object? sender, NavigationEventArgs e)
        {
            int part = e.Part;
            int topicNumber = e.PageNumber;
            int startPage;
            
            if (part == 1)
            {
                // Частина 1: Теми та підтеми
                // Зміст: сторінки 1-2
                // Тема 1: сторінки 3-5, Тема 2: сторінки 6-8, і т.д.
                startPage = 2 + (topicNumber - 1) * 3 + 1;
            }
            else if (part == 2)
            {
                // Частина 2: Словник по темах
                // Починається з сторінки 57
                // Тема 1: сторінки 57-59, Тема 2: сторінки 60-62, і т.д.
                startPage = 56 + (topicNumber - 1) * 3 + 1;
            }
            else if (part == 3)
            {
                // Частина 3: Граматика B1
                // Починається з сторінки 111
                // Тема 1: сторінки 111-113, Тема 2: сторінки 114-116, і т.д.
                startPage = 110 + (topicNumber - 1) * 3 + 1;
            }
            else if (part == 4)
            {
                // Частина 4: Шаблони листів
                // Починається з сторінки 147
                // Тема 1: сторінки 147-149, Тема 2: сторінки 150-152, і т.д.
                startPage = 146 + (topicNumber - 1) * 3 + 1;
            }
            else
            {
                startPage = 1;
            }
            
            // Робимо startPage непарним (ліва сторінка)
            if (startPage % 2 == 0)
                startPage--;
            
            LoadBookPages(startPage);
        }
        
        /// <summary>
        /// Завантажує пару сторінок книжки (ліва непарна, права парна)
        /// </summary>
        private void LoadBookPages(int leftPageNumber)
        {
            // Ліва сторінка завжди непарна
            if (leftPageNumber % 2 == 0)
                leftPageNumber--;
            if (leftPageNumber < 1)
                leftPageNumber = 1;
                
            _currentLeftPage = leftPageNumber;
            int rightPageNumber = leftPageNumber + 1;
            
            var leftPanel = this.FindControl<StackPanel>("LeftContentPanel");
            var rightPanel = this.FindControl<StackPanel>("RightContentPanel");

            if (leftPanel == null || rightPanel == null) return;

            leftPanel.Children.Clear();
            rightPanel.Children.Clear();
            
            // Сторінки 1-2 = Зміст
            if (leftPageNumber == 1)
            {
                // Ліва панель - ContentsPage1
                leftPanel.Children.Add(new ContentsPage1());
                // Права панель - ContentsPage2
                rightPanel.Children.Add(new ContentsPage2());
            }
            else
            {
                // Створюємо ContentPageView для обох панелей
                _leftPageView = new ContentPageView();
                _rightPageView = new ContentPageView();
                
                // Завантажуємо контент сторінок
                LoadPageContent(_leftPageView, leftPageNumber);
                LoadPageContent(_rightPageView, rightPageNumber);
                
                leftPanel.Children.Add(_leftPageView);
                rightPanel.Children.Add(_rightPageView);
            }
            
            // Оновлюємо текст активної вкладки з номером частини та сторінок
            UpdateActiveTabTitle(leftPageNumber, rightPageNumber);
            
            // Оновлюємо список сторінок для активної вкладки
            if (_activeTabIndex < _tabPages.Count)
                _tabPages[_activeTabIndex] = leftPageNumber;
            
            // Сповіщаємо всіх слухачів про зміну сторінки
            NavigationService.NotifyPageChanged(leftPageNumber, rightPageNumber);
        }
        
        /// <summary>
        /// Оновлює заголовок активної вкладки з поточними номерами частини та сторінок
        /// </summary>
        private void UpdateActiveTabTitle(int leftPage, int rightPage)
        {
            var activeTab = FindTabByIndex(_activeTabIndex);
            if (activeTab == null) return;
            
            // Знаходимо TextBlock всередині вкладки
            var grid = activeTab.Child as Grid;
            if (grid == null) return;
            
            TextBlock? tabTitle = null;
            foreach (var child in grid.Children)
            {
                if (child is TextBlock tb && tb.Classes.Contains("tab-title"))
                {
                    tabTitle = tb;
                    break;
                }
            }
            
            if (tabTitle == null) return;
            
            // Визначаємо номер частини за номером сторінки
            int partNumber = GetPartNumber(leftPage);
            
            // Формуємо текст вкладки
            tabTitle.Text = $"Ч{partNumber} СТ{leftPage}-{rightPage}";
        }
        
        /// <summary>
        /// Визначає номер частини за номером сторінки
        /// </summary>
        private int GetPartNumber(int pageNumber)
        {
            // Частина 1: сторінки 1-56 (зміст 1-2, теми 3-56)
            // Частина 2: сторінки 57-110 (словник по темах)
            // Частина 3: сторінки 111-146 (граматика B1)
            // Частина 4: сторінки 147-164 (шаблони листів)
            if (pageNumber <= 56)
                return 1;
            else if (pageNumber <= 110)
                return 2;
            else if (pageNumber <= 146)
                return 3;
            else
                return 4;
        }
        
        /// <summary>
        /// Завантажує контент для конкретної сторінки
        /// </summary>
        private void LoadPageContent(ContentPageView pageView, int pageNumber)
        {
            // Спробуємо отримати дані зі словника ресурсів
            string titleKey = $"Page{pageNumber}_Title";
            string subtitleKey = $"Page{pageNumber}_Subtitle";
            string contentKey = $"Page{pageNumber}_Content";
            
            // Перевіряємо чи існують ресурси для цієї сторінки
            if (App.Current?.Resources.TryGetResource(titleKey, null, out var titleObj) == true &&
                App.Current?.Resources.TryGetResource(subtitleKey, null, out var subtitleObj) == true)
            {
                string title = titleObj?.ToString() ?? "";
                string subtitle = subtitleObj?.ToString() ?? "";
                
                pageView.SetPage(pageNumber, title, subtitle, contentKey);
            }
            else
            {
                // Порожня сторінка для майбутнього контенту
                pageView.SetEmptyPage(pageNumber);
            }
        }
        
        protected override void OnClosed(EventArgs e)
        {
            // Зберігаємо сесію для відновлення при наступному запуску
            SaveSessionForRestore();
            
            // Відписуємося від подій
            WindowManagerService.TabTransferRequested -= OnTabTransferRequested;
            WindowManagerService.UnregisterWindow(this);
            NavigationService.NavigateToPage -= OnNavigateToPage;
            
            base.OnClosed(e);
        }
        
        /// <summary>
        /// Зберігає поточну сесію вікна в базу даних
        /// </summary>
        private void SaveCurrentSession()
        {
            try
            {
                SessionDatabaseService.SaveActiveWindowSession(
                    WindowId,
                    _tabPages,
                    _activeTabIndex,
                    Position.X,
                    Position.Y,
                    Width,
                    Height,
                    WindowState == WindowState.Maximized
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving session: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Зберігає сесію для відновлення при наступному запуску
        /// </summary>
        private void SaveSessionForRestore()
        {
            try
            {
                var session = new WindowSession
                {
                    WindowId = WindowId,
                    TabPages = new List<int>(_tabPages),
                    ActiveTabIndex = _activeTabIndex,
                    PositionX = Position.X,
                    PositionY = Position.Y,
                    Width = Width,
                    Height = Height,
                    IsMaximized = WindowState == WindowState.Maximized,
                    LastSaved = DateTime.Now
                };
                
                SessionDatabaseService.SaveWindowSessionForRestore(session);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving session for restore: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Приймає вкладку з іншого вікна
        /// </summary>
        public void AcceptTabFromOtherWindow(int pageNumber)
        {
            // Перевіряємо ліміт вкладок
            if (_tabPages.Count >= MAX_TABS)
            {
                // Показуємо повідомлення що досягнуто ліміту
                return;
            }
            
            // Робимо сторінку непарною
            if (pageNumber % 2 == 0)
                pageNumber--;
            
            // Додаємо нову вкладку
            AddNewTab(pageNumber);
            
            // Зберігаємо сесію
            SaveCurrentSession();
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
            instrumentsWindow.Show(this);
        }
        
        private void NewWindowButton_Click(object? sender, RoutedEventArgs e)
        {
            var newMainWindow = new MainWindow();
            newMainWindow.Show();
        }

        /// <summary>
        /// Кнопка "Назад" - переходить на попередню пару сторінок
        /// </summary>
        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentLeftPage > 1)
            {
                LoadBookPages(_currentLeftPage - 2);
            }
        }

        /// <summary>
        /// Кнопка "Вперед" - переходить на наступну пару сторінок
        /// </summary>
        private void ForwardButton_Click(object? sender, RoutedEventArgs e)
        {
            // Максимальна сторінка:
            // Частина 1 (теми): 3-56
            // Частина 2 (словник): 57-110
            // Частина 3 (граматика): 111-146
            // Частина 4 (листи): 147-164
            int maxPage = 164;
            
            if (_currentLeftPage + 2 <= maxPage)
            {
                LoadBookPages(_currentLeftPage + 2);
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
        
        #region Tab Management
        
        /// <summary>
        /// Обробник кнопки додавання вкладки - показує діалог введення номера сторінки
        /// </summary>
        private async void AddTabButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_tabPages.Count >= MAX_TABS)
            {
                // Показуємо повідомлення про максимальну кількість вкладок
                return;
            }
            
            // Створюємо стилізоване діалогове вікно
            var dialog = new NewTabDialog();
            await dialog.ShowDialog(this);
            
            if (dialog.ResultPage > 0)
            {
                int resultPage = dialog.ResultPage;
                // Робимо сторінку непарною
                if (resultPage % 2 == 0)
                    resultPage--;
                
                AddNewTab(resultPage);
            }
        }
        
        /// <summary>
        /// Додає нову вкладку з вказаною сторінкою
        /// </summary>
        private void AddNewTab(int pageNumber)
        {
            if (_tabPages.Count >= MAX_TABS) return;
            
            var tabsContainer = this.FindControl<StackPanel>("TabsContainer");
            if (tabsContainer == null) return;
            
            int newTabIndex = _tabPages.Count;
            _tabPages.Add(pageNumber);
            
            // Створюємо нову вкладку з таким же стилем
            var newTab = CreateTabBorder(newTabIndex, pageNumber, true);
            tabsContainer.Children.Add(newTab);
            
            // Активуємо нову вкладку
            SetActiveTab(newTabIndex);
        }
        
        /// <summary>
        /// Створює Border для вкладки
        /// </summary>
        private Border CreateTabBorder(int tabIndex, int pageNumber, bool isClosable)
        {
            int partNumber = GetPartNumber(pageNumber);
            int rightPage = pageNumber + 1;
            
            var border = new Border
            {
                Tag = tabIndex,
                Margin = new Avalonia.Thickness(2, 0, 0, 0)
            };
            border.Classes.Add("tab");
            
            var grid = new Grid
            {
                ColumnDefinitions = ColumnDefinitions.Parse("Auto,*,Auto")
            };
            
            // Іконка сторінки
            var iconBorder = new Border
            {
                Width = 18,
                Height = 18,
                CornerRadius = new Avalonia.CornerRadius(3),
                Background = new SolidColorBrush(Color.Parse("#A78BFA")),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 0, 8, 0)
            };
            Grid.SetColumn(iconBorder, 0);
            
            var viewbox = new Viewbox { Width = 12, Height = 12 };
            var canvas = new Canvas { Width = 24, Height = 24 };
            var path = new Avalonia.Controls.Shapes.Path
            {
                Fill = Brushes.White,
                Data = Avalonia.Media.Geometry.Parse("M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z")
            };
            canvas.Children.Add(path);
            viewbox.Child = canvas;
            iconBorder.Child = viewbox;
            
            // Текст вкладки
            var textBlock = new TextBlock
            {
                Text = $"Ч{partNumber} СТ{pageNumber}-{rightPage}",
                VerticalAlignment = VerticalAlignment.Center
            };
            textBlock.Classes.Add("tab-title");
            Grid.SetColumn(textBlock, 1);
            
            grid.Children.Add(iconBorder);
            grid.Children.Add(textBlock);
            
            if (isClosable)
            {
                // Кнопка закриття
                var closeButton = new Button
                {
                    Width = 24,
                    Height = 24,
                    Padding = new Avalonia.Thickness(0),
                    Background = Brushes.Transparent,
                    BorderThickness = new Avalonia.Thickness(0),
                    Tag = tabIndex
                };
                closeButton.Classes.Add("tab-close");
                Grid.SetColumn(closeButton, 2);
                
                var closeViewbox = new Viewbox { Width = 12, Height = 12 };
                var closeCanvas = new Canvas { Width = 24, Height = 24 };
                var closePath = new Avalonia.Controls.Shapes.Path
                {
                    Fill = new SolidColorBrush(Color.Parse("#64748B")),
                    Data = Avalonia.Media.Geometry.Parse("M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z")
                };
                closeCanvas.Children.Add(closePath);
                closeViewbox.Child = closeCanvas;
                closeButton.Content = closeViewbox;
                
                closeButton.Click += CloseTab_Click;
                
                grid.Children.Add(closeButton);
            }
            else
            {
                // Пустий Border для вирівнювання
                var spacer = new Border { Width = 12 };
                Grid.SetColumn(spacer, 2);
                grid.Children.Add(spacer);
            }
            
            border.Child = grid;
            
            // Обробники подій для drag-and-drop та активації
            border.PointerPressed += Tab_PointerPressed;
            border.PointerMoved += Tab_PointerMoved;
            border.PointerReleased += Tab_PointerReleased;
            
            return border;
        }
        
        /// <summary>
        /// Закриття вкладки
        /// </summary>
        private void CloseTab_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int tabIndex)
            {
                if (tabIndex == 0) return; // Першу вкладку не можна закрити
                
                var tabsContainer = this.FindControl<StackPanel>("TabsContainer");
                if (tabsContainer == null) return;
                
                // Знаходимо вкладку по Tag
                Border? tabToRemove = null;
                foreach (var child in tabsContainer.Children)
                {
                    if (child is Border tab && tab.Tag is int idx && idx == tabIndex)
                    {
                        tabToRemove = tab;
                        break;
                    }
                }
                
                if (tabToRemove != null)
                {
                    tabsContainer.Children.Remove(tabToRemove);
                }
                
                _tabPages.RemoveAt(tabIndex);
                
                // Перенумеровуємо вкладки (тільки Tag, не Name)
                RenumberTabs();
                
                // Якщо закрита активна вкладка, переключаємось на попередню
                if (_activeTabIndex >= _tabPages.Count)
                {
                    _activeTabIndex = _tabPages.Count - 1;
                }
                
                SetActiveTab(_activeTabIndex);
            }
        }
        
        /// <summary>
        /// Перенумеровує вкладки після видалення (оновлює тільки Tag, не Name)
        /// </summary>
        private void RenumberTabs()
        {
            var tabsContainer = this.FindControl<StackPanel>("TabsContainer");
            if (tabsContainer == null) return;
            
            for (int i = 0; i < tabsContainer.Children.Count; i++)
            {
                if (tabsContainer.Children[i] is Border tab)
                {
                    // Оновлюємо тільки Tag (Name не можна змінювати в Avalonia після стилізації)
                    tab.Tag = i;
                    
                    // Оновлюємо Tag кнопки закриття
                    if (tab.Child is Grid grid)
                    {
                        foreach (var child in grid.Children)
                        {
                            if (child is Button closeBtn && closeBtn.Classes.Contains("tab-close"))
                            {
                                closeBtn.Tag = i;
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Встановлює активну вкладку
        /// </summary>
        private void SetActiveTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= _tabPages.Count) return;
            
            var tabsContainer = this.FindControl<StackPanel>("TabsContainer");
            if (tabsContainer == null) return;
            
            // Прибираємо клас active з усіх вкладок
            foreach (var child in tabsContainer.Children)
            {
                if (child is Border tab)
                {
                    tab.Classes.Remove("active");
                }
            }
            
            // Додаємо клас active до вибраної вкладки
            var activeTab = FindTabByIndex(tabIndex);
            if (activeTab != null)
            {
                activeTab.Classes.Add("active");
            }
            
            _activeTabIndex = tabIndex;
            
            // Завантажуємо сторінки для цієї вкладки
            LoadBookPages(_tabPages[tabIndex]);
            
            // Зберігаємо сесію після зміни активної вкладки
            SaveCurrentSession();
        }
        
        #endregion
        
        #region Tab Drag and Drop
        
        private void Tab_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Border tab && tab.Tag is int tabIndex)
            {
                var point = e.GetCurrentPoint(tab);
                
                // Ліва кнопка - активуємо вкладку та починаємо drag
                if (point.Properties.IsLeftButtonPressed)
                {
                    SetActiveTab(tabIndex);
                    
                    _isDraggingTab = true;
                    _draggingTab = tab;
                    _draggingTabIndex = tabIndex;
                    _dragStartPoint = e.GetPosition(this);
                    e.Handled = true;
                }
                // Права кнопка - показуємо контекстне меню для перенесення в інше вікно
                else if (point.Properties.IsRightButtonPressed)
                {
                    ShowTabContextMenu(tab, tabIndex, e);
                    e.Handled = true;
                }
            }
        }
        
        /// <summary>
        /// Показує контекстне меню для вкладки (права кнопка миші)
        /// </summary>
        private void ShowTabContextMenu(Border tab, int tabIndex, PointerPressedEventArgs e)
        {
            var contextMenu = new ContextMenu();
            
            // Пункт меню "Перемістити в нове вікно"
            var moveToNewWindowItem = new MenuItem { Header = "Перемістити в нове вікно" };
            moveToNewWindowItem.Click += (s, args) =>
            {
                MoveTabToNewWindow(tabIndex);
            };
            contextMenu.Items.Add(moveToNewWindowItem);
            
            // Отримуємо список інших відкритих вікон
            var otherWindows = WindowManagerService.GetOpenWindows()
                .Where(w => w != this && w.CanAcceptTab())
                .ToList();
            
            if (otherWindows.Count > 0)
            {
                contextMenu.Items.Add(new Separator());
                
                foreach (var window in otherWindows)
                {
                    var windowItem = new MenuItem 
                    { 
                        Header = $"Перемістити у вікно: Ч{window.GetCurrentPartNumber()} СТ{window.GetCurrentPages()}"
                    };
                    var targetWindow = window;
                    windowItem.Click += (s, args) =>
                    {
                        MoveTabToOtherWindow(tabIndex, targetWindow);
                    };
                    contextMenu.Items.Add(windowItem);
                }
            }
            
            // Separator та пункт закриття вкладки (якщо це не перша вкладка)
            if (tabIndex > 0)
            {
                contextMenu.Items.Add(new Separator());
                var closeTabItem = new MenuItem { Header = "Закрити вкладку" };
                closeTabItem.Click += (s, args) =>
                {
                    CloseTabByIndex(tabIndex);
                };
                contextMenu.Items.Add(closeTabItem);
            }
            
            contextMenu.Open(tab);
        }
        
        /// <summary>
        /// Перевіряє чи вікно може прийняти нову вкладку
        /// </summary>
        public bool CanAcceptTab()
        {
            return _tabPages.Count < MAX_TABS;
        }
        
        /// <summary>
        /// Отримує номер поточної частини для відображення
        /// </summary>
        public int GetCurrentPartNumber()
        {
            return GetPartNumber(_currentLeftPage);
        }
        
        /// <summary>
        /// Отримує рядок з поточними сторінками для відображення
        /// </summary>
        public string GetCurrentPages()
        {
            return $"{_currentLeftPage}-{_currentLeftPage + 1}";
        }
        
        /// <summary>
        /// Переміщує вкладку в нове вікно
        /// </summary>
        private void MoveTabToNewWindow(int tabIndex)
        {
            if (tabIndex <= 0 || tabIndex >= _tabPages.Count) return;
            if (_tabPages.Count <= 1) return; // Не можна перемістити останню вкладку
            
            int pageNumber = _tabPages[tabIndex];
            
            // Створюємо нове вікно з цією сторінкою
            var newSession = new WindowSession
            {
                TabPages = new List<int> { pageNumber },
                ActiveTabIndex = 0
            };
            
            var newWindow = new MainWindow(newSession);
            newWindow.Show();
            
            // Закриваємо вкладку в поточному вікні
            CloseTabByIndex(tabIndex);
        }
        
        /// <summary>
        /// Переміщує вкладку в інше існуюче вікно
        /// </summary>
        private void MoveTabToOtherWindow(int tabIndex, MainWindow targetWindow)
        {
            if (tabIndex <= 0 || tabIndex >= _tabPages.Count) return;
            if (_tabPages.Count <= 1) return; // Не можна перемістити останню вкладку
            if (!targetWindow.CanAcceptTab()) return;
            
            int pageNumber = _tabPages[tabIndex];
            
            // Сповіщаємо цільове вікно про переміщення вкладки
            WindowManagerService.RequestTabTransfer(this, tabIndex, pageNumber, targetWindow, 0, 0);
            
            // Закриваємо вкладку в поточному вікні
            CloseTabByIndex(tabIndex);
        }
        
        /// <summary>
        /// Закриває вкладку за індексом
        /// </summary>
        private void CloseTabByIndex(int tabIndex)
        {
            if (tabIndex <= 0 || tabIndex >= _tabPages.Count) return;
            
            var tabsContainer = this.FindControl<StackPanel>("TabsContainer");
            if (tabsContainer == null) return;
            
            // Знаходимо вкладку по Tag
            var tabToRemove = FindTabByIndex(tabIndex);
            if (tabToRemove != null)
            {
                tabsContainer.Children.Remove(tabToRemove);
            }
            
            _tabPages.RemoveAt(tabIndex);
            
            // Перенумеровуємо вкладки
            RenumberTabs();
            
            // Якщо закрита активна вкладка, переключаємось на попередню
            if (_activeTabIndex >= _tabPages.Count)
            {
                _activeTabIndex = _tabPages.Count - 1;
            }
            
            SetActiveTab(_activeTabIndex);
            SaveCurrentSession();
        }
        
        private void Tab_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDraggingTab && _draggingTab != null)
            {
                var currentPoint = e.GetPosition(this);
                var distance = Math.Sqrt(Math.Pow(currentPoint.X - _dragStartPoint.X, 2) + 
                                         Math.Pow(currentPoint.Y - _dragStartPoint.Y, 2));
                
                // Якщо перетягування вийшло за межі вікна
                if (distance > 50)
                {
                    Cursor = new Cursor(StandardCursorType.DragMove);
                    
                    // Перевіряємо чи курсор над іншим вікном
                    var screenPos = this.PointToScreen(currentPoint);
                    var targetWindow = WindowManagerService.GetWindowAtPosition(screenPos.X, screenPos.Y, this);
                    
                    if (targetWindow != null && targetWindow.CanAcceptTab())
                    {
                        // Підсвічуємо цільове вікно
                        Cursor = new Cursor(StandardCursorType.DragCopy);
                    }
                }
            }
        }
        
        private void Tab_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_isDraggingTab && _draggingTab != null && _draggingTabIndex > 0)
            {
                var currentPoint = e.GetPosition(this);
                var distance = Math.Sqrt(Math.Pow(currentPoint.X - _dragStartPoint.X, 2) + 
                                         Math.Pow(currentPoint.Y - _dragStartPoint.Y, 2));
                
                // Якщо було значне перетягування - перевіряємо цільове вікно
                if (distance > 50)
                {
                    var screenPos = this.PointToScreen(currentPoint);
                    var targetWindow = WindowManagerService.GetWindowAtPosition(screenPos.X, screenPos.Y, this);
                    
                    if (targetWindow != null && targetWindow.CanAcceptTab() && _tabPages.Count > 1)
                    {
                        // Переміщуємо вкладку в інше вікно
                        MoveTabToOtherWindow(_draggingTabIndex, targetWindow);
                    }
                    else if (targetWindow == null && _tabPages.Count > 1)
                    {
                        // Якщо вкладку кинули поза всіма вікнами - створюємо нове вікно
                        MoveTabToNewWindow(_draggingTabIndex);
                    }
                }
            }
            
            _isDraggingTab = false;
            _draggingTab = null;
            _draggingTabIndex = -1;
            Cursor = Cursor.Default;
        }
        
        #endregion
    }
}
