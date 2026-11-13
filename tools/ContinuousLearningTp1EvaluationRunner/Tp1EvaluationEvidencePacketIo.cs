// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearningTp1EvaluationRunner;

using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32.SafeHandles;

internal static class Tp1EvaluationEvidencePacketIo
{
    internal static readonly UTF8Encoding StrictUtf8 =
        new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    internal static StringComparer PathComparer { get; } = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    internal static Tp1RetainedEvidencePacket OpenExactPacket(
        string directory,
        IReadOnlyCollection<string> leafNames,
        long maximumArtifactBytes)
        => Tp1RetainedEvidencePacket.Open(
            directory,
            leafNames,
            maximumArtifactBytes);

    internal static string RequireAbsoluteFile(string path)
    {
        if (!Path.IsPathFullyQualified(path))
            throw new Tp1EvidencePacketInputException();
        var full = Path.GetFullPath(path);
        if (!File.Exists(full) || Directory.Exists(full))
            throw new Tp1EvidencePacketInputException();
        return full;
    }

    internal static string RequireAbsoluteDirectory(string path)
    {
        if (!Path.IsPathFullyQualified(path))
            throw new Tp1EvidencePacketInputException();
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
    }

    internal static void RejectExistingReparseSegments(string path)
    {
        var full = Path.GetFullPath(path);
        var root = Path.GetPathRoot(full)
            ?? throw new Tp1EvidencePacketInputException();
        var current = root;
        foreach (var segment in full[root.Length..].Split(
                     Path.DirectorySeparatorChar,
                     StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, segment);
            if (!File.Exists(current) && !Directory.Exists(current))
                break;
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw new Tp1EvidencePacketInputException();
        }
    }

    internal static bool IsContainedBy(string candidate, string root)
        => candidate.StartsWith(
            Path.TrimEndingDirectorySeparator(root) + Path.DirectorySeparatorChar,
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    internal static bool IsSafeIdentity(string value)
        => !string.IsNullOrEmpty(value)
            && value.Length <= 128
            && char.IsAsciiLetterOrDigit(value[0])
            && value.All(static character => char.IsAsciiLetterOrDigit(character)
                || character is '_' or '-' or '.' or ':');

    internal static bool IsSafeWindowsLeafName(string value)
    {
        if (string.IsNullOrEmpty(value)
            || value.EndsWith('.')
            || value.Any(static character => character < ' '
                || character is '<' or '>' or ':' or '"' or '/' or '\\'
                    or '|' or '?' or '*'))
            return false;
        var upperBaseName = value.Split('.', 2)[0].ToUpperInvariant();
        return upperBaseName is not ("CON" or "PRN" or "AUX" or "NUL")
            && !(upperBaseName.Length == 4
                && upperBaseName[..3] is "COM" or "LPT"
                && upperBaseName[3] is >= '1' and <= '9');
    }

    internal static bool IsLowerHex(string value, int length)
        => value is not null
            && value.Length == length
            && value.All(static character => character is >= '0' and <= '9'
                or >= 'a' and <= 'f');

    internal static bool IsCanonicalEvidenceRef(string value)
        => value is not null
            && value.StartsWith("sha256:", StringComparison.Ordinal)
            && IsLowerHex(value[7..], 64);

    internal static string HashRef(byte[] bytes)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    internal static void ValidateStrictUtf8WithoutBom(byte[] bytes)
    {
        if (bytes.AsSpan().StartsWith(Encoding.UTF8.Preamble))
            throw new Tp1EvidencePacketInputException();
        try
        {
            _ = StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException exception)
        {
            throw new Tp1EvidencePacketInputException(exception);
        }
    }

    private static bool IsRecoverable(Exception exception)
        => exception is not (OutOfMemoryException or StackOverflowException
            or AccessViolationException or BadImageFormatException);
}

internal sealed class Tp1RetainedEvidencePacket : IDisposable
{
    private readonly Dictionary<string, Tp1RetainedEvidenceFile> _files;
    private readonly Tp1RetainedEvidenceDirectory _directoryLease;
    private readonly string[] _leafNames;

    private Tp1RetainedEvidencePacket(
        string directory,
        string[] leafNames,
        Tp1RetainedEvidenceDirectory directoryLease,
        Dictionary<string, Tp1RetainedEvidenceFile> files)
    {
        Directory = directory;
        _leafNames = leafNames;
        _directoryLease = directoryLease;
        _files = files;
    }

    internal string Directory { get; }
    internal string PhysicalDirectory => _directoryLease.PhysicalPath;
    internal long TotalLength => _files.Values.Sum(static file => file.Length);

    internal static Tp1RetainedEvidencePacket Open(
        string directory,
        IReadOnlyCollection<string> leafNames,
        long maximumArtifactBytes)
    {
        if (leafNames is null
            || leafNames.Count == 0
            || leafNames.Count != leafNames.Distinct(StringComparer.Ordinal).Count()
            || maximumArtifactBytes <= 0)
            throw new Tp1EvidencePacketInputException();
        var exactLeaves = leafNames.Order(StringComparer.Ordinal).ToArray();
        ValidateExactArtifactSet(directory, exactLeaves);
        Tp1RetainedEvidenceDirectory? directoryLease = null;
        var files = new Dictionary<string, Tp1RetainedEvidenceFile>(
            StringComparer.Ordinal);
        try
        {
            directoryLease = Tp1RetainedEvidenceDirectory.Open(directory);
            foreach (var leaf in exactLeaves)
            {
                files.Add(
                    leaf,
                    Tp1RetainedEvidenceFile.Open(
                        Path.Combine(directory, leaf),
                        maximumArtifactBytes,
                        directoryLease.PhysicalPath));
            }
            var retained = new Tp1RetainedEvidencePacket(
                directory,
                exactLeaves,
                directoryLease,
                files);
            directoryLease = null;
            retained.VerifyUnchanged();
            return retained;
        }
        catch
        {
            foreach (var file in files.Values)
                file.Dispose();
            directoryLease?.Dispose();
            throw;
        }
    }

    internal byte[] ReadAllBytes(string leafName)
    {
        if (!_files.TryGetValue(leafName, out var file))
            throw new Tp1EvidencePacketInputException();
        return file.ReadAllBytes();
    }

    internal void VerifyUnchanged()
    {
        _directoryLease.VerifyUnchanged();
        ValidateExactArtifactSet(Directory, _leafNames);
        foreach (var file in _files.Values)
            file.VerifyUnchanged();
    }

    public void Dispose()
    {
        foreach (var file in _files.Values)
            file.Dispose();
        _directoryLease.Dispose();
    }

    private static void ValidateExactArtifactSet(
        string directory,
        IReadOnlyList<string> exactLeaves)
    {
        try
        {
            var files = System.IO.Directory.EnumerateFiles(directory)
                .Select(Path.GetFileName)
                .Order(StringComparer.Ordinal)
                .ToArray();
            if (!files.SequenceEqual(exactLeaves, StringComparer.Ordinal)
                || System.IO.Directory.EnumerateDirectories(directory).Any())
                throw new Tp1EvidencePacketMismatchException();
        }
        catch (Tp1EvidencePacketException)
        {
            throw;
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            throw new Tp1EvidencePacketInputException(exception);
        }
    }

    private static bool IsRecoverable(Exception exception)
        => exception is not (OutOfMemoryException or StackOverflowException
            or AccessViolationException or BadImageFormatException);
}

internal sealed class Tp1RetainedEvidenceDirectory : IDisposable
{
    private const uint FileReadAttributes = 0x00000080;
    private const uint FileShareRead = 0x00000001;
    private const uint FileShareWrite = 0x00000002;
    private const uint OpenExisting = 3;
    private const uint FileFlagBackupSemantics = 0x02000000;
    private const uint FileFlagOpenReparsePoint = 0x00200000;

    private readonly SafeFileHandle _handle;
    private readonly Tp1EvidenceFileIdentity _identity;
    private readonly string _physicalPath;

    private Tp1RetainedEvidenceDirectory(
        SafeFileHandle handle,
        Tp1EvidenceFileIdentity identity,
        string physicalPath)
    {
        _handle = handle;
        _identity = identity;
        _physicalPath = physicalPath;
    }

    internal string PhysicalPath => _physicalPath;

    internal static Tp1RetainedEvidenceDirectory Open(string path)
    {
        if (!OperatingSystem.IsWindows())
        {
            return new Tp1RetainedEvidenceDirectory(
                new SafeFileHandle(IntPtr.Zero, ownsHandle: false),
                default,
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(path)));
        }
        var handle = CreateFileWindows(
            path,
            FileReadAttributes,
            FileShareRead | FileShareWrite,
            IntPtr.Zero,
            OpenExisting,
            FileFlagBackupSemantics | FileFlagOpenReparsePoint,
            IntPtr.Zero);
        if (handle.IsInvalid)
        {
            handle.Dispose();
            throw new Tp1EvidencePacketInputException();
        }
        try
        {
            var identity = Tp1EvidenceFileIdentity.Read(
                handle,
                length: 0,
                requireDirectory: true);
            var physical = PhysicalPathResolver.NormalizeFinalPath(
                PhysicalPathResolver.GetWindowsFinalPath(handle));
            return new Tp1RetainedEvidenceDirectory(handle, identity, physical);
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    internal void VerifyUnchanged()
    {
        if (!OperatingSystem.IsWindows())
            return;
        var actual = Tp1EvidenceFileIdentity.Read(
            _handle,
            length: 0,
            requireDirectory: true);
        var physical = PhysicalPathResolver.NormalizeFinalPath(
            PhysicalPathResolver.GetWindowsFinalPath(_handle));
        if (actual != _identity
            || !Tp1EvaluationEvidencePacketIo.PathComparer.Equals(
                physical,
                _physicalPath))
            throw new Tp1EvidencePacketMismatchException();
    }

    public void Dispose() => _handle.Dispose();

    [DllImport(
        "kernel32.dll",
        EntryPoint = "CreateFileW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern SafeFileHandle CreateFileWindows(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);
}

internal sealed class Tp1RetainedEvidenceFile : IDisposable
{
    private const uint GenericRead = 0x80000000;
    private const uint FileShareRead = 0x00000001;
    private const uint OpenExisting = 3;
    private const uint FileFlagOpenReparsePoint = 0x00200000;
    private const uint FileFlagSequentialScan = 0x08000000;

    private readonly FileStream _stream;
    private readonly Tp1EvidenceFileIdentity _identity;
    private readonly long _maximumLength;
    private readonly string _physicalPath;
    private readonly string _expectedPhysicalParent;

    private Tp1RetainedEvidenceFile(
        FileStream stream,
        Tp1EvidenceFileIdentity identity,
        long maximumLength,
        string physicalPath,
        string expectedPhysicalParent)
    {
        _stream = stream;
        _identity = identity;
        _maximumLength = maximumLength;
        _physicalPath = physicalPath;
        _expectedPhysicalParent = expectedPhysicalParent;
    }

    internal long Length => _identity.Length;

    internal static Tp1RetainedEvidenceFile Open(
        string path,
        long maximumLength,
        string expectedPhysicalParent)
    {
        FileStream? stream = null;
        try
        {
            if (OperatingSystem.IsWindows())
            {
                var handle = CreateFileWindows(
                    path,
                    GenericRead,
                    FileShareRead,
                    IntPtr.Zero,
                    OpenExisting,
                    FileFlagOpenReparsePoint | FileFlagSequentialScan,
                    IntPtr.Zero);
                if (handle.IsInvalid)
                {
                    handle.Dispose();
                    throw new Tp1EvidencePacketInputException();
                }
                try
                {
                    stream = new FileStream(
                        handle,
                        FileAccess.Read,
                        bufferSize: 64 * 1024,
                        isAsync: false);
                }
                catch
                {
                    handle.Dispose();
                    throw;
                }
            }
            else
            {
                stream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 64 * 1024,
                    FileOptions.SequentialScan);
            }
            var identity = Tp1EvidenceFileIdentity.Read(
                stream.SafeFileHandle,
                stream.Length,
                requireDirectory: false);
            if (identity.Length is <= 0 || identity.Length > maximumLength)
                throw new Tp1EvidencePacketInputException();
            var physicalPath = OperatingSystem.IsWindows()
                ? PhysicalPathResolver.NormalizeFinalPath(
                    PhysicalPathResolver.GetWindowsFinalPath(stream.SafeFileHandle))
                : Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
            ValidatePhysicalParent(physicalPath, expectedPhysicalParent);
            var retained = new Tp1RetainedEvidenceFile(
                stream,
                identity,
                maximumLength,
                physicalPath,
                expectedPhysicalParent);
            stream = null;
            return retained;
        }
        catch (Tp1EvidencePacketException)
        {
            stream?.Dispose();
            throw;
        }
        catch (Exception exception) when (IsRecoverable(exception))
        {
            stream?.Dispose();
            throw new Tp1EvidencePacketInputException(exception);
        }
    }

    internal byte[] ReadAllBytes()
    {
        VerifyUnchanged();
        if (_identity.Length > int.MaxValue)
            throw new Tp1EvidencePacketInputException();
        var bytes = GC.AllocateUninitializedArray<byte>((int)_identity.Length);
        _stream.Position = 0;
        var read = 0;
        while (read < bytes.Length)
        {
            var count = _stream.Read(bytes, read, bytes.Length - read);
            if (count == 0)
                throw new Tp1EvidencePacketMismatchException();
            read += count;
        }
        if (_stream.ReadByte() != -1)
            throw new Tp1EvidencePacketMismatchException();
        VerifyUnchanged();
        return bytes;
    }

    internal void VerifyUnchanged()
    {
        var actual = Tp1EvidenceFileIdentity.Read(
            _stream.SafeFileHandle,
            _stream.Length,
            requireDirectory: false);
        var physicalPath = OperatingSystem.IsWindows()
            ? PhysicalPathResolver.NormalizeFinalPath(
                PhysicalPathResolver.GetWindowsFinalPath(_stream.SafeFileHandle))
            : _physicalPath;
        ValidatePhysicalParent(physicalPath, _expectedPhysicalParent);
        if (actual != _identity
            || actual.Length > _maximumLength
            || !Tp1EvaluationEvidencePacketIo.PathComparer.Equals(
                physicalPath,
                _physicalPath))
            throw new Tp1EvidencePacketMismatchException();
    }

    public void Dispose() => _stream.Dispose();

    private static void ValidatePhysicalParent(
        string physicalPath,
        string expectedPhysicalParent)
    {
        var parent = Path.GetDirectoryName(physicalPath);
        if (string.IsNullOrEmpty(parent)
            || !Tp1EvaluationEvidencePacketIo.PathComparer.Equals(
                Path.TrimEndingDirectorySeparator(parent),
                Path.TrimEndingDirectorySeparator(expectedPhysicalParent)))
            throw new Tp1EvidencePacketMismatchException();
    }

    private static bool IsRecoverable(Exception exception)
        => exception is not (OutOfMemoryException or StackOverflowException
            or AccessViolationException or BadImageFormatException);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "CreateFileW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern SafeFileHandle CreateFileWindows(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);
}

internal readonly record struct Tp1EvidenceFileIdentity(
    uint VolumeSerialNumber,
    uint FileIndexHigh,
    uint FileIndexLow,
    long Length)
{
    internal static Tp1EvidenceFileIdentity Read(
        SafeFileHandle handle,
        long length,
        bool requireDirectory)
    {
        if (!OperatingSystem.IsWindows())
            return new Tp1EvidenceFileIdentity(0, 0, 0, length);
        if (!GetFileInformationByHandle(handle, out var information))
            throw new IOException("Retained source identity is unavailable.");
        var isDirectory =
            (information.FileAttributes & (uint)FileAttributes.Directory) != 0;
        var isReparsePoint =
            (information.FileAttributes & (uint)FileAttributes.ReparsePoint) != 0;
        if (isReparsePoint || isDirectory != requireDirectory)
            throw new IOException("A retained source has the wrong artifact kind.");
        return new Tp1EvidenceFileIdentity(
            information.VolumeSerialNumber,
            information.FileIndexHigh,
            information.FileIndexLow,
            length);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandle(
        SafeFileHandle file,
        out Tp1ByHandleFileInformation fileInformation);
}

[StructLayout(LayoutKind.Sequential)]
internal struct Tp1ByHandleFileInformation
{
    internal uint FileAttributes;
    internal System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
    internal System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
    internal System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
    internal uint VolumeSerialNumber;
    internal uint FileSizeHigh;
    internal uint FileSizeLow;
    internal uint NumberOfLinks;
    internal uint FileIndexHigh;
    internal uint FileIndexLow;
}

internal abstract class Tp1EvidencePacketException(Exception? inner = null)
    : Exception("TP1 evidence packet I/O failed.", inner);

internal sealed class Tp1EvidencePacketInputException(Exception? inner = null)
    : Tp1EvidencePacketException(inner);

internal sealed class Tp1EvidencePacketMismatchException(Exception? inner = null)
    : Tp1EvidencePacketException(inner);
