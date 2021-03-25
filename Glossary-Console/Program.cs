using ClassLibrary;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace Labb4
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintParameterInfo();
                return;
            }

            bool isCommandValid = false;

            if (args[0].Equals("-lists", StringComparison.InvariantCultureIgnoreCase))
                isCommandValid = ExecuteListsCommand();

            else if (args[0].Equals("-new", StringComparison.InvariantCultureIgnoreCase))
                isCommandValid = ExecuteNewCommand(args);

            else if (args[0].Equals("-add", StringComparison.InvariantCultureIgnoreCase))
                isCommandValid = ExecuteAddCommand(args);

            else if (args[0].Equals("-remove", StringComparison.InvariantCultureIgnoreCase))
                isCommandValid = ExecuteRemoveCommand(args);

            else if (args[0].Equals("-words",StringComparison.InvariantCultureIgnoreCase))
                isCommandValid = ExecuteWordsCommand(args);

            else if (args[0].Equals("-count", StringComparison.InvariantCultureIgnoreCase))
                isCommandValid = ExecuteCountCommand(args);

            else if (args[0].Equals("-practice", StringComparison.InvariantCultureIgnoreCase))
                isCommandValid = ExecutePracticeCommand(args);

            if (!isCommandValid)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid command!");

                Console.ForegroundColor = ConsoleColor.Gray;
                PrintParameterInfo();
            }
        }

        /// <summary>
        /// Konsollprintar de möjliga parametrarna som kan användas för det här programmet.
        /// </summary>
        static void PrintParameterInfo()
        {
            Console.WriteLine("Use any of the following parameters:\n" +
                    "-lists\n" +
                    "-new <list name> <language 1> <language 2> .. <langauge n>\n" +
                    "-add <list name>\n" +
                    "-remove <list name> <language> <word 1> <word 2> .. <word n>\n" +
                    "-words<listname> <sortByLanguage>\n" +
                    "-count <listname>\n" +
                    "-practice<listname>\n");
        }

        /// <summary>
        /// Listar namnen på alla ordlistor från mappen i appdata/local/Labb4
        /// </summary>
        /// <returns>Förutsätter att kommandot "-lists" skrevs in. Returnerar alltid true.</returns>
        static bool ExecuteListsCommand()
        {
            var lists = WordList.GetLists();

            Console.WriteLine("Word List .dat files found in {0}:", WordList.DestinationFolderPath);

            for (int i = 0; i < lists.Length; i++)
                Console.WriteLine($"{i + 1}: {lists[i]}");

            return true;
        }

        /// <summary>
        /// Skapar en ny ordlista med angivet namn och så många språk som angivits. Skickar ordlistan som parameter till metoden AddWordsToList och slutligen sparar listan om det går.
        /// </summary>
        /// <param name="args">Måste innehålla parametrarna "list name", "language 1", language 2" .. "language n"</param>
        /// <returns>True om kommandot är skrivet i rätt format, annars false.</returns>
        static bool ExecuteNewCommand(string[] args)
        {
            if (args.Length >= 4)
            {
                string[] languages = new string[args.Length - 2];

                for (int i = 2; i < args.Length; i++)
                    languages[i - 2] = args[i];

                AddWordsToList(new WordList(name: args[1], languages));

                return true;
            }
            return false;
        }

        /// <summary>
        /// Laddar namngiven ordlista och skickar den som parameter till metoden AddWordsToList.
        /// </summary>
        /// <param name="args">Måste innehålla parametern "list name".</param>
        /// <returns>True om kommandot är skrivet i rätt format, annars false.</returns>
        static bool ExecuteAddCommand(string[] args)
        {
            if (args.Length == 2)
            {
                AddWordsToList(WordList.LoadList(name: args[1]));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Raderar angivna ord från namngiven lista och språk.
        /// </summary>
        /// <param name="args">Måste innehålla parametrarna "list name", "language", "word 1", "word 2" .. "word n"</param>
        /// <returns>True om kommandot är skrivet i rätt format, annars false.</returns>
        static bool ExecuteRemoveCommand(string[] args)
        {
            if (args.Length >= 4)
            {
                WordList wordList = WordList.LoadList(name: args[1]);

                if (wordList == null)
                {
                    PrintWordListNotLoadedError();
                    return true;
                }

                string language = args[2];
                string[] wordsToBeRemoved = new string[args.Length - 3];

                for (int i = 3; i < args.Length; i++)
                    wordsToBeRemoved[i - 3] = args[i];

                int languageIndex = -1;

                for (int i = 0; i < wordList.Languages.Length; i++)
                {
                    if (language == wordList.Languages[i])
                    {
                        languageIndex = i;
                        break;
                    }
                }

                if (languageIndex == -1)
                {
                    Console.WriteLine($"Error: {wordList.Name}.dat does not contain the language {language}.");
                    return true;
                }

                bool isWordRemoved = false;

                Console.WriteLine("Removing the following  translations from {0}.dat:", wordList.Name);
                foreach (string wordStr in wordsToBeRemoved)
                {
                    Word currentWord = wordList.GetWord(wordStr);

                    if (wordList.Remove(languageIndex, wordStr))
                    {
                        for (int i = 0; i < currentWord.Translations.Length; i++)
                        {
                            if (i == currentWord.Translations.Length - 1)
                                Console.Write(currentWord.Translations[i]);
                            else
                                Console.Write(currentWord.Translations[i] + ", ");
                        }
                        isWordRemoved = true;
                    }

                    Console.WriteLine();
                }

                if (isWordRemoved)
                    wordList.Save();
                else
                    Console.WriteLine("None of the entered words could be found and therefore could not be removed.");

                return true;
            }
            return false;
        }

        /// <summary>
        /// Listar ord(alla språk) från angiven lista.Om man anger språk sorteras listan efter det, annars sortera efter första språket.
        /// </summary>
        /// <param name="args">Måste innehålla parametrarna "list name" och valfritt "sortByLanguage".</param>
        /// <returns>True om kommandot är skrivet i rätt format, annars false.</returns>
        static bool ExecuteWordsCommand(string[] args)
        {
            if (args.Length == 2 || args.Length == 3)
            {
                WordList wordList = WordList.LoadList(args[1]);

                if (wordList == null)
                {
                    PrintWordListNotLoadedError();
                    return true;
                }

                int sortByLanguage;

                if (args.Length == 2)
                    sortByLanguage = 0;
                else
                    sortByLanguage = Array.FindIndex(wordList.Languages, 
                        lang => lang.Equals(args[2], StringComparison.InvariantCultureIgnoreCase));

                if (sortByLanguage == -1)
                {
                    Console.WriteLine($"The language \"{args[2]}\" is not available for the word list {wordList.Name}.dat");
                    return true;
                }

                Action<string[]> showTranslations = (string[] translations) =>
                {
                    for (int i = 0; i < translations.Length; i++)
                    {
                        if (i == translations.Length - 1)
                            Console.Write(translations[i]);
                        else
                            Console.Write(translations[i] + ", ");
                    }
                    Console.WriteLine();
                };
                
                wordList.List(sortByLanguage, showTranslations);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Skriver ut hur många ord det finns i namngiven lista. 
        /// </summary>
        /// <param name="args">Måste innehålla parametern "list name".</param>
        /// <returns>True om kommandot är skrivet i rätt format, annars false.</returns>
        static bool ExecuteCountCommand(string[] args)
        {
            if (args.Length == 2)
            {
                string listName = args[1];
                WordList wordList = WordList.LoadList(listName);

                if (wordList == null)
                {
                    PrintWordListNotLoadedError();
                    return true;
                }

                Console.WriteLine("Word count in {0}: {1}", listName, wordList.Count());
                Console.WriteLine("Translation count in {0}: {1}", listName, wordList.Count() * wordList.Languages.Length);
                return true;
            }
            return false;
        }

        /// <summary>
        ///Ber användaren översätta ett slumpvis valt ord ur listan från ett slumpvis valt språk till ett annat.Skriver ut om det var rätt eller fel, och fortsätter fråga efter ord tills användaren lämnar en tom inmatning.Då skrivs antal övade ord ut, samt
        /// hur stor andel av orden man haft rätt på.
        /// </summary>
        /// <param name="args">Måste innehålla parametern "list name".</param>
        /// <returns>True om kommandot är skrivet i rätt format, annars false.</returns>
        static bool ExecutePracticeCommand(string[] args)
        {
            if (args.Length == 2)
            {
                WordList wordList = WordList.LoadList(name: args[1]);

                if (wordList == null)
                {
                    PrintWordListNotLoadedError();
                    return true;
                }

                int score = 0;
                var timeStart = DateTime.Now;

                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine("Starting Word Practicing Game!");

                int consumedCount = 0;
                Word[] consumedWords = new Word[wordList.Count() * wordList.Languages.Length];

                while (true)
                {
                    if (consumedCount >= consumedWords.Length)
                    {
                        EndPracticeAndPrintScore(score, timeStart);
                        return true;
                    }

                    Word practiceWord = wordList.GetWordToPractice();

                    while (Array.Exists(consumedWords, w => w != null && w.Equals(practiceWord)))
                    {
                        practiceWord = wordList.GetWordToPractice();
                    }

                    consumedWords[consumedCount++] = practiceWord;

                    Console.ForegroundColor = ConsoleColor.Gray;

                    Console.Write("Please translate {0} in {1} to {2}: ", 
                        practiceWord.Translations[practiceWord.FromLanguage], wordList.Languages[practiceWord.FromLanguage], wordList.Languages[practiceWord.ToLanguage]);

                    string answer = Console.ReadLine();
                    
                    if (!answer.Equals(practiceWord.Translations[practiceWord.ToLanguage], StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (answer == string.Empty)
                        {
                            EndPracticeAndPrintScore(score, timeStart);
                            return true;
                        }

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Wrong answer!");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Correct answer!");

                        score++;
                    }
                }
            }
            return false;
        }

        static void EndPracticeAndPrintScore(int score, DateTime startTime)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("Ending Word Practicing Game.");

            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("Score = {0}", score);

            var timeResult = DateTime.Now.Subtract(startTime);
            Console.WriteLine("Time: {0}m {1}s", (int)timeResult.TotalSeconds / 60, (int)timeResult.TotalSeconds % 60);

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        /// Frågar användaren efter ett nytt ord(på listans första språk), och frågar därefter i /tur och ordning efter översättningar till alla språk i listan.
        /// Sedan fortsätter den att fråga efter nya ord tills användaren avbryter genom att mata in en tom rad. Sparar slutligen listan om det går.
        /// </summary>
        /// <param name="wordList">Listan att addera ord till.</param>
        static void AddWordsToList(WordList wordList)
        {
            if (wordList == null)
            {
                PrintWordListNotLoadedError();
                return;
            }

            bool areWordsAdded = false;

            while (true)
            {
                Console.Write("Write a word in {0}: ", wordList.Languages[0]);

                string[] translations = new string[wordList.Languages.Length];
                string firstTranslation = translations[0] = Console.ReadLine();

                if (firstTranslation == string.Empty)
                    break;

                for (int i = 1; i < translations.Length; i++)
                {
                    string transl;
                    do
                    {
                        Console.Write("Translate {0} to {1}: ", firstTranslation, wordList.Languages[i]);
                        transl = Console.ReadLine();

                    } while (transl == string.Empty);

                    translations[i] = transl;
                }

                wordList.Add(translations);
                areWordsAdded = true;
            }

            if (areWordsAdded)
                wordList.Save();
        }

        /// <summary>
        /// Konsollprintar felmeddelandet om att en ordlista inte kunde laddas.
        /// </summary>
        static void PrintWordListNotLoadedError() 
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: The specified Word List could not be loaded.");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"Tip: You can create a Word List file by entering the command: -new <list name> <language 1> <language 2> .. <langauge n>");

            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
