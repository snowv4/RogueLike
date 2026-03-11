using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    public Transform firePoint;
    public Bullet bulletPrefab;

    [Header("Tiro")]
    public float fireRate = 3.0f;       // quantos tiros por segundo dentro da rajada
    public float range = 8f;            // distância máxima para começar o comportamento
    public int bulletDamage = 1;        // dano do tiro
    public float bulletSpeed = 200f;

    [Header("Movimento em torno do player")]
    public float moveSpeed = 3f;
    public float desiredDistance = 3f;      // distância alvo em relação ao player
    public float minDistanceFromPlayer = 1.5f; // não chega mais perto que isso
    public float stopTolerance = 0.05f;     // quão perto do alvo ele considera que chegou
    public int shotsPerBurst = 3;
    public float timeBetweenBursts = 0.8f;  // pausa entre rajadas

    Transform player;
    bool isShooting = false;
    Vector2 currentMoveTarget;
    bool hasMoveTarget = false;

    void Start()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) player = go.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > range) return;

        // se está atirando, não se move
        if (isShooting) return;

        // se ainda não tem um alvo de movimento, define o primeiro
        if (!hasMoveTarget)
        {
            currentMoveTarget = CalcularPontoInicial(dist);
            hasMoveTarget = true;
        }

        // move até o ponto alvo
        Vector2 pos2D = transform.position;
        Vector2 dirMove = (currentMoveTarget - pos2D);

        if (dirMove.sqrMagnitude > stopTolerance * stopTolerance)
        {
            dirMove.Normalize();
            transform.position = pos2D + dirMove * moveSpeed * Time.deltaTime;
        }
        else
        {
            // chegou no ponto: inicia rajada
            StartCoroutine(ShootBurst());
        }
    }

    Vector2 CalcularPontoInicial(float distAtual)
    {
        Vector2 playerPos = player.position;
        Vector2 selfPos = transform.position;

        // se está muito longe, caminha em linha reta até a distância desejada
        if (distAtual > desiredDistance + 0.1f)
        {
            Vector2 dir = (selfPos - playerPos).normalized;
            return playerPos + dir * desiredDistance;
        }

        // se já está mais ou menos na distância, fica onde está
        return selfPos;
    }

    Vector2 CalcularNovoPontoEmTornoDoPlayer()
    {
        Vector2 playerPos = player.position;

        // pega uma direção aleatória ao redor do player
        Vector2 dir = Random.insideUnitCircle.normalized;
        float dist = desiredDistance;

        Vector2 alvo = playerPos + dir * dist;

        // garante que não fique perto demais
        Vector2 toPlayer = playerPos - alvo;
        if (toPlayer.magnitude < minDistanceFromPlayer)
        {
            alvo = playerPos - toPlayer.normalized * minDistanceFromPlayer;
        }

        return alvo;
    }

    System.Collections.IEnumerator ShootBurst()
    {
        isShooting = true;
        hasMoveTarget = false;

        float interval = 1f / fireRate;

        for (int i = 0; i < shotsPerBurst; i++)
        {
            ShootOnce();
            yield return new WaitForSeconds(interval);
        }

        // escolhe um novo ponto em torno do player
        currentMoveTarget = CalcularNovoPontoEmTornoDoPlayer();
        hasMoveTarget = true;

        // pequena pausa antes de começar a andar de novo
        yield return new WaitForSeconds(timeBetweenBursts);

        isShooting = false;
    }

    void ShootOnce()
    {
        Vector2 dir = (player.position - firePoint.position).normalized;

        var bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.owner = Bullet.BulletOwner.Enemy;
        bullet.damage = bulletDamage;
        bullet.speed = bulletSpeed;
        bullet.SetDirection(dir);
    }
}