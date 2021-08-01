using System;
using System.IO;

namespace PS2MapTool.Areas
{
    /// <summary>
    /// Contains information about an areas data source.
    /// </summary>
    public record AreasSourceInfo : IDisposable
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
        /// Gets a value indicating if this <see cref="AreasSourceInfo"/> object has been disposed.
        /// </summary>
        public bool IsDisposed { get; protected set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="AreasSourceInfo"/> object.
        /// </summary>
        /// <param name="world">The world of the areas.</param>
        /// <param name="dataSource">The data source.</param>
        public AreasSourceInfo(World world, Stream dataSource)
        {
            World = world;
            DataSource = dataSource;
        }

        /// <summary>
        /// Disposes of the <see cref="DataSource"/>.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'DisposeAsync(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    DataSource.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}
