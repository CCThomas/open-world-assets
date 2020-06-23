using System;
using System.Collections.Generic;

[System.Serializable]
public class Chunk
{
    // "tag" is a variable on mono behaviors. To avoid interefing with that varaible, use "chunkTag".
    public string chunkTag;
    public List<string> neighboringTags;

    // Trackers
    bool active;

    public Chunk(string chunkTag, List<string> neighboringTags)
    {
        this.chunkTag = chunkTag;
        this.neighboringTags = neighboringTags;
    }

    public void SetActive(bool active) { this.active = active; }

    public bool IsActive() { return active; }

    public override string ToString()
    {
        return "Chunk chunkTag=" + chunkTag + ", active=" + active + ", neighboringTags=" + neighboringTags;
    }
}
