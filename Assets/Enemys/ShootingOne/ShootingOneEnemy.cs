using UnityEngine;
using System.Collections;

public class ShootingOneEnemy : MonoBehaviour, IDamageable
{
    public EnemyData data;
    public float currentHealth;

    SpriteRenderer sr;
    Color defaultColor;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            defaultColor = sr.color;
            // Só sobrescreve se o sprite do Inspector estiver vazio
            if (sr.sprite == null && data != null && data.sprite != null)
                sr.sprite = data.sprite;
        }

        currentHealth = data != null ? data.maxHealth : currentHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        StartCoroutine(HitFlash());

        if (currentHealth <= 0)
            Destroy(gameObject);
    }

    IEnumerator HitFlash()
    {
        if (sr == null) yield break;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        sr.color = defaultColor;
    }
}

