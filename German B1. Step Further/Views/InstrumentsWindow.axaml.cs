using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using WebViewControl;

namespace German_B1._Step_Further.Views
{
    public partial class InstrumentsWindow : Window
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        
        // Статичний кеш іконок (спільний для всіх екземплярів вікна)
        private static readonly System.Collections.Generic.Dictionary<string, Bitmap> _iconCache = new();

        private WebView? _webView;
        
        // Кеш WebView для різних сервісів
        private readonly System.Collections.Generic.Dictionary<string, WebView> _webViewCache = new();
        private string? _currentServiceUrl;
        
        // Посилання на Image контроли для іконок
        private Image? _geminiIcon;
        private Image? _copilotIcon;
        private Image? _chatgptIcon;
        private Image? _perplexityIcon;

        public InstrumentsWindow()
        {
            InitializeComponent();
            SetupEventHandlers();
            ShowToolsList();
            
            // Завантажуємо іконки коли вікно вже відкрилося та відрендерилося
            Opened += OnWindowOpened;
        }
        
        private async void OnWindowOpened(object? sender, EventArgs e)
        {
            // Відписуємося щоб не викликалося повторно
            Opened -= OnWindowOpened;
            
            // Невелика затримка щоб UI встиг відмалюватися
            await Task.Delay(50);
            
            // Тепер завантажуємо іконки
            await LoadFaviconsAsync();
        }

        private void SetupEventHandlers()
        {
            var closeButton = this.FindControl<Button>("CloseButton");
            var titleBarRow = this.FindControl<Grid>("TitleBarRow");
            var backToToolsButton = this.FindControl<Button>("BackToToolsButton");

            if (closeButton != null)
                closeButton.Click += CloseButton_Click;

            if (titleBarRow != null)
                titleBarRow.PointerPressed += TitleBar_PointerPressed;

            if (backToToolsButton != null)
                backToToolsButton.Click += BackToToolsButton_Click;

            // AI Tool Buttons
            var geminiButton = this.FindControl<Button>("GeminiButton");
            var copilotButton = this.FindControl<Button>("CopilotButton");
            var chatgptButton = this.FindControl<Button>("ChatGPTButton");
            var perplexityButton = this.FindControl<Button>("PerplexityButton");

            if (geminiButton != null)
                geminiButton.Click += AiToolButton_Click;
            if (copilotButton != null)
                copilotButton.Click += AiToolButton_Click;
            if (chatgptButton != null)
                chatgptButton.Click += AiToolButton_Click;
            if (perplexityButton != null)
                perplexityButton.Click += AiToolButton_Click;
            
            // Зберігаємо посилання на Image контроли для іконок
            _geminiIcon = this.FindControl<Image>("GeminiIcon");
            _copilotIcon = this.FindControl<Image>("CopilotIcon");
            _chatgptIcon = this.FindControl<Image>("ChatGPTIcon");
            _perplexityIcon = this.FindControl<Image>("PerplexityIcon");
        }

        private void ShowToolsList()
        {
            var webTopBar = this.FindControl<Border>("WebTopBar");
            var toolsListScroll = this.FindControl<ScrollViewer>("ToolsListScroll");
            var webHost = this.FindControl<Border>("WebHost");

            if (webTopBar != null) webTopBar.IsVisible = false;
            if (toolsListScroll != null) toolsListScroll.IsVisible = true;
            if (webHost != null) webHost.IsVisible = false;

            var currentServiceText = this.FindControl<TextBlock>("CurrentServiceText");
            if (currentServiceText != null) currentServiceText.Text = string.Empty;
        }

        private void ShowWebView(string title, string url)
        {
            var webTopBar = this.FindControl<Border>("WebTopBar");
            var toolsListScroll = this.FindControl<ScrollViewer>("ToolsListScroll");
            var webHost = this.FindControl<Border>("WebHost");
            var webViewContainer = this.FindControl<Grid>("WebViewContainer");

            if (webTopBar != null) webTopBar.IsVisible = true;
            if (toolsListScroll != null) toolsListScroll.IsVisible = false;
            if (webHost != null) webHost.IsVisible = true;

            var currentServiceText = this.FindControl<TextBlock>("CurrentServiceText");
            if (currentServiceText != null) currentServiceText.Text = title;

            if (webViewContainer == null) return;

            // Якщо ми вже відкривали цей сервіс, просто переключаємось на нього
            if (_currentServiceUrl == url && _webView != null)
            {
                Console.WriteLine($"Reusing existing WebView for {url}");
                return;
            }

            // Ховаємо поточний WebView якщо він є
            if (_webView != null && _currentServiceUrl != null)
            {
                Console.WriteLine($"Hiding WebView for {_currentServiceUrl}");
                _webViewCache[_currentServiceUrl] = _webView;
                webViewContainer.Children.Remove(_webView);
            }

            // Перевіряємо чи є WebView в кеші для цього URL
            if (_webViewCache.TryGetValue(url, out var cachedWebView))
            {
                Console.WriteLine($"Using cached WebView for {url}");
                _webView = cachedWebView;
                webViewContainer.Children.Clear();
                webViewContainer.Children.Add(_webView);
            }
            else
            {
                // Створюємо новий WebView
                Console.WriteLine($"Creating new WebView for {url}");
                _webView = new WebView
                {
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                    Address = url
                };

                webViewContainer.Children.Clear();
                webViewContainer.Children.Add(_webView);
            }

            _currentServiceUrl = url;
        }

        private async Task LoadFaviconsAsync()
        {
            // Завантажуємо всі іконки паралельно для швидкості
            var tasks = new[]
            {
                LoadFaviconAsync(_geminiIcon, "gemini.google.com"),
                LoadFaviconAsync(_copilotIcon, "copilot.microsoft.com"),
                LoadFaviconAsync(_chatgptIcon, "chatgpt.com"),
                LoadFaviconAsync(_perplexityIcon, "perplexity.ai")
            };
            
            await Task.WhenAll(tasks);
        }

        private async Task LoadFaviconAsync(Image? imageControl, string domain)
        {
            try
            {
                if (imageControl == null)
                {
                    Console.WriteLine($"Image control is null for domain: {domain}");
                    return;
                }

                // Перевіряємо чи є іконка в кеші
                if (_iconCache.TryGetValue(domain, out var cachedBitmap))
                {
                    Console.WriteLine($"Using cached favicon for {domain}");
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        imageControl.Source = cachedBitmap;
                    });
                    return;
                }

                // Google's favicon service (sz=64 for 64x64 size)
                var faviconUrl = $"https://www.google.com/s2/favicons?domain={domain}&sz=64";
                Console.WriteLine($"Loading favicon from: {faviconUrl}");
                
                var response = await _httpClient.GetAsync(faviconUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to load favicon for {domain}: {response.StatusCode}");
                    return;
                }

                var imageData = await response.Content.ReadAsByteArrayAsync();
                Console.WriteLine($"Loaded {imageData.Length} bytes for {domain}");
                
                using var stream = new MemoryStream(imageData);
                var bitmap = new Bitmap(stream);

                // Зберігаємо в кеш
                _iconCache[domain] = bitmap;
                Console.WriteLine($"Cached favicon for {domain}");

                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    imageControl.Source = bitmap;
                    Console.WriteLine($"Set bitmap for {domain}");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load favicon for {domain}: {ex.Message}");
            }
        }

        private void AiToolButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            var url = button.Tag as string;
            if (string.IsNullOrWhiteSpace(url))
                return;

            var title = button.Name switch
            {
                "GeminiButton" => "Google Gemini",
                "CopilotButton" => "Microsoft Copilot",
                "ChatGPTButton" => "ChatGPT",
                "PerplexityButton" => "Perplexity",
                _ => "Інструмент"
            };

            ShowWebView(title, url);
        }

        private void BackToToolsButton_Click(object? sender, RoutedEventArgs e)
        {
            ShowToolsList();
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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Важливо: dispose всіх WebView (CEF) щоб не тримати процеси
            try
            {
                // Dispose поточного WebView
                _webView?.Dispose();
                
                // Dispose всіх кешованих WebView
                foreach (var cachedWebView in _webViewCache.Values)
                {
                    try
                    {
                        cachedWebView.Dispose();
                    }
                    catch
                    {
                        // ignore individual disposal errors
                    }
                }
                
                _webViewCache.Clear();
            }
            catch
            {
                // ignore
            }
            finally
            {
                _webView = null;
                _currentServiceUrl = null;
            }
        }
    }
}
