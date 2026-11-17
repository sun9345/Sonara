using ATL;
using CommunityToolkit.Mvvm.ComponentModel;
using Sonara.Core;

namespace Sonara.App;

[QueryProperty(nameof(Track), "Track")]
public partial class TagEditorViewModel : ObservableObject
{
    [ObservableProperty]
    private TrackInfo? track;

    // TODO: поля формы, загрузка, проверка, сохранение
}