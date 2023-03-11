using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GameSpec.Metadata;
using NAudio.Wave;
using System.IO;
using NLayer.NAudioSupport;

namespace GameSpec.App.Explorer.Views
{
    /// <summary>
    /// Interaction logic for AudioPlayer.xaml
    /// </summary>
    public partial class AudioPlayer : UserControl, INotifyPropertyChanged
    {
        WaveOutEvent WaveOut = new();
        WaveStream WaveStream;

        public AudioPlayer()
        {
            InitializeComponent();
            WaveOut.PlaybackStopped += WaveOut_PlaybackStopped;
            Unloaded += AudioPlayer_Unloaded;
        }

        void AudioPlayer_Unloaded(object sender, RoutedEventArgs e)
        {
            WaveOut?.Dispose();
            WaveOut = null;
            WaveStream?.Dispose();
            WaveStream = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static readonly DependencyProperty StreamProperty = DependencyProperty.Register(nameof(Stream), typeof(Stream), typeof(AudioPlayer),
            new PropertyMetadata((d, e) => (d as AudioPlayer).LoadSound()));
        public Stream Stream
        {
            get => GetValue(StreamProperty) as Stream;
            set => SetValue(StreamProperty, value);
        }

        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(nameof(Format), typeof(string), typeof(AudioPlayer),
             new PropertyMetadata((d, e) => (d as AudioPlayer).LoadSound()));
        public string Format
        {
            get => GetValue(FormatProperty) as string;
            set => SetValue(FormatProperty, value);
        }

        void LoadSound()
        {
            if (Format != null && Stream != null)
                try
                {
                    WaveStream = Format.ToLowerInvariant() switch
                    {
                        ".wav" => new WaveFileReader(Stream),
                        ".mp3" => new Mp3FileReader(Stream, wf => new Mp3FrameDecompressor(wf)),
                        ".aac" => new StreamMediaFoundationReader(Stream),
                        _ => throw new ArgumentOutOfRangeException(nameof(Format), Format),
                    };
                    WaveOut.Init(WaveStream);
                }
                catch (Exception e) { Console.Error.WriteLine(e); }
        }

        void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e) => PlayButton.Content = "Play";

        void Play_Click(object sender, RoutedEventArgs e)
        {
            if (WaveOut.PlaybackState == PlaybackState.Stopped && WaveStream != null) WaveStream.Position = 0;
            if (WaveOut.PlaybackState == PlaybackState.Playing) { WaveOut.Pause(); PlayButton.Content = "Play"; }
            else { WaveOut.Play(); PlayButton.Content = "Pause"; }
        }
    }
}
