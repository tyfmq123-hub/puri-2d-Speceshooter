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
    [SerializeField] private Transform firePoint;

    [Header("Follower")]
    [SerializeField] private Follower followerPrefab;
    [SerializeField] private Transform followerRoot;

    [Header("Damage")]
    [SerializeField] private float invincibilityDuration = 1f;
    
    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 1.5f;
    [SerializeField] private Transform respawnPoint;

    private const string MoveStateParam = "State";
    
    
    public States state = States.Idle;
    private Animator animator;
    private Vector2 moveInput;
    private float lastAttackTime;
    private int moveStateHash;
    private int previousLife;
    private States previousState;
    private bool deathLogged;
    private bool deathHandled;
    private Vector3 initialSpawnPosition;
    private int pendingUiDamage;
    private float hitStateEndTime;
    private Coroutine fireCoroutine;
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
        lastAttackTime = -attackCooldown;
        moveStateHash = Animator.StringToHash(MoveStateParam);
        previousLife = life;
        previousState = state;
        initialSpawnPosition = transform.position;
        Debug.Log($"[Player] Life initialized: {life}", this);
        Debug.Log($"[Player] State initialized: {state}", this);
        positionHistory.Add(transform.position);
        SyncFollowerCount();
    }

    public float RespawnDelay => respawnDelay;

    public Vector3 GetRespawnPosition()
    {
        return respawnPoint != null ? respawnPoint.position : transform.position;
    }

    public void TakeDamage(int amount)
    {
        if (state == States.Dead || IsHitInvincible())
        {
            return;
        }

        int appliedDamage = Mathf.Max(1, amount);
        life = Mathf.Max(0, life - appliedDamage);
        pendingUiDamage += appliedDamage;

        if (life > 0)
        {
            EnterHitState();
        }
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
    }

    // Update is called once per frame
    void Update()
    {
        if (state == States.Hit && Time.time >= hitStateEndTime && life > 0)
        {
            state = moveInput == Vector2.zero ? States.Idle : States.Move;
        }

        if (life != previousLife)
        {
            int oldLife = previousLife;
            int newLife = life;
            Debug.Log($"[Player] Life changed: {previousLife} -> {life}", this);
            previousLife = newLife;
            deathLogged = false;

            if (newLife < oldLife)
            {
                HandleLifeReduced(oldLife - newLife);
            }
        }

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
        HandleBoom();
        SyncFollowerCount();
        UpdateAnimation();
        LogStateChanged();
    }

    private void HandleLifeReduced(int damageAmount)
    {
        if (damageAmount <= 0)
        {
            return;
        }

        int uiDamage = Mathf.Min(damageAmount, pendingUiDamage);
        if (UIManager.Instance != null && uiDamage > 0)
        {
            for (int i = 0; i < uiDamage; i++)
            {
                UIManager.Instance.DecreaseLife();
            }
        }
        pendingUiDamage = Mathf.Max(0, pendingUiDamage - uiDamage);

        if (life > 0)
        {
            transform.position = initialSpawnPosition;
            moveInput = Vector2.zero;

            positionHistory.Clear();
            positionHistory.Add(transform.position);
        }
    }

    private void OnDisable()
    {
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
        if (!deathLogged)
        {
            Debug.Log("[Player] Life reached 0. State changed to Dead.", this);
            deathLogged = true;
        }

        state = States.Dead;
        LogStateChanged();

        if (deathHandled)
        {
            return;
        }

        deathHandled = true;
        RemoveAllFollowers();
        hitStateEndTime = 0f;

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    private void LogStateChanged()
    {
        if (state == previousState)
        {
            return;
        }

        Debug.Log($"[Player] State changed: {previousState} -> {state}", this);
        previousState = state;
    }

    private void SyncFollowerCount()
    {
        int desiredCount = Mathf.Clamp(power, 0, 3);

        if (followerPrefab == null)
        {
            syncedFollowerCount = desiredCount;
            return;
        }

        followers.RemoveAll(follower => follower == null);

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
        
        
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            horizontal -= 1f;
            
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            horizontal += 1f;
            
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            vertical += 1f;
            
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            vertical -= 1f;
            
        }

        Vector2 targetInput = new Vector2(horizontal, vertical).normalized;
        moveInput = Vector2.Lerp(moveInput, targetInput, moveInputSmoothing);
        transform.position += (Vector3)(moveInput * moveSpeed * Time.deltaTime);
    }

    private void HandleBoom()
      {
        if (Input.GetKeyDown(KeyCode.Z))
            BoomManager.Instance?.UseBoom();
      }

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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (fireCoroutine != null) StopCoroutine(fireCoroutine);
            fireCoroutine = StartCoroutine(FireRoutine());
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (fireCoroutine != null)
            {
                StopCoroutine(fireCoroutine);
                fireCoroutine = null;
            }
            if (state == States.Attack)
                state = moveInput == Vector2.zero ? States.Idle : States.Move;
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
