// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.SftpCustom.Entities
{
    using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
    using System;

    internal class SftpWrapperStream : ServiceProviderWrapperStream
    {

        private long? streamLength = null;

        //<inheritdoc/>
        public override void SetLength(long value) => this.streamLength = value;

        //<inheritdoc/>
        public override long Length => this.streamLength ?? throw new NotSupportedException("Length is not supported for this stream.");
    }
}
