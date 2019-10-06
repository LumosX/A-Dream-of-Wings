using System;
using UnityEngine;

public class AudioController : MonoBehaviour {
    public AudioClip track1;
    public AudioClip track2;
    public AudioClip gainCash;
    public AudioClip bellNotification;
    public AudioClip startDialogue;
    public AudioClip selectDialogueOption;
    public AudioClip readNewsNoNotification;
    public AudioClip rain;


    public AudioSource rainSource;
    public AudioSource guitarSource;
    public AudioSource effectSource;

    public bool GuitarMuted { private set; get; } = false;
    
    
    public void ToggleRainNoise(bool onOff) {
        onOff = !onOff; // I HAVE NO IDEA WHAT I'VE FUCKED UP BUT THIS IS NECESSARY.
        rainSource.loop = true;
        rainSource.clip = rain;
        FadeRain(onOff);
        rainSource.Play();
    }


    public void ToggleGuitarTrack(bool onOff, int track = -1) {
        onOff = !onOff; // again, same deal, my fading code is backwards but I don't care
        
        guitarSource.loop = true;
        
        if (track == -1) track = PlayerController.RNG.NextDouble() > 0.5 ? 0 : 1;
        // guitarSource.clip = track == 0 ? track1 : track2;
        guitarSource.clip = track1;
        FadeGuitar(onOff);
        guitarSource.Play();
    }

    public void ToggleGuitarMute(bool onOff) {
        GuitarMuted = onOff;
        guitarFadeTime = Time.time - 0.2f; // abort any fades
        if (onOff) guitarSource.volume = 0;
        else guitarSource.volume = LoudestGuitarVol;
    }

    
    public void PlayEffect(AudioEffect effect) {
        switch (effect) {
            case AudioEffect.GainCash:
                effectSource.PlayOneShot(gainCash);
                break;
            case AudioEffect.BellNotification:
                effectSource.PlayOneShot(bellNotification);
                break;
            case AudioEffect.StartDialogue:
                effectSource.PlayOneShot(startDialogue);
                break;
            case AudioEffect.SelectDialogueOption:
                effectSource.PlayOneShot(selectDialogueOption);
                break;
            case AudioEffect.ReadNewsNoNotif:
                effectSource.PlayOneShot(readNewsNoNotification);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(effect), effect, null);
        }
    }

    public enum AudioEffect {
        GainCash,
        BellNotification,
        StartDialogue,
        SelectDialogueOption,
        ReadNewsNoNotif,
    }


    // I fucking hate coroutines, man
    private float guitarFadeTime = 0;
    private float guitarStartVol = 0;
    private float guitarTargetVol = LoudestGuitarVol;
    private float guitarFadeSpeed = 5f;
    private float rainFadeTime = 0;
    private float rainStartVol = 0;
    private float rainTargetVol = LoudestRainVol;
    private float rainFadeSpeed = 4f;
    
    
    private const float LoudestGuitarVol = 0.225f;
    private const float LoudestRainVol = 1;
    void Update() {

        if (guitarFadeTime > Time.time) {
            var lerpVal = (guitarFadeTime - Time.time) / guitarFadeSpeed;
            guitarSource.volume = Mathf.Lerp(guitarStartVol, guitarTargetVol, lerpVal);
        }
        
        if (rainFadeTime > Time.time) {
            var lerpVal = (rainFadeTime - Time.time) / rainFadeSpeed;
            rainSource.volume = Mathf.Lerp(rainStartVol, rainTargetVol, lerpVal);
        }
        
    }

    void FadeGuitar(bool fadeIn) {
        guitarFadeTime = Time.time + guitarFadeSpeed;
        // Fade in = vol 0 to target; fade out = vol target to 0
        guitarSource.volume = fadeIn ? 0 : LoudestGuitarVol;
        guitarStartVol = fadeIn ? 0 : LoudestGuitarVol;
        guitarTargetVol = fadeIn ? LoudestGuitarVol : 0;
    }

    void FadeRain(bool fadeIn) {
        rainFadeTime = Time.time + rainFadeSpeed;
        rainSource.volume = fadeIn ? 0 : LoudestRainVol;
        rainStartVol = fadeIn ? 0 : LoudestRainVol;
        rainTargetVol = fadeIn ? LoudestRainVol : 0;
    }
    
    // Coroutines are fucking garbage and never work properly. Case in point:
    //
    // // stolen off the unity forum somehwere
    // private static IEnumerator FadeOut(AudioSource audioSource, float fadeTime) {
    //     var startVolume = audioSource.volume;
    //     var adjustedVolume = startVolume;
    //
    //     while (adjustedVolume > 0) {
    //         adjustedVolume -= startVolume * Time.deltaTime / fadeTime;
    //         audioSource.volume = adjustedVolume;
    //         //Debug.Log(adjustedVolume);
    //         yield return null;
    //     }
    //
    //     audioSource.Stop();
    //     audioSource.volume = startVolume;
    // }
    //
    // // adapted from the stolen code above
    // private static IEnumerator FadeIn(AudioSource audioSource, float fadeTime, float targetVolume = 1.0f) {
    //     var startVolume = audioSource.volume;
    //     var adjustedVolume = 0.0f;
    //
    //     while (adjustedVolume < targetVolume) {
    //         adjustedVolume += startVolume * Time.deltaTime / fadeTime;
    //         audioSource.volume = adjustedVolume;
    //         //Debug.Log(adjustedVolume);
    //         yield return null;
    //     }
    // }
}