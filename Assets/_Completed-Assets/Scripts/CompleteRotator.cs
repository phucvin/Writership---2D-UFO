using UnityEngine;
using Writership;

public class CompleteRotator : MonoBehaviour
{
    private IEl<Quaternion> rotation;

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        W.Mark(transform, "rotation");

        rotation = G.Engine.El(transform.rotation);
    }

    private void OnEnable()
    {
        G.Engine.Computer(cd,
            new object[] { G.Tick, G.Restart },
            () =>
            {
                var r = rotation.Read();
                var t = G.Tick.Read();
                if (G.Restart.Read().Count > 0) r = Quaternion.identity;
                if (t.Count > 0)
                {
                    float dt = 0;
                    for (int i = 0, n = t.Count; i < n; ++i)
                    {
                        dt += t[i];
                    }
                    r *= Quaternion.Euler(new Vector3(0, 0, 45) * dt);
                }
                // Can write directly to transform,
                // just need to to use register listener.
                // But use computer here to demonstrate how to delegate
                // heavy computation to compute thread
                if (r != rotation.Read()) rotation.Write(r);
            }
        );

        G.Engine.Reader(cd,
            new object[] { rotation },
            () =>
            {
                transform.rotation = rotation.Read();
            }
        );
    }

    private void OnDisable()
    {
        cd.Dispose();
    }
}
