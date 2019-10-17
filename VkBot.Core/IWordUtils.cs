using System;
using System.Collections.Generic;
using System.Text;

namespace VkBot.Core
{
    public interface IWordUtils
    {
        /// <summary>
        /// Подбирает слово с нужным окончанием в зависимости от числа.
        /// Слова подбираются просто: 0 яблок, 1 яблоко, 2 яблока
        /// </summary>
        /// <param name="number">Число</param>
        /// <param name="zero">Слово для чисел 0, 5, 6, ..., 19 </param>
        /// <param name="one">Слово для числа 1</param>
        /// <param name="two">Слово для чисел 2, 3, 4</param>
        /// <returns>Слово с нужным окончанием</returns>
        string FormatCaseWord(int number, string zero, string one, string two);
    }
}
