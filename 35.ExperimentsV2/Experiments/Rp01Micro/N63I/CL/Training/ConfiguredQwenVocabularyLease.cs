// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
namespace ContinuousLearning; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
using System.Buffers.Binary; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
using System.Runtime.InteropServices; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
using System.Security.Cryptography; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
using System.ComponentModel; // Classify normal native loader errors without bypassing platform policy.
using System.Text; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
using AgentInfrastructure; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
using Microsoft.Win32.SafeHandles; // Retain the pinned vocabulary resource and refuse incomplete native evidence.

// Own one pinned Windows vocabulary-only model resource; no backend, context, inference or training is constructed.
internal sealed class ConfiguredQwenVocabularyLease : IDisposable // Retain the pinned vocabulary resource and refuse incomplete native evidence.
{ // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    internal const string Unavailable = "CONFIGURED_QWEN_RESOURCE_UNAVAILABLE"; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    internal const string Mismatch = "CONFIGURED_QWEN_CONFIGURATION_MISMATCH"; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    internal const string Changed = "CONFIGURED_QWEN_RESOURCE_CHANGED"; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    internal const string Unsupported = "CONFIGURED_QWEN_NATIVE_ABI_UNSUPPORTED"; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    internal const string BudgetExceeded = "CONFIGURED_QWEN_TOKEN_BUDGET_EXCEEDED"; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    internal const string ManifestSha = "6488c96fa5faab64bb65cbd30d4289e20e6130ef535a93ef9a49f42eda893ea7"; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    internal const string ModelSha = "dec52a44569a2a25341c4e4d3fee25846eed4f6f0b936278e3a3c900bb99d37c"; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    internal const string RendererSha = "49571a1de65ec98ecdcea07bb4069d63065ceab0c30f397b019aa3c3d16f2d7a"; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    internal const string RendererRevision = "d67ad83426633195089509347ffd4fe795120198-qwen35-single-user-think-false-v1"; // Identify the actual installed Ollama revision's restricted renderer branch, without claiming Go execution.
    internal const string ChatTemplateSha = "a4aee8afcf2e0711942cf848899be66016f8d14a889ff9ede07bca099c28f715"; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private static readonly UTF8Encoding Utf8 = new(false, true); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private static readonly (string Name, string Sha, long Size)[] DllPins = // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    [ // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        ("libllama.dll", "382712d7705344b0ebe3fc984177ea91e3852686ace1b65883d20826dfa0df2c", 3180936), // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        ("ggml-base.dll", "d2f29167f072c6c52b392b8e93aee8e951c283ed7bf3eb2735a7483a6079ce04", 827272), // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        ("ggml.dll", "cd4cb1fac9ac372f13d5f6e53215afbaedd55fa035333c98667982e43d58e6bb", 50568), // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        ("libc++.dll", "bffa5120a8b45b942fdb436068676a24b15926321799165442670584f474cb67", 1767816), // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        ("libunwind.dll", "4a96c969495afcc953f1bce0d4fba71923c5916ca51e298f12cdf9589de4aaee", 107912), // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        ("libomp.dll", "eea774f0f3d0a334b5db5e76dbc8d365530c4c7f824abe84ba2e4c597a104126", 965000) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    ]; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private readonly object gate = new(); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private readonly object logGate = new(); // Native callbacks must never wait for the counting/disposal gate.
    private readonly StringBuilder nativeLog = new(); // Retain bounded native diagnostics, including the vocabulary-only tensor-skip witness.
    private string operation = "native-open"; // Attach the exact resource operation to a fail-closed diagnostic.
    private static int nativeOwnerHeld; // Serialize the process-global llama logging callback and private module lifetime across vocabulary owners.
    private bool ownsNativeRuntime; // Release only the runtime reservation acquired by this lease.
    private readonly List<HeldPath> held = []; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private readonly List<CustodyDependency> syntheticCustody = []; // Asset-free test handles use the same dependency borrow and safe/unsafe cleanup boundary as actual custody.
    private readonly List<IntPtr> modules = []; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private readonly ContinuousLearningTp1DpoConfiguration configuration; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private readonly ProgressCallback progress; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private readonly LogCallback log; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private CancellationToken activeToken; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private NativeModelHandle? model; // The critical SafeHandle roots this lease's callbacks and DLL graph until native model release finishes.
    private IntPtr vocab; // Borrowed vocabulary pointer; never owns allocation and is usable only while the model SafeHandle is borrowed.
    private static ConfiguredQwenVocabularyLease? quarantinedRuntime; // Retain at most the one unsafe native runtime graph instead of unloading possibly live code/callbacks.
    private readonly List<(string Stage, Exception Error)> cleanupFailures = []; // Keep secondary release failures separate from a primary open error or cancellation.
    private readonly List<string> cleanupAttempts = []; // Observe the actual common cleanup pipeline without minting token evidence.
    private bool cleanupCompleted; // Make partial-open and SafeHandle/finalizer cleanup idempotent independently of the public disposed flag.
    private Action<string>? syntheticRelease; // Explicit Synthetic-only replacement for release operations; it never loads resources or tokenizes.
    private FreeModel? freeModel; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private SetLog? setLog; // Detach the retained managed callback before unloading its native module.
    private Tokenize? tokenize; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private bool disposed; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    internal string ResourceFingerprint { get; private set; } = ""; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private ConfiguredQwenVocabularyLease(ContinuousLearningTp1DpoConfiguration configuration) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        this.configuration = configuration; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        progress = (_, _) => !activeToken.IsCancellationRequested; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        log = (_, text, _) => // Keep callback exceptions inside managed code and retain native load errors instead of hiding them.
        { // The pinned library emits UTF-8 log fragments; retain at most 32768 decoded characters.
            try // Never propagate a managed logging exception across the C ABI.
            { // Bound pointer reads independently of any native-reported message length.
                var bytes = new List<byte>(4096); // Native diagnostic text is data, not an instruction or an admission.
                for (var i = 0; i < 4096; i++) { var value = Marshal.ReadByte(text, i); if (value == 0) break; bytes.Add(value); } // Bound each trusted-library fragment before decoding it.
                var fragment = Encoding.UTF8.GetString(bytes.ToArray()); // Logs do not influence tokenizer bytes or token IDs.
                lock (logGate) { nativeLog.Append(fragment); if (nativeLog.Length > 32768) nativeLog.Remove(0, nativeLog.Length - 32768); } // Keep the most recent load diagnostics and tensor-skip evidence.
            } // Logging remains independent of model lifetime and inference state.
            catch (Exception ex) when (ex is ArgumentException or IOException) { lock (logGate) { nativeLog.Append("[native diagnostic read failed: " + ex.GetType().Name + "]"); if (nativeLog.Length > 32768) nativeLog.Remove(0, nativeLog.Length - 32768); } } // Bound the failure witness too, without crossing the ABI.
        }; // Root this exact delegate until the model is freed and the callback is detached.
    } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    internal string CopyNativeDiagnostics() { lock (logGate) return nativeLog.ToString(); } // Copy diagnostic text for explicit native runtime evidence; discovery never calls the loader.

    // Opening is explicit and resource-only; discovery and default builds never touch local native assets.
    internal static OperationResult<ConfiguredQwenVocabularyLease> OpenInstalledWindows(ContinuousLearningTp1DpoConfiguration configuration, CancellationToken token = default) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        token.ThrowIfCancellationRequested(); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        if (!OperatingSystem.IsWindows() || RuntimeInformation.ProcessArchitecture != Architecture.X64 || !HasSupportedLayout()) // Check the complete managed by-value ABI before calling any configured native export.
            return Fail<ConfiguredQwenVocabularyLease>(Unsupported); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        if (configuration.ModelId != "qwen3.5:9b-q4_K_M" || configuration.ExpectedOllamaManifestDigest != ManifestSha || // The existing configuration stores canonical raw lowercase digests, without a transport prefix.
            configuration.ExpectedOllamaModelLayerDigest != ModelSha) // Match the exact configured tag and blob before allocating native resources.
            return Fail<ConfiguredQwenVocabularyLease>(Mismatch); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        var lease = new ConfiguredQwenVocabularyLease(configuration); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        if (Interlocked.CompareExchange(ref nativeOwnerHeld, 1, 0) != 0) return Fail<ConfiguredQwenVocabularyLease>(Unavailable); // Refuse a second native owner instead of replacing a live process-global callback.
        lease.ownsNativeRuntime = true; // All failure paths below dispose this exact acquired reservation.
        try // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            lease.activeToken = token; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            lease.model = new NativeModelHandle(lease); // Allocate the owning SafeHandle before the native load can return an unmanaged model.
            var models = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ollama", "models"); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            var dllRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Ollama", "lib", "ollama"); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            lease.Hold(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Ollama", "ollama.exe"), "11d7729cb18bb4876ad91a14fbe9ba3b6985eaabc3475a62d47d874be24a9b54", 36556680, token); // Bind the installed binary metadata revision; do not execute or identify an active daemon.
            lease.Hold(Path.Combine(models, "manifests", "registry.ollama.ai", "library", "qwen3.5", "9b-q4_K_M"), ManifestSha, 709, token); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            lease.Hold(Path.Combine(models, "blobs", "sha256-be595b49fe22012bd1f5605ec14c7ffa58331783a88a4fd8c22e5fc8ec42cf9f"), "be595b49fe22012bd1f5605ec14c7ffa58331783a88a4fd8c22e5fc8ec42cf9f", 475, token); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            lease.Hold(Path.Combine(models, "blobs", "sha256-9371364b27a52acac9d87f88bd93c9db1174d8d6ec57f6888925cdc1788871ff"), "9371364b27a52acac9d87f88bd93c9db1174d8d6ec57f6888925cdc1788871ff", 65, token); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            lease.Hold(Path.Combine(models, "blobs", "sha256-7339fa418c9ad3e8e12e74ad0fd26a9cc4be8703f9c110728a992b193be85cb2"), "7339fa418c9ad3e8e12e74ad0fd26a9cc4be8703f9c110728a992b193be85cb2", 11355, token); // Bind every declared manifest layer; this resource evidence does not issue legal training authority.
            var modelPath = Path.Combine(models, "blobs", "sha256-" + ModelSha); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            lease.Hold(modelPath, ModelSha, 6594462816, token); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            foreach (var pin in DllPins) lease.Hold(Path.Combine(dllRoot, pin.Name), pin.Sha, pin.Size, token); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            foreach (var pin in DllPins) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                var path = Path.Combine(dllRoot, pin.Name); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                var loaded = GetModuleHandleW(pin.Name); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                if (loaded != IntPtr.Zero && !SameModulePath(loaded, path)) throw new ResourceException(Changed, "A foreign module already occupies " + pin.Name); // Do not substitute a same-basename module from another directory.
                if (loaded != IntPtr.Zero && pin.Name == "libllama.dll") throw new ResourceException(Unavailable, "The configured llama module already has an external process owner."); // A pre-existing model/context owner's global callback cannot be overwritten by this resource-only lease.
            } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            foreach (var pin in DllPins.Reverse()) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                var path = Path.Combine(dllRoot, pin.Name); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                var module = LoadLibraryExW(path, IntPtr.Zero, 0x00000100 | 0x00000800); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                if (module == IntPtr.Zero) throw new ResourceException(Unavailable, "LoadLibraryExW " + path + ": " + new Win32Exception(Marshal.GetLastWin32Error()).Message); // Report policy/loader failure without attempting a bypass or alternate DLL.
                lease.modules.Add(module); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                if (!SameModulePath(module, path)) throw new ResourceException(Changed); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            var llama = lease.modules[^1]; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            if (GetProcAddress(llama, "llama_model_default_params").ToInt64() - llama.ToInt64() != 0xd4420) throw new ResourceException(Unsupported, "The pinned default-params export RVA differs."); // Bind the installed export witness to the header's by-value return convention.
            var defaults = lease.Export<DefaultParams>(llama, "llama_model_default_params"); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            var parameters = defaults(); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            if (parameters.NGpuLayers != -1 || parameters.SplitMode != 1 || parameters.LoadMode != -1 || parameters.MainGpu != 0 || parameters.VocabOnly != 0 || // Validate the observed default bytes before altering the documented by-value ABI.
                parameters.Devices != IntPtr.Zero || parameters.TensorBufferOverrides != IntPtr.Zero || parameters.TensorSplit != IntPtr.Zero || // Default pointers must not introduce an unbound device or tensor override.
                parameters.ProgressCallback != IntPtr.Zero || parameters.ProgressUserData != IntPtr.Zero || parameters.KvOverrides != IntPtr.Zero || // Install only this lease's rooted cancellation callback.
                parameters.CheckTensors != 0 || parameters.ExtraBufferTypes != 1 || parameters.NoHost != 0 || parameters.NoAlloc != 0 || parameters.LoadMtp != 0) // Reject an unsupported default layout rather than interpreting arbitrary native bytes.
                throw new ResourceException(Unsupported); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            parameters.NGpuLayers = 0; parameters.VocabOnly = 1; // Represent the header's C bool as one blittable byte in the by-value model parameters.
            parameters.ProgressCallback = Marshal.GetFunctionPointerForDelegate(lease.progress); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            parameters.ProgressUserData = IntPtr.Zero; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            lease.setLog = lease.Export<SetLog>(llama, "llama_log_set"); lease.setLog(lease.log, IntPtr.Zero); // Root the quiet resource-only callback for the complete native model lifetime.
            lease.freeModel = lease.Export<FreeModel>(llama, "llama_model_free"); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            lease.tokenize = lease.Export<Tokenize>(llama, "llama_tokenize"); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            lease.operation = "llama_model_load_from_file(vocab_only=true,n_gpu_layers=0): " + modelPath; // Allocate one vocabulary-only model resource, with no context or inference calls.
            lease.model.Initialize(lease.Export<LoadModel>(llama, "llama_model_load_from_file")(modelPath, parameters)); // Transfer the returned pointer immediately into critical SafeHandle ownership.
            token.ThrowIfCancellationRequested(); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            if (lease.model.IsInvalid) throw new ResourceException(Unavailable, lease.CopyNativeDiagnostics()); // An unsuccessful native load remains a partial-open cleanup, never a model receipt.
            lease.vocab = lease.Export<GetVocab>(llama, "llama_model_get_vocab")(lease.model.DangerousGetHandle()); // The unpublished lease and its SafeHandle remain rooted throughout initialization.
            if (lease.vocab == IntPtr.Zero || lease.Export<VocabInt>(llama, "llama_vocab_n_tokens")(lease.vocab) != 248320 || // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                lease.Export<VocabBool>(llama, "llama_vocab_get_add_bos")(lease.vocab) || // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                lease.Export<VocabBool>(llama, "llama_vocab_get_add_eos")(lease.vocab) || // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                lease.Export<VocabInt>(llama, "llama_vocab_eos")(lease.vocab) != 248046 || // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                lease.Export<VocabInt>(llama, "llama_vocab_pad")(lease.vocab) != 248044) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                throw new ResourceException(Unsupported); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            lease.Recheck(token); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            lease.ResourceFingerprint = Binding("configured-qwen-resource-v1", configuration.ModelId, ManifestSha, ModelSha, ChatTemplateSha, // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                RendererRevision, RendererSha, "llama-b10434-header:1fbcba4003cfc089fa9681973acb3d9e24e465f32c3506ceb0ec7fa29952bdae-params72-vocab248320-bosfalse-eosfalse-eos248046-pad248044-vocab-only-ngpu0-add-special-false-parse-special-true", // Bind the documented ABI, default witness relation and exact native tokenization flags.
                string.Join("|", lease.held.Select(p => p.Path + ":" + p.Sha + ":" + p.Size))); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            return OperationResult<ConfiguredQwenVocabularyLease>.Ok(lease); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        catch (OperationCanceledException ex) { lease.CleanupPreservingPrimary(ex); throw; } // Cleanup diagnostics cannot replace the exact original cancellation object or token.
        catch (ResourceException ex) { var detail = lease.operation + ": " + ex.Message; lease.CleanupPreservingPrimary(ex); return Fail<ConfiguredQwenVocabularyLease>(ex.Code, lease.WithCleanupDiagnostics(detail)); } // Keep the primary refusal code/message and append separate secondary diagnostics.
        catch (MarshalDirectiveException ex) { var detail = lease.operation + ": " + ex.Message; lease.CleanupPreservingPrimary(ex); return Fail<ConfiguredQwenVocabularyLease>(Unsupported, lease.WithCleanupDiagnostics(detail)); } // Preserve the ABI failure even when a later release fails.
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Win32Exception or ArgumentException or EntryPointNotFoundException or BadImageFormatException) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        { var detail = lease.operation + ": " + ex.GetType().Name + ": " + ex.Message; lease.CleanupPreservingPrimary(ex); return Fail<ConfiguredQwenVocabularyLease>(Unavailable, lease.WithCleanupDiagnostics(detail)); } // Partial-open cleanup releases independent resources while retaining the primary filesystem/loader error.
        catch (Exception ex) { lease.CleanupPreservingPrimary(ex); throw; } // Unexpected partial-open failures also close the owned graph and propagate the identical primary exception.
    } // Retain the pinned vocabulary resource and refuse incomplete native evidence.

    // Token IDs are private-issued from the native vocabulary, then copied into an immutable readback.
    internal OperationResult<TokenReadback> Count(string rendered, ContinuousLearningTp1DpoConfiguration expected, CancellationToken token) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        lock (gate) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            token.ThrowIfCancellationRequested(); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            if (disposed || model is null || model.IsClosed || model.IsInvalid) return Fail<TokenReadback>(Unavailable); // A released model cannot lend a vocabulary pointer.
            if (syntheticRelease is not null) return Fail<TokenReadback>(Unsupported); // The cleanup seam can never issue NativeConfigured token evidence.
            if (expected.ModelId != configuration.ModelId || expected.ExpectedOllamaManifestDigest != configuration.ExpectedOllamaManifestDigest || // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                expected.ExpectedOllamaModelLayerDigest != configuration.ExpectedOllamaModelLayerDigest) return Fail<TokenReadback>(Mismatch); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            var borrowedModel = model; bool modelBorrowed = false; // Keep the critical owner alive for every use of subordinate vocab and native code.
            try // Native counting cannot race finalizer/model release while this borrow exists.
            { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                borrowedModel.DangerousAddRef(ref modelBorrowed); // The SafeHandle owns model lifetime; the vocab pointer is only a temporary borrower.
                Recheck(token); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                var bytes = Utf8.GetBytes(rendered); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                if (bytes.Length > LocalQwenOllamaTeacherInferenceClient.MaximumPromptUtf8Bytes + 1024) return Fail<TokenReadback>(Unsupported); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                var input = Marshal.AllocHGlobal(checked(bytes.Length + 1)); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                try // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                    Marshal.Copy(bytes, 0, input, bytes.Length); Marshal.WriteByte(input, bytes.Length, 0); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                    var required = tokenize!(vocab, input, bytes.Length, IntPtr.Zero, 0, false, true); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                    if (required >= 0 || required == int.MinValue || -required > bytes.Length + 1024) return Fail<TokenReadback>(Unsupported); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                    var count = -required; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                    var output = Marshal.AllocHGlobal(checked(count * sizeof(int))); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                    try // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                    { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                        token.ThrowIfCancellationRequested(); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                        var actual = tokenize(vocab, input, bytes.Length, output, count, false, true); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                        if (actual != count) return Fail<TokenReadback>(Unsupported); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                        var ids = new int[count]; Marshal.Copy(output, ids, 0, count); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                        if (ids.Any(id => id < 0 || id >= 248320)) return Fail<TokenReadback>(Unsupported); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                        Recheck(token); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                        return OperationResult<TokenReadback>.Ok(TokenReadback.Issue(this, ids, Binding("rendered-input-v1", rendered))); // Only the current locked native owner can mint this readback.
                    } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                    finally { Marshal.FreeHGlobal(output); } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                finally { Marshal.FreeHGlobal(input); } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            catch (ResourceException ex) { return Fail<TokenReadback>(ex.Code, operation + ": " + ex.Message); } // Preserve the exact read-only recheck operation on native refusal.
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return Fail<TokenReadback>(Changed); } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            finally { if (modelBorrowed) borrowedModel.DangerousRelease(); } // Release the model borrow after all buffers and native token calls finish.
        } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    } // Retain the pinned vocabulary resource and refuse incomplete native evidence.

    internal OperationResult<bool> ValidateCurrent(CancellationToken token) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        lock (gate) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            token.ThrowIfCancellationRequested(); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            if (disposed) return Fail<bool>(Unavailable); // Closed ownership cannot authorize a final resource readback.
            if (syntheticRelease is not null) return Fail<bool>(Unsupported); // Synthetic cleanup ownership is not a configured vocabulary resource.
            try { Recheck(token); return OperationResult<bool>.Ok(true); } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            catch (ResourceException ex) { return Fail<bool>(ex.Code, operation + ": " + ex.Message); } // Final resource refusal retains the operation rather than impersonating a missing asset.
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return Fail<bool>(Changed); } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    internal sealed class TokenReadback // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        private readonly int[] ids; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        internal int Count => ids.Length; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        internal string ResourceFingerprint { get; } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        internal string RenderedBinding { get; } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        internal int[] CopyIds() => ids.ToArray(); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        private TokenReadback(int[] ids, string resourceFingerprint, string renderedBinding) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        { this.ids = ids.ToArray(); ResourceFingerprint = resourceFingerprint; RenderedBinding = renderedBinding; } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        internal static TokenReadback Issue(ConfiguredQwenVocabularyLease owner, int[] ids, string renderedBinding) // Keep construction inside the private readback type and require the current native count lock.
        { // No public DTO or caller-supplied count can become a native configured readback.
            if (!Monitor.IsEntered(owner.gate) || owner.disposed || owner.syntheticRelease is not null || owner.model is null || owner.model.IsClosed || owner.vocab == IntPtr.Zero) throw new InvalidOperationException("Native count owner is not held."); // Only an actual borrowed model can issue native configured token evidence.
            return new(ids, owner.ResourceFingerprint, renderedBinding); // Detach the exact native output before releasing the lease gate.
        } // A prepared request retains copied evidence, never a raw native pointer.
    } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    internal static string Binding(string domain, params string[] fields) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        foreach (var value in new[] { domain + "\0" }.Concat(fields)) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            var bytes = Utf8.GetBytes(value); Span<byte> length = stackalloc byte[8]; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            BinaryPrimitives.WriteInt64BigEndian(length, bytes.Length); hash.AppendData(length); hash.AppendData(bytes); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant(); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    internal static string TokenIdsSha(int[] ids) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        var bytes = new byte[checked(ids.Length * 4)]; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        for (var i = 0; i < ids.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4, 4), ids[i]); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    } // Retain the pinned vocabulary resource and refuse incomplete native evidence.

    // Retained file and ancestor identities exclude reparse substitution and write/delete sharing.
    private void Hold(string path, string sha, long size, CancellationToken token) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        var full = Path.GetFullPath(path); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        operation = "read/hash/hold: " + full; // Failure diagnostics name the exact read-only resource operation.
        foreach (var directory in Ancestors(Path.GetDirectoryName(full)!)) // Acquire each custody borrow before publishing it to this lease.
        { // Unpublished custody must still balance its dependency if the owner list rejects insertion.
            if (held.Any(p => p.Path == directory)) continue; // Reuse the already owned directory custody instead of adding a second borrow.
            var parent = HeldPath.Open(directory, "", -1); // The new path already owns its explicit SafeHandle dependency reference.
            try { held.Add(parent); } catch { parent.Dispose(); throw; } // Balance unpublished directory custody before preserving the insertion error.
        } // Published paths now belong to safe cleanup or unsafe retained lifetime.
        var file = HeldPath.Open(full, sha, size); // The new file has one dependent custody borrow before verification.
        try { held.Add(file); } catch { file.Dispose(); throw; } // An insertion failure must not strand an untracked SafeHandle dependency.
        file.Verify(token); // Verification failure is handled by the owning lease's partial-open cleanup.
    } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private static IEnumerable<string> Ancestors(string path) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        var paths = new List<string>(); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        for (var directory = new DirectoryInfo(path); directory is not null; directory = directory.Parent) paths.Add(directory.FullName); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        paths.Reverse(); return paths; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private void Recheck(CancellationToken token) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    { foreach (var path in held) { operation = "recheck/hash/hold: " + path.Path; path.Verify(token); } token.ThrowIfCancellationRequested(); } // Every full resource check retains the exact failing path and remains cancellation-bounded by hash chunks.
    private T Export<T>(IntPtr module, string name) where T : Delegate // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        var address = GetProcAddress(module, name); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        if (address == IntPtr.Zero) throw new ResourceException(Unsupported); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        return Marshal.GetDelegateForFunctionPointer<T>(address); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private static bool SameModulePath(IntPtr module, string path) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        var buffer = new StringBuilder(32768); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        var size = GetModuleFileNameW(module, buffer, buffer.Capacity); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        return size > 0 && size < buffer.Capacity && StringComparer.OrdinalIgnoreCase.Equals(Path.GetFullPath(buffer.ToString()), Path.GetFullPath(path)); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    // A forgotten published lease still reaches its critical model owner; partial-open resources also have a finite final cleanup.
    ~ConfiguredQwenVocabularyLease() // Maintain abandoned lease finalization without granting a prepared/native admission.
    { // Enter abandoned lease finalization scope 1.
        try { Dispose(); } catch (Exception ex) { RetainUnsafeRuntime(ex); } // Finalize abandoned custody without letting a release exception escape.
    } // Finish abandoned lease finalization scope without granting new tokenizer or call authority.
    public void Dispose() // Maintain explicit lease close without granting a prepared/native admission.
    { // Enter explicit lease close scope 2.
        lock (gate) // Serialize this lifetime stage against active Count and Dispose.
        { // Enter explicit lease close scope 3.
            if (disposed) return; // A closed lease cannot repeat native free or independent release actions.
            disposed = true; vocab = IntPtr.Zero; // Stop lending the subordinate vocabulary before closing native ownership.
            try { model?.Dispose(); } // Let the real SafeHandle release its model and rooted runtime graph.
            finally // Continue the next independent safe release stage even when the preceding operation failed.
            { // Enter explicit lease close scope 4.
                // An invalid SafeHandle has no ReleaseHandle invocation; partial open still executes the same remaining cleanup.
                if (!cleanupCompleted) ReleaseResources(IntPtr.Zero); // An invalid/partial-open model still needs callback, DLL, path and reservation cleanup.
                GC.SuppressFinalize(this); // Explicit cleanup has now attempted this lease's complete lifetime graph.
            } // Finish explicit lease close scope without granting new tokenizer or call authority.
        } // Finish explicit lease close scope without granting new tokenizer or call authority.
    } // Finish explicit lease close scope without granting new tokenizer or call authority.
    private bool ReleaseResources(IntPtr nativeModel) // Maintain independent model/callback/DLL/path release stages without granting a prepared/native admission.
    { // Enter independent model/callback/DLL/path release stages scope 5.
        lock (gate) // Serialize this lifetime stage against active Count and Dispose.
        { // Enter independent model/callback/DLL/path release stages scope 6.
            if (cleanupCompleted) return nativeModel == IntPtr.Zero; // Idempotence is independent of the public disposed flag.
            cleanupCompleted = true; disposed = true; vocab = IntPtr.Zero; // Stop lending the subordinate vocabulary before closing native ownership.
            var modelReleased = nativeModel == IntPtr.Zero; // A zero pointer represents partial open, with no native model to free.
            try // Bound the following operation inside independent model/callback/DLL/path release stages.
            { // Enter independent model/callback/DLL/path release stages scope 7.
                if (nativeModel != IntPtr.Zero) modelReleased = RunRelease("model", () => freeModel!(nativeModel)); // Native free runs before any callback detachment or DLL unloading.
                else cleanupAttempts.Add("model"); // Observe the no-model partial-open branch without calling a native free.
            } // Finish independent model/callback/DLL/path release stages scope without granting new tokenizer or call authority.
            finally // Continue the next independent safe release stage even when the preceding operation failed.
            { // Enter independent model/callback/DLL/path release stages scope 8.
                // Callback detachment is safe only after successful model free; a failed free has an unknown native outcome.
                var callbacksDetached = modelReleased && RunRelease("callbacks", () => setLog?.Invoke(null, IntPtr.Zero)); // Uncertain model free keeps callback/code roots retained rather than unloading them.
                if (!modelReleased) cleanupAttempts.Add("callbacks-retained"); // Record that detachment is deliberately unsafe after ambiguous model release.
                try // Bound the following operation inside independent model/callback/DLL/path release stages.
                { // Enter independent model/callback/DLL/path release stages scope 9.
                    for (var i = (syntheticRelease is null ? modules.Count : 2) - 1; i >= 0; i--) // Attempt each remaining DLL release independently, in reverse load order.
                    { // Enter independent model/callback/DLL/path release stages scope 10.
                        var index = i; // Capture this release index before invoking its isolated action.
                        if (callbacksDetached) RunRelease("module:" + index, () => // Unload code only after model free and callback detachment both succeeded.
                        { // Enter independent model/callback/DLL/path release stages scope 11.
                            if (!FreeLibrary(modules[index])) throw new Win32Exception(Marshal.GetLastWin32Error()); // Record the exact Windows release failure while continuing the remaining modules.
                        }); // Maintain independent model/callback/DLL/path release stages without granting a prepared/native admission.
                        else cleanupAttempts.Add("module-retained:" + index); // An unsafe runtime retains DLL code instead of executing an invalid unload.
                    } // Finish independent model/callback/DLL/path release stages scope without granting new tokenizer or call authority.
                } // Finish independent model/callback/DLL/path release stages scope without granting new tokenizer or call authority.
                finally // Continue the next independent safe release stage even when the preceding operation failed.
                { // Enter independent model/callback/DLL/path release stages scope 12.
                    try // Bound the following operation inside independent model/callback/DLL/path release stages.
                    { // Enter independent model/callback/DLL/path release stages scope 13.
                        for (var i = (syntheticRelease is null ? held.Count : 2) - 1; i >= 0; i--) // Attempt every independent retained path even after a prior module failure.
                        { // Enter independent model/callback/DLL/path release stages scope 14.
                            var index = i; // Capture this release index before invoking its isolated action.
                            if (callbacksDetached) RunRelease("path:" + index, () => held[index].Dispose()); // Close each safe custody handle through the same isolated release flow.
                            else cleanupAttempts.Add("path-retained:" + index); // Keep original custody with an uncertain live native runtime.
                        } // Finish independent model/callback/DLL/path release stages scope without granting new tokenizer or call authority.
                    } // Finish independent model/callback/DLL/path release stages scope without granting new tokenizer or call authority.
                    finally // Continue the next independent safe release stage even when the preceding operation failed.
                    { // Enter independent model/callback/DLL/path release stages scope 15.
                        for (var i = syntheticCustody.Count - 1; i >= 0; i--) // The asset-free GC control executes the real managed dependency release at the same model/callback boundary.
                        { // Each dependency borrow is independent, just like actual HeldPath.Dispose below the same safety gate.
                            var index = i; // Preserve this dependency index for its release callback.
                            if (callbacksDetached) RunRelease("dependency:" + index, () => syntheticCustody[index].Dispose(), syntheticEffect: false); // A successful native-lifetime boundary permits the actual SafeHandle dependency to drop.
                            else cleanupAttempts.Add("dependency-retained:" + index); // Queued critical finalizers cannot close custody while its explicit dependency borrow remains held.
                        } // Unsafe custody borrows remain owned by the quarantined graph.
                        try // Bound the following operation inside independent model/callback/DLL/path release stages.
                        { // Enter independent model/callback/DLL/path release stages scope 16.
                            if (callbacksDetached) // Successful model/callback release permits remaining cleanup and reservation reuse.
                            { // Enter independent model/callback/DLL/path release stages scope 17.
                                modules.Clear(); held.Clear(); syntheticCustody.Clear(); // Drop managed lists only after all safe independent release attempts.
                                cleanupAttempts.Add("reservation"); // Observe process reservation release after the last path attempt.
                                if (ownsNativeRuntime) { ownsNativeRuntime = false; Interlocked.Exchange(ref nativeOwnerHeld, 0); } // Release only this owner's process slot, allowing a later valid owner.
                            } // Finish independent model/callback/DLL/path release stages scope without granting new tokenizer or call authority.
                            else // Retain unsafe ownership rather than unload code that may still be used.
                            { // Enter independent model/callback/DLL/path release stages scope 18.
                                cleanupAttempts.Add("reservation-retained"); // Uncertain native lifetime keeps the single process reservation held.
                                RetainUnsafeRuntime(null); // Strongly root callbacks, code and custody for the one unresolved runtime.
                            } // Finish independent model/callback/DLL/path release stages scope without granting new tokenizer or call authority.
                        } // Finish independent model/callback/DLL/path release stages scope without granting new tokenizer or call authority.
                        finally { GC.KeepAlive(progress); GC.KeepAlive(log); } // Callbacks stay rooted through the last native/model/module operation.
                    } // Finish independent model/callback/DLL/path release stages scope without granting new tokenizer or call authority.
                } // Finish independent model/callback/DLL/path release stages scope without granting new tokenizer or call authority.
            } // Finish independent model/callback/DLL/path release stages scope without granting new tokenizer or call authority.
            return modelReleased; // SafeHandle reports whether its native model pointer was actually released.
        } // Finish independent model/callback/DLL/path release stages scope without granting new tokenizer or call authority.
    } // Finish independent model/callback/DLL/path release stages scope without granting new tokenizer or call authority.
    private bool RunRelease(string stage, Action nativeRelease, bool syntheticEffect = true) // Isolate release errors; managed dependency release remains real even in the Synthetic resource seam.
    { // Enter one isolated release operation scope 19.
        cleanupAttempts.Add(stage); // Capture the real production cleanup stage before attempting its release.
        try // Bound the following operation inside one isolated release operation.
        { // Enter one isolated release operation scope 20.
            if (syntheticRelease is null || !syntheticEffect) nativeRelease(); // Actual custody dependency lifetime is never replaced by a Synthetic stage callback.
            else syntheticRelease(stage); // Synthetic tests replace release effects only and cannot count a vocabulary.
            return true; // This isolated release operation completed successfully.
        } // Finish one isolated release operation scope without granting new tokenizer or call authority.
        catch (Exception ex) // Capture this secondary release/diagnostic failure within one isolated release operation.
        { // Enter one isolated release operation scope 21.
            // ReleaseHandle cannot throw: retain each error and continue every independent safe release stage.
            cleanupFailures.Add((stage, ex)); return false; // Keep the exact secondary exception object and continue safe cleanup.
        } // Finish one isolated release operation scope without granting new tokenizer or call authority.
    } // Finish one isolated release operation scope without granting new tokenizer or call authority.
    private void RetainUnsafeRuntime(Exception? failure) // Maintain bounded unsafe-runtime retention without granting a prepared/native admission.
    { // Enter bounded unsafe-runtime retention scope 22.
        // One process reservation bounds this retention; never unload code while a model/callback may still use it.
        criticalReleaseFailure ??= failure; disposed = true; vocab = IntPtr.Zero; // Stop lending the subordinate vocabulary before closing native ownership.
        Interlocked.CompareExchange(ref quarantinedRuntime, this, null); // One held process slot bounds this exceptional retained graph.
    } // Finish bounded unsafe-runtime retention scope without granting new tokenizer or call authority.
    private Exception? criticalReleaseFailure; // Retain a critical release failure separately from all primary open exceptions.
    internal (string Stage, Exception Error)[] CopyCleanupFailures() // Maintain copied secondary cleanup diagnostics without granting a prepared/native admission.
    { lock (gate) return cleanupFailures.ToArray(); } // Serialize this lifetime stage against active Count and Dispose.
    internal string[] CopyCleanupAttempts() // Maintain copied cleanup-stage observations without granting a prepared/native admission.
    { lock (gate) return cleanupAttempts.ToArray(); } // Serialize this lifetime stage against active Count and Dispose.
    private void CleanupPreservingPrimary(Exception primary) // Maintain primary-exception preservation without granting a prepared/native admission.
    { // Enter primary-exception preservation scope 23.
        try { Dispose(); } // The normal production close flow runs before primary propagation.
        catch (Exception secondary) { RetainUnsafeRuntime(secondary); } // A release exception remains secondary to the original open failure.
        // Attaching diagnostics is secondary too; neither cleanup nor diagnostic allocation may replace the primary.
        try // Bound the following operation inside primary-exception preservation.
        { // Enter primary-exception preservation scope 24.
            var failures = CopyCleanupFailures(); // Copy secondary diagnostics after every independent safe stage was attempted.
            if (failures.Length != 0) primary.Data["ConfiguredQwenCleanupFailures"] = failures; // Attach release failures without replacing the original exception object/token.
            if (criticalReleaseFailure is not null) primary.Data["ConfiguredQwenCleanupCriticalFailure"] = criticalReleaseFailure; // Attach the separately retained critical cleanup failure when present.
        } // Finish primary-exception preservation scope without granting new tokenizer or call authority.
        catch (Exception secondary) { criticalReleaseFailure ??= secondary; } // Store a secondary failure without allocating a replacement primary exception.
    } // Finish primary-exception preservation scope without granting new tokenizer or call authority.
    private string WithCleanupDiagnostics(string primary) // Maintain primary refusal plus secondary diagnostic text without granting a prepared/native admission.
        => cleanupFailures.Count == 0 && criticalReleaseFailure is null ? primary : // Preserve the primary refusal text when cleanup had no secondary failure.
            primary + "; secondary cleanup: " + string.Join(", ", cleanupFailures.Select(f => f.Stage + ":" + f.Error.GetType().Name)) + // Maintain primary refusal plus secondary diagnostic text without granting a prepared/native admission.
            (criticalReleaseFailure is null ? "" : "; critical cleanup:" + criticalReleaseFailure.GetType().Name); // Maintain primary refusal plus secondary diagnostic text without granting a prepared/native admission.

    // This seam replaces only release operations in the production SafeHandle/cleanup pipeline; it cannot load or count a vocabulary.
    internal static ConfiguredQwenVocabularyLease OpenSyntheticCleanupForTesting(ContinuousLearningTp1DpoConfiguration configuration, Action<string> release, bool allocatedModel = true) // Exercise either partial-open branch with explicitly Synthetic release operations and no resource admission.
    { // Enter Synthetic-only cleanup owner scope 25.
        ArgumentNullException.ThrowIfNull(release); // A missing test release function must not turn a synthetic pointer into a native resource path.
        var lease = new ConfiguredQwenVocabularyLease(configuration); // Construct only managed Synthetic lifetime state, with no installed resources.
        if (Interlocked.CompareExchange(ref nativeOwnerHeld, 1, 0) != 0) throw new InvalidOperationException("Native runtime reservation is held."); // Use the same process reservation as the production lifetime owner.
        lease.ownsNativeRuntime = true; lease.syntheticRelease = release; lease.ResourceFingerprint = "SyntheticCleanupOnly"; // Mark this owner Synthetic-only before installing the non-native test handle.
        lease.model = new NativeModelHandle(lease); lease.model.Initialize(allocatedModel ? new IntPtr(1) : IntPtr.Zero); // A zero inert pointer exercises cleanup when SafeHandle has no native allocation to release.
        return lease; // This test owner cannot issue a NativeConfigured tokenizer readback.
    } // Finish Synthetic-only cleanup owner scope without granting new tokenizer or call authority.
    internal void FailSyntheticOpenForTesting(Exception primary) // Maintain Synthetic partial-open primary failure without granting a prepared/native admission.
    { // Enter Synthetic partial-open primary failure scope 26.
        if (syntheticRelease is null) throw new InvalidOperationException("Synthetic cleanup owner required."); // A production vocabulary owner cannot use the Synthetic failure seam.
        CleanupPreservingPrimary(primary); // Use the same cleanup/primary preservation as actual Open catch paths.
        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(primary).Throw(); // Rethrow the identical primary exception, preserving its token and stack.
    } // Finish Synthetic partial-open primary failure scope without granting new tokenizer or call authority.
    internal SyntheticCustodyObservation AddSyntheticCustodyForTesting() // Give the GC control genuine finalizable managed custody through the production dependency owner, with no installed assets.
    { // The returned counters contain no strong reference to either the lease or its custody handle.
        lock (gate) // Bind the test dependency before the lease can be abandoned or closed.
        { // This seam is limited to the explicitly Synthetic, resource-free model owner.
            if (syntheticRelease is null || disposed || ResourceFingerprint != "SyntheticCleanupOnly" || modules.Count != 0 || held.Count != 0) // Installed resource owners cannot acquire test custody or alter their path authority.
                throw new InvalidOperationException("Active Synthetic cleanup owner required."); // Refuse mixing actual custody with the managed GC control.
            var observation = new SyntheticCustodyObservation(); var custody = new SyntheticCustodyHandle(observation); // Own a real managed pin using a finalizable SafeHandle, not a fake close counter.
            CustodyDependency? dependency = null; // Keep an acquired but unpublished borrow available to the failure path.
            try { dependency = new CustodyDependency(custody); syntheticCustody.Add(dependency); } // Publish one borrowed test custody object to the production cleanup owner.
            catch { if (dependency is null) custody.Dispose(); else dependency.Dispose(); throw; } // Constructor/list failure balances both ownership and any acquired borrow immediately.
            return observation; // These counters observe actual finalization/release without keeping the owner reachable.
        } // No file, configured DLL, vocabulary, inference or tokenizer was opened.
    } // The test resource cannot issue a NativeConfigured readback.
    internal void ResetRetainedSyntheticCleanupForTesting() // Retire only an inert Synthetic model and sealed managed test pins after assertions; this cannot recover actual uncertain native ownership.
    { // The test must observe the retained slot before retiring its Synthetic graph, which owns no installed resource.
        lock (gate) // Keep the test-only retirement serialized with the same lifetime pipeline.
        { // These predicates exclude configured DLL/path/vocab ownership and admit only the sealed managed-pin test custody type.
            if (syntheticRelease is null || ResourceFingerprint != "SyntheticCleanupOnly" || modules.Count != 0 || held.Count != 0 || syntheticCustody.Any(d => !d.IsManagedSynthetic) ||
                !cleanupCompleted || !disposed || model is null || !model.IsClosed || vocab != IntPtr.Zero ||
                !ownsNativeRuntime || !ReferenceEquals(quarantinedRuntime, this)) // A native or active owner cannot clear the unsafe-runtime reservation through this seam.
                throw new InvalidOperationException("Retained Synthetic cleanup owner required."); // Refuse misuse instead of releasing uncertain native ownership.
            model.RetireSyntheticPointer(); // This inert test pointer never represented a native allocation.
            foreach (var dependency in syntheticCustody) dependency.Dispose(); syntheticCustody.Clear(); // Retire only managed test pins after retained-lifetime assertions; never release installed resource custody here.
            Interlocked.CompareExchange(ref quarantinedRuntime, null, this); // Retire only this exact Synthetic retained graph.
            ownsNativeRuntime = false; Interlocked.Exchange(ref nativeOwnerHeld, 0); // The explicit test reset ends Synthetic ownership, not a successful native cleanup claim.
            cleanupAttempts.Add("synthetic-retention-reset"); // Keep test retirement distinct from production reservation release.
        } // No native release operation or resource write occurs in this test-only retirement.
    } // Actual uncertain native owners remain retained and unavailable to future opens.
    private sealed class CustodyDependency : IDisposable // A dependency borrow survives queued SafeHandle finalization; managed resurrection alone is insufficient.
    { // This same lifetime owner is used by actual HeldPath and the asset-free abandoned-owner control.
        private readonly SafeHandle handle; private int borrowed; // The reference count, rather than reachability, keeps underlying custody alive.
        internal bool IsManagedSynthetic => handle is SyntheticCustodyHandle; // Restrict explicit test retirement to the sealed managed-pin handle type.
        internal CustodyDependency(SafeHandle handle) // Acquire the dependent reference while the new custody handle is still strongly reachable.
        { // An unpublished acquisition must never leak its borrowed reference if construction fails.
            this.handle = handle; bool acquired = false; // Keep acquisition state independent of the public lease's disposed flag.
            try { handle.DangerousAddRef(ref acquired); borrowed = acquired ? 1 : 0; } // Critical finalization can drop ownership but cannot close this explicit dependent reference.
            catch { if (acquired) handle.DangerousRelease(); throw; } // Balance any completed acquisition before preserving the constructor's primary exception.
        } // SafeHandle now has one explicit reference held through model/callback lifetime.
        public void Dispose() // Called only after safe model/callback cleanup, or explicit retirement of a sealed managed Synthetic test pin.
        { // Unsafe native quarantine deliberately never enters this release method.
            if (Interlocked.Exchange(ref borrowed, 0) == 0) return; // Repeated or concurrent cleanup balances the dependency exactly once.
            try { handle.Dispose(); } finally { handle.DangerousRelease(); } // Drop ownership and the borrow even if a previously queued finalizer already dropped ownership.
        } // The final dependent reference may now execute the actual underlying release exactly once.
    } // This wrapper has no finalizer that could accidentally release unsafe dependent custody.
    internal sealed class SyntheticCustodyObservation // Counters observe real managed SafeHandle finalization/free and keep no resource or lease alive.
    { // The GC tests may keep this object strongly reachable without preventing custody abandonment.
        private int releases, finalizers; // Cross-thread observations are updated/read atomically by the finalizer and test threads.
        internal int ReleaseCount => Volatile.Read(ref releases); // Count successful underlying managed-pin free, not an attempted cleanup stage.
        internal int FinalizerCount => Volatile.Read(ref finalizers); // Prove that the queued critical-finalizer path actually ran in the authored runtime test.
        internal void Released() => Interlocked.Increment(ref releases); // Record release only after the underlying GCHandle was freed.
        internal void Finalized() => Interlocked.Increment(ref finalizers); // Derived finalization is followed by the SafeHandle base finalizer.
    } // This is Synthetic managed-runtime evidence, never configured tokenizer evidence.
    private sealed class SyntheticCustodyHandle : SafeHandle // A genuine finalizable managed resource tests the production dependency borrow without installed assets.
    { // Only this sealed type can be retired through the explicitly Synthetic reset guard.
        private readonly SyntheticCustodyObservation observation; // The observer holds counters only, so it does not root this SafeHandle.
        internal SyntheticCustodyHandle(SyntheticCustodyObservation observation) : base(IntPtr.Zero, ownsHandle: true) // Allocate one bounded managed pin owned by this SafeHandle.
        { this.observation = observation; SetHandle(GCHandle.ToIntPtr(GCHandle.Alloc(new byte[1], GCHandleType.Pinned))); } // The underlying resource is a real managed GC pin, not an installed model/file handle.
        public override bool IsInvalid => handle == IntPtr.Zero; // A released managed pin cannot be freed twice.
        ~SyntheticCustodyHandle() { observation.Finalized(); } // Record genuine derived critical finalization before the SafeHandle base drops ownership.
        protected override bool ReleaseHandle() // Critical cleanup never propagates a managed release exception.
        { // SafeHandle invokes this only when both ownership and the dependency borrow are gone.
            try { GCHandle.FromIntPtr(handle).Free(); handle = IntPtr.Zero; observation.Released(); return true; } // Exactly one successful release frees the actual managed pin.
            catch { return false; } // A critical finalizer must not throw; the test fails its release-count control if the pin was not released.
        } // No native asset, file, model, inference or tokenization operation is involved.
    } // The dependency owner is identical to actual HeldPath custody, while resource origin remains Synthetic.
    private sealed class NativeModelHandle : SafeHandle // Maintain critical model SafeHandle without granting a prepared/native admission.
    { // Enter critical model SafeHandle scope 27.
        private readonly ConfiguredQwenVocabularyLease owner; // The critical model handle roots callback delegates and the complete DLL graph.
        internal NativeModelHandle(ConfiguredQwenVocabularyLease owner) : base(IntPtr.Zero, ownsHandle: true) { this.owner = owner; } // The model pointer has one critical owner, before native allocation returns.
        public override bool IsInvalid => handle == IntPtr.Zero; // A zero native load result requires partial-open resource cleanup.
        internal void Initialize(IntPtr pointer) => SetHandle(pointer); // Transfer the load result into SafeHandle ownership before cancellation can escape.
        internal void RetireSyntheticPointer() => handle = IntPtr.Zero; // Called only after the closed inert Synthetic handle's retention has been verified by its owner.
        protected override bool ReleaseHandle() // Maintain nonthrowing critical model release without granting a prepared/native admission.
        { // Enter nonthrowing critical model release scope 28.
            try // Bound the following operation inside nonthrowing critical model release.
            { // Enter nonthrowing critical model release scope 29.
                var released = owner.ReleaseResources(handle); // Critical finalization uses the same ordered production cleanup pipeline.
                if (released) handle = IntPtr.Zero; // Invalidate the pointer only after successful model free.
                return released; // A failed free leaves uncertain native ownership recorded and rooted.
            } // Finish nonthrowing critical model release scope without granting new tokenizer or call authority.
            catch (Exception ex) // Capture this secondary release/diagnostic failure within nonthrowing critical model release.
            { // Enter nonthrowing critical model release scope 30.
                // A critical finalizer must not throw or unload an uncertain graph, even if recording an error failed.
                owner.RetainUnsafeRuntime(ex); return false; // The critical finalizer cannot throw or unload possibly live native code.
            } // Finish nonthrowing critical model release scope without granting new tokenizer or call authority.
        } // Finish nonthrowing critical model release scope without granting new tokenizer or call authority.
    } // Finish nonthrowing critical model release scope without granting new tokenizer or call authority.
    private static OperationResult<T> Fail<T>(string code, string? diagnostic = null) => OperationResult<T>.Fail(diagnostic ?? "The pinned configured vocabulary resource was not admitted.", code); // Carry explicit native failures without granting partial admission.
    private sealed class ResourceException(string code, string? detail = null) : IOException(detail ?? code) { internal string Code { get; } = code; } // Keep the failure class separate from the resource operation's exact diagnostic.
    private sealed class HeldPath : IDisposable // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        internal string Path { get; } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        internal string Sha { get; } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        internal long Size { get; } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        private readonly SafeFileHandle handle; // The owning SafeFileHandle may be queued for critical finalization alongside an abandoned lease.
        private readonly CustodyDependency dependency; // Hold an explicit SafeHandle reference until model free and callback detachment have made custody release safe.
        private readonly byte[] identity; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        private HeldPath(string path, string sha, long size, SafeFileHandle handle) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        { // Borrow before any metadata operation can fail or a lease can become abandoned.
            Path = path; Sha = sha; Size = size; this.handle = handle; dependency = new CustodyDependency(handle); // Queued SafeHandle finalization alone cannot drop the dependent custody reference.
            try { identity = Identity(handle); } catch { dependency.Dispose(); throw; } // Constructor failure balances the borrow immediately; no native model can depend on this unpublished path.
        } // The published HeldPath keeps its dependency borrow until safe cleanup, including through unsafe owner resurrection.
        internal static HeldPath Open(string path, string sha, long size) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            var handle = CreateFileW(path, 0x80000000, 1, IntPtr.Zero, 3, 0x00200000 | (size < 0 ? 0x02000000u : 0u), IntPtr.Zero); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            if (handle.IsInvalid) { handle.Dispose(); throw new ResourceException(Unavailable); } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            try { return new(path, sha, size, handle); } catch { handle.Dispose(); throw; } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        private static byte[] Identity(SafeFileHandle handle) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            var tags = new byte[8]; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            if (!GetFileInformationByHandleEx(handle, 9, tags, 8) || (BinaryPrimitives.ReadUInt32LittleEndian(tags) & 0x400) != 0) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                throw new ResourceException(Changed); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            var id = new byte[24]; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            if (!GetFileInformationByHandleEx(handle, 18, id, 24)) throw new ResourceException(Unsupported); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            return id; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        internal void Verify(CancellationToken token) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            token.ThrowIfCancellationRequested(); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            if (!identity.AsSpan().SequenceEqual(Identity(handle))) throw new ResourceException(Changed); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            using var reopened = Open(Path, Sha, Size); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            if (!identity.AsSpan().SequenceEqual(reopened.identity)) throw new ResourceException(Changed); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            if (Size < 0) return; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            using var stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            if (stream.Length != Size) throw new ResourceException(Changed); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            var bytes = new byte[1024 * 1024]; int count; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            while ((count = stream.Read(bytes)) > 0) { token.ThrowIfCancellationRequested(); hash.AppendData(bytes.AsSpan(0, count)); } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
            if (Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant() != Sha || !identity.AsSpan().SequenceEqual(Identity(handle))) // Retain the pinned vocabulary resource and refuse incomplete native evidence.
                throw new ResourceException(Changed); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        public void Dispose() => dependency.Dispose(); // Safe cleanup closes ownership and drops the dependent reference exactly once; unsafe cleanup never calls this method.
    } // Retain the pinned vocabulary resource and refuse incomplete native evidence.

    // This ABI is bound to the pinned b10434 Windows x64 header and observed default-parameter layout.
    private static bool HasSupportedLayout() // Validate every field offset, including the six native C bool bytes.
        => Marshal.SizeOf<ModelParams>() == 72 && new (string Field, int Offset)[] // Do not infer matching ABI merely from total structure size.
        { // Offsets come from the pinned b10434 header and the installed default-params witness.
            (nameof(ModelParams.Devices), 0), (nameof(ModelParams.TensorBufferOverrides), 8), (nameof(ModelParams.NGpuLayers), 16), // First native pointers and int32 GPU selector.
            (nameof(ModelParams.SplitMode), 20), (nameof(ModelParams.LoadMode), 24), (nameof(ModelParams.MainGpu), 28), // Preserve the remaining native int32 values.
            (nameof(ModelParams.TensorSplit), 32), (nameof(ModelParams.ProgressCallback), 40), (nameof(ModelParams.ProgressUserData), 48), // Retain exact callback and data pointer layout.
            (nameof(ModelParams.KvOverrides), 56), (nameof(ModelParams.VocabOnly), 64), (nameof(ModelParams.CheckTensors), 65), // Vocabulary-only is a one-byte C bool after the last pointer.
            (nameof(ModelParams.ExtraBufferTypes), 66), (nameof(ModelParams.NoHost), 67), (nameof(ModelParams.NoAlloc), 68), (nameof(ModelParams.LoadMtp), 69) // Final C bool fields and native padding.
        }.All(field => Marshal.OffsetOf<ModelParams>(field.Field).ToInt64() == field.Offset); // A mismatching managed layout cannot reach native resource allocation.
    [StructLayout(LayoutKind.Sequential)] // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private struct ModelParams // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    { // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        internal IntPtr Devices, TensorBufferOverrides; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        internal int NGpuLayers, SplitMode, LoadMode, MainGpu; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        internal IntPtr TensorSplit, ProgressCallback, ProgressUserData, KvOverrides; // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        internal byte VocabOnly; // Native C bool at offset64; a blittable byte avoids nonblittable struct-return marshalling.
        internal byte CheckTensors; // Native C bool at offset65; preserve the documented by-value layout.
        internal byte ExtraBufferTypes; // Native C bool at offset66; the observed default byte is one.
        internal byte NoHost; // Native C bool at offset67; this resource-only load keeps the observed default zero.
        internal byte NoAlloc; // Native C bool at offset68; vocabulary allocation remains the genuine native resource operation.
        internal byte LoadMtp; // Native C bool at offset69; the two trailing ABI padding bytes remain implicit.
    } // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    private delegate bool ProgressCallback(float progress, IntPtr userData); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void LogCallback(int level, IntPtr text, IntPtr userData); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void SetLog(LogCallback? callback, IntPtr userData); // Native nullptr restores the default logger before callback ownership is released.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate ModelParams DefaultParams(); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate IntPtr LoadModel([MarshalAs(UnmanagedType.LPUTF8Str)] string path, ModelParams parameters); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void FreeModel(IntPtr model); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate IntPtr GetVocab(IntPtr model); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int VocabInt(IntPtr vocab); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] private delegate bool VocabBool(IntPtr vocab); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int Tokenize(IntPtr vocab, IntPtr text, int byteLength, IntPtr tokens, int capacity, // Retain the pinned vocabulary resource and refuse incomplete native evidence.
        [MarshalAs(UnmanagedType.I1)] bool addSpecial, [MarshalAs(UnmanagedType.I1)] bool parseSpecial); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr LoadLibraryExW(string path, IntPtr file, uint flags); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    [DllImport("kernel32", SetLastError = true)] private static extern bool FreeLibrary(IntPtr module); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    [DllImport("kernel32", CharSet = CharSet.Unicode)] private static extern IntPtr GetModuleHandleW(string name); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    [DllImport("kernel32", CharSet = CharSet.Unicode)] private static extern uint GetModuleFileNameW(IntPtr module, StringBuilder path, int size); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true)] private static extern IntPtr GetProcAddress(IntPtr module, string name); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)] private static extern SafeFileHandle CreateFileW(string path, uint access, uint share, IntPtr security, uint creation, uint flags, IntPtr template); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
    [DllImport("kernel32", SetLastError = true)] private static extern bool GetFileInformationByHandleEx(SafeFileHandle handle, int kind, [Out] byte[] data, uint size); // Retain the pinned vocabulary resource and refuse incomplete native evidence.
} // Retain the pinned vocabulary resource and refuse incomplete native evidence.
