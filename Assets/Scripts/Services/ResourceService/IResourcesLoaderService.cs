using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IResourcesLoaderService
{
    UniTask<T> LoadAsync<T>(string key) where T : Object;
    T LoadSync<T>(string key) where T : Object;
    void Release(string key);
    void ReleaseAll();
}
