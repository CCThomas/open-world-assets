using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static float defaultGravity;
    public List<ChunkGravity> chunkGravityList;

    static Dictionary<string, float> chunkGravity = new Dictionary<string, float>();

    // Use this for initialization
    void Start()
    {
        foreach(ChunkGravity chunkGrav in chunkGravityList)
        {
            chunkGravity.Add(chunkGrav.chunkTag, chunkGrav.gravity);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static float GetGravity(string chunkTag)
    {
        return chunkGravity.ContainsKey(chunkTag) ? chunkGravity[chunkTag] : defaultGravity;
    }
}

[System.Serializable]
public class ChunkGravity
{
    public string chunkTag;
    public float gravity;
}