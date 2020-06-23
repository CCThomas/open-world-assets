using System;
using System.Collections.Generic;

public class DialogueManager
{
    Random random = new Random();
    List<Dialogue> primary;
    Dictionary<string, Dialogue> dialogue;

    public DialogueManager(List<Dialogue> dialogue)
    {
        foreach (Dialogue dialogue1 in dialogue)
        {
            if (dialogue1.primary)
            {
                primary.Add(dialogue1);
            }
            this.dialogue.Add(dialogue1.key, dialogue1);
        }
    }

    public Dialogue GetOpeningLine()
    {
        int index = random.Next(primary.Count);
        return primary[index];
    }

    public Dialogue GetDialogue(string key)
    {
        return dialogue[key];
    }
}
