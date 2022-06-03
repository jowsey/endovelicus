using System.Linq;
using TMPro;
using UnityEngine;

public class ConsoleManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI outputTmp;

    public GameObject console;

    public static bool isInConsole;

    private void Start()
    {
        console.SetActive(false);

        inputField.onSubmit.AddListener((val) =>
        {
            outputTmp.text += "\n" + val;
            inputField.ActivateInputField();

            // another incredibly designed system from Jowsey Enterprises, i really wish i had more time but at least this isn't a user-facing feature
            // so does it really matter? (yes)

            var words = val.Split(' ');
            switch (words[0].ToLower())
            {
                case "givedevotion":
                    if (int.TryParse(words[1], out var amount))
                    {
                        UIManager.instance.inventory.GetStatistic("devotion").Count += amount;
                        outputTmp.text += $"\nGained {amount} devotion";
                    }

                    break;
                case "givegold":
                    if (int.TryParse(words[1], out amount))
                    {
                        UIManager.instance.inventory.GetStatistic("gold").Count += amount;
                        outputTmp.text += $"\nGained {amount} gold";
                    }

                    break;
                case "takearea":
                    if (int.TryParse(words[1], out var index))
                    {
                        MapManager.instance.areas.FirstOrDefault(a => a.index == index)?.villages.ForEach(v => v.takeoverProgress = 100);
                        outputTmp.text += $"\nTook over area {index}";
                    }

                    break;
                case "losearea":
                    if (int.TryParse(words[1], out index))
                    {
                        MapManager.instance.areas.FirstOrDefault(a => a.index == index)?.villages.ForEach(v => v.takeoverProgress = -100);
                        outputTmp.text += $"\nLost area {index}";
                    }

                    break;
                case "win":
                    for (var i = 0; i < MapManager.instance.areas.Count; i++)
                    {
                        MapManager.instance.areas.FirstOrDefault(a => a.index == i)?.villages.ForEach(v => v.takeoverProgress = 100);
                    }

                    isInConsole = !isInConsole;
                    console.SetActive(isInConsole);

                    break;
                case "lose":
                    for (var i = 0; i < MapManager.instance.areas.Count; i++)
                    {
                        MapManager.instance.areas.FirstOrDefault(a => a.index == i)?.villages.ForEach(v => v.takeoverProgress = -100);
                    }

                    isInConsole = !isInConsole;
                    console.SetActive(isInConsole);

                    break;
                case "settimescale":
                    if (float.TryParse(words[1], out var scale))
                    {
                        TimeManager.instance.SetSpeed(scale);
                        outputTmp.text += $"\nSet time scale to {scale}";
                    }

                    break;
                case "givexp":
                    if (int.TryParse(words[1], out amount))
                    {
                        LevelManager.instance.xp += amount;
                        outputTmp.text += $"\nGained {amount} xp";
                    }

                    break;
                case "levelup":
                    LevelManager.instance.deityLevel++;
                    outputTmp.text += $"\nLeveled up to {LevelManager.instance.deityLevel}";

                    break;
                case "sv_cheats": // editor's note: hehe
                    if (words[1] == "1")
                    {
                        outputTmp.text += "\nCheat commands are now enabled. To disable cheat commands, run 'sv_cheats 0' at any time.";
                    }
                    else if (words[1] == "0")
                    {
                        outputTmp.text += "\nCheat commands are now disabled.";
                    }

                    break;
            }
        });
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            isInConsole = !isInConsole;
            console.SetActive(isInConsole);

            TimeManager.instance.ToggleTimeControls(!isInConsole);
            TimeManager.instance.SetSpeed(isInConsole ? 0f : 1f);

            if (isInConsole)
                inputField.ActivateInputField();
        }
    }
}
