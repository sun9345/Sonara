using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonara.Core
{
    public interface IAudioPlayer : IDisposable
    {
        PlayerState State { get; }
        TimeSpan Position { get; }
        float Volume { get; set; }
        event EventHandler? StateChanged;
        event EventHandler? TrackEnded;
        Task PlayAsync(string path);
        void Pause();
        void Resume();
        void Stop();
        void Seek(TimeSpan position);
    }
}
