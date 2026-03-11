using System.Collections;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Transform hubSpawnPoint;
    public float respawnDelay = 2f;

    [Header("Opcional")]
    public EnemyRoomController enemyRoomToReset;

    PlayerHealth health;
    Rigidbody2D rb;
    PlayerMovement movement;
    Collider2D col;

    Coroutine respawnRoutine;

    void Awake()
    {
        health = GetComponent<PlayerHealth>();
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        col = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        if (health != null) health.Died += OnDied;
    }

    void OnDisable()
    {
        if (health != null) health.Died -= OnDied;
    }

    void OnDied()
    {
        if (respawnRoutine != null) StopCoroutine(respawnRoutine);
        respawnRoutine = StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        if (movement != null) movement.enabled = false;
        if (col != null) col.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(respawnDelay);

        if (hubSpawnPoint != null)
            transform.position = hubSpawnPoint.position;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (health != null) health.RespawnAtFull();

        if (enemyRoomToReset != null)
            enemyRoomToReset.ResetRoom();

        if (col != null) col.enabled = true;
        if (movement != null) movement.enabled = true;
    }
}

