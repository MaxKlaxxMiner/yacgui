/*
 *   Easing functions ported from JQuery 
 *
 */

#region # using *.*
using System;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
#endregion

namespace YacGui
{
  /// <summary>
  /// Easing functions
  /// </summary>
  public static class Easing
  {
    /// <summary>
    /// Capping the Value between 0 and 1
    /// </summary>
    /// <param name="t">Input value</param>
    /// <returns>Output value (0.0-1.0)</returns>
    static double Cap(double t) { return Math.Max(0, Math.Min(1, t)); }

    /// <summary>
    /// no easing, no acceleration
    /// </summary>
    /// <param name="t">Input value (0.0-1.0)</param>
    /// <returns>Output value (0.0-1.0)</returns>
    public static double Linear(double t) { t = Cap(t); return t; }

    /// <summary>
    /// accelerating from zero velocity
    /// </summary>
    /// <param name="t">Input value (0.0-1.0)</param>
    /// <returns>Output value (0.0-1.0)</returns>
    public static double InQuad(double t) { t = Cap(t); return t * t; }

    /// <summary>
    /// decelerating to zero velocity
    /// </summary>
    /// <param name="t">Input value (0.0-1.0)</param>
    /// <returns>Output value (0.0-1.0)</returns>
    public static double OutQuad(double t) { t = Cap(t); return t * (2.0 - t); }

    /// <summary>
    /// acceleration until halfway, then deceleration
    /// </summary>
    /// <param name="t">Input value (0.0-1.0)</param>
    /// <returns>Output value (0.0-1.0)</returns>
    public static double InOutQuad(double t) { t = Cap(t); return t < 0.5 ? 2.0 * t * t : -1.0 + (4.0 - 2.0 * t) * t; }

    /// <summary>
    /// accelerating from zero velocity
    /// </summary>
    /// <param name="t">Input value (0.0-1.0)</param>
    /// <returns>Output value (0.0-1.0)</returns>
    public static double InCubic(double t) { t = Cap(t); return t * t * t; }

    /// <summary>
    /// decelerating to zero velocity
    /// </summary>
    /// <param name="t">Input value (0.0-1.0)</param>
    /// <returns>Output value (0.0-1.0)</returns>
    public static double OutCubic(double t) { t = Cap(t); return --t * t * t + 1.0; }

    /// <summary>
    /// acceleration until halfway, then deceleration
    /// </summary>
    /// <param name="t">Input value (0.0-1.0)</param>
    /// <returns>Output value (0.0-1.0)</returns>
    public static double InOutCubic(double t) { t = Cap(t); return t < 0.5 ? 4.0 * t * t * t : (t - 1.0) * (2.0 * t - 2.0) * (2.0 * t - 2.0) + 1.0; }

    /// <summary>
    /// accelerating from zero velocity
    /// </summary>
    /// <param name="t">Input value (0.0-1.0)</param>
    /// <returns>Output value (0.0-1.0)</returns>
    public static double InQuart(double t) { t = Cap(t); return t * t * t * t; }

    /// <summary>
    /// decelerating to zero velocity
    /// </summary>
    /// <param name="t">Input value (0.0-1.0)</param>
    /// <returns>Output value (0.0-1.0)</returns>
    public static double OutQuart(double t) { t = Cap(t); return 1.0 - --t * t * t * t; }

    /// <summary>
    /// acceleration until halfway, then deceleration
    /// </summary>
    /// <param name="t">Input value (0.0-1.0)</param>
    /// <returns>Output value (0.0-1.0)</returns>
    public static double InOutQuart(double t) { t = Cap(t); return t < 0.5 ? 8.0 * t * t * t * t : 1.0 - 8.0 * --t * t * t * t; }

    /// <summary>
    /// accelerating from zero velocity
    /// </summary>
    /// <param name="t">Input value (0.0-1.0)</param>
    /// <returns>Output value (0.0-1.0)</returns>
    public static double InQuint(double t) { t = Cap(t); return t * t * t * t * t; }

    /// <summary>
    /// decelerating to zero velocity
    /// </summary>
    /// <param name="t">Input value (0.0-1.0)</param>
    /// <returns>Output value (0.0-1.0)</returns>
    public static double OutQuint(double t) { t = Cap(t); return 1.0 + --t * t * t * t * t; }

    /// <summary>
    /// accelerating from zero velocity
    /// </summary>
    /// <param name="t">Input value (0.0-1.0)</param>
    /// <returns>Output value (0.0-1.0)</returns>
    public static double InSine(double t) { t = Cap(t); return 1.0 - Math.Cos(t * Math.PI / 2.0); }

    /// <summary>
    /// decelerating to zero velocity
    /// </summary>
    /// <param name="t">Input value (0.0-1.0)</param>
    /// <returns>Output value (0.0-1.0)</returns>
    public static double OutSine(double t) { t = Cap(t); return Math.Sin(t * Math.PI / 2.0); }

    /// <summary>
    /// JQuery default: acceleration until halfway, then deceleration
    /// </summary>
    /// <param name="t">Input value (0.0-1.0)</param>
    /// <returns>Output value (0.0-1.0)</returns>
    public static double InOutSine(double t) { t = Cap(t); return -(Math.Cos(Math.PI * t) - 1.0) / 2.0; }
  }
}
