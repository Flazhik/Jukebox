using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Jukebox.Components.HUD;

public static class HudFontMaterials
{
    private static readonly int CullModeId = Shader.PropertyToID("_CullMode");
    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    private static readonly int TextureWidthId = Shader.PropertyToID("_TextureWidth");
    private static readonly int TextureHeightId = Shader.PropertyToID("_TextureHeight");
    private static readonly int GradientScaleId = Shader.PropertyToID("_GradientScale");

    private static Material _overlayTextMaterial;
    private static Material _normalTextMaterial;

    private static readonly Dictionary<(TMP_FontAsset, Material), Material> Variants = new();

    public static void Load()
    {
        _overlayTextMaterial =
            Addressables.LoadAssetAsync<Material>("Assets/Fonts/VCR_OSD_MONO Overlay.mat").WaitForCompletion();
        _normalTextMaterial =
            Addressables.LoadAssetAsync<Material>("Assets/Fonts/VCR_OSD_MONO Outline.mat").WaitForCompletion();
    }

    public static void RestoreOverlay(Transform root) => Restore(root, _overlayTextMaterial);

    public static void RestoreNormal(Transform root) => Restore(root, _normalTextMaterial);

    private static void Restore(Transform root, Material template)
    {
        if (template == null)
            return;

        foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            var font = text.font;
            if (font == null)
                continue;

            var variant = GetVariant(font, template);
            if (variant == null)
                continue;

            text.fontSharedMaterial = variant;

            if (font.fallbackFontAssetTable != null)
                foreach (var fallback in font.fallbackFontAssetTable)
                {
                    var fallbackVariant = GetVariant(fallback, template);
                    if (fallbackVariant != null)
                        fallback.material = fallbackVariant;
                }

            text.ForceMeshUpdate();
        }
    }

    private static Material GetVariant(TMP_FontAsset font, Material template)
    {
        if (Variants.TryGetValue((font, template), out var variant))
            return variant;

        var source = font.material;
        if (source == null)
            return null;

        variant = CreateVariant(source, template);
        Variants[(font, template)] = variant;
        return variant;
    }

    private static Material CreateVariant(Material source, Material template)
    {
        var material = new Material(source)
        {
            shader = template.shader
        };

        material.SetTexture(MainTexId, source.GetTexture(MainTexId));
        material.SetFloat(TextureWidthId, source.GetFloat(TextureWidthId));
        material.SetFloat(TextureHeightId, source.GetFloat(TextureHeightId));
        material.SetFloat(GradientScaleId, source.GetFloat(GradientScaleId));

        if (material.HasProperty(CullModeId))
            material.SetFloat(CullModeId, template.GetFloat(CullModeId));

        return material;
    }
}
