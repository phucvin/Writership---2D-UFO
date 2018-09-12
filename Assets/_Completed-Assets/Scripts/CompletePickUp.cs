using UnityEngine;
using Writership;

public class CompletePickUp : MonoBehaviour
{
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
                }
            }
        ));
    }

    private void OnDestroy()
    {
        cd.Dispose();
    }
}
