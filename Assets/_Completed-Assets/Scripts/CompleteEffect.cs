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
        G.Engine.Reader(cd, new object[] { G.Restart, G.RequestDestroy }, () =>
        {
            if (G.Restart) Destroy(gameObject);
            else
            {
                for (int i = 0, n = G.RequestDestroy.Count; i < n; ++i)
                {
                    if (G.RequestDestroy[i] == gameObject)
                    {
                        Destroy(gameObject);
                        return;
                    }
                }
            }
        });
    }

    private void OnDisable()
    {
        cd.Dispose();
    }
}
