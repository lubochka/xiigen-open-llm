// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
using System.Globalization; // Parse finite budgets without locale-dependent decimals.
using System.Security.Cryptography; // Read pins from actual saved bytes.
using System.Text.Json; // Keep CLI results structured and free of training text.
using ContinuousLearning; // The existing learning owner performs all material/admission/trainer work.
using ModelServer; // Reuse the production pinned configuration/tokenizer reader.
using TrainableRouter.Training; // Reuse canonical saved-root full-LoRA startup.
using TranslationDomain;
using AgentInfrastructure.Logical;
using DPO.Models;

const string Root = "C:/Projects/llm_mvp_core"; // This task operates only in the existing canonical checkout.
const string ConfigHash = "e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40"; // Actual accepted generation configuration, never a fresh random baseline.
const string B0Hash = "a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911"; // Immutable initial full-LoRA saved weights.
const string B0ManifestHash = "c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d"; // Canonical manifest identity reported by the existing loader.
const string CalibrationHash = "c4391d3d75432533e3faa0712119af81016312a5f1bbc85de3e46baf81f085ae"; // Separate excluded calibration pair.
const string MainHash = "c78a620a01cd2ab1d35962eb3521a9e87ee51deeedc325dfc809112b0e035935"; // Exact human-authorized 500-record input.
try // Native exceptions and typed refusals remain nonzero exits, never automatic retries.
{ // Argument validation happens before model loading or run effects.
    if (args.Length >= 2 && Path.GetFullPath(args[1]) != Path.GetFullPath(Root)) throw new ArgumentException("Canonical repository root mismatch."); // Do not create a sibling model tree.
    if (args.Length == 9 && args[0] == "first-decision-enru")
    {
        using var diagnosticStop = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; diagnosticStop.Cancel(); };
        RunFirstDecisionDiagnostic(args, diagnosticStop.Token);
        return 0;
    }
    if (args.Length == 2 && args[0] == "inspect-enru-source")
    {
        var source = SimulationEnRuContinuation.ReadState(Root);
        Console.WriteLine(JsonSerializer.Serialize(new { status = "VERIFIED_PINNED_RESEARCH_STATE", sourceStep = source.Optimizer.Step, optimizerEntries = source.Optimizer.Entries.Count, randomStep = source.Random.RandomStateStep, source.Proof.SourceIdentity, modelCalls = 0, modelAllocations = 0, optimizerCalls = 0 }));
        return 0;
    }
    if (args.Length == 5 && args[0] == "preflight-enru")
    {
        // This explicitly selected runtime reads the pinned panel programmatically; it allocates no Student.
        var budget = int.Parse(args[4], CultureInfo.InvariantCulture);
        if (budget is < 0 or > 256) throw new ArgumentException("Invalid preflight token budget.");
        var config = ReadConfiguration(CancellationToken.None);
        var bytes = File.ReadAllBytes(args[2]);
        if (bytes.Length > 1024 * 1024 || Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant() != args[3]) throw new InvalidDataException("Preflight panel pin mismatch.");
        var rows = new List<LogicalDpoPreferenceExample>();
        var lines = new System.Text.UTF8Encoding(false, true).GetString(bytes).Split('\n');
        var panelIds = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < lines.Length; index++)
        {
            if (index == lines.Length - 1 && lines[index].Length == 0) break;
            using var row = JsonDocument.Parse(lines[index]); var value = row.RootElement;
            if (!panelIds.Add(value.GetProperty("id").GetString()!)) throw new InvalidDataException("Preflight panel duplicate ID.");
            var prompt = value.GetProperty("prompt").GetString()!;
            _ = SimulationEnRuPrompt.Extract(prompt);
            var chosen = value.GetProperty("chosen").GetString()!; var rejected = value.GetProperty("rejected").GetString()!;
            foreach (var answer in new[] { chosen, rejected })
            {
                var ids = config.Tokenizer.Encode(answer);
                if (config.Tokenizer.Decode(ids) != answer || budget > 0 && ids.Count > budget) throw new InvalidDataException("Preflight answer is lossy or does not fit frozen free budget.");
            }
            if (config.Tokenizer.Encode(prompt).Count + Math.Max(0, budget - 1) > config.StartRequest.ModelConfig.ContextLen) throw new InvalidDataException("Preflight generation exceeds context.");
            rows.Add(new(prompt, chosen, rejected) { ChosenAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator, RejectedAuthorKind = DpoPairSideAuthorKind.DeterministicGenerator });
        }
        if (rows.Count is < 1 or > 100) throw new InvalidDataException("Preflight panel count invalid.");
        var material = LogicalDpoPreparedBatchMaterializer.Prepare(rows.ToArray(), config.Tokenizer);
        if (!material.IsSuccess || material.Value is null) throw new InvalidDataException(material.ErrorCode);
        SimulationEnRuContinuation.VerifyReadOnlyMaterial(material.Value, config.StartRequest.ModelConfig.VocabSize, config.StartRequest.ModelConfig.ContextLen, config.Tokenizer.ComputeStateSha256());
        var exact = material.Value.Select(x => x.Batch.ExactMaterial!).ToArray();
        Console.WriteLine(JsonSerializer.Serialize(new { status = "LOSSLESS_EXACT_MATERIAL_AND_BUDGET_VERIFIED", panelSha256 = args[3], pairs = rows.Count, budget,
            tokenizerSha256 = config.Tokenizer.ComputeStateSha256(), losslessAll = true, contextFitsAll = true, responseMasksValidAll = true,
            chosenTargetsMin = exact.Min(x => x.ChosenLength - x.PromptLength), chosenTargetsMax = exact.Max(x => x.ChosenLength - x.PromptLength),
            rejectedTargetsMin = exact.Min(x => x.RejectedLength - x.PromptLength), rejectedTargetsMax = exact.Max(x => x.RejectedLength - x.PromptLength),
            modelAllocations = 0, studentCalls = 0, optimizerCalls = 0 }));
        return 0;
    }
    if (args.Length == 5 && args[0] is "inspect" or "inspect-enru") // This read-only mode is also useful for focused import rejection checks.
    { // It constructs neither a model nor a mutation lease.
        var input = CodeDocumentationSimulationDpoExperiment.Inspect(args[2], args[3], int.Parse(args[4], CultureInfo.InvariantCulture), args[0] == "inspect-enru" ? SimulationDpoDomain.EnRu : SimulationDpoDomain.CodeDocumentation); // Validate complete exact source bytes.
        Console.WriteLine(JsonSerializer.Serialize(new { input.IsSuccess, input.ErrorCode, pairCount = input.Value?.Count, modelCalls = 0, optimizerCalls = 0 })); // Do not print prompt/response metadata.
        return input.IsSuccess ? 0 : 2; // Refusal is observable to the native caller.
    } // No output directory is created by inspect.
    if (args.Length == 7 && args[0] == "translate-enru")
    {
        // Cold ordinary request; unlike evaluation this input is raw English with no training prefix.
        using var translationStop = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; translationStop.Cancel(); };
        var runId = args[3];
        if (runId.Length is < 1 or > 128 || !runId.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_')) throw new ArgumentException("Invalid translation run ID.");
        var rawEnglish = new System.Text.UTF8Encoding(false, true).GetString(File.ReadAllBytes(args[2]));
        _ = SimulationEnRuPrompt.Build(rawEnglish);
        var directory = Path.Combine(Root, "training-artifacts", "experiments", "code-documentation-simulation", runId);
        if (File.Exists(directory) || Directory.Exists(directory) || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(directory)) throw new InvalidOperationException("Translation output exists or has invalid custody; reconcile before retry.");
        var settings = new CodeDocumentationStudentGenerationSettings(int.Parse(args[6], CultureInfo.InvariantCulture), 0, 0, 1, 20261014, 4096);
        if (settings.MaxNewTokens is < 1 or > 256) throw new ArgumentException("Invalid generation budget.");
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(directory);
        var output = Path.Combine(directory, "ordinary-translation.json");
        WriteNew(Path.Combine(directory, "intent.json"), new { runId, inputSha256 = CodeDocumentationStudentGeneration.InputSha256(rawEnglish), checkpoint = args[4], checkpointSha256 = args[5], settings, status = "ISSUED_RECONCILE_BEFORE_RETRY" });
        var student = SimulationEnRuContinuation.OpenStudent(Root, LoadModels(translationStop.Token), args[4], args[5], translationStop.Token);
        var translationResult = Translate(student, runId, rawEnglish, settings, translationStop.Token);
        WriteNew(output, translationResult);
        Console.WriteLine(JsonSerializer.Serialize(new { status = "MECHANICAL_TYPED_ROUTE_COMPLETED_QUALITY_UNKNOWN", receipt = output, studentGenerationCalls = 1, optimizerCalls = 0 }));
        return 0;
    }
    if (args.Length == 8 && args[0] is "evaluate" or "evaluate-enru") // The independent owner supplies a new pinned panel and frozen decoder budget later.
    { // This command cannot issue a mutation capability or fabricate a correctness grade.
        using var evaluationStop = new CancellationTokenSource(); // Explicit caller cancellation has no hidden evaluator timeout.
        Console.CancelKeyPress += (_, eventArgs) => { eventArgs.Cancel = true; evaluationStop.Cancel(); }; // Preserve caller lifetime through the actual Student APIs.
        var enru = args[0] == "evaluate-enru";
        if (enru && args[5] == "B0") throw new ArgumentException("ENRU control must explicitly select the complete C50 checkpoint and hash.");
        var evaluationResult = CodeDocumentationSimulationDpoExperiment.Evaluate(new(Root, args[4], args[2], args[3], // Panel bodies are read only by the actual runtime after explicit command selection.
            args[5] == "B0" ? null : args[5], args[5] == "B0" ? null : args[6], new(int.Parse(args[7], CultureInfo.InvariantCulture), 0, 0, 1, 20261014, 4096), LoadModels,
            enru ? SimulationDpoDomain.EnRu : SimulationDpoDomain.CodeDocumentation,
            enru ? (student, row, settings, token) => Translate(student, row.Id, SimulationEnRuPrompt.Extract(row.Prompt), settings, token, row.Prompt) : null), evaluationStop.Token); // Both arms reach the same typed translation owner and preserve raw answers.
        Console.WriteLine(JsonSerializer.Serialize(new { evaluationResult.IsSuccess, evaluationResult.ErrorCode, receipt = evaluationResult.Value, pid = Environment.ProcessId, nativeEndedUtc = DateTimeOffset.UtcNow })); // Raw answers remain in private files for the independent examiner.
        return evaluationResult.IsSuccess ? 0 : 1; // Real runtime failure stays nonzero and is not retried.
    } // Evaluation is optional and separate from training's zero-external-call critical path.
    if (!((args.Length == 7 && args[0] == "calibrate") || (args.Length == 8 && args[0] is "train" or "train-enru")))
    { // Print a concrete executable contract instead of choosing a schedule silently.
        Console.Error.WriteLine("Usage: inspect|inspect-enru <root> <input.jsonl> <sha256> <count>; inspect-enru-source <root>; preflight-enru <root> <panel.jsonl> <sha256> <0|positive-max-new-tokens>; calibrate <root> <input.jsonl> <sha256> <run-id> 1 1; train|train-enru <root> <input.jsonl> <sha256> <run-id> <batch-size> <max-updates> <none|positive-finite-wall-budget-seconds>; evaluate|evaluate-enru <root> <panel.jsonl> <sha256> <run-id> <candidate.bin|B0> <sha256|B0> <max-new-tokens>; translate-enru <root> <raw-english.txt> <run-id> <candidate.bin> <sha256> <max-new-tokens>");
        Console.Error.WriteLine("first-decision-enru <root> <panel.jsonl> <panel-sha256> <new-run-id> <C50.bin> <C50-sha256> <C100.bin> <C100-sha256>");
        return 2; // Unknown commands have no effects.
    } // The tool has no publish, retry, reset or legacy experiment-resume command.
    var calibration = args[0] == "calibrate"; // Calibration and main are distinct candidate arms.
    var enruTraining = args[0] == "train-enru";
    if (args[3] != (enruTraining ? SimulationEnRuContinuation.DatasetSha256 : calibration ? CalibrationHash : MainHash)) throw new ArgumentException("The selected exact simulation dataset pin is not authorized for this arm.");
    using var stop = new CancellationTokenSource(); // Explicit caller cancellation remains available with or without the optional main wall-clock budget.
    Console.CancelKeyPress += (_, eventArgs) => { eventArgs.Cancel = true; stop.Cancel(); }; // Keep cancellation cooperative between existing transactional updates.
    Console.WriteLine(JsonSerializer.Serialize(new { nativeStartedUtc = DateTimeOffset.UtcNow, pid = Environment.ProcessId, mode = args[0], runId = args[4], buildConfiguration = BuildConfiguration() })); // Record actual PID/start before deferred model work.
    var result = CodeDocumentationSimulationDpoExperiment.Run(new SimulationDpoRunOptions(Root, args[4], calibration ? "calibration" : "main", // Existing core owns the experiment effect, this host stays thin.
        args[2], args[3], calibration ? 1 : 500, int.Parse(args[5], CultureInfo.InvariantCulture), int.Parse(args[6], CultureInfo.InvariantCulture), // Fixed pair/update counts are separate inputs.
        calibration ? null : ParseMainWallBudget(args[7]), LoadModels, enruTraining ? SimulationDpoDomain.EnRu : SimulationDpoDomain.CodeDocumentation), stop.Token); // ENRU restores C50; legacy main still starts from B0.
    Console.WriteLine(JsonSerializer.Serialize(new { result.IsSuccess, result.ErrorCode, receipt = result.Value, nativeEndedUtc = DateTimeOffset.UtcNow, pid = Environment.ProcessId })); // A saved candidate is private, not production adoption or a quality claim.
    return result.IsSuccess ? 0 : 1; // The native exit reflects the actual run/readback outcome.
} // No second run is automatically issued after an unknown effect.
catch (Exception exception) // Retain concrete local failure identity without dumping source texts.
{ Console.Error.WriteLine(JsonSerializer.Serialize(new { status = "REFUSED_OR_FAILED", exception = exception.GetType().Name, exception.Message, nativeEndedUtc = DateTimeOffset.UtcNow, pid = Environment.ProcessId })); return 1; } // The existing attempt must be reconciled before retry.

object Translate(SimulationDpoReadOnlyStudent student, string requestId, string rawEnglish, CodeDocumentationStudentGenerationSettings settings, CancellationToken token, string? expectedPrompt = null)
{
    var adapter = new ResearchCheckpointEnRuAdapter(student, settings);
    var machine = new TranslationMachine(new TranslationRoutingFlow(new Dictionary<TranslationDirection, ITranslationDirectionAdapter> { [TranslationDirection.EN_RU] = adapter }));
    var selected = machine.GetOrchestratorAsync(token).GetAwaiter().GetResult();
    if (!selected.IsSuccess || selected.Value is null) throw new InvalidOperationException(selected.ErrorCode);
    var request = new LogicalOrchestratorRequest(requestId, TranslationRoutingFlow.ScenarioId, rawEnglish, new LogicalExecutionFlags(),
        new Dictionary<string, object> { ["source_language"] = "en", ["target_language"] = "ru", ["topic_domain"] = "general" });
    var routed = selected.Value.RunAsync(request, token).GetAwaiter().GetResult();
    if (!routed.IsSuccess || routed.Value is null || adapter.LastGeneration is null) throw new InvalidOperationException(routed.ErrorCode ?? "RESEARCH_ENRU_GENERATION_MISSING");
    if (expectedPrompt is not null && adapter.LastGeneration.FramedInputSha256 != CodeDocumentationStudentGeneration.InputSha256(expectedPrompt)) throw new InvalidDataException("RESEARCH_ENRU_EVALUATION_FRAME_MISMATCH");
    return new { id = requestId, route = routed.Value, trace = adapter.LastGeneration, qualityVerdict = "UNKNOWN_NOT_GRADED", productionAdopted = false };
}

static void WriteNew(string path, object value)
{
    using var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read, FileOptions.WriteThrough);
    stream.Write(JsonSerializer.SerializeToUtf8Bytes(value, new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true })); stream.Flush(true);
}

void RunFirstDecisionDiagnostic(string[] input, CancellationToken token)
{
    const string PanelHash = "e5f7a31f28cad6d0dd2de31843bb1efa55d06f57cf881cce84d9418f288861a8";
    const string PromptHash = "a0c74b4bddbc51fd3f5fbe3cb7abe0da5ad9f7eb0774fbadbac3e45a8475e60e";
    const string C50Hash = "6f4d0284ea823ba99932b0f28576855c14205703115b48c983c7a9f03749179b";
    const string C100Hash = "3acb5a9080d0454942c261eca4c1b886aab011db83729947509be4463785fab5";
    token.ThrowIfCancellationRequested();
    var panelBytes = File.ReadAllBytes(input[2]);
    if (input[3] != PanelHash || Convert.ToHexString(SHA256.HashData(panelBytes)).ToLowerInvariant() != PanelHash ||
        input[6] != C50Hash || input[8] != C100Hash)
        throw new InvalidDataException("FIRST_DECISION_FROZEN_PIN_MISMATCH");
    string? prompt = null;
    foreach (var line in new System.Text.UTF8Encoding(false, true).GetString(panelBytes).Split('\n', StringSplitOptions.RemoveEmptyEntries))
    {
        using var row = JsonDocument.Parse(line);
        if (row.RootElement.GetProperty("id").GetString() != "enru-e01") continue;
        if (prompt is not null) throw new InvalidDataException("FIRST_DECISION_ROW_DUPLICATE");
        // Только уже раскрытый prompt; выбранные/отклонённые ответы не читаются.
        prompt = row.RootElement.GetProperty("prompt").GetString();
    }
    if (prompt is null || System.Text.Encoding.UTF8.GetByteCount(prompt) != 70 ||
        CodeDocumentationStudentGeneration.InputSha256(prompt) != PromptHash)
        throw new InvalidDataException("FIRST_DECISION_PROMPT_PIN_MISMATCH");
    var runId = input[4];
    if (runId.Length is < 1 or > 128 || !runId.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_'))
        throw new ArgumentException("Invalid diagnostic run ID.");
    var directory = Path.Combine(Root, ".kiro", "xiigen-slices", "work", "results", "RP01-first-decision-20261016", runId);
    if (File.Exists(directory) || Directory.Exists(directory) || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(directory))
        throw new InvalidOperationException("Diagnostic output exists or has invalid custody; reconcile before retry.");
    var sourcePins = DiagnosticSourcePins();
    var runtimePins = DiagnosticRuntimePins();
    var settings = new CodeDocumentationStudentGenerationSettings(1, 0, 0, 1, 20261014, 4096);
    SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(directory);
    var total = System.Diagnostics.Stopwatch.StartNew();
    WriteNew(Path.Combine(directory, "issued.json"), new { status = "ISSUED_RECONCILE_BEFORE_RETRY", provenance = "simulation",
        rowId = "enru-e01", exactPrompt = prompt, promptSha256 = PromptHash, promptUtf8Bytes = 70, panelPath = Path.GetFullPath(input[2]), panelSha256 = PanelHash,
        arms = new[] { new { id = "C50", checkpointPath = Path.GetFullPath(input[5]), checkpointSha256 = C50Hash }, new { id = "C100", checkpointPath = Path.GetFullPath(input[7]), checkpointSha256 = C100Hash } },
        settings, intendedPrefillForwardCalls = 2, intendedTokenChoices = 2, intendedDecodeForwardCalls = 0,
        optimizerCalls = 0, sourcePins, runtimePins, configSha256 = ConfigHash,
        process = DiagnosticProcessObservation(), issuedUtc = DateTimeOffset.UtcNow });

    SimulationDpoFirstDecision RunArm(string id, string checkpoint, string hash)
    {
        token.ThrowIfCancellationRequested();
        WriteNew(Path.Combine(directory, id + "-issued.json"), new { id, checkpoint, checkpointSha256 = hash,
            status = "ISSUED_RECONCILE_BEFORE_RETRY", startedUtc = DateTimeOffset.UtcNow, process = DiagnosticProcessObservation() });
        var armTime = System.Diagnostics.Stopwatch.StartNew();
        var loadTime = System.Diagnostics.Stopwatch.StartNew();
        // Для каждого arm — независимый model context; TextGenerator создаёт свежий KV cache.
        var student = SimulationEnRuContinuation.OpenStudent(Root, LoadModels(token), checkpoint, hash, token);
        loadTime.Stop();
        var decision = student.GenerateFirstDecision(prompt, settings, token);
        var logits = decision.Generation.Decision.Logits;
        var bits = logits.Select(BitConverter.SingleToInt32Bits).ToArray();
        var rawBytes = new byte[checked(bits.Length * sizeof(float))];
        for (var i = 0; i < bits.Length; i++)
            System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(rawBytes.AsSpan(i * sizeof(float), sizeof(float)), bits[i]);
        var vectorHash = Convert.ToHexString(SHA256.HashData(rawBytes)).ToLowerInvariant();
        WriteNew(Path.Combine(directory, id + "-logits.json"), new { id, finiteFloat32 = true,
            encoding = "float32 little-endian raw bits", sha256 = vectorHash, values = logits, rawBitsInt32 = bits });
        WriteNew(Path.Combine(directory, id + "-completed.json"), new { id, source = student.Source,
            decision.Before, decision.After, decision.CheckpointSha256Before, decision.CheckpointSha256After,
            decision.TokenizerSha256, promptSha256 = PromptHash, promptTokenIds = decision.Generation.Decision.PromptTokenIds,
            generation = decision.Generation.Receipt, decision.Generation.PrefillForwardCalls, decision.Generation.DecodeForwardCalls,
            selectedTokenId = decision.Generation.Decision.SelectedTokenId, deterministicArgmax = DiagnosticArgmax(logits),
            selectedMatchesDeterministicArgmax = decision.Generation.Decision.SelectedTokenId == DiagnosticArgmax(logits),
            top = DiagnosticTop(logits), rawFloatBitsSha256 = vectorHash, vocabularySize = logits.Count,
            loadMilliseconds = loadTime.Elapsed.TotalMilliseconds, decision.Generation.PrefillElapsedMilliseconds,
            endToEndMilliseconds = armTime.Elapsed.TotalMilliseconds, process = DiagnosticProcessObservation(), completedUtc = DateTimeOffset.UtcNow });
        Console.WriteLine(JsonSerializer.Serialize(new { arm = id, status = "FIRST_DECISION_COMPLETED", selectedTokenId = decision.Generation.Decision.SelectedTokenId,
            decision.Generation.PrefillForwardCalls, decision.Generation.DecodeForwardCalls, loadMilliseconds = loadTime.Elapsed.TotalMilliseconds,
            decision.Generation.PrefillElapsedMilliseconds, receipt = Path.Combine(directory, id + "-completed.json") }));
        return decision;
    }

    var first = RunArm("C50", input[5], C50Hash);
    var second = RunArm("C100", input[7], C100Hash);
    var a = first.Generation.Decision.Logits; var b = second.Generation.Decision.Logits;
    if (a.Count != b.Count || first.TokenizerSha256 != second.TokenizerSha256 ||
        !first.Generation.Decision.PromptTokenIds.SequenceEqual(second.Generation.Decision.PromptTokenIds))
        throw new InvalidDataException("FIRST_DECISION_ARMS_NOT_COMPARABLE");
    var sourceAfter = DiagnosticSourcePins(); var runtimeAfter = DiagnosticRuntimePins();
    if (JsonSerializer.Serialize(sourcePins) != JsonSerializer.Serialize(sourceAfter) ||
        JsonSerializer.Serialize(runtimePins) != JsonSerializer.Serialize(runtimeAfter) || HashFile(input[2]) != PanelHash)
        throw new InvalidDataException("FIRST_DECISION_EXECUTION_PINS_CHANGED");
    bool rawEqual = a.Zip(b).All(pair => BitConverter.SingleToInt32Bits(pair.First) == BitConverter.SingleToInt32Bits(pair.Second));
    double shift = (double)b[0] - a[0];
    bool uniformShift = a.Zip(b).All(pair => (double)pair.Second - pair.First == shift);
    bool sameArgmax = DiagnosticArgmax(a) == DiagnosticArgmax(b);
    var logA = DiagnosticLogSoftmax(a); var logB = DiagnosticLogSoftmax(b);
    string status = rawEqual ? "IDENTICAL_LOGITS" : uniformShift ? "SAME_DISTRIBUTION_RAW_UNIFORM_SHIFT" :
        sameArgmax ? "CHANGED_DISTRIBUTION_SAME_ARGMAX" : "CHANGED_DISTRIBUTION_CHANGED_ARGMAX";
    var comparison = new { status, provenance = "simulation", qualityVerdict = "UNKNOWN_NOT_GRADED", productionAdopted = false,
        rawBitsEqual = rawEqual, exactUniformRawShift = uniformShift, uniformRawShift = uniformShift ? (double?)shift : null,
        maxAbsoluteLogitDelta = a.Zip(b).Max(pair => Math.Abs((double)pair.First - pair.Second)),
        maxAbsoluteLogSoftmaxDelta = logA.Zip(logB).Max(pair => Math.Abs(pair.First - pair.Second)),
        numericConvention = "Exact finite float32 bit comparison; uniform shift tested exactly in double; stable log-softmax in double, no scientific acceptance threshold. Numeric differences are not quality gains.",
        selectedTokenIds = new[] { first.Generation.Decision.SelectedTokenId, second.Generation.Decision.SelectedTokenId }, sameArgmax,
        actualPrefillForwardCalls = first.Generation.PrefillForwardCalls + second.Generation.PrefillForwardCalls,
        actualDecodeForwardCalls = first.Generation.DecodeForwardCalls + second.Generation.DecodeForwardCalls,
        actualTokenChoices = first.Generation.Receipt.GeneratedTokenCount + second.Generation.Receipt.GeneratedTokenCount,
        optimizerCalls = 0, baseKeyedValuesEqual = first.Before.BaseKeyedValuesSha256 == second.Before.BaseKeyedValuesSha256,
        adapterKeyedValuesEqual = first.Before.AdapterKeyedValuesSha256 == second.Before.AdapterKeyedValuesSha256,
        sourcePins, runtimePins, endToEndMilliseconds = total.Elapsed.TotalMilliseconds, process = DiagnosticProcessObservation(), completedUtc = DateTimeOffset.UtcNow };
    WriteNew(Path.Combine(directory, "comparison.json"), comparison);
    Console.WriteLine(JsonSerializer.Serialize(new { status, receipt = Path.Combine(directory, "comparison.json"), qualityVerdict = "UNKNOWN_NOT_GRADED" }));
}

static int DiagnosticArgmax(IReadOnlyList<float> logits)
{
    // TensorOps.Argmax сохраняет первый индекс при равных максимумах.
    int best = 0;
    for (int i = 1; i < logits.Count; i++) if (logits[i] > logits[best]) best = i;
    return best;
}
static object DiagnosticTop(IReadOnlyList<float> logits)
{
    var ids = Enumerable.Range(0, logits.Count).OrderByDescending(i => logits[i]).ThenBy(i => i).Take(5).ToArray();
    return new { tokenIds = ids, logits = ids.Select(i => logits[i]).ToArray(), top1Top2Gap = (double)logits[ids[0]] - logits[ids[1]] };
}
static double[] DiagnosticLogSoftmax(IReadOnlyList<float> logits)
{
    double max = logits.Max();
    var shifted = logits.Select(value => (double)value - max).ToArray();
    double normalizer = Math.Log(shifted.Sum(Math.Exp));
    return shifted.Select(value => value - normalizer).ToArray();
}
Dictionary<string, string> DiagnosticSourcePins() => new[] {
    "09.Inference/TextGenerator.cs", "09.Inference/Sampler.cs", "09.Inference/KvCache.cs", "03.TransformerModel/LlmModel.cs",
    "00.TensorCore/Core/TensorOps.cs", "14.ContinuousLearning/Training/SimulationEnRuContinuation.cs", "tools/Rp01SimulationDpo/Program.cs"
}.ToDictionary(path => path, path => HashFile(Path.Combine(Root, path)), StringComparer.Ordinal);
static Dictionary<string, string> DiagnosticRuntimePins()
{
    // Фиксируем также shared framework, а не только собственные сборки приложения.
    return new[] { AppContext.BaseDirectory, Path.GetDirectoryName(typeof(object).Assembly.Location)! }
        .SelectMany(Directory.EnumerateFiles).Where(path => path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
        path.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".runtimeconfig.json", StringComparison.OrdinalIgnoreCase))
        .Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.Ordinal).ToDictionary(path => Path.GetFullPath(path), HashFile, StringComparer.Ordinal);
}
static object DiagnosticProcessObservation()
{
    using var process = System.Diagnostics.Process.GetCurrentProcess(); process.Refresh();
    return new { pid = Environment.ProcessId, workingSetBytes = process.WorkingSet64, privateMemoryBytes = process.PrivateMemorySize64,
        peakWorkingSetBytes = process.PeakWorkingSet64, gcTotalAvailableMemoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
        configuredMaximumWorkingSetBytes = OperatingSystem.IsWindows() ? (long?)process.MaxWorkingSet : null,
        memoryScope = "Own process and runtime-reported GC limit; working-set configuration is not a hard process memory limit. No diagnostic RAM floor or timeout.",
        framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription, architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
        operatingSystem = System.Runtime.InteropServices.RuntimeInformation.OSDescription, logicalProcessors = Environment.ProcessorCount, buildConfiguration = BuildConfiguration() };
}

CodeDocumentationGenerationOptions ReadConfiguration(CancellationToken cancellation)
{
    var verified = CodeDocumentationGenerationConfiguration.ReadVerified(name => name switch // Pass this invocation's immutable selection; never mutate global environment or configuration.
    { "XIIGEN_CODE_DOCUMENTATION_GENERATION_ENABLED" => "true", "XIIGEN_CODE_DOCUMENTATION_GENERATION_CONFIG_SHA256" => ConfigHash, _ => null }, Root, cancellation); // Existing ACL/custody and tokenizer validation remain active.
    if (!verified.IsSuccess || verified.Value is null) throw new InvalidOperationException(verified.ErrorCode); // A bad config cannot cause random initialization fallback.
    return verified.Value;
}

SimulationDpoModelContext LoadModels(CancellationToken cancellation) // The entire callback is timed as load/materialization by the learning owner.
{ // The production reader verifies config/tokenizer bytes and denies cold start.
    var options = ReadConfiguration(cancellation);
    CodeDocumentationRepositoryOwnedModelCheckpointStart Load() // Use the same canonical saved-root loader for policy, independent reference and fresh readback.
    { // Use normal saved-checkpoint START; the exact Step1/hash guards below refuse any descendant or replacement.
        var loaded = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(options.StartRequest, cancellation); // Existing full keyed base+LoRA schema is validated and imported here; cold start remains denied.
        if (!loaded.IsSuccess || loaded.Value is null) throw new InvalidOperationException(loaded.ErrorCode); // No fresh/random fallback is authorized.
        var parent = loaded.Value.ParentManifest; // Verify the saved root selected by the loader, not a guessed path.
        if (parent is null || parent.Step != 1 || parent.ParentCheckpoint is not null || parent.ManifestSha256 != B0ManifestHash || parent.WeightsSha256 != B0Hash || // Keep old canonical B0 immutable.
            parent.DpoApplication != RepositoryOwnedModelCheckpointDpoApplication.InitialBaselineNoUpdate || loaded.Value.Model.GetCheckpointParameterEntries().Count != 55 || loaded.Value.Model.BaseParameters.Count != 39) // Bind the actual complete topology, including 16 q/v adapter tensors.
            throw new InvalidOperationException("Verified initial saved-root identity/topology mismatch."); // No artifact name is treated as proof by itself.
        return loaded.Value; // Caller receives independently allocated loaded model state.
    } // Each call imports all saved tensors through the existing service.
    var policy = Load(); var reference = Load(); // Both models begin from identical accepted B0 bytes with disjoint storage.
    var weights = Path.GetFullPath(Path.Combine(Root, policy.ParentManifest!.RepositoryRelativeWeightsPath)); // Filename and path come from the actual verified manifest.
    if (HashFile(weights) != B0Hash) throw new InvalidOperationException("Saved B0 changed after canonical import."); // Detect source mutation before training admission.
    return new(policy.Model, reference.Model, options.Tokenizer, options.StartRequest.LoraConfig!, options.StartRequest.AdapterSeedAuthority!, weights, B0Hash, token => // Preserve the original profile and complete topology.
    { token.ThrowIfCancellationRequested(); return Load().Model; }); // Fresh readback does not create a different baseline or optimizer.
} // Existing canonical source/pointers are never written here.
static string HashFile(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); } // Hash actual source bytes independently of manifest claims.
static double? ParseMainWallBudget(string value) => value == "none" ? null : double.Parse(value, CultureInfo.InvariantCulture); // Existing core validation refuses zero, negative and nonfinite numeric budgets.
static string BuildConfiguration() // Report actual compilation mode for cost interpretation.
{ // No global runtime or compiler setting is changed.
#if DEBUG
    return "Debug"; // An accidental debug calibration remains visible.
#else
    return "Release"; // The requested benchmark build is compiled with normal project Release settings.
#endif
} // This host contains no trainer, tokenizer, grader, external model or acceptance policy.
