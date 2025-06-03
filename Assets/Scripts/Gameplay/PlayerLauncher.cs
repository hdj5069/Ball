// Assets/_Scripts/Gameplay/PlayerLauncher.cs
using UnityEngine;
using System.Collections.Generic; // LineRenderer 예측선용

public class PlayerLauncher : MonoBehaviour
{
    public GameObject ballPrefab; // BallController를 가진 프리팹
    public Transform launchPoint;
    public LineRenderer aimLineRenderer;
    public float launchForce = 10f; // BallController의 moveSpeed와 연동 또는 별도 사용
    public int maxReflectionCount = 1; // 예측선 반사 횟수

    private Vector2 startDragPos;
    private Vector2 currentDragPos;
    private bool isDragging = false;

    // 멀티볼 관련
    public int ballsToLaunchThisTurn = 1; // 현재 턴에 발사할 공의 수 (업그레이드로 변경)

    void Start()
    {
        if (aimLineRenderer)
        {
            aimLineRenderer.positionCount = maxReflectionCount + 2; // 시작점, 첫 충돌점, 반사 후 끝점...
            aimLineRenderer.enabled = false;
        }
        // 초기 발사 가능 공 개수는 PlayerStats 또는 GameManager에서 관리
        ballsToLaunchThisTurn = GameManager.Instance.playerStats.GetInitialBallsPerTurn();
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Playing) return;
        if (GameManager.Instance.AreAllBallsIdleOrDestroyed() == false) return; // 모든 공이 멈추거나 파괴된 후에만 조작 가능

        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startDragPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // 발사 영역 제한 (선택적)
            // if (!IsWithinLaunchArea(startDragPos)) return;
            isDragging = true;
            if (aimLineRenderer) aimLineRenderer.enabled = true;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            currentDragPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (currentDragPos - (Vector2)launchPoint.position).normalized; // 또는 startDragPos 기준
             if (direction == Vector2.zero) direction = Vector2.up; // 0벡터 방지

            DrawAimLine(launchPoint.position, direction);
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            if (aimLineRenderer) aimLineRenderer.enabled = false;

            Vector2 direction = (currentDragPos - (Vector2)launchPoint.position).normalized;
            if (direction == Vector2.zero) direction = Vector2.up;

            // 너무 낮은 각도 발사 방지 (선택적)
            // if (Mathf.Abs(direction.y) < 0.1f) direction.y = Mathf.Sign(direction.y) * 0.1f;

            LaunchBalls(direction);
            GameManager.Instance.OnPlayerAction(); // 플레이어가 행동했음을 알림 (턴 진행 등)
        }
    }

    void DrawAimLine(Vector2 startPos, Vector2 direction)
    {
        if (!aimLineRenderer) return;

        aimLineRenderer.SetPosition(0, startPos);
        Vector2 currentPos = startPos;
        Vector2 currentDir = direction.normalized;

        for (int i = 0; i < maxReflectionCount + 1; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPos, currentDir, 100f, LayerMask.GetMask("Wall", "Brick")); // Wall과 Brick 레이어에만 충돌 예측
            if (hit.collider != null)
            {
                aimLineRenderer.SetPosition(i + 1, hit.point);
                if (i < maxReflectionCount) // 마지막 반사는 그리지 않음
                {
                    currentPos = hit.point - currentDir * 0.01f; // 약간 안쪽에서 시작해야 다음 레이캐스트가 바로 충돌 안함
                    currentDir = Vector2.Reflect(currentDir, hit.normal);
                }
                else // 최대 반사 횟수에 도달하면 여기서 라인 끝
                {
                     for(int j = i + 2; j < aimLineRenderer.positionCount; j++)
                        aimLineRenderer.SetPosition(j, hit.point); // 나머지 점들을 마지막 지점으로 채움
                    break;
                }
            }
            else
            {
                aimLineRenderer.SetPosition(i + 1, currentPos + currentDir * 100f); // 충돌 없으면 멀리 그림
                for(int j = i + 2; j < aimLineRenderer.positionCount; j++)
                     aimLineRenderer.SetPosition(j, currentPos + currentDir * 100f);
                break;
            }
        }
    }

    void LaunchBalls(Vector2 direction)
    {
        // 멀티볼 구현: ballsToLaunchThisTurn 만큼 공 발사
        // 약간의 시간차나 각도차를 두어 발사할 수 있음
        for (int i = 0; i < ballsToLaunchThisTurn; i++)
        {
            // GameObject ballObj = Instantiate(ballPrefab, launchPoint.position, Quaternion.identity);
            GameObject ballObj = ObjectPooler.Instance.GetPooledObject(PoolObjectType.Ball); // 오브젝트 풀러 사용
            if (ballObj != null)
            {
                ballObj.transform.position = launchPoint.position;
                ballObj.SetActive(true);

                BallController ball = ballObj.GetComponent<BallController>();
                if (ball != null)
                {
                    // PlayerStats에서 공의 현재 스탯 가져오기
                    int ballHp = GameManager.Instance.playerStats.GetCurrentBallHp();
                    int ballDamage = GameManager.Instance.playerStats.GetCurrentBallDamage();
                    ball.Initialize(ballHp, ballDamage);
                    // ball.moveSpeed = launchForce; // BallController의 moveSpeed를 사용하도록 통일하는 것이 좋음

                    // 멀티볼일 경우 약간의 각도 변화 (선택적)
                    Vector2 launchDir = direction;
                    if (ballsToLaunchThisTurn > 1)
                    {
                        float angleOffset = (i - (ballsToLaunchThisTurn -1) / 2.0f) * 2.5f; // 예: -2.5, 0, 2.5도
                        launchDir = Quaternion.Euler(0, 0, angleOffset) * direction;
                    }
                    ball.Launch(launchDir.normalized);
                    GameManager.Instance.RegisterActiveBall(ball);
                }
            }
        }
        // 다음 턴을 위해 ballsToLaunchThisTurn을 기본값으로 리셋 (영구적 멀티볼 업그레이드가 아니라면)
        // ballsToLaunchThisTurn = GameManager.Instance.playerStats.GetInitialBallsPerTurn();
        // -> "다음 발사 시 공 N개 추가" 같은 업그레이드는 GameManager나 PlayerStats에서 관리 후 적용해야 함
    }

    public void SetBallsToLaunch(int count)
    {
        ballsToLaunchThisTurn = Mathf.Max(1, count);
    }
}