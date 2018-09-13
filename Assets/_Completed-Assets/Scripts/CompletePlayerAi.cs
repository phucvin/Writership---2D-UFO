using UnityEngine;
using Writership;

public class CompletePlayerAi : MonoBehaviour
{
    public IEl<bool> IsEnabled { get; private set; }
    public IEl<bool> IsGunFiring { get; private set; }

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        IsEnabled = G.Engine.El(enabled);
        IsGunFiring = G.Engine.El(false);
    }

    private void OnEnable()
    {
        cd.Add(G.Engine.RegisterListener(
            new object[] { IsEnabled, G.Tick },
            () =>
            {
                bool g = IsGunFiring.Read();
                if (!IsEnabled.Read()) g = false;
                else if (G.Tick.Read().Count > 0)
                {
                    var hit = Physics2D.Raycast(transform.position, transform.right, 10, 1 << 8);
                    g = hit && hit.transform;
                }
                if (g != IsGunFiring.Read()) IsGunFiring.Write(g);
            }
        ));
    }

    private void OnDisable()
    {
        cd.Dispose();
    }
}
