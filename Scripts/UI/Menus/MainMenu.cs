using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// coming back to this way later into the project, i have no idea why i coded it like this
// i guess that just shows how quickly you can learn when you're making stuff huh
// regardless, it works so i will not be remaking it (nor the pause menu which i just copy-pasted from this)

public class MainMenu : MonoBehaviour
{
    private List<Button> menuOptions;
    private Camera cam;

    [Header("Custom game")]
    [SerializeField]
    private RectTransform customGamePanel;

    [Header("Credits")]
    [SerializeField]
    private RectTransform creditsPanel;

    [Header("Settings")]
    [SerializeField]
    private RectTransform settingsPanel;

    public TextMeshProUGUI versionTmp;
    
    private void SelectButton(GameObject button)
    {
        UnselectAllButtons();
        var tmp = button.GetComponentInChildren<TextMeshProUGUI>();
        tmp.color = new Color(244 / 255f, 173 / 255f, 82 / 255f, 1f);
        tmp.text = "<color=white>></color> " + tmp.text;
    }

    private void UnselectAllButtons()
    {
        // Remove all select effects from each button (we go through every button to cover edge cases when using both mouse and arrow keys)
        foreach (var tmp in menuOptions.Select(button => button.GetComponentInChildren<TextMeshProUGUI>()))
        {
            tmp.color = Color.white;
            tmp.text = tmp.text.Replace("<color=white>></color> ", "");

            // Set cursor to standard
            var cursorPointer = Addressables.LoadAssetAsync<Texture2D>("Cursor/Standard").WaitForCompletion();
            Cursor.SetCursor(cursorPointer, Vector2.zero, CursorMode.Auto);
        }
    }

    private void RunButtonEffect(Object button)
    {
        switch (button.name)
        {
            case "Custom game":
                customGamePanel.gameObject.SetActive(true);
                creditsPanel.gameObject.SetActive(false);
                settingsPanel.gameObject.SetActive(false);
                break;
            case "Credits":
                customGamePanel.gameObject.SetActive(false);
                creditsPanel.gameObject.SetActive(true);
                settingsPanel.gameObject.SetActive(false);
                break;
            case "Itch":
                Application.OpenURL("https://jowsey.itch.io/endovelicus");
                break;
            case "Settings":
                customGamePanel.gameObject.SetActive(false);
                creditsPanel.gameObject.SetActive(false);
                settingsPanel.gameObject.SetActive(true);
                break;
            case "Quit":
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
                Application.Quit();
                break;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        cam = Camera.main;
        versionTmp.text = "v" + Application.version;
        
        // Get every button in the menu, then for each one, register events for selection
        menuOptions = GameObject.Find("MenuPanel").GetComponentsInChildren<Button>().Where(b => b.interactable).ToList();
        foreach (var button in menuOptions)
        {
            var trigger = button.AddComponent<EventTrigger>();

            var enter = new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
            enter.callback.AddListener(data =>
            {
                SelectButton(button.gameObject);
                SoundManager.Play(SoundManager.GetAudioClip("SFX/ButtonHover"), 0.25f);

                // Set cursor to pointer
                var cursorPointer = Addressables.LoadAssetAsync<Texture2D>("Cursor/Pointer").WaitForCompletion();
                Cursor.SetCursor(cursorPointer, Vector2.zero, CursorMode.Auto);
            });

            var exit = new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
            exit.callback.AddListener(data => UnselectAllButtons());

            var click = new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
            click.callback.AddListener(data => RunButtonEffect(button));

            trigger.triggers.Add(enter);
            trigger.triggers.Add(exit);
            trigger.triggers.Add(click);
        }

        // Setup panels
        customGamePanel.gameObject.SetActive(false);
        creditsPanel.gameObject.SetActive(false);
        settingsPanel.gameObject.SetActive(false);

        StartCoroutine(BackgroundSpawner());
    }

    private static IEnumerator BackgroundSpawner()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);

            var numberOfFriendlies = FindObjectsOfType<Unit>().Count(u => u.ownership == UnitOwnership.Friendly);
            var numberOfRomans = FindObjectsOfType<Unit>().Count(u => u.ownership == UnitOwnership.Roman);
            
            var unit = Addressables.InstantiateAsync("Prefabs/Units/Basic").WaitForCompletion().GetComponent<Unit>();
            
            unit.ownership = numberOfFriendlies > numberOfRomans ? UnitOwnership.Roman : UnitOwnership.Friendly;

            var classSelection = Random.value;
            if(classSelection <= 0.1f)
                unit.unitClass = UnitClass.Mage;
            else if(classSelection <= 0.75f)
                unit.unitClass = UnitClass.Melee;
            else
                unit.unitClass = UnitClass.Archer;

            NavMesh.SamplePosition(
                new Vector3(
                    unit.ownership == UnitOwnership.Friendly ? Random.Range(-5.5f, -4f) : Random.Range(2f, 3.5f),
                    Random.Range(-1f, 1f),
                    0
                ), 
                out var hit,
                10f,
                NavMesh.AllAreas
            );
            
            unit.navMeshAgent.Warp(hit.position);
        }
    }

    private void Update()
    {
        // Use arrow keys to navigate through menu
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            var selectedIndex =
                menuOptions.IndexOf(menuOptions.FirstOrDefault(b =>
                    b.GetComponentInChildren<TextMeshProUGUI>().color != Color.white));

            if(selectedIndex >= menuOptions.Count - 1) return;
            SelectButton(menuOptions[selectedIndex + 1].gameObject);
        }
        else if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            var selectedIndex =
                menuOptions.IndexOf(menuOptions.FirstOrDefault(b =>
                    b.GetComponentInChildren<TextMeshProUGUI>().color != Color.white));
            if(selectedIndex == -1) selectedIndex = menuOptions.Count;

            if(selectedIndex <= 0) return;
            SelectButton(menuOptions[selectedIndex - 1].gameObject);
        }

        if(Input.GetKeyUp(KeyCode.Return))
        {
            var selectedButton =
                menuOptions.FirstOrDefault(b => b.GetComponentInChildren<TextMeshProUGUI>().color != Color.white);

            if(selectedButton != null)
            {
                RunButtonEffect(selectedButton);
            }
        }
        else if(Input.GetKeyUp(KeyCode.Escape))
        {
            UnselectAllButtons();
            customGamePanel.gameObject.SetActive(false);
            creditsPanel.gameObject.SetActive(false);
            settingsPanel.gameObject.SetActive(false);
        }

        // Tilemap is 16:9 by default, if the screen is not 16:9, scale the camera's ortho size so it fits accordingly
        var screenAspect = (float) Screen.width / Screen.height;
        const float tilemapAspect = 16f / 9f;

        if(screenAspect > tilemapAspect)
        {
            cam.orthographicSize = 5f * (tilemapAspect / screenAspect);
        }
        else
        {
            cam.orthographicSize = 5f;
        }
    }
}
