using System;
using System.IO;
using System.Runtime.Serialization;
using GraZaDuzoZaMalo.Model;

namespace GraCLI
{
    public abstract class GameSaver
    {
        protected String saveFileName;
        public virtual void SerializeGame(Gra gra)
        {
            using (var stream = new FileStream(saveFileName, FileMode.Create, FileAccess.Write))
            {
                ProcessSaving(stream, gra);
            }
        }
        public virtual Gra LoadGame()
        {
            using (var stream = new FileStream(saveFileName, FileMode.Open, FileAccess.Read))
            {
                return ProcessLoading(stream);
            }
        }
        public void DeleteSave()
        {
            if (SaveExists)
            {
                File.Delete(saveFileName);
            }
        }
        public bool SaveExists { get => File.Exists(saveFileName); }
        protected abstract void ProcessSaving(Stream stream, Gra gra);
        protected abstract Gra ProcessLoading(Stream stream);
    }
}
