using UnityEngine;
using System.Collections;
using System;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 6;
    public float invincibilityTime = 1f; // tempo sem levar dano dps de acertado

    [Header("Feedback de Dano")]
    public GameObject damagePopupPrefab;
    public Vector3 damagePopupOffset = new Vector3(0f, 0.5f, 0f);

    public int currentHealth;

    int invulnerabilityStacks = 0;
    public bool IsInvulnerable => invulnerabilityStacks > 0;

    public bool IsDead => isDead;
    public event Action Died;
    bool isDead = false;

    SpriteRenderer sr;
    Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    public bool TryTakeDamage(float amount)
    {
        if (isDead) return false;
        if (IsInvulnerable) return false;

        int damageInt = Mathf.RoundToInt(amount);
        currentHealth -= damageInt;

        StartCoroutine(DamageFlash());
        AddTimedInvulnerability(invincibilityTime);

        SpawnDamagePopup(damageInt);

        if (currentHealth <= 0)
        {
            Die();
        }

        return true;
    }

    public void TakeDamage(float amount)
    {
        TryTakeDamage(amount);
    }

    IEnumerator DamageFlash()
    {
        int flashes = 3;
        float flashDuration = 0.08f;

        for (int i = 0; i < flashes; i++)
        {
            sr.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
            sr.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }
    }

    void SpawnDamagePopup(int damageAmount)
    {
        if (damagePopupPrefab == null) return;

        Vector3 spawnPos = transform.position + damagePopupOffset;
        GameObject popupObj = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);

        DamagePopup popup = popupObj.GetComponentInChildren<DamagePopup>();
        if (popup != null)
        {
            popup.Setup(damageAmount);
        }
    }

    public void AddInvulnerability()
    {
        invulnerabilityStacks++;
    }

    public void RemoveInvulnerability()
    {
        invulnerabilityStacks = Mathf.Max(0, invulnerabilityStacks - 1);
    }

    public void AddTimedInvulnerability(float duration)
    {
        StartCoroutine(TimedInvulnerability(duration));
    }

    IEnumerator TimedInvulnerability(float duration)
    {
        AddInvulnerability();
        yield return new WaitForSeconds(duration);
        RemoveInvulnerability();
    }

    public void RespawnAtFull()
    {
        isDead = false;
        invulnerabilityStacks = 0;
        currentHealth = maxHealth;

        StopAllCoroutines();
        if (sr != null) sr.color = originalColor;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("Player morreu!");
        Died?.Invoke();
    }
}
