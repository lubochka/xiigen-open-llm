// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure; // Use the foundational canonical result contract for the new declared-topology API.
using AgentInfrastructure.Logical; // Reuse nonfatal exception classification while preserving explicit cancellation.
using TensorCore.Core;
using TensorCore.Layers;
using TransformerModel.Caching;
using TransformerModel.Config;
using TransformerModel.Layers;

[assembly: InternalsVisibleTo("LoRA")]
[assembly: InternalsVisibleTo("LlmFromScratch.Tests")]

namespace TransformerModel;

public sealed record LlmCheckpointParameterEntry(string Key, Parameter Parameter);

internal enum LlmForwardPhase
{
    AfterEmbedding,
    AfterBlock,
    BeforeLanguageModelHead
}

internal sealed class AttentionProjectionAdapterSlot : Module
{
    public string Key { get; }
    public Linear BaseProjection { get; }
    public IAttentionProjectionAdapter? Adapter { get; private set; }

    internal AttentionProjectionAdapterSlot(string key, Linear baseProjection)
    {
        Key = key;
        BaseProjection = baseProjection;
    }

    internal void PopulatePrevalidated(
        LlmModel.AdapterMutationLease lease,
        IAttentionProjectionAdapter adapter)
    {
        lease.ValidateActive();
        if (!ReferenceEquals(adapter.BaseProjection, BaseProjection))
            throw new InvalidOperationException("LORA_REGISTRATION_KEY_CONFLICT");
        if (Adapter is not null && !ReferenceEquals(Adapter, adapter))
            throw new InvalidOperationException("LORA_REGISTRATION_KEY_CONFLICT");
        if (ReferenceEquals(Adapter, adapter))
            return;

        Adapter = adapter;
        RegisterModule("adapter", adapter.RegistrationModule);
    }
}

public class LlmModel : Module
{
    private const int AdapterCheckpointMagic = unchecked((int)0xC14F5241);
    private const int VersionOneCheckpoint = 1;
    private const int CurrentCheckpointVersion = 2;
    private const int CheckpointFingerprintBytes = 32;
    private const int MaximumCheckpointKeyBytes = 256;
    private const int MaximumCheckpointEntries = 1_000_000;
    private const int MaximumStaticAdapterCheckpointEntries = 16_384;
    private const long MaximumStaticAdapterRecordPayloadBytes = 64L * 1024 * 1024;
    private const long MaximumStaticAdapterAggregatePayloadBytes = 1024L * 1024 * 1024;
    private const string CheckpointParameterCountMismatchCode =
        "LLM_CHECKPOINT_PARAMETER_COUNT_MISMATCH";
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private readonly Embedding _tokenEmbedding;
    private readonly List<TransformerBlock> _blocks;
    private readonly RmsNorm _finalNorm;
    private readonly Linear _lmHead;
    private readonly ModelConfig _cfg;
    private readonly Parameter[] _baseParameters;
    private readonly IReadOnlyList<Parameter> _baseParameterView;
    private readonly LlmCheckpointParameterEntry[] _baseCheckpointEntries;
    private readonly byte[] _configFingerprint;
    private readonly Dictionary<string, AttentionProjectionAdapterSlot> _adapterSlots =
        new(StringComparer.Ordinal);
    private readonly List<AttentionProjectionAdapterSlot> _orderedAdapterSlots = [];
    private readonly object _adapterMutationSync = new();

    public ModelConfig Config => _cfg;
    public IReadOnlyList<TransformerBlock> Blocks => _blocks;
    public IReadOnlyList<Parameter> BaseParameters => _baseParameterView;

    public static bool IsCheckpointParameterCountMismatch(Exception? exception)
        => exception is CheckpointValidationException validationException
           && StringComparer.Ordinal.Equals(
               validationException.Code,
               CheckpointParameterCountMismatchCode);

    public LlmModel(ModelConfig cfg)
    {
        _cfg = cfg;
        _tokenEmbedding = new Embedding(cfg.VocabSize, cfg.HiddenDim);
        _blocks = Enumerable.Range(0, cfg.NumLayers)
            .Select(_ => new TransformerBlock(cfg))
            .ToList();
        _finalNorm = new RmsNorm(cfg.HiddenDim, cfg.RmsNormEps);
        _lmHead = new Linear(cfg.HiddenDim, cfg.VocabSize, useBias: false);

        RegisterModule("token_embedding", _tokenEmbedding);
        for (var index = 0; index < _blocks.Count; index++)
            RegisterModule($"block_{index}", _blocks[index]);
        RegisterModule("final_norm", _finalNorm);
        RegisterModule("lm_head", _lmHead);

        _baseCheckpointEntries = NamedParameters()
            .Select(entry => new LlmCheckpointParameterEntry(
                $"base.{entry.Name}",
                entry.Parameter))
            .ToArray();
        _baseParameters = _baseCheckpointEntries
            .Select(static entry => entry.Parameter)
            .ToArray();
        _baseParameterView = Array.AsReadOnly(_baseParameters);
        _configFingerprint = ComputeConfigFingerprint(cfg);

        for (var blockIndex = 0; blockIndex < _blocks.Count; blockIndex++)
        {
            foreach (var projection in CausalSelfAttention.CanonicalProjectionNames)
            {
                var binding = _blocks[blockIndex].Attention.GetProjectionBinding(projection);
                var key = BuildAdapterSlotKey(blockIndex, projection);
                var slot = new AttentionProjectionAdapterSlot(key, binding.BaseProjection);
                _adapterSlots.Add(key, slot);
                _orderedAdapterSlots.Add(slot);
                RegisterModule(key, slot);
            }
        }
    }

    public Tensor Forward(Tensor tokenIds) => Forward(tokenIds, training: false);

    public Tensor Forward(Tensor tokenIds, bool training)
        => Forward(tokenIds, training, CancellationToken.None);

    public Tensor Forward(
        Tensor tokenIds,
        bool training,
        CancellationToken cancellationToken)
        => Forward(tokenIds, training, cancellationToken, phaseProbe: null);

    public Tensor ForwardWithCache(Tensor tokenIds, ITransformerKvCache cache)
        => ForwardWithCache(tokenIds, cache, CancellationToken.None);

    public Tensor ForwardWithCache(
        Tensor tokenIds,
        ITransformerKvCache cache,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(tokenIds);
        ArgumentNullException.ThrowIfNull(cache);

        if (tokenIds.Shape.Rank != 2)
            throw new ArgumentException("LLM_KV_TOKEN_IDS_RANK_INVALID");
        if (cache.LayerCount != _blocks.Count)
            throw new InvalidOperationException("LLM_KV_CACHE_LAYER_COUNT_MISMATCH");

        int pastKeyLength = cache.GetUniformSequenceLength();
        int chunkLength = tokenIds.Shape.Dimensions[1];
        long totalSequenceLength = checked((long)pastKeyLength + chunkLength);
        if (totalSequenceLength > _cfg.ContextLen)
            throw new InvalidOperationException("LLM_KV_CONTEXT_LENGTH_EXCEEDED");

        try
        {
            var x = _tokenEmbedding.Forward(tokenIds, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            for (var layerIndex = 0; layerIndex < _blocks.Count; layerIndex++)
            {
                x = _blocks[layerIndex].ForwardWithCache(
                    x,
                    cache,
                    layerIndex,
                    pastKeyLength,
                    cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }

            x = _finalNorm.Forward(x, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            x = _lmHead.Forward(x, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            if (cache.GetUniformSequenceLength() != totalSequenceLength)
                throw new InvalidOperationException("LLM_KV_CACHE_FINAL_LENGTH_MISMATCH");

            return x;
        }
        catch (Exception operationException)
        {
            try
            {
                cache.Clear();
            }
            catch (Exception resetException)
            {
                throw new AggregateException(
                    "LLM_KV_CACHE_RESET_FAILED",
                    operationException,
                    resetException);
            }

            throw;
        }
    }

    internal Tensor Forward(
        Tensor tokenIds,
        bool training,
        CancellationToken cancellationToken,
        Action<LlmForwardPhase>? phaseProbe)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var x = _tokenEmbedding.Forward(tokenIds, cancellationToken);
        phaseProbe?.Invoke(LlmForwardPhase.AfterEmbedding);
        cancellationToken.ThrowIfCancellationRequested();
        foreach (var block in _blocks)
        {
            x = block.Forward(x, training, cancellationToken);
            phaseProbe?.Invoke(LlmForwardPhase.AfterBlock);
            cancellationToken.ThrowIfCancellationRequested();
        }
        x = _finalNorm.Forward(x, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        phaseProbe?.Invoke(LlmForwardPhase.BeforeLanguageModelHead);
        cancellationToken.ThrowIfCancellationRequested();
        x = _lmHead.Forward(x, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return x;
    }

    public Tensor ForwardHidden(Tensor tokenIds) => ForwardHidden(tokenIds, training: false);

    public Tensor ForwardHidden(Tensor tokenIds, bool training)
        => ForwardHidden(tokenIds, training, CancellationToken.None);

    public Tensor ForwardHidden(
        Tensor tokenIds,
        bool training,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var x = _tokenEmbedding.Forward(tokenIds, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        foreach (var block in _blocks)
        {
            x = block.Forward(x, training, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }
        x = _finalNorm.Forward(x, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return x;
    }

    public IReadOnlyList<LlmCheckpointParameterEntry> GetCheckpointParameterEntries()
    {
        lock (_adapterMutationSync)
            return BuildVersionOneCheckpointEntries().ToArray();
    }

    public IAttentionProjectionAdapter? GetAttachedProjectionAdapter(
        int blockIndex,
        string projection)
    {
        lock (_adapterMutationSync)
            return InspectProjection(blockIndex, projection).SlotAdapter;
    }

    /// <summary>
    /// Captures only the attached adapter streams in canonical slot order. The
    /// caller supplies the identity hashes because the transformer layer must
    /// remain independent from LoRA and DPO.
    /// </summary>
    public LlmModelRandomStateSnapshot CaptureRandomState(
        string topologySha256,
        string loraConfigurationSha256,
        int randomStateStep)
    {
        ValidateRandomStateIdentity(
            topologySha256,
            loraConfigurationSha256,
            randomStateStep);
        lock (_adapterMutationSync)
        {
            var entries = new List<LlmModelRandomStateEntry>();
            foreach (var slot in _orderedAdapterSlots)
            {
                if (slot.Adapter is null)
                    continue;
                var state = slot.Adapter.CaptureRandomState();
                entries.Add(new LlmModelRandomStateEntry(
                    slot.Key,
                    state.StreamDomain,
                    state.InitializationSeed,
                    state.DropoutSeed,
                    state.ConsumedCursor));
            }

            return new LlmModelRandomStateSnapshot(
                LlmModelRandomStateSnapshot.CurrentFormatVersion,
                LlmModelRandomStateSnapshot.CurrentAlgorithm,
                entries,
                topologySha256,
                loraConfigurationSha256,
                randomStateStep);
        }
    }

    /// <summary>
    /// Restores all adapter cursors only after the full one-to-one mapping has
    /// been validated. No cursor is changed when the identity is incomplete.
    /// </summary>
    public void RestoreRandomState(
        LlmModelRandomStateSnapshot snapshot,
        LlmModelRandomStateIdentity expectedIdentity)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(expectedIdentity);
        if (snapshot.FormatVersion != LlmModelRandomStateSnapshot.CurrentFormatVersion ||
            !string.Equals(
                snapshot.Algorithm,
                LlmModelRandomStateSnapshot.CurrentAlgorithm,
                StringComparison.Ordinal) ||
            snapshot.RandomStateStep < 0 ||
            !IsSha256(snapshot.TopologySha256) ||
            !IsSha256(snapshot.LoraConfigurationSha256) ||
            !IsSha256(expectedIdentity.TopologySha256) ||
            !IsSha256(expectedIdentity.LoraConfigurationSha256))
        {
            throw new InvalidOperationException("DPO_RANDOM_STATE_FORMAT_INVALID");
        }
        if (!StringComparer.Ordinal.Equals(
                snapshot.TopologySha256,
                expectedIdentity.TopologySha256) ||
            !StringComparer.Ordinal.Equals(
                snapshot.LoraConfigurationSha256,
                expectedIdentity.LoraConfigurationSha256))
        {
            throw new InvalidOperationException(
                "DPO_RANDOM_STATE_IDENTITY_MISMATCH");
        }

        lock (_adapterMutationSync)
        {
            var expectedSlots = _orderedAdapterSlots
                .Where(static slot => slot.Adapter is not null)
                .ToArray();
            if (snapshot.Entries.Count != expectedSlots.Length)
                throw new InvalidOperationException("DPO_RANDOM_STATE_TOPOLOGY_MISMATCH");

            var byKey = snapshot.Entries.ToDictionary(
                static entry => entry.AdapterKey,
                StringComparer.Ordinal);
            if (byKey.Count != snapshot.Entries.Count)
                throw new InvalidOperationException("DPO_RANDOM_STATE_DUPLICATE_SLOT");

            var pending = new List<(IAttentionProjectionAdapter Adapter,
                TransformerAdapterRandomState State)>(expectedSlots.Length);
            foreach (var slot in expectedSlots)
            {
                if (!byKey.TryGetValue(slot.Key, out var entry) ||
                    slot.Adapter is null)
                {
                    throw new InvalidOperationException(
                        "DPO_RANDOM_STATE_SLOT_MISMATCH");
                }
                var state = new TransformerAdapterRandomState(
                    entry.StreamDomain,
                    entry.InitializationSeed,
                    entry.DropoutSeed,
                    entry.ConsumedCursor);
                ValidateAdapterRandomState(slot.Adapter, state);
                pending.Add((slot.Adapter, state));
            }

            foreach (var item in pending)
                item.Adapter.RestoreRandomState(item.State);
        }
    }

    private static void ValidateAdapterRandomState(
        IAttentionProjectionAdapter adapter,
        TransformerAdapterRandomState expected)
    {
        var actual = adapter.CaptureRandomState();
        if (!StringComparer.Ordinal.Equals(
                actual.StreamDomain,
                expected.StreamDomain))
        {
            throw new InvalidOperationException("DPO_RANDOM_STATE_DOMAIN_MISMATCH");
        }
        if (actual.InitializationSeed != expected.InitializationSeed ||
            actual.DropoutSeed != expected.DropoutSeed)
        {
            throw new InvalidOperationException("DPO_RANDOM_STATE_SEED_MISMATCH");
        }
    }

    private static void ValidateRandomStateIdentity(
        string topologySha256,
        string loraConfigurationSha256,
        int randomStateStep)
    {
        if (randomStateStep < 0 ||
            !IsSha256(topologySha256) ||
            !IsSha256(loraConfigurationSha256))
        {
            throw new InvalidOperationException("DPO_RANDOM_STATE_IDENTITY_INVALID");
        }
    }

    private static bool IsSha256(string? value)
        => value is not null && value.Length == 64 &&
           value.All(static c => c is >= '0' and <= '9' or >= 'a' and <= 'f');

    internal T ExecuteAdapterMutation<T>(Func<AdapterMutationLease, T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        lock (_adapterMutationSync)
        {
            var lease = new AdapterMutationLease(this);
            try
            {
                return action(lease);
            }
            finally
            {
                lease.Invalidate();
            }
        }
    }

    internal sealed class AdapterMutationLease
    {
        private readonly LlmModel _owner;
        private readonly int _threadId;
        private bool _active = true;

        internal AdapterMutationLease(LlmModel owner)
        {
            _owner = owner;
            _threadId = Environment.CurrentManagedThreadId;
        }

        internal IReadOnlyList<Parameter> BaseParameters
        {
            get
            {
                ValidateActive();
                return _owner._baseParameterView;
            }
        }

        internal AdapterAttachmentState InspectProjection(int blockIndex, string projection)
        {
            ValidateActive();
            return _owner.InspectProjection(blockIndex, projection);
        }

        internal void PopulateAdapterPair(
            int blockIndex,
            string projection,
            IAttentionProjectionAdapter adapter)
        {
            ValidateActive();
            _owner.PopulateAdapterPairPrevalidated(this, blockIndex, projection, adapter);
        }

        internal void ValidateActive()
        {
            if (!_active
                || Environment.CurrentManagedThreadId != _threadId
                || !Monitor.IsEntered(_owner._adapterMutationSync))
            {
                throw new InvalidOperationException("LORA_MUTATION_LEASE_INVALID");
            }
        }

        internal void Invalidate() => _active = false;
    }

    internal sealed record AdapterAttachmentState(
        string SlotKey,
        Linear BaseProjection,
        IAttentionProjectionAdapter? AttentionAdapter,
        IAttentionProjectionAdapter? SlotAdapter);

    private AdapterAttachmentState InspectProjection(int blockIndex, string projection)
    {
        if (blockIndex < 0 || blockIndex >= _blocks.Count)
            throw new InvalidOperationException("LORA_REGISTRATION_KEY_CONFLICT");

        var key = BuildAdapterSlotKey(blockIndex, projection);
        if (!_adapterSlots.TryGetValue(key, out var slot))
            throw new InvalidOperationException("LORA_REGISTRATION_KEY_CONFLICT");

        var binding = _blocks[blockIndex].Attention.GetProjectionBinding(projection);
        if (!ReferenceEquals(binding.BaseProjection, slot.BaseProjection))
            throw new InvalidOperationException("LORA_REGISTRATION_KEY_CONFLICT");

        return new AdapterAttachmentState(
            key,
            binding.BaseProjection,
            binding.Adapter,
            slot.Adapter);
    }

    private void PopulateAdapterPairPrevalidated(
        AdapterMutationLease lease,
        int blockIndex,
        string projection,
        IAttentionProjectionAdapter adapter)
    {
        ValidateLease(lease);
        var state = InspectProjection(blockIndex, projection);
        if (!ReferenceEquals(state.BaseProjection, adapter.BaseProjection))
            throw new InvalidOperationException("LORA_REGISTRATION_KEY_CONFLICT");
        if (state.AttentionAdapter is not null
            && !ReferenceEquals(state.AttentionAdapter, adapter))
        {
            throw new InvalidOperationException("LORA_PROJECTION_ADAPTER_CONFLICT");
        }
        if (state.SlotAdapter is not null && !ReferenceEquals(state.SlotAdapter, adapter))
            throw new InvalidOperationException("LORA_REGISTRATION_KEY_CONFLICT");

        var slot = _adapterSlots[state.SlotKey];
        slot.PopulatePrevalidated(lease, adapter);
        _blocks[blockIndex].Attention.AttachProjectionAdapterPrevalidated(
            lease,
            projection,
            adapter);
    }

    private void ValidateLease(AdapterMutationLease lease)
    {
        if (lease is null)
            throw new InvalidOperationException("LORA_MUTATION_LEASE_INVALID");
        lease.ValidateActive();
    }

    public void SaveCheckpoint(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        lock (_adapterMutationSync)
        {
            string? temporaryPath = null;
            CheckpointValidationException? primaryFailure = null;

            try
            {
                var finalPath = Path.GetFullPath(path);
                var directory = Path.GetDirectoryName(finalPath)
                    ?? throw CheckpointFailure(
                        "LLM_CHECKPOINT_INCOMPLETE",
                        "Checkpoint directory cannot be resolved.");
                temporaryPath = Path.Combine(
                    directory,
                    $"{Path.GetFileName(finalPath)}.{Guid.NewGuid():N}.tmp");
                using (var stream = new FileStream(
                           temporaryPath,
                           FileMode.CreateNew,
                           FileAccess.Write,
                           FileShare.None))
                {
                    WriteCheckpointToStream(stream);
                    stream.Flush(flushToDisk: true);
                }

                File.Move(temporaryPath, finalPath, overwrite: true);
                if (!File.Exists(finalPath) || new FileInfo(finalPath).Length <= 0)
                {
                    throw CheckpointFailure(
                        "LLM_CHECKPOINT_INCOMPLETE",
                        "Checkpoint was not completed.");
                }
            }
            catch (CheckpointValidationException exception)
            {
                primaryFailure = exception;
                throw;
            }
            catch (Exception exception) when (
                exception is IOException
                    or UnauthorizedAccessException
                    or ArgumentException
                    or NotSupportedException)
            {
                primaryFailure = CheckpointFailure(
                    "LLM_CHECKPOINT_INCOMPLETE",
                    "Checkpoint could not be completed.",
                    exception);
                throw primaryFailure;
            }
            finally
            {
                try
                {
                    if (temporaryPath is not null && File.Exists(temporaryPath))
                        File.Delete(temporaryPath);
                }
                catch (Exception exception) when (
                    exception is IOException
                        or UnauthorizedAccessException)
                {
                    if (primaryFailure is not null)
                    {
                        primaryFailure.Data[
                            "CheckpointTemporaryCleanupException"] = exception;
                    }
                    else
                    {
                        throw CheckpointFailure(
                            "LLM_CHECKPOINT_INCOMPLETE",
                            "Checkpoint temporary artifact could not be cleaned up.",
                            exception);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Writes the canonical full checkpoint bytes directly to a caller-owned stream.
    /// The stream remains open so callers that created and retained the destination
    /// handle can flush and validate that same file identity before releasing it.
    /// </summary>
    public void SaveCheckpointToStream(Stream destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        lock (_adapterMutationSync)
        {
            try
            {
                var canWrite = destination.CanWrite;
                var canSeek = destination.CanSeek;
                var position = destination.Position;
                var length = destination.Length;
                if (!canWrite || !canSeek || position != 0 || length != 0)
                {
                    throw CheckpointFailure(
                        "LLM_CHECKPOINT_INCOMPLETE",
                        "Checkpoint destination must be writable, seekable, and empty at position zero.");
                }
                WriteCheckpointToStream(destination);
            }
            catch (CheckpointValidationException)
            {
                throw;
            }
            catch (Exception exception) when (
                exception is IOException
                    or UnauthorizedAccessException
                    or ArgumentException
                    or NotSupportedException
                    or ObjectDisposedException)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_INCOMPLETE",
                    "Checkpoint could not be completed.",
                    exception);
            }
        }
    }

    private void WriteCheckpointToStream(Stream destination)
    {
        var entries = BuildCheckpointEntries();
        using var writer = new BinaryWriter(destination, Encoding.UTF8, leaveOpen: true);
        WriteKeyedCheckpoint(
            writer,
            CheckpointArtifactKind.Full,
            _configFingerprint,
            entries);
        writer.Flush();
    }

    public void SaveAdapterCheckpoint(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        lock (_adapterMutationSync)
        {
            var entries = BuildAdapterCheckpointEntries();
            string? temporaryPath = null;
            CheckpointValidationException? primaryFailure = null;

            try
            {
                var finalPath = Path.GetFullPath(path);
                var directory = Path.GetDirectoryName(finalPath)
                    ?? throw CheckpointFailure(
                        "LLM_ADAPTER_CHECKPOINT_INCOMPLETE",
                        "Adapter checkpoint directory cannot be resolved.");
                temporaryPath = Path.Combine(
                    directory,
                    $"{Path.GetFileName(finalPath)}.{Guid.NewGuid():N}.tmp");
                using (var stream = new FileStream(
                           temporaryPath,
                           FileMode.CreateNew,
                           FileAccess.Write,
                           FileShare.None))
                {
                    WriteAdapterCheckpoint(stream, entries);
                    stream.Flush(flushToDisk: true);
                }

                File.Move(temporaryPath, finalPath, overwrite: true);
                if (!File.Exists(finalPath) || new FileInfo(finalPath).Length <= 0)
                {
                    throw CheckpointFailure(
                        "LLM_ADAPTER_CHECKPOINT_INCOMPLETE",
                        "Adapter checkpoint was not completed.");
                }
            }
            catch (CheckpointValidationException exception)
            {
                primaryFailure = exception;
                throw;
            }
            catch (Exception exception) when (
                exception is IOException
                    or UnauthorizedAccessException
                    or ArgumentException
                    or NotSupportedException)
            {
                primaryFailure = CheckpointFailure(
                    "LLM_ADAPTER_CHECKPOINT_INCOMPLETE",
                    "Adapter checkpoint could not be completed.",
                    exception);
                throw primaryFailure;
            }
            finally
            {
                try
                {
                    if (temporaryPath is not null && File.Exists(temporaryPath))
                        File.Delete(temporaryPath);
                }
                catch (Exception exception) when (
                    exception is IOException
                        or UnauthorizedAccessException)
                {
                    if (primaryFailure is not null)
                    {
                        primaryFailure.Data[
                            "AdapterCheckpointTemporaryCleanupException"] = exception;
                    }
                    else
                    {
                        throw CheckpointFailure(
                            "LLM_ADAPTER_CHECKPOINT_INCOMPLETE",
                            "Adapter checkpoint temporary artifact could not be cleaned up.",
                            exception);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Serializes the current adapter-only v2 artifact to a caller-owned
    /// writable, seekable stream. The stream is left open; the caller retains
    /// its lifetime, position, and durable-flush policy.
    /// </summary>
    public void SaveAdapterCheckpoint(Stream callerOwnedStream)
    {
        ArgumentNullException.ThrowIfNull(callerOwnedStream);
        if (!callerOwnedStream.CanWrite || !callerOwnedStream.CanSeek)
        {
            throw new ArgumentException(
                "Adapter checkpoint stream must be writable and seekable.",
                nameof(callerOwnedStream));
        }

        lock (_adapterMutationSync)
        {
            WriteAdapterCheckpoint(
                callerOwnedStream,
                BuildAdapterCheckpointEntries());
        }
    }

    /// <summary>
    /// Validates the generic structure and payload of an adapter-only checkpoint
    /// without constructing or mutating a model.
    /// </summary>
    public static void ValidateAdapterCheckpointArtifact(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        ValidateAdapterCheckpointArtifact(stream);
    }

    /// <summary>
    /// Validates an adapter-only checkpoint from the caller-owned readable,
    /// seekable stream. The stream remains open and is consumed to exact EOF.
    /// </summary>
    public static void ValidateAdapterCheckpointArtifact(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
        {
            throw new ArgumentException(
                "Adapter checkpoint stream must be readable and seekable.",
                nameof(stream));
        }

        try
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
            if (reader.ReadInt32() != AdapterCheckpointMagic)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_FORMAT_UNSUPPORTED",
                    "Adapter checkpoint must use the keyed checkpoint format.");
            }

            var version = ReadCheckpointVersion(reader);
            if (version == VersionOneCheckpoint)
            {
                ValidateKeyedRecordsStreaming(
                    reader,
                    stream,
                    requireAdapterOnlyKeys: true,
                    validateVersionTwoKeyForm: false);
            }
            else
            {
                ReadAndValidateVersionTwoHeader(
                    reader,
                    stream,
                    CheckpointArtifactKind.AdapterOnly,
                    expectedFingerprint: null);
                ValidateKeyedRecordsStreaming(
                    reader,
                    stream,
                    requireAdapterOnlyKeys: true,
                    validateVersionTwoKeyForm: true);
            }
        }
        catch (CheckpointValidationException)
        {
            throw;
        }
        catch (Exception exception)
            when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is EndOfStreamException
                or DecoderFallbackException
                or OverflowException
                or IOException
                or ArgumentException)
        {
            throw CheckpointFailure(
                "LLM_CHECKPOINT_MALFORMED",
                "Adapter checkpoint artifact is malformed.",
                exception);
        }
    }

    /// <summary>
    /// Validates a version-two full checkpoint against an explicit model
    /// configuration without constructing or mutating a model. This is the
    /// startup-safe boundary for large checkpoints: it verifies the artifact
    /// kind and configuration fingerprint while streaming the payload.
    /// </summary>
    public static void ValidateFullCheckpointArtifact(
        string path,
        ModelConfig expectedConfig,
        long maximumArtifactBytes = long.MaxValue,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        ValidateFullCheckpointArtifact(
            stream,
            expectedConfig,
            maximumArtifactBytes,
            cancellationToken);
    }

    /// <summary>
    /// Validates a version-two full checkpoint against an explicit model
    /// configuration without constructing or mutating a model. This is the
    /// startup-safe boundary for large checkpoints: it verifies the artifact
    /// kind and configuration fingerprint while streaming the payload.
    /// </summary>
    public static void ValidateFullCheckpointArtifact(
        Stream stream,
        ModelConfig expectedConfig,
        long maximumArtifactBytes = long.MaxValue,
        CancellationToken cancellationToken = default)
        => ValidateFullCheckpointArtifactCore( // Reuse exact key, shape, payload, fingerprint and EOF checks.
            stream, expectedConfig, null, maximumArtifactBytes, cancellationToken); // Keep retained reads inside the admitted exact artifact size.

    /// <summary>Validates the exact declared full base-and-adapter schema without constructing a model.</summary>
    public static OperationResult<bool> ValidateFullCheckpointArtifact( // The new declared-topology API returns contextual failures; legacy void overloads stay unchanged.
        Stream stream, // Validate the caller-owned retained snapshot without taking ownership.
        ModelConfig expectedConfig, // Use the explicit complete base shape for the same fingerprint and tensor checks.
        FullCheckpointLoraTopology adapterTopology, // Require an explicit exact slot schema rather than accepting extra records.
        long maximumArtifactBytes = long.MaxValue, // Preserve the caller's artifact budget before streaming payloads.
        CancellationToken cancellationToken = default) // Keep explicit cancellation separate from malformed-checkpoint results.
    { // This result admits only artifact format and shape, never model or training authority.
        if (adapterTopology is null) // A missing declaration must not select a permissive full-LoRA path.
            return OperationResult<bool>.Fail("Full checkpoint adapter topology is required.", "LLM_CHECKPOINT_CONFIG_MISMATCH"); // Refuse absent topology through the canonical API contract.
        try // Reuse the existing exception-based codec internally while making new public business outcomes typed.
        { // Legacy and declared callers still share the same exact parser.
            ValidateFullCheckpointArtifactCore( // Preserve count/key/shape/finite payload/fingerprint/exact EOF validation.
                stream, expectedConfig, adapterTopology, maximumArtifactBytes, cancellationToken); // Supply only the explicitly declared adapter schema.
            return OperationResult<bool>.Ok(true); // Success means the complete declared artifact was admitted, never a false comparison result.
        } // The stream stays open and consumed exactly as the shared codec specifies.
        catch (OperationCanceledException) // Preserve the existing explicit caller cancellation control flow.
        { // Cancellation cannot be represented as artifact corruption.
            throw; // Retain the original cancellation exception and token.
        } // No success or business refusal hides cancellation.
        catch (CheckpointValidationException exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Preserve exact codec families without hiding nested fatal/cancellation graphs.
        { // The parser already owns the stable count/key/shape and malformed-artifact codes.
            return OperationResult<bool>.Fail(exception.Message, exception.Code); // Return the original sanitized codec reason and exact code.
        } // New callers can handle exact schema refusals without catching business exceptions.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Normalize unreadable/invalid caller streams without masking process-fatal failures.
        { // No raw stream exception or private path crosses this public result boundary.
            return OperationResult<bool>.Fail("Full checkpoint artifact is malformed.", "LLM_CHECKPOINT_MALFORMED"); // Keep malformed input distinct from successful validation.
        } // The existing legacy void overloads retain their original exception behavior.
    } // A typed successful result still grants no execution or optimizer permission.

    private static void ValidateFullCheckpointArtifactCore( // Reuse exact key, shape, payload, fingerprint and EOF checks.
        Stream stream, // Read the full checkpoint from this caller-owned seekable stream without taking ownership of it.
        ModelConfig expectedConfig, // Use the admitted complete base configuration as the expected checkpoint identity and tensor shape.
        FullCheckpointLoraTopology? adapterTopology, // Keep strict base-only admission when null; otherwise admit only this explicitly declared adapter topology.
        long maximumArtifactBytes, // Keep retained reads inside the admitted exact artifact size.
        CancellationToken cancellationToken) // Preserve caller cancellation at the retained-read and import boundary.
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(expectedConfig);
        if (!stream.CanRead || !stream.CanSeek)
        {
            throw new ArgumentException(
                "Checkpoint stream must be readable and seekable.",
                nameof(stream));
        }
        if (maximumArtifactBytes <= 0 || stream.Length > maximumArtifactBytes)
        {
            throw CheckpointFailure(
                "LLM_CHECKPOINT_MALFORMED",
                "Full checkpoint exceeds the admitted artifact limit.");
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var expectedEntries = BuildFullCheckpointSchema(expectedConfig, adapterTopology); // Build the exact base and declared adapter entry schema before admitting any streamed tensor records.
            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
            if (reader.ReadInt32() != AdapterCheckpointMagic)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_FORMAT_UNSUPPORTED",
                    "Full checkpoint must use the version-two keyed checkpoint format.");
            }

            if (ReadCheckpointVersion(reader) != CurrentCheckpointVersion)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_FORMAT_UNSUPPORTED",
                    "Full checkpoint must use the version-two keyed checkpoint format.");
            }

            ReadAndValidateVersionTwoHeader(
                reader,
                stream,
                CheckpointArtifactKind.Full,
                ComputeConfigFingerprint(expectedConfig));

            ValidateFullCheckpointRecordsStreaming(
                reader,
                stream,
                expectedEntries,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (CheckpointValidationException)
        {
            throw;
        }
        catch (Exception exception)
            when (exception is EndOfStreamException
                or DecoderFallbackException
                or OverflowException
                or IOException
                or ArgumentException)
        {
            throw CheckpointFailure(
                "LLM_CHECKPOINT_MALFORMED",
                "Full checkpoint artifact is malformed.",
                exception);
        }
    }

    /// <summary>
    /// Builds the exact base schema and, when explicitly declared, adapter slots.
    /// Startup validation must not construct a model:
    /// a large checkpoint is hostile input and construction would allocate its
    /// entire configured weight set before the checkpoint has been admitted.
    /// </summary>
    private static IReadOnlyList<FullCheckpointSchemaEntry> BuildFullCheckpointSchema(
        ModelConfig config, // Derive every expected base tensor dimension from this model configuration.
        FullCheckpointLoraTopology? adapterTopology) // Use an optional declared topology only to append its exact adapter A/B tensor entries.
    {
        ValidateCheckpointModelConfiguration(config);

        var headDim = config.HiddenDim / config.NumHeads;
        var entries = new List<FullCheckpointSchemaEntry>(3 + (config.NumLayers * 9))
        {
            FullEntry("base.token_embedding.weight", config.VocabSize, config.HiddenDim)
        };

        for (var index = 0; index < config.NumLayers; index++)
        {
            var prefix = $"base.block_{index}.";
            entries.Add(FullEntry($"{prefix}norm1.gamma", config.HiddenDim));
            entries.Add(FullEntry($"{prefix}attn.q_proj.weight", config.HiddenDim, config.HiddenDim));
            entries.Add(FullEntry(
                $"{prefix}attn.k_proj.weight",
                config.NumKvHeads * headDim,
                config.HiddenDim));
            entries.Add(FullEntry(
                $"{prefix}attn.v_proj.weight",
                config.NumKvHeads * headDim,
                config.HiddenDim));
            entries.Add(FullEntry($"{prefix}attn.o_proj.weight", config.HiddenDim, config.HiddenDim));
            entries.Add(FullEntry($"{prefix}norm2.gamma", config.HiddenDim));
            entries.Add(FullEntry($"{prefix}ffn.gate_proj.weight", config.FfnHiddenDim, config.HiddenDim));
            entries.Add(FullEntry($"{prefix}ffn.up_proj.weight", config.FfnHiddenDim, config.HiddenDim));
            entries.Add(FullEntry($"{prefix}ffn.down_proj.weight", config.HiddenDim, config.FfnHiddenDim));
        }

        entries.Add(FullEntry("base.final_norm.gamma", config.HiddenDim));
        entries.Add(FullEntry("base.lm_head.weight", config.VocabSize, config.HiddenDim));
        if (adapterTopology is not null) // Append adapter entries only when the caller explicitly supplied their topology.
        { // The shared codec keeps exact base and explicitly declared adapter schemas.
            for (var index = 0; index < config.NumLayers; index++) // Derive slots from every configured block and declared projection.
            foreach (var projection in adapterTopology.TargetModules) // Enumerate this layer's previously admitted targets and append the two expected entries for each.
            { // The shared codec keeps exact base and explicitly declared adapter schemas.
                var outputDim = projection is "k_proj" or "v_proj" // Use grouped-query k/v width while q/o retain the full hidden width.
                    ? config.NumKvHeads * headDim : config.HiddenDim; // Use grouped-query k/v width while q/o retain the full hidden width.
                var slot = BuildAdapterSlotKey(index, projection); // Reuse canonical block/projection keys for the exact serialized slots.
                entries.Add(FullEntry($"{slot}.lora_a", adapterTopology.Rank, config.HiddenDim)); // Expect A with the declared rank and projection input width.
                entries.Add(FullEntry($"{slot}.lora_b", outputDim, adapterTopology.Rank)); // Use grouped-query k/v width while q/o retain the full hidden width.
            } // The shared codec keeps exact base and explicitly declared adapter schemas.
        } // The shared codec keeps exact base and explicitly declared adapter schemas.
        return entries;
    }

    private static FullCheckpointSchemaEntry FullEntry(string key, params int[] dimensions)
        => new(key, dimensions, ComputeElementCount(dimensions));

    private static void ValidateCheckpointModelConfiguration(ModelConfig config)
    {
        if (config.VocabSize <= 0
            || config.ContextLen <= 0
            || config.HiddenDim <= 0
            || config.NumLayers <= 0
            || config.NumHeads <= 0
            || config.NumKvHeads <= 0
            || config.FfnHiddenDim <= 0
            || !float.IsFinite(config.RopeTheta)
            || config.RopeTheta <= 0f
            || !float.IsFinite(config.RmsNormEps)
            || config.RmsNormEps <= 0f
            || config.HiddenDim % config.NumHeads != 0
            || config.NumHeads % config.NumKvHeads != 0)
        {
            throw CheckpointFailure(
                "LLM_CHECKPOINT_CONFIG_MISMATCH",
                "Full checkpoint requires a constructible transformer model configuration.");
        }
    }

    public void LoadCheckpoint(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        LoadCheckpoint(stream);
    }

    /// <summary>
    /// Loads a complete checkpoint from the caller-owned readable, seekable stream.
    /// The stream is consumed from its current position through exact EOF and remains open.
    /// </summary>
    public void LoadCheckpoint(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
        {
            throw new ArgumentException(
                "Checkpoint stream must be readable and seekable.",
                nameof(stream));
        }

        lock (_adapterMutationSync)
        {
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
                var first = reader.ReadInt32();
                if (first == AdapterCheckpointMagic)
                {
                    var version = ReadCheckpointVersion(reader);
                    IReadOnlyList<LlmCheckpointParameterEntry> expected;
                    IReadOnlyList<ParsedKeyedCheckpointEntry> parsed;
                    if (version == VersionOneCheckpoint)
                    {
                        expected = BuildVersionOneCheckpointEntries();
                        parsed = ParseKeyedRecords(
                            reader,
                            stream,
                            expected,
                            requireAdapterOnlyKeys: false,
                            orderIsAuthority: true,
                            validateVersionTwoKeyForm: false);
                    }
                    else
                    {
                        expected = BuildCheckpointEntries();
                        ReadAndValidateVersionTwoHeader(
                            reader,
                            stream,
                            CheckpointArtifactKind.Full,
                            _configFingerprint);
                        parsed = ParseKeyedRecords(
                            reader,
                            stream,
                            expected,
                            requireAdapterOnlyKeys: false,
                            orderIsAuthority: false,
                            validateVersionTwoKeyForm: true);
                    }
                    for (var index = 0; index < parsed.Count; index++)
                    {
                        Array.Copy(
                            parsed[index].Values,
                            expected[index].Parameter.Value.Data,
                            parsed[index].Values.Length);
                    }
                }
                else if (first >= 0)
                {
                    var parsed = ParseLegacyCheckpoint(first, reader, stream);
                    foreach (var entry in parsed)
                        Array.Copy(entry.Values, entry.Destination.Value.Data, entry.Values.Length);
                }
                else
                {
                    throw CheckpointFailure(
                        "LLM_CHECKPOINT_FORMAT_UNSUPPORTED",
                        "Checkpoint format is unsupported.");
                }
            }
            catch (CheckpointValidationException)
            {
                throw;
            }
            catch (Exception exception) when (
                exception is EndOfStreamException
                    or DecoderFallbackException
                    or OverflowException
                    or IOException
                    or ArgumentException)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_MALFORMED",
                    "Checkpoint is malformed.",
                    exception);
            }
        }
    }

    public void LoadAdapterCheckpoint(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        lock (_adapterMutationSync)
        {
            // Preserve the fail-fast contract: model readiness is checked before
            // any caller-supplied path is touched.
            _ = BuildAdapterCheckpointEntries();
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
            LoadAdapterCheckpoint(stream);
        }
    }

    /// <summary>
    /// Loads an adapter-only checkpoint from the caller-owned readable,
    /// seekable stream. The stream remains open and is consumed to exact EOF.
    /// </summary>
    public void LoadAdapterCheckpoint(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
        {
            throw new ArgumentException(
                "Adapter checkpoint stream must be readable and seekable.",
                nameof(stream));
        }

        lock (_adapterMutationSync)
        {
            var expected = BuildAdapterCheckpointEntries();
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
                if (reader.ReadInt32() != AdapterCheckpointMagic)
                {
                    throw CheckpointFailure(
                        "LLM_CHECKPOINT_FORMAT_UNSUPPORTED",
                        "Adapter checkpoint must use the keyed checkpoint format.");
                }

                var version = ReadCheckpointVersion(reader);
                IReadOnlyList<ParsedKeyedCheckpointEntry> parsed;
                if (version == VersionOneCheckpoint)
                {
                    parsed = ParseKeyedRecords(
                        reader,
                        stream,
                        expected,
                        requireAdapterOnlyKeys: true,
                        orderIsAuthority: true,
                        validateVersionTwoKeyForm: false);
                }
                else
                {
                    ReadAndValidateVersionTwoHeader(
                        reader,
                        stream,
                        CheckpointArtifactKind.AdapterOnly,
                        _configFingerprint);
                    parsed = ParseKeyedRecords(
                        reader,
                        stream,
                        expected,
                        requireAdapterOnlyKeys: true,
                        orderIsAuthority: false,
                        validateVersionTwoKeyForm: true);
                }
                for (var index = 0; index < parsed.Count; index++)
                {
                    Array.Copy(
                        parsed[index].Values,
                        expected[index].Parameter.Value.Data,
                        parsed[index].Values.Length);
                }
            }
            catch (CheckpointValidationException)
            {
                throw;
            }
            catch (Exception exception) when (
                exception is EndOfStreamException
                    or DecoderFallbackException
                    or OverflowException
                    or IOException
                    or ArgumentException)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_MALFORMED",
                    "Adapter checkpoint is malformed.",
                    exception);
            }
        }
    }

    /// <summary>
    /// Validates a caller-owned adapter checkpoint against this model's exact
    /// current adapter schema and v2 configuration fingerprint without loading
    /// or mutating any model parameter. The stream remains open and is consumed
    /// to exact EOF.
    /// </summary>
    public void ValidateCurrentAdapterCheckpointArtifact(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
        {
            throw new ArgumentException(
                "Adapter checkpoint stream must be readable and seekable.",
                nameof(stream));
        }

        lock (_adapterMutationSync)
        {
            var expected = BuildAdapterCheckpointEntries();
            try
            {
                using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
                if (reader.ReadInt32() != AdapterCheckpointMagic)
                {
                    throw CheckpointFailure(
                        "LLM_CHECKPOINT_FORMAT_UNSUPPORTED",
                        "Adapter checkpoint must use the keyed checkpoint format.");
                }

                if (ReadCheckpointVersion(reader) != CurrentCheckpointVersion)
                {
                    throw CheckpointFailure(
                        "LLM_CHECKPOINT_FORMAT_UNSUPPORTED",
                        "Adapter checkpoint must use the current keyed checkpoint version.");
                }

                ReadAndValidateVersionTwoHeader(
                    reader,
                    stream,
                    CheckpointArtifactKind.AdapterOnly,
                    _configFingerprint);
                _ = ParseKeyedRecords(
                    reader,
                    stream,
                    expected,
                    requireAdapterOnlyKeys: true,
                    orderIsAuthority: false,
                    validateVersionTwoKeyForm: true);
            }
            catch (CheckpointValidationException)
            {
                throw;
            }
            catch (Exception exception) when (
                exception is EndOfStreamException
                    or DecoderFallbackException
                    or OverflowException
                    or IOException
                    or ArgumentException)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_MALFORMED",
                    "Adapter checkpoint is malformed.",
                    exception);
            }
        }
    }

    private List<LlmCheckpointParameterEntry> BuildCheckpointEntries()
    {
        var entries = new List<LlmCheckpointParameterEntry>(
            _baseParameters.Length + (_orderedAdapterSlots.Count * 2));
        entries.AddRange(_baseCheckpointEntries);

        foreach (var slot in _orderedAdapterSlots)
        {
            if (slot.Adapter is null)
                continue;

            foreach (var (suffix, parameter) in slot.Adapter.AdapterParameters)
                entries.Add(new LlmCheckpointParameterEntry($"{slot.Key}.{suffix}", parameter));
        }

        return entries;
    }

    private List<LlmCheckpointParameterEntry> BuildVersionOneCheckpointEntries()
    {
        var entries = new List<LlmCheckpointParameterEntry>(
            _baseParameters.Length + (_orderedAdapterSlots.Count * 2));
        for (var index = 0; index < _baseParameters.Length; index++)
        {
            entries.Add(new LlmCheckpointParameterEntry(
                $"base.{index:D6}",
                _baseParameters[index]));
        }

        foreach (var slot in _orderedAdapterSlots)
        {
            if (slot.Adapter is null)
                continue;

            foreach (var (suffix, parameter) in slot.Adapter.AdapterParameters)
                entries.Add(new LlmCheckpointParameterEntry($"{slot.Key}.{suffix}", parameter));
        }

        return entries;
    }

    private List<LlmCheckpointParameterEntry> BuildAdapterCheckpointEntries()
    {
        var entries = new List<LlmCheckpointParameterEntry>(_orderedAdapterSlots.Count * 2);
        var hasAttachedAdapter = false;
        foreach (var slot in _orderedAdapterSlots)
        {
            if (slot.Adapter is null)
                continue;

            hasAttachedAdapter = true;
            foreach (var (suffix, parameter) in slot.Adapter.AdapterParameters)
            {
                entries.Add(new LlmCheckpointParameterEntry(
                    $"{slot.Key}.{suffix}",
                    parameter));
            }
        }

        if (!hasAttachedAdapter)
        {
            throw CheckpointFailure(
                "LLM_ADAPTER_CHECKPOINT_ADAPTERS_REQUIRED",
                "At least one attached adapter is required.");
        }

        if (entries.Count == 0)
        {
            throw CheckpointFailure(
                "LLM_ADAPTER_CHECKPOINT_INCOMPLETE",
                "Attached adapters expose no checkpoint parameters.");
        }

        return entries;
    }

    private void WriteAdapterCheckpoint(
        Stream stream,
        IReadOnlyList<LlmCheckpointParameterEntry> entries)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        WriteKeyedCheckpoint(
            writer,
            CheckpointArtifactKind.AdapterOnly,
            _configFingerprint,
            entries);
        writer.Flush();
    }

    private static void WriteKeyedCheckpoint(
        BinaryWriter writer,
        CheckpointArtifactKind artifactKind,
        byte[] configFingerprint,
        IReadOnlyList<LlmCheckpointParameterEntry> entries)
    {
        writer.Write(AdapterCheckpointMagic);
        writer.Write(CurrentCheckpointVersion);
        writer.Write((int)artifactKind);
        writer.Write(configFingerprint.Length);
        writer.Write(configFingerprint);
        writer.Write(entries.Count);
        foreach (var entry in entries)
        {
            var keyBytes = StrictUtf8.GetBytes(entry.Key);
            writer.Write(keyBytes.Length);
            writer.Write(keyBytes);
            writer.Write(entry.Parameter.Value.Shape.Rank);
            foreach (var dimension in entry.Parameter.Value.Shape.Dimensions)
                writer.Write(dimension);
            writer.Write(checked((long)entry.Parameter.Value.Data.Length * sizeof(float)));
            foreach (var value in entry.Parameter.Value.Data)
                writer.Write(value);
        }
    }

    private IReadOnlyList<ParsedCheckpointEntry> ParseLegacyCheckpoint(
        int count,
        BinaryReader reader,
        Stream stream)
    {
        if (count != _baseParameters.Length)
        {
            throw CheckpointFailure(
                "LLM_CHECKPOINT_PARAMETER_COUNT_MISMATCH",
                "Legacy checkpoint parameter count does not match the model.");
        }

        var parsed = new List<ParsedCheckpointEntry>(count);
        for (var index = 0; index < count; index++)
        {
            var destination = _baseParameters[index];
            var rank = reader.ReadInt32();
            if (rank < 0 || rank > 32)
                throw CheckpointFailure("LLM_CHECKPOINT_MALFORMED", "Checkpoint rank is malformed.");
            if (rank != destination.Value.Shape.Rank)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_PARAMETER_SHAPE_MISMATCH",
                    "Legacy checkpoint parameter rank does not match.");
            }

            var dimensions = ReadDimensions(reader, rank);
            var elementCount = ComputeElementCount(dimensions);
            if (!dimensions.SequenceEqual(destination.Value.Shape.Dimensions))
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_PARAMETER_SHAPE_MISMATCH",
                    "Legacy checkpoint parameter shape does not match.");
            }

            EnsurePayloadAvailable(stream, checked(elementCount * sizeof(float)));
            parsed.Add(new ParsedCheckpointEntry(
                destination,
                ReadValues(
                    reader,
                    checked((int)elementCount),
                    requireFinite: true)));
        }

        RequireExactEof(stream);
        return parsed;
    }

    private static IReadOnlyList<ParsedKeyedCheckpointEntry> ParseKeyedRecords(
        BinaryReader reader,
        Stream stream,
        IReadOnlyList<LlmCheckpointParameterEntry>? expected,
        bool requireAdapterOnlyKeys,
        bool orderIsAuthority,
        bool validateVersionTwoKeyForm)
    {
        var count = reader.ReadInt32();
        if (count <= 0 || count > MaximumCheckpointEntries)
            throw CheckpointFailure("LLM_CHECKPOINT_MALFORMED", "Checkpoint entry count is malformed.");

        if (expected is not null && count != expected.Count)
        {
            throw CheckpointFailure(
                CheckpointParameterCountMismatchCode,
                "Adapter checkpoint parameter count does not match the attached model.");
        }

        var parsed = new List<ParsedKeyedCheckpointEntry>(count);
        var parsedByKey = orderIsAuthority
            ? null
            : new Dictionary<string, ParsedKeyedCheckpointEntry>(StringComparer.Ordinal);
        var expectedByKey = expected is null || orderIsAuthority
            ? null
            : expected.ToDictionary(static entry => entry.Key, StringComparer.Ordinal);
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < count; index++)
        {
            var keyLength = reader.ReadInt32();
            if (keyLength < 1 || keyLength > MaximumCheckpointKeyBytes)
                throw CheckpointFailure("LLM_CHECKPOINT_MALFORMED", "Checkpoint key length is malformed.");
            EnsurePayloadAvailable(stream, keyLength);
            var keyBytes = reader.ReadBytes(keyLength);
            if (keyBytes.Length != keyLength)
                throw CheckpointFailure("LLM_CHECKPOINT_MALFORMED", "Checkpoint key is truncated.");
            var key = StrictUtf8.GetString(keyBytes);
            if (!seenKeys.Add(key))
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH",
                    "Adapter checkpoint contains a duplicate parameter key.");
            }
            if (requireAdapterOnlyKeys
                && (!key.StartsWith("lora.", StringComparison.Ordinal)
                    || key.Length == "lora.".Length))
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH",
                    "Adapter checkpoint contains a non-adapter parameter key.");
            }
            if (validateVersionTwoKeyForm
                && !IsVersionTwoCheckpointKey(key, requireAdapterOnlyKeys))
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH",
                    "Checkpoint parameter key is malformed.");
            }

            LlmCheckpointParameterEntry? expectedEntry = null;
            if (expected is not null && orderIsAuthority)
                expectedEntry = expected[index];
            else if (expectedByKey is not null
                     && !expectedByKey.TryGetValue(key, out expectedEntry))
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH",
                    "Checkpoint contains an unknown parameter key.");
            }

            if (expectedEntry is not null
                && orderIsAuthority
                && !StringComparer.Ordinal.Equals(key, expectedEntry.Key))
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH",
                    "Adapter checkpoint parameter key does not match.");
            }

            var rank = reader.ReadInt32();
            if (rank < 0 || rank > 32)
                throw CheckpointFailure("LLM_CHECKPOINT_MALFORMED", "Checkpoint rank is malformed.");
            if (expectedEntry is not null
                && rank != expectedEntry.Parameter.Value.Shape.Rank)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_PARAMETER_SHAPE_MISMATCH",
                    "Adapter checkpoint parameter rank does not match.");
            }

            var dimensions = ReadDimensions(reader, rank);
            var elementCount = ComputeElementCount(dimensions);
            if (expectedEntry is not null
                && !dimensions.SequenceEqual(
                    expectedEntry.Parameter.Value.Shape.Dimensions))
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_PARAMETER_SHAPE_MISMATCH",
                    "Adapter checkpoint parameter shape does not match.");
            }

            var expectedPayloadBytes = checked(elementCount * sizeof(float));
            var payloadBytes = reader.ReadInt64();
            if (payloadBytes < 0 || payloadBytes != expectedPayloadBytes)
                throw CheckpointFailure("LLM_CHECKPOINT_MALFORMED", "Checkpoint payload length is malformed.");
            EnsurePayloadAvailable(stream, payloadBytes);
            var parsedEntry = new ParsedKeyedCheckpointEntry(
                key,
                dimensions,
                ReadValues(
                    reader,
                    checked((int)elementCount),
                    requireFinite: true));
            parsed.Add(parsedEntry);
            parsedByKey?.Add(key, parsedEntry);
        }

        if (expected is not null && seenKeys.Count != expected.Count)
        {
            throw CheckpointFailure(
                "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH",
                "Adapter checkpoint key set is incomplete.");
        }

        RequireExactEof(stream);
        if (expected is null || orderIsAuthority)
            return parsed;

        return expected
            .Select(entry => parsedByKey![entry.Key])
            .ToArray();
    }

    private static void ValidateKeyedRecordsStreaming(
        BinaryReader reader,
        Stream stream,
        bool requireAdapterOnlyKeys,
        bool validateVersionTwoKeyForm)
    {
        var count = reader.ReadInt32();
        if (count <= 0 || count > MaximumStaticAdapterCheckpointEntries)
        {
            throw CheckpointFailure(
                "LLM_CHECKPOINT_MALFORMED",
                "Checkpoint entry count is malformed.");
        }

        var seenKeys = new HashSet<string>(StringComparer.Ordinal);
        var aggregatePayloadBytes = 0L;
        for (var index = 0; index < count; index++)
        {
            var keyLength = reader.ReadInt32();
            if (keyLength < 1 || keyLength > MaximumCheckpointKeyBytes)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_MALFORMED",
                    "Checkpoint key length is malformed.");
            }

            EnsurePayloadAvailable(stream, keyLength);
            var keyBytes = reader.ReadBytes(keyLength);
            if (keyBytes.Length != keyLength)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_MALFORMED",
                    "Checkpoint key is truncated.");
            }

            var key = StrictUtf8.GetString(keyBytes);
            if (!seenKeys.Add(key))
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH",
                    "Adapter checkpoint contains a duplicate parameter key.");
            }
            if (requireAdapterOnlyKeys
                && (!key.StartsWith("lora.", StringComparison.Ordinal)
                    || key.Length == "lora.".Length))
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH",
                    "Adapter checkpoint contains a non-adapter parameter key.");
            }
            if (validateVersionTwoKeyForm
                && !IsVersionTwoCheckpointKey(key, requireAdapterOnlyKeys))
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH",
                    "Checkpoint parameter key is malformed.");
            }

            var rank = reader.ReadInt32();
            if (rank < 0 || rank > 32)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_MALFORMED",
                    "Checkpoint rank is malformed.");
            }

            var dimensions = ReadDimensions(reader, rank);
            var elementCount = ComputeElementCount(dimensions);
            var expectedPayloadBytes = checked(elementCount * sizeof(float));
            var payloadBytes = reader.ReadInt64();
            if (payloadBytes < 0 || payloadBytes != expectedPayloadBytes)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_MALFORMED",
                    "Checkpoint payload length is malformed.");
            }
            if (payloadBytes > MaximumStaticAdapterRecordPayloadBytes)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_MALFORMED",
                    "Checkpoint record payload exceeds the static validation limit.");
            }

            aggregatePayloadBytes = checked(aggregatePayloadBytes + payloadBytes);
            if (aggregatePayloadBytes > MaximumStaticAdapterAggregatePayloadBytes)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_MALFORMED",
                    "Checkpoint aggregate payload exceeds the static validation limit.");
            }

            EnsurePayloadAvailable(stream, payloadBytes);
            ReadAndValidateValues(reader, checked((int)elementCount));
        }

        RequireExactEof(stream);
    }

    private static void ValidateFullCheckpointRecordsStreaming(
        BinaryReader reader,
        Stream stream,
        IReadOnlyList<FullCheckpointSchemaEntry> expectedEntries,
        CancellationToken cancellationToken)
    {
        var count = reader.ReadInt32();
        if (count != expectedEntries.Count)
        {
            throw CheckpointFailure(
                CheckpointParameterCountMismatchCode,
                "Full checkpoint parameter count does not match the configured model.");
        }

        var expectedByKey = expectedEntries.ToDictionary(
            static entry => entry.Key,
            StringComparer.Ordinal);
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var keyLength = reader.ReadInt32();
            if (keyLength < 1 || keyLength > MaximumCheckpointKeyBytes)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_MALFORMED",
                    "Checkpoint key length is malformed.");
            }

            EnsurePayloadAvailable(stream, keyLength);
            var keyBytes = reader.ReadBytes(keyLength);
            if (keyBytes.Length != keyLength)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_MALFORMED",
                    "Checkpoint key is truncated.");
            }

            var key = StrictUtf8.GetString(keyBytes);
            if (!seenKeys.Add(key) || !expectedByKey.TryGetValue(key, out var expected))
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH",
                    "Full checkpoint parameter keys do not match the configured base model.");
            }

            var rank = reader.ReadInt32();
            if (rank != expected.Dimensions.Length)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_PARAMETER_SHAPE_MISMATCH",
                    "Full checkpoint parameter rank does not match the configured model.");
            }

            var dimensions = ReadDimensions(reader, rank);
            if (!dimensions.SequenceEqual(expected.Dimensions))
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_PARAMETER_SHAPE_MISMATCH",
                    "Full checkpoint parameter shape does not match the configured model.");
            }

            var payloadBytes = reader.ReadInt64();
            var expectedPayloadBytes = checked(expected.ElementCount * sizeof(float));
            if (payloadBytes < 0 || payloadBytes != expectedPayloadBytes)
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_MALFORMED",
                    "Full checkpoint payload length is malformed.");
            }

            EnsurePayloadAvailable(stream, payloadBytes);
            ReadAndValidateValuesStreaming(reader, expected.ElementCount, cancellationToken);
        }

        if (seenKeys.Count != expectedEntries.Count)
        {
            throw CheckpointFailure(
                "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH",
                "Full checkpoint parameter key set is incomplete.");
        }

        RequireExactEof(stream);
    }

    private static int ReadCheckpointVersion(BinaryReader reader)
    {
        var version = reader.ReadInt32();
        if (version is not VersionOneCheckpoint and not CurrentCheckpointVersion)
        {
            throw CheckpointFailure(
                "LLM_CHECKPOINT_FORMAT_UNSUPPORTED",
                "Checkpoint version is unsupported.");
        }

        return version;
    }

    private static void ReadAndValidateVersionTwoHeader(
        BinaryReader reader,
        Stream stream,
        CheckpointArtifactKind expectedArtifactKind,
        byte[]? expectedFingerprint)
    {
        var artifactKindValue = reader.ReadInt32();
        if (!Enum.IsDefined(typeof(CheckpointArtifactKind), artifactKindValue))
        {
            throw CheckpointFailure(
                "LLM_CHECKPOINT_MALFORMED",
                "Checkpoint artifact kind is malformed.");
        }

        var artifactKind = (CheckpointArtifactKind)artifactKindValue;
        if (artifactKind != expectedArtifactKind)
        {
            throw CheckpointFailure(
                "LLM_CHECKPOINT_ARTIFACT_KIND_MISMATCH",
                "Checkpoint artifact kind does not match the requested operation.");
        }

        var fingerprintLength = reader.ReadInt32();
        if (fingerprintLength != CheckpointFingerprintBytes)
        {
            throw CheckpointFailure(
                "LLM_CHECKPOINT_MALFORMED",
                "Checkpoint fingerprint length is malformed.");
        }

        EnsurePayloadAvailable(stream, fingerprintLength);
        var fingerprint = reader.ReadBytes(fingerprintLength);
        if (fingerprint.Length != fingerprintLength)
        {
            throw CheckpointFailure(
                "LLM_CHECKPOINT_MALFORMED",
                "Checkpoint fingerprint is truncated.");
        }

        if (expectedFingerprint is not null
            && !CryptographicOperations.FixedTimeEquals(fingerprint, expectedFingerprint))
        {
            throw CheckpointFailure(
                "LLM_CHECKPOINT_CONFIG_MISMATCH",
                "Checkpoint model configuration does not match the receiving model.");
        }
    }

    private static byte[] ComputeConfigFingerprint(ModelConfig config)
    {
        Span<byte> canonical = stackalloc byte[9 * sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(canonical[0..4], config.VocabSize);
        BinaryPrimitives.WriteInt32LittleEndian(canonical[4..8], config.ContextLen);
        BinaryPrimitives.WriteInt32LittleEndian(canonical[8..12], config.HiddenDim);
        BinaryPrimitives.WriteInt32LittleEndian(canonical[12..16], config.NumLayers);
        BinaryPrimitives.WriteInt32LittleEndian(canonical[16..20], config.NumHeads);
        BinaryPrimitives.WriteInt32LittleEndian(canonical[20..24], config.NumKvHeads);
        BinaryPrimitives.WriteInt32LittleEndian(canonical[24..28], config.FfnHiddenDim);
        BinaryPrimitives.WriteInt32LittleEndian(
            canonical[28..32],
            BitConverter.SingleToInt32Bits(config.RopeTheta));
        BinaryPrimitives.WriteInt32LittleEndian(
            canonical[32..36],
            BitConverter.SingleToInt32Bits(config.RmsNormEps));
        return SHA256.HashData(canonical);
    }

    private static bool IsVersionTwoCheckpointKey(
        string key,
        bool requireAdapterOnlyKeys)
    {
        if (key.StartsWith("base.", StringComparison.Ordinal))
            return !requireAdapterOnlyKeys && key.Length > "base.".Length;
        if (!key.StartsWith("lora.", StringComparison.Ordinal))
            return false;

        var segments = key.Split('.');
        if (segments.Length != 4
            || segments[0] != "lora"
            || !segments[1].StartsWith("block_", StringComparison.Ordinal)
            || !CausalSelfAttention.CanonicalProjectionNames.Contains(
                segments[2],
                StringComparer.Ordinal)
            || segments[3] is not ("lora_a" or "lora_b"))
        {
            return false;
        }

        var blockIndexText = segments[1]["block_".Length..];
        return int.TryParse(
                   blockIndexText,
                   NumberStyles.None,
                   CultureInfo.InvariantCulture,
                   out var blockIndex)
               && string.Equals(
                   segments[1],
                   $"block_{blockIndex:D4}",
                   StringComparison.Ordinal);
    }

    private static int[] ReadDimensions(BinaryReader reader, int rank)
    {
        var dimensions = new int[rank];
        for (var index = 0; index < rank; index++)
        {
            dimensions[index] = reader.ReadInt32();
            if (dimensions[index] <= 0)
                throw CheckpointFailure("LLM_CHECKPOINT_MALFORMED", "Checkpoint dimension is malformed.");
        }

        return dimensions;
    }

    private static long ComputeElementCount(IReadOnlyList<int> dimensions)
    {
        var count = 1L;
        checked
        {
            foreach (var dimension in dimensions)
                count *= dimension;
        }

        if (count > int.MaxValue)
            throw CheckpointFailure("LLM_CHECKPOINT_MALFORMED", "Checkpoint tensor is too large.");
        return count;
    }

    private static void EnsurePayloadAvailable(Stream stream, long byteCount)
    {
        if (byteCount < 0 || byteCount > stream.Length - stream.Position)
            throw CheckpointFailure("LLM_CHECKPOINT_MALFORMED", "Checkpoint payload is truncated.");
    }

    private static float[] ReadValues(
        BinaryReader reader,
        int count,
        bool requireFinite)
    {
        var values = new float[count];
        for (var index = 0; index < count; index++)
        {
            values[index] = reader.ReadSingle();
            if (requireFinite && !float.IsFinite(values[index]))
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_MALFORMED",
                    "Checkpoint contains a non-finite parameter value.");
            }
        }
        return values;
    }

    private static void ReadAndValidateValues(BinaryReader reader, int count)
    {
        for (var index = 0; index < count; index++)
        {
            if (!float.IsFinite(reader.ReadSingle()))
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_MALFORMED",
                    "Checkpoint contains a non-finite parameter value.");
            }
        }
    }

    private static void ReadAndValidateValuesStreaming(
        BinaryReader reader,
        long count,
        CancellationToken cancellationToken)
    {
        for (var index = 0L; index < count; index++)
        {
            if ((index & 0x3FFF) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            if (!float.IsFinite(reader.ReadSingle()))
            {
                throw CheckpointFailure(
                    "LLM_CHECKPOINT_MALFORMED",
                    "Checkpoint contains a non-finite parameter value.");
            }
        }
    }

    private static void RequireExactEof(Stream stream)
    {
        if (stream.Position != stream.Length)
            throw CheckpointFailure("LLM_CHECKPOINT_MALFORMED", "Checkpoint contains trailing bytes.");
    }

    private static string BuildAdapterSlotKey(int blockIndex, string projection)
    {
        if (!CausalSelfAttention.CanonicalProjectionNames.Contains(
                projection,
                StringComparer.Ordinal))
        {
            throw new InvalidOperationException("LORA_REGISTRATION_KEY_CONFLICT");
        }

        return $"lora.block_{blockIndex:D4}.{projection}";
    }

    private static CheckpointValidationException CheckpointFailure(
        string code,
        string message,
        Exception? inner = null)
        => new(code, $"{code}: {message}", inner);

    private sealed record ParsedCheckpointEntry(Parameter Destination, float[] Values);
    private sealed record ParsedKeyedCheckpointEntry(
        string Key,
        int[] Dimensions,
        float[] Values);
    private sealed record FullCheckpointSchemaEntry(
        string Key,
        int[] Dimensions,
        long ElementCount);

    private enum CheckpointArtifactKind
    {
        Full = 1,
        AdapterOnly = 2
    }

    private sealed class CheckpointValidationException : Exception
    {
        public CheckpointValidationException(
            string code,
            string message,
            Exception? inner)
            : base(message, inner)
        {
            Code = code;
        }

        internal string Code { get; }
    }
}
