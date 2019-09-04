using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibleConsole
{
    public class Book
    {
        public string Title { get; set; }
        public Dictionary<string, string> ChapterVerseDict { get; set; }

        public Book(string title, Dictionary<string, string> chapterVerseDict = null)
        {
            Title = title;
            ChapterVerseDict = chapterVerseDict ?? new Dictionary<string, string>();
        }
    }
}
