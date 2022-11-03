using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class GlobalSoundManager : MonoBehaviour
{
    static EventInstance[] everyEvent;
    public EventInstance[] everyEvent_Attribution;

    [EventRef]
    static string[] OneS_Sound;
    [EventRef]
    public string[] OneS_Sound_Attribution;


    // Start is called before the first frame update
    void Start()
    {
        everyEvent = everyEvent_Attribution;
        OneS_Sound = OneS_Sound_Attribution;
    }

    public static void PlayOneShot(int sNumber, Vector3 position)
    {

        if(sNumber>= OneS_Sound.Length)
        {
          //  Debug.LogError("[FMOD] Event not found because the number calls is out of  bound ");
            return;
        } 
            
        try
        {   
            RuntimeManager.PlayOneShot(OneS_Sound[sNumber], position);
        }
        catch (EventNotFoundException)
        {
            Debug.LogWarning("[FMOD] Event not found: " + OneS_Sound[sNumber]);
        }
    }


}
