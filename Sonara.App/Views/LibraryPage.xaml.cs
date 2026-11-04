namespace Sonara.App
{
    public partial class LibraryPage : ContentPage
    {
        private readonly LibraryViewModel _viewModel;

        public LibraryPage(LibraryViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
        }
    }
}