using System;
using System.Collections.Generic;

public static class AudioKeys
{
    public const string Bgm1 = "Bgm1";
    public const string Bgm2 = "Bgm2";
    public const string Bgm3 = "Bgm3";
    public const string Bgm4 = "Bgm4";
    public const string GameAudioMixer = "GameAudioMixer";
    public const string bell1 = "bell1";
    public const string bell2 = "bell2";
    public const string chopping1 = "chopping1";
    public const string chopping2 = "chopping2";
    public const string coin_drop1 = "coin_drop1";
    public const string commonbutton = "commonbutton";
    public const string cooking1 = "cooking1";
    public const string correct = "correct";
    public const string rain_calming = "rain_calming";
    public const string rain_light = "rain_light";
    public const string refrigerator_close = "refrigerator_close";
    public const string refrigerator_open = "refrigerator_open";
    public const string success1 = "success1";
    public const string takeit = "takeit";
    public const string washing_dishes_1 = "washing_dishes_1";
    public const string washing_dishes_2 = "washing_dishes_2";

    public static Dictionary<string, string> AudioPaths = new Dictionary<string, string>()
    {
        { Bgm1, "Assets/Audio/Bgm/Bgm1.mp3" },
        { Bgm2, "Assets/Audio/Bgm/Bgm2.mp3" },
        { Bgm3, "Assets/Audio/Bgm/Bgm3.mp3" },
        { Bgm4, "Assets/Audio/Bgm/Bgm4.mp3" },
        { GameAudioMixer, "Assets/Audio/GameAudioMixer.mixer" },
        { bell1, "Assets/Audio/Sfx/bell1.mp3" },
        { bell2, "Assets/Audio/Sfx/bell2.mp3" },
        { chopping1, "Assets/Audio/Sfx/chopping1.mp3" },
        { chopping2, "Assets/Audio/Sfx/chopping2.mp3" },
        { coin_drop1, "Assets/Audio/Sfx/coin_drop1.mp3" },
        { commonbutton, "Assets/Audio/Sfx/commonbutton.mp3" },
        { cooking1, "Assets/Audio/Sfx/cooking1.mp3" },
        { correct, "Assets/Audio/Sfx/correct.mp3" },
        { rain_calming, "Assets/Audio/Sfx/rain_calming.mp3" },
        { rain_light, "Assets/Audio/Sfx/rain_light.mp3" },
        { refrigerator_close, "Assets/Audio/Sfx/refrigerator_close.mp3" },
        { refrigerator_open, "Assets/Audio/Sfx/refrigerator_open.mp3" },
        { success1, "Assets/Audio/Sfx/success1.mp3" },
        { takeit, "Assets/Audio/Sfx/takeit.mp3" },
        { washing_dishes_1, "Assets/Audio/Sfx/washing_dishes_1.mp3" },
        { washing_dishes_2, "Assets/Audio/Sfx/washing_dishes_2.mp3" },
    };

    public static string GetAudioPath(string tag)
    {
        if (AudioPaths.TryGetValue(tag, out var path))
        {
            return path;
        }
        return string.Empty;
    }
}
