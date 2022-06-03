using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class SpellUI : MonoBehaviour
{
    public static SpellUI instance { get; private set; }
    public readonly List<Spell> spells = new List<Spell>();
    private readonly List<Button> spellButtons = new List<Button>();

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

    public static Spell Get<T>() where T : Spell => instance.spells.Find(spell => spell is T);

    public void AddSpell<T>(int level) where T : Spell
    {
        var existingSpell = spells.Find(x => x.GetType() == typeof(T));

        if (existingSpell != null)
        {
            existingSpell.Level = level;
        }
        else
        {
            // editor's note: this sucks, what was i thinking
            var spell = (Spell)Activator.CreateInstance(typeof(T), level);
            spells.Add(spell);

            var spellButton = Addressables.InstantiateAsync("Prefabs/UI/SpellButton", transform)
                .WaitForCompletion()
                .GetComponent<SpellButton>();

            spellButton.spell = spell;
            spellButtons.Add(spellButton.GetComponent<Button>());
        }
    }

    public void ToggleSpellUsage(bool toggle)
    {
        foreach (var button in spellButtons)
        {
            button.interactable = toggle;
            button.GetComponent<CanvasGroup>().alpha = toggle ? 1f : 0.75f;
        }
    }
}
