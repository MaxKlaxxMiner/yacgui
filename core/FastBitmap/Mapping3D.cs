using System;

namespace YacGui
{
  public class Mapping3D
  {
    /// <summary>
    /// a mapping-vertex with texture coordinates
    /// </summary>
    public struct Vertex
    {
      /// <summary>
      /// horizontal texture coordinate
      /// </summary>
      public double u;
      /// <summary>
      /// vertical texture coordinate
      /// </summary>
      public double v;

      /// <summary>
      /// x-coordinate
      /// </summary>
      public double x;
      /// <summary>
      /// y-coordinate
      /// </summary>
      public double y;
    }

    /// <summary>
    /// 3x3 Matrix
    /// </summary>
    public struct Matrix33
    {
      public double m00, m01, m02;
      public double m10, m11, m12;
      public double m20, m21, m22;

      /// <summary>
      /// matrix multiply: c = a * b
      /// </summary>
      /// <param name="a">first matrix</param>
      /// <param name="b">second matrix</param>
      /// <returns>output matrix</returns>
      public static Matrix33 Multiply(Matrix33 a, Matrix33 b)
      {
        return new Matrix33
        {
          m00 = a.m00 * b.m00 + a.m01 * b.m10 + a.m02 * b.m20,
          m01 = a.m00 * b.m01 + a.m01 * b.m11 + a.m02 * b.m21,
          m02 = a.m00 * b.m02 + a.m01 * b.m12 + a.m02 * b.m22,
          m10 = a.m10 * b.m00 + a.m11 * b.m10 + a.m12 * b.m20,
          m11 = a.m10 * b.m01 + a.m11 * b.m11 + a.m12 * b.m21,
          m12 = a.m10 * b.m02 + a.m11 * b.m12 + a.m12 * b.m22,
          m20 = a.m20 * b.m00 + a.m21 * b.m10 + a.m22 * b.m20,
          m21 = a.m20 * b.m01 + a.m21 * b.m11 + a.m22 * b.m21,
          m22 = a.m20 * b.m02 + a.m21 * b.m12 + a.m22 * b.m22
        };
      }

      /// <summary>
      /// b = adjoint(a), returns determinant(a)
      /// </summary>
      /// <param name="a">input</param>
      /// <param name="b">output</param>
      /// <returns>determinant(a)</returns>
      public static double AdJoint(Matrix33 a, out Matrix33 b)
      {
        b = new Matrix33
        {
          m00 = Det(a.m11, a.m12, a.m21, a.m22),
          m10 = Det(a.m12, a.m10, a.m22, a.m20),
          m20 = Det(a.m10, a.m11, a.m20, a.m21),
          m01 = Det(a.m21, a.m22, a.m01, a.m02),
          m11 = Det(a.m22, a.m20, a.m02, a.m00),
          m21 = Det(a.m20, a.m21, a.m00, a.m01),
          m02 = Det(a.m01, a.m02, a.m11, a.m12),
          m12 = Det(a.m02, a.m00, a.m12, a.m10),
          m22 = Det(a.m00, a.m01, a.m10, a.m11)
        };

        return a.m00 * b.m00 + a.m01 * b.m01 + a.m02 * b.m02;
      }
    }

    /// <summary>
    /// 4x2 Screen structure
    /// </summary>
    struct Screen42
    {
      public double x1, y1;
      public double x2, y2;
      public double x3, y3;
      public double x4, y4;
    }

    /// <summary>
    /// Mapping result-type
    /// </summary>
    [Flags]
    public enum MappingType
    {
      /// <summary>
      /// invalid mapping
      /// </summary>
      Invalid = -1,

      /// <summary>
      /// mapping is affine
      /// </summary>
      Affine = 0,

      /// <summary>
      /// mapping is projective
      /// </summary>
      Projective = 1
    }

    /// <summary>
    /// check if small enough for zero
    /// </summary>
    /// <param name="value">check value</param>
    /// <returns>true if zero (or near zero)</returns>
    static bool IsZero(double value)
    {
      return value < 1e-13 && value > -1e-13;
    }

    /// <summary>
    /// calculate determinante
    /// |a b|
    /// |c d|
    /// </summary>
    /// <param name="a">left top</param>
    /// <param name="b">right top</param>
    /// <param name="c">left bottom</param>
    /// <param name="d">right bottom</param>
    /// <returns>result</returns>
    static double Det(double a, double b, double c, double d)
    {
      return a * d - b * c;
    }

    /// <summary>
    /// find mapping between unit square and quadrilateral
    /// </summary>
    /// <param name="quad">vertices of quadrilateral</param>
    /// <param name="outputMatrix">square->quad transform</param>
    /// <returns>MappingType</returns>
    static MappingType pmap_square_quad(Screen42 quad, out Matrix33 outputMatrix)
    {
      double px = quad.x1 - quad.x2 + quad.x3 - quad.x4;
      double py = quad.y1 - quad.y2 + quad.y3 - quad.y4;

      if (IsZero(px) && IsZero(py)) // affine
      {
        outputMatrix.m00 = quad.x2 - quad.x1;
        outputMatrix.m10 = quad.x3 - quad.x2;
        outputMatrix.m20 = quad.x1;
        outputMatrix.m01 = quad.y2 - quad.y1;
        outputMatrix.m11 = quad.y3 - quad.y2;
        outputMatrix.m21 = quad.y1;
        outputMatrix.m02 = 0.0;
        outputMatrix.m12 = 0.0;
        outputMatrix.m22 = 1.0;

        return MappingType.Affine;
      }

      // projective
      double dx1 = quad.x2 - quad.x3;
      double dx2 = quad.x4 - quad.x3;
      double dy1 = quad.y2 - quad.y3;
      double dy2 = quad.y4 - quad.y3;
      double del = Det(dx1, dx2, dy1, dy2);
      if (del == 0.0)
      {
        outputMatrix = new Matrix33();
        return MappingType.Invalid;
      }

      outputMatrix.m02 = Det(px, dx2, py, dy2) / del;
      outputMatrix.m12 = Det(dx1, px, dy1, py) / del;
      outputMatrix.m22 = 1.0;
      outputMatrix.m00 = quad.x2 - quad.x1 + outputMatrix.m02 * quad.x2;
      outputMatrix.m10 = quad.x4 - quad.x1 + outputMatrix.m12 * quad.x4;
      outputMatrix.m20 = quad.x1;
      outputMatrix.m01 = quad.y2 - quad.y1 + outputMatrix.m02 * quad.y2;
      outputMatrix.m11 = quad.y4 - quad.y1 + outputMatrix.m12 * quad.y4;
      outputMatrix.m21 = quad.y1;

      return MappingType.Projective;
    }

    /// <summary>
    /// find mapping between quadrilateral and rectangle
    /// </summary>
    /// <param name="u0">bounds of rectangle (left u)</param>
    /// <param name="v0">bounds of rectangle (top v)</param>
    /// <param name="u1">bounds of rectangle (right u)</param>
    /// <param name="v1">bounds of rectangle (bottom v)</param>
    /// <param name="quad">vertices of quadrilateral</param>
    /// <param name="outputMatrix">output matrix (quad->rect transform)</param>
    /// <returns>MappingType</returns>
    static MappingType MapQuadRect(double u0, double v0, double u1, double v1, Screen42 quad, out Matrix33 outputMatrix)
    {
      Matrix33 transformMatrix;

      double du = u1 - u0;
      double dv = v1 - v0;

      if (du == 0.0 || dv == 0.0)
      {
        outputMatrix = new Matrix33();
        return MappingType.Invalid;
      }

      // first find mapping from unit uv square to xy quadrilateral
      var ret = pmap_square_quad(quad, out transformMatrix);
      if (ret == MappingType.Invalid)
      {
        outputMatrix = new Matrix33();
        return MappingType.Invalid;
      }

      // concatenate transform from uv rectangle (u0, v0, u1, v1) to unit square
      transformMatrix.m00 /= du;
      transformMatrix.m10 /= dv;
      transformMatrix.m20 -= transformMatrix.m00 * u0 + transformMatrix.m10 * v0;
      transformMatrix.m01 /= du;
      transformMatrix.m11 /= dv;
      transformMatrix.m21 -= transformMatrix.m01 * u0 + transformMatrix.m11 * v0;
      transformMatrix.m02 /= du;
      transformMatrix.m12 /= dv;
      transformMatrix.m22 -= transformMatrix.m02 * u0 + transformMatrix.m12 * v0;

      // now RQ is transform from uv rectangle to xy quadrilateral
      // QR = inverse transform, which maps xy to uv
      if (Matrix33.AdJoint(transformMatrix, out outputMatrix) == 0.0)
      {
        outputMatrix = new Matrix33();
        return MappingType.Invalid;
      }

      return ret;
    }

    /// <summary>
    /// find the projective mapping from screen to texture space
    /// given the screen and texture coordinates at the vertices of a quadrilateral.
    /// </summary>
    /// <param name="poly">vertices</param>
    /// <param name="outputMatrix">output matrix</param>
    /// <returns>output MappingType</returns>
    static MappingType MapQuadQuad(Vertex[] poly, out Matrix33 outputMatrix)
    {
      Screen42 quad;
      Matrix33 transformMatrix;
      Matrix33 screenMidMatrix, midTextureMatrix;

      quad.x1 = poly[0].x; quad.y1 = poly[0].y;
      quad.x2 = poly[1].x; quad.y2 = poly[1].y;
      quad.x3 = poly[2].x; quad.y3 = poly[2].y;
      quad.x4 = poly[3].x; quad.y4 = poly[3].y;

      var type1 = pmap_square_quad(quad, out transformMatrix);
      if (Matrix33.AdJoint(transformMatrix, out screenMidMatrix) == 0.0)
      {
        outputMatrix = new Matrix33();
        return MappingType.Invalid;
      }

      quad.x1 = poly[0].u; quad.y1 = poly[0].v;
      quad.x2 = poly[1].u; quad.y2 = poly[1].v;
      quad.x3 = poly[2].u; quad.y3 = poly[2].v;
      quad.x4 = poly[3].u; quad.y4 = poly[3].v;

      var type2 = pmap_square_quad(quad, out midTextureMatrix);

      if ((type1 | type2) == MappingType.Invalid)
      {
        outputMatrix = new Matrix33();
        return MappingType.Invalid;
      }

      outputMatrix = Matrix33.Multiply(screenMidMatrix, midTextureMatrix);

      return type1 | type2; // PMAP.PROJECTIVE prevails
    }

    public static MappingType MapPolygon(Vertex[] poly, out Matrix33 matrix33)
    {
      if (poly.Length != 4) throw new ArgumentException("only poly.Length == 4 supported");

      var scr = new Screen42(); // vertices of screen quadrilateral

      // if edges 0-1 and 2-3 are horz, 1-2 and 3-0 are vert
      if (poly[0].v == poly[1].v && poly[2].v == poly[3].v && poly[1].u == poly[2].u && poly[3].u == poly[0].u)
      {
        scr.x1 = poly[0].x; scr.y1 = poly[0].y;
        scr.x2 = poly[1].x; scr.y2 = poly[1].y;
        scr.x3 = poly[2].x; scr.y3 = poly[2].y;
        scr.x4 = poly[3].x; scr.y4 = poly[3].y;

        return MapQuadRect(poly[0].u, poly[0].v, poly[2].u, poly[2].v, scr, out matrix33);
      }

      // if edges 0-1 and 2-3 are vert, 1-2 and 3-0 are horz
      if (poly[0].u == poly[1].u && poly[2].u == poly[3].u && poly[1].v == poly[2].v && poly[3].v == poly[0].v)
      {
        scr.x1 = poly[1].x; scr.y1 = poly[1].y;
        scr.x2 = poly[2].x; scr.y2 = poly[2].y;
        scr.x3 = poly[3].x; scr.y3 = poly[3].y;
        scr.x4 = poly[0].x; scr.y4 = poly[0].y;

        return MapQuadRect(poly[1].u, poly[1].v, poly[3].u, poly[3].v, scr, out matrix33);
      }

      // if texture is not an othogonally-oriented rectangle
      return MapQuadQuad(poly, out matrix33);
    }
  }
}
