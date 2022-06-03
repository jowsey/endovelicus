using UnityEngine;
using UnityEngine.AddressableAssets;

public static class Icons
{
    public const string Church = "church";
    public const string HollowCircle = "circle";
    public const string FastForward = "fast-forward-button";
    public const string FoldedPaper = "folded-paper";
    public const string House = "house";
    public const string PauseButton = "pause-button";
    public const string FullCircle = "plain-circle";
    public const string PlayButton = "play-button";
    public const string PositionMarker = "position-marker";
    public const string SwordBrandish = "sword-brandish";
    public const string TwoCoins = "two-coins";
    public const string WingedArrow = "winged-arrow";
    public const string SwordBreak = "sword-break";
    public const string RoundShield = "round-shield";
    public const string Gloves = "gloves";
    public const string Healing = "healing";
    public const string HighShot = "high-shot";
    public const string WizardStaff = "wizard-staff";
    public const string CrystalWand = "crystal-wand";
    public const string Robe = "robe";
    public const string Bubbles = "bubbles";

    public static Sprite ResolveIcon(string iconName)
    {
        return Addressables.LoadAssetAsync<Sprite>("Icons/" + iconName).WaitForCompletion();
    }
}

public static class Constants
{
    public static readonly Color timeSelectedColour = new Color(134 / 255f, 239 / 255f, 121 / 255f, 1f);
    public static readonly Color ultraTimeSelectedColour = new Color(239 / 255f, 134 / 255f, 121 / 255f, 1f);
    public static readonly Color friendlyColour = new Color(103 / 255f, 102 / 255f, 51 / 255f, 1f);
    public static readonly Color enemyColour = new Color(179 / 255f, 56 / 255f, 49 / 255f, 1f);
    public static readonly Color neutralColour = new Color(0.85f, 0.85f, 0.85f, 1f);

    public static readonly Color towerGoldColor = new Color(255 / 255f, 195 / 255f, 55 / 255f, 1f);

    public static readonly Color winTextColor = new Color(135 / 255f, 245 / 255f, 126 / 255f, 1f);
    public static readonly Color loseTextColor = new Color(135 / 255f, 126 / 255f, 245 / 255f, 1f);

    public static readonly Color attackMoveColour = new Color(184 / 255f, 255 / 255f, 188 / 255f, 1f);
    public static readonly Color directMoveColour = new Color(184 / 255f, 188 / 255f, 255 / 255f, 1f);

    public static readonly Gradient redGreenGradient = new Gradient
    {
        colorKeys = new[]
        {
            new GradientColorKey(Color.red, 0f),
            new GradientColorKey(Color.yellow, 0.5f),
            new GradientColorKey(Color.white, 1f)
        }
    };

    public static class GameRules
    {
        public const int defaultAreaCount = 5;
        public const int defaultAreaWidth = 72;
        public const int defaultAreaHeight = 120;
        public const int defaultVillagesPerArea = 3;
        public const int defaultLotsPerVillage = 4;

        public const bool defaultStartWithUnits = true;
        public const float defaultCaptureSpeedMultiplier = 1f;
    }
}
