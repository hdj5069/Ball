// Assets/_Scripts/Gameplay/BrickSpawner.cs
using UnityEngine;
using System.Collections.Generic;

public class BrickSpawner : MonoBehaviour
{
    public GameObject brickPrefab; // Brick 컴포넌트를 가진 기본 프리팹
    // TODO: 특수 벽돌 프리팹들도 리스트나 배열로 관리하여 랜덤 생성 가능
    public List<GameObject> specialBrickPrefabs;

    public int columns = 7;
    public float brickWidth = 1.0f; // 벽돌 가로 크기
    public float brickHeight = 0.5f; // 벽돌 세로 크기
    public Transform spawnStartPoint; // 첫 줄 생성 기준점 (화면 상단 바깥)
    public float rowSpacing = 0.1f; // 줄 간 간격
    public float initialSpawnInterval = 2f; // 초기 벽돌 생성 간격 (초)
    public float minSpawnInterval = 0.5f;   // 최소 벽돌 생성 간격
    public float spawnIntervalDecreaseRate = 0.05f; // 시간당 생성 간격 감소율

    private float currentSpawnInterval;
    private float timeSinceLastSpawn;
    private int waveCount = 0;
    private List<Brick> activeBricks = new List<Brick>();

    // 난이도 조절용 변수
    private int minHpPerBrick = 1;
    private int maxHpPerBrick = 3;
    private float specialBrickChance = 0.1f; // 특수 벽돌 등장 확률

    // 다음 줄 미리보기 UI 관련 (GameManager나 UIManager와 연동 필요)
    // private List<BrickType> nextRowPreview;

    void Start()
    {
        currentSpawnInterval = initialSpawnInterval;
        timeSinceLastSpawn = 0f; // 첫 줄은 바로 생성하도록
        // SpawnNewRow(); // 게임 시작 시 바로 생성 또는 GameManager에서 호출
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        timeSinceLastSpawn += Time.deltaTime;
        if (timeSinceLastSpawn >= currentSpawnInterval)
        {
            MoveAllBricksDown();
            SpawnNewRow();
            timeSinceLastSpawn = 0f;

            // 난이도 점진적 상승
            currentSpawnInterval = Mathf.Max(minSpawnInterval, currentSpawnInterval - spawnIntervalDecreaseRate);
            AdjustDifficulty();
        }
    }

    void AdjustDifficulty()
    {
        waveCount++;
        // 예시: 5 웨이브마다 벽돌 체력 증가
        if (waveCount % 5 == 0)
        {
            minHpPerBrick++;
            maxHpPerBrick++;
        }
        // 특수 벽돌 등장 확률 점진적 증가 (최대치 설정)
        specialBrickChance = Mathf.Min(0.5f, specialBrickChance + 0.01f);
    }

    public void SpawnNewRow()
    {
        // TODO: 다음 줄 벽돌 미리보기 UI 업데이트 로직 (여기서 생성할 벽돌 정보를 미리 결정)

        float startX = spawnStartPoint.position.x - (columns / 2f * brickWidth) + (brickWidth / 2f);
        if (columns % 2 == 0) startX += brickWidth / 2f; // 짝수 열일 때 중앙 정렬 보정


        for (int i = 0; i < columns; i++)
        {
            // 빈 공간 생성 확률 (시간이 지날수록 감소)
            float emptySpaceChance = Mathf.Max(0.1f, 0.5f - (waveCount * 0.02f));
            if (Random.value < emptySpaceChance && waveCount > 2) // 초반 몇 웨이브는 꽉 채움
            {
                continue;
            }

            Vector3 spawnPos = new Vector3(startX + (i * brickWidth), spawnStartPoint.position.y, 0);
            //GameObject brickObj = Instantiate(brickPrefab, spawnPos, Quaternion.identity, transform); // 부모를 Spawner로
            GameObject brickObjToSpawn = brickPrefab; // 기본 벽돌
            BrickType currentBrickType = BrickType.Normal;

            // 특수 벽돌 생성 로직
            if (Random.value < specialBrickChance && specialBrickPrefabs.Count > 0)
            {
                brickObjToSpawn = specialBrickPrefabs[Random.Range(0, specialBrickPrefabs.Count)];
                // 프리팹 이름이나 태그로 타입 추론, 또는 Brick 컴포넌트에 미리 설정된 타입 사용
                // 예시: Brick 컴포넌트에 public BrickType defaultType 설정 후 사용
                Brick tempBrickInfo = brickObjToSpawn.GetComponent<Brick>();
                if(tempBrickInfo != null) currentBrickType = tempBrickInfo.brickType; // 프리팹에 설정된 타입
            }


            GameObject brickInstance = ObjectPooler.Instance.GetPooledObject(PoolObjectType.Brick); // 일반 벽돌 풀 사용 가정
            // 만약 특수 벽돌마다 풀이 다르다면, GetPooledObject(특수벽돌타입) 호출
            if(brickInstance == null)
            {
                Debug.LogError("Failed to get brick from pool. Make sure pool is large enough.");
                continue;
            }

            brickInstance.transform.position = spawnPos;
            brickInstance.transform.SetParent(transform); // Spawner를 부모로
            brickInstance.SetActive(true);


            Brick brick = brickInstance.GetComponent<Brick>();
            if (brick != null)
            {
                int hp = Random.Range(minHpPerBrick, maxHpPerBrick + 1);
                int xp = hp * 5; // 체력 비례 XP
                int gold = (currentBrickType == BrickType.Coin) ? Random.Range(1, 4) : 0; // 코인 벽돌만 골드
                if (currentBrickType == BrickType.XPBoost) xp *= 3; // XP 부스트 벽돌

                brick.Initialize(hp, xp, gold, currentBrickType);
                activeBricks.Add(brick);
                GameManager.Instance.RegisterActiveBrick(brick);
            }
        }
    }

    public void MoveAllBricksDown()
    {
        // 현재 활성화된 모든 벽돌을 한 칸 아래로 이동
        // GameManager가 관리하는 activeBricks 리스트를 사용하거나, Spawner가 직접 관리
        // 여기서는 Spawner가 관리하는 것으로 가정 (GameManager는 전체 목록만 참조)
        for (int i = activeBricks.Count - 1; i >= 0; i--)
        {
            Brick brick = activeBricks[i];
            if (brick == null || !brick.gameObject.activeSelf)
            {
                activeBricks.RemoveAt(i);
                continue;
            }
            brick.transform.position += Vector3.down * (brickHeight + rowSpacing);

            // 게임 오버 라인 체크
            if (brick.transform.position.y <= GameManager.Instance.gameOverLineY)
            {
                GameManager.Instance.GameOver();
                return; // 즉시 게임 오버
            }
        }
    }

    public void RemoveBrickFromList(Brick brick)
    {
        if (activeBricks.Contains(brick))
        {
            activeBricks.Remove(brick);
        }
    }

    public void ClearAllBricks()
    {
        for (int i = activeBricks.Count - 1; i >= 0; i--)
        {
            if(activeBricks[i] != null && activeBricks[i].gameObject.activeSelf)
            {
                activeBricks[i].gameObject.SetActive(false); // 풀로 반환
            }
        }
        activeBricks.Clear();
        waveCount = 0;
        minHpPerBrick = 1;
        maxHpPerBrick = 3;
        specialBrickChance = 0.1f;
        currentSpawnInterval = initialSpawnInterval;
        timeSinceLastSpawn = 0f;
    }
}