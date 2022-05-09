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

namespace GameSpec.Metadata.View
{
    /// <summary>
    /// Interaction logic for AudioPlayer.xaml
    /// </summary>
    public partial class AudioPlayer : UserControl, INotifyPropertyChanged
    {
        WaveOutEvent _waveOut = new();
        WaveStream _waveStream;

        public AudioPlayer()
        {
            InitializeComponent();
            _waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
            Unloaded += AudioPlayer_Unloaded;
        }

        void AudioPlayer_Unloaded(object sender, RoutedEventArgs e)
        {
            _waveOut?.Dispose();
            _waveOut = null;
            _waveStream?.Dispose();
            _waveStream = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
                    _waveStream = Format.ToLowerInvariant() switch
                    {
                        "wav" => new WaveFileReader(Stream),
                        "mp3" => new Mp3FileReader(Stream, wf => new Mp3FrameDecompressor(wf)),
                        "aac" => new StreamMediaFoundationReader(Stream),
                        _ => throw new ArgumentOutOfRangeException(nameof(Format), Format),
                    };
                    _waveOut.Init(_waveStream);
                }
                catch (Exception e) { Console.Error.WriteLine(e); }
        }

        void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
            => PlayButton.Content = "Play";

        void Play_Click(object sender, RoutedEventArgs e)
        {
            if (_waveOut.PlaybackState == PlaybackState.Stopped && _waveStream != null) _waveStream.Position = 0;
            if (_waveOut.PlaybackState == PlaybackState.Playing) { _waveOut.Pause(); PlayButton.Content = "Play"; }
            else { _waveOut.Play(); PlayButton.Content = "Pause"; }
        }
    }
}
