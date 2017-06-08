// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;

namespace Microsoft.Azure.WebJobs
{
    public static class EventGridJobHostConfigurationExtensions
    {
        public static void UseEventGrid(this JobHostConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            // Register our extension configuration provider
            // done by the function runtime
            config.RegisterExtensionConfigProvider(new EventGridExtensionConfig());
        }
    }
}
