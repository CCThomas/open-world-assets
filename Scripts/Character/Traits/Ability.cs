[System.Serializable]
public class Ability : Trait
{
    public string secondaryKey;

    public Ability(string key, string secondaryKey) : base(key)
    {
        this.secondaryKey = secondaryKey;
    }


    public Ability(string key, string secondaryKey, float value) : base(key, value)
    {
        this.secondaryKey = secondaryKey;
    }
}