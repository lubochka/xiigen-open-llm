namespace LlmFromScratch.Tests;

using System.Text.Json;
using DPO;

public sealed class DpoTrainingReproducibilityContextTests
{
    private const ulong RootSeed = 123456789UL;
    private static readonly string Fingerprint = new('a', 64);

    [Fact]
    public void CreateCurrent_BuildsStableEvidenceAndCanonicalHash()
    {
        var context = CreateContext();
        var evidence = DpoTrainingReproducibilityEvidence.CreateCurrent(context);

        Assert.True(DpoTrainingReproducibilityContextContract.IsValidCurrent(context));
        Assert.True(DpoTrainingReproducibilityContextContract.IsValidEvidence(evidence));
        Assert.Equal(
            DpoTrainingReproducibilityContextContract.ComputeCanonicalSha256(context),
            evidence.ContextSha256);
        Assert.Contains(
            $"{DpoTrainingReproducibilityContextContract.ContextSha256EvidencePrefix}{evidence.ContextSha256}",
            evidence.EvidenceRefs);

        var same = CreateContext();
        var changed = DpoTrainingReproducibilityContext.CreateCurrent(
            RootSeed + 1,
            Fingerprint,
            CreateTopology(),
            "translation.policy",
            "translation.reference",
            "translation.fresh-load");
        Assert.Equal(
            evidence.ContextSha256,
            DpoTrainingReproducibilityContextContract.ComputeCanonicalSha256(same));
        Assert.NotEqual(
            evidence.ContextSha256,
            DpoTrainingReproducibilityContextContract.ComputeCanonicalSha256(changed));
    }

    [Theory]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaag")]
    public void CreateCurrent_RejectsNonCanonicalTrainingConfigurationFingerprint(
        string fingerprint)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            DpoTrainingReproducibilityContext.CreateCurrent(
                RootSeed,
                fingerprint,
                CreateTopology(),
                "translation.policy",
                "translation.reference",
                "translation.fresh-load"));

        Assert.Contains("fingerprint", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IsValidCurrent_RejectsTamperedDerivationAndDuplicateStreamDomains()
    {
        var context = CreateContext();
        var tamperedScheme = context with
        {
            RandomDerivationScheme = "xiigen.lora.adapter-random-seed-derivation/v999"
        };
        var duplicateDomains = context with
        {
            FreshLoadStreamDomain = context.PolicyStreamDomain
        };

        Assert.False(DpoTrainingReproducibilityContextContract.IsValidCurrent(tamperedScheme));
        Assert.False(DpoTrainingReproducibilityContextContract.IsValidCurrent(duplicateDomains));
    }

    [Fact]
    public void OldJsonWithoutSeededLoraTopology_RemainsReadableButIsNotCurrent()
    {
        const string oldJson = """
        {
          "schema_version": 1,
          "root_initialization_seed": 123456789,
          "training_configuration_fingerprint": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
          "random_derivation_scheme": "xiigen.lora.adapter-random-seed-derivation/v1",
          "initialization_domain": "xiigen.lora.adapter.initialization-seed/v1",
          "dropout_domain": "xiigen.lora.adapter.dropout-seed/v1",
          "policy_stream_domain": "translation.policy",
          "reference_stream_domain": "translation.reference",
          "fresh_load_stream_domain": "translation.fresh-load",
          "reproducibility_scope": "initialization-only-no-mid-step-dropout-resume"
        }
        """;

        var context = JsonSerializer.Deserialize<DpoTrainingReproducibilityContext>(
            oldJson,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

        Assert.NotNull(context);
        Assert.False(
            DpoTrainingReproducibilityContextContract.IsValidCurrent(context));
    }

    [Fact]
    public void IsValidEvidence_RejectsHashMutationAndMissingReservedReferences()
    {
        var context = CreateContext();
        var evidence = DpoTrainingReproducibilityEvidence.CreateCurrent(context);
        var badHash = evidence with { ContextSha256 = new string('b', 64) };
        var missingRootSeed = evidence with
        {
            EvidenceRefs = evidence.EvidenceRefs
                .Where(item => !item.StartsWith(
                    DpoTrainingReproducibilityContextContract.RootInitializationSeedEvidencePrefix,
                    StringComparison.Ordinal))
                .ToArray()
        };

        Assert.False(DpoTrainingReproducibilityContextContract.IsValidEvidence(badHash));
        Assert.False(DpoTrainingReproducibilityContextContract.IsValidEvidence(missingRootSeed));
    }

    private static DpoTrainingReproducibilityContext CreateContext()
        => DpoTrainingReproducibilityContext.CreateCurrent(
            RootSeed,
            Fingerprint,
            CreateTopology(),
            "translation.policy",
            "translation.reference",
            "translation.fresh-load");

    private static DpoTrainingSeededLoraTopology CreateTopology()
        => new(2, 4f, 0f, ["q_proj", "v_proj"]);
}
