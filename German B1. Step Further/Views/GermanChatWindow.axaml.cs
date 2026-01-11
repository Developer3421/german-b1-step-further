using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using LLama;
using LLama.Common;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace German_B1._Step_Further.Views
{
    public partial class GermanChatWindow : Window
    {
        // Static constructor — executed once before any other calls
        static GermanChatWindow()
        {
            // Select AVX2 instructions for improved quality and speed
            // AVX2 is supported by most modern processors (Intel Haswell+, AMD Excavator+)
            try
            {
                LLama.Native.NativeLibraryConfig.All.WithAvx(LLama.Native.AvxLevel.Avx2);
            }
            catch
            {
                // If library is already loaded — ignore
            }
        }

        private LLamaWeights? _model;
        private LLamaContext? _context;
        private InteractiveExecutor? _executor;
        private bool _isModelLoaded;
        private CancellationTokenSource? _cancellationTokenSource;

        private TextBlock? _statusText;
        private Border? _statusBar;
        private StackPanel? _chatMessagesPanel;
        private ScrollViewer? _chatScrollViewer;
        private TextBox? _messageInput;
        private Button? _sendButton;
        private ComboBox? _answerLanguageCombo;

        private const int MaxAiAnswerCharacters = 1024;

        // Minimal system prompts: only answer language.
        private const string SystemPromptUkrainian =
            "Ти — професійний викладач німецької мови рівня B1. Відповідай ВИКЛЮЧНО українською мовою. " +
            "Навіть якщо запит містить німецькі слова або речення — німецькі приклади/цитати залишай німецькою (без перекладу, якщо користувач не просив). " +
            "Пиши чітко, без сленгу, без зайвих вибачень. " +
            "Форматуй відповідь структуровано: коротке пояснення → правило/схема → 2–4 приклади німецькою → типові помилки (за потреби). " +
            "Не повторюй запит користувача і не додавай службових фраз.";

        private const string SystemPromptGerman = "Antworte auf Deutsch.";

        // Separate, maximally stable system prompt for grammar topic commands.
        private const string SystemPromptUkrainianGrammar =
            "Ти — професійний викладач німецької мови (B1). Відповідай ВИКЛЮЧНО українською. " +
            "Використовуй німецькі слова тільки всередині прикладів. " +
            "Дотримуйся СТАЛОЇ структури і не відхиляйся: " +
            "1) Назва теми. 2) Ключове правило (1–3 речення). 3) Схема/формула (коротко). " +
            "4) Приклади (рівно 4, німецькою). 5) Типові помилки (2 пункти). " +
            "Без вступів, без висновків, без повторення питання.";

        // Dictionary of grammar topics for special requests
        private static readonly System.Collections.Generic.Dictionary<int, string> GrammarTopics = new()
        {
            { 1, "Часи дієслів у німецькій мові (Präsens, Präteritum, Perfekt, Plusquamperfekt, Futur I, Futur II)" },
            { 2, "Модальні дієслова (Modalverben): können, müssen, wollen, sollen, dürfen, mögen" },
            { 3, "Підрядні речення (Nebensätze): weil, dass, wenn, obwohl, damit та порядок слів" },
            { 4, "Пасивний стан (Passiv): Vorgangspassiv та Zustandspassiv" },
            { 5, "Умовний спосіб (Konjunktiv II): würde + Infinitiv, hätte, wäre" },
            { 6, "Infinitiv mit zu: коли вживається і як будується" },
            { 7, "Означальні речення (Relativsätze): der, die, das як відносні займенники" },
            { 8, "Прийменники з відмінками (Präpositionen): Akkusativ, Dativ, Genitiv, Wechselpräpositionen" },
            { 9, "Відмінювання прикметників (Adjektivdeklination): після der/ein/ohne артикля" },
            { 10, "Непряма мова (Indirekte Rede): Konjunktiv I" },
            { 11, "Артиклі та відмінювання іменників (Artikeldeklination)" },
            { 12, "Однина і множина (Singular und Plural): правила утворення множини" }
        };

        private const int MaxBadAnswerRetries = 1;

        private ModelParams? _modelParams;

        public GermanChatWindow()
        {
            InitializeComponent();
            SetupEventHandlers();
            
            // Load model when opening window
            Opened += OnWindowOpened;
        }

        private void SetupEventHandlers()
        {
            var closeButton = this.FindControl<Button>("CloseButton");
            var titleBarRow = this.FindControl<Grid>("TitleBarRow");

            if (closeButton != null)
                closeButton.Click += CloseButton_Click;

            if (titleBarRow != null)
                titleBarRow.PointerPressed += TitleBar_PointerPressed;

            _statusText = this.FindControl<TextBlock>("StatusText");
            _statusBar = this.FindControl<Border>("StatusBar");
            _chatMessagesPanel = this.FindControl<StackPanel>("ChatMessagesPanel");
            _chatScrollViewer = this.FindControl<ScrollViewer>("ChatScrollViewer");
            _messageInput = this.FindControl<TextBox>("MessageInput");
            _sendButton = this.FindControl<Button>("SendButton");
            _answerLanguageCombo = this.FindControl<ComboBox>("AnswerLanguageCombo");

            if (_sendButton != null)
                _sendButton.Click += SendButton_Click;

            if (_messageInput != null)
                _messageInput.KeyDown += MessageInput_KeyDown;
        }

        private async void OnWindowOpened(object? sender, EventArgs e)
        {
            Opened -= OnWindowOpened;
            await Task.Delay(100);
            await LoadModelAsync();
        }

        private async Task LoadModelAsync()
        {
            try
            {
                UpdateStatus("Завантаження AI моделі Gemma 3...");

                // Model path
                var modelPath = Path.Combine(AppContext.BaseDirectory, "Ai model", "gemma-3-270m-it-UD-Q2_K_XL.gguf");
                
                if (!File.Exists(modelPath))
                {
                    // Try alternative path (for development)
                    var projectDir = Path.GetDirectoryName(AppContext.BaseDirectory);
                    while (projectDir != null && !Directory.Exists(Path.Combine(projectDir, "Ai model")))
                    {
                        projectDir = Path.GetDirectoryName(projectDir);
                    }
                    
                    if (projectDir != null)
                    {
                        modelPath = Path.Combine(projectDir, "Ai model", "gemma-3-270m-it-UD-Q2_K_XL.gguf");
                    }
                }

                if (!File.Exists(modelPath))
                {
                    UpdateStatus($"❌ Модель не знайдена: {modelPath}", isError: true);
                    return;
                }

                UpdateStatus($"Завантаження: {Path.GetFileName(modelPath)}...");

                await Task.Run(() =>
                {
                    var parameters = new ModelParams(modelPath)
                    {
                        ContextSize = 2048,
                        GpuLayerCount = 0 // CPU only для сумісності
                    };


                    _modelParams = parameters;

                    _model = LLamaWeights.LoadFromFile(parameters);
                    _context = _model.CreateContext(parameters);
                    _executor = new InteractiveExecutor(_context);
                });

                _isModelLoaded = true;
                UpdateStatus("✅ Модель завантажена! Можете почати розмову.", isSuccess: true);
                
                if (_sendButton != null)
                    _sendButton.IsEnabled = true;

                // Add welcome message
                AddMessage("Hallo! Ich bin dein Gesprächspartner für Deutsch B1. Schreib mir auf Deutsch oder Ukrainisch, und ich werde dir helfen, dein Deutsch zu verbessern! 🇩🇪", isUser: false);
            }
            catch (Exception ex)
            {
                UpdateStatus($"❌ Помилка завантаження: {ex.Message}", isError: true);
                Console.WriteLine($"Model loading error: {ex}");
            }
        }

        private void UpdateStatus(string message, bool isError = false, bool isSuccess = false)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_statusText != null)
                    _statusText.Text = message;

                if (_statusBar != null)
                {
                    if (isError)
                        _statusBar.Background = new SolidColorBrush(Color.Parse("#FFFFCDD2"));
                    else if (isSuccess)
                        _statusBar.Background = new SolidColorBrush(Color.Parse("#FFC8E6C9"));
                    else
                        _statusBar.Background = new SolidColorBrush(Color.Parse("#FFFFF8DC"));
                }
            });
        }

        private void AddMessage(string text, bool isUser)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_chatMessagesPanel == null) return;

                var messageBorder = new Border
                {
                    Background = isUser 
                        ? new SolidColorBrush(Color.Parse("#FFDCEDC8")) 
                        : new SolidColorBrush(Color.Parse("#FFFFFFFF")),
                    CornerRadius = isUser 
                        ? new CornerRadius(12, 12, 0, 12) 
                        : new CornerRadius(12, 12, 12, 0),
                    Padding = new Thickness(12, 8),
                    Margin = isUser 
                        ? new Thickness(60, 4, 8, 4) 
                        : new Thickness(8, 4, 60, 4),
                    HorizontalAlignment = isUser 
                        ? HorizontalAlignment.Right 
                        : HorizontalAlignment.Left,
                    MaxWidth = 500
                };

                var textBlock = new TextBlock
                {
                    Text = text,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.Parse("#FF333333"))
                };

                messageBorder.Child = textBlock;
                _chatMessagesPanel.Children.Add(messageBorder);

                // Scroll down
                _chatScrollViewer?.ScrollToEnd();
            });
        }

        private Border? _currentAiMessageBorder;
        private TextBlock? _currentAiMessageText;

        private void StartAiMessage()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_chatMessagesPanel == null) return;

                _currentAiMessageBorder = new Border
                {
                    Background = new SolidColorBrush(Color.Parse("#FFFFFFFF")),
                    CornerRadius = new CornerRadius(12, 12, 12, 0),
                    Padding = new Thickness(12, 8),
                    Margin = new Thickness(8, 4, 60, 4),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    MaxWidth = 500
                };

                _currentAiMessageText = new TextBlock
                {
                    Text = "...",
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.Parse("#FF333333"))
                };

                _currentAiMessageBorder.Child = _currentAiMessageText;
                _chatMessagesPanel.Children.Add(_currentAiMessageBorder);
                _chatScrollViewer?.ScrollToEnd();
            });
        }

        private void UpdateAiMessage(string text)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_currentAiMessageText != null)
                {
                    _currentAiMessageText.Text = text;
                    _chatScrollViewer?.ScrollToEnd();
                }
            });
        }

        private async void SendButton_Click(object? sender, RoutedEventArgs e)
        {
            await SendMessageAsync();
        }

        private async void MessageInput_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                e.Handled = true;
                await SendMessageAsync();
            }
        }

        private async Task SendMessageAsync()
        {
            if (!_isModelLoaded || _executor == null || _messageInput == null || _sendButton == null)
                return;

            var userMessage = _messageInput.Text?.Trim();
            if (string.IsNullOrEmpty(userMessage))
                return;

            _messageInput.Text = string.Empty;
            _sendButton.IsEnabled = false;

            AddMessage(userMessage, isUser: true);

            // Check if this is a grammar topic request
            var (isGrammarTopic, specialPrompt, specialSystem) = TryGetGrammarTopicPrompt(userMessage);

            string systemPrompt;
            string actualUserMessage;

            if (isGrammarTopic && specialPrompt != null && specialSystem != null)
            {
                // Special mode for grammar topics — professional Ukrainian and stable format.
                systemPrompt = specialSystem;
                actualUserMessage = specialPrompt;
            }
            else
            {
                // Normal mode — depends on language selection
                var answerLanguage = GetSelectedAnswerLanguage();
                systemPrompt = answerLanguage switch
                {
                    AnswerLanguage.German => SystemPromptGerman,
                    AnswerLanguage.NoRestriction => "", // Без системного промпту
                    _ => SystemPromptUkrainian
                };
                actualUserMessage = userMessage;
            }

            // system+user prompt (or just user if no restriction)
            var prompt = string.IsNullOrEmpty(systemPrompt) 
                ? BuildUserOnlyPrompt(actualUserMessage) 
                : BuildPrompt(systemPrompt, actualUserMessage);

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                StartAiMessage();

                var inferenceParams = new InferenceParams
                {
                    // For grammar topics give slightly more tokens, but keep structure.
                    MaxTokens = isGrammarTopic ? 650 : 512,
                    AntiPrompts = new[] { "<end_of_turn>", "</s>", "User:", "<start_of_turn>" }
                };

                // For stability of template answers reduce temperature.
                inferenceParams.SamplingPipeline = new LLama.Sampling.DefaultSamplingPipeline
                {
                    Temperature = isGrammarTopic ? 0.25f : 0.60f,
                    TopP = isGrammarTopic ? 0.85f : 0.9f,
                    RepeatPenalty = 1.15f
                };

                string finalResponse = await GenerateLimitedAnswerAsync(prompt, inferenceParams, _cancellationTokenSource);

                // If model repeated/translated request instead of answering — regenerate once with stricter rules.
                // But not for grammar topics (they already have special prompt)
                if (!isGrammarTopic)
                {
                    for (var retry = 0; retry < MaxBadAnswerRetries && LooksLikeEchoOrTranslation(finalResponse, actualUserMessage); retry++)
                    {
                        var answerLanguage = GetSelectedAnswerLanguage();
                        var stricterSystem = systemPrompt + "\n" +
                                            (answerLanguage == AnswerLanguage.German
                                                ? "NOCHMAL: Gib NUR die fertige Antwort. Keine Frage, kein Zitat, keine Übersetzung."
                                                : "ЩЕ РАЗ: Дай ТІЛЬКИ готову відповідь. Без запиту, без цитат, без перекладу.");

                        // Додатково: явний маркер OUTPUT ONLY, щоб модель не починала з перефразування питання.
                        var retryUser = "OUTPUT ONLY:\n" + actualUserMessage;
                        var retryPrompt = BuildPrompt(stricterSystem, retryUser);
                        finalResponse = await GenerateLimitedAnswerAsync(retryPrompt, inferenceParams, _cancellationTokenSource);
                    }
                }

                if (string.IsNullOrWhiteSpace(finalResponse))
                    finalResponse = "Вибач, не зрозумів. Переформулюй, будь ласка.";

                UpdateAiMessage(finalResponse);
            }
            catch (OperationCanceledException)
            {
                UpdateAiMessage("[Відповідь скасована]");
            }
            catch (Exception ex)
            {
                UpdateAiMessage($"[Помилка: {ex.Message}]");
                Console.WriteLine($"Inference error: {ex}");
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                
                if (_sendButton != null)
                    _sendButton.IsEnabled = true;
                
                _currentAiMessageBorder = null;
                _currentAiMessageText = null;

                // FULL REBOOT of LLamaSharp after each response
                await RebootModelContextAsync();
            }
        }

        private static bool LooksLikeEchoOrTranslation(string answer, string userMessage)
        {
            if (string.IsNullOrWhiteSpace(answer) || string.IsNullOrWhiteSpace(userMessage))
                return false;

            static string Norm(string s) => s.Trim().Replace("\r", " ").Replace("\n", " ").ToLowerInvariant();

            var a = Norm(answer);
            var u = Norm(userMessage);

            // If answer is long enough (>150 chars) — consider it adequate
            // (model generated something, not just repeated)
            if (a.Length > 150)
                return false;

            // If user explicitly requested translation/explanation — don't block
            var isExplanationRequest = u.Contains("поясни") || u.Contains("explain") || u.Contains("erkläre") ||
                                       u.Contains("різниц") || u.Contains("unterschied") || u.Contains("difference") ||
                                       u.Contains("приклад") || u.Contains("beispiel") || u.Contains("example") ||
                                       u.Contains("переклад") || u.Contains("übersetz") || u.Contains("translat");
            if (isExplanationRequest)
                return false;

            // 1) Explicit FULL repetition of request (almost identical text)
            if (a.Length > 10 && a.Contains(u) && a.Length < u.Length * 1.5)
                return true;

            // 2) Answer start EXACTLY repeats request start (explicit echo)
            var prefixLen = Math.Min(50, Math.Min(a.Length, u.Length));
            if (prefixLen >= 30 && a.Substring(0, prefixLen) == u.Substring(0, prefixLen))
                return true;

            // 3) Too large word overlap AND short answer (echo/paraphrase)
            var overlap = WordOverlapRatio(a, u);
            if (overlap >= 0.75 && a.Length < 100)
                return true;

            return false;
        }

        private static double WordOverlapRatio(string a, string u)
        {
            static string[] Tokens(string s)
            {
                var sb = new StringBuilder(s.Length);
                foreach (var ch in s)
                {
                    sb.Append(char.IsLetterOrDigit(ch) ? ch : ' ');
                }

                return sb.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }

            var aTokens = Tokens(a);
            var uTokens = Tokens(u);
            if (aTokens.Length == 0 || uTokens.Length == 0)
                return 0;

            var aSet = new System.Collections.Generic.HashSet<string>(aTokens);
            var uSet = new System.Collections.Generic.HashSet<string>(uTokens);

            var intersect = 0;
            foreach (var t in aSet)
            {
                if (uSet.Contains(t)) intersect++;
            }

            // fraction of common words from answer words
            return intersect / (double)Math.Max(1, aSet.Count);
        }

        private enum AnswerLanguage
        {
            Ukrainian,
            German,
            NoRestriction
        }

        private AnswerLanguage GetSelectedAnswerLanguage()
        {
            // 0 = Ukrainian, 1 = German, 2 = NoRestriction
            var idx = _answerLanguageCombo?.SelectedIndex ?? 0;
            return idx switch
            {
                1 => AnswerLanguage.German,
                2 => AnswerLanguage.NoRestriction,
                _ => AnswerLanguage.Ukrainian
            };
        }

        private static string BuildPrompt(string system, string user)
        {
            return $"<start_of_turn>system\n{system}<end_of_turn>\n<start_of_turn>user\n{user}<end_of_turn>\n<start_of_turn>model\n";
        }

        private static string BuildUserOnlyPrompt(string user)
        {
            // Without system turn: just user -> model
            return $"<start_of_turn>user\n{user}<end_of_turn>\n<start_of_turn>model\n";
        }

        /// <summary>
        /// Checks if request is a grammar topic request and returns special prompt
        /// </summary>
        private static (bool isGrammarTopic, string? specialPrompt, string? specialSystem) TryGetGrammarTopicPrompt(string userMessage)
        {
            var lower = userMessage.ToLowerInvariant().Trim();

            // Patterns: "граматична тема 1", "тема 1", "grammar topic 1" etc.
            var patterns = new[] { "граматична тема", "тема", "grammar topic", "grammatik thema" };

            foreach (var pattern in patterns)
            {
                if (lower.StartsWith(pattern))
                {
                    var rest = lower.Substring(pattern.Length).Trim();
                    if (int.TryParse(rest, out var topicNum) && GrammarTopics.TryGetValue(topicNum, out var topicName))
                    {
                        // Deterministic template request for grammar topic.
                        // This increases stability and reduces "creative" deviations of model.
                        var prompt =
                            "ТЕМА: " + topicName + "\n" +
                            "ЗАВДАННЯ: Поясни тему для рівня B1 за заданою структурою.\n" +
                            "ВИМОГИ: Відповідь українською. Приклади тільки німецькою.\n" +
                            "ФОРМАТ: 1) Назва теми. 2) Ключове правило. 3) Схема/формула. 4) Приклади (4). 5) Типові помилки (2).";

                        return (true, prompt, SystemPromptUkrainianGrammar);
                    }
                }
            }

            return (false, null, null);
        }


        private async Task RebootModelContextAsync()
        {
            // Reboot: create new context/executor on same weights (without re-LoadFromFile).
            // Need real ModelParams. We save it during LoadModelAsync.
            try
            {
                if (_model == null || _modelParams == null)
                    return;

                await Task.Run(() =>
                {
                    _executor = null;

                    _context?.Dispose();
                    _context = null;

                    // Create new context with same parameters (ContextSize/GpuLayerCount etc.)
                    _context = _model.CreateContext(_modelParams);
                    _executor = new InteractiveExecutor(_context);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RebootModelContext error: {ex}");
            }
        }

        private async Task<string> GenerateLimitedAnswerAsync(string prompt, InferenceParams inferenceParams, CancellationTokenSource cts)
        {
            if (_executor == null)
                return string.Empty;

            var responseBuilder = new StringBuilder();

            try
            {
                await foreach (var text in _executor.InferAsync(prompt, inferenceParams, cts.Token))
                {
                    responseBuilder.Append(text);

                    var currentText = CleanResponse(responseBuilder.ToString());
                    currentText = LimitToMaxChars(currentText, MaxAiAnswerCharacters, out var wasTruncated);

                    UpdateAiMessage(currentText);

                    if (wasTruncated)
                    {
                        cts.Cancel();
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // If we stopped generation by limit ourselves — it's ok.
            }

            var finalText = CleanResponse(responseBuilder.ToString());
            return LimitToMaxChars(finalText, MaxAiAnswerCharacters, out _);
        }

         /// <summary>
         /// Cleaning response from repetitions and nonsense
         /// </summary>
         private static string CleanResponse(string response)
         {
            if (string.IsNullOrWhiteSpace(response))
                return string.Empty;

            try
            {
                var cleaned = response.Trim();

                // Remove model tags
                cleaned = cleaned.Replace("<end_of_turn>", "")
                                 .Replace("</s>", "")
                                 .Replace("<start_of_turn>", "")
                                 .Replace("user\n", "")
                                 .Replace("model\n", "")
                                 .Trim();

                // Remove repetitive sentences
                cleaned = RemoveRepetitions(cleaned);

                return cleaned.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CleanResponse error: {ex.Message}");
                // Return safe variant without aggressive truncate — limit is applied above.
                return response.Trim();
            }
        }

        private static string LimitToMaxChars(string text, int maxChars, out bool wasTruncated)
        {
            wasTruncated = false;
            if (string.IsNullOrEmpty(text) || maxChars <= 0)
                return string.Empty;

            if (text.Length <= maxChars)
                return text;

            wasTruncated = true;
            return text.Substring(0, maxChars).TrimEnd();
        }

        /// <summary>
        /// Removes repetitive sentences and phrases
        /// </summary>
        private static string RemoveRepetitions(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            try
            {
                // Split into sentences
                var sentences = text.Split(new[] { ". ", "! ", "? " }, StringSplitOptions.RemoveEmptyEntries);
                var uniqueSentences = new System.Collections.Generic.List<string>();
                var seen = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var sentence in sentences)
                {
                    var trimmed = sentence.Trim();
                    if (trimmed.Length > 0 && !seen.Contains(trimmed))
                    {
                        seen.Add(trimmed);
                        uniqueSentences.Add(trimmed);
                    }
                }

                if (uniqueSentences.Count == 0)
                    return text;

                // Join unique sentences
                var result = string.Join(". ", uniqueSentences);
                
                // Add period at end if missing
                if (result.Length > 0 && !result.EndsWith(".") && !result.EndsWith("!") && !result.EndsWith("?"))
                    result += ".";

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RemoveRepetitions error: {ex.Message}");
                return text; // Return original if something went wrong
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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Cancel current generation
            _cancellationTokenSource?.Cancel();

            // Clean up LLama resources
            try
            {
                _executor = null;
                _context?.Dispose();
                _model?.Dispose();
            }
            catch
            {
                // ignore
            }
            finally
            {
                _context = null;
                _model = null;
            }
        }
    }
}

