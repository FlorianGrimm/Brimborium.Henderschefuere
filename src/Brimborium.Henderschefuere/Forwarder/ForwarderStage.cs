// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.Forwarder;

internal enum ForwarderStage : int
{
    SendAsyncStart = 1,
    SendAsyncStop,
    RequestContentTransferStart,
    ResponseContentTransferStart,
    ResponseUpgrade,
}
