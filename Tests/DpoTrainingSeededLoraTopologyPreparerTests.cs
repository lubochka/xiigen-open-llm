namespace LlmFromScratch.Tests;

using DPO;
using LoRA;
using TransformerModel;
using TransformerModel.Config;

public sealed class DpoTrainingSeededLoraTopologyPreparerTests
{
    [Fact]
    public void ApplyFreshSeededTriple_PreparesThreeRolesAndRejectsSwappedRoleValidation()
    {
        var context = CreateContext();
        var policy = new LlmModel(BuildTinyConfig());
        var reference = new LlmModel(BuildTinyConfig());
        var freshLoad = new LlmModel(BuildTinyConfig());

        var evidence = DpoTrainingSeededLoraTopologyPreparer
            .ApplyFreshSeededTriple(policy, reference, freshLoad, context);

        Assert.True(
            DpoTrainingSeededLoraTopologyContract.TryValidateApplicationEvidence(
                evidence,
                context,
                out var reason),
            reason);
        Assert.Equal(2, evidence.PolicyAdapterCount);
        Assert.Equal(2, evidence.ReferenceAdapterCount);
        Assert.Equal(2, evidence.FreshLoadAdapterCount);
        Assert.Equal(
            evidence.PolicyAdapterCount,
            DpoTrainingSeededLoraTopologyPreparer
                .ValidateSeededRole(
                    policy,
                    context,
                    DpoTrainingSeededLoraModelRole.Policy)
                .Count);

        // Один и тот же набор адаптеров нельзя выдать за другую роль: домен
        // роли входит в seed каждого LoRA-слота.
        var mismatch = Assert.Throws<InvalidOperationException>(() =>
            DpoTrainingSeededLoraTopologyPreparer.ValidateSeededRole(
                policy,
                context,
                DpoTrainingSeededLoraModelRole.Reference));
        Assert.Equal(
            LoraApplicator.SeededAdapterProvenanceMismatchErrorCode,
            mismatch.Message);
    }

    [Fact]
    public void ApplyFreshSeededTriple_PreoccupiedFreshLoadFailsAtomicallyAndLeavesOtherRolesFresh()
    {
        var context = CreateContext();
        var policy = new LlmModel(BuildTinyConfig());
        var reference = new LlmModel(BuildTinyConfig());
        var freshLoad = new LlmModel(BuildTinyConfig());
        var config = context.SeededLoraTopology.ToLoraConfig();
        _ = LoraApplicator.Apply(freshLoad, config);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            DpoTrainingSeededLoraTopologyPreparer.ApplyFreshSeededTriple(
                policy,
                reference,
                freshLoad,
                context));

        Assert.Equal(
            LoraApplicator.SeededAdapterReuseUnprovenErrorCode,
            exception.Message);
        LoraApplicator.ValidateFreshApplication(policy, config);
        LoraApplicator.ValidateFreshApplication(reference, config);
    }

    private static DpoTrainingReproducibilityContext CreateContext()
        => DpoTrainingReproducibilityContext.CreateCurrent(
            123456789UL,
            new string('e', 64),
            new DpoTrainingSeededLoraTopology(
                2,
                4f,
                0f,
                ["q_proj", "v_proj"]),
            "translation.policy",
            "translation.reference",
            "translation.fresh-load");

    private static ModelConfig BuildTinyConfig()
        => new()
        {
            VocabSize = 512,
            ContextLen = 64,
            HiddenDim = 16,
            NumLayers = 1,
            NumHeads = 4,
            NumKvHeads = 2,
            FfnHiddenDim = 32
        };
}
