using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public enum DoorSide
    {
        Left,
        Right,
        Top,
        Bottom
    }

    [Header("Rooms")]
    [Tooltip("Obrigatoriamente Room005 (a inicial).")]
    public GameObject entryRoomPrefab;

    [Tooltip("Salas possíveis para gerar aleatoriamente (pode incluir Room005, mas não precisa).")]
    public List<GameObject> roomPrefabs = new List<GameObject>();

    [Header("Generation")]
    public int targetRoomCount = 6;
    public int maxPlacementAttemptsPerRoom = 25;
    public bool avoidOverlaps = true;
    public float overlapPadding = 0f;

    [Header("Runtime")]
    public bool clearExistingRoomsOnGenerate = true;

    readonly List<GameObject> spawnedRooms = new List<GameObject>();

    class RoomData
    {
        public GameObject roomGO;
        public readonly Dictionary<DoorSide, bool> doorUsed = new Dictionary<DoorSide, bool>();
        public readonly Dictionary<DoorSide, bool> hasDoor = new Dictionary<DoorSide, bool>();
    }

    static DoorSide Opposite(DoorSide side)
    {
        switch (side)
        {
            case DoorSide.Left: return DoorSide.Right;
            case DoorSide.Right: return DoorSide.Left;
            case DoorSide.Top: return DoorSide.Bottom;
            case DoorSide.Bottom: return DoorSide.Top;
            default: return side;
        }
    }

    static string DoorName(DoorSide side)
    {
        switch (side)
        {
            case DoorSide.Left: return "DoorLeft";
            case DoorSide.Right: return "DoorRight";
            case DoorSide.Top: return "DoorTop";
            case DoorSide.Bottom: return "DoorBottom";
            default: return "DoorLeft";
        }
    }

    static Transform FindByNameRecursive(Transform root, string name)
    {
        if (root == null) return null;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t != null && t.name == name)
                return t;
        }
        return null;
    }

    static bool TryGetDoorLocalPosition(GameObject prefab, DoorSide side, out Vector3 localPos)
    {
        localPos = default;
        if (prefab == null) return false;

        string doorName = DoorName(side);
        Transform doorT = FindByNameRecursive(prefab.transform, doorName);
        if (doorT == null) return false;

        // Portas são filhas diretas nos seus prefabs; se não forem, ainda assim funciona
        // porque usamos InverseTransformPoint para obter offset relativo ao root.
        localPos = prefab.transform.InverseTransformPoint(doorT.position);
        return true;
    }

    static bool PrefabHasDoor(GameObject prefab, DoorSide side)
    {
        if (prefab == null) return false;
        Transform doorT = FindByNameRecursive(prefab.transform, DoorName(side));
        return doorT != null;
    }

    static BoxCollider2D GetRoomCollider(GameObject roomGO)
    {
        if (roomGO == null) return null;
        Transform collT = roomGO.transform.Find("RoomCollider");
        if (collT != null) return collT.GetComponent<BoxCollider2D>();
        return roomGO.GetComponentInChildren<BoxCollider2D>();
    }

    static bool OverlapsAnyRoom(List<GameObject> existingRooms, GameObject candidate, float padding)
    {
        var candidateCol = GetRoomCollider(candidate);
        if (candidateCol == null) return false;

        Bounds cBounds = candidateCol.bounds;
        cBounds.Expand(padding);

        for (int i = 0; i < existingRooms.Count; i++)
        {
            var otherCol = GetRoomCollider(existingRooms[i]);
            if (otherCol == null) continue;

            Bounds oBounds = otherCol.bounds;
            oBounds.Expand(padding);

            if (cBounds.Intersects(oBounds))
                return true;
        }
        return false;
    }

    static Vector3 GetRoomColliderLocalPosition(GameObject prefab)
    {
        if (prefab == null) return Vector3.zero;
        Transform collT = FindByNameRecursive(prefab.transform, "RoomCollider");
        if (collT == null) return Vector3.zero;
        return prefab.transform.InverseTransformPoint(collT.position);
    }

    static Vector3 GetDoorWorldPosition(GameObject roomGO, DoorSide side)
    {
        if (roomGO == null) return Vector3.zero;
        var doorT = FindByNameRecursive(roomGO.transform, DoorName(side));
        return doorT != null ? doorT.position : Vector3.zero;
    }

    static RoomData BuildRoomData(GameObject roomGO)
    {
        var data = new RoomData { roomGO = roomGO };
        foreach (DoorSide side in System.Enum.GetValues(typeof(DoorSide)))
        {
            bool has = FindByNameRecursive(roomGO.transform, DoorName(side)) != null;
            data.hasDoor[side] = has;
            data.doorUsed[side] = false;
        }
        return data;
    }

    void ClearSpawnedRooms()
    {
        for (int i = 0; i < spawnedRooms.Count; i++)
        {
            if (spawnedRooms[i] != null)
                Destroy(spawnedRooms[i]);
        }
        spawnedRooms.Clear();
    }

    public GameObject GenerateAtRoomColliderCenter(Vector3 roomColliderCenterWorldPos, Quaternion roomRotation)
    {
        if (entryRoomPrefab == null)
        {
            Debug.LogWarning("RoomGenerator: entryRoomPrefab não foi definido.");
            return null;
        }

        if (clearExistingRoomsOnGenerate)
            ClearSpawnedRooms();

        // Garante uma lista mínima pra fallback.
        if (roomPrefabs == null || roomPrefabs.Count == 0)
            roomPrefabs = new List<GameObject> { entryRoomPrefab };

        Vector3 entryColliderLocalOffset = GetRoomColliderLocalPosition(entryRoomPrefab);
        Vector3 entryRoomRootPos = roomColliderCenterWorldPos - (roomRotation * entryColliderLocalOffset);

        var entryRoomGO = Instantiate(entryRoomPrefab, entryRoomRootPos, roomRotation, transform);
        spawnedRooms.Add(entryRoomGO);

        var rooms = new List<RoomData> { BuildRoomData(entryRoomGO) };

        // Gera conexões.
        while (spawnedRooms.Count < targetRoomCount)
        {
            // Frontier: qualquer porta ainda não usada em qualquer sala.
            var frontier = new List<DoorSlot>();
            for (int i = 0; i < rooms.Count; i++)
            {
                foreach (DoorSide side in System.Enum.GetValues(typeof(DoorSide)))
                {
                    if (rooms[i].hasDoor.TryGetValue(side, out bool has) && has)
                    {
                        if (rooms[i].doorUsed.TryGetValue(side, out bool used) && !used)
                            frontier.Add(new DoorSlot { roomIndex = i, side = side });
                    }
                }
            }

            if (frontier.Count == 0)
                break;

            var chosen = frontier[Random.Range(0, frontier.Count)];
            int currentRoomIndex = chosen.roomIndex;
            DoorSide fromSide = chosen.side;
            DoorSide toSide = Opposite(fromSide);

            GameObject currentRoomGO = rooms[currentRoomIndex].roomGO;
            Vector3 fromDoorWorldPos = GetDoorWorldPosition(currentRoomGO, fromSide);

            // Filtra salas candidatas que possuem a porta "toSide".
            var candidates = new List<GameObject>();
            for (int i = 0; i < roomPrefabs.Count; i++)
            {
                var p = roomPrefabs[i];
                if (p == null) continue;
                if (PrefabHasDoor(p, toSide))
                    candidates.Add(p);
            }

            if (candidates.Count == 0)
                break;

            bool placed = false;
            // Tentativas: escolhe candidato aleatório e tenta encaixar sem sobrepor.
            for (int attempt = 0; attempt < maxPlacementAttemptsPerRoom && !placed; attempt++)
            {
                var candidatePrefab = candidates[Random.Range(0, candidates.Count)];
                if (candidatePrefab == null) continue;

                if (!TryGetDoorLocalPosition(candidatePrefab, toSide, out Vector3 candidateToDoorLocal))
                    continue;

                Vector3 candidateRoomRootPos = fromDoorWorldPos - (roomRotation * candidateToDoorLocal);

                GameObject candidateRoomGO = Instantiate(candidatePrefab, candidateRoomRootPos, roomRotation, transform);

                if (avoidOverlaps && OverlapsAnyRoom(spawnedRooms, candidateRoomGO, overlapPadding))
                {
                    Destroy(candidateRoomGO);
                    continue;
                }

                spawnedRooms.Add(candidateRoomGO);

                var newRoomData = BuildRoomData(candidateRoomGO);
                rooms.Add(newRoomData);

                // Marca as portas conectadas como "usadas".
                rooms[currentRoomIndex].doorUsed[fromSide] = true;
                newRoomData.doorUsed[toSide] = true;

                placed = true;
            }

            if (!placed)
                break;
        }

        return entryRoomGO;
    }

    struct DoorSlot
    {
        public int roomIndex;
        public DoorSide side;
    }
}

