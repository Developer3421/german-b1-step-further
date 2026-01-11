namespace German_B1._Step_Further.Services
{
    /// <summary>
    /// Single source of truth for mapping between:
    /// - absolute book pages (1..164)
    /// - part number (1..4)
    /// - topic number inside the part
    ///
    /// IMPORTANT: All navigation and highlighting logic should use this class
    /// to avoid mismatched offsets between windows (Part3/Part4) and MainWindow.
    /// </summary>
    public static class BookNavigationMap
    {
        // Absolute book page range (as used by Resources Page{N}_* keys).
        public const int MinPage = 1;
        public const int MaxPage = 164;

        // Part ranges (absolute pages).
        public const int Part1StartPage = 1;
        public const int Part1EndPage = 56;

        public const int Part2StartPage = 57;
        public const int Part2EndPage = 110;

        public const int Part3StartPage = 111;
        public const int Part3EndPage = 146;

        public const int Part4StartPage = 147;
        public const int Part4EndPage = 164;

        // How many pages each topic consumes (left-to-right spread is 2 pages).
        // Current content is organized as 3 pages per topic across all parts.
        public const int PagesPerTopic = 3;

        /// <summary>
        /// Returns which part an absolute page belongs to.
        /// </summary>
        public static int GetPartNumber(int absolutePage)
        {
            if (absolutePage <= Part1EndPage) return 1;
            if (absolutePage <= Part2EndPage) return 2;
            if (absolutePage <= Part3EndPage) return 3;
            return 4;
        }

        /// <summary>
        /// Converts a (part, topicNumber) to the absolute start page of that topic.
        /// The returned page is clamped and normalized to the left page (odd).
        /// </summary>
        public static int GetAbsoluteLeftPageForTopic(int part, int topicNumber)
        {
            if (topicNumber < 1) topicNumber = 1;

            int baseStart = part switch
            {
                1 => 3,   // after contents pages 1-2
                2 => Part2StartPage,
                3 => Part3StartPage,
                4 => Part4StartPage,
                _ => MinPage
            };

            int page = baseStart + (topicNumber - 1) * PagesPerTopic;
            return ClampToValidLeftPage(page);
        }

        /// <summary>
        /// For a given absolute left page, returns topic number within that part.
        /// Returns -1 if the page is not within the requested part.
        /// </summary>
        public static int GetTopicNumberForLeftPage(int part, int leftPage)
        {
            leftPage = ClampToValidLeftPage(leftPage);

            var range = GetPartRange(part);
            int start = range.Start;
            int end = range.End;

            if (leftPage < start || leftPage > end) return -1;

            int baseStart = part switch
            {
                1 => 3,
                2 => Part2StartPage,
                3 => Part3StartPage,
                4 => Part4StartPage,
                _ => MinPage
            };

            // Integer division gives 0-based topic index.
            return (leftPage - baseStart) / PagesPerTopic + 1;
        }

        public static (int Start, int End) GetPartRange(int part)
        {
            return part switch
            {
                1 => (Part1StartPage, Part1EndPage),
                2 => (Part2StartPage, Part2EndPage),
                3 => (Part3StartPage, Part3EndPage),
                4 => (Part4StartPage, Part4EndPage),
                _ => (MinPage, MaxPage)
            };
        }

        /// <summary>
        /// Ensures the page is within [MinPage..MaxPage] and is a left page (odd).
        /// </summary>
        public static int ClampToValidLeftPage(int page)
        {
            if (page < MinPage) page = MinPage;
            if (page > MaxPage) page = MaxPage;

            // Left page must be odd.
            if (page % 2 == 0) page--;

            // If MaxPage is even, page-- could make it 163, which is fine.
            if (page < MinPage) page = MinPage;
            return page;
        }

        /// <summary>
        /// Returns a user-friendly absolute page range label for a topic.
        /// Example: "Pages 111-113".
        /// Uses <see cref="PagesPerTopic"/> and clamps to the part range.
        /// </summary>
        public static string GetTopicPageRangeLabel(int part, int topicNumber, string prefix = "Pages")
        {
            int left = GetAbsoluteLeftPageForTopic(part, topicNumber);

            // Topics span PagesPerTopic pages, but UI shows a left-right spread.
            // We'll show the full topic page span as [left..left+PagesPerTopic-1],
            // then clamp by part range.
            int right = left + PagesPerTopic - 1;

            var range = GetPartRange(part);
            if (left < range.Start) left = range.Start;
            if (right > range.End) right = range.End;

            return $"{prefix} {left}-{right}";
        }
    }
}
