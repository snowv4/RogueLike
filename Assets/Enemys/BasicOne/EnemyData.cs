using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Stats")]
    public float maxHealth = 30;
    public float speed = 2;
    public float damage = 1;
    public float damageCooldown = 1f;
    public float hitStunTime = 0.2f;

    [Header("Visual")]
    public Sprite sprite;
}



