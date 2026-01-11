using System;
using System.Collections.Generic;
using System.Linq;
using German_B1._Step_Further;

namespace German_B1._Step_Further.Services
{
    /// <summary>
    /// Service for managing windows and drag-and-drop tabs between windows
    /// </summary>
    public static class WindowManagerService
    {
        /// <summary>
        /// List of all open main windows
        /// </summary>
        private static readonly List<WeakReference<MainWindow>> _openWindows = new();
        
        /// <summary>
        /// Event for notifying about tab transfer to another window
        /// </summary>
        public static event EventHandler<TabTransferEventArgs>? TabTransferRequested;
        
        /// <summary>
        /// Registers a new open window
        /// </summary>
        public static void RegisterWindow(MainWindow window)
        {
            // Clean up dead references
            CleanDeadReferences();
            
            _openWindows.Add(new WeakReference<MainWindow>(window));
        }
        
        /// <summary>
        /// Removes window from registry
        /// </summary>
        public static void UnregisterWindow(MainWindow window)
        {
            _openWindows.RemoveAll(wr => 
            {
                if (wr.TryGetTarget(out var w))
                    return w == window;
                return true; // Remove dead references
            });
        }
        
        /// <summary>
        /// Gets list of all open windows
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
        /// Number of open windows
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
        /// Finds window by its unique identifier
        /// </summary>
        public static MainWindow? GetWindowById(string windowId)
        {
            return GetOpenWindows().FirstOrDefault(w => w.WindowId == windowId);
        }
        
        /// <summary>
        /// Finds window at cursor position (for drag-and-drop)
        /// </summary>
        public static MainWindow? GetWindowAtPosition(double screenX, double screenY, MainWindow? excludeWindow = null)
        {
            foreach (var window in GetOpenWindows())
            {
                if (window == excludeWindow) continue;
                
                // Check if cursor is over the window
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
        /// Notifies about tab transfer request
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
        /// Cleans up dead references to closed windows
        /// </summary>
        private static void CleanDeadReferences()
        {
            _openWindows.RemoveAll(wr => !wr.TryGetTarget(out _));
        }
    }
    
    /// <summary>
    /// Tab transfer event arguments
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

