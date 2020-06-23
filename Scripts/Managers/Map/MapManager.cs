using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public int numberOfChunksX, numberOfChunksZ, loadSize;
    public List<Chunk> chunksList;
    static Dictionary<string, Chunk> chunks = new Dictionary<string, Chunk>();
    public List<PlayerChunk> playerChunkList;
    static Dictionary<string, PlayerChunk> playerChunks = new Dictionary<string, PlayerChunk>();

    static Dictionary<string, GameObject[]> gameObjects = new Dictionary<string, GameObject[]>();

    // Start is called before the first frame update
    void Start()
    {
        // Load
        foreach (Chunk chunk in chunksList)
        {
            GameObject[] gos = GameObject.FindGameObjectsWithTag(chunk.chunkTag);
            gameObjects.Add(chunk.chunkTag, gos);
            chunks.Add(chunk.chunkTag, chunk);
            DeactivateChunk(chunk.chunkTag);
        }

        foreach (PlayerChunk playerChunk in playerChunkList)
        {
            for (int i = 1; i <= playerChunk.numberOfPlayers; i++)
            {
                PlayerChangedChunks(null, playerChunk.chunkTag);
            }
        }
    }

    public static void PlayerChangedChunks(string previousTag, string newTag)
    {
        List<string> tagsToActivate = new List<string>();
        List<string> tagsToDeactivate = new List<string>();

        // Tags to Deactivate
        if (previousTag != null && playerChunks.ContainsKey(previousTag))
        {
            PlayerChunk playerChunk = playerChunks[previousTag];
            playerChunk.numberOfPlayers--;
            if (playerChunk.numberOfPlayers <= 0)
            {
                playerChunks.Remove(playerChunk.chunkTag);
            }

            Chunk chunk = chunks[playerChunk.chunkTag];
            foreach (string chunkTag in chunk.neighboringTags)
            {
                tagsToDeactivate.Add(chunkTag);    
            }
        }

        // Tags to Activate
        if (newTag != null && !playerChunks.ContainsKey(newTag))
        {
            PlayerChunk playerChunk = new PlayerChunk(newTag, 1);
            playerChunks.Add(playerChunk.chunkTag, playerChunk);

            Chunk chunk = chunks[playerChunk.chunkTag];
            foreach (string chunkTag in chunk.neighboringTags)
            {
                tagsToActivate.Add(chunkTag);
            }
        } else if (newTag != null && playerChunks.ContainsKey(newTag))
        {
            playerChunks[newTag].numberOfPlayers++;
        }

        // Remove Tags to deactive from list if they are in the active list
        List<string> tagsToDeactivateFinal = new List<string>();
        foreach (string tag in tagsToDeactivate)
        {
            if (!tagsToActivate.Contains(tag))
            {
                tagsToDeactivateFinal.Add(tag);
            }
        }

        // Deactivate Chunks
        foreach (string tag in tagsToDeactivateFinal)
        {
            DeactivateChunk(tag);
        }
        // Activate Chunks
        foreach (string tag in tagsToActivate)
        {
            ActivateChunk(tag);
        }
    }

    private static void ActivateChunk(string tag)
    {
        SetChunkActive(tag, true);
    }

    private static void DeactivateChunk(string tag)
    {
        SetChunkActive(tag, false);
    }

    private static void SetChunkActive(string tag, bool active)
    {
        foreach (GameObject gameObject in gameObjects[tag])
        {
            gameObject.SetActive(active);
        }
        chunks[tag].SetActive(active);
    }
}