[System.Serializable]
public class Trait
{
    public string key;
    public float value;

    public Trait(string key)
    {
        this.key = key;
        this.value = 1;
    }

    public Trait(string key, float value)
    {
        this.key = key;
        this.value = value;
    }
}