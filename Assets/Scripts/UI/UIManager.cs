// Assets/_Scripts/UI/UIManager.cs
using UnityEngine;
using TMPro; // TextMeshPro 사용
using UnityEngine.UI; // 기본 UI 요소 사용
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Game UI")]
    public GameObject gameUIPanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public Slider xpSlider;
    public TextMeshProUGUI xpText; // 예: "150/200 XP"
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI waveText; // 현재 웨이브/시간 표시용

    [Header("Level Up UI")]
    public GameObject levelUpPanel;
    public Button[] upgradeCardButtons; // 3개의 카드 버튼
    public TextMeshProUGUI[] upgradeCardTitles;
    public TextMeshProUGUI[] upgradeCardDescs;
    private List<UpgradeCardData> currentUpgradeChoices;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalGoldText;
    public Button retryButton;
    public Button mainMenuButtonGO; // Game Over 화면의 메인메뉴 버튼

    [Header("Main Menu UI")]
    public GameObject mainMenuPanel;
    public Button startGameButton;
    public Button metaUpgradeButton; // 메타 업그레이드 화면으로
    public Button quitButton;

    [Header("Pause Menu UI")]
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button mainMenuButtonPause; // 일시정지 화면의 메인메뉴 버튼


    // [Header("Next Brick Preview UI")]
    // public GameObject nextBrickPreviewPanel;
    // public Image[] nextBrickPreviewImages; // 다음 줄 벽돌 종류/위치 표시

    void Start()
    {
        // 버튼 이벤트 리스너 등록
        for (int i = 0; i < upgradeCardButtons.Length; i++)
        {
            int index = i; // 클로저 문제 방지
            upgradeCardButtons[i].onClick.AddListener(() => OnUpgradeCardSelected(index));
        }

        if(retryButton) retryButton.onClick.AddListener(OnRetryButtonClicked);
        if(mainMenuButtonGO) mainMenuButtonGO.onClick.AddListener(OnMainMenuButtonClicked);

        if(startGameButton) startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        // if(metaUpgradeButton) metaUpgradeButton.onClick.AddListener(OnMetaUpgradeButtonClicked);
        if(quitButton) quitButton.onClick.AddListener(OnQuitButtonClicked);

        if(resumeButton) resumeButton.onClick.AddListener(OnResumeButtonClicked);
        if(mainMenuButtonPause) mainMenuButtonPause.onClick.AddListener(OnMainMenuButtonClicked);


        // 초기 UI 상태
        HideAllScreens(); // 일단 다 숨기고 시작
        if (GameManager.Instance.CurrentState == GameState.MainMenu)
        {
            ShowMainMenu();
        } else if(GameManager.Instance.CurrentState == GameState.Playing)
        {
            ShowGameUI();
        }
    }

    public void UpdateScore(int score)
    {
        if(scoreText) scoreText.text = "Score: " + score;
    }

    public void UpdateLevel(int level)
    {
        if(levelText) levelText.text = "Level: " + level;
    }

    public void UpdateXP(int currentXP, int xpToNextLevel)
    {
        if(xpSlider) xpSlider.value = (float)currentXP / xpToNextLevel;
        if(xpText) xpText.text = $"{currentXP} / {xpToNextLevel} XP";
    }

    public void UpdateGold(int gold)
    {
        if(goldText) goldText.text = "Gold: " + gold;
    }

    public void UpdateWave(int wave)
    {
        if(waveText) waveText.text = "Wave: " + wave;
    }

    // UIManager.cs - ShowUpgradeScreen 메서드 수정 예시
    public void ShowUpgradeScreen(List<UpgradeCardData> choices)
    {
        // HideAllScreens(); // 이 줄을 주석 처리하거나 삭제

        // gameUIPanel을 제외한 다른 주요 패널들을 비활성화
        if(gameOverPanel && gameOverPanel.activeSelf) gameOverPanel.SetActive(false);
        if(mainMenuPanel && mainMenuPanel.activeSelf) mainMenuPanel.SetActive(false);
        if(pauseMenuPanel && pauseMenuPanel.activeSelf) pauseMenuPanel.SetActive(false);
        // 필요하다면 다른 패널들도 여기에 추가

        if (levelUpPanel == null)
        {
            Debug.LogError("LevelUpPanel is not assigned in UIManager!");
            return;
        }
        levelUpPanel.SetActive(true); // 레벨업 패널 활성화
        currentUpgradeChoices = choices;

        for (int i = 0; i < upgradeCardButtons.Length; i++)
        {
            if (upgradeCardButtons[i] == null)
            {
                Debug.LogError($"UpgradeCardButton at index {i} is not assigned!");
                continue; // 다음 버튼으로 넘어감
            }

            if (i < choices.Count)
            {
                upgradeCardButtons[i].gameObject.SetActive(true);
                if (upgradeCardTitles[i] == null)
                {
                    Debug.LogError($"UpgradeCardTitle at index {i} is not assigned!");
                }
                else
                {
                    upgradeCardTitles[i].text = choices[i].cardName;
                }

                if (upgradeCardDescs[i] == null)
                {
                    Debug.LogError($"UpgradeCardDesc at index {i} is not assigned!");
                }
                else
                {
                    upgradeCardDescs[i].text = choices[i].description;
                }
            }
            else
            {
                upgradeCardButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void OnUpgradeCardSelected(int index)
    {
        if (index < currentUpgradeChoices.Count)
        {
            GameManager.Instance.SelectUpgrade(currentUpgradeChoices[index]);
            HideAllScreens(); // 선택 후 업그레이드 화면 숨김
            ShowGameUI();
        }
    }

    // UIManager.cs
    public void ShowGameOverScreen(int finalScore, int earnedGold)
    {
        // HideAllScreens(); // 이 줄을 주석 처리하거나 삭제

        // gameUIPanel을 제외한 다른 주요 패널들을 비활성화 (필요한 경우)
        // 예를 들어, 레벨업 패널이나 일시정지 패널이 떠있었다면 숨겨야 할 수 있습니다.
        if(levelUpPanel && levelUpPanel.activeSelf) levelUpPanel.SetActive(false);
        if(pauseMenuPanel && pauseMenuPanel.activeSelf) pauseMenuPanel.SetActive(false);
        // mainMenuPanel은 게임오버 후 갈 수 있으므로 여기서는 건드리지 않거나,
        // 게임오버 화면에서 메인메뉴 버튼을 누를 때 처리합니다.

        if(gameOverPanel == null)
        {
            Debug.LogError("GameOverPanel is not assigned in UIManager!");
            return;
        }
        gameOverPanel.SetActive(true); // 게임 오버 패널 활성화

        if(finalScoreText) finalScoreText.text = "Final Score: " + finalScore;
        if(finalGoldText) finalGoldText.text = "Gold Earned: " + earnedGold;

        // GameUIPanel은 그대로 유지됩니다.
    }
    public void ShowMainMenu()
    {
        HideAllScreens();
        if(mainMenuPanel) mainMenuPanel.SetActive(true);
    }
    public void ShowGameUI()
    {
        HideAllScreens();
        if(gameUIPanel) gameUIPanel.SetActive(true);
    }

    public void ShowPauseMenu()
    {
        HideAllScreens();
        if(pauseMenuPanel) pauseMenuPanel.SetActive(true);
    }


    public void HideAllScreens()
    {
        if(gameUIPanel) gameUIPanel.SetActive(false);
        if(levelUpPanel) levelUpPanel.SetActive(false);
        if(gameOverPanel) gameOverPanel.SetActive(false);
        if(mainMenuPanel) mainMenuPanel.SetActive(false);
        if(pauseMenuPanel) pauseMenuPanel.SetActive(false);
    }

    // --- Button Click Handlers ---
    void OnRetryButtonClicked()
    {
        GameManager.Instance.StartNewGame(); // 새 게임 시작
    }

    void OnMainMenuButtonClicked()
    {
        GameManager.Instance.ChangeState(GameState.MainMenu);
        // 만약 GameManager가 DontDestroyOnLoad라면 씬을 로드해야함
        // SceneManager.LoadScene("MainMenuScene"); // 또는 GameManager가 직접 처리
    }

    void OnStartGameButtonClicked()
    {
        // GameManager가 DontDestroyOnLoad이고 현재 씬이 GameScene이 아니라면 GameScene 로드
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "GameScene")
        {
            GameManager.Instance.LoadGameScene();
        }
        else // 이미 GameScene이라면
        {
            GameManager.Instance.StartNewGame();
        }
    }

    void OnQuitButtonClicked()
    {
        GameManager.Instance.QuitGame();
    }

    void OnResumeButtonClicked()
    {
        GameManager.Instance.ChangeState(GameState.Playing); // 일시정지 해제
    }

    // public void UpdateNextBrickPreview(List<BrickType> nextBricks) { /* ... */ }
}