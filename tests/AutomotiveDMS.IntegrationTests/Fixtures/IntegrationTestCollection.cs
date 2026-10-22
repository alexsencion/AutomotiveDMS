using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.IntegrationTests.Fixtures
{
    [CollectionDefinition(Name, DisableParallelization = true)]
    public class IntegrationTestCollection : ICollectionFixture<WebAppFixture>
    {
        public const string Name = "Integration Tests Collection";
    }
}
