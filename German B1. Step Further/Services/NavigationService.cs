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
        
        /// <summary>
        /// Викликає подію навігації до конкретної сторінки
        /// </summary>
        /// <param name="part">Номер частини (1-4)</param>
        /// <param name="pageNumber">Номер сторінки відносно частини</param>
        public static void RequestNavigation(int part, int pageNumber)
        {
            NavigateToPage?.Invoke(null, new NavigationEventArgs(part, pageNumber));
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
}

