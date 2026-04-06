using System.Threading;
using Cysharp.Threading.Tasks;

public interface IGameAction
{
    UniTask ExecuteAsync(CancellationToken ct);
}
