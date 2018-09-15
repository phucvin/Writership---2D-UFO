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

    private El<float> currentDelay;
    private El<int> currentEnemies;
    private Li<Spawning> spawning;
    private Wa spawningCurrentDelayWatcher;
    private Spawning.Factory spawningFactory;
    private Op<Vector2> spawn;
    private System.Random rand;

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        currentDelay = G.Engine.El(delay);
        currentEnemies = G.Engine.El(0);
        spawn = G.Engine.Op<Vector2>();
        spawning = G.Engine.Li(new List<Spawning>());
        spawningCurrentDelayWatcher = G.Engine.Wa(cd, spawning, it => it.CurrentDelay);
        spawningFactory = new Spawning.Factory();
        rand = new System.Random();

        spawningFactory.Setup(cd, G.Engine, G.Tick, spawn);
    }

    private void OnEnable()
    {
        G.Engine.Computer(cd, new object[] { currentEnemies, currentDelay, G.Tick, spawning, G.Restart }, () =>
        {
            if (G.Restart) currentDelay.Write(delay);
            else if (currentEnemies + spawning.Count < maxEnemies)
            {
                currentDelay.Write(Mathf.Max(0, (currentDelay <= 0 ? delay : currentDelay) - G.Tick.Reduced));
            }
            else currentDelay.Write(delay);
        });
        G.Engine.Computer(cd, new object[] { spawn, G.Hit, G.Restart }, () =>
        {
            if (G.Restart) currentEnemies.Write(0);
            else currentEnemies.Write(currentEnemies + spawn.Count - G.Hit.Count);
        });
        G.Engine.Computer(cd, new object[] { currentDelay, currentEnemies, spawningCurrentDelayWatcher, G.Restart }, () =>
        {
            var spawning = this.spawning.AsWriteProxy();
            if (currentDelay <= 0)
            {
                var at = new Vector2(
                    rand.Next((int)(-area.x / 2), (int)(area.x / 2)),
                    rand.Next((int)(-area.y / 2), (int)(area.y / 2))
                );
                spawning.Add(spawningFactory.Create(at, indicating));
            }
            spawning.RemoveAll(it =>
            {
                if (it.CurrentDelay.Read() <= 0 || G.Restart)
                {
                    spawningFactory.Dispose(it);
                    return true;
                }
                else return false;
            });
            spawning.Commit();
        });

        G.Engine.Reader(cd, new object[] { spawn }, () =>
        {
            for (int i = 0, n = spawn.Count; i < n; ++i)
            {
                var e = Instantiate(what, transform.position + (Vector3)spawn[i], transform.rotation);
                W.Mark(e, "active");
                e.SetActive(true);
            }
        });
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
            G.Engine.Reader(cd, new object[] { spawning }, () =>
            {
                destroyCreated();

                for (int i = 0, n = spawning.Count; i < n; ++i)
                {
                    var o = Instantiate(indicator, transform.position + (Vector3)spawning[i].At, transform.rotation);
                    W.Mark(o, "destroy");
                    W.Mark(o, "active");
                    o.SetActive(true);
                    created.Add(o);
                }
            });
        }
    }

    private void OnDisable()
    {
        cd.Dispose();
    }

    private class Spawning
    {
        public readonly Vector2 At;
        public readonly El<float> CurrentDelay;

        public Spawning(IEngine engine, Vector2 at, float delay)
        {
            At = at;
            CurrentDelay = engine.El(delay);
        }

        public void Setup(CompositeDisposable cd, IEngine engine,
            Op<float> tick, Op<Vector2> spawn)
        {
            engine.Computer(cd, new object[] { tick }, () =>
            {
                CurrentDelay.Write(CurrentDelay - tick.Reduced);
            });
            engine.Computer(cd, new object[] { CurrentDelay }, () =>
            {
                if (CurrentDelay <= 0) spawn.Fire(At);
            });
        }

        public class Factory : CompositeDisposableFactory<Spawning>
        {
            private IEngine engine;
            private Op<float> tick;
            private Op<Vector2> spawn;

            public void Setup(CompositeDisposable cd, IEngine engine,
                Op<float> tick, Op<Vector2> spawn)
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