using System;
using System.Collections.Generic;
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

    private struct Value
    {
        // TODO Save whole stack trace ?
        public string FileName;
        public int LineNumber;
    }

    private static readonly Dictionary<Key, Value> dict = new Dictionary<Key, Value>();

    public static void Mark(object target, string child = null)
    {
        if (ReferenceEquals(target, null)) throw new InvalidOperationException("target");

        Key key = new Key { Target = target, Child = child };
        Value value;
        dict.TryGetValue(key, out value);

        var frame = new System.Diagnostics.StackFrame(1, true);
        string nowFileName = frame.GetFileName();
        int nowLineNumber = frame.GetFileLineNumber();

        if (string.IsNullOrEmpty(nowFileName) || nowLineNumber <= 0)
        {
            throw new InvalidOperationException("Cannot get stack frame");
        }

        if (value.FileName == null)
        {
            value.FileName = nowFileName;
            value.LineNumber = nowLineNumber;
            dict.Add(key, value);
        }
        else if (value.FileName != nowFileName || value.LineNumber != nowLineNumber)
        {
            Debug.LogWarningFormat("W.Mark different at: {0}:{1} != {2}:{3}",
                System.IO.Path.GetFileName(nowFileName), nowLineNumber,
                System.IO.Path.GetFileName(value.FileName), value.LineNumber);
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
}
