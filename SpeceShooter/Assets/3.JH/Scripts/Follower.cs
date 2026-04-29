using UnityEngine;

public class Follower : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private float followSpeed = 15f;
    [SerializeField] private float followDistance = 1.2f;
    [SerializeField] private int followerIndex;

    [Header("Attack")]
    [SerializeField] private GameObject followerBulletPrefab;
    [SerializeField] private float attackCooldown = 0.3f;

    private float lastAttackTime = -999f;

    public void SetFollowerIndex(int index)
    {
        followerIndex = index;
    }

    public void Fire(GameObject fallbackBulletPrefab, Quaternion rotation)
    {
        if (Time.time - lastAttackTime < attackCooldown)
        {
            return;
        }

        GameObject prefabToUse = followerBulletPrefab != null ? followerBulletPrefab : fallbackBulletPrefab;
        if (prefabToUse == null)
        {
            return;
        }

        lastAttackTime = Time.time;
        Instantiate(prefabToUse, transform.position, rotation);
    }

    void Update()
    {
        var player = Player.Instance;
        if (player == null)
        {
            return;
        }

        float dist = Mathf.Max(followDistance, 1.2f);
        Vector3 target = player.GetHistoryPosition(dist * (followerIndex + 1));
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            followSpeed * Time.deltaTime
        );
    }
}
