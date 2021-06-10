using System;
using System.IO;
using System.Threading.Tasks;

namespace PS2MapTool.Core
{
    /// <summary>
    /// Contains information about an areas data source.
    /// </summary>
    public record AreasInfo : IAsyncDisposable
    {
        /// <summary>
        /// The world of the areas.
        /// </summary>
        public World World { get; init; }

        /// <summary>
        /// The data source.
        /// </summary>
        public Stream DataSource { get; init; }

        /// <summary>
        /// Gets a value indicating if this <see cref="AreasInfo"/> object has been disposed.
        /// </summary>
        public bool IsDisposed { get; protected set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="AreasInfo"/> object.
        /// </summary>
        /// <param name="world">The world of the areas.</param>
        /// <param name="dataSource">The data source.</param>
        public AreasInfo(World world, Stream dataSource)
        {
            World = world;
            DataSource = dataSource;
        }

        /// <summary>
        /// Disposes of the <see cref="DataSource"/>.
        /// </summary>
        /// <returns>A value representing the task.</returns>
        public async ValueTask DisposeAsync()
        {
            // Do not change this code. Put cleanup code in 'DisposeAsync(bool disposing)' method
            await DisposeAsync(disposing: true).ConfigureAwait(false);
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
            GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    await DataSource.DisposeAsync().ConfigureAwait(false);
                }

                IsDisposed = true;
            }
        }
    }
}
