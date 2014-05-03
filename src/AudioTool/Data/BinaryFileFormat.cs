using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AudioTool.Data
{
    public class BinaryFileFormat : FileFormat
    {
        public enum Type : sbyte
        {
            Folder = 1,
            Cues = 2
        }

        public override void Save(string path, Document document)
        {
            using (var stream = File.Open(path, FileMode.Create, FileAccess.ReadWrite))
            {
                var writer = new BinaryWriter(stream);
                WriteDocument(writer, document);
                stream.Flush();
            }
        }

        public override Document Load(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var reader = new BinaryReader(stream);
                var document = new Document();
                ReadDocument(reader, document);

                InitializeCommandTree(document);

                return document;
            }
        }

        public static void InitializeCommandTree(Document document)
        {
            // Initialize commands
            document.Initialize();
            var elements = Flatten(document.Children);
            Parallel.ForEach(elements, e => e.Initialize());
        }

        static IEnumerable<INode> Flatten(IEnumerable<INode> collection)
        {
            foreach (var node in collection)
            {
                yield return node;
                if (node.Children == null) continue;
                foreach (var child in Flatten(node.Children))
                {
                    yield return child;
                }
            }
        }

        private static void ReadDocument(BinaryReader reader, Document document)
        {
            document.Name = reader.ReadString();
            document.Filename = reader.ReadString();
            ReadChildren(reader, document);
        }

        private static void WriteDocument(BinaryWriter writer, Document document)
        {
            writer.Write(document.Name);
            writer.Write(document.Filename);
            writer.Write(document.Children.Count);
            WriteChildren(writer, document.Children);
        }

        private static void ReadChildren(BinaryReader reader, INode parent)
        {
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                ReadChild(reader, parent, parent.Children);
            }
        }

        private static void WriteChildren(BinaryWriter writer, IEnumerable<INode> children)
        {
            foreach (var child in children)
            {
                WriteChild(writer, child);
            }
        }

        private static void ReadChild(BinaryReader reader, INode parent, ICollection<INode> container)
        {
            var type = (Type)reader.ReadSByte();
            if (type == Type.Folder)
            {
                ReadFolder(reader, parent, container);
            }
            else
            {
                ReadCue(reader, parent, container);
            }
        }

        private static void WriteChild(BinaryWriter writer, INode child)
        {
            if (child is Folder)
            {
                WriteFolder(writer, child);
            }
            else
            {
                WriteCue(writer, child);
            }
        }
        
        private static void ReadFolder(BinaryReader reader, INode parent, ICollection<INode> container)
        {
            var folder = new Folder { Name = reader.ReadString(), Parent = parent };
            container.Add(folder);
            ReadChildren(reader, folder);
        }

        
        private static void WriteFolder(BinaryWriter writer, INode child)
        {
            writer.Write((sbyte)Type.Folder);
            writer.Write(child.Name);
            writer.Write(child.Children.Count);
            WriteChildren(writer, child.Children);
        }

        private static void WriteCue(BinaryWriter writer, INode child)
        {
            var cue = (Cue)child;
            writer.Write((sbyte)Type.Cues);
            writer.Write(cue.Approved);
            writer.Write(cue.Instances);
            writer.Write(cue.Looped);
            writer.Write(cue.Name);
            writer.Write(cue.Pan);
            writer.Write(cue.Pitch);
            writer.Write(cue.Radius);
            writer.Write(cue.Volume);
            writer.Write(cue.CenterPoint.X);
            writer.Write(cue.CenterPoint.Y);
            writer.Write(cue.DefinedCenter.X);
            writer.Write(cue.DefinedCenter.Y);
            writer.Write((int)cue.CuePlaybackMode);
            
            var sounds = cue.Children;
            writer.Write(sounds.Count);
            foreach (var sound in cue.Children.Cast<Sound>())
            {
                WriteSound(writer, sound);
            }
        }

        private static void ReadCue(BinaryReader reader, INode parent, ICollection<INode> container)
        {
            var approved = reader.ReadBoolean();
            var instances = reader.ReadInt32();
            var looped = reader.ReadBoolean();
            var name = reader.ReadString();
            var pan = reader.ReadSingle();
            var pitch = reader.ReadSingle();
            var radius = reader.ReadSingle();
            var volume = reader.ReadSingle();
            var centerPointX = reader.ReadDouble();
            var centerPointY = reader.ReadDouble();
            var definedCenterX = reader.ReadDouble();
            var definedCenterY = reader.ReadDouble();
            var cuePlaybackMode = (CuePlaybackMode)reader.ReadInt32();

            var cue = new Cue
            {
                Approved = approved,
                Instances = instances,
                Looped = looped,
                Name = name,
                Pan = pan,
                Pitch = pitch,
                Radius = radius,
                Volume = volume,
                CenterPoint = new Point(centerPointX, centerPointY),
                DefinedCenter = new Point(definedCenterX, definedCenterY),
                CuePlaybackMode = cuePlaybackMode,
                Parent = parent
            };
            container.Add(cue);
            
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                ReadSound(reader, cue);
            }
        }
        
        private static void WriteSound(BinaryWriter writer, Sound sound)
        {
            writer.Write(sound.Name);
            writer.Write(sound.Approved);
            writer.Write(sound.Data.LongLength);
            writer.Write(sound.Data);
            writer.Write(sound.FilePath);
            writer.Write(sound.IsMuted);
            writer.Write(sound.Looped);
            writer.Write(sound.Instances.HasValue ? sound.Instances.Value : int.MinValue);
            writer.Write(sound.Pan.HasValue ? sound.Pan.Value : float.MinValue);
            writer.Write(sound.Pitch.HasValue ? sound.Pitch.Value : float.MinValue);
            writer.Write(sound.Volume.HasValue ? sound.Volume.Value : float.MinValue);
            writer.Write(sound.FileLastModified.ToBinary());
        }

        private static void ReadSound(BinaryReader reader, INode cue)
        {
            var name = reader.ReadString();
            var approved = reader.ReadBoolean();
            var dataLength = reader.ReadInt64();
            var data = new byte[dataLength];
            for (var j = 0; j < dataLength; j++)
            {
                data[j] = reader.ReadByte();
            }
            var filePath = reader.ReadString();
            var isMuted = reader.ReadBoolean();
            var looped = reader.ReadBoolean();
            var instances = reader.ReadInt32();
            var pan = reader.ReadSingle();
            var pitch = reader.ReadSingle();
            var volume = reader.ReadSingle();
            var fileLastModified = reader.ReadInt64();

            var sound = new Sound(data)
            {
                Approved = approved,
                Name = name,
                FilePath = filePath,
                IsMuted = isMuted,
                Looped = looped,
                Instances = instances,
                Pan = Math.Abs(pan - float.MinValue) < 0f ? (float?) null : pan,
                Pitch = Math.Abs(pitch - float.MinValue) < 0f ? (float?) null : pitch,
                Volume = Math.Abs(volume - float.MinValue) < 0f ? (float?) null : volume,
                FileLastModified = DateTime.FromBinary(fileLastModified),
                Parent = cue
            };

            cue.Children.Add(sound);
        }
    }
}