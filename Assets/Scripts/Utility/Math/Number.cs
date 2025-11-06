namespace Halloween.Utility.Math
{
    public static class Number
    {
        /// <summary>
        /// 正の整数の桁数を、浮動小数点演算を使わずに計算する.
        /// </summary>
        /// <param name="value">桁数を計算する正の整数</param>
        /// <returns>整数の桁数</returns>
        public static int GetDigitCountIntOnly(int value)
        {
            if (value < 10) return 1;
            if (value < 100) return 2;
            if (value < 1000) return 3;
            if (value < 10000) return 4;
            if (value < 100000) return 5;
            if (value < 1000000) return 6;
            if (value < 10000000) return 7;
            if (value < 100000000) return 8;
            if (value < 1000000000) return 9;

            // int.MaxValue は 2,147,483,647 で10桁.
            return 10;
        }
    }
}
