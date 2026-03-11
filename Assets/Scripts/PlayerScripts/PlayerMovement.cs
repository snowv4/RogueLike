using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator anim;
    bool isFiring;
    private PlayerHealth playerHealth;
   
    [Header("Sprite Settings")]
    SpriteRenderer srBody;
    SpriteRenderer srHandR;
    SpriteRenderer srHandL;
    public float HandDistance = 0.5f;
    public Transform Hand_R;
    public Transform Hand_L;
    public Transform WeaponHolder; // pra flipar a arma horizontalmente
    Weapon currentWeapon;
   

    [Header("Dodge Settings")]
    public float dodgeForce = 40f;
    public float dodgeDuration = 0.25f;     // tempo rolando
    public float dodgeCooldown = 1f;        // tempo até poder usar de novo
    private bool isDodging = false;
    private bool dodgeOnCooldown = false;
    private float dodgeTimer = 0f;
    private float cooldownTimer = 0f;
    private Vector2 dodgeDir; // direção travada no início

    [Header("Dodge Collision")]
    [SerializeField] private bool dodgeIgnoresEnemies = true;
    [SerializeField] private string enemyLayerName = "Enemy";
    private int cachedEnemyLayer = int.MinValue;
    private bool enemyCollisionIgnored = false;

    [SerializeField] private SpriteRenderer sr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        srBody = GetComponent<SpriteRenderer>();
        srHandR = Hand_R.GetComponent<SpriteRenderer>();
        srHandL = Hand_L.GetComponent<SpriteRenderer>();
        currentWeapon = Hand_R.GetComponentInChildren<Weapon>(true); // mesmo desativada, encontra
        currentWeapon.gameObject.SetActive(true);

        playerHealth = GetComponent<PlayerHealth>();

    }

    // OS DOIS COMANDOS BASICOS PRA JOGAR PRECISO DE UMA FUNÇÃO
    //PRA MOVER
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    //PRA ATIRAR
    public void OnFire(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && currentWeapon != null && !isDodging)
        {
            currentWeapon.Shoot();
        }
        
        if (ctx.started){isFiring = true;}
    
        if (ctx.canceled){isFiring = false;}
        
    }

    void Update()
{
    // vira o sprite pelo mouse
    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    bool lookingUp = mouseWorldPos.y>transform.position.y;
    bool lookingLeft = mouseWorldPos.x< transform.position.x;
    sr.flipX = lookingLeft;
    Vector2 dir = (mouseWorldPos - transform.position).normalized;

    // fazer ele olhar pra cima
    anim.SetBool("lookingUp",lookingUp);

    // mao gira na direção do mouse
    Hand_R.position = transform.position + (Vector3)dir * HandDistance;

    // rotação da mão 
    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    Hand_R.rotation = Quaternion.Euler(0, 0, angle);
    currentWeapon.AimAt(dir);


  
    // maozinhas pra tras do player
    if (lookingUp)
        {
            
            srHandR.sortingOrder = srBody.sortingOrder - 1;
            srHandL.sortingOrder = srBody.sortingOrder - 1;
        } 
        else{
        srHandR.sortingOrder = srBody.sortingOrder + 1; // na frente
        srHandL.sortingOrder = srBody.sortingOrder + 1; // na frente
        
        }

    // timer do dodge1
    if (isDodging)
    {
        dodgeTimer -= Time.deltaTime;
        if (dodgeTimer <= 0)
        {
            isDodging = false;
            SetEnemyCollisionIgnored(false);
            srHandL.enabled = true;
            srHandR.enabled = true;
        }
    }

    // timer de cooldown
    if (dodgeOnCooldown)
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0)
            dodgeOnCooldown = false;
    }
    
    if (isFiring && currentWeapon != null && !isDodging){ 
        currentWeapon.Shoot();
        }
       

}



    private void FixedUpdate()
    {
        if (!isDodging)
        rb.linearVelocity = moveInput * moveSpeed;

        anim.SetBool("IsMoving", moveInput.sqrMagnitude > 0.1f);
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (context.performed && !isDodging  && !dodgeOnCooldown)
        {
            StartDodge();
        }
    }

    void StartDodge()
    {
        isDodging = true;
        dodgeOnCooldown = true;
        dodgeTimer = dodgeDuration;
        cooldownTimer = dodgeCooldown;

        isFiring = false;
        if (playerHealth != null)
        {
            playerHealth.AddTimedInvulnerability(dodgeDuration);
        }

        SetEnemyCollisionIgnored(true);

        // animação
        anim.SetTrigger("Dodge");


        // esconde a mão
        srHandR.enabled =false;
        srHandL.enabled =false;

        // direção do dodge = direção que o player está se movendo OU onde o mouse está
        dodgeDir = moveInput.normalized;

        if (dodgeDir == Vector2.zero)
        {
            // se o jogador estiver parado, dodge em direção ao mouse
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 dir = (mousePos - transform.position);

            // se o mouse estiver muito perto do player
            if (dir.sqrMagnitude < 0.01f)
            {
                dir = Vector2.right; // fallback padrão
            }

            dodgeDir = dir.normalized;
        }

        rb.linearVelocity = dodgeDir * dodgeForce;
    }

    int GetEnemyLayer()
    {
        if (cachedEnemyLayer != int.MinValue) return cachedEnemyLayer;
        cachedEnemyLayer = LayerMask.NameToLayer(enemyLayerName);
        return cachedEnemyLayer;
    }

    void SetEnemyCollisionIgnored(bool ignored)
    {
        if (!dodgeIgnoresEnemies) return;

        int enemyLayer = GetEnemyLayer();
        if (enemyLayer < 0) return;

        int playerLayer = gameObject.layer;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, ignored);
        enemyCollisionIgnored = ignored;
    }

    void OnDisable()
    {
        if (enemyCollisionIgnored)
        {
            SetEnemyCollisionIgnored(false);
        }
    }

}

