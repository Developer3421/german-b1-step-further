using System;
using System.Collections.Generic;

namespace German_B1._Step_Further.Services
{
    /// <summary>
    /// Service for navigation between content pages with support for "Back" and "Forward" buttons
    /// </summary>
    public static class PageNavigationService
    {
        // Current page index
        private static int _currentPageIndex = 0;
        
        // List of pages for navigation (Part, SubPart, Page)
        private static readonly List<PageInfo> _pages = new();
        
        // Events for notifying about page changes
        public static event EventHandler<PageNavigationEventArgs>? PageChanged;
        
        /// <summary>
        /// Current page index
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
        /// Total number of pages
        /// </summary>
        public static int TotalPages => _pages.Count;
        
        /// <summary>
        /// Whether can go back
        /// </summary>
        public static bool CanGoBack => _currentPageIndex > 0;
        
        /// <summary>
        /// Whether can go forward
        /// </summary>
        public static bool CanGoForward => _currentPageIndex < _pages.Count - 1;
        
        /// <summary>
        /// Current page
        /// </summary>
        public static PageInfo? CurrentPage => _pages.Count > 0 && _currentPageIndex >= 0 && _currentPageIndex < _pages.Count 
            ? _pages[_currentPageIndex] 
            : null;
        
        /// <summary>
        /// Initialize list of pages
        /// </summary>
        public static void Initialize(List<PageInfo> pages)
        {
            _pages.Clear();
            _pages.AddRange(pages);
            _currentPageIndex = 0;
            OnPageChanged();
        }
        
        /// <summary>
        /// Add page
        /// </summary>
        public static void AddPage(PageInfo page)
        {
            _pages.Add(page);
        }
        
        /// <summary>
        /// Go to previous page
        /// </summary>
        public static void GoBack()
        {
            if (CanGoBack)
            {
                CurrentPageIndex--;
            }
        }
        
        /// <summary>
        /// Go to next page
        /// </summary>
        public static void GoForward()
        {
            if (CanGoForward)
            {
                CurrentPageIndex++;
            }
        }
        
        /// <summary>
        /// Go to specific page by index
        /// </summary>
        public static void GoToPage(int index)
        {
            if (index >= 0 && index < _pages.Count)
            {
                CurrentPageIndex = index;
            }
        }
        
        /// <summary>
        /// Go to page by part and topic number
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
    /// Page information
    /// </summary>
    public class PageInfo
    {
        /// <summary>
        /// Part number (1-4)
        /// </summary>
        public int Part { get; set; }
        
        /// <summary>
        /// Topic number
        /// </summary>
        public int Topic { get; set; }
        
        /// <summary>
        /// Subtopic number
        /// </summary>
        public int Subtopic { get; set; }
        
        /// <summary>
        /// Page title
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Subtitle (subtopic name)
        /// </summary>
        public string Subtitle { get; set; } = string.Empty;
        
        /// <summary>
        /// Resource key for content
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
    /// Page change event arguments for PageNavigationService
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

