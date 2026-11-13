// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OssEquivalents;

public static class ExternalAiStateGuards
{
    public const string ExaminerConfidenceKey = "examiner_confidence";
    public const string MissingCompetenceKey = "missing_competence";
    public const string ApprovalRequestedKey = "external_ai_approval_requested";

    public static bool ShouldRequestExternalAiApproval(Dictionary<string, object> state, double confidenceThreshold = 0.50)
    {
        if (!state.TryGetValue(MissingCompetenceKey, out var missingObj) ||
            missingObj is not string missing ||
            string.IsNullOrWhiteSpace(missing))
            return false;

        if (state.TryGetValue(ApprovalRequestedKey, out var requestedObj) &&
            requestedObj is bool alreadyRequested &&
            alreadyRequested)
            return false;

        double confidence = 1.0;
        if (state.TryGetValue(ExaminerConfidenceKey, out var confidenceObj))
            confidence = confidenceObj switch
            {
                double d => d,
                float f => f,
                decimal m => (double)m,
                int i => i,
                _ => confidence
            };

        return confidence < confidenceThreshold;
    }
}
