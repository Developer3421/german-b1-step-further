using System;

namespace German_B1._Step_Further.Services
{
    /// <summary>
    /// Service for synchronizing navigation between part windows and main window
    /// </summary>
    public static class NavigationService
    {
        // Events for navigation
        public static event EventHandler<NavigationEventArgs>? NavigateToPage;
        
        // Event for updating current page in all listeners
        public static event EventHandler<PageChangedEventArgs>? PageChanged;
        
        /// <summary>
        /// Invokes navigation event to specific page
        /// </summary>
        /// <param name="part">Part number (1-4)</param>
        /// <param name="pageNumber">Page number relative to the part</param>
        public static void RequestNavigation(int part, int pageNumber)
        {
            NavigateToPage?.Invoke(null, new NavigationEventArgs(part, pageNumber));
        }
        
        /// <summary>
        /// Notifies all listeners about current page change
        /// </summary>
        public static void NotifyPageChanged(int leftPage, int rightPage)
        {
            PageChanged?.Invoke(null, new PageChangedEventArgs(leftPage, rightPage));
        }
    }
    
    public class NavigationEventArgs : EventArgs
    {
        public int Part { get; }
        public int PageNumber { get; }
        
        public NavigationEventArgs(int part, int pageNumber)
        {
            Part = part;
            PageNumber = pageNumber;
        }
    }
    
    public class PageChangedEventArgs : EventArgs
    {
        public int LeftPage { get; }
        public int RightPage { get; }
        
        public PageChangedEventArgs(int leftPage, int rightPage)
        {
            LeftPage = leftPage;
            RightPage = rightPage;
        }
    }
}

