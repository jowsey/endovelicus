using System;
using System.Collections.Generic;
using ElRaccoone.Tweens;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class TowerMenu : MonoBehaviour
{
    public static TowerMenu instance { get; private set; }
    public Button openCloseButton;
    public GameObject mainPanel;
    public CanvasGroup canvasGroup;

    public readonly List<TowerData> towers = new List<TowerData>();
    public readonly List<Button> towerButtons = new List<Button>();

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
    
    public TowerData Get<T>() where T : TowerData => towers.Find(tower => tower is T);

    // Start is called before the first frame update
    private void Start()
    {
        mainPanel.SetActive(false);

        openCloseButton.onClick.AddListener(() => ToggleOpen(!mainPanel.activeSelf));
    }

    private void ToggleOpen(bool toggle)
    {
        if(toggle)
            mainPanel.SetActive(true);

        canvasGroup.TweenCanvasGroupAlpha(toggle ? 1f : 0f, 0.25f)
            .SetEaseCubicInOut()
            .SetOnComplete(() =>
            {
                if(!toggle)
                    mainPanel.SetActive(false);
            });
    }

    public void AddTowerType<T>() where T : TowerData
    {
        var towerData = (TowerData) Activator.CreateInstance(typeof(T));
        towers.Add(towerData);

        var towerButton = Addressables.InstantiateAsync("Prefabs/UI/TowerButton", mainPanel.transform)
            .WaitForCompletion()
            .GetComponent<TowerButton>();

        towerButton.towerData = towerData;
        towerButtons.Add(towerButton.GetComponent<Button>());
    }

    public void ToggleTowerControls(bool toggle)
    {
        if(!toggle)
            ToggleOpen(false);
        
        openCloseButton.interactable = toggle;
    }
}
