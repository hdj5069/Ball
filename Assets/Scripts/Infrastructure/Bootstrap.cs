// Assets/Scripts/Infrastructure/Bootstrap.cs
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    void Awake()
    {
        // Register commonly used services
        DIContainer.Register(GameManager.Instance);
        if (GameManager.Instance.uiManager != null)
            DIContainer.Register(GameManager.Instance.uiManager);
        if (GameManager.Instance.playerLauncher != null)
            DIContainer.Register(GameManager.Instance.playerLauncher);
        if (GameManager.Instance.brickSpawner != null)
            DIContainer.Register(GameManager.Instance.brickSpawner);
    }
}
