// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Brimborium.Henderschefuere.AllInOne.Controllers;

/// <summary>
/// Sample controller.
/// </summary>
[ApiController]
public class HttpController : ControllerBase
{
    /// <summary>
    /// Returns a 200 response.
    /// </summary>
    [HttpGet]
    [Route("/api/noop")]
    public void NoOp()
    {
    }

    /// <summary>
    /// Returns a 200 response dumping all info from the incoming request.
    /// </summary>
    [HttpGet, HttpPost]
    [Route("/api/dump")]
    [Route("/{**catchall}", Order = int.MaxValue)] // Make this the default route if nothing matches
    public async Task<IActionResult> Dump()
    {
        var result = new {
            this.Request.Protocol,
            this.Request.Method,
            this.Request.Scheme,
            Host = this.Request.Host.Value,
            PathBase = this.Request.PathBase.Value,
            Path = this.Request.Path.Value,
            Query = this.Request.QueryString.Value,
            Headers = this.Request.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray()),
            Time = DateTimeOffset.UtcNow,
            Body = await new StreamReader(this.Request.Body).ReadToEndAsync(),
        };

        return this.Ok(result);
    }

    /// <summary>
    /// Returns a 200 response dumping all info from the incoming request.
    /// </summary>
    [HttpGet]
    [Route("/api/statuscode")]
    public void Status(int statusCode)
    {
        this.Response.StatusCode = statusCode;
    }

    /// <summary>
    /// Returns a 200 response dumping all info from the incoming request.
    /// </summary>
    [HttpGet]
    [Route("/api/headers")]
    public void Headers([FromBody] Dictionary<string, string> headers)
    {
        foreach (var (key, value) in headers)
        {
            this.Response.Headers[key] = value;
        }
    }
}
