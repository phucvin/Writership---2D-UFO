using UnityEngine;
using Writership;

public class CompletePlayerAi : MonoBehaviour
{
    public El<bool> IsEnabled { get; private set; }
    public El<bool> IsGunFiring { get; private set; }

    private Op<Empty> toggle;

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        IsEnabled = G.Engine.El(false);
        IsGunFiring = G.Engine.El(false);
        toggle = G.Engine.Op<Empty>();
    }

    private void OnEnable()
    {
        G.Engine.Computer(cd, new object[] { toggle }, () =>
        {
            if (toggle.Count % 2 == 1) IsEnabled.Write(!IsEnabled);
        });

        G.Engine.Writer(cd, new object[] { IsEnabled, G.Tick }, () =>
        {
            if (!IsEnabled) IsGunFiring.Write(false);
            else if (G.Tick)
            {
                var hit = Physics2D.Raycast(transform.position, transform.right, 10, 1 << 8);
                IsGunFiring.Write(hit && hit.transform);
            }
        });
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
