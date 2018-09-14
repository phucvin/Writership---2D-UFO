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

        G.Engine.Reader(cd,
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
                    gameObject.SetActive(active);

                    if (!active)
                    {
                        var e = Instantiate(effect, transform.position, transform.rotation);
                        W.Mark(e, "active");
                        e.SetActive(true);
                    }
                }
            }
        );
    }

    private void OnDestroy()
    {
        cd.Dispose();
    }
}
