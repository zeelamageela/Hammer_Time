using UnityEngine.Audio;
using System;
using UnityEngine;


public class AudioManager : MonoBehaviour
{

    public Sound[] sounds;

    public static AudioManager instance;

    public HammerMixer hm;

    bool scrape;
    bool hit;
    GameObject rockGO;
    Rigidbody2D rb;
    float vel;
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);


        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        hm = GetComponent<HammerMixer>();
        hm.StartBG();
    }

    private void Update()
    {
        if (scrape)
        {
            float vel = rb.velocity.x;
            //Sound Array.Find(sounds, sound => sound.name == "RockScrape");

        }
    }

    public void PlayBG(int scenario)
    {
        hm = GetComponent<HammerMixer>();
        switch(scenario)
        {
            case 0:
                hm.EnableLayers(new bool[] { true, false, true, true, false, true });
                break;
            case 1:
                hm.EnableLayers(new bool[] { true, true, false, false, false, true });
                break;
            case 2:
                hm.EnableLayers(new bool[] { true, false, false, false, true, false });
                break;
            case 3:
                hm.EnableLayers(new bool[] { false, false, false, false, true, true });
                break;

            default:
                hm.EnableLayers(new bool[] { true, false, false, false, false, false });
                break;

        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        
        s.source.Stop();
    }

    public void Volume(string name, float volume)
    {
        if (name == "Theme")
        {
            for (int i = 0; i < hm.themeLayers.Length; i++)
            {
                foreach(Sound s in hm.themeLayers)
                {
                    s.source.volume = volume;
                }
            }
        }
        else
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);
            s.source.volume = volume;
        }
    }

    public void PlayHit(string name, float velocity)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        s.source.volume = (velocity) / (4f);
        s.source.Play();
    }

    public void PlayScrape(string name, GameObject rock)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        rb = rock.GetComponent<Rigidbody2D>();
        scrape = true;
        s.source.Play();
    }
}
