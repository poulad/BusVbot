using System;

// ReSharper disable once CheckNamespace
namespace BusV.Data
{
    public class RepositoryException : Exception
    {
        public RepositoryException(string message)
            : base(message)
        {
        }
    }
}
