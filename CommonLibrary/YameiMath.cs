namespace CommonLibrary
{
    public static class YameiMath
    {
        /// <summary>
        /// 计算阶乘
        /// </summary>
        public static int Factorial(int n)
        {
            if (n == 0)
                return 1;

            return n * Factorial(n - 1);
        }

        /// <summary>
        /// 计算1+到N
        /// </summary>
        public static int SumFromOneToN(int n)
        {
            if (n == 0)
                return 0;

            return n * (n + 1) / 2;
        }
    }
}