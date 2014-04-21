using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AudioTool.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Newtonsoft.Json;

namespace AudioTool.Data
{
    public sealed class Sound : NodeWithName
    {
        #region FilePath

        private string _filePath;

        [JsonProperty("filepath")]
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                Set(ref _filePath, value);
                Glue.Instance.DocumentIsSaved = false;
            }
        }

        #endregion

        #region FileLastModified

        private DateTime _fileLastModified;

        [JsonProperty("FileLastModified")]
        public DateTime FileLastModified 
        {
            get
            {
                return _fileLastModified;
            }
            set
            {
                Set(ref _fileLastModified, value);
                Glue.Instance.DocumentIsSaved = false;
            }
        }

        #endregion

        #region Data

        private byte[] _data;

        [JsonProperty("data")]
        public byte[] Data
        {
            get { return _data; }
            set
            {
                Set(ref _data, value);
                Glue.Instance.DocumentIsSaved = false;
            }
        }

        #endregion  

        #region SoundEffect	

        private SoundEffect _soundEffect;

        [JsonIgnore]
        public SoundEffect SoundEffect
        {
            get { return _soundEffect; }
            set
            {
                Set(ref _soundEffect, value);
            }
        }

        #endregion

        #region Instances	

        private Nullable<int> _instances;

        [JsonProperty("instances")]
        public Nullable<int> Instances
        {
            get { return _instances; }
            set
            {
                Set(ref _instances, value);
                Glue.Instance.DocumentIsSaved = false;
            }
        }

        #endregion

        #region Pitch	

        private Nullable<float> _pitch;

        [JsonProperty("pitch")]
        public Nullable<float> Pitch
        {
            get { return _pitch; }
            set
            {
                Set(ref _pitch, value);
                if (value != null)
                    PlayingInstance.Pitch = value.Value;
                else
                {
                    PlayingInstance.Pitch = (Parent as Cue).Pitch;
                }
                Glue.Instance.DocumentIsSaved = false;
            }
        }

        #endregion

        #region Volume	

        private Nullable<float> _volume;

        [JsonProperty("volume")]
        public Nullable<float> Volume
        {
            get { return _volume; }
            set
            {
                Set(ref _volume, value);
                if (value != null)
                    PlayingInstance.Volume = value.Value;
                else
                {
                    PlayingInstance.Volume = (Parent as Cue).Volume;
                }
                Glue.Instance.DocumentIsSaved = false;
            }
        }

        #endregion

        #region Pan	

        private Nullable<float> _pan;

         [JsonProperty("pan")]
        public Nullable<float> Pan
        {
            get { return _pan; }
            set
            {
                Set(ref _pan, value);
                if (value != null)
                    PlayingInstance.Pan = value.Value;
                else
                {
                    PlayingInstance.Pan = (Parent as Cue).Pan;
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

        public Sound()
        {
            Name = "New Sound";
            AudioManager.SoundStateChanged += AudioManager_SoundStateChanged;
            SoundState = SoundState.Stopped;
        }

        void AudioManager_SoundStateChanged(object sender, SoundStateChangedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(
              new Action(() => SoundState = PlayingInstance.State));
        }

        public Sound(string path)
            : this()
        {
            var soundfile = new FileStream(path, FileMode.Open);
            SoundEffect = SoundEffect.FromStream(soundfile);
            soundfile.Close();
            soundfile.Dispose();
            soundfile = new FileStream(path, FileMode.Open);
            Data = Helper.ReadFully(soundfile);
            soundfile.Close();
            soundfile.Dispose();
            FilePath = path;
            //Save the last modified time so that way we can use a "ReImport All" or "ReImport Selected"
            //command to just check the date times. If the last write time was older than the current, reimport and overwrite
            FileLastModified = File.GetLastWriteTime(path).ToUniversalTime();

            Name = Path.GetFileNameWithoutExtension(path);
            PlayingInstance = SoundEffect.CreateInstance();
            AudioManager.AddSoundInstance(PlayingInstance);
        }

        [JsonConstructor]
        public Sound(byte[] data) : this()
        {
            try
            {
                var soundfile = new MemoryStream(data);
                SoundEffect = SoundEffect.FromStream(soundfile);
                soundfile.Close();
                soundfile.Dispose();
                soundfile = new MemoryStream(data);
                Data = Helper.ReadFully(soundfile);
                soundfile.Close();
                soundfile.Dispose();
                PlayingInstance = SoundEffect.CreateInstance();
                AudioManager.AddSoundInstance(PlayingInstance);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }
        }

        public void RefreshProperties()
        {
            Pitch = Pitch;
            Pan = Pan;
            Volume = Volume;
        }

        private void InitializeInstance(SoundEffectInstance instance)
        {
            if (Pan.HasValue)
                instance.Pan = Pan.Value;
            if (Pitch.HasValue)
                instance.Pitch = Pitch.Value;
            if (Volume.HasValue)
                instance.Volume = Volume.Value;
        }

        private void InitializeInstance(SoundEffectInstance instance, Cue cue)
        {
            if (Pan.HasValue)
                instance.Pan = Pan.Value;
            else
            {
                instance.Pan = cue.Pan;
            }
            if (Pitch.HasValue)
                instance.Pitch = Pitch.Value;
            else
            {
                instance.Pitch = cue.Pitch;
            }
            if (Volume.HasValue)
                instance.Volume = Volume.Value;
            else
            {
                instance.Volume = cue.Volume;
            }
        }

        public void Play()
        {
            if (PlayingInstance.State == SoundState.Paused)
                PlayingInstance.Play();
            else
            {
                PlayingInstance.Stop();
                AudioManager.RemoveSoundInstance(PlayingInstance);
                PlayingInstance.Dispose();
                PlayingInstance = SoundEffect.CreateInstance();
                InitializeInstance(PlayingInstance);
                AudioManager.AddSoundInstance(PlayingInstance);
                PlayingInstance.IsLooped = Looped;
                PlayingInstance.Play();
            }
        }

        public void Stop()
        {
                PlayingInstance.Stop();
        }

        public void Pause()
        {
                PlayingInstance.Pause();
        }

        [JsonIgnore]
        public SoundEffectInstance PlayingInstance
        {
            get; private set;
        }

        #region PlayCommand
        [JsonIgnore]
        public SmartCommand<object> PlayCommand { get; private set; }

        public bool CanExecutePlayCommand(object o)
        {
            return SoundState != SoundState.Playing;
        }

        public async void ExecutePlayCommand(object o)
        {
            Play();
        }

        #endregion

        #region StopCommand
        [JsonIgnore]
        public SmartCommand<object> StopCommand { get; private set; }

        public bool CanExecuteStopCommand(object o)
        {
            return PlayingInstance != null && SoundState != SoundState.Stopped;
        }

        public async void ExecuteStopCommand(object o)
        {
            Stop();
        }

        #endregion

        #region PauseCommand
        [JsonIgnore]
        public SmartCommand<object> PauseCommand { get; private set; }

        public bool CanExecutePauseCommand(object o)
        {
            return PlayingInstance != null && SoundState == SoundState.Playing;
        }

        public async void ExecutePauseCommand(object o)
        {
            Pause();
        }

        #endregion

        #region ClearVolumeCommand

        [JsonIgnore]
        public SmartCommand<object> ClearVolumeCommand { get; private set; }

        public bool CanExecuteClearVolumeCommand(object o)
        {
            return Volume != null;
        }

        public async void ExecuteClearVolumeCommand(object o)
        {
            Volume = null;
        }

        #endregion

        #region ClearPitchCommand
        [JsonIgnore]
        public SmartCommand<object> ClearPitchCommand { get; private set; }

        public bool CanExecuteClearPitchCommand(object o)
        {
            return Pitch != null;
        }

        public async void ExecuteClearPitchCommand(object o)
        {
            Pitch = null;
        }

        #endregion

        #region ClearPanCommand
        [JsonIgnore]
        public SmartCommand<object> ClearPanCommand { get; private set; }

        public bool CanExecuteClearPanCommand(object o)
        {
            return Pan != null;
        }

        public async void ExecuteClearPanCommand(object o)
        {
            Pan = null;
        }

        #endregion

        #region ClearInstancesCommand
         [JsonIgnore]
        public SmartCommand<object> ClearInstancesCommand { get; private set; }

        public bool CanExecuteClearInstancesCommand(object o)
        {
            return Instances != null;
        }

        public async void ExecuteClearInstancesCommand(object o)
        {
            Instances = null;
        }

        #endregion

        #region SoundState	

        private SoundState _soundState;

        public SoundState SoundState
        {
            get { return _soundState; }
            set
            {
                Set(ref _soundState, value);
            }
        }

        #endregion

        public override void Remove()
        {
            Stop();
            base.Remove();
        }

        #region ReimportNewVersionCommand
        public SmartCommand<object> ReImportNewVersionCommand { get; private set; }

        public void ExecuteReImportNewVersion(object obj)
        {
            var path = obj as string;
            //Does a reimport with the same file path as what the old file was.
            var soundfile = new FileStream(path, FileMode.Open);
            SoundEffect = SoundEffect.FromStream(soundfile);
            soundfile.Close();
            soundfile.Dispose();
            soundfile = new FileStream(path, FileMode.Open);
            Data = Helper.ReadFully(soundfile);
            soundfile.Close();
            soundfile.Dispose();
            FilePath = path;
            //Save the last modified time so that way we can use a "ReImport All" or "ReImport Selected"
            //command to just check the date times. If the last write time was older than the current, reimport and overwrite
            FileLastModified = File.GetLastWriteTime(path).ToUniversalTime();

            Name = Path.GetFileNameWithoutExtension(FilePath);
            PlayingInstance = SoundEffect.CreateInstance();
            AudioManager.AddSoundInstance(PlayingInstance);
        }
        #endregion

        #region ReimportCommand
        public SmartCommand<object> ReImportCommand { get; private set; }

        public void ExecuteReImport(object obj)
        {
            //Does a reimport with the same file path as what the old file was.
            var soundfile = new FileStream(FilePath, FileMode.Open);
            SoundEffect = SoundEffect.FromStream(soundfile);
            soundfile.Close();
            soundfile.Dispose();
            soundfile = new FileStream(FilePath, FileMode.Open);
            Data = Helper.ReadFully(soundfile);
            soundfile.Close();
            soundfile.Dispose();
            FilePath = FilePath;
            //Save the last modified time so that way we can use a "ReImport All" or "ReImport Selected"
            //command to just check the date times. If the last write time was older than the current, reimport and overwrite
            FileLastModified = File.GetLastWriteTime(FilePath).ToUniversalTime();

            Name = Path.GetFileNameWithoutExtension(FilePath);
            PlayingInstance = SoundEffect.CreateInstance();
            AudioManager.AddSoundInstance(PlayingInstance);
        }
        #endregion

        protected override void InitializeCommands()
        {
            ClearPanCommand = new SmartCommand<object>(ExecuteClearPanCommand, CanExecuteClearPanCommand);
            ClearPitchCommand = new SmartCommand<object>(ExecuteClearPitchCommand, CanExecuteClearPitchCommand);
            ClearVolumeCommand = new SmartCommand<object>(ExecuteClearVolumeCommand, CanExecuteClearVolumeCommand);
            ClearInstancesCommand = new SmartCommand<object>(ExecuteClearInstancesCommand, CanExecuteClearInstancesCommand);  
            PlayCommand = new SmartCommand<object>(ExecutePlayCommand, CanExecutePlayCommand);
            StopCommand = new SmartCommand<object>(ExecuteStopCommand, CanExecuteStopCommand);
            PauseCommand = new SmartCommand<object>(ExecutePauseCommand, CanExecutePauseCommand);
            ReImportCommand = new SmartCommand<object>(ExecuteReImport);
            ReImportNewVersionCommand = new SmartCommand<object>(ExecuteReImportNewVersion);

            base.InitializeCommands();
        }
    }
}
