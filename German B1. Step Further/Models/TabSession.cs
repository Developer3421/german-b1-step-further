using LiteDB;
using System;
using System.Collections.Generic;

namespace German_B1._Step_Further.Models
{
    /// <summary>
    /// Модель для збереження стану вкладки в сесії
    /// </summary>
    public class TabSession
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.NewObjectId();
        
        /// <summary>
        /// Унікальний ідентифікатор вікна
        /// </summary>
        public string WindowId { get; set; } = string.Empty;
        
        /// <summary>
        /// Індекс вкладки в вікні
        /// </summary>
        public int TabIndex { get; set; }
        
        /// <summary>
        /// Номер лівої сторінки для цієї вкладки
        /// </summary>
        public int PageNumber { get; set; }
        
        /// <summary>
        /// Чи є ця вкладка активною
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Час останнього доступу
        /// </summary>
        public DateTime LastAccess { get; set; } = DateTime.Now;
    }
    
    /// <summary>
    /// Модель для збереження сесії всього вікна
    /// </summary>
    public class WindowSession
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.NewObjectId();
        
        /// <summary>
        /// Унікальний ідентифікатор вікна
        /// </summary>
        public string WindowId { get; set; } = string.Empty;
        
        /// <summary>
        /// Список номерів сторінок для кожної вкладки
        /// </summary>
        public List<int> TabPages { get; set; } = new();
        
        /// <summary>
        /// Індекс активної вкладки
        /// </summary>
        public int ActiveTabIndex { get; set; }
        
        /// <summary>
        /// Позиція X вікна
        /// </summary>
        public double PositionX { get; set; }
        
        /// <summary>
        /// Позиція Y вікна
        /// </summary>
        public double PositionY { get; set; }
        
        /// <summary>
        /// Ширина вікна
        /// </summary>
        public double Width { get; set; }
        
        /// <summary>
        /// Висота вікна
        /// </summary>
        public double Height { get; set; }
        
        /// <summary>
        /// Час останнього збереження
        /// </summary>
        public DateTime LastSaved { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Чи було вікно розгорнуте
        /// </summary>
        public bool IsMaximized { get; set; }
    }
}

