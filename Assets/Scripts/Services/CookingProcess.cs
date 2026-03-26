using System.Threading;

/// <summary>
/// 조리 진행 중인 프로세스 정보
/// </summary>
public class CookingProcess
{
    public CookingStep Step { get; set; }
    public CancellationTokenSource Cts { get; set; }
    public float ElapsedTime { get; set; }

    public float Progress => Step != null && Step.duration > 0
        ? UnityEngine.Mathf.Clamp01(ElapsedTime / Step.duration)
        : 0f;
}
