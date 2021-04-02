/*
 *   Distance transform algorithm described in:
 *
 *   Distance Transforms of Sampled Functions
 *   Pedro F. Felzenszwalb and Daniel P. Huttenlocher
 *   Cornell Computing and Information Science TR2004-1963
 *
 */

using System;
// ReSharper disable UnusedMember.Global

namespace FastBitmapLib.Extras
{
  public static class DistanceTransform
  {
    /// <summary>
    /// "infinity" value
    /// </summary>
    const float INF = 1e20f;

    /// <summary>
    /// Calculate (squared) distances for a single line
    /// </summary>
    /// <param name="f">Pointer to the line</param>
    /// <param name="n">Pixelcount</param>
    static unsafe void TransformLine(float* f, int n)
    {
      var d = &f[n];
      var z = &d[n];
      var v = (int*)&z[n + 1];

      int k = 0;
      v[0] = 0;
      z[0] = -INF;
      z[1] = +INF;
      for (int q = 1; q < n; q++)
      {
        int q2 = q * q;
        float s = (f[q] + q2 - (f[v[k]] + v[k] * v[k])) / (2 * q - 2 * v[k]);
        while (s <= z[k])
        {
          k--;
          s = (f[q] + q2 - (f[v[k]] + v[k] * v[k])) / (2 * q - 2 * v[k]);
        }
        k++;
        v[k] = q;
        z[k] = s;
        z[k + 1] = +INF;
      }

      k = 0;
      for (int q = 0; q < n; q++)
      {
        while (z[k + 1] < q) k++;
        d[q] = f[v[k]] + (q - v[k]) * (q - v[k]);
      }
    }

    /// <summary>
    /// Calculate (squared) distances for a complete picture
    /// </summary>
    /// <param name="distanceValues">Pointer to the entire picture</param>
    /// <param name="width">Width of the picture</param>
    /// <param name="height">Height of the picture</param>
    static unsafe void TransformField(float* distanceValues, int width, int height)
    {
      var tmpBuffer = new float[Math.Max(width, height) * 4 + 1];
      fixed (float* f = tmpBuffer)
      {
        // transform along columns
        for (int x = 0; x < width; x++)
        {
          for (int y = 0; y < height; y++) f[y] = distanceValues[x + y * width];
          TransformLine(f, height);
          for (int y = 0; y < height; y++) distanceValues[x + y * width] = f[height + y];
        }

        // transform along rows
        for (int y = 0; y < height; y++)
        {
          var imf = &distanceValues[y * width];
          for (int x = 0; x < width; x++) f[x] = imf[x];
          TransformLine(f, width);
          for (int x = 0; x < width; x++) imf[x] = f[width + x];
        }
      }
    }

    /// <summary>
    /// Generate the pixel distances (multiplied by 256)
    /// </summary>
    /// <param name="bits">Bits of the picture (True = solid, False = empty)</param>
    /// <param name="width">Width of the picture</param>
    /// <param name="height">Height of the picture</param>
    /// <returns>Calculated pixel distances</returns>
    public static unsafe int[] GenerateMap(bool[] bits, int width, int height)
    {
      if (bits == null) throw new NullReferenceException("bits");
      if (bits.Length < width * height) throw new ArgumentException();

      var distances = new int[bits.Length];

      fixed (int* tmpData = distances)
      {
        for (int i = 0; i < distances.Length; i++)
        {
          ((float*)tmpData)[i] = bits[i] ? 0 : INF;
        }

        TransformField((float*)tmpData, width, height);

        for (int i = 0; i < distances.Length; i++)
        {
          distances[i] = (int)(Math.Sqrt(((float*)tmpData)[i]) * 256);
        }
      }

      return distances;
    }

    /// <summary>
    /// Generate the pixel distances (multiplied by 256)
    /// </summary>
    /// <param name="values">Values of the picture (255 = solid, 0 = empty, 128 = half filled etc.)</param>
    /// <param name="width">Width of the picture</param>
    /// <param name="height">Height of the picture</param>
    /// <returns>Calculated pixel distances</returns>
    public static unsafe int[] GenerateMap(byte[] values, int width, int height)
    {
      if (values == null) throw new NullReferenceException("values");
      if (values.Length < width * height) throw new ArgumentException();

      var distances = new int[values.Length];

      fixed (int* tmpData = distances)
      {
        for (int i = 0; i < distances.Length; i++)
        {
          ((float*)tmpData)[i] = values[i] > 0 ? (255 - values[i]) * (255 - values[i]) * (1f / (255f * 255f)) : INF;
        }

        TransformField((float*)tmpData, width, height);

        for (int i = 0; i < distances.Length; i++)
        {
          distances[i] = (int)(Math.Sqrt(((float*)tmpData)[i]) * 256);
        }
      }

      return distances;
    }

    /// <summary>
    /// very slow reference version of GenerateMap
    /// </summary>
    /// <param name="bits">Bits of the picture (True = solid, False = empty)</param>
    /// <param name="width">Width of the picture</param>
    /// <param name="height">Height of the picture</param>
    /// <returns>Calculated pixel distances</returns>
    public static int[] GenerateMapSlowReference(bool[] bits, int width, int height)
    {
      var distances = new int[bits.Length];
      for (int y = 0; y < height; y++)
      {
        for (int x = 0; x < width; x++)
        {
          int minDist = int.MaxValue;
          for (int cy = 0; cy < height; cy++)
          {
            int dy2 = (cy - y) * (cy - y);
            if (dy2 > minDist) continue;
            for (int cx = 0; cx < width; cx++)
            {
              if (bits[cx + cy * width])
              {
                int dx = cx - x;
                int dist = dx * dx + dy2;
                if (dist < minDist) minDist = dist;
              }
            }
          }
          distances[x + y * width] = (int)(Math.Sqrt(minDist) * 256);
        }
      }

      return distances;
    }
  }
}
