using System.Threading;
using Cysharp.Threading.Tasks;

public interface IStackable
{
    bool CanStack(ICarriable item);
    UniTask Stack(ICarriable item, CharacterBase character, CancellationToken ct);
}
