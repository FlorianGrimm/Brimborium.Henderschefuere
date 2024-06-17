// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;

namespace Brimborium.Tests.Common;

internal sealed class TestTrailersFeature : IHttpResponseTrailersFeature
{
    public IHeaderDictionary Trailers { get; set; } = new HeaderDictionary();
}
