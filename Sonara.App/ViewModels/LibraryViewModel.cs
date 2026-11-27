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

        public IReadOnlyList<string> SortOptions { get; } =
            new[] { "По названию", "По исполнителю", "По альбому" };

        private List<TrackInfo> _currentTracks = new();

        [ObservableProperty]
        private string searchText = "";

        [ObservableProperty]
        private int selectedSortIndex;

        partial void OnSelectedSortIndexChanged(int value) => ApplySort();

        [RelayCommand]
        public async Task LoadAsync()
        {
            var tracks = await repository.GetAllTracksAsync();
            ShowTracks(tracks);
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            var tracks = string.IsNullOrWhiteSpace(SearchText)
                ? await repository.GetAllTracksAsync()
                : await repository.SearchAsync(SearchText);
            ShowTracks(tracks);
        }

        private void ShowTracks(IEnumerable<TrackInfo> tracks)
        {
            _currentTracks = tracks.ToList();
            ApplySort();
        }

        private void ApplySort()
        {
            var comparer = NaturalStringComparer.Instance;

            IEnumerable<TrackInfo> sorted = SelectedSortIndex switch
            {
                1 => _currentTracks.OrderBy(t => t.Artist, comparer).ThenBy(t => t.Title, comparer),
                2 => _currentTracks.OrderBy(t => t.Album, comparer).ThenBy(t => t.Title, comparer),
                _ => _currentTracks.OrderBy(t => t.Title, comparer),
            };

            Tracks.Clear();
            foreach (var t in sorted)
                Tracks.Add(t);
        }
    }
}