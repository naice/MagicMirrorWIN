using NcodedUniversal.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NcodedUniversal.Storage
{
    /// <summary>
    /// Threadsafe async access to a storagefile, json
    /// </summary>
    /// <typeparam name="StorageContent"></typeparam>
    public class Storage<StorageContent> : IDisposable
        where StorageContent : class, new()
    {
        private StorageContent storageContent;
        private SemaphoreSlim storageLock = new SemaphoreSlim(1, 1);

        private readonly string _name;
        private readonly IStorageIO _storageIO;
        private readonly IConverter _converter;

        public Storage(string name)
            : this(name, Configuration.DefaultStorageIO)
        { }

        public Storage(string name, IStorageIO storageIO)
            : this(name, storageIO, Json.Instance)
        { }

        public Storage(string name, IStorageIO storageIO, IConverter converter)
        {
            _name = name ?? throw new ArgumentException(nameof(name));
            _storageIO = storageIO ?? throw new ArgumentException(nameof(storageIO));
            _converter = converter ?? throw new ArgumentException(nameof(converter));
        }

        private async Task UnsafeOpen()
        {
            string raw = await _storageIO.ReadAllTextAsync(_name);
            if (raw == null)
            {
                storageContent = new StorageContent();
            }
            else
            {
                storageContent = _converter.DeserializeObject<StorageContent>(raw);
            }
        }

        /// <summary>
        /// Opens the storage to gain access for stored information. Will force a reload of persisted data.
        /// </summary>
        public async Task Open()
        {
            await storageLock.WaitAsync();

            try
            {
                await UnsafeOpen();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                storageLock.Release();
            }
        }

        /// <summary>
        /// Performs a threadsafe action on the storage content. Will load persisted data if not done.
        /// </summary>
        /// <param name="action">Action that is performed threadsafe on storage content.</param>
        /// <param name="save">After a successful action the storage content will be persisted.</param>
        public async Task Perform(Action<StorageContent> action, bool save = false)
        {
            await storageLock.WaitAsync();

            if (storageContent == null)
            {
                await UnsafeOpen();
            }

            try
            {
                action(storageContent);

                if (save)
                {
                    await _storageIO.WriteAllTextAsync(
                        _name,
                        _converter.SerializeObject(storageContent));
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                storageLock.Release();
            }
        }

        public async Task Replace(StorageContent content)
        {
            await storageLock.WaitAsync();

            storageContent = content;

            storageLock.Release();
        }

        public async Task<StorageContent> Get()
        {
            await storageLock.WaitAsync();

            if (storageContent == null)
            {
                await UnsafeOpen();
            }

            try
            {
                return storageContent;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                storageLock.Release();
            }
        }

        public void Dispose()
        {
            if (storageLock != null) storageLock.Dispose();
        }
    }
}