using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Writership;

public class CompletePlayerController : MonoBehaviour
{
    [SerializeField]
    private float speed = 10;
    [SerializeField]
    private Text countText = null;
    [SerializeField]
    private Text winText = null;

    private Rigidbody2D rb2d;
    private Vector3 orgPos;

    public IEl<Vector3> Position { get; private set; }
    public IEl<int> Score { get; private set; }

    private IEl<Vector2> movement;

    private readonly CompositeDisposable cd = new CompositeDisposable();

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        orgPos = transform.position;

        W.Mark(rb2d);
        W.Mark(transform, "position");
        W.Mark(countText, "text");
        W.Mark(winText, "text");

        Position = G.Engine.El(transform.position);
        Score = G.Engine.El(0);
        movement = G.Engine.El(Vector2.zero);
    }

    private void OnEnable()
    {
        cd.Add(G.Engine.RegisterComputer(
            new object[] { G.PickUp, G.Restart },
            () =>
            {
                int score = Score.Read();
                if (G.Restart.Read().Count > 0) score = 0;
                score += G.PickUp.Read().Count;
                if (score != Score.Read()) Score.Write(score);
            }
        ));
        cd.Add(G.Engine.RegisterComputer(
            new object[] { Score, G.TotalItemCount },
            () =>
            {
                bool w = Score.Read() >= G.TotalItemCount.Read();
                if (w != G.IsWinning.Read()) G.IsWinning.Write(w);
            }
        ));

        cd.Add(G.Engine.RegisterListener(
            new object[] { Score, G.TotalItemCount },
            () =>
            {
                countText.text = string.Format("Count: {0} / {1}",
                    Score.Read(), G.TotalItemCount.Read());
            }
        ));
        cd.Add(G.Engine.RegisterListener(
            new object[] { Score, G.TotalItemCount },
            () =>
            {
                if (Score.Read() >= G.TotalItemCount.Read()) winText.text = "You win!";
                else winText.text = "";
            }
        ));
        {
            bool canRestart = false;
            Coroutine lastCoroutine = null;
            cd.Add(new DisposableAction(() =>
            {
                if (lastCoroutine != null) G.Instance.StopCoroutine(lastCoroutine);
            }));
            cd.Add(G.Engine.RegisterListener(
                new object[] { Score, G.TotalItemCount },
                () =>
                {
                    int score = Score.Read();
                    int total = G.TotalItemCount.Read();
                    if (score < total) canRestart = true;
                    if (score >= total && canRestart)
                    {
                        canRestart = false;
                        if (lastCoroutine != null) G.Instance.StopCoroutine(lastCoroutine);
                        lastCoroutine = G.Instance.StartCoroutine(WaitThenRestart(G.Restart));
                    }
                }
            ));
        }
        cd.Add(G.Engine.RegisterListener(
            new object[] { G.Tick, G.Restart },
            () =>
            {
                if (G.Tick.Read().Count <= 0 && G.Restart.Read().Count <= 0) return;

                if (G.Restart.Read().Count > 0)
                {
                    transform.position = orgPos;
                }
                Position.Write(transform.position);
            }
        ));
    }

    private void OnApplicationQuit()
    {
        cd.Dispose();
    }

    private void OnDisable()
    {
        cd.Dispose();
    }

    private void FixedUpdate()
    {
        if (G.IsGameRunning.Read())
        {
            rb2d.AddForce(movement.Read() * speed);
        }
        else
        {
            rb2d.velocity = Vector2.zero;
            rb2d.angularVelocity = 0;
        }
    }

    private void Update()
    {
        var v = Vector2.zero;
        if (G.IsGameRunning.Read())
        {
            v.x = Input.GetAxis("Horizontal");
            v.y = Input.GetAxis("Vertical");
        }
        movement.Write(v);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var pickUp = collision.gameObject.GetComponent<CompletePickUp>();
        if (!pickUp) return;

        G.PickUp.Fire(new Ops.PickUp
        {
            Player = this,
            Item = pickUp
        });
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var enemy = collision.gameObject.GetComponent<CompleteEnemy>();
        if (!enemy) return;

        G.TouchEnemy.Fire(Empty.Instance);
    }

    private static IEnumerator WaitThenRestart(IOp<Empty> restart)
    {
        yield return new WaitForSeconds(1f);
        restart.Fire(Empty.Instance);
    }

    private static IEnumerator LoopFixedUpdate(Rigidbody2D rb2d, bool needStop, Vector2 forceToAdd)
    {
        if (needStop)
        {
            rb2d.velocity = Vector2.zero;
            rb2d.angularVelocity = 0;
        }
        else
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                rb2d.AddForce(forceToAdd);
            }
        }
    }
}
