// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Runtime.InteropServices;
using AgentInfrastructure;
using Microsoft.Win32.SafeHandles;

internal sealed class DesignConversionDeterministicMutationLease : IDisposable, IAsyncDisposable
{
    private readonly FileStream _stream;
    private bool _disposed;

    internal DesignConversionDeterministicMutationLease(string lockPath, FileStream stream)
    {
        LockPath = lockPath;
        _stream = stream;
    }

    internal string LockPath { get; }

    internal FileStream Stream => _stream;

    public void Dispose()
    {
        if (_disposed)
            return;

        _stream.Dispose();
        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await _stream.DisposeAsync();
        _disposed = true;
    }
}

internal static class DesignConversionDeterministicMutationLock
{
    internal const int WindowsSharingViolationNativeCode = 32;
    internal const int WindowsLockViolationNativeCode = 33;
    internal const int LinuxWouldBlockNativeCode = 11;

    private const int LinuxFlockExclusive = 2;
    private const int LinuxFlockNonBlocking = 4;

    internal static OperationResult<DesignConversionDeterministicMutationLease> TryAcquire(
        string lockPath,
        string conflictMessage,
        string conflictCode,
        string failureCode)
    {
        try
        {
            return OperationResult<DesignConversionDeterministicMutationLease>.Ok(OpenLease(lockPath));
        }
        catch (IOException ex) when (IsContention(ex))
        {
            return OperationResult<DesignConversionDeterministicMutationLease>.Fail(
                conflictMessage,
                conflictCode);
        }
        catch (Exception ex)
        {
            return OperationResult<DesignConversionDeterministicMutationLease>.Fail(
                ex.Message,
                failureCode);
        }
    }

    internal static async Task<DesignConversionDeterministicMutationLease> WaitUntilAcquiredAsync(
        string lockPath,
        TimeSpan retryDelay,
        CancellationToken ct)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                return OpenLease(lockPath);
            }
            catch (IOException ex) when (IsContention(ex))
            {
                await Task.Delay(retryDelay, ct);
            }
        }
    }

    internal static DesignConversionDeterministicMutationLease AcquireForTesting(string lockPath)
        => OpenLease(lockPath);

    internal static bool IsContentionForTesting(IOException exception)
        => IsContention(exception);

    internal static IOException CreateLinuxFlockContentionForTesting()
        => LinuxFlockContentionIOException.Create("test-only-linux-flock-contention", LinuxWouldBlockNativeCode);

    private static DesignConversionDeterministicMutationLease OpenLease(string lockPath)
    {
        if (string.IsNullOrWhiteSpace(lockPath))
            throw new ArgumentException("DesignConversion mutation lock path is required.", nameof(lockPath));

        var fullLockPath = Path.GetFullPath(lockPath);
        var directory = Path.GetDirectoryName(fullLockPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        FileStream? stream = null;
        try
        {
            stream = OpenLeaseStream(fullLockPath);
            if (OperatingSystem.IsLinux())
                AcquireLinuxFlock(stream, fullLockPath);

            // Файл lock-а является только стабильным root-local sidecar-именем:
            // вся аренда живёт в OS fd/flock, поэтому payload тут запрещён.
            stream.SetLength(0);
            return new DesignConversionDeterministicMutationLease(fullLockPath, stream);
        }
        catch
        {
            stream?.Dispose();
            throw;
        }
    }

    private static FileStream OpenLeaseStream(string fullLockPath)
    {
        try
        {
            return new FileStream(
                fullLockPath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                OperatingSystem.IsLinux() ? FileShare.ReadWrite | FileShare.Delete : FileShare.None,
                bufferSize: 1,
                FileOptions.Asynchronous);
        }
        catch (IOException ex) when (OperatingSystem.IsLinux() && GetNativeErrorCode(ex) == LinuxWouldBlockNativeCode)
        {
            // На Linux .NET может вернуть EAGAIN уже при открытии sidecar-файла,
            // до нашего явного flock. Нормализуем его только здесь, в узком
            // acquire-пути mutation lease, чтобы чужой raw IOException(11) не стал
            // фальшивым сигналом "занято" для остального хранилища.
            throw LinuxFlockContentionIOException.Create(fullLockPath, LinuxWouldBlockNativeCode);
        }
    }

    private static void AcquireLinuxFlock(FileStream stream, string lockPath)
    {
        var descriptor = ToFileDescriptor(stream.SafeFileHandle);
        if (FlockLinux(descriptor, LinuxFlockExclusive | LinuxFlockNonBlocking) == 0)
            return;

        throw LinuxFlockContentionIOException.Create(lockPath, Marshal.GetLastPInvokeError());
    }

    private static int ToFileDescriptor(SafeFileHandle handle)
        => checked((int)handle.DangerousGetHandle().ToInt64());

    private static bool IsContention(IOException exception)
    {
        if (exception is LinuxFlockContentionIOException { NativeErrorCode: LinuxWouldBlockNativeCode })
            return true;

        return GetNativeErrorCode(exception) is
            WindowsSharingViolationNativeCode or WindowsLockViolationNativeCode;
    }

    private static int GetNativeErrorCode(IOException exception)
        => exception.HResult & 0xFFFF;

    [DllImport("libc", EntryPoint = "flock", SetLastError = true)]
    private static extern int FlockLinux(int fileDescriptor, int operation);

    private sealed class LinuxFlockContentionIOException : IOException
    {
        private LinuxFlockContentionIOException(string message, int nativeErrorCode)
            : base(message, nativeErrorCode)
        {
            NativeErrorCode = nativeErrorCode;
        }

        internal int NativeErrorCode { get; }

        internal static IOException Create(string lockPath, int nativeErrorCode)
            => new LinuxFlockContentionIOException(
                $"DesignConversion mutation lock flock acquisition failed for '{lockPath}'.",
                nativeErrorCode);
    }
}
