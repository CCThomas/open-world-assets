using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public float defaultGravity;
    public List<ChunkGravity> chunkGravityList;

    public static float gravity;

    static Dictionary<string, float> chunkGravity = new Dictionary<string, float>();

    // Use this for initialization
    void Start()
    {
        gravity = defaultGravity;
        foreach (ChunkGravity chunkGrav in chunkGravityList)
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
        if (chunkTag == null)
        {
            return gravity;
        }
        return chunkGravity.ContainsKey(chunkTag) ? chunkGravity[chunkTag] : gravity;
    }
}

[System.Serializable]
public class ChunkGravity
{
    public string chunkTag;
    public float gravity;
}