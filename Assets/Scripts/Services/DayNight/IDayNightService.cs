using Cysharp.Threading.Tasks;

namespace OrderRush.Services
{
    public interface IDayNightService
    {
        UniTask Initialize();
        void Dispose();
    }
}
