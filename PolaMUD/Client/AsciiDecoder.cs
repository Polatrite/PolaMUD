using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    class AsciiDecoder
    {
        public static string AsciiToUnicode(byte[] bytes, int lengthToConvert)
        {
            Decoder decoder = System.Text.Encoding.ASCII.GetDecoder();

            char[] chars = new char[lengthToConvert];
            int charLen = decoder.GetChars(bytes, 0, lengthToConvert, chars, 0);
            return new string(chars);
        }
    }
}
