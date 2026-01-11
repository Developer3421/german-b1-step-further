using LiteDB;
using System;
using System.Collections.Generic;

namespace German_B1._Step_Further.Models
{
    /// <summary>
    /// Model for saving tab state in session
    /// </summary>
    public class TabSession
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.NewObjectId();
        
        /// <summary>
        /// Unique window identifier
        /// </summary>
        public string WindowId { get; set; } = string.Empty;
        
        /// <summary>
        /// Tab index in window
        /// </summary>
        public int TabIndex { get; set; }
        
        /// <summary>
        /// Left page number for this tab
        /// </summary>
        public int PageNumber { get; set; }
        
        /// <summary>
        /// Whether this tab is active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Last access time
        /// </summary>
        public DateTime LastAccess { get; set; } = DateTime.Now;
    }
    
    /// <summary>
    /// Model for saving entire window session
    /// </summary>
    public class WindowSession
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.NewObjectId();
        
        /// <summary>
        /// Unique window identifier
        /// </summary>
        public string WindowId { get; set; } = string.Empty;
        
        /// <summary>
        /// List of page numbers for each tab
        /// </summary>
        public List<int> TabPages { get; set; } = new();
        
        /// <summary>
        /// Active tab index
        /// </summary>
        public int ActiveTabIndex { get; set; }
        
        /// <summary>
        /// Window X position
        /// </summary>
        public double PositionX { get; set; }
        
        /// <summary>
        /// Window Y position
        /// </summary>
        public double PositionY { get; set; }
        
        /// <summary>
        /// Window width
        /// </summary>
        public double Width { get; set; }
        
        /// <summary>
        /// Window height
        /// </summary>
        public double Height { get; set; }
        
        /// <summary>
        /// Last save time
        /// </summary>
        public DateTime LastSaved { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Whether window was maximized
        /// </summary>
        public bool IsMaximized { get; set; }
    }
}

