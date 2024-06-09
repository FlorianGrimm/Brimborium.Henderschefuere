// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Brimborium.Henderschefuere.AllInOne.Controllers;

/// <summary>
/// Sample controller.
/// </summary>
[ApiController]
public class WebSocketsController : ControllerBase
{
    private readonly ILogger<WebSocketsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketsController" /> class.
    /// </summary>
    public WebSocketsController(ILogger<WebSocketsController> logger)
    {
        this._logger = logger;
    }

    /// <summary>
    /// Returns a 200 response.
    /// </summary>
    [HttpGet]
    [Route("/api/websockets")]
    public async Task WebSockets()
    {
        if (!this.HttpContext.WebSockets.IsWebSocketRequest)
        {
            this.HttpContext.Response.ContentType = "text/html";
            await this.HttpContext.Response.SendFileAsync("./wwwroot/index.html");
            return;
        }

        using (var webSocket = await this.HttpContext.WebSockets.AcceptWebSocketAsync())
        {
            this._logger.LogInformation("WebSockets established.");
            await RunPingPongAsync(webSocket, this.HttpContext.RequestAborted);
        }

        this._logger.LogInformation("WebSockets finished.");
    }

    private static async Task RunPingPongAsync(WebSocket webSocket, CancellationToken cancellation)
    {
        var buffer = new byte[1024];
        while (true)
        {
            var message = await webSocket.ReceiveAsync(buffer, cancellation);
            if (message.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Bye", cancellation);
                return;
            }

            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, message.Count),
                message.MessageType,
                message.EndOfMessage,
                cancellation);
        }
    }
}
