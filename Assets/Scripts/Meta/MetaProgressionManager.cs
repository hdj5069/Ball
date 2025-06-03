// Assets/_Scripts/Meta/MetaProgressionManager.cs
using UnityEngine;
using UnityEngine.UI; // UI 요소 연결용 (예시)
using TMPro;          // TextMeshPro 사용 시

public class MetaProgressionManager : MonoBehaviour
{
    // 예시: 시작 시 공 체력 업그레이드
    public Button upgradeStartBallHpButton;
    public TextMeshProUGUI startBallHpLevelText;
    public TextMeshProUGUI startBallHpCostText;
    private int startBallHpLevel = 0;
    private int startBallHpBaseCost = 50;

    // GameManager의 골드 참조 또는 직접 골드 표시 UI 연결
    public TextMeshProUGUI playerGoldText_MetaMenu;


    void Start()
    {
        LoadMetaProgress();
        UpdateUI();

        if(upgradeStartBallHpButton) upgradeStartBallHpButton.onClick.AddListener(UpgradeStartBallHp);
    }

    void LoadMetaProgress()
    {
        startBallHpLevel = PlayerPrefs.GetInt(Constants.META_START_BALL_HP_KEY, 0);
        // 다른 메타 업그레이드들도 여기서 로드
    }

    void SaveMetaProgress()
    {
        PlayerPrefs.SetInt(Constants.META_START_BALL_HP_KEY, startBallHpLevel);
        // 다른 메타 업그레이드 저장
        PlayerPrefs.Save();
    }

    void UpdateUI()
    {
        int currentGold = PlayerPrefs.GetInt(Constants.GOLD_KEY, 0);
        if(playerGoldText_MetaMenu) playerGoldText_MetaMenu.text = "Gold: " + currentGold;

        if(startBallHpLevelText) startBallHpLevelText.text = "Lv. " + startBallHpLevel;
        if(startBallHpCostText) startBallHpCostText.text = "Cost: " + GetUpgradeCost(startBallHpBaseCost, startBallHpLevel);

        if(upgradeStartBallHpButton)
            upgradeStartBallHpButton.interactable = currentGold >= GetUpgradeCost(startBallHpBaseCost, startBallHpLevel);

        // 다른 메타 업그레이드 UI 업데이트
    }

    int GetUpgradeCost(int baseCost, int level)
    {
        return baseCost + level * 25; // 간단한 비용 증가 공식
    }

    public void UpgradeStartBallHp()
    {
        int cost = GetUpgradeCost(startBallHpBaseCost, startBallHpLevel);
        int currentGold = PlayerPrefs.GetInt(Constants.GOLD_KEY, 0);

        if (currentGold >= cost)
        {
            currentGold -= cost;
            PlayerPrefs.SetInt(Constants.GOLD_KEY, currentGold);
            startBallHpLevel++;
            SaveMetaProgress();
            UpdateUI();
            Debug.Log("Upgraded Start Ball HP to level: " + startBallHpLevel);
        }
    }

    // 이 화면을 나갈 때 GameManager의 PlayerStats에 변경된 메타 스탯을 다시 로드하도록 신호를 줄 수 있음.
    // 또는 게임 시작 시 PlayerStats가 PlayerPrefs에서 직접 읽도록. (현재 PlayerStats는 그렇게 구현됨)
}