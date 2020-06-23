using System;

[System.Serializable]
public class PlayerChunk
{
    public string chunkTag;
    public int numberOfPlayers;

    public PlayerChunk(string chunkTag, int numberOfPlayers)
    {
        this.chunkTag = chunkTag;
        this.numberOfPlayers = numberOfPlayers;
    }
    public override string ToString()
    {
        return "PlayerChunk tag=" + chunkTag + ", numberOfPlayers=" + numberOfPlayers;
    }
}