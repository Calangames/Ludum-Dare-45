using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.spatialBlend = s.spatialBlend;
            s.source.loop = s.loop;
        }
    }

    public void Play (string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    public void PlayDelayed (string name, float delay)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.PlayDelayed(delay);
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Stop();
    }

    public void FadeIn(string name, float duration, float delay = 0f)
    {
        if (duration > 0)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            if (s.RunningCoroutine)
            {
                StopCoroutine(s.coroutine);
            }
            s.coroutine = FadeInCoroutine(s, duration, delay);
            StartCoroutine(s.coroutine);
        }
    }

    public void FadeOut(string name, float duration, float delay = 0f)
    {
        if (duration > 0)
        {
            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!");
                return;
            }
            if (s.RunningCoroutine)
            {
                StopCoroutine(s.coroutine);
            }
            s.coroutine = FadeOutCoroutine(s, duration, delay);
            StartCoroutine(s.coroutine);
        }
    }

    public void Crossfade(string currentName, string nextName, float duration, float delay = 0f)
    {
        if (duration > 0)
        {
            Sound currentS = Array.Find(sounds, sound => sound.name == currentName);
            if (currentS == null)
            {
                Debug.LogWarning("Sound: " + currentName + " not found!");
                return;
            }
            Sound nextS = Array.Find(sounds, sound => sound.name == nextName);
            if (nextS == null)
            {
                Debug.LogWarning("Sound: " + nextName + " not found!");
                return;
            }
            if (currentS.RunningCoroutine)
            {
                StopCoroutine(currentS.coroutine);
            }
            if (nextS.RunningCoroutine)
            {
                StopCoroutine(nextS.coroutine);
            }
            currentS.coroutine = FadeOutCoroutine(currentS, duration, delay);
            nextS.coroutine = FadeInCoroutine(nextS, duration, delay);
            StartCoroutine(currentS.coroutine);
            StartCoroutine(nextS.coroutine);
        }
    }

    private IEnumerator FadeInCoroutine(Sound s, float duration, float delay)
    {
        s.RunningCoroutine = true;
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        s.source.volume = 0f;
        s.source.Play();
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            s.source.volume = Mathf.Min(s.volume, s.source.volume + (Time.deltaTime * s.volume / duration));
            yield return null;
        }
        s.RunningCoroutine = false;
    }

    private IEnumerator FadeOutCoroutine(Sound s, float duration, float delay)
    {
        s.RunningCoroutine = true;
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        float initialVolume = s.source.volume;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            s.source.volume -= (Time.deltaTime * s.volume / duration);
            yield return null;
        }
        s.source.Stop();
        s.source.volume = initialVolume;
        s.RunningCoroutine = false;
    }
}