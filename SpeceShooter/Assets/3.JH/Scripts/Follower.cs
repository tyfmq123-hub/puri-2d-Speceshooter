using UnityEngine;

public class Follower : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private float followSpeed = 15f;
    [SerializeField] private float followDistance = 1.2f;
    [SerializeField] private int followerIndex;

    public void SetFollowerIndex(int index)
    {
        followerIndex = index;
    }

    public void Fire()
    {
        SpawnBullet();
    }

    private void SpawnBullet()
    {
        if (PoolManager.Instance == null) return;

        GameObject bullet = PoolManager.Instance.GetPlayerBullet01();
        if (bullet == null) return;

        PlayerBullet pb = bullet.GetComponent<PlayerBullet>();
        if (pb != null)
        {
            pb.speed = PlayerBulletManager.Instance != null ? PlayerBulletManager.Instance.bulletSpeed : 10f;
            pb.damage = PlayerBulletManager.Instance != null ? PlayerBulletManager.Instance.bullet01Damage : 10;

            if (Player.Instance != null && Player.Instance.power >= 4)
            {
                Sprite blue = PlayerBulletManager.Instance?.bullet03CenterSprite;
                if (blue != null)
                {
                    pb.SetSprite(blue);
                    pb.damage = 20;
                }
            }
        }

        bullet.transform.position = transform.position;
        bullet.transform.rotation = Quaternion.identity;
        bullet.SetActive(true);
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
