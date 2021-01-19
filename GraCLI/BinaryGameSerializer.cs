using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GraZaDuzoZaMalo.Model;

namespace GraCLI
{
    public class BinaryGameSerializer : GameSaver
    {
        private BinaryFormatter formatter = new BinaryFormatter();
        public BinaryGameSerializer()
        {
            saveFileName = "save.bin";
        }

        protected override Gra ProcessLoading(Stream stream)
        {
            return (Gra) formatter.Deserialize(stream);
        }

        protected override void ProcessSaving(Stream stream, Gra gra)
        {
            formatter.Serialize(stream, gra);
        }
    }
}
