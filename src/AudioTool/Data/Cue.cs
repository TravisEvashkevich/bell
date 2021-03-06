﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows;
using AudioTool.Core;
using Microsoft.Win32;
using Microsoft.Xna.Framework.Audio;
using Newtonsoft.Json;

namespace AudioTool.Data
{
    //- Parallel (all sounds in the cue play at the same time)
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
                Glue.Instance.DocumentIsSaved = false;
            }
        }

        #region Radius
        private float _radius;

        public float Radius { get { return _radius; } set
        {
            CenterPoint = new Point(_definedCenter.X - (value / 2), _definedCenter.Y - (value/2));
            Set(ref _radius, value);
            Glue.Instance.DocumentIsSaved = false;
        }
            
        }
        #endregion

        #region center

        //This is the center that the circle uses
        private Point _centerPoint;
        [JsonIgnore]
        public Point CenterPoint
        {
            get { return _centerPoint; }
            set { Set(ref _centerPoint, value); }
        }

        //This is the values we get from the textboxes and help to make sure the circle stays centered by 
        //adjusting the CenterPoint var.
        private Point _definedCenter;
        [JsonIgnore]
        public Point DefinedCenter
        {
            get { return _definedCenter; }
            set
            {
                CenterPoint = new Point(value.X - (Radius / 2), value.Y - (Radius / 2));
                Set(ref _definedCenter, value);
                Glue.Instance.DocumentIsSaved = false;
            }
        }

        #endregion  

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
                    sound.RefreshProperties(this);
                }
                Glue.Instance.DocumentIsSaved = false;
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
                    sound.RefreshProperties(this);
                }
                Glue.Instance.DocumentIsSaved = false;
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
                    sound.RefreshProperties(this);
                }
                Glue.Instance.DocumentIsSaved = false;
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
                Glue.Instance.DocumentIsSaved = false;
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
                Glue.Instance.DocumentIsSaved = false;
            }
        }

        #endregion

        #region Playing

        private bool _playing;
        [JsonIgnore]
        public bool Playing
        {
            get { return _playing; }
            set
            {
                //for things like Parallel play, longer sounds can still be playing
                //even though the short ones have set the bool to false so we do a check
                if (CuePlaybackMode == CuePlaybackMode.Parallel)
                    foreach (Sound child in Children)
                    {
                        {
                            if (child.PlayingInstance.State == SoundState.Playing)
                            {
                                Set(ref _playing, true);
                                foreach (Sound sound in Children)
                                {
                                    sound.ParentIsPlaying = true;
                                }
                                return;
                            }
                        }
                    }
                Set(ref _playing, value);
                foreach (Sound sound in Children)
                {
                    sound.ParentIsPlaying = value;
                }
            }
        }

        #endregion

        #region Play Options
        private void PlayParallel()
        {
            foreach (Sound sound in Children)
            {
                sound.Play();
            }
        }

        private void PlayRandom()
        {
            if (CheckIfAllSoundsMuted(Children.ToList()))
            {
                return;
            }
            var count = Children.Count;

            var random = new Random((int)DateTime.Now.Ticks);
            var index = random.Next(count);
            if(!(Children[index] as Sound).IsMuted)
                (Children[index] as Sound).Play();
            else
            {
                PlayRandom();
            }
        }

        private int _playingIndex = 0;

        private Timer _timer = new Timer();

        private void PlaySerial()
        {
            if (_playingIndex >= Children.Count)
            {
                _playingIndex = 0;
            }

            if (Playing && _playingIndex < Children.Count)
            {
                var sound = Children[_playingIndex] as Sound;
                if (!sound.IsMuted)
                {
                    var duration = sound.SoundEffect.Duration;
                    sound.Play();
                    _playingIndex++;
                    _timer.Stop();
                    _timer.Interval = duration.TotalMilliseconds;
                    _timer.Start(); 
                }
                else
                {
                    // We'll do a check to make sure that not all the sounds are muted in serial so we don't get an infinite loop
                    bool allMuted = true;
                    foreach (Sound child in Children)
                    {
                        if (!child.IsMuted)
                        {
                            allMuted = false;
                        }
                    }
                    if(allMuted == false)
                    {
                        ++_playingIndex;
                        PlaySerial();
                    }
                    else
                    {
                        MessageBox.Show("All sounds are Muted");
                        Stop();
                    }
                }
                
            }

        }

        private void PlayCycle()
        {

            if (CheckIfAllSoundsMuted(Children.ToList()))
            {
                Stop();
                return;
            }
            //Check for index count BEFORE playing the cycle so that way we don't have to do a full extra click before playing
            //and also doesn't do the recursive call it prev did which made it loop constantly
            if (_playingIndex >= Children.Count)
                _playingIndex = 0;

            if (Playing && _playingIndex < Children.Count)
            {
                var sound = Children[_playingIndex] as Sound;
                if (sound.IsMuted)
                {
                    ++_playingIndex;
                    PlayCycle();
                    return;
                }
                sound.Play();
                _playingIndex++;
            }           
        }

        [JsonIgnore]
        private List<INode> _soundsList1 = new List<INode>();

        private void PlayRandomCycle()
        {
            if (CheckIfAllSoundsMuted(Children.ToList()))
            {
                Stop();
                return;
            }
            //check to see if the list is empty before we go to play.
            if (_soundsList1.Count <= 0)
            {
                _soundsList1 = new List<INode>(Children);
            }

            //Check to see if all sounds are muted before even going into the playing
            if (CheckIfAllSoundsMuted(_soundsList1) && _soundsList1.Count < Children.Count)
            {
                _soundsList1 = new List<INode>(Children);
            }

            if (Playing && _soundsList1.Count >0)
            {
                var random = new Random();
                var index = random.Next(_soundsList1.Count);
                var sound = _soundsList1[index] as Sound;
                //check to see if the sound is muted, if it is, 
                //remove it from the list and then call playrandomcycle again so it can find another sound that does play
                //Return after it comes out of the method so that way it doesn't try to play a sound that is removed
                if (sound.IsMuted)
                {
                    _soundsList1.Remove(sound);
                    PlayRandomCycle();
                    return;
                }
                sound.Play();
                //Remove sound from list.
                _soundsList1.Remove(sound);
            }
        }

        public void Play()
        {
            Playing = true;

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
                    _playingIndex = 0;
                    PlaySerial();
                    break; 
            }
        }
        #endregion

        public bool CheckIfAllSoundsMuted(List<INode> toCheckList )
        {
            //We'll do a check to make sure that not all the sounds are muted in serial
            //so we don't get an infinite loop
            bool allMuted = true;
            foreach (Sound child in toCheckList)
            {
                if (!child.IsMuted)
                {
                    allMuted = false;
                    break;
                }
            }
            if (allMuted == false)
            {
                return false;
            }
            //Stop();
            return true;
        }

        public void Stop()
        {
            
            _timer.Stop();
            foreach (Sound sound in Children)
            {
                sound.Stop();
            }
            Playing = false;
        }

        private bool IsAnySoundPlay()
        {
            foreach (Sound sound in Children)
            {
                if (sound.PlayingInstance != null && sound.PlayingInstance.State == SoundState.Playing)
                    return true;
            }

            return false;
        }

        public void DecreaseSoundIndex(Sound sound)
        {
            var index = -1;
            index =Children.IndexOf(sound);

            if (index == -1)
                return;
            
            if (index >0)
            {
                Children.RemoveAt(index);
                Children.Insert(index-1,sound);
                
            }
            else if (index == 0)
            {
                Children.RemoveAt(index);
                Children.Insert(Children.Count,sound);
            }
            Glue.Instance.DocumentIsSaved = false;
        }

        public void IncreaseSoundIndex(Sound sound)
        {
            var index = -1;
            index = Children.IndexOf(sound);

            if (index == -1)
                return;

            if (index == Children.Count-1)
            {
                Children.RemoveAt(index);
                Children.Insert(0, sound);

            }
            else if(index >= 0)
            {
                Children.RemoveAt(index);
                Children.Insert(index + 1, sound);
            }
            Glue.Instance.DocumentIsSaved = false;
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

        #region PlaySerialCommand
        [JsonIgnore]
        public SmartCommand<object> PlaySerialCommand { get; private set; }

        public bool CanExecutePlaySerialCommand(object o)
        {
            //Doesn't make sense to make this NOT playable via the stub if only 1 child
            //What's it matter really besides making it harder and more aggrivating to use if you 
            //have to go one layer deeper just to play the sound.
            return !Playing /*&& Children.Count > 1*/;
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
            return !Playing /*&& Children.Count > 1*/;
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

        #region ReimportAll Command
        [JsonIgnore]
        public SmartCommand<object> ReimportAllCommand { get; private set; }

        public void ExecuteReimportAllCommand(object obj)
        {
            foreach (Sound child in Children)
            {
                child.ExecuteReImport(null);
            }
        }
        #endregion

        #region MuteAllCommand
        [JsonIgnore]
        public SmartCommand<object> MuteAllCommand { get; private set; }

        public bool CanExecuteMuteAll(object o)
        {
            return !Playing;
        }

        public void ExecuteMuteAll(object obj)
        {
            foreach (Sound child in Children)
            {
                child.IsMuted = true;
            }
        }
        #endregion

        #region Un-MuteAllCommand
        [JsonIgnore]
        public SmartCommand<object> UnMuteAllCommand { get; private set; }

        public void ExecuteUnMuteAll(object o)
        {
            foreach (Sound child in Children)
            {
                child.IsMuted = false;
            }
        }
        #endregion

        public Cue()
        {
            Name = "New Cue";
            Children = new ObservableCollection<INode>();
            _timer = new Timer();
            _timer.Elapsed += _timer_Elapsed;
            _definedCenter.X = 200;
            _definedCenter.Y = 100;

            _radius = 4;
            _centerPoint.X = _definedCenter.X - (_radius / 2);
            _centerPoint.Y = _definedCenter.Y - (_radius / 2);
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_playbackInUse == CuePlaybackMode.Serial)
                PlaySerial();
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
            ReimportAllCommand = new SmartCommand<object>(ExecuteReimportAllCommand);

            MuteAllCommand = new SmartCommand<object>(ExecuteMuteAll, CanExecuteMuteAll);
            UnMuteAllCommand = new SmartCommand<object>(ExecuteUnMuteAll);
            base.InitializeCommands();
        }

    }
}
