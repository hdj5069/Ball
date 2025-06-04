// Assets/_Scripts/Gameplay/Brick.cs
using UnityEngine;
using TMPro; // TextMeshPro 사용 시

public enum BrickType { Normal, Explosive, Coin, XPBoost, Heal, Buff_Damage, Buff_Speed }

public class Brick : MonoBehaviour
{
    public int maxHp = 1;
    public int currentHp;
    public int xpValue = 10;
    public int goldValue = 1; // 코인 벽돌이 아닐 경우 0으로 설정 가능

    public BrickType brickType = BrickType.Normal;

    // 특수 벽돌 관련 설정
    public float explosionRadius = 1.5f; // 폭발 벽돌용
    public int explosionDamage = 10;   // 폭발 벽돌용
    public int healAmount = 1;         // 회복 벽돌용
    public float buffDuration = 5f;    // 버프 벽돌용
    public int buffValue = 2;          // 버프 벽돌용 (데미지 증가량 등)


    [SerializeField] private SpriteRenderer spriteRenderer; // 색상 변경 등 시각적 표현
    [SerializeField] private TextMeshPro hpText; // 체력 표시용 (선택)
    // TODO: 파괴 이펙트 프리팹 연결

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(int hp, int xp, int gold, BrickType type = BrickType.Normal)
    {
        maxHp = hp;
        currentHp = hp;
        xpValue = xp;
        goldValue = gold;
        brickType = type;
        UpdateVisuals();
        gameObject.SetActive(true);
    }

    public void TakeDamage(int damageAmount)
    {
        currentHp -= damageAmount;
        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            UpdateVisuals();
            // TODO: 피격 이펙트/사운드
        }
    }

    void Die()
    {
        // TODO: 파괴 이펙트/사운드 재생
        // GameManager에 알림
        DIContainer.Resolve<GameManager>().AddXP(xpValue);
        if (brickType == BrickType.Coin || goldValue > 0) // 코인 벽돌 또는 일반 벽돌도 골드 지급 가능
        {
            DIContainer.Resolve<GameManager>().AddGold(goldValue);
        }

        HandleSpecialEffectOnDeath();

        DIContainer.Resolve<GameManager>().OnBrickDestroyed(this);
        gameObject.SetActive(false); // 오브젝트 풀링
    }

    void HandleSpecialEffectOnDeath()
    {
        switch (brickType)
        {
            case BrickType.Explosive:
                Explode();
                break;
            case BrickType.Heal:
                DIContainer.Resolve<GameManager>().HealAllActiveBalls(healAmount);
                break;
            case BrickType.Buff_Damage:
                DIContainer.Resolve<GameManager>().ApplyGlobalBallBuff(BallBuffType.Damage, buffValue, buffDuration);
                break;
            case BrickType.Buff_Speed:
                DIContainer.Resolve<GameManager>().ApplyGlobalBallBuff(BallBuffType.Speed, (int)buffValue, buffDuration); // 속도는 float이지만 편의상 int로
                break;
            case BrickType.XPBoost:
                // XP는 이미 AddXP에서 처리됨 (xpValue를 높게 설정)
                break;
        }
    }

    void Explode()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in colliders)
        {
            if (hit.CompareTag(Constants.BRICK_TAG))
            {
                Brick nearbyBrick = hit.GetComponent<Brick>();
                if (nearbyBrick != null && nearbyBrick != this)
                {
                    nearbyBrick.TakeDamage(explosionDamage);
                }
            }
        }
        // TODO: 폭발 이펙트
        Debug.Log("Brick Exploded!");
    }


    void UpdateVisuals()
    {
        if (hpText != null)
        {
            hpText.text = currentHp.ToString();
        }
        // 예시: 체력에 따라 색상 변경
        if (spriteRenderer != null)
        {
            // 간단한 예시: 최대 체력 대비 현재 체력 비율로 색상 변경 (흰색 -> 빨강)
            // float healthRatio = (float)currentHp / maxHp;
            // spriteRenderer.color = Color.Lerp(Color.red, Color.white, healthRatio);
            // 또는 미리 정의된 색상 사용
        }
    }

    void OnBecameInvisible()
    {
        // 화면 밖으로 나갔을 때 (최하단 도달 전에) 비활성화 (오브젝트 풀링 반환 로직 추가 가능)
        // 단, GameManager에서 최하단 도달 체크를 하고 있으므로, 여기서는 중복될 수 있음.
        // 필요에 따라 사용
    }
}

public enum BallBuffType { Damage, Speed }