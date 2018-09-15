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
            // TODO Reduce boilerplate in unity objects
            bool active = gameObject.activeSelf;
            if (G.Restart) active = true;
            for (int i = 0, n = G.PickUp.Count; i < n; ++i)
            {
                if (G.PickUp[i].Item == this)
                {
                    active = false;
                    break;
                }
            }
            if (active != gameObject.activeSelf)
            {
                gameObject.SetActive(active);

                // TODO Maybe not correct, should use op or check if picked up, not anything else
                if (!active)
                {
                    var e = Instantiate(effect, transform.position, transform.rotation);
                    W.Mark(e, "active");
                    e.SetActive(true);
                }
            }
        });
    }

    private void OnDestroy()
    {
        cd.Dispose();
    }
}
