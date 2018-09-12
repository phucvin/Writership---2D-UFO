using UnityEngine;
using Writership;

public class CompleteEnemySpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject what = null;
    [SerializeField]
    private Vector2 area = new Vector2(10, 10);
    [SerializeField]
    private float delay = 1f;
    [SerializeField]
    private int maxEnemies = 2;

    private IEl<float> currentDelay;
    private IEl<int> currentEnemies;
    private IOp<Vector2> spawn;
    private System.Random rand;

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        currentDelay = G.Engine.El(delay);
        currentEnemies = G.Engine.El(0);
        spawn = G.Engine.Op<Vector2>();
        rand = new System.Random();
    }

    private void OnEnable()
    {
        cd.Add(G.Engine.RegisterComputer(
            new object[] { currentEnemies, G.Tick, spawn },
            () =>
            {
                if (currentEnemies.Read() >= maxEnemies) return;
                var t = G.Tick.Read();
                var d = currentDelay.Read();
                if (spawn.Read().Count > 0) d = delay;
                for (int i = 0, n = t.Count; i < n; ++i) d -= t[i];
                d = Mathf.Max(0, d);
                if (d != currentDelay.Read()) currentDelay.Write(d);
            }
        ));
        cd.Add(G.Engine.RegisterComputer(
            new object[] { spawn, G.Hit },
            () =>
            {
                int e = currentEnemies.Read();
                e = e + spawn.Read().Count - G.Hit.Read().Count;
                if (e != currentEnemies.Read()) currentEnemies.Write(e);
            }
        ));
        cd.Add(G.Engine.RegisterComputer(
            new object[] { currentDelay, currentEnemies },
            () =>
            {
                if (currentDelay.Read() <= 0 && currentEnemies.Read() < maxEnemies)
                {
                    var at = new Vector2(
                        rand.Next((int)(-area.x / 2), (int)(area.x / 2)),
                        rand.Next((int)(-area.y / 2), (int)(area.y / 2))
                    );
                    spawn.Fire(at);
                }
            }
        ));

        cd.Add(G.Engine.RegisterListener(
            new object[] { spawn },
            () =>
            {
                var s = spawn.Read();
                for (int i = 0, n = s.Count; i < n; ++i)
                {
                    Vector3 at = s[i];
                    var e = Instantiate(what, transform.position + at, transform.rotation);
                    W.Mark(e, "active");
                    e.SetActive(true);
                }
            }
        ));
    }

    private void OnDisable()
    {
        cd.Dispose();
    }
}