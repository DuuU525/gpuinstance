using System;
using System.Globalization;
using UnityEngine.Scripting;

namespace H3Unity
{
    [Preserve]
    public static partial class H3Utils
    {
        // ---- Angle Conversion ----

        public static double DegsToRads(double degrees) => degrees * (Math.PI / 180.0);

        public static double RadsToDegs(double radians) => radians * (180.0 / Math.PI);

        // ---- Hex Index Conversion ----

        public static string H3ToString(ulong index) => index.ToString("X");

        public static ulong StringToH3(string hex)
        {
            try
            {
                return ulong.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new H3Exception($"Failed to parse H3 index from string: '{hex}'", ex);
            }
        }

        public static bool TryParseH3(string hex, out ulong result)
        {
            return ulong.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
        }

        // ---- Bitmask Decoding ----

        public static int GetResolution(ulong h3) => (int)((h3 >> 52) & 0x0F);

        public static int GetBaseCellNumber(ulong h3) => (int)((h3 >> 45) & 0x7F);
        
        /// <summary>
        /// 补充H3Utils中缺失的基础单元格Hex转换方法
        /// </summary>
        /// <param name="baseCell"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        // 基础单元格索引转Hex字符串（H3基础单元格0-121）
        public static string GetBaseCellHex(int baseCell)
        {
            if (baseCell < 0 || baseCell >= 122)
                throw new ArgumentOutOfRangeException(nameof(baseCell), "Base cell must be 0-121");

            // 基础单元格的H3索引格式：分辨率0 + 基础单元格编号
            ulong baseIndex = (ulong)baseCell << 45; // 基础单元格编号占7位（bit45-51）
            baseIndex |= (ulong)0 << 52; // 分辨率0占4位（bit52-55）
            return baseIndex.ToString("X");
        }
    }
}
