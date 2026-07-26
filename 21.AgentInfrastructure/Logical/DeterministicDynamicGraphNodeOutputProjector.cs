// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

public interface IDynamicGraphNodeOutputProjector
{
    OperationResult<NodeOutputToEdgeInputAdapter> Project(
        string sourceNodeId,
        IReadOnlyList<GraphNodeCandidate> nodeOutputs);
}

// Projects materialized node outputs into the immutable L2-to-L3 edge-input boundary only.
public sealed class DeterministicDynamicGraphNodeOutputProjector
    : IDynamicGraphNodeOutputProjector
{
    private const string ProjectionRule =
        "GraphNodeCandidate.Id -> UpstreamCandidateRefs (order-free set)";

    private const string SourceRequiredCode = "DYNAMIC_GRAPH_NODE_OUTPUT_SOURCE_REQUIRED";
    private const string OutputsRequiredCode = "DYNAMIC_GRAPH_NODE_OUTPUTS_REQUIRED";
    private const string CandidateRequiredCode = "DYNAMIC_GRAPH_NODE_OUTPUT_CANDIDATE_REQUIRED";
    private const string IdentityInvalidCode = "DYNAMIC_GRAPH_NODE_OUTPUT_IDENTITY_INVALID";
    private const string DuplicateIdentityCode = "DYNAMIC_GRAPH_NODE_OUTPUT_IDENTITY_DUPLICATE";
    private const string HashFailedCode = "DYNAMIC_GRAPH_NODE_OUTPUT_HASH_FAILED";

    public OperationResult<NodeOutputToEdgeInputAdapter> Project(
        string sourceNodeId,
        IReadOnlyList<GraphNodeCandidate> nodeOutputs)
    {
        if (!IsCanonicalValue(sourceNodeId))
            return Failure("Dynamic graph source node identity is required.", SourceRequiredCode);

        if (nodeOutputs is null)
            return Failure("Dynamic graph node outputs are required.", OutputsRequiredCode);

        var outputCount = nodeOutputs.Count;
        if (outputCount == 0)
            return Failure("Dynamic graph node outputs are required.", OutputsRequiredCode);

        var candidateRefs = new string[outputCount];
        var distinctRefs = new HashSet<string>(StringComparer.Ordinal);

        for (var index = 0; index < outputCount; index++)
        {
            var candidate = nodeOutputs[index];
            if (candidate is null)
                return Failure("Dynamic graph node-output candidate is required.", CandidateRequiredCode);

            // CandidateId is the current canonical identity field represented by the existing
            // Plan-02 logical projection name; no route, evidence, or session identifier is a candidate ref.
            if (!IsCanonicalValue(candidate.CandidateId))
            {
                return Failure(
                    "Dynamic graph node-output candidate identity is invalid.",
                    IdentityInvalidCode);
            }

            if (!distinctRefs.Add(candidate.CandidateId))
            {
                return Failure(
                    "Dynamic graph node-output candidate identity is duplicated.",
                    DuplicateIdentityCode);
            }

            candidateRefs[index] = candidate.CandidateId;
        }

        Array.Sort(candidateRefs, StringComparer.Ordinal);

        if (!TryComputeOutputHash(candidateRefs, out var outputHash))
            return Failure("Dynamic graph node-output hash could not be computed.", HashFailedCode);

        return OperationResult<NodeOutputToEdgeInputAdapter>.Ok(
            new NodeOutputToEdgeInputAdapter(
                sourceNodeId,
                Array.AsReadOnly(candidateRefs),
                outputHash,
                ProjectionRule));
    }

    internal static bool TryComputeOutputHash(
        IReadOnlyList<string>? sortedCandidateRefs,
        out string outputHash)
    {
        outputHash = string.Empty;
        if (sortedCandidateRefs is null || sortedCandidateRefs.Count == 0)
            return false;

        for (var index = 0; index < sortedCandidateRefs.Count; index++)
        {
            if (!IsCanonicalValue(sortedCandidateRefs[index])
                || (index > 0
                    && StringComparer.Ordinal.Compare(
                        sortedCandidateRefs[index - 1],
                        sortedCandidateRefs[index]) >= 0))
            {
                return false;
            }
        }

        try
        {
            outputHash = ComputeOutputHashCore(sortedCandidateRefs);
            return true;
        }
        catch (Exception exception) when (IsNonFatalHashException(exception))
        {
            outputHash = string.Empty;
            return false;
        }
    }

    private static string ComputeOutputHashCore(IReadOnlyList<string> sortedCandidateRefs)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var strictUtf8 = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false,
            throwOnInvalidBytes: true);
        Span<byte> lengthPrefix = stackalloc byte[sizeof(int)];

        for (var index = 0; index < sortedCandidateRefs.Count; index++)
        {
            var encodedRef = strictUtf8.GetBytes(sortedCandidateRefs[index]);
            BinaryPrimitives.WriteInt32BigEndian(lengthPrefix, encodedRef.Length);
            hash.AppendData(lengthPrefix);
            hash.AppendData(encodedRef);
        }

        return $"sha256:{Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant()}";
    }

    private static bool IsCanonicalValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !StringComparer.Ordinal.Equals(value, value.Trim())
            || value.IndexOfAny(['\r', '\n', '\u0085', '\u2028', '\u2029']) >= 0)
        {
            return false;
        }

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsControl(character))
                return false;

            if (char.IsHighSurrogate(character))
            {
                if (index + 1 >= value.Length || !char.IsLowSurrogate(value[index + 1]))
                    return false;

                index++;
                continue;
            }

            if (char.IsLowSurrogate(character))
                return false;
        }

        return true;
    }

    private static bool IsNonFatalHashException(Exception exception)
        => exception is CryptographicException or EncoderFallbackException;

    private static OperationResult<NodeOutputToEdgeInputAdapter> Failure(
        string message,
        string code)
        => OperationResult<NodeOutputToEdgeInputAdapter>.Fail(message, code);
}
