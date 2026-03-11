using System.Collections.Generic;
using UnityEngine;

public class EnemyRoomController : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject[] enemyPrefabs; // seus 3 prefabs

    readonly List<GameObject> spawned = new List<GameObject>();

    public void SpawnEnemiesIfNeeded()
    {
        CleanupDead();
        if (spawned.Count > 0) return; // já tem inimigos vivos

        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            GameObject prefab = enemyPrefabs[i];
            if (prefab == null) continue;

            Transform sp = spawnPoints[Mathf.Min(i, spawnPoints.Length - 1)];
            var obj = Instantiate(prefab, sp.position, Quaternion.identity);
            spawned.Add(obj);
        }

        CleanupDead();
    }

    public void ResetRoom()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i] != null)
                Destroy(spawned[i]);
        }
        spawned.Clear();
    }

    void CleanupDead()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] == null) spawned.RemoveAt(i);
        }
    }
}

