// Assets/_Scripts/Core/Constants.cs
public static class Constants
{
    // Tags
    public const string BALL_TAG = "Ball";
    public const string BRICK_TAG = "Brick";
    public const string WALL_TAG = "Wall";
    public const string BOTTOM_WALL_TAG = "BottomWall"; // 공이 여기 닿아도 체력 안 닳는 벽
    public const string GAME_OVER_LINE_TAG = "GameOverLine";

    // Physics
    public const float BALL_BOUNCINESS = 1f;
    public const float BALL_FRICTION = 0f;

    // PlayerPrefs Keys (메타 데이터 저장용)
    public const string GOLD_KEY = "PlayerGold";
    public const string META_START_BALL_HP_KEY = "MetaStartBallHP";
    public const string META_START_BALL_DMG_KEY = "MetaStartBallDMG";
    // ... 기타 메타 업그레이드 키
}