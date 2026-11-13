// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using ContinuousLearning;
using LoRA;
using TensorCore.Core;
using TensorCore.Init;
using TensorCore.Layers;
using TensorCore.Randomness;
using TransformerModel;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

public sealed class LoraRandomSeedTests
{
    [Fact]
    public void SplitMix64_SeedZeroMatchesGoldenWords()
    {
        var random = new SplitMix64Random(0);

        Assert.Equal(
            [
                0xE220A8397B1DCDAFUL,
                0x6E789E6AA1B965F4UL,
                0x06C45D188009454FUL,
                0xF88BB8A8724C81ECUL,
            ],
            Enumerable.Range(0, 4).Select(_ => random.NextUInt64()).ToArray());
    }

    [Fact]
    public void SplitMix64_NextUnitDoubleUsesExact53BitFormulaAndRange()
    {
        var words = new SplitMix64Random(123456789UL);
        var units = new SplitMix64Random(123456789UL);

        for (var index = 0; index < 128; index++)
        {
            var expected = (words.NextUInt64() >> 11)
                           * (1.0 / 9007199254740992.0);
            var actual = units.NextUnitDouble();
            Assert.Equal(expected, actual);
            Assert.InRange(actual, 0.0, Math.BitDecrement(1.0));
        }
    }

    [Fact]
    public void SeededKaiming_IsRepeatableOrderIndependentAndSeedSensitive()
    {
        var first = Initializers.KaimingUniform(3, 7, 11UL);
        _ = Initializers.KaimingUniform(9, 2);
        _ = Initializers.KaimingUniform(3, 7, 999UL);
        var repeated = Initializers.KaimingUniform(3, 7, 11UL);
        var different = Initializers.KaimingUniform(3, 7, 12UL);

        Assert.Equal(first, repeated);
        Assert.False(first.SequenceEqual(different));
    }

    [Fact]
    public void SeededDropout_RepeatsMaskAndLegacyIntSeedRemainsCallable()
    {
        var input = new Tensor(
            Enumerable.Repeat(1f, 64).ToArray(),
            new TensorShape(4, 16));
        var first = new Dropout(0.5f, 77UL).Forward(input, training: true);
        var repeated = new Dropout(0.5f, 77UL).Forward(input, training: true);
        var different = new Dropout(0.5f, 78UL).Forward(input, training: true);
        var legacyFirst = new Dropout(0.5f, seed: 7)
            .Forward(input, training: true);
        var legacyRepeated = new Dropout(0.5f, seed: 7)
            .Forward(input, training: true);

        Assert.Equal(first.Data, repeated.Data);
        Assert.False(first.Data.SequenceEqual(different.Data));
        Assert.Equal(legacyFirst.Data, legacyRepeated.Data);
    }

    [Fact]
    public void CaptureRestoreReplaysNextDropoutMask()
    {
        var adapter = new LoraLinear(
            new Linear(4, 4),
            Config(["q_proj"], dropout: 0.5f),
            new LoraAdapterRandomSeeds(101UL, 202UL),
            "policy.step");
        Array.Fill(adapter.AdapterParameters[1].Parameter.Value.Data, 1f);
        var input = new Tensor(
            Enumerable.Repeat(1f, 16).ToArray(),
            new TensorShape(4, 4));

        _ = adapter.Forward(input, training: true);
        var captured = adapter.CaptureRandomState();
        var next = adapter.Forward(input, training: true).Data;

        adapter.RestoreRandomState(captured);
        var replayed = adapter.Forward(input, training: true).Data;

        Assert.Equal(next, replayed);
        Assert.Equal(captured.ConsumedCursor + (ulong)input.Shape.TotalElements,
            adapter.CaptureRandomState().ConsumedCursor);
    }

    [Fact]
    public void InferenceForwardDoesNotAdvanceCursor()
    {
        var adapter = new LoraLinear(
            new Linear(4, 4),
            Config(["q_proj"], dropout: 0.5f),
            new LoraAdapterRandomSeeds(303UL, 404UL),
            "policy.step");
        var input = new Tensor(
            Enumerable.Repeat(1f, 16).ToArray(),
            new TensorShape(4, 4));
        var before = adapter.CaptureRandomState();

        _ = adapter.Forward(input, training: false);

        Assert.Equal(before, adapter.CaptureRandomState());
    }

    [Fact]
    public void RandomStateReaderRejectsWrongSeedAlgorithmOrSlot()
    {
        var snapshot = RandomSnapshot();
        var bytes = WriteRandomStateBytes(snapshot);
        var algorithmOffset = 4 + sizeof(uint) + sizeof(uint);
        bytes[algorithmOffset] = (byte)'X';

        var path = Path.Combine(
            Path.GetTempPath(),
            $"random-state-invalid-{Guid.NewGuid():N}",
            RepositoryOwnedDpoRandomStateFile.BuildFileName(snapshot.RandomStateStep));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        try
        {
            File.WriteAllBytes(path, bytes);
            var result = RepositoryOwnedDpoRandomStateFile.OpenAndValidate(
                path,
                Sha256(bytes),
                bytes.LongLength,
                snapshot.RandomStateStep,
                snapshot.TopologySha256,
                snapshot.LoraConfigurationSha256);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                RepositoryOwnedDpoRandomStateFile.InvalidErrorCode,
                result.ErrorCode);
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(path)!, recursive: true);
        }
    }

    [Fact]
    public void RandomStateReaderRejectsDuplicateTrailingAndOutOfBoundsFrames()
    {
        var snapshot = RandomSnapshot();
        var valid = WriteRandomStateBytes(snapshot);
        var tempRoot = Path.Combine(
            Path.GetTempPath(), $"random-state-frames-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        try
        {
            var trailing = valid.Append((byte)0x7f).ToArray();
            AssertRandomBytesRejected(tempRoot, snapshot, trailing, "trailing");

            var duplicate = valid[..169].Concat(valid[169..]).Concat(valid[169..])
                .ToArray();
            BinaryPrimitives.WriteUInt32LittleEndian(
                duplicate.AsSpan(165, sizeof(uint)), 2);
            AssertRandomBytesRejected(tempRoot, snapshot, duplicate, "duplicate");

            var oversizedKey = valid.ToArray();
            BinaryPrimitives.WriteUInt32LittleEndian(
                oversizedKey.AsSpan(169, sizeof(uint)), 257);
            AssertRandomBytesRejected(tempRoot, snapshot, oversizedKey, "bounds");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void RandomStateReaderRejectsDuplicateAdapterKeyAcrossDomains()
    {
        var snapshot = TwoEntryRandomSnapshot();
        var valid = WriteRandomStateBytes(snapshot);
        var duplicateKey = RewriteSecondEntryKey(valid, snapshot.Entries[0].AdapterKey);
        var tempRoot = Path.Combine(
            Path.GetTempPath(), $"random-state-duplicate-key-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        try
        {
            AssertRandomBytesRejected(
                tempRoot,
                snapshot,
                duplicateKey,
                "duplicate-adapter-key");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void RandomStateReaderRejectsOutOfOrderCanonicalEntries()
    {
        var snapshot = TwoEntryRandomSnapshot();
        var valid = WriteRandomStateBytes(snapshot);
        var outOfOrder = SwapSerializedEntries(valid);
        var tempRoot = Path.Combine(
            Path.GetTempPath(), $"random-state-order-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        try
        {
            AssertRandomBytesRejected(
                tempRoot,
                snapshot,
                outOfOrder,
                "out-of-order");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Fact]
    public void LlmModelRestoreRandomStateRejectsIdentityMismatchWithoutMutation()
    {
        var model = new LlmModel(TinyConfig());
        var config = Config(["q_proj"], dropout: 0.5f);
        var authority = new LoraPairRandomSeedAuthority(
            707UL,
            "policy.step",
            "reference.step");
        _ = LoraApplicator.Apply(
            model,
            config,
            (block, projection) => LoraAdapterRandomSeedDerivation.Derive(
                authority.RootInitializationSeed,
                block,
                projection,
                authority.PolicyDropoutStreamDomain));
        var expectedIdentity = new LlmModelRandomStateIdentity(
            LoraRandomStateIdentity.ComputeTopologySha256(
                model.Config,
                config,
                authority),
            LoraRandomStateIdentity.ComputeLoraConfigurationSha256(
                config,
                authority));
        var captured = model.CaptureRandomState(
            expectedIdentity.TopologySha256,
            expectedIdentity.LoraConfigurationSha256,
            randomStateStep: 7);
        var input = new Tensor(
            Enumerable.Repeat(1f, 16).ToArray(),
            new TensorShape(4, 4));
        _ = model.GetAttachedProjectionAdapter(0, "q_proj")!
            .Forward(input, training: true);
        var before = model.CaptureRandomState(
            expectedIdentity.TopologySha256,
            expectedIdentity.LoraConfigurationSha256,
            randomStateStep: 7);
        var invalid = captured with
        {
            TopologySha256 = new string('c', 64)
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            model.RestoreRandomState(invalid, expectedIdentity));

        Assert.Contains(
            "DPO_RANDOM_STATE_IDENTITY_MISMATCH",
            exception.Message,
            StringComparison.Ordinal);
        var after = model.CaptureRandomState(
            expectedIdentity.TopologySha256,
            expectedIdentity.LoraConfigurationSha256,
            randomStateStep: 7);
        Assert.Equal(before.FormatVersion, after.FormatVersion);
        Assert.Equal(before.Algorithm, after.Algorithm);
        Assert.Equal(before.TopologySha256, after.TopologySha256);
        Assert.Equal(
            before.LoraConfigurationSha256,
            after.LoraConfigurationSha256);
        Assert.Equal(before.RandomStateStep, after.RandomStateStep);
        Assert.Equal(before.Entries.Count, after.Entries.Count);
        for (var index = 0; index < before.Entries.Count; index++)
        {
            var expectedEntry = before.Entries[index];
            var actualEntry = after.Entries[index];
            Assert.Equal(expectedEntry.AdapterKey, actualEntry.AdapterKey);
            Assert.Equal(expectedEntry.StreamDomain, actualEntry.StreamDomain);
            Assert.Equal(
                expectedEntry.InitializationSeed,
                actualEntry.InitializationSeed);
            Assert.Equal(expectedEntry.DropoutSeed, actualEntry.DropoutSeed);
            Assert.Equal(expectedEntry.ConsumedCursor, actualEntry.ConsumedCursor);
        }
    }

    [Fact]
    public void LlmModelRestoreRandomStateLateAdapterMismatchPreservesEveryCursor()
    {
        var model = new LlmModel(TinyConfig());
        var config = Config(["q_proj", "k_proj"], dropout: 0.5f);
        var authority = new LoraPairRandomSeedAuthority(
            808UL,
            "policy.step",
            "reference.step");
        _ = LoraApplicator.Apply(
            model,
            config,
            (block, projection) => LoraAdapterRandomSeedDerivation.Derive(
                authority.RootInitializationSeed,
                block,
                projection,
                authority.PolicyDropoutStreamDomain));
        var identity = new LlmModelRandomStateIdentity(
            LoraRandomStateIdentity.ComputeTopologySha256(
                model.Config,
                config,
                authority),
            LoraRandomStateIdentity.ComputeLoraConfigurationSha256(
                config,
                authority));
        var initial = model.CaptureRandomState(
            identity.TopologySha256,
            identity.LoraConfigurationSha256,
            randomStateStep: 7);
        Assert.Equal(2, initial.Entries.Count);
        var validEntries = initial.Entries
            .Select((entry, index) => entry with
            {
                ConsumedCursor = (ulong)((index + 1) * 5)
            })
            .ToArray();
        var valid = initial with { Entries = validEntries };
        var invalid = valid with
        {
            Entries =
            [
                validEntries[0],
                validEntries[1] with { StreamDomain = "late.invalid" }
            ]
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            model.RestoreRandomState(invalid, identity));

        Assert.Contains(
            "DPO_RANDOM_STATE_DOMAIN_MISMATCH",
            exception.Message,
            StringComparison.Ordinal);
        var afterRejected = model.CaptureRandomState(
            identity.TopologySha256,
            identity.LoraConfigurationSha256,
            randomStateStep: 7);
        Assert.Equal(
            initial.Entries.Select(entry => entry.ConsumedCursor),
            afterRejected.Entries.Select(entry => entry.ConsumedCursor));

        model.RestoreRandomState(valid, identity);
        var afterValid = model.CaptureRandomState(
            identity.TopologySha256,
            identity.LoraConfigurationSha256,
            randomStateStep: 7);
        Assert.Equal(valid.Entries.ToArray(), afterValid.Entries.ToArray());
    }

    [Fact]
    public void RandomStateCreateNewFailureAfterOpenLeavesNoOwnedArtifactAndPreservesPreExistingTarget()
    {
        var snapshot = RandomSnapshot();
        var root = Path.Combine(
            Path.GetTempPath(), $"random-state-create-new-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var path = Path.Combine(
            root,
            RepositoryOwnedDpoRandomStateFile.BuildFileName(
                snapshot.RandomStateStep));
        var priorHook = RepositoryOwnedDpoRandomStateFile.AfterCreateNewForTesting;
        try
        {
            RepositoryOwnedDpoRandomStateFile.AfterCreateNewForTesting = _ =>
                throw new IOException("injected post-open random-state failure");
            var failure = RepositoryOwnedDpoRandomStateFile.WriteCreateNew(
                path,
                snapshot);

            Assert.False(failure.IsSuccess);
            Assert.Null(failure.Value);
            Assert.False(File.Exists(path));

            var sentinel = Encoding.UTF8.GetBytes("pre-existing-target");
            File.WriteAllBytes(path, sentinel);
            var existingTarget = RepositoryOwnedDpoRandomStateFile.WriteCreateNew(
                path,
                snapshot);

            Assert.False(existingTarget.IsSuccess);
            Assert.Equal(sentinel, File.ReadAllBytes(path));
        }
        finally
        {
            RepositoryOwnedDpoRandomStateFile.AfterCreateNewForTesting = priorHook;
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void RandomStateCleanupFailureIsExplicitAndLeavesTargetUncreated()
    {
        var snapshot = RandomSnapshot();
        var root = Path.Combine(
            Path.GetTempPath(), $"random-state-cleanup-failure-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var path = Path.Combine(
            root,
            RepositoryOwnedDpoRandomStateFile.BuildFileName(
                snapshot.RandomStateStep));
        var priorWriteHook = RepositoryOwnedDpoRandomStateFile.AfterCreateNewForTesting;
        var priorDeleteHook =
            RepositoryOwnedDpoRandomStateFile.BeforeDeleteUnpublishedForTesting;
        try
        {
            RepositoryOwnedDpoRandomStateFile.AfterCreateNewForTesting = _ =>
                throw new IOException("injected write failure");
            RepositoryOwnedDpoRandomStateFile.BeforeDeleteUnpublishedForTesting = _ =>
                throw new IOException("injected delete failure");

            var failure = RepositoryOwnedDpoRandomStateFile.WriteCreateNew(
                path,
                snapshot);

            Assert.False(failure.IsSuccess);
            Assert.Null(failure.Value);
            Assert.Equal(
                RepositoryOwnedDpoRandomStateFile.CleanupFailedErrorCode,
                failure.ErrorCode);
            Assert.Contains("injected delete failure", failure.ErrorMessage);
            Assert.Contains("No OwnershipProof was issued", failure.ErrorMessage);
            Assert.False(
                File.Exists(path));

            var unpublished = Directory.GetFiles(root);
            Assert.Single(unpublished);
            Assert.Contains(
                Path.GetFileName(unpublished[0]),
                failure.ErrorMessage,
                StringComparison.Ordinal);

            var sentinel = Encoding.UTF8.GetBytes("pre-existing-target");
            File.WriteAllBytes(path, sentinel);
            var existingTarget = RepositoryOwnedDpoRandomStateFile.WriteCreateNew(
                path,
                snapshot);

            Assert.False(existingTarget.IsSuccess);
            Assert.Equal(
                RepositoryOwnedDpoRandomStateFile.UnavailableErrorCode,
                existingTarget.ErrorCode);
            Assert.Null(existingTarget.Value);
            Assert.Equal(sentinel, File.ReadAllBytes(path));
        }
        finally
        {
            RepositoryOwnedDpoRandomStateFile.AfterCreateNewForTesting = priorWriteHook;
            RepositoryOwnedDpoRandomStateFile.BeforeDeleteUnpublishedForTesting =
                priorDeleteHook;
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void DropoutContinuationRejectsLegacySystemRandomState()
    {
        var adapter = new LoraLinear(
            new Linear(4, 4),
            Config(["q_proj"], dropout: 0.5f));

        var exception = Assert.Throws<InvalidOperationException>(
            () => adapter.CaptureRandomState());

        Assert.Contains(
            "DPO_RANDOM_STATE_NONDETERMINISTIC_ADAPTER",
            exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void SeedDerivation_MatchesHardcodedGoldenAndSeparatesSlots()
    {
        var golden = LoraAdapterRandomSeedDerivation.Derive(
            0UL,
            0,
            "q_proj",
            "training.step");

        Assert.Equal(6618547244736900535UL, golden.InitializationSeed);
        Assert.Equal(9770923377924943182UL, golden.DropoutSeed);
        Assert.NotEqual(
            golden,
            LoraAdapterRandomSeedDerivation.Derive(
                0UL,
                0,
                "k_proj",
                "training.step"));
        Assert.NotEqual(
            golden,
            LoraAdapterRandomSeedDerivation.Derive(
                0UL,
                1,
                "q_proj",
                "training.step"));
        Assert.Contains(
            "LORA_BLOCK_INDEX_INVALID",
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                LoraAdapterRandomSeedDerivation.Derive(
                    0, -1, "q_proj", "training.step")).Message);
        Assert.Contains(
            "LORA_PROJECTION_INVALID",
            Assert.Throws<ArgumentException>(() =>
                LoraAdapterRandomSeedDerivation.Derive(
                    0, 0, "Q_PROJ", "training.step")).Message);
        Assert.Contains(
            "LORA_DROPOUT_STREAM_DOMAIN_INVALID",
            Assert.Throws<ArgumentException>(() =>
                LoraAdapterRandomSeedDerivation.Derive(
                    0, 0, "q_proj", "Training_step")).Message);
    }

    [Fact]
    public void DropoutRoleDomain_ChangesOnlyDropoutSeed()
    {
        var training = LoraAdapterRandomSeedDerivation.Derive(
            91UL, 3, "v_proj", "training.step");
        var evaluation = LoraAdapterRandomSeedDerivation.Derive(
            91UL, 3, "v_proj", "evaluation.step");

        Assert.Equal(training.InitializationSeed, evaluation.InitializationSeed);
        Assert.NotEqual(training.DropoutSeed, evaluation.DropoutSeed);
    }

    [Fact]
    public void SeededLoraLinear_RepeatsInitializationAndDropoutButDifferentSeedsSeparate()
    {
        var projection = new Linear(16, 3);
        var config = Config(["q_proj"], dropout: 0.5f);
        var legacy = new LoraLinear(projection, config);
        var firstSeeds = new LoraAdapterRandomSeeds(100, 200);
        var first = new LoraLinear(
            projection,
            config,
            firstSeeds);
        var repeated = new LoraLinear(
            projection,
            config,
            new LoraAdapterRandomSeeds(100, 200));
        var differentInitialization = new LoraLinear(
            projection,
            config,
            new LoraAdapterRandomSeeds(101, 200));
        var differentDropout = new LoraLinear(
            projection,
            config,
            new LoraAdapterRandomSeeds(100, 201));
        foreach (var adapter in new[]
                 { first, repeated, differentInitialization, differentDropout })
        {
            Assert.All(
                adapter.AdapterParameters[1].Parameter.Value.Data,
                value => Assert.Equal(0f, value));
            Array.Fill(adapter.AdapterParameters[1].Parameter.Value.Data, 1f);
        }
        var input = new Tensor(
            Enumerable.Repeat(1f, 32).ToArray(),
            new TensorShape(2, 16));

        Assert.Equal(AdapterA(first), AdapterA(repeated));
        Assert.Equal(AdapterA(first), AdapterA(differentDropout));
        Assert.Null(legacy.RandomSeeds);
        Assert.Equal(firstSeeds, first.RandomSeeds);
        var changedCopy = firstSeeds with { DropoutSeed = 999 };
        Assert.NotEqual(changedCopy, first.RandomSeeds);
        Assert.Equal(firstSeeds, first.RandomSeeds);
        Assert.False(AdapterA(first).SequenceEqual(
            AdapterA(differentInitialization)));
        var firstOutput = first.Forward(input, training: true).Data;
        var repeatedOutput = repeated.Forward(input, training: true).Data;
        var differentDropoutOutput = differentDropout
            .Forward(input, training: true).Data;
        Assert.Equal(firstOutput, repeatedOutput);
        Assert.False(firstOutput.SequenceEqual(differentDropoutOutput));
    }

    [Fact]
    public void SeededApplicator_CanonicalSlotsIgnoreRequestedTargetOrdering()
    {
        var firstModel = new LlmModel(TinyConfig(numLayers: 2));
        var secondModel = new LlmModel(TinyConfig(numLayers: 2));
        var firstCalls = new List<string>();
        var secondCalls = new List<string>();
        LoraAdapterRandomSeeds Resolve(
            List<string> calls,
            int block,
            string projection)
        {
            calls.Add($"{block}:{projection}");
            return LoraAdapterRandomSeedDerivation.Derive(
                55UL, block, projection, "training.step");
        }

        var first = LoraApplicator.Apply(
            firstModel,
            Config(["v_proj", "q_proj"]),
            (block, projection) => Resolve(firstCalls, block, projection));
        var second = LoraApplicator.Apply(
            secondModel,
            Config(["q_proj", "v_proj", "q_proj"]),
            (block, projection) => Resolve(secondCalls, block, projection));

        Assert.Equal(
            ["0:q_proj", "0:v_proj", "1:q_proj", "1:v_proj"],
            firstCalls);
        Assert.Equal(firstCalls, secondCalls);
        Assert.Equal(first.Count, second.Count);
        Assert.All(
            first.Zip(second),
            pair => Assert.Equal(AdapterA(pair.First), AdapterA(pair.Second)));
    }

    [Fact]
    public void SeededApplicator_LateResolverFailureLeavesModelUnchangedAndUnfrozen()
    {
        var model = new LlmModel(TinyConfig());
        var before = model.Parameters().ToArray();
        var requiredGradBefore = before.Select(parameter =>
            parameter.Value.RequiresGrad).ToArray();
        var gradientsBefore = before.Select(parameter =>
            parameter.Value.Grad).ToArray();
        var calls = 0;

        var nullResolver = Assert.Throws<ArgumentNullException>(() =>
            LoraApplicator.Apply(
                model,
                Config(["q_proj"]),
                randomSeedResolver: null!));
        Assert.Contains(
            "LORA_RANDOM_SEED_RESOLVER_REQUIRED",
            nullResolver.Message,
            StringComparison.Ordinal);
        Assert.Equal(before, model.Parameters().ToArray());

        var exception = Assert.Throws<InvalidOperationException>(() =>
            LoraApplicator.Apply(
                model,
                Config(["q_proj", "k_proj", "v_proj", "o_proj"]),
                (block, projection) => ++calls == 3
                    ? throw new InvalidOperationException("resolver-failed-late")
                    : LoraAdapterRandomSeedDerivation.Derive(
                        8UL, block, projection, "training.step")));

        Assert.Equal("resolver-failed-late", exception.Message);
        Assert.Equal(3, calls);
        Assert.Equal(before, model.Parameters().ToArray());
        Assert.Equal(
            requiredGradBefore,
            model.Parameters().Select(parameter =>
                parameter.Value.RequiresGrad).ToArray());
        Assert.Equal(
            gradientsBefore,
            model.Parameters().Select(parameter =>
                parameter.Value.Grad).ToArray());
        Assert.All(
            new[] { "q_proj", "k_proj", "v_proj", "o_proj" },
            projection => Assert.Null(
                model.GetAttachedProjectionAdapter(0, projection)));
    }

    [Fact]
    public void SeededApplicator_RejectsReuseWithoutResolverCallOrMutation()
    {
        var model = new LlmModel(TinyConfig());
        _ = Assert.Single(LoraApplicator.Apply(
            model,
            Config(["q_proj"]),
            (block, projection) => LoraAdapterRandomSeedDerivation.Derive(
                7UL, block, projection, "training.step")));
        SetDistinctParameterState(model);
        var before = CaptureModel(model);
        var calls = 0;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            LoraApplicator.Apply(
                model,
                Config(["q_proj"]),
                (_, _) =>
                {
                    calls++;
                    throw new InvalidOperationException("must-not-run");
                }));

        Assert.Contains(
            LoraApplicator.SeededAdapterReuseUnprovenErrorCode,
            exception.Message,
            StringComparison.Ordinal);
        Assert.Equal(0, calls);
        AssertModelSnapshot(before, model);
    }

    [Fact]
    public void LegacyApplicator_TwoArgumentOverloadReusesAdapterIdentity()
    {
        var model = new LlmModel(TinyConfig());
        var first = Assert.Single(LoraApplicator.Apply(
            model,
            Config(["q_proj"])));
        var parameterCount = model.Parameters().Count();

        var second = Assert.Single(LoraApplicator.Apply(
            model,
            Config(["q_proj"])));

        Assert.Same(first, second);
        Assert.Equal(parameterCount, model.Parameters().Count());
        Assert.Null(second.RandomSeeds);
    }

    [Fact]
    public void ValidateSeededProvenance_SucceedsInCanonicalOrderOncePerSlotWithoutMutation()
    {
        var model = new LlmModel(TinyConfig(numLayers: 2));
        var requested = Config(["v_proj", "q_proj", "q_proj"]);
        var applied = LoraApplicator.Apply(
            model,
            requested,
            (block, projection) => LoraAdapterRandomSeedDerivation.Derive(
                71UL,
                block,
                projection,
                "training.step"));
        SetDistinctParameterState(model);
        var before = CaptureModel(model);
        var calls = new List<string>();

        var validated = LoraApplicator.ValidateSeededApplicationProvenance(
            model,
            requested,
            (block, projection) =>
            {
                calls.Add($"{block}:{projection}");
                return LoraAdapterRandomSeedDerivation.Derive(
                    71UL,
                    block,
                    projection,
                    "training.step");
            });

        Assert.Equal(
            ["0:q_proj", "0:v_proj", "1:q_proj", "1:v_proj"],
            calls);
        Assert.Equal(applied.Count, validated.Count);
        Assert.All(
            applied.Zip(validated),
            pair => Assert.Same(pair.First, pair.Second));
        AssertModelSnapshot(before, model);
    }

    [Fact]
    public void ValidateSeededProvenance_RejectsWrongAuthorityAndNullProvenanceWithoutMutation()
    {
        var seeded = new LlmModel(TinyConfig());
        _ = LoraApplicator.Apply(
            seeded,
            Config(["q_proj"]),
            (block, projection) => LoraAdapterRandomSeedDerivation.Derive(
                81UL,
                block,
                projection,
                "training.step"));
        SetDistinctParameterState(seeded);
        var seededBefore = CaptureModel(seeded);

        var wrongRoot = Assert.Throws<InvalidOperationException>(() =>
            LoraApplicator.ValidateSeededApplicationProvenance(
                seeded,
                Config(["q_proj"]),
                (block, projection) => LoraAdapterRandomSeedDerivation.Derive(
                    82UL,
                    block,
                    projection,
                    "training.step")));
        Assert.Contains(
            LoraApplicator.SeededAdapterProvenanceMismatchErrorCode,
            wrongRoot.Message,
            StringComparison.Ordinal);
        AssertModelSnapshot(seededBefore, seeded);

        var wrongDomain = Assert.Throws<InvalidOperationException>(() =>
            LoraApplicator.ValidateSeededApplicationProvenance(
                seeded,
                Config(["q_proj"]),
                (block, projection) => LoraAdapterRandomSeedDerivation.Derive(
                    81UL,
                    block,
                    projection,
                    "evaluation.step")));
        Assert.Contains(
            LoraApplicator.SeededAdapterProvenanceMismatchErrorCode,
            wrongDomain.Message,
            StringComparison.Ordinal);
        AssertModelSnapshot(seededBefore, seeded);

        var legacy = new LlmModel(TinyConfig());
        _ = LoraApplicator.Apply(legacy, Config(["q_proj"]));
        SetDistinctParameterState(legacy);
        var legacyBefore = CaptureModel(legacy);

        var nullProvenance = Assert.Throws<InvalidOperationException>(() =>
            LoraApplicator.ValidateSeededApplicationProvenance(
                legacy,
                Config(["q_proj"]),
                (block, projection) => LoraAdapterRandomSeedDerivation.Derive(
                    81UL,
                    block,
                    projection,
                    "training.step")));
        Assert.Contains(
            LoraApplicator.SeededAdapterProvenanceMismatchErrorCode,
            nullProvenance.Message,
            StringComparison.Ordinal);
        AssertModelSnapshot(legacyBefore, legacy);
    }

    [Fact]
    public void SeededPair_EmptyTargetsValidateBothDomainsWithoutMutation()
    {
        var first = new LlmModel(TinyConfig());
        var second = new LlmModel(TinyConfig());
        var firstBefore = CaptureModel(first);
        var secondBefore = CaptureModel(second);
        var empty = Config([]);

        var invalidFirst = Assert.Throws<ArgumentException>(() =>
            LoraApplicator.ApplyFreshSeededPair(
                first,
                second,
                empty,
                1UL,
                "Invalid_domain",
                "reference.step"));
        Assert.Contains(
            "LORA_DROPOUT_STREAM_DOMAIN_INVALID",
            invalidFirst.Message,
            StringComparison.Ordinal);
        AssertModelSnapshot(firstBefore, first);
        AssertModelSnapshot(secondBefore, second);

        var invalidSecond = Assert.Throws<ArgumentException>(() =>
            LoraApplicator.ApplyFreshSeededPair(
                first,
                second,
                empty,
                1UL,
                "policy.step",
                "Invalid_domain"));
        Assert.Contains(
            "LORA_DROPOUT_STREAM_DOMAIN_INVALID",
            invalidSecond.Message,
            StringComparison.Ordinal);
        AssertModelSnapshot(firstBefore, first);
        AssertModelSnapshot(secondBefore, second);

        var application = LoraApplicator.ApplyFreshSeededPair(
            first,
            second,
            empty,
            1UL,
            "policy.step",
            "reference.step");

        Assert.Empty(application.FirstModelAdapters);
        Assert.Empty(application.SecondModelAdapters);
        AssertModelSnapshot(firstBefore, first);
        AssertModelSnapshot(secondBefore, second);
        Assert.All(first.BaseParameters, parameter =>
            Assert.True(parameter.Value.RequiresGrad));
        Assert.All(second.BaseParameters, parameter =>
            Assert.True(parameter.Value.RequiresGrad));
    }

    [Fact]
    public void SeededTriple_EmptyTargetsReturnThreeEmptyListsWithoutMutation()
    {
        var policy = new LlmModel(TinyConfig(numLayers: 2));
        var reference = new LlmModel(TinyConfig(numLayers: 2));
        var freshLoad = new LlmModel(TinyConfig(numLayers: 2));
        SetDistinctParameterState(policy);
        SetDistinctParameterState(reference);
        SetDistinctParameterState(freshLoad);
        var policyBefore = CaptureModel(policy);
        var referenceBefore = CaptureModel(reference);
        var freshLoadBefore = CaptureModel(freshLoad);

        var application = LoraApplicator.ApplyFreshSeededTriple(
            policy,
            reference,
            freshLoad,
            Config([]),
            92UL,
            "policy.step",
            "reference.step",
            "fresh-load.step");

        Assert.Empty(application.PolicyModelAdapters);
        Assert.Empty(application.ReferenceModelAdapters);
        Assert.Empty(application.FreshLoadModelAdapters);
        AssertModelSnapshot(policyBefore, policy);
        AssertModelSnapshot(referenceBefore, reference);
        AssertModelSnapshot(freshLoadBefore, freshLoad);
    }

    [Theory]
    [InlineData(0, "")]
    [InlineData(0, "Invalid_domain")]
    [InlineData(1, "")]
    [InlineData(1, "Invalid_domain")]
    [InlineData(2, "")]
    [InlineData(2, "Invalid_domain")]
    public void SeededTriple_EmptyTargetsStillRejectEveryInvalidDomainWithoutMutation(
        int invalidDomainIndex,
        string invalidDomain)
    {
        var policy = new LlmModel(TinyConfig(numLayers: 2));
        var reference = new LlmModel(TinyConfig(numLayers: 2));
        var freshLoad = new LlmModel(TinyConfig(numLayers: 2));
        SetDistinctParameterState(policy);
        SetDistinctParameterState(reference);
        SetDistinctParameterState(freshLoad);
        var policyBefore = CaptureModel(policy);
        var referenceBefore = CaptureModel(reference);
        var freshLoadBefore = CaptureModel(freshLoad);
        var domains = new[]
        {
            "policy.step",
            "reference.step",
            "fresh-load.step"
        };
        domains[invalidDomainIndex] = invalidDomain;

        var exception = Assert.Throws<ArgumentException>(() =>
            LoraApplicator.ApplyFreshSeededTriple(
                policy,
                reference,
                freshLoad,
                Config([]),
                93UL,
                domains[0],
                domains[1],
                domains[2]));

        Assert.Contains(
            "LORA_DROPOUT_STREAM_DOMAIN_INVALID",
            exception.Message,
            StringComparison.Ordinal);
        AssertModelSnapshot(policyBefore, policy);
        AssertModelSnapshot(referenceBefore, reference);
        AssertModelSnapshot(freshLoadBefore, freshLoad);
    }

    [Fact]
    public void SeededPair_SecondModelLateOccupiedSlotLeavesFirstCompletelyFresh()
    {
        var first = new LlmModel(TinyConfig());
        var second = new LlmModel(TinyConfig());
        var occupied = Assert.Single(LoraApplicator.Apply(
            second,
            Config(["v_proj"])));
        var firstBefore = CaptureModel(first);
        var secondBefore = CaptureModel(second);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            LoraApplicator.ApplyFreshSeededPair(
                first,
                second,
                Config(["q_proj", "v_proj"]),
                2UL,
                "policy.step",
                "reference.step"));

        Assert.Contains(
            LoraApplicator.SeededAdapterReuseUnprovenErrorCode,
            exception.Message,
            StringComparison.Ordinal);
        AssertModelSnapshot(firstBefore, first);
        AssertModelSnapshot(secondBefore, second);
        Assert.All(first.BaseParameters, parameter =>
            Assert.True(parameter.Value.RequiresGrad));
        Assert.Null(first.GetAttachedProjectionAdapter(0, "q_proj"));
        Assert.Null(first.GetAttachedProjectionAdapter(0, "v_proj"));
        Assert.Same(occupied, second.GetAttachedProjectionAdapter(0, "v_proj"));
    }

    [Theory]
    [InlineData(nameof(LoraSeededTripleApplyPhase.BeforeReferencePlanConstruction))]
    [InlineData(nameof(LoraSeededTripleApplyPhase.BeforeFreshLoadPlanConstruction))]
    [InlineData(nameof(LoraSeededTripleApplyPhase.BeforeReferencePreflightReread))]
    [InlineData(nameof(LoraSeededTripleApplyPhase.BeforeFreshLoadPreflightReread))]
    public void SeededTriple_SecondOrThirdModelInjectedFailureLeavesAllModelsBitwiseUntouched(
        string failurePhaseName)
    {
        var failurePhase = Enum.Parse<LoraSeededTripleApplyPhase>(failurePhaseName);
        var policy = new LlmModel(TinyConfig(numLayers: 2));
        var reference = new LlmModel(TinyConfig(numLayers: 2));
        var freshLoad = new LlmModel(TinyConfig(numLayers: 2));
        _ = LoraApplicator.Apply(
            policy,
            Config(["o_proj"]),
            (block, projection) => LoraAdapterRandomSeedDerivation.Derive(
                701UL,
                block,
                projection,
                "audit.policy"));
        _ = LoraApplicator.Apply(
            reference,
            Config(["o_proj"]),
            (block, projection) => LoraAdapterRandomSeedDerivation.Derive(
                702UL,
                block,
                projection,
                "audit.reference"));
        _ = LoraApplicator.Apply(
            freshLoad,
            Config(["o_proj"]),
            (block, projection) => LoraAdapterRandomSeedDerivation.Derive(
                703UL,
                block,
                projection,
                "audit.fresh"));
        SetDistinctParameterState(policy);
        SetDistinctParameterState(reference);
        SetDistinctParameterState(freshLoad);
        var policyBefore = CaptureModel(policy);
        var referenceBefore = CaptureModel(reference);
        var freshLoadBefore = CaptureModel(freshLoad);

        var exception = Assert.Throws<InjectedTripleFailureException>(() =>
            LoraApplicator.ApplyFreshSeededTriple(
                policy,
                reference,
                freshLoad,
                Config(["q_proj", "v_proj"], dropout: 0.25f),
                704UL,
                "policy.step",
                "reference.step",
                "fresh-load.step",
                phase =>
                {
                    if (phase == failurePhase)
                        throw new InjectedTripleFailureException();
                }));

        Assert.Equal("injected triple preflight failure", exception.Message);
        AssertModelSnapshot(policyBefore, policy);
        AssertModelSnapshot(referenceBefore, reference);
        AssertModelSnapshot(freshLoadBefore, freshLoad);
    }

    [Fact]
    public void SeededPair_SuccessAttachesExactIdentitiesAndRoleProvenance()
    {
        var first = new LlmModel(TinyConfig(numLayers: 2));
        var second = new LlmModel(TinyConfig(numLayers: 2));
        const ulong rootSeed = 91UL;
        var application = LoraApplicator.ApplyFreshSeededPair(
            first,
            second,
            Config(["v_proj", "q_proj"], dropout: 0.25f),
            rootSeed,
            "policy.step",
            "reference.step");
        var slots = Enumerable.Range(0, 2)
            .SelectMany(block => new[] { "q_proj", "v_proj" }
                .Select(projection => (Block: block, Projection: projection)))
            .ToArray();

        Assert.Equal(slots.Length, application.FirstModelAdapters.Count);
        Assert.Equal(slots.Length, application.SecondModelAdapters.Count);
        for (var index = 0; index < slots.Length; index++)
        {
            var slot = slots[index];
            var policyAdapter = application.FirstModelAdapters[index];
            var referenceAdapter = application.SecondModelAdapters[index];
            var policySeeds = LoraAdapterRandomSeedDerivation.Derive(
                rootSeed,
                slot.Block,
                slot.Projection,
                "policy.step");
            var referenceSeeds = LoraAdapterRandomSeedDerivation.Derive(
                rootSeed,
                slot.Block,
                slot.Projection,
                "reference.step");

            Assert.Same(
                policyAdapter,
                first.GetAttachedProjectionAdapter(slot.Block, slot.Projection));
            Assert.Same(
                referenceAdapter,
                second.GetAttachedProjectionAdapter(slot.Block, slot.Projection));
            Assert.NotSame(policyAdapter, referenceAdapter);
            Assert.Equal(policySeeds, policyAdapter.RandomSeeds);
            Assert.Equal(referenceSeeds, referenceAdapter.RandomSeeds);
            Assert.Equal(
                policySeeds.InitializationSeed,
                referenceSeeds.InitializationSeed);
            Assert.NotEqual(policySeeds.DropoutSeed, referenceSeeds.DropoutSeed);
            Assert.Equal(AdapterA(policyAdapter), AdapterA(referenceAdapter));
        }

        Assert.All(first.BaseParameters, parameter =>
            Assert.False(parameter.Value.RequiresGrad));
        Assert.All(second.BaseParameters, parameter =>
            Assert.False(parameter.Value.RequiresGrad));
        Assert.All(
            application.FirstModelAdapters
                .Concat(application.SecondModelAdapters)
                .SelectMany(adapter => adapter.AdapterParameters),
            pair => Assert.True(pair.Parameter.Value.RequiresGrad));
    }

    [Fact]
    public void SeededPair_SameModelRejectsBeforeMutation()
    {
        var model = new LlmModel(TinyConfig());
        var before = CaptureModel(model);

        var exception = Assert.Throws<ArgumentException>(() =>
            LoraApplicator.ApplyFreshSeededPair(
                model,
                model,
                Config(["q_proj"]),
                3UL,
                "policy.step",
                "reference.step"));

        Assert.Contains(
            "LORA_MODELS_MUST_BE_DISTINCT",
            exception.Message,
            StringComparison.Ordinal);
        AssertModelSnapshot(before, model);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SeededPair_TransactionalCallbacksSerializeCompetingSingleApply(
        bool competeWithFirstModel)
    {
        var first = new LlmModel(TinyConfig());
        var second = new LlmModel(TinyConfig());
        using var afterCallbackEntered = new ManualResetEventSlim();
        using var releaseAfterCallback = new ManualResetEventSlim();
        using var competingBefore = new ManualResetEventSlim();
        using var competingInside = new ManualResetEventSlim();
        var beforeCallbackCount = 0;
        var afterCallbackCount = 0;
        var releaseIssued = 0;
        var beforeCallbackSawFreshModels = false;
        var afterCallbackSawCommittedIdentities = false;
        var competitorEnteredAfterRelease = false;
        var competitorEnteredBeforeCallbackExit = false;
        LoraSeededPairApplication? callbackApplication = null;

        var pairTask = Task.Run(() => LoraApplicator.ApplyFreshSeededPair(
            first,
            second,
            Config(["q_proj"]),
            4UL,
            "policy.step",
            "reference.step",
            beforeMutationUnderPairLock: () =>
            {
                Interlocked.Increment(ref beforeCallbackCount);
                beforeCallbackSawFreshModels =
                    first.GetAttachedProjectionAdapter(0, "q_proj") is null &&
                    second.GetAttachedProjectionAdapter(0, "q_proj") is null;
            },
            afterCommitUnderPairLock: application =>
            {
                Interlocked.Increment(ref afterCallbackCount);
                callbackApplication = application;
                afterCallbackSawCommittedIdentities = ReferenceEquals(
                    application.FirstModelAdapters[0],
                    first.GetAttachedProjectionAdapter(0, "q_proj")) &&
                    ReferenceEquals(
                    application.SecondModelAdapters[0],
                    second.GetAttachedProjectionAdapter(0, "q_proj"));
                afterCallbackEntered.Set();
                if (!releaseAfterCallback.Wait(TimeSpan.FromSeconds(20)))
                    throw new TimeoutException("after-callback release was not observed");
                competitorEnteredBeforeCallbackExit = competingInside.IsSet;
            }));

        Task<IReadOnlyList<LoraLinear>>? competingTask = null;
        var afterCallbackWasObserved = false;
        var competingBeforeWasObserved = false;
        var competingEnteredWhileCallbackBlocked = false;
        try
        {
            afterCallbackWasObserved = afterCallbackEntered.Wait(
                TimeSpan.FromSeconds(10));
            if (afterCallbackWasObserved)
            {
                var selectedModel = competeWithFirstModel ? first : second;
                competingTask = Task.Run(() => LoraApplicator.Apply(
                    selectedModel,
                    Config(["o_proj"]),
                    phase =>
                    {
                        if (phase == LoraApplyPhase.BeforeModelLock)
                            competingBefore.Set();
                        if (phase == LoraApplyPhase.InsideModelLock)
                        {
                            competitorEnteredAfterRelease =
                                Volatile.Read(ref releaseIssued) == 1;
                            competingInside.Set();
                        }
                    }));
                competingBeforeWasObserved = competingBefore.Wait(
                    TimeSpan.FromSeconds(10));
                if (competingBeforeWasObserved)
                {
                    competingEnteredWhileCallbackBlocked = competingInside.Wait(
                        TimeSpan.FromMilliseconds(150));
                }
            }
        }
        finally
        {
            Interlocked.Exchange(ref releaseIssued, 1);
            releaseAfterCallback.Set();
        }

        var application = await pairTask.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.True(afterCallbackWasObserved);
        Assert.NotNull(competingTask);
        var competing = await competingTask!.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.True(competingBeforeWasObserved);
        Assert.False(competingEnteredWhileCallbackBlocked);
        Assert.True(competingInside.IsSet);
        Assert.True(competitorEnteredAfterRelease);
        Assert.False(competitorEnteredBeforeCallbackExit);
        Assert.Equal(1, beforeCallbackCount);
        Assert.Equal(1, afterCallbackCount);
        Assert.True(beforeCallbackSawFreshModels);
        Assert.True(afterCallbackSawCommittedIdentities);
        Assert.Same(application, callbackApplication);
        Assert.Single(application.FirstModelAdapters);
        Assert.Single(application.SecondModelAdapters);
        Assert.Single(competing);
        Assert.Same(
            competing[0],
            (competeWithFirstModel ? first : second)
                .GetAttachedProjectionAdapter(0, "o_proj"));
        Assert.Same(
            application.FirstModelAdapters[0],
            first.GetAttachedProjectionAdapter(0, "q_proj"));
        Assert.Same(
            application.SecondModelAdapters[0],
            second.GetAttachedProjectionAdapter(0, "q_proj"));
    }

    private static float[] AdapterA(LoraLinear adapter)
        => adapter.AdapterParameters[0].Parameter.Value.Data.ToArray();

    private static ModelSnapshot CaptureModel(LlmModel model)
        => new(
            model.Parameters().ToArray(),
            model.Parameters().Select(CaptureParameter).ToArray(),
            Enumerable.Range(0, model.Blocks.Count)
                .SelectMany(block => new[]
                {
                    model.GetAttachedProjectionAdapter(block, "q_proj"),
                    model.GetAttachedProjectionAdapter(block, "k_proj"),
                    model.GetAttachedProjectionAdapter(block, "v_proj"),
                    model.GetAttachedProjectionAdapter(block, "o_proj")
                })
                .Select(adapter => new AttachedAdapterSnapshot(
                    adapter,
                    (adapter as LoraLinear)?.RandomSeeds))
                .ToArray());

    private static ParameterSnapshot CaptureParameter(Parameter parameter)
        => new(
            parameter.Value,
            parameter.Value.Storage,
            parameter.Value.Data,
            parameter.Value.Data.Select(BitConverter.SingleToInt32Bits).ToArray(),
            parameter.Value.RequiresGrad,
            parameter.Value.Grad,
            parameter.Value.Grad?.Storage,
            parameter.Value.Grad?.Data,
            parameter.Value.Grad?.Data
                .Select(BitConverter.SingleToInt32Bits)
                .ToArray());

    private static void AssertModelSnapshot(ModelSnapshot expected, LlmModel actual)
    {
        var actualParameters = actual.Parameters().ToArray();
        Assert.Equal(expected.ParameterReferences.Length, actualParameters.Length);
        for (var index = 0; index < actualParameters.Length; index++)
        {
            Assert.Same(expected.ParameterReferences[index], actualParameters[index]);
            var expectedParameter = expected.Parameters[index];
            var actualParameter = actualParameters[index];
            Assert.Same(expectedParameter.TensorReference, actualParameter.Value);
            Assert.Same(expectedParameter.StorageReference, actualParameter.Value.Storage);
            Assert.Same(expectedParameter.DataReference, actualParameter.Value.Data);
            Assert.Equal(
                expectedParameter.ValueBits,
                actualParameter.Value.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray());
            Assert.Equal(
                expectedParameter.RequiresGrad,
                actualParameter.Value.RequiresGrad);
            Assert.Same(
                expectedParameter.GradientReference,
                actualParameter.Value.Grad);
            Assert.Same(
                expectedParameter.GradientStorageReference,
                actualParameter.Value.Grad?.Storage);
            Assert.Same(
                expectedParameter.GradientDataReference,
                actualParameter.Value.Grad?.Data);
            Assert.Equal(
                expectedParameter.GradientValueBits,
                actualParameter.Value.Grad?.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray());
        }

        var actualAdapters = Enumerable.Range(0, actual.Blocks.Count)
            .SelectMany(block => new[]
            {
                actual.GetAttachedProjectionAdapter(block, "q_proj"),
                actual.GetAttachedProjectionAdapter(block, "k_proj"),
                actual.GetAttachedProjectionAdapter(block, "v_proj"),
                actual.GetAttachedProjectionAdapter(block, "o_proj")
            })
            .Select(adapter => new AttachedAdapterSnapshot(
                adapter,
                (adapter as LoraLinear)?.RandomSeeds))
            .ToArray();
        Assert.Equal(expected.AttachedAdapters.Length, actualAdapters.Length);
        for (var index = 0; index < actualAdapters.Length; index++)
        {
            Assert.Same(
                expected.AttachedAdapters[index].Adapter,
                actualAdapters[index].Adapter);
            Assert.Equal(
                expected.AttachedAdapters[index].RandomSeeds,
                actualAdapters[index].RandomSeeds);
        }
    }

    private static void SetDistinctParameterState(LlmModel model)
    {
        var index = 0;
        foreach (var parameter in model.Parameters())
        {
            parameter.Value.RequiresGrad = index % 2 == 0;
            parameter.Value.Grad = new Tensor(
                Enumerable.Range(0, parameter.Value.Data.Length)
                    .Select(valueIndex => index + (valueIndex * 0.01f))
                    .ToArray(),
                parameter.Value.Shape);
            index++;
        }
    }

    private sealed record ModelSnapshot(
        Parameter[] ParameterReferences,
        ParameterSnapshot[] Parameters,
        AttachedAdapterSnapshot[] AttachedAdapters);

    private sealed record ParameterSnapshot(
        Tensor TensorReference,
        TensorStorage StorageReference,
        float[] DataReference,
        int[] ValueBits,
        bool RequiresGrad,
        Tensor? GradientReference,
        TensorStorage? GradientStorageReference,
        float[]? GradientDataReference,
        int[]? GradientValueBits);

    private sealed record AttachedAdapterSnapshot(
        object? Adapter,
        LoraAdapterRandomSeeds? RandomSeeds);

    private sealed class InjectedTripleFailureException : Exception
    {
        internal InjectedTripleFailureException()
            : base("injected triple preflight failure")
        {
        }
    }

    private static LlmModelRandomStateSnapshot RandomSnapshot()
        => new(
            LlmModelRandomStateSnapshot.CurrentFormatVersion,
            LlmModelRandomStateSnapshot.CurrentAlgorithm,
            [
                new LlmModelRandomStateEntry(
                    "lora.block_0000.q_proj",
                    "policy.step",
                    101UL,
                    202UL,
                    3UL)
            ],
            new string('a', 64),
            new string('b', 64),
            7);

    private static LlmModelRandomStateSnapshot TwoEntryRandomSnapshot()
        => new(
            LlmModelRandomStateSnapshot.CurrentFormatVersion,
            LlmModelRandomStateSnapshot.CurrentAlgorithm,
            [
                new LlmModelRandomStateEntry(
                    "lora.block_0000.q_proj",
                    "alpha.step",
                    101UL,
                    202UL,
                    3UL),
                new LlmModelRandomStateEntry(
                    "lora.block_0000.v_proj",
                    "audit.step",
                    303UL,
                    404UL,
                    5UL)
            ],
            new string('a', 64),
            new string('b', 64),
            7);

    private static byte[] RewriteSecondEntryKey(
        byte[] bytes,
        string replacementKey)
    {
        var (firstStart, firstEnd, secondStart, secondEnd) =
            SerializedEntryRanges(bytes);
        var firstKeyLength = checked((int)ReadUInt32(bytes, firstStart));
        var secondKeyLength = checked((int)ReadUInt32(bytes, secondStart));
        var replacement = Encoding.UTF8.GetBytes(replacementKey);
        Assert.Equal(firstKeyLength, replacement.Length);
        Assert.Equal(firstKeyLength, secondKeyLength);
        var secondKeyStart = secondStart + sizeof(uint);
        Buffer.BlockCopy(
            replacement,
            0,
            bytes,
            secondKeyStart,
            replacement.Length);
        Assert.True(secondEnd > firstEnd);
        return bytes;
    }

    private static byte[] SwapSerializedEntries(byte[] bytes)
    {
        var (firstStart, firstEnd, secondStart, secondEnd) =
            SerializedEntryRanges(bytes);
        var firstLength = firstEnd - firstStart;
        var secondLength = secondEnd - secondStart;
        Assert.Equal(firstLength, secondLength);
        var first = bytes[firstStart..firstEnd];
        var second = bytes[secondStart..secondEnd];
        Buffer.BlockCopy(second, 0, bytes, firstStart, secondLength);
        Buffer.BlockCopy(first, 0, bytes, secondStart, firstLength);
        return bytes;
    }

    private static (int FirstStart, int FirstEnd, int SecondStart, int SecondEnd)
        SerializedEntryRanges(byte[] bytes)
    {
        var offset = 0;
        offset += 4; // XRGN
        offset += sizeof(uint); // format
        var algorithmLength = checked((int)ReadUInt32(bytes, offset));
        offset += sizeof(uint) + algorithmLength;
        offset += sizeof(int); // step
        offset += sizeof(uint) + 64; // topology hash
        offset += sizeof(uint) + 64; // LoRA configuration hash
        var count = checked((int)ReadUInt32(bytes, offset));
        Assert.Equal(2, count);
        offset += sizeof(uint);
        var firstStart = offset;
        var firstEnd = SkipSerializedEntry(bytes, firstStart);
        var secondStart = firstEnd;
        var secondEnd = SkipSerializedEntry(bytes, secondStart);
        Assert.Equal(bytes.Length, secondEnd);
        return (firstStart, firstEnd, secondStart, secondEnd);
    }

    private static int SkipSerializedEntry(byte[] bytes, int offset)
    {
        var keyLength = checked((int)ReadUInt32(bytes, offset));
        offset += sizeof(uint) + keyLength;
        var domainLength = checked((int)ReadUInt32(bytes, offset));
        offset += sizeof(uint) + domainLength + (sizeof(ulong) * 3);
        return offset;
    }

    private static uint ReadUInt32(byte[] bytes, int offset)
        => BinaryPrimitives.ReadUInt32LittleEndian(
            bytes.AsSpan(offset, sizeof(uint)));

    private static byte[] WriteRandomStateBytes(
        LlmModelRandomStateSnapshot snapshot)
    {
        var root = Path.Combine(
            Path.GetTempPath(), $"random-state-source-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var path = Path.Combine(
            root,
            RepositoryOwnedDpoRandomStateFile.BuildFileName(
                snapshot.RandomStateStep));
        try
        {
            var written = RepositoryOwnedDpoRandomStateFile.WriteCreateNew(
                path,
                snapshot);
            Assert.True(written.IsSuccess, written.ErrorMessage);
            return File.ReadAllBytes(path);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static void AssertRandomBytesRejected(
        string root,
        LlmModelRandomStateSnapshot snapshot,
        byte[] bytes,
        string name)
    {
        var directory = Path.Combine(root, name);
        Directory.CreateDirectory(directory);
        var path = Path.Combine(
            directory,
            RepositoryOwnedDpoRandomStateFile.BuildFileName(
                snapshot.RandomStateStep));
        var valid = RepositoryOwnedDpoRandomStateFile.WriteCreateNew(
            path,
            snapshot);
        Assert.True(valid.IsSuccess, valid.ErrorMessage);
        using (var stream = new FileStream(
                   path,
                   FileMode.Open,
                   FileAccess.Write,
                   FileShare.None))
        {
            stream.SetLength(bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush(flushToDisk: true);
        }
        var result = RepositoryOwnedDpoRandomStateFile.OpenAndValidate(
            path,
            Sha256(bytes),
            bytes.LongLength,
            snapshot.RandomStateStep,
            snapshot.TopologySha256,
            snapshot.LoraConfigurationSha256);
        Assert.False(result.IsSuccess, name);
        Assert.Equal(
            RepositoryOwnedDpoRandomStateFile.InvalidErrorCode,
            result.ErrorCode);
    }

    private static string Sha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static LoraConfig Config(string[] targets, float dropout = 0f)
        => new()
        {
            Rank = 2,
            Alpha = 4,
            Dropout = dropout,
            TargetModules = targets,
        };

    private static ModelConfig TinyConfig(int numLayers = 1)
        => new()
        {
            VocabSize = 16,
            ContextLen = 4,
            HiddenDim = 4,
            NumLayers = numLayers,
            NumHeads = 1,
            NumKvHeads = 1,
            FfnHiddenDim = 8,
        };
}
