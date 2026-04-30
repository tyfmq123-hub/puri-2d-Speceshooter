using UnityEngine;
using System.Text;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public StageData StageData { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadStageData();
    }

    void LoadStageData()
    {
        TextAsset json = Resources.Load<TextAsset>("stage_data");
        if (json == null)
        {
            Debug.LogError("[DataManager] stage_data.json을 Resources 폴더에서 찾을 수 없습니다.");
            return;
        }

        
        StageData = JsonUtility.FromJson<StageData>(json.text);

        if (StageData == null || StageData.waves == null)
        {
            Debug.LogError("[DataManager] stage_data 파싱 실패 또는 waves 데이터가 없습니다.");
            return;
        }
        
        Debug.Log($"[DataManager] stage_data 로드 완료 — {StageData.waves.Length}웨이브");

        LogRawJson(json.text);
    }

    void LogRawJson(string rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            Debug.LogWarning("[DataManager] 출력할 JSON 원문이 없습니다.");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("[DataManager] ===== stage_data.json Raw =====");
        sb.AppendLine(rawJson);
        sb.AppendLine("[DataManager] =================================");
        Debug.Log(sb.ToString());
    }
}
