using UnityEngine;
using Writership;

public class CompletePickUp : MonoBehaviour
{
    [SerializeField]
    private GameObject effect = null;

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        W.Mark(gameObject, "active");

        G.AddItem.Fire(Empty.Instance);

        G.Engine.Reader(cd, new object[] { G.Restart, G.PickUp }, () =>
        {
            if (G.Restart) gameObject.SetActive(true);
            else
            {
                for (int i = 0, n = G.PickUp.Count; i < n; ++i)
                {
                    if (G.PickUp[i].Item == this)
                    {
                        gameObject.SetActive(false);
                        return;
                    }
                }
            }
        });
        G.Engine.Reader(cd, new object[] { G.PickUp }, () =>
        {
            for (int i = 0, n = G.PickUp.Count; i < n; ++i)
            {
                if (G.PickUp[i].Item == this)
                {
                    var e = Instantiate(effect, transform.position, transform.rotation);
                    W.Mark(e, "active");
                    e.SetActive(true);
                    return;
                }
            }
        });
    }

    private void OnDestroy()
    {
        cd.Dispose();
    }
}
