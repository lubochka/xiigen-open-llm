// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Globalization;
using ContinuousLearning;
using TrainingLoop;
using TransformerModel.Config;

public sealed class ContinuousLearningRootTrainingSeedAuthorityTests
{
    [Fact]
    public void ActiveTraining_MissingRootInitializationSeed_IsInvalid()
    {
        using var fixture = new SeedAuthorityFixture();
        var environment = fixture.ValidEnvironment(
            trainingActivationRequested: true,
            seedText: null);

        var resolution = Resolve(environment);

        AssertInvalid(resolution);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("+1")]
    [InlineData("-1")]
    [InlineData("01")]
    [InlineData("1a")]
    [InlineData("\u0661")]
    [InlineData("18446744073709551616")]
    public void ActiveTraining_NonCanonicalRootInitializationSeed_IsInvalid(
        string seedText)
    {
        using var fixture = new SeedAuthorityFixture();
        var environment = fixture.ValidEnvironment(
            trainingActivationRequested: true,
            seedText);

        var resolution = Resolve(environment);

        AssertInvalid(resolution);
    }

    [Fact]
    public void RootInitializationSeedEnvironmentName_IsExactAuthorityAnchor()
    {
        Assert.Equal(
            "XIIGEN_CONTINUOUS_LEARNING_TRAINING_ROOT_INITIALIZATION_SEED",
            ContinuousLearningModelServerConfigurationLoader
                .TrainingRootInitializationSeedEnvironmentName);
    }

    [Theory]
    [InlineData("0", 0UL)]
    [InlineData("18446744073709551615", ulong.MaxValue)]
    public void ActiveTraining_CanonicalBoundarySeed_IsAdmitted(
        string seedText,
        ulong expectedSeed)
    {
        using var fixture = new SeedAuthorityFixture();
        var resolution = Resolve(fixture.ValidEnvironment(
            trainingActivationRequested: true,
            seedText));

        Assert.Equal(ContinuousLearningConfigurationState.Enabled, resolution.State);
        var plan = Assert.IsType<ContinuousLearningDeferredCompositionPlan>(
            resolution.DeferredPlan);
        Assert.True(plan.TrainingActivationRequested);
        Assert.Equal<ulong?>(expectedSeed, plan.RootInitializationSeed);
        Assert.Equal<ulong?>(
            expectedSeed,
            plan.TrainingConfig.RootInitializationSeed);
        Assert.Equal(
            ContinuousLearningTrainingConfigurationFingerprint.Compute(
                plan.TrainingConfig),
            plan.TrainingConfigurationFingerprint);
    }

    [Fact]
    public void InactiveTraining_MissingRootInitializationSeed_RemainsNullAuthority()
    {
        using var fixture = new SeedAuthorityFixture();
        var resolution = Resolve(fixture.ValidEnvironment(
            trainingActivationRequested: false,
            seedText: null));

        Assert.Equal(ContinuousLearningConfigurationState.Enabled, resolution.State);
        var plan = Assert.IsType<ContinuousLearningDeferredCompositionPlan>(
            resolution.DeferredPlan);
        Assert.False(plan.TrainingActivationRequested);
        Assert.Null(plan.RootInitializationSeed);
        Assert.Null(plan.TrainingConfig.RootInitializationSeed);
    }

    [Fact]
    public void EqualInputsAcrossCultures_ProduceEqualSeedAndFingerprint()
    {
        using var fixture = new SeedAuthorityFixture();
        var environment = fixture.ValidEnvironment(
            trainingActivationRequested: true,
            seedText: "18446744073709551615");

        var french = InCulture("fr-FR", () => Resolve(environment));
        var arabic = InCulture("ar-SA", () => Resolve(environment));

        var frenchPlan = Assert.IsType<ContinuousLearningDeferredCompositionPlan>(
            french.DeferredPlan);
        var arabicPlan = Assert.IsType<ContinuousLearningDeferredCompositionPlan>(
            arabic.DeferredPlan);
        Assert.Equal<ulong?>(ulong.MaxValue, frenchPlan.RootInitializationSeed);
        Assert.Equal(frenchPlan.RootInitializationSeed, arabicPlan.RootInitializationSeed);
        Assert.Equal(
            frenchPlan.TrainingConfigurationFingerprint,
            arabicPlan.TrainingConfigurationFingerprint);
    }

    [Fact]
    public void RootInitializationSeedChange_ChangesFingerprint()
    {
        var configuration = BaselineTrainingConfiguration();

        var first = ContinuousLearningTrainingConfigurationFingerprint.Compute(
            configuration with { RootInitializationSeed = 0UL });
        var second = ContinuousLearningTrainingConfigurationFingerprint.Compute(
            configuration with { RootInitializationSeed = 1UL });
        var absent = ContinuousLearningTrainingConfigurationFingerprint.Compute(
            configuration with { RootInitializationSeed = null });

        Assert.NotEqual(first, second);
        Assert.NotEqual(first, absent);
        Assert.NotEqual(second, absent);
    }

    [Fact]
    public void FullySpecifiedTrainingConfiguration_MatchesGoldenFingerprint()
    {
        const string expected =
            "34e7834579dea1af834b67f769104e3d9639846b1d32783a94852739d7817e48";

        var actual = ContinuousLearningTrainingConfigurationFingerprint.Compute(
            BaselineTrainingConfiguration());

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(nameof(TrainingConfig.BatchSize))]
    [InlineData(nameof(TrainingConfig.SeqLen))]
    [InlineData(nameof(TrainingConfig.MaxLr))]
    [InlineData(nameof(TrainingConfig.MinLr))]
    [InlineData(nameof(TrainingConfig.WarmupSteps))]
    [InlineData(nameof(TrainingConfig.TotalSteps))]
    [InlineData(nameof(TrainingConfig.GradAccumSteps))]
    [InlineData(nameof(TrainingConfig.GradClipNorm))]
    [InlineData(nameof(TrainingConfig.WeightDecay))]
    [InlineData(nameof(TrainingConfig.CheckpointEvery))]
    [InlineData(nameof(TrainingConfig.LogEvery))]
    public void EveryIncludedScalarTrainingField_ChangesFingerprint(string field)
    {
        var baseline = BaselineTrainingConfiguration();
        var changed = field switch
        {
            nameof(TrainingConfig.BatchSize) => baseline with { BatchSize = baseline.BatchSize + 1 },
            nameof(TrainingConfig.SeqLen) => baseline with { SeqLen = baseline.SeqLen + 1 },
            nameof(TrainingConfig.MaxLr) => baseline with { MaxLr = baseline.MaxLr + 0.001f },
            nameof(TrainingConfig.MinLr) => baseline with { MinLr = baseline.MinLr + 0.0001f },
            nameof(TrainingConfig.WarmupSteps) => baseline with { WarmupSteps = baseline.WarmupSteps + 1 },
            nameof(TrainingConfig.TotalSteps) => baseline with { TotalSteps = baseline.TotalSteps + 1 },
            nameof(TrainingConfig.GradAccumSteps) => baseline with { GradAccumSteps = baseline.GradAccumSteps + 1 },
            nameof(TrainingConfig.GradClipNorm) => baseline with { GradClipNorm = baseline.GradClipNorm + 0.25f },
            nameof(TrainingConfig.WeightDecay) => baseline with { WeightDecay = baseline.WeightDecay + 0.01f },
            nameof(TrainingConfig.CheckpointEvery) => baseline with { CheckpointEvery = baseline.CheckpointEvery + 1 },
            nameof(TrainingConfig.LogEvery) => baseline with { LogEvery = baseline.LogEvery + 1 },
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null),
        };

        Assert.NotEqual(
            ContinuousLearningTrainingConfigurationFingerprint.Compute(baseline),
            ContinuousLearningTrainingConfigurationFingerprint.Compute(changed));
    }

    [Fact]
    public void BindingSnapshot_PreservesSeedAndFingerprint_WithDefensiveModelCopies()
    {
        using var fixture = new SeedAuthorityFixture();
        var resolution = Resolve(fixture.ValidEnvironment(
            trainingActivationRequested: true,
            seedText: "42"));
        var plan = Assert.IsType<ContinuousLearningDeferredCompositionPlan>(
            resolution.DeferredPlan);
        var sourceModel = plan.ModelConfig;
        var sourceTraining = plan.TrainingConfig;
        var fingerprint = plan.TrainingConfigurationFingerprint;
        var binding = new ContinuousLearningDpoPolicyBaseBindingConfiguration(
            sourceModel,
            "registry/qwen3.5-base-9b",
            "base-checkpoint-r17",
            plan.RootInitializationSeed,
            fingerprint);

        var firstModel = binding.ModelConfig;
        var changedReturnedModel = firstModel with
        {
            HiddenDim = firstModel.HiddenDim + 8,
        };
        sourceModel = sourceModel with { HiddenDim = sourceModel.HiddenDim + 16 };
        sourceTraining = sourceTraining with
        {
            RootInitializationSeed = checked(
                sourceTraining.RootInitializationSeed!.Value + 1UL),
        };

        Assert.Equal<ulong?>(42UL, binding.RootInitializationSeed);
        Assert.Equal(fingerprint, binding.TrainingConfigurationFingerprint);
        Assert.Equal(
            ContinuousLearningTrainingConfigurationFingerprint.Compute(
                plan.TrainingConfig),
            binding.TrainingConfigurationFingerprint);
        Assert.NotSame(firstModel, binding.ModelConfig);
        Assert.NotEqual(changedReturnedModel, binding.ModelConfig);
        Assert.NotEqual(sourceModel, binding.ModelConfig);
        Assert.NotEqual(
            ContinuousLearningTrainingConfigurationFingerprint.Compute(
                sourceTraining),
            binding.TrainingConfigurationFingerprint);
    }

    [Fact]
    public void Fingerprint_IsLowercaseSha256Hex_AndExcludesCheckpointPathText()
    {
        var rawPath = Path.Combine(
            Path.GetTempPath(),
            "raw-secret-checkpoint-config-text");
        var configuration = BaselineTrainingConfiguration() with
        {
            CheckpointDir = rawPath,
        };
        var otherPathConfiguration = configuration with
        {
            CheckpointDir = Path.Combine(Path.GetTempPath(), "different-path"),
        };

        var fingerprint =
            ContinuousLearningTrainingConfigurationFingerprint.Compute(
                configuration);

        Assert.Matches("^[0-9a-f]{64}$", fingerprint);
        Assert.DoesNotContain(rawPath, fingerprint, StringComparison.Ordinal);
        Assert.Equal(
            fingerprint,
            ContinuousLearningTrainingConfigurationFingerprint.Compute(
                otherPathConfiguration));
    }

    private static ContinuousLearningConfigurationResolution Resolve(
        IReadOnlyDictionary<string, string> environment)
        => ContinuousLearningModelServerConfigurationLoader.Resolve(name =>
            environment.TryGetValue(name, out var value) ? value : null);

    private static void AssertInvalid(
        ContinuousLearningConfigurationResolution resolution)
    {
        Assert.Equal(ContinuousLearningConfigurationState.Invalid, resolution.State);
        Assert.Null(resolution.DeferredPlan);
        Assert.Equal(
            "CONTINUOUS_LEARNING_CONFIGURATION_INVALID",
            resolution.ErrorCode);
    }

    private static T InCulture<T>(string cultureName, Func<T> action)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            return action();
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    private static TrainingConfig BaselineTrainingConfiguration()
        => new()
        {
            BatchSize = 2,
            SeqLen = 8,
            MaxLr = 0.01f,
            MinLr = 0.001f,
            WarmupSteps = 1,
            TotalSteps = 10,
            GradAccumSteps = 2,
            GradClipNorm = 1.25f,
            WeightDecay = 0.1f,
            CheckpointEvery = 5,
            CheckpointDir = "not-part-of-training-identity",
            LogEvery = 2,
            RootInitializationSeed = 42UL,
        };

    private sealed class SeedAuthorityFixture : IDisposable
    {
        internal SeedAuthorityFixture()
        {
            Root = Path.GetFullPath(Path.Combine(
                Path.GetTempPath(),
                $"xiigen-seed-authority-{Guid.NewGuid():N}"));
            Directory.CreateDirectory(Root);
            BaseCheckpoint = Path.Combine(Root, "base-checkpoint.bin");
            Tokenizer = Path.Combine(Root, "tokenizer.json");
            File.WriteAllBytes(BaseCheckpoint, [1]);
            File.WriteAllBytes(Tokenizer, [1]);
            AdapterRoot = CreateDirectory("adapters");
            CacheRoot = CreateDirectory("cache");
            RegistryRoot = CreateDirectory("registry");
            RepositoryRoot = Path.Combine(Root, "repository-root");
            CreateRepositorySentinels(RepositoryRoot);
            PairRoot = CreateDirectory(Path.Combine(
                "repository-root",
                "training-data",
                "dpo",
                "html-css",
                "learning-unit",
                "v0001",
                "pairs"));
        }

        internal static ModelConfig ModelConfiguration => new()
        {
            VocabSize = 256,
            ContextLen = 8,
            HiddenDim = 8,
            NumLayers = 1,
            NumHeads = 1,
            NumKvHeads = 1,
            FfnHiddenDim = 16,
            RopeTheta = 10000f,
            RmsNormEps = 1e-5f,
        };

        private string Root { get; }
        private string BaseCheckpoint { get; }
        private string Tokenizer { get; }
        private string AdapterRoot { get; }
        private string CacheRoot { get; }
        private string RegistryRoot { get; }
        private string RepositoryRoot { get; }
        private string PairRoot { get; }

        internal Dictionary<string, string> ValidEnvironment(
            bool trainingActivationRequested,
            string? seedText)
        {
            var model = ModelConfiguration;
            var environment = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName] = bool.TrueString,
                [ContinuousLearningModelServerConfigurationLoader.TrainingActivationEnabledEnvironmentName] = trainingActivationRequested.ToString(),
                [ContinuousLearningModelServerConfigurationLoader.ModelIdEnvironmentName] = "qwen3.5:9b",
                [ContinuousLearningModelServerConfigurationLoader.OllamaManifestDigestEnvironmentName] = new string('d', 64),
                [ContinuousLearningModelServerConfigurationLoader.OllamaModelLayerDigestEnvironmentName] = new string('c', 64),
                [ContinuousLearningModelServerConfigurationLoader.BaseModelIdEnvironmentName] = "registry/qwen3.5-base-9b",
                [ContinuousLearningModelServerConfigurationLoader.BaseModelRevisionEnvironmentName] = "base-checkpoint-r17",
                [ContinuousLearningModelServerConfigurationLoader.MaxTeacherCallsEnvironmentName] = "100",
                [ContinuousLearningModelServerConfigurationLoader.AuditSaltEnvironmentName] = Convert.ToBase64String(Enumerable.Range(1, 32).Select(static value => checked((byte)value)).ToArray()),
                [ContinuousLearningModelServerConfigurationLoader.LocalRetentionPolicyRefEnvironmentName] = $"sha256:{new string('a', 64)}",
                [ContinuousLearningModelServerConfigurationLoader.OutputLicenseRefEnvironmentName] = $"sha256:{new string('b', 64)}",
                [ContinuousLearningModelServerConfigurationLoader.BaseCheckpointEnvironmentName] = BaseCheckpoint,
                [ContinuousLearningModelServerConfigurationLoader.TokenizerEnvironmentName] = Tokenizer,
                [ContinuousLearningModelServerConfigurationLoader.AdapterRootEnvironmentName] = AdapterRoot,
                [ContinuousLearningModelServerConfigurationLoader.CacheRootEnvironmentName] = CacheRoot,
                [ContinuousLearningModelServerConfigurationLoader.RegistryRootEnvironmentName] = RegistryRoot,
                [ContinuousLearningModelServerConfigurationLoader.RepositoryRootEnvironmentName] = RepositoryRoot,
                [ContinuousLearningModelServerConfigurationLoader.DpoPairRootEnvironmentName] = PairRoot,
                [ContinuousLearningModelServerConfigurationLoader.ModelVocabSizeEnvironmentName] = model.VocabSize.ToString(CultureInfo.InvariantCulture),
                [ContinuousLearningModelServerConfigurationLoader.ModelContextLenEnvironmentName] = model.ContextLen.ToString(CultureInfo.InvariantCulture),
                [ContinuousLearningModelServerConfigurationLoader.ModelHiddenDimEnvironmentName] = model.HiddenDim.ToString(CultureInfo.InvariantCulture),
                [ContinuousLearningModelServerConfigurationLoader.ModelNumLayersEnvironmentName] = model.NumLayers.ToString(CultureInfo.InvariantCulture),
                [ContinuousLearningModelServerConfigurationLoader.ModelNumHeadsEnvironmentName] = model.NumHeads.ToString(CultureInfo.InvariantCulture),
                [ContinuousLearningModelServerConfigurationLoader.ModelNumKvHeadsEnvironmentName] = model.NumKvHeads.ToString(CultureInfo.InvariantCulture),
                [ContinuousLearningModelServerConfigurationLoader.ModelFfnHiddenDimEnvironmentName] = model.FfnHiddenDim.ToString(CultureInfo.InvariantCulture),
                [ContinuousLearningModelServerConfigurationLoader.ModelRopeThetaEnvironmentName] = "10000",
                [ContinuousLearningModelServerConfigurationLoader.ModelRmsNormEpsEnvironmentName] = "1E-05",
                [ContinuousLearningModelServerConfigurationLoader.TrainingBatchSizeEnvironmentName] = "2",
                [ContinuousLearningModelServerConfigurationLoader.TrainingSeqLenEnvironmentName] = "8",
                [ContinuousLearningModelServerConfigurationLoader.TrainingMaxLrEnvironmentName] = "0.01",
                [ContinuousLearningModelServerConfigurationLoader.TrainingMinLrEnvironmentName] = "0.001",
                [ContinuousLearningModelServerConfigurationLoader.TrainingWarmupStepsEnvironmentName] = "1",
                [ContinuousLearningModelServerConfigurationLoader.TrainingTotalStepsEnvironmentName] = "10",
                [ContinuousLearningModelServerConfigurationLoader.TrainingGradAccumStepsEnvironmentName] = "2",
                [ContinuousLearningModelServerConfigurationLoader.TrainingGradClipNormEnvironmentName] = "1.25",
                [ContinuousLearningModelServerConfigurationLoader.TrainingWeightDecayEnvironmentName] = "0.1",
                [ContinuousLearningModelServerConfigurationLoader.TrainingCheckpointEveryEnvironmentName] = "5",
                [ContinuousLearningModelServerConfigurationLoader.TrainingLogEveryEnvironmentName] = "2",
            };
            if (seedText is not null)
            {
                environment[ContinuousLearningModelServerConfigurationLoader
                    .TrainingRootInitializationSeedEnvironmentName] = seedText;
            }
            return environment;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }

        private string CreateDirectory(string name)
        {
            var path = Path.Combine(Root, name);
            Directory.CreateDirectory(path);
            return path;
        }

        private static void CreateRepositorySentinels(string repositoryRoot)
        {
            Directory.CreateDirectory(repositoryRoot);
            File.WriteAllText(
                Path.Combine(repositoryRoot, "LlmFromScratch.sln"),
                "fixture solution sentinel");
            Directory.CreateDirectory(Path.Combine(repositoryRoot, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(repositoryRoot, "11.DPO"));
        }
    }
}
