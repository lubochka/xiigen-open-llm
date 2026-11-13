// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Runtime.InteropServices;
using System.Text;
using DPO;
using Microsoft.Win32.SafeHandles;

internal static partial class LogicalCapabilityFileCustody
{
    private static FileStream? OpenWindowsIdentityStream(
        string path,
        uint desiredAccess,
        FileShare share,
        uint creationDisposition)
    {
        var handle = CreateWindowsIdentityFile(
            path,
            desiredAccess,
            (uint)share,
            IntPtr.Zero,
            creationDisposition,
            WindowsFileAttributeNormal,
            IntPtr.Zero);
        if (handle.IsInvalid)
        {
            handle.Dispose();
            return null;
        }

        return new FileStream(handle, FileAccessFor(desiredAccess));
    }

    private static LogicalCapabilityFileShape CaptureWindowsFileShape(
        SafeFileHandle handle)
    {
        if (!GetWindowsFileInformationByHandle(handle, out var fileInfo) ||
            !GetWindowsFileIdentityByHandle(
                handle,
                WindowsFileIdInfoClass,
                out var identityInfo,
                (uint)Marshal.SizeOf<WindowsFileIdInfo>()))
        {
            throw new IOException("LOGICAL_CAPABILITY_FILE_IDENTITY_PROOF_UNAVAILABLE");
        }

        if ((fileInfo.FileAttributes & WindowsFileAttributeDirectory) != 0)
            throw new IOException("LOGICAL_CAPABILITY_FILE_IDENTITY_NON_REGULAR_FILE");
        if (fileInfo.NumberOfLinks != 1)
            throw new IOException("LOGICAL_CAPABILITY_FILE_IDENTITY_LINK_COUNT_UNSAFE");

        var length = checked(
            ((long)fileInfo.FileSizeHigh << 32) | fileInfo.FileSizeLow);
        return new LogicalCapabilityFileShape(
            length,
            fileInfo.NumberOfLinks,
            new DpoTrainerArtifactIdentity(
                DpoTrainerArtifactIdentityPlatform.Windows,
                identityInfo.VolumeSerialNumber,
                identityInfo.FileIdLow,
                identityInfo.FileIdHigh));
    }

    private static bool WindowsHandleMatchesPath(
        SafeFileHandle handle,
        string canonicalPath)
    {
        var buffer = new StringBuilder(32768);
        var length = GetWindowsFinalPathNameByHandle(
            handle,
            buffer,
            (uint)buffer.Capacity,
            0);
        if (length == 0 || length >= buffer.Capacity)
            return false;
        var value = buffer.ToString();
        if (value.StartsWith("\\\\?\\UNC\\", StringComparison.OrdinalIgnoreCase))
            value = "\\\\" + value[8..];
        else if (value.StartsWith("\\\\?\\", StringComparison.OrdinalIgnoreCase))
            value = value[4..];
        return StringComparer.OrdinalIgnoreCase.Equals(
            Path.GetFullPath(value),
            Path.GetFullPath(canonicalPath));
    }

    private static bool WindowsHandlesHaveSameIdentity(
        SafeFileHandle left,
        SafeFileHandle right)
        => GetWindowsFileInformationByHandle(left, out var leftInfo) &&
           GetWindowsFileInformationByHandle(right, out var rightInfo) &&
           leftInfo.VolumeSerialNumber == rightInfo.VolumeSerialNumber &&
           leftInfo.FileIndexHigh == rightInfo.FileIndexHigh &&
           leftInfo.FileIndexLow == rightInfo.FileIndexLow;

    private static void MarkWindowsIdentityForDeletion(FileStream stream)
    {
        var disposition = new WindowsFileDispositionInformation
        {
            DeleteFile = 1
        };
        if (!SetWindowsFileInformationByHandle(
                stream.SafeFileHandle,
                WindowsFileDispositionInfoClass,
                ref disposition,
                (uint)Marshal.SizeOf<WindowsFileDispositionInformation>()))
        {
            throw new IOException("LOGICAL_CAPABILITY_FILE_DELETE_DISPOSITION_REJECTED");
        }
    }

    private static bool TryCreateWindowsHardLinkForTesting(
        string existingPath,
        string newPath)
        => CreateWindowsHardLink(
            newPath,
            existingPath,
            IntPtr.Zero);

    private const int WindowsFileIdInfoClass = 18;
    private const int WindowsFileDispositionInfoClass = 4;
    private const uint WindowsFileAttributeDirectory = 0x00000010;
    private const uint WindowsFileAttributeNormal = 0x00000080;

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowsFileDispositionInformation
    {
        // Win32 BOOLEAN занимает один байт. Если сделать его bool, Windows
        // получает неверный размер структуры и может не поставить delete-pending.
        [MarshalAs(UnmanagedType.U1)]
        internal byte DeleteFile;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowsFileIdInfo
    {
        internal ulong VolumeSerialNumber;
        internal ulong FileIdLow;
        internal ulong FileIdHigh;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowsByHandleFileInformation
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

    [DllImport(
        "kernel32.dll",
        EntryPoint = "CreateFileW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern SafeFileHandle CreateWindowsIdentityFile(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetFileInformationByHandle",
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowsFileInformationByHandle(
        SafeFileHandle fileHandle,
        out WindowsByHandleFileInformation fileInformation);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetFileInformationByHandleEx",
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowsFileIdentityByHandle(
        SafeFileHandle fileHandle,
        int fileInformationClass,
        out WindowsFileIdInfo fileInformation,
        uint bufferSize);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "SetFileInformationByHandle",
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowsFileInformationByHandle(
        SafeFileHandle fileHandle,
        int fileInformationClass,
        ref WindowsFileDispositionInformation fileInformation,
        uint bufferSize);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetWindowsFinalPathNameByHandle(
        SafeFileHandle handle,
        StringBuilder path,
        uint pathLength,
        uint flags);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "CreateHardLinkW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateWindowsHardLink(
        string newFileName,
        string existingFileName,
        IntPtr securityAttributes);
}
