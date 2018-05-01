using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NETStandard.RestServer
{
    public interface IFileRequest
    {
        /// <summary>
        /// The stream.
        /// </summary>
        Stream Stream { get; set; }

        /// <summary>
        /// Throws IO, will overwrite.
        /// </summary>
        void ToFile(string fileName);

        /// <summary>
        /// Copys <see cref="Stream"/> to destination.
        /// </summary>
        void ToStream(Stream destination);
    }
}
