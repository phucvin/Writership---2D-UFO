using UnityEngine;
using Writership;

public class CompleteEffect : MonoBehaviour
{
    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        W.Mark(gameObject, "destroy");
    }

    private void OnEnable()
    {
        G.Engine.Reader(cd,
            new object[] { G.Restart, G.RequestDestroy },
            () =>
            {
                var r = G.RequestDestroy.Read();
                bool isRequested = false;
                for (int i = 0, n = r.Count; i < n; ++i)
                {
                    if (r[i] == gameObject)
                    {
                        isRequested = true;
                        break;
                    }
                }
                if (isRequested || G.Restart.Read().Count > 0)
                {
                    Destroy(gameObject);
                }
            }
        );
    }

    private void OnDisable()
    {
        cd.Dispose();
    }
}
