using Fakes;
using Sonara.Core;
using Sonara.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonara.App
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddSonaraServices(this IServiceCollection s)
        {
            s.AddSingleton<ILibraryRepository, InMemoryLibraryRepository>();  // позже: SqliteLibraryRepository (щас заглушки тут)
            s.AddSingleton<ITagService, FakeTagService>();                    // позже: AtlTagService
            s.AddSingleton<IAudioPlayer, FakeAudioPlayer>();                  // позже: NAudioPlayer
            s.AddSingleton<ICoverService>(sp => new CoverService(sp.GetRequiredService<ITagService>()));
            return s;
        }
    }
}
