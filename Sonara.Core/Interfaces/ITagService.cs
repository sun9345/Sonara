using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonara.Core
{
    public interface ITagService
    {
        /// <summary>
        /// считывает теги трека
        /// </summary>
        Task<TrackInfo> ReadAsync(string path, CancellationToken ct = default);

        /// <summary>
        /// обновляет теги трека
        /// </summary>
        Task WriteAsync(string path, TrackInfo tags, CancellationToken ct = default);

        /// <summary>
        /// считывает обложку трека как массив байтов 
        /// </summary>
        Task<byte[]?> ReadCoverAsync(string path, CancellationToken ct = default);

        /// <summary>
        /// записывает в трек обложку
        /// </summary>
        Task WriteCoverAsync(string path, byte[] image, string mimeType, CancellationToken ct = default);
    }
}
