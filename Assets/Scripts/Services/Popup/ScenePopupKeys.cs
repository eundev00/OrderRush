using System;

// 씬이 소유한 팝업 키 묶음 — ScenePopupRegistrar 에 "타입 기반"으로 주입하기 위한 값 객체.
// (WithParameter 문자열 이름 매칭의 런타임 리스크를 피하려는 목적.)
// 각 씬 LifetimeScope 가 자기 인스턴스를 RegisterInstance 하면, 그 씬 스코프의
// ScenePopupRegistrar 가 자기 것만 주입받는다(스코프 격리).
public sealed class ScenePopupKeys
{
    public readonly string[] Keys;

    public ScenePopupKeys(params string[] keys)
    {
        Keys = keys ?? Array.Empty<string>();
    }
}
