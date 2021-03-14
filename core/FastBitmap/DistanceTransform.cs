/*
 *   Distance transform algorithm described in:
 *
 *   Distance Transforms of Sampled Functions
 *   Pedro F. Felzenszwalb and Daniel P. Huttenlocher
 *   Cornell Computing and Information Science TR2004-1963
 *
 */

using System;
// ReSharper disable UnusedType.Global

namespace YacGui
{
  public static class DistanceTransform
  {
    const float INF = 1e20f;

    static unsafe void TransformLine(float* f, int n)
    {
      float* d = &f[n];
      float* z = &d[n];
      int* v = (int*)&z[n + 1];

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

    static unsafe void TransformField(float* distanceValues, int width, int height)
    {
      float[] tmpBuffer = new float[Math.Max(width, height) * 4 + 1];
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
          float* imf = &distanceValues[y * width];
          for (int x = 0; x < width; x++) f[x] = imf[x];
          TransformLine(f, width);
          for (int x = 0; x < width; x++) imf[x] = f[width + x];
        }
      }
    }

    public static unsafe int[] GenerateMap(bool[] bits, int width, int height)
    {
      if (bits == null) throw new NullReferenceException("bits");
      if (bits.Length < width * height) throw new ArgumentException();

      int[] distances = new int[bits.Length];

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

    public static unsafe int[] GenerateMap(byte[] bits, int width, int height)
    {
      if (bits == null) throw new NullReferenceException("bits");
      if (bits.Length < width * height) throw new ArgumentException();

      int[] distances = new int[bits.Length];

      fixed (int* tmpData = distances)
      {
        for (int i = 0; i < distances.Length; i++)
        {
          ((float*)tmpData)[i] = bits[i] > 0 ? (255 - bits[i]) * (255 - bits[i]) * (1f / (255f * 255f)) : INF;
        }

        TransformField((float*)tmpData, width, height);

        for (int i = 0; i < distances.Length; i++)
        {
          distances[i] = (int)(Math.Sqrt(((float*)tmpData)[i]) * 256);
        }
      }

      return distances;
    }

    public static int[] GenerateMapSlowReference(bool[] bits, int width, int height)
    {
      int[] distances = new int[bits.Length];
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
