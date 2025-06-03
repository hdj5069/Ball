// Assets/_Scripts/Gameplay/BallController.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class BallController : MonoBehaviour
{
    public int currentHp;
    public int damage;
    public float moveSpeed = 10f; // 초기 발사 속도 또는 유지 속도 (필요에 따라)

    private Rigidbody2D rb;
    private CircleCollider2D col;
    private bool launched = false;

    // 업그레이드 관련 변수
    public bool canSplit = false;
    public float splitChance = 0.1f; // 10%

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();

        // 물리 설정
        rb.gravityScale = 0; // 2D 벽돌깨기에서는 중력 불필요
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 빠른 물체 충돌 정확도 향상
        if (col.sharedMaterial == null)
        {
            PhysicsMaterial2D material = new PhysicsMaterial2D();
            material.bounciness = Constants.BALL_BOUNCINESS;
            material.friction = Constants.BALL_FRICTION;
            col.sharedMaterial = material;
        }
    }

    public void Initialize(int startHp, int startDamage)
    {
        currentHp = startHp;
        damage = startDamage;
        launched = false;
        gameObject.SetActive(true); // 오브젝트 풀링 시 재활성화
    }

    public void Launch(Vector2 direction)
    {
        if (launched) return;
        rb.linearVelocity = direction.normalized * moveSpeed;
        launched = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(Constants.BRICK_TAG))
        {
            Brick brick = collision.gameObject.GetComponent<Brick>();
            if (brick != null)
            {
                brick.TakeDamage(damage);
                TakeDamage(1); // 공 체력 감소

                // 분열 로직 (업그레이드 시)
                if (canSplit && Random.value < splitChance)
                {
                    // TODO: 분열된 작은 공 생성 로직 (새로운 공 생성 또는 ObjectPooler 사용)
                    // 예: GameManager.Instance.SpawnSplitBall(transform.position, rb.velocity.normalized);
                    Debug.Log("Ball Split!");
                }
            }
        }
        else if (collision.gameObject.CompareTag(Constants.WALL_TAG) || collision.gameObject.CompareTag(Constants.BOTTOM_WALL_TAG))
        {
            // 벽이나 바닥(안전지대) 충돌 시 체력 감소 없음
            // 소리 재생 등
        }
        // GameOverLine과의 충돌은 GameManager에서 별도 처리 (Ball이 직접 게임오버를 트리거하지 않음)
    }

    public void TakeDamage(int amount)
    {
        currentHp -= amount;
        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        // TODO: 최대 체력 제한 필요 (PlayerStats 등에서 관리)
        currentHp += amount;
        Debug.Log("Ball healed by " + amount);
    }

    void Die()
    {
        // TODO: 파괴 이펙트 재생
        Debug.Log("Ball Destroyed!");
        GameManager.Instance.OnBallDestroyed(this);
        gameObject.SetActive(false); // 오브젝트 풀링으로 반환
    }

    // 버프 적용 예시
    public void ApplyDamageBuff(int additionalDamage, float duration)
    {
        int originalDamage = damage - additionalDamage; // 버프 중첩 방지를 위해 원래 데미지 저장 필요
        damage += additionalDamage;
        StartCoroutine(RemoveBuffAfterDuration(duration, () => damage = originalDamage)); // 간단한 예시
    }

    private System.Collections.IEnumerator RemoveBuffAfterDuration(float duration, System.Action onComplete)
    {
        yield return new WaitForSeconds(duration);
        onComplete?.Invoke();
    }
}