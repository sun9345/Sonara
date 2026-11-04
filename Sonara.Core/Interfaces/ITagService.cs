using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonara.Core
{
    public interface ITagService
    {
        Task<TrackInfo> ReadAsync(string path, CancellationToken ct = default);
        Task WriteAsync(string path, TrackInfo tags, CancellationToken ct = default);
    }
}
