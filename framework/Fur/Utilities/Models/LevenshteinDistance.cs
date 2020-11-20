using System;

namespace Fur.Utilities
{
    /// <summary>
    /// 编辑距离算法
    /// </summary>
    public class LevenshteinDistance
    {
        /// <summary>
        /// 取最小的一位数
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="third"></param>
        /// <returns></returns>
        private int LowerOfThree(int first, int second, int third)
        {
            int min = Math.Min(first, second);
            return Math.Min(min, third);
        }

        /// <summary>
        /// 比较相似度内部代码
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        private int Levenshtein_Distance(string str1, string str2)
        {
            int[,] Matrix;
            int n = str1.Length;
            int m = str2.Length;
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

            int i;
            for (i = 0; i <= n; i++)
            {
                //初始化第一列
                Matrix[i, 0] = i;
            }

            int j;
            for (j = 0; j <= m; j++)
            {
                //初始化第一行
                Matrix[0, j] = j;
            }

            for (i = 1; i <= n; i++)
            {
                ch1 = str1[i - 1];
                for (j = 1; j <= m; j++)
                {
                    ch2 = str2[j - 1];

                    int temp;
                    if (ch1.Equals(ch2))
                    {
                        temp = 0;
                    }
                    else
                    {
                        temp = 1;
                    }
                    Matrix[i, j] = LowerOfThree(Matrix[i - 1, j] + 1, Matrix[i, j - 1] + 1, Matrix[i - 1, j - 1] + temp);
                }
            }
            for (i = 0; i <= n; i++)
            {
                for (j = 0; j <= m; j++)
                {
                    Console.Write(" {0} ", Matrix[i, j]);
                }
                Console.WriteLine("");
            }

            return Matrix[n, m];
        }

        /// <summary>
        /// 计算字符串相似度
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        public decimal LevenshteinDistancePercent(string str1, string str2)
        {
            //int maxLenth = str1.Length > str2.Length ? str1.Length : str2.Length;
            int val = Levenshtein_Distance(str1, str2);
            return 1 - (decimal)val / Math.Max(str1.Length, str2.Length);
        }
    }
}