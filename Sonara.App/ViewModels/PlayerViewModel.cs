
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sonara.Core;

namespace Sonara.App.ViewModels
{
    public partial class PlayerViewModel : ObservableObject
    {
        private readonly IAudioPlayer audioPlayer;

        public PlayerViewModel(IAudioPlayer audioPlayer)
        {
            this.audioPlayer = audioPlayer;
            audioPlayer.StateChanged += OnPlayerStateChanged;

            UpdateStateText();
        }

        [ObservableProperty]
        private string stateText = "Остановлено";

        [ObservableProperty]
        private float volume;

        [RelayCommand]
        private void Play()
        {
            if (audioPlayer.State == PlayerState.Paused)
            {
                audioPlayer.Resume();
            }
        }

        [RelayCommand]
        private void Pause()
        {
            audioPlayer.Pause();
        }

        [RelayCommand]
        private void Stop()
        {
            audioPlayer.Stop();
        }

        private void OnPlayerStateChanged(object? sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(UpdateStateText);
        }
        partial void OnVolumeChanged(float value)
        {
            audioPlayer.Volume = value;
        }
        private void UpdateStateText()
        {
            StateText = audioPlayer.State switch
            {
                PlayerState.Played => "Играет",
                PlayerState.Paused => "Пауза",
                _ => "Остановлено"
            };

            Volume = audioPlayer.Volume;
        }
    }
}
