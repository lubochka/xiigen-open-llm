// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Runtime.CompilerServices;
using TransformerModel;

[assembly: InternalsVisibleTo("LlmFromScratch.Tests")]
[assembly: InternalsVisibleTo("CapabilityTraining")]

namespace LoRA;

internal enum LoraApplyPhase
{
    BeforeModelLock,
    InsideModelLock,
    AfterCandidateConstruction,
    AfterPreflightReread
}

internal enum LoraSeededTripleApplyPhase
{
    BeforePolicyPlanConstruction,
    BeforeReferencePlanConstruction,
    BeforeFreshLoadPlanConstruction,
    BeforePolicyPreflightReread,
    BeforeReferencePreflightReread,
    BeforeFreshLoadPreflightReread,
    BeforeCommit
}

public sealed record LoraSeededPairApplication(
    IReadOnlyList<LoraLinear> FirstModelAdapters,
    IReadOnlyList<LoraLinear> SecondModelAdapters);

public sealed record LoraSeededTripleApplication(
    IReadOnlyList<LoraLinear> PolicyModelAdapters,
    IReadOnlyList<LoraLinear> ReferenceModelAdapters,
    IReadOnlyList<LoraLinear> FreshLoadModelAdapters);

public static class LoraApplicator
{
    public const string SeededAdapterReuseUnprovenErrorCode =
        "LORA_SEEDED_ADAPTER_REUSE_UNPROVEN";
    public const string SeededAdapterProvenanceMismatchErrorCode =
        "LORA_SEEDED_ADAPTER_PROVENANCE_MISMATCH";

    private static readonly string[] CanonicalTargets =
        ["q_proj", "k_proj", "v_proj", "o_proj"];
    private static readonly object AdapterIdentityHashCollisionGate = new();

    public static IReadOnlyList<LoraLinear> Apply(LlmModel model, LoraConfig config)
        => Apply(model, config, phaseHook: null);

    /// <summary>
    /// Applies deterministic per-slot randomness. The resolver must be pure,
    /// fast, and must not mutate <paramref name="model"/>.
    /// </summary>
    public static IReadOnlyList<LoraLinear> Apply(
        LlmModel model,
        LoraConfig config,
        Func<int, string, LoraAdapterRandomSeeds> randomSeedResolver)
    {
        if (randomSeedResolver is null)
        {
            throw new ArgumentNullException(
                nameof(randomSeedResolver),
                "LORA_RANDOM_SEED_RESOLVER_REQUIRED");
        }
        return ApplyCore(
            model,
            config,
            phaseHook: null,
            randomSeedResolver,
            randomSeedStreamDomain: null);
    }

    public static IReadOnlyList<LoraLinear> ApplySeeded(
        LlmModel model,
        LoraConfig config,
        ulong rootSeed,
        string dropoutStreamDomain)
    {
        ArgumentNullException.ThrowIfNull(dropoutStreamDomain);
        _ = LoraAdapterRandomSeedDerivation.Derive(
            rootSeed,
            0,
            "q_proj",
            dropoutStreamDomain);
        return ApplyCore(
            model,
            config,
            phaseHook: null,
            randomSeedResolver: (blockIndex, projection) =>
                LoraAdapterRandomSeedDerivation.Derive(
                    rootSeed,
                    blockIndex,
                    projection,
                    dropoutStreamDomain),
            randomSeedStreamDomain: dropoutStreamDomain);
    }

    internal static IReadOnlyList<LoraLinear> Apply(
        LlmModel model,
        LoraConfig config,
        Action<LoraApplyPhase>? phaseHook)
        => ApplyCore(
            model,
            config,
            phaseHook,
            randomSeedResolver: null,
            randomSeedStreamDomain: null);

    public static LoraSeededPairApplication ApplyFreshSeededPair(
        LlmModel firstModel,
        LlmModel secondModel,
        LoraConfig config,
        ulong rootSeed,
        string firstDropoutStreamDomain,
        string secondDropoutStreamDomain)
        => ApplyFreshSeededPair(
            firstModel,
            secondModel,
            config,
            rootSeed,
            firstDropoutStreamDomain,
            secondDropoutStreamDomain,
            beforeMutationUnderPairLock: null,
            afterCommitUnderPairLock: null);

    /// <summary>
    /// The callbacks are trusted internal synchronous transaction participants.
    /// They run with both model mutation leases held continuously, may use
    /// reentrant model snapshot APIs, and must not recursively apply adapters or
    /// start asynchronous work.
    /// </summary>
    internal static LoraSeededPairApplication ApplyFreshSeededPair(
        LlmModel firstModel,
        LlmModel secondModel,
        LoraConfig config,
        ulong rootSeed,
        string firstDropoutStreamDomain,
        string secondDropoutStreamDomain,
        Action? beforeMutationUnderPairLock,
        Action<LoraSeededPairApplication>? afterCommitUnderPairLock)
    {
        if (firstModel is null)
            throw new ArgumentNullException(nameof(firstModel), "LORA_MODEL_REQUIRED");
        if (secondModel is null)
            throw new ArgumentNullException(nameof(secondModel), "LORA_MODEL_REQUIRED");
        if (ReferenceEquals(firstModel, secondModel))
        {
            throw new ArgumentException(
                "LORA_MODELS_MUST_BE_DISTINCT",
                nameof(secondModel));
        }
        if (firstDropoutStreamDomain is null)
        {
            throw new ArgumentNullException(
                nameof(firstDropoutStreamDomain),
                "LORA_DROPOUT_STREAM_DOMAIN_REQUIRED");
        }
        if (secondDropoutStreamDomain is null)
        {
            throw new ArgumentNullException(
                nameof(secondDropoutStreamDomain),
                "LORA_DROPOUT_STREAM_DOMAIN_REQUIRED");
        }

        var canonical = ValidateAndCanonicalize(config);
        ValidateSeededPairDomain(rootSeed, firstDropoutStreamDomain);
        ValidateSeededPairDomain(rootSeed, secondDropoutStreamDomain);
        if (canonical.TargetModules.Length == 0)
        {
            return new LoraSeededPairApplication(
                Array.Empty<LoraLinear>(),
                Array.Empty<LoraLinear>());
        }

        var firstIdentityHash = RuntimeHelpers.GetHashCode(firstModel);
        var secondIdentityHash = RuntimeHelpers.GetHashCode(secondModel);
        if (firstIdentityHash < secondIdentityHash)
        {
            return ApplyFreshSeededPairWithOrderedLocks(
                firstModel,
                secondModel,
                lockFirstRepresentsFirstModel: true,
                firstModel: firstModel,
                secondModel: secondModel,
                canonical: canonical,
                rootSeed: rootSeed,
                firstDropoutStreamDomain: firstDropoutStreamDomain,
                secondDropoutStreamDomain: secondDropoutStreamDomain,
                beforeMutationUnderPairLock: beforeMutationUnderPairLock,
                afterCommitUnderPairLock: afterCommitUnderPairLock);
        }
        if (firstIdentityHash > secondIdentityHash)
        {
            return ApplyFreshSeededPairWithOrderedLocks(
                secondModel,
                firstModel,
                lockFirstRepresentsFirstModel: false,
                firstModel: firstModel,
                secondModel: secondModel,
                canonical: canonical,
                rootSeed: rootSeed,
                firstDropoutStreamDomain: firstDropoutStreamDomain,
                secondDropoutStreamDomain: secondDropoutStreamDomain,
                beforeMutationUnderPairLock: beforeMutationUnderPairLock,
                afterCommitUnderPairLock: afterCommitUnderPairLock);
        }

        lock (AdapterIdentityHashCollisionGate)
        {
            return ApplyFreshSeededPairWithOrderedLocks(
                firstModel,
                secondModel,
                lockFirstRepresentsFirstModel: true,
                firstModel: firstModel,
                secondModel: secondModel,
                canonical: canonical,
                rootSeed: rootSeed,
                firstDropoutStreamDomain: firstDropoutStreamDomain,
                secondDropoutStreamDomain: secondDropoutStreamDomain,
                beforeMutationUnderPairLock: beforeMutationUnderPairLock,
                afterCommitUnderPairLock: afterCommitUnderPairLock);
        }
    }

    public static LoraSeededTripleApplication ApplyFreshSeededTriple(
        LlmModel policyModel,
        LlmModel referenceModel,
        LlmModel freshLoadModel,
        LoraConfig config,
        ulong rootSeed,
        string policyDropoutStreamDomain,
        string referenceDropoutStreamDomain,
        string freshLoadDropoutStreamDomain)
        => ApplyFreshSeededTriple(
            policyModel,
            referenceModel,
            freshLoadModel,
            config,
            rootSeed,
            policyDropoutStreamDomain,
            referenceDropoutStreamDomain,
            freshLoadDropoutStreamDomain,
            phaseHook: null);

    internal static LoraSeededTripleApplication ApplyFreshSeededTriple(
        LlmModel policyModel,
        LlmModel referenceModel,
        LlmModel freshLoadModel,
        LoraConfig config,
        ulong rootSeed,
        string policyDropoutStreamDomain,
        string referenceDropoutStreamDomain,
        string freshLoadDropoutStreamDomain,
        Action<LoraSeededTripleApplyPhase>? phaseHook)
    {
        if (policyModel is null)
            throw new ArgumentNullException(nameof(policyModel), "LORA_MODEL_REQUIRED");
        if (referenceModel is null)
            throw new ArgumentNullException(nameof(referenceModel), "LORA_MODEL_REQUIRED");
        if (freshLoadModel is null)
            throw new ArgumentNullException(nameof(freshLoadModel), "LORA_MODEL_REQUIRED");
        if (ReferenceEquals(policyModel, referenceModel) ||
            ReferenceEquals(policyModel, freshLoadModel) ||
            ReferenceEquals(referenceModel, freshLoadModel))
        {
            throw new ArgumentException(
                "LORA_MODELS_MUST_BE_DISTINCT",
                nameof(freshLoadModel));
        }
        if (policyDropoutStreamDomain is null)
        {
            throw new ArgumentNullException(
                nameof(policyDropoutStreamDomain),
                "LORA_DROPOUT_STREAM_DOMAIN_REQUIRED");
        }
        if (referenceDropoutStreamDomain is null)
        {
            throw new ArgumentNullException(
                nameof(referenceDropoutStreamDomain),
                "LORA_DROPOUT_STREAM_DOMAIN_REQUIRED");
        }
        if (freshLoadDropoutStreamDomain is null)
        {
            throw new ArgumentNullException(
                nameof(freshLoadDropoutStreamDomain),
                "LORA_DROPOUT_STREAM_DOMAIN_REQUIRED");
        }

        var canonical = ValidateAndCanonicalize(config);
        ValidateSeededPairDomain(rootSeed, policyDropoutStreamDomain);
        ValidateSeededPairDomain(rootSeed, referenceDropoutStreamDomain);
        ValidateSeededPairDomain(rootSeed, freshLoadDropoutStreamDomain);
        if (canonical.TargetModules.Length == 0)
        {
            return new LoraSeededTripleApplication(
                Array.Empty<LoraLinear>(),
                Array.Empty<LoraLinear>(),
                Array.Empty<LoraLinear>());
        }

        var orderedLocks = new[]
        {
            new SeededTripleLockTarget(
                policyModel,
                SemanticIndex: 0,
                IdentityHash: RuntimeHelpers.GetHashCode(policyModel)),
            new SeededTripleLockTarget(
                referenceModel,
                SemanticIndex: 1,
                IdentityHash: RuntimeHelpers.GetHashCode(referenceModel)),
            new SeededTripleLockTarget(
                freshLoadModel,
                SemanticIndex: 2,
                IdentityHash: RuntimeHelpers.GetHashCode(freshLoadModel))
        };
        Array.Sort(orderedLocks, static (left, right) =>
        {
            var hashComparison = left.IdentityHash.CompareTo(right.IdentityHash);
            return hashComparison != 0
                ? hashComparison
                : left.SemanticIndex.CompareTo(right.SemanticIndex);
        });

        var hasIdentityHashCollision =
            orderedLocks[0].IdentityHash == orderedLocks[1].IdentityHash ||
            orderedLocks[1].IdentityHash == orderedLocks[2].IdentityHash;
        if (!hasIdentityHashCollision)
        {
            return ApplyFreshSeededTripleWithOrderedLocks(
                orderedLocks,
                canonical,
                rootSeed,
                policyDropoutStreamDomain,
                referenceDropoutStreamDomain,
                freshLoadDropoutStreamDomain,
                phaseHook);
        }

        lock (AdapterIdentityHashCollisionGate)
        {
            return ApplyFreshSeededTripleWithOrderedLocks(
                orderedLocks,
                canonical,
                rootSeed,
                policyDropoutStreamDomain,
                referenceDropoutStreamDomain,
                freshLoadDropoutStreamDomain,
                phaseHook);
        }
    }

    /// <summary>
    /// Validates that every requested canonical slot can receive a new seeded
    /// adapter. The validation runs under the model adapter lock and does not
    /// construct adapters or mutate model state.
    /// </summary>
    public static void ValidateFreshApplication(LlmModel model, LoraConfig config)
    {
        if (model is null)
            throw new ArgumentNullException(nameof(model), "LORA_MODEL_REQUIRED");

        _ = model.ExecuteAdapterMutation(lease =>
        {
            var canonical = ValidateAndCanonicalize(config);
            ValidateFreshSlots(lease, model.Blocks.Count, canonical);
            return true;
        });
    }

    /// <summary>
    /// Validates and adopts an already attached seeded topology without
    /// constructing adapters or mutating model state.
    /// </summary>
    public static IReadOnlyList<LoraLinear> ValidateSeededApplicationProvenance(
        LlmModel model,
        LoraConfig config,
        Func<int, string, LoraAdapterRandomSeeds> randomSeedResolver)
    {
        if (model is null)
            throw new ArgumentNullException(nameof(model), "LORA_MODEL_REQUIRED");
        if (randomSeedResolver is null)
        {
            throw new ArgumentNullException(
                nameof(randomSeedResolver),
                "LORA_RANDOM_SEED_RESOLVER_REQUIRED");
        }

        return model.ExecuteAdapterMutation(lease =>
        {
            var canonical = ValidateAndCanonicalize(config);
            var plans = new List<SeededProvenancePlan>(
                model.Blocks.Count * canonical.TargetModules.Length);
            for (var blockIndex = 0; blockIndex < model.Blocks.Count; blockIndex++)
            {
                foreach (var projection in canonical.TargetModules)
                {
                    var state = InspectSeededProvenanceProjection(
                        lease,
                        blockIndex,
                        projection);
                    var adapter = ValidateSeededProvenanceStructure(
                        state,
                        canonical);
                    var expectedSeeds = randomSeedResolver(blockIndex, projection);
                    if (adapter.RandomSeeds is not { } actualSeeds ||
                        actualSeeds != expectedSeeds)
                    {
                        throw new InvalidOperationException(
                            SeededAdapterProvenanceMismatchErrorCode);
                    }

                    plans.Add(new SeededProvenancePlan(
                        blockIndex,
                        projection,
                        state,
                        adapter,
                        expectedSeeds));
                }
            }

            ValidateSeededProvenancePlansUnchanged(lease, plans, canonical);
            return plans.Select(plan => plan.Adapter).ToArray();
        });
    }

    private static IReadOnlyList<LoraLinear> ApplyCore(
        LlmModel model,
        LoraConfig config,
        Action<LoraApplyPhase>? phaseHook,
        Func<int, string, LoraAdapterRandomSeeds>? randomSeedResolver,
        string? randomSeedStreamDomain)
    {
        if (model is null)
            throw new ArgumentNullException(nameof(model), "LORA_MODEL_REQUIRED");

        phaseHook?.Invoke(LoraApplyPhase.BeforeModelLock);
        return model.ExecuteAdapterMutation(lease =>
        {
            phaseHook?.Invoke(LoraApplyPhase.InsideModelLock);
            var canonical = ValidateAndCanonicalize(config);
            if (canonical.TargetModules.Length == 0)
                return Array.Empty<LoraLinear>();

            if (randomSeedResolver is not null)
                ValidateFreshSlots(lease, model.Blocks.Count, canonical);

            var plans = new List<ApplicationPlan>(
                model.Blocks.Count * canonical.TargetModules.Length);
            for (var blockIndex = 0; blockIndex < model.Blocks.Count; blockIndex++)
            {
                foreach (var projection in canonical.TargetModules)
                {
                    var state = lease.InspectProjection(blockIndex, projection);
                    plans.Add(BuildPlan(
                        blockIndex,
                        projection,
                        state,
                        canonical,
                        randomSeedResolver,
                        randomSeedStreamDomain));
                }
            }

            phaseHook?.Invoke(LoraApplyPhase.AfterCandidateConstruction);
            ValidatePlansUnchanged(lease, plans, canonical);
            phaseHook?.Invoke(LoraApplyPhase.AfterPreflightReread);
            ValidatePlansUnchanged(lease, plans, canonical);

            foreach (var parameter in lease.BaseParameters)
                parameter.Value.RequiresGrad = false;

            foreach (var plan in plans)
            {
                if (plan.IsNew)
                {
                    lease.PopulateAdapterPair(
                        plan.BlockIndex,
                        plan.Projection,
                        plan.Adapter);
                }
            }

            foreach (var plan in plans)
                plan.Adapter.EnableAdapterGradients();

            return plans.Select(plan => plan.Adapter).ToArray();
        });
    }

    private static LoraSeededPairApplication ApplyFreshSeededPairWithOrderedLocks(
        LlmModel lockFirstModel,
        LlmModel lockSecondModel,
        bool lockFirstRepresentsFirstModel,
        LlmModel firstModel,
        LlmModel secondModel,
        LoraConfig canonical,
        ulong rootSeed,
        string firstDropoutStreamDomain,
        string secondDropoutStreamDomain,
        Action? beforeMutationUnderPairLock,
        Action<LoraSeededPairApplication>? afterCommitUnderPairLock)
        => lockFirstModel.ExecuteAdapterMutation(lockFirstLease =>
            lockSecondModel.ExecuteAdapterMutation(lockSecondLease =>
            {
                var firstLease = lockFirstRepresentsFirstModel
                    ? lockFirstLease
                    : lockSecondLease;
                var secondLease = lockFirstRepresentsFirstModel
                    ? lockSecondLease
                    : lockFirstLease;

                beforeMutationUnderPairLock?.Invoke();

                ValidateFreshSlots(firstLease, firstModel.Blocks.Count, canonical);
                ValidateFreshSlots(secondLease, secondModel.Blocks.Count, canonical);

                var expectedFirstAdapterCount = checked(
                    firstModel.Blocks.Count * canonical.TargetModules.Length);
                var expectedSecondAdapterCount = checked(
                    secondModel.Blocks.Count * canonical.TargetModules.Length);
                if (expectedFirstAdapterCount == 0 ||
                    expectedSecondAdapterCount == 0)
                {
                    throw new InvalidOperationException(
                        SeededAdapterProvenanceMismatchErrorCode);
                }

                var firstPlans = BuildFreshSeededPlans(
                    firstLease,
                    firstModel.Blocks.Count,
                    canonical,
                    rootSeed,
                    firstDropoutStreamDomain);
                var secondPlans = BuildFreshSeededPlans(
                    secondLease,
                    secondModel.Blocks.Count,
                    canonical,
                    rootSeed,
                    secondDropoutStreamDomain);
                var firstAdapters = firstPlans
                    .Select(static plan => plan.Adapter)
                    .ToArray();
                var secondAdapters = secondPlans
                    .Select(static plan => plan.Adapter)
                    .ToArray();
                var application = new LoraSeededPairApplication(
                    firstAdapters,
                    secondAdapters);
                if (firstPlans.Count != expectedFirstAdapterCount ||
                    secondPlans.Count != expectedSecondAdapterCount ||
                    firstAdapters.Length != expectedFirstAdapterCount ||
                    secondAdapters.Length != expectedSecondAdapterCount)
                {
                    throw new InvalidOperationException(
                        SeededAdapterProvenanceMismatchErrorCode);
                }

                ValidateFreshSeededPlansUnchanged(firstLease, firstPlans, canonical);
                ValidateFreshSeededPlansUnchanged(secondLease, secondPlans, canonical);

                FreezeBaseParameters(firstLease);
                FreezeBaseParameters(secondLease);
                PopulateFreshSeededPlans(firstLease, firstPlans);
                PopulateFreshSeededPlans(secondLease, secondPlans);
                EnableFreshSeededPlanGradients(firstPlans);
                EnableFreshSeededPlanGradients(secondPlans);

                ValidateSeededProvenancePlansUnchanged(
                    firstLease,
                    firstPlans,
                    canonical);
                ValidateSeededProvenancePlansUnchanged(
                    secondLease,
                    secondPlans,
                    canonical);
                afterCommitUnderPairLock?.Invoke(application);
                return application;
            }));

    private static LoraSeededTripleApplication ApplyFreshSeededTripleWithOrderedLocks(
        IReadOnlyList<SeededTripleLockTarget> orderedLocks,
        LoraConfig canonical,
        ulong rootSeed,
        string policyDropoutStreamDomain,
        string referenceDropoutStreamDomain,
        string freshLoadDropoutStreamDomain,
        Action<LoraSeededTripleApplyPhase>? phaseHook)
        => orderedLocks[0].Model.ExecuteAdapterMutation(firstLockLease =>
            orderedLocks[1].Model.ExecuteAdapterMutation(secondLockLease =>
                orderedLocks[2].Model.ExecuteAdapterMutation(thirdLockLease =>
                {
                    var semanticModels = new LlmModel[3];
                    var semanticLeases = new LlmModel.AdapterMutationLease[3];
                    semanticModels[orderedLocks[0].SemanticIndex] = orderedLocks[0].Model;
                    semanticModels[orderedLocks[1].SemanticIndex] = orderedLocks[1].Model;
                    semanticModels[orderedLocks[2].SemanticIndex] = orderedLocks[2].Model;
                    semanticLeases[orderedLocks[0].SemanticIndex] = firstLockLease;
                    semanticLeases[orderedLocks[1].SemanticIndex] = secondLockLease;
                    semanticLeases[orderedLocks[2].SemanticIndex] = thirdLockLease;

                    for (var index = 0; index < semanticModels.Length; index++)
                    {
                        ValidateFreshSlots(
                            semanticLeases[index],
                            semanticModels[index].Blocks.Count,
                            canonical);
                    }

                    var expectedAdapterCounts = new int[3];
                    for (var index = 0; index < semanticModels.Length; index++)
                    {
                        expectedAdapterCounts[index] = checked(
                            semanticModels[index].Blocks.Count *
                            canonical.TargetModules.Length);
                        if (expectedAdapterCounts[index] == 0)
                        {
                            throw new InvalidOperationException(
                                SeededAdapterProvenanceMismatchErrorCode);
                        }
                    }

                    var domains = new[]
                    {
                        policyDropoutStreamDomain,
                        referenceDropoutStreamDomain,
                        freshLoadDropoutStreamDomain
                    };
                    var constructionPhases = new[]
                    {
                        LoraSeededTripleApplyPhase.BeforePolicyPlanConstruction,
                        LoraSeededTripleApplyPhase.BeforeReferencePlanConstruction,
                        LoraSeededTripleApplyPhase.BeforeFreshLoadPlanConstruction
                    };
                    var rereadPhases = new[]
                    {
                        LoraSeededTripleApplyPhase.BeforePolicyPreflightReread,
                        LoraSeededTripleApplyPhase.BeforeReferencePreflightReread,
                        LoraSeededTripleApplyPhase.BeforeFreshLoadPreflightReread
                    };
                    var plans = new List<SeededProvenancePlan>[3];
                    var adapters = new LoraLinear[3][];
                    for (var index = 0; index < semanticModels.Length; index++)
                    {
                        phaseHook?.Invoke(constructionPhases[index]);
                        plans[index] = BuildFreshSeededPlans(
                            semanticLeases[index],
                            semanticModels[index].Blocks.Count,
                            canonical,
                            rootSeed,
                            domains[index]);
                        adapters[index] = plans[index]
                            .Select(static plan => plan.Adapter)
                            .ToArray();
                        if (plans[index].Count != expectedAdapterCounts[index] ||
                            adapters[index].Length != expectedAdapterCounts[index])
                        {
                            throw new InvalidOperationException(
                                SeededAdapterProvenanceMismatchErrorCode);
                        }
                    }

                    for (var index = 0; index < semanticModels.Length; index++)
                    {
                        phaseHook?.Invoke(rereadPhases[index]);
                        ValidateFreshSeededPlansUnchanged(
                            semanticLeases[index],
                            plans[index],
                            canonical);
                    }

                    phaseHook?.Invoke(LoraSeededTripleApplyPhase.BeforeCommit);
                    for (var index = 0; index < semanticModels.Length; index++)
                        FreezeBaseParameters(semanticLeases[index]);
                    for (var index = 0; index < semanticModels.Length; index++)
                        PopulateFreshSeededPlans(semanticLeases[index], plans[index]);
                    for (var index = 0; index < semanticModels.Length; index++)
                        EnableFreshSeededPlanGradients(plans[index]);
                    for (var index = 0; index < semanticModels.Length; index++)
                    {
                        ValidateSeededProvenancePlansUnchanged(
                            semanticLeases[index],
                            plans[index],
                            canonical);
                    }

                    return new LoraSeededTripleApplication(
                        adapters[0],
                        adapters[1],
                        adapters[2]);
                })));

    private static void ValidateSeededPairDomain(
        ulong rootSeed,
        string dropoutStreamDomain)
        => _ = LoraAdapterRandomSeedDerivation.Derive(
            rootSeed,
            blockIndex: 0,
            canonicalProjection: CanonicalTargets[0],
            dropoutStreamDomain: dropoutStreamDomain);

    public static LoraConfig ValidateAndCanonicalize(LoraConfig config)
    {
        if (config is null)
            throw new ArgumentNullException(nameof(config), "LORA_CONFIG_REQUIRED");
        if (config.Rank <= 0)
            throw new ArgumentOutOfRangeException(nameof(config), "LORA_RANK_INVALID");
        if (!float.IsFinite(config.Alpha))
            throw new ArgumentOutOfRangeException(nameof(config), "LORA_ALPHA_INVALID");

        var scaling = config.Alpha / config.Rank;
        if (!float.IsFinite(scaling))
            throw new ArgumentOutOfRangeException(nameof(config), "LORA_SCALING_INVALID");
        if (!float.IsFinite(config.Dropout) || config.Dropout < 0f || config.Dropout >= 1f)
            throw new ArgumentOutOfRangeException(nameof(config), "LORA_DROPOUT_INVALID");
        if (config.TargetModules is null)
            throw new ArgumentException("LORA_TARGET_MODULES_REQUIRED", nameof(config));

        var requested = new HashSet<string>(StringComparer.Ordinal);
        foreach (var target in config.TargetModules)
        {
            if (target is null
                || !CanonicalTargets.Contains(target, StringComparer.Ordinal))
            {
                throw new ArgumentException("LORA_TARGET_MODULE_INVALID", nameof(config));
            }

            requested.Add(target);
        }

        return new LoraConfig
        {
            Rank = config.Rank,
            Alpha = config.Alpha,
            Dropout = config.Dropout,
            TargetModules = CanonicalTargets
                .Where(requested.Contains)
                .ToArray()
        };
    }

    private static ApplicationPlan BuildPlan(
        int blockIndex,
        string projection,
        LlmModel.AdapterAttachmentState state,
        LoraConfig canonical,
        Func<int, string, LoraAdapterRandomSeeds>? randomSeedResolver,
        string? randomSeedStreamDomain)
    {
        if (state.AttentionAdapter is null && state.SlotAdapter is null)
        {
            var adapter = randomSeedResolver is null
                ? new LoraLinear(state.BaseProjection, canonical)
                : randomSeedStreamDomain is null
                    ? new LoraLinear(
                        state.BaseProjection,
                        canonical,
                        randomSeedResolver(blockIndex, projection))
                    : new LoraLinear(
                        state.BaseProjection,
                        canonical,
                        randomSeedResolver(blockIndex, projection),
                        randomSeedStreamDomain);
            return new ApplicationPlan(
                blockIndex,
                projection,
                state,
                adapter,
                IsNew: true);
        }

        if (randomSeedResolver is not null)
            throw new InvalidOperationException(SeededAdapterReuseUnprovenErrorCode);

        if (state.AttentionAdapter is null
            || state.SlotAdapter is null
            || !ReferenceEquals(state.AttentionAdapter, state.SlotAdapter))
        {
            throw new InvalidOperationException("LORA_REGISTRATION_KEY_CONFLICT");
        }

        if (state.AttentionAdapter is not LoraLinear existing)
            throw new InvalidOperationException("LORA_PROJECTION_ADAPTER_CONFLICT");
        if (!ReferenceEquals(existing.BaseProjection, state.BaseProjection))
            throw new InvalidOperationException("LORA_PROJECTION_ADAPTER_CONFLICT");
        if (!existing.HasCompatibleConfiguration(canonical))
            throw new InvalidOperationException("LORA_ADAPTER_CONFIG_MISMATCH");

        return new ApplicationPlan(
            blockIndex,
            projection,
            state,
            existing,
            IsNew: false);
    }

    private static void ValidateFreshSlots(
        LlmModel.AdapterMutationLease lease,
        int blockCount,
        LoraConfig canonical)
    {
        for (var blockIndex = 0; blockIndex < blockCount; blockIndex++)
        {
            foreach (var projection in canonical.TargetModules)
            {
                var state = lease.InspectProjection(blockIndex, projection);
                if (state.AttentionAdapter is not null || state.SlotAdapter is not null)
                {
                    throw new InvalidOperationException(
                        SeededAdapterReuseUnprovenErrorCode);
                }
            }
        }
    }

    private static List<SeededProvenancePlan> BuildFreshSeededPlans(
        LlmModel.AdapterMutationLease lease,
        int blockCount,
        LoraConfig canonical,
        ulong rootSeed,
        string dropoutStreamDomain)
    {
        var plans = new List<SeededProvenancePlan>(
            blockCount * canonical.TargetModules.Length);
        for (var blockIndex = 0; blockIndex < blockCount; blockIndex++)
        {
            foreach (var projection in canonical.TargetModules)
            {
                var state = lease.InspectProjection(blockIndex, projection);
                if (state.AttentionAdapter is not null || state.SlotAdapter is not null)
                {
                    throw new InvalidOperationException(
                        SeededAdapterReuseUnprovenErrorCode);
                }

                var expectedSeeds = LoraAdapterRandomSeedDerivation.Derive(
                    rootSeed,
                    blockIndex,
                    projection,
                    dropoutStreamDomain);
                plans.Add(new SeededProvenancePlan(
                    blockIndex,
                    projection,
                    state,
                    new LoraLinear(
                        state.BaseProjection,
                        canonical,
                        expectedSeeds,
                        dropoutStreamDomain),
                    expectedSeeds));
            }
        }

        return plans;
    }

    private static void ValidateFreshSeededPlansUnchanged(
        LlmModel.AdapterMutationLease lease,
        IReadOnlyList<SeededProvenancePlan> plans,
        LoraConfig canonical)
    {
        for (var index = 0; index < plans.Count; index++)
        {
            var plan = plans[index];
            var current = lease.InspectProjection(
                plan.BlockIndex,
                plan.Projection);
            if (!ReferenceEquals(
                    current.BaseProjection,
                    plan.OriginalState.BaseProjection) ||
                current.AttentionAdapter is not null ||
                current.SlotAdapter is not null ||
                !plan.Adapter.HasCompatibleConfiguration(canonical) ||
                plan.Adapter.RandomSeeds is not { } actualSeeds ||
                actualSeeds != plan.ExpectedSeeds)
            {
                throw new InvalidOperationException(
                    SeededAdapterProvenanceMismatchErrorCode);
            }
        }
    }

    private static void FreezeBaseParameters(
        LlmModel.AdapterMutationLease lease)
    {
        var parameters = lease.BaseParameters;
        for (var index = 0; index < parameters.Count; index++)
            parameters[index].Value.RequiresGrad = false;
    }

    private static void PopulateFreshSeededPlans(
        LlmModel.AdapterMutationLease lease,
        IReadOnlyList<SeededProvenancePlan> plans)
    {
        for (var index = 0; index < plans.Count; index++)
        {
            var plan = plans[index];
            lease.PopulateAdapterPair(
                plan.BlockIndex,
                plan.Projection,
                plan.Adapter);
        }
    }

    private static void EnableFreshSeededPlanGradients(
        IReadOnlyList<SeededProvenancePlan> plans)
    {
        for (var index = 0; index < plans.Count; index++)
            plans[index].Adapter.EnableAdapterGradients();
    }

    private static LoraLinear ValidateSeededProvenanceStructure(
        LlmModel.AdapterAttachmentState state,
        LoraConfig canonical)
    {
        if (state.AttentionAdapter is null ||
            state.SlotAdapter is null ||
            !ReferenceEquals(state.AttentionAdapter, state.SlotAdapter) ||
            state.AttentionAdapter is not LoraLinear adapter ||
            !ReferenceEquals(adapter.BaseProjection, state.BaseProjection) ||
            !adapter.HasCompatibleConfiguration(canonical))
        {
            throw new InvalidOperationException(
                SeededAdapterProvenanceMismatchErrorCode);
        }

        return adapter;
    }

    private static LlmModel.AdapterAttachmentState
        InspectSeededProvenanceProjection(
            LlmModel.AdapterMutationLease lease,
            int blockIndex,
            string projection)
    {
        try
        {
            return lease.InspectProjection(blockIndex, projection);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException(
                SeededAdapterProvenanceMismatchErrorCode);
        }
    }

    private static void ValidateSeededProvenancePlansUnchanged(
        LlmModel.AdapterMutationLease lease,
        IReadOnlyList<SeededProvenancePlan> plans,
        LoraConfig canonical)
    {
        foreach (var plan in plans)
        {
            var current = InspectSeededProvenanceProjection(
                lease,
                plan.BlockIndex,
                plan.Projection);
            if (!ReferenceEquals(current.BaseProjection, plan.OriginalState.BaseProjection) ||
                !ReferenceEquals(current.AttentionAdapter, plan.Adapter) ||
                !ReferenceEquals(current.SlotAdapter, plan.Adapter) ||
                !plan.Adapter.HasCompatibleConfiguration(canonical) ||
                plan.Adapter.RandomSeeds is not { } currentSeeds ||
                currentSeeds != plan.ExpectedSeeds)
            {
                throw new InvalidOperationException(
                    SeededAdapterProvenanceMismatchErrorCode);
            }
        }
    }

    private static void ValidatePlansUnchanged(
        LlmModel.AdapterMutationLease lease,
        IReadOnlyList<ApplicationPlan> plans,
        LoraConfig canonical)
    {
        foreach (var plan in plans)
        {
            var current = lease.InspectProjection(plan.BlockIndex, plan.Projection);
            if (!ReferenceEquals(current.BaseProjection, plan.OriginalState.BaseProjection)
                || !ReferenceEquals(current.AttentionAdapter, plan.OriginalState.AttentionAdapter)
                || !ReferenceEquals(current.SlotAdapter, plan.OriginalState.SlotAdapter))
            {
                throw new InvalidOperationException("LORA_REGISTRATION_KEY_CONFLICT");
            }

            if (!plan.IsNew && !plan.Adapter.HasCompatibleConfiguration(canonical))
                throw new InvalidOperationException("LORA_ADAPTER_CONFIG_MISMATCH");
        }
    }

    private sealed record ApplicationPlan(
        int BlockIndex,
        string Projection,
        LlmModel.AdapterAttachmentState OriginalState,
        LoraLinear Adapter,
        bool IsNew);

    private sealed record SeededProvenancePlan(
        int BlockIndex,
        string Projection,
        LlmModel.AdapterAttachmentState OriginalState,
        LoraLinear Adapter,
        LoraAdapterRandomSeeds ExpectedSeeds);

    private sealed record SeededTripleLockTarget(
        LlmModel Model,
        int SemanticIndex,
        int IdentityHash);
}
