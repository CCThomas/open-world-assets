using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Effect", menuName = "Character/Effect/Effect")]
public class Effect
{
    float duration;
    float frequency;
    EffectType effectType;
    List<TraitModifier> modifiers;

}
