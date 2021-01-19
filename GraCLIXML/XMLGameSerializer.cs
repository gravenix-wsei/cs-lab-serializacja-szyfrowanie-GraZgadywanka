using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using GraZaDuzoZaMalo.Model;

namespace GraCLI
{
    public class XMLGameSerializer : GameSaver
    {
        DataContractSerializer serializer = new DataContractSerializer(typeof(Gra));
        Aes key;
        public XMLGameSerializer()
        {
            saveFileName = "save.xml";

            key = Aes.Create();
            key.Key = new byte[32] { 12, 13, 42, 21, 53, 23, 32, 12, 68, 32, 53, 5, 120, 230, 12, 12, 5, 59, 240, 32, 64, 132, 65, 75, 2, 12, 23, 90, 65, 26, 73, 251 };
        }
        public override void SerializeGame(Gra gra)
        {
            if (SaveExists)
            {
                DecryptFile();
            }
            using (var stream = new FileStream(saveFileName, FileMode.Create, FileAccess.Write))
            {
                ProcessSaving(stream, gra);
            }
            EncryptFile();
        }
        public override Gra LoadGame()
        {
            if (SaveExists)
            {
                DecryptFile();
                Gra gra = null; 
                using (var stream = new FileStream(saveFileName, FileMode.Open, FileAccess.Read))
                {
                    gra = ProcessLoading(stream);
                }
                EncryptFile();
                return gra;
            }
            else
            {
                throw new SerializationException();
            }
        }

        protected override Gra ProcessLoading(Stream stream)
        {
            return (Gra)serializer.ReadObject(stream);
        }

        protected override void ProcessSaving(Stream stream, Gra gra)
        {
            serializer.WriteObject(stream, gra);
        }

        public void EncryptFile()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(saveFileName);
            XmlElement elementToEncrypt = xmlDoc.GetElementsByTagName("liczbaDoOdgadniecia")[0] as XmlElement;
            EncryptedXml eXml = new EncryptedXml();

            byte[] encryptedElement = eXml.EncryptData(elementToEncrypt, key, false);
            EncryptedData edElement = new EncryptedData();
            edElement.Type = EncryptedXml.XmlEncElementUrl;

            string encryptionMethod = null;

            if (key is Aes)
            {
                encryptionMethod = EncryptedXml.XmlEncAES256Url;
            }
            else
            {
                // Throw an exception if the transform is not AES
                throw new CryptographicException("The specified algorithm is not supported or not recommended for XML Encryption.");
            }

            edElement.EncryptionMethod = new EncryptionMethod(encryptionMethod);
            edElement.CipherData.CipherValue = encryptedElement;
            EncryptedXml.ReplaceElement(elementToEncrypt, edElement, false);
            using (var stream = new FileStream(saveFileName, FileMode.Create, FileAccess.Write))
            {
                xmlDoc.Save(stream);
            }
        }

        public void DecryptFile()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            xmlDoc.Load(saveFileName);
            XmlElement encryptedElement = xmlDoc.GetElementsByTagName("EncryptedData")[0] as XmlElement;

            // If the EncryptedData element was not found, throw an exception.
            if (encryptedElement == null)
            {
                throw new XmlException("The EncryptedData element was not found.");
            }

            // Create an EncryptedData object and populate it.
            EncryptedData edElement = new EncryptedData();
            edElement.LoadXml(encryptedElement);

            // Create a new EncryptedXml object.
            EncryptedXml exml = new EncryptedXml();

            // Decrypt the element using the symmetric key.
            byte[] rgbOutput = exml.DecryptData(edElement, key);

            // Replace the encryptedData element with the plaintext XML element.
            exml.ReplaceData(encryptedElement, rgbOutput);
            using (var stream = new FileStream(saveFileName, FileMode.Create, FileAccess.Write))
            {
                xmlDoc.Save(stream);
            }
        }
    }
}
