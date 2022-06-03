using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

// another horrible way of handling things but yknow what screw it we ball    
[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }
    [FormerlySerializedAs("audioSource")]
    public AudioSource sfxSource;
    public AudioSource ambienceSource;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }

    private void Update()
    {
        if(MapManager.instance == null || !MapManager.instance.loadingFinished) return;
        ambienceSource.volume = CameraManager.cam.orthographicSize / CameraManager.maxZoomSize * 0.5f;
    }
    
    public static void Play(AudioClip clip, float volume)
    {
        instance.sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public static void PlayShortRanged(AudioClip clip, Vector3 position)
    {
        // by half-zoom, short range sounds will be silent
        var cameraZoomModifier = CameraManager.cam != null ? 1f - Mathf.Clamp01(CameraManager.cam.orthographicSize / (CameraManager.maxZoomSize / 2f)) : 0.5f;

        // by 20 units away from sound source, short-range sounds will be silent
        var distanceModifier = CameraManager.cam != null ? 1f - Mathf.Clamp01(Vector3.Distance(position, CameraManager.cam.transform.position + new Vector3(0, 0, 10)) / 20f) : 0.5f;
        var volume = 0.33f * distanceModifier * cameraZoomModifier;
        
        instance.sfxSource.PlayOneShot(clip, volume);
    }
    
    public static AudioClip GetAudioClip(string clip)
    {
        return Addressables.LoadAssetAsync<AudioClip>("Audio/" + clip).WaitForCompletion();
    }
}
