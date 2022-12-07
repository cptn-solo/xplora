using Assets.Scripts.Services.App;
using Assets.Scripts.UI.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Assets.Scripts
{
    public class IngameMusic : MonoBehaviour
    {

        [Inject] private readonly AudioPlaybackService audioService;

        [SerializeField] private AudioClip[] sounds;
        
        private AudioSource audioSource;
        private readonly Dictionary<string, AudioClip> soundsDict = new();

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            foreach (var clip in sounds)
                soundsDict.Add(clip.name, clip);
        }

        private void Start()
        {
            audioService.AttachMusic(this);
        }

        public void Play(SFX sfx) =>
            StartCoroutine(ChangeTheme(soundsDict[sfx.FileName]));

        public void Pause()
        {
            if (audioSource.isPlaying)
                audioSource.Pause();
        }

        public void Resume()
        {
            if (audioSource.clip != null)
                audioSource.Play();
        }

        public void Stop()
        {
            if (audioSource.clip != null)
                audioSource.Stop();
        }

        private IEnumerator ChangeTheme(AudioClip theme)
        {
            audioSource.Stop();

            while (audioSource.isPlaying)
                yield return null;

            if (theme != null)
            {
                audioSource.clip = theme;
                audioSource.Play();
            }
        }

        private void OnDisable()
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }

    }
}