using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // 싱글톤: 어디서든 UIManager.Instance로 접근
    public static UIManager Instance;

    // 목숨 아이콘 이미지 배열 (Life_0, Life_1, Life_2)
    public Image[] images;

    // GAME OVER 패널
    public GameObject gameOverPanel;

    // Retry 버튼
    public Button retryButton;

    // 점수 텍스트
    public TextMeshProUGUI scoreText;

    // 붐 이미지 배열 (BoomImage_0, BoomImage_1, BoomImage_2)
    public Image[] boomImages;
    private const int MaxBoomCount = 3;
    private int boomCount = 0;

    private int score;
    public int CurrentBoomCount => boomCount;
    public int MaxBoom => MaxBoomCount;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        AutoBindUIReferences();
    }

    void Start()
    {
        AutoBindUIReferences();

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryButtonClick);

        InitializeUI();
        UpdateScoreText();
        UpdateBoomUI();
    }

    private void OnDestroy()
    {
        if (retryButton != null)
            retryButton.onClick.RemoveListener(OnRetryButtonClick);
    }

    // 목숨 1개 감소
    // 반환값: true = 게임오버, false = 아직 목숨 남음
    public bool DecreaseLife()
    {
        if (images == null || images.Length == 0)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
            return true;
        }

        int visibleLifeCount = 0;
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].color.a > 0f)
                visibleLifeCount++;
        }

        for (int i = images.Length - 1; i >= 0; i--)
        {
            if (images[i] == null) continue;

            Color color = images[i].color;
            if (color.a > 0f)
            {
                // Alpha를 0으로 만들어서 안보이게 함
                color.a = 0f;
                images[i].color = color;

                if (visibleLifeCount == 1)
                {
                    if (gameOverPanel != null)
                    {
                        gameOverPanel.SetActive(true);
                    }
                    return true;
                }

                return false;
            }
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        return true;
    }

    // Player.TakeDamage()에서 직접 호출 — 데미지 적용 후 Player에 피드백 위임
    public void ApplyPlayerDamage(Player player, int amount)
    {
        if (player == null) return;

        int appliedDamage = Mathf.Max(1, amount);
        bool isGameOver = false;

        for (int i = 0; i < appliedDamage; i++)
        {
            isGameOver = DecreaseLife();
            if (isGameOver) break;
        }

        player.life = Mathf.Max(0, player.life - appliedDamage);
        player.ApplyDamageFeedback(survived: !isGameOver);
    }

    // 플레이어 피격 처리
    public void HandlePlayerHit(GameObject playerGo, Vector3 respawnPosition, float respawnDelay, int damageAmount = 1)
    {
        if (playerGo == null) return;

        Player player = playerGo.GetComponent<Player>();
        if (player != null && player.IsInvincible) return;
        int appliedDamage = Mathf.Max(1, damageAmount);
        bool isGameOver = false;
        for (int i = 0; i < appliedDamage; i++)
        {
            isGameOver = DecreaseLife();
            if (isGameOver) break;
        }

        if (player != null)
        {
            player.life = Mathf.Max(0, player.life - appliedDamage);
        }

        playerGo.SetActive(false);

        if (isGameOver) return;

        StartCoroutine(RespawnPlayer(playerGo, respawnPosition, respawnDelay));
    }

    // 일정 시간 후 플레이어 리스폰
    private System.Collections.IEnumerator RespawnPlayer(GameObject playerGo, Vector3 respawnPosition, float respawnDelay)
    {
        if (playerGo == null) yield break;

        yield return new WaitForSeconds(respawnDelay);

        if (playerGo == null) yield break;
        if (gameOverPanel != null && gameOverPanel.activeSelf) yield break;

        playerGo.transform.position = respawnPosition;
        playerGo.SetActive(true);

        Player player = playerGo.GetComponent<Player>();
        player?.ActivateRespawnInvincibility(2f);
    }

    // Retry 버튼 클릭 시 현재 씬 재시작
    public void OnRetryButtonClick()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    // 적 타입에 따라 점수 추가
    public void AddScoreByEnemyType(Enemy.EnemyType enemyType)
    {
        switch (enemyType)
        {
            case Enemy.EnemyType.A: score += 100; break;
            case Enemy.EnemyType.B: score += 200; break;
            case Enemy.EnemyType.C: score += 300; break;
        }

        UpdateScoreText();
    }

    // 점수 텍스트 업데이트 (천 단위 콤마 적용)
    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = score.ToString("#,##0");
    }
    public void CreateItem(Vector3 pos)
    {
        if (PoolManager.Instance == null) return;

        // None 30% / Coin 30% / Power 30% / Boom 10%
        int rand = Random.Range(0, 10);
        if (rand < 3) return;

        GameObject item = null;
        if (rand < 6)      item = PoolManager.Instance.GetCoinItem();
        else if (rand < 9) item = PoolManager.Instance.GetPowerItem();
        else               item = PoolManager.Instance.GetBoomItem();

        if (item == null) return;

        item.transform.position = pos;
        item.SetActive(true);
        item.GetComponent<Item>()?.BeginMove();
    }
    
    // 점수 직접 추가
    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    public void AddBoom()
    {
        if (boomCount >= MaxBoomCount) return;
        boomCount++;
        UpdateBoomUI();
    }

    public bool UseBoom()
    {
        if (boomCount <= 0) return false;
        boomCount--;
        UpdateBoomUI();
        return true;
    }

    private void UpdateBoomUI()
    {
        if (boomImages == null)
        {
            return;
        }

        for (int i = 0; i < boomImages.Length; i++)
        {
            if (boomImages[i] == null)
            {
                continue;
            }

            // boomCount보다 작은 인덱스만 보이게
            Color color = boomImages[i].color;
            color.a = i < boomCount ? 1f : 0f;
            boomImages[i].color = color;
        }
    }

    private void InitializeUI()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (images != null)
        {
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] == null)
                {
                    continue;
                }

                Color color = images[i].color;
                color.a = 1f;
                images[i].color = color;
            }
        }
    }

    private void AutoBindUIReferences()
    {
        if (gameOverPanel == null)
        {
            GameObject foundPanel = GameObject.Find("GameOverPanel") ?? GameObject.Find("GAME OVER") ?? GameObject.Find("GameOver");
            if (foundPanel != null)
            {
                gameOverPanel = foundPanel;
            }
        }

        if (retryButton == null)
        {
            GameObject foundRetry = GameObject.Find("RetryButton") ?? GameObject.Find("Retry");
            if (foundRetry != null)
            {
                retryButton = foundRetry.GetComponent<Button>();
            }
        }

        if (scoreText == null)
        {
            GameObject foundScore = GameObject.Find("ScoreText") ?? GameObject.Find("Score");
            if (foundScore != null)
            {
                scoreText = foundScore.GetComponent<TextMeshProUGUI>();
            }
        }

        if ((images == null || images.Length == 0))
        {
            images = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject foundLife = GameObject.Find($"Life_{i}");
                if (foundLife != null)
                {
                    images[i] = foundLife.GetComponent<Image>();
                }
            }
        }

        if ((boomImages == null || boomImages.Length == 0))
        {
            boomImages = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject foundBoom = GameObject.Find($"BoomImage_{i}");
                if (foundBoom != null)
                {
                    boomImages[i] = foundBoom.GetComponent<Image>();
                }
            }
        }
    }
}