using System;
using System.IO;
using System.Threading.Tasks;
using Sonara.Audio;
using Sonara.Core;

namespace Sonara.Audio.Tests
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var player = new NAudioPlayer();

            player.StateChanged += (sender, e) =>
            {
                Console.WriteLine($"Состояние плеера: {player.State}");
            };

            player.TrackEnded += (sender, e) =>
            {
                Console.WriteLine("Трек закончился.");
            };

            Console.WriteLine("Введите полный путь к аудиофайлу:");
            string? path = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                Console.WriteLine("Файл не найден.");
                return;
            }

            try
            {
                await player.PlayAsync(path);
                Console.WriteLine("Воспроизведение началось.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось запустить файл: {ex.Message}");
                return;
            }

            bool isRunning = true;

            while (isRunning)
            {
                Console.WriteLine();
                Console.WriteLine("Выберите действие:");
                Console.WriteLine("1 — Пауза");
                Console.WriteLine("2 — Продолжить");
                Console.WriteLine("3 — Остановить");
                Console.WriteLine("4 — Перемотать");
                Console.WriteLine("5 — Изменить громкость");
                Console.WriteLine("6 — Показать позицию");
                Console.WriteLine("0 — Выход");

                string? command = Console.ReadLine();

                switch (command)
                {
                    case "1":
                        player.Pause();
                        break;

                    case "2":
                        player.Resume();
                        break;

                    case "3":
                        player.Stop();
                        Console.WriteLine("Воспроизведение остановлено.");
                        break;

                    case "4":
                        Console.WriteLine("Введите позицию в секундах:");
                        if (double.TryParse(Console.ReadLine(), out double seconds))
                        {
                            player.Seek(TimeSpan.FromSeconds(seconds));
                            Console.WriteLine($"Текущая позиция: {player.Position}");
                        }
                        else
                        {
                            Console.WriteLine("Некорректное значение.");
                        }
                        break;

                    case "5":
                        Console.WriteLine("Введите громкость от 0 до 1:");
                        if (float.TryParse(Console.ReadLine(), out float volume))
                        {
                            player.Volume = volume;
                            Console.WriteLine($"Громкость: {player.Volume}");
                        }
                        else
                        {
                            Console.WriteLine("Некорректное значение.");
                        }
                        break;

                    case "6":
                        Console.WriteLine($"Позиция: {player.Position}");
                        Console.WriteLine($"Состояние: {player.State}");
                        break;

                    case "0":
                        isRunning = false;
                        break;

                    default:
                        Console.WriteLine("Неизвестная команда.");
                        break;
                }
            }

            player.Stop();
            Console.WriteLine("Тестирование завершено.");
        }
    }
}
