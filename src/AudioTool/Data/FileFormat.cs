using System.IO;

namespace AudioTool.Data
{
    public abstract class FileFormat
    {
        public abstract void Save(string filename, Document document);
        public abstract Document Load(string filename);
    }

    public class JsonFileFormat : FileFormat
    {
        public override void Save(string filename, Document document)
        {
            var json = Core.JsonSerializer.Serialize(this);
            File.WriteAllText(filename, json);
        }

        public override Document Load(string filename)
        {
            var json = File.ReadAllText(filename);
            var deserialized = Core.JsonSerializer.Deserialize<Document>(json);
            deserialized.Filename = filename;
            return deserialized;
        }
    }
}
