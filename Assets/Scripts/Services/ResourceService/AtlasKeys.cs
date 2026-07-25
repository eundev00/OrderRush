using System.Collections.Generic;

public static class AtlasKeys
{
    public const string IconAtlas = "IconAtlas";
    public const string UIAtlas = "UIAtlas";

    public static Dictionary<string, string> AtlasPaths = new Dictionary<string, string>()
    {
        { IconAtlas, "Assets/Art/Images/IconAtlas.spriteatlas" },
        { UIAtlas, "Assets/Art/Images/UIAtlas.spriteatlas" },
    };

    public static string GetAtlasPath(string tag)
    {
        if (AtlasPaths.TryGetValue(tag, out var path))
            return path;
        return string.Empty;
    }
}
