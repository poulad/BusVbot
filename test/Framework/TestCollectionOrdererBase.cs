using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Framework
{
    public abstract class TestCollectionOrdererBase : ITestCollectionOrderer
    {
        private readonly string[] _definedCollections;

        protected TestCollectionOrdererBase(
            string[] definedCollections
        )
        {
            _definedCollections = definedCollections;
        }

        public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollectionsEnumerable)
        {
            var testCollections = testCollectionsEnumerable.ToArray();

            var discoveredCollections = testCollections
                .Select(c => c.DisplayName)
                .ToArray();

            // ensure all discovered collections are in the passed array therefore have an order
            var missingCollections = discoveredCollections
                .Where(c => !_definedCollections.Contains(c))
                .ToArray();
            if (missingCollections.Any())
            {
                throw new InvalidOperationException(string.Format(
                    "{0} collections do not have an order defined: {1}",
                    missingCollections.Length,
                    string.Join(", ", missingCollections)
                ));
            }

            foreach (var collectionName in _definedCollections)
            {
                var collection = testCollections.Single(c => c.DisplayName == collectionName);
                yield return collection;
            }
        }
    }
}
