using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.IntegrationTests.Fixtures
{
    [CollectionDefinition("Integration Tests Collection", DisableParallelization = true)]
    public class IntegrationTestCollection : ICollectionFixture<WebAppFixture>
    {

    }
}
