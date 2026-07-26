// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using SmartSkillsDomain;

[Collection("ProcessSpawningTests")]
public sealed class ExtensionQuality03ProofContractRunnerTests
{
    private static readonly TimeSpan ProcessTimeout = TimeSpan.FromSeconds(120);

    private static readonly string[] RequiredEvidenceFiles =
    [
        "row-specific-held-out-split.jsonl",
        "row-specific-source-disjoint-check.jsonl",
        "row-specific-nearest-neighbour-report.jsonl",
        "row-specific-ablation-report.jsonl",
        "row-specific-no-leakage-report.jsonl",
        "row-specific-no-memorization-report.jsonl",
        "row-specific-inter-rater-adjudication.jsonl",
        "per-row-blocking-proof-refs.jsonl"
    ];

    private static readonly string[] RequiredControls =
    [
        "exact_prompt_lookup_ablation",
        "filename_or_bm25_route_ablation",
        "candidate_position_order_ablation",
        "nearest_only_retrieval_only_ablation"
    ];

    private static readonly CategoryFixture[] Categories =
    [
        new("held_out_split", "approved_held_out_source_corpus_manifest_ref"),
        new("source_disjoint", "row_specific_source_disjoint_pass_ref"),
        new("nearest_neighbour_similarity_risk", "row_specific_nearest_neighbour_pass_ref"),
        new("ablation", "row_specific_ablation_runner_pass_ref"),
        new("no_leakage", "row_specific_no_leakage_pass_ref"),
        new("no_memorization", "row_specific_no_memorization_pass_ref"),
        new("inter_rater_adjudication", "independent_second_rater_adjudication_pass_ref")
    ];

    [Fact]
    public void Run_MissingApprovedCorpusRejectsHeldOutAndSourceDisjointPass()
    {
        using var workspace = TempWorkspace();
        var fixture = CreateFixture(workspace, new FixtureOptions
        {
            SourceDisjointRowsPass = true
        });

        var result = Run(fixture);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;
        Assert.Equal(0, CountPass(report, "held_out_split"));
        Assert.Equal(0, CountPass(report, "source_disjoint"));
        Assert.False(report.FullProofPass);
        Assert.False(File.Exists(Path.Combine(fixture.OutputDirectory, "runner-ready-eval-inputs.jsonl")));
    }

    [Fact]
    public void Run_ApprovedSplitAndSourceDisjointHappyPathPassesThoseEighteenRefs()
    {
        using var workspace = TempWorkspace();
        var fixture = CreateFixture(workspace, new FixtureOptions
        {
            ApprovedCorpus = true,
            SourceDisjointRowsPass = true
        });

        var result = Run(fixture);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;
        Assert.Equal(9, CountPass(report, "held_out_split"));
        Assert.Equal(9, CountPass(report, "source_disjoint"));
        Assert.Equal(18, report.BlockingProofRefsPass);
        Assert.Equal(0, report.RunnerReadyRows);
        Assert.Equal(0, report.QualityPassRows);
    }

    [Fact]
    public void Run_NearestNeighbourRowsCannotPassWithoutApprovedIndexManifest()
    {
        using var workspace = TempWorkspace();
        var fixture = CreateFixture(workspace, new FixtureOptions
        {
            ApprovedCorpus = true,
            SourceDisjointRowsPass = true,
            NearestNeighbourRowsPass = true
        });

        var result = Run(fixture);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;
        Assert.Equal(0, CountPass(report, "nearest_neighbour_similarity_risk"));
        Assert.All(CategoryDecisions(report, "nearest_neighbour_similarity_risk"), decision =>
            Assert.Contains("MISSING_APPROVED_NEAREST_NEIGHBOUR_INDEX_MANIFEST", decision.RiskCodes));
    }

    [Fact]
    public void Run_DeterministicAblationRejectsShortcutOnlyAndNotRunEvidence()
    {
        using var notRunWorkspace = TempWorkspace();
        var notRunFixture = CreateFixture(notRunWorkspace, new FixtureOptions
        {
            ApprovedCorpus = true,
            AblationRowsPass = true
        });

        var notRun = Run(notRunFixture).Value!;

        Assert.Equal(0, CountPass(notRun, "ablation"));
        Assert.Contains(CategoryDecisions(notRun, "ablation"), decision =>
            decision.RiskCodes.Contains("ABLATION_RUNNER_NOT_RUN"));

        using var shortcutWorkspace = TempWorkspace();
        var shortcutFixture = CreateFixture(shortcutWorkspace, new FixtureOptions
        {
            ApprovedCorpus = true,
            AblationRowsPass = true,
            AblationRunnerExecuted = true,
            AblationRunnerContractAvailable = true,
            AblationShortcutOnly = true
        });

        var shortcut = Run(shortcutFixture).Value!;

        Assert.Equal(0, CountPass(shortcut, "ablation"));
        Assert.Contains(CategoryDecisions(shortcut, "ablation"), decision =>
            decision.RiskCodes.Contains("ABLATION_SHORTCUT_ONLY"));
    }

    [Fact]
    public void Run_ReplayProbesStayRiskWithoutApprovedRawPrivateRefs()
    {
        using var workspace = TempWorkspace();
        var fixture = CreateFixture(workspace, new FixtureOptions
        {
            ApprovedCorpus = true,
            NoLeakageRowsPass = true,
            NoLeakageReplayExecuted = true,
            NoMemorizationRowsPass = true,
            NoMemorizationReplayExecuted = true
        });

        var result = Run(fixture);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;
        Assert.Equal(0, CountPass(report, "no_leakage"));
        Assert.Equal(0, CountPass(report, "no_memorization"));
        Assert.All(CategoryDecisions(report, "no_leakage"), decision =>
            Assert.Contains("NO_LEAKAGE_APPROVED_RAW_PRIVATE_REFS_MISSING", decision.RiskCodes));
        Assert.All(CategoryDecisions(report, "no_memorization"), decision =>
            Assert.Contains("NO_MEMORIZATION_APPROVED_RAW_PRIVATE_REFS_MISSING", decision.RiskCodes));
    }

    [Fact]
    public void Run_IndependentSecondRaterRejectsDeterministicSelfChecker()
    {
        using var workspace = TempWorkspace();
        var fixture = CreateFixture(workspace, new FixtureOptions
        {
            SecondRaterRowsPass = true,
            SecondRaterAvailable = true,
            SecondRaterSelfChecker = true
        });

        var result = Run(fixture);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;
        Assert.Equal(0, CountPass(report, "inter_rater_adjudication"));
        Assert.All(CategoryDecisions(report, "inter_rater_adjudication"), decision =>
            Assert.Contains("DETERMINISTIC_SELF_CHECKER_NOT_INDEPENDENT", decision.RiskCodes));
    }

    [Fact]
    public void Run_SecondRaterPacketArgumentResolvesOnlyInterRaterCategory()
    {
        using var workspace = TempWorkspace();
        var fixture = CreateFixture(workspace, new FixtureOptions());
        var packetDirectory = CreateAuthorizedSecondRaterPacket(workspace);

        var result = Run(fixture, packetDirectory);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;
        Assert.Equal(9, CountPass(report, "inter_rater_adjudication"));
        Assert.Equal(0, CountPass(report, "held_out_split"));
        Assert.Equal(0, CountPass(report, "source_disjoint"));
        Assert.Equal(0, CountPass(report, "nearest_neighbour_similarity_risk"));
        Assert.Equal(0, CountPass(report, "ablation"));
        Assert.Equal(0, CountPass(report, "no_leakage"));
        Assert.Equal(0, CountPass(report, "no_memorization"));
        Assert.Equal(9, report.BlockingProofRefsPass);
        Assert.Equal(0, report.RunnerReadyRows);
        Assert.Equal(0, report.QualityPassRows);
        Assert.Equal(NormalizedAuthorizedPacketPath(packetDirectory), report.SecondRaterPacketPath);
        Assert.Contains("QUALITY-03-MISSING-NON-INTER-RATER-PROOF-PRODUCERS-09L", report.NextBoundedWorkOrder, StringComparison.Ordinal);

        var cliOutput = Path.Combine(workspace, "cli-output");
        Directory.CreateDirectory(cliOutput);
        var cli = RunCli(fixture, packetDirectory, cliOutput);

        AssertCliSuccess(cli);
        Assert.Contains("blocking_proof_refs_pass=9/63", cli.StdOut, StringComparison.Ordinal);
        var cliReport = JsonNode.Parse(File.ReadAllText(Path.Combine(cliOutput, "quality-03-proof-contract-runner-report.json"), Encoding.UTF8))!.AsObject();
        Assert.Equal(9, cliReport["blocking_proof_refs_pass"]!.GetValue<int>());
        Assert.Equal(0, cliReport["runner_ready_rows"]!.GetValue<int>());
        Assert.Equal(0, cliReport["quality_pass_rows"]!.GetValue<int>());
        Assert.Equal(NormalizedAuthorizedPacketPath(packetDirectory), cliReport["second_rater_packet_path"]!.GetValue<string>());
    }

    [Fact]
    public void Run_MalformedMismatchedOrFakedSecondRaterPacketDoesNotPass()
    {
        using var malformedWorkspace = TempWorkspace();
        var malformedFixture = CreateFixture(malformedWorkspace, FixtureOptions.FullPass());
        var malformedPacket = CreateMalformedSecondRaterPacket(malformedWorkspace);
        var malformed = Run(malformedFixture, malformedPacket).Value!;

        Assert.Equal(0, CountPass(malformed, "inter_rater_adjudication"));
        Assert.Equal(54, malformed.BlockingProofRefsPass);
        Assert.False(malformed.FullProofPass);

        using var mismatchedWorkspace = TempWorkspace();
        var mismatchedFixture = CreateFixture(mismatchedWorkspace, FixtureOptions.FullPass());
        var mismatchedPacket = CreateAuthorizedSecondRaterPacket(mismatchedWorkspace, (row, index) =>
        {
            if (index == 1)
                row["row_id"] = "QUALITY-03-REVIEW-ROW-MISMATCH";
        });
        var mismatched = Run(mismatchedFixture, mismatchedPacket).Value!;

        Assert.Equal(0, CountPass(mismatched, "inter_rater_adjudication"));
        Assert.Equal(54, mismatched.BlockingProofRefsPass);
        Assert.Contains(CategoryDecisions(mismatched, "inter_rater_adjudication"), decision =>
            decision.RiskCodes.Contains("AUTHORIZED_SECOND_RATER_PACKET_ROW_SET_MISMATCH"));

        using var fakedWorkspace = TempWorkspace();
        var fakedFixture = CreateFixture(fakedWorkspace, FixtureOptions.FullPass());
        var fakedPacket = CreateAuthorizedSecondRaterPacket(fakedWorkspace, (row, _) => row["faked_independence"] = true);
        var faked = Run(fakedFixture, fakedPacket).Value!;

        Assert.Equal(0, CountPass(faked, "inter_rater_adjudication"));
        Assert.Equal(54, faked.BlockingProofRefsPass);
        Assert.All(CategoryDecisions(faked, "inter_rater_adjudication"), decision =>
            Assert.Contains("DETERMINISTIC_SELF_CHECKER_NOT_INDEPENDENT", decision.RiskCodes));
    }

    [Fact]
    public void Run_SecondRaterPacketCannotCreateRunnerReadyUnlessAllSixtyThreeProofRefsPass()
    {
        using var workspace = TempWorkspace();
        var fixture = CreateFixture(workspace, new FixtureOptions());
        var packetDirectory = CreateAuthorizedSecondRaterPacket(workspace);

        var report = Run(fixture, packetDirectory).Value!;

        Assert.False(report.FullProofPass);
        Assert.Equal(9, report.BlockingProofRefsPass);
        Assert.Equal(0, report.RunnerReadyRows);
        Assert.Equal(0, report.QualityPassRows);
        Assert.False(report.RunnerReadyEvalInputsCreated);
        Assert.False(File.Exists(Path.Combine(fixture.OutputDirectory, "runner-ready-eval-inputs.jsonl")));
        Assert.All(report.RowDecisions, row => Assert.False(row.RunnerScorerRunAllowed));
    }

    [Fact]
    public void Run_SecondRaterPacketRejectsExternalModelTrainingOrMutationFlags()
    {
        using var externalWorkspace = TempWorkspace();
        var externalFixture = CreateFixture(externalWorkspace, FixtureOptions.FullPass());
        var externalPacket = CreateAuthorizedSecondRaterPacket(externalWorkspace, (row, _) => row["external_ai_model_called"] = true);
        var external = Run(externalFixture, externalPacket).Value!;

        Assert.Equal(0, CountPass(external, "inter_rater_adjudication"));
        Assert.Equal(54, external.BlockingProofRefsPass);
        Assert.True(external.ExternalAiModelCalled);
        Assert.All(CategoryDecisions(external, "inter_rater_adjudication"), decision =>
            Assert.Contains("EXTERNAL_AI_MODEL_CALLED", decision.RiskCodes));

        using var trainingWorkspace = TempWorkspace();
        var trainingFixture = CreateFixture(trainingWorkspace, FixtureOptions.FullPass());
        var trainingPacket = CreateAuthorizedSecondRaterPacket(trainingWorkspace, (row, _) => row["training_dpo_checkpoint_model_locator_mutation"] = true);
        var training = Run(trainingFixture, trainingPacket).Value!;

        Assert.Equal(0, CountPass(training, "inter_rater_adjudication"));
        Assert.Equal(54, training.BlockingProofRefsPass);
        Assert.True(training.TrainingDpoCheckpointModelLocatorMutation);
        Assert.All(CategoryDecisions(training, "inter_rater_adjudication"), decision =>
            Assert.Contains("TRAINING_DPO_CHECKPOINT_MODEL_LOCATOR_MUTATION", decision.RiskCodes));
    }

    [Fact]
    public void Run_RunnerReadyEvalInputsAbsentUntilAllSixtyThreeProofRefsPass()
    {
        using var blockedWorkspace = TempWorkspace();
        var blockedFixture = CreateFixture(blockedWorkspace, new FixtureOptions
        {
            ApprovedCorpus = true,
            SourceDisjointRowsPass = true
        });

        var blocked = Run(blockedFixture).Value!;

        Assert.False(blocked.FullProofPass);
        Assert.False(blocked.RunnerReadyEvalInputsCreated);
        Assert.Equal(0, blocked.RunnerReadyRows);
        Assert.False(File.Exists(Path.Combine(blockedFixture.OutputDirectory, "runner-ready-eval-inputs.jsonl")));

        using var passWorkspace = TempWorkspace();
        var passFixture = CreateFixture(passWorkspace, FixtureOptions.FullPass());

        var pass = Run(passFixture).Value!;

        Assert.True(pass.FullProofPass);
        Assert.True(File.Exists(Path.Combine(passFixture.OutputDirectory, "runner-ready-eval-inputs.jsonl")));
        Assert.Equal(63, pass.BlockingProofRefsPass);
        Assert.Equal(9, pass.RunnerReadyRows);
        Assert.Equal(0, pass.QualityPassRows);
        Assert.Equal(9, File.ReadLines(Path.Combine(passFixture.OutputDirectory, "runner-ready-eval-inputs.jsonl"), Encoding.UTF8).Count());
    }

    [Fact]
    public void Run_LocalSevenOfSevenCandidateKeepsScorerLockedUntilGlobalSixtyThreePass()
    {
        using var workspace = TempWorkspace();
        var fixture = CreateFixture(workspace, FixtureOptions.FullPass());
        KeepOnlyFirstRowFullyPassing(fixture);

        var result = Run(fixture);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;
        Assert.False(report.FullProofPass);
        Assert.Equal(7, report.BlockingProofRefsPass);
        Assert.Equal(0, report.RunnerReadyRows);
        Assert.Equal(0, report.QualityPassRows);
        Assert.False(report.RunnerReadyEvalInputsCreated);
        Assert.False(File.Exists(Path.Combine(fixture.OutputDirectory, "runner-ready-eval-inputs.jsonl")));

        var candidate = Assert.Single(report.RowDecisions, row => row.RunnerReadyCandidate);
        Assert.Equal("QUALITY-03-REVIEW-ROW-001", candidate.RowId);
        Assert.Contains("candidate only", candidate.Decision, StringComparison.Ordinal);
        Assert.Contains("scorer remains locked until 63/63 PASS", candidate.Decision, StringComparison.Ordinal);
        Assert.All(report.RowDecisions, row => Assert.False(row.RunnerScorerRunAllowed));

        var serializedRows = File.ReadLines(Path.Combine(fixture.OutputDirectory, "quality-03-proof-contract-row-results.jsonl"), Encoding.UTF8)
            .Select(line => JsonNode.Parse(line)!.AsObject())
            .ToArray();
        Assert.Equal(9, serializedRows.Length);
        Assert.All(serializedRows, row => Assert.False(row["runner_scorer_run_allowed"]!.GetValue<bool>()));
    }

    [Fact]
    public void Run_NoExternalModelTrainingOrMutationFlagsRemainFalse()
    {
        using var workspace = TempWorkspace();
        var fixture = CreateFixture(workspace, FixtureOptions.FullPass());

        var result = Run(fixture);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;
        Assert.False(report.ExternalAiModelCalled);
        Assert.False(report.TrainingDpoCheckpointModelLocatorMutation);
        Assert.False(report.RunnerScorerRun);
        Assert.False(report.ClaimLocks.QualityImprovementClaimed);
        Assert.False(report.ClaimLocks.SemanticQualityClaimed);
        Assert.False(report.ClaimLocks.SemanticReadinessClaimed);
        Assert.False(report.ClaimLocks.LearnabilityClaimed);
        Assert.False(report.ClaimLocks.ProductReadinessClaimed);
        Assert.False(report.ClaimLocks.FinalReadinessClaimed);
    }

    [Fact]
    public void ResolveOutputPath_BlocksWritesOutsideAllowedOutputRoot()
    {
        using var workspace = TempWorkspace();
        var runner = new Quality03ProofContractRunner();

        var escape = runner.ResolveOutputPath(workspace, Path.Combine("..", "escape.json"));
        var rooted = runner.ResolveOutputPath(workspace, Path.GetFullPath(Path.Combine(workspace, "..", "escape.json")));
        var safe = runner.ResolveOutputPath(workspace, "safe.json");

        Assert.False(escape.IsSuccess);
        Assert.Equal("OUTPUT_PATH_OUTSIDE_ROOT", escape.ErrorCode);
        Assert.False(rooted.IsSuccess);
        Assert.Equal("OUTPUT_PATH_OUTSIDE_ROOT", rooted.ErrorCode);
        Assert.True(safe.IsSuccess, safe.ErrorMessage);
    }

    [Fact]
    public void Run_WritesGeneratedArtifactsWithoutUtf8BomCrLfOrTrailingWhitespace()
    {
        using var blockedWorkspace = TempWorkspace();
        var blockedFixture = CreateFixture(blockedWorkspace, new FixtureOptions());

        var blocked = Run(blockedFixture);

        Assert.True(blocked.IsSuccess, blocked.ErrorMessage);
        Assert.All(blocked.Value!.OutputRefs, AssertGeneratedTextArtifactIsUtf8NoBomLfOnlyAndClean);

        using var passWorkspace = TempWorkspace();
        var passFixture = CreateFixture(passWorkspace, FixtureOptions.FullPass());

        var pass = Run(passFixture);

        Assert.True(pass.IsSuccess, pass.ErrorMessage);
        Assert.Contains(pass.Value!.OutputRefs, outputPath =>
            string.Equals(Path.GetFileName(outputPath), "runner-ready-eval-inputs.jsonl", StringComparison.Ordinal));
        Assert.All(pass.Value!.OutputRefs, AssertGeneratedTextArtifactIsUtf8NoBomLfOnlyAndClean);
    }

    private static OperationResult<Quality03ProofContractRunnerReport> Run(
        FixturePaths fixture,
        string? secondRaterPacketPath = null)
        => new Quality03ProofContractRunner().Run(new Quality03ProofContractRunnerOptions
        {
            EvidencePackDirectory = fixture.EvidencePackDirectory,
            ProofContractPackDirectory = fixture.ProofContractPackDirectory,
            SecondRaterPacketPath = secondRaterPacketPath,
            OutputDirectory = fixture.OutputDirectory,
            RunId = "quality-03-proof-contract-runner-test"
        });

    private static CliProcessResult RunCli(
        FixturePaths fixture,
        string secondRaterPacketPath,
        string outputDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = RepositoryRoot(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var argument in new[]
        {
            "run",
            "--no-restore",
            "--project",
            "43.SmartSkillsDomain.Tools/Quality03ProofContractRunner/Quality03ProofContractRunner.csproj",
            "--",
            "--evidence-pack",
            fixture.EvidencePackDirectory,
            "--proof-contract-pack",
            fixture.ProofContractPackDirectory,
            "--second-rater-packet",
            secondRaterPacketPath,
            "--output-dir",
            outputDirectory,
            "--run-id",
            "quality-03-proof-contract-runner-cli-test"
        })
        {
            startInfo.ArgumentList.Add(argument);
        }

        return RunProcessWithTimeoutAsync(startInfo, ProcessTimeout)
            .GetAwaiter()
            .GetResult();
    }

    private static async Task<CliProcessResult> RunProcessWithTimeoutAsync(
        ProcessStartInfo startInfo,
        TimeSpan timeout)
    {
        var command = DescribeCommand(startInfo);
        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Process could not start: {command}");
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        using var timeoutCts = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            await KillProcessTreeAsync(process);
            var stdoutTail = await ReadOutputTailAsync(stdoutTask);
            var stderrTail = await ReadOutputTailAsync(stderrTask);
            throw new TimeoutException(
                $"Process timed out after {timeout.TotalSeconds:0} seconds.{Environment.NewLine}" +
                $"Command: {command}{Environment.NewLine}" +
                $"Stdout tail:{Environment.NewLine}{stdoutTail}{Environment.NewLine}" +
                $"Stderr tail:{Environment.NewLine}{stderrTail}");
        }

        var stdout = await stdoutTask;
        var stderr = await stderrTask;
        return new CliProcessResult(process.ExitCode, stdout, stderr, command, timeout);
    }

    private static void AssertCliSuccess(CliProcessResult cli)
    {
        Assert.True(
            cli.ExitCode == 0,
            $"CLI exit code was {cli.ExitCode}; expected 0.{Environment.NewLine}" +
            $"Command: {cli.Command}{Environment.NewLine}" +
            $"Timeout: {cli.Timeout.TotalSeconds:0} seconds{Environment.NewLine}" +
            $"Stdout tail:{Environment.NewLine}{Tail(cli.StdOut)}{Environment.NewLine}" +
            $"Stderr tail:{Environment.NewLine}{Tail(cli.StdErr)}");
    }

    private static async Task KillProcessTreeAsync(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (NotSupportedException)
                {
                    if (!process.HasExited)
                        process.Kill();
                }
            }
        }
        catch (InvalidOperationException)
        {
            return;
        }

        try
        {
            await Task.WhenAny(process.WaitForExitAsync(), Task.Delay(TimeSpan.FromSeconds(5)));
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static async Task<string> ReadOutputTailAsync(Task<string> outputTask)
    {
        var completedTask = await Task.WhenAny(outputTask, Task.Delay(TimeSpan.FromSeconds(5)));
        if (completedTask != outputTask)
            return "<output was not fully drained after process kill>";

        try
        {
            return Tail(await outputTask);
        }
        catch (Exception ex)
        {
            return $"<output read failed: {ex.Message}>";
        }
    }

    private static string DescribeCommand(ProcessStartInfo startInfo)
    {
        var arguments = startInfo.ArgumentList.Count > 0
            ? string.Join(" ", startInfo.ArgumentList.Select(QuoteArgument))
            : startInfo.Arguments;
        return $"{startInfo.FileName} {arguments}".Trim();
    }

    private static string QuoteArgument(string argument)
    {
        if (argument.Length == 0)
            return "\"\"";

        if (!argument.Any(char.IsWhiteSpace) && !argument.Contains('"'))
            return argument;

        return $"\"{argument.Replace("\"", "\\\"")}\"";
    }

    private static string Tail(string value, int maxChars = 4000)
        => value.Length <= maxChars ? value : value[^maxChars..];

    private static int CountPass(Quality03ProofContractRunnerReport report, string categoryId)
        => CategoryDecisions(report, categoryId).Count(decision => decision.BlockingRefResolved);

    private static IReadOnlyList<Quality03ProofContractCategoryDecision> CategoryDecisions(
        Quality03ProofContractRunnerReport report,
        string categoryId)
        => report.RowDecisions
            .SelectMany(row => row.CategoryResults)
            .Where(decision => string.Equals(decision.CategoryId, categoryId, StringComparison.Ordinal))
            .ToArray();

    private static string CreateAuthorizedSecondRaterPacket(
        string workspace,
        Action<JsonObject, int>? mutate = null)
    {
        var packetDirectory = Path.Combine(workspace, "authorized-second-rater-packet");
        Directory.CreateDirectory(packetDirectory);
        WriteJsonl(
            Path.Combine(packetDirectory, "per-row-authorized-second-rater-adjudication.jsonl"),
            Enumerable.Range(1, 9).Select(index =>
            {
                var row = AuthorizedSecondRaterRow(Row(index), packetDirectory);
                mutate?.Invoke(row, index);
                return row;
            }));
        return packetDirectory;
    }

    private static string CreateMalformedSecondRaterPacket(string workspace)
    {
        var packetDirectory = Path.Combine(workspace, "malformed-second-rater-packet");
        Directory.CreateDirectory(packetDirectory);
        File.WriteAllText(
            Path.Combine(packetDirectory, "per-row-authorized-second-rater-adjudication.jsonl"),
            "{ malformed-json" + Environment.NewLine,
            Encoding.UTF8);
        return packetDirectory;
    }

    private static JsonObject AuthorizedSecondRaterRow(RowFixture row, string packetDirectory)
    {
        var json = CategoryRow(row, "inter_rater_adjudication", true);
        var packetRef = NormalizedAuthorizedPacketPath(packetDirectory) + "#" + row.RowId;
        json["artifact_ref"] = packetRef;
        json["concrete_proof_ref"] = packetRef;
        json["proof_status"] = "PROOF_PASS";
        json["independent_second_rater_available"] = true;
        json["reviewer_id_1"] = "reviewer-1";
        json["reviewer_id_2"] = "authorized-independent-reviewer-2";
        json["adjudication_status"] = "ADJUDICATED_PASS_LIMITED_TO_INTER_RATER_PROOF_REF";
        json["faked_independence"] = false;
        json["pass_scope"] = "INTER_RATER_ADJUDICATION_PROOF_REF_ONLY_NOT_QUALITY_PASS";
        json["can_satisfy_inter_rater_adjudication_proof"] = true;
        json["runner_ready_after_this_item"] = false;
        json["quality_pass_after_this_item"] = false;
        return json;
    }

    private static string NormalizedAuthorizedPacketPath(string packetDirectory)
        => Path.GetFullPath(Path.Combine(packetDirectory, "per-row-authorized-second-rater-adjudication.jsonl")).Replace('\\', '/');

    private static FixturePaths CreateFixture(string workspace, FixtureOptions options)
    {
        var evidencePack = Path.Combine(workspace, "evidence-pack");
        var proofPack = Path.Combine(workspace, "proof-pack");
        var output = Path.Combine(workspace, "output");
        Directory.CreateDirectory(evidencePack);
        Directory.CreateDirectory(proofPack);
        Directory.CreateDirectory(output);

        var rows = Enumerable.Range(1, 9).Select(Row).ToArray();
        foreach (var fileName in RequiredEvidenceFiles)
            WriteJsonl(Path.Combine(evidencePack, fileName), rows.Select(GenericEvidenceRow));

        WriteJsonl(Path.Combine(proofPack, "per-row-blocking-proof-refs.jsonl"), rows.Select(BlockingProofRow));
        WriteJson(Path.Combine(proofPack, "approved-held-out-source-corpus-manifest.json"), ApprovedCorpusManifest(rows, options));
        WriteJson(Path.Combine(proofPack, "row-specific-nearest-neighbour-index-manifest.json"), NearestNeighbourIndexManifest(options));
        WriteJsonl(Path.Combine(proofPack, "row-specific-source-disjoint-pass-or-risk.jsonl"), rows.Select(row => SourceDisjointRow(row, options)));
        WriteJsonl(Path.Combine(proofPack, "row-specific-nearest-neighbour-pass-or-risk.jsonl"), rows.Select(row => NearestNeighbourRow(row, options)));
        WriteJsonl(Path.Combine(proofPack, "row-specific-ablation-runner-report.jsonl"), rows.Select(row => AblationRow(row, options)));
        WriteJsonl(Path.Combine(proofPack, "row-specific-no-leakage-pass-or-risk.jsonl"), rows.Select(row => ReplayRow(row, options, "no_leakage", options.NoLeakageRowsPass, options.NoLeakageReplayExecuted, options.NoLeakageApprovedRawPrivateRefs)));
        WriteJsonl(Path.Combine(proofPack, "row-specific-no-memorization-pass-or-risk.jsonl"), rows.Select(row => ReplayRow(row, options, "no_memorization", options.NoMemorizationRowsPass, options.NoMemorizationReplayExecuted, options.NoMemorizationApprovedRawPrivateRefs)));
        WriteJsonl(Path.Combine(proofPack, "row-specific-independent-second-rater-adjudication.jsonl"), rows.Select(row => SecondRaterRow(row, options)));

        return new FixturePaths(evidencePack, proofPack, output);
    }

    private static void KeepOnlyFirstRowFullyPassing(FixturePaths fixture)
    {
        KeepOnlyFirstApprovedCorpusRow(fixture);
        foreach (var fileName in new[]
        {
            "row-specific-source-disjoint-pass-or-risk.jsonl",
            "row-specific-nearest-neighbour-pass-or-risk.jsonl",
            "row-specific-ablation-runner-report.jsonl",
            "row-specific-no-leakage-pass-or-risk.jsonl",
            "row-specific-no-memorization-pass-or-risk.jsonl",
            "row-specific-independent-second-rater-adjudication.jsonl"
        })
        {
            KeepOnlyFirstJsonlRowPassing(Path.Combine(fixture.ProofContractPackDirectory, fileName));
        }
    }

    private static void KeepOnlyFirstApprovedCorpusRow(FixturePaths fixture)
    {
        var path = Path.Combine(fixture.ProofContractPackDirectory, "approved-held-out-source-corpus-manifest.json");
        var root = JsonNode.Parse(File.ReadAllText(path, Encoding.UTF8))!.AsObject();
        var rows = root["rows"]!.AsArray();
        foreach (var row in rows.Skip(1).OfType<JsonObject>())
        {
            row["approved_assignment"] = "NONE_DEFERRED";
            row["source_disjoint_pass"] = false;
        }

        WriteJson(path, root);
    }

    private static void KeepOnlyFirstJsonlRowPassing(string path)
    {
        var rows = File.ReadLines(path, Encoding.UTF8)
            .Select(line => JsonNode.Parse(line)!.AsObject())
            .ToArray();
        foreach (var row in rows.Skip(1))
        {
            row["pass"] = false;
            row["pass_or_risk"] = "RISK";
            row["proof_status"] = "RISK_DEFERRED_NOT_PASS";
            row["blocking_ref_resolved"] = false;
            row["risk_codes"] = JsonArrayFrom(["TEST_GLOBAL_FULL_PROOF_BLOCK"]);
        }

        WriteJsonl(path, rows);
    }

    private static JsonObject ApprovedCorpusManifest(IReadOnlyList<RowFixture> rows, FixtureOptions options)
    {
        var rowArray = new JsonArray();
        foreach (var row in rows)
        {
            rowArray.Add(new JsonObject
            {
                ["row_id"] = row.RowId,
                ["pair_id"] = row.PairId,
                ["host"] = row.Host,
                ["method"] = row.Method,
                ["prompt_sha256"] = row.PromptSha256,
                ["approved_assignment"] = options.ApprovedCorpus ? "held_out" : "NONE_DEFERRED",
                ["source_disjoint_pass"] = options.SourceDisjointRowsPass
            });
        }

        return new JsonObject
        {
            ["schema"] = "test.approved_corpus_manifest.v1",
            ["approved"] = options.ApprovedCorpus,
            ["manifest_status"] = options.ApprovedCorpus ? "PASS" : "DEFERRED_NOT_APPROVED_NOT_PASS",
            ["approved_train_source_corpus_manifest_ref"] = options.ApprovedCorpus ? "artifact://approved-train-source-corpus" : null,
            ["approved_held_out_split_manifest_ref"] = options.ApprovedCorpus ? "artifact://approved-held-out-split" : null,
            ["approved_raw_private_corpus_ref"] = options.ApprovedCorpus ? "user-local://approved-private-corpus" : null,
            ["rows"] = rowArray
        };
    }

    private static JsonObject NearestNeighbourIndexManifest(FixtureOptions options)
        => new()
        {
            ["schema"] = "test.nearest_neighbour_index_manifest.v1",
            ["manifest_status"] = options.NearestNeighbourIndexPass ? "PASS" : "DEFERRED_INDEX_NOT_BUILT_NOT_PASS",
            ["index_built"] = options.NearestNeighbourIndexPass,
            ["pass"] = options.NearestNeighbourIndexPass,
            ["corpus_manifest_ref"] = options.NearestNeighbourIndexPass ? "artifact://approved-train-source-corpus" : null,
            ["index_algorithm"] = options.NearestNeighbourIndexPass ? "deterministic-local-bruteforce" : "NOT_RUN",
            ["similarity_threshold"] = options.NearestNeighbourIndexPass ? 0.82 : null,
            ["top_k"] = options.NearestNeighbourIndexPass ? 5 : null
        };

    private static JsonObject SourceDisjointRow(RowFixture row, FixtureOptions options)
    {
        var json = CategoryRow(row, "source_disjoint", options.SourceDisjointRowsPass);
        json["risk_codes"] = JsonArrayFrom(options.SourceDisjointRowsPass ? Array.Empty<string>() : ["SOURCE_DISJOINT_NOT_PASS"]);
        return json;
    }

    private static JsonObject NearestNeighbourRow(RowFixture row, FixtureOptions options)
    {
        var json = CategoryRow(row, "nearest_neighbour_similarity_risk", options.NearestNeighbourRowsPass);
        json["index_contract_present"] = options.NearestNeighbourRowsPass;
        json["top_k_results"] = options.NearestNeighbourRowsPass ? JsonArrayFrom(["artifact://nn-top-k"]) : new JsonArray();
        json["risk_codes"] = JsonArrayFrom(options.NearestNeighbourRowsPass ? Array.Empty<string>() : ["NN_NOT_PASS"]);
        return json;
    }

    private static JsonObject AblationRow(RowFixture row, FixtureOptions options)
    {
        var json = CategoryRow(row, "ablation", options.AblationRowsPass);
        json["runner_executed"] = options.AblationRunnerExecuted;
        json["runner_contract_available_for_current_rows"] = options.AblationRunnerContractAvailable;
        json["shortcut_only"] = options.AblationShortcutOnly;
        json["runner_status"] = options.AblationRunnerExecuted ? "EXECUTED" : "NOT_RUN";
        json["controls_required"] = JsonArrayFrom(RequiredControls);
        json["controls_run"] = JsonArrayFrom(options.AblationRunnerExecuted && !options.AblationShortcutOnly ? RequiredControls : Array.Empty<string>());
        json["risk_codes"] = JsonArrayFrom(options.AblationRowsPass ? Array.Empty<string>() : ["ABLATION_NOT_PASS"]);
        return json;
    }

    private static JsonObject ReplayRow(
        RowFixture row,
        FixtureOptions options,
        string categoryId,
        bool pass,
        bool replayExecuted,
        bool approvedRefs)
    {
        var json = CategoryRow(row, categoryId, pass);
        json["replay_probe_executed"] = replayExecuted;
        json["pass_boundary_flags_only"] = false;
        json["approved_raw_private_refs"] = approvedRefs ? JsonArrayFrom(["user-local://approved-private-corpus"]) : new JsonArray();
        json["risk_codes"] = JsonArrayFrom(pass ? Array.Empty<string>() : [$"{categoryId.ToUpperInvariant()}_NOT_PASS"]);
        return json;
    }

    private static JsonObject SecondRaterRow(RowFixture row, FixtureOptions options)
    {
        var json = CategoryRow(row, "inter_rater_adjudication", options.SecondRaterRowsPass);
        json["independent_second_rater_available"] = options.SecondRaterAvailable;
        json["reviewer_id_1"] = "reviewer-1";
        json["reviewer_id_2"] = options.SecondRaterAvailable
            ? options.SecondRaterSelfChecker ? "deterministic-self-checker" : "reviewer-2"
            : null;
        json["adjudication_status"] = options.SecondRaterAvailable ? "ADJUDICATED_PASS" : "DEFERRED_REQUIRES_SECOND_RATER";
        json["faked_independence"] = false;
        json["risk_codes"] = JsonArrayFrom(options.SecondRaterRowsPass ? Array.Empty<string>() : ["SECOND_RATER_NOT_PASS"]);
        return json;
    }

    private static JsonObject BlockingProofRow(RowFixture row)
    {
        var categories = new JsonArray();
        foreach (var category in Categories)
        {
            categories.Add(new JsonObject
            {
                ["category_id"] = category.CategoryId,
                ["required_proof_ref"] = category.RequiredProofRef,
                ["runner_blocking"] = true,
                ["pass_or_risk"] = "RISK",
                ["proof_status"] = "RISK_DEFERRED_NOT_PASS",
                ["blocking_ref_resolved"] = false
            });
        }

        var json = CommonRow(row);
        json["category_results"] = categories;
        return json;
    }

    private static JsonObject GenericEvidenceRow(RowFixture row)
    {
        var json = CommonRow(row);
        json["artifact_row_produced"] = true;
        return json;
    }

    private static JsonObject CategoryRow(RowFixture row, string categoryId, bool pass)
    {
        var json = CommonRow(row);
        json["category_id"] = categoryId;
        json["artifact_ref"] = $"artifact://{categoryId}#{row.RowId}";
        json["pass"] = pass;
        json["pass_or_risk"] = pass ? "PASS" : "RISK";
        json["proof_status"] = pass ? "PASS" : "RISK_DEFERRED_NOT_PASS";
        json["blocking_ref_resolved"] = pass;
        json["external_ai_model_called"] = false;
        json["training_dpo_checkpoint_model_locator_mutation"] = false;
        return json;
    }

    private static JsonObject CommonRow(RowFixture row)
        => new()
        {
            ["schema"] = "test.quality03.row.v1",
            ["row_id"] = row.RowId,
            ["pair_sequence_id"] = row.PairSequenceId,
            ["pair_id"] = row.PairId,
            ["scenario_id"] = row.ScenarioId,
            ["source_scenario_id"] = row.SourceScenarioId,
            ["host"] = row.Host,
            ["method"] = row.Method,
            ["evidence_class"] = "test_fixture",
            ["prompt_sha256"] = row.PromptSha256
        };

    private static RowFixture Row(int index)
        => new(
            RowId: $"QUALITY-03-REVIEW-ROW-{index:000}",
            PairSequenceId: $"token-pair-{index:000}",
            PairId: $"TOKEN-PAIR-TEST-{index:000}",
            ScenarioId: $"TOKEN-SCENARIO-TEST-{index:000}",
            SourceScenarioId: $"SOURCE-SCENARIO-TEST-{index:000}",
            Host: index <= 3 ? "Claude" : index <= 6 ? "Cursor" : "Codex",
            Method: index % 3 == 1 ? "prompt-to-task" : index % 3 == 2 ? "orchestration-plan" : "skill-selection",
            PromptSha256: new string('a', 63) + index.ToString("x"));

    private static JsonArray JsonArrayFrom(IEnumerable<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
            array.Add(value);
        return array;
    }

    private static bool HasUtf8Bom(byte[] bytes)
        => bytes.Length >= 3 &&
           bytes[0] == 0xEF &&
           bytes[1] == 0xBB &&
           bytes[2] == 0xBF;

    private static void AssertGeneratedTextArtifactIsUtf8NoBomLfOnlyAndClean(string outputPath)
    {
        var bytes = File.ReadAllBytes(outputPath);
        Assert.False(HasUtf8Bom(bytes), outputPath);

        var text = Encoding.UTF8.GetString(bytes);
        Assert.DoesNotContain("\r", text);

        var lines = text.Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            Assert.False(
                line.EndsWith(" ", StringComparison.Ordinal) ||
                line.EndsWith("\t", StringComparison.Ordinal),
                $"{outputPath}:{index + 1}");
        }
    }

    private static void WriteJson(string path, JsonObject root)
        => File.WriteAllText(path, root.ToJsonString(SerializerOptions()) + Environment.NewLine, Encoding.UTF8);

    private static void WriteJsonl(string path, IEnumerable<JsonObject> rows)
        => File.WriteAllLines(path, rows.Select(row => row.ToJsonString(SerializerOptions())), Encoding.UTF8);

    private static JsonSerializerOptions SerializerOptions()
        => new()
        {
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "43.SmartSkillsDomain.Tools")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root with 43.SmartSkillsDomain.Tools was not found.");
    }

    private static TempDirectory TempWorkspace()
        => new();

    private sealed record FixturePaths(string EvidencePackDirectory, string ProofContractPackDirectory, string OutputDirectory);

    private sealed record RowFixture(
        string RowId,
        string PairSequenceId,
        string PairId,
        string ScenarioId,
        string SourceScenarioId,
        string Host,
        string Method,
        string PromptSha256);

    private sealed record CategoryFixture(string CategoryId, string RequiredProofRef);

    private sealed record CliProcessResult(
        int ExitCode,
        string StdOut,
        string StdErr,
        string Command,
        TimeSpan Timeout);

    private sealed class FixtureOptions
    {
        public bool ApprovedCorpus { get; init; }
        public bool SourceDisjointRowsPass { get; init; }
        public bool NearestNeighbourIndexPass { get; init; }
        public bool NearestNeighbourRowsPass { get; init; }
        public bool AblationRowsPass { get; init; }
        public bool AblationRunnerExecuted { get; init; }
        public bool AblationRunnerContractAvailable { get; init; }
        public bool AblationShortcutOnly { get; init; }
        public bool NoLeakageRowsPass { get; init; }
        public bool NoLeakageReplayExecuted { get; init; }
        public bool NoLeakageApprovedRawPrivateRefs { get; init; }
        public bool NoMemorizationRowsPass { get; init; }
        public bool NoMemorizationReplayExecuted { get; init; }
        public bool NoMemorizationApprovedRawPrivateRefs { get; init; }
        public bool SecondRaterRowsPass { get; init; }
        public bool SecondRaterAvailable { get; init; }
        public bool SecondRaterSelfChecker { get; init; }

        public static FixtureOptions FullPass()
            => new()
            {
                ApprovedCorpus = true,
                SourceDisjointRowsPass = true,
                NearestNeighbourIndexPass = true,
                NearestNeighbourRowsPass = true,
                AblationRowsPass = true,
                AblationRunnerExecuted = true,
                AblationRunnerContractAvailable = true,
                NoLeakageRowsPass = true,
                NoLeakageReplayExecuted = true,
                NoLeakageApprovedRawPrivateRefs = true,
                NoMemorizationRowsPass = true,
                NoMemorizationReplayExecuted = true,
                NoMemorizationApprovedRawPrivateRefs = true,
                SecondRaterRowsPass = true,
                SecondRaterAvailable = true
            };
    }

    private sealed class TempDirectory : IDisposable
    {
        private readonly string _path = Path.Combine(Path.GetTempPath(), $"quality03-proof-runner-tests-{Guid.NewGuid():N}");

        public TempDirectory()
        {
            Directory.CreateDirectory(_path);
        }

        public static implicit operator string(TempDirectory directory)
            => directory._path;

        public void Dispose()
        {
            if (Directory.Exists(_path))
                Directory.Delete(_path, recursive: true);
        }
    }
}
