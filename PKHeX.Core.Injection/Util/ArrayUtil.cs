using System;
using System.Collections.Generic;

namespace PKHeX.Core.Injection;

/// <summary>
/// Array reusable logic
/// </summary>
public static class ArrayUtil
{
    public static bool Contains<T>(this ReadOnlySpan<T> arr, T value) where T : Enum
    {
        foreach (var v in arr)
        {
            if (v.Equals(value))
                return true;
        }
        return false;
    }

    internal static T[] ConcatAll<T>(params T[][] arr)
    {
        int len = 0;
        foreach (var a in arr)
            len += a.Length;

        var result = new T[len];

        int ctr = 0;
        foreach (var a in arr)
        {
            a.CopyTo(result, ctr);
            ctr += a.Length;
        }

        return result;
    }

    public static T[][] Split<T>(this ReadOnlySpan<T> data, int size)
    {
        var result = new T[data.Length / size][];
        for (int i = 0; i < data.Length; i += size)
            result[i / size] = data.Slice(i, size).ToArray();

        return result;
    }

    internal static T[] ConcatAll<T>(T[] arr1, T[] arr2)
    {
        int len = arr1.Length + arr2.Length;
        var result = new T[len];
        arr1.CopyTo(result, 0);
        arr2.CopyTo(result, arr1.Length);
        return result;
    }

    public static IEnumerable<T[]> EnumerateSplit<T>(T[] bin, int size, int start = 0)
    {
        for (int i = start; i < bin.Length; i += size)
            yield return bin.AsSpan(i, size).ToArray();
    }
}