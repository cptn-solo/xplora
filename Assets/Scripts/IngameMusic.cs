using Assets.Scripts.UI.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class IngameMusic : MonoBehaviour
    {
        [SerializeField] private AudioClip[] sounds;
        
        private AudioSource audioSource;
        private readonly Dictionary<string, AudioClip> soundsDict = new();

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            foreach (var clip in sounds)
                soundsDict.Add(clip.name, clip);
        }

        public void Play(SFX sfx)
        {
            Debug.Log($"Play: sfx {sfx}");
            var clip = soundsDict[sfx.FileName];
            
            Debug.Log($"Play: clip {clip.name}");

            StartCoroutine(nameof(ChangeTheme), clip);
        }

        public void Stop()
        {
            if (audioSource.clip != null)
                audioSource.Stop();
        }

        private IEnumerator ChangeTheme(AudioClip theme)
        {
            Debug.Log($"ChangeTheme {theme.name}");
            audioSource.Stop();

            while (audioSource.isPlaying)
                yield return null;

            if (theme != null)
            {
                Debug.Log("ChangeTheme: PLAY");

                audioSource.clip = theme;
                audioSource.Play();
            }
            yield return null;
        }
    }
}