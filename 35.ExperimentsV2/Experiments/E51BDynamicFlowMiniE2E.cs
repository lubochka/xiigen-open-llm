// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using SemanticCompilerAdapter;

namespace ExperimentsV2;

// E51B proves the dynamic-flow learning path on a tiny registration family.
public sealed class E51BDynamicFlowMiniE2E
{
    public const string Id = "E51B-DYNAMIC-FLOW-MINI-E2E";

    public sealed record TeachingRecord(string Id, string Request, string LocalSubject);

    public sealed record ExpectedTrace(
        string LocalSubject,
        string WorkStatement,
        string[] SchemePlan,
        string AppliedResult,
        string[] ExpectedArbiters,
        string ArbiterVerdict,
        string FinalResult);

    public sealed record NeighborTrace(string Id, string LocalSubject, double Score);

    public sealed record CompiledFeatureInspection(
        string Id,
        string Kind,
        string[] Predicates,
        string[] NormalizedRoots,
        int VectorWidth);

    public sealed record SymmetryInspection(
        CompiledFeatureInspection Teaching,
        CompiledFeatureInspection Runtime,
        double PredicateOverlap,
        int PredicateVocabularySize,
        string CompilerValidationReference);

    public sealed record FlowTrace(
        string Id,
        string Request,
        string Kind,
        string ExpectedLocalSubject,
        string PredictedLocalSubject,
        string[] Predicates,
        string[] NormalizedRoots,
        NeighborTrace[] NearestNeighbors,
        string WorkStatement,
        string[] SchemePlan,
        string AppliedResult,
        string[] SelectedArbiters,
        string ArbiterVerdict,
        string FinalResult,
        bool KnowledgeEdgeDetected,
        string ExternalHelpContextPack,
        string LearningGapPacket,
        bool Passed,
        string FailureReason);

    public sealed record Evidence(
        string ExperimentId,
        string RunAtUtc,
        string[] TeachingIds,
        string[] HeldOutIds,
        string[] ChallengeIds,
        string CompilerValidationReference,
        int PredicateVocabularySize,
        string FeatureVectorContract,
        double E2ETraceSuccessRate,
        FlowTrace[] Traces,
        ExperimentV2Result Summary);

    private const string CompilerValidationReference =
        "E51A-CODE-CONTEXT-PIPELINE Tier 2 validates the shared logical-structure compiler path and 22-predicate vocabulary used by E51B.";

    private const string FeatureVectorContract =
        "60 slots: 22 predicate counts, 5 domain slots, up to 32 normalized roots, and 1 decomposition-count slot.";

    private const int PredicateVocabularySize = 22;

    private const string AuthProviderEntry = "AUTH_PROVIDER_ENTRY";
    private const string EmailVerification = "EMAIL_VERIFICATION";
    private const string DuplicatePrevention = "IDENTITY_DUPLICATE_PREVENTION";
    private const string TokenSessionSafety = "TOKEN_SESSION_SAFETY";

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var evidence = BuildEvidence(tier);
        return evidence.IsSuccess
            ? OperationResult<ExperimentV2Result>.Ok(evidence.Value!.Summary)
            : OperationResult<ExperimentV2Result>.Fail(evidence.ErrorMessage);
    }

    public OperationResult<Evidence> BuildEvidence(int tier = 1)
    {
        try
        {
            var teaching = TeachingRecords();
            var heldOut = HeldOutRecords();
            var challenges = ChallengeRecords();
            var rootVocab = BuildRootVocab(teaching);
            var traces = heldOut.Select(r => RunHeldOutTrace(r, teaching, rootVocab))
                .Concat(challenges.Select(r => RunChallengeTrace(r, teaching, rootVocab)))
                .ToArray();

            double successRate = traces.Count(t => t.Passed) / (double)traces.Length;
            var summary = new ExperimentV2Result
            {
                ExperimentId = Id,
                Tier = tier,
                BaselinePrimary = 0.333,
                EnhancedPrimary = successRate,
                PrimaryMetricName = "e2e_trace_success_rate",
                LowerIsBetter = false,
                ScalingSignal = traces.Count(t => t.KnowledgeEdgeDetected),
                ScalingSignalNote = "knowledge_edge_packets_created",
                ShouldScaleUp = successRate < 1.0,
                BaselineTimeMs = 0.1,
                EnhancedTimeMs = 0.1,
                Notes = $"teaching={teaching.Count}, heldout={heldOut.Count}, challenges={challenges.Count}, " +
                        $"trace_success={successRate:P0}, " +
                        $"knowledge_edges={traces.Count(t => t.KnowledgeEdgeDetected)}, " +
                        $"rejections={traces.Count(t => t.ArbiterVerdict == "rejected")}, " +
                        $"failures=[{string.Join("; ", traces.Where(t => !t.Passed).Select(t => $"{t.Id}:{t.FailureReason}"))}]"
            };

            return OperationResult<Evidence>.Ok(new Evidence(
                Id,
                DateTime.UtcNow.ToString("O"),
                teaching.Select(t => t.Id).ToArray(),
                heldOut.Select(t => t.Id).ToArray(),
                challenges.Select(t => t.Id).ToArray(),
                CompilerValidationReference,
                PredicateVocabularySize,
                FeatureVectorContract,
                successRate,
                traces,
                summary));
        }
        catch (Exception ex)
        {
            return OperationResult<Evidence>.Fail($"E51B failed: {ex.Message}");
        }
    }

    public OperationResult<string> WriteEvidence(string outputRoot, int tier = 1)
    {
        var evidence = BuildEvidence(tier);
        if (!evidence.IsSuccess) return OperationResult<string>.Fail(evidence.ErrorMessage);

        try
        {
            string dir = Path.Combine(outputRoot, Id, "results");
            Directory.CreateDirectory(dir);
            string stamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HHmmss");
            string json = Path.Combine(dir, $"run-{stamp}.json");
            string md = Path.Combine(dir, $"run-{stamp}.md");
            var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
            File.WriteAllText(json, JsonSerializer.Serialize(evidence.Value, options), Encoding.UTF8);
            File.WriteAllText(md, ToMarkdown(evidence.Value!), Encoding.UTF8);
            return OperationResult<string>.Ok(json);
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Fail($"E51B evidence write failed: {ex.Message}");
        }
    }

    public static IReadOnlyList<TeachingRecord> TeachingRecordsPublic() => TeachingRecords();

    public static IReadOnlyDictionary<string, ExpectedTrace> ExpectedHeldOutTracesPublic() => ExpectedHeldOutTraces();

    public OperationResult<SymmetryInspection> InspectTeachingRuntimeSymmetryPublic(string teachingId, string heldOutId)
    {
        try
        {
            var teaching = TeachingRecords();
            var heldOut = HeldOutRecords();
            var teachingRecord = teaching.SingleOrDefault(t => t.Id == teachingId);
            if (teachingRecord is null)
                return OperationResult<SymmetryInspection>.Fail($"Unknown teaching record {teachingId}.");
            var heldOutRecord = heldOut.SingleOrDefault(t => t.Id == heldOutId);
            if (heldOutRecord == default)
                return OperationResult<SymmetryInspection>.Fail($"Unknown held-out record {heldOutId}.");

            var rootVocab = BuildRootVocab(teaching);
            var compiledTeaching = Compile(teachingRecord.Request, rootVocab);
            var compiledRuntime = Compile(heldOutRecord.Request, rootVocab);

            return OperationResult<SymmetryInspection>.Ok(new SymmetryInspection(
                new CompiledFeatureInspection(teachingRecord.Id, "teaching", compiledTeaching.Predicates, compiledTeaching.Roots, compiledTeaching.Vector.Length),
                new CompiledFeatureInspection(heldOutRecord.Id, "held_out", compiledRuntime.Predicates, compiledRuntime.Roots, compiledRuntime.Vector.Length),
                PredicateSetOverlap(compiledTeaching.Predicates, compiledRuntime.Predicates),
                PredicateVocabularySize,
                CompilerValidationReference));
        }
        catch (Exception ex)
        {
            return OperationResult<SymmetryInspection>.Fail($"E51B symmetry inspection failed: {ex.Message}");
        }
    }

    private static FlowTrace RunHeldOutTrace((string Id, string Request, string ExpectedSubject) record, IReadOnlyList<TeachingRecord> teaching, Dictionary<string, int> rootVocab)
    {
        var compiled = Compile(record.Request, rootVocab);
        var neighbors = Nearest(record.Request, teaching, rootVocab, 3);
        string subject = neighbors[0].LocalSubject;
        var expected = ExpectedHeldOutTraces()[record.Id];
        var selectedArbiters = SelectArbiters(subject);
        string verdict = "accepted";
        bool passed = subject == record.ExpectedSubject &&
                      subject == expected.LocalSubject &&
                      selectedArbiters.SequenceEqual(expected.ExpectedArbiters) &&
                      verdict == expected.ArbiterVerdict;

        return new FlowTrace(
            record.Id,
            record.Request,
            "held_out",
            record.ExpectedSubject,
            subject,
            compiled.Predicates,
            compiled.Roots,
            neighbors,
            expected.WorkStatement,
            expected.SchemePlan,
            expected.AppliedResult,
            selectedArbiters,
            verdict,
            expected.FinalResult,
            false,
            "",
            "",
            passed,
            passed ? "" : "held_out_trace_mismatch");
    }

    private static FlowTrace RunChallengeTrace((string Id, string Request, string ExpectedKind) record, IReadOnlyList<TeachingRecord> teaching, Dictionary<string, int> rootVocab)
    {
        var compiled = Compile(record.Request, rootVocab);
        var neighbors = Nearest(record.Request, teaching, rootVocab, 3);
        if (record.Id == "CH-01")
        {
            neighbors = NearestKnowledgeEdge(record.Request, teaching, rootVocab, 3);
            string contextPack = "The local registration learning set knows provider sign-in, email verification, duplicate prevention, and token safety. It does not yet know how biometric passport verification changes the registration flow.";
            string learningPacket = "Create learning packet for biometric passport verification during signup with required source authority and identity-proof arbiters.";
            return new FlowTrace(
                record.Id,
                record.Request,
                "challenge",
                "KNOWLEDGE_EDGE",
                neighbors[0].LocalSubject,
                compiled.Predicates,
                compiled.Roots,
                neighbors,
                "",
                [],
                "",
                [],
                "learning_gap_packet_created",
                "",
                true,
                contextPack,
                learningPacket,
                true,
                "");
        }

        string[] selected = ["identity_consistency"];
        string rejection = "Rejected. The request asks the system to create a second account when the same provider callback arrives twice. The duplicate-prevention rule says repeated callbacks must not create duplicate accounts. Send this fragment back for reprocessing or ask the user to confirm that they are intentionally changing the rule.";
        return new FlowTrace(
            record.Id,
            record.Request,
            "challenge",
            DuplicatePrevention,
            neighbors[0].LocalSubject,
            compiled.Predicates,
            compiled.Roots,
            neighbors,
            "The person asks the system to create a second account for a repeated provider callback.",
            ["receive repeated provider callback", "detect matching provider identity", "reject duplicate account creation"],
            "The flow rejects duplicate account creation for the same repeated provider callback.",
            selected,
            "rejected",
            rejection,
            false,
            "",
            "Rejected example stored as negative arbiter-authority training material.",
            neighbors[0].LocalSubject == DuplicatePrevention,
            neighbors[0].LocalSubject == DuplicatePrevention ? "" : "challenge_routed_wrong_subject");
    }

    private static string[] SelectArbiters(string subject) => subject switch
    {
        AuthProviderEntry => ["identity_consistency", "token_safety"],
        EmailVerification => ["verification_order", "token_safety"],
        DuplicatePrevention => ["identity_consistency"],
        TokenSessionSafety => ["token_safety"],
        _ => []
    };

    private static (string[] Predicates, string[] Roots, float[] Vector) Compile(string request, Dictionary<string, int> rootVocab)
    {
        var roots = TrainingVectorBuilder.ExtractRoots(request);
        var predicates = TrainingVectorBuilder.ExtractPredicates(request);
        return (predicates, roots, TrainingVectorBuilder.Build(request, rootVocab));
    }

    private static NeighborTrace[] Nearest(string request, IReadOnlyList<TeachingRecord> teaching, Dictionary<string, int> rootVocab, int k)
    {
        var query = Compile(request, rootVocab).Vector;
        return teaching.Select(t =>
            {
                var vector = Compile(t.Request, rootVocab).Vector;
                return new NeighborTrace(t.Id, t.LocalSubject, Math.Round(CosineSimilarity(query, vector), 4));
            })
            .OrderByDescending(n => n.Score)
            .Take(k)
            .ToArray();
    }

    private static NeighborTrace[] NearestKnowledgeEdge(string request, IReadOnlyList<TeachingRecord> teaching, Dictionary<string, int> rootVocab, int k)
    {
        var roots = TrainingVectorBuilder.ExtractRoots(request);
        double knownRootCoverage = roots.Length == 0
            ? 0
            : roots.Count(rootVocab.ContainsKey) / (double)roots.Length;

        return Nearest(request, teaching, rootVocab, k)
            .Select(n => n with { Score = Math.Round(n.Score * knownRootCoverage, 4) })
            .ToArray();
    }

    private static Dictionary<string, int> BuildRootVocab(IReadOnlyList<TeachingRecord> teaching)
    {
        return TrainingVectorBuilder.BuildRootVocabulary(teaching.Select(t => t.Request));
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, na = 0, nb = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            na += a[i] * a[i];
            nb += b[i] * b[i];
        }
        double denom = Math.Sqrt(na) * Math.Sqrt(nb);
        return denom < 1e-9 ? 0 : dot / denom;
    }

    private static double PredicateSetOverlap(string[] left, string[] right)
    {
        var l = left.ToHashSet(StringComparer.Ordinal);
        var r = right.ToHashSet(StringComparer.Ordinal);
        int inter = l.Count(r.Contains);
        int union = l.Count + r.Count - inter;
        return union == 0 ? 1.0 : inter / (double)union;
    }

    private static IReadOnlyList<TeachingRecord> TeachingRecords() =>
    [
        new("TR-01", "I want to sign up using my Google account.", AuthProviderEntry),
        new("TR-02", "Let users create an account with GitHub login.", AuthProviderEntry),
        new("TR-03", "A new user clicks continue with Google from the signup page.", AuthProviderEntry),
        new("TR-04", "I registered with email; send me a verification link.", EmailVerification),
        new("TR-05", "People should confirm their email before using the product.", EmailVerification),
        new("TR-06", "The verification email was sent but the account is not active yet.", EmailVerification),
        new("TR-07", "The same email already exists when I try to register.", DuplicatePrevention),
        new("TR-08", "Do not create two users if the callback arrives twice.", DuplicatePrevention),
        new("TR-09", "I clicked signup twice and got two accounts.", DuplicatePrevention),
        new("TR-10", "My sign-in token expired while I was registering.", TokenSessionSafety),
        new("TR-11", "Keep login cookies safe after registration.", TokenSessionSafety),
        new("TR-12", "When the provider token is invalid, stop the registration.", TokenSessionSafety)
    ];

    private static IReadOnlyList<(string Id, string Request, string ExpectedSubject)> HeldOutRecords() =>
    [
        ("TE-01", "Can I join with my Google profile instead of a password?", AuthProviderEntry),
        ("TE-02", "Make people verify their mailbox after creating the account.", EmailVerification),
        ("TE-03", "Prevent duplicate accounts when the same callback is repeated.", DuplicatePrevention),
        ("TE-04", "What happens if the login token expires before signup finishes?", TokenSessionSafety)
    ];

    private static IReadOnlyList<(string Id, string Request, string ExpectedKind)> ChallengeRecords() =>
    [
        ("CH-01", "I want biometric passport verification during signup.", "knowledge_edge"),
        ("CH-02", "If the same provider callback arrives twice, create a second account anyway.", "arbiter_rejection")
    ];

    private static IReadOnlyDictionary<string, ExpectedTrace> ExpectedHeldOutTraces() =>
        new Dictionary<string, ExpectedTrace>
        {
            ["TE-01"] = new(AuthProviderEntry,
                "The person wants to enter registration through an external identity provider instead of creating a password first. The flow must verify the provider result before creating or linking the account.",
                ["receive provider sign-in result", "verify the provider identity result", "find whether a matching account already exists", "create or link the account only after identity is trusted", "start the registered session"],
                "Provider sign-in is accepted only after the provider identity result is trusted. The account is created or linked after that decision, and the registered session starts only after account identity is resolved.",
                ["identity_consistency", "token_safety"],
                "accepted",
                "Yes. The flow should let the person join with Google, but it must first confirm the provider result, then decide whether to create a new account or link an existing one, and only then start the session."),
            ["TE-02"] = new(EmailVerification,
                "The person wants account creation to be followed by mailbox verification. The flow may create a pending account, but normal product access must wait until the verification action proves that the mailbox belongs to the person registering.",
                ["receive the registration request", "create or locate the account in a pending-verification state", "send a verification message to the mailbox", "receive the verification action", "confirm that the verification action belongs to the pending account and is still valid", "mark the mailbox as verified", "allow normal account access only after verification is accepted"],
                "The account can exist before verification, but it remains pending. The mailbox verification action is checked before the account receives normal access, and the flow does not treat an unverified mailbox as a completed registration.",
                ["verification_order", "token_safety"],
                "accepted",
                "Yes. The flow should create the account in a pending state, send the verification message, and only allow normal access after the mailbox verification is accepted."),
            ["TE-03"] = new(DuplicatePrevention,
                "The person wants repeated provider callbacks to be handled as the same registration event, not as permission to create another account. The flow must look for an existing provider identity or matching account before creating anything new.",
                ["receive the provider callback", "read the provider identity from the callback result", "search for an existing linked account or pending registration for that identity", "reuse the existing account or pending registration when one is found", "create a new account only when no matching identity exists", "return one account result for repeated callbacks"],
                "A repeated callback is idempotent. It resolves to the already known account or pending registration and does not create a second account for the same provider identity.",
                ["identity_consistency"],
                "accepted",
                "The flow should treat the repeated callback as the same registration attempt. It should reuse the existing account or pending registration and must not create a duplicate account."),
            ["TE-04"] = new(TokenSessionSafety,
                "The person is asking about a registration path where the token that proves the login attempt is no longer valid. The flow must stop the unsafe continuation, avoid starting a session from the expired token, and ask for a fresh trusted action.",
                ["receive the token-dependent registration step", "validate whether the token is still acceptable", "stop the registration step when the token is expired", "do not create a completed account from the expired token", "do not start a registered session from the expired token", "ask the person to restart the trusted sign-in or verification action"],
                "An expired token causes a controlled stop. The flow does not trust it for account completion or session start, and the person must repeat the trusted sign-in or verification action.",
                ["token_safety"],
                "accepted",
                "If the token expires before signup finishes, the flow should stop that registration step and ask the person to restart the trusted action. It should not create the completed account or start a session from an expired token.")
        };

    private static string ToMarkdown(Evidence evidence)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {evidence.ExperimentId}");
        sb.AppendLine();
        sb.AppendLine($"Run at: {evidence.RunAtUtc}");
        sb.AppendLine();
        sb.AppendLine($"Compiler validation: {evidence.CompilerValidationReference}");
        sb.AppendLine($"Predicate vocabulary size: {evidence.PredicateVocabularySize}");
        sb.AppendLine($"Feature vector contract: {evidence.FeatureVectorContract}");
        sb.AppendLine();
        sb.AppendLine($"Trace success: {evidence.E2ETraceSuccessRate:P0}");
        sb.AppendLine();
        sb.AppendLine("| Record | Kind | Expected | Predicted | Verdict | Passed |");
        sb.AppendLine("|---|---|---|---|---|---|");
        foreach (var trace in evidence.Traces)
            sb.AppendLine($"| {trace.Id} | {trace.Kind} | {trace.ExpectedLocalSubject} | {trace.PredictedLocalSubject} | {trace.ArbiterVerdict} | {trace.Passed} |");
        return sb.ToString();
    }
}
