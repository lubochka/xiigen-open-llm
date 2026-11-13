// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.IO.Pipes;

namespace OssEquivalents;

// Machine Control Protocol server over Windows named pipes.
// Listens for incoming JSON tool-call requests, dispatches them, and sends back responses.
// This is the .NET equivalent of @modelcontextprotocol/sdk.
public sealed class McpTransport : IAsyncDisposable
{
    private readonly string            _pipeName;
    private readonly McpToolDispatcher _dispatcher;

    public McpTransport(string pipeName, McpToolDispatcher dispatcher)
    {
        _pipeName   = pipeName;
        _dispatcher = dispatcher;
    }

    // Start listening for one client connection on the named pipe.
    // Reads newline-delimited JSON messages until the pipe closes or cancellation is requested.
    public async Task StartListeningAsync(CancellationToken cancellationToken)
    {
        using var pipe = new NamedPipeServerStream(
            _pipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Message,
            PipeOptions.Asynchronous);

        await pipe.WaitForConnectionAsync(cancellationToken);

        using var reader = new StreamReader(pipe, leaveOpen: true);
        using var writer = new StreamWriter(pipe, leaveOpen: true) { AutoFlush = true };

        while (!cancellationToken.IsCancellationRequested && pipe.IsConnected)
        {
            string? line = await reader.ReadLineAsync(cancellationToken);
            if (line is null) break;

            var result   = await _dispatcher.DispatchAsync(line);
            string reply = result.IsSuccess ? result.Value! : $"{{\"error\":\"{result.ErrorMessage}\"}}";

            await writer.WriteLineAsync(reply);
        }
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
