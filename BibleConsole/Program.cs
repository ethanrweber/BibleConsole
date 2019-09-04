using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BibleConsole
{
    class Program
    {
        private const string biblefile = "bible.txt";
        static void Main(string[] args)
        {
            Console.WriteLine("loading...");
            var Bible = BuildBible();

            string bookPattern = "[a-zA-z]\\w+";
            string bookChapterPattern = bookPattern+"[ ][1-9]+";
            string bookChapterVersePattern = bookChapterPattern+"[:][1-9]+";

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
            Dictionary<string, Book> bible = new Dictionary<string, Book>();

            // each book of the bible
            Book book = new Book("genesis");

            // the current line - book chapter#:verse# verse as a string
            foreach (string line in bibleArr)
            {
                // split the current line into an array of strings
                string[] lineSplit = line.Split();

                // get the title of the book for the current line
                string bookTitle = char.IsDigit(lineSplit[0][0]) // if c/v in form "# book" instead of "book"
                    ? (lineSplit[0] + " " + lineSplit[1]).ToLower()
                    : lineSplit[0].ToLower();

                // when a new book is encountered, add the current book to the bible and reset the bibleBook variable
                if (book.Title != bookTitle)
                {
                    bible[book.Title] = book;
                    book = new Book(bookTitle);
                }
                // add the current verse to the current book
                book.ChapterVerseDict[lineSplit[char.IsDigit(line[0]) ? 2 : 1]] = line;
            }

            return bible;
        }

        private static void PrintSection(Dictionary<string, Book> bible, string input)
        {
            string[] inputArr = input.Split(' ', ':');
            string book, chapter = "", verse = "";
            // check for special cases of titles - "1 Samuel" for ex.
            if (char.IsDigit(inputArr[0][0]))
            {
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
                // error check
                int chapterNum = int.Parse(chapter);
                var a = bible[book].ChapterVerseDict.Last();
                var b = a.Value.Split(':')[0].Substring(book.Length).Trim();
                int.TryParse(b, out int lastChapter);
                if (chapterNum < 1 || chapterNum > lastChapter)
                {
                    Console.WriteLine($"seems like {book} doesn't contain chapter {chapterNum} - it only goes to chapter {chapterNum}");
                    return;
                }
                // go through each line
                foreach (KeyValuePair<string, string> kvp in bible[book].ChapterVerseDict)
                {
                    // find first occurrence of number (chapter)
                    string[] verseSplit = kvp.Value.Split(' ', ':');
                    foreach (string s in verseSplit)
                    {
                        int.TryParse(s, out int i);
                        if (i < 1 || i > lastChapter)
                        {
                            Console.WriteLine("oh fuck");
                            return;
                        }
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
                string chapVerse = chapter + ":" + verse;
                Console.WriteLine(bible[book].ChapterVerseDict.ContainsKey(chapVerse)
                    ? bible[book].ChapterVerseDict[chapVerse]
                    : $"the book of {book} doesn't have chapter {chapter} verse {verse} :(");
            }

            Console.WriteLine("\n");
        }
    }
}
