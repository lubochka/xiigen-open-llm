// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using DPO;
using TensorCore.Core;
using TensorCore.Init;
using TensorCore.Layers;
using TransformerModel;
using TransformerModel.Config;

public sealed class DesignConversionDpoPolicyContinuationTests
{

    // Доказательство неизменности сверочных моделей после переноса построения
    // из моста в поставщика: набор, полученный у поставщика, обязан быть
    // побайтово тем же состоянием, что строил закрытый метод моста до переноса.
    // Именно это состояние хешируется в отпечаток, поэтому равенство байтов
    // означает равенство отпечатка.
    [Fact]
    public async Task ProviderSuppliedVerificationSetMatchesBridgeOwnedConstructionByteForByte()
    {
        // Настройка и случайный контекст — те же, что передаёт мост.
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        var randomContext = DesignConversionDpoPolicyBaseTestProvider
            .CreateCurrentRandomContext();
        var provider = new DesignConversionDpoPolicyBaseTestProvider();

        // Набор от поставщика: новый вход узкого договора продолжения.
        // Строим под детерминированным посевом, чтобы базовые веса моделей не
        // были случайными: иначе различие по роли невозможно наблюдать, потому
        // что случайная основа перекрывает посев малых поправок.
        OperationResult<DesignConversionDpoPolicyBaseModelSet> supplied;
        using (Initializers.UseDeterministicSeed(
            DesignConversionDpoPolicyBaseTestProvider
                .CurrentRootInitializationSeed))
        {
            supplied = await provider
                .CreateContinuationVerificationModelSetAsync(
                    config,
                    randomContext,
                    CancellationToken.None);
        }
        Assert.True(supplied.IsSuccess, supplied.ErrorMessage);
        Assert.NotNull(supplied.Value);
        // Отдельный счётчик сверочных обращений увеличился ровно на один.
        Assert.Equal(1, provider.VerificationInvocationCount);
        // Прежний счётчик обращений к поставщику не тронут: сверочный вход
        // однократность обычного пути не нарушает.
        Assert.Equal(0, provider.InvocationCount);

        // Три роли обязаны быть тремя разными объектами, иначе загрузка
        // контрольной точки в один испортила бы остальные.
        Assert.NotSame(supplied.Value!.Policy, supplied.Value.Reference);
        Assert.NotSame(supplied.Value.Policy, supplied.Value.FreshLoadTarget);
        Assert.NotSame(supplied.Value.Reference, supplied.Value.FreshLoadTarget);

        // Эталон строим ровно так, как строил удалённый закрытый метод моста:
        // копия настройки плюс посев канонной топологии по той же роли.
        // Эталон строим под тем же посевом и в том же порядке ролей, иначе
        // сравнение потеряет смысл.
        LlmModel expectedPolicy;
        LlmModel expectedReference;
        LlmModel expectedFreshLoad;
        using (Initializers.UseDeterministicSeed(
            DesignConversionDpoPolicyBaseTestProvider
                .CurrentRootInitializationSeed))
        {
            expectedPolicy = BuildBridgeOwnedStyleModel(
                config,
                randomContext,
                DesignConversionDpoPolicyModelRole.Policy);
            expectedReference = BuildBridgeOwnedStyleModel(
                config,
                randomContext,
                DesignConversionDpoPolicyModelRole.Reference);
            expectedFreshLoad = BuildBridgeOwnedStyleModel(
                config,
                randomContext,
                DesignConversionDpoPolicyModelRole.FreshLoad);
        }

        // Главное условие роли: ДО загрузки контрольной точки состояние
        // каждой роли у поставщика обязано совпасть с эталоном той же роли
        // побайтово. Это и есть проверка, что посев канонной топологии сделан
        // той же функцией и с той же ролью, а не какой-то другой.
        Assert.Equal(
            CheckpointBytes(expectedPolicy),
            CheckpointBytes(supplied.Value.Policy));
        Assert.Equal(
            CheckpointBytes(expectedReference),
            CheckpointBytes(supplied.Value.Reference));
        Assert.Equal(
            CheckpointBytes(expectedFreshLoad),
            CheckpointBytes(supplied.Value.FreshLoadTarget));

        // Контроль, что сравнение различает роли: под одним посевом состояния
        // трёх ролей обязаны отличаться друг от друга. Если бы поставщик
        // выдал одну и ту же роль трижды, предыдущие три утверждения прошли
        // бы вырожденно, а это утверждение упадёт.
        Assert.NotEqual(
            CheckpointBytes(supplied.Value.Policy),
            CheckpointBytes(supplied.Value.Reference));
        Assert.NotEqual(
            CheckpointBytes(supplied.Value.Policy),
            CheckpointBytes(supplied.Value.FreshLoadTarget));

        // До загрузки контрольной точки базовые веса модели случайны — так
        // было и в удалённом закрытом методе моста, поэтому сравнивать их
        // побайтово нельзя. Мост сравнивает состояние ПОСЛЕ загрузки
        // допущенной контрольной точки, и именно это состояние хешируется в
        // отпечаток. Поэтому загружаем одну и ту же контрольную точку в оба
        // набора и сравниваем результат.
        var checkpoint = CheckpointBytes(expectedPolicy);
        LoadCheckpointBytes(expectedPolicy, checkpoint);
        LoadCheckpointBytes(expectedReference, checkpoint);
        LoadCheckpointBytes(expectedFreshLoad, checkpoint);
        LoadCheckpointBytes(supplied.Value.Policy, checkpoint);
        LoadCheckpointBytes(supplied.Value.Reference, checkpoint);
        LoadCheckpointBytes(supplied.Value.FreshLoadTarget, checkpoint);

        // Побайтовое равенство состояния по каждой роли после загрузки.
        Assert.Equal(
            CheckpointBytes(expectedPolicy),
            CheckpointBytes(supplied.Value.Policy));
        Assert.Equal(
            CheckpointBytes(expectedReference),
            CheckpointBytes(supplied.Value.Reference));
        Assert.Equal(
            CheckpointBytes(expectedFreshLoad),
            CheckpointBytes(supplied.Value.FreshLoadTarget));

        // Контрольный признак, что сравнение не вырождено: до загрузки
        // состояния отличались, значит равенство получено загрузкой, а не
        // тем, что сравниваются одни и те же объекты.
        Assert.NotSame(expectedPolicy, supplied.Value.Policy);
    }

    // Загрузка контрольной точки в модель из байтов, как это делает мост.
    private static void LoadCheckpointBytes(LlmModel model, byte[] checkpoint)
    {
        using var buffer = new MemoryStream(checkpoint, writable: false);
        model.LoadCheckpoint(buffer);
    }

    // Новая точка отказа после переноса: путь продолжения на поставщике без
    // договора продолжения обязан отказывать закрытым кодом, а не строить
    // модели самостоятельно и не проходить молча.
    [Fact]
    public async Task ContinuationVerificationSetIsFailClosedWithoutContinuationContract()
    {
        // Поставщик только базового договора договором продолжения не является,
        // и именно это проверяет приведение типа внутри моста.
        var baseOnly = new BaseOnlyPolicyProviderForVerificationGate();
        Assert.IsNotAssignableFrom<
            IDesignConversionDpoPolicyContinuationProvider>(baseOnly);

        // Поставщик договора продолжения, который сверочный вход не
        // переопределил, обязан получить отказ по умолчанию с точным кодом.
        // Ссылку держим типом договора: отказ по умолчанию живёт на
        // интерфейсе и через конкретный класс не вызывается.
        IDesignConversionDpoPolicyContinuationProvider withoutOverride =
            new ContinuationProviderWithoutVerificationOverride();
        var refused = await withoutOverride
            .CreateContinuationVerificationModelSetAsync(
                DesignConversionDpoPolicyBaseTestProvider.CreateConfig(),
                DesignConversionDpoPolicyBaseTestProvider
                    .CreateCurrentRandomContext(),
                CancellationToken.None);
        Assert.False(refused.IsSuccess);
        Assert.Equal(
            IDesignConversionDpoPolicyContinuationProvider
                .VerificationModelSetUnavailableErrorCode,
            refused.ErrorCode);
    }

    // Эталонная сборка одной модели в стиле удалённого закрытого метода моста.
    private static LlmModel BuildBridgeOwnedStyleModel(
        ModelConfig config,
        DesignConversionDpoPolicyRandomContext randomContext,
        DesignConversionDpoPolicyModelRole role)
    {
        var model = new LlmModel(
            DesignConversionDpoPolicyBaseMetadata.CopyConfig(config));
        DesignConversionDpoPolicyBaseMetadata.AttachSeededCanonicalLoraTopology(
            model,
            randomContext,
            role);
        return model;
    }

    // Состояние модели снимаем в память, чтобы сравнивать именно байты.
    private static byte[] CheckpointBytes(LlmModel model)
    {
        using var buffer = new MemoryStream();
        model.SaveCheckpointToStream(buffer);
        return buffer.ToArray();
    }

    // Поставщик только базового договора: нужен, чтобы показать, что
    // приведение к договору продолжения на нём не проходит.
    private sealed class BaseOnlyPolicyProviderForVerificationGate
        : IDesignConversionDpoPolicyBaseProvider
    {
        public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
            CreateModelSetAsync(CancellationToken cancellationToken)
            => Task.FromResult(
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                    "base-only provider must not be used for continuation",
                    "BASE_ONLY_PROVIDER"));
    }

    // Поставщик договора продолжения без переопределения сверочного входа:
    // нужен, чтобы показать отказ по умолчанию с точным кодом.
    private sealed class ContinuationProviderWithoutVerificationOverride
        : IDesignConversionDpoPolicyContinuationProvider
    {
        public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
            CreateModelSetAsync(CancellationToken cancellationToken)
            => Task.FromResult(
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                    "cold path must not be used for this gate",
                    "COLD_PATH_NOT_USED"));

        public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
            CreateContinuedModelSetAsync(
                DesignConversionDpoPolicyContinuationRequest continuationRequest,
                CancellationToken cancellationToken)
            => Task.FromResult(
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                    "continuation path must not be used for this gate",
                    "CONTINUATION_PATH_NOT_USED"));
    }

    [Fact]
    public void BridgeContinuationSnapshotOwnsValueGraphAndRetainsOpaqueCheckpointSource()
    {
        using var fixture = new ContinuationFixture();
        var checkpointSource = fixture.SourceFor(fixture.ParentCheckpointBytes);
        var request = fixture.Request(
            fixture.ParentContracts,
            checkpointSource,
            fixture.ParentCheckpointBytes,
            ContinuationFixture.ParentDpoStepCount);
        var snapshotMethod = typeof(DesignConversionDpoPolicyCheckpointBridge)
            .GetMethod(
                "SnapshotContinuationRequest",
                BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(snapshotMethod);

        var snapshot = Assert.IsType<DesignConversionDpoPolicyContinuationRequest>(
            snapshotMethod!.Invoke(null, [request]));

        Assert.NotSame(request, snapshot);
        Assert.Same(checkpointSource, snapshot.CheckpointSource);
        Assert.NotSame(request.SourceEncoderContract, snapshot.SourceEncoderContract);
        Assert.NotSame(request.SourceEncoderContract.Binding, snapshot.SourceEncoderContract.Binding);
        Assert.NotSame(
            request.SourceEncoderContract.Binding.SourceEncoderDescriptor,
            snapshot.SourceEncoderContract.Binding.SourceEncoderDescriptor);
        Assert.NotSame(request.PolicyCheckpointContract, snapshot.PolicyCheckpointContract);
        Assert.NotSame(request.PolicyCheckpointContract.Binding, snapshot.PolicyCheckpointContract.Binding);
        Assert.NotSame(
            request.PolicyCheckpointContract.TrainingContext,
            snapshot.PolicyCheckpointContract.TrainingContext);
        Assert.NotSame(
            request.PolicyCheckpointContract.TrainingContext.RandomContext,
            snapshot.PolicyCheckpointContract.TrainingContext.RandomContext);
        Assert.Equal(
            request.PolicyCheckpointContract.TrainingContext.RandomContext,
            snapshot.PolicyCheckpointContract.TrainingContext.RandomContext);
        Assert.NotSame(
            request.PolicyCheckpointContract.TrainingContext.Lora,
            snapshot.PolicyCheckpointContract.TrainingContext.Lora);
        Assert.NotSame(
            request.PolicyCheckpointContract.TrainingContext.Lora!.TargetModules,
            snapshot.PolicyCheckpointContract.TrainingContext.Lora!.TargetModules);
        Assert.NotSame(request.TokenizerArtifact, snapshot.TokenizerArtifact);
        Assert.NotSame(
            request.TokenizerArtifact!.Vocabulary,
            snapshot.TokenizerArtifact!.Vocabulary);
        Assert.NotSame(request.RuntimeEncodingBinding, snapshot.RuntimeEncodingBinding);
        Assert.Same(
            request.ParentCanonicalBpeTokenizerSnapshot,
            snapshot.ParentCanonicalBpeTokenizerSnapshot);
    }

    [Fact]
    public async Task BridgeContinuationSnapshotPrecedesLeaseAcquisitionAliasMutation()
    {
        using var fixture = new ContinuationFixture();
        var checkpointSource = fixture.SourceFor(fixture.ParentCheckpointBytes);
        var request = fixture.Request(
            fixture.ParentContracts,
            checkpointSource,
            fixture.ParentCheckpointBytes,
            ContinuationFixture.ParentDpoStepCount);
        var aliasedTargets = Assert.IsType<string[]>(
            request.PolicyCheckpointContract.TrainingContext.Lora!.TargetModules);
        var observedLeaseAcquisitions = 0;
        var priorLeaseObserver =
            DesignConversionDpoPolicyCheckpointBridge.LeaseAcquisitionObserverForTesting;
        try
        {
            DesignConversionDpoPolicyCheckpointBridge.LeaseAcquisitionObserverForTesting =
                boundary =>
                {
                    Assert.Equal("dpo_bridge:standalone:lease_acquisition", boundary);
                    Interlocked.Increment(ref observedLeaseAcquisitions);
                    aliasedTargets[0] = "foreign_proj";
                };

            var result = await ExecuteContinuationBridgeAsync(
                fixture,
                fixture.Provider,
                request,
                "snapshot-before-lease-alias-mutation");

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(1, observedLeaseAcquisitions);
            Assert.Equal("foreign_proj", aliasedTargets[0]);
            Assert.Equal(1, checkpointSource.CallCount);
            Assert.NotNull(result.Value!.PolicyCheckpointRef);
        }
        finally
        {
            DesignConversionDpoPolicyCheckpointBridge.LeaseAcquisitionObserverForTesting =
                priorLeaseObserver;
        }
    }

    [Fact]
    public async Task ColdApisRemainAvailableButLegacyBridgeShimsFailClosed()
    {
        using var fixture = new ContinuationFixture();
        var coldBytes = fixture.CreateColdCheckpointBytes();
        var coldSource = new CountingCheckpointSource(
            _ => OperationResult<Stream>.Ok(new TrackingMemoryStream(coldBytes)));
        var created = DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
            coldSource,
            fixture.Config,
            fixture.BaseModelId,
            fixture.BaseRevision,
            Hash(coldBytes));
        Assert.True(created.IsSuccess, created.ErrorMessage);

        var modelSet = await created.Value!.CreateModelSetAsync(CancellationToken.None);
        Assert.True(modelSet.IsSuccess, modelSet.ErrorMessage);
        Assert.Equal(1, coldSource.CallCount);

        var provider = new DesignConversionDpoPolicyBaseTestProvider();
#pragma warning disable CS0618
        var legacyTokenOverload = await
            DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                Array.Empty<DesignConversionDpoTrainingRecord>(),
                fixture.OutputDirectory,
                "legacy-token-overload",
                fixture.RuntimeCheckpointPath,
                provider,
                CancellationToken.None);
        var legacyPositionalNullOptions = await
            DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
                Array.Empty<DesignConversionDpoTrainingRecord>(),
                fixture.OutputDirectory,
                "legacy-null-options",
                fixture.RuntimeCheckpointPath,
                provider,
                null);
#pragma warning restore CS0618

        Assert.Equal(
            "LEGAL_TEACHER_CONFIGURATION_REQUIRED",
            legacyTokenOverload.ErrorCode);
        Assert.Equal(
            legacyTokenOverload.ErrorCode,
            legacyPositionalNullOptions.ErrorCode);
        Assert.Equal(0, provider.InvocationCount);
    }

    [Fact]
    public async Task ColdBridge_SameSeedAuthorityAndBaseAreDeterministic_ChangedRootDiverges()
    {
        using var fixture = new ContinuationFixture();
        var coldBytes = fixture.CreateColdCheckpointBytes();
        var coldSha256 = Hash(coldBytes);
        var roots = new[]
        {
            DesignConversionDpoPolicyBaseTestProvider.CurrentRootInitializationSeed,
            DesignConversionDpoPolicyBaseTestProvider.CurrentRootInitializationSeed,
            checked(DesignConversionDpoPolicyBaseTestProvider
                .CurrentRootInitializationSeed + 1UL)
        };
        var executions = new List<ColdExecutionSnapshot>();

        for (var index = 0; index < roots.Length; index++)
        {
            var source = new CountingCheckpointSource(
                _ => OperationResult<Stream>.Ok(
                    new TrackingMemoryStream(coldBytes)));
            var created = DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .CreateWithOptions(
                    source,
                    fixture.Config,
                    fixture.BaseModelId,
                    fixture.BaseRevision,
                    coldSha256,
                    new DesignConversionDpoPolicyBaseProviderOptions(
                        roots[index],
                        DesignConversionDpoPolicyBaseTestProvider
                            .CurrentTrainingConfigurationFingerprint));
            Assert.True(created.IsSuccess, created.ErrorMessage);
            var provider = new CapturingColdProvider(created.Value!);
            var paths = fixture.ResolvePublicationPaths(
                $"cold-seed-authority-{index}");

            var result = await DesignConversionDpoPolicyCheckpointBridge
                .ExecuteAndConvertAsync(
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    fixture.Records,
                    paths.OutputDirectory,
                    paths.SafeRunId,
                    paths.RuntimeCheckpointPath,
                    provider,
                    new DesignConversionDpoPolicyCheckpointBridgeOptions(
                        MaxBridgePreferenceCount: 2,
                        DpoStepCount: 1),
                    CancellationToken.None);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            var value = Assert.IsType<
                DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>(
                    result.Value);
            Assert.Equal(1, source.CallCount);
            Assert.Equal(fixture.Records.Count, value.SourceRecordCount);
            Assert.Equal(2, value.BridgePreferenceCount);
            Assert.Equal(1, value.DpoStepCount);
            Assert.True(value.PolicyCheckpointFreshLoadVerified);
            Assert.Equal(
                DesignConversionDpoTrainerRouteBGate.RuntimeIngredientIds(),
                value.IngredientIds);
            Assert.False(value.RuntimeCheckpoint.HasTrainingScaffoldHistory);
            Assert.InRange(
                value.RuntimeCheckpoint.AccuracyAfterLastBatch,
                0d,
                1d);
            var modelSet = Assert.IsType<DesignConversionDpoPolicyBaseModelSet>(
                provider.LastModelSet);
            Assert.Equal(1, provider.DescribePreparationCallCount);
            Assert.Equal(0, provider.LegacyCreateModelSetCallCount);
            Assert.Equal(1, provider.DescriptorCreateModelSetCallCount);
            Assert.Same(
                provider.LastDescribedPreparation,
                provider.LastCreatePreparation);
            Assert.Same(modelSet, provider.LastModelSet);
            var randomContext = Assert.IsType<DesignConversionDpoPolicyRandomContext>(
                modelSet.RandomContext);
            Assert.Equal(roots[index], randomContext.RootInitializationSeed);
            var policyRef = Assert.IsType<DesignConversionDpoPolicyCheckpointRef>(
                value.PolicyCheckpointRef);
            Assert.Equal(randomContext, policyRef.TrainingContext.RandomContext);
            var rootEvidenceRef =
                $"{DesignConversionDpoPolicyRandomContextContract.RootInitializationSeedEvidencePrefix}" +
                roots[index].ToString(CultureInfo.InvariantCulture);
            Assert.Contains(
                rootEvidenceRef,
                policyRef.EvidenceRefs);
            executions.Add(CaptureColdExecution(
                value,
                paths,
                modelSet,
                rootEvidenceRef));
        }

        Assert.Equal(
            executions[0].SeedPreparation.RandomSeeds,
            executions[1].SeedPreparation.RandomSeeds);
        Assert.Equal(
            executions[0].RootInitializationSeedEvidenceRef,
            executions[1].RootInitializationSeedEvidenceRef);
        Assert.Equal(
            executions[0].SeedPreparation.AdapterInitializationValueBits.Length,
            executions[1].SeedPreparation.AdapterInitializationValueBits.Length);
        for (var index = 0;
             index < executions[0].SeedPreparation
                 .AdapterInitializationValueBits.Length;
             index++)
        {
            Assert.Equal(
                executions[0].SeedPreparation
                    .AdapterInitializationValueBits[index],
                executions[1].SeedPreparation
                    .AdapterInitializationValueBits[index]);
        }
        Assert.Equal(
            executions[0].PolicyCheckpointSha256,
            executions[1].PolicyCheckpointSha256);
        Assert.Equal(
            executions[0].PolicyCheckpointByteLength,
            executions[1].PolicyCheckpointByteLength);
        Assert.Equal(
            executions[0].PolicyCheckpointBytes,
            executions[1].PolicyCheckpointBytes);
        Assert.Equal(
            executions[0].SourceRecordCount,
            executions[1].SourceRecordCount);
        Assert.Equal(
            executions[0].BridgePreferenceCount,
            executions[1].BridgePreferenceCount);
        Assert.Equal(executions[0].DpoStepCount, executions[1].DpoStepCount);
        Assert.Equal(
            executions[0].PolicyCheckpointFreshLoadVerified,
            executions[1].PolicyCheckpointFreshLoadVerified);
        Assert.Equal(
            executions[0].PolicyCheckpointPath,
            executions[1].PolicyCheckpointPath);
        Assert.Equal(executions[0].IngredientIds, executions[1].IngredientIds);
        Assert.Equal(executions[0].EvidenceRefs, executions[1].EvidenceRefs);
        Assert.Equal(
            executions[0].PolicyCheckpointReferenceBytes,
            executions[1].PolicyCheckpointReferenceBytes);
        Assert.Equal(
            executions[0].SourceEncoderDescriptorBytes,
            executions[1].SourceEncoderDescriptorBytes);
        Assert.Equal(
            executions[0].RuntimeAccuracyAfterLastBatch,
            executions[1].RuntimeAccuracyAfterLastBatch);
        Assert.Equal(
            executions[0].RuntimeHasTrainingScaffoldHistory,
            executions[1].RuntimeHasTrainingScaffoldHistory);
        Assert.Equal(
            executions[0].NormalizedTrainingHistoryBytes,
            executions[1].NormalizedTrainingHistoryBytes);
        Assert.Equal(
            executions[0].NormalizedRuntimeCheckpointBytes,
            executions[1].NormalizedRuntimeCheckpointBytes);
        Assert.Equal(
            executions[0].RuntimeExportAccuracyAtExport,
            executions[1].RuntimeExportAccuracyAtExport);
        Assert.Equal(
            executions[0].NormalizedRuntimeExportBytes,
            executions[1].NormalizedRuntimeExportBytes);
        Assert.Equal(
            executions[0].NormalizedRuntimeCheckpointPath,
            executions[1].NormalizedRuntimeCheckpointPath);

        Assert.NotEqual(
            executions[0].RootInitializationSeedEvidenceRef,
            executions[2].RootInitializationSeedEvidenceRef);
        Assert.DoesNotContain(
            executions[0].RootInitializationSeedEvidenceRef,
            executions[2].PolicyCheckpointReferenceEvidenceRefs);
        Assert.DoesNotContain(
            executions[2].RootInitializationSeedEvidenceRef,
            executions[0].PolicyCheckpointReferenceEvidenceRefs);
        Assert.Contains(
            Enumerable.Range(
                0,
                executions[0].SeedPreparation.RandomSeeds.Length),
            index => executions[0].SeedPreparation.RandomSeeds[index] !=
                     executions[2].SeedPreparation.RandomSeeds[index]);
        Assert.Contains(
            Enumerable.Range(
                0,
                executions[0].SeedPreparation
                    .AdapterInitializationValueBits.Length),
            index => !executions[0].SeedPreparation
                .AdapterInitializationValueBits[index]
                .SequenceEqual(
                    executions[2].SeedPreparation
                        .AdapterInitializationValueBits[index]));
    }

    [Fact]
    public async Task BridgeRequiresContinuationCapabilityAndNeverFallsBackToCold()
    {
        using var fixture = new ContinuationFixture();
        var source = fixture.SourceFor(fixture.ParentCheckpointBytes);
        var request = fixture.Request(
            fixture.ParentContracts,
            source,
            fixture.ParentCheckpointBytes,
            ContinuationFixture.ParentDpoStepCount);
        var baseOnly = new CountingColdOnlyProvider();

        var missingCapability = await ExecuteContinuationBridgeAsync(
            fixture,
            baseOnly,
            request,
            "missing-capability");
        var missingProvider = await ExecuteContinuationBridgeAsync(
            fixture,
            null,
            request,
            "missing-provider");

        Assert.Equal(
            DesignConversionDpoPolicyCheckpointBridge.ContinuationProviderRequiredErrorCode,
            missingCapability.ErrorCode);
        Assert.Equal(
            DesignConversionDpoPolicyCheckpointBridge.ContinuationProviderRequiredErrorCode,
            missingProvider.ErrorCode);
        Assert.Equal(0, baseOnly.ColdCallCount);
        Assert.Equal(0, source.CallCount);
        Assert.False(Directory.Exists(fixture.OutputDirectory));

        var preparation = fixture.Provider.DescribePreparation();
        Assert.True(preparation.IsSuccess, preparation.ErrorMessage);
        var continuationOnlyFailure = new CountingContinuationProvider(
            preparation.Value!,
            OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                "private/provider/path/that/must/not/escape",
                DesignConversionCheckpointBackedDpoPolicyBaseProvider
                    .ContinuationContractInvalidErrorCode));
        var failed = await ExecuteContinuationBridgeAsync(
            fixture,
            continuationOnlyFailure,
            request,
            "continuation-failure");

        Assert.Equal(0, continuationOnlyFailure.ColdCallCount);
        Assert.Equal(1, continuationOnlyFailure.DescribePreparationCallCount);
        Assert.Equal(0, continuationOnlyFailure.LegacyContinuationCallCount);
        Assert.Equal(1, continuationOnlyFailure.DescriptorContinuationCallCount);
        Assert.Equal(
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .ContinuationContractInvalidErrorCode,
            failed.ErrorCode);
        Assert.DoesNotContain("private", failed.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.False(Directory.Exists(fixture.OutputDirectory));
        Assert.False(File.Exists(fixture.RuntimeCheckpointPath));
    }

    [Fact]
    public async Task ContinuationLoadsIndependentCanonicalModelsAndBindsImmediateParent()
    {
        using var fixture = new ContinuationFixture();
        var source = fixture.SourceFor(fixture.ParentCheckpointBytes);

        var result = await fixture.Provider.CreateContinuedModelSetAsync(
            fixture.Request(
                fixture.ParentContracts,
                source,
                fixture.ParentCheckpointBytes,
                ContinuationFixture.ParentDpoStepCount),
            CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, source.CallCount);
        Assert.Equal(1, source.LastStream!.DisposeCount);
        var value = result.Value!;
        Assert.Equal(
            fixture.ParentContracts.Policy.Binding.PolicyCheckpointSha256,
            value.Identity.BaseCheckpointSha256);
        Assert.Equal("checkpoint_backed", value.Identity.ProviderKind);
        Assert.Equal("explicit_model_config_v1", value.Identity.ConfigurationKind);
        Assert.Equal(fixture.CurrentRandomContext, value.RandomContext);
        DesignConversionDpoPolicyBaseMetadata.ValidateSeededCanonicalLoraTopology(
            value.Policy,
            fixture.CurrentRandomContext,
            DesignConversionDpoPolicyModelRole.Policy);
        DesignConversionDpoPolicyBaseMetadata.ValidateSeededCanonicalLoraTopology(
            value.Reference,
            fixture.CurrentRandomContext,
            DesignConversionDpoPolicyModelRole.Reference);
        DesignConversionDpoPolicyBaseMetadata.ValidateSeededCanonicalLoraTopology(
            value.FreshLoadTarget,
            fixture.CurrentRandomContext,
            DesignConversionDpoPolicyModelRole.FreshLoad);
        Assert.NotSame(value.Policy, value.Reference);
        Assert.NotSame(value.Policy, value.FreshLoadTarget);
        Assert.NotSame(value.Reference, value.FreshLoadTarget);
        Assert.NotSame(value.Policy.Config, value.Reference.Config);
        Assert.NotSame(value.Policy.Config, value.FreshLoadTarget.Config);
        Assert.NotNull(value.RuntimeEncodingBinding);
        Assert.Equal(
            fixture.ParentContracts.RuntimeEncodingBinding,
            value.RuntimeEncodingBinding);

        foreach (var projection in new[] { "q_proj", "v_proj" })
        {
            var policyAdapter = value.Policy.GetAttachedProjectionAdapter(0, projection);
            var referenceAdapter = value.Reference.GetAttachedProjectionAdapter(0, projection);
            var freshAdapter = value.FreshLoadTarget.GetAttachedProjectionAdapter(0, projection);
            Assert.NotNull(policyAdapter);
            Assert.NotNull(referenceAdapter);
            Assert.NotNull(freshAdapter);
            Assert.NotSame(policyAdapter, referenceAdapter);
            Assert.NotSame(policyAdapter, freshAdapter);
            Assert.NotSame(referenceAdapter, freshAdapter);
        }

        AssertIndependentAndEqual(
            value.Policy.GetCheckpointParameterEntries(),
            value.Reference.GetCheckpointParameterEntries(),
            value.FreshLoadTarget.GetCheckpointParameterEntries());
    }

    [Fact]
    public async Task ContinuationCanonicalBpeBindingReopensConfiguredSnapshotWithoutParentReference()
    {
        using var fixture = new ContinuationFixture();
        var canonical = fixture.CreateCanonicalBpeSnapshot();
        var pair = fixture.WithCanonicalBpeTokenizer(
            fixture.WithContinuationLegalTeacherAdmissionCommitment(
                fixture.ParentContracts),
            canonical);
        var source = fixture.SourceFor(fixture.ParentCheckpointBytes);
        var provider = fixture.CreateProviderWithCanonicalBpe(canonical);
        var request = fixture.Request(
            pair,
            source,
            fixture.ParentCheckpointBytes,
            ContinuationFixture.ParentDpoStepCount);

        Assert.Null(request.ParentCanonicalBpeTokenizerSnapshot);
        Assert.NotNull(pair.Policy.Binding.CanonicalBpeTokenizer);

        var result = await provider.CreateContinuedModelSetAsync(
            request,
            CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, source.CallCount);
        Assert.Equal(1, canonical.Source.InvocationCount);
        Assert.Equal(
            pair.Policy.Binding.CanonicalBpeTokenizer!.Sha256,
            result.Value!.CanonicalBpeTokenizerSnapshot!.Descriptor.Sha256);
        Assert.Equal(
            pair.Policy.Binding.CanonicalBpeTokenizer.ByteLength,
            result.Value.CanonicalBpeTokenizerSnapshot.Descriptor.ByteLength);
        Assert.Equal(
            pair.Policy.Binding.CanonicalBpeTokenizer.Descriptor,
            result.Value.CanonicalBpeTokenizerSnapshot.Descriptor);
        Assert.Equal(pair.RuntimeEncodingBinding, result.Value.RuntimeEncodingBinding);
    }

    [Fact]
    public async Task ContinuationRuntimeEncodingBindingReachesBridgeAndAllowsNewDatasetDescriptor()
    {
        using var fixture = new ContinuationFixture();
        var source = fixture.SourceFor(fixture.ParentCheckpointBytes);
        var newRows = fixture.CreateNewDatasetRecords();

        var result = await ExecuteContinuationBridgeAsync(
            fixture,
            fixture.Provider,
            fixture.Request(
                fixture.ParentContracts,
                source,
                fixture.ParentCheckpointBytes,
                ContinuationFixture.ParentDpoStepCount),
            "runtime-encoding-new-dataset",
            newRows);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, source.CallCount);
        var value = result.Value!;
        var descriptor = value.SourceEncoderDescriptor!;
        var parentBinding = fixture.ParentContracts.RuntimeEncodingBinding!;
        Assert.NotEqual(
            fixture.ParentContracts.Source.Binding.SourceEncoderDescriptor
                .SourceSignalSetSha256,
            descriptor.SourceSignalSetSha256);
        Assert.Equal(parentBinding.SourceDescriptorSchema, descriptor.ContractSchema);
        Assert.Equal(parentBinding.PolicyTokenMapping, descriptor.PolicyTokenMapping);
        Assert.Equal(
            parentBinding.PolicyContextTruncation,
            descriptor.PolicyContextTruncation);
        Assert.Equal(parentBinding.PolicyTrainingPadding, descriptor.PolicyTrainingPadding);
        Assert.Equal(parentBinding.TokenIdMinimum, descriptor.TokenIdMinimum);
        Assert.Equal(parentBinding.TokenIdMaximum, descriptor.TokenIdMaximum);
        Assert.Equal(parentBinding.ContextLength, descriptor.ContextLength);
        Assert.Contains(
            "DESIGN_CONVERSION_DPO_CONTINUATION_RUNTIME_ENCODING_BOUND",
            value.EvidenceRefs);
        Assert.Contains(
            $"dpo_runtime_encoding_vocabulary_sha256:{parentBinding.TokenizerVocabularyIdentitySha256}",
            value.EvidenceRefs);
    }

    [Fact]
    public void RuntimeInferenceEncodingAcceptsExactLegacyDescriptorBinding()
    {
        using var fixture = new ContinuationFixture();
        var config = fixture.Config;
        var binding = fixture.ParentContracts.RuntimeEncodingBinding!;

        var encoded = DesignConversionDpoPolicyCheckpointBridge
            .EncodeRuntimePolicyTextForInference(
                "\u0001ABC",
                binding,
                config,
                maxTokenCount: 3);
        var legacy = DesignConversionDpoPolicyCheckpointBridge
            .EncodeRuntimePolicyTextForInference(
                "\u0001ABC",
                config,
                maxTokenCount: 3);
        var shortEncoded = DesignConversionDpoPolicyCheckpointBridge
            .EncodeRuntimePolicyTextForInference(
                "\u0001",
                binding,
                config,
                maxTokenCount: 2);
        var shortLegacy = DesignConversionDpoPolicyCheckpointBridge
            .EncodeRuntimePolicyTextForInference(
                "\u0001",
                config,
                maxTokenCount: 2);

        Assert.True(encoded.IsSuccess, encoded.ErrorMessage);
        Assert.True(shortEncoded.IsSuccess, shortEncoded.ErrorMessage);
        Assert.Equal(new[] { 1, 65, 66 }, encoded.Value!.ToArray());
        Assert.Equal(legacy, encoded.Value);
        Assert.Equal(new[] { 1, 0 }, shortEncoded.Value!.ToArray());
        Assert.Equal(new[] { 1, 0 }, shortLegacy.ToArray());
    }

    [Theory]
    [InlineData("context")]
    [InlineData("minimum")]
    [InlineData("maximumBelowMinimum")]
    [InlineData("maximumOutsideVocabulary")]
    [InlineData("maximumInsideVocabulary")]
    [InlineData("tokenizerArtifactIdentity")]
    [InlineData("tokenizerVocabularyIdentity")]
    public void RuntimeInferenceEncodingFailsClosedForIncoherentBindingConfig(
        string caseName)
    {
        using var fixture = new ContinuationFixture();
        var binding = fixture.ParentContracts.RuntimeEncodingBinding!;
        var incoherent = caseName switch
        {
            "context" => binding with
            {
                ContextLength = fixture.Config.ContextLen + 1
            },
            "minimum" => binding with
            {
                TokenIdMinimum = -1
            },
            "maximumBelowMinimum" => binding with
            {
                TokenIdMinimum = 10,
                TokenIdMaximum = 9
            },
            "maximumOutsideVocabulary" => binding with
            {
                TokenIdMaximum = fixture.Config.VocabSize
            },
            "maximumInsideVocabulary" => binding with
            {
                TokenIdMaximum = binding.TokenIdMaximum - 1
            },
            "tokenizerArtifactIdentity" => binding with
            {
                TokenizerArtifactSha256 = MutateCanonicalSha256(
                    binding.TokenizerArtifactSha256)
            },
            "tokenizerVocabularyIdentity" => binding with
            {
                TokenizerVocabularyIdentitySha256 = MutateCanonicalSha256(
                    binding.TokenizerVocabularyIdentitySha256)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(caseName))
        };

        var encoded = DesignConversionDpoPolicyCheckpointBridge
            .EncodeRuntimePolicyTextForInference(
                "ABC",
                incoherent,
                fixture.Config);

        Assert.False(encoded.IsSuccess);
        Assert.Equal(
            DesignConversionDpoPolicyCheckpointBridge
                .ContinuationRuntimeEncodingInvalidErrorCode,
            encoded.ErrorCode);
    }

    [Theory]
    [InlineData("absent")]
    [InlineData("incompatible")]
    public async Task ContinuationRuntimeEncodingBoundaryFailsBeforeTrainerArtifacts(
        string caseName)
    {
        using var fixture = new ContinuationFixture();
        var source = fixture.SourceFor(fixture.ParentCheckpointBytes);
        var modelSet = await fixture.Provider.CreateContinuedModelSetAsync(
            fixture.Request(
                fixture.ParentContracts,
                source,
                fixture.ParentCheckpointBytes,
                ContinuationFixture.ParentDpoStepCount),
            CancellationToken.None);
        Assert.True(modelSet.IsSuccess, modelSet.ErrorMessage);

        var suppliedModelSet = caseName == "absent"
            ? modelSet.Value! with { RuntimeEncodingBinding = null }
            : modelSet.Value! with
            {
                RuntimeEncodingBinding = modelSet.Value!.RuntimeEncodingBinding!
                    with { PolicyTokenMapping = "incompatible_mapping" }
            };
        var preparation = fixture.Provider.DescribePreparation();
        Assert.True(preparation.IsSuccess, preparation.ErrorMessage);
        var provider = new CountingContinuationProvider(
            preparation.Value!,
            OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(
                suppliedModelSet));
        var ignoredSource = fixture.SourceFor(fixture.ParentCheckpointBytes);

        var result = await ExecuteContinuationBridgeAsync(
            fixture,
            provider,
            fixture.Request(
                fixture.ParentContracts,
                ignoredSource,
                fixture.ParentCheckpointBytes,
                ContinuationFixture.ParentDpoStepCount),
            $"runtime-encoding-{caseName}");

        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionDpoPolicyCheckpointBridge
                .ContinuationRuntimeEncodingInvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(1, provider.DescribePreparationCallCount);
        Assert.Equal(0, provider.LegacyContinuationCallCount);
        Assert.Equal(1, provider.DescriptorContinuationCallCount);
        Assert.Equal(0, provider.ColdCallCount);
        Assert.Equal(0, ignoredSource.CallCount);
        Assert.False(Directory.Exists(fixture.OutputDirectory));
        Assert.False(File.Exists(fixture.RuntimeCheckpointPath));
        Assert.DoesNotContain(
            fixture.Root,
            result.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "payload",
            result.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ContinuationProviderForeignActualWeightsFailBridgeOwnedProofBeforeArtifacts()
    {
        using var fixture = new ContinuationFixture();
        var preloadSource = fixture.SourceFor(fixture.ParentCheckpointBytes);
        var loaded = await fixture.Provider.CreateContinuedModelSetAsync(
            fixture.Request(
                fixture.ParentContracts,
                preloadSource,
                fixture.ParentCheckpointBytes,
                ContinuationFixture.ParentDpoStepCount),
            CancellationToken.None);
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Equal(1, preloadSource.CallCount);
        var supplied = loaded.Value!;

        // Это имитирует прежний bearer-bypass: provider получает request для
        // checkpoint A и возвращает публично совместимый ModelSet, но с
        // фактическими весами не из admitted checkpoint A.
        PerturbContinuationModelSetWeightsInLockstep(supplied);
        var modelSnapshots = new[]
        {
            SnapshotModelState(supplied.Policy),
            SnapshotModelState(supplied.Reference),
            SnapshotModelState(supplied.FreshLoadTarget)
        };
        var provider = new CountingContinuationProvider(
            OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(supplied));
        var ignoredSource = fixture.SourceFor(fixture.ParentCheckpointBytes);
        var request = fixture.Request(
            fixture.ParentContracts,
            ignoredSource,
            fixture.ParentCheckpointBytes,
            ContinuationFixture.ParentDpoStepCount);
        var proofSource = Assert.IsType<CountingCheckpointSource>(
            request.BridgeProofCheckpointSource);
        var mutationBoundaries = new List<string>();
        var previousBoundary =
            DesignConversionDpoPolicyCheckpointBridge
                .MutationBoundaryObserverForTesting;

        try
        {
            DesignConversionDpoPolicyCheckpointBridge
                .MutationBoundaryObserverForTesting = mutationBoundaries.Add;

            var result = await ExecuteContinuationBridgeAsync(
                fixture,
                provider,
                request,
                "foreign-provider-actual-weights");

            Assert.False(result.IsSuccess);
            Assert.Equal(
                DesignConversionDpoCheckpointMutationAdmission.MismatchErrorCode,
                result.ErrorCode);
            Assert.Equal(1, provider.DescribePreparationCallCount);
            Assert.Equal(0, provider.LegacyContinuationCallCount);
            Assert.Equal(1, provider.DescriptorContinuationCallCount);
            Assert.Equal(1, provider.ContinuationCallCount);
            Assert.Equal(0, provider.ColdCallCount);
            Assert.NotNull(provider.LastContinuationRequest);
            Assert.Equal(
                fixture.ParentContracts.Policy.Binding.PolicyCheckpointSha256,
                provider.LastContinuationRequest!.PolicyCheckpointContract
                    .Binding.PolicyCheckpointSha256);
            Assert.Equal(1, proofSource.CallCount);
            Assert.Equal(1, proofSource.LastStream!.DisposeCount);
            Assert.Equal(0, ignoredSource.CallCount);
            Assert.Empty(mutationBoundaries);
            Assert.DoesNotContain(
                "dpo_bridge:standalone:policy_provider",
                mutationBoundaries);
            Assert.DoesNotContain(
                "dpo_bridge:standalone:policy_provider_result",
                mutationBoundaries);
            Assert.DoesNotContain(
                "dpo_bridge:standalone:lora_attach",
                mutationBoundaries);
            Assert.DoesNotContain(
                "dpo_bridge:standalone:artifact_namespace",
                mutationBoundaries);
            Assert.DoesNotContain(
                mutationBoundaries,
                boundary => boundary.StartsWith(
                    "dpo_bridge:standalone:dpo_step",
                    StringComparison.Ordinal));
            Assert.False(Directory.Exists(fixture.OutputDirectory));
            Assert.False(File.Exists(fixture.RuntimeCheckpointPath));
            foreach (var snapshot in modelSnapshots)
                AssertModelStateUnchanged(snapshot);
        }
        finally
        {
            DesignConversionDpoPolicyCheckpointBridge
                .MutationBoundaryObserverForTesting = previousBoundary;
        }

        var missingProofProvider = new CountingContinuationProvider(
            OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(supplied));
        var providerSource = fixture.SourceFor(fixture.ParentCheckpointBytes);
        var missingProofRequest = fixture.Request(
            fixture.ParentContracts,
            providerSource,
            fixture.ParentCheckpointBytes,
            ContinuationFixture.ParentDpoStepCount) with
            {
                BridgeProofCheckpointSource = null
            };
        var missingProofBoundaries = new List<string>();
        previousBoundary =
            DesignConversionDpoPolicyCheckpointBridge
                .MutationBoundaryObserverForTesting;
        try
        {
            DesignConversionDpoPolicyCheckpointBridge
                .MutationBoundaryObserverForTesting =
                missingProofBoundaries.Add;

            var missingProof = await ExecuteContinuationBridgeAsync(
                fixture,
                missingProofProvider,
                missingProofRequest,
                "missing-bridge-proof-source");

            Assert.False(missingProof.IsSuccess);
            Assert.Equal(
                DesignConversionDpoCheckpointMutationAdmission
                    .AttestationRequiredErrorCode,
                missingProof.ErrorCode);
            Assert.Equal(1, missingProofProvider.DescribePreparationCallCount);
            Assert.Equal(0, missingProofProvider.LegacyContinuationCallCount);
            Assert.Equal(0, missingProofProvider.DescriptorContinuationCallCount);
            Assert.Equal(0, missingProofProvider.ContinuationCallCount);
            Assert.Equal(0, missingProofProvider.ColdCallCount);
            Assert.Equal(0, providerSource.CallCount);
            Assert.Empty(missingProofBoundaries);
            Assert.False(Directory.Exists(fixture.OutputDirectory));
            Assert.False(File.Exists(fixture.RuntimeCheckpointPath));
        }
        finally
        {
            DesignConversionDpoPolicyCheckpointBridge
                .MutationBoundaryObserverForTesting = previousBoundary;
        }
    }

    [Theory]
    [InlineData("wrong-role")]
    [InlineData("wrong-root-context")]
    [InlineData("legacy-null-provenance")]
    public async Task ContinuationSeedProvenanceMismatchFailsBeforeTrainerAndPreservesModels(
        string caseName)
    {
        using var fixture = new ContinuationFixture();
        var source = fixture.SourceFor(fixture.ParentCheckpointBytes);
        var loaded = await fixture.Provider.CreateContinuedModelSetAsync(
            fixture.Request(
                fixture.ParentContracts,
                source,
                fixture.ParentCheckpointBytes,
                ContinuationFixture.ParentDpoStepCount),
            CancellationToken.None);
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        var valid = loaded.Value!;
        var supplied = caseName switch
        {
            "wrong-role" => valid with
            {
                Policy = valid.Reference,
                Reference = valid.Policy
            },
            "wrong-root-context" => valid with
            {
                RandomContext = DesignConversionDpoPolicyRandomContextContract
                    .CreateCurrent(
                        checked(fixture.CurrentRandomContext.RootInitializationSeed + 1UL),
                        fixture.CurrentRandomContext.TrainingConfigurationFingerprint)
            },
            "legacy-null-provenance" => valid with
            {
                Policy = CreateLegacyTopologyClone(valid.Policy)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(caseName))
        };
        var modelSnapshots = new[]
        {
            SnapshotModelState(supplied.Policy),
            SnapshotModelState(supplied.Reference),
            SnapshotModelState(supplied.FreshLoadTarget)
        };
        var provider = new CountingContinuationProvider(
            OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(supplied));
        var ignoredSource = fixture.SourceFor(fixture.ParentCheckpointBytes);

        var result = await ExecuteContinuationBridgeAsync(
            fixture,
            provider,
            fixture.Request(
                fixture.ParentContracts,
                ignoredSource,
                fixture.ParentCheckpointBytes,
                ContinuationFixture.ParentDpoStepCount),
            $"seed-provenance-{caseName}");

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "DESIGN_CONVERSION_DPO_POLICY_LORA_APPLICATION_FAILED",
            result.ErrorCode);
        Assert.Equal(1, provider.ContinuationCallCount);
        Assert.Equal(0, provider.ColdCallCount);
        Assert.Equal(0, ignoredSource.CallCount);
        Assert.False(Directory.Exists(fixture.OutputDirectory));
        Assert.False(File.Exists(fixture.RuntimeCheckpointPath));
        foreach (var snapshot in modelSnapshots)
            AssertModelStateUnchanged(snapshot);
    }

    [Fact]
    public async Task ColdBridgeRejectsPreAttachedSeededTopologyWithoutMutatingIt()
    {
        using var fixture = new ContinuationFixture();
        ModelStateSnapshot[] snapshots = [];
        var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
        {
            var randomContext = Assert.IsType<DesignConversionDpoPolicyRandomContext>(
                modelSet.RandomContext);
            DesignConversionDpoPolicyBaseMetadata.AttachSeededCanonicalLoraTopology(
                modelSet.Policy,
                randomContext,
                DesignConversionDpoPolicyModelRole.Policy);
            DesignConversionDpoPolicyBaseMetadata.AttachSeededCanonicalLoraTopology(
                modelSet.Reference,
                randomContext,
                DesignConversionDpoPolicyModelRole.Reference);
            DesignConversionDpoPolicyBaseMetadata.AttachSeededCanonicalLoraTopology(
                modelSet.FreshLoadTarget,
                randomContext,
                DesignConversionDpoPolicyModelRole.FreshLoad);
            DesignConversionDpoPolicyBaseTestProvider.CopyParameters(
                modelSet.Policy,
                modelSet.Reference);
            DesignConversionDpoPolicyBaseTestProvider.CopyParameters(
                modelSet.Policy,
                modelSet.FreshLoadTarget);
            snapshots =
            [
                SnapshotModelState(modelSet.Policy),
                SnapshotModelState(modelSet.Reference),
                SnapshotModelState(modelSet.FreshLoadTarget)
            ];
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(modelSet);
        });

        var result = await DesignConversionDpoPolicyCheckpointBridge
            .ExecuteAndConvertAsync(
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                fixture.Records,
                fixture.OutputDirectory,
                "cold-preattached-seeded-topology",
                fixture.RuntimeCheckpointPath,
                provider,
                new DesignConversionDpoPolicyCheckpointBridgeOptions(
                    MaxBridgePreferenceCount: 2,
                    DpoStepCount: 1),
                CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "DESIGN_CONVERSION_DPO_POLICY_LORA_APPLICATION_FAILED",
            result.ErrorCode);
        Assert.Equal(1, provider.InvocationCount);
        Assert.Equal(0, provider.ContinuationInvocationCount);
        Assert.False(Directory.Exists(fixture.OutputDirectory));
        Assert.False(File.Exists(fixture.RuntimeCheckpointPath));
        Assert.Equal(3, snapshots.Length);
        foreach (var snapshot in snapshots)
            AssertModelStateUnchanged(snapshot);
    }

    [Fact]
    public async Task ColdBridgePublicationContinuesThroughRealChildAndGrandchildLineage()
    {
        using var fixture = new ContinuationFixture();
        var coldBytes = fixture.CreateColdCheckpointBytes();
        var coldSha = Hash(coldBytes);
        var coldSource = fixture.SourceFor(coldBytes);
        var providerResult = DesignConversionCheckpointBackedDpoPolicyBaseProvider
            .CreateWithOptions(
                coldSource,
                fixture.Config,
                fixture.BaseModelId,
                fixture.BaseRevision,
                coldSha,
                DesignConversionDpoPolicyBaseTestProvider
                    .CreateCurrentProviderOptions());
        Assert.True(providerResult.IsSuccess, providerResult.ErrorMessage);
        var provider = providerResult.Value!;
        var options = new DesignConversionDpoPolicyCheckpointBridgeOptions(
            MaxBridgePreferenceCount: 2,
            DpoStepCount: 1);
        var authority = DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted();
        var parentRecords = fixture.CreateTeacherAuthoredDatasetRecords("parent");
        var childRecords = fixture.CreateTeacherAuthoredDatasetRecords("child");
        var grandchildRecords = fixture.CreateTeacherAuthoredDatasetRecords("grandchild");
        var lineageRecords = parentRecords.Concat(childRecords).Concat(grandchildRecords)
            .ToArray();
        Assert.Equal(
            lineageRecords.Length,
            lineageRecords.Select(record => record.RecordId).Distinct().Count());
        Assert.All(lineageRecords, record => Assert.NotNull(record.TeacherProvenance));
        Assert.Equal(
            lineageRecords.Length,
            lineageRecords
                .Select(record => record.TeacherProvenance!.RequestId)
                .Distinct()
                .Count());

        var parentPaths = fixture.ResolvePublicationPaths("lineage-parent");
        var parentBridge = await DesignConversionDpoPolicyCheckpointBridge
            .ExecuteAndConvertAsync(
                authority,
                parentRecords,
                parentPaths.OutputDirectory,
                parentPaths.SafeRunId,
                parentPaths.RuntimeCheckpointPath,
                provider,
                options,
                CancellationToken.None);
        Assert.True(parentBridge.IsSuccess, parentBridge.ErrorMessage);
        var parentPublication = await
            DesignConversionDpoPromotionArtifactContractCodec.PublishAsync(
                parentPaths,
                parentBridge.Value,
                CancellationToken.None);
        Assert.True(parentPublication.IsSuccess, parentPublication.ErrorMessage);
        Assert.Equal(
            coldSha,
            parentPublication.Value!.PolicyCheckpointContract
                .TrainingContext.BaseCheckpointSha256);

        var parentPolicyBytes = File.ReadAllBytes(Path.Combine(
            parentPaths.OutputDirectory,
            parentBridge.Value!.PolicyCheckpointPath));
        var parentRequest = fixture.Request(
            parentPublication.Value.SourceEncoderContract,
            parentPublication.Value.PolicyCheckpointContract,
            parentPublication.Value.TokenizerArtifact,
            fixture.SourceFor(parentPolicyBytes),
            parentPolicyBytes,
            parentBridge.Value.DpoStepCount,
            "parent-publication");
        var childPaths = fixture.ResolvePublicationPaths("lineage-child");
        var childBridge = await DesignConversionDpoPolicyCheckpointBridge
            .ExecuteAndConvertAsync(
                authority,
                childRecords,
                childPaths.OutputDirectory,
                childPaths.SafeRunId,
                childPaths.RuntimeCheckpointPath,
                provider,
                options,
                parentRequest,
                CancellationToken.None);
        Assert.True(childBridge.IsSuccess, childBridge.ErrorMessage);
        var parentPolicySha = parentPublication.Value.PolicyCheckpointContract
            .Binding.PolicyCheckpointSha256;
        Assert.Equal(
            parentPolicySha,
            childBridge.Value!.PolicyCheckpointRef!.TrainingContext
                .BaseCheckpointSha256);
        var childPublication = await
            DesignConversionDpoPromotionArtifactContractCodec.PublishAsync(
                childPaths,
                childBridge.Value,
                CancellationToken.None);
        Assert.True(childPublication.IsSuccess, childPublication.ErrorMessage);
        Assert.Equal(
            parentPolicySha,
            childPublication.Value!.PolicyCheckpointContract
                .TrainingContext.BaseCheckpointSha256);

        var childPolicyBytes = File.ReadAllBytes(Path.Combine(
            childPaths.OutputDirectory,
            childBridge.Value.PolicyCheckpointPath));
        var childRequest = fixture.Request(
            childPublication.Value.SourceEncoderContract,
            childPublication.Value.PolicyCheckpointContract,
            childPublication.Value.TokenizerArtifact,
            fixture.SourceFor(childPolicyBytes),
            childPolicyBytes,
            childBridge.Value.DpoStepCount,
            "child-publication");
        var grandchildPaths = fixture.ResolvePublicationPaths("lineage-grandchild");
        var grandchildBridge = await DesignConversionDpoPolicyCheckpointBridge
            .ExecuteAndConvertAsync(
                authority,
                grandchildRecords,
                grandchildPaths.OutputDirectory,
                grandchildPaths.SafeRunId,
                grandchildPaths.RuntimeCheckpointPath,
                provider,
                options,
                childRequest,
                CancellationToken.None);
        Assert.True(grandchildBridge.IsSuccess, grandchildBridge.ErrorMessage);
        var childPolicySha = childPublication.Value.PolicyCheckpointContract
            .Binding.PolicyCheckpointSha256;
        Assert.Equal(
            childPolicySha,
            grandchildBridge.Value!.PolicyCheckpointRef!.TrainingContext
                .BaseCheckpointSha256);
        Assert.NotEqual(parentPolicySha, childPolicySha);
        Assert.True(
            DesignConversionDpoPromotionArtifactContractCodec.ValidateContractPair(
                parentPublication.Value.SourceEncoderContract,
                parentPublication.Value.PolicyCheckpointContract));
        Assert.True(
            DesignConversionDpoPromotionArtifactContractCodec.ValidateContractPair(
                childPublication.Value.SourceEncoderContract,
                childPublication.Value.PolicyCheckpointContract));
    }

    [Fact]
    public async Task ContractSchemaMatrix_AllowsLegacyInspectionButQuarantinesMutationContinuation()
    {
        using var fixture = new ContinuationFixture();
        var current = fixture.ParentContracts;
        var committedLegacyContext = current.Policy.TrainingContext with
        {
            SchemaVersion = 3,
            RandomContext = null
        };
        var committedLegacy = current.Policy with
        {
            Schema = DesignConversionDpoPromotionArtifactContractCodec
                .LegacyCommittedPolicyCheckpointContractSchema,
            TrainingContext = committedLegacyContext
        };
        var legacyContext = current.Policy.TrainingContext with
        {
            SchemaVersion = 2,
            LegalTeacherAdmissionCommitment = null,
            RandomContext = null
        };
        var legacy = current.Policy with
        {
            Schema = DesignConversionDpoPromotionArtifactContractCodec
                .LegacyPolicyCheckpointContractSchema,
            TrainingContext = legacyContext
        };

        Assert.True(DesignConversionDpoPromotionArtifactContractCodec
            .ValidateContractPair(current.Source, current.Policy));
        Assert.True(DesignConversionDpoPromotionArtifactContractCodec
            .ValidateContractPairForRead(current.Source, current.Policy));
        Assert.True(DesignConversionDpoPromotionArtifactContractCodec
            .ValidateContractPairForRead(current.Source, committedLegacy));
        Assert.False(DesignConversionDpoPromotionArtifactContractCodec
            .ValidateContractPair(current.Source, committedLegacy));
        Assert.True(DesignConversionDpoPromotionArtifactContractCodec
            .ValidateContractPairForRead(current.Source, legacy));
        Assert.False(DesignConversionDpoPromotionArtifactContractCodec
            .ValidateContractPair(current.Source, legacy));
        Assert.False(DesignConversionDpoPromotionArtifactContractCodec
            .ValidateContractPairForRead(
                current.Source,
                legacy with { TrainingContext = current.Policy.TrainingContext }));
        Assert.False(DesignConversionDpoPromotionArtifactContractCodec
            .ValidateContractPair(
                current.Source,
                current.Policy with { TrainingContext = legacyContext }));

        var committedLegacySource = fixture.SourceFor(fixture.ParentCheckpointBytes);
        var committedLegacyRequest = fixture.Request(
            current.Source,
            committedLegacy,
            current.TokenizerArtifact!,
            committedLegacySource,
            fixture.ParentCheckpointBytes,
            ContinuationFixture.ParentDpoStepCount,
            "committed-legacy-mutation-quarantine");
        var committedLegacyMutation =
            await fixture.Provider.CreateContinuedModelSetAsync(
                committedLegacyRequest,
                CancellationToken.None);
        Assert.False(committedLegacyMutation.IsSuccess);
        Assert.Equal(
            DesignConversionDpoCheckpointMutationAdmission
                .LegacyQuarantinedErrorCode,
            committedLegacyMutation.ErrorCode);
        Assert.Equal(0, committedLegacySource.CallCount);

        var legacySource = fixture.SourceFor(fixture.ParentCheckpointBytes);
        var legacyRequest = fixture.Request(
            current.Source,
            legacy,
            current.TokenizerArtifact!,
            legacySource,
            fixture.ParentCheckpointBytes,
            ContinuationFixture.ParentDpoStepCount,
            "legacy-mutation-quarantine");
        var legacyMutation = await fixture.Provider.CreateContinuedModelSetAsync(
            legacyRequest,
            CancellationToken.None);
        Assert.False(legacyMutation.IsSuccess);
        Assert.Equal(
            DesignConversionDpoCheckpointMutationAdmission
                .LegacyQuarantinedErrorCode,
            legacyMutation.ErrorCode);
        Assert.Equal(0, legacySource.CallCount);
    }

    [Fact]
    public async Task ContractTamperingFailsBeforeOpeningCheckpointSource()
    {
        using var fixture = new ContinuationFixture();
        var valid = fixture.ParentContracts;
        var binding = valid.Policy.Binding;
        var descriptor = binding.SourceEncoderDescriptor;
        var context = valid.Policy.TrainingContext;

        var mismatchedPolicyBinding = valid.Policy with
        {
            Binding = binding with { RuntimeCheckpointByteLength = 2 }
        };
        var badSetIdBinding = binding with
        {
            ArtifactSetId = "dpo-promotion-set-" + new string('0', 64)
        };
        var badDescriptor = descriptor with { EncoderRole = "tampered_role" };
        var badDescriptorBinding = fixture.RecomputeBinding(
            binding with { SourceEncoderDescriptor = badDescriptor });
        var changedConfig = context with
        {
            RopeTheta = context.RopeTheta + 1f
        };
        changedConfig = changedConfig with
        {
            ConfigSha256 = DesignConversionDpoPolicyBaseMetadata
                .ComputeConfigSha256(changedConfig)
        };
        var badLeafBinding = fixture.RecomputeBinding(binding with
        {
            PolicyCheckpointFileName = DpoTrainer.BuildPolicyCheckpointFileName(2)
        });
        var badMechanism = valid.Policy with { SourceMechanism = "tampered" };
        var badRuntime = valid.Policy with { RuntimeBridgeKind = "tampered" };
        var badSourceSystem = valid.Policy with { SourceSystem = "tampered" };
        var badLora = context with
        {
            Lora = context.Lora! with { Rank = context.Lora!.Rank + 1 }
        };
        var badLoraAlphaBits = context with
        {
            Lora = context.Lora! with
            {
                Alpha = BitConverter.Int32BitsToSingle(
                    BitConverter.SingleToInt32Bits(context.Lora!.Alpha) + 1)
            }
        };
        var badLoraDropoutBits = context with
        {
            Lora = context.Lora! with
            {
                Dropout = BitConverter.Int32BitsToSingle(int.MinValue)
            }
        };
        var badLoraMembership = context with
        {
            Lora = context.Lora! with { TargetModules = ["q_proj", "o_proj"] }
        };
        var badLoraOrder = context with
        {
            Lora = context.Lora! with { TargetModules = ["v_proj", "q_proj"] }
        };
        var badLoraAdapterCount = context with
        {
            Lora = context.Lora! with
            {
                AdapterCount = context.Lora!.AdapterCount + 1
            }
        };
        var staleConfigSha = context with { ConfigSha256 = Hash("stale-config") };
        var changedCommittedEvidenceRefs = context with
        {
            LegalTeacherAdmissionCommitment = context.LegalTeacherAdmissionCommitment! with
            {
                CheckpointEvidenceSetSha256 = Hash("test:added-after-commitment")
            }
        };

        var contractCases = new[]
        {
            new ContractPair(valid.Source, mismatchedPolicyBinding),
            fixture.WithBinding(valid, badSetIdBinding),
            fixture.WithBinding(valid, badDescriptorBinding),
            new ContractPair(valid.Source, valid.Policy with { TrainingContext = changedConfig }),
            new ContractPair(valid.Source, valid.Policy with
            {
                TrainingContext = context with { BaseModelId = "other/model" }
            }),
            new ContractPair(valid.Source, valid.Policy with
            {
                TrainingContext = context with { BaseRevision = "other-revision" }
            }),
            new ContractPair(valid.Source, valid.Policy with
            {
                TrainingContext = context with { ProviderKind = "other_provider" }
            }),
            new ContractPair(valid.Source, valid.Policy with
            {
                TrainingContext = context with { ConfigKind = "other.config.v2" }
            }),
            new ContractPair(valid.Source, valid.Policy with { TrainingContext = badLora }),
            new ContractPair(valid.Source, valid.Policy with
            {
                TrainingContext = badLoraAlphaBits
            }),
            new ContractPair(valid.Source, valid.Policy with
            {
                TrainingContext = badLoraDropoutBits
            }),
            new ContractPair(valid.Source, valid.Policy with
            {
                TrainingContext = badLoraMembership
            }),
            new ContractPair(valid.Source, valid.Policy with
            {
                TrainingContext = badLoraOrder
            }),
            new ContractPair(valid.Source, valid.Policy with
            {
                TrainingContext = badLoraAdapterCount
            }),
            new ContractPair(valid.Source, valid.Policy with
            {
                TrainingContext = staleConfigSha
            }),
            new ContractPair(valid.Source, valid.Policy with
            {
                TrainingContext = changedCommittedEvidenceRefs
            }),
            fixture.WithBinding(valid, badLeafBinding),
            new ContractPair(valid.Source, badMechanism),
            new ContractPair(valid.Source, badRuntime),
            new ContractPair(valid.Source, badSourceSystem)
        };

        foreach (var pair in contractCases)
        {
            var source = fixture.SourceFor(fixture.ParentCheckpointBytes);
            var result = await fixture.Provider.CreateContinuedModelSetAsync(
                fixture.Request(
                    pair,
                    source,
                    fixture.ParentCheckpointBytes,
                    ContinuationFixture.ParentDpoStepCount),
                CancellationToken.None);
            Assert.False(result.IsSuccess);
            Assert.Contains(
                result.ErrorCode,
                new[]
                {
                    DesignConversionCheckpointBackedDpoPolicyBaseProvider
                        .ContinuationContractInvalidErrorCode,
                    DesignConversionCheckpointBackedDpoPolicyBaseProvider
                        .ContinuationLoraInvalidErrorCode
                });
            Assert.Equal(0, source.CallCount);
        }
    }

    [Fact]
    public async Task CoordinatedStepFileAndBindingTamperingCannotMintAdmission()
    {
        using var fixture = new ContinuationFixture();
        var claimedStep = ContinuationFixture.ParentDpoStepCount + 1;
        var claimedBinding = fixture.RecomputeBinding(
            fixture.ParentContracts.Policy.Binding with
            {
                PolicyCheckpointFileName =
                    DpoTrainer.BuildPolicyCheckpointFileName(claimedStep)
            });
        var coordinatedPair = fixture.WithBinding(
            fixture.ParentContracts,
            claimedBinding);
        coordinatedPair = coordinatedPair with
        {
            Policy = coordinatedPair.Policy with { DpoStepCount = claimedStep }
        };
        Assert.True(
            DesignConversionDpoPromotionArtifactContractCodec.ValidateContractPair(
                coordinatedPair.Source,
                coordinatedPair.Policy));

        var source = fixture.SourceFor(fixture.ParentCheckpointBytes);
        // Согласованный договор выглядит самодостаточным, но verifier знает
        // фактический шаг отдельно и не позволяет договору аттестовать себя.
        var request = fixture.Request(
            coordinatedPair,
            source,
            fixture.ParentCheckpointBytes,
            ContinuationFixture.ParentDpoStepCount);
        Assert.Null(request.CheckpointMutationAdmission);
        Assert.Null(request.BridgeProofCheckpointSource);

        var result = await fixture.Provider.CreateContinuedModelSetAsync(
            request,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionDpoCheckpointMutationAdmission
                .AttestationRequiredErrorCode,
            result.ErrorCode);
        Assert.Equal(0, source.CallCount);
        Assert.Null(source.LastStream);
    }

    [Fact]
    public async Task ShaLengthAndByteTamperingAreRejectedBeforeCheckpointSource()
    {
        using var fixture = new ContinuationFixture();
        var valid = fixture.ParentContracts;
        var binding = valid.Policy.Binding;
        var cases = new[]
        {
            (
                Pair: fixture.WithBinding(
                    valid,
                    fixture.RecomputeBinding(binding with
                    {
                        PolicyCheckpointSha256 = Hash("different-policy")
                    })),
                Bytes: fixture.ParentCheckpointBytes),
            (
                Pair: fixture.WithBinding(
                    valid,
                    fixture.RecomputeBinding(binding with
                    {
                        PolicyCheckpointByteLength = binding.PolicyCheckpointByteLength + 1
                    })),
                Bytes: fixture.ParentCheckpointBytes),
            (
                Pair: valid,
                Bytes: fixture.ParentCheckpointBytes
                    .Select((value, index) => index == 16 ? (byte)(value ^ 0x5a) : value)
                    .ToArray())
        };

        foreach (var item in cases)
        {
            var source = fixture.SourceFor(item.Bytes);
            var result = await fixture.Provider.CreateContinuedModelSetAsync(
                fixture.Request(
                    item.Pair,
                    source,
                    item.Bytes,
                    ContinuationFixture.ParentDpoStepCount),
                CancellationToken.None);
            Assert.False(result.IsSuccess);
            // Verifier видит несовпадение заявленного договора с фактическим
            // снимком до передачи права открыть mutable checkpoint provider'у.
            Assert.Equal(
                DesignConversionDpoCheckpointMutationAdmission
                    .AttestationRequiredErrorCode,
                result.ErrorCode);
            Assert.Equal(0, source.CallCount);
            Assert.Null(source.LastStream);
        }
    }

    [Fact]
    public async Task MalformedTruncatedOversizedAndMutatedStreamsAreRejected()
    {
        using var fixture = new ContinuationFixture();
        var malformed = Encoding.UTF8.GetBytes("not-a-checkpoint");
        var malformedPair = fixture.CreateContracts(
            malformed,
            fixture.ParentContracts.Policy.Binding.PolicyCheckpointSha256);
        var truncated = fixture.ParentCheckpointBytes[..^17];
        var truncatedPair = fixture.CreateContracts(
            truncated,
            fixture.ParentContracts.Policy.Binding.PolicyCheckpointSha256);

        foreach (var item in new[]
                 {
                     (Pair: malformedPair, Bytes: malformed),
                     (Pair: truncatedPair, Bytes: truncated)
                 })
        {
            var source = fixture.SourceFor(item.Bytes);
            var result = await fixture.Provider.CreateContinuedModelSetAsync(
                fixture.Request(
                    item.Pair,
                    source,
                    item.Bytes,
                    ContinuationFixture.ParentDpoStepCount),
                CancellationToken.None);
            Assert.Equal(
                DesignConversionCheckpointBackedDpoPolicyBaseProvider
                    .ContinuationCheckpointMismatchErrorCode,
                result.ErrorCode);
            Assert.Equal(1, source.LastStream!.DisposeCount);
        }

        var oversizedBinding = fixture.RecomputeBinding(
            fixture.ParentContracts.Policy.Binding with
            {
                PolicyCheckpointByteLength = (16L * 1024 * 1024 * 1024) + 1
            });
        var oversizedPair = fixture.WithBinding(fixture.ParentContracts, oversizedBinding);
        var oversizedSource = fixture.SourceFor(fixture.ParentCheckpointBytes);
        var oversized = await fixture.Provider.CreateContinuedModelSetAsync(
            fixture.Request(
                oversizedPair,
                oversizedSource,
                fixture.ParentCheckpointBytes,
                ContinuationFixture.ParentDpoStepCount),
            CancellationToken.None);
        Assert.Equal(
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .ContinuationContractInvalidErrorCode,
            oversized.ErrorCode);
        Assert.Equal(0, oversizedSource.CallCount);

        var mutatedBytes = fixture.ParentCheckpointBytes.ToArray();
        mutatedBytes[^1] ^= 0x01;
        var mutatingStream = new MutatingSnapshotStream(
            fixture.ParentCheckpointBytes,
            mutatedBytes,
            mutateAtZeroRewind: 3);
        var mutatingSource = new CountingCheckpointSource(
            _ => OperationResult<Stream>.Ok(mutatingStream));
        var mutated = await fixture.Provider.CreateContinuedModelSetAsync(
            fixture.Request(
                fixture.ParentContracts,
                mutatingSource,
                fixture.ParentCheckpointBytes,
                ContinuationFixture.ParentDpoStepCount),
            CancellationToken.None);
        Assert.Equal(
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .ContinuationCheckpointMismatchErrorCode,
            mutated.ErrorCode);
        Assert.Equal(1, mutatingStream.DisposeCount);
    }

    [Fact]
    public async Task StreamAdmissionFailuresDisposeExactlyOnceAndMapCheckpointMismatch()
    {
        using var fixture = new ContinuationFixture();
        var unreadable = new CapabilitySnapshotStream(
            fixture.ParentCheckpointBytes,
            readable: false,
            seekable: true);
        var unseekable = new CapabilitySnapshotStream(
            fixture.ParentCheckpointBytes,
            readable: true,
            seekable: false);
        var nonzero = new TrackingMemoryStream(fixture.ParentCheckpointBytes)
        {
            Position = 1
        };

        foreach (var stream in new Stream[] { unreadable, unseekable, nonzero })
        {
            var source = new CountingCheckpointSource(
                _ => OperationResult<Stream>.Ok(stream));
            var result = await fixture.Provider.CreateContinuedModelSetAsync(
                fixture.Request(
                    fixture.ParentContracts,
                    source,
                    fixture.ParentCheckpointBytes,
                    ContinuationFixture.ParentDpoStepCount),
                CancellationToken.None);
            Assert.Equal(
                DesignConversionCheckpointBackedDpoPolicyBaseProvider
                    .ContinuationCheckpointMismatchErrorCode,
                result.ErrorCode);
        }

        Assert.Equal(1, unreadable.DisposeCount);
        Assert.Equal(1, unseekable.DisposeCount);
        Assert.Equal(1, nonzero.DisposeCount);
    }

    [Fact]
    public async Task DisposalOutcomePreservesOrdinaryResultMapsRequestedAndLetsUnsafeGraphsEscape()
    {
        using var fixture = new ContinuationFixture();

        var ordinaryStream = new DisposalThrowingSnapshotStream(
            fixture.ParentCheckpointBytes,
            () => new IOException("private-dispose-path"));
        var ordinary = await fixture.Provider.CreateContinuedModelSetAsync(
            fixture.Request(
                fixture.ParentContracts,
                new CountingCheckpointSource(
                    _ => OperationResult<Stream>.Ok(ordinaryStream)),
                fixture.ParentCheckpointBytes,
                ContinuationFixture.ParentDpoStepCount),
            CancellationToken.None);
        Assert.True(ordinary.IsSuccess, ordinary.ErrorMessage);
        Assert.Equal(1, ordinaryStream.DisposeCount);

        using var requestedCts = new CancellationTokenSource();
        var requestedStream = new DisposalThrowingSnapshotStream(
            fixture.ParentCheckpointBytes,
            () =>
            {
                requestedCts.Cancel();
                return new OperationCanceledException(requestedCts.Token);
            });
        var requested = await fixture.Provider.CreateContinuedModelSetAsync(
            fixture.Request(
                fixture.ParentContracts,
                new CountingCheckpointSource(
                    _ => OperationResult<Stream>.Ok(requestedStream)),
                fixture.ParentCheckpointBytes,
                ContinuationFixture.ParentDpoStepCount),
            requestedCts.Token);
        Assert.Equal("CANCELLED", requested.ErrorCode);
        Assert.Equal(1, requestedStream.DisposeCount);

        var fatal = new OutOfMemoryException("fatal-dispose");
        var fatalStream = new DisposalThrowingSnapshotStream(
            fixture.ParentCheckpointBytes,
            () => fatal);
        Assert.Same(
            fatal,
            await Record.ExceptionAsync(() =>
                fixture.Provider.CreateContinuedModelSetAsync(
                    fixture.Request(
                        fixture.ParentContracts,
                        new CountingCheckpointSource(
                            _ => OperationResult<Stream>.Ok(fatalStream)),
                        fixture.ParentCheckpointBytes,
                        ContinuationFixture.ParentDpoStepCount),
                    CancellationToken.None)));
        Assert.Equal(1, fatalStream.DisposeCount);

        var unrequested = new OperationCanceledException("unrequested-dispose");
        var unrequestedStream = new DisposalThrowingSnapshotStream(
            fixture.ParentCheckpointBytes,
            () => unrequested);
        Assert.Same(
            unrequested,
            await Record.ExceptionAsync(() =>
                fixture.Provider.CreateContinuedModelSetAsync(
                    fixture.Request(
                        fixture.ParentContracts,
                        new CountingCheckpointSource(
                            _ => OperationResult<Stream>.Ok(unrequestedStream)),
                        fixture.ParentCheckpointBytes,
                        ContinuationFixture.ParentDpoStepCount),
                    CancellationToken.None)));
        Assert.Equal(1, unrequestedStream.DisposeCount);

        var nested = new IOException(
            "nested-dispose",
            new OperationCanceledException("nested-cancellation"));
        var nestedStream = new DisposalThrowingSnapshotStream(
            fixture.ParentCheckpointBytes,
            () => nested);
        Assert.Same(
            nested,
            await Record.ExceptionAsync(() =>
                fixture.Provider.CreateContinuedModelSetAsync(
                    fixture.Request(
                        fixture.ParentContracts,
                        new CountingCheckpointSource(
                            _ => OperationResult<Stream>.Ok(nestedStream)),
                        fixture.ParentCheckpointBytes,
                        ContinuationFixture.ParentDpoStepCount),
                    CancellationToken.None)));
        Assert.Equal(1, nestedStream.DisposeCount);
    }

    [Fact]
    public async Task AcquiredStreamUnsafeReadAndLoadGraphsEscapeByIdentityAfterOneDispose()
    {
        using var fixture = new ContinuationFixture();
        var fatal = new OutOfMemoryException("fatal-acquired-read");
        var unrequested = new OperationCanceledException("unrequested-acquired-load");
        var nested = new AggregateException(
            "nested-acquired-load",
            new OperationCanceledException("nested-acquired-cancellation"));
        var cases = new[]
        {
            new ProcessingExceptionCase(fatal, ThrowOnAsyncRead: true),
            new ProcessingExceptionCase(unrequested, ThrowOnAsyncRead: false),
            new ProcessingExceptionCase(nested, ThrowOnAsyncRead: false)
        };

        foreach (var item in cases)
        {
            var stream = new ProcessingThrowingSnapshotStream(
                fixture.ParentCheckpointBytes,
                item.Exception,
                item.ThrowOnAsyncRead);
            var source = new CountingCheckpointSource(
                _ => OperationResult<Stream>.Ok(stream));

            var caught = await Record.ExceptionAsync(() =>
                fixture.Provider.CreateContinuedModelSetAsync(
                    fixture.Request(
                        fixture.ParentContracts,
                        source,
                        fixture.ParentCheckpointBytes,
                        ContinuationFixture.ParentDpoStepCount),
                    CancellationToken.None));

            Assert.Same(item.Exception, caught);
            Assert.Equal(1, source.CallCount);
            Assert.Equal(1, stream.DisposeCount);
        }
    }

    [Fact]
    public async Task CancellationAndExceptionGraphsRespectStableBoundary()
    {
        using var fixture = new ContinuationFixture();
        using var preCancelled = new CancellationTokenSource();
        preCancelled.Cancel();
        var unopened = fixture.SourceFor(fixture.ParentCheckpointBytes);
        var pre = await fixture.Provider.CreateContinuedModelSetAsync(
            fixture.Request(
                fixture.ParentContracts,
                unopened,
                fixture.ParentCheckpointBytes,
                ContinuationFixture.ParentDpoStepCount),
            preCancelled.Token);
        Assert.Equal("CANCELLED", pre.ErrorCode);
        Assert.Equal(0, unopened.CallCount);

        using var midCancellation = new CancellationTokenSource();
        var cancellingStream = new CancellingSnapshotStream(
            fixture.ParentCheckpointBytes,
            midCancellation);
        var midSource = new CountingCheckpointSource(
            _ => OperationResult<Stream>.Ok(cancellingStream));
        var mid = await fixture.Provider.CreateContinuedModelSetAsync(
            fixture.Request(
                fixture.ParentContracts,
                midSource,
                fixture.ParentCheckpointBytes,
                ContinuationFixture.ParentDpoStepCount),
            midCancellation.Token);
        Assert.Equal("CANCELLED", mid.ErrorCode);
        Assert.Equal(1, cancellingStream.DisposeCount);

        const string secret = "C:/private/continuation-policy.bin";
        var ordinary = new ThrowingCheckpointSource(
            new IOException(secret));
        var ordinaryResult = await fixture.Provider.CreateContinuedModelSetAsync(
            fixture.Request(
                fixture.ParentContracts,
                ordinary,
                fixture.ParentCheckpointBytes,
                ContinuationFixture.ParentDpoStepCount),
            CancellationToken.None);
        Assert.Equal(
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .ContinuationCheckpointMismatchErrorCode,
            ordinaryResult.ErrorCode);
        Assert.DoesNotContain(secret, ordinaryResult.ErrorMessage, StringComparison.Ordinal);

        var fatal = new OutOfMemoryException("fatal-secret");
        Assert.Same(
            fatal,
            await Record.ExceptionAsync(() =>
                fixture.Provider.CreateContinuedModelSetAsync(
                    fixture.Request(
                        fixture.ParentContracts,
                        new ThrowingCheckpointSource(fatal),
                        fixture.ParentCheckpointBytes,
                        ContinuationFixture.ParentDpoStepCount),
                    CancellationToken.None)));

        var unrequested = new OperationCanceledException("unrequested-secret");
        Assert.Same(
            unrequested,
            await Record.ExceptionAsync(() =>
                fixture.Provider.CreateContinuedModelSetAsync(
                    fixture.Request(
                        fixture.ParentContracts,
                        new ThrowingCheckpointSource(unrequested),
                        fixture.ParentCheckpointBytes,
                        ContinuationFixture.ParentDpoStepCount),
                    CancellationToken.None)));

        var nested = new IOException(
            "outer-secret",
            new OperationCanceledException("nested-secret"));
        Assert.Same(
            nested,
            await Record.ExceptionAsync(() =>
                fixture.Provider.CreateContinuedModelSetAsync(
                    fixture.Request(
                        fixture.ParentContracts,
                        new ThrowingCheckpointSource(nested),
                        fixture.ParentCheckpointBytes,
                        ContinuationFixture.ParentDpoStepCount),
                    CancellationToken.None)));
    }

    [Fact]
    public void ContractsBindCanonicalArtifactSetAndSerializeNoCheckpointOrRawPath()
    {
        using var fixture = new ContinuationFixture();
        var pair = fixture.ParentContracts;
        Assert.True(
            DesignConversionDpoPromotionArtifactContractCodec.ValidateContractPair(
                pair.Source,
                pair.Policy));
        Assert.Equal(
            DesignConversionDpoPromotionArtifactContractCodec.BuildArtifactSetId(
                pair.Policy.Binding.PolicyCheckpointFileName,
                pair.Policy.Binding.PolicyCheckpointSha256,
                pair.Policy.Binding.PolicyCheckpointByteLength,
                pair.Policy.Binding.RuntimeCheckpointFileName,
                pair.Policy.Binding.RuntimeCheckpointSha256,
                pair.Policy.Binding.RuntimeCheckpointByteLength,
                pair.Policy.Binding.TokenizerArtifactFileName,
                pair.Policy.Binding.TokenizerArtifactSha256,
                pair.Policy.Binding.TokenizerArtifactByteLength,
                pair.Policy.Binding.SourceEncoderDescriptor),
            pair.Policy.Binding.ArtifactSetId);

        var json = JsonSerializer.Serialize(pair, DesignConversionCheckpointJsonOptions.Standard);
        Assert.DoesNotContain(fixture.Root, json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            Convert.ToBase64String(fixture.ParentCheckpointBytes),
            json,
            StringComparison.Ordinal);
        Assert.Equal(
            fixture.UnrelatedContractRootSha,
            pair.Policy.TrainingContext.BaseCheckpointSha256);
        Assert.NotEqual(
            fixture.ConfiguredRootSha,
            pair.Policy.TrainingContext.BaseCheckpointSha256);
    }

    private static async Task<OperationResult<DesignConversionDpoPolicyRuntimeCheckpointBridgeResult>>
        ExecuteContinuationBridgeAsync(
            ContinuationFixture fixture,
            IDesignConversionDpoPolicyBaseProvider? provider,
            DesignConversionDpoPolicyContinuationRequest request,
            string runId,
            IReadOnlyList<DesignConversionDpoTrainingRecord>? records = null)
        => await DesignConversionDpoPolicyCheckpointBridge.ExecuteAndConvertAsync(
            DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
            records ?? fixture.Records,
            fixture.OutputDirectory,
            runId,
            fixture.RuntimeCheckpointPath,
            provider,
            new DesignConversionDpoPolicyCheckpointBridgeOptions(
                MaxBridgePreferenceCount: 2,
                DpoStepCount: 1),
            request,
            CancellationToken.None);

    private static void AssertIndependentAndEqual(
        params IReadOnlyList<LlmCheckpointParameterEntry>[] sets)
    {
        Assert.Equal(3, sets.Length);
        Assert.Equal(sets[0].Count, sets[1].Count);
        Assert.Equal(sets[0].Count, sets[2].Count);
        for (var index = 0; index < sets[0].Count; index++)
        {
            Assert.Equal(sets[0][index].Key, sets[1][index].Key);
            Assert.Equal(sets[0][index].Key, sets[2][index].Key);
            Assert.NotSame(sets[0][index].Parameter, sets[1][index].Parameter);
            Assert.NotSame(sets[0][index].Parameter, sets[2][index].Parameter);
            Assert.NotSame(
                sets[0][index].Parameter.Value.Storage,
                sets[1][index].Parameter.Value.Storage);
            Assert.NotSame(
                sets[0][index].Parameter.Value.Storage,
                sets[2][index].Parameter.Value.Storage);
            Assert.Equal(
                sets[0][index].Parameter.Value.Data,
                sets[1][index].Parameter.Value.Data);
            Assert.Equal(
                sets[0][index].Parameter.Value.Data,
                sets[2][index].Parameter.Value.Data);
        }
    }

    private static ColdExecutionSnapshot CaptureColdExecution(
        DesignConversionDpoPolicyRuntimeCheckpointBridgeResult result,
        DesignConversionDpoPromotionArtifactPaths paths,
        DesignConversionDpoPolicyBaseModelSet modelSet,
        string rootInitializationSeedEvidenceRef)
    {
        var policyCheckpointPath = Path.Combine(
            paths.OutputDirectory,
            result.PolicyCheckpointPath);
        Assert.True(File.Exists(policyCheckpointPath));
        var policyCheckpointBytes = File.ReadAllBytes(policyCheckpointPath);
        Assert.NotEmpty(policyCheckpointBytes);
        Assert.Equal(result.PolicyCheckpointSha256, Hash(policyCheckpointBytes));
        var policyRef = Assert.IsType<DesignConversionDpoPolicyCheckpointRef>(
            result.PolicyCheckpointRef);
        Assert.Equal(
            policyCheckpointBytes.LongLength,
            (long)policyRef.PolicyCheckpointByteLength);
        Assert.Equal(result.PolicyCheckpointSha256, policyRef.Sha256);
        Assert.Equal(
            result.PolicyCheckpointFreshLoadVerified,
            policyRef.FreshLoadVerified);
        Assert.Equal(
            result.RuntimeCheckpoint.AccuracyAfterLastBatch,
            result.RuntimeExport.AccuracyAtExport);
        Assert.Equal(
            result.RuntimeCheckpoint.ExportedAtUtc,
            result.RuntimeExport.ExportedAtUtc);
        Assert.False(string.IsNullOrWhiteSpace(result.RuntimeCheckpointPath));
        Assert.Equal(
            result.RuntimeCheckpointPath,
            result.RuntimeExport.CheckpointRef);

        var normalizedTrainingHistory = result.RuntimeCheckpoint.TrainingHistory
            .Select(entry => entry with
            {
                CpuMs = 0,
                PeakRamMb = 0,
                ElapsedMs = 0
            })
            .ToArray();
        var normalizedRuntimeCheckpoint = result.RuntimeCheckpoint with
        {
            ExportedAtUtc = DateTimeOffset.UnixEpoch,
            TrainingHistory = normalizedTrainingHistory
        };
        var normalizedRuntimeExport = result.RuntimeExport with
        {
            CheckpointRef = "<runtime-checkpoint>",
            ExportedAtUtc = DateTimeOffset.UnixEpoch
        };
        var sourceEncoderDescriptor =
            Assert.IsType<DesignConversionDpoSourceEncoderDescriptor>(
                result.SourceEncoderDescriptor);

        return new ColdExecutionSnapshot(
            CaptureColdSeedPreparation(modelSet),
            rootInitializationSeedEvidenceRef,
            result.PolicyCheckpointSha256,
            policyCheckpointBytes.LongLength,
            policyCheckpointBytes,
            result.SourceRecordCount,
            result.BridgePreferenceCount,
            result.DpoStepCount,
            result.PolicyCheckpointFreshLoadVerified,
            result.PolicyCheckpointPath,
            result.IngredientIds.ToArray(),
            result.EvidenceRefs.ToArray(),
            policyRef.EvidenceRefs.ToArray(),
            JsonSerializer.SerializeToUtf8Bytes(
                policyRef,
                DesignConversionCheckpointJsonOptions.Standard),
            JsonSerializer.SerializeToUtf8Bytes(
                sourceEncoderDescriptor,
                DesignConversionCheckpointJsonOptions.Standard),
            result.RuntimeCheckpoint.AccuracyAfterLastBatch,
            result.RuntimeCheckpoint.HasTrainingScaffoldHistory,
            JsonSerializer.SerializeToUtf8Bytes(
                normalizedTrainingHistory,
                DesignConversionCheckpointJsonOptions.Standard),
            JsonSerializer.SerializeToUtf8Bytes(
                normalizedRuntimeCheckpoint,
                DesignConversionCheckpointJsonOptions.Standard),
            result.RuntimeExport.AccuracyAtExport,
            JsonSerializer.SerializeToUtf8Bytes(
                normalizedRuntimeExport,
                DesignConversionCheckpointJsonOptions.Standard),
            "<runtime-checkpoint>");
    }

    private static ColdSeedPreparationSnapshot CaptureColdSeedPreparation(
        DesignConversionDpoPolicyBaseModelSet modelSet)
    {
        static LoRA.LoraLinear[] CaptureAdapters(params LlmModel[] models)
            => models
                .SelectMany(model => Enumerable.Range(0, model.Config.NumLayers)
                    .SelectMany(blockIndex => new[] { "q_proj", "v_proj" }
                        .Select(projection => Assert.IsType<LoRA.LoraLinear>(
                            model.GetAttachedProjectionAdapter(
                                blockIndex,
                                projection)))))
                .ToArray();

        var adapters = CaptureAdapters(
            modelSet.Policy,
            modelSet.Reference,
            modelSet.FreshLoadTarget);
        var stepZeroAdapters = CaptureAdapters(
            modelSet.Reference,
            modelSet.FreshLoadTarget);
        Assert.All(adapters, adapter => Assert.NotNull(adapter.RandomSeeds));
        return new ColdSeedPreparationSnapshot(
            adapters.Select(adapter => adapter.RandomSeeds).ToArray(),
            stepZeroAdapters.Select(adapter => adapter.AdapterParameters
                    .Single(parameter => string.Equals(
                        parameter.Suffix,
                        "lora_a",
                        StringComparison.Ordinal))
                    .Parameter.Value.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray())
                .ToArray());
    }

    private static string Hash(string value)
        => Hash(Encoding.UTF8.GetBytes(value));

    private static string Hash(byte[] value)
        => Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();

    private static string MutateCanonicalSha256(string value)
        => $"{(value[0] == '0' ? '1' : '0')}{value[1..]}";

    private static LlmModel CreateLegacyTopologyClone(LlmModel source)
    {
        var clone = new LlmModel(
            DesignConversionDpoPolicyBaseMetadata.CopyConfig(source.Config));
        DesignConversionDpoPolicyBaseMetadata.AttachCanonicalLoraTopology(clone);
        var sourceEntries = source.GetCheckpointParameterEntries();
        var cloneEntries = clone.GetCheckpointParameterEntries();
        Assert.Equal(sourceEntries.Count, cloneEntries.Count);
        for (var index = 0; index < sourceEntries.Count; index++)
        {
            Assert.Equal(sourceEntries[index].Key, cloneEntries[index].Key);
            Assert.Equal(
                sourceEntries[index].Parameter.Value.Shape.Dimensions,
                cloneEntries[index].Parameter.Value.Shape.Dimensions);
            sourceEntries[index].Parameter.Value.Data.CopyTo(
                cloneEntries[index].Parameter.Value.Data,
                0);
        }
        return clone;
    }

    private static void PerturbContinuationModelSetWeightsInLockstep(
        DesignConversionDpoPolicyBaseModelSet modelSet)
    {
        foreach (var model in new[]
                 {
                     modelSet.Policy,
                     modelSet.Reference,
                     modelSet.FreshLoadTarget
                 })
        {
            var entries = model.GetCheckpointParameterEntries();
            Assert.NotEmpty(entries);
            var values = entries[0].Parameter.Value.Data;
            Assert.NotEmpty(values);
            values[0] += 0.03125f;
        }
    }

    private static ModelStateSnapshot SnapshotModelState(LlmModel model)
    {
        var entries = model.GetCheckpointParameterEntries();
        var parameters = entries.Select(entry =>
        {
            var tensor = entry.Parameter.Value;
            var gradient = tensor.Grad;
            return new ModelParameterStateSnapshot(
                entry.Key,
                entry.Parameter,
                tensor,
                tensor.Storage,
                tensor.Data,
                tensor.Shape.Dimensions.ToArray(),
                tensor.Data.Select(BitConverter.SingleToInt32Bits).ToArray(),
                tensor.RequiresGrad,
                gradient,
                gradient?.Storage,
                gradient?.Data,
                gradient?.Shape.Dimensions.ToArray(),
                gradient?.Data.Select(BitConverter.SingleToInt32Bits).ToArray(),
                gradient?.RequiresGrad);
        }).ToArray();
        var adapters = Enumerable.Range(0, model.Config.NumLayers)
            .SelectMany(blockIndex => new[] { "q_proj", "v_proj" }
                .Select(projection => Assert.IsType<LoRA.LoraLinear>(
                    model.GetAttachedProjectionAdapter(blockIndex, projection))))
            .ToArray();
        return new ModelStateSnapshot(
            model,
            parameters,
            adapters,
            adapters.Select(adapter => adapter.RandomSeeds).ToArray());
    }

    private static void AssertModelStateUnchanged(ModelStateSnapshot snapshot)
    {
        var entries = snapshot.Model.GetCheckpointParameterEntries();
        Assert.Equal(snapshot.Parameters.Length, entries.Count);
        for (var index = 0; index < entries.Count; index++)
        {
            var expected = snapshot.Parameters[index];
            var actualParameter = entries[index].Parameter;
            var actualTensor = actualParameter.Value;
            var actualGradient = actualTensor.Grad;

            Assert.Equal(expected.Key, entries[index].Key);
            Assert.Same(expected.Parameter, actualParameter);
            Assert.Same(expected.Tensor, actualTensor);
            Assert.Same(expected.Storage, actualTensor.Storage);
            Assert.Same(expected.Data, actualTensor.Data);
            Assert.Equal(
                expected.Shape,
                actualTensor.Shape.Dimensions);
            Assert.Equal(
                expected.ValueBits,
                actualTensor.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray());
            Assert.Equal(expected.RequiresGrad, actualTensor.RequiresGrad);
            Assert.Equal(expected.Gradient is null, actualGradient is null);
            Assert.Same(expected.Gradient, actualGradient);
            if (expected.Gradient is null)
            {
                Assert.Null(actualGradient);
            }
            else
            {
                Assert.NotNull(actualGradient);
                Assert.Same(expected.GradientStorage, actualGradient!.Storage);
                Assert.Same(expected.GradientData, actualGradient.Data);
                Assert.Equal(
                    expected.GradientShape,
                    actualGradient.Shape.Dimensions);
                Assert.Equal(
                    expected.GradientValueBits,
                    actualGradient.Data
                        .Select(BitConverter.SingleToInt32Bits)
                        .ToArray());
                Assert.Equal(
                    expected.GradientRequiresGrad,
                    actualGradient.RequiresGrad);
            }
        }
        var adapters = Enumerable.Range(0, snapshot.Model.Config.NumLayers)
            .SelectMany(blockIndex => new[] { "q_proj", "v_proj" }
                .Select(projection => Assert.IsType<LoRA.LoraLinear>(
                    snapshot.Model.GetAttachedProjectionAdapter(
                        blockIndex,
                        projection))))
            .ToArray();
        Assert.Equal(snapshot.Adapters.Length, adapters.Length);
        for (var index = 0; index < adapters.Length; index++)
        {
            Assert.Same(snapshot.Adapters[index], adapters[index]);
            Assert.Equal(snapshot.RandomSeeds[index], adapters[index].RandomSeeds);
        }
    }

    // Публичная пара хранит только переносимый договор. Байты checkpoint не
    // входят в сериализуемый граф и передаются проверяющей стороне отдельно.
    private sealed record ContractPair(
        DesignConversionDpoSourceEncoderContract Source,
        DesignConversionDpoPolicyCheckpointContract Policy,
        DesignConversionDpoUtf8ByteTokenizerArtifact? TokenizerArtifact = null,
        DesignConversionDpoRuntimeEncodingBinding? RuntimeEncodingBinding = null);

    private sealed record ModelStateSnapshot(
        LlmModel Model,
        ModelParameterStateSnapshot[] Parameters,
        LoRA.LoraLinear[] Adapters,
        LoRA.LoraAdapterRandomSeeds?[] RandomSeeds);

    private sealed record ModelParameterStateSnapshot(
        string Key,
        Parameter Parameter,
        Tensor Tensor,
        TensorStorage Storage,
        float[] Data,
        int[] Shape,
        int[] ValueBits,
        bool RequiresGrad,
        Tensor? Gradient,
        TensorStorage? GradientStorage,
        float[]? GradientData,
        int[]? GradientShape,
        int[]? GradientValueBits,
        bool? GradientRequiresGrad);

    private sealed record ColdSeedPreparationSnapshot(
        LoRA.LoraAdapterRandomSeeds?[] RandomSeeds,
        int[][] AdapterInitializationValueBits);

    private sealed record ColdExecutionSnapshot(
        ColdSeedPreparationSnapshot SeedPreparation,
        string RootInitializationSeedEvidenceRef,
        string PolicyCheckpointSha256,
        long PolicyCheckpointByteLength,
        byte[] PolicyCheckpointBytes,
        int SourceRecordCount,
        int BridgePreferenceCount,
        int DpoStepCount,
        bool PolicyCheckpointFreshLoadVerified,
        string PolicyCheckpointPath,
        string[] IngredientIds,
        string[] EvidenceRefs,
        string[] PolicyCheckpointReferenceEvidenceRefs,
        byte[] PolicyCheckpointReferenceBytes,
        byte[] SourceEncoderDescriptorBytes,
        double RuntimeAccuracyAfterLastBatch,
        bool RuntimeHasTrainingScaffoldHistory,
        byte[] NormalizedTrainingHistoryBytes,
        byte[] NormalizedRuntimeCheckpointBytes,
        double RuntimeExportAccuracyAtExport,
        byte[] NormalizedRuntimeExportBytes,
        string NormalizedRuntimeCheckpointPath);

    private sealed record CanonicalBpeSnapshot(
        byte[] Bytes,
        DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor Descriptor,
        InMemoryCanonicalBpeTokenizerSnapshotSource Source);

    private sealed record ProcessingExceptionCase(
        Exception Exception,
        bool ThrowOnAsyncRead);

    private sealed class CapturingColdProvider(
        IDesignConversionDpoPolicyBaseProvider inner)
        : IDesignConversionDpoPolicyBaseProvider
    {
        public DesignConversionDpoPolicyBaseModelSet? LastModelSet { get; private set; }
        public int DescribePreparationCallCount { get; private set; }
        public int LegacyCreateModelSetCallCount { get; private set; }
        public int DescriptorCreateModelSetCallCount { get; private set; }
        public DesignConversionDpoPolicyBasePreparationDescriptor? LastDescribedPreparation { get; private set; }
        public DesignConversionDpoPolicyBasePreparationDescriptor? LastCreatePreparation { get; private set; }

        public OperationResult<DesignConversionDpoPolicyBasePreparationDescriptor>
            DescribePreparation()
        {
            DescribePreparationCallCount++;
            var result = inner.DescribePreparation();
            if (result.IsSuccess)
                LastDescribedPreparation = result.Value;
            return result;
        }

        public async Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
            CreateModelSetAsync(CancellationToken cancellationToken)
        {
            LegacyCreateModelSetCallCount++;
            var result = await inner.CreateModelSetAsync(cancellationToken);
            if (result.IsSuccess)
                LastModelSet = result.Value;
            return result;
        }

        public async Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
            CreateModelSetAsync(
                DesignConversionDpoPolicyBasePreparationDescriptor preparation,
                CancellationToken cancellationToken)
        {
            DescriptorCreateModelSetCallCount++;
            LastCreatePreparation = preparation;
            var result = await inner.CreateModelSetAsync(
                preparation,
                cancellationToken);
            if (result.IsSuccess)
                LastModelSet = result.Value;
            return result;
        }
    }

    private sealed class ContinuationFixture : IDisposable
    {
        public const int ParentDpoStepCount = 1;

        public string Root { get; } = Path.Combine(
            Path.GetTempPath(),
            $"design-conversion-dpo-continuation-{Guid.NewGuid():N}");
        public string OutputDirectory => Path.Combine(Root, "bridge-output");
        public string RuntimeCheckpointPath => Path.Combine(Root, "runtime.json");
        public string BaseModelId => "test-org/continuation-policy";
        public string BaseRevision => "revision-v2";
        public string ConfiguredRootSha { get; } = Hash("configured-root-checkpoint");
        public string UnrelatedContractRootSha { get; } = Hash("contract-parent-root");
        public ModelConfig Config { get; } = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        public DesignConversionDpoPolicyRandomContext CurrentRandomContext { get; } =
            DesignConversionDpoPolicyBaseTestProvider.CreateCurrentRandomContext();
        public IReadOnlyList<DesignConversionDpoTrainingRecord> Records { get; }
        public byte[] ParentCheckpointBytes { get; }
        public ContractPair ParentContracts { get; }
        public DesignConversionCheckpointBackedDpoPolicyBaseProvider Provider { get; }

        public ContinuationFixture()
        {
            Directory.CreateDirectory(Root);
            Records =
            [
                BuildTrainingRecord(
                    "continuation-row-a",
                    DesignConversionDpoUnits.ImproverUnit,
                    0.97),
                BuildTrainingRecord(
                    "continuation-row-b",
                    DesignConversionDpoUnits.CriticUnit,
                    0.96)
            ];
            var model = new LlmModel(
                DesignConversionDpoPolicyBaseMetadata.CopyConfig(Config));
            DesignConversionDpoPolicyBaseMetadata.AttachSeededCanonicalLoraTopology(
                model,
                CurrentRandomContext,
                DesignConversionDpoPolicyModelRole.Policy);
            ParentCheckpointBytes = SaveCheckpoint(model, "parent-policy.bin");
            ParentContracts = CreateContracts(
                ParentCheckpointBytes,
                UnrelatedContractRootSha);

            var coldSource = new CountingCheckpointSource(
                _ => OperationResult<Stream>.Fail("must not be called", "COLD_CALLED"));
            var created = DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .CreateWithOptions(
                    coldSource,
                    Config,
                    BaseModelId,
                    BaseRevision,
                    ConfiguredRootSha,
                    DesignConversionDpoPolicyBaseTestProvider
                        .CreateCurrentProviderOptions());
            Assert.True(created.IsSuccess, created.ErrorMessage);
            Provider = created.Value!;
        }

        public ContractPair CreateContracts(byte[] checkpointBytes, string contextBaseSha)
        {
            var descriptorResult =
                DesignConversionDpoPolicyCheckpointBridge
                    .BuildSourceEncoderDescriptorPreviewForTests(
                        Records,
                        Config,
                        new DesignConversionDpoPolicyCheckpointBridgeOptions(
                            MaxBridgePreferenceCount: 2,
                            DpoStepCount: ParentDpoStepCount));
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
                PolicyCheckpointFileName:
                    DpoTrainer.BuildPolicyCheckpointFileName(ParentDpoStepCount),
                PolicyCheckpointSha256: checkpointSha,
                PolicyCheckpointByteLength: checkpointBytes.LongLength,
                RuntimeCheckpointFileName: "continuation-runtime.json",
                RuntimeCheckpointSha256: Hash("runtime-checkpoint"),
                RuntimeCheckpointByteLength: 1,
                TokenizerArtifactFileName: "continuation" +
                    DesignConversionDpoPromotionArtifactContractCodec.TokenizerArtifactSuffix,
                TokenizerArtifactSha256: Hash(tokenizerBytes),
                TokenizerArtifactByteLength: tokenizerBytes.LongLength,
                SourceEncoderDescriptor: descriptor);
            binding = RecomputeBinding(binding);
            var source = new DesignConversionDpoSourceEncoderContract(
                DesignConversionDpoPromotionArtifactContractCodec
                    .SourceEncoderContractSchema,
                "design_conversion_dpo_source_encoder_contract",
                binding);
            var context = new DesignConversionDpoPolicyCheckpointTrainingContext(
                ConfigKind: "explicit_model_config_v1",
                VocabSize: Config.VocabSize,
                ContextLength: Config.ContextLen,
                HiddenDim: Config.HiddenDim,
                NumLayers: Config.NumLayers,
                NumHeads: Config.NumHeads,
                NumKvHeads: Config.NumKvHeads,
                FfnHiddenDim: Config.FfnHiddenDim,
                RawPayloadPersisted: true,
                AbsolutePathPersisted: false,
                SchemaVersion: 4,
                RopeTheta: Config.RopeTheta,
                RmsNormEps: Config.RmsNormEps,
                ProviderKind: "checkpoint_backed",
                BaseModelId: BaseModelId,
                BaseRevision: BaseRevision,
                BaseCheckpointSha256: contextBaseSha,
                ConfigSha256: DesignConversionDpoPolicyBaseMetadata
                    .ComputeConfigSha256(Config),
                InitialWeightSha256: Hash("initial-continuation-weights"),
                Lora: DesignConversionDpoPolicyBaseMetadata
                    .CreateCanonicalLoraContext(Config, Config.NumLayers * 2)) with
            {
                LegalTeacherAdmissionCommitment =
                    DesignConversionDpoLegalTeacherAdmissionCommitmentTestFactory.Create(
                        ["test:policy-continuation-contract"]),
                RandomContext = CurrentRandomContext
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
                DpoStepCount: ParentDpoStepCount,
                TrainingContext: context,
                RawPayloadPersisted: false,
                AbsolutePathPersisted: false);
            var runtimeBinding = DesignConversionDpoPromotionArtifactContractCodec
                .BuildRuntimeEncodingBinding(source, tokenizerResult.Value!);
            Assert.True(runtimeBinding.IsSuccess, runtimeBinding.ErrorMessage);
            var pair = new ContractPair(
                source,
                policy,
                tokenizerResult.Value,
                runtimeBinding.Value);
            Assert.True(
                DesignConversionDpoPromotionArtifactContractCodec
                    .ValidateContractPair(pair.Source, pair.Policy));
            return pair;
        }

        public DesignConversionDpoPromotionArtifactBinding RecomputeBinding(
            DesignConversionDpoPromotionArtifactBinding binding)
            => binding with
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
                        binding.SourceEncoderDescriptor)
            };

        private static DesignConversionDpoLegalTeacherAdmissionCommitment
            CreateContinuationLegalTeacherAdmissionCommitment(
                IEnumerable<string> checkpointEvidenceRefs)
        {
            var authority = LegalTrainingTeacherAdmissionAuthority.Create(
                [],
                maxTeacherCalls: 0,
                Enumerable.Repeat((byte)0x5a, 32).ToArray());
            Assert.True(authority.IsSuccess, authority.ErrorMessage);

            var trainingContent = BoundedDpoTrainingContentProjector.Project(
            [
                new DpoTrainingContentRowInput(
                    "design-conversion policy continuation fixture",
                    "canonical BPE snapshot sidecar is reopened",
                    "canonical BPE snapshot sidecar is absent",
                    [1.0, 0.0],
                    "design-conversion-dpo-policy-continuation-test-v1")
            ]);
            Assert.True(trainingContent.IsSuccess, trainingContent.ErrorMessage);

            var provenance = TrainingTeacherProvenance
                .Deterministic("test-custody-commitment") with
            {
                TrainingContentSha256 = trainingContent.Value!.Rows[0].Sha256
            };
            var admission = authority.Value!.ValidateBatch(
                trainingContent.Value,
                [provenance]);
            Assert.True(admission.IsSuccess, admission.ErrorMessage);
            var core = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
                .CaptureAdmission(admission.Value!);
            Assert.True(core.IsSuccess, core.ErrorMessage);
            var commitment = DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
                .FinalizeCommitment(core.Value!, checkpointEvidenceRefs);
            Assert.True(commitment.IsSuccess, commitment.ErrorMessage);
            return commitment.Value!;
        }

        public DesignConversionDpoPromotionArtifactBinding RecomputeBindingWithCanonicalBpe(
            DesignConversionDpoPromotionArtifactBinding binding)
            => binding with
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
                        binding.CanonicalBpeTokenizer)
            };

        public ContractPair WithBinding(
            ContractPair pair,
            DesignConversionDpoPromotionArtifactBinding binding)
            => new(
                pair.Source with { Binding = binding },
                pair.Policy with { Binding = binding },
                pair.TokenizerArtifact,
                pair.RuntimeEncodingBinding);

        public ContractPair WithContinuationLegalTeacherAdmissionCommitment(
            ContractPair pair)
        {
            var policy = pair.Policy with
            {
                TrainingContext = pair.Policy.TrainingContext with
                {
                    LegalTeacherAdmissionCommitment =
                        CreateContinuationLegalTeacherAdmissionCommitment(
                            ["test:policy-continuation-contract"])
                }
            };
            return new ContractPair(
                pair.Source,
                policy,
                pair.TokenizerArtifact,
                pair.RuntimeEncodingBinding);
        }

        public ContractPair WithCanonicalBpeTokenizer(
            ContractPair pair,
            CanonicalBpeSnapshot canonical)
        {
            var binding = pair.Policy.Binding with
            {
                CanonicalBpeTokenizer =
                    new DesignConversionDpoCanonicalBpeTokenizerArtifactBinding(
                        "continuation" +
                        DesignConversionDpoPromotionArtifactContractCodec
                            .CanonicalBpeTokenizerArtifactSuffix,
                        canonical.Descriptor.Sha256,
                        canonical.Descriptor.ByteLength,
                        canonical.Descriptor)
            };
            return WithBinding(pair, RecomputeBindingWithCanonicalBpe(binding));
        }

        public CountingCheckpointSource SourceFor(byte[] bytes)
            => new(_ => OperationResult<Stream>.Ok(new TrackingMemoryStream(bytes)));

        public CanonicalBpeSnapshot CreateCanonicalBpeSnapshot()
        {
            var tokenizer = new Tokenizer.BpeTokenizer();
            tokenizer.Train(
                string.Concat(Enumerable.Repeat(
                    "aaaaabbbbbccccc alpha beta gamma ",
                    32)),
                vocabSize: 258);

            using var stream = new MemoryStream();
            tokenizer.Save(stream);
            var bytes = stream.ToArray();
            var descriptor = DesignConversionDpoPromotionArtifactContractCodec
                .BuildCanonicalBpeTokenizerArtifactDescriptor(bytes, Config);
            Assert.True(descriptor.IsSuccess, descriptor.ErrorMessage);
            var source = new InMemoryCanonicalBpeTokenizerSnapshotSource(bytes);
            return new CanonicalBpeSnapshot(bytes, descriptor.Value!, source);
        }

        public DesignConversionCheckpointBackedDpoPolicyBaseProvider
            CreateProviderWithCanonicalBpe(CanonicalBpeSnapshot canonical)
        {
            var coldSource = new CountingCheckpointSource(
                _ => OperationResult<Stream>.Fail("must not be called", "COLD_CALLED"));
            var created = DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .CreateWithOptions(
                    coldSource,
                    Config,
                    BaseModelId,
                    BaseRevision,
                    ConfiguredRootSha,
                    DesignConversionDpoPolicyBaseTestProvider
                        .CreateCurrentProviderOptions(),
                    canonical.Descriptor,
                    canonical.Source);
            Assert.True(created.IsSuccess, created.ErrorMessage);
            return created.Value!;
        }

        public DesignConversionDpoPolicyContinuationRequest Request(
            ContractPair pair,
            IDesignConversionDpoPolicyCheckpointSource source,
            byte[] actualCheckpointSnapshot,
            int actualDpoStepCount)
        {
            // Один неизменяемый снимок питает и verifier admission, и отдельный
            // одноразовый proof-source. Фактический шаг приходит отдельным
            // аргументом, поэтому договор не может сам назначить себе имя.
            var ownedSnapshot = actualCheckpointSnapshot.ToArray();
            var admission = CreateAdmissionIfCurrent(
                pair,
                ownedSnapshot,
                actualDpoStepCount);
            return new DesignConversionDpoPolicyContinuationRequest(
                source,
                pair.Source,
                pair.Policy,
                pair.TokenizerArtifact,
                pair.RuntimeEncodingBinding)
            {
                CheckpointMutationAdmission = admission,
                BridgeProofCheckpointSource = admission is null
                    ? null
                    : CreateOneShotProofSource(ownedSnapshot)
            };
        }

        public DesignConversionDpoPolicyContinuationRequest Request(
            DesignConversionDpoSourceEncoderContract sourceContract,
            DesignConversionDpoPolicyCheckpointContract policyContract,
            DesignConversionDpoUtf8ByteTokenizerArtifact tokenizerArtifact,
            IDesignConversionDpoPolicyCheckpointSource source,
            byte[] actualCheckpointSnapshot,
            int actualDpoStepCount,
            string label)
        {
            var runtimeBinding = DesignConversionDpoPromotionArtifactContractCodec
                .BuildRuntimeEncodingBinding(sourceContract, tokenizerArtifact);
            Assert.True(
                runtimeBinding.IsSuccess,
                $"{label}: {runtimeBinding.ErrorMessage}");
            var pair = new ContractPair(
                sourceContract,
                policyContract,
                tokenizerArtifact,
                runtimeBinding.Value);
            return Request(
                pair,
                source,
                actualCheckpointSnapshot,
                actualDpoStepCount);
        }

        private static DesignConversionDpoCheckpointMutationAdmission?
            CreateAdmissionIfCurrent(
                ContractPair pair,
                byte[] actualCheckpointSnapshot,
                int actualDpoStepCount)
        {
            if (pair.TokenizerArtifact is null ||
                pair.RuntimeEncodingBinding is null ||
                !DesignConversionDpoPromotionArtifactContractCodec
                    .ValidateContractPair(pair.Source, pair.Policy))
            {
                return null;
            }

            var admission =
                DesignConversionDpoCheckpointMutationAdmission.CreateTrustedCurrent(
                    pair.Source,
                    pair.Policy,
                    Hash(actualCheckpointSnapshot),
                    actualCheckpointSnapshot.LongLength,
                    DpoTrainer.BuildPolicyCheckpointFileName(actualDpoStepCount),
                    pair.Source.Binding.SourceEncoderDescriptor
                        .SourceSignalSetSha256);
            return admission.IsSuccess ? admission.Value : null;
        }

        private static CountingCheckpointSource CreateOneShotProofSource(byte[] bytes)
        {
            var ownedBytes = bytes.ToArray();
            var opened = 0;
            return new CountingCheckpointSource(_ =>
                Interlocked.Exchange(ref opened, 1) == 0
                    ? OperationResult<Stream>.Ok(
                        new TrackingMemoryStream(ownedBytes))
                    : OperationResult<Stream>.Fail(
                        "Checkpoint proof snapshot may be opened only once.",
                        "DESIGN_CONVERSION_DPO_TEST_PROOF_SOURCE_REOPENED"));
        }

        public IReadOnlyList<DesignConversionDpoTrainingRecord> CreateNewDatasetRecords()
            =>
            [
                BuildTrainingRecord(
                    "continuation-new-row-a",
                    DesignConversionDpoUnits.ImproverUnit,
                    0.94),
                BuildTrainingRecord(
                    "continuation-new-row-b",
                    DesignConversionDpoUnits.CriticUnit,
                    0.93)
            ];

        public IReadOnlyList<DesignConversionDpoTrainingRecord>
            CreateTeacherAuthoredDatasetRecords(string generation)
            =>
            [
                BuildTrainingRecord(
                    $"continuation-{generation}-row-a",
                    DesignConversionDpoUnits.ImproverUnit,
                    0.94),
                BuildTrainingRecord(
                    $"continuation-{generation}-row-b",
                    DesignConversionDpoUnits.CriticUnit,
                    0.93)
            ];

        public DesignConversionDpoPromotionArtifactPaths ResolvePublicationPaths(
            string runId)
        {
            var resolved = DesignConversionDpoPromotionArtifactContractCodec.ResolvePaths(
                Root,
                Path.Combine(Root, runId),
                runId);
            Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
            return resolved.Value!;
        }

        public byte[] SaveCheckpoint(LlmModel model, string leaf)
        {
            var path = Path.Combine(Root, leaf);
            model.SaveCheckpoint(path);
            return File.ReadAllBytes(path);
        }

        public byte[] CreateColdCheckpointBytes()
            => SaveCheckpoint(
                new LlmModel(DesignConversionDpoPolicyBaseMetadata.CopyConfig(Config)),
                "cold-base.bin");

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
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
    }

    private sealed class CountingCheckpointSource : IDesignConversionDpoPolicyCheckpointSource
    {
        private readonly Func<CancellationToken, OperationResult<Stream>> _factory;

        public int CallCount { get; private set; }
        public TrackingMemoryStream? LastStream { get; private set; }

        public CountingCheckpointSource(
            Func<CancellationToken, OperationResult<Stream>> factory)
            => _factory = factory;

        public Task<OperationResult<Stream>> OpenCheckpointSnapshotAsync(
            CancellationToken cancellationToken)
        {
            CallCount++;
            var result = _factory(cancellationToken);
            LastStream = result.Value as TrackingMemoryStream;
            return Task.FromResult(result);
        }
    }

    private sealed class ThrowingCheckpointSource : IDesignConversionDpoPolicyCheckpointSource
    {
        private readonly Exception _exception;

        public ThrowingCheckpointSource(Exception exception) => _exception = exception;

        public Task<OperationResult<Stream>> OpenCheckpointSnapshotAsync(
            CancellationToken cancellationToken)
            => throw _exception;
    }

    private sealed class InMemoryCanonicalBpeTokenizerSnapshotSource(byte[] bytes)
        : IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource
    {
        private readonly byte[] _bytes = bytes.ToArray();

        public int InvocationCount { get; private set; }

        public Task<OperationResult<Stream>> OpenCanonicalBpeTokenizerSnapshotAsync(
            CancellationToken cancellationToken)
        {
            InvocationCount++;
            return cancellationToken.IsCancellationRequested
                ? Task.FromResult(OperationResult<Stream>.Fail("cancelled", "CANCELLED"))
                : Task.FromResult(OperationResult<Stream>.Ok(
                    new MemoryStream(_bytes.ToArray(), writable: false)));
        }
    }

    private sealed class TrackingMemoryStream : MemoryStream
    {
        public int DisposeCount { get; private set; }

        public TrackingMemoryStream(byte[] bytes) : base(bytes.ToArray(), writable: false)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DisposeCount++;
            base.Dispose(disposing);
        }
    }

    private sealed class CancellingSnapshotStream : MemoryStream
    {
        private readonly CancellationTokenSource _source;
        public int DisposeCount { get; private set; }

        public CancellingSnapshotStream(
            byte[] bytes,
            CancellationTokenSource source)
            : base(bytes.ToArray(), writable: false)
            => _source = source;

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            _source.Cancel();
            return ValueTask.FromException<int>(
                new OperationCanceledException(_source.Token));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DisposeCount++;
            base.Dispose(disposing);
        }
    }

    private sealed class MutatingSnapshotStream : Stream
    {
        private readonly byte[] _original;
        private readonly byte[] _mutated;
        private readonly int _mutateAtZeroRewind;
        private byte[] _current;
        private long _position;
        private int _zeroRewinds;

        public int DisposeCount { get; private set; }

        public MutatingSnapshotStream(
            byte[] original,
            byte[] mutated,
            int mutateAtZeroRewind)
        {
            _original = original.ToArray();
            _mutated = mutated.ToArray();
            _current = _original;
            _mutateAtZeroRewind = mutateAtZeroRewind;
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _current.LongLength;

        public override long Position
        {
            get => _position;
            set
            {
                if (value < 0 || value > Length)
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (value == 0)
                {
                    _zeroRewinds++;
                    if (_zeroRewinds >= _mutateAtZeroRewind)
                        _current = _mutated;
                }
                _position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
            => Read(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> buffer)
        {
            var remaining = (int)Math.Min(buffer.Length, Length - _position);
            if (remaining <= 0)
                return 0;
            _current.AsSpan((int)_position, remaining).CopyTo(buffer);
            _position += remaining;
            return remaining;
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
            var target = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => checked(_position + offset),
                SeekOrigin.End => checked(Length + offset),
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };
            Position = target;
            return _position;
        }

        public override void Flush()
        {
        }

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DisposeCount++;
            base.Dispose(disposing);
        }
    }

    private sealed class CapabilitySnapshotStream : Stream
    {
        private readonly MemoryStream _inner;
        private readonly bool _readable;
        private readonly bool _seekable;

        public int DisposeCount { get; private set; }

        public CapabilitySnapshotStream(
            byte[] bytes,
            bool readable,
            bool seekable)
        {
            _inner = new MemoryStream(bytes.ToArray(), writable: false);
            _readable = readable;
            _seekable = seekable;
        }

        public override bool CanRead => _readable;
        public override bool CanSeek => _seekable;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
            => _inner.Read(buffer, offset, count);

        public override int Read(Span<byte> buffer) => _inner.Read(buffer);

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
            => _inner.ReadAsync(buffer, cancellationToken);

        public override long Seek(long offset, SeekOrigin origin)
            => _inner.Seek(offset, origin);

        public override void Flush()
        {
        }

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCount++;
                _inner.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    private sealed class DisposalThrowingSnapshotStream : MemoryStream
    {
        private readonly Func<Exception> _exceptionFactory;

        public int DisposeCount { get; private set; }

        public DisposalThrowingSnapshotStream(
            byte[] bytes,
            Func<Exception> exceptionFactory)
            : base(bytes.ToArray(), writable: false)
            => _exceptionFactory = exceptionFactory;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCount++;
                throw _exceptionFactory();
            }
            base.Dispose(disposing);
        }
    }

    private sealed class ProcessingThrowingSnapshotStream : MemoryStream
    {
        private readonly Exception _exception;
        private readonly bool _throwOnAsyncRead;

        public int DisposeCount { get; private set; }

        public ProcessingThrowingSnapshotStream(
            byte[] bytes,
            Exception exception,
            bool throwOnAsyncRead)
            : base(bytes.ToArray(), writable: false)
        {
            _exception = exception;
            _throwOnAsyncRead = throwOnAsyncRead;
        }

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            if (_throwOnAsyncRead)
                throw _exception;
            return base.ReadAsync(buffer, cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!_throwOnAsyncRead)
                throw _exception;
            return base.Read(buffer, offset, count);
        }

        public override int Read(Span<byte> buffer)
        {
            if (!_throwOnAsyncRead)
                throw _exception;
            return base.Read(buffer);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DisposeCount++;
            base.Dispose(disposing);
        }
    }

    private sealed class CountingColdOnlyProvider : IDesignConversionDpoPolicyBaseProvider
    {
        public int ColdCallCount { get; private set; }

        public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
            CreateModelSetAsync(CancellationToken cancellationToken)
        {
            ColdCallCount++;
            return Task.FromResult(
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                    "cold path must not run",
                    "COLD_PATH_RAN"));
        }
    }

    private sealed class CountingContinuationProvider
        : IDesignConversionDpoPolicyContinuationProvider
    {
        private readonly DesignConversionDpoPolicyBasePreparationDescriptor
            _preparation;
        private readonly OperationResult<DesignConversionDpoPolicyBaseModelSet>
            _continuationResult;

        public int ColdCallCount { get; private set; }
        public int DescribePreparationCallCount { get; private set; }
        public int LegacyContinuationCallCount { get; private set; }
        public int DescriptorContinuationCallCount { get; private set; }
        public DesignConversionDpoPolicyContinuationRequest?
            LastContinuationRequest { get; private set; }
        public int ContinuationCallCount =>
            LegacyContinuationCallCount + DescriptorContinuationCallCount;

        public CountingContinuationProvider(
            OperationResult<DesignConversionDpoPolicyBaseModelSet>
                continuationResult)
            : this(CreatePreparationDescriptor(continuationResult),
                  continuationResult)
        {
        }

        public CountingContinuationProvider(
            DesignConversionDpoPolicyBasePreparationDescriptor preparation,
            OperationResult<DesignConversionDpoPolicyBaseModelSet> continuationResult)
        {
            _preparation = preparation;
            _continuationResult = continuationResult;
        }

        private static DesignConversionDpoPolicyBasePreparationDescriptor
            CreatePreparationDescriptor(
                OperationResult<DesignConversionDpoPolicyBaseModelSet>
                    continuationResult)
        {
            if (!continuationResult.IsSuccess || continuationResult.Value is null)
            {
                throw new ArgumentException(
                    "A successful continuation result is required.",
                    nameof(continuationResult));
            }

            return new DesignConversionDpoPolicyBasePreparationDescriptor(
                continuationResult.Value.Identity,
                continuationResult.Value.Policy.Config);
        }

        public OperationResult<DesignConversionDpoPolicyBasePreparationDescriptor>
            DescribePreparation()
        {
            DescribePreparationCallCount++;
            return OperationResult<DesignConversionDpoPolicyBasePreparationDescriptor>.Ok(
                _preparation);
        }

        public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
            CreateModelSetAsync(CancellationToken cancellationToken)
        {
            ColdCallCount++;
            return Task.FromResult(
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                    "cold path must not run",
                    "COLD_PATH_RAN"));
        }

        public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
            CreateModelSetAsync(
                DesignConversionDpoPolicyBasePreparationDescriptor preparation,
                CancellationToken cancellationToken)
            => CreateModelSetAsync(cancellationToken);

        public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
            CreateContinuedModelSetAsync(
            DesignConversionDpoPolicyContinuationRequest continuationRequest,
            CancellationToken cancellationToken)
        {
            LegacyContinuationCallCount++;
            LastContinuationRequest = continuationRequest;
            return Task.FromResult(
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                    "legacy continuation path must not run",
                    "LEGACY_CONTINUATION_PATH_RAN"));
        }

        public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
            CreateContinuedModelSetAsync(
                DesignConversionDpoPolicyBasePreparationDescriptor preparation,
            DesignConversionDpoPolicyContinuationRequest continuationRequest,
            CancellationToken cancellationToken)
        {
            DescriptorContinuationCallCount++;
            LastContinuationRequest = continuationRequest;
            if (!ReferenceEquals(preparation, _preparation))
            {
                return Task.FromResult(
                    OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                        "unexpected preparation descriptor",
                        IDesignConversionDpoPolicyBaseProvider
                            .PreparationDescriptorUnavailableErrorCode));
            }

            return Task.FromResult(_continuationResult);
        }

        // Отдельный счётчик сверочных обращений: прежние счётчики не трогаем.
        public int VerificationInvocationCount { get; private set; }

        // Сверочный набор для доказательства неизменности продолжения: три
        // независимые модели по ролям с тем же посевом канонной топологии.
        public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
            CreateContinuationVerificationModelSetAsync(
                ModelConfig config,
                DesignConversionDpoPolicyRandomContext randomContext,
                CancellationToken cancellationToken)
        {
            VerificationInvocationCount++;
            // Три модели строятся из настройки, переданной мостом.
            var policy = BuildVerificationModel(
                config,
                randomContext,
                DesignConversionDpoPolicyModelRole.Policy);
            var reference = BuildVerificationModel(
                config,
                randomContext,
                DesignConversionDpoPolicyModelRole.Reference);
            var freshLoadTarget = BuildVerificationModel(
                config,
                randomContext,
                DesignConversionDpoPolicyModelRole.FreshLoad);
            return Task.FromResult(
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(
                    new DesignConversionDpoPolicyBaseModelSet(
                        // Личность собираем из плоских полей описателя
                        // подготовки: отдельного объекта личности у него нет.
                        new DesignConversionDpoPolicyBaseIdentity(
                            _preparation.ProviderKind,
                            _preparation.ConfigurationKind,
                            _preparation.BaseModelId,
                            _preparation.BaseRevision,
                            _preparation.BaseCheckpointSha256),
                        policy,
                        reference,
                        freshLoadTarget)
                    {
                        RandomContext = randomContext
                    }));
        }

        // Одна сверочная модель: копия настройки плюс посев по роли.
        private static LlmModel BuildVerificationModel(
            ModelConfig config,
            DesignConversionDpoPolicyRandomContext randomContext,
            DesignConversionDpoPolicyModelRole role)
        {
            var model = new LlmModel(
                DesignConversionDpoPolicyBaseMetadata.CopyConfig(config));
            DesignConversionDpoPolicyBaseMetadata
                .AttachSeededCanonicalLoraTopology(
                    model,
                    randomContext,
                    role);
            return model;
        }
    }
}
