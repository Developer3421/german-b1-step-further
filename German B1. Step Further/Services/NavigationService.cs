using System;

namespace German_B1._Step_Further.Services
{
    /// <summary>
    /// Сервіс для синхронізації навігації між вікнами розділів та головним вікном
    /// </summary>
    public static class NavigationService
    {
        // Події для навігації
        public static event EventHandler<NavigationEventArgs>? NavigateToPage;
        
        // Подія для оновлення поточної сторінки у всіх слухачів
        public static event EventHandler<PageChangedEventArgs>? PageChanged;
        
        /// <summary>
        /// Викликає подію навігації до конкретної сторінки
        /// </summary>
        /// <param name="part">Номер частини (1-4)</param>
        /// <param name="pageNumber">Номер сторінки відносно частини</param>
        public static void RequestNavigation(int part, int pageNumber)
        {
            NavigateToPage?.Invoke(null, new NavigationEventArgs(part, pageNumber));
        }
        
        /// <summary>
        /// Сповіщає всіх слухачів про зміну поточної сторінки
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

