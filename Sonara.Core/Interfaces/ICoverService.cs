using Sonara.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonara.Core
{
    public interface ICoverService
    {
        /// <summary>
        /// Возвращает пуь к обложке трека в нужном размере, хеш считается пока что при каждом вызове
        /// </summary>
        Task<string?> GetCoverPathAsync(TrackInfo track, CoverSize size, CancellationToken ct);
    }
}
