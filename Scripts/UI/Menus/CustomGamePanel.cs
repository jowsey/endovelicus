using System.Collections;
using System.Collections.Generic;
using ElRaccoone.Tweens;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomGamePanel : MonoBehaviour
{
    public Button startGameButton;
    public Button resetToDefaultButton;

    public Slider areaCountSlider;
    public Slider areaWidthSlider;
    public Slider areaHeightSlider;
    public Slider villagesPerAreaSlider;
    public Slider lotsPerVillageSlider;

    public TMP_InputField seedInput;
    public TextMeshProUGUI seedInputPlaceholder;
    public string randomSeed;

    public Toggle startWithUnitsToggle;
    public Slider captureSpeedMultiplierSlider;

    public CanvasGroup loadingScreen;
    public TextMeshProUGUI loadingProgress;
    public TextMeshProUGUI loreText;

    public List<string> lore = new List<string>();

    private void StartGame()
    {
        GameManager.instance.areaCount = (int) areaCountSlider.value;
        GameManager.instance.areaWidth = (int) areaWidthSlider.value;
        GameManager.instance.areaHeight = (int) areaHeightSlider.value;
        GameManager.instance.villagesPerArea = (int) villagesPerAreaSlider.value;
        GameManager.instance.lotsPerVillage = (int) lotsPerVillageSlider.value;
        
        GameManager.instance.seed = seedInput.text.Length > 0 ? seedInput.text : randomSeed;

        GameManager.instance.startWithUnits = startWithUnitsToggle.isOn;
        GameManager.instance.captureSpeedMultiplier = captureSpeedMultiplierSlider.value;

        // Setup loading screen
        loadingScreen.gameObject.SetActive(true);
        loreText.text = lore[Random.Range(0, lore.Count)];
        
        loadingScreen.alpha = 0;
        loadingScreen.TweenCanvasGroupAlpha(1f, 0.25f).SetEaseQuadInOut().SetUseUnscaledTime(true).SetOnComplete(() =>
        {
            AsyncOperation loadScene;

            IEnumerator Load()
            {
                loadScene = SceneManager.LoadSceneAsync("Scenes/Battle");

                while (loadScene.progress < 1)
                {
                    loadingProgress.text = Mathf.RoundToInt(loadScene.progress * 100) + "%";
                    yield return new WaitForEndOfFrame();
                }

                yield return null;
            }

            StartCoroutine(Load());
        });
    }

    private void ResetToDefault()
    {
        areaCountSlider.value = Constants.GameRules.defaultAreaCount;
        areaWidthSlider.value = Constants.GameRules.defaultAreaWidth;
        areaHeightSlider.value = Constants.GameRules.defaultAreaHeight;
        villagesPerAreaSlider.value = Constants.GameRules.defaultVillagesPerArea;
        lotsPerVillageSlider.value = Constants.GameRules.defaultLotsPerVillage;
        
        startWithUnitsToggle.isOn = Constants.GameRules.defaultStartWithUnits;
        captureSpeedMultiplierSlider.value = Constants.GameRules.defaultCaptureSpeedMultiplier;
    }

    // Start is called before the first frame update
    private void Start()
    {
        startGameButton.onClick.AddListener(StartGame);
        resetToDefaultButton.onClick.AddListener(ResetToDefault);

        ResetToDefault();

        randomSeed = Random.Range(int.MinValue, int.MaxValue).ToString();

        seedInputPlaceholder.text = randomSeed;
    }
}
