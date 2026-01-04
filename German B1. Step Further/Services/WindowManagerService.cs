using System;
using System.Collections.Generic;
using System.Linq;
using German_B1._Step_Further;

namespace German_B1._Step_Further.Services
{
    /// <summary>
    /// Сервіс для управління вікнами та drag-and-drop вкладок між вікнами
    /// </summary>
    public static class WindowManagerService
    {
        /// <summary>
        /// Список всіх відкритих головних вікон
        /// </summary>
        private static readonly List<WeakReference<MainWindow>> _openWindows = new();
        
        /// <summary>
        /// Подія для сповіщення про переміщення вкладки в інше вікно
        /// </summary>
        public static event EventHandler<TabTransferEventArgs>? TabTransferRequested;
        
        /// <summary>
        /// Реєструє нове відкрите вікно
        /// </summary>
        public static void RegisterWindow(MainWindow window)
        {
            // Очищаємо мертві посилання
            CleanDeadReferences();
            
            _openWindows.Add(new WeakReference<MainWindow>(window));
        }
        
        /// <summary>
        /// Видаляє вікно з реєстру
        /// </summary>
        public static void UnregisterWindow(MainWindow window)
        {
            _openWindows.RemoveAll(wr => 
            {
                if (wr.TryGetTarget(out var w))
                    return w == window;
                return true; // Видаляємо мертві посилання
            });
        }
        
        /// <summary>
        /// Отримує список всіх відкритих вікон
        /// </summary>
        public static List<MainWindow> GetOpenWindows()
        {
            CleanDeadReferences();
            
            var windows = new List<MainWindow>();
            foreach (var wr in _openWindows)
            {
                if (wr.TryGetTarget(out var window))
                    windows.Add(window);
            }
            return windows;
        }
        
        /// <summary>
        /// Кількість відкритих вікон
        /// </summary>
        public static int WindowCount
        {
            get
            {
                CleanDeadReferences();
                return _openWindows.Count;
            }
        }
        
        /// <summary>
        /// Знаходить вікно за його унікальним ідентифікатором
        /// </summary>
        public static MainWindow? GetWindowById(string windowId)
        {
            return GetOpenWindows().FirstOrDefault(w => w.WindowId == windowId);
        }
        
        /// <summary>
        /// Знаходить вікно за позицією курсора (для drag-and-drop)
        /// </summary>
        public static MainWindow? GetWindowAtPosition(double screenX, double screenY, MainWindow? excludeWindow = null)
        {
            foreach (var window in GetOpenWindows())
            {
                if (window == excludeWindow) continue;
                
                // Перевіряємо чи курсор знаходиться над вікном
                var bounds = window.Bounds;
                var pos = window.Position;
                
                if (screenX >= pos.X && screenX <= pos.X + bounds.Width &&
                    screenY >= pos.Y && screenY <= pos.Y + bounds.Height)
                {
                    return window;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Сповіщає про запит на переміщення вкладки
        /// </summary>
        public static void RequestTabTransfer(MainWindow sourceWindow, int tabIndex, int pageNumber, 
            MainWindow? targetWindow, double screenX, double screenY)
        {
            TabTransferRequested?.Invoke(null, new TabTransferEventArgs
            {
                SourceWindow = sourceWindow,
                TabIndex = tabIndex,
                PageNumber = pageNumber,
                TargetWindow = targetWindow,
                ScreenX = screenX,
                ScreenY = screenY
            });
        }
        
        /// <summary>
        /// Очищає мертві посилання на закриті вікна
        /// </summary>
        private static void CleanDeadReferences()
        {
            _openWindows.RemoveAll(wr => !wr.TryGetTarget(out _));
        }
    }
    
    /// <summary>
    /// Аргументи події переміщення вкладки
    /// </summary>
    public class TabTransferEventArgs : EventArgs
    {
        public MainWindow? SourceWindow { get; set; }
        public int TabIndex { get; set; }
        public int PageNumber { get; set; }
        public MainWindow? TargetWindow { get; set; }
        public double ScreenX { get; set; }
        public double ScreenY { get; set; }
    }
}

