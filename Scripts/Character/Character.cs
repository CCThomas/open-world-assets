using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Character/Character")]
public class Character : ScriptableObject
{
    public Species species;
    public bool intialized;

    Dictionary<string, Trait> traitsDictionary = new Dictionary<string, Trait>();
    Dictionary<string, Ability> abilityDictionary = new Dictionary<string, Ability>();

    public void Initialize()
    {
        traitsDictionary.Clear();
        foreach (Trait trait in species.traits)
        {
            traitsDictionary.Add(trait.key, trait);
        }

        abilityDictionary.Clear();
        foreach (Ability ability in species.abilities)
        {
            abilityDictionary.Add(ability.key, ability);
        }
    }

    public Transform getBodyPartHead(Transform graphics)
    {
        string armaturePath = species.armatureHead;
        return graphics.Find(armaturePath);
    }

    public float GetAbilityValue(string key)
    {
        if (!abilityDictionary.ContainsKey(key))
        {
            return 0; // False
        }

        Ability ability = abilityDictionary[key];
        if (!traitsDictionary.ContainsKey(ability.secondaryKey))
        {
            return ability.value;
        }
        return ability.value * traitsDictionary[ability.secondaryKey].value;
    }

    internal float GetTraitValue(string key)
    {
        return traitsDictionary[key].value;
    }
}