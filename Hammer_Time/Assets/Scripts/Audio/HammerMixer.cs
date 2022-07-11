using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerMixer : MonoBehaviour
{
    
    public AudioSource PercussionLayer;
    public AudioSource MelodyLayer1;
    public AudioSource MelodyLayer2;
    public AudioSource TextureLayer1;
    public AudioSource TextureLayer2;

    // DM: fixme, proper sizes with instantiation of vars
    // DM: fixme, these public so AUdioManager can call 
    public int numLayers = 5;
    
    // DM: fixme, change this name, because we get Sources.source later..
    public Sound[] AudioSamples = new Sound[5];
    private bool[] arr = new bool[5];

    // Start is called before the first frame update
    void Start()
    {
        AudioSamples[0].source = PercussionLayer;
        AudioSamples[1].source = MelodyLayer1;
        AudioSamples[2].source = MelodyLayer2;
        AudioSamples[3].source = TextureLayer1;
        AudioSamples[4].source = TextureLayer2;

        foreach (Sound s in AudioSamples)
        {
            s.source.Play(0);
        }

        SyncSources();
        
        Debug.Log("started all layers");
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 20, 150, 30), "Percussion Only"))
        {
            AudioSamples[0].source.Play(0);
            AudioSamples[1].source.Pause();
            AudioSamples[2].source.Pause();
            AudioSamples[3].source.Pause();
            AudioSamples[4].source.Pause();

            Debug.Log("paused all layers: " + PercussionLayer.time);
        }

        if (GUI.Button(new Rect(10, 70, 150, 30), "All but percussion"))
        {
            AudioSamples[0].source.Pause();
            AudioSamples[1].source.UnPause();
            AudioSamples[2].source.UnPause();
            AudioSamples[3].source.UnPause();
            AudioSamples[4].source.UnPause();

            SyncSources();
            Debug.Log("melody layers o: " + PercussionLayer.time);
        }

        if (GUI.Button(new Rect(10, 120, 150, 30), "MelodyLayers1 and 2"))
        {
            foreach (Sound s in AudioSamples)
            {
                s.source.UnPause();
            }

            EnableLayers( new bool[] {false, true, true, false, false} );
            Debug.Log("just MelodyLayers " + PercussionLayer.time);
        } 

        if (GUI.Button(new Rect(10, 170, 150, 30), "TextureLayers 1 and 2"))
        {
            foreach (Sound s in AudioSamples)
            {
                s.source.UnPause();
            }

            EnableLayers( new bool[] {false, false, false, true, true} );
            Debug.Log("just the TextureLayers" + PercussionLayer.time);
        }
    }

    // TODO: combine wth below
    public void EnableLayers(bool[] LayerStates)
    {
        // FIXME
        for (int i = 0; i < 5; i++)
        {
            if(LayerStates[i] == true){
                AudioSamples[i].source.volume = 1.0f;
            }else if (LayerStates[i] == false){
                AudioSamples[i].source.volume = 0.0f;
            }
        }
    }    
    
    // TODO: combine with above
    public void EnableLayersByVolume(float[] LayerVolumes)
    {
        // FIXME
        for (int i = 0; i < 5; i++)
        {
            AudioSamples[i].source.volume = LayerVolumes[i];
        }
    }


    private IEnumerator SyncSources()
    {
        AudioSource Clock = PercussionLayer;
        foreach (Sound s in AudioSamples) 
        {
            s.source.timeSamples = Clock.timeSamples;
        }
        yield return null;
    }
}
