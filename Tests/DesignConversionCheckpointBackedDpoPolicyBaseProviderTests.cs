// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using DPO;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

public sealed class DesignConversionCheckpointBackedDpoPolicyBaseProviderTests
{
    private const string BaseModelId = "test-org/checkpoint-backed-policy";
    private const string BaseRevision = "revision-v2";
    private const string ConfigurationInvalidMessage =
        "DesignConversion DPO checkpoint-backed policy base configuration is invalid.";
    private const string SourceFailedMessage =
        "The DesignConversion DPO policy base checkpoint source failed.";
    private const string StreamInvalidMessage =
        "The DesignConversion DPO policy base checkpoint stream is invalid.";
    private const string HashMismatchMessage =
        "The DesignConversion DPO policy base checkpoint hash does not match.";
    private const string ChangedDuringLoadMessage =
        "The DesignConversion DPO policy base checkpoint changed during loading.";
    private const string CheckpointInvalidMessage =
        "The DesignConversion DPO policy base checkpoint is invalid.";
    private const string CancelledMessage =
        "DesignConversion DPO checkpoint-backed policy base creation was cancelled.";
    private const string TrainingConfigurationFingerprint =
        DesignConversionDpoPolicyBaseTestProvider
            .CurrentTrainingConfigurationFingerprint;

    [Fact]
    public async Task CreateModelSetAsync_ValidSnapshotCreatesIndependentModelsAndDisposesStream()
    {
        var fixture = CreateCheckpointFixture();
        DisposalTrackingMemoryStream? issuedStream = null;
        var source = new RecordingCheckpointSource(_ =>
        {
            issuedStream = new DisposalTrackingMemoryStream(fixture.Bytes);
            return Task.FromResult(OperationResult<Stream>.Ok(issuedStream));
        });
        var provider = CreateProvider(source, fixture.Config, fixture.Sha256);

        var result = await provider.CreateModelSetAsync(CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, source.InvocationCount);
        Assert.NotNull(issuedStream);
        Assert.True(issuedStream!.IsDisposed);
        var modelSet = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(result.Value);
        Assert.Equal("checkpoint_backed", modelSet.Identity.ProviderKind);
        Assert.Equal("explicit_model_config_v1", modelSet.Identity.ConfigurationKind);
        Assert.Equal(BaseModelId, modelSet.Identity.BaseModelId);
        Assert.Equal(BaseRevision, modelSet.Identity.BaseRevision);
        Assert.Equal(fixture.Sha256, modelSet.Identity.BaseCheckpointSha256);

        Assert.NotSame(modelSet.Policy, modelSet.Reference);
        Assert.NotSame(modelSet.Policy, modelSet.FreshLoadTarget);
        Assert.NotSame(modelSet.Reference, modelSet.FreshLoadTarget);
        Assert.NotSame(fixture.Config, modelSet.Policy.Config);
        Assert.NotSame(fixture.Config, modelSet.Reference.Config);
        Assert.NotSame(fixture.Config, modelSet.FreshLoadTarget.Config);
        Assert.NotSame(modelSet.Policy.Config, modelSet.Reference.Config);
        Assert.NotSame(modelSet.Policy.Config, modelSet.FreshLoadTarget.Config);
        Assert.NotSame(modelSet.Reference.Config, modelSet.FreshLoadTarget.Config);
        AssertModelsPhysicallyIndependentAndBitwiseEqual(modelSet.Policy, modelSet.Reference);
        AssertModelsPhysicallyIndependentAndBitwiseEqual(modelSet.Policy, modelSet.FreshLoadTarget);
        AssertModelsPhysicallyIndependentAndBitwiseEqual(modelSet.Reference, modelSet.FreshLoadTarget);

        var referenceBits = SnapshotParameterBits(modelSet.Reference);
        var freshLoadBits = SnapshotParameterBits(modelSet.FreshLoadTarget);
        var policyEntry = modelSet.Policy.GetCheckpointParameterEntries()[0];
        policyEntry.Parameter.Value.Data[0] = MathF.BitIncrement(
            policyEntry.Parameter.Value.Data[0]);

        AssertParameterBitsEqual(referenceBits, modelSet.Reference);
        AssertParameterBitsEqual(freshLoadBits, modelSet.FreshLoadTarget);
        Assert.NotEqual(
            BitConverter.SingleToInt32Bits(policyEntry.Parameter.Value.Data[0]),
            referenceBits[0][0]);
    }

    [Fact]
    public async Task CreateModelSetAsync_ParallelCallsShareNoModelStateAndOpenFreshStreams()
    {
        var fixture = CreateCheckpointFixture();
        var source = new RendezvousCheckpointSource(fixture.Bytes);
        var provider = CreateProvider(source, fixture.Config, fixture.Sha256);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var first = provider.CreateModelSetAsync(timeout.Token);
        Assert.False(first.IsCompleted);
        Assert.Equal(1, source.InvocationCount);
        Assert.Equal(1, source.ActiveCallCount);
        var second = provider.CreateModelSetAsync(timeout.Token);
        var results = await Task.WhenAll(first, second).WaitAsync(timeout.Token);

        Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
        Assert.Equal(2, source.InvocationCount);
        Assert.Equal(2, source.MaximumConcurrentCallCount);
        Assert.Equal(2, source.ActiveCallCountAtRelease);
        Assert.Equal(0, source.ActiveCallCount);
        Assert.Equal(2, source.Streams.Count);
        Assert.Equal(
            2,
            source.Streams.Distinct(ReferenceEqualityComparer.Instance).Count());
        Assert.All(source.Streams, stream => Assert.True(stream.IsDisposed));
        var models = results
            .SelectMany(result => new[]
            {
                result.Value!.Policy,
                result.Value.Reference,
                result.Value.FreshLoadTarget
            })
            .ToArray();
        Assert.Equal(models.Length, models.Distinct(ReferenceEqualityComparer.Instance).Count());
        Assert.Equal(
            models.Length,
            models.Select(model => model.Config)
                .Distinct(ReferenceEqualityComparer.Instance)
                .Count());
        for (var left = 0; left < models.Length; left++)
        {
            for (var right = left + 1; right < models.Length; right++)
            {
                AssertModelsPhysicallyIndependentAndBitwiseEqual(
                    models[left],
                    models[right]);
            }
        }
    }

    [Fact]
    public void Create_InvalidInputsFailBeforeSourceInvocation()
    {
        var fixture = CreateCheckpointFixture();
        var source = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("SOURCE_MUST_NOT_BE_INVOKED"));
        var attempts = new[]
        {
            DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
                null,
                fixture.Config,
                BaseModelId,
                BaseRevision,
                fixture.Sha256),
            DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
                source,
                null,
                BaseModelId,
                BaseRevision,
                fixture.Sha256),
            DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
                source,
                fixture.Config with { VocabSize = 1 },
                BaseModelId,
                BaseRevision,
                fixture.Sha256),
            DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
                source,
                fixture.Config,
                @"C:\private\model",
                BaseRevision,
                fixture.Sha256),
            DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
                source,
                fixture.Config,
                BaseModelId,
                string.Empty,
                fixture.Sha256),
            DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
                source,
                fixture.Config,
                BaseModelId,
                BaseRevision,
                fixture.Sha256.ToUpperInvariant())
        };

        Assert.All(attempts, attempt =>
        {
            Assert.False(attempt.IsSuccess);
            Assert.Equal(
                DesignConversionCheckpointBackedDpoPolicyBaseProvider.ConfigurationInvalidErrorCode,
                attempt.ErrorCode);
            Assert.Equal(ConfigurationInvalidMessage, attempt.ErrorMessage);
            Assert.Null(attempt.Value);
            Assert.DoesNotContain("C:\\private", attempt.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        });
        Assert.Equal(0, source.InvocationCount);
    }

    // Публичное имя модели не должно превращаться в канал для локального пути
    // только потому, что тест запущен в другой операционной системе.
    [Theory]
    // Абсолютный путь Unix нельзя публиковать как имя модели.
    [InlineData("/tmp/private-checkpoint")]
    // Оба варианта абсолютного пути Windows должны отсеиваться и в Linux.
    [InlineData("C:/private/model")]
    [InlineData(@"C:\private\model")]
    // Сетевой путь Windows остаётся приватным независимо от платформы запуска.
    [InlineData(@"\\server\share\model")]
    // Относительный путь с диском Windows также не является логическим именем.
    [InlineData("C:private/model")]
    // Обход каталога запрещён как в начале, так и внутри допустимого слеша.
    [InlineData("../private/model")]
    [InlineData("organization/../model")]
    // Разметка и структурированные данные не являются идентификатором модели.
    [InlineData("<main>model</main>")]
    [InlineData("{\"model\":\"private\"}")]
    public void Create_PublicModelIdentityCrossPlatform_FileSystemOrStructuredValueFailsBeforeSourceInvocation(
        string unsafeBaseModelId)
    {
        // Источник бросит исключение, если проверка пропустит значение дальше границы.
        var fixture = CreateCheckpointFixture();
        var source = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("SOURCE_MUST_NOT_BE_INVOKED"));

        // Проверяется именно публичное имя базовой модели, а не путь к файлу checkpoint.
        var result = DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
            source,
            fixture.Config,
            unsafeBaseModelId,
            BaseRevision,
            fixture.Sha256);

        // Отказ обязан быть стабильным, обезличенным и произойти до чтения файла.
        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionCheckpointBackedDpoPolicyBaseProvider.ConfigurationInvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(ConfigurationInvalidMessage, result.ErrorMessage);
        Assert.Null(result.Value);
        Assert.DoesNotContain(unsafeBaseModelId, result.ErrorMessage, StringComparison.Ordinal);
        Assert.Equal(0, source.InvocationCount);
    }

    [Fact]
    public void Create_PublicModelIdentityCrossPlatform_OrganizationModelRevisionIsPreserved()
    {
        // Один внутренний слеш разделяет организацию и модель, а суффикс после @ задаёт версию.
        const string publicModelIdentity = "organization/model@revision";
        var fixture = CreateCheckpointFixture();
        var source = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("SOURCE_MUST_NOT_BE_INVOKED"));

        // Корректное логическое имя должно пройти без нормализации и без обращения к файлу.
        var result = DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
            source,
            fixture.Config,
            publicModelIdentity,
            BaseRevision,
            fixture.Sha256);

        // Подготовительное описание является публичным переносчиком исходного имени.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        var preparation = result.Value!.DescribePreparation();
        Assert.True(preparation.IsSuccess, preparation.ErrorMessage);
        Assert.Equal(publicModelIdentity, preparation.Value!.BaseModelId);
        Assert.Equal(0, source.InvocationCount);
    }

    [Fact]
    public void CreateWithOptions_RootSeedZeroAndMaximumRetainExactAuthority()
    {
        var fixture = CreateCheckpointFixture();
        var source = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("SOURCE_MUST_NOT_BE_INVOKED"));

        foreach (var rootSeed in new[] { 0UL, ulong.MaxValue })
        {
            var options = new DesignConversionDpoPolicyBaseProviderOptions(
                rootSeed,
                TrainingConfigurationFingerprint);
            var result = DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .CreateWithOptions(
                    source,
                    fixture.Config,
                    BaseModelId,
                    BaseRevision,
                    fixture.Sha256,
                    options);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            var provider = Assert.IsType<
                DesignConversionCheckpointBackedDpoPolicyBaseProvider>(result.Value);
            Assert.Equal(options, provider.ProviderOptions);
            Assert.NotSame(options, provider.ProviderOptions);
            Assert.True(
                DesignConversionDpoPolicyRandomContextContract.MatchesCurrentAuthority(
                    provider.CurrentRandomContext,
                    rootSeed,
                    TrainingConfigurationFingerprint));
            Assert.Equal(rootSeed, provider.CurrentRandomContext!.RootInitializationSeed);
        }

        Assert.Equal(0, source.InvocationCount);
    }

    [Fact]
    public void CreateWithOptions_NullRootSeedRetainsFingerprintWithoutSynthesizingContext()
    {
        var fixture = CreateCheckpointFixture();
        var source = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("SOURCE_MUST_NOT_BE_INVOKED"));
        var options = new DesignConversionDpoPolicyBaseProviderOptions(
            RootInitializationSeed: null,
            TrainingConfigurationFingerprint);

        var result = DesignConversionCheckpointBackedDpoPolicyBaseProvider
            .CreateWithOptions(
                source,
                fixture.Config,
                BaseModelId,
                BaseRevision,
                fixture.Sha256,
                options);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var provider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(result.Value);
        Assert.Equal(options, provider.ProviderOptions);
        Assert.NotSame(options, provider.ProviderOptions);
        Assert.Null(provider.ProviderOptions!.RootInitializationSeed);
        Assert.Equal(
            TrainingConfigurationFingerprint,
            provider.ProviderOptions.TrainingConfigurationFingerprint);
        Assert.Null(provider.CurrentRandomContext);
        Assert.Equal(0, source.InvocationCount);
    }

    [Fact]
    public void CreateWithOptions_InvalidFingerprintFailsBeforeEitherSourceInvocation()
    {
        var fixture = CreateCheckpointFixture(
            DesignConversionDpoPolicyBaseTestProvider.CreateConfig());
        var tokenizerFixture = CreateCanonicalBpeFixture(fixture.Config);
        var checkpointSource = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("CHECKPOINT_SOURCE_MUST_NOT_BE_INVOKED"));
        var tokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
            throw new InvalidOperationException("TOKENIZER_SOURCE_MUST_NOT_BE_INVOKED"));
        var invalidFingerprints = new string?[]
        {
            TrainingConfigurationFingerprint.ToUpperInvariant(),
            TrainingConfigurationFingerprint[..^1],
            TrainingConfigurationFingerprint[..^1] + "g",
            " " + TrainingConfigurationFingerprint[..^1],
            null
        };
        var invalidOptions = invalidFingerprints
            .Select(fingerprint =>
                (DesignConversionDpoPolicyBaseProviderOptions?)new(
                    0,
                    fingerprint!))
            .Append(null);

        foreach (var options in invalidOptions)
        {
            var result = DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .CreateWithOptions(
                    checkpointSource,
                    fixture.Config,
                    BaseModelId,
                    BaseRevision,
                    fixture.Sha256,
                    options,
                    tokenizerFixture.Descriptor,
                    tokenizerSource);

            AssertConfigurationInvalid(result);
        }

        Assert.Equal(0, checkpointSource.InvocationCount);
        Assert.Equal(0, tokenizerSource.InvocationCount);
    }

    [Fact]
    public void CreateTrusted_LegacyPositionalFourthNullRemainsUnambiguous()
    {
        var fixture = CreateCheckpointFixture();
        var source = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("SOURCE_MUST_NOT_BE_INVOKED"));
        var identity = new DesignConversionDpoPolicyBaseIdentity(
            "test_in_memory",
            "test.non_default.policy.v2",
            BaseModelId,
            BaseRevision,
            fixture.Sha256);

        var result = DesignConversionCheckpointBackedDpoPolicyBaseProvider
            .CreateTrusted(
                source,
                fixture.Config,
                identity,
                null);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Null(result.Value!.ProviderOptions);
        Assert.Null(result.Value.CurrentRandomContext);
        Assert.Equal(0, source.InvocationCount);
    }

    [Fact]
    public void CreateTrustedWithOptions_NullOrInvalidFailsBeforeSourceInvocation()
    {
        var fixture = CreateCheckpointFixture();
        var source = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("SOURCE_MUST_NOT_BE_INVOKED"));
        var identity = new DesignConversionDpoPolicyBaseIdentity(
            "test_in_memory",
            "test.non_default.policy.v2",
            BaseModelId,
            BaseRevision,
            fixture.Sha256);
        var attempts = new DesignConversionDpoPolicyBaseProviderOptions?[]
        {
            null,
            DesignConversionDpoPolicyBaseTestProvider
                .CreateCurrentProviderOptions() with
                {
                    TrainingConfigurationFingerprint =
                        TrainingConfigurationFingerprint[..^1] + "g"
                }
        };

        foreach (var options in attempts)
        {
            var result = DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .CreateTrustedWithOptions(
                    source,
                    fixture.Config,
                    identity,
                    options);
            AssertConfigurationInvalid(result);
        }

        Assert.Equal(0, source.InvocationCount);
    }

    [Fact]
    public async Task CreateTrustedWithOptions_ValidPreservesIdentityAndExactAuthority()
    {
        var fixture = CreateCheckpointFixture(
            DesignConversionDpoPolicyBaseTestProvider.CreateConfig());
        var source = new RecordingCheckpointSource(_ =>
            Task.FromResult(OperationResult<Stream>.Ok(
                new DisposalTrackingMemoryStream(fixture.Bytes))));
        var identity = new DesignConversionDpoPolicyBaseIdentity(
            "test_in_memory",
            "test.non_default.policy.v2",
            BaseModelId,
            BaseRevision,
            fixture.Sha256);
        var options = DesignConversionDpoPolicyBaseTestProvider
            .CreateCurrentProviderOptions();

        var created = DesignConversionCheckpointBackedDpoPolicyBaseProvider
            .CreateTrustedWithOptions(
                source,
                fixture.Config,
                identity,
                options);

        Assert.True(created.IsSuccess, created.ErrorMessage);
        Assert.Equal(options, created.Value!.ProviderOptions);
        Assert.NotSame(options, created.Value.ProviderOptions);
        Assert.Equal(
            DesignConversionDpoPolicyBaseTestProvider.CreateCurrentRandomContext(),
            created.Value.CurrentRandomContext);

        var modelSet = await created.Value.CreateModelSetAsync(
            CancellationToken.None);

        Assert.True(modelSet.IsSuccess, modelSet.ErrorMessage);
        Assert.Equal(identity, modelSet.Value!.Identity);
        Assert.Equal(
            DesignConversionDpoPolicyBaseTestProvider.CreateCurrentRandomContext(),
            modelSet.Value.RandomContext);
        Assert.Equal(1, source.InvocationCount);
    }

    [Fact]
    public void Create_LegacyPathRemainsOptionsFreeAndModelSetHasNullableRandomContext()
    {
        var fixture = CreateCheckpointFixture();
        var source = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("SOURCE_MUST_NOT_BE_INVOKED"));

        var result = DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
            source,
            fixture.Config,
            BaseModelId,
            BaseRevision,
            fixture.Sha256);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var provider = Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(result.Value);
        Assert.Null(provider.ProviderOptions);
        Assert.Null(provider.CurrentRandomContext);
        var randomContextProperty = typeof(DesignConversionDpoPolicyBaseModelSet)
            .GetProperty(
            "RandomContext",
            BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(randomContextProperty);
        Assert.Equal(
            typeof(DesignConversionDpoPolicyRandomContext),
            randomContextProperty!.PropertyType);
        Assert.Equal(0, source.InvocationCount);
    }

    [Fact]
    public async Task CreateModelSetAsync_PreCancelledTokenDoesNotInvokeSource()
    {
        var fixture = CreateCheckpointFixture();
        var source = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("PRE_CANCELLED_SOURCE_MUST_NOT_RUN"));
        var provider = CreateProvider(source, fixture.Config, fixture.Sha256);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        var result = await provider.CreateModelSetAsync(cancellationSource.Token);

        AssertFailure(result, "CANCELLED", CancelledMessage);
        Assert.Equal(0, source.InvocationCount);
    }

    [Fact]
    public async Task CreateModelSetAsync_ValidCanonicalBpeReferenceMatchesRetainedBytesAndOwnedConfig()
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var checkpointFixture = CreateCheckpointFixture(config);
        var tokenizerFixture = CreateCanonicalBpeFixture(config);
        var checkpointSource = new RecordingCheckpointSource(_ =>
            Task.FromResult(OperationResult<Stream>.Ok(
                new DisposalTrackingMemoryStream(checkpointFixture.Bytes))));
        var tokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
            OperationResult<Stream>.Ok(
                new DisposalTrackingMemoryStream(tokenizerFixture.Bytes)));
        var provider = CreateProvider(
            checkpointSource,
            checkpointFixture.Config,
            checkpointFixture.Sha256,
            tokenizerFixture.Descriptor,
            tokenizerSource);

        var result = await provider.CreateModelSetAsync(CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, checkpointSource.InvocationCount);
        Assert.Equal(1, tokenizerSource.InvocationCount);
        Assert.NotNull(tokenizerSource.LastStream);
        Assert.True(Assert.IsAssignableFrom<IDisposalProbe>(
            tokenizerSource.LastStream!).IsDisposed);
        var modelSet = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(result.Value);
        var reference = AssertCanonicalBpeReference(
            modelSet,
            tokenizerFixture.Descriptor);

        var reopened = await reference.OpenSnapshotAsync(CancellationToken.None);

        Assert.True(reopened.IsSuccess, reopened.ErrorMessage);
        Assert.Equal(2, tokenizerSource.InvocationCount);
        Assert.NotSame(tokenizerSource.Streams[0], reopened.Value);
        var reopenedStream = Assert.IsType<DisposalTrackingMemoryStream>(reopened.Value);
        Assert.False(reopenedStream.IsDisposed);
        Assert.Equal(0, reopenedStream.Position);
        using (reopenedStream)
        {
            var revalidated = DesignConversionDpoPromotionArtifactContractCodec
                .RevalidateCanonicalBpeTokenizerArtifactDescriptor(
                    reopenedStream,
                    checkpointFixture.Config,
                    tokenizerFixture.Descriptor);
            Assert.True(revalidated.IsSuccess, revalidated.ErrorMessage);
            Assert.Equal(tokenizerFixture.Descriptor, revalidated.Value);
            Assert.False(reopenedStream.IsDisposed);
            Assert.Equal(0, reopenedStream.Position);
        }
        Assert.True(reopenedStream.IsDisposed);

        AssertPublicTokenizerReferenceIsPathAndPayloadFree(reference);
    }

    [Fact]
    public void PolicyBaseModelSet_PreservesFivePositionalConstructorAndDeconstructContract()
    {
        var recordType = typeof(DesignConversionDpoPolicyBaseModelSet);
        var positionalTypes = new[]
        {
            typeof(DesignConversionDpoPolicyBaseIdentity),
            typeof(LlmModel),
            typeof(LlmModel),
            typeof(LlmModel),
            typeof(DesignConversionDpoRuntimeEncodingBinding)
        };
        var constructor = recordType.GetConstructor(positionalTypes);
        Assert.NotNull(constructor);
        Assert.Equal(positionalTypes, constructor!.GetParameters()
            .Select(parameter => parameter.ParameterType));
        var deconstruct = recordType.GetMethod(
            "Deconstruct",
            BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: positionalTypes
                .Select(type => type.MakeByRefType())
                .ToArray(),
            modifiers: null);
        Assert.NotNull(deconstruct);
        Assert.Equal(typeof(void), deconstruct!.ReturnType);
        Assert.All(deconstruct.GetParameters(), parameter => Assert.True(parameter.IsOut));

        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var identity = new DesignConversionDpoPolicyBaseIdentity(
            "checkpoint_backed",
            "explicit_model_config_v1",
            BaseModelId,
            BaseRevision,
            new string('a', 64));
        var policy = new LlmModel(
            DesignConversionDpoPolicyBaseMetadata.CopyConfig(config));
        var reference = new LlmModel(
            DesignConversionDpoPolicyBaseMetadata.CopyConfig(config));
        var freshLoadTarget = new LlmModel(
            DesignConversionDpoPolicyBaseMetadata.CopyConfig(config));
        var tokenizerFixture = CreateCanonicalBpeFixture(config);
        var tokenizerReference = new DesignConversionDpoCanonicalBpeTokenizerSnapshotReference(
            tokenizerFixture.Descriptor,
            config,
            new RecordingCanonicalBpeTokenizerSource(_ =>
                OperationResult<Stream>.Ok(
                    new DisposalTrackingMemoryStream(tokenizerFixture.Bytes))));
        DesignConversionDpoRuntimeEncodingBinding? runtimeEncodingBinding = null;

        var modelSet = new DesignConversionDpoPolicyBaseModelSet(
            identity,
            policy,
            reference,
            freshLoadTarget,
            runtimeEncodingBinding)
        {
            CanonicalBpeTokenizerSnapshot = tokenizerReference
        };
        var (
            deconstructedIdentity,
            deconstructedPolicy,
            deconstructedReference,
            deconstructedFreshLoadTarget,
            deconstructedRuntimeEncodingBinding) = modelSet;

        Assert.Same(identity, deconstructedIdentity);
        Assert.Same(policy, deconstructedPolicy);
        Assert.Same(reference, deconstructedReference);
        Assert.Same(freshLoadTarget, deconstructedFreshLoadTarget);
        Assert.Null(deconstructedRuntimeEncodingBinding);
        Assert.Same(tokenizerReference, modelSet.CanonicalBpeTokenizerSnapshot);
    }

    [Fact]
    public void PolicyContinuationRequest_PreservesFivePositionalConstructorAndAddsParentSnapshotAsInitProperty()
    {
        var requestType = typeof(DesignConversionDpoPolicyContinuationRequest);
        var positionalTypes = new[]
        {
            typeof(IDesignConversionDpoPolicyCheckpointSource),
            typeof(DesignConversionDpoSourceEncoderContract),
            typeof(DesignConversionDpoPolicyCheckpointContract),
            typeof(DesignConversionDpoUtf8ByteTokenizerArtifact),
            typeof(DesignConversionDpoRuntimeEncodingBinding)
        };
        var constructor = requestType.GetConstructor(positionalTypes);
        Assert.NotNull(constructor);
        Assert.Equal(
            positionalTypes,
            constructor!.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.DoesNotContain(
            constructor.GetParameters(),
            parameter => string.Equals(
                parameter.Name,
                nameof(DesignConversionDpoPolicyContinuationRequest
                    .ParentCanonicalBpeTokenizerSnapshot),
                StringComparison.Ordinal));
        var deconstruct = requestType.GetMethod(
            "Deconstruct",
            BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: positionalTypes
                .Select(type => type.MakeByRefType())
                .ToArray(),
            modifiers: null);
        Assert.NotNull(deconstruct);
        Assert.Equal(
            typeof(DesignConversionDpoCanonicalBpeTokenizerSnapshotReference),
            requestType
                .GetProperty(nameof(DesignConversionDpoPolicyContinuationRequest
                    .ParentCanonicalBpeTokenizerSnapshot))!
                .PropertyType);
    }

    [Fact]
    public async Task CanonicalBpeReference_OpenSnapshotAsync_RejectsAndDisposesMutatedReopen()
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var checkpointFixture = CreateCheckpointFixture(config);
        var tokenizerFixture = CreateCanonicalBpeFixture(config);
        var mutated = tokenizerFixture.Bytes.ToArray();
        mutated[^1] ^= 0x01;
        Assert.NotEqual(tokenizerFixture.Bytes, mutated);
        var tokenizerOpenCount = 0;
        var tokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
            OperationResult<Stream>.Ok(new DisposalTrackingMemoryStream(
                Interlocked.Increment(ref tokenizerOpenCount) == 1
                    ? tokenizerFixture.Bytes
                    : mutated)));
        var checkpointSource = new RecordingCheckpointSource(_ =>
            Task.FromResult(OperationResult<Stream>.Ok(
                new DisposalTrackingMemoryStream(checkpointFixture.Bytes))));
        var provider = CreateProvider(
            checkpointSource,
            checkpointFixture.Config,
            checkpointFixture.Sha256,
            tokenizerFixture.Descriptor,
            tokenizerSource);

        var modelSetResult = await provider.CreateModelSetAsync(CancellationToken.None);

        Assert.True(modelSetResult.IsSuccess, modelSetResult.ErrorMessage);
        var reference = AssertCanonicalBpeReference(
            Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(modelSetResult.Value),
            tokenizerFixture.Descriptor);
        var reopened = await reference.OpenSnapshotAsync(CancellationToken.None);

        Assert.False(reopened.IsSuccess);
        Assert.Null(reopened.Value);
        Assert.Equal(
            DesignConversionDpoCanonicalBpeTokenizerSnapshotReference.InvalidErrorCode,
            reopened.ErrorCode);
        Assert.Equal(
            "The DesignConversion DPO canonical BPE tokenizer snapshot is invalid.",
            reopened.ErrorMessage);
        Assert.Equal(2, tokenizerSource.InvocationCount);
        var secondStream = Assert.IsType<DisposalTrackingMemoryStream>(
            tokenizerSource.Streams[1]);
        Assert.True(secondStream.IsDisposed);
        Assert.Equal(1, secondStream.DisposeCount);
        Assert.DoesNotContain(
            tokenizerFixture.Descriptor.Sha256,
            reopened.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/", reopened.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(@"C:\", reopened.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CanonicalBpeReference_OpenSnapshotAsync_PreCancelledDoesNotReopenSource()
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var checkpointFixture = CreateCheckpointFixture(config);
        var tokenizerFixture = CreateCanonicalBpeFixture(config);
        var tokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
            OperationResult<Stream>.Ok(
                new DisposalTrackingMemoryStream(tokenizerFixture.Bytes)));
        var checkpointSource = new RecordingCheckpointSource(_ =>
            Task.FromResult(OperationResult<Stream>.Ok(
                new DisposalTrackingMemoryStream(checkpointFixture.Bytes))));
        var provider = CreateProvider(
            checkpointSource,
            checkpointFixture.Config,
            checkpointFixture.Sha256,
            tokenizerFixture.Descriptor,
            tokenizerSource);
        var modelSetResult = await provider.CreateModelSetAsync(CancellationToken.None);
        Assert.True(modelSetResult.IsSuccess, modelSetResult.ErrorMessage);
        var reference = AssertCanonicalBpeReference(
            Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(modelSetResult.Value),
            tokenizerFixture.Descriptor);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var reopened = await reference.OpenSnapshotAsync(cancellation.Token);

        Assert.False(reopened.IsSuccess);
        Assert.Null(reopened.Value);
        Assert.Equal(
            DesignConversionDpoCanonicalBpeTokenizerSnapshotReference.CancelledErrorCode,
            reopened.ErrorCode);
        Assert.Equal(
            "DesignConversion DPO canonical BPE tokenizer snapshot opening was cancelled.",
            reopened.ErrorMessage);
        Assert.Equal(1, tokenizerSource.InvocationCount);
    }

    [Fact]
    public async Task CanonicalBpeReference_OpenSnapshotAsync_MidReadCancellationDisposesReopen()
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var checkpointFixture = CreateCheckpointFixture(config);
        var tokenizerFixture = CreateCanonicalBpeFixture(config);
        using var cancellation = new CancellationTokenSource();
        CancellingTokenizerSnapshotStream? cancellingStream = null;
        var tokenizerOpenCount = 0;
        var tokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
        {
            if (Interlocked.Increment(ref tokenizerOpenCount) == 1)
            {
                return OperationResult<Stream>.Ok(
                    new DisposalTrackingMemoryStream(tokenizerFixture.Bytes));
            }

            cancellingStream = new CancellingTokenizerSnapshotStream(
                tokenizerFixture.Bytes,
                cancellation);
            return OperationResult<Stream>.Ok(cancellingStream);
        });
        var checkpointSource = new RecordingCheckpointSource(_ =>
            Task.FromResult(OperationResult<Stream>.Ok(
                new DisposalTrackingMemoryStream(checkpointFixture.Bytes))));
        var provider = CreateProvider(
            checkpointSource,
            checkpointFixture.Config,
            checkpointFixture.Sha256,
            tokenizerFixture.Descriptor,
            tokenizerSource);
        var modelSetResult = await provider.CreateModelSetAsync(CancellationToken.None);
        Assert.True(modelSetResult.IsSuccess, modelSetResult.ErrorMessage);
        var reference = AssertCanonicalBpeReference(
            Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(modelSetResult.Value),
            tokenizerFixture.Descriptor);

        var reopened = await reference.OpenSnapshotAsync(cancellation.Token);

        Assert.False(reopened.IsSuccess);
        Assert.Null(reopened.Value);
        Assert.Equal(
            DesignConversionDpoCanonicalBpeTokenizerSnapshotReference.CancelledErrorCode,
            reopened.ErrorCode);
        Assert.NotNull(cancellingStream);
        Assert.True(cancellingStream!.IsDisposed);
        Assert.Equal(1, cancellingStream.DisposeCount);
        Assert.Equal(2, tokenizerSource.InvocationCount);
    }

    [Fact]
    public async Task CreateModelSetAsync_ChangedCanonicalBpeSnapshotFailsBeforeModelSetReturn()
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var checkpointFixture = CreateCheckpointFixture(config);
        var tokenizerFixture = CreateCanonicalBpeFixture(config);
        var mutated = tokenizerFixture.Bytes.ToArray();
        mutated[^1] ^= 0x01;
        Assert.NotEqual(tokenizerFixture.Bytes, mutated);
        var checkpointSource = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("CHECKPOINT_SOURCE_MUST_NOT_RUN"));
        var tokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
            OperationResult<Stream>.Ok(new DisposalTrackingMemoryStream(mutated)));
        var provider = CreateProvider(
            checkpointSource,
            checkpointFixture.Config,
            checkpointFixture.Sha256,
            tokenizerFixture.Descriptor,
            tokenizerSource);

        var result = await provider.CreateModelSetAsync(CancellationToken.None);

        AssertFailure(
            result,
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .CanonicalBpeTokenizerInvalidErrorCode,
            "The DesignConversion DPO canonical BPE tokenizer snapshot is invalid.");
        Assert.Equal(1, tokenizerSource.InvocationCount);
        Assert.NotNull(tokenizerSource.LastStream);
        Assert.True(Assert.IsAssignableFrom<IDisposalProbe>(
            tokenizerSource.LastStream!).IsDisposed);
        Assert.Equal(0, checkpointSource.InvocationCount);
    }

    [Fact]
    public async Task CreateModelSetAsync_ConfigIncompatibleCanonicalBpeVocabularyFailsBeforeCheckpointOpen()
    {
        var descriptorConfig = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var tokenizerFixture = CreateCanonicalBpeFixture(descriptorConfig);
        Assert.True(tokenizerFixture.Descriptor.VocabularySize > 256);
        var incompatibleConfig = descriptorConfig with
        {
            VocabSize = tokenizerFixture.Descriptor.VocabularySize - 1
        };
        var checkpointFixture = CreateCheckpointFixture(incompatibleConfig);
        var checkpointSource = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("CHECKPOINT_SOURCE_MUST_NOT_RUN"));
        var tokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
            OperationResult<Stream>.Ok(
                new DisposalTrackingMemoryStream(tokenizerFixture.Bytes)));
        var provider = CreateProvider(
            checkpointSource,
            checkpointFixture.Config,
            checkpointFixture.Sha256,
            tokenizerFixture.Descriptor,
            tokenizerSource);

        var result = await provider.CreateModelSetAsync(CancellationToken.None);

        AssertFailure(
            result,
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .CanonicalBpeTokenizerInvalidErrorCode,
            "The DesignConversion DPO canonical BPE tokenizer snapshot is invalid.");
        Assert.Equal(1, tokenizerSource.InvocationCount);
        Assert.NotNull(tokenizerSource.LastStream);
        Assert.True(Assert.IsAssignableFrom<IDisposalProbe>(
            tokenizerSource.LastStream!).IsDisposed);
        Assert.Equal(0, checkpointSource.InvocationCount);
    }

    [Fact]
    public async Task CreateContinuedModelSetAsync_RevalidatesCanonicalBpeAndCarriesSnapshotIdentity()
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var tokenizerFixture = CreateCanonicalBpeFixture(config);
        var baseCheckpointFixture = CreateCheckpointFixture(config);
        var continuationFixture = CreateContinuationFixture(
            config,
            BaseModelId,
            BaseRevision,
            baseCheckpointFixture.Sha256);
        var coldCheckpointSource = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("COLD_CHECKPOINT_SOURCE_MUST_NOT_RUN"));
        var tokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
            OperationResult<Stream>.Ok(
                new DisposalTrackingMemoryStream(tokenizerFixture.Bytes)));
        var provider = CreateCurrentProvider(
            coldCheckpointSource,
            config,
            baseCheckpointFixture.Sha256,
            tokenizerFixture.Descriptor,
            tokenizerSource);
        var continuationSource = new RecordingCheckpointSource(_ =>
            Task.FromResult(OperationResult<Stream>.Ok(
                new DisposalTrackingMemoryStream(
                    continuationFixture.PolicyCheckpointBytes))));
        var request = continuationFixture.Request(continuationSource);

        var result = await provider.CreateContinuedModelSetAsync(
            request,
            CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, coldCheckpointSource.InvocationCount);
        Assert.Equal(1, tokenizerSource.InvocationCount);
        Assert.Equal(1, continuationSource.InvocationCount);
        var modelSet = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(result.Value);
        Assert.NotNull(request.CheckpointMutationAdmission);
        Assert.Equal(
            DesignConversionDpoCheckpointMutationAdmissionStatus.CurrentTrusted,
            request.CheckpointMutationAdmission!.Status);
        Assert.Null(typeof(DesignConversionDpoPolicyBaseModelSet).GetProperty(
            "CheckpointMutationAdmission",
            BindingFlags.Public | BindingFlags.Instance));
        Assert.Equal(
            continuationFixture.Contracts.Policy.Binding.PolicyCheckpointSha256,
            modelSet.Identity.BaseCheckpointSha256);
        var expectedRandomContext = DesignConversionDpoPolicyBaseTestProvider
            .CreateCurrentRandomContext();
        Assert.Equal(expectedRandomContext, modelSet.RandomContext);
        DesignConversionDpoPolicyBaseMetadata.ValidateSeededCanonicalLoraTopology(
            modelSet.Policy,
            expectedRandomContext,
            DesignConversionDpoPolicyModelRole.Policy);
        DesignConversionDpoPolicyBaseMetadata.ValidateSeededCanonicalLoraTopology(
            modelSet.Reference,
            expectedRandomContext,
            DesignConversionDpoPolicyModelRole.Reference);
        DesignConversionDpoPolicyBaseMetadata.ValidateSeededCanonicalLoraTopology(
            modelSet.FreshLoadTarget,
            expectedRandomContext,
            DesignConversionDpoPolicyModelRole.FreshLoad);
        Assert.NotNull(modelSet.RuntimeEncodingBinding);
        AssertCanonicalBpeReference(modelSet, tokenizerFixture.Descriptor);
    }

    [Fact]
    public async Task CreateContinuedModelSetAsync_MissingAdmissionFailsBeforeOpeningAnySource()
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var tokenizerFixture = CreateCanonicalBpeFixture(config);
        var baseCheckpointFixture = CreateCheckpointFixture(config);
        var continuationFixture = CreateContinuationFixture(
            config,
            BaseModelId,
            BaseRevision,
            baseCheckpointFixture.Sha256);
        var coldCheckpointSource = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("COLD_CHECKPOINT_SOURCE_MUST_NOT_RUN"));
        var tokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
            throw new InvalidOperationException("TOKENIZER_SOURCE_MUST_NOT_RUN"));
        var provider = CreateCurrentProvider(
            coldCheckpointSource,
            config,
            baseCheckpointFixture.Sha256,
            tokenizerFixture.Descriptor,
            tokenizerSource);
        var continuationSource = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("CONTINUATION_SOURCE_MUST_NOT_RUN"));

        var result = await provider.CreateContinuedModelSetAsync(
            continuationFixture.Request(
                continuationSource,
                includeAdmission: false),
            CancellationToken.None);

        AssertFailure(
            result,
            DesignConversionDpoCheckpointMutationAdmission
                .AttestationRequiredErrorCode,
            DesignConversionDpoCheckpointMutationAdmission
                .SafeAttestationRequiredMessage);
        Assert.Equal(0, coldCheckpointSource.InvocationCount);
        Assert.Equal(0, tokenizerSource.InvocationCount);
        Assert.Equal(0, continuationSource.InvocationCount);
    }

    [Theory]
    [InlineData("hash")]
    [InlineData("length")]
    [InlineData("filename")]
    [InlineData("relative-identity")]
    [InlineData("policy-schema")]
    [InlineData("training-context-schema")]
    [InlineData("legal-commitment")]
    [InlineData("base-sha")]
    public async Task CreateContinuedModelSetAsync_AdmissionMismatchFailsTypedBeforeSource(
        string mismatch)
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var tokenizerFixture = CreateCanonicalBpeFixture(config);
        var baseCheckpointFixture = CreateCheckpointFixture(config);
        var continuationFixture = CreateContinuationFixture(
            config,
            BaseModelId,
            BaseRevision,
            baseCheckpointFixture.Sha256);
        var coldCheckpointSource = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("COLD_CHECKPOINT_SOURCE_MUST_NOT_RUN"));
        var tokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
            throw new InvalidOperationException("TOKENIZER_SOURCE_MUST_NOT_RUN"));
        var provider = CreateCurrentProvider(
            coldCheckpointSource,
            config,
            baseCheckpointFixture.Sha256,
            tokenizerFixture.Descriptor,
            tokenizerSource);
        var continuationSource = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("CONTINUATION_SOURCE_MUST_NOT_RUN"));
        var valid = continuationFixture.Request(continuationSource);
        var policy = valid.PolicyCheckpointContract;
        var binding = policy.Binding;
        var mismatchedPolicy = mismatch switch
        {
            "hash" => policy with
            {
                Binding = binding with
                {
                    PolicyCheckpointSha256 = Hash("wrong-policy-checkpoint")
                }
            },
            "length" => policy with
            {
                Binding = binding with
                {
                    PolicyCheckpointByteLength =
                        binding.PolicyCheckpointByteLength + 1
                }
            },
            "filename" => policy with
            {
                Binding = binding with
                {
                    PolicyCheckpointFileName = DpoTrainer
                        .BuildPolicyCheckpointFileName(policy.DpoStepCount + 1)
                }
            },
            "relative-identity" => policy with
            {
                Binding = binding with
                {
                    ArtifactSetId = "dpo-promotion-set-" +
                        new string('f', 64)
                }
            },
            "policy-schema" => policy with
            {
                Schema = policy.Schema + "-tampered"
            },
            "training-context-schema" => policy with
            {
                TrainingContext = policy.TrainingContext with
                {
                    SchemaVersion =
                        policy.TrainingContext.SchemaVersion + 1
                }
            },
            "legal-commitment" => policy with
            {
                TrainingContext = policy.TrainingContext with
                {
                    LegalTeacherAdmissionCommitment =
                        DesignConversionDpoLegalTeacherAdmissionCommitmentTestFactory
                            .Create(["test:checkpoint-mutation-admission-other"])
                }
            },
            "base-sha" => policy with
            {
                TrainingContext = policy.TrainingContext with
                {
                    BaseCheckpointSha256 = Hash("different-base")
                }
            },
            _ => throw new InvalidOperationException("Unknown mismatch case.")
        };
        var request = valid with { PolicyCheckpointContract = mismatchedPolicy };

        var result = await provider.CreateContinuedModelSetAsync(
            request,
            CancellationToken.None);

        AssertFailure(
            result,
            DesignConversionDpoCheckpointMutationAdmission.MismatchErrorCode,
            DesignConversionDpoCheckpointMutationAdmission.SafeMismatchMessage);
        Assert.Equal(0, coldCheckpointSource.InvocationCount);
        Assert.Equal(0, tokenizerSource.InvocationCount);
        Assert.Equal(0, continuationSource.InvocationCount);
    }

    [Fact]
    public void CheckpointMutationAdmission_ReattestationWrongSourceManifestFailsTyped()
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var baseCheckpointFixture = CreateCheckpointFixture(config);
        var continuationFixture = CreateContinuationFixture(
            config,
            BaseModelId,
            BaseRevision,
            baseCheckpointFixture.Sha256);
        var contracts = continuationFixture.Contracts;

        var result =
            DesignConversionDpoCheckpointMutationAdmission.ReattestTrustedCurrent(
                contracts.Source,
                contracts.Policy,
                contracts.Policy.Binding.PolicyCheckpointSha256,
                contracts.Policy.Binding.PolicyCheckpointByteLength,
                contracts.Policy.Binding.PolicyCheckpointFileName,
                contracts.Source.Binding.SourceEncoderDescriptor
                    .SourceSignalSetSha256,
                Hash("wrong-source-training-manifest"));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            DesignConversionDpoCheckpointMutationAdmission.MismatchErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public void CheckpointMutationAdmission_PublicApiCannotDirectlyConstructAuthority()
    {
        var publicConstructors = typeof(
                DesignConversionDpoCheckpointMutationAdmission)
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        Assert.Empty(publicConstructors);
        Assert.Null(typeof(DesignConversionDpoPolicyContinuationRequest)
            .GetProperty(
                "CheckpointMutationAdmission",
                BindingFlags.Public | BindingFlags.Instance));
        var guardedConstructor = Assert.Single(typeof(
                DesignConversionDpoCheckpointMutationAdmission)
            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        var thrown = Assert.IsType<TargetInvocationException>(
            Record.Exception(() => guardedConstructor.Invoke(
            [
                DesignConversionDpoCheckpointMutationAdmission.CurrentSchema,
                DesignConversionDpoCheckpointMutationAdmissionStatus.CurrentTrusted,
                new string('0', 64),
                1L,
                DpoTrainer.BuildPolicyCheckpointFileName(1),
                "dpo-promotion-set-" + new string('1', 64),
                DesignConversionDpoPromotionArtifactContractCodec
                    .PolicyCheckpointContractSchema,
                4,
                new string('2', 64),
                new string('3', 64),
                new string('4', 64),
                new string('5', 64),
                null!
            ])));
        Assert.IsType<InvalidOperationException>(thrown.InnerException);
    }

    [Theory]
    [InlineData("root")]
    [InlineData("fingerprint")]
    [InlineData("missing-random-context")]
    [InlineData("missing-training-context")]
    public async Task CreateContinuedModelSetAsync_RandomAuthorityMismatchFailsBeforeAnySource(
        string mismatch)
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var tokenizerFixture = CreateCanonicalBpeFixture(config);
        var baseCheckpointFixture = CreateCheckpointFixture(config);
        var binding = CreateCanonicalBpeBinding(
            "random-authority" +
            DesignConversionDpoPromotionArtifactContractCodec
                .CanonicalBpeTokenizerArtifactSuffix,
            tokenizerFixture.Descriptor);
        var continuationFixture = CreateContinuationFixture(
            config,
            BaseModelId,
            BaseRevision,
            baseCheckpointFixture.Sha256,
            binding);
        var coldCheckpointSource = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("COLD_CHECKPOINT_SOURCE_MUST_NOT_RUN"));
        var configuredTokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
            throw new InvalidOperationException("CONFIGURED_TOKENIZER_MUST_NOT_RUN"));
        var provider = CreateCurrentProvider(
            coldCheckpointSource,
            config,
            baseCheckpointFixture.Sha256,
            tokenizerFixture.Descriptor,
            configuredTokenizerSource);
        var continuationSource = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("CONTINUATION_SOURCE_MUST_NOT_RUN"));
        var parentTokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
            throw new InvalidOperationException("PARENT_TOKENIZER_MUST_NOT_RUN"));
        var parentReference =
            new DesignConversionDpoCanonicalBpeTokenizerSnapshotReference(
                tokenizerFixture.Descriptor,
                config,
                parentTokenizerSource);
        var current = DesignConversionDpoPolicyBaseTestProvider
            .CreateCurrentRandomContext();
        DesignConversionDpoPolicyRandomContext? mismatched = mismatch switch
        {
            "root" => DesignConversionDpoPolicyRandomContextContract.CreateCurrent(
                checked(current.RootInitializationSeed + 1UL),
                current.TrainingConfigurationFingerprint),
            "fingerprint" =>
                DesignConversionDpoPolicyRandomContextContract.CreateCurrent(
                    current.RootInitializationSeed,
                    (current.TrainingConfigurationFingerprint[0] == '0' ? "1" : "0") +
                    current.TrainingConfigurationFingerprint[1..]),
            "missing-random-context" or "missing-training-context" => null,
            _ => throw new InvalidOperationException("Unknown mismatch case.")
        };
        var mismatchedPolicy = mismatch == "missing-training-context"
            ? continuationFixture.Contracts.Policy with { TrainingContext = null! }
            : continuationFixture.Contracts.Policy with
            {
                TrainingContext = continuationFixture.Contracts.Policy.TrainingContext with
                {
                    RandomContext = mismatched
                }
            };
        var request = continuationFixture.Request(
            continuationSource,
            parentReference) with
        {
            PolicyCheckpointContract = mismatchedPolicy
        };

        var result = await provider.CreateContinuedModelSetAsync(
            request,
            CancellationToken.None);

        AssertFailure(
            result,
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .ContinuationRandomContextMismatchErrorCode,
            "The DesignConversion DPO continuation random context does not match its provider authority.");
        Assert.Equal(0, coldCheckpointSource.InvocationCount);
        Assert.Equal(0, configuredTokenizerSource.InvocationCount);
        Assert.Equal(0, parentTokenizerSource.InvocationCount);
        Assert.Equal(0, continuationSource.InvocationCount);
    }

    [Fact]
    public async Task CreateContinuedModelSetAsync_ParentCanonicalBpeSnapshotWinsOverConfiguredSource()
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var parentTokenizerFixture = CreateCanonicalBpeFixture(
            config,
            "parent alpha beta gamma delta epsilon zeta ");
        var configuredTokenizerFixture = CreateCanonicalBpeFixture(
            config,
            "configured theta iota kappa lambda mu nu ");
        Assert.NotEqual(
            parentTokenizerFixture.Descriptor.Sha256,
            configuredTokenizerFixture.Descriptor.Sha256);
        var baseCheckpointFixture = CreateCheckpointFixture(config);
        var parentBinding = CreateCanonicalBpeBinding(
            "continuation-parent" +
            DesignConversionDpoPromotionArtifactContractCodec
                .CanonicalBpeTokenizerArtifactSuffix,
            parentTokenizerFixture.Descriptor);
        var continuationFixture = CreateContinuationFixture(
            config,
            BaseModelId,
            BaseRevision,
            baseCheckpointFixture.Sha256,
            parentBinding);
        var coldCheckpointSource = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("COLD_CHECKPOINT_SOURCE_MUST_NOT_RUN"));
        var configuredTokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
            OperationResult<Stream>.Ok(
                new DisposalTrackingMemoryStream(configuredTokenizerFixture.Bytes)));
        var provider = CreateCurrentProvider(
            coldCheckpointSource,
            config,
            baseCheckpointFixture.Sha256,
            configuredTokenizerFixture.Descriptor,
            configuredTokenizerSource);
        var parentTokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
            OperationResult<Stream>.Ok(
                new DisposalTrackingMemoryStream(parentTokenizerFixture.Bytes)));
        var parentReference =
            new DesignConversionDpoCanonicalBpeTokenizerSnapshotReference(
                parentTokenizerFixture.Descriptor,
                config,
                parentTokenizerSource);
        var continuationSource = new RecordingCheckpointSource(_ =>
            Task.FromResult(OperationResult<Stream>.Ok(
                new DisposalTrackingMemoryStream(
                    continuationFixture.PolicyCheckpointBytes))));

        var result = await provider.CreateContinuedModelSetAsync(
            continuationFixture.Request(continuationSource, parentReference),
            CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, coldCheckpointSource.InvocationCount);
        Assert.Equal(0, configuredTokenizerSource.InvocationCount);
        Assert.Equal(0, parentTokenizerSource.InvocationCount);
        Assert.Equal(1, continuationSource.InvocationCount);
        var modelSet = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(result.Value);
        Assert.Same(parentReference, modelSet.CanonicalBpeTokenizerSnapshot);
        var reference = AssertCanonicalBpeReference(
            modelSet,
            parentTokenizerFixture.Descriptor);
        var opened = await reference.OpenSnapshotAsync(CancellationToken.None);
        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        var openedStream = opened.Value!;
        await using (openedStream)
        {
            Assert.Equal(
                parentTokenizerFixture.Bytes,
                await ReadAllBytesAsync(openedStream));
        }
        Assert.Equal(1, parentTokenizerSource.InvocationCount);
        Assert.Equal(0, configuredTokenizerSource.InvocationCount);
    }

    [Fact]
    public async Task CreateContinuedModelSetAsync_CanonicalBindingRejectsNonNullMismatchedParentSnapshot()
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var parentTokenizerFixture = CreateCanonicalBpeFixture(
            config,
            "required parent alpha beta gamma ");
        var otherTokenizerFixture = CreateCanonicalBpeFixture(
            config,
            "required other delta epsilon zeta ");
        Assert.NotEqual(
            parentTokenizerFixture.Descriptor.Sha256,
            otherTokenizerFixture.Descriptor.Sha256);
        var baseCheckpointFixture = CreateCheckpointFixture(config);
        var parentBinding = CreateCanonicalBpeBinding(
            "required-parent" +
            DesignConversionDpoPromotionArtifactContractCodec
                .CanonicalBpeTokenizerArtifactSuffix,
            parentTokenizerFixture.Descriptor);
        var continuationFixture = CreateContinuationFixture(
            config,
            BaseModelId,
            BaseRevision,
            baseCheckpointFixture.Sha256,
            parentBinding);
        var coldCheckpointSource = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("COLD_CHECKPOINT_SOURCE_MUST_NOT_RUN"));
        var configuredTokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
            throw new InvalidOperationException("CONFIGURED_TOKENIZER_MUST_NOT_RUN"));
        var provider = CreateCurrentProvider(
            coldCheckpointSource,
            config,
            baseCheckpointFixture.Sha256,
            otherTokenizerFixture.Descriptor,
            configuredTokenizerSource);
        var continuationSource = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("CONTINUATION_SOURCE_MUST_NOT_RUN"));
        var mismatchedReference =
            new DesignConversionDpoCanonicalBpeTokenizerSnapshotReference(
                otherTokenizerFixture.Descriptor,
                config,
                new RecordingCanonicalBpeTokenizerSource(_ =>
                    OperationResult<Stream>.Ok(
                        new DisposalTrackingMemoryStream(
                            otherTokenizerFixture.Bytes))));

        var mismatched = await provider.CreateContinuedModelSetAsync(
            continuationFixture.Request(continuationSource, mismatchedReference),
            CancellationToken.None);

        AssertFailure(
            mismatched,
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .ContinuationContractInvalidErrorCode,
            "The DesignConversion DPO continuation contract is invalid.");
        Assert.Equal(0, coldCheckpointSource.InvocationCount);
        Assert.Equal(0, configuredTokenizerSource.InvocationCount);
        Assert.Equal(0, continuationSource.InvocationCount);
    }

    [Fact]
    public async Task CreateContinuedModelSetAsync_CanonicalBindingWithoutParentReopensMatchingConfiguredSnapshot()
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var tokenizerFixture = CreateCanonicalBpeFixture(
            config,
            "configured continuation alpha beta gamma delta ");
        var baseCheckpointFixture = CreateCheckpointFixture(config);
        var canonicalBinding = CreateCanonicalBpeBinding(
            "configured-continuation" +
            DesignConversionDpoPromotionArtifactContractCodec
                .CanonicalBpeTokenizerArtifactSuffix,
            tokenizerFixture.Descriptor);
        var continuationFixture = CreateContinuationFixture(
            config,
            BaseModelId,
            BaseRevision,
            baseCheckpointFixture.Sha256,
            canonicalBinding);
        var sourceOrder = new ConcurrentQueue<string>();
        var coldCheckpointSource = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("COLD_CHECKPOINT_SOURCE_MUST_NOT_RUN"));
        var configuredTokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
        {
            sourceOrder.Enqueue("configured-tokenizer");
            return OperationResult<Stream>.Ok(
                new DisposalTrackingMemoryStream(tokenizerFixture.Bytes));
        });
        var provider = CreateCurrentProvider(
            coldCheckpointSource,
            config,
            baseCheckpointFixture.Sha256,
            tokenizerFixture.Descriptor,
            configuredTokenizerSource);
        var continuationSource = new RecordingCheckpointSource(_ =>
        {
            sourceOrder.Enqueue("continuation-checkpoint");
            return Task.FromResult(OperationResult<Stream>.Ok(
                new DisposalTrackingMemoryStream(
                    continuationFixture.PolicyCheckpointBytes)));
        });

        var result = await provider.CreateContinuedModelSetAsync(
            continuationFixture.Request(continuationSource),
            CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, coldCheckpointSource.InvocationCount);
        Assert.Equal(1, configuredTokenizerSource.InvocationCount);
        Assert.Equal(1, continuationSource.InvocationCount);
        Assert.Equal(
            new[] { "configured-tokenizer", "continuation-checkpoint" },
            sourceOrder.ToArray());
        var modelSet = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(result.Value);
        var reference = AssertCanonicalBpeReference(
            modelSet,
            canonicalBinding.Descriptor);
        Assert.Equal(canonicalBinding.Descriptor, reference.Descriptor);
        var openedDuringCreation = Assert.IsType<DisposalTrackingMemoryStream>(
            configuredTokenizerSource.LastStream);
        Assert.True(openedDuringCreation.IsDisposed);
        Assert.Equal(tokenizerFixture.Bytes, openedDuringCreation.ToArray());
    }

    [Fact]
    public async Task CreateModelSetAsync_PreCancelledTokenInvokesNeitherCheckpointNorTokenizerSource()
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var checkpointFixture = CreateCheckpointFixture(config);
        var tokenizerFixture = CreateCanonicalBpeFixture(config);
        var checkpointSource = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("CHECKPOINT_SOURCE_MUST_NOT_RUN"));
        var tokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
            throw new InvalidOperationException("TOKENIZER_SOURCE_MUST_NOT_RUN"));
        var provider = CreateProvider(
            checkpointSource,
            checkpointFixture.Config,
            checkpointFixture.Sha256,
            tokenizerFixture.Descriptor,
            tokenizerSource);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await provider.CreateModelSetAsync(cancellation.Token);

        AssertFailure(result, "CANCELLED", CancelledMessage);
        Assert.Equal(0, checkpointSource.InvocationCount);
        Assert.Equal(0, tokenizerSource.InvocationCount);
    }

    [Fact]
    public async Task CreateModelSetAsync_CancelledCanonicalBpeReadDisposesStreamAndDoesNotOpenCheckpoint()
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var checkpointFixture = CreateCheckpointFixture(config);
        var tokenizerFixture = CreateCanonicalBpeFixture(config);
        using var cancellation = new CancellationTokenSource();
        CancellingTokenizerSnapshotStream? tokenizerStream = null;
        var checkpointSource = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("CHECKPOINT_SOURCE_MUST_NOT_RUN"));
        var tokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
        {
            tokenizerStream = new CancellingTokenizerSnapshotStream(
                tokenizerFixture.Bytes,
                cancellation);
            return OperationResult<Stream>.Ok(tokenizerStream);
        });
        var provider = CreateProvider(
            checkpointSource,
            checkpointFixture.Config,
            checkpointFixture.Sha256,
            tokenizerFixture.Descriptor,
            tokenizerSource);

        var result = await provider.CreateModelSetAsync(cancellation.Token);

        AssertFailure(result, "CANCELLED", CancelledMessage);
        Assert.Equal(1, tokenizerSource.InvocationCount);
        Assert.NotNull(tokenizerStream);
        Assert.True(tokenizerStream!.IsDisposed);
        Assert.Equal(0, checkpointSource.InvocationCount);
    }

    [Fact]
    public async Task CreateModelSetAsync_MissingNoncanonicalOrOversizedCanonicalBpeFailsSafeAndDisposesOpenedStream()
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var checkpointFixture = CreateCheckpointFixture(config);
        var tokenizerFixture = CreateCanonicalBpeFixture(config);
        var privateFailure = "C:/private/tokenizers/aaaaabbbbb/AA==";
        var cases = new (string Name, RecordingCanonicalBpeTokenizerSource Source)[]
        {
            (
                "missing",
                new RecordingCanonicalBpeTokenizerSource(_ =>
                    OperationResult<Stream>.Fail(privateFailure, "PRIVATE_TOKENIZER_CODE"))),
            (
                "noncanonical",
                new RecordingCanonicalBpeTokenizerSource(_ =>
                    OperationResult<Stream>.Ok(
                        new DisposalTrackingMemoryStream(
                            tokenizerFixture.Bytes.Concat([(byte)' ']).ToArray())))),
            (
                "oversized",
                new RecordingCanonicalBpeTokenizerSource(_ =>
                    OperationResult<Stream>.Ok(
                        new ProbeStream(
                            [(byte)'x'],
                            canRead: true,
                            canSeek: true,
                            reportedLength: BpeTokenizer.MaximumCanonicalArtifactBytes + 1L))))
        };

        foreach (var testCase in cases)
        {
            var checkpointSource = new RecordingCheckpointSource(_ =>
                throw new InvalidOperationException("CHECKPOINT_SOURCE_MUST_NOT_RUN"));
            var provider = CreateProvider(
                checkpointSource,
                checkpointFixture.Config,
                checkpointFixture.Sha256,
                tokenizerFixture.Descriptor,
                testCase.Source);

            var result = await provider.CreateModelSetAsync(CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.DoesNotContain(privateFailure, result.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain("aaaaa", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("AA==", result.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain("C:/private", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(1, testCase.Source.InvocationCount);
            Assert.Equal(0, checkpointSource.InvocationCount);
            if (testCase.Source.LastStream is IDisposalProbe disposalProbe)
                Assert.True(disposalProbe.IsDisposed, testCase.Name);
        }
    }

    [Fact]
    public void Create_OnlyOneCanonicalBpeConfigurationInputFailsDeterministically()
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var checkpointFixture = CreateCheckpointFixture(config);
        var checkpointSource = new RecordingCheckpointSource(_ =>
            throw new InvalidOperationException("CHECKPOINT_SOURCE_MUST_NOT_RUN"));
        var tokenizerFixture = CreateCanonicalBpeFixture(config);
        var tokenizerSource = new RecordingCanonicalBpeTokenizerSource(_ =>
            throw new InvalidOperationException("TOKENIZER_SOURCE_MUST_NOT_RUN"));

        var descriptorOnly = DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
            checkpointSource,
            checkpointFixture.Config,
            BaseModelId,
            BaseRevision,
            checkpointFixture.Sha256,
            tokenizerFixture.Descriptor,
            canonicalBpeTokenizerSnapshotSource: null);
        var sourceOnly = DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
            checkpointSource,
            checkpointFixture.Config,
            BaseModelId,
            BaseRevision,
            checkpointFixture.Sha256,
            expectedCanonicalBpeTokenizerArtifactDescriptor: null,
            tokenizerSource);

        AssertConfigurationInvalid(descriptorOnly);
        AssertConfigurationInvalid(sourceOnly);
        Assert.Equal(0, checkpointSource.InvocationCount);
        Assert.Equal(0, tokenizerSource.InvocationCount);
    }

    [Fact]
    public async Task CreateModelSetAsync_SourceCancelsSuppliedTokenBeforeReturningFailureReturnsCancelled()
    {
        const string privateFailure = "C:/private/checkpoints/CANCELLED-SOURCE-FAILURE";
        var fixture = CreateCheckpointFixture();
        using var cancellationSource = new CancellationTokenSource();
        var source = new RecordingCheckpointSource(token =>
        {
            Assert.Equal(cancellationSource.Token, token);
            cancellationSource.Cancel();
            return Task.FromResult(OperationResult<Stream>.Fail(
                privateFailure,
                "PRIVATE_CANCELLED_SOURCE_CODE"));
        });
        var provider = CreateProvider(source, fixture.Config, fixture.Sha256);

        var result = await provider.CreateModelSetAsync(cancellationSource.Token);

        AssertFailure(result, "CANCELLED", CancelledMessage);
        Assert.Equal(1, source.InvocationCount);
        Assert.Null(result.Value);
        Assert.DoesNotContain(privateFailure, result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateModelSetAsync_SourceCancelsSuppliedTokenBeforeThrowingNonFatalExceptionReturnsCancelled()
    {
        const string privateFailure = "C:/private/checkpoints/CANCELLED-SOURCE-EXCEPTION";
        var fixture = CreateCheckpointFixture();
        using var cancellationSource = new CancellationTokenSource();
        var source = new RecordingCheckpointSource(token =>
        {
            Assert.Equal(cancellationSource.Token, token);
            cancellationSource.Cancel();
            throw new IOException(privateFailure);
        });
        var provider = CreateProvider(source, fixture.Config, fixture.Sha256);

        var result = await provider.CreateModelSetAsync(cancellationSource.Token);

        AssertFailure(result, "CANCELLED", CancelledMessage);
        Assert.Equal(1, source.InvocationCount);
        Assert.Null(result.Value);
        Assert.DoesNotContain(privateFailure, result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateModelSetAsync_SourceCancelsSuppliedTokenBeforeReturningStreamDisposesStreamAndReturnsCancelled()
    {
        var fixture = CreateCheckpointFixture();
        var stream = new DisposalTrackingMemoryStream(fixture.Bytes);
        using var cancellationSource = new CancellationTokenSource();
        var source = new RecordingCheckpointSource(token =>
        {
            Assert.Equal(cancellationSource.Token, token);
            cancellationSource.Cancel();
            return Task.FromResult(OperationResult<Stream>.Ok(stream));
        });
        var provider = CreateProvider(source, fixture.Config, fixture.Sha256);

        var result = await provider.CreateModelSetAsync(cancellationSource.Token);

        AssertFailure(result, "CANCELLED", CancelledMessage);
        Assert.Equal(1, source.InvocationCount);
        Assert.True(stream.IsDisposed);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task CreateModelSetAsync_SourceFailuresAreStableSanitizedAndSideEffectFree()
    {
        const string privateFailure = "C:/private/checkpoints/PRIVATE-SOURCE-FAILURE";
        var fixture = CreateCheckpointFixture();
        var sources = new[]
        {
            new RecordingCheckpointSource(_ => Task.FromResult(
                OperationResult<Stream>.Fail(privateFailure, "PRIVATE_SOURCE_CODE"))),
            new RecordingCheckpointSource(_ => throw new IOException(privateFailure))
        };

        foreach (var source in sources)
        {
            var provider = CreateProvider(source, fixture.Config, fixture.Sha256);

            var result = await provider.CreateModelSetAsync(CancellationToken.None);

            AssertFailure(
                result,
                DesignConversionCheckpointBackedDpoPolicyBaseProvider.CheckpointSourceFailedErrorCode,
                SourceFailedMessage);
            Assert.Equal(1, source.InvocationCount);
            Assert.DoesNotContain(privateFailure, result.ErrorMessage, StringComparison.Ordinal);
            Assert.DoesNotContain("C:/private", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task CreateModelSetAsync_DirectSuppliedCancellationMapsButOtherCancellationAndFatalGraphsEscape()
    {
        var fixture = CreateCheckpointFixture();
        using var suppliedCancellation = new CancellationTokenSource();
        var suppliedCancellationSource = new RecordingCheckpointSource(_ =>
        {
            suppliedCancellation.Cancel();
            return Task.FromException<OperationResult<Stream>>(
                new OperationCanceledException(suppliedCancellation.Token));
        });
        var suppliedCancellationProvider = CreateProvider(
            suppliedCancellationSource,
            fixture.Config,
            fixture.Sha256);

        var cancelled = await suppliedCancellationProvider.CreateModelSetAsync(
            suppliedCancellation.Token);

        AssertFailure(cancelled, "CANCELLED", CancelledMessage);
        Assert.Equal(1, suppliedCancellationSource.InvocationCount);

        var unassociatedCancellation = new OperationCanceledException(
            "PRIVATE-UNASSOCIATED-CANCELLATION");
        var unassociatedSource = new RecordingCheckpointSource(_ =>
            Task.FromException<OperationResult<Stream>>(unassociatedCancellation));
        var unassociatedProvider = CreateProvider(
            unassociatedSource,
            fixture.Config,
            fixture.Sha256);
        var actualCancellation = await Assert.ThrowsAsync<OperationCanceledException>(
            () => unassociatedProvider.CreateModelSetAsync(CancellationToken.None));
        Assert.Same(unassociatedCancellation, actualCancellation);

        var nestedCancellation = new AggregateException(
            "PRIVATE-NESTED-CANCELLATION",
            new IOException("wrapper"),
            new OperationCanceledException("nested"));
        var nestedCancellationSource = new RecordingCheckpointSource(_ =>
            Task.FromException<OperationResult<Stream>>(nestedCancellation));
        var nestedCancellationProvider = CreateProvider(
            nestedCancellationSource,
            fixture.Config,
            fixture.Sha256);
        var actualNestedCancellation = await Assert.ThrowsAsync<AggregateException>(
            () => nestedCancellationProvider.CreateModelSetAsync(CancellationToken.None));
        Assert.Same(nestedCancellation, actualNestedCancellation);

        var fatalGraph = new AggregateException(
            "PRIVATE-FATAL-GRAPH",
            new InvalidOperationException(
                "wrapper",
                new OutOfMemoryException("fatal")));
        var fatalSource = new RecordingCheckpointSource(_ =>
            Task.FromException<OperationResult<Stream>>(fatalGraph));
        var fatalProvider = CreateProvider(fatalSource, fixture.Config, fixture.Sha256);
        var actualFatal = await Assert.ThrowsAsync<AggregateException>(
            () => fatalProvider.CreateModelSetAsync(CancellationToken.None));
        Assert.Same(fatalGraph, actualFatal);
    }

    [Fact]
    public async Task CreateModelSetAsync_InvalidStreamsFailWithoutReadingOversizedStreamAndDispose()
    {
        var fixture = CreateCheckpointFixture();
        var oversized = new ProbeStream(
            [0x01],
            canRead: true,
            canSeek: true,
            reportedLength: (16L * 1024 * 1024 * 1024) + 1);
        var cases = new (string Name, Func<Stream?> Factory)[]
        {
            ("null", static () => null),
            ("unreadable", () => new ProbeStream(fixture.Bytes, canRead: false, canSeek: true)),
            ("unseekable", () => new ProbeStream(fixture.Bytes, canRead: true, canSeek: false)),
            ("nonzero", () => new ProbeStream(fixture.Bytes, canRead: true, canSeek: true, initialPosition: 1)),
            ("empty", static () => new ProbeStream([], canRead: true, canSeek: true)),
            ("oversized", () => oversized)
        };

        foreach (var testCase in cases)
        {
            Stream? issued = null;
            var source = new RecordingCheckpointSource(_ =>
            {
                issued = testCase.Factory();
                return Task.FromResult(OperationResult<Stream>.Ok(issued!));
            });
            var provider = CreateProvider(source, fixture.Config, fixture.Sha256);

            var result = await provider.CreateModelSetAsync(CancellationToken.None);

            AssertFailure(
                result,
                DesignConversionCheckpointBackedDpoPolicyBaseProvider.CheckpointStreamInvalidErrorCode,
                StreamInvalidMessage);
            Assert.Equal(1, source.InvocationCount);
            if (issued is IDisposalProbe disposalProbe)
                Assert.True(disposalProbe.IsDisposed, $"{testCase.Name} stream was not disposed.");
        }

        Assert.Equal(0, oversized.ReadCount);
    }

    [Fact]
    public async Task CreateModelSetAsync_HashMismatchFailsBeforeModelConstructionAndDisposesStream()
    {
        var fixture = CreateCheckpointFixture();
        var stream = new DisposalTrackingMemoryStream(fixture.Bytes);
        var source = new RecordingCheckpointSource(_ =>
            Task.FromResult(OperationResult<Stream>.Ok(stream)));
        var mismatchedHash = new string(
            fixture.Sha256[0] == 'a' ? 'b' : 'a',
            fixture.Sha256.Length);
        var provider = CreateProvider(source, fixture.Config, mismatchedHash);

        var result = await provider.CreateModelSetAsync(CancellationToken.None);

        AssertFailure(
            result,
            DesignConversionCheckpointBackedDpoPolicyBaseProvider.CheckpointHashMismatchErrorCode,
            HashMismatchMessage);
        Assert.True(stream.IsDisposed);
        Assert.Equal(1, source.InvocationCount);
    }

    [Fact]
    public async Task CreateModelSetAsync_MalformedAndConfigMismatchedArtifactsAreInvalidAndDisposed()
    {
        var fixture = CreateCheckpointFixture();
        var malformedBytes = Enumerable.Range(0, 97).Select(value => (byte)value).ToArray();
        var malformedHash = Hash(malformedBytes);
        var malformedStream = new DisposalTrackingMemoryStream(malformedBytes);
        var malformedSource = new RecordingCheckpointSource(_ =>
            Task.FromResult(OperationResult<Stream>.Ok(malformedStream)));
        var malformedProvider = CreateProvider(
            malformedSource,
            fixture.Config,
            malformedHash);

        var malformed = await malformedProvider.CreateModelSetAsync(CancellationToken.None);

        AssertFailure(
            malformed,
            DesignConversionCheckpointBackedDpoPolicyBaseProvider.CheckpointInvalidErrorCode,
            CheckpointInvalidMessage);
        Assert.True(malformedStream.IsDisposed);

        var mismatchedConfig = fixture.Config with
        {
            RopeTheta = fixture.Config.RopeTheta + 1
        };
        var mismatchStream = new DisposalTrackingMemoryStream(fixture.Bytes);
        var mismatchSource = new RecordingCheckpointSource(_ =>
            Task.FromResult(OperationResult<Stream>.Ok(mismatchStream)));
        var mismatchProvider = CreateProvider(
            mismatchSource,
            mismatchedConfig,
            fixture.Sha256);

        var mismatch = await mismatchProvider.CreateModelSetAsync(CancellationToken.None);

        AssertFailure(
            mismatch,
            DesignConversionCheckpointBackedDpoPolicyBaseProvider.CheckpointInvalidErrorCode,
            CheckpointInvalidMessage);
        Assert.True(mismatchStream.IsDisposed);
        Assert.Equal(1, malformedSource.InvocationCount);
        Assert.Equal(1, mismatchSource.InvocationCount);
    }

    [Fact]
    public async Task CreateModelSetAsync_ChangedSnapshotBeforeFinalHashFailsAndDisposesStream()
    {
        var fixture = CreateCheckpointFixture();
        var changedBytes = fixture.Bytes.ToArray();
        changedBytes[^1] ^= 0x01;
        var stream = new ChangeOnFinalHashStream(fixture.Bytes, changedBytes);
        var source = new RecordingCheckpointSource(_ =>
            Task.FromResult(OperationResult<Stream>.Ok(stream)));
        var provider = CreateProvider(source, fixture.Config, fixture.Sha256);

        var result = await provider.CreateModelSetAsync(CancellationToken.None);

        AssertFailure(
            result,
            DesignConversionCheckpointBackedDpoPolicyBaseProvider.CheckpointChangedDuringLoadErrorCode,
            ChangedDuringLoadMessage);
        Assert.True(stream.IsDisposed);
        Assert.True(stream.ChangedForFinalHash);
        Assert.Equal(1, source.InvocationCount);
    }

    [Fact]
    public void ProviderSurfaceExposesSnapshotAndContinuationCreationWithoutPathTrainingOrStepVectorActivation()
    {
        var providerType = typeof(DesignConversionCheckpointBackedDpoPolicyBaseProvider);
        Assert.Empty(providerType.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        var publicInstanceMethods = providerType.GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToArray();
        var methodsByName = publicInstanceMethods
            .GroupBy(method => method.Name)
            .ToDictionary(group => group.Key, group => group.ToArray());
        Assert.Equal(
            new[]
            {
                nameof(IDesignConversionDpoPolicyBaseProvider.DescribePreparation),
                nameof(IDesignConversionDpoPolicyBaseProvider.CreateModelSetAsync),
                nameof(IDesignConversionDpoPolicyContinuationProvider.CreateContinuedModelSetAsync),
                nameof(IDesignConversionDpoPolicyContinuationProvider.CreateContinuationVerificationModelSetAsync)
            }.Order(StringComparer.Ordinal),
            methodsByName.Keys.Order(StringComparer.Ordinal));
        AssertProviderSurfaceSignature(
            Assert.Single(methodsByName[
                nameof(IDesignConversionDpoPolicyBaseProvider.DescribePreparation)]),
            typeof(OperationResult<
                DesignConversionDpoPolicyBasePreparationDescriptor>));
        var coldMethods = methodsByName[
            nameof(IDesignConversionDpoPolicyBaseProvider.CreateModelSetAsync)];
        AssertProviderSurfaceSignature(
            Assert.Single(coldMethods, method => method.GetParameters().Length == 1),
            typeof(Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>),
            typeof(CancellationToken));
        AssertProviderSurfaceSignature(
            Assert.Single(coldMethods, method => method.GetParameters().Length == 2),
            typeof(Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>),
            typeof(DesignConversionDpoPolicyBasePreparationDescriptor),
            typeof(CancellationToken));
        var continuationMethods = methodsByName[
            nameof(IDesignConversionDpoPolicyContinuationProvider
                .CreateContinuedModelSetAsync)];
        AssertProviderSurfaceSignature(
            Assert.Single(
                continuationMethods,
                method => method.GetParameters().Length == 2),
            typeof(Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>),
            typeof(DesignConversionDpoPolicyContinuationRequest),
            typeof(CancellationToken));
        AssertProviderSurfaceSignature(
            Assert.Single(
                continuationMethods,
                method => method.GetParameters().Length == 3),
            typeof(Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>),
            typeof(DesignConversionDpoPolicyBasePreparationDescriptor),
            typeof(DesignConversionDpoPolicyContinuationRequest),
            typeof(CancellationToken));
        Assert.DoesNotContain(
            providerType.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                BindingFlags.Static | BindingFlags.DeclaredOnly),
            method => method.Name.Contains("StepVector", StringComparison.Ordinal) ||
                      method.Name.Contains("Train", StringComparison.Ordinal) ||
                      method.GetParameters().Any(parameter =>
                          parameter.ParameterType == typeof(string) &&
                          parameter.Name?.Contains("path", StringComparison.OrdinalIgnoreCase) == true));

        var sourceMethod = Assert.Single(
            typeof(IDesignConversionDpoPolicyCheckpointSource).GetMethods());
        Assert.Equal(nameof(IDesignConversionDpoPolicyCheckpointSource.OpenCheckpointSnapshotAsync), sourceMethod.Name);
        Assert.Equal(typeof(Task<OperationResult<Stream>>), sourceMethod.ReturnType);
        AssertProviderSurfaceSignature(
            sourceMethod,
            typeof(Task<OperationResult<Stream>>),
            typeof(CancellationToken));
    }

    private static void AssertProviderSurfaceSignature(
        MethodInfo method,
        Type expectedReturnType,
        params Type[] expectedParameterTypes)
    {
        Assert.Equal(expectedReturnType, method.ReturnType);
        var parameters = method.GetParameters();
        Assert.Equal(expectedParameterTypes.Length, parameters.Length);
        for (var index = 0; index < expectedParameterTypes.Length; index++)
        {
            Assert.Equal(expectedParameterTypes[index], parameters[index].ParameterType);
            Assert.False(parameters[index].IsOptional);
            Assert.False(parameters[index].HasDefaultValue);
        }
    }

    private static DesignConversionCheckpointBackedDpoPolicyBaseProvider CreateProvider(
        IDesignConversionDpoPolicyCheckpointSource source,
        ModelConfig config,
        string expectedHash,
        DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor?
            expectedCanonicalBpeTokenizerArtifactDescriptor = null,
        IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource?
            canonicalBpeTokenizerSnapshotSource = null)
    {
        var created = DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
            source,
            config,
            BaseModelId,
            BaseRevision,
            expectedHash,
            expectedCanonicalBpeTokenizerArtifactDescriptor,
            canonicalBpeTokenizerSnapshotSource);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        return Assert.IsType<DesignConversionCheckpointBackedDpoPolicyBaseProvider>(created.Value);
    }

    private static DesignConversionCheckpointBackedDpoPolicyBaseProvider
        CreateCurrentProvider(
            IDesignConversionDpoPolicyCheckpointSource source,
            ModelConfig config,
            string expectedHash,
            DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor?
                expectedCanonicalBpeTokenizerArtifactDescriptor = null,
            IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource?
                canonicalBpeTokenizerSnapshotSource = null)
    {
        var created = DesignConversionCheckpointBackedDpoPolicyBaseProvider
            .CreateWithOptions(
                source,
                config,
                BaseModelId,
                BaseRevision,
                expectedHash,
                DesignConversionDpoPolicyBaseTestProvider
                    .CreateCurrentProviderOptions(),
                expectedCanonicalBpeTokenizerArtifactDescriptor,
                canonicalBpeTokenizerSnapshotSource);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        return Assert.IsType<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>(created.Value);
    }

    private static CheckpointFixture CreateCheckpointFixture(ModelConfig? modelConfig = null)
    {
        var config = modelConfig is null
            ? new ModelConfig
            {
                VocabSize = 256,
                ContextLen = 8,
                HiddenDim = 8,
                NumLayers = 1,
                NumHeads = 2,
                NumKvHeads = 1,
                FfnHiddenDim = 16,
                RopeTheta = 7_777f,
                RmsNormEps = 2e-5f
            }
            : DesignConversionDpoPolicyBaseMetadata.CopyConfig(modelConfig);
        var root = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-checkpoint-provider-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "policy-v2.bin");
        Directory.CreateDirectory(root);
        try
        {
            new LlmModel(config).SaveCheckpoint(path);
            var bytes = File.ReadAllBytes(path);
            return new CheckpointFixture(config, bytes, Hash(bytes));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static CanonicalBpeFixture CreateCanonicalBpeFixture(
        ModelConfig config,
        string corpus = "aaaaabbbbbccccc alpha beta gamma ")
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(
            string.Concat(Enumerable.Repeat(corpus, 32)),
            vocabSize: 258);
        Assert.Equal(258, tokenizer.VocabSize);

        using var stream = new MemoryStream();
        tokenizer.Save(stream);
        var bytes = stream.ToArray();
        var descriptor = DesignConversionDpoPromotionArtifactContractCodec
            .BuildCanonicalBpeTokenizerArtifactDescriptor(bytes, config);
        Assert.True(descriptor.IsSuccess, descriptor.ErrorMessage);
        return new CanonicalBpeFixture(bytes, descriptor.Value!);
    }

    private static DesignConversionDpoCanonicalBpeTokenizerArtifactBinding
        CreateCanonicalBpeBinding(
            string fileName,
            DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor descriptor)
        => new(
            fileName,
            descriptor.Sha256,
            descriptor.ByteLength,
            descriptor);

    private static ContinuationFixture CreateContinuationFixture(
        ModelConfig config,
        string baseModelId,
        string baseRevision,
        string baseCheckpointSha256,
        DesignConversionDpoCanonicalBpeTokenizerArtifactBinding?
            canonicalBpeTokenizer = null)
    {
        var model = new LlmModel(
            DesignConversionDpoPolicyBaseMetadata.CopyConfig(config));
        var randomContext = DesignConversionDpoPolicyBaseTestProvider
            .CreateCurrentRandomContext();
        DesignConversionDpoPolicyBaseMetadata.AttachSeededCanonicalLoraTopology(
            model,
            randomContext,
            DesignConversionDpoPolicyModelRole.Policy);
        var checkpointBytes = SaveCheckpointBytes(model);
        var records = new[]
        {
            BuildTrainingRecord(
                "checkpoint-provider-continuation-row-a",
                DesignConversionDpoUnits.ImproverUnit,
                0.97),
            BuildTrainingRecord(
                "checkpoint-provider-continuation-row-b",
                DesignConversionDpoUnits.CriticUnit,
                0.96)
        };
        var descriptorResult = DesignConversionDpoPolicyCheckpointBridge
            .BuildSourceEncoderDescriptorPreviewForTests(
                records,
                config,
                new DesignConversionDpoPolicyCheckpointBridgeOptions(
                    MaxBridgePreferenceCount: 2,
                    DpoStepCount: 1));
        Assert.True(descriptorResult.IsSuccess, descriptorResult.ErrorMessage);
        var descriptor = descriptorResult.Value!;
        var tokenizerResult = DesignConversionDpoPromotionArtifactContractCodec
            .BuildCanonicalTokenizerArtifact(descriptor);
        Assert.True(tokenizerResult.IsSuccess, tokenizerResult.ErrorMessage);
        var tokenizerBytes = JsonSerializer.SerializeToUtf8Bytes(
            tokenizerResult.Value!,
            new JsonSerializerOptions(DesignConversionCheckpointJsonOptions.Standard)
            {
                WriteIndented = false
            });
        var checkpointSha = Hash(checkpointBytes);
        var binding = new DesignConversionDpoPromotionArtifactBinding(
            ArtifactSetId: string.Empty,
            PolicyCheckpointFileName: DpoTrainer.BuildPolicyCheckpointFileName(1),
            PolicyCheckpointSha256: checkpointSha,
            PolicyCheckpointByteLength: checkpointBytes.LongLength,
            RuntimeCheckpointFileName: "continuation-runtime.json",
            RuntimeCheckpointSha256: Hash("runtime-checkpoint"),
            RuntimeCheckpointByteLength: 1,
            TokenizerArtifactFileName: "continuation" +
                DesignConversionDpoPromotionArtifactContractCodec.TokenizerArtifactSuffix,
            TokenizerArtifactSha256: Hash(tokenizerBytes),
            TokenizerArtifactByteLength: tokenizerBytes.LongLength,
            SourceEncoderDescriptor: descriptor)
        {
            CanonicalBpeTokenizer = canonicalBpeTokenizer
        };
        binding = binding with
        {
            ArtifactSetId = DesignConversionDpoPromotionArtifactContractCodec
                .BuildArtifactSetId(
                    binding.PolicyCheckpointFileName,
                    binding.PolicyCheckpointSha256,
                    binding.PolicyCheckpointByteLength,
                    binding.RuntimeCheckpointFileName,
                    binding.RuntimeCheckpointSha256,
                    binding.RuntimeCheckpointByteLength,
                    binding.TokenizerArtifactFileName,
                    binding.TokenizerArtifactSha256,
                    binding.TokenizerArtifactByteLength,
                    binding.SourceEncoderDescriptor,
                    evaluation: null,
                    canonicalBpeTokenizer)
        };
        var source = new DesignConversionDpoSourceEncoderContract(
            DesignConversionDpoPromotionArtifactContractCodec
                .SourceEncoderContractSchema,
            "design_conversion_dpo_source_encoder_contract",
            binding);
        var context = new DesignConversionDpoPolicyCheckpointTrainingContext(
            ConfigKind: "explicit_model_config_v1",
            VocabSize: config.VocabSize,
            ContextLength: config.ContextLen,
            HiddenDim: config.HiddenDim,
            NumLayers: config.NumLayers,
            NumHeads: config.NumHeads,
            NumKvHeads: config.NumKvHeads,
            FfnHiddenDim: config.FfnHiddenDim,
            RawPayloadPersisted: true,
            AbsolutePathPersisted: false,
            SchemaVersion: 4,
            RopeTheta: config.RopeTheta,
            RmsNormEps: config.RmsNormEps,
            ProviderKind: "checkpoint_backed",
            BaseModelId: baseModelId,
            BaseRevision: baseRevision,
            BaseCheckpointSha256: baseCheckpointSha256,
            ConfigSha256: DesignConversionDpoPolicyBaseMetadata
                .ComputeConfigSha256(config),
            InitialWeightSha256: Hash("checkpoint-provider-continuation-weights"),
            Lora: DesignConversionDpoPolicyBaseMetadata
                .CreateCanonicalLoraContext(config, config.NumLayers * 2)) with
        {
            LegalTeacherAdmissionCommitment =
                DesignConversionDpoLegalTeacherAdmissionCommitmentTestFactory.Create(
                    ["test:checkpoint-backed-policy-base"]),
            RandomContext = randomContext
        };
        var policy = new DesignConversionDpoPolicyCheckpointContract(
            DesignConversionDpoPromotionArtifactContractCodec
                .PolicyCheckpointContractSchema,
            "design_conversion_dpo_policy_checkpoint_contract",
            binding,
            "11.DPO:DpoTrainer",
            "11.DPO:DpoTrainer.StepVector:fresh_loaded",
            DesignConversionDpoPolicyCheckpointBridge.DirectInferenceRuntimeBridgeKind,
            FreshLoadVerified: true,
            ConvertedToRuntimeWeights: true,
            SourceRecordCount: 2,
            BridgePreferenceCount: 2,
            DpoStepCount: 1,
            TrainingContext: context,
            RawPayloadPersisted: false,
            AbsolutePathPersisted: false);
        var runtimeBinding = DesignConversionDpoPromotionArtifactContractCodec
            .BuildRuntimeEncodingBinding(source, tokenizerResult.Value!);
        Assert.True(runtimeBinding.IsSuccess, runtimeBinding.ErrorMessage);
        var contracts = new ContinuationContracts(
            source,
            policy,
            tokenizerResult.Value!,
            runtimeBinding.Value!);
        Assert.True(
            DesignConversionDpoPromotionArtifactContractCodec.ValidateContractPair(
                contracts.Source,
                contracts.Policy));
        return new ContinuationFixture(checkpointBytes, contracts);
    }

    private static DesignConversionDpoTrainingRecord BuildTrainingRecord(
        string id,
        string unitId,
        double usefulness)
    {
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords(
            [DesignConversionDpoGeneratorTests.CreateExample(id, unitId, usefulness)]);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.QuarantinedRecords);
        return Assert.Single(build.Value.AcceptedRecords);
    }

    private static byte[] SaveCheckpointBytes(LlmModel model)
    {
        using var stream = new MemoryStream();
        model.SaveCheckpointToStream(stream);
        return stream.ToArray();
    }

    private static DesignConversionDpoCanonicalBpeTokenizerSnapshotReference
        AssertCanonicalBpeReference(
            DesignConversionDpoPolicyBaseModelSet modelSet,
            DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor expected)
    {
        var reference = Assert.IsType<DesignConversionDpoCanonicalBpeTokenizerSnapshotReference>(
            modelSet.CanonicalBpeTokenizerSnapshot);
        Assert.Equal(expected, reference.Descriptor);
        Assert.False(reference.Descriptor.RawPayloadPersisted);
        Assert.False(reference.Descriptor.AbsolutePathPersisted);
        return reference;
    }

    private static void AssertPublicTokenizerReferenceIsPathAndPayloadFree(
        DesignConversionDpoCanonicalBpeTokenizerSnapshotReference reference)
    {
        var properties = reference.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);
        Assert.Equal(
            new[] { nameof(DesignConversionDpoCanonicalBpeTokenizerSnapshotReference.Descriptor) },
            properties.Select(property => property.Name).Order(StringComparer.Ordinal));
        Assert.Empty(reference.GetType().GetFields(
            BindingFlags.Public | BindingFlags.Instance));
        var publicData = string.Join(
            "|",
            reference.Descriptor.Schema,
            reference.Descriptor.ArtifactKind,
            reference.Descriptor.Sha256,
            reference.Descriptor.ByteLength.ToString(),
            reference.Descriptor.VocabularySize.ToString(),
            reference.Descriptor.CanonicalStateIdentitySha256,
            reference.Descriptor.RawPayloadPersisted.ToString(),
            reference.Descriptor.AbsolutePathPersisted.ToString());
        Assert.DoesNotContain("aaaaa", publicData, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AA==", publicData, StringComparison.Ordinal);
        Assert.DoesNotContain(@"C:\", publicData, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/", publicData, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertConfigurationInvalid(
        OperationResult<DesignConversionCheckpointBackedDpoPolicyBaseProvider> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            DesignConversionCheckpointBackedDpoPolicyBaseProvider.ConfigurationInvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(ConfigurationInvalidMessage, result.ErrorMessage);
    }

    private static string Hash(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string Hash(string value)
        => Hash(Encoding.UTF8.GetBytes(value));

    private static void AssertFailure(
        OperationResult<DesignConversionDpoPolicyBaseModelSet> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
        Assert.Null(result.Value);
    }

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream)
    {
        stream.Position = 0;
        using var copy = new MemoryStream();
        await stream.CopyToAsync(copy);
        return copy.ToArray();
    }

    private static void AssertModelsPhysicallyIndependentAndBitwiseEqual(
        LlmModel left,
        LlmModel right)
    {
        Assert.NotSame(left, right);
        Assert.NotSame(left.Config, right.Config);
        var leftEntries = left.GetCheckpointParameterEntries();
        var rightEntries = right.GetCheckpointParameterEntries();
        Assert.Equal(leftEntries.Count, rightEntries.Count);
        for (var index = 0; index < leftEntries.Count; index++)
        {
            Assert.Equal(leftEntries[index].Key, rightEntries[index].Key);
            Assert.Equal(
                leftEntries[index].Parameter.Value.Shape.Dimensions,
                rightEntries[index].Parameter.Value.Shape.Dimensions);
            Assert.NotSame(leftEntries[index].Parameter, rightEntries[index].Parameter);
            Assert.NotSame(leftEntries[index].Parameter.Value, rightEntries[index].Parameter.Value);
            Assert.NotSame(
                leftEntries[index].Parameter.Value.Storage,
                rightEntries[index].Parameter.Value.Storage);
            Assert.NotSame(
                leftEntries[index].Parameter.Value.Data,
                rightEntries[index].Parameter.Value.Data);
            Assert.Equal(
                leftEntries[index].Parameter.Value.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray(),
                rightEntries[index].Parameter.Value.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray());
        }
    }

    private static IReadOnlyList<int[]> SnapshotParameterBits(LlmModel model)
        => model.GetCheckpointParameterEntries()
            .Select(entry => entry.Parameter.Value.Data
                .Select(BitConverter.SingleToInt32Bits)
                .ToArray())
            .ToArray();

    private static void AssertParameterBitsEqual(
        IReadOnlyList<int[]> expected,
        LlmModel actual)
    {
        var actualEntries = actual.GetCheckpointParameterEntries();
        Assert.Equal(expected.Count, actualEntries.Count);
        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(
                expected[index],
                actualEntries[index].Parameter.Value.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray());
        }
    }

    private sealed record CheckpointFixture(
        ModelConfig Config,
        byte[] Bytes,
        string Sha256);

    private sealed record CanonicalBpeFixture(
        byte[] Bytes,
        DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor Descriptor);

    private sealed record ContinuationFixture(
        byte[] PolicyCheckpointBytes,
        ContinuationContracts Contracts)
    {
        public DesignConversionDpoPolicyContinuationRequest Request(
            IDesignConversionDpoPolicyCheckpointSource checkpointSource,
            DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?
                parentCanonicalBpeTokenizerSnapshot = null,
            bool includeAdmission = true)
            => new(
                checkpointSource,
                Contracts.Source,
                Contracts.Policy,
                Contracts.TokenizerArtifact,
                Contracts.RuntimeEncodingBinding)
            {
                ParentCanonicalBpeTokenizerSnapshot =
                    parentCanonicalBpeTokenizerSnapshot,
                CheckpointMutationAdmission = includeAdmission
                    ? CreateAdmission(Contracts)
                    : null
            };
    }

    private sealed record ContinuationContracts(
        DesignConversionDpoSourceEncoderContract Source,
        DesignConversionDpoPolicyCheckpointContract Policy,
        DesignConversionDpoUtf8ByteTokenizerArtifact TokenizerArtifact,
        DesignConversionDpoRuntimeEncodingBinding RuntimeEncodingBinding);

    private static DesignConversionDpoCheckpointMutationAdmission CreateAdmission(
        ContinuationContracts contracts)
    {
        var admission =
            DesignConversionDpoCheckpointMutationAdmission.CreateTrustedCurrent(
                contracts.Source,
                contracts.Policy,
                contracts.Policy.Binding.PolicyCheckpointSha256,
                contracts.Policy.Binding.PolicyCheckpointByteLength,
                contracts.Policy.Binding.PolicyCheckpointFileName,
                contracts.Source.Binding.SourceEncoderDescriptor
                    .SourceSignalSetSha256);
        Assert.True(admission.IsSuccess, admission.ErrorMessage);
        return admission.Value!;
    }

    private sealed class RecordingCheckpointSource(
        Func<CancellationToken, Task<OperationResult<Stream>>> open)
        : IDesignConversionDpoPolicyCheckpointSource
    {
        private int _invocationCount;

        public int InvocationCount => Volatile.Read(ref _invocationCount);

        public Task<OperationResult<Stream>> OpenCheckpointSnapshotAsync(
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _invocationCount);
            return open(cancellationToken);
        }
    }

    private sealed class RecordingCanonicalBpeTokenizerSource(
        Func<CancellationToken, OperationResult<Stream>> open)
        : IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource
    {
        private readonly ConcurrentQueue<Stream> _streams = new();
        private int _invocationCount;

        public int InvocationCount => Volatile.Read(ref _invocationCount);
        public Stream? LastStream { get; private set; }
        public IReadOnlyList<Stream> Streams => _streams.ToArray();

        public Task<OperationResult<Stream>> OpenCanonicalBpeTokenizerSnapshotAsync(
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _invocationCount);
            var result = open(cancellationToken);
            if (result.Value is not null)
            {
                LastStream = result.Value;
                _streams.Enqueue(result.Value);
            }

            return Task.FromResult(result);
        }
    }

    private sealed class RendezvousCheckpointSource(byte[] bytes)
        : IDesignConversionDpoPolicyCheckpointSource
    {
        private readonly TaskCompletionSource _bothCallsEntered = new(
            TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly ConcurrentQueue<DisposalTrackingMemoryStream> _streams = new();
        private int _invocationCount;
        private int _activeCallCount;
        private int _maximumConcurrentCallCount;
        private int _activeCallCountAtRelease;

        public int InvocationCount => Volatile.Read(ref _invocationCount);
        public int ActiveCallCount => Volatile.Read(ref _activeCallCount);
        public int MaximumConcurrentCallCount =>
            Volatile.Read(ref _maximumConcurrentCallCount);
        public int ActiveCallCountAtRelease =>
            Volatile.Read(ref _activeCallCountAtRelease);
        public IReadOnlyList<DisposalTrackingMemoryStream> Streams =>
            _streams.ToArray();

        public async Task<OperationResult<Stream>> OpenCheckpointSnapshotAsync(
            CancellationToken cancellationToken)
        {
            var invocation = Interlocked.Increment(ref _invocationCount);
            var active = Interlocked.Increment(ref _activeCallCount);
            UpdateMaximumConcurrentCallCount(active);
            if (invocation == 2)
            {
                Volatile.Write(ref _activeCallCountAtRelease, active);
                _bothCallsEntered.TrySetResult();
            }

            try
            {
                await _bothCallsEntered.Task.WaitAsync(cancellationToken);
                var stream = new DisposalTrackingMemoryStream(bytes);
                _streams.Enqueue(stream);
                return OperationResult<Stream>.Ok(stream);
            }
            finally
            {
                Interlocked.Decrement(ref _activeCallCount);
            }
        }

        private void UpdateMaximumConcurrentCallCount(int candidate)
        {
            while (true)
            {
                var observed = Volatile.Read(ref _maximumConcurrentCallCount);
                if (candidate <= observed)
                    return;
                if (Interlocked.CompareExchange(
                        ref _maximumConcurrentCallCount,
                        candidate,
                        observed) == observed)
                {
                    return;
                }
            }
        }
    }

    private interface IDisposalProbe
    {
        bool IsDisposed { get; }
    }

    private sealed class DisposalTrackingMemoryStream(byte[] bytes)
        : MemoryStream(bytes, writable: false), IDisposalProbe
    {
        private int _disposed;
        private int _disposeCount;

        public bool IsDisposed => Volatile.Read(ref _disposed) != 0;
        public int DisposeCount => Volatile.Read(ref _disposeCount);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Interlocked.Exchange(ref _disposed, 1);
                Interlocked.Increment(ref _disposeCount);
            }
            base.Dispose(disposing);
        }
    }

    private sealed class CancellingTokenizerSnapshotStream(
        byte[] bytes,
        CancellationTokenSource cancellation)
        : MemoryStream(bytes, writable: false), IDisposalProbe
    {
        private int _disposed;
        private int _disposeCount;

        public bool IsDisposed => Volatile.Read(ref _disposed) != 0;
        public int DisposeCount => Volatile.Read(ref _disposeCount);

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = base.Read(buffer, offset, Math.Min(count, 13));
            if (read > 0)
                cancellation.Cancel();
            return read;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Interlocked.Exchange(ref _disposed, 1);
                Interlocked.Increment(ref _disposeCount);
            }
            base.Dispose(disposing);
        }
    }

    private sealed class ProbeStream : Stream, IDisposalProbe
    {
        private readonly byte[] _bytes;
        private readonly bool _canRead;
        private readonly bool _canSeek;
        private readonly long _reportedLength;
        private long _position;
        private int _disposed;

        public ProbeStream(
            byte[] bytes,
            bool canRead,
            bool canSeek,
            long? reportedLength = null,
            long initialPosition = 0)
        {
            _bytes = bytes;
            _canRead = canRead;
            _canSeek = canSeek;
            _reportedLength = reportedLength ?? bytes.LongLength;
            _position = initialPosition;
        }

        public int ReadCount { get; private set; }
        public bool IsDisposed => Volatile.Read(ref _disposed) != 0;
        public override bool CanRead => _canRead;
        public override bool CanSeek => _canSeek;
        public override bool CanWrite => false;
        public override long Length => _reportedLength;
        public override long Position
        {
            get => _position;
            set
            {
                if (!_canSeek)
                    throw new NotSupportedException();
                _position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ReadCount++;
            if (!_canRead)
                throw new NotSupportedException();
            if (_position >= _bytes.LongLength)
                return 0;
            var read = (int)Math.Min(count, _bytes.LongLength - _position);
            Array.Copy(_bytes, _position, buffer, offset, read);
            _position += read;
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!_canSeek)
                throw new NotSupportedException();
            Position = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => checked(_position + offset),
                SeekOrigin.End => checked(_reportedLength + offset),
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };
            return _position;
        }

        public override void Flush()
        {
        }

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Interlocked.Exchange(ref _disposed, 1);
            base.Dispose(disposing);
        }
    }

    private sealed class ChangeOnFinalHashStream : Stream, IDisposalProbe
    {
        private readonly byte[] _initialBytes;
        private readonly byte[] _changedBytes;
        private long _position;
        private int _rewindCount;
        private int _disposed;

        public ChangeOnFinalHashStream(byte[] initialBytes, byte[] changedBytes)
        {
            Assert.Equal(initialBytes.Length, changedBytes.Length);
            _initialBytes = initialBytes;
            _changedBytes = changedBytes;
        }

        public bool ChangedForFinalHash => Volatile.Read(ref _rewindCount) >= 5;
        public bool IsDisposed => Volatile.Read(ref _disposed) != 0;
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _initialBytes.LongLength;
        public override long Position
        {
            get => _position;
            set
            {
                if (value == 0 && _position != 0)
                    Interlocked.Increment(ref _rewindCount);
                _position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var activeBytes = ChangedForFinalHash ? _changedBytes : _initialBytes;
            if (_position >= activeBytes.LongLength)
                return 0;
            var read = (int)Math.Min(count, activeBytes.LongLength - _position);
            Array.Copy(activeBytes, _position, buffer, offset, read);
            _position += read;
            return read;
        }

        public override int Read(Span<byte> buffer)
        {
            var activeBytes = ChangedForFinalHash ? _changedBytes : _initialBytes;
            if (_position >= activeBytes.LongLength)
                return 0;
            var read = (int)Math.Min(buffer.Length, activeBytes.LongLength - _position);
            activeBytes.AsSpan((int)_position, read).CopyTo(buffer);
            _position += read;
            return read;
        }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(Read(buffer.Span));
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            Position = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => checked(_position + offset),
                SeekOrigin.End => checked(Length + offset),
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };
            return _position;
        }

        public override void Flush()
        {
        }

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Interlocked.Exchange(ref _disposed, 1);
            base.Dispose(disposing);
        }
    }
}
