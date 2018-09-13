using System;
using Writership;

public static class RegisterExtensions
{
    public static void Compute<T>(this IEl<T> el,
        CompositeDisposable cd, IEngine engine,
        object[] targets, Func<T, T> f)
    {
        cd.Add(engine.RegisterComputer(targets, () =>
        {
            var v = f(el.Read());
            if (!Equals(v, el.Read())) el.Write(v);
        }));
    }

    public static void Listen<T>(this IEl<T> el,
        CompositeDisposable cd, IEngine engine,
        Action<T> a)
    {
        cd.Add(engine.RegisterListener(new object[] { el }, () =>
        {
            a(el.Read());
        }));
    }

    public static void Listen<T>(this object[] targets,
        CompositeDisposable cd, IEngine engine,
        Action a)
    {
        cd.Add(engine.RegisterListener(targets, () =>
        {
            a();
        }));
    }
}

public static class OpExtensions
{
    public static int Count<T>(this IOp<T> op)
    {
        return op.Read().Count;
    }

    public static bool Is<T>(this IOp<T> op)
    {
        return op.Count() > 0;
    }

    public static float Sum(this IOp<float> op_)
    {
        float f = 0;
        var op = op_.Read();
        for (int i = 0, n = op.Count; i < n; ++i) f += op[i];
        return f;
    }
}
