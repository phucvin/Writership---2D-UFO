using UnityEngine;

public class CompleteBullet : MonoBehaviour
{
    [SerializeField]
    private GameObject effect = null;

    private void Awake()
    {
        W.Mark(gameObject, "destroy");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Wall")) return;

        Destroy(gameObject);
        var e = Instantiate(effect, transform.position, transform.rotation);
        W.Mark(e, "active");
        e.SetActive(true);
    }
}
