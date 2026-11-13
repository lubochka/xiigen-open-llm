using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using SmartSkillsDomain;

internal static class Program
{
    private const string PhaseId = "ROW14.4";
    private const string PassVerdict = "ROW14_4_HELDOUT_FRESH_LOAD_PASS";
    private const string FailVerdict = "ROW14_4_HELDOUT_FAIL";
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    private static readonly JsonSerializerOptions CompactJsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static async Task<int> Main(string[] args)
    {
        try
        {
            var repoRoot = FindRepoRoot();
            var evidenceDir = RequireEvidenceDir(args, repoRoot);
            Directory.CreateDirectory(evidenceDir);

            var root = Path.Combine(
                repoRoot,
                "docs",
                "plans",
                "evidence",
                "extension-full-readiness-dual-path",
                "scenario-execution",
                "frdp-p01-cursor-learnability-training-continue");
            var row14_3Dir = Path.Combine(root, "row14-3-staged-continue-training-export-20260626T103934Z");
            var stagedExportDir = Path.Combine(row14_3Dir, "staged-export");
            var stagedCheckpointPath = Path.Combine(stagedExportDir, "skill-selection-checkpoint.json");
            var stagedManifestPath = Path.Combine(stagedExportDir, "skill-selection-manifest.json");
            var row14_2SummaryPath = Path.Combine(
                root,
                "row14-2-dpo-rows-20260626T102909Z",
                "row14-2-dpo-summary.json");

            RequireFile(stagedCheckpointPath, "Row14.3 staged checkpoint");
            RequireFile(stagedManifestPath, "Row14.3 staged manifest");
            RequireFile(row14_2SummaryPath, "Row14.2 summary");

            var hashesBefore = HashStaged(stagedCheckpointPath, stagedManifestPath);
            var stagedManifest = ReadJson<SkillSelectionCheckpointManifest>(stagedManifestPath)
                ?? throw new InvalidOperationException("Row14.3 staged manifest deserialized null.");
            if (stagedManifest.SemanticQualityClaimed)
                throw new InvalidOperationException("Row14.3 staged manifest claims semantic quality.");

            var stagedExport = new LearningMachineExport(
                stagedManifest.MachineId,
                stagedManifest.DomainTag,
                stagedManifest.Version,
                stagedCheckpointPath,
                stagedManifestPath,
                stagedManifest.AccuracyAtExport,
                stagedManifest.ExportedAtUtc);

            var importedMachine = new SkillSelectionMachine();
            var freshImport = await importedMachine.ImportAsync(stagedExport);
            RequireSuccess(freshImport, "Fresh import of Row14.3 staged checkpoint/manifest failed.");

            var checkpointLoad = await SkillSelectionMachine.LoadCheckpointAsync(stagedCheckpointPath);
            RequireSuccess(checkpointLoad, "Fresh LoadCheckpointAsync of Row14.3 staged checkpoint failed.");

            var heldOut = ReadHeldOutProbe(row14_2SummaryPath);
            var request = new SkillSelectionRequest(
                Prompt: heldOut.RedactedProbeTaskText,
                ProjectRoot: repoRoot,
                AvailableSkillIds: heldOut.CandidateSkillIds,
                HostFacts: new Dictionary<string, string>
                {
                    ["phase"] = PhaseId,
                    ["splitRole"] = heldOut.SplitRole,
                    ["source"] = "row14.2-summary-held-out-probe",
                    ["freshLoad"] = "true",
                    ["heldOut"] = "true",
                    ["locatorUpdate"] = "false",
                    ["promotion"] = "false",
                    ["trainingPerformed"] = "false"
                },
                UserPromptSourceEntryId: heldOut.ProbeId);

            var decisionResult = importedMachine.DecideFromTrainedState(request);
            RequireSuccess(decisionResult, "Held-out selection failed.");
            var decision = decisionResult.Value!;
            var passed = SameSkillSet(decision.SelectedSkillIds, heldOut.ExpectedSelectedSkillIds);
            var hashesAfter = HashStaged(stagedCheckpointPath, stagedManifestPath);
            var stagedUnchanged = SameHashes(hashesBefore, hashesAfter);
            if (!stagedUnchanged)
                throw new InvalidOperationException("Row14.3 staged checkpoint/manifest hashes changed during eval.");

            var flags = new
            {
                trainingPerformed = false,
                exportPromotion = false,
                locatorUpdate = false,
                productionMutation = false,
                uiRun = false,
                readinessClaimed = false,
                checkpointMutation = false,
                tokenSavingsClaimed = false,
                fullSemanticQualityClaimed = false,
                trainingReadinessClaimed = false,
                fullExtensionReadinessClaimed = false,
                gitAction = false
            };
            var readBackPhrases = new[]
            {
                "fresh-load",
                "held-out",
                "client-adaptive-learning-loop",
                "trainingPerformed=false",
                "locator update not performed",
                "promotion not performed"
            };
            var verdict = passed ? PassVerdict : FailVerdict;
            var report = new
            {
                phase = PhaseId,
                verdict,
                generatedAtUtc = DateTimeOffset.UtcNow,
                scope = "eval-only held-out/fresh-load selection against Row14.3 staged export",
                sourceInputs = new
                {
                    row14_3StagedExport = Relative(repoRoot, stagedExportDir),
                    stagedCheckpoint = Relative(repoRoot, stagedCheckpointPath),
                    stagedManifest = Relative(repoRoot, stagedManifestPath),
                    row14_2Summary = Relative(repoRoot, row14_2SummaryPath)
                },
                freshLoad = new
                {
                    phrase = "fresh-load",
                    importMethod = "SkillSelectionMachine.ImportAsync",
                    checkpointLoadMethod = "SkillSelectionMachine.LoadCheckpointAsync",
                    manifestLoaded = freshImport.IsSuccess,
                    checkpointLoaded = checkpointLoad.IsSuccess,
                    trainedPairCount = checkpointLoad.Value!.Checkpoint.TrainedPairIds.Count,
                    machineId = stagedManifest.MachineId,
                    domainTag = stagedManifest.DomainTag,
                    version = stagedManifest.Version
                },
                heldOutProbe = new
                {
                    phrase = "held-out",
                    heldOut.ProbeId,
                    heldOut.SplitRole,
                    notCopiedIntoTrainingRows = heldOut.NotCopiedIntoTrainingRows,
                    redactedProbeTaskText = heldOut.RedactedProbeTaskText,
                    candidateSkillIds = heldOut.CandidateSkillIds,
                    expectedSelectedSkillIds = heldOut.ExpectedSelectedSkillIds,
                    expectedSkill = "client-adaptive-learning-loop",
                    leakageGuard = heldOut.LeakageGuard
                },
                eval = new
                {
                    evalOnly = true,
                    method = "SkillSelectionMachine.DecideFromTrainedState",
                    selectedSkillIds = decision.SelectedSkillIds,
                    expectedSelectedSkillIds = heldOut.ExpectedSelectedSkillIds,
                    expectedSkill = "client-adaptive-learning-loop",
                    pass = passed,
                    accuracy = passed ? 1.0 : 0.0,
                    confidence = decision.Confidence,
                    requiresSecondPass = decision.RequiresSecondPass,
                    evidenceFeatures = decision.EvidenceFeatures,
                    decisionText = decision.DecisionText
                },
                stagedHashes = new
                {
                    before = hashesBefore,
                    after = hashesAfter,
                    unchanged = stagedUnchanged
                },
                notPerformed = new
                {
                    training = "training not performed",
                    locatorUpdate = "locator update not performed",
                    promotion = "promotion not performed",
                    exportPromotion = "export promotion not performed",
                    ui = "UI not run",
                    productionMutation = "production/common/user locator not mutated"
                },
                flags,
                readBackPhrases,
                readBackLine = "fresh-load; held-out; client-adaptive-learning-loop; trainingPerformed=false; locator update not performed; promotion not performed",
                limitations = new[]
                {
                    "This is scoped held-out/fresh-load SkillSelection evidence only.",
                    "No training was performed.",
                    "No export promotion was performed.",
                    "No locator was updated.",
                    "No UI was run.",
                    "No product readiness, token savings, full semantic quality, training readiness, or full extension readiness is claimed."
                },
                nextSmallestWorkOrder = "Row14.4 review/packaging"
            };

            var reportPath = Path.Combine(evidenceDir, "row14-4-heldout-fresh-load-report.json");
            var resultJsonlPath = Path.Combine(evidenceDir, "row14-4-heldout-fresh-load-results.jsonl");
            var validationPath = Path.Combine(evidenceDir, "row14-4-heldout-fresh-load-validation.json");
            var markdownPath = Path.Combine(evidenceDir, "row14-4-heldout-fresh-load-report.md");
            WriteJson(reportPath, report);
            WriteText(resultJsonlPath, JsonSerializer.Serialize(new
            {
                row = "14.4",
                probeId = heldOut.ProbeId,
                selectedSkillIds = decision.SelectedSkillIds,
                expectedSelectedSkillIds = heldOut.ExpectedSelectedSkillIds,
                pass = passed,
                accuracy = passed ? 1.0 : 0.0,
                evidenceFeatures = decision.EvidenceFeatures,
                trainingPerformed = false,
                exportPromotion = false,
                locatorUpdate = false,
                productionMutation = false,
                uiRun = false,
                readinessClaimed = false
            }, CompactJsonOptions));
            WriteJson(validationPath, new
            {
                phase = PhaseId,
                verdict,
                reportParsedByRunner = true,
                checks = new
                {
                    freshImportSucceeded = freshImport.IsSuccess,
                    freshCheckpointLoadSucceeded = checkpointLoad.IsSuccess,
                    heldOutProbeReadFromRow14_2Summary = true,
                    expectedSkill = "client-adaptive-learning-loop",
                    selectedExpectedSkill = passed,
                    stagedCheckpointSha256Before = hashesBefore["staged_checkpoint_sha256"],
                    stagedCheckpointSha256After = hashesAfter["staged_checkpoint_sha256"],
                    stagedManifestSha256Before = hashesBefore["staged_manifest_sha256"],
                    stagedManifestSha256After = hashesAfter["staged_manifest_sha256"],
                    stagedUnchanged,
                    flags.trainingPerformed,
                    flags.exportPromotion,
                    flags.locatorUpdate,
                    flags.productionMutation,
                    flags.uiRun,
                    flags.readinessClaimed
                },
                readBackPhrases
            });
            WriteText(markdownPath, BuildMarkdown(report));

            Console.WriteLine(JsonSerializer.Serialize(new
            {
                verdict,
                reportPath,
                resultJsonlPath,
                validationPath,
                selectedSkillIds = decision.SelectedSkillIds,
                expectedSelectedSkillIds = heldOut.ExpectedSelectedSkillIds,
                pass = passed,
                stagedUnchanged
            }, CompactJsonOptions));
            return passed ? 0 : 2;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static HeldOutProbe ReadHeldOutProbe(string summaryPath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(summaryPath, Encoding.UTF8));
        var probe = document.RootElement.GetProperty("row14_3PlannedHeldOutProbe");
        var expected = RequiredStringArray(probe.GetProperty("expectedSelectedSkillIds"));
        if (!expected.Contains("client-adaptive-learning-loop", StringComparer.Ordinal))
            throw new InvalidOperationException("Held-out expected skill is not client-adaptive-learning-loop.");
        if (!probe.GetProperty("notCopiedIntoTrainingRows").GetBoolean())
            throw new InvalidOperationException("Held-out probe is not marked as excluded from training rows.");

        return new HeldOutProbe(
            ProbeId: RequiredString(probe, "probeId"),
            NotCopiedIntoTrainingRows: true,
            SplitRole: RequiredString(probe, "splitRole"),
            RedactedProbeTaskText: RequiredString(probe, "redactedProbeTaskText"),
            CandidateSkillIds: RequiredStringArray(probe.GetProperty("candidateSkillIds")),
            ExpectedSelectedSkillIds: expected,
            LeakageGuard: RequiredString(probe, "leakageGuard"));
    }

    private static IReadOnlyDictionary<string, string> HashStaged(string checkpointPath, string manifestPath)
        => new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["staged_checkpoint_sha256"] = Sha256File(checkpointPath),
            ["staged_manifest_sha256"] = Sha256File(manifestPath)
        };

    private static bool SameHashes(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after)
        => before.Count == after.Count
            && before.All(item =>
                after.TryGetValue(item.Key, out var value)
                && string.Equals(value, item.Value, StringComparison.Ordinal));

    private static bool SameSkillSet(IReadOnlyList<string> left, IReadOnlyList<string> right)
        => left.Count == right.Count
            && left.Order(StringComparer.Ordinal).SequenceEqual(right.Order(StringComparer.Ordinal), StringComparer.Ordinal);

    private static void RequireSuccess<T>(OperationResult<T> result, string message)
    {
        if (!result.IsSuccess)
            throw new InvalidOperationException($"{message} {result.ErrorCode}: {result.ErrorMessage}");
    }

    private static void RequireFile(string path, string label)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"{label} not found.", path);
    }

    private static T? ReadJson<T>(string path)
        => JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), JsonOptions);

    private static void WriteJson(string path, object value)
        => File.WriteAllText(path, NormalizeText(JsonSerializer.Serialize(value, JsonOptions)), Utf8NoBom);

    private static void WriteText(string path, string value)
        => File.WriteAllText(path, NormalizeText(value), Utf8NoBom);

    private static string BuildMarkdown(object report)
    {
        var json = JsonSerializer.Serialize(report, JsonOptions);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var fresh = root.GetProperty("freshLoad");
        var eval = root.GetProperty("eval");
        var hashes = root.GetProperty("stagedHashes");
        var flags = root.GetProperty("flags");
        return $"""
            # ROW14.4 held-out fresh-load eval

            Verdict: `{root.GetProperty("verdict").GetString()}`

            This report proves scoped `fresh-load`, `held-out`, eval-only selection evidence for `client-adaptive-learning-loop`.

            - Fresh import: `{fresh.GetProperty("manifestLoaded").GetBoolean()}`
            - Fresh checkpoint load: `{fresh.GetProperty("checkpointLoaded").GetBoolean()}`
            - Selected skills: `{string.Join(", ", eval.GetProperty("selectedSkillIds").EnumerateArray().Select(item => item.GetString()))}`
            - Expected skill: `client-adaptive-learning-loop`
            - Pass: `{eval.GetProperty("pass").GetBoolean()}`
            - Accuracy: `{eval.GetProperty("accuracy").GetDouble():F1}`
            - Staged checkpoint unchanged: `{hashes.GetProperty("unchanged").GetBoolean()}`
            - trainingPerformed=false
            - locator update not performed
            - promotion not performed
            - UI run: `{flags.GetProperty("uiRun").GetBoolean()}`
            - Readiness claimed: `{flags.GetProperty("readinessClaimed").GetBoolean()}`

            Next smallest work order: Row14.4 review/packaging.
            """;
    }

    private static string NormalizeText(string value)
    {
        var normalized = value
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);
        return string.Join('\n', normalized.Split('\n').Select(line => line.TrimEnd())).TrimEnd() + "\n";
    }

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string RequiredString(JsonElement element, string propertyName)
    {
        var value = element.GetProperty(propertyName).GetString();
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Required string property {propertyName} is missing.");
        return value;
    }

    private static string[] RequiredStringArray(JsonElement element)
        => element.EnumerateArray()
            .Select(item => item.GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item!)
            .ToArray();

    private static string RequireEvidenceDir(string[] args, string repoRoot)
    {
        var index = Array.IndexOf(args, "--evidence-dir");
        if (index < 0 || index + 1 >= args.Length)
            throw new InvalidOperationException("Usage: --evidence-dir <ROW14.4 evidence directory>");

        var evidenceDir = Path.GetFullPath(args[index + 1]);
        var allowedRoot = Path.GetFullPath(Path.Combine(
            repoRoot,
            "docs",
            "plans",
            "evidence",
            "extension-full-readiness-dual-path",
            "scenario-execution",
            "frdp-p01-cursor-learnability-training-continue"));
        if (!evidenceDir.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase)
            || Path.GetFileName(evidenceDir).StartsWith("row14-4-heldout-fresh-load-eval-", StringComparison.Ordinal) is false)
            throw new InvalidOperationException($"Evidence directory is outside Row14.4 allowed scope: {evidenceDir}");

        return evidenceDir;
    }

    private static string FindRepoRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (Directory.Exists(Path.Combine(current, "43.SmartSkillsDomain"))
                && Directory.Exists(Path.Combine(current, "docs")))
                return Path.GetFullPath(current);
            current = Directory.GetParent(current)?.FullName ?? "";
        }

        current = Directory.GetCurrentDirectory();
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (Directory.Exists(Path.Combine(current, "43.SmartSkillsDomain"))
                && Directory.Exists(Path.Combine(current, "docs")))
                return Path.GetFullPath(current);
            current = Directory.GetParent(current)?.FullName ?? "";
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

    private static string Relative(string repoRoot, string path)
        => Path.GetRelativePath(repoRoot, path).Replace(Path.DirectorySeparatorChar, '/');

    private sealed record HeldOutProbe(
        string ProbeId,
        bool NotCopiedIntoTrainingRows,
        string SplitRole,
        string RedactedProbeTaskText,
        IReadOnlyList<string> CandidateSkillIds,
        IReadOnlyList<string> ExpectedSelectedSkillIds,
        string LeakageGuard);
}
