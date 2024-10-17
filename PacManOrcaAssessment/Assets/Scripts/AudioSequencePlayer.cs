using UnityEngine;

public class AudioSequencePlayer : MonoBehaviour
{
    private AudioSource[] audioSources;
    private int currentAudioIndex = 0;

    void Start()
    {
        // Get all AudioSources attached to this GameObject
        audioSources = GetComponents<AudioSource>();

        foreach (var audioSource in audioSources)
        {
            audioSource.Stop();
        }
        
        // Play the first audio clip
        if (audioSources.Length > 0)
        {
            PlayAudioClip(currentAudioIndex);
        }
    }

    void Update()
    {
        // Check if the current clip is no longer playing
        if (!audioSources[currentAudioIndex].isPlaying)
        {
            // Move to the next audio clip
            if (currentAudioIndex < audioSources.Length - 1) {
            currentAudioIndex++;
            }
            
            PlayAudioClip(currentAudioIndex);
        }
    }

    void PlayAudioClip(int index)
    {
        if (index >= 0 && index < audioSources.Length)
        {
            audioSources[index].Play();
        }
    }
}