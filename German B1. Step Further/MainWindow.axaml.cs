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
        // Unique window identifier for saving session
        public string WindowId { get; } = Guid.NewGuid().ToString();
        
        // Current left page of book (odd: 1, 3, 5...)
        private int _currentLeftPage = 1;
        
        // ContentPageView for left and right panels
        private ContentPageView? _leftPageView;
        private ContentPageView? _rightPageView;
        
        // Maximum number of tabs
        private const int MAX_TABS = 3;
        
        // Tab information (tab index -> page number)
        private readonly List<int> _tabPages = new() { 1 };
        
        // Current active tab
        private int _activeTabIndex = 0;
        
        // For Drag and Drop
        private bool _isDraggingTab = false;
        private Border? _draggingTab = null;
        private Point _dragStartPoint;
        private int _draggingTabIndex = -1;
        
        // For session restore
        private WindowSession? _restoredSession;

        public MainWindow() : this(null)
        {
        }
        
        public MainWindow(WindowSession? restoredSession)
        {
            _restoredSession = restoredSession;
            
            // If restoring session, use saved tabs
            if (_restoredSession != null && _restoredSession.TabPages.Count > 0)
            {
                _tabPages.Clear();
                _tabPages.AddRange(_restoredSession.TabPages);
                _activeTabIndex = _restoredSession.ActiveTabIndex;
            }
            
            InitializeComponent();
            
            // Register window in WindowManagerService
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
            
            // Subscribe to navigation events from Part1Window etc.
            NavigationService.NavigateToPage += OnNavigateToPage;
            
            // Initialize when window opens
            this.Opened += MainWindow_Opened;
        }
        
        /// <summary>
        /// Window opening handler - initializes tabs after UI is ready
        /// </summary>
        private void MainWindow_Opened(object? sender, EventArgs e)
        {
            // Restore tabs from session
            if (_restoredSession != null && _restoredSession.TabPages.Count > 1)
            {
                RestoreTabsFromSession();
            }
            else
            {
                // Load initial pages (1-2 = contents)
                LoadBookPages(_tabPages[0]);
            }
            
            // Save initial session
            SaveCurrentSession();
        }
        
        /// <summary>
        /// Restores tabs from session
        /// </summary>
        private void RestoreTabsFromSession()
        {
            var tabsContainer = this.FindControl<StackPanel>("TabsContainer");
            if (tabsContainer == null) return;
            
            // Remove all tabs except first
            while (tabsContainer.Children.Count > 1)
            {
                tabsContainer.Children.RemoveAt(1);
            }
            
            // Update first tab
            UpdateTabTitle(0, _tabPages[0]);
            
            // Add other tabs
            for (int i = 1; i < _tabPages.Count; i++)
            {
                var newTab = CreateTabBorder(i, _tabPages[i], true);
                tabsContainer.Children.Add(newTab);
            }
            
            // Activate saved tab
            SetActiveTab(_activeTabIndex);
        }
        
        /// <summary>
        /// Finds tab by its index (by Tag)
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
        /// Updates tab title by index
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
        /// Tab transfer between windows handler
        /// </summary>
        private void OnTabTransferRequested(object? sender, TabTransferEventArgs e)
        {
            // If this is target window - accept tab
            if (e.TargetWindow == this && e.SourceWindow != this)
            {
                AcceptTabFromOtherWindow(e.PageNumber);
            }
        }
        
        /// <summary>
        /// Navigation handling from Part1Window/Part2Window/Part3Window/Part4Window
        /// </summary>
        private void OnNavigateToPage(object? sender, NavigationEventArgs e)
        {
            int part = e.Part;
            int topicNumber = e.PageNumber;

            int startPage = BookNavigationMap.GetAbsoluteLeftPageForTopic(part, topicNumber);
            LoadBookPages(startPage);
        }
        
        /// <summary>
        /// Loads a pair of book pages (left odd, right even)
        /// </summary>
        private void LoadBookPages(int leftPageNumber)
        {
            // Left page is always odd
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
            
            // Pages 1-2 = Contents
            if (leftPageNumber == 1)
            {
                // Left panel - ContentsPage1
                leftPanel.Children.Add(new ContentsPage1());
                // Right panel - ContentsPage2
                rightPanel.Children.Add(new ContentsPage2());
            }
            else
            {
                // Create ContentPageView for both panels
                _leftPageView = new ContentPageView();
                _rightPageView = new ContentPageView();
                
                // Load page content
                LoadPageContent(_leftPageView, leftPageNumber);
                LoadPageContent(_rightPageView, rightPageNumber);
                
                leftPanel.Children.Add(_leftPageView);
                rightPanel.Children.Add(_rightPageView);
            }
            
            // Update active tab text with part number and pages
            UpdateActiveTabTitle(leftPageNumber, rightPageNumber);
            
            // Update page list for active tab
            if (_activeTabIndex < _tabPages.Count)
                _tabPages[_activeTabIndex] = leftPageNumber;
            
            // Notify all listeners about page change
            NavigationService.NotifyPageChanged(leftPageNumber, rightPageNumber);
        }
        
        /// <summary>
        /// Updates active tab title with current part number and pages
        /// </summary>
        private void UpdateActiveTabTitle(int leftPage, int rightPage)
        {
            var activeTab = FindTabByIndex(_activeTabIndex);
            if (activeTab == null) return;
            
            // Find TextBlock inside tab
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
            
            // Determine part number by page number
            int partNumber = GetPartNumber(leftPage);
            
            // Form tab text
            tabTitle.Text = $"Ч{partNumber} СТ{leftPage}-{rightPage}";
        }
        
        /// <summary>
        /// Determines part number by page number
        /// </summary>
        private int GetPartNumber(int pageNumber)
        {
            return BookNavigationMap.GetPartNumber(pageNumber);
        }
        
        /// <summary>
        /// Loads content for specific page
        /// </summary>
        private void LoadPageContent(ContentPageView pageView, int pageNumber)
        {
            // Try to get data from resource dictionary
            string titleKey = $"Page{pageNumber}_Title";
            string subtitleKey = $"Page{pageNumber}_Subtitle";
            string contentKey = $"Page{pageNumber}_Content";
            
            // Check if resources exist for this page
            if (App.Current?.Resources.TryGetResource(titleKey, null, out var titleObj) == true &&
                App.Current?.Resources.TryGetResource(subtitleKey, null, out var subtitleObj) == true)
            {
                string title = titleObj?.ToString() ?? "";
                string subtitle = subtitleObj?.ToString() ?? "";
                
                pageView.SetPage(pageNumber, title, subtitle, contentKey);
            }
            else
            {
                // Empty page for future content
                pageView.SetEmptyPage(pageNumber);
            }
        }
        
        protected override void OnClosed(EventArgs e)
        {
            // Save session for restore on next launch
            SaveSessionForRestore();
            
            // Unsubscribe from events
            WindowManagerService.TabTransferRequested -= OnTabTransferRequested;
            WindowManagerService.UnregisterWindow(this);
            NavigationService.NavigateToPage -= OnNavigateToPage;
            
            base.OnClosed(e);
        }
        
        /// <summary>
        /// Saves current window session to database
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
        /// Saves session for restore on next launch
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
        /// Accepts tab from another window
        /// </summary>
        public void AcceptTabFromOtherWindow(int pageNumber)
        {
            // Check tab limit
            if (_tabPages.Count >= MAX_TABS)
            {
                // Show message that limit reached
                return;
            }
            
            // Make page odd
            if (pageNumber % 2 == 0)
                pageNumber--;
            
            // Add new tab
            AddNewTab(pageNumber);
            
            // Save session
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
        /// "Back" button - goes to previous pair of pages
        /// </summary>
        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentLeftPage > 1)
            {
                LoadBookPages(_currentLeftPage - 2);
            }
        }

        /// <summary>
        /// "Forward" button - goes to next pair of pages
        /// </summary>
        private void ForwardButton_Click(object? sender, RoutedEventArgs e)
        {
            int maxPage = BookNavigationMap.MaxPage;

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
        /// Add tab button handler - shows dialog for entering page number
        /// </summary>
        private async void AddTabButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_tabPages.Count >= MAX_TABS)
            {
                // Show message about maximum tab count
                return;
            }
            
            // Create styled dialog window
            var dialog = new NewTabDialog();
            await dialog.ShowDialog(this);
            
            if (dialog.ResultPage > 0)
            {
                int resultPage = dialog.ResultPage;
                // Make page odd
                if (resultPage % 2 == 0)
                    resultPage--;
                
                AddNewTab(resultPage);
            }
        }
        
        /// <summary>
        /// Adds new tab with specified page
        /// </summary>
        private void AddNewTab(int pageNumber)
        {
            if (_tabPages.Count >= MAX_TABS) return;
            
            var tabsContainer = this.FindControl<StackPanel>("TabsContainer");
            if (tabsContainer == null) return;
            
            int newTabIndex = _tabPages.Count;
            _tabPages.Add(pageNumber);
            
            // Create new tab with same style
            var newTab = CreateTabBorder(newTabIndex, pageNumber, true);
            tabsContainer.Children.Add(newTab);
            
            // Activate new tab
            SetActiveTab(newTabIndex);
        }
        
        /// <summary>
        /// Creates Border for tab
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
            
            // Tab text
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
                // Close button
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
                // Empty Border for alignment
                var spacer = new Border { Width = 12 };
                Grid.SetColumn(spacer, 2);
                grid.Children.Add(spacer);
            }
            
            border.Child = grid;
            
            // Event handlers for drag-and-drop and activation
            border.PointerPressed += Tab_PointerPressed;
            border.PointerMoved += Tab_PointerMoved;
            border.PointerReleased += Tab_PointerReleased;
            
            return border;
        }
        
        /// <summary>
        /// Close tab
        /// </summary>
        private void CloseTab_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int tabIndex)
            {
                if (tabIndex == 0) return; // First tab cannot be closed
                
                var tabsContainer = this.FindControl<StackPanel>("TabsContainer");
                if (tabsContainer == null) return;
                
                // Find tab by Tag
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
                
                // Renumber tabs (only Tag, not Name)
                RenumberTabs();
                
                // If active tab closed, switch to previous
                if (_activeTabIndex >= _tabPages.Count)
                {
                    _activeTabIndex = _tabPages.Count - 1;
                }
                
                SetActiveTab(_activeTabIndex);
            }
        }
        
        /// <summary>
        /// Renumbers tabs after deletion (updates only Tag, not Name)
        /// </summary>
        private void RenumberTabs()
        {
            var tabsContainer = this.FindControl<StackPanel>("TabsContainer");
            if (tabsContainer == null) return;
            
            for (int i = 0; i < tabsContainer.Children.Count; i++)
            {
                if (tabsContainer.Children[i] is Border tab)
                {
                    // Update only Tag (Name cannot be changed in Avalonia after styling)
                    tab.Tag = i;
                    
                    // Update close button Tag
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
        /// Sets active tab
        /// </summary>
        private void SetActiveTab(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= _tabPages.Count) return;
            
            var tabsContainer = this.FindControl<StackPanel>("TabsContainer");
            if (tabsContainer == null) return;
            
            // Remove active class from all tabs
            foreach (var child in tabsContainer.Children)
            {
                if (child is Border tab)
                {
                    tab.Classes.Remove("active");
                }
            }
            
            // Add active class to selected tab
            var activeTab = FindTabByIndex(tabIndex);
            if (activeTab != null)
            {
                activeTab.Classes.Add("active");
            }
            
            _activeTabIndex = tabIndex;
            
            // Load pages for this tab
            LoadBookPages(_tabPages[tabIndex]);
            
            // Save session after changing active tab
            SaveCurrentSession();
        }
        
        #endregion
        
        #region Tab Drag and Drop
        
        private void Tab_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Border tab && tab.Tag is int tabIndex)
            {
                var point = e.GetCurrentPoint(tab);
                
                // Left button - activate tab and start drag
                if (point.Properties.IsLeftButtonPressed)
                {
                    SetActiveTab(tabIndex);
                    
                    _isDraggingTab = true;
                    _draggingTab = tab;
                    _draggingTabIndex = tabIndex;
                    _dragStartPoint = e.GetPosition(this);
                    e.Handled = true;
                }
                // Right button - show context menu for moving to another window
                else if (point.Properties.IsRightButtonPressed)
                {
                    ShowTabContextMenu(tab, tabIndex, e);
                    e.Handled = true;
                }
            }
        }
        
        /// <summary>
        /// Shows context menu for tab (right mouse button)
        /// </summary>
        private void ShowTabContextMenu(Border tab, int tabIndex, PointerPressedEventArgs e)
        {
            var contextMenu = new ContextMenu();
            
            // Menu item "Move to new window"
            var moveToNewWindowItem = new MenuItem { Header = "Move to new window" };
            moveToNewWindowItem.Click += (s, args) =>
            {
                MoveTabToNewWindow(tabIndex);
            };
            contextMenu.Items.Add(moveToNewWindowItem);
            
            // Get list of other open windows
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
                        Header = $"Move to window: P{window.GetCurrentPartNumber()} PG{window.GetCurrentPages()}"
                    };
                    var targetWindow = window;
                    windowItem.Click += (s, args) =>
                    {
                        MoveTabToOtherWindow(tabIndex, targetWindow);
                    };
                    contextMenu.Items.Add(windowItem);
                }
            }
            
            // Separator and close tab item (if not first tab)
            if (tabIndex > 0)
            {
                contextMenu.Items.Add(new Separator());
                var closeTabItem = new MenuItem { Header = "Close tab" };
                closeTabItem.Click += (s, args) =>
                {
                    CloseTabByIndex(tabIndex);
                };
                contextMenu.Items.Add(closeTabItem);
            }
            
            contextMenu.Open(tab);
        }
        
        /// <summary>
        /// Checks if window can accept new tab
        /// </summary>
        public bool CanAcceptTab()
        {
            return _tabPages.Count < MAX_TABS;
        }
        
        /// <summary>
        /// Gets current part number for display
        /// </summary>
        public int GetCurrentPartNumber()
        {
            return GetPartNumber(_currentLeftPage);
        }
        
        /// <summary>
        /// Gets string with current pages for display
        /// </summary>
        public string GetCurrentPages()
        {
            return $"{_currentLeftPage}-{_currentLeftPage + 1}";
        }
        
        /// <summary>
        /// Moves tab to new window
        /// </summary>
        private void MoveTabToNewWindow(int tabIndex)
        {
            if (tabIndex <= 0 || tabIndex >= _tabPages.Count) return;
            if (_tabPages.Count <= 1) return; // Cannot move last tab
            
            int pageNumber = _tabPages[tabIndex];
            
            // Create new window with this page
            var newSession = new WindowSession
            {
                TabPages = new List<int> { pageNumber },
                ActiveTabIndex = 0
            };
            
            var newWindow = new MainWindow(newSession);
            newWindow.Show();
            
            // Close tab in current window
            CloseTabByIndex(tabIndex);
        }
        
        /// <summary>
        /// Moves tab to another existing window
        /// </summary>
        private void MoveTabToOtherWindow(int tabIndex, MainWindow targetWindow)
        {
            if (tabIndex <= 0 || tabIndex >= _tabPages.Count) return;
            if (_tabPages.Count <= 1) return; // Cannot move last tab
            if (!targetWindow.CanAcceptTab()) return;
            
            int pageNumber = _tabPages[tabIndex];
            
            // Notify target window about tab transfer
            WindowManagerService.RequestTabTransfer(this, tabIndex, pageNumber, targetWindow, 0, 0);
            
            // Close tab in current window
            CloseTabByIndex(tabIndex);
        }
        
        /// <summary>
        /// Closes tab by index
        /// </summary>
        private void CloseTabByIndex(int tabIndex)
        {
            if (tabIndex <= 0 || tabIndex >= _tabPages.Count) return;
            
            var tabsContainer = this.FindControl<StackPanel>("TabsContainer");
            if (tabsContainer == null) return;
            
            // Find tab by Tag
            var tabToRemove = FindTabByIndex(tabIndex);
            if (tabToRemove != null)
            {
                tabsContainer.Children.Remove(tabToRemove);
            }
            
            _tabPages.RemoveAt(tabIndex);
            
            // Renumber tabs
            RenumberTabs();
            
            // If active tab closed, switch to previous
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
                
                // If dragging went outside window bounds
                if (distance > 50)
                {
                    Cursor = new Cursor(StandardCursorType.DragMove);
                    
                    // Check if cursor is over another window
                    var screenPos = this.PointToScreen(currentPoint);
                    var targetWindow = WindowManagerService.GetWindowAtPosition(screenPos.X, screenPos.Y, this);
                    
                    if (targetWindow != null && targetWindow.CanAcceptTab())
                    {
                        // Highlight target window
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
                
                // If significant dragging occurred - check target window
                if (distance > 50)
                {
                    var screenPos = this.PointToScreen(currentPoint);
                    var targetWindow = WindowManagerService.GetWindowAtPosition(screenPos.X, screenPos.Y, this);
                    
                    if (targetWindow != null && targetWindow.CanAcceptTab() && _tabPages.Count > 1)
                    {
                        // Move tab to another window
                        MoveTabToOtherWindow(_draggingTabIndex, targetWindow);
                    }
                    else if (targetWindow == null && _tabPages.Count > 1)
                    {
                        // If tab dropped outside all windows - create new window
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
