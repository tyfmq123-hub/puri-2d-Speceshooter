using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    //#. 싱글톤
    public static UIManager Instance;

    //#. 목숨 아이콘 이미지 배열 (Life_0, Life_1, Life_2)
    public Image[] images;

    //#. GAME OVER 패널
    public GameObject gameOverPanel;

    //#. Retry 버튼
    public Button retryButton;

    //#. 점수 텍스트
    public TextMeshProUGUI scoreText;

    //#. 붐 이미지 배열
    public Image[] boomImages;
    private const int MaxBoomCount = 3;
    private int boomCount = 0;

    //#. 점수
    private int score;
    public int CurrentBoomCount => boomCount;
    public int MaxBoom => MaxBoomCount;

    //#. 라이프 감소 중복 차단용 플래그
    private bool isProcessingHit = false;

    void Awake()
    {
        //#. 싱글톤 중복 방지
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
        //#. UI 참조 바인딩
        AutoBindUIReferences();

        //#. Retry 버튼 리스너 등록
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryButtonClick);

        //#. UI 초기화
        InitializeUI();
        UpdateScoreText();
        UpdateBoomUI();
    }

    private void OnDestroy()
    {
        //#. Retry 버튼 리스너 해제
        if (retryButton != null)
            retryButton.onClick.RemoveListener(OnRetryButtonClick);
    }

    //#. 목숨 1개 감소 (반환값: true = 게임오버, false = 목숨 남음)
    public bool DecreaseLife()
    {
        //#. 이미지 없으면 게임오버 처리
        if (images == null || images.Length == 0)
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
            return true;
        }

        //#. 현재 남은 목숨 수 계산
        int visibleLifeCount = 0;
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].color.a > 0f)
                visibleLifeCount++;
        }

        //#. 뒤에서부터 목숨 아이콘 하나 숨기기
        for (int i = images.Length - 1; i >= 0; i--)
        {
            if (images[i] == null) continue;

            Color color = images[i].color;
            if (color.a > 0f)
            {
                //#. 알파 0으로 숨기기
                color.a = 0f;
                images[i].color = color;

                //#. 마지막 목숨이었으면 게임오버
                if (visibleLifeCount == 1)
                {
                    if (gameOverPanel != null)
                        gameOverPanel.SetActive(true);
                    return true;
                }

                return false;
            }
        }

        //#. 목숨 아이콘 못 찾으면 게임오버
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        return true;
    }

    //#. Player.TakeDamage()에서 직접 호출
    public void ApplyPlayerDamage(Player player, int amount)
    {
        //#. 중복 피격 차단
        if (isProcessingHit) return;
        if (player == null) return;

        //#. 피격 처리 시작
        isProcessingHit = true;

        int appliedDamage = Mathf.Max(1, amount);
        bool isGameOver = false;

        //#. 데미지만큼 라이프 감소
        for (int i = 0; i < appliedDamage; i++)
        {
            isGameOver = DecreaseLife();
            if (isGameOver) break;
        }

        //#. 플레이어 라이프 동기화
        player.life = Mathf.Max(0, player.life - appliedDamage);

        //#. 피격 피드백 전달
        player.ApplyDamageFeedback(survived: !isGameOver);

        //#. 피격 처리 완료
        isProcessingHit = false;
    }

    //#. 플레이어 피격 처리 (총알 등 외부에서 호출)
    public void HandlePlayerHit(GameObject playerGo, Vector3 respawnPosition, float respawnDelay, int damageAmount = 1)
    {
        //#. 중복 피격 차단
        if (isProcessingHit) return;
        if (playerGo == null) return;

        Player player = playerGo.GetComponent<Player>();

        //#. 무적 상태면 무시
        if (player != null && player.IsInvincible) return;

        //#. 피격 처리 시작
        isProcessingHit = true;

        int appliedDamage = Mathf.Max(1, damageAmount);
        bool isGameOver = false;

        //#. 데미지만큼 라이프 감소
        for (int i = 0; i < appliedDamage; i++)
        {
            isGameOver = DecreaseLife();
            if (isGameOver) break;
        }

        //#. 플레이어 라이프 동기화
        if (player != null)
            player.life = Mathf.Max(0, player.life - appliedDamage);

        //#. 플레이어 비활성화
        playerGo.SetActive(false);

        //#. 피격 처리 완료
        isProcessingHit = false;

        //#. 게임오버면 리스폰 안 함
        if (isGameOver) return;

        //#. 리스폰 코루틴 시작
        StartCoroutine(RespawnPlayer(playerGo, respawnPosition, respawnDelay));
    }

    //#. 일정 시간 후 플레이어 리스폰
    private System.Collections.IEnumerator RespawnPlayer(GameObject playerGo, Vector3 respawnPosition, float respawnDelay)
    {
        //#. 오브젝트 없으면 중단
        if (playerGo == null) yield break;

        //#. 리스폰 딜레이 대기
        yield return new WaitForSeconds(respawnDelay);

        //#. 대기 중 오브젝트 사라지면 중단
        if (playerGo == null) yield break;

        //#. 게임오버 상태면 리스폰 안 함
        if (gameOverPanel != null && gameOverPanel.activeSelf) yield break;

        //#. 리스폰 위치로 이동 후 활성화
        playerGo.transform.position = respawnPosition;
        playerGo.SetActive(true);

        //#. 리스폰 무적 적용
        Player player = playerGo.GetComponent<Player>();
        player?.ActivateRespawnInvincibility(2f);
    }

    //#. Retry 버튼 클릭 시 현재 씬 재시작
    public void OnRetryButtonClick()
    {
        //#. 1. 모든 코루틴 중지
        StopAllCoroutines();

        //#. 2. 피격 처리 플래그 초기화
        isProcessingHit = false;

        //#. 3. UI 상태 초기화
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        //#. 4. 점수 초기화
        score = 0;
        UpdateScoreText();

        //#. 5. 라이프 초기화
        InitializeUI();

        //#. 6. 붐 초기화
        boomCount = 0;
        UpdateBoomUI();

        //#. 7. 씬 재시작 로그
        Debug.Log("[UIManager] Retry - 상태 초기화 완료 / 씬 재시작");

        //#. 8. 씬 재시작 (가장 마지막에 실행)
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    //#. 적 타입에 따라 점수 추가
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

    //#. 점수 텍스트 업데이트 (천 단위 콤마 적용)
    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = score.ToString("#,##0");
    }

    //#. 아이템 생성
    public void CreateItem(Vector3 pos)
    {
        if (PoolManager.Instance == null) return;

        //#. None 30% / Coin 30% / Power 30% / Boom 10%
        int rand = Random.Range(0, 10);
        if (rand < 3) return;

        GameObject item = null;
        if (rand < 6)      item = PoolManager.Instance.GetCoinItem();
        else if (rand < 9) item = PoolManager.Instance.GetPowerItem();
        else               item = PoolManager.Instance.GetBoomItem();

        if (item == null) return;

        //#. 아이템 위치 설정 후 활성화
        item.transform.position = pos;
        item.SetActive(true);
        item.GetComponent<Item>()?.BeginMove();
    }

    //#. 점수 직접 추가
    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    //#. 붐 추가
    public void AddBoom()
    {
        if (boomCount >= MaxBoomCount) return;
        boomCount++;
        UpdateBoomUI();
    }

    //#. 붐 사용
    public bool UseBoom()
    {
        if (boomCount <= 0) return false;
        boomCount--;
        UpdateBoomUI();
        return true;
    }

    //#. 붐 UI 업데이트
    private void UpdateBoomUI()
    {
        if (boomImages == null) return;

        for (int i = 0; i < boomImages.Length; i++)
        {
            if (boomImages[i] == null) continue;

            //#. boomCount보다 작은 인덱스만 보이게
            Color color = boomImages[i].color;
            color.a = i < boomCount ? 1f : 0f;
            boomImages[i].color = color;
        }
    }

    //#. UI 초기화 (라이프 전부 보이게, 게임오버 패널 숨기기)
    private void InitializeUI()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (images != null)
        {
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] == null) continue;

                Color color = images[i].color;
                color.a = 1f;
                images[i].color = color;
            }
        }
    }

    //#. UI 참조 자동 바인딩
    private void AutoBindUIReferences()
    {
        //#. 게임오버 패널 찾기
        if (gameOverPanel == null)
        {
            GameObject foundPanel = GameObject.Find("GameOverPanel") ?? GameObject.Find("GAME OVER") ?? GameObject.Find("GameOver");
            if (foundPanel != null)
                gameOverPanel = foundPanel;
        }

        //#. Retry 버튼 찾기
        if (retryButton == null)
        {
            GameObject foundRetry = GameObject.Find("RetryButton") ?? GameObject.Find("Retry");
            if (foundRetry != null)
                retryButton = foundRetry.GetComponent<Button>();
        }

        //#. 점수 텍스트 찾기
        if (scoreText == null)
        {
            GameObject foundScore = GameObject.Find("ScoreText") ?? GameObject.Find("Score");
            if (foundScore != null)
                scoreText = foundScore.GetComponent<TextMeshProUGUI>();
        }

        //#. 라이프 이미지 찾기
        if (images == null || images.Length == 0)
        {
            images = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject foundLife = GameObject.Find($"Life_{i}");
                if (foundLife != null)
                    images[i] = foundLife.GetComponent<Image>();
            }
        }

        //#. 붐 이미지 찾기
        if (boomImages == null || boomImages.Length == 0)
        {
            boomImages = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject foundBoom = GameObject.Find($"BoomImage_{i}");
                if (foundBoom != null)
                    boomImages[i] = foundBoom.GetComponent<Image>();
            }
        }
    }
}