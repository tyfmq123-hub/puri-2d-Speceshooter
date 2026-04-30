using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMain : MonoBehaviour
{
    public static GameMain Instance { get; private set; }

    [Header("Boundary Padding")]
    [SerializeField] private float paddingX = 0.5f;
    [SerializeField] private float paddingY = 0.5f;

    private Camera mainCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCamera = Camera.main;
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera == null || Player.Instance == null)
        {
            return;
        }

        float camHalfHeight = mainCamera.orthographicSize;
        float camHalfWidth = camHalfHeight * mainCamera.aspect;
        Vector3 camPos = mainCamera.transform.position;

        float minX = camPos.x - camHalfWidth + paddingX;
        float maxX = camPos.x + camHalfWidth - paddingX;
        float minY = camPos.y - camHalfHeight + paddingY;
        float maxY = camPos.y + camHalfHeight - paddingY;

        Transform playerTransform = Player.Instance.transform;
        Vector3 pos = playerTransform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        playerTransform.position = pos;
    }
}
