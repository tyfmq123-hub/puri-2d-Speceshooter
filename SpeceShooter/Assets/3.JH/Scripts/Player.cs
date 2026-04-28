using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public enum States
    {
        Idle,
        Move,
        Attack
    }
    public int life = 3; // 한대 맞으면 1씩 줄음
    public int attackPoint;
    public int power; // 총알 파워
    public int boomSlot; //가지고 있는 폭탄 갯수 최대치

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 0.15f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    private const string MoveStateParam = "State";
    
    
    States state = States.Idle;
    private Animator animator;
    private Vector2 moveInput;
    private float lastAttackTime;
    private int moveStateHash;

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
    }

    // Update is called once per frame
    void Update()
    {
        //움직임 기능
        //방향키로 이동 대각선 포함
        //애니메이션 구현 Player_Idle,Player_Left,Player_Right
        HandleMovement();
        HandleAttack();
        UpdateAnimation();
    }

    private void HandleMovement()
    {
        float horizontal = 0f;
        float vertical = 0f;

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

        moveInput = new Vector2(horizontal, vertical).normalized;
        transform.position += (Vector3)(moveInput * moveSpeed * Time.deltaTime);
    }

    private void HandleAttack()
    {
        if (!Input.GetKey(KeyCode.Space))
        {
            if (state == States.Attack)
            {
                state = moveInput == Vector2.zero ? States.Idle : States.Move;
            }
            return;
        }

        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;
        state = States.Attack;
        attackPoint = power;
        SpawnBullet();
    }

    private void SpawnBullet()
    {
        if (bulletPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        Quaternion spawnRotation = firePoint != null ? firePoint.rotation : transform.rotation;

        Instantiate(bulletPrefab, spawnPosition, spawnRotation);
    }

    private void UpdateAnimation()
    {
        if (moveInput == Vector2.zero && state != States.Attack)
        {
            state = States.Idle;
        }
        else if (moveInput != Vector2.zero && state != States.Attack)
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
