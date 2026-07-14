using UnityEngine;
using System.Collections.Generic;

public class InteractableHighlight : MonoBehaviour
{
    // 렌더러 하나당 body/outline 머티리얼과 원본 색상을 묶어서 보관
    private class RendererMaterials
    {
        public Material Body;
        public Material Outline;
        public Color OriginalBaseColor;
        public Color OriginalEmission;
    }

    private readonly List<RendererMaterials> _targets = new List<RendererMaterials>();

    [SerializeField] private float _tintStrength = 0.1f;

    private void Start()
    {
        // 프리펩 하위의 모든 렌더러(상판+하단 등)를 한 번에 수집
        var renderers = GetComponentsInChildren<Renderer>();

        foreach (var renderer in renderers)
        {
            if (renderer == null) continue;

            // 0=body, 1=outline 슬롯 구조를 따르는 렌더러만 대상으로 처리
            if (renderer.materials.Length < 2) continue;

            var body = renderer.materials[0];
            var outline = renderer.materials[1];

            _targets.Add(new RendererMaterials
            {
                Body = body,
                Outline = outline,
                OriginalBaseColor = body.GetColor("_BaseColor"),
                OriginalEmission = body.GetColor("_EmissionColor")
            });
        }
    }

    public void SetSelected(bool selected)
    {
        foreach (var t in _targets)
        {
            if (selected)
            {
                Color yellow = new Color(1f, 0.95f, 0.6f);
                Color tint = Color.Lerp(Color.white, yellow, _tintStrength);
                t.Body.SetColor("_BaseColor", t.OriginalBaseColor * tint);

                t.Body.EnableKeyword("_EMISSION");
                t.Body.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.2f) * 0.5f);

                t.Outline.SetColor("_OutlineColor", Color.yellow);
            }
            else
            {
                t.Body.SetColor("_BaseColor", t.OriginalBaseColor);
                t.Body.SetColor("_EmissionColor", t.OriginalEmission);
                t.Outline.SetColor("_OutlineColor", Color.black);
            }
        }
    }
}