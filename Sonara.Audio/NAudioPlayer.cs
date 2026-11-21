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
        private WaveOut? _outputDevice;
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
            if (_disposed)
                throw new ObjectDisposedException(nameof(NAudioPlayer));

            if (_outputDevice == null || _state != PlayerState.Played)
                return;
            _outputDevice.Pause();
            _state = PlayerState.Paused;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (sender != _outputDevice)
                return;
            if (_disposed)
                return;

            _state = PlayerState.Stopped;
            StateChanged?.Invoke(this, EventArgs.Empty);

            if(_stopRequested)
            {
                _stopRequested = false;
                return;
            }

            if (e.Exception == null)
                TrackEnded?.Invoke(this, EventArgs.Empty);
        }

        public Task PlayAsync(string path)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NAudioPlayer));

            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("Путь к аудиофайлу не может быть пустым.",nameof(path));

            AudioFileReader? newAudioFile = null;
            WaveOut? newOutputDevice = null;

            try
            {
                newAudioFile = new AudioFileReader(path);
                newAudioFile.Volume = _volume;
                newOutputDevice = new WaveOut();
                newOutputDevice.Init(newAudioFile);
            }
            catch
            {
                newOutputDevice?.Dispose();
                newAudioFile?.Dispose();
                throw;
            }

            if (_outputDevice != null)
            {
                _outputDevice.PlaybackStopped -= OnPlaybackStopped;
                _outputDevice.Stop();
                _outputDevice.Dispose();
                _outputDevice = null;
            }
            _audioFile?.Dispose();
            _audioFile = null;
            _audioFile = newAudioFile;
            _outputDevice = newOutputDevice;
            _stopRequested = false;
            newOutputDevice.PlaybackStopped += OnPlaybackStopped;

            try
            {
                newOutputDevice.Play();
            }
            catch 
            {
                newOutputDevice.PlaybackStopped -= OnPlaybackStopped;
                newOutputDevice.Dispose();
                newAudioFile.Dispose();

                _outputDevice = null;
                _audioFile = null;
                _state = PlayerState.Stopped;
                StateChanged?.Invoke(this, EventArgs.Empty);
                throw;
            }

            _state = PlayerState.Played;
            StateChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        public void Resume()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NAudioPlayer));

            if (_outputDevice == null || _state != PlayerState.Paused)
                return;

            _outputDevice.Play();
            _state = PlayerState.Played;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Seek(TimeSpan position)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NAudioPlayer));

            if (_audioFile == null)
                return;

            if (position < TimeSpan.Zero)
                position = TimeSpan.Zero;

            if (position > _audioFile.TotalTime)
                position = _audioFile.TotalTime;
            _audioFile.CurrentTime = position;
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
