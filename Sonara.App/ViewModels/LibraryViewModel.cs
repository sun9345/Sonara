using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sonara.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sonara.App
{
    public partial class LibraryViewModel(ILibraryRepository repository) : ObservableObject
    {
        public ObservableCollection<TrackInfo> Tracks { get; } = new();

        [ObservableProperty]
        private string searchText = "";

        [RelayCommand]
        public async Task LoadAsync()
        {
            var tracks = await repository.GetAllTracksAsync();
            Tracks.Clear();
            foreach (var t in tracks) Tracks.Add(t);
        }

        [RelayCommand]  
        private async Task SearchAsync()
        {
            var tracks = string.IsNullOrWhiteSpace(SearchText)
                ? await repository.GetAllTracksAsync()
                : await repository.SearchAsync(SearchText);
            Tracks.Clear();
            foreach (var t in tracks) Tracks.Add(t);
        }
    }
}
