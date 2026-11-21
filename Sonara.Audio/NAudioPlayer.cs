using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sonara.Core;
using NAudio.Wave;

namespace Sonara.Audio
{
    public class NAudioPlayer : IAudioPlayer
    {
        private AudioFileReader? _audioFile;
        private WaveOutEvent? _outputDevice;
        private PlayerState _state = PlayerState.Stopped;
        private float _volume = 1.0f;
        private bool _stopRequested;
        private bool _disposed;

        public PlayerState State => _state;

        public TimeSpan Position => _audioFile?.CurrentTime ?? TimeSpan.Zero;

        public float Volume 
        { 
            get => _volume;
            set 
            { 
                _volume = Math.Clamp(value, 0.0f, 1.0f);
                if (_audioFile != null)
                {
                    _audioFile.Volume = _volume;
                }
            } 
        }

        public event EventHandler? StateChanged;
        public event EventHandler? TrackEnded;

        public void Dispose()
        {
            if (_disposed)
                return;

            _stopRequested = true;
            _outputDevice?.Stop();
            _outputDevice?.Dispose();
            _outputDevice = null;

            _disposed = true;
            _audioFile?.Dispose();
            _audioFile = null;
            _state = PlayerState.Stopped;
        }

        public void Pause()
        {
            if (_outputDevice == null || _state != PlayerState.Played)
                return;
            _outputDevice.Pause();
            _state = PlayerState.Paused;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public Task PlayAsync(string path)
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            if(_outputDevice == null || _state != PlayerState.Paused)
                return;

            _outputDevice.Play();
            _state = PlayerState.Played;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Seek(TimeSpan position)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
