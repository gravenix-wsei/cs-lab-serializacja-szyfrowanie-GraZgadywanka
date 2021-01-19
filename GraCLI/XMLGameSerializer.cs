using System;
using System.IO;
using System.Runtime.Serialization;
using GraZaDuzoZaMalo.Model;

namespace GraCLI
{
    public class XMLGameSerializer : GameSaver
    {
        DataContractSerializer serializer = new DataContractSerializer(typeof(Gra));
        public XMLGameSerializer()
        {
            saveFileName = "save.xml";
        }

        protected override Gra ProcessLoading(Stream stream)
        {
            return (Gra) serializer.ReadObject(stream);
        }

        protected override void ProcessSaving(Stream stream, Gra gra)
        {
            serializer.WriteObject(stream, gra);
        }
    }
}
