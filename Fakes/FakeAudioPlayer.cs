using Sonara.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fakes
{
    public class FakeAudioPlayer : IAudioPlayer, IDisposable
    {
        private readonly TimeSpan _fakeDuration = TimeSpan.FromSeconds(30);
        private readonly System.Threading.Timer _timer;
        private TimeSpan _position;
        private PlayerState _state = PlayerState.Stopped;

        public FakeAudioPlayer() => _timer = new(OnTick, null, Timeout.Infinite, 250);

        public PlayerState State => _state;
        public TimeSpan Position => _position;
        public float Volume { get; set; } = 1f;
        public event EventHandler? StateChanged;
        public event EventHandler? TrackEnded;

        public Task PlayAsync(string path)
        {
            _position = TimeSpan.Zero;
            SetState(PlayerState.Played);
            _timer.Change(250, 250);
            return Task.CompletedTask;
        }

        public void Pause() { _timer.Change(Timeout.Infinite, 250); SetState(PlayerState.Paused); }
        public void Resume() { _timer.Change(250, 250); SetState(PlayerState.Played); }
        public void Stop() { _timer.Change(Timeout.Infinite, 250); _position = TimeSpan.Zero; SetState(PlayerState.Stopped); }
        public void Seek(TimeSpan position) => _position = position;

        private void OnTick(object? _)
        {
            _position += TimeSpan.FromMilliseconds(250);
            if (_position >= _fakeDuration)
            {
                Stop();
                TrackEnded?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetState(PlayerState s) { _state = s; StateChanged?.Invoke(this, EventArgs.Empty); }
        public void Dispose() => _timer.Dispose();
    }
}
