using System;

namespace AssetBundleBrowser.Utilities
{
    public static class LevenshteinDistance
    {
        /// <summary>
        /// Get smaller num
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="third"></param>
        /// <returns></returns>
        private static int _LowerOfThree(int first, int second, int third)
        {
            int min = Math.Min(first, second);
            return Math.Min(min, third);
        }

        /// <summary>
        /// Caculate similar string return int [Levenshtein distance algorithm]
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        public static int LevenshteinDistanceInt(string str1, string str2)
        {
            int[,] Matrix;
            int n = str1.Length;
            int m = str2.Length;

            int temp, i, j;
            char ch1;
            char ch2;
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {

                return n;
            }
            Matrix = new int[n + 1, m + 1];

            for (i = 0; i <= n; i++)
            {
                // init first row
                Matrix[i, 0] = i;
            }

            for (j = 0; j <= m; j++)
            {
                // init first column
                Matrix[0, j] = j;
            }

            for (i = 1; i <= n; i++)
            {
                ch1 = str1[i - 1];
                for (j = 1; j <= m; j++)
                {
                    ch2 = str2[j - 1];
                    if (ch1.Equals(ch2))
                    {
                        temp = 0;
                    }
                    else
                    {
                        temp = 1;
                    }
                    Matrix[i, j] = _LowerOfThree(Matrix[i - 1, j] + 1, Matrix[i, j - 1] + 1, Matrix[i - 1, j - 1] + temp);
                }
            }

            return Matrix[n, m];
        }

        /// <summary>
        /// Caculate similar string return decimal (Percent) [Levenshtein distance algorithm]
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        public static decimal LevenshteinDistanceDecimal(string str1, string str2)
        {
            int val = LevenshteinDistanceInt(str1, str2);
            return 1 - (decimal)val / Math.Max(str1.Length, str2.Length);
        }
    }
}