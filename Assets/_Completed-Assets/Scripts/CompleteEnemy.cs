using UnityEngine;
using Writership;

public class CompleteEnemy : MonoBehaviour
{
    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        W.Mark(gameObject, "destroy");
    }

    private void OnEnable()
    {
        cd.Add(G.Engine.RegisterListener(
            new object[] { G.Hit, G.Restart },
            () =>
            {
                var h = G.Hit.Read();
                var r = G.Restart.Read().Count > 0;
                bool d = false;
                for (int i = 0, n = h.Count; i < n; ++i)
                {
                    if (h[i].ToEnemy == gameObject)
                    {
                        d = true;
                        break;
                    }
                }
                if (d || r) Destroy(gameObject);
            }
        ));
    }

    private void OnDisable()
    {
        cd.Dispose();
    }
}
