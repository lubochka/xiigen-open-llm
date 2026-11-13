// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using DesignConversionDomain;
using LoRA;
using TensorCore.Core;
using TensorCore.Layers;
using TransformerModel;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

public sealed class LlmCheckpointIdentityV2Tests
{
    [Fact]
    public void StaticFullLoraValidation_RequiresExactDeclaredGqaTopologyAndPreservesLegacyBaseOnly()
    {
        var directory = CreateRoot();
        try
        {
        var config = TinyConfig() with { HiddenDim = 8, NumHeads = 4, NumKvHeads = 1 };
        var lora = new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] };
        var model = new LlmModel(config);
        LoraApplicator.ApplySeeded(model, lora, 20261014, "test-policy");
        FillSequential(model.Parameters());
        var fullPath = Path.Combine(directory, "full-lora.bin");
        model.SaveCheckpoint(fullPath);
        using var full = new MemoryStream(File.ReadAllBytes(fullPath), writable: false);
        var exact = new FullCheckpointLoraTopology(lora.Rank, lora.TargetModules);
        full.Position = 0;
        var admitted = LlmModel.ValidateFullCheckpointArtifact(full, config, exact);
        Assert.True(admitted.IsSuccess, admitted.ErrorCode + ": " + admitted.ErrorMessage);
        Assert.True(admitted.Value);
        Assert.Equal(full.Length, full.Position);
        full.Position = 0;
        Assert.ThrowsAny<Exception>(() => LlmModel.ValidateFullCheckpointArtifact(full, config));
        full.Position = 0;
        var wrongRank = LlmModel.ValidateFullCheckpointArtifact(full, config, new FullCheckpointLoraTopology(3, lora.TargetModules));
        Assert.False(wrongRank.IsSuccess); Assert.Equal("LLM_CHECKPOINT_PARAMETER_SHAPE_MISMATCH", wrongRank.ErrorCode);
        full.Position = 0;
        var wrongTargets = LlmModel.ValidateFullCheckpointArtifact(full, config, new FullCheckpointLoraTopology(2, ["k_proj", "o_proj"]));
        Assert.False(wrongTargets.IsSuccess); Assert.Equal("LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH", wrongTargets.ErrorCode);
        full.Position = 0;
        using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
        Assert.Throws<OperationCanceledException>(() => LlmModel.ValidateFullCheckpointArtifact(full, config, exact, cancellationToken: cancelled.Token));
        var basePath = Path.Combine(directory, "base-only.bin");
        new LlmModel(config).SaveCheckpoint(basePath);
        using var baseOnly = new MemoryStream(File.ReadAllBytes(basePath), writable: false);
        baseOnly.Position = 0;
        LlmModel.ValidateFullCheckpointArtifact(baseOnly, config);
        baseOnly.Position = 0;
        var missingAdapters = LlmModel.ValidateFullCheckpointArtifact(baseOnly, config, exact);
        Assert.False(missingAdapters.IsSuccess); Assert.Equal("LLM_CHECKPOINT_PARAMETER_COUNT_MISMATCH", missingAdapters.ErrorCode);
        Assert.Throws<ArgumentException>(() => new FullCheckpointLoraTopology(2, ["ffn"]));
        Assert.Throws<ArgumentException>(() => new FullCheckpointLoraTopology(2, ["q_proj", "q_proj"]));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    private const int CheckpointMagic = unchecked((int)0xC14F5241);
    private const int VersionTwo = 2;
    private const int FullArtifact = 1;
    private const int AdapterArtifact = 2;

    [Fact]
    public void AdapterCheckpointStreamOverload_UsesExactV2ArtifactAndLeavesCallerStreamOpen()
    {
        var root = CreateRoot();
        try
        {
            var model = CreateAttachedModel();
            FillSequential(model.Parameters());
            using var stream = new MemoryStream();

            model.SaveAdapterCheckpoint(stream);

            Assert.True(stream.CanRead);
            Assert.True(stream.CanWrite);
            Assert.Equal(stream.Length, stream.Position);
            Assert.True(stream.Length > 0);
            var streamedBytes = stream.ToArray();
            stream.Position = 0;
            model.ValidateCurrentAdapterCheckpointArtifact(stream);
            Assert.Equal(stream.Length, stream.Position);
            stream.Position = 0;
            LlmModel.ValidateAdapterCheckpointArtifact(stream);
            Assert.Equal(stream.Length, stream.Position);

            var path = Path.Combine(root, "adapter-path.bin");
            model.SaveAdapterCheckpoint(path);
            Assert.Equal(File.ReadAllBytes(path), streamedBytes);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void AdapterCheckpointStreamOverload_RejectsUnsupportedOrUnreadyInputWithoutTakingOwnership()
    {
        using var unsupported = new NonSeekableWritableStream();
        var attachedModel = CreateAttachedModel();

        Assert.Throws<ArgumentException>(() => attachedModel.SaveAdapterCheckpoint(unsupported));
        Assert.True(unsupported.CanWrite);

        using var owned = new MemoryStream();
        var noAdapterModel = new LlmModel(TinyConfig());
        Assert.ThrowsAny<Exception>(() => noAdapterModel.SaveAdapterCheckpoint(owned));
        Assert.True(owned.CanWrite);
        Assert.Equal(0, owned.Length);
    }

    [Fact]
    public void VersionTwoBaseEntriesUseSemanticRegistrationPaths()
    {
        var root = CreateRoot();
        try
        {
            var path = Path.Combine(root, "semantic.bin");
            new LlmModel(TinyConfig()).SaveCheckpoint(path);

            var keys = ReadVersionTwo(path).Entries
                .Select(static entry => entry.Key)
                .ToArray();

            Assert.Equal(
                [
                    "base.token_embedding.weight",
                    "base.block_0.norm1.gamma",
                    "base.block_0.attn.q_proj.weight",
                    "base.block_0.attn.k_proj.weight",
                    "base.block_0.attn.v_proj.weight",
                    "base.block_0.attn.o_proj.weight",
                    "base.block_0.norm2.gamma",
                    "base.block_0.ffn.gate_proj.weight",
                    "base.block_0.ffn.up_proj.weight",
                    "base.block_0.ffn.down_proj.weight",
                    "base.final_norm.gamma",
                    "base.lm_head.weight"
                ],
                keys);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void PublicCheckpointEntriesPreserveVersionOnePositionalIdentityContract()
    {
        var model = CreateAttachedModel();

        var entries = model.GetCheckpointParameterEntries();

        Assert.Equal(
            Enumerable.Range(0, model.BaseParameters.Count)
                .Select(static index => $"base.{index:D6}"),
            entries.Take(model.BaseParameters.Count).Select(static entry => entry.Key));
        Assert.Equal(
            [
                "lora.block_0000.q_proj.lora_a",
                "lora.block_0000.q_proj.lora_b",
                "lora.block_0000.v_proj.lora_a",
                "lora.block_0000.v_proj.lora_b"
            ],
            entries.Skip(model.BaseParameters.Count).Select(static entry => entry.Key));
    }

    [Fact]
    public void DesignConversionInitialWeightIdentityRemainsVersionOneCompatible()
    {
        var model = CreateAttachedModel();
        FillSequential(model.Parameters());

        var expected = ComputeVersionOneInitialWeightSha256(model);

        Assert.Equal(
            expected,
            DesignConversionDpoPolicyBaseMetadata.ComputeInitialWeightSha256(model));
    }

    [Fact]
    public void VersionTwoFingerprintUsesAllNineFieldsInCanonicalBinaryOrder()
    {
        var root = CreateRoot();
        try
        {
            var config = new ModelConfig
            {
                VocabSize = 17,
                ContextLen = 19,
                HiddenDim = 6,
                NumLayers = 1,
                NumHeads = 2,
                NumKvHeads = 1,
                FfnHiddenDim = 10,
                RopeTheta = 12_345.125f,
                RmsNormEps = BitConverter.Int32BitsToSingle(0x3727C5AC)
            };
            var path = Path.Combine(root, "fingerprint.bin");
            new LlmModel(config).SaveCheckpoint(path);
            var artifact = ReadVersionTwo(path);
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

            Assert.Equal(SHA256.HashData(canonical), artifact.Fingerprint);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void VersionTwoFullCheckpointIgnoresRecordOrderAndRoundTripsBaseAndAdapters()
    {
        var root = CreateRoot();
        try
        {
            var source = CreateAttachedModel();
            FillSequential(source.Parameters());
            var canonicalPath = Path.Combine(root, "canonical.bin");
            var shuffledPath = Path.Combine(root, "shuffled.bin");
            source.SaveCheckpoint(canonicalPath);
            var artifact = ReadVersionTwo(canonicalPath);
            Assert.Equal(FullArtifact, artifact.Kind);
            WriteVersionTwo(
                shuffledPath,
                artifact.Kind,
                artifact.Fingerprint,
                artifact.Entries.Reverse().ToArray());

            var destination = CreateAttachedModel();
            FillSequential(destination.Parameters(), 10_000);
            destination.LoadCheckpoint(shuffledPath);

            AssertParameterValues(source.Parameters(), destination.Parameters());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void VersionTwoAdapterCheckpointRoundTripsWithConfigIdentity()
    {
        var root = CreateRoot();
        try
        {
            var source = CreateAttachedModel();
            FillSequential(source.Parameters());
            var path = Path.Combine(root, "adapter.bin");
            var shuffledPath = Path.Combine(root, "adapter-shuffled.bin");
            source.SaveAdapterCheckpoint(path);
            var artifact = ReadVersionTwo(path);
            Assert.Equal(AdapterArtifact, artifact.Kind);
            Assert.Equal(32, artifact.Fingerprint.Length);
            WriteVersionTwo(
                shuffledPath,
                artifact.Kind,
                artifact.Fingerprint,
                artifact.Entries.Reverse().ToArray());
            LlmModel.ValidateAdapterCheckpointArtifact(shuffledPath);

            var destination = CreateAttachedModel();
            FillSequential(destination.Parameters(), 20_000);
            var baseBefore = SnapshotBits(destination.BaseParameters);
            destination.LoadAdapterCheckpoint(shuffledPath);

            AssertBitsEqual(baseBefore, destination.BaseParameters);
            AssertParameterValues(
                source.Parameters().Skip(source.BaseParameters.Count),
                destination.Parameters().Skip(destination.BaseParameters.Count));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void VersionTwoAdapterCheckpointRejectsDifferentConfigWithoutMutation()
    {
        var root = CreateRoot();
        try
        {
            var source = CreateAttachedModel();
            var path = Path.Combine(root, "adapter-config.bin");
            source.SaveAdapterCheckpoint(path);
            var destination = new LlmModel(TinyConfig() with { ContextLen = 7 });
            _ = LoraApplicator.Apply(
                destination,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4,
                    Dropout = 0,
                    TargetModules = ["q_proj", "v_proj"]
                });
            FillSequential(destination.Parameters(), 25_000);
            var before = SnapshotBits(destination.Parameters());

            var exception = Assert.ThrowsAny<Exception>(
                () => destination.LoadAdapterCheckpoint(path));

            Assert.StartsWith("LLM_CHECKPOINT_CONFIG_MISMATCH:", exception.Message);
            AssertBitsEqual(before, destination.Parameters());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void StaticVersionTwoAdapterValidationChecksKindFingerprintKeyFormAndEof()
    {
        var root = CreateRoot();
        try
        {
            var source = CreateAttachedModel();
            var validPath = Path.Combine(root, "valid-adapter.bin");
            source.SaveAdapterCheckpoint(validPath);
            var artifact = ReadVersionTwo(validPath);
            LlmModel.ValidateAdapterCheckpointArtifact(validPath);

            var wrongKindPath = Path.Combine(root, "wrong-kind.bin");
            WriteVersionTwo(
                wrongKindPath,
                FullArtifact,
                artifact.Fingerprint,
                artifact.Entries);
            var wrongKind = Assert.ThrowsAny<Exception>(
                () => LlmModel.ValidateAdapterCheckpointArtifact(wrongKindPath));
            Assert.StartsWith(
                "LLM_CHECKPOINT_ARTIFACT_KIND_MISMATCH:",
                wrongKind.Message);

            var fingerprintPath = Path.Combine(root, "fingerprint-length.bin");
            WriteVersionTwo(
                fingerprintPath,
                AdapterArtifact,
                artifact.Fingerprint[..31],
                artifact.Entries);
            var fingerprint = Assert.ThrowsAny<Exception>(
                () => LlmModel.ValidateAdapterCheckpointArtifact(fingerprintPath));
            Assert.StartsWith("LLM_CHECKPOINT_MALFORMED:", fingerprint.Message);

            var malformedKeyPath = Path.Combine(root, "malformed-key.bin");
            var malformedEntries = CloneEntries(artifact.Entries);
            malformedEntries[0] = malformedEntries[0] with
            {
                Key = "lora.block_x.q_proj.lora_a"
            };
            WriteVersionTwo(
                malformedKeyPath,
                AdapterArtifact,
                artifact.Fingerprint,
                malformedEntries);
            var malformedKey = Assert.ThrowsAny<Exception>(
                () => LlmModel.ValidateAdapterCheckpointArtifact(malformedKeyPath));
            Assert.StartsWith(
                "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH:",
                malformedKey.Message);

            var trailingPath = Path.Combine(root, "trailing.bin");
            File.WriteAllBytes(
                trailingPath,
                File.ReadAllBytes(validPath).Append((byte)0x5a).ToArray());
            var trailing = Assert.ThrowsAny<Exception>(
                () => LlmModel.ValidateAdapterCheckpointArtifact(trailingPath));
            Assert.StartsWith("LLM_CHECKPOINT_MALFORMED:", trailing.Message);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void StaticFullValidationRequiresExactConfiguredBaseSchemaAndRejectsAdapters()
    {
        var root = CreateRoot();
        try
        {
            var config = TinyConfig();
            var canonicalPath = Path.Combine(root, "canonical-full.bin");
            new LlmModel(config).SaveCheckpoint(canonicalPath);
            LlmModel.ValidateFullCheckpointArtifact(canonicalPath, config);

            var canonical = ReadVersionTwo(canonicalPath);
            var foreignKeyEntries = CloneEntries(canonical.Entries);
            foreignKeyEntries[2] = foreignKeyEntries[2] with
            {
                // This has the same rank/payload as q_proj and is accepted by
                // the generic v2 base-key grammar, but is not a base parameter.
                Key = "base.block_0.attn.foreign_proj.weight"
            };
            var foreignKeyPath = Path.Combine(root, "foreign-base-key.bin");
            WriteVersionTwo(
                foreignKeyPath,
                FullArtifact,
                canonical.Fingerprint,
                foreignKeyEntries);
            var foreignKeyFailure = Assert.ThrowsAny<Exception>(
                () => LlmModel.ValidateFullCheckpointArtifact(foreignKeyPath, config));
            Assert.StartsWith(
                "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH:",
                foreignKeyFailure.Message);

            var attached = CreateAttachedModel();
            var adapterContaminatedPath = Path.Combine(root, "adapter-contaminated-full.bin");
            attached.SaveCheckpoint(adapterContaminatedPath);
            var adapterFailure = Assert.ThrowsAny<Exception>(
                () => LlmModel.ValidateFullCheckpointArtifact(adapterContaminatedPath, config));
            Assert.StartsWith(
                "LLM_CHECKPOINT_PARAMETER_COUNT_MISMATCH:",
                adapterFailure.Message);

            var wrongShapeEntries = CloneEntries(canonical.Entries);
            wrongShapeEntries[2] = wrongShapeEntries[2] with
            {
                // q_proj remains 16 values and a self-consistent payload, but
                // not the configured [4,4] parameter shape.
                Dimensions = [2, 8]
            };
            var wrongShapePath = Path.Combine(root, "wrong-configured-shape.bin");
            WriteVersionTwo(
                wrongShapePath,
                FullArtifact,
                canonical.Fingerprint,
                wrongShapeEntries);
            var wrongShapeFailure = Assert.ThrowsAny<Exception>(
                () => LlmModel.ValidateFullCheckpointArtifact(wrongShapePath, config));
            Assert.StartsWith(
                "LLM_CHECKPOINT_PARAMETER_SHAPE_MISMATCH:",
                wrongShapeFailure.Message);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void StaticFullValidation_EnforcesAdmittedSizeAndCancellationContracts()
    {
        var root = CreateRoot();
        try
        {
            var config = TinyConfig();
            var path = Path.Combine(root, "bounded-full.bin");
            new LlmModel(config).SaveCheckpoint(path);
            var length = new FileInfo(path).Length;

            LlmModel.ValidateFullCheckpointArtifact(path, config, length);
            var sizeFailure = Assert.ThrowsAny<Exception>(
                () => LlmModel.ValidateFullCheckpointArtifact(path, config, length - 1));
            Assert.StartsWith("LLM_CHECKPOINT_MALFORMED:", sizeFailure.Message);

            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.Throws<OperationCanceledException>(
                () => LlmModel.ValidateFullCheckpointArtifact(
                    path,
                    config,
                    maximumArtifactBytes: length,
                    cancellationToken: cancellation.Token));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void StaticFullValidation_AllowsConstructibleOddHeadDimension()
    {
        var root = CreateRoot();
        try
        {
            var oddHeadConfig = TinyConfig() with
            {
                HiddenDim = 3,
                NumHeads = 1,
                NumKvHeads = 1,
                FfnHiddenDim = 6,
            };
            var path = Path.Combine(root, "odd-head-dimension.bin");
            new LlmModel(oddHeadConfig).SaveCheckpoint(path);

            LlmModel.ValidateFullCheckpointArtifact(path, oddHeadConfig);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("lora.block_0.q_proj.lora_a", false)]
    [InlineData("lora.block_00.q_proj.lora_a", false)]
    [InlineData("lora.block_00000.q_proj.lora_a", false)]
    [InlineData("lora.block_+000.q_proj.lora_a", false)]
    [InlineData("lora.block_999999999999999999999999.q_proj.lora_a", false)]
    [InlineData("lora.block_0000.q_proj.lora_a", true)]
    [InlineData("lora.block_10000.q_proj.lora_a", true)]
    public void StaticVersionTwoAdapterValidationRequiresCanonicalBlockIndex(
        string key,
        bool valid)
    {
        using var stream = CreateVersionTwoAdapterStream(
            [new RawEntry(key, [1], [0.25f])]);

        if (valid)
        {
            LlmModel.ValidateAdapterCheckpointArtifact(stream);
            return;
        }

        var exception = Assert.ThrowsAny<Exception>(
            () => LlmModel.ValidateAdapterCheckpointArtifact(stream));
        Assert.StartsWith(
            "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH:",
            exception.Message);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void StaticAdapterValidationRejectsExcessiveEntryCountBeforeRecordAllocation(
        int version)
    {
        using var stream = CreateEntryCountOnlyStream(version, 16_385);

        var exception = Assert.ThrowsAny<Exception>(
            () => LlmModel.ValidateAdapterCheckpointArtifact(stream));

        Assert.StartsWith("LLM_CHECKPOINT_MALFORMED:", exception.Message);
        Assert.Equal(stream.Length, stream.Position);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void StaticAdapterValidationRejectsOversizedRecordBeforePayloadRead(int version)
    {
        const int elementCount = (64 * 1024 * 1024 / sizeof(float)) + 1;
        var prefix = CreateAdapterRecordPrefix(
            version,
            "lora.block_0000.q_proj.lora_a",
            elementCount);
        using var stream = new SparsePayloadStream(
            prefix,
            checked((long)elementCount * sizeof(float)));

        var exception = Assert.ThrowsAny<Exception>(
            () => LlmModel.ValidateAdapterCheckpointArtifact(stream));

        Assert.StartsWith("LLM_CHECKPOINT_MALFORMED:", exception.Message);
        Assert.Equal(0, stream.PayloadReadAttempts);
        Assert.Equal(prefix.Length, stream.Position);
    }

    [Fact]
    public void LegacyAndVersionOneFullFixturesRemainReadable()
    {
        var root = CreateRoot();
        try
        {
            var source = new LlmModel(TinyConfig());
            FillSequential(source.BaseParameters);
            var legacyPath = Path.Combine(root, "legacy.bin");
            var versionOneSource = CreateAttachedModel();
            FillSequential(versionOneSource.Parameters(), 10_000);
            var versionOnePath = Path.Combine(root, "version-one-full.bin");
            WriteLegacy(legacyPath, source.BaseParameters);
            WriteVersionOne(
                versionOnePath,
                CreateVersionOneFullEntries(versionOneSource));

            var legacyDestination = new LlmModel(TinyConfig());
            var versionOneDestination = CreateAttachedModel();
            legacyDestination.LoadCheckpoint(legacyPath);
            versionOneDestination.LoadCheckpoint(versionOnePath);

            AssertParameterValues(source.BaseParameters, legacyDestination.BaseParameters);
            AssertParameterValues(versionOneSource.Parameters(), versionOneDestination.Parameters());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void VersionOneAdapterFixtureRemainsValidAndReadable()
    {
        var root = CreateRoot();
        try
        {
            var source = CreateAttachedModel();
            FillSequential(source.Parameters());
            var path = Path.Combine(root, "version-one-adapter.bin");
            WriteVersionOne(
                path,
                source.GetCheckpointParameterEntries()
                    .Skip(source.BaseParameters.Count)
                    .Select(ToRawEntry)
                    .ToArray());

            LlmModel.ValidateAdapterCheckpointArtifact(path);
            var destination = CreateAttachedModel();
            var baseBefore = SnapshotBits(destination.BaseParameters);
            destination.LoadAdapterCheckpoint(path);

            AssertBitsEqual(baseBefore, destination.BaseParameters);
            AssertParameterValues(
                source.Parameters().Skip(source.BaseParameters.Count),
                destination.Parameters().Skip(destination.BaseParameters.Count));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("ContextLen")]
    [InlineData("RopeTheta")]
    [InlineData("RmsNormEps")]
    public void VersionTwoRejectsSameShapeDifferentConfigurationWithoutMutation(string field)
    {
        var root = CreateRoot();
        try
        {
            var source = new LlmModel(TinyConfig());
            FillSequential(source.BaseParameters);
            var path = Path.Combine(root, field + ".bin");
            source.SaveCheckpoint(path);
            var changed = field switch
            {
                "ContextLen" => TinyConfig() with { ContextLen = 7 },
                "RopeTheta" => TinyConfig() with { RopeTheta = 20_000f },
                "RmsNormEps" => TinyConfig() with { RmsNormEps = 2e-5f },
                _ => throw new InvalidOperationException("Unknown test field.")
            };
            var destination = new LlmModel(changed);
            FillSequential(destination.BaseParameters, 30_000);
            var before = SnapshotBits(destination.Parameters());

            var exception = Assert.ThrowsAny<Exception>(() => destination.LoadCheckpoint(path));

            Assert.StartsWith("LLM_CHECKPOINT_CONFIG_MISMATCH:", exception.Message);
            AssertBitsEqual(before, destination.Parameters());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("same-shaped-substitution")]
    [InlineData("duplicate")]
    [InlineData("missing")]
    [InlineData("unknown")]
    public void VersionTwoRejectsInvalidSemanticKeySetsBeforeMutation(string caseName)
    {
        var root = CreateRoot();
        try
        {
            var source = new LlmModel(TinyConfig());
            FillSequential(source.BaseParameters);
            var canonicalPath = Path.Combine(root, "canonical.bin");
            source.SaveCheckpoint(canonicalPath);
            var artifact = ReadVersionTwo(canonicalPath);
            var entries = CloneEntries(artifact.Entries).ToList();
            switch (caseName)
            {
                case "same-shaped-substitution":
                    entries[2] = entries[2] with
                    {
                        Key = "base.block_0.attn.x_proj.weight"
                    };
                    break;
                case "duplicate":
                    entries[3] = entries[3] with { Key = entries[2].Key };
                    break;
                case "missing":
                    entries.RemoveAt(entries.Count - 1);
                    break;
                case "unknown":
                    entries[^1] = entries[^1] with { Key = "base.unknown.weight" };
                    break;
            }

            var malformedPath = Path.Combine(root, caseName + ".bin");
            WriteVersionTwo(
                malformedPath,
                artifact.Kind,
                artifact.Fingerprint,
                entries);
            var destination = new LlmModel(TinyConfig());
            FillSequential(destination.BaseParameters, 40_000);
            var before = SnapshotBits(destination.Parameters());

            var exception = Assert.ThrowsAny<Exception>(() => destination.LoadCheckpoint(malformedPath));

            Assert.Contains(
                caseName == "missing"
                    ? "LLM_CHECKPOINT_PARAMETER_COUNT_MISMATCH"
                    : "LLM_CHECKPOINT_PARAMETER_KEY_MISMATCH",
                exception.Message,
                StringComparison.Ordinal);
            AssertBitsEqual(before, destination.Parameters());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("fingerprint-length", "LLM_CHECKPOINT_MALFORMED")]
    [InlineData("invalid-kind", "LLM_CHECKPOINT_MALFORMED")]
    [InlineData("trailing", "LLM_CHECKPOINT_MALFORMED")]
    public void VersionTwoMalformedHeadersAndTailRejectBeforeMutation(
        string caseName,
        string expectedCode)
    {
        var root = CreateRoot();
        try
        {
            var source = new LlmModel(TinyConfig());
            var canonicalPath = Path.Combine(root, "canonical.bin");
            source.SaveCheckpoint(canonicalPath);
            var artifact = ReadVersionTwo(canonicalPath);
            var malformedPath = Path.Combine(root, caseName + ".bin");
            if (caseName == "fingerprint-length")
            {
                WriteVersionTwo(
                    malformedPath,
                    artifact.Kind,
                    artifact.Fingerprint[..31],
                    artifact.Entries);
            }
            else if (caseName == "invalid-kind")
            {
                WriteVersionTwo(
                    malformedPath,
                    kind: 99,
                    artifact.Fingerprint,
                    artifact.Entries);
            }
            else
            {
                File.WriteAllBytes(
                    malformedPath,
                    File.ReadAllBytes(canonicalPath).Append((byte)0x5a).ToArray());
            }

            var destination = new LlmModel(TinyConfig());
            FillSequential(destination.BaseParameters, 50_000);
            var before = SnapshotBits(destination.Parameters());
            var exception = Assert.ThrowsAny<Exception>(() => destination.LoadCheckpoint(malformedPath));

            Assert.StartsWith(expectedCode + ":", exception.Message);
            AssertBitsEqual(before, destination.Parameters());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void VersionTwoFullAndAdapterArtifactsCannotCrossLoad()
    {
        var root = CreateRoot();
        try
        {
            var source = CreateAttachedModel();
            var fullPath = Path.Combine(root, "full.bin");
            var adapterPath = Path.Combine(root, "adapter.bin");
            source.SaveCheckpoint(fullPath);
            source.SaveAdapterCheckpoint(adapterPath);
            var destination = CreateAttachedModel();
            var before = SnapshotBits(destination.Parameters());

            var adapterException = Assert.ThrowsAny<Exception>(
                () => destination.LoadAdapterCheckpoint(fullPath));
            Assert.StartsWith(
                "LLM_CHECKPOINT_ARTIFACT_KIND_MISMATCH:",
                adapterException.Message);
            AssertBitsEqual(before, destination.Parameters());

            var fullException = Assert.ThrowsAny<Exception>(
                () => destination.LoadCheckpoint(adapterPath));
            Assert.StartsWith(
                "LLM_CHECKPOINT_ARTIFACT_KIND_MISMATCH:",
                fullException.Message);
            AssertBitsEqual(before, destination.Parameters());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static ModelConfig TinyConfig()
        => new()
        {
            VocabSize = 16,
            ContextLen = 4,
            HiddenDim = 4,
            NumLayers = 1,
            NumHeads = 1,
            NumKvHeads = 1,
            FfnHiddenDim = 8,
            RopeTheta = 10_000f,
            RmsNormEps = 1e-5f
        };

    private static LlmModel CreateAttachedModel()
    {
        var model = new LlmModel(TinyConfig());
        _ = LoraApplicator.Apply(
            model,
            new LoraConfig
            {
                Rank = 2,
                Alpha = 4,
                Dropout = 0,
                TargetModules = ["q_proj", "v_proj"]
            });
        return model;
    }

    private static RawEntry[] CreateVersionOneFullEntries(LlmModel model)
        => model.GetCheckpointParameterEntries()
            .Select((entry, index) => ToRawEntry(
                entry,
                index < model.BaseParameters.Count
                    ? $"base.{index:D6}"
                    : entry.Key))
            .ToArray();

    private static RawEntry ToRawEntry(LlmCheckpointParameterEntry entry)
        => ToRawEntry(entry, entry.Key);

    private static RawEntry ToRawEntry(
        LlmCheckpointParameterEntry entry,
        string key)
        => new(
            key,
            entry.Parameter.Value.Shape.Dimensions.ToArray(),
            entry.Parameter.Value.Data.ToArray());

    private static VersionTwoArtifact ReadVersionTwo(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        Assert.Equal(CheckpointMagic, reader.ReadInt32());
        Assert.Equal(VersionTwo, reader.ReadInt32());
        var kind = reader.ReadInt32();
        var fingerprintLength = reader.ReadInt32();
        var fingerprint = reader.ReadBytes(fingerprintLength);
        Assert.Equal(fingerprintLength, fingerprint.Length);
        var entries = ReadEntries(reader);
        Assert.Equal(stream.Length, stream.Position);
        return new VersionTwoArtifact(kind, fingerprint, entries);
    }

    private static MemoryStream CreateVersionTwoAdapterStream(
        IReadOnlyList<RawEntry> entries)
    {
        var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(CheckpointMagic);
            writer.Write(VersionTwo);
            writer.Write(AdapterArtifact);
            writer.Write(32);
            writer.Write(new byte[32]);
            WriteEntries(writer, entries);
        }
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreateEntryCountOnlyStream(int version, int count)
    {
        var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(CheckpointMagic);
            writer.Write(version);
            if (version == VersionTwo)
            {
                writer.Write(AdapterArtifact);
                writer.Write(32);
                writer.Write(new byte[32]);
            }
            writer.Write(count);
        }
        stream.Position = 0;
        return stream;
    }

    private static byte[] CreateAdapterRecordPrefix(
        int version,
        string key,
        int elementCount)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(CheckpointMagic);
            writer.Write(version);
            if (version == VersionTwo)
            {
                writer.Write(AdapterArtifact);
                writer.Write(32);
                writer.Write(new byte[32]);
            }
            writer.Write(1);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            writer.Write(keyBytes.Length);
            writer.Write(keyBytes);
            writer.Write(1);
            writer.Write(elementCount);
            writer.Write(checked((long)elementCount * sizeof(float)));
        }
        return stream.ToArray();
    }

    private static RawEntry[] ReadEntries(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        var entries = new RawEntry[count];
        for (var entryIndex = 0; entryIndex < count; entryIndex++)
        {
            var keyLength = reader.ReadInt32();
            var key = Encoding.UTF8.GetString(reader.ReadBytes(keyLength));
            var rank = reader.ReadInt32();
            var dimensions = new int[rank];
            for (var dimensionIndex = 0; dimensionIndex < rank; dimensionIndex++)
                dimensions[dimensionIndex] = reader.ReadInt32();
            var payloadBytes = reader.ReadInt64();
            var values = new float[checked((int)(payloadBytes / sizeof(float)))];
            for (var valueIndex = 0; valueIndex < values.Length; valueIndex++)
                values[valueIndex] = reader.ReadSingle();
            entries[entryIndex] = new RawEntry(key, dimensions, values);
        }

        return entries;
    }

    private static void WriteVersionTwo(
        string path,
        int kind,
        byte[] fingerprint,
        IReadOnlyList<RawEntry> entries)
    {
        using var writer = new BinaryWriter(File.Create(path), Encoding.UTF8, leaveOpen: false);
        writer.Write(CheckpointMagic);
        writer.Write(VersionTwo);
        writer.Write(kind);
        writer.Write(fingerprint.Length);
        writer.Write(fingerprint);
        WriteEntries(writer, entries);
    }

    private static void WriteVersionOne(
        string path,
        IReadOnlyList<RawEntry> entries)
    {
        using var writer = new BinaryWriter(File.Create(path), Encoding.UTF8, leaveOpen: false);
        writer.Write(CheckpointMagic);
        writer.Write(1);
        WriteEntries(writer, entries);
    }

    private static void WriteEntries(
        BinaryWriter writer,
        IReadOnlyList<RawEntry> entries)
    {
        writer.Write(entries.Count);
        foreach (var entry in entries)
        {
            var keyBytes = Encoding.UTF8.GetBytes(entry.Key);
            writer.Write(keyBytes.Length);
            writer.Write(keyBytes);
            writer.Write(entry.Dimensions.Length);
            foreach (var dimension in entry.Dimensions)
                writer.Write(dimension);
            writer.Write(checked((long)entry.Values.Length * sizeof(float)));
            foreach (var value in entry.Values)
                writer.Write(value);
        }
    }

    private static void WriteLegacy(
        string path,
        IReadOnlyList<Parameter> parameters)
    {
        using var writer = new BinaryWriter(File.Create(path));
        writer.Write(parameters.Count);
        foreach (var parameter in parameters)
        {
            writer.Write(parameter.Value.Shape.Rank);
            foreach (var dimension in parameter.Value.Shape.Dimensions)
                writer.Write(dimension);
            foreach (var value in parameter.Value.Data)
                writer.Write(value);
        }
    }

    private static RawEntry[] CloneEntries(IEnumerable<RawEntry> entries)
        => entries.Select(static entry => entry with
        {
            Dimensions = entry.Dimensions.ToArray(),
            Values = entry.Values.ToArray()
        }).ToArray();

    private static int[][] SnapshotBits(IEnumerable<Parameter> parameters)
        => parameters.Select(static parameter => parameter.Value.Data
            .Select(BitConverter.SingleToInt32Bits)
            .ToArray())
            .ToArray();

    private static void AssertBitsEqual(
        IReadOnlyList<int[]> expected,
        IEnumerable<Parameter> actual)
    {
        var actualBits = SnapshotBits(actual);
        Assert.Equal(expected.Count, actualBits.Length);
        for (var index = 0; index < expected.Count; index++)
            Assert.Equal(expected[index], actualBits[index]);
    }

    private static void AssertParameterValues(
        IEnumerable<Parameter> expected,
        IEnumerable<Parameter> actual)
    {
        var expectedArray = expected.ToArray();
        var actualArray = actual.ToArray();
        Assert.Equal(expectedArray.Length, actualArray.Length);
        for (var index = 0; index < expectedArray.Length; index++)
            Assert.Equal(expectedArray[index].Value.Data, actualArray[index].Value.Data);
    }

    private static void FillSequential(
        IEnumerable<Parameter> parameters,
        int start = 1)
    {
        var value = start;
        foreach (var parameter in parameters)
        {
            for (var index = 0; index < parameter.Value.Data.Length; index++)
                parameter.Value.Data[index] = value++;
        }
    }

    private static string ComputeVersionOneInitialWeightSha256(LlmModel model)
    {
        var publicEntries = model.GetCheckpointParameterEntries();
        var entries = model.BaseParameters
            .Select((parameter, index) => new LlmCheckpointParameterEntry(
                $"base.{index:D6}",
                parameter))
            .Concat(publicEntries.Skip(model.BaseParameters.Count))
            .ToArray();
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendIdentityString(hash, "design-conversion-dpo-initial-weights-v1");
        AppendIdentityInt32(hash, entries.Length);
        foreach (var entry in entries)
        {
            AppendIdentityString(hash, entry.Key);
            AppendIdentityInt32(hash, entry.Parameter.Value.Shape.Rank);
            foreach (var dimension in entry.Parameter.Value.Shape.Dimensions)
                AppendIdentityInt32(hash, dimension);
            AppendIdentityInt32(hash, entry.Parameter.Value.Data.Length);
            foreach (var value in entry.Parameter.Value.Data)
                AppendIdentityInt32(hash, BitConverter.SingleToInt32Bits(value));
        }
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void AppendIdentityString(IncrementalHash hash, string value)
    {
        var bytes = new UTF8Encoding(false, true).GetBytes(value);
        AppendIdentityInt32(hash, bytes.Length);
        hash.AppendData(bytes);
    }

    private static void AppendIdentityInt32(IncrementalHash hash, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        hash.AppendData(bytes);
    }

    private static string CreateRoot()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"checkpoint-v2-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return root;
    }

    private sealed record RawEntry(
        string Key,
        int[] Dimensions,
        float[] Values);

    private sealed record VersionTwoArtifact(
        int Kind,
        byte[] Fingerprint,
        RawEntry[] Entries);

    private sealed class NonSeekableWritableStream : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
        }
    }

    private sealed class SparsePayloadStream : Stream
    {
        private readonly byte[] _prefix;
        private readonly long _length;
        private long _position;

        public SparsePayloadStream(byte[] prefix, long payloadLength)
        {
            _prefix = prefix;
            _length = checked(prefix.Length + payloadLength);
        }

        public int PayloadReadAttempts { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _length;
        public override long Position
        {
            get => _position;
            set
            {
                if (value < 0 || value > _length)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
            => Read(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> buffer)
        {
            if (_position >= _prefix.Length)
            {
                PayloadReadAttempts++;
                throw new InvalidOperationException("TEST_PAYLOAD_READ_ATTEMPT");
            }

            var available = checked((int)Math.Min(_prefix.Length - _position, buffer.Length));
            _prefix.AsSpan(checked((int)_position), available).CopyTo(buffer);
            _position += available;
            return available;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var target = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => checked(_position + offset),
                SeekOrigin.End => checked(_length + offset),
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };
            Position = target;
            return _position;
        }

        public override void Flush()
        {
        }

        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
