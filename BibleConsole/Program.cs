using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace BibleConsole
{
    class Book
    {
        public string Title { get; set; }
        public Dictionary<string, string> ChapterVerseDict { get; set; }

        public Book(string title, Dictionary<string, string> chapterVerseDict = null)
        {
            Title = title;
            ChapterVerseDict = chapterVerseDict ?? new Dictionary<string, string>();
        }
    }

    class Program
    {
        private const string biblefile = "bible.txt";
        static void Main(string[] args)
        {
            Console.WriteLine("loading...");
            var Bible = BuildBible();

            const string bookPattern = "[a-zA-z]\\w+";
            const string bookChapterPattern = "[a-zA-z]\\w+[ ][1-9]+";
            const string bookChapterVersePattern = "[a-zA-Z]\\w+[ ][1-9]+[:][1-9]+";

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Welcome to the bible! Here's your options:");
                Console.WriteLine("1. type a book to display the whole book (this takes a moment to print!)");
                Console.WriteLine("2. type a book and chapter as 'book #' to display that book's chapter");
                Console.WriteLine("3. type a book, chapter, and verse as 'book #:#' to display a specific verse");
                Console.WriteLine("4. type 'quit' to quit\n");

                string input = Console.ReadLine();
                if (input.Trim().ToLower() == "quit")
                    break;

                if (Regex.IsMatch(input, bookChapterVersePattern) 
                    || Regex.IsMatch(input, bookChapterPattern)
                    || Regex.IsMatch(input, bookPattern))
                {
                    PrintSection(Bible, input);
                    Console.WriteLine("Would you like to find another passage? click enter to run again, or type anything else to quit.");
                    string next = Console.ReadLine();
                    if (!string.IsNullOrEmpty(next))
                        break;
                }
                else
                    Console.WriteLine("Sorry, that doesn't look like a valid bible verse! Try again");
            }
        }

        private static Dictionary<string, Book> BuildBible()
        {
            string[] bibleArr = File.ReadAllLines(biblefile);

            // the whole bible
            Dictionary<string, Book> Bible = new Dictionary<string, Book>();

            // each book of the bible
            Book bibleBook = new Book("genesis");

            // the current line - book chapter#:verse# verse as a string
            foreach (string bibleLine in bibleArr)
            {
                // split the current line into an array of strings
                string[] bibleLineSplit = bibleLine.Split();

                // get the title of the book for the current line
                string book = char.IsDigit(bibleLineSplit[0][0]) // if c/v in form "# book" instead of "book"
                    ? (bibleLineSplit[0] + " " + bibleLineSplit[1]).ToLower()
                    : bibleLineSplit[0].ToLower();

                // when a new book is encountered, add the current book to the bible and reset the bibleBook variable
                if (bibleBook.Title != book)
                {
                    Bible[bibleBook.Title] = bibleBook;
                    bibleBook = new Book(book);
                }
                // add the current verse to the current book
                bibleBook.ChapterVerseDict[bibleLineSplit[char.IsDigit(bibleLine[0]) ? 2 : 1]] = bibleLine;
            }

            return Bible;
        }

        private static void PrintSection(Dictionary<string, Book> bible, string input)
        {
            string[] inputArr = input.Split(' ', ':');
            string book;
            string chapter = "";
            string verse = "";
            bool numberBook = false;
            if (char.IsDigit(inputArr[0][0]))
            {
                numberBook = true;
                book = (inputArr[0] + " " + inputArr[1]).ToLower();
                if(inputArr.Length > 2)
                    chapter = inputArr[2];
                if(inputArr.Length > 3)
                    verse = inputArr[3];
            }
            else
            {
                book = inputArr[0].ToLower();
                if(inputArr.Length > 1)
                    chapter = inputArr[1];
                if(inputArr.Length > 2)
                    verse = inputArr[2];
            }

            if (string.IsNullOrWhiteSpace(book))
                throw new Exception("book not parsed correctly");

            if (!bible.ContainsKey(book))
            {
                Console.WriteLine($"whoops, the bible doesn't seem to contain the book {book}");
                return;
            }

            // print book
            if (string.IsNullOrWhiteSpace(chapter))
            {
                foreach (KeyValuePair<string, string> kvp in bible[book].ChapterVerseDict)
                    Console.WriteLine(kvp.Value);
            }
            // print chapter
            else if (string.IsNullOrWhiteSpace(verse))
            {
                // go through each line
                foreach (KeyValuePair<string, string> kvp in bible[book].ChapterVerseDict)
                {
                    // find first occurrence of number (chapter)
                    int chapterNum = int.Parse(chapter);
                    string[] verseSplit = kvp.Value.Split(' ', ':');
                    foreach (string s in verseSplit)
                    {
                        int.TryParse(s, out int i);
                        // if the chapter in this line is the requested chapter, print it
                        if (i == chapterNum)
                        {
                            Console.WriteLine(kvp.Value);
                            break;
                        }
                    }
                }
            }
            // print verse
            else
            {
                Console.WriteLine(bible[book].ChapterVerseDict[chapter + ":" + verse]);
            }

            
            Console.WriteLine("\n");
        }
    }
}
