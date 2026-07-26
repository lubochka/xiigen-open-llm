// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Net;
using System.Net.Sockets;
using AgentInfrastructure;

public static class DesignConversionExternalVisualJudgeNetwork
{
    public const string DnsResolutionFailedErrorCode = "EXTERNAL_VISUAL_JUDGE_DNS_RESOLUTION_FAILED";
    public const string DnsEmptyErrorCode = "EXTERNAL_VISUAL_JUDGE_DNS_EMPTY";
    public const string DestinationBlockedErrorCode = "EXTERNAL_VISUAL_JUDGE_DNS_DESTINATION_BLOCKED";
    public const string SocketConnectFailedErrorCode = "EXTERNAL_VISUAL_JUDGE_PINNED_CONNECT_FAILED";

    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

    public static HttpClient CreateSecureHttpClient()
        => CreateSecureHttpClient(null, null);

    public static HttpClient CreateSecureHttpClient(
        Func<string, CancellationToken, Task<OperationResult<IReadOnlyList<IPAddress>>>>? resolveAddressesAsync,
        Func<IPAddress, int, CancellationToken, ValueTask<Stream>>? connectSocketAsync)
        => new(CreateSecureSocketsHttpHandler(resolveAddressesAsync, connectSocketAsync), disposeHandler: true)
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

    public static SocketsHttpHandler CreateSecureSocketsHttpHandler(
        Func<string, CancellationToken, Task<OperationResult<IReadOnlyList<IPAddress>>>>? resolveAddressesAsync = null,
        Func<IPAddress, int, CancellationToken, ValueTask<Stream>>? connectSocketAsync = null)
    {
        var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            ConnectTimeout = DefaultTimeout,
            PooledConnectionLifetime = TimeSpan.Zero,
            PooledConnectionIdleTimeout = TimeSpan.Zero,
            UseProxy = false,
            Proxy = null
        };

        handler.ConnectCallback = async (context, cancellationToken) =>
        {
            var connected = await ConnectPinnedAsync(
                context.DnsEndPoint,
                resolveAddressesAsync,
                connectSocketAsync,
                cancellationToken);
            if (!connected.IsSuccess)
                throw new HttpRequestException(connected.ErrorMessage);

            return connected.Value!;
        };

        return handler;
    }

    public static async Task<OperationResult<Stream>> ConnectPinnedAsync(
        DnsEndPoint endPoint,
        Func<string, CancellationToken, Task<OperationResult<IReadOnlyList<IPAddress>>>>? resolveAddressesAsync = null,
        Func<IPAddress, int, CancellationToken, ValueTask<Stream>>? connectSocketAsync = null,
        CancellationToken cancellationToken = default)
    {
        if (endPoint is null || string.IsNullOrWhiteSpace(endPoint.Host))
        {
            return OperationResult<Stream>.Fail(
                "External visual judge DNS target is invalid.",
                DnsResolutionFailedErrorCode);
        }

        if (endPoint.Port <= 0 || endPoint.Port > 65535)
        {
            return OperationResult<Stream>.Fail(
                "External visual judge DNS target is invalid.",
                DnsResolutionFailedErrorCode);
        }

        var resolver = resolveAddressesAsync ?? ResolveWithSystemDnsAsync;
        OperationResult<IReadOnlyList<IPAddress>> resolved;
        try
        {
            resolved = await resolver(endPoint.Host.Trim(), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return OperationResult<Stream>.Fail(
                "External visual judge DNS resolution failed.",
                DnsResolutionFailedErrorCode);
        }

        if (!resolved.IsSuccess)
        {
            return OperationResult<Stream>.Fail(
                "External visual judge DNS resolution failed.",
                DnsResolutionFailedErrorCode);
        }

        var addresses = (resolved.Value ?? Array.Empty<IPAddress>())
            .Where(address => address is not null)
            .ToArray();
        if (addresses.Length == 0)
        {
            return OperationResult<Stream>.Fail(
                "External visual judge DNS resolution returned no usable addresses.",
                DnsEmptyErrorCode);
        }

        if (addresses.Any(DesignConversionBrowserPngEvidenceInspector.IsBlockedDestinationAddress))
        {
            return OperationResult<Stream>.Fail(
                "External visual judge DNS resolution returned a blocked destination.",
                DestinationBlockedErrorCode);
        }

        var connector = connectSocketAsync ?? ConnectSocketAsync;
        try
        {
            var stream = await connector(addresses[0], endPoint.Port, cancellationToken);
            if (stream is null)
            {
                return OperationResult<Stream>.Fail(
                    "External visual judge pinned connection failed.",
                    SocketConnectFailedErrorCode);
            }

            return OperationResult<Stream>.Ok(stream);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return OperationResult<Stream>.Fail(
                "External visual judge pinned connection failed.",
                SocketConnectFailedErrorCode);
        }
    }

    private static async Task<OperationResult<IReadOnlyList<IPAddress>>> ResolveWithSystemDnsAsync(
        string host,
        CancellationToken cancellationToken)
    {
        try
        {
            return OperationResult<IReadOnlyList<IPAddress>>.Ok(
                await Dns.GetHostAddressesAsync(host, cancellationToken));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return OperationResult<IReadOnlyList<IPAddress>>.Fail(
                "External visual judge DNS resolution failed.",
                DnsResolutionFailedErrorCode);
        }
    }

    private static async ValueTask<Stream> ConnectSocketAsync(
        IPAddress address,
        int port,
        CancellationToken cancellationToken)
    {
        var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true
        };

        try
        {
            await socket.ConnectAsync(new IPEndPoint(address, port), cancellationToken);
            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }
}
