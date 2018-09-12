using UnityEngine;

public class CompleteBullet : MonoBehaviour
{
    [SerializeField]
    private CompletePlayerController player = null;
    [SerializeField]
    private GameObject effect = null;

    private void Awake()
    {
        W.Mark(gameObject, "destroy");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") ||
            collision.gameObject.CompareTag("PickUp")) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            G.Hit.Fire(new Ops.Hit
            {
                FromPlayer = player,
                WithBullet = this,
                ToEnemy = collision.gameObject
            });
        }

        Destroy(gameObject);
        var e = Instantiate(effect, transform.position, transform.rotation);
        W.Mark(e, "active");
        e.SetActive(true);
    }
}
