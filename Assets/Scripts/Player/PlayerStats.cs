// Assets/_Scripts/Player/PlayerStats.cs
using UnityEngine;
using System.Collections.Generic;

public class PlayerStats
{
    // 기본 스탯 (메타 프로그레션으로 강화 가능)
    private int baseBallHp = 3;
    private int baseBallDamage = 1;
    private int baseBallsPerTurn = 1; // 한 번에 발사하는 기본 공 개수

    // 현재 게임 내 스탯 (업그레이드로 변화)
    public int CurrentBallHp { get; private set; }
    public int CurrentBallDamage { get; private set; }
    public int CurrentMaxActiveBalls { get; private set; } // 필드에 존재 가능한 최대 공 (멀티볼 업그레이드)
    public int BallsToLaunchNext { get; set; } // "다음 발사 시 공 N개 추가" 같은 임시 버프용

    public float XPGainModifier { get; private set; } = 1f;
    public float GoldGainModifier { get; private set; } = 1f;

    // 업그레이드로 획득한 특수 능력
    public bool HasBallSplit { get; private set; } = false;
    public float BallSplitChance { get; private set; } = 0f;
    // ... 기타 등등

    public PlayerStats()
    {
        LoadMetaUpgrades(); // 메타 업그레이드 불러오기
        ResetForNewGame();
    }

    public void LoadMetaUpgrades()
    {
        baseBallHp += PlayerPrefs.GetInt(Constants.META_START_BALL_HP_KEY, 0);
        baseBallDamage += PlayerPrefs.GetInt(Constants.META_START_BALL_DMG_KEY, 0);
        // ... 기타 메타 스탯 로드
    }

    public void ResetForNewGame()
    {
        CurrentBallHp = baseBallHp;
        CurrentBallDamage = baseBallDamage;
        CurrentMaxActiveBalls = baseBallsPerTurn; // 초기에는 기본 발사 수만큼만 필드에 존재
        BallsToLaunchNext = baseBallsPerTurn;

        XPGainModifier = 1f + PlayerPrefs.GetFloat("MetaXPGain", 0f); // 예시
        GoldGainModifier = 1f + PlayerPrefs.GetFloat("MetaGoldGain", 0f);

        HasBallSplit = false;
        BallSplitChance = 0f;
    }

    public int GetInitialBallsPerTurn()
    {
        return baseBallsPerTurn; // 메타 업그레이드로 시작 시 공 개수 늘릴 수 있음
    }


    // 업그레이드 적용 메서드들
    public void ApplyUpgrade(UpgradeEffect effect, float value)
    {
        switch (effect)
        {
            case UpgradeEffect.BallHpUp:
                CurrentBallHp += (int)value;
                break;
            case UpgradeEffect.BallDamageUp:
                CurrentBallDamage += (int)value;
                break;
            case UpgradeEffect.IncreaseMaxActiveBalls: // 영구적으로 필드에 공 추가 (해당 런 한정)
                CurrentMaxActiveBalls += (int)value;
                BallsToLaunchNext = CurrentMaxActiveBalls; // 다음 발사 시 적용되도록
                // PlayerLauncher에도 알려야 함. DIContainer를 통해 참조를 얻음.
                DIContainer.Resolve<PlayerLauncher>().SetBallsToLaunch(BallsToLaunchNext);
                break;
            case UpgradeEffect.AddBallsNextLaunch: // 일회성으로 다음 발사에 공 추가
                BallsToLaunchNext += (int)value;
                DIContainer.Resolve<PlayerLauncher>().SetBallsToLaunch(BallsToLaunchNext);
                break;
            case UpgradeEffect.XPGainUp:
                XPGainModifier += value; // value는 퍼센트이므로 0.1f (10%) 등으로 전달
                break;
            case UpgradeEffect.GoldGainUp:
                GoldGainModifier += value;
                break;
            case UpgradeEffect.EnableBallSplit:
                HasBallSplit = true;
                BallSplitChance = value; // value는 확률 (0.0 ~ 1.0)
                // BallController에 이 정보를 전달해야 함. GameManager를 통해 모든 활성 공에게 전달하거나,
                // 공 생성 시 PlayerStats를 참조하도록.
                DIContainer.Resolve<GameManager>().UpdateActiveBallsSplitAbility(HasBallSplit, BallSplitChance);
                break;
            // ... 기타 업그레이드
        }
        Debug.Log($"Upgrade Applied: {effect}, Value: {value}. Current Dmg: {CurrentBallDamage}, HP: {CurrentBallHp}");
    }

    public int GetCurrentBallHp() => CurrentBallHp;
    public int GetCurrentBallDamage() => CurrentBallDamage;
}

// 업그레이드 카드에 사용될 효과 타입 정의
public enum UpgradeEffect
{
    BallHpUp, BallDamageUp, BallSizeUp, BallSizeDown,
    IncreaseMaxActiveBalls, // 영구적으로 필드 위의 공 개수 증가 (이번 판 한정)
    AddBallsNextLaunch,   // 다음 발사 시에만 공 개수 추가
    EnableBallSplit,
    HealLastBrickExplosion, // N초마다 마지막 파괴된 벽돌 위치에 폭발
    BrickDestroyHealBall,
    XPGainUp, GoldGainUp,
    // ... 더 많은 효과들
}