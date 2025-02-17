﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static float ModLoop(this float lhs, float rhs)
    {
        return ((lhs % rhs) + rhs) % rhs;
    }

    public static Vector3 Divide(this Vector3 a, Vector3 b)
    {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }

    public static Vector3 Multiply(this Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    /// <summary>
    /// The length of the component of vector a that is in the direction of b.
    /// </summary>
    /// <param name="a">Original vector</param>
    /// <param name="b">Your directional vector. Length is irrelevent.</param>
    /// <returns>The length of the component of vector b that is in the direction of a.</returns>
    public static float ScalarProjection(this Vector3 a, Vector3 b)
    {
        float topOfFraction = Vector3.Dot(a, b);
        float bottomOfFraction = Mathf.Pow(b.magnitude, 2);

        return topOfFraction / bottomOfFraction;
    }

    /// <summary>
    /// The component of vector a that is in the direction of b.
    /// </summary>
    /// <param name="a">Original vector</param>
    /// <param name="b">Your directional vector. Length is irrelevent. Returned vector will be in this direction.</param>
    /// <returns>The component of vector b that is in the direction of a.</returns>
    public static Vector3 VectorProjection(this Vector3 a, Vector3 b)
    {
        float scalarProjection = a.ScalarProjection(b);

        return scalarProjection * b;
    }

    public static Vector3 AbsoluteVector(this Vector3 vectorToAbsolute)
    {
        return new Vector3(Mathf.Abs(vectorToAbsolute.x), Mathf.Abs(vectorToAbsolute.y), Mathf.Abs(vectorToAbsolute.z));
    }

    /// <summary>
    /// The length of the component of vector a that is in the direction of b.
    /// </summary>
    /// <param name="a">Original vector</param>
    /// <param name="b">Your directional vector. Length is irrelevent.</param>
    /// <returns>The length of the component of vector b that is in the direction of a.</returns>
    public static float ScalarProjection(this Vector2 a, Vector2 b)
    {
        float topOfFraction = Vector2.Dot(a, b);
        float bottomOfFraction = Mathf.Pow(b.magnitude, 2);

        return topOfFraction / bottomOfFraction;
    }

    /// <summary>
    /// The component of vector a that is in the direction of b.
    /// </summary>
    /// <param name="a">Original vector</param>
    /// <param name="b">Your directional vector. Length is irrelevent. Returned vector will be in this direction.</param>
    /// <returns>The component of vector b that is in the direction of a.</returns>
    public static Vector2 VectorProjection(this Vector2 a, Vector2 b)
    {
        float scalarProjection = a.ScalarProjection(b);

        return scalarProjection * b;
    }

    public static Vector2 AbsoluteVector(this Vector2 vectorToAbsolute)
    {
        return new Vector2(Mathf.Abs(vectorToAbsolute.x), Mathf.Abs(vectorToAbsolute.y));
    }

    /// <summary>
    /// Returns a random vector3 with components 0 to range in each component.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public static Vector3 RandomVector3(this float range)
    {
        return new Vector3(Random.Range(0, range), Random.Range(0, range), Random.Range(0, range));
    }

    /// <summary>
    /// Returns a random vector3 with components 0 to range in each component.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public static Vector2 RandomVector2(this float range)
    {
        return new Vector2(Random.Range(0, range), Random.Range(0, range));
    }

    public static int Layermask_to_layer(this LayerMask layerMask)
    {
        int layerNumber = 0;
        int layer = layerMask.value;
        while (layer > 0)
        {
            layer = layer >> 1;
            layerNumber++;
        }
        return layerNumber - 1;
    }

    public static BulletSharp.Math.Vector3 normalized(this BulletSharp.Math.Vector3 v)
    {
        if (v.Length < BulletSharp.MathUtil.SIMD_EPSILON)
        {
            return BulletSharp.Math.Vector3.Zero;
        }

        return BulletSharp.Math.Vector3.Normalize(v);
    }

    #region Logs
    public static void Log(this GameObject gameObject, string message)
    {
        Debug.Log(message, gameObject);
    }

    public static void Log(this MonoBehaviour component, string message)
    {
        Debug.Log(message, component);
    }

    public static string Bold(this string text)
    {
        return "<b>"+text+"</b>";
    }

    public static string Italics(this string text)
    {
        return "<i>" + text + "</i>";
    }

    public static string Size(this string text, int size)
    {
        return "<size="+size+">" + text + "</size>";
    }

    public static string Color(this string text, Color color)
    {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + text + "</color>";
    }
    #endregion
}
