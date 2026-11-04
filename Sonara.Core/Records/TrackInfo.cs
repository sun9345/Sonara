using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonara.Core
{
    public record TrackInfo(
        string FilePath,
        string Title,
        string Artist,
        string Album,
        int? TrackNumber,
        string AlbumArtists,
        int? Year,
        string Genre,
        TimeSpan Duration,
        string Format
        );
}
