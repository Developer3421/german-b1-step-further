using Avalonia.Controls;
using Avalonia.Interactivity;

namespace German_B1._Step_Further.Views
{
    public partial class ContentPageView : UserControl
    {
        private TextBlock? _pageTitle;
        private TextBlock? _pageSubtitle;
        private TextBlock? _pageContent;
        private TextBlock? _pageNumber;
        
        // Поточний номер сторінки
        private int _currentPageNumber = 1;

        public ContentPageView()
        {
            InitializeComponent();
            
            // Знаходимо контроли
            _pageTitle = this.FindControl<TextBlock>("PageTitle");
            _pageSubtitle = this.FindControl<TextBlock>("PageSubtitle");
            _pageContent = this.FindControl<TextBlock>("PageContent");
            _pageNumber = this.FindControl<TextBlock>("PageNumber");
        }

        /// <summary>
        /// Встановити контент сторінки за номером
        /// </summary>
        public void SetPage(int pageNumber, string title, string subtitle, string contentKey)
        {
            _currentPageNumber = pageNumber;
            
            if (_pageTitle != null)
                _pageTitle.Text = title;
            
            if (_pageSubtitle != null)
                _pageSubtitle.Text = subtitle;
            
            if (_pageContent != null)
            {
                // contentKey вже передається як "Page{n}_Content"
                if (App.Current?.Resources.TryGetResource(contentKey, null, out var resource) == true && resource is string content)
                {
                    _pageContent.Text = content;
                }
                else
                {
                    _pageContent.Text = $"Контент для сторінки {pageNumber} не знайдено.";
                }
            }
            
            if (_pageNumber != null)
                _pageNumber.Text = $"— {pageNumber} —";
        }
        
        /// <summary>
        /// Встановити порожню сторінку (для сторінок без контенту)
        /// </summary>
        public void SetEmptyPage(int pageNumber)
        {
            _currentPageNumber = pageNumber;
            
            if (_pageTitle != null)
                _pageTitle.Text = "";
            
            if (_pageSubtitle != null)
                _pageSubtitle.Text = "";
            
            if (_pageContent != null)
                _pageContent.Text = "";
            
            if (_pageNumber != null)
                _pageNumber.Text = $"— {pageNumber} —";
        }

        /// <summary>
        /// Поточний номер сторінки
        /// </summary>
        public int CurrentPageNumber => _currentPageNumber;
    }
}

