using UnityEngine;
using Writership;

public class CompleteRotator : MonoBehaviour
{
    private El<Quaternion> rotation;

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        W.Mark(transform, "rotation");

        rotation = G.Engine.El(transform.rotation);
    }

    private void OnEnable()
    {
        G.Engine.Computer(cd, new object[] { G.Tick, G.Restart }, () =>
        {
            var r = rotation.Read();
            if (G.Restart) r = Quaternion.identity;
            if (G.Tick)
            {
                r *= Quaternion.Euler(new Vector3(0, 0, 45) * G.Tick.Reduced);
            }
            // Can write directly to transform,
            // just need to to use register listener.
            // But use computer here to demonstrate how to delegate
            // heavy computation to compute thread
            rotation.Write(r);
        });

        G.Engine.Reader(cd, new object[] { rotation }, () =>
        {
            transform.rotation = rotation;
        });
    }

    private void OnDisable()
    {
        cd.Dispose();
    }
}
