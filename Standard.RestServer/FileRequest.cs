using System.IO;

namespace NETStandard.RestServer
{
    class FileRequest : IFileRequest
    {
        public Stream Stream { get; set; }

        public FileRequest(Stream stream)
        {
            Stream = stream;
        }

        public void ToFile(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                ToStream(fs);
            }
        }

        public void ToStream(Stream destination)
        {
            Stream source = Stream;
            byte[] buf = new byte[4096];
            int bytesRead;
            while ((bytesRead = source.Read(buf, 0, buf.Length)) > 0)
            {
                destination.Write(buf, 0, bytesRead);
            }
        }
    }
}
