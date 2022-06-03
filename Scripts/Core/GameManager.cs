using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [NonSerialized]
    public int areaCount = Constants.GameRules.defaultAreaCount;

    [NonSerialized]
    public int areaWidth = Constants.GameRules.defaultAreaWidth;

    [NonSerialized]
    public int areaHeight = Constants.GameRules.defaultAreaHeight;

    [NonSerialized]
    public int villagesPerArea = Constants.GameRules.defaultVillagesPerArea;

    [NonSerialized]
    public int lotsPerVillage = Constants.GameRules.defaultLotsPerVillage;

    [NonSerialized]
    public string seed;

    [NonSerialized]
    public bool startWithUnits = Constants.GameRules.defaultStartWithUnits;

    [NonSerialized]
    public float captureSpeedMultiplier = Constants.GameRules.defaultCaptureSpeedMultiplier;

    private void Awake()
    {
        // We make sure there can only ever be one instance of this class by destroying it when created if one already exists
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Set the seed to a random value if it is not set
        seed ??= Random.Range(int.MinValue, int.MaxValue).ToString();
    }
}
