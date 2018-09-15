using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class W/*itership*/
{
    private class Key : IEquatable<Key>
    {
        public object Target;
        public string Child;

        public override bool Equals(object obj)
        {
            return Equals((Key)obj);
        }

        public override int GetHashCode()
        {
            return Target.GetHashCode();
        }

        public bool Equals(Key other)
        {
            if (!ReferenceEquals(other.Target, null))
            {
                return Target == other.Target && (
                    string.IsNullOrEmpty(Child) ||
                    string.IsNullOrEmpty(other.Child) ||
                    Child == other.Child
                );
            }
            else return false;
        }
    }

    private static readonly Dictionary<Key, StackTrace> dict = new Dictionary<Key, StackTrace>();

    public static void Mark(object target, string child = null)
    {
        if (ReferenceEquals(target, null)) throw new InvalidOperationException("target");

        Key key = new Key { Target = target, Child = child };
        StackTrace value;
        dict.TryGetValue(key, out value);

        var now = new StackTrace(1, true);

        if (value == null)
        {
            value = now;
            dict.Add(key, value);
        }
        else if (!IsSame(now, value))
        {
            UnityEngine.Debug.LogWarning("Last mark: \n" + value.ToString());
            UnityEngine.Debug.LogWarning("Now mark: \n" + now.ToString());
            throw new InvalidOperationException("Cannot mark to same at different places");
        }
    }

    public static void Cull()
    {
        var deadKeys = new List<Key>();
        foreach (var key in dict.Keys)
        {
            var uo = key.Target as UnityEngine.Object;
            if (!ReferenceEquals(uo, null) && !uo) deadKeys.Add(key);
        }

        for (int i = 0, n = deadKeys.Count; i < n; ++i)
        {
            dict.Remove(deadKeys[i]);
        }
    }

    private static bool IsSame(StackTrace a, StackTrace b)
    {
        if (a.FrameCount != b.FrameCount) return false;
        for (int i = 0, n = a.FrameCount; i < n; ++i)
        {
            var af = a.GetFrame(i);
            var bf = b.GetFrame(i);
            if (af.GetMethod() != bf.GetMethod())
            {
                return false;
            }
        }
        return true;
    }
}
