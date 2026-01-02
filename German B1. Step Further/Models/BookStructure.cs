using System.Collections.Generic;

namespace German_B1._Step_Further.Models
{
    public class BookPart
    {
        public int PartNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<BookTopic> Topics { get; set; } = new();
        public int StartPage { get; set; }
        public int EndPage { get; set; }
    }

    public class BookTopic
    {
        public int TopicNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<BookSubtopic> Subtopics { get; set; } = new();
        public int PageNumber { get; set; }
        public string? FilePath { get; set; }
    }

    public class BookSubtopic
    {
        public int SubtopicNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public string? FilePath { get; set; }
    }

    public class BookPage
    {
        public int PageNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<BookPart> Parts { get; set; } = new();
    }
}
