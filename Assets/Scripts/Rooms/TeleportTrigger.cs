using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    public Transform destination;

    [Header("Opcional")]
    public EnemyRoomController enemyRoomToSpawn;

    [Header("Opcional - Gerador de Salas")]
    public RoomGenerator roomGenerator;
    [Tooltip("Se ligado, a sala inicial (Room005) é alinhada com a posição do portal.")]
    public bool usePortalPositionAsRoomColliderCenter = true;
    [Tooltip("Offset aplicado na posição final do jogador dentro da sala inicial.")]
    public Vector3 playerOffsetInEntryRoom = Vector3.zero;
    [Tooltip("Gera apenas 1 vez por sessão (evita recriar salas repetidamente).")]
    public bool generateOncePerRun = true;
    bool hasGenerated;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (roomGenerator != null && (!generateOncePerRun || !hasGenerated))
        {
            Vector3 anchorWorldPos = usePortalPositionAsRoomColliderCenter ? transform.position : (destination != null ? destination.position : transform.position);
            var entryRoom = roomGenerator.GenerateAtRoomColliderCenter(anchorWorldPos, Quaternion.identity);
            if (entryRoom != null)
            {
                other.transform.position = anchorWorldPos + playerOffsetInEntryRoom;
                hasGenerated = true;
            }
        }
        else
        {
            if (destination != null)
                other.transform.position = destination.position;
        }

        if (enemyRoomToSpawn != null)
            enemyRoomToSpawn.SpawnEnemiesIfNeeded();
    }
}

