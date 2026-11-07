using Sonara.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fakes
{
    public class FakeAudioPlayer : IAudioPlayer
    {
        private const int TickMs = 250;
        private static readonly TimeSpan FakeTrackDuration = TimeSpan.FromSeconds(30);

        private readonly object _lock = new();
        private readonly System.Threading.Timer _timer;

        private TimeSpan _position;
        private TimeSpan _duration; // ноль, пока трек не загружен
        private PlayerState _state = PlayerState.Stopped;
        private float _volume = 1f;
        private bool _disposed;

        public FakeAudioPlayer()
        {
            _timer = new System.Threading.Timer(OnTick, null, Timeout.Infinite, TickMs);
        }

        public PlayerState State { get { lock (_lock) return _state; } }
        public TimeSpan Position { get { lock (_lock) return _position; } }
        public TimeSpan Duration { get { lock (_lock) return _duration; } }

        public float Volume
        {
            get { lock (_lock) return _volume; }
            set { lock (_lock) _volume = Math.Clamp(value, 0f, 1f); }
        }

        public event EventHandler? StateChanged;
        public event EventHandler? TrackEnded;

        public Task PlayAsync(string path)
        {
            ThrowIfDisposed();

            lock (_lock)
            {
                _position = TimeSpan.Zero;
                _duration = FakeTrackDuration;
            }

            _timer.Change(TickMs, TickMs);
            SetState(PlayerState.Played);
            _timer.Change(250, 250);
            return Task.CompletedTask;
        }

        public void Pause()
        {
            ThrowIfDisposed();
            lock (_lock)
            {
                if (_state != PlayerState.Played) return;
            }
            _timer.Change(Timeout.Infinite, TickMs);
            SetState(PlayerState.Paused);
        }

        public void Resume()
        {
            ThrowIfDisposed();
            lock (_lock)
            {
                if (_state != PlayerState.Paused) return;
            }
            _timer.Change(TickMs, TickMs);
            SetState(PlayerState.Played);
        }

        public void Stop()
        {
            ThrowIfDisposed();
            _timer.Change(Timeout.Infinite, TickMs);
            lock (_lock) _position = TimeSpan.Zero;
            SetState(PlayerState.Stopped);
        }

        public void Seek(TimeSpan position)
        {
            ThrowIfDisposed();
            lock (_lock)
            {
                if (_duration == TimeSpan.Zero) return; // трек не загружен
                _position = TimeSpan.FromTicks(Math.Clamp(position.Ticks, 0, _duration.Ticks));
            }
        }

        private void OnTick(object? _)
        {
            bool ended;
            lock (_lock)
            {
                if (_disposed || _state != PlayerState.Played) return;
                _position += TimeSpan.FromMilliseconds(TickMs);
                ended = _position >= _duration;
            }

            if (ended)
            {
                Stop();
                TrackEnded?.Invoke(this, EventArgs.Empty);   // только при естественном конце
            }
        }

        private void SetState(PlayerState newState)
        {
            lock (_lock)
            {
                if (_state == newState) return;
                _state = newState;
            }
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _timer.Dispose();
        }
    }
}
