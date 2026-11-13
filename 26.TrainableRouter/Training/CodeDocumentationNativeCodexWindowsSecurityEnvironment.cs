// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace TrainableRouter.Training;

using System.Buffers.Binary;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using ContinuousLearning;

/// <summary>RESOURCE-only PSEC ownership. This cannot issue MAIN permission or a launch binding.</summary>
internal sealed class CodeDocumentationNativeCodexWindowsSecurityEnvironment : IDisposable
{
    // ABI/schema source: microsoft/mxc 6cd3d58f05d3447e67109cfb75e042803b843ca4,
    // external/windows-sdk/ProcessSecurityEnvironment.fbs and windows/src/secenv.rs (MIT).
    private readonly FileStream image;
    private readonly string imagePath;
    private readonly long imageLength;
    private readonly DateTime imageModified;
    private readonly CodeDocumentationNativeCodexInstalledMainInputs? inputs;
    private readonly CodeDocumentationNativeCodexInstalledResources? resources;
    private readonly NativeLifetime lifetime;
    private bool invalidated, disposed;

    private CodeDocumentationNativeCodexWindowsSecurityEnvironment(FileStream image, string path,
        NativeLifetime lifetime, CodeDocumentationNativeCodexInstalledMainInputs? inputs,
        CodeDocumentationNativeCodexInstalledResources? resources)
    {
        this.image = image; imagePath = path; imageLength = image.Length; imageModified = File.GetLastWriteTimeUtc(path);
        this.lifetime = lifetime; this.inputs = inputs; this.resources = resources;
    }

    internal sealed class Refusal : IOException
    {
        internal Refusal(string operation, int nativeResult) : base(operation + "; native-result=0x" + unchecked((uint)nativeResult).ToString("X8"))
        { Operation = operation; NativeResult = nativeResult; }
        internal Refusal(string operation) : base(operation + "; predicate-refused; native-result=not-applicable") { Operation = operation; }
        internal Refusal(string operation, Exception original) : base(operation + "; cause-source=FileStream-acquisition; original-type=" + original.GetType().Name +
            "; original-hresult=0x" + unchecked((uint)original.HResult).ToString("X8"), original)
        { Operation = operation; NativeResult = original.HResult; HResult = original.HResult; } // Preserve the actual filesystem HRESULT/cause; this is not a Query/Create result.
        internal string Operation { get; }
        internal int? NativeResult { get; }
    }

    internal static CodeDocumentationNativeCodexWindowsSecurityEnvironment Open(
        CodeDocumentationNativeCodexInstalledMainInputs inputs, CodeDocumentationNativeCodexInstalledResources resources, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!inputs.IsCurrentForResources(resources)) throw new IOException("RESOURCE security environment lost its original MAIN inputs.");
        return Create(resources.WorkingDirectory, resources.ReleaseDirectory, inputs, resources, token);
    }

    // A fixture owner is permanently unable to match installed inputs/resources. It grants no installed authority.
    internal static CodeDocumentationNativeCodexWindowsSecurityEnvironment OpenPublicFixture(CodeDocumentationNativeCodexWindowsProcess.PublicProtectionFixture fixture, CancellationToken token)
        => Create(fixture.Session, fixture.Release, null, null, token);

    private static CodeDocumentationNativeCodexWindowsSecurityEnvironment Create(string session, string release,
        CodeDocumentationNativeCodexInstalledMainInputs? inputs, CodeDocumentationNativeCodexInstalledResources? resources, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!OperatingSystem.IsWindows()) throw new Refusal("Windows security environment platform unavailable");
        var path = Path.Combine(Environment.SystemDirectory, "processmodel.dll");
        FileStream? file = null; NativeLifetime? pending = null; Exception? primary = null; byte[]? wire = null;
        try
        {
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path)) throw new Refusal("System32 processmodel path redirected");
            file = OpenModuleImageForRetention(path); // Only this fixed System32 acquisition can enter production module absence classification.
            if (file.Length is <= 0 or > 16777216 || !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(file.SafeFileHandle, path))
                throw new Refusal("System32 processmodel physical identity unavailable");
            pending = NativeLifetime.Load(path);
            token.ThrowIfCancellationRequested();
            pending.RequireDenySupport(); // An export/build label is never a kernel-support certificate.
            wire = EncodeResourcePolicy(session, release, Environment.SystemDirectory);
            token.ThrowIfCancellationRequested();
            if (inputs is not null && !inputs.IsCurrentForResources(resources!)) throw new IOException("RESOURCE inputs changed before security environment creation.");
            pending.Create(wire); // flags0; actual HRESULT and nonnull handle decide success.
            token.ThrowIfCancellationRequested();
            var owner = new CodeDocumentationNativeCodexWindowsSecurityEnvironment(file, path, pending, inputs, resources);
            if (!owner.IsCurrent()) throw new IOException("System32 processmodel identity changed during acquisition.");
            file = null; pending = null; return owner;
        }
        catch (Exception error) { primary = error; throw; }
        finally
        {
            try { CloseOwners(primary, pending, file); }
            finally { if (wire is not null) Array.Clear(wire); }
        }
    }

    // Shared readonly file-acquisition seam; an arbitrary test leaf cannot mint any environment or installed owner.
    internal static FileStream OpenModuleImageForRetention(string path)
    {
        try { return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read); }
        catch (Exception original) when (original is FileNotFoundException or DirectoryNotFoundException)
        { throw ClassifyModuleImageAcquisition(original); } // Only actual absence from this operation is translated; all other faults escape unchanged.
    }
    internal static Exception ClassifyModuleImageAcquisition(Exception original)
        => original is FileNotFoundException or DirectoryNotFoundException
            ? new Refusal("PSEC module FileStream acquisition unavailable: " + original.GetType().Name, original) : original;

    internal bool IsCurrentFor(CodeDocumentationNativeCodexInstalledMainInputs expected, CodeDocumentationNativeCodexInstalledResources expectedResources)
        => inputs is not null && resources is not null && ReferenceEquals(inputs, expected) && ReferenceEquals(resources, expectedResources) && IsCurrent();

    internal bool IsCurrent()
    {
        if (disposed || invalidated) return false;
        try
        {
            if (lifetime.Handle != IntPtr.Zero && image.Length == imageLength && File.GetLastWriteTimeUtc(imagePath) == imageModified &&
                !SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(imagePath) && SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(image.SafeFileHandle, imagePath)) return true;
        }
        catch { invalidated = true; throw; }
        invalidated = true; return false;
    }

    internal IntPtr Handle => IsCurrent() ? lifetime.Handle : throw new IOException("RESOURCE security environment is no longer current.");
    public void Dispose()
    { if (disposed) return; disposed = true; CloseOwners(null, lifetime, image); }

    // Release-only helper shared by actual pending/accepted owners and deny-only data fixtures. It issues no owner or authority.
    internal static void CloseOwners(Exception? primary, IDisposable? environment, IDisposable? image)
    {
        var first = primary; var owned = new[] { environment, image };
        for (var i = 0; i < owned.Length; i++)
        {
            try { owned[i]?.Dispose(); }
            catch (Exception cleanup)
            { if (first is null) first = cleanup; else first.Data["SecurityEnvironmentOwnerCleanup" + i] = cleanup.GetType().Name + ":" + cleanup.HResult; }
        }
        if (primary is null && first is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(first).Throw();
    }

    /// <summary>Closed forward FlatBuffer writer; no schema/compiler/package or permission factory.</summary>
    internal static byte[] EncodeResourcePolicy(string session, string release, string runtime)
    {
        string[] grants = [session, release, runtime];
        foreach (var path in grants)
            if (string.IsNullOrWhiteSpace(path) || path.IndexOf('\0') >= 0 || !Path.IsPathFullyQualified(path) ||
                !StringComparer.OrdinalIgnoreCase.Equals(Path.GetFullPath(path), path)) throw new IOException("PSEC requires complete canonical absolute paths.");
        if (grants.Select(Path.TrimEndingDirectorySeparator).Distinct(StringComparer.OrdinalIgnoreCase).Count() != 3)
            throw new IOException("PSEC read-only grants are ambiguous.");
        var deny = Path.Combine(session, CodeDocumentationNativeCodexSelectionWire.RequestFileName);
        var writer = new ResourceWire();
        // Schema slots0 version(struct),5 fs_read_only,6 fs_deny,7 network_policy. All other slots absent/default.
        var root = writer.Table([4, 0, 0, 0, 0, 8, 12, 16], 20);
        writer.UInt32(0, (uint)root); writer.UInt16(root + 4, 1); writer.UInt16(root + 6, 0);
        writer.StringVector(root + 8, grants); writer.StringVector(root + 12, [deny]);
        // NetworkPolicy slots0 proxy absent,1 explicit egress,2 peer absent.
        var network = writer.Table([0, 4, 0], 8); writer.Offset(root + 16, network);
        var egress = writer.Table([4, 0, 0], 8); writer.Offset(network + 4, egress);
        // EndpointPolicy slot0 FilterAction deny(0) explicitly present; no allow/deny rule vectors.
        return writer.Finish();
    }

    private sealed class ResourceWire
    {
        private const int Maximum = 65536;
        private static readonly UTF8Encoding StrictUtf8 = new(false, true);
        private readonly byte[] bytes = new byte[Maximum]; private int used = 8;
        internal ResourceWire() { "PSEC"u8.CopyTo(bytes.AsSpan(4)); }
        private int Allocate(int count, int alignment = 4)
        {
            var start = checked((used + alignment - 1) & ~(alignment - 1));
            if (count < 0 || start > Maximum - count) throw new IOException("PSEC exceeds its 65536-byte native allocation bound.");
            used = start + count; return start;
        }
        internal void UInt16(int position, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(position, 2), value);
        internal void UInt32(int position, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(position, 4), value);
        internal void Offset(int position, int target) => UInt32(position, checked((uint)(target - position)));
        internal int Table(ushort[] slots, int objectSize)
        {
            var vtable = Allocate(checked(4 + slots.Length * 2), 2); UInt16(vtable, checked((ushort)(4 + slots.Length * 2))); UInt16(vtable + 2, checked((ushort)objectSize));
            for (var i = 0; i < slots.Length; i++) UInt16(vtable + 4 + i * 2, slots[i]);
            var table = Allocate(objectSize); UInt32(table, checked((uint)(table - vtable))); return table;
        }
        internal void StringVector(int field, string[] values)
        {
            var vector = Allocate(checked(4 + values.Length * 4)); Offset(field, vector); UInt32(vector, (uint)values.Length);
            for (var i = 0; i < values.Length; i++)
            {
                int length; try { length = StrictUtf8.GetByteCount(values[i]); } catch (EncoderFallbackException error) { throw new IOException("PSEC path is not complete UTF8.", error); }
                var text = Allocate(checked(5 + length)); UInt32(text, (uint)length);
                StrictUtf8.GetBytes(values[i], bytes.AsSpan(text + 4, length)); Offset(vector + 4 + i * 4, text);
            }
        }
        internal byte[] Finish() => bytes.AsSpan(0, used).ToArray();
    }

    // This acquisition/cleanup unit is shared with data-only fault tests. It is not an installed owner.
    internal sealed class NativeLifetime : IDisposable
    {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate int Query(out ulong support);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate int CreateEnvironment(byte[] wire, uint size, uint flags, out IntPtr handle);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] internal delegate void CloseEnvironment(IntPtr handle);
        private readonly Query query; private readonly CreateEnvironment create; private readonly CloseEnvironment close; private readonly Action unload;
        internal IntPtr Handle { get; private set; } private bool disposed, attempted;
        internal NativeLifetime(Query query, CreateEnvironment create, CloseEnvironment close, Action unload)
        { this.query = query; this.create = create; this.close = close; this.unload = unload; }
        internal static NativeLifetime Load(string path)
        {
            var module = LoadLibraryExW(path, IntPtr.Zero, 0x800); // Absolute System32 image and System32-only dependency lookup.
            if (module == IntPtr.Zero) throw new Refusal("LoadLibraryExW System32 processmodel", Marshal.GetLastWin32Error());
            try
            {
                var name = new StringBuilder(32768); var length = GetModuleFileNameW(module, name, name.Capacity);
                if (length == 0 || length >= name.Capacity || !StringComparer.OrdinalIgnoreCase.Equals(name.ToString(), path))
                    throw new Refusal("Loaded processmodel physical path mismatch");
                T Export<T>(string name) where T : Delegate
                {
                    var address = GetProcAddress(module, name);
                    if (address == IntPtr.Zero) throw new Refusal("GetProcAddress " + name, Marshal.GetLastWin32Error());
                    return Marshal.GetDelegateForFunctionPointer<T>(address);
                }
                return new(Export<Query>("QueryProcessSecurityEnvironmentSupport"), Export<CreateEnvironment>("CreateProcessSecurityEnvironment"),
                    Export<CloseEnvironment>("CloseProcessSecurityEnvironment"), () => { if (!FreeLibrary(module)) throw new Win32Exception(Marshal.GetLastWin32Error(), "FreeLibrary processmodel failed."); });
            }
            catch (Exception error) { if (!FreeLibrary(module)) error.Data["SecurityEnvironmentModuleCleanup"] = Marshal.GetLastWin32Error(); throw; }
        }
        internal void RequireDenySupport()
        {
            if (disposed) throw new ObjectDisposedException(nameof(NativeLifetime));
            var result = query(out var flags);
            if (result < 0) throw new Refusal("QueryProcessSecurityEnvironmentSupport", result);
            if ((flags & 1) == 0) throw new Refusal("QueryProcessSecurityEnvironmentSupport missing FS_DENY; support-flags=0x" + flags.ToString("X16"), result);
        }
        internal void Create(byte[] wire)
        {
            if (disposed || attempted) throw new InvalidOperationException("Security environment creation cannot repeat.");
            attempted = true;
            var handle = IntPtr.Zero; int result;
            try { result = create(wire, checked((uint)wire.Length), 0, out handle); }
            finally { Handle = handle; } // Retain even a partial out-handle when marshalling or a deny-only fixture throws.
            if (result < 0) throw new Refusal("CreateProcessSecurityEnvironment", result);
            if (handle == IntPtr.Zero || handle == new IntPtr(-1)) throw new Refusal("CreateProcessSecurityEnvironment returned invalid handle", result);
        }
        public void Dispose()
        {
            if (disposed) return; disposed = true;
            Exception? primary = null;
            try { if (Handle != IntPtr.Zero && Handle != new IntPtr(-1)) { var handle = Handle; Handle = IntPtr.Zero; close(handle); } }
            catch (Exception error) { primary = error; }
            try { unload(); }
            catch (Exception cleanup) { if (primary is null) primary = cleanup; else primary.Data["SecurityEnvironmentModuleCleanup"] = cleanup.GetType().Name + ":" + cleanup.HResult; }
            if (primary is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(primary).Throw();
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr LoadLibraryExW(string path, IntPtr file, uint flags);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)] private static extern IntPtr GetProcAddress(IntPtr module, string name);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern uint GetModuleFileNameW(IntPtr module, StringBuilder path, int size);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool FreeLibrary(IntPtr module);
    }
}
