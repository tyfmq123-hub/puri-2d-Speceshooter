using UnityEngine;
using System.Collections;

public class Follower : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private float followSpeed = 15f;
    [SerializeField] private float followDistance = 1.2f;
    [SerializeField] private int followerIndex;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 2f;

    private bool isWaiting = false;

    public void SetFollowerIndex(int index)
    {
        followerIndex = index;
    }

    public void Fire()
    {
        if (isWaiting) return;
        StartCoroutine(FireRoutine());
    }

    private IEnumerator FireRoutine()
    {
        isWaiting = true;
        SpawnBullet();
        yield return new WaitForSeconds(attackCooldown);
        isWaiting = false;
    }

    private void SpawnBullet()
    {
        if (PoolManager.Instance != null)
        {
            GameObject bullet = PoolManager.Instance.GetPlayerBullet01();
            if (bullet != null)
            {
                bullet.transform.position = transform.position;
                bullet.transform.rotation = Quaternion.identity;

                PlayerBullet pb = bullet.GetComponent<PlayerBullet>();
                if (pb != null && PlayerBulletManager.Instance != null)
                    pb.speed = PlayerBulletManager.Instance.bulletSpeed;

                bullet.SetActive(true);
                return;
            }
        }

        // 풀 없으면 PlayerBulletManager 단발 발사 위임
        PlayerBulletManager.Instance?.FireSingleAt((Vector2)transform.position);
    }

    void Update()
    {
        var player = Player.Instance;
        if (player == null) return;

        if (player.state == Player.States.Dead)
        {
            Destroy(gameObject);
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
