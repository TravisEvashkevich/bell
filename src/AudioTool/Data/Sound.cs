using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AudioTool.Core;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Newtonsoft.Json;

namespace AudioTool.Data
{
    public sealed class Sound : NodeWithName
    {
        private readonly object Sync = new object(); 

        #region Mute
        private bool _isMuted;
        public bool IsMuted { get { return _isMuted; } set { Set(ref _isMuted, value); } }
        #endregion

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

        #region ParentIsPlaying
        private bool _parentIsPlaying ;

        public bool ParentIsPlaying
        {
            get { return _parentIsPlaying; } set { Set(ref _parentIsPlaying, value); }
        }
        #endregion

        public Sound()
        {
            Name = "New Sound";
            AudioManager.SoundStateChanged += AudioManager_SoundStateChanged;
            SoundState = SoundState.Stopped;
            IsMuted = false;
        }

        void AudioManager_SoundStateChanged(object sender, SoundStateChangedEventArgs e)
        {
            lock (Sync)
            {
                if (!PlayingInstance.IsDisposed)
                {
                    //Seems that the PlayingInstance on one thread can be out of sync (disposed)
                    //even though on this thread it is not? Only thing I can come up with.
                    Application.Current.Dispatcher.BeginInvoke(
                        new Action(() => SoundState = PlayingInstance.State));
                }
            }
        }

        public Sound(string path): this()
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

        public void RefreshProperties(Cue cue)
        {
            Pitch = cue.Pitch;
            Pan = cue.Pan;
            Volume = cue.Volume;
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
            else if(!IsMuted)
            {
                if (Parent != null)
                {
                    var parent = Parent as Cue;
                    if (parent.CuePlaybackMode == CuePlaybackMode.Serial)
                    {
                        PlayingInstance.Stop();
                        AudioManager.RemoveSoundInstance(PlayingInstance);
                        lock (Sync)
                        {
                            var instance = PlayingInstance;
                            PlayingInstance = SoundEffect.CreateInstance();
                            instance.Dispose();
                        }
                        InitializeInstance(PlayingInstance, parent as Cue);
                        AudioManager.AddSoundInstance(PlayingInstance);
                        PlayingInstance.IsLooped = false;
                        PlayingInstance.Play();
                    }
                    else
                    {
                        PlayingInstance.Stop();
                        AudioManager.RemoveSoundInstance(PlayingInstance);
                        lock (Sync)
                        {
                            var instance = PlayingInstance;
                            PlayingInstance = SoundEffect.CreateInstance();
                            instance.Dispose();
                        }
                        InitializeInstance(PlayingInstance, parent as Cue);
                        AudioManager.AddSoundInstance(PlayingInstance);
                        PlayingInstance.IsLooped = Looped;
                        PlayingInstance.Play();
                    }
                }
                
            }
        }

        public void Stop()
        {
            PlayingInstance.Stop(true);
            (Parent as Cue).CheckIfAllSoundsMuted((Parent as Cue).Children.ToList());
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
                if (value == SoundState.Stopped && Parent != null)
                {
                    //EASIEST way to make sure the Sound and the Cue buttons are in sync for when a sound has ended
                    var parent = Parent as Cue;
                    if(parent.CuePlaybackMode != CuePlaybackMode.Serial)
                    parent.Playing = false;
                }
            }
        }

        #endregion

        public override void Remove()
        {
            Stop();
            base.Remove();
        }

        #region ReimportCommand
        [JsonIgnore]
        public SmartCommand<object> ReImportCommand { get; private set; }

        public void ExecuteReImport(object obj)
        {
            
            //ALWAYS stop the sound before you reimport as it will completely screw up the cue if you don't
            Stop();
            //quick check to see if the file even exists
            if (!File.Exists(FilePath))
            {
                //Search says it doesn't exist, inform the user to FIND the file
                if (MessageBox.Show("The file doesn't exist at the old path, would you like to find the file?",
                    "Missing File.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    //The thing is, the filename could have technically changed or have a different syntax or something
                    var dialog = new OpenFileDialog();
                    dialog.Filter = ".Wav (*.Wav)|*.Wav";

                    bool? result = dialog.ShowDialog();
                    if (result == true)
                    {
                        //we'll check the filename they selected against the filename in the sound and ask if they want to overwrite
                        //if they are different
                        //get the path
                        string fileName = Path.GetFileNameWithoutExtension(dialog.FileName);

                        if (fileName != Name)
                        {
                            if (MessageBox.Show(
                                String.Format("The selected file \"{0}\" has a different name than what you are trying to overwrite ({1}). Would you like to proceed anyway?",fileName,Name),
                                "FileName doesn't match", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                //pass the filename from the dialog to the Reimport new version so it will Create and overwrite
                                //the sound thus updating in the program.
                                ReimportSoundFile(dialog.FileName);
                            }
                        }
                        else
                        {
                            ReimportSoundFile(dialog.FileName);
                        }
                    }
                }
            }
            else
            {
                //the file exists at the path, so we get the modified time to see if it's newer or not
                var date = File.GetLastWriteTime(FilePath).ToUniversalTime();

                if (date > FileLastModified)
                {
                    if(obj != null)
                        ReimportSoundFile(obj.ToString());
                    else
                    {
                        ReimportSoundFile();
                    }
                    MessageBox.Show(String.Format("{0} was reimported successfully!",Name),"Success!");
                }
                else if (date <= FileLastModified)
                {
                    //we inform the user that the file is not newer than the version they are using and ask if they want to reimport anyways
                    if (MessageBox.Show(
                        String.Format("The version of: \n {0} \n Is older than the current version. Continue Reimport?",Path.GetFileNameWithoutExtension(FilePath)),
                        "Warning", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                    {
                        if (obj != null)
                            ReimportSoundFile(obj.ToString());
                        else
                        {
                            ReimportSoundFile();
                        }
                        MessageBox.Show(String.Format("{0} was reimported successfully!", Path.GetFileNameWithoutExtension(FilePath)), "Success!");
                    }
                }
            }
            
        }

        private void ReimportSoundFile()
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

            //Name = Path.GetFileNameWithoutExtension(FilePath);
            PlayingInstance = SoundEffect.CreateInstance();
            AudioManager.AddSoundInstance(PlayingInstance);
        }

        public void ReimportSoundFile(string path)
        {
            //Does a reimport with the specified path
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

            //Name = Path.GetFileNameWithoutExtension(FilePath);
            PlayingInstance = SoundEffect.CreateInstance();
            AudioManager.AddSoundInstance(PlayingInstance);
        }
        #endregion

        #region MoveUpInCueCommand

        public SmartCommand<object> MoveUpInCueCommand { get; private set; }

        public void ExecuteMoveUpInCue(object o)
        {
            (Parent as Cue).DecreaseSoundIndex(this);
        }
        #endregion

        #region MoveDownInCueCommand

        public SmartCommand<object> MoveDownInCueCommand { get; private set; }

        public void ExecuteMoveDownInCue(object o)
        {
            (Parent as Cue).IncreaseSoundIndex(this);
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

            MoveUpInCueCommand = new SmartCommand<object>(ExecuteMoveUpInCue);
            MoveDownInCueCommand = new SmartCommand<object>(ExecuteMoveDownInCue);
            base.InitializeCommands();
        }
    }
}
