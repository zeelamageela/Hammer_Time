using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerMixer : MonoBehaviour
{

    // public AudioSource Menu;

    public AudioSource PercussionLayer;
    public AudioSource MelodyLayer1;
    public AudioSource MelodyLayer2;
    public AudioSource TextureLayer1;
    public AudioSource TextureLayer2;

    private AudioSource[] Sources = new AudioSource[5];
    private bool[] arr = new bool[5];

    // Start is called before the first frame update
    void Start()
    {
        Sources[0] = PercussionLayer;
        Sources[1] = MelodyLayer1;
        Sources[2] = MelodyLayer2;
        Sources[3] = TextureLayer1;
        Sources[4] = TextureLayer2;

        foreach (AudioSource source in Sources)
        {
            source.Play(0);
        }

        SyncSources();

        Debug.Log("started all layers");
    }

    //void OnGUI()
    //{
    //    if (GUI.Button(new Rect(10, 20, 150, 30), "Percussion Only"))
    //    {
    //        Sources[0].Play(0);
    //        Sources[1].Pause();
    //        Sources[2].Pause();
    //        Sources[3].Pause();
    //        Sources[4].Pause();

    //        Debug.Log("paused all layers: " + PercussionLayer.time);
    //    }

    //    if (GUI.Button(new Rect(10, 70, 150, 30), "All but percussion"))
    //    {
    //        Sources[0].Pause();
    //        Sources[1].UnPause();
    //        Sources[2].UnPause();
    //        Sources[3].UnPause();
    //        Sources[4].UnPause();

    //        SyncSources();
    //        Debug.Log("melody layers o: " + PercussionLayer.time);
    //    }

    //    if (GUI.Button(new Rect(10, 120, 150, 30), "MelodyLayers1 and 2"))
    //    {
    //        foreach (AudioSource source in Sources)
    //        {
    //            source.UnPause();
    //        }

    //        EnableLayers(new bool[] { false, true, true, false, false });
    //        Debug.Log("just MelodyLayers " + PercussionLayer.time);
    //    }

    //    if (GUI.Button(new Rect(10, 170, 150, 30), "TextureLayers 1 and 2"))
    //    {
    //        foreach (AudioSource source in Sources)
    //        {
    //            source.UnPause();
    //        }

    //        EnableLayers(new bool[] { false, false, false, true, true });
    //        Debug.Log("just the TextureLayers" + PercussionLayer.time);
    //    }
    //}

    //LayerStates[] corresponds to Sources[]
    private void EnableLayers(bool[] LayerStates)
    {
        // FIXME
        for (int i = 0; i < 5; i++)
        {
            if (LayerStates[i] == true)
            {
                Sources[i].volume = 1.0f;
            }
            else if (LayerStates[i] == false)
            {
                Sources[i].volume = 0.0f;
            }
        }
    }


    private IEnumerator SyncSources()
    {
        AudioSource Clock = PercussionLayer;
        foreach (AudioSource source in Sources)
        {
            source.timeSamples = Clock.timeSamples;
        }
        yield return null;
    }
}
