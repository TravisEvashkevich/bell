using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using AudioTool.Core;
using Microsoft.Win32;
using Microsoft.Xna.Framework.Audio;
using Newtonsoft.Json;

namespace AudioTool.Data
{
//    - Parallel (all sounds in the cue play at the same time)
//- Serial (all sounds play one after the other)
//- Cycle(first cue plays sound1, second cue plays sound2, repeating etc.)
//- Random (each cue plays randomly one of the sounds in it)
//- RandomCycle (each cue plays randomly but does not replay a sound until all other sounds have been played, 
//i.e. sound1, sound3, sound2, sound1, for a three sound cue)
    public enum CuePlaybackMode
    {
        Parallel,
        Serial,
        Cycle,
        Random,
        RandomCycle
    }

    public sealed class Cue : NodeWithName
    {
        private CuePlaybackMode _playbackInUse;

        [JsonProperty("sounds")]
        public override ObservableCollection<INode> Children
        {
            get
            {
                return _children;
            }
            set
            {
                Set(ref _children, value);
                Glue.DocumentIsSaved = false;
            }
        }

        #region Pitch

        private float _pitch;

        [JsonProperty("pitch")]
        public float Pitch
        {
            get { return _pitch; }
            set
            {
                Set(ref _pitch, value);
                foreach (Sound sound in Children)
                {
                    sound.RefreshProperties();
                }
                Glue.DocumentIsSaved = false;
            }
        }

        #endregion

        #region Volume

        private float _volume = 1f;

        [JsonProperty("volume")]
        public float Volume
        {
            get { return _volume; }
            set
            {
                Set(ref _volume, value);
                foreach (Sound sound in Children)
                {
                    sound.RefreshProperties();
                }
                Glue.DocumentIsSaved = false;
            }
        }

        #endregion

        #region Pan

        private float _pan;

        [JsonProperty("pan")]
        public float Pan
        {
            get { return _pan; }
            set
            {
                Set(ref _pan, value);
                foreach (Sound sound in Children)
                {
                    sound.RefreshProperties();
                }
                Glue.DocumentIsSaved = false;
            }
        }

        #endregion

        #region Looped

        private bool _looped;

        [JsonProperty("looped")]
        public bool Looped
        {
            get { return _looped; }
            set
            {
                Set(ref _looped, value);
                Glue.DocumentIsSaved = false;
            }
        }

        #endregion

        #region Instances	

        private int _instances;

        [JsonProperty("instances")]
        public int Instances
        {
            get { return _instances; }
            set
            {
                Set(ref _instances, value);
                Glue.DocumentIsSaved = false;
            }
        }

        #endregion

        #region CuePlaybackMode	

        private CuePlaybackMode _cuePlaybackMode;

        public CuePlaybackMode CuePlaybackMode
        {
            get { return _cuePlaybackMode; }
            set
            {
                Set(ref _cuePlaybackMode, value);
                Glue.DocumentIsSaved = false;
            }
        }

        #endregion


        private void PlayParallel()
        {
            foreach (Sound sound in Children)
            {
                sound.Play();
            }
        }

        private void PlayRandom()
        {
            var count = Children.Count;

            var random = new Random((int)DateTime.Now.Ticks);
            var index = random.Next(count);

            (Children[index] as Sound).Play();
        }

        private int _playingIndex = 0;

        private Timer _timer = new Timer();

        #region Playing	

        private bool _playing;
        [JsonIgnore]
        public bool Playing
        {
            get { return _playing; }
            set
            {
                Set(ref _playing, value);
            }
        }

        #endregion


        private void PlaySerial()
        {
            if (Playing && _playingIndex < Children.Count)
            {
                var sound = Children[_playingIndex] as Sound;
                var duration = sound.SoundEffect.Duration;
                sound.Play();
                _playingIndex++;
                _timer.Stop();
                _timer.Interval = duration.TotalMilliseconds;
                _timer.Start();
            }
            else
            {
                _timer.Stop();
                Playing = false;
            }

        }

        private void PlayCycle()
        {
            if (Playing && _playingIndex < Children.Count)
            {
                var sound = Children[_playingIndex] as Sound;
                var duration = sound.SoundEffect.Duration;
                sound.Play();
                _playingIndex++;
                _timer.Stop();
                _timer.Interval = duration.TotalMilliseconds;
                _timer.Start();
            }
            else
            {
                _playingIndex = 0;
                PlayCycle();
            }
        }
        [JsonIgnore]
        private List<INode> _soundsListToUse = new List<INode>(); 

        private void PlayRandomCycle()
        {
            if (Playing && _soundsListToUse.Count > 0)
            {
                var count = _soundsListToUse.Count;

                var random = new Random((int)DateTime.Now.Ticks);
                var index = random.Next(count);

                var sound = _soundsListToUse[index] as Sound;
                var duration = sound.SoundEffect.Duration;
                sound.Play();
                _soundsListToUse.Remove(sound);
                _timer.Stop();
                _timer.Interval = duration.TotalMilliseconds;
                _timer.Start();
            }
            else
            {
                _soundsListToUse = new List<INode>(Children);
                PlayRandomCycle();
            }
        }

        public void Play()
        {
            Playing = true;
            _soundsListToUse = new List<INode>(Children);
            _playingIndex = 0;

            switch (_playbackInUse)
            {
                case CuePlaybackMode.Parallel:
                    PlayParallel();
                    break;
                case CuePlaybackMode.Cycle:
                    PlayCycle();
                    break;
                case CuePlaybackMode.RandomCycle:
                    PlayRandomCycle();
                    break;
                case CuePlaybackMode.Random:
                    PlayRandom();
                    break;
                case CuePlaybackMode.Serial:
                    PlaySerial();
                    break;
            }
        }

        public void Stop()
        {
            Playing = false;
            _timer.Stop();
            foreach (Sound sound in Children)
            {
                sound.Stop();
            }
        }

        #region PlayParallelCommand
        [JsonIgnore]
        public SmartCommand<object> PlayParallelCommand { get; private set; }

        public bool CanExecutePlayParallelCommand(object o)
        {
            var value = Children.Count > 0 && !IsAnySoundPlay();
            if (value && _playbackInUse == CuePlaybackMode.Parallel)
                Playing = false;
            return value;
        }

        public void ExecutePlayParallelCommand(object o)
        {
            _playbackInUse = CuePlaybackMode.Parallel;
            Play();
        }

        #endregion

        private bool IsAnySoundPlay()
        {
            foreach (Sound sound in Children)
            {
                if (sound.PlayingInstance != null && sound.PlayingInstance.State == SoundState.Playing)
                    return true;
            }

            return false;
        }

        #region PlaySerialCommand
        [JsonIgnore]
        public SmartCommand<object> PlaySerialCommand { get; private set; }

        public bool CanExecutePlaySerialCommand(object o)
        {
            return !Playing && Children.Count > 1;
        }

        public void ExecutePlaySerialCommand(object o)
        {
            _playbackInUse = CuePlaybackMode.Serial;
            Play();
        }

        #endregion

        #region PlayRandomCommand
        [JsonIgnore]
        public SmartCommand<object> PlayRandomCommand { get; private set; }

        public bool CanExecutePlayRandomCommand(object o)
        {
            var value = Children.Count > 0 && !IsAnySoundPlay();
            if (value && _playbackInUse == CuePlaybackMode.Random)
                Playing = false;
            return value;
        }

        public async void ExecutePlayRandomCommand(object o)
        {
            _playbackInUse = CuePlaybackMode.Random;
            Play();
        }

        #endregion

        #region PlayCycleCommand
        [JsonIgnore]
        public SmartCommand<object> PlayCycleCommand { get; private set; }

        public bool CanExecutePlayCycleCommand(object o)
        {
            return !Playing && Children.Count > 0;
        }

        public async void ExecutePlayCycleCommand(object o)
        {
            _playbackInUse = CuePlaybackMode.Cycle;
            Play();
        }

        #endregion

        #region PlayRandomCycleCommand
        [JsonIgnore]
        public SmartCommand<object> PlayRandomCycleCommand { get; private set; }

        public bool CanExecutePlayRandomCycleCommand(object o)
        {
            return !Playing && Children.Count > 1;
        }

        public async void ExecutePlayRandomCycleCommand(object o)
        {
            _playbackInUse = CuePlaybackMode.RandomCycle;
            Play();
        }

        #endregion

        #region StopCommand
        [JsonIgnore]
        public SmartCommand<object> StopCommand { get; private set; }

        public bool CanExecuteStopCommand(object o)
        {
            return Playing;
        }

        public async void ExecuteStopCommand(object o)
        {
            Stop();
        }

        #endregion

        #region AddSoundCommand

        [JsonIgnore]
        public SmartCommand<object> AddSoundCommand { get; private set; }

        public bool CanExecuteAddSoundCommand(object o)
        {
            return !Playing;
        }

        public async void ExecuteAddSoundCommand(object o)
        {
            var dialog = new OpenFileDialog { Filter = "Wav File (*.wav)|*.wav", Multiselect = true };
            var result = dialog.ShowDialog();
            if (result.Value)
            {
                foreach (var filename in dialog.FileNames)
                {
                    try
                    {
                        var sound = new Sound(filename);
                        AddChild(sound);
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.ToString());
                    }
                }
            }
        }

        #endregion

        #region PlayCommand
         [JsonIgnore]
        public SmartCommand<object> PlayCommand { get; private set; }

        public bool CanExecutePlayCommand(object o)
        {
            if (CuePlaybackMode == CuePlaybackMode.Parallel)
            {
                return CanExecutePlayParallelCommand(null);
            }
            else if (CuePlaybackMode == CuePlaybackMode.Serial)
            {
                return CanExecutePlaySerialCommand(null);
            }
            else if (CuePlaybackMode == CuePlaybackMode.Cycle)
            {
                return CanExecutePlayCycleCommand(null);
            }
            else if (CuePlaybackMode == CuePlaybackMode.RandomCycle)
            {
                return CanExecutePlayRandomCycleCommand(null);
            }
            else if (CuePlaybackMode == CuePlaybackMode.Random)
            {
                return CanExecutePlayRandomCommand(null);
            }

            return false;
        }

        public async void ExecutePlayCommand(object o)
        {

            _playbackInUse = CuePlaybackMode;
            Play();
        }

        #endregion

        public Cue()
        {
            Name = "New Cue";
            Children = new ObservableCollection<INode>();
            _timer = new Timer();
            _timer.Elapsed += _timer_Elapsed;
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_playbackInUse == CuePlaybackMode.Serial)
                PlaySerial();
            else if (_playbackInUse == CuePlaybackMode.Cycle)
                PlayCycle();
            else if (_playbackInUse == CuePlaybackMode.RandomCycle)
                PlayRandomCycle();
        }

        [JsonConstructor]
        public Cue(ObservableCollection<Sound> sounds) : this()
        {
            foreach (var sound in sounds)
            {
                AddChild(sound);
            }
        }

        protected override void InitializeCommands()
        {
            PlayCommand = new SmartCommand<object>(ExecutePlayCommand, CanExecutePlayCommand);  
            PlayParallelCommand = new SmartCommand<object>(ExecutePlayParallelCommand, CanExecutePlayParallelCommand);
            PlaySerialCommand = new SmartCommand<object>(ExecutePlaySerialCommand, CanExecutePlaySerialCommand);
            PlayRandomCommand = new SmartCommand<object>(ExecutePlayRandomCommand, CanExecutePlayRandomCommand);
            PlayCycleCommand = new SmartCommand<object>(ExecutePlayCycleCommand, CanExecutePlayCycleCommand);
            PlayRandomCycleCommand = new SmartCommand<object>(ExecutePlayRandomCycleCommand, CanExecutePlayRandomCycleCommand);  

            StopCommand = new SmartCommand<object>(ExecuteStopCommand, CanExecuteStopCommand);
            AddSoundCommand = new SmartCommand<object>(ExecuteAddSoundCommand, CanExecuteAddSoundCommand);  
            base.InitializeCommands();
        }
    }
}
