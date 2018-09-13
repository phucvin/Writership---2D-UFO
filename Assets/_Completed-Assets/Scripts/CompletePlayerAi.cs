using UnityEngine;
using Writership;

public class CompletePlayerAi : MonoBehaviour
{
    public IEl<bool> IsEnabled { get; private set; }
    public IEl<bool> IsGunFiring { get; private set; }

    private IOp<Empty> toggle;

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        IsEnabled = G.Engine.El(false);
        IsGunFiring = G.Engine.El(false);
        toggle = G.Engine.Op<Empty>();
    }

    private void OnEnable()
    {
        cd.Add(G.Engine.RegisterComputer(
            new object[] { toggle },
            () =>
            {
                if (toggle.Read().Count % 2 == 1)
                {
                    IsEnabled.Write(!IsEnabled.Read());
                }
            }
        ));
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            toggle.Fire(Empty.Instance);
        }
    }
}
