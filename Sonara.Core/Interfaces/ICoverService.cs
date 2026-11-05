using Sonara.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonara.Core.Interfaces
{
    public interface ICoverService
    {
        Task<string?> GetCoverPathAsync(TrackInfo track, CoverSize size, CancellationToken ct);
    }
}
