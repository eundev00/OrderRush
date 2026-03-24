// Scripts/Core/GameBootstrap.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        SceneManager.LoadScene("Gameplay");
    }
}
