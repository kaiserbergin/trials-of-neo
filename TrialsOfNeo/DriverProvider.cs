using System;
using Neo4j.Driver;

namespace TrialsOfNeo
{
    public class DriverProvider : IDisposable
    {
        private bool _disposed = false;
        public IDriver Driver { get; }

        ~DriverProvider() => Dispose(false);

        public DriverProvider(NeoDriverConfigurationSettings settings, NeoLogger neoLogger)
        {
            Driver = GraphDatabase.Driver(
                settings.Url, 
                AuthTokens.Basic(settings.Username, settings.Password),
                builder => builder.WithLogger(neoLogger));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Driver?.Dispose();
            }

            _disposed = true;
        }
    }
}