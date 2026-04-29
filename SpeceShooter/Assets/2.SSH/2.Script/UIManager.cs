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

    private int score;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryButtonClick);

        UpdateScoreText();
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
                    gameOverPanel.SetActive(true);
                    return true;
                }

                return false;
            }
        }

        gameOverPanel.SetActive(true);
        return true;
    }

    // 플레이어 피격 처리
    public void HandlePlayerHit(GameObject playerGo, Vector3 respawnPosition, float respawnDelay)
    {
        if (playerGo == null) return;

        bool isGameOver = DecreaseLife();
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
}