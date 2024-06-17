// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Brimborium.Henderschefuere.WebSocketsTelemetry;

internal enum WebSocketCloseReason : int
{
    Unknown,
    ClientGracefulClose,
    ServerGracefulClose,
    ClientDisconnect,
    ServerDisconnect,
    ActivityTimeout,
}
