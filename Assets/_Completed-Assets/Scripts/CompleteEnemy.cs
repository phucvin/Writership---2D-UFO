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
        G.Engine.Reader(cd, new object[] { G.Hit, G.Restart }, () =>
        {
            bool hit = false;
            for (int i = 0, n = G.Hit.Count; i < n; ++i)
            {
                if (G.Hit[i].ToEnemy == gameObject)
                {
                    hit = true;
                    break;
                }
            }
            if (hit || G.Restart) Destroy(gameObject);
        });
    }

    private void OnDisable()
    {
        cd.Dispose();
    }
}
