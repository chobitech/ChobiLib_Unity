
using System.Collections.Generic;
using Chobitech;

namespace ChobiLib
{
    public class RandomString : Randomize<string>
    {
        private const string digits = "0123456789";
        private const string lowerHex = "abcdef";
        private const string lowerNonHex = "ghijklmnopqrstuvwxyz";

        public static RandomString GetDigitsRandomString() => new(digits);
        public static RandomString GetLowerHexRandomString() => new(digits + lowerHex);
        public static RandomString GetUpperHexRandomString() => new(digits + lowerHex.ToUpper());
        public static RandomString GetHexRandomString() => new(digits + lowerHex + lowerHex.ToUpper());
        public static RandomString GetLowerCharDigitsRandomString() => new(digits + lowerHex + lowerNonHex);
        public static RandomString GetUpperCharDigitsRandomString() => new(digits + (lowerHex + lowerNonHex).ToUpper());
        public static RandomString GetCharDigitsRandomString()
        {
            var cStr = lowerHex + lowerNonHex;
            return new(digits + cStr + cStr.ToUpper());
        }


        public RandomString(IList<string> seeds) : base(seeds)
        {

        }

        public RandomString(string str) : base(str.ToStringList())
        {

        }


        public string GetRandomString(int length, bool duplicate = true) => GetRandomArray(length, duplicate).JoinToString();
    }
}