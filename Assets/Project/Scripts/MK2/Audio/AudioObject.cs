using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Sounds
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioObject : MonoBehaviour
    {
        [SerializeField] protected bool PlayAtStart = true;

        [SerializeField] protected AudioSource Source;

        [SerializeField] protected UnityEvent AfterPlayEvent;

        [SerializeField][Range(0f, 1f)] public float Volume = 1f;

        [SerializeField] private float _volumeSlideBaseTime = 1f;

        [SerializeField] private bool _playAudioReversed;

        public float Pitch { get => Source.pitch; set => Source.pitch = value; }
        public float SpatialBlend { get => Source.spatialBlend; set => Source.spatialBlend = value; }

        public bool Loop
        {
            get => Source.loop; 
            set => Source.loop = value;
        }
        
        private enum AudioType
        {
             Sound,
             Music
        }

        [SerializeField] private AudioType _type;

        protected bool HasPlayed;

        [SerializeField] private bool _resetAfterEventPlayedOnPlay;

        private bool _afterEventPlayed;
        private void Awake()
        {
            if (!this.Source)
            {
                Source = GetComponent<AudioSource>();                
            }
        }
        protected virtual void Start()
        {        
            if (this.PlayAtStart)
            {
                this.Play();
            }
        }

        protected virtual void Update()
        {
            if (this.HasPlayed && !this.Source.isPlaying && !this._afterEventPlayed)
            {
                this._afterEventPlayed = true;
                if (this.AfterPlayEvent != null)
                {
                    this.AfterPlayEvent.Invoke();
                }
            }
        }

        public virtual void Play()
        {
            if (_playAudioReversed)
            {
                Source.pitch = -Mathf.Abs(Source.pitch);
                Source.timeSamples = Source.clip.samples - 1;
            }

            this.SetVolume(this.Volume);
            this.Source.Play();
            this.HasPlayed = true;
            if(_resetAfterEventPlayedOnPlay)
            {
                _afterEventPlayed = false;
            }
        }

        public virtual void StartVolumeSliding(float newVolume, float slideDuration)
        {
            StartCoroutine(SlideVolumeToNewValue(newVolume, slideDuration));
        }

        public virtual void StartVolumeSliding(float newVolume)
        {
            StartCoroutine(SlideVolumeToNewValue(newVolume, _volumeSlideBaseTime));
        }

        public virtual IEnumerator SlideVolumeToNewValue(float newVolume, float slideDuration)
        {
            float timeCounter = 0;
            //Debug.Log($"timeCounter {timeCounter} newVolume is {newVolume} slideDuration is {slideDuration}");
            while(timeCounter < slideDuration)
            {
                //Debug.Log("setting");
                timeCounter += Time.deltaTime;
                float slideProcessTime = Mathf.Abs(Volume - newVolume) / (slideDuration - timeCounter) * Time.deltaTime;

                SetVolume(Mathf.MoveTowards(Volume, newVolume, slideProcessTime));
                yield return null;
            }

            //Debug.Log("done!");
            SetVolume(newVolume);
        }

        public virtual void Stop()
        {
            this.Source.Stop();
        }

        public virtual void SetVolume(float newVolume)
        {
            Source.volume = newVolume;
        }


        public void SetMinDistance(float minDistance)
        {
            Source.minDistance = minDistance;
        }

        public void SetClip(AudioClip clip)
        {
            Source.clip = clip;
        }

        public AudioClip GetClip()
        {
            return Source.clip;
        }

        public bool IsPlaying()
        {
            return this.Source.isPlaying;
        }

        public void PlayOneShot(AudioClip clip) => Source.PlayOneShot(clip);
        public void StopPlayback() => Source.Stop();

    }
}
