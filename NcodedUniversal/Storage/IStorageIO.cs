using System.Threading.Tasks;

namespace NcodedUniversal.Storage
{
    public interface IStorageIO
    {
        /// <summary>
        /// Reads all file contents to text.
        /// </summary>
        /// <param name="name">Name of the file to read.</param>
        /// <returns>null if not exists otherwise the text.</returns>
        Task<string> ReadAllTextAsync(string name);

        /// <summary>
        /// Writes all text to the file.
        /// </summary>
        /// <param name="name">Name of the file.</param>
        /// <param name="text">Text to be written.</param>
        Task WriteAllTextAsync(string name, string text);
    }
}