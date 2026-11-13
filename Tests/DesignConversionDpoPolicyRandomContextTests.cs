// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text;
using System.Text.Json;
using System.Threading;
using DesignConversionDomain;
using LoRA;
using TransformerModel;

namespace LlmFromScratch.Tests;

public sealed class DesignConversionDpoPolicyRandomContextTests
{
    private const string FingerprintA =
        "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string FingerprintB =
        "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

    [Fact]
    public void CurrentFactory_HasExactValuesAndAcceptsBothRootBoundaries()
    {
        foreach (var root in new[] { 0UL, ulong.MaxValue })
        {
            var context = DesignConversionDpoPolicyRandomContextContract
                .CreateCurrent(root, FingerprintA);

            Assert.Equal(1, context.SchemaVersion);
            Assert.Equal(root, context.RootInitializationSeed);
            Assert.Equal(FingerprintA, context.TrainingConfigurationFingerprint);
            Assert.Equal(
                "xiigen.lora.adapter-random-seed-derivation/v1",
                context.DerivationScheme);
            Assert.Equal(
                "xiigen.lora.adapter.initialization-seed/v1",
                context.InitializationDomain);
            Assert.Equal(
                "xiigen.lora.adapter.dropout-seed/v1",
                context.DropoutDomain);
            Assert.Equal("design-conversion.policy", context.PolicyDropoutStreamDomain);
            Assert.Equal("design-conversion.reference", context.ReferenceDropoutStreamDomain);
            Assert.Equal("design-conversion.fresh-load", context.FreshLoadDropoutStreamDomain);
            Assert.Equal(
                "initialization-only-no-mid-step-dropout-resume",
                context.ReproducibilityScope);
            Assert.True(DesignConversionDpoPolicyRandomContextContract
                .IsValidCurrent(context));
        }

        Assert.Equal(
            "xiigen.lora.adapter-random-seed-derivation/v1",
            LoraAdapterRandomSeedDerivation.DerivationScheme);
        Assert.Equal(
            "xiigen.lora.adapter.initialization-seed/v1",
            LoraAdapterRandomSeedDerivation.InitializationDomain);
        Assert.Equal(
            "xiigen.lora.adapter.dropout-seed/v1",
            LoraAdapterRandomSeedDerivation.DropoutDomain);
    }

    [Fact]
    public void EveryRandomContextField_IsValidatedOrAuthorityMatchedExactly()
    {
        var current = DesignConversionDpoPolicyRandomContextContract
            .CreateCurrent(17UL, FingerprintA);
        var mutations = new (string Field, DesignConversionDpoPolicyRandomContext Value)[]
        {
            (nameof(current.SchemaVersion), current with { SchemaVersion = 2 }),
            (nameof(current.RootInitializationSeed), current with { RootInitializationSeed = 18UL }),
            (nameof(current.TrainingConfigurationFingerprint), current with { TrainingConfigurationFingerprint = FingerprintB }),
            (nameof(current.DerivationScheme), current with { DerivationScheme = current.DerivationScheme + ".changed" }),
            (nameof(current.InitializationDomain), current with { InitializationDomain = current.InitializationDomain + ".changed" }),
            (nameof(current.DropoutDomain), current with { DropoutDomain = current.DropoutDomain + ".changed" }),
            (nameof(current.PolicyDropoutStreamDomain), current with { PolicyDropoutStreamDomain = current.PolicyDropoutStreamDomain + ".changed" }),
            (nameof(current.ReferenceDropoutStreamDomain), current with { ReferenceDropoutStreamDomain = current.ReferenceDropoutStreamDomain + ".changed" }),
            (nameof(current.FreshLoadDropoutStreamDomain), current with { FreshLoadDropoutStreamDomain = current.FreshLoadDropoutStreamDomain + ".changed" }),
            (nameof(current.ReproducibilityScope), current with { ReproducibilityScope = current.ReproducibilityScope + ".changed" }),
        };

        Assert.Equal(10, mutations.Length);
        foreach (var (field, mutation) in mutations)
        {
            var valueOnlyField = field is nameof(current.RootInitializationSeed)
                or nameof(current.TrainingConfigurationFingerprint);
            Assert.Equal(
                valueOnlyField,
                DesignConversionDpoPolicyRandomContextContract
                    .IsValidCurrent(mutation));
            Assert.False(DesignConversionDpoPolicyRandomContextContract
                .MatchesCurrentAuthority(mutation, 17UL, FingerprintA));
        }
    }

    [Fact]
    public void FingerprintValidation_RejectsCaseLengthNonHexAndWhitespace()
    {
        var current = DesignConversionDpoPolicyRandomContextContract
            .CreateCurrent(0, FingerprintA);
        Assert.False(DesignConversionDpoPolicyRandomContextContract
            .IsValidCurrent(null));
        var invalid = new[]
        {
            new string('A', 64),
            new string('a', 63),
            new string('a', 65),
            new string('g', 64),
            " " + new string('a', 63),
            new string('a', 63) + " ",
        };

        foreach (var fingerprint in invalid)
        {
            Assert.False(DesignConversionDpoPolicyRandomContextContract
                .IsValidCurrent(current with
                {
                    TrainingConfigurationFingerprint = fingerprint,
                }));
            Assert.Throws<ArgumentException>(() =>
                DesignConversionDpoPolicyRandomContextContract.CreateCurrent(
                    0,
                    fingerprint));
        }
        Assert.False(DesignConversionDpoPolicyRandomContextContract
            .IsValidCurrent(current with
            {
                TrainingConfigurationFingerprint = null!,
            }));
        Assert.Throws<ArgumentException>(() =>
            DesignConversionDpoPolicyRandomContextContract.CreateCurrent(
                0,
                null!));
    }

    [Fact]
    public void MatchesCurrentAuthority_RequiresExactRootAndFingerprint()
    {
        var context = DesignConversionDpoPolicyRandomContextContract
            .CreateCurrent(9UL, FingerprintA);

        Assert.True(DesignConversionDpoPolicyRandomContextContract
            .MatchesCurrentAuthority(context, 9UL, FingerprintA));
        Assert.False(DesignConversionDpoPolicyRandomContextContract
            .MatchesCurrentAuthority(context, 10UL, FingerprintA));
        Assert.False(DesignConversionDpoPolicyRandomContextContract
            .MatchesCurrentAuthority(context, 9UL, FingerprintB));
        Assert.False(DesignConversionDpoPolicyRandomContextContract
            .MatchesCurrentAuthority(context, 9UL, FingerprintA.ToUpperInvariant()));
        Assert.False(DesignConversionDpoPolicyRandomContextContract
            .MatchesCurrentAuthority(null, 9UL, FingerprintA));
    }

    [Fact]
    public void RoleDomains_KeepInitializationSeedAndSeparateDropoutStreams()
    {
        var domains = new[]
        {
            DesignConversionDpoPolicyRandomContextContract.PolicyDropoutStreamDomain,
            DesignConversionDpoPolicyRandomContextContract.ReferenceDropoutStreamDomain,
            DesignConversionDpoPolicyRandomContextContract.FreshLoadDropoutStreamDomain,
        };
        var seeds = domains.Select(domain =>
            LoraAdapterRandomSeedDerivation.Derive(
                123UL,
                2,
                "v_proj",
                domain)).ToArray();

        Assert.Single(seeds.Select(seed => seed.InitializationSeed).Distinct());
        Assert.Equal(3, seeds.Select(seed => seed.DropoutSeed).Distinct().Count());
    }

    [Fact]
    public void ReproducibilityScope_ExplicitlyDisclaimsMidStepDropoutResume()
    {
        Assert.Equal(
            "initialization-only-no-mid-step-dropout-resume",
            DesignConversionDpoPolicyRandomContextContract.ReproducibilityScope);
        Assert.DoesNotContain(
            "mid-step-dropout-resume-supported",
            DesignConversionDpoPolicyRandomContextContract.ReproducibilityScope,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task TestProvider_BaseModelWeightsIgnorePriorLlmModelConstructionOrder()
    {
        var clean = await CreateProviderModelHashesAfterUnrelatedModelNoiseAsync(0);
        var noisy = await CreateProviderModelHashesAfterUnrelatedModelNoiseAsync(5);

        Assert.Equal(clean, noisy);
        Assert.Equal(clean.PolicySha256, clean.ReferenceSha256);
        Assert.Equal(clean.PolicySha256, clean.FreshLoadSha256);
    }

    [Fact]
    public void LegacySchemaV3WithoutRandomContext_HasExactNormalizedPriorJsonBytes()
    {
        const string expected = "{\n" +
            "  \"configKind\": \"legacy-v3\",\n" +
            "  \"vocabSize\": 256,\n" +
            "  \"contextLength\": 128,\n" +
            "  \"hiddenDim\": 64,\n" +
            "  \"numLayers\": 2,\n" +
            "  \"numHeads\": 4,\n" +
            "  \"numKvHeads\": 2,\n" +
            "  \"ffnHiddenDim\": 128,\n" +
            "  \"rawPayloadPersisted\": true,\n" +
            "  \"absolutePathPersisted\": false,\n" +
            "  \"schemaVersion\": 3,\n" +
            "  \"ropeTheta\": 0,\n" +
            "  \"rmsNormEps\": 0,\n" +
            "  \"providerKind\": null,\n" +
            "  \"baseModelId\": null,\n" +
            "  \"baseRevision\": null,\n" +
            "  \"baseCheckpointSha256\": null,\n" +
            "  \"configSha256\": null,\n" +
            "  \"initialWeightSha256\": null,\n" +
            "  \"lora\": null\n" +
            "}";
        var context = LegacySchemaV3Context();

        var actual = NormalizeNewlines(JsonSerializer.Serialize(
            context,
            DesignConversionCheckpointJsonOptions.Standard));

        Assert.Null(context.RandomContext);
        Assert.DoesNotContain("randomContext", actual, StringComparison.Ordinal);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(actual));
        var roundTrip = JsonSerializer.Deserialize<
            DesignConversionDpoPolicyCheckpointTrainingContext>(
                actual,
                DesignConversionCheckpointJsonOptions.Standard);
        Assert.NotNull(roundTrip);
        Assert.Null(roundTrip.RandomContext);
        Assert.Equal(
            expected,
            NormalizeNewlines(JsonSerializer.Serialize(
                roundTrip,
                DesignConversionCheckpointJsonOptions.Standard)));
    }

    [Fact]
    public void NonNullRandomContext_SerializesFullTypedObjectAndRoundTripsExactly()
    {
        var randomContext = DesignConversionDpoPolicyRandomContextContract
            .CreateCurrent(ulong.MaxValue, FingerprintA);
        var context = LegacySchemaV3Context() with
        {
            RandomContext = randomContext,
        };

        var json = JsonSerializer.Serialize(
            context,
            DesignConversionCheckpointJsonOptions.Standard);
        using var document = JsonDocument.Parse(json);
        var serialized = document.RootElement.GetProperty("randomContext");
        Assert.Equal(10, serialized.EnumerateObject().Count());
        Assert.Equal(1, serialized.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(ulong.MaxValue, serialized.GetProperty("rootInitializationSeed").GetUInt64());
        Assert.Equal(FingerprintA, serialized.GetProperty("trainingConfigurationFingerprint").GetString());
        Assert.Equal(randomContext.DerivationScheme, serialized.GetProperty("derivationScheme").GetString());
        Assert.Equal(randomContext.InitializationDomain, serialized.GetProperty("initializationDomain").GetString());
        Assert.Equal(randomContext.DropoutDomain, serialized.GetProperty("dropoutDomain").GetString());
        Assert.Equal(randomContext.PolicyDropoutStreamDomain, serialized.GetProperty("policyDropoutStreamDomain").GetString());
        Assert.Equal(randomContext.ReferenceDropoutStreamDomain, serialized.GetProperty("referenceDropoutStreamDomain").GetString());
        Assert.Equal(randomContext.FreshLoadDropoutStreamDomain, serialized.GetProperty("freshLoadDropoutStreamDomain").GetString());
        Assert.Equal(randomContext.ReproducibilityScope, serialized.GetProperty("reproducibilityScope").GetString());

        var roundTrip = JsonSerializer.Deserialize<
            DesignConversionDpoPolicyCheckpointTrainingContext>(
                json,
                DesignConversionCheckpointJsonOptions.Standard);
        Assert.NotNull(roundTrip);
        Assert.Equal(context, roundTrip);
        Assert.Equal(randomContext, roundTrip!.RandomContext);
    }

    private static DesignConversionDpoPolicyCheckpointTrainingContext
        LegacySchemaV3Context()
        => new(
            ConfigKind: "legacy-v3",
            VocabSize: 256,
            ContextLength: 128,
            HiddenDim: 64,
            NumLayers: 2,
            NumHeads: 4,
            NumKvHeads: 2,
            FfnHiddenDim: 128,
            RawPayloadPersisted: true,
            AbsolutePathPersisted: false,
            SchemaVersion: 3);

    private static async Task<ProviderModelHashes>
        CreateProviderModelHashesAfterUnrelatedModelNoiseAsync(int noiseModelCount)
    {
        var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
        for (var index = 0; index < noiseModelCount; index++)
        {
            // Эти модели намеренно создаются вне scoped seed: проверка ловит
            // прежнюю зависимость provider от уже сдвинутого process-wide RNG.
            _ = new LlmModel(config);
        }

        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var modelSet = await provider.CreateModelSetAsync(CancellationToken.None);
        Assert.True(modelSet.IsSuccess, modelSet.ErrorMessage);
        Assert.NotNull(modelSet.Value);
        Assert.Equal(1, provider.InvocationCount);

        return new ProviderModelHashes(
            DesignConversionDpoPolicyBaseMetadata.ComputeInitialWeightSha256(
                modelSet.Value!.Policy),
            DesignConversionDpoPolicyBaseMetadata.ComputeInitialWeightSha256(
                modelSet.Value.Reference),
            DesignConversionDpoPolicyBaseMetadata.ComputeInitialWeightSha256(
                modelSet.Value.FreshLoadTarget));
    }

    private sealed record ProviderModelHashes(
        string PolicySha256,
        string ReferenceSha256,
        string FreshLoadSha256);

    private static string NormalizeNewlines(string value)
        => value.Replace("\r\n", "\n", StringComparison.Ordinal);
}
