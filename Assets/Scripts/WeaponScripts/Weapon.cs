using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float fireRate = 0.2f;
    public int damage = 10; // 👈 adicionado

    public float rotacaox = 0;
    public float rotacaoy = 0;
    public float rotacaoz = 0;

    float nextShotTime = 0f;
    private float cooldown;
    private SpriteRenderer sr;

    private void Awake() {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (cooldown > 0)
            cooldown -= Time.deltaTime;
    }

    public void Shoot()
    {
        if (cooldown > 0) return;
        if (Time.time < nextShotTime) return;

        nextShotTime = Time.time + fireRate;
        cooldown = fireRate;

       var b = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
        var bulletComp = b.GetComponent<Bullet>();
        if (bulletComp != null) {
            bulletComp.damage = damage; // garante que passe o dano
            Debug.Log($"[Weapon] Spawned bullet with damage = {bulletComp.damage}");
        } else {
            Debug.LogWarning("[Weapon] prefab não tem Bullet!");
        }

    }

    public void AimAt(Vector3 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.localRotation = Quaternion.Euler(0, 0, 0);

        // flip automático
        if (angle > 90 || angle < -90)
        {
            transform.localRotation = Quaternion.Euler(rotacaox, rotacaoy, rotacaoz);
        }
    }
}
