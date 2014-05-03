using System.Security.Cryptography;
using AudioTool.Data;
using NUnit.Framework;

namespace Bell.Tests
{
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void Convert_from_json_to_binary()
        {
            const string source = @"D:\src\rcr-game\meta\rcru.auf"; // <--reference a json file
            const string target = @"D:\src\rcr-game\meta\rcru-bin.auf"; // <--reference a json file
            var document = new JsonFileFormat().Load(source);
            new BinaryFileFormat().Save(target, document);
        }
    }
}
