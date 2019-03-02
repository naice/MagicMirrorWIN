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
        private SemaphoreSlim storageLock = new SemaphoreSlim(1, 1);

        private readonly string _name;
        private readonly IStorageIO _storageIO;
        private readonly IConverter _converter;

        public StorageContent Content { get; private set; }

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
                Content = new StorageContent();
            }
            else
            {
                Content = _converter.DeserializeObject<StorageContent>(raw);
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
        public async Task Perform(Action<StorageContent> action)
        {
            await storageLock.WaitAsync();

            if (Content == null)
            {
                await UnsafeOpen();
            }

            try
            {
                action(Content);
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
        /// Persists current state.
        /// </summary>
        public async Task<bool> Save()
        {
            await storageLock.WaitAsync();
            try
            {
                await _storageIO.WriteAllTextAsync(
                    _name,
                    _converter.SerializeObject(Content));
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                storageLock.Release();
            }

            return true;
        }

        /// <summary>
        /// Replace the current <see cref="StorageContent"/> object.
        /// </summary>
        public async Task Replace(StorageContent content)
        {
            await storageLock.WaitAsync();

            Content = content;

            storageLock.Release();
        }

        /// <summary>
        /// Loads persisted data to <see cref="StorageContent"/> also returns created object.
        /// </summary>
        public async Task<StorageContent> Get()
        {
            await storageLock.WaitAsync();
            try
            {
                if (Content == null)
                {
                    await UnsafeOpen();
                }
                return Content;
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