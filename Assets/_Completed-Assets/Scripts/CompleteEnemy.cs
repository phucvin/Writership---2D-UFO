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
            new object[] { G.Hit },
            () =>
            {
                var h = G.Hit.Read();
                for (int i = 0, n = h.Count; i < n; ++i)
                {
                    if (h[i].ToEnemy == gameObject)
                    {
                        Destroy(gameObject);
                        break;
                    }
                }
            }
        ));
    }

    private void OnDisable()
    {
        cd.Dispose();
    }
}
