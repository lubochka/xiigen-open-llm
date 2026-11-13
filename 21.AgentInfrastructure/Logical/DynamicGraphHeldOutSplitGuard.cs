// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

/// <summary>
/// Admits only canonical, source-disjoint DynamicGraph held-out split evidence.
/// It validates evidence already computed by a caller; it does not calculate hashes,
/// near-duplicate scores, accuracy, training state, or promotion decisions.
/// </summary>
public static class DynamicGraphHeldOutSplitGuard
{
    private const string InvalidSplitProofCode = "INVALID_SPLIT_PROOF";
    private const string InvalidSplitProofMessage = "Dynamic graph measurement split proof is invalid.";
    private const string TrainHeldOutOverlapCode = "TRAIN_HELDOUT_OVERLAP";
    private const string TrainHeldOutOverlapMessage = "Training and held-out source roots overlap.";
    private const string TrainHeldOutNearDuplicateCode = "TRAIN_HELDOUT_NEAR_DUPLICATE";
    private const string TrainHeldOutNearDuplicateMessage = "Training and held-out sources contain near-duplicates.";

    /// <summary>
    /// Snapshots every caller-owned collection before validation and returns an independently
    /// owned proof only when all structural and contamination checks pass.
    /// </summary>
    public static OperationResult<DynamicGraphMeasurementSplitProof> Validate(
        DynamicGraphMeasurementSplitProof? proof)
    {
        if (proof is null)
            return InvalidSplitProof();

        IReadOnlyList<string>? trainSourceRoots;
        IReadOnlyList<string>? evalSourceRoots;
        IReadOnlyList<string>? sourceHashes;
        try
        {
            // Snapshot all collection carriers before validating or consuming any of them.
            trainSourceRoots = Snapshot(proof.TrainSourceRoots);
            evalSourceRoots = Snapshot(proof.EvalSourceRoots);
            sourceHashes = Snapshot(proof.SourceHashes);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidSplitProof();
        }

        if (!IsCanonicalUniqueCollection(trainSourceRoots)
            || !IsCanonicalUniqueCollection(evalSourceRoots)
            || !IsCanonicalUniqueCollection(sourceHashes)
            || proof.CrossSplitNearDuplicateCount < 0
            || !IsCanonicalMethod(proof.SimhashOrMinhash)
            || !double.IsFinite(proof.NearDuplicateThreshold)
            || proof.NearDuplicateThreshold < 0d)
        {
            return InvalidSplitProof();
        }

        var evalRootSet = new HashSet<string>(evalSourceRoots!, StringComparer.Ordinal);
        if (trainSourceRoots!.Any(evalRootSet.Contains))
        {
            // Exact source overlap has precedence over a reported near-duplicate count.
            return OperationResult<DynamicGraphMeasurementSplitProof>.Fail(
                TrainHeldOutOverlapMessage,
                TrainHeldOutOverlapCode);
        }

        if (proof.CrossSplitNearDuplicateCount > 0)
        {
            return OperationResult<DynamicGraphMeasurementSplitProof>.Fail(
                TrainHeldOutNearDuplicateMessage,
                TrainHeldOutNearDuplicateCode);
        }

        var ownedProof = new DynamicGraphMeasurementSplitProof(
            trainSourceRoots!,
            evalSourceRoots!,
            sourceHashes!,
            proof.CrossSplitNearDuplicateCount,
            proof.SimhashOrMinhash,
            proof.NearDuplicateThreshold);
        return OperationResult<DynamicGraphMeasurementSplitProof>.Ok(ownedProof);
    }

    private static IReadOnlyList<string>? Snapshot(IReadOnlyList<string>? source)
    {
        if (source is null)
            return null;

        var values = source.ToArray();
        return Array.AsReadOnly(values);
    }

    private static bool IsCanonicalUniqueCollection(IReadOnlyList<string>? values)
    {
        if (values is null || values.Count == 0)
            return false;

        var unique = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in values)
        {
            if (!IsCanonicalValue(value) || !unique.Add(value))
                return false;
        }

        return true;
    }

    private static bool IsCanonicalMethod(string? method)
        => StringComparer.Ordinal.Equals(method, "simhash")
           || StringComparer.Ordinal.Equals(method, "minhash");

    private static bool IsCanonicalValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && string.Equals(value, value.Trim(), StringComparison.Ordinal)
           && value.All(character =>
               !char.IsControl(character)
               && character is not '\r'
               && character is not '\n'
               && character is not '\u0085'
               && character is not '\u2028'
               && character is not '\u2029');

    private static OperationResult<DynamicGraphMeasurementSplitProof> InvalidSplitProof()
        => OperationResult<DynamicGraphMeasurementSplitProof>.Fail(
            InvalidSplitProofMessage,
            InvalidSplitProofCode);
}
