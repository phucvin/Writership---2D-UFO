using UnityEngine;

public class CompleteForward : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rb2d = null;
    [SerializeField]
    private float speed = 10f;

    private void Awake()
    {
        W.Mark(rb2d);
    }

    private void OnEnable()
    {
        rb2d.velocity = transform.right * speed;
    }

    private void OnDisable()
    {
        rb2d.velocity = Vector2.zero;
    }
}
