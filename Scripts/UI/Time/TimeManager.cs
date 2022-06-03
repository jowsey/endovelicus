using ElRaccoone.Tweens;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// this *feels* like a horrible way of doing it but i'm not sure
public class TimeManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button pauseButton;
    public Button playButton;
    public Button speedUpButton;
    public TextMeshProUGUI speedText;
    private UnityEvent ultraSpeedUp = new UnityEvent();

    private bool timeControlsActive = true;

    public static TimeManager instance { get; private set; }

    public void SetSpeed(float speed)
    {
        gameObject.TweenCancelAll();
        gameObject.TweenValueFloat(speed, 0.25f, val => Time.timeScale = val)
            .SetUseUnscaledTime(true)
            .SetFrom(Time.timeScale);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        pauseButton.onClick.AddListener(() =>
        {
            UIManager.instance.spellUI.ToggleSpellUsage(false);
            TowerMenu.instance.ToggleTowerControls(false);

            SetSpeed(0f);

            pauseButton.image.color = Constants.timeSelectedColour;
            playButton.image.color = Color.white;
            speedUpButton.image.color = Color.white;
        });

        playButton.onClick.AddListener(() =>
        {
            UIManager.instance.spellUI.ToggleSpellUsage(true);
            TowerMenu.instance.ToggleTowerControls(true);
            SetSpeed(1f);

            pauseButton.image.color = Color.white;
            playButton.image.color = Constants.timeSelectedColour;
            speedUpButton.image.color = Color.white;
        });

        speedUpButton.onClick.AddListener(() =>
        {
            UIManager.instance.spellUI.ToggleSpellUsage(true);
            TowerMenu.instance.ToggleTowerControls(true);
            SetSpeed(2f);

            pauseButton.image.color = Color.white;
            playButton.image.color = Color.white;
            speedUpButton.image.color = Constants.timeSelectedColour;
        });

        ultraSpeedUp.AddListener(() =>
        {
            UIManager.instance.spellUI.ToggleSpellUsage(true);
            TowerMenu.instance.ToggleTowerControls(true);
            SetSpeed(3f);

            pauseButton.image.color = Color.white;
            playButton.image.color = Color.white;
            speedUpButton.image.color = Constants.ultraTimeSelectedColour;
        });

        playButton.onClick.Invoke();
    }

    private void Update()
    {
        speedText.text = Time.timeScale.ToString("0.0") + "x";
        if (!timeControlsActive) return;

        if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.timeScale == 0f)
            {
                playButton.onClick.Invoke();
            }
            else
            {
                pauseButton.onClick.Invoke();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            playButton.onClick.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (speedUpButton.image.color == Constants.timeSelectedColour)
            {
                ultraSpeedUp.Invoke();
            }
            else
            {
                speedUpButton.onClick.Invoke();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ultraSpeedUp.Invoke();
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            SetSpeed(15f);
        }
#endif
    }

    public void ToggleTimeControls(bool toggle)
    {
        pauseButton.interactable = toggle;
        playButton.interactable = toggle;
        speedUpButton.interactable = toggle;
        timeControlsActive = toggle;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(Addressables.LoadAssetAsync<Texture2D>("Cursor/Pointer").WaitForCompletion(), Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(Addressables.LoadAssetAsync<Texture2D>("Cursor/Standard").WaitForCompletion(), Vector2.zero, CursorMode.Auto);
    }
}
