using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sonara.Core;

namespace Sonara.Audio
{
    public class NAudioPlayer : IAudioPlayer
    {
        public PlayerState State => throw new NotImplementedException();

        public TimeSpan Position => throw new NotImplementedException();

        public float Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event EventHandler? StateChanged;
        public event EventHandler? TrackEnded;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public Task PlayAsync(string path)
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
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
