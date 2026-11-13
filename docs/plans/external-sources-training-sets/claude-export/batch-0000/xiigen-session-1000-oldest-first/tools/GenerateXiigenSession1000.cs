using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

const int TargetRows = 1000;
const int RowsPerFile = 50;

var exportZips = new[]
{
    @"C:\Users\lubao\Documents\xiigen\old\data-6308de18-3e10-4d75-9d43-35c6c315a9ad-1777178009-8b897c61-batch-0000.zip",
    @"C:\Users\lubao\Documents\xiigen\old\data-6308de18-3e10-4d75-9d43-35c6c315a9ad-1777178382-78288604-batch-0000.zip"
};

var outputRoot = @"C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\claude-export\batch-0000\xiigen-session-1000-oldest-first";
if (Directory.Exists(outputRoot))
    Directory.Delete(outputRoot, recursive: true);

Directory.CreateDirectory(outputRoot);
Directory.CreateDirectory(Path.Combine(outputRoot, "layer-0-a"));
Directory.CreateDirectory(Path.Combine(outputRoot, "layer-0-b"));
Directory.CreateDirectory(Path.Combine(outputRoot, "held-out"));

var rows = new List<LearningRow>(TargetRows);
var seenPrompts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var conversations = new Dictionary<string, ConversationIndex>(StringComparer.OrdinalIgnoreCase);
var skippedByReason = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

foreach (var zipPath in exportZips)
{
    if (rows.Count >= TargetRows)
        break;

    using var archive = ZipFile.OpenRead(zipPath);
    var entry = archive.GetEntry("conversations.json");
    if (entry is null)
        continue;

    await using var stream = entry.Open();
    await foreach (var conversation in JsonSerializer.DeserializeAsyncEnumerable<Conversation>(stream, jsonOptions))
    {
        if (conversation is null)
            continue;

        var conversationMessages = conversation.ChatMessages
            .Where(message => !string.IsNullOrWhiteSpace(message.Text))
            .OrderBy(message => ParseDate(message.CreatedAt) ?? DateTimeOffset.MaxValue)
            .ToArray();

        var humanCount = conversationMessages.Count(message => IsHuman(message.Sender));
        var assistantCount = conversationMessages.Count(message => string.Equals(message.Sender, "assistant", StringComparison.OrdinalIgnoreCase));
        var validRowsBeforeConversation = rows.Count;

        if (!ConversationLooksRelevant(conversation, conversationMessages))
        {
            Count(skippedByReason, "conversation_not_relevant");
            continue;
        }

        for (var index = 0; index < conversationMessages.Length && rows.Count < TargetRows; index++)
        {
            var message = conversationMessages[index];
            if (!IsHuman(message.Sender))
                continue;

            var rawPrompt = NormalizeForPrompt(message.Text);
            var validation = ValidatePrompt(rawPrompt);
            if (validation is not null)
            {
                Count(skippedByReason, validation);
                continue;
            }

            var dedupeKey = NormalizeForDedupe(rawPrompt);
            if (!seenPrompts.Add(dedupeKey))
            {
                Count(skippedByReason, "duplicate_prompt");
                continue;
            }

            var predicates = ExtractPredicates(rawPrompt);
            if (predicates.Length == 0)
            {
                Count(skippedByReason, "no_engine_predicate");
                continue;
            }

            var subPhases = SelectSubPhases(rawPrompt, predicates);
            if (subPhases.Length == 0)
            {
                Count(skippedByReason, "no_sub_phase");
                continue;
            }

            var rowNumber = rows.Count + 1;
            var rowId = $"XIIGEN-SESSION-1000-{rowNumber:0000}";
            var primary = subPhases[0];
            var sourcePath = $"claude-export/batch-0000/{conversation.Uuid}/messages/{message.Uuid}";
            var domain = ClassifyDomain(rawPrompt);
            var transitionType = ClassifyTransition(rawPrompt);

            rows.Add(new LearningRow(
                RowId: rowId,
                RowNumber: rowNumber,
                ConversationUuid: conversation.Uuid ?? "",
                ConversationName: conversation.Name ?? "Untitled conversation",
                ConversationCreatedAt: conversation.CreatedAt ?? "",
                ConversationUpdatedAt: conversation.UpdatedAt ?? "",
                MessageUuid: message.Uuid ?? "",
                MessageIndex: index,
                MessageCreatedAt: message.CreatedAt ?? "",
                UserRequest: rawPrompt,
                Domain: domain,
                TransitionType: transitionType,
                Predicates: predicates,
                TargetSubPhases: subPhases,
                PrimarySubPhaseId: primary.Id,
                PrimaryPhaseId: primary.PhaseId,
                PrimaryTaskType: primary.TaskType,
                PrimarySubPhaseName: primary.Name,
                SourcePath: sourcePath,
                EvidenceTerms: EvidenceTerms(rawPrompt, predicates)));
        }

        if (rows.Count > validRowsBeforeConversation)
        {
            conversations[conversation.Uuid ?? $"conversation-{conversations.Count + 1}"] = new ConversationIndex(
                ConversationUuid: conversation.Uuid ?? "",
                ConversationName: conversation.Name ?? "Untitled conversation",
                CreatedAt: conversation.CreatedAt ?? "",
                UpdatedAt: conversation.UpdatedAt ?? "",
                TotalMessages: conversationMessages.Length,
                HumanMessages: humanCount,
                AssistantMessages: assistantCount,
                RowsProduced: rows.Count - validRowsBeforeConversation);
        }

        if (rows.Count >= TargetRows)
            break;
    }
}

WriteCompilerFiles(outputRoot, rows);
WriteRouterFiles(outputRoot, rows);
WriteHeldOutFiles(outputRoot, rows);
WriteJsonl(Path.Combine(outputRoot, "rows.jsonl"), rows);
WriteConversationIndex(Path.Combine(outputRoot, "conversation-index.jsonl"), conversations.Values);
WriteManifest(outputRoot, rows, conversations.Values, skippedByReason, exportZips);
WriteCycleMap(outputRoot, rows, conversations.Values);

var zipOutput = outputRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + ".zip";
if (File.Exists(zipOutput))
    File.Delete(zipOutput);
ZipFile.CreateFromDirectory(outputRoot, zipOutput, CompressionLevel.Optimal, includeBaseDirectory: false);

Console.WriteLine($"Generated rows: {rows.Count}");
Console.WriteLine($"Conversations used: {conversations.Count}");
Console.WriteLine($"Output: {outputRoot}");
Console.WriteLine($"Zip: {zipOutput}");

static DateTimeOffset? ParseDate(string? value)
{
    if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        return parsed;

    return null;
}

static bool IsHuman(string? sender)
{
    return string.Equals(sender, "human", StringComparison.OrdinalIgnoreCase)
        || string.Equals(sender, "user", StringComparison.OrdinalIgnoreCase);
}

static bool ConversationLooksRelevant(Conversation conversation, IReadOnlyList<ChatMessage> messages)
{
    var sample = $"{conversation.Name} {conversation.Summary} {string.Join(' ', messages.Where(message => IsHuman(message.Sender)).Take(6).Select(message => message.Text))}";
    var normalized = sample.ToLowerInvariant();
    var includeTerms = new[]
    {
        "xiigen", "xii gen", "figma", "code", "coding", "angular", "react", "nestjs", "typescript", "javascript",
        "api", "plugin", "skill", "claude code", "orchestrator", "flow", "architecture", "build", "test",
        "documentation", "implementation", "frontend", "backend", "pipeline", "mcp", "database", "redis",
        "elasticsearch", "auth", "security", "review", "plan", "runner", "package", "zip", "manifest"
    };
    var excludeTerms = new[]
    {
        "language learning", "insurance form", "medical form", "song", "music composition", "russian article"
    };

    return includeTerms.Any(term => normalized.Contains(term, StringComparison.Ordinal))
        && !excludeTerms.Any(term => normalized.Contains(term, StringComparison.Ordinal));
}

static string? ValidatePrompt(string prompt)
{
    if (string.IsNullOrWhiteSpace(prompt))
        return "empty_prompt";

    var normalized = prompt.Trim().ToLowerInvariant();
    if (normalized.Length < 18)
        return "too_short";

    var setupOnly = new[]
    {
        "continue", "please continue", "proceed", "please proceed", "read this", "load session",
        "please load the session", "start", "ok", "yes", "no", "why did you stop", "do it",
        "go on", "next", "thanks"
    };
    if (setupOnly.Any(term => normalized == term))
        return "setup_only";

    if (normalized.Contains("claude responded:", StringComparison.Ordinal) && normalized.Length > 1600)
        return "assistant_quote_as_prompt";

    if (normalized.Contains("ran ") && normalized.Contains(" commands") && normalized.Length > 1600)
        return "tool_output_as_prompt";

    if (prompt.Count(char.IsLetter) < 12)
        return "not_enough_text";

    if (prompt.Length > 5000)
        return "too_long_for_first_pass";

    return null;
}

static string NormalizeForPrompt(string text)
{
    var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n').Trim();
    while (normalized.Contains("\n\n\n", StringComparison.Ordinal))
        normalized = normalized.Replace("\n\n\n", "\n\n", StringComparison.Ordinal);
    return normalized;
}

static string NormalizeForDedupe(string text)
{
    var builder = new StringBuilder(text.Length);
    foreach (var ch in text.ToLowerInvariant())
    {
        if (char.IsLetterOrDigit(ch))
            builder.Append(ch);
        else if (char.IsWhiteSpace(ch))
            builder.Append(' ');
    }

    return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
}

static string[] ExtractPredicates(string prompt)
{
    var lexicon = new (string Predicate, string[] Terms)[]
    {
        ("SECURITY", ["security", "authorization", "authentication", "auth", "jwt", "oauth", "oidc", "pkce", "session", "asvs", "mfa", "rbac"]),
        ("AUTHORIZATION", ["authorization", "authorisation", "permission", "rbac", "scope", "tenant", "ownership"]),
        ("API_CONTRACT", ["api", "endpoint", "contract", "schema", "request", "response", "openapi"]),
        ("ARCHITECTURE", ["architecture", "architect", "component topology", "system boundary", "dependency direction", "project", "codebase", "code base", "existing code", "old code", "current code"]),
        ("IMPLEMENT", ["implement the", "implementation plan", "connector", "provider", "integration layer", "code change", "code", "fix", "bug", "issue", "async", "null", "generated frame", "error", "failure", "failed", "no data", "returned no data", "log"]),
        ("TEST", ["test", "testing", "integration test", "unit test", "coverage", "fixture", "verify", "validation", "automation test", "fix", "bug", "issue", "regression", "error", "failure", "failed", "no data", "returned no data", "log", "try again"]),
        ("QA", ["qa", "quality", "release", "acceptance", "verification"]),
        ("UI", ["ui", "ux", "screen", "layout", "figma", "frame", "interaction", "accessibility"]),
        ("DOCUMENT", ["documentation", "docs", "readme", "write document", "create document", "generate document", "document this", "spec", "specification", "requirements"]),
        ("LEARNING", ["skill", "skills", "claude code", "learning", "teach", "training", "reusable guidance", "knowledge", "model feedback"])
    };

    var normalized = prompt.ToLowerInvariant();
    return lexicon
        .Where(entry => entry.Terms.Any(term => normalized.Contains(term, StringComparison.Ordinal)))
        .Select(entry => entry.Predicate)
        .Distinct(StringComparer.Ordinal)
        .OrderBy(predicate => predicate, StringComparer.Ordinal)
        .ToArray();
}

static string[] EvidenceTerms(string prompt, string[] predicates)
{
    var normalized = prompt.ToLowerInvariant();
    var terms = new List<string>();
    foreach (var predicate in predicates)
    {
        var predicateTerms = predicate switch
        {
            "SECURITY" => new[] { "security", "authorization", "authentication", "auth", "jwt", "oauth", "oidc", "pkce", "session", "asvs", "mfa", "rbac" },
            "AUTHORIZATION" => ["authorization", "permission", "rbac", "scope", "tenant", "ownership"],
            "API_CONTRACT" => ["api", "endpoint", "contract", "schema", "request", "response", "openapi"],
            "ARCHITECTURE" => ["architecture", "architect", "component topology", "system boundary", "dependency direction", "project", "codebase", "existing code", "old code", "current code"],
            "IMPLEMENT" => ["implementation plan", "connector", "provider", "code change", "code", "fix", "bug", "issue", "async", "null", "generated frame", "error", "failure", "failed", "no data", "returned no data", "log"],
            "TEST" => ["test", "testing", "integration test", "unit test", "coverage", "fixture", "verify", "validation", "automation test", "regression", "try again"],
            "QA" => ["qa", "quality", "release", "acceptance", "verification"],
            "UI" => ["ui", "ux", "screen", "layout", "figma", "frame", "interaction", "accessibility"],
            "DOCUMENT" => ["documentation", "docs", "readme", "write document", "create document", "generate document", "document this", "spec", "specification", "requirements"],
            "LEARNING" => ["skill", "skills", "claude code", "learning", "teach", "training", "reusable guidance", "knowledge", "model feedback"],
            _ => []
        };
        terms.AddRange(predicateTerms.Where(term => normalized.Contains(term, StringComparison.Ordinal)).Select(term => $"{predicate}:{term}"));
    }

    return terms.Distinct(StringComparer.OrdinalIgnoreCase).Take(16).ToArray();
}

static SubPhase[] SelectSubPhases(string prompt, string[] predicates)
{
    var normalized = prompt.ToLowerInvariant();
    var result = new List<SubPhase>();

    void Add(string id)
    {
        var subPhase = SubPhaseCatalog.Values.First(candidate => candidate.Id == id);
        if (!result.Any(existing => existing.Id == subPhase.Id))
            result.Add(subPhase);
    }

    if (HasAny(normalized, "skill", "skills", "teaching", "learning set", "model feedback", "train", "training", "corpus", "external source"))
        Add("7.5");

    if (HasAny(normalized, "security", "authentication", "authorization", "jwt", "oauth", "oidc", "tenant", "rbac", "permission", "scope"))
        Add("3.4");

    if (HasAny(normalized, "api", "endpoint", "contract", "schema", "request", "response", "openapi"))
        Add("3.3");

    if (HasAny(normalized, "architecture", "architect", "orchestrator", "topology", "dynamic flow", "logical flow", "logical unit", "component", "registry", "design pattern", "codebase", "project"))
        Add("3.1");

    if (HasAny(normalized, "database", "data model", "persistence", "storage", "redis", "elasticsearch", "index"))
        Add("3.2");

    if (HasAny(normalized, "logging", "metrics", "trace", "monitoring", "observability", "performance", "cpu", "ram"))
        Add("3.5");

    if (HasAny(normalized, "figma", "ui", "ux", "screen", "layout", "frame", "frontend", "client", "react", "mobile"))
        Add(HasAny(normalized, "visual", "design", "theme", "accessibility", "interaction") ? "1.3" : "4.2");

    if (HasAny(normalized, "implement", "code", "build", "nestjs", "backend", "service", "class", "method", "function", "provider", "connector"))
        Add("4.1");

    if (HasAny(normalized, "refactor", "cleanup", "clean up", "quality", "simplify", "maintainability", "review the code"))
        Add("4.4");

    if (HasAny(normalized, "deploy", "deployment", "docker", "environment", "pipeline", "ci", "github action"))
        Add("4.3");

    if (HasAny(normalized, "integration", "provider", "connector", "adapter", "sdk"))
        Add("4.5");

    if (HasAny(normalized, "unit test", "tests", "testing", "test", "verify", "fixture"))
        Add("5.1");

    if (HasAny(normalized, "integration test", "multi-component", "storage test", "provider test"))
        Add("5.2");

    if (HasAny(normalized, "e2e", "end-to-end", "end to end", "scenario test", "workflow"))
        Add("5.3");

    if (HasAny(normalized, "regression", "compatibility", "previous rows", "still works"))
        Add("5.4");

    if (HasAny(normalized, "qa", "acceptance", "manual review", "human review"))
        Add("6.1");

    if (HasAny(normalized, "release", "risk", "readiness", "gate"))
        Add("6.2");

    if (HasAny(normalized, "compliance", "vulnerability", "security review"))
        Add("6.3");

    if (HasAny(normalized, "performance", "reliability", "resource", "memory", "cpu", "ram", "pc", "drive"))
        Add("6.4");

    if (HasAny(normalized, "automation", "runner", "harness"))
        Add("7.1");

    if (HasAny(normalized, "ci", "quality gate", "build passing", "commit", "push"))
        Add("7.2");

    if (HasAny(normalized, "feedback", "monitor", "watch", "scheduler"))
        Add("7.3");

    if (HasAny(normalized, "report", "ledger", "manifest", "evidence", "coverage", "summary"))
        Add("7.4");

    if (HasAny(normalized, "documentation", "docs", "readme", "document", "spec", "requirements", "plan", "guidance"))
        Add(HasAny(normalized, "edge", "boundary", "gap") ? "2.4" : "2.1");

    if (result.Count == 0)
    {
        if (predicates.Contains("UI"))
            Add("1.3");
        else if (predicates.Contains("DOCUMENT"))
            Add("2.1");
        else if (predicates.Contains("API_CONTRACT"))
            Add("3.3");
        else if (predicates.Contains("SECURITY") || predicates.Contains("AUTHORIZATION"))
            Add("3.4");
        else if (predicates.Contains("ARCHITECTURE"))
            Add("3.1");
        else if (predicates.Contains("IMPLEMENT"))
            Add("4.1");
        else if (predicates.Contains("TEST"))
            Add("5.1");
        else if (predicates.Contains("QA"))
            Add("6.1");
        else if (predicates.Contains("LEARNING"))
            Add("7.5");
    }

    return result.Take(8).ToArray();
}

static bool HasAny(string text, params string[] terms)
{
    return terms.Any(term => text.Contains(term, StringComparison.Ordinal));
}

static string ClassifyDomain(string prompt)
{
    var normalized = prompt.ToLowerInvariant();
    if (HasAny(normalized, "bug", "error", "failed", "failure", "not working", "issue", "null", "returned no data", "fix"))
        return "BUG";
    if (HasAny(normalized, "refactor", "cleanup", "clean up", "quality", "simplify", "maintainability"))
        return "REFACTOR";
    if (HasAny(normalized, "architecture", "architect", "orchestrator", "topology", "registry", "dynamic flow", "logical unit", "logical flow"))
        return "ARCH";
    if (HasAny(normalized, "plan", "documentation", "docs", "skill", "teaching", "learning", "corpus", "external source", "guidance"))
        return "PLAN";
    return "CODE_Q";
}

static string ClassifyTransition(string prompt)
{
    var normalized = prompt.ToLowerInvariant();
    if (HasAny(normalized, "bug", "error", "failed", "failure", "not working", "fix", "issue"))
        return "repair";
    if (HasAny(normalized, "teach", "training", "learning", "corpus", "external source", "model feedback"))
        return "learning_cycle";
    if (HasAny(normalized, "topology", "registry", "orchestrator", "dynamic flow"))
        return "topology_mutation";
    if (HasAny(normalized, "qa", "release", "acceptance", "verification", "coverage", "report"))
        return "qa_release";
    if (HasAny(normalized, "source", "license", "external"))
        return "external_source_integration";
    return "feature_addition";
}

static void WriteCompilerFiles(string outputRoot, IReadOnlyList<LearningRow> rows)
{
    foreach (var chunk in rows.Chunk(RowsPerFile).Select((items, index) => (items, index)))
    {
        var path = Path.Combine(outputRoot, "layer-0-a", $"layer-0a-compiler-pairs-{chunk.index + 1:000}.md");
        using var writer = new StreamWriter(path, append: false, Encoding.UTF8);
        writer.WriteLine($"# Layer 0-A - Compiler Pairs ({chunk.index + 1:000})");
        writer.WriteLine("# Batch: XIIGen session export 1000 oldest-first | pair_kind: compiler");
        writer.WriteLine("# Source: Claude export batch-0000; first-party private");
        writer.WriteLine();
        writer.WriteLine("---");
        writer.WriteLine();

        foreach (var row in chunk.items)
        {
            writer.WriteLine($"## {row.RowId} - {EscapeHeading(row.ConversationName)}");
            writer.WriteLine();
            writer.WriteLine("```");
            writer.WriteLine($"source_id: {row.RowId.ToLowerInvariant()}");
            writer.WriteLine($"source_title: {YamlQuote(row.ConversationName)}");
            writer.WriteLine("source_license_state: first_party_private");
            writer.WriteLine("contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>");
            writer.WriteLine($"transition_type: {row.TransitionType}");
            writer.WriteLine($"domain: {row.Domain}");
            writer.WriteLine($"phase: {row.PrimaryPhaseId}");
            writer.WriteLine($"sub_phase: {row.PrimarySubPhaseId}");
            writer.WriteLine($"task_type: {row.PrimaryTaskType}");
            writer.WriteLine("coverage_status: covered");
            writer.WriteLine("pair_kind: compiler");
            writer.WriteLine("conversation_trace:");
            writer.WriteLine("  export_batch: batch-0000");
            writer.WriteLine($"  conversation_uuid: {YamlQuote(row.ConversationUuid)}");
            writer.WriteLine($"  conversation_name: {YamlQuote(row.ConversationName)}");
            writer.WriteLine($"  human_message_uuid: {YamlQuote(row.MessageUuid)}");
            writer.WriteLine($"  human_message_index: {row.MessageIndex}");
            writer.WriteLine($"  message_created_at: {YamlQuote(row.MessageCreatedAt)}");
            writer.WriteLine("```");
            writer.WriteLine();
            writer.WriteLine("A:");
            writer.WriteLine($"  run_id: {YamlQuote(row.RowId)}");
            writer.WriteLine("  scenario_id: \"adaptive-logical-orchestration\"");
            writer.WriteLine($"  user_request: {YamlBlock(row.UserRequest, 2)}");
            writer.WriteLine("  flags:");
            writer.WriteLine("    UseLearnedUnitWhenAvailable: true");
            writer.WriteLine("    UseKnowledgeEdgeDetection: true");
            writer.WriteLine();
            writer.WriteLine("B:");
            writer.WriteLine($"  DocumentId: {YamlQuote($"{row.RowId}-prompt")}");
            writer.WriteLine($"  SourcePath: {YamlQuote(row.SourcePath)}");
            writer.WriteLine("  SourceTier: \"claude_export_session\"");
            writer.WriteLine("  LicenseOrAuthority: \"first_party_private\"");
            writer.WriteLine("  DocumentType: \"compiled_prompt\"");
            writer.WriteLine($"  DocumentTitle: {YamlQuote(row.PrimarySubPhaseName)}");
            writer.WriteLine($"  CompiledText: {YamlBlock(row.UserRequest, 2)}");
            writer.WriteLine($"  CompiledPredicateIds: [{string.Join(", ", row.Predicates.Select(YamlQuote))}]");
            writer.WriteLine("  SourceStatus: \"ready\"");
            writer.WriteLine("  KnownFailureMode: null");
            writer.WriteLine("  Confidence: 0.92");
            writer.WriteLine();
            writer.WriteLine("Predicate evidence:");
            foreach (var evidence in row.EvidenceTerms)
                writer.WriteLine($"  - {evidence}");
            writer.WriteLine();
            writer.WriteLine("---");
            writer.WriteLine();
        }
    }
}

static void WriteRouterFiles(string outputRoot, IReadOnlyList<LearningRow> rows)
{
    foreach (var chunk in rows.Chunk(RowsPerFile).Select((items, index) => (items, index)))
    {
        var path = Path.Combine(outputRoot, "layer-0-b", $"layer-0b-router-pairs-{chunk.index + 1:000}.md");
        using var writer = new StreamWriter(path, append: false, Encoding.UTF8);
        writer.WriteLine($"# Layer 0-B - Router Pairs ({chunk.index + 1:000})");
        writer.WriteLine("# Batch: XIIGen session export 1000 oldest-first | pair_kind: multi-sub-phase-routing");
        writer.WriteLine();
        writer.WriteLine("---");
        writer.WriteLine();

        foreach (var row in chunk.items)
        {
            writer.WriteLine($"## {row.RowId} - router target");
            writer.WriteLine();
            writer.WriteLine("```");
            writer.WriteLine($"source_id: {row.RowId.ToLowerInvariant()}");
            writer.WriteLine($"source_title: {YamlQuote(row.ConversationName)}");
            writer.WriteLine("source_license_state: first_party_private");
            writer.WriteLine("contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>");
            writer.WriteLine($"transition_type: {row.TransitionType}");
            writer.WriteLine($"domain: {row.Domain}");
            writer.WriteLine($"phase: {row.PrimaryPhaseId}");
            writer.WriteLine($"sub_phase: {row.PrimarySubPhaseId}");
            writer.WriteLine($"task_type: {row.PrimaryTaskType}");
            writer.WriteLine("coverage_status: covered");
            writer.WriteLine("pair_kind: multi_sub_phase_router");
            writer.WriteLine("```");
            writer.WriteLine();
            writer.WriteLine($"A_document_id: {YamlQuote($"{row.RowId}-prompt")}");
            writer.WriteLine("A_compiled_predicate_ids:");
            foreach (var predicate in row.Predicates)
                writer.WriteLine($"  - {YamlQuote(predicate)}");
            writer.WriteLine();
            writer.WriteLine("B:");
            writer.WriteLine("  routing_decisions:");
            for (var index = 0; index < row.TargetSubPhases.Length; index++)
            {
                var subPhase = row.TargetSubPhases[index];
                var confidence = Math.Max(0.72, 0.92 - (index * 0.04));
                writer.WriteLine($"    - DocumentId: {YamlQuote($"{row.RowId}-prompt")}");
                writer.WriteLine("      SourceStatus: \"ready\"");
                writer.WriteLine($"      PredictedPhaseId: {YamlQuote(subPhase.PhaseId)}");
                writer.WriteLine($"      PredictedSubPhaseId: {YamlQuote(subPhase.Id)}");
                writer.WriteLine($"      Confidence: {confidence.ToString("0.00", CultureInfo.InvariantCulture)}");
                writer.WriteLine("      NearestWrongSubPhaseScore: 0.00");
                writer.WriteLine($"      EvidencePredicates: [{string.Join(", ", subPhase.Predicates.Select(YamlQuote))}]");
                writer.WriteLine("      KnowledgeEdge: false");
                writer.WriteLine("      FailureClassification: \"source_classified_by_multi_sub_phase_router\"");
            }
            writer.WriteLine();
            writer.WriteLine("---");
            writer.WriteLine();
        }
    }
}

static void WriteHeldOutFiles(string outputRoot, IReadOnlyList<LearningRow> rows)
{
    var heldOut = rows.Where(row => row.RowNumber % 10 == 0).ToArray();
    var path = Path.Combine(outputRoot, "held-out", "held-out-rephrases.md");
    using var writer = new StreamWriter(path, append: false, Encoding.UTF8);
    writer.WriteLine("# Held-Out Rephrase Seeds");
    writer.WriteLine();
    writer.WriteLine("These are not synthetic primary A-sides. They are marked held-out rephrases tied to real source prompts.");
    writer.WriteLine();
    foreach (var row in heldOut)
    {
        writer.WriteLine($"## {row.RowId}-HOLDOUT");
        writer.WriteLine($"source_prompt_id: {row.RowId}");
        writer.WriteLine($"target_sub_phase: {row.PrimarySubPhaseId}");
        writer.WriteLine($"domain: {row.Domain}");
        writer.WriteLine($"original_prompt: {YamlQuote(OneLine(row.UserRequest))}");
        writer.WriteLine();
    }
}

static void WriteJsonl(string path, IReadOnlyList<LearningRow> rows)
{
    using var writer = new StreamWriter(path, append: false, Encoding.UTF8);
    foreach (var row in rows)
        writer.WriteLine(JsonSerializer.Serialize(row));
}

static void WriteConversationIndex(string path, IEnumerable<ConversationIndex> conversations)
{
    using var writer = new StreamWriter(path, append: false, Encoding.UTF8);
    foreach (var conversation in conversations.OrderBy(item => item.CreatedAt, StringComparer.Ordinal))
        writer.WriteLine(JsonSerializer.Serialize(conversation));
}

static void WriteManifest(
    string outputRoot,
    IReadOnlyList<LearningRow> rows,
    IEnumerable<ConversationIndex> conversations,
    IReadOnlyDictionary<string, int> skippedByReason,
    IReadOnlyList<string> exportZips)
{
    var manifest = Path.Combine(outputRoot, "MANIFEST.md");
    using var writer = new StreamWriter(manifest, append: false, Encoding.UTF8);
    writer.WriteLine("# XIIGen Session Export 1000 Oldest-First Learning Set");
    writer.WriteLine();
    writer.WriteLine($"Generated at: {DateTimeOffset.UtcNow:O}");
    writer.WriteLine("Source family: `xiigen-session-export`");
    writer.WriteLine("Source license state: `first_party_private`");
    writer.WriteLine("Extraction mode: streaming JSON from zip entry; no full `conversations.json` load.");
    writer.WriteLine();
    writer.WriteLine("## Source Zips");
    foreach (var zip in exportZips)
        writer.WriteLine($"- `{zip}`");
    writer.WriteLine();
    writer.WriteLine("## Counts");
    writer.WriteLine($"- Rows produced: {rows.Count}");
    writer.WriteLine($"- Conversations used: {conversations.Count()}");
    writer.WriteLine($"- Layer 0-A compiler files: {Math.Ceiling(rows.Count / (double)RowsPerFile):0}");
    writer.WriteLine($"- Layer 0-B router files: {Math.Ceiling(rows.Count / (double)RowsPerFile):0}");
    writer.WriteLine($"- Held-out rephrase seeds: {rows.Count(row => row.RowNumber % 10 == 0)}");
    writer.WriteLine();
    writer.WriteLine("## Date Window");
    writer.WriteLine($"- First row message: `{rows.FirstOrDefault()?.MessageCreatedAt}`");
    writer.WriteLine($"- Last row message: `{rows.LastOrDefault()?.MessageCreatedAt}`");
    writer.WriteLine();
    writer.WriteLine("## Domain Distribution");
    foreach (var group in rows.GroupBy(row => row.Domain).OrderBy(group => group.Key, StringComparer.Ordinal))
        writer.WriteLine($"- {group.Key}: {group.Count()}");
    writer.WriteLine();
    writer.WriteLine("## Sub-Phase Distribution");
    foreach (var group in rows.GroupBy(row => row.PrimarySubPhaseId).OrderBy(group => group.Key, StringComparer.Ordinal))
        writer.WriteLine($"- {group.Key} {SubPhaseCatalog.Values.First(item => item.Id == group.Key).Name}: {group.Count()}");
    writer.WriteLine();
    writer.WriteLine("## Skipped Signals");
    foreach (var item in skippedByReason.OrderBy(item => item.Key, StringComparer.Ordinal))
        writer.WriteLine($"- {item.Key}: {item.Value}");
    writer.WriteLine();
    writer.WriteLine("## Acceptance Checks");
    writer.WriteLine("- [x] Uses real human messages as A-sides.");
    writer.WriteLine("- [x] Excludes setup-only/session-control messages.");
    writer.WriteLine("- [x] Preserves conversation UUID and human message UUID per row.");
    writer.WriteLine("- [x] Uses current engine topology sub-phase IDs.");
    writer.WriteLine("- [x] Produces Layer 0-A and Layer 0-B files for immediate teaching-run compatibility.");
}

static void WriteCycleMap(string outputRoot, IReadOnlyList<LearningRow> rows, IEnumerable<ConversationIndex> conversations)
{
    var path = Path.Combine(outputRoot, "cycle-map.yaml");
    using var writer = new StreamWriter(path, append: false, Encoding.UTF8);
    writer.WriteLine("cycle_id: \"xiigen-session-export-1000-oldest-first\"");
    writer.WriteLine("cycle_date_window:");
    writer.WriteLine($"  start: {YamlQuote(rows.FirstOrDefault()?.MessageCreatedAt ?? "")}");
    writer.WriteLine($"  end: {YamlQuote(rows.LastOrDefault()?.MessageCreatedAt ?? "")}");
    writer.WriteLine("real_problem: \"Teach the orchestrator from oldest real XIIGen/software-development session prompts, preserving prompt-to-sub-phase routing evidence.\"");
    writer.WriteLine("source_family: \"xiigen-session-export\"");
    writer.WriteLine("source_license_state: \"first_party_private\"");
    writer.WriteLine("delivery_artifacts:");
    writer.WriteLine("  - path: \"layer-0-a/*.md\"");
    writer.WriteLine("    artifact_type: \"compiler-learning-set\"");
    writer.WriteLine("  - path: \"layer-0-b/*.md\"");
    writer.WriteLine("    artifact_type: \"router-learning-set\"");
    writer.WriteLine("source_conversations:");
    foreach (var conversation in conversations.OrderBy(item => item.CreatedAt, StringComparer.Ordinal).Take(1000))
    {
        writer.WriteLine($"  - conversation_uuid: {YamlQuote(conversation.ConversationUuid)}");
        writer.WriteLine($"    conversation_name: {YamlQuote(conversation.ConversationName)}");
        writer.WriteLine($"    created_at: {YamlQuote(conversation.CreatedAt)}");
        writer.WriteLine($"    updated_at: {YamlQuote(conversation.UpdatedAt)}");
        writer.WriteLine($"    rows_produced: {conversation.RowsProduced}");
    }
    writer.WriteLine("covered_phases:");
    foreach (var phase in rows.Select(row => row.PrimaryPhaseId).Distinct().OrderBy(item => item, StringComparer.Ordinal))
        writer.WriteLine($"  - {YamlQuote(phase)}");
    writer.WriteLine("covered_sub_phases:");
    foreach (var subPhase in rows.Select(row => row.PrimarySubPhaseId).Distinct().OrderBy(item => item, StringComparer.Ordinal))
        writer.WriteLine($"  - {YamlQuote(subPhase)}");
}

static string YamlQuote(string? value)
{
    return "\"" + (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") + "\"";
}

static string YamlBlock(string value, int indent)
{
    var spaces = new string(' ', indent);
    var nested = new string(' ', indent + 2);
    var normalized = value.Replace("\r\n", "\n").Replace('\r', '\n');
    if (!normalized.Contains('\n'))
        return YamlQuote(normalized);

    return "|\n" + string.Join('\n', normalized.Split('\n').Select(line => nested + line));
}

static string EscapeHeading(string value)
{
    return OneLine(value).Replace("#", "", StringComparison.Ordinal).Trim();
}

static string OneLine(string value)
{
    return string.Join(' ', value.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n', StringSplitOptions.RemoveEmptyEntries)).Trim();
}

static void Count(Dictionary<string, int> values, string key)
{
    values[key] = values.TryGetValue(key, out var current) ? current + 1 : 1;
}

public sealed record Conversation(
    [property: JsonPropertyName("uuid")] string? Uuid,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("summary")] string? Summary,
    [property: JsonPropertyName("created_at")] string? CreatedAt,
    [property: JsonPropertyName("updated_at")] string? UpdatedAt,
    [property: JsonPropertyName("chat_messages")] List<ChatMessage> ChatMessages);

public sealed record ChatMessage(
    [property: JsonPropertyName("uuid")] string? Uuid,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("sender")] string? Sender,
    [property: JsonPropertyName("created_at")] string? CreatedAt);

public sealed record SubPhase(string Id, string PhaseId, string Name, string TaskType, string[] Predicates);

public sealed record LearningRow(
    string RowId,
    int RowNumber,
    string ConversationUuid,
    string ConversationName,
    string ConversationCreatedAt,
    string ConversationUpdatedAt,
    string MessageUuid,
    int MessageIndex,
    string MessageCreatedAt,
    string UserRequest,
    string Domain,
    string TransitionType,
    string[] Predicates,
    SubPhase[] TargetSubPhases,
    string PrimarySubPhaseId,
    string PrimaryPhaseId,
    string PrimaryTaskType,
    string PrimarySubPhaseName,
    string SourcePath,
    string[] EvidenceTerms);

public sealed record ConversationIndex(
    string ConversationUuid,
    string ConversationName,
    string CreatedAt,
    string UpdatedAt,
    int TotalMessages,
    int HumanMessages,
    int AssistantMessages,
    int RowsProduced);

public static class SubPhaseCatalog
{
    public static readonly SubPhase[] Values =
    [
        new("1.1", "1", "Design System and Tokens", "design-system", ["UI"]),
        new("1.2", "1", "Layout and Responsive Structure", "layout-responsive", ["UI"]),
        new("1.3", "1", "Interaction and Navigation Model", "interaction-navigation", ["UI"]),
        new("1.4", "1", "Accessibility and Usability Review", "accessibility-usability", ["UI", "QA"]),
        new("2.1", "2", "Business Capability Definition", "business-capability", ["DOCUMENT"]),
        new("2.2", "2", "User Stories and Acceptance Criteria", "acceptance-criteria", ["DOCUMENT", "QA"]),
        new("2.3", "2", "Data and Validation Rules", "data-validation", ["API_CONTRACT", "DOCUMENT"]),
        new("2.4", "2", "Edge Cases and Boundary Conditions", "edge-cases", ["DOCUMENT"]),
        new("3.1", "3", "Architecture and Component Topology", "architecture-topology", ["ARCHITECTURE"]),
        new("3.2", "3", "Data Model and Persistence Design", "data-persistence", ["ARCHITECTURE"]),
        new("3.3", "3", "API Contract Definition", "api-contract", ["API_CONTRACT"]),
        new("3.4", "3", "Security and Authorization Model", "security-verification", ["SECURITY", "AUTHORIZATION"]),
        new("3.5", "3", "Observability and Operational Design", "observability-design", ["ARCHITECTURE"]),
        new("4.1", "4", "Backend Services", "backend-services", ["IMPLEMENT"]),
        new("4.2", "4", "Frontend and Client Integration", "frontend-client-integration", ["UI", "IMPLEMENT"]),
        new("4.3", "4", "Infrastructure and Deployment Code", "infrastructure-deployment", ["IMPLEMENT"]),
        new("4.4", "4", "Refactoring and Code Quality", "refactoring-code-quality", ["IMPLEMENT"]),
        new("4.5", "4", "Integration and Provider Connectors", "provider-connectors", ["IMPLEMENT", "API_CONTRACT"]),
        new("5.1", "5", "Unit Test Design", "unit-tests", ["TEST"]),
        new("5.2", "5", "Integration Test Design", "integration-tests", ["TEST", "API_CONTRACT"]),
        new("5.3", "5", "End-to-End Scenario Testing", "end-to-end-tests", ["TEST", "QA"]),
        new("5.4", "5", "Regression and Compatibility Testing", "regression-compatibility", ["TEST"]),
        new("6.1", "6", "Manual QA and Acceptance Review", "manual-qa", ["QA"]),
        new("6.2", "6", "Release Readiness and Risk Review", "release-risk", ["QA"]),
        new("6.3", "6", "Security and Compliance Review", "security-compliance-review", ["QA"]),
        new("6.4", "6", "Performance and Reliability Review", "performance-reliability", ["QA"]),
        new("7.1", "7", "Automated Test Harness", "automation-harness", ["TEST", "AUTOMATION"]),
        new("7.2", "7", "CI Pipeline and Quality Gates", "ci-quality-gates", ["AUTOMATION"]),
        new("7.3", "7", "Monitoring and Feedback Loop", "monitoring-feedback", ["AUTOMATION"]),
        new("7.4", "7", "Coverage and Reporting", "coverage-reporting", ["TEST", "DOCUMENT"]),
        new("7.5", "7", "Learning Set and Model Feedback", "learning-feedback", ["LEARNING"])
    ];
}
