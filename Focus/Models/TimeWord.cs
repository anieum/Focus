using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Focus.Models
{
    class TimeWord
    {
        public string Word { get; private set; }
        public int Length { get; private set; }

        private static readonly Dictionary<string, int> _translationTable = new();

        public TimeWord(string word, int lengthInSeconds)
        {
            Word = word;
            Length = lengthInSeconds;

            _translationTable[word.ToUpper()] = lengthInSeconds;
        }

        public static int TryTranslate(string word)
        {
            int temp;
            _translationTable.TryGetValue(word.ToUpper(), out temp);
            return temp;
        }
    }
}
