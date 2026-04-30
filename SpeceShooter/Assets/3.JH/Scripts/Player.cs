using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public enum States
    {
        Idle,
        Move,
        Attack,
        Hit,
        Dead
    }
    public int life = 3; // 한대 맞으면 1씩 줄음
    public int attackPoint;
    public int power; // 총알 파워
    public int boomSlot; //가지고 있는 폭탄 갯수 최대치
    public int score;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveInputSmoothing = 0.15f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 0.15f;
    [SerializeField] private float fireInputGraceTime = 0.08f;
    [SerializeField] private Transform firePoint;

    [Header("Follower")]
    [SerializeField] private Follower followerPrefab;
    [SerializeField] private Transform followerRoot;

    [Header("Damage")]
    [SerializeField] private float invincibilityDuration = 0.5f;
    [SerializeField] private float invincibleBlinkInterval = 0.1f;
    [SerializeField] private float invincibleAlpha = 0.45f;
    
    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 1.5f;
    [SerializeField] private float respawnInvincibilityDuration = 2f;
    [SerializeField] private Transform respawnPoint;

    private const string MoveStateParam = "State";
    
    
    public States state = States.Idle;
    private Animator animator;
    private SpriteRenderer[] spriteRenderers;
    private Vector2 moveInput;
    private float lastAttackTime;
    private int moveStateHash;
    private bool deathHandled;
    private float hitStateEndTime;
    private float respawnInvincibleEndTime;
    private float lastFireInputTime = -999f;
    private Coroutine fireCoroutine;
    private Camera cachedCamera;
    private readonly List<Follower> followers = new List<Follower>();
    private int syncedFollowerCount = -1;
    private readonly List<Vector3> positionHistory = new List<Vector3>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        cachedCamera = Camera.main;
        lastAttackTime = -attackCooldown;
        moveStateHash = Animator.StringToHash(MoveStateParam);
        positionHistory.Add(transform.position);
        SyncFollowerCount();
        ApplyVisualAlpha(1f);
    }

    public float RespawnDelay => respawnDelay;

    public Vector3 GetRespawnPosition()
    {
        return respawnPoint != null ? respawnPoint.position : transform.position;
    }

    public bool IsInvincible =>
        state == States.Dead ||
        (state == States.Hit && Time.time < hitStateEndTime) ||
        Time.time < respawnInvincibleEndTime;

    public void ActivateRespawnInvincibility(float duration)
    {
        float appliedDuration = Mathf.Min(duration, respawnInvincibilityDuration);
        respawnInvincibleEndTime = Time.time + appliedDuration;
        UpdateInvincibilityVisuals();
    }

    public void TakeDamage(int amount)
    {
        if (IsInvincible)
        {
            return;
        }

        if (UIManager.Instance != null)
            UIManager.Instance.HandlePlayerHit(gameObject, GetRespawnPosition(), RespawnDelay, amount);
    }

    public void ApplyDamageFeedback(bool survived)
    {
        if (!survived)
        {
            return;
        }

        EnterHitState();
        transform.position = GetRespawnPosition();
        moveInput = Vector2.zero;
        ActivateRespawnInvincibility(respawnInvincibilityDuration);

        positionHistory.Clear();
        positionHistory.Add(transform.position);
    }

    private bool IsHitInvincible()
    {
        return state == States.Hit && Time.time < hitStateEndTime;
    }

    private void EnterHitState()
    {
        state = States.Hit;
        hitStateEndTime = Time.time + invincibilityDuration;

        if (fireCoroutine != null)
        {
            StopCoroutine(fireCoroutine);
            fireCoroutine = null;
        }

        UpdateInvincibilityVisuals();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == States.Hit && Time.time >= hitStateEndTime && life > 0)
        {
            state = moveInput == Vector2.zero ? States.Idle : States.Move;
        }

        UpdateInvincibilityVisuals();

        if (life <= 0)
        {
            if (fireCoroutine != null)
            {
                StopCoroutine(fireCoroutine);
                fireCoroutine = null;
            }
            HandleDeath();
            return;
        }

        //움직임 기능
        //방향키로 이동 대각선 포함
        //애니메이션 구현 Player_Idle,Player_Left,Player_Right
        HandleMovement();
        RecordPositionHistory();
        HandleAttack();
        SyncFollowerCount();
        UpdateAnimation();
    }

    private void OnDisable()
    {
        ApplyVisualAlpha(1f);

        if (life <= 0)
        {
            HandleDeath();
        }
    }

    private void OnDestroy()
    {
        if (life <= 0)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        state = States.Dead;

        if (deathHandled) return;

        deathHandled = true;
        RemoveAllFollowers();
        hitStateEndTime = 0f;

        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }

    private void UpdateInvincibilityVisuals()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            return;
        }

        float invincibleEndTime = Mathf.Max(hitStateEndTime, respawnInvincibleEndTime);
        if (Time.time >= invincibleEndTime)
        {
            ApplyVisualAlpha(1f);
            return;
        }

        float blinkInterval = Mathf.Max(0.05f, invincibleBlinkInterval);
        bool useTransparentAlpha = Mathf.FloorToInt(Time.time / blinkInterval) % 2 == 0;
        ApplyVisualAlpha(useTransparentAlpha ? invincibleAlpha : 1f);
    }

    private void ApplyVisualAlpha(float alpha)
    {
        if (spriteRenderers == null)
        {
            return;
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            SpriteRenderer spriteRenderer = spriteRenderers[i];
            if (spriteRenderer == null)
            {
                continue;
            }

            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }

    private void SyncFollowerCount()
    {
        int desiredCount = Mathf.Clamp(power - 2, 0, 3);

        if (followerPrefab == null)
        {
            syncedFollowerCount = desiredCount;
            return;
        }

        followers.RemoveAll(follower => follower == null);

        if (followers.Count == desiredCount)
        {
            syncedFollowerCount = desiredCount;
            return;
        }

        Transform root = followerRoot;
        if (root != null && !root.gameObject.scene.IsValid())
        {
            // Assigned object is a persistent asset (e.g. prefab), cannot be a runtime parent.
            root = null;
        }

        while (followers.Count < desiredCount)
        {
            int spawnIndex = followers.Count;
            Vector3 spawnPos = transform.position + Vector3.down * 1.2f * (spawnIndex + 1);
            Follower follower = Instantiate(followerPrefab, spawnPos, Quaternion.identity, root);
            followers.Add(follower);
        }

        while (followers.Count > desiredCount)
        {
            int lastIndex = followers.Count - 1;
            Follower follower = followers[lastIndex];
            followers.RemoveAt(lastIndex);

            if (follower != null)
            {
                Destroy(follower.gameObject);
            }
        }

        for (int i = 0; i < followers.Count; i++)
        {
            if (followers[i] == null)
            {
                continue;
            }

            followers[i].SetFollowerIndex(i);
        }

        syncedFollowerCount = desiredCount;
    }

    private void RemoveAllFollowers()
    {
        foreach (var follower in followers)
        {
            if (follower != null)
            {
                Destroy(follower.gameObject);
            }
        }
        followers.Clear();
        syncedFollowerCount = 0;
    }

    private void RecordPositionHistory()
    {
        if (positionHistory.Count == 0 || Vector3.Distance(transform.position, positionHistory[0]) > 0.05f)
        {
            positionHistory.Insert(0, transform.position);

            float maxLength = 3 * 1.5f + 5f;
            float accumulated = 0f;
            for (int i = 1; i < positionHistory.Count - 1; i++)
            {
                accumulated += Vector3.Distance(positionHistory[i - 1], positionHistory[i]);
                if (accumulated > maxLength)
                {
                    positionHistory.RemoveRange(i, positionHistory.Count - i);
                    break;
                }
            }
        }
    }

    public Vector3 GetHistoryPosition(float distanceFromHead)
    {
        if (positionHistory.Count == 0)
        {
            return transform.position;
        }

        float accumulated = 0f;
        for (int i = 0; i < positionHistory.Count - 1; i++)
        {
            float segLen = Vector3.Distance(positionHistory[i], positionHistory[i + 1]);
            if (accumulated + segLen >= distanceFromHead)
            {
                float t = (distanceFromHead - accumulated) / segLen;
                return Vector3.Lerp(positionHistory[i], positionHistory[i + 1], t);
            }
            accumulated += segLen;
        }
        return positionHistory[positionHistory.Count - 1];
    }

    private void HandleMovement()
    {
        float horizontal = 0f;
        float vertical = 0f;
        
        var h = Input.GetAxisRaw("Horizontal");
        
        
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            horizontal -= 1f;
            
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            horizontal += 1f;
            
        }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            vertical += 1f;
            
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            vertical -= 1f;
            
        }

        Vector2 targetInput = new Vector2(horizontal, vertical).normalized;
        moveInput = Vector2.Lerp(moveInput, targetInput, moveInputSmoothing);
        transform.position += (Vector3)(moveInput * moveSpeed * Time.deltaTime);
        ClampPositionToScreen();
    }

    private void ClampPositionToScreen()
    {
        if (cachedCamera == null) cachedCamera = Camera.main;
        if (cachedCamera == null) return;

        float z = transform.position.z - cachedCamera.transform.position.z;
        Vector3 min = cachedCamera.ViewportToWorldPoint(new Vector3(0f, 0f, z));
        Vector3 max = cachedCamera.ViewportToWorldPoint(new Vector3(1f, 1f, z));

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, min.x, max.x);
        pos.y = Mathf.Clamp(pos.y, min.y, max.y);
        transform.position = pos;
    }

    //private void HandleBoom()
   // {
       // if (Input.GetKeyDown(KeyCode.Z))
            //BoomManager.Instance?.UseBoom();
    //}

    private void HandleAttack()
    {
        if (state == States.Hit)
        {
            if (fireCoroutine != null)
            {
                StopCoroutine(fireCoroutine);
                fireCoroutine = null;
            }
            return;
        }

        bool isFireInputPressed = Input.GetKey(KeyCode.Space) || Input.GetButton("Fire1");
        if (isFireInputPressed)
        {
            lastFireInputTime = Time.time;
        }

        bool shouldKeepFiring = (Time.time - lastFireInputTime) <= fireInputGraceTime;

        if (shouldKeepFiring)
        {
            if (fireCoroutine == null)
            {
                fireCoroutine = StartCoroutine(FireRoutine());
            }
        }
        else
        {
            if (fireCoroutine != null)
            {
                StopCoroutine(fireCoroutine);
                fireCoroutine = null;
            }

            if (state == States.Attack)
            {
                state = moveInput == Vector2.zero ? States.Idle : States.Move;
            }
        }
    }

    private IEnumerator FireRoutine()
    {
        while (true)
        {
            state = States.Attack;
            attackPoint = power;
            SpawnBullet();
            yield return new WaitForSeconds(attackCooldown);
        }
    }

    private void SpawnBullet()
    {
        Vector2 spawnPos = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        PlayerBulletManager.Instance?.Fire(spawnPos);

        foreach (var follower in followers)
        {
            if (follower != null)
                follower.Fire();
        }
    }

    private void UpdateAnimation()
    {
        if (moveInput == Vector2.zero && state != States.Attack && state != States.Hit)
        {
            state = States.Idle;
        }
        else if (moveInput != Vector2.zero && state != States.Attack && state != States.Hit)
        {
            state = States.Move;
        }

        if (animator == null)
        {
            return;
        }

        int moveStateValue = 0;

        if (moveInput.x < -0.01f)
        {
            moveStateValue = -1;
        }
        else if (moveInput.x > 0.01f)
        {
            moveStateValue = 1;
        }

        animator.SetInteger(moveStateHash, moveStateValue);
    }
}
