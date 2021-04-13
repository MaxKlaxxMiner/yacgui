using System;
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace FastBitmapLib.Extras
{
  /// <summary>
  /// Mapping-Class for conversion GDI+ 13-Bit to 16-Bit and back (inclusive gamma correction)
  /// </summary>
  public static class Color13Mapping
  {
    /// <summary>
    /// base mapping table (256 values)
    /// </summary>
    static readonly ushort[] sRgbMap =
    {
      0, 2, 5, 7, 10, 12, 15, 17, 20, 22, 25, 27, 30, 33, 36, 39,                                      // 0x00 - 0x0f
      42, 46, 50, 53, 57, 61, 66, 70, 75, 80, 85, 90, 95, 101, 106, 112,                               // 0x10 - 0x1f
      118, 125, 131, 138, 145, 152, 159, 166, 174, 182, 190, 198, 206, 215, 224, 233,                  // 0x20 - 0x2f
      242, 252, 261, 271, 281, 292, 302, 313, 324, 335, 347, 358, 370, 382, 395, 407,                  // 0x30 - 0x3f
      420, 433, 446, 460, 474, 488, 502, 516, 531, 546, 561, 576, 592, 608, 624, 641,                  // 0x40 - 0x4f
      657, 674, 691, 709, 726, 744, 762, 781, 799, 818, 838, 857, 877, 897, 917, 937,                  // 0x50 - 0x5f
      958, 979, 1001, 1022, 1044, 1066, 1088, 1111, 1134, 1157, 1181, 1204, 1228, 1253, 1277, 1302,    // 0x60 - 0x6f
      1327, 1353, 1378, 1404, 1431, 1457, 1484, 1511, 1539, 1566, 1594, 1623, 1651, 1680, 1709, 1739,  // 0x70 - 0x7f
      1768, 1798, 1829, 1859, 1890, 1921, 1953, 1985, 2017, 2049, 2082, 2115, 2148, 2182, 2216, 2250,  // 0x80 - 0x8f
      2285, 2320, 2355, 2390, 2426, 2462, 2498, 2535, 2572, 2610, 2647, 2685, 2723, 2762, 2801, 2840,  // 0x90 - 0x9f
      2880, 2920, 2960, 3000, 3041, 3082, 3124, 3166, 3208, 3250, 3293, 3336, 3380, 3423, 3467, 3512,  // 0xa0 - 0xaf
      3557, 3602, 3647, 3693, 3739, 3785, 3832, 3879, 3927, 3974, 4022, 4071, 4120, 4169, 4218, 4268,  // 0xb0 - 0xbf
      4318, 4369, 4419, 4471, 4522, 4574, 4626, 4679, 4732, 4785, 4838, 4892, 4947, 5001, 5056, 5111,  // 0xc0 - 0xcf
      5167, 5223, 5280, 5336, 5393, 5451, 5509, 5567, 5625, 5684, 5743, 5803, 5863, 5923, 5984, 6045,  // 0xd0 - 0xdf
      6106, 6168, 6230, 6293, 6356, 6419, 6482, 6546, 6611, 6675, 6740, 6806, 6871, 6938, 7004, 7071,  // 0xe0 - 0xef
      7138, 7206, 7274, 7342, 7411, 7480, 7550, 7619, 7690, 7760, 7831, 7903, 7974, 8047, 8119, 8192,  // 0xf0 - 0xff
      8192, 8192 // overflow placeholder
    };

    /// <summary>
    /// create mapping groups (count 16-bit samples per 13-bit value)
    /// </summary>
    /// <returns>counter groups</returns>
    static int[] GetRGBGroups()
    {
      var groups = new int[8193];
      int groupsLength = -1;
      uint lastSimple = uint.MaxValue;
      int maxSamples = 0;
      for (int g = 0; g <= 256; g++)
      {
        uint first = sRgbMap[g];
        uint next = sRgbMap[g + 1];
        for (uint i = 0; i <= 256 && maxSamples < 65536; i++, maxSamples++)
        {
          uint simple = first + (next - first) * i / 257;
          if (simple == lastSimple)
          {
            groups[groupsLength]++;
          }
          else
          {
            groups[++groupsLength] = 1;
            lastSimple = simple;
          }
        }
      }
      if (groupsLength + 1 != groups.Length) throw new IndexOutOfRangeException();

      return groups;
    }

    /// <summary>
    /// smooth counter group from rounding errors
    /// </summary>
    /// <param name="groups">counter groups</param>
    static void SmoothRGBGroups(int[] groups)
    {
      bool swaps = true;

      while (swaps)
      {
        swaps = false;
        for (int i = groups.Length - 2; i >= 0; i--)
        {
          while (groups[i] - 1 > groups[i + 1])
          {
            groups[i]--;
            groups[i + 1]++;
            swaps = true;
          }
        }

        for (int i = 0; i < groups.Length - 1; i++)
        {
          while (groups[i + 1] - 1 > groups[i])
          {
            groups[i]++;
            groups[i + 1]--;
            swaps = true;
          }
        }
      }
    }

    /// <summary>
    /// generate the 16-Bit to 13-Bit lookup table (0-65535) -> (0-8192)
    /// </summary>
    /// <returns>lookup-table</returns>
    public static ushort[] GetMap16to13()
    {
      var groups = GetRGBGroups();
      SmoothRGBGroups(groups);

      var results = new ushort[65536];
      int resultsLength = 0;

      for (uint g = 0; g < groups.Length; g++)
      {
        var group = groups[g];
        for (int i = 0; i < group; i++)
        {
          results[resultsLength++] = (ushort)g;
        }
      }
      if (resultsLength != results.Length) throw new IndexOutOfRangeException();

      return results;
    }

    /// <summary>
    /// generate the 13-Bit to 16-Bit lookup table (0-8192) -> (0-65535)
    /// </summary>
    /// <returns>lookup-table</returns>
    public static ushort[] GetMap13to16()
    {
      var groups = GetRGBGroups();
      SmoothRGBGroups(groups);

      var result = new ushort[8193];
      for (int i = 1; i < result.Length; i++)
      {
        result[i] = (ushort)(result[i - 1] + groups[i - 1]);
      }
      result[result.Length - 1] = 65535;

      return result;
    }
  }
}
