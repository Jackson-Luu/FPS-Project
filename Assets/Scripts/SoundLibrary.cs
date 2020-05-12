using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour
{
    public SoundGroup[] soundGroups;

    private Dictionary<string, AudioClip[]> groupDictionary = new Dictionary<string, AudioClip[]>();

    private void Awake()
    {
        foreach(SoundGroup soundGroup in soundGroups) {
            groupDictionary.Add(soundGroup.groupID, soundGroup.group);
        }
    }
    public AudioClip GetClip(string name, int index)
    {
        if (groupDictionary.ContainsKey(name))
        {
            return groupDictionary[name][index];
        }
        return null;
    }

    [System.Serializable]
    public class SoundGroup {
        public string groupID;
        public AudioClip[] group;
    }
}
