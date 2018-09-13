using System.Collections.Generic;
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
    [SerializeField]
    private GameObject indicator = null;
    [SerializeField]
    private float indicating = 1f;

    private IEl<float> currentDelay;
    private IEl<int> currentEnemies;
    private IOp<Vector2> preSpawn;
    private ILi<Spawning> spawning;
    private Spawning.Factory spawningFactory;
    private IOp<Vector2> spawn;
    private System.Random rand;

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        currentDelay = G.Engine.El(delay);
        currentEnemies = G.Engine.El(0);
        spawn = G.Engine.Op<Vector2>();
        spawning = G.Engine.Li(new List<Spawning>());
        preSpawn = G.Engine.Op<Vector2>();
        spawningFactory = new Spawning.Factory();
        rand = new System.Random();

        spawningFactory.Setup(cd, G.Engine, G.Tick, spawn);
    }

    private void OnEnable()
    {
        cd.Add(G.Engine.RegisterComputer(
            new object[] { currentEnemies, G.Tick, spawn, G.Restart },
            () =>
            {
                var t = G.Tick.Read();
                var d = currentDelay.Read();
                if (G.Restart.Read().Count > 0)
                {
                    d = delay;
                }
                else if (currentEnemies.Read() < maxEnemies)
                {
                    if (spawn.Read().Count > 0) d = delay;
                    for (int i = 0, n = t.Count; i < n; ++i) d -= t[i];
                    d = Mathf.Max(0, d);
                }
                if (d != currentDelay.Read()) currentDelay.Write(d);
            }
        ));
        cd.Add(G.Engine.RegisterComputer(
            new object[] { spawn, G.Hit, G.Restart },
            () =>
            {
                int e = currentEnemies.Read();
                if (G.Restart.Read().Count > 0) e = 0;
                else e = e + spawn.Read().Count - G.Hit.Read().Count;
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
                    preSpawn.Fire(at);
                }
            }
        ));
        cd.Add(G.Engine.RegisterComputer(
            new object[] { preSpawn, G.Tick.Applied, G.Restart },
            () =>
            {
                var p = preSpawn.Read();
                var t = G.Tick.Applied.Read();
                var r = G.Restart.Read().Count > 0;
                if (p.Count <= 0 && t.Count <= 0 && !r) return;

                var s = spawning.AsWrite();
                for (int i = 0, n = p.Count; i < n; ++i)
                {
                    s.Add(spawningFactory.Create(p[i], indicating));
                }
                if (t.Count > 0 || r)
                {
                    s.RemoveAll(it =>
                    {
                        if (it.CurrentDelay.Read() <= 0 || r)
                        {
                            spawningFactory.Dispose(it);
                            return true;
                        }
                        else return false;
                    });
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
                    var e = Instantiate(what, transform.position + (Vector3)s[i], transform.rotation);
                    W.Mark(e, "active");
                    e.SetActive(true);
                }
            }
        ));
        {
            var created = new List<GameObject>();
            System.Action destroyCreated = () =>
            {
                for (int i = 0, n = created.Count; i < n; ++i)
                {
                    Destroy(created[i]);
                }
                created.Clear();
            };
            cd.Add(new DisposableAction(destroyCreated));
            cd.Add(G.Engine.RegisterListener(
                new object[] { spawning },
                () =>
                {
                    destroyCreated();

                    var s = spawning.Read();
                    for (int i = 0, n = s.Count; i < n; ++i)
                    {
                        var o = Instantiate(indicator, transform.position + (Vector3)s[i].At, transform.rotation);
                        W.Mark(o, "destroy");
                        W.Mark(o, "active");
                        o.SetActive(true);
                        created.Add(o);
                    }
                }
            ));
        }
    }

    private void OnDisable()
    {
        cd.Dispose();
    }

    private class Spawning
    {
        public readonly Vector2 At;
        public readonly IEl<float> CurrentDelay;

        public Spawning(IEngine engine, Vector2 at, float delay)
        {
            At = at;
            CurrentDelay = engine.El(delay);
        }

        public void Setup(CompositeDisposable cd, IEngine engine,
            IOp<float> tick, IOp<Vector2> spawn)
        {
            cd.Add(engine.RegisterComputer(
                new object[] { tick },
                () =>
                {
                    float d = CurrentDelay.Read();
                    var t = tick.Read();
                    for (int i = 0, n = t.Count; i < n; ++i) d -= t[i];
                    if (d != CurrentDelay.Read()) CurrentDelay.Write(d);
                }
            ));
            cd.Add(engine.RegisterComputer(
                new object[] { CurrentDelay },
                () =>
                {
                    if (CurrentDelay.Read() <= 0)
                    {
                        spawn.Fire(At);
                    }
                }
            ));
        }

        public class Factory : CompositeDisposableFactory<Spawning>
        {
            private IEngine engine;
            private IOp<float> tick;
            private IOp<Vector2> spawn;

            public void Setup(CompositeDisposable cd, IEngine engine,
                IOp<float> tick, IOp<Vector2> spawn)
            {
                this.engine = engine;
                this.tick = tick;
                this.spawn = spawn;

                cd.Add(this);
            }

            public Spawning Create(Vector2 at, float delay)
            {
                var s = new Spawning(engine, at, delay);
                var cd = Add(s);
                s.Setup(cd, engine, tick, spawn);
                return s;
            }

            public void Dispose(Spawning s)
            {
                Remove(s).Dispose();
            }
        }
    }
}