using Cysharp.Threading.Tasks;

public interface IDayNightService
{
    UniTask Initialize();
    void Dispose();
}
