using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

[CreateAssetMenu(fileName = "New Species", menuName = "Character/Species")]
public class Species : ScriptableObject
{
    public string modelName;
    public string armatureHead;

    public List<Trait> traits = new List<Trait>()
    {
        new Trait("agility"),
        new Trait("height"),
    };

    public List<Ability> abilities = new List<Ability>()
    {
        new Ability("climb", "agility", 0),
        new Ability("fly", "agility", 0),
        new Ability("jump", "agility", 0),
        new Ability("speed", "agility", 0),
};
}
