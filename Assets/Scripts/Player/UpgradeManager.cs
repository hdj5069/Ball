// Assets/_Scripts/Player/UpgradeManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Linq 사용

public class UpgradeManager
{
    private List<UpgradeCardData> allAvailableCards;
    private List<UpgradeCardData> commonCards;
    private List<UpgradeCardData> rareCards;
    private List<UpgradeCardData> epicCards;

    public UpgradeManager()
    {
        LoadAllUpgradeCards(); // 실제로는 DB, JSON, ScriptableObjects 등에서 로드
    }

    void LoadAllUpgradeCards()
    {
        allAvailableCards = new List<UpgradeCardData>();
        // 예시 카드 데이터 (실제로는 더 많이, 다양하게)
        allAvailableCards.Add(new UpgradeCardData("공 체력 증가", "+2 공 체력", CardRarity.Common,
            new List<UpgradeEffectValue> { new UpgradeEffectValue(UpgradeEffect.BallHpUp, 2) }));
        allAvailableCards.Add(new UpgradeCardData("공 데미지 증가", "+1 공 데미지", CardRarity.Common,
            new List<UpgradeEffectValue> { new UpgradeEffectValue(UpgradeEffect.BallDamageUp, 1) }));

        allAvailableCards.Add(new UpgradeCardData("멀티볼!", "다음 발사 시 공 1개 추가", CardRarity.Rare,
            new List<UpgradeEffectValue> { new UpgradeEffectValue(UpgradeEffect.AddBallsNextLaunch, 1) }));
        allAvailableCards.Add(new UpgradeCardData("영구 멀티볼!", "최대 발사 공 +1 (이번 판)", CardRarity.Epic,
            new List<UpgradeEffectValue> { new UpgradeEffectValue(UpgradeEffect.IncreaseMaxActiveBalls, 1) }));

        allAvailableCards.Add(new UpgradeCardData("분열하는 공", "벽돌 충돌 시 10% 확률로 공 분열", CardRarity.Rare,
            new List<UpgradeEffectValue> { new UpgradeEffectValue(UpgradeEffect.EnableBallSplit, 0.1f) }));

        allAvailableCards.Add(new UpgradeCardData("XP 부스트", "XP 획득량 20% 증가", CardRarity.Common,
            new List<UpgradeEffectValue> { new UpgradeEffectValue(UpgradeEffect.XPGainUp, 0.2f) }));


        // 등급별로 분류 (가중치 랜덤 선택을 위함)
        commonCards = allAvailableCards.Where(card => card.rarity == CardRarity.Common).ToList();
        rareCards = allAvailableCards.Where(card => card.rarity == CardRarity.Rare).ToList();
        epicCards = allAvailableCards.Where(card => card.rarity == CardRarity.Epic).ToList();
    }

    public List<UpgradeCardData> GetRandomUpgradeChoices(int count)
    {
        List<UpgradeCardData> choices = new List<UpgradeCardData>();
        List<UpgradeCardData> deckToDrawFrom = new List<UpgradeCardData>(allAvailableCards); // 복사해서 사용

        // 간단한 가중치 랜덤 (등급별 확률)
        // 예: Common 60%, Rare 30%, Epic 10%
        for (int i = 0; i < count; i++)
        {
            if (deckToDrawFrom.Count == 0) break; // 뽑을 카드가 없으면 종료

            UpgradeCardData selectedCard = null;
            float randomPick = Random.value;

            if (randomPick < 0.1f && epicCards.Count > 0 && deckToDrawFrom.Any(c => c.rarity == CardRarity.Epic)) // 10% 에픽
            {
                List<UpgradeCardData> availableEpics = deckToDrawFrom.Where(c => c.rarity == CardRarity.Epic).ToList();
                if(availableEpics.Count > 0) selectedCard = availableEpics[Random.Range(0, availableEpics.Count)];
            }
            else if (randomPick < 0.4f && rareCards.Count > 0 && deckToDrawFrom.Any(c => c.rarity == CardRarity.Rare)) // 30% 레어 (0.1 ~ 0.4)
            {
                 List<UpgradeCardData> availableRares = deckToDrawFrom.Where(c => c.rarity == CardRarity.Rare).ToList();
                 if(availableRares.Count > 0) selectedCard = availableRares[Random.Range(0, availableRares.Count)];
            }

            if(selectedCard == null) // 기본은 커먼 또는 남은 카드 중 랜덤
            {
                List<UpgradeCardData> availableCommons = deckToDrawFrom.Where(c => c.rarity == CardRarity.Common).ToList();
                if(availableCommons.Count > 0) selectedCard = availableCommons[Random.Range(0, availableCommons.Count)];
                else if (deckToDrawFrom.Count > 0) selectedCard = deckToDrawFrom[Random.Range(0, deckToDrawFrom.Count)]; // 등급무관 남은거
            }


            if (selectedCard != null && !choices.Contains(selectedCard)) // 중복 방지
            {
                choices.Add(selectedCard);
                deckToDrawFrom.Remove(selectedCard); // 이미 선택된 카드는 후보에서 제외
            }
            else if (selectedCard != null && choices.Contains(selectedCard)) // 중복되서 다시 뽑아야 할때
            {
                 i--; // 다시 뽑기
            }
            else if(selectedCard == null && deckToDrawFrom.Count > 0) // 특정 등급 카드가 다 떨어졌을 때
            {
                selectedCard = deckToDrawFrom[Random.Range(0, deckToDrawFrom.Count)];
                if (selectedCard != null && !choices.Contains(selectedCard))
                {
                    choices.Add(selectedCard);
                    deckToDrawFrom.Remove(selectedCard);
                } else i--;
            }
        }
        return choices;
    }

    public void ApplyUpgradeToPlayer(PlayerStats playerStats, UpgradeCardData card)
    {
        foreach (var effectValue in card.effects)
        {
            playerStats.ApplyUpgrade(effectValue.effectType, effectValue.value);
        }
    }
}