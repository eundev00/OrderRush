// Scripts/Core/GameBootstrap.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class Launcher : MonoBehaviour
{
    public void Start()
    {
        SceneManager.LoadScene("Gameplay");
    }
}