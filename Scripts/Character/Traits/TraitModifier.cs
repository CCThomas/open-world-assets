[System.Serializable]
public class TraitModifier
{
    public string name;
    public string key;
    public float modifier;
    public MathermaticalOperation mathermaticalOperation;
    public TraitModifierType traitModifierType;

    public TraitModifier(string name, string key, float modifier, MathermaticalOperation mathermaticalOperation, TraitModifierType traitModifierType)
    {
        this.name = name;
        this.key = key;
        this.modifier = modifier;
        this.mathermaticalOperation = mathermaticalOperation;
        this.traitModifierType = traitModifierType;
    }
}

public enum MathermaticalOperation
{
    Addition, Multiplication
}

public enum TraitModifierType
{
    Trait, Ability
}

