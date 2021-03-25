using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary
{
    public class Word
    {
        public string[] Translations { get; }
        public int FromLanguage { get; }
        public int ToLanguage { get; }

        public Word(params string[] translations) : this(0,0, translations) { }

        public Word(int fromLanguage, int toLanguage, params string[] translations)
        {
            FromLanguage = fromLanguage;
            ToLanguage = toLanguage;
            Translations = translations;
        }

        /// <summary>
        /// Kollar om två Words är lika genom att jämföra deras egenskaper.
        /// </summary>
        /// <param name="word">Word att jämföra med.</param>
        /// <returns></returns>
        public bool Equals(Word word)
        {
            return (Translations.SequenceEqual(word.Translations) && FromLanguage == word.FromLanguage && ToLanguage == word.ToLanguage);
        }
    }
}
