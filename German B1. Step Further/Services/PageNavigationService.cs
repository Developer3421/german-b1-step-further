using System;
using System.Collections.Generic;

namespace German_B1._Step_Further.Services
{
    /// <summary>
    /// Сервіс для навігації між сторінками контенту з підтримкою кнопок "Назад" та "Вперед"
    /// </summary>
    public static class PageNavigationService
    {
        // Поточний індекс сторінки
        private static int _currentPageIndex = 0;
        
        // Список сторінок для навігації (Part, SubPart, Page)
        private static readonly List<PageInfo> _pages = new();
        
        // Події для сповіщення про зміну сторінки
        public static event EventHandler<PageNavigationEventArgs>? PageChanged;
        
        /// <summary>
        /// Поточний індекс сторінки
        /// </summary>
        public static int CurrentPageIndex
        {
            get => _currentPageIndex;
            private set
            {
                if (value >= 0 && value < _pages.Count)
                {
                    _currentPageIndex = value;
                    OnPageChanged();
                }
            }
        }
        
        /// <summary>
        /// Загальна кількість сторінок
        /// </summary>
        public static int TotalPages => _pages.Count;
        
        /// <summary>
        /// Чи можна перейти назад
        /// </summary>
        public static bool CanGoBack => _currentPageIndex > 0;
        
        /// <summary>
        /// Чи можна перейти вперед
        /// </summary>
        public static bool CanGoForward => _currentPageIndex < _pages.Count - 1;
        
        /// <summary>
        /// Поточна сторінка
        /// </summary>
        public static PageInfo? CurrentPage => _pages.Count > 0 && _currentPageIndex >= 0 && _currentPageIndex < _pages.Count 
            ? _pages[_currentPageIndex] 
            : null;
        
        /// <summary>
        /// Ініціалізація списку сторінок
        /// </summary>
        public static void Initialize(List<PageInfo> pages)
        {
            _pages.Clear();
            _pages.AddRange(pages);
            _currentPageIndex = 0;
            OnPageChanged();
        }
        
        /// <summary>
        /// Додати сторінку
        /// </summary>
        public static void AddPage(PageInfo page)
        {
            _pages.Add(page);
        }
        
        /// <summary>
        /// Перейти на попередню сторінку
        /// </summary>
        public static void GoBack()
        {
            if (CanGoBack)
            {
                CurrentPageIndex--;
            }
        }
        
        /// <summary>
        /// Перейти на наступну сторінку
        /// </summary>
        public static void GoForward()
        {
            if (CanGoForward)
            {
                CurrentPageIndex++;
            }
        }
        
        /// <summary>
        /// Перейти на конкретну сторінку за індексом
        /// </summary>
        public static void GoToPage(int index)
        {
            if (index >= 0 && index < _pages.Count)
            {
                CurrentPageIndex = index;
            }
        }
        
        /// <summary>
        /// Перейти на сторінку за частиною та номером підтеми
        /// </summary>
        public static void GoToPage(int part, int topic, int subtopic)
        {
            var index = _pages.FindIndex(p => p.Part == part && p.Topic == topic && p.Subtopic == subtopic);
            if (index >= 0)
            {
                CurrentPageIndex = index;
            }
        }
        
        private static void OnPageChanged()
        {
            PageChanged?.Invoke(null, new PageNavigationEventArgs(CurrentPage, _currentPageIndex, _pages.Count));
        }
    }
    
    /// <summary>
    /// Інформація про сторінку
    /// </summary>
    public class PageInfo
    {
        /// <summary>
        /// Номер частини (1-4)
        /// </summary>
        public int Part { get; set; }
        
        /// <summary>
        /// Номер теми
        /// </summary>
        public int Topic { get; set; }
        
        /// <summary>
        /// Номер підтеми
        /// </summary>
        public int Subtopic { get; set; }
        
        /// <summary>
        /// Заголовок сторінки
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Підзаголовок (назва підтеми)
        /// </summary>
        public string Subtitle { get; set; } = string.Empty;
        
        /// <summary>
        /// Ключ ресурсу для контенту
        /// </summary>
        public string ContentResourceKey { get; set; } = string.Empty;
        
        public PageInfo() { }
        
        public PageInfo(int part, int topic, int subtopic, string title, string subtitle, string contentKey)
        {
            Part = part;
            Topic = topic;
            Subtopic = subtopic;
            Title = title;
            Subtitle = subtitle;
            ContentResourceKey = contentKey;
        }
    }
    
    /// <summary>
    /// Аргументи події зміни сторінки для PageNavigationService
    /// </summary>
    public class PageNavigationEventArgs : EventArgs
    {
        public PageInfo? CurrentPage { get; }
        public int CurrentIndex { get; }
        public int TotalPages { get; }
        
        public PageNavigationEventArgs(PageInfo? currentPage, int currentIndex, int totalPages)
        {
            CurrentPage = currentPage;
            CurrentIndex = currentIndex;
            TotalPages = totalPages;
        }
    }
}

