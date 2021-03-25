using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class WordList
    {
        public string Name { get; }
        public string[] Languages { get; }

        private List<Word> list;

        public static string DestinationFolderPath { get => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Labb4"); }

        public Word this[int i]
        {
            get => list[i];
        }

        public WordList(string name, params string[] languages)
        {
            Name = name;
            Languages = languages;
            list = new List<Word>();
        }

        /// <summary>
        ///Returnerar array med namn på alla listor som finns lagrade(utan filändelsen). 
        /// </summary>
        public static string[] GetLists()
        {
            var dirInfo = Directory.CreateDirectory(DestinationFolderPath);

            var destinationFolderDATFiles = (from fileName in Directory.GetFiles(dirInfo.FullName, "*.dat")
                                             select Path.GetFileName(fileName)).ToArray();

            var sourceFolderDATFiles = (from fileName in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.dat")
                                        select Path.GetFileName(fileName)).ToArray();

            foreach (var sFileName in sourceFolderDATFiles)
            { 
                // om en .dat fils namn från source-mappen inte kan hittas i destinationsmappen, kopiera då över den .dat filen från source- till destinationsmappen.
                if (!Array.Exists(destinationFolderDATFiles, dFileName => dFileName.Equals(sFileName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    File.Copy(sourceFileName: Path.Combine(Directory.GetCurrentDirectory(), sFileName), destFileName: Path.Combine(dirInfo.FullName, sFileName));
                }
            }

            return (from fileName in Directory.GetFiles(dirInfo.FullName, "*.dat")
                    select Path.GetFileNameWithoutExtension(fileName)).ToArray();
        }

            /// <summary>
            /// Laddar in ordlistan och returnerar som WordList.
            /// </summary>
            /// <param name="name">Namn på ordlistan (utan filändelse).</param>
            public static WordList LoadList(string name)
            {
                string[] languages;
                WordList result = null;

                try
                {
                    var dirInfo = Directory.CreateDirectory(DestinationFolderPath);
                    string destFilePath = Path.Combine(dirInfo.FullName, $"{name}.dat");

                    if (!File.Exists(destFilePath))
                    {
                        string sourcefilePath = $@"..\..\..\{name}.dat";

                        try
                        {
                            File.Copy(sourcefilePath, destFilePath);
                        }
                        catch (FileNotFoundException)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;

                            Console.WriteLine($"Error: The Word List {name}.dat does not exist inside destination folder {dirInfo.FullName} nor could be copied over from source folder {Directory.GetCurrentDirectory()}.");

                            Console.ForegroundColor = ConsoleColor.Gray;

                            return null;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            return null;
                        }
                    }

                    using (var reader = new StreamReader(destFilePath))
                    {
                        languages = reader.ReadLine().Split(';').Where(s => s != string.Empty).ToArray();

                        result = new WordList(name, languages);

                        string nextLine;

                        while ((nextLine = reader.ReadLine()) != null)
                        {
                            result.Add(nextLine.Split(';').Where(s => s != string.Empty).ToArray());
                        }
                    }

                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                }

                return result;
            }

            /// <summary> 
            /// Sparar listan till en fil med samma namn som listan och filändelse.dat
            /// </summary>
            public void Save()
            {
                try
                {
                    Directory.CreateDirectory(DestinationFolderPath);
                    using (var writer = new StreamWriter(new FileStream(Path.Combine(DestinationFolderPath, Name + ".dat"), FileMode.Create)))
                    {
                        foreach (string l in Languages)
                            writer.Write($"{l};");

                        writer.WriteLine();

                        foreach (Word word in list)
                        {
                            if (word.Translations.Length != Languages.Length)
                                throw new ArgumentException("Wrong number of translations.");

                            foreach (string t in word.Translations)
                                writer.Write($"{t};");

                            writer.WriteLine();
                        }
                    }
                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            /// <summary>
            /// Lägger till ord i listan.
            /// </summary>
            /// <exception cref="System.ArgumentException">Kastas när fel antal translations anges.</exception>
            /// <param name="translations"></param>
            public void Add(params string[] translations)
            {
                if (translations.Length != Languages.Length)
                    throw new ArgumentException("Wrong number of translations.");
                else
                    list.Add(new Word(translations));
            }

            /// <summary>
            /// Sök igenom språket och ta bort ordet. 
            /// </summary>
            /// <param name="translation">Motsvarar index i this.Languages</param>
            /// <param name="wordStr">Ordet som ska sökas efter och tas bort.</param>
            /// <returns></returns>
            public bool Remove(int translation, string wordStr)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Translations[translation].Equals(wordStr, StringComparison.InvariantCulture))
                    {
                        list.RemoveAt(i);
                        return true;
                    }
                }
                return false;
                //TODO Ta bort alla översatta ord eller bara det på det angivna språket?
            }

            /// <summary>
            /// Returnerar det totala antalet ord i listan.
            /// </summary>
            public int Count() => list.Count;

            /// <summary>
            /// Sorterar översättningarna i ordlistan baserat på ett specificerat språk, i bokstavsordning. Anropar sedan ett callback med den sorterade datan som argument.
            /// </summary>
            /// <param name="sortByTranslation">Vilket språk listan ska sorteras på.</param>
            /// <param name="showTranslations">Callback som anropas för varje ord i listan.</param>
            public void List(int sortByTranslation, Action<string[]> showTranslations)
            {
                var translationsQuery = from w in list
                                        orderby w.Translations[sortByTranslation] ascending
                                        select w.Translations;

                foreach (string[] t in translationsQuery)
                    showTranslations(t);
            }

            /// <summary>
            /// Returnerar slumpmässigt Word från listan, med slumpmässigt valda FromLanguage och ToLanguage(dock inte samma).
            /// </summary>
            public Word GetWordToPractice()
            {
                Word result = null;
                Random rand = new Random();

                Word tempWord = list[rand.Next(0, list.Count)];

                if (tempWord.Translations.Length > 1)
                {
                    int fromLanguage = rand.Next(0, tempWord.Translations.Length);
                    int toLanguage;

                    do
                        toLanguage = rand.Next(0, tempWord.Translations.Length);
                    while (toLanguage == fromLanguage);

                    result = new Word(fromLanguage, toLanguage, tempWord.Translations);
                }
                return result;
            }

            public Word GetWord(string translation)
            {
                foreach (var word in list)
                    foreach (var t in word.Translations)
                        if (translation.Equals(t, StringComparison.InvariantCultureIgnoreCase))
                            return word;

                return null;
            }
        }
    }
