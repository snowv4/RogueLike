using UnityEngine;

public class Bullet : MonoBehaviour
{
    public enum BulletOwner
    {
        Player,
        Enemy
    }

    [Header("Config")]
    public float speed = 200f;
    public int damage = 10;
    public float lifetime = 2f;
    public BulletOwner owner = BulletOwner.Player;

    Rigidbody2D rb;
    Collider2D col;
    Vector2 moveDir;
    bool hasMoveDir = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }
    
    void Start()
    {
        if (!hasMoveDir)
        {
            moveDir = ((Vector2)transform.right).normalized;
            hasMoveDir = true;
        }

        rb.linearVelocity = moveDir * speed;
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 direction)
    {
        moveDir = direction.sqrMagnitude > 0 ? direction.normalized : Vector2.right;
        hasMoveDir = true;

        if (rb != null)
        {
            rb.linearVelocity = moveDir * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (owner == BulletOwner.Player)
        {
            // Se acertar inimigo (qualquer coisa que implemente IDamageable)
            IDamageable dmg = collision.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                Debug.Log($"[Bullet] Hit {collision.name} with damage = {damage}");
                dmg.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            // Se acertar player
            PlayerHealth ph = collision.GetComponentInParent<PlayerHealth>();
            if (ph != null)
            {
                bool didDamage = ph.TryTakeDamage(damage);

                // Se deu dano, bala destrói (hit normal)
                if (didDamage)
                {
                    Destroy(gameObject);
                    return;
                }

                // Se NÃO deu dano (i-frames/dodge), atravessa e continua o trajeto
                if (col != null)
                {
                    Collider2D playerCol = collision;
                    Physics2D.IgnoreCollision(col, playerCol, true);
                }
                return;
            }
        }

        // parede
        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
