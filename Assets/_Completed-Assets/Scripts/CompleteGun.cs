using UnityEngine;
using Writership;

public class CompleteGun : MonoBehaviour
{
    [SerializeField]
    private GameObject bullet = null;

    [SerializeField]
    private float delay = 0.2f;

    [SerializeField]
    private CompletePlayerAi ai = null;

    private IEl<bool> isFiring;
    private IEl<bool> isManualFiring;
    private IEl<float> currentDelay;
    private IOp<Empty> fire;

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        isFiring = G.Engine.El(false);
        isManualFiring = G.Engine.El(false);
        currentDelay = G.Engine.El(0f);
        fire = G.Engine.Op<Empty>();
    }

    private void OnEnable()
    {
        cd.Add(G.Engine.RegisterComputer(
            new object[] { isFiring, currentDelay, G.Tick },
            () =>
            {
                bool f = isFiring.Read();
                float d = currentDelay.Read();
                var t = G.Tick.Read();
                float ticks = 0;
                for (int i = 0, n = t.Count; i < n; ++i)
                {
                    ticks += t[i];
                }

                if (f)
                {
                    d -= ticks;
                    while (d < 0)
                    {
                        d += delay;
                        fire.Fire(Empty.Instance);
                    }
                }
                else
                {
                    d = Mathf.Max(0, d - ticks);
                }

                if (d != currentDelay.Read()) currentDelay.Write(d);
            }
        ));
        cd.Add(G.Engine.RegisterComputer(
            new object[] { isManualFiring, ai.IsGunFiring },
            () =>
            {
                bool f = isFiring.Read();
                f = isManualFiring.Read() || ai.IsGunFiring.Read();
                if (f != isFiring.Read()) isFiring.Write(f);
            }
        ));

        cd.Add(G.Engine.RegisterListener(
            new object[] { fire },
            () =>
            {
                var f = fire.Read();
                for (int i = 0, n = f.Count; i < n; ++i)
                {
                    var b = Instantiate(bullet, transform.position, transform.rotation);
                    W.Mark(b, "active");
                    b.SetActive(true);
                }
            }
        ));
    }

    private void OnDisable()
    {
        cd.Dispose();
    }

    private void Update()
    {
        bool f = Input.GetButton("Fire1");
        if (f != isManualFiring.Read()) isManualFiring.Write(f);
    }
}
