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
            if (G.Restart) Destroy(gameObject);
            else
            {
                for (int i = 0, n = G.Hit.Count; i < n; ++i)
                {
                    if (G.Hit[i].ToEnemy == gameObject)
                    {
                        Destroy(gameObject);
                        break;
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
