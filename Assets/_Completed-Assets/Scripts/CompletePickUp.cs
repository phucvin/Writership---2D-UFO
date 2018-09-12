using UnityEngine;
using Writership;

public class CompletePickUp : MonoBehaviour
{
    [SerializeField]
    private GameObject effect = null;

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        G.AddItem.Fire(Empty.Instance);

        cd.Add(G.Engine.RegisterListener(
            new object[] { G.Restart, G.PickUp },
            () =>
            {
                var pickUp = G.PickUp.Read();
                bool active = gameObject.activeSelf;
                if (G.Restart.Read().Count > 0) active = true;
                for (int i = 0, n = pickUp.Count; i < n; ++i)
                {
                    if (pickUp[i].Item == this)
                    {
                        active = false;
                        break;
                    }
                }
                if (active != gameObject.activeSelf)
                {
                    W.Mark(gameObject, "active");
                    gameObject.SetActive(active);

                    if (!active)
                    {
                        var e = Instantiate(effect, transform.position, transform.rotation);
                        W.Mark(e);
                        e.SetActive(true);
                        Destroy(e, 1f);
                    }
                }
            }
        ));
    }

    private void OnDestroy()
    {
        cd.Dispose();
    }
}
