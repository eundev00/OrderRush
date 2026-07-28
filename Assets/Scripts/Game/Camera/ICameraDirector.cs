using Cysharp.Threading.Tasks;

public interface ICameraDirector
{
    UniTask PlayDayIntroAsync();
    UniTask PlayDayOutroAsync();
}
