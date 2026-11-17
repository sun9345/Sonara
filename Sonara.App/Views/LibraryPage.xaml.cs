
using Sonara.App.ViewModels;
using Sonara.App.Views;

namespace Sonara.App
{
    public partial class LibraryPage : ContentPage
    {
        private readonly LibraryViewModel _viewModel;

        public LibraryPage(
            LibraryViewModel viewModel,
            PlayerViewModel playerViewModel)
        {
            InitializeComponent();

            BindingContext = _viewModel = viewModel;
            PlayerPanel.BindingContext = playerViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
        }
    }
}
