using Sonara.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fakes
{
    public class FakeTagService : ITagService
    {
        public async Task<TrackInfo> ReadAsync(string path, CancellationToken ct = default)
        {
            await Task.Delay(50, ct);
            return new TrackInfo(
                path, 
                Path.GetFileNameWithoutExtension(path),
                "Unknown Artist", 
                "Unknown Album", 
                null, 
                "", 
                null,
                "jazz",
                TimeSpan.FromMinutes(3), 
                Path.GetExtension(path).TrimStart('.'));
        }

        public async Task WriteAsync(string path, TrackInfo tags, CancellationToken ct = default)
        {
            await Task.Delay(100, ct);   // ничего не пишет
        }
    }
}
