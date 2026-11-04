using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonara.Core
{
    public interface ILibraryRepository
    {
        Task<IReadOnlyList<TrackInfo>> GetAllTracksAsync();
        Task<IReadOnlyList<TrackInfo>> SearchAsync(string query);
        Task UpsertTracksAsync(IEnumerable<TrackInfo> tracks);
        Task<IReadOnlyList<string>> GetLibraryFoldersAsync();
        Task AddLibraryFolderAsync(string path);
    }
}
