// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Runtime.CompilerServices;
using System.Security;

[assembly: InternalsVisibleTo("DesignConversionLiveLocalServer")]

namespace ModelServer.DesignConversion;

internal static class DesignConversionLiveLocalReadyFileLifecycle
{
    public static async Task RunAsync(
        Func<Task> startAsync,
        Action writeReadyFile,
        Func<Task> waitForShutdownAsync,
        Action cleanupReadyFile)
    {
        ArgumentNullException.ThrowIfNull(startAsync);
        ArgumentNullException.ThrowIfNull(writeReadyFile);
        ArgumentNullException.ThrowIfNull(waitForShutdownAsync);
        ArgumentNullException.ThrowIfNull(cleanupReadyFile);

        try
        {
            await startAsync();
            writeReadyFile();
            await waitForShutdownAsync();
        }
        finally
        {
            TryCleanupReadyFile(cleanupReadyFile);
        }
    }

    private static void TryCleanupReadyFile(Action cleanupReadyFile)
    {
        try
        {
            cleanupReadyFile();
        }
        catch (Exception exception) when (IsExpectedCleanupFailure(exception))
        {
            // Ready-file cleanup is best-effort. Do not replace a primary lifecycle
            // failure or disclose path-bearing cleanup details.
        }
    }

    private static bool IsExpectedCleanupFailure(Exception exception)
        => exception is ArgumentException
            or NotSupportedException
            or PathTooLongException
            or SecurityException
            or IOException
            or UnauthorizedAccessException;
}
