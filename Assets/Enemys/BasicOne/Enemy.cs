using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
    , IDamageable
{
    float speed;
    float damage;
    float damageCooldown;
    float hitStunTime;
    public EnemyData data;
    Transform player;
    public float currentHealth;
    float damageTimer = 0.2f;
    bool isStunned = false;

    SpriteRenderer sr;
    Color defaultColor;

    void Start()
    {
        currentHealth = data.maxHealth;
        speed = data.speed;
        damage = data.damage;
        damageCooldown = data.damageCooldown;
        hitStunTime = data.hitStunTime;
        player = GameObject.FindGameObjectWithTag("Player").transform;

        sr = GetComponent<SpriteRenderer>();
        // Só sobrescreve se o sprite do Inspector estiver vazio
        if (sr.sprite == null && data != null && data.sprite != null)
            sr.sprite = data.sprite;
        defaultColor = sr.color;
    }

    void Update()
    {
        if (player == null) return;

        // reduz cooldown de dano
        if (damageTimer > 0)
            damageTimer -= Time.deltaTime;

        // se estiver tomando hit, não anda
        if (isStunned) return;

        // anda em direção ao player
        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)dir * speed * Time.deltaTime;

        // vira pro lado certo
        sr.flipX = dir.x < 0;
        AvoidOtherEnemies();

    }

    public void TakeDamage(float amount)
    {
        Debug.Log($"[Enemy] TakeDamage called with amount = {amount} (hp antes = {currentHealth})");
        currentHealth -= amount;

        StartCoroutine(HitFlash());
        StartCoroutine(HitStun());

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator HitFlash()
    {
        sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        sr.color = defaultColor;
    }

    IEnumerator HitStun()
    {
        isStunned = true;
        yield return new WaitForSeconds(hitStunTime);
        isStunned = false;
    }

    void Die()
    {
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log($"[Enemy] OnTriggerStay2D with: {collision.gameObject.name}, tag:{collision.tag}");
        if (!collision.CompareTag("Player")) return;

        Debug.Log("[Enemy] PLAYER detected in trigger!");
        if (damageTimer <= 0)
        {
            var ph = collision.GetComponent<PlayerHealth>();
            Debug.Log($"PlayerHealth component found on same object? { (ph!=null) }");
            // tenta buscar em parent caso esteja em child
            if (ph == null) ph = collision.GetComponentInParent<PlayerHealth>();
            Debug.Log($"PlayerHealth found in parent? { (ph!=null) }");
            if (ph != null) {
                ph.TakeDamage(damage);
                damageTimer = damageCooldown;
                Debug.Log("[Enemy] Dealt damage to player");
            } else {
                Debug.LogWarning("[Enemy] PlayerHealth not found on collided object or parents.");
            }
        }
    }


    void AvoidOtherEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.5f);

        foreach (var h in hits)
        {
            if (h.CompareTag("Enemy") && h.gameObject != gameObject)
            {
                Vector2 away = (transform.position - h.transform.position).normalized;
                transform.position += (Vector3)away * 2f * Time.deltaTime;
            }
        }
    }

}
