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

    private AudioSource[] Sources;

    public int musicFade;

    private bool[] arr = new bool[5];

    public Sound[] themeLayers;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void StartBG()
    {
        Sources = new AudioSource[themeLayers.Length];

        foreach (Sound s in themeLayers)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        Debug.Log("Sources.Length is " + Sources.Length);

        //for (int i = 0; i < Sources.Length; i++)
        //{
        //    Sources[i] = themeLayers[i].source;
        //}

        foreach (Sound s in themeLayers)
        {
            s.source.Play(0);
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

    public void EnableLayers(bool[] LayerStates)
    {
        if (themeLayers[0].source == null)
        {
            StartBG();
        }

        // FIXME
        for (int i = 0; i < LayerStates.Length; i++)
        {
            if (LayerStates[i] == true)
            {
                themeLayers[i].source.volume = 1f;
            }
            else if (LayerStates[i] == false)
            {
                themeLayers[i].source.volume = 0.0f;
            }
        }
    }

    private IEnumerator SyncSources()
    {
        AudioSource Clock = PercussionLayer;
        foreach (Sound s in themeLayers)
        {
            s.source.timeSamples = Clock.timeSamples;
        }
        yield return null;
    }
}
