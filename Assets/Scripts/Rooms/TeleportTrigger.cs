using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    public Transform destination;

    [Header("Opcional")]
    public EnemyRoomController enemyRoomToSpawn;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (destination != null)
            other.transform.position = destination.position;

        if (enemyRoomToSpawn != null)
            enemyRoomToSpawn.SpawnEnemiesIfNeeded();
    }
}

