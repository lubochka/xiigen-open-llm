// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using DesignConversionDomain;
using TensorCore.Init;
using TransformerModel;
using TransformerModel.Config;

internal sealed class DesignConversionDpoPolicyBaseTestProvider
    : IDesignConversionDpoPolicyContinuationProvider
{
    private const string ProviderKind = "test_in_memory";
    private const string ConfigurationKind = "test.non_default.policy.v2";
    private const string BaseModelId = "test-org/non-default-policy";
    private const string BaseRevision = "revision-v1";
    internal const ulong CurrentRootInitializationSeed = 0UL;
    internal const string CurrentTrainingConfigurationFingerprint =
        "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    private readonly Func<DesignConversionDpoPolicyBaseModelSet,
        OperationResult<DesignConversionDpoPolicyBaseModelSet>>? _transform;

    public int InvocationCount { get; private set; }
    public int ContinuationInvocationCount { get; private set; }
    public DesignConversionDpoPolicyBaseModelSet? LastModelSet { get; private set; }

    public DesignConversionDpoPolicyBaseTestProvider(
        Func<DesignConversionDpoPolicyBaseModelSet,
            OperationResult<DesignConversionDpoPolicyBaseModelSet>>? transform = null)
    {
        _transform = transform;
    }

    public OperationResult<DesignConversionDpoPolicyBasePreparationDescriptor>
        DescribePreparation()
        => OperationResult<DesignConversionDpoPolicyBasePreparationDescriptor>.Ok(
            new DesignConversionDpoPolicyBasePreparationDescriptor(
                CreateIdentity(),
                CreateConfig()));

    public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>> CreateModelSetAsync(
        CancellationToken cancellationToken)
    {
        InvocationCount++;
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                "Test provider cancelled.",
                "CANCELLED"));
        }

        var config = CreateConfig();
        // Базовые веса DPO-провайдера должны зависеть только от опубликованного
        // root seed, а не от того, какие LlmModel уже построили соседние тесты.
        using var deterministicInitialization =
            Initializers.UseDeterministicSeed(CurrentRootInitializationSeed);
        var policy = new LlmModel(config);
        var reference = new LlmModel(config);
        var freshLoadTarget = new LlmModel(config);
        CopyParameters(policy, reference);
        CopyParameters(policy, freshLoadTarget);

        var modelSet = new DesignConversionDpoPolicyBaseModelSet(
            CreateIdentity(),
            policy,
            reference,
            freshLoadTarget)
        {
            RandomContext = CreateCurrentRandomContext()
        };
        LastModelSet = modelSet;
        return Task.FromResult(_transform?.Invoke(modelSet) ??
            OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(modelSet));
    }

    public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateModelSetAsync(
            DesignConversionDpoPolicyBasePreparationDescriptor preparation,
            CancellationToken cancellationToken)
        => PreparationMatches(preparation)
            ? CreateModelSetAsync(cancellationToken)
            : Task.FromResult(OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                "Test provider preparation descriptor mismatch.",
                IDesignConversionDpoPolicyBaseProvider.PreparationDescriptorUnavailableErrorCode));

    // Отдельный счётчик сверочных обращений: существующий счётчик обращений к
    // поставщику не трогается, иначе утверждения "обращений ровно одно" и
    // "обращений ноль" стали бы ложными.
    public int VerificationInvocationCount { get; private set; }

    // Сверочный набор для доказательства неизменности продолжения: три
    // независимые модели по ролям с тем же посевом канонной топологии,
    // которым строил модели закрытый метод моста до переноса.
    public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateContinuationVerificationModelSetAsync(
            ModelConfig config,
            DesignConversionDpoPolicyRandomContext randomContext,
            CancellationToken cancellationToken)
    {
        VerificationInvocationCount++;
        // Отмену уважаем до любой работы над моделями.
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                    "Test provider verification model set cancelled.",
                    "CANCELLED"));
        }

        // Каждая модель получает свою роль, поэтому посев топологии у них
        // разный, а настройка — одна, переданная мостом.
        var policy = CreateVerificationModel(
            config,
            randomContext,
            DesignConversionDpoPolicyModelRole.Policy);
        var reference = CreateVerificationModel(
            config,
            randomContext,
            DesignConversionDpoPolicyModelRole.Reference);
        var freshLoadTarget = CreateVerificationModel(
            config,
            randomContext,
            DesignConversionDpoPolicyModelRole.FreshLoad);
        // Сверочный набор не проходит через преобразование набора обучаемых
        // моделей и не записывается в последний набор: он расходный.
        return Task.FromResult(
            OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(
                new DesignConversionDpoPolicyBaseModelSet(
                    CreateIdentity(),
                    policy,
                    reference,
                    freshLoadTarget)
                {
                    RandomContext = randomContext
                }));
    }

    // Одна сверочная модель строится из переданной настройки и посева по роли.
    private static LlmModel CreateVerificationModel(
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

    public async Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateContinuedModelSetAsync(
            DesignConversionDpoPolicyContinuationRequest continuationRequest,
            CancellationToken cancellationToken)
    {
        ContinuationInvocationCount++;

        var identity = CreateIdentity();
        var delegateResult = DesignConversionCheckpointBackedDpoPolicyBaseProvider
            .CreateTrustedWithOptions(
                new FailClosedColdCheckpointSource(),
                CreateConfig(),
                identity,
                CreateCurrentProviderOptions());
        if (!delegateResult.IsSuccess)
        {
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                delegateResult.ErrorMessage,
                delegateResult.ErrorCode);
        }

        var continued = await delegateResult.Value!.CreateContinuedModelSetAsync(
            continuationRequest,
            cancellationToken);
        if (!continued.IsSuccess || continued.Value is null)
            return continued;

        LastModelSet = continued.Value;
        return _transform?.Invoke(continued.Value) ??
            OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(continued.Value);
    }

    public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateContinuedModelSetAsync(
            DesignConversionDpoPolicyBasePreparationDescriptor preparation,
            DesignConversionDpoPolicyContinuationRequest continuationRequest,
            CancellationToken cancellationToken)
        => PreparationMatches(preparation)
            ? CreateContinuedModelSetAsync(continuationRequest, cancellationToken)
            : Task.FromResult(OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                "Test provider preparation descriptor mismatch.",
                IDesignConversionDpoPolicyBaseProvider.PreparationDescriptorUnavailableErrorCode));

    private static bool PreparationMatches(
        DesignConversionDpoPolicyBasePreparationDescriptor? preparation)
        => preparation is not null &&
           string.IsNullOrEmpty(preparation.ProviderBindingRef) &&
           preparation.ProviderBindingGeneration == 0 &&
           preparation.ToIdentity() == CreateIdentity() &&
           string.Equals(
               preparation.ConfigSha256,
               DesignConversionDpoPolicyBaseMetadata.ComputeConfigSha256(CreateConfig()),
               StringComparison.Ordinal);

    public static ModelConfig CreateConfig()
        => new()
        {
            VocabSize = 300,
            ContextLen = 17,
            HiddenDim = 12,
            NumLayers = 1,
            NumHeads = 3,
            NumKvHeads = 1,
            FfnHiddenDim = 20,
            RopeTheta = 7_777f,
            RmsNormEps = 2e-5f
        };

    internal static DesignConversionDpoPolicyBaseProviderOptions
        CreateCurrentProviderOptions()
        => new(
            CurrentRootInitializationSeed,
            CurrentTrainingConfigurationFingerprint);

    internal static DesignConversionDpoPolicyRandomContext
        CreateCurrentRandomContext()
        => DesignConversionDpoPolicyRandomContextContract.CreateCurrent(
            CurrentRootInitializationSeed,
            CurrentTrainingConfigurationFingerprint);

    public static void CopyParameters(LlmModel source, LlmModel destination)
    {
        var sourceEntries = source.GetCheckpointParameterEntries();
        var destinationEntries = destination.GetCheckpointParameterEntries();
        if (sourceEntries.Count != destinationEntries.Count)
            throw new InvalidOperationException("Test model parameter counts differ.");

        for (var index = 0; index < sourceEntries.Count; index++)
            sourceEntries[index].Parameter.Value.Data.CopyTo(destinationEntries[index].Parameter.Value.Data, 0);
    }

    private static string Hash(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static DesignConversionDpoPolicyBaseIdentity CreateIdentity()
        => new(
            ProviderKind,
            ConfigurationKind,
            BaseModelId,
            BaseRevision,
            Hash("independent-test-base-artifact-v1"));

    private sealed class FailClosedColdCheckpointSource
        : IDesignConversionDpoPolicyCheckpointSource
    {
        public Task<OperationResult<Stream>> OpenCheckpointSnapshotAsync(
            CancellationToken cancellationToken)
            => Task.FromResult(OperationResult<Stream>.Fail(
                "Test continuation delegate cold source must not be opened.",
                "TEST_CONTINUATION_DELEGATE_COLD_SOURCE_OPENED"));
    }
}
