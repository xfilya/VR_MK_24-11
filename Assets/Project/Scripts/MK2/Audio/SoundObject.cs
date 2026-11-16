using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sounds
{
    public class SoundObject : AudioObject 
    {
        [SerializeField] private AfterPlayBehaviour _afterPlayBehaviour;
        [SerializeField] private AudioClip[] _clips;
        [SerializeField, Range(0f, 1f)] private float _randomPitch;
        [SerializeField] private bool _changeClipOnEnable;

        [SerializeField] private float _timeBetweenSounds = 0f;
        private float _lastPlayTime;
        private void OnEnable()
        {
            if (_changeClipOnEnable)
            {
                Stop();
                Play();
            }
        }

        protected override void Update()
        {
            base.Update();
            if (this.HasPlayed && !this.Source.isPlaying)
            {
                if (this._afterPlayBehaviour == AfterPlayBehaviour.PlayAnother)
                {
                    if (_timeBetweenSounds > 0 && Time.timeSinceLevelLoad - _lastPlayTime < _timeBetweenSounds)
                    {
                        return;
                    }
                    this.Play();
                    return;
                }
                if(this._afterPlayBehaviour == AfterPlayBehaviour.DestroyScript) 
                {
                    Object.Destroy(this);
                    Destroy(Source);
                    return;
                }
                if(this._afterPlayBehaviour == AfterPlayBehaviour.DestroyObject)
                {
                    Object.Destroy(base.gameObject);
                }
            }
        }

        public override void Play()
        {
            if (_clips.Length > 1)
            {
                this.Source.clip = this._clips[Random.Range(0, this._clips.Length)];
            }
            else
            {
                this.Source.clip = _clips[0];
            }
            if (this._randomPitch > 0f)
            {
                this.Source.pitch = 1f + Random.Range(-this._randomPitch, this._randomPitch);
            }
            _lastPlayTime = Time.timeSinceLevelLoad;
            base.Play();
        }

        public override void Stop()
        {
            base.Stop();
            this.HasPlayed = false;
        }

        public void SetSilent(bool silent)
        {
            if (!this.Source)
            {
                return;
            }
            if (silent)
            {
                this.Source.volume = 0f;
                return;
            }
            this.SetVolume(this.Volume);
        }

        public void SetClips(AudioClip[] clips)
        {
            _clips = clips;
        }
    }
}
