// Assets/_Scripts/Core/GameManager.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // 씬 전환용

public enum GameState { MainMenu, Playing, LevelUp, Paused, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState CurrentState { get; private set; }
    public UIManager uiManager;
    public PlayerLauncher playerLauncher;
    public BrickSpawner brickSpawner;
    // public ObjectPooler objectPooler; // Instance로 접근 가능

    public PlayerStats playerStats;
    private UpgradeManager upgradeManager;

    public int currentScore = 0;
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;
    public int gold = 0;

    public float gameOverLineY = -4.5f; // 이 Y좌표 아래로 벽돌이 내려가면 게임 오버

    private List<BallController> activeBalls = new List<BallController>();
    private List<Brick> activeBricks = new List<Brick>(); // BrickSpawner에서도 관리하지만, 게임매니저도 파괴 등을 위해 참조

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // GameManager는 씬 전환 시 유지 (선택적)
            InitializeGameManagers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeGameManagers()
    {
        playerStats = new PlayerStats();
        upgradeManager = new UpgradeManager();
        // uiManager 등은 현재 씬에서 찾아오거나, 시작 시 할당 필요
    }
    void Start()
    {
        // PlayerPrefs에서 골드 불러오기 등 (메타 프로그레션)
        gold = PlayerPrefs.GetInt(Constants.GOLD_KEY, 0);

        // 게임 시작 시 초기 상태 설정 (MainMenu 등)
        // 여기서는 바로 게임 시작하는 것으로 가정
        if (SceneManager.GetActiveScene().name == "GameScene") // GameScene일때만 자동시작
        {
            StartNewGame();
        } else {
            ChangeState(GameState.MainMenu);
        }
    }

    public void StartNewGame()
    {
        currentScore = 0;
        currentLevel = 1;
        currentXP = 0;
        xpToNextLevel = CalculateXpForLevel(currentLevel); // 레벨별 필요 경험치 계산
        playerStats.ResetForNewGame();

        // 기존 오브젝트 정리
        ClearField();

        if(brickSpawner) brickSpawner.enabled = true; // 스포너 활성화
        if(playerLauncher) playerLauncher.enabled = true; // 런처 활성화

        // UI 업데이트
        if (uiManager != null)
        {
            uiManager.UpdateScore(currentScore);
            uiManager.UpdateLevel(currentLevel);
            uiManager.UpdateXP(currentXP, xpToNextLevel);
            uiManager.UpdateGold(gold); // 시작 시 보유 골드
            uiManager.ShowGameUI();
        }
        ChangeState(GameState.Playing);
        // 첫 줄 벽돌 생성 (BrickSpawner의 Start에서 하거나 여기서 명시적 호출)
        if(brickSpawner) brickSpawner.SpawnNewRow();
    }


    void Update()
    {
        // 테스트용 레벨업 키
        if (Input.GetKeyDown(KeyCode.L) && CurrentState == GameState.Playing)
        {
            LevelUp();
        }
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1; // 시간 흐름 정상화
                if(uiManager) uiManager.ShowMainMenu();
                ClearField();
                if(brickSpawner) brickSpawner.enabled = false;
                if(playerLauncher) playerLauncher.enabled = false;
                break;
            case GameState.Playing:
                Time.timeScale = 1;
                if(uiManager) uiManager.HideAllScreens(); // 모든 팝업창 숨김
                if(uiManager) uiManager.ShowGameUI();
                break;
            case GameState.LevelUp:
                Time.timeScale = 0; // 게임 일시 정지
                PresentUpgradeChoices();
                break;
            case GameState.Paused:
                Time.timeScale = 0;
                if(uiManager) uiManager.ShowPauseMenu();
                break;
            case GameState.GameOver:
                Time.timeScale = 1; // 게임오버 화면은 시간이 흐를 수 있도록 (애니메이션 등)
                PlayerPrefs.SetInt(Constants.GOLD_KEY, gold); // 골드 저장
                PlayerPrefs.Save();
                if(uiManager) uiManager.ShowGameOverScreen(currentScore, gold); // 최종 점수, 획득 골드 전달
                 if(brickSpawner) brickSpawner.enabled = false;
                 if(playerLauncher) playerLauncher.enabled = false;
                break;
        }
    }

    public void AddXP(int amount)
    {
        if (CurrentState != GameState.Playing) return;

        currentXP += (int)(amount * playerStats.XPGainModifier);
        if (uiManager != null) uiManager.UpdateXP(currentXP, xpToNextLevel);

        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        currentLevel++;
        currentXP -= xpToNextLevel; // 초과분은 유지
        xpToNextLevel = CalculateXpForLevel(currentLevel);

        if (uiManager != null)
        {
            uiManager.UpdateLevel(currentLevel);
            uiManager.UpdateXP(currentXP, xpToNextLevel);
        }
        ChangeState(GameState.LevelUp);
    }

    int CalculateXpForLevel(int level)
    {
        return 100 + (level -1) * 50; // 간단한 레벨업 필요 경험치 공식
    }

    void PresentUpgradeChoices()
    {
        List<UpgradeCardData> choices = upgradeManager.GetRandomUpgradeChoices(3);
        if (uiManager != null) uiManager.ShowUpgradeScreen(choices);
    }

    public void SelectUpgrade(UpgradeCardData selectedCard)
    {
        upgradeManager.ApplyUpgradeToPlayer(playerStats, selectedCard);
        // PlayerStats가 변경되었으므로, BallController 등 관련 객체에 반영 필요 시 여기서 처리
        // 예: playerLauncher.SetBallsToLaunch(playerStats.BallsToLaunchNext);

        ChangeState(GameState.Playing); // 게임 재개
    }

    public void AddGold(int amount)
    {
        gold += (int)(amount * playerStats.GoldGainModifier);
        if (uiManager != null) uiManager.UpdateGold(gold);
    }

    public void RegisterActiveBall(BallController ball)
    {
        if (!activeBalls.Contains(ball))
        {
            activeBalls.Add(ball);
            // 새로 생성된 공에 현재 플레이어 스탯 적용 (분열 등)
            ball.canSplit = playerStats.HasBallSplit;
            ball.splitChance = playerStats.BallSplitChance;
        }
    }

    public void OnBallDestroyed(BallController ball)
    {
        if (activeBalls.Contains(ball))
        {
            activeBalls.Remove(ball);
        }
        // 모든 공이 파괴되었는지 체크 -> 만약 추가 발사할 공이 없다면 게임오버 조건이 될 수도 있음
        // 여기서는 PlayerLauncher가 다음 발사를 제어하므로, 특별한 처리는 불필요할 수 있음
        CheckAllBallsIdleOrDestroyed();
    }

    public void RegisterActiveBrick(Brick brick)
    {
        if (!activeBricks.Contains(brick))
        {
            activeBricks.Add(brick);
        }
    }
    public void OnBrickDestroyed(Brick brick)
    {
        if (activeBricks.Contains(brick))
        {
            activeBricks.Remove(brick);
        }
        if (brickSpawner != null)
        {
            brickSpawner.RemoveBrickFromList(brick); // Spawner의 리스트에서도 제거
        }
        currentScore += 10 * brick.maxHp; // 벽돌 체력 비례 점수 (예시)
        if (uiManager != null) uiManager.UpdateScore(currentScore);

        // N초마다 마지막 파괴된 벽돌 위치에 폭발 (업그레이드) 같은 로직 여기서 트리거 가능
    }

    public bool AreAllBallsIdleOrDestroyed()
    {
        if (activeBalls.Count == 0) return true;
        foreach (BallController ball in activeBalls)
        {
            // Rigidbody의 속도가 매우 낮으면 멈춘 것으로 간주
            if (ball.gameObject.activeSelf && ball.GetComponent<Rigidbody2D>().linearVelocity.sqrMagnitude > 0.01f)
            {
                return false;
            }
        }
        // 모든 공이 멈췄다면, 다음 턴을 위해 공들을 특정 위치로 모으거나 할 수 있음.
        // 현재 기획에서는 공이 바닥에 닿아도 계속 움직이므로, 이 함수는 주로 "모든 공이 파괴되었는가" 또는
        // "모든 공이 발사 후 벽돌과 상호작용을 마쳤는가(속도가 0에 가까워졌는가)"를 체크하는데 사용 가능.
        // 기획상 공이 계속 움직이므로, '모든 공이 파괴되었거나 화면에 남아있지만 발사 대기 상태'일 때 true를 반환해야 함.
        // 현재는 PlayerLauncher가 발사 제어를 하므로, 단순히 activeBalls.Count == 0만 체크해도 될 수 있음.
        // 좀 더 정확히는, "현재 발사된 공들이 모두 활동을 멈췄거나 파괴되었는가"
        return true; // 임시. 실제로는 속도 체크 필요
    }

     public void CheckAllBallsIdleOrDestroyed() // 이름 변경 및 로직 수정
    {
        if (CurrentState != GameState.Playing) return;

        bool allIdle = true;
        if (activeBalls.Count > 0)
        {
            foreach (BallController ball in activeBalls)
            {
                if (ball.gameObject.activeSelf && ball.GetComponent<Rigidbody2D>().linearVelocity.sqrMagnitude > 0.01f)
                {
                    allIdle = false;
                    break;
                }
            }
        }

        if (allIdle)
        {
            // 모든 공이 멈추거나 파괴됨. 플레이어가 다음 행동을 할 수 있음.
            // 이때 playerLauncher.ballsToLaunchThisTurn을 playerStats.BallsToLaunchNext 기준으로 리셋할 수 있음.
            // (다음 발사 시 공 N개 추가 같은 일회성 버프 처리)
            if (playerLauncher && playerStats != null)
            {
                playerLauncher.SetBallsToLaunch(playerStats.BallsToLaunchNext);
                // 일회성 버프였다면, 사용 후 기본값으로 돌리는 로직 필요
                // 예: if (playerStats.BallsToLaunchNext > playerStats.CurrentMaxActiveBalls) playerStats.BallsToLaunchNext = playerStats.CurrentMaxActiveBalls;
                // 또는 UpgradeManager에서 버프 적용 시 타이머나 플래그로 관리
            }
        }
    }


    public void GameOver()
    {
        if (CurrentState == GameState.GameOver) return; // 중복 호출 방지
        Debug.Log("GAME OVER");
        ChangeState(GameState.GameOver);
        ClearField(); // 게임 필드 정리
    }

    void ClearField()
    {
        // 모든 공 비활성화 (풀로 반환)
        foreach (var ball in activeBalls)
        {
            if(ball != null && ball.gameObject.activeSelf) ball.gameObject.SetActive(false);
        }
        activeBalls.Clear();

        // 모든 벽돌 비활성화 (풀로 반환)
        // BrickSpawner가 직접 관리하는 activeBricks를 사용하거나, GameManager의 리스트 사용
        if (brickSpawner != null) brickSpawner.ClearAllBricks(); // 스포너에게 정리 요청
        activeBricks.Clear(); // GameManager의 리스트도 클리어
    }

    // 특수 벽돌 효과들
    public void HealAllActiveBalls(int amount)
    {
        foreach (BallController ball in activeBalls)
        {
            if (ball.gameObject.activeSelf)
            {
                ball.Heal(amount);
            }
        }
    }

    public void ApplyGlobalBallBuff(BallBuffType buffType, int value, float duration)
    {
        foreach (BallController ball in activeBalls)
        {
            if (ball.gameObject.activeSelf)
            {
                if (buffType == BallBuffType.Damage)
                {
                    ball.ApplyDamageBuff(value, duration); // BallController에 해당 메서드 구현 필요
                }
                else if (buffType == BallBuffType.Speed)
                {
                    // ball.ApplySpeedBuff(value, duration); // BallController에 해당 메서드 구현 필요
                }
            }
        }
    }

    public void UpdateActiveBallsSplitAbility(bool canSplit, float chance)
    {
        foreach (BallController ball in activeBalls)
        {
            if(ball.gameObject.activeSelf)
            {
                ball.canSplit = canSplit;
                ball.splitChance = chance;
            }
        }
    }

    public void OnPlayerAction() // 플레이어가 공을 발사했을 때 호출됨
    {
        // 턴이 진행되었다는 신호.
        // 이때 "다음 발사 시 공 N개 추가" 같은 버프가 있었다면 초기화.
        // PlayerStats.BallsToLaunchNext를 playerStats.CurrentMaxActiveBalls로 리셋.
        playerStats.BallsToLaunchNext = playerStats.CurrentMaxActiveBalls;
        if(playerLauncher) playerLauncher.SetBallsToLaunch(playerStats.BallsToLaunchNext);
    }

    // 메인 메뉴 버튼 등에서 호출
    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene"); // 실제 게임 씬 이름으로 변경
        // GameScene 로드 후 StartNewGame()이 호출되도록 Start() 메서드 수정 필요
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}