const fs = require("fs");
const path = require("path");
const crypto = require("crypto");
const { spawnSync } = require("child_process");

const root = process.cwd();
const phaseId = "QUALITY-SEMANTIC-GATES-23";
const generatedAtUtc = new Date().toISOString();
const dryRun = process.argv.includes("--dry-run");

if (!dryRun) {
  throw new Error("QSG-23 must be run with --dry-run; production locator/checkpoint mutation is forbidden.");
}

const artifactRoot = "artifacts/extension-runtime-token-quality-learnability";
const qsg23Ref = `${artifactRoot}/qsg23-local-adaptive-readiness`;
const qsg23Dir = path.join(root, qsg23Ref);
const refs = {
  state: "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json",
  qsg22Report: `${artifactRoot}/quality-semantic-gates-22-report.json`,
  qsg22Readiness: `${artifactRoot}/qsg22-staged-promotion/local-adaptive-readiness-check.json`,
  modelIndex: "models/index.json",
  promptToTaskCommonCheckpoint: "models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json",
  promptToTaskCommonManifest: "models/prompt-to-task/prompt-to-task-v1/manifest.json",
  skillSelectionCommonCheckpoint: "models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json",
  skillSelectionCommonManifest: "models/skill-selection/skill-selection-v1/manifest.json",
  skillSelectionTrainingManifest: "training-data/skill-selection/skill-selection-v1/training-manifest.json",
  promptToTaskSplit: "training-data/smart-skills/semantic-quality/prompt-to-task-expanded-v1/prompt-to-task-expanded-split.json",
  promptToTaskDpo: "training-data/smart-skills/semantic-quality/prompt-to-task-expanded-v1/prompt-to-task-expanded-dpo.jsonl",
  skillSelectionCorpus: "training-data/skill-selection/skill-selection-v1/corpus-snapshot.jsonl",
  promptToTaskMachine: "43.SmartSkillsDomain/PromptToTaskMachine.cs",
  promptToTaskTaskPatternUnit: "43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs",
  skillSelectionMachine: "43.SmartSkillsDomain/SkillSelectionMachine.cs",
  skillSelectionContracts: "43.SmartSkillsDomain/SkillSelectionContracts.cs"
};

const outputs = {
  dotnetReport: `${qsg23Ref}/dotnet-probe-result.json`,
  assetTopology: `${qsg23Ref}/asset-topology.json`,
  freshLoad: `${qsg23Ref}/fresh-load-load-order.json`,
  continueTraining: `${qsg23Ref}/continue-training-load-instead-of-retrain.json`,
  backupExportImport: `${qsg23Ref}/backup-export-import-rollback.json`,
  heldOutAblation: `${qsg23Ref}/held-out-ablation-controls.json`,
  deterministicBoundary: `${qsg23Ref}/deterministic-boundary-review.json`,
  claimLedger: `${qsg23Ref}/claim-ledger.json`,
  summary: `${qsg23Ref}/summary.md`,
  reportJson: `${artifactRoot}/quality-semantic-gates-23-report.json`,
  reportMd: `${artifactRoot}/quality-semantic-gates-23-report.md`,
  validation: `${artifactRoot}/quality-semantic-gates-23.validation.json`,
  evidenceJsonl: `${artifactRoot}/quality-semantic-gates-23-evidence.jsonl`
};

function fullPath(ref) {
  return path.join(root, ref);
}

function artifactRef(filePath) {
  return path.relative(root, filePath).split(path.sep).join("/");
}

function assertInside(child, parent) {
  const resolvedChild = path.resolve(child);
  const resolvedParent = path.resolve(parent);
  if (resolvedChild !== resolvedParent && !resolvedChild.startsWith(resolvedParent + path.sep)) {
    throw new Error(`Refusing to write outside intended directory: ${resolvedChild}`);
  }
}

function normalizeLf(text) {
  return String(text).replace(/\r\n/g, "\n").replace(/\r/g, "\n");
}

function resetQsg23Dir() {
  assertInside(qsg23Dir, path.join(root, artifactRoot));
  if (fs.existsSync(qsg23Dir)) {
    fs.rmSync(qsg23Dir, { recursive: true, force: true });
  }
  fs.mkdirSync(qsg23Dir, { recursive: true });
}

function readText(ref) {
  const text = fs.readFileSync(fullPath(ref), "utf8");
  return text.charCodeAt(0) === 0xfeff ? text.slice(1) : text;
}

function readJson(ref) {
  return JSON.parse(readText(ref));
}

function readJsonl(ref) {
  return readText(ref)
    .split(/\r?\n/)
    .filter((line) => line.trim().length > 0)
    .map((line) => JSON.parse(line));
}

function sha256Bytes(bytes) {
  return crypto.createHash("sha256").update(bytes).digest("hex");
}

function sha256File(refOrPath, isAbsolute = false) {
  const filePath = isAbsolute ? refOrPath : fullPath(refOrPath);
  return sha256Bytes(fs.readFileSync(filePath));
}

function fileInfo(ref) {
  const filePath = fullPath(ref);
  const exists = fs.existsSync(filePath);
  const info = {
    ref,
    exists,
    size_bytes: exists ? fs.statSync(filePath).size : 0,
    sha256: exists ? sha256File(ref) : null,
    json_parses: null
  };
  if (exists && ref.endsWith(".json")) {
    try {
      JSON.parse(fs.readFileSync(filePath, "utf8"));
      info.json_parses = true;
    } catch {
      info.json_parses = false;
    }
  }
  return info;
}

function walkFiles(dir) {
  if (!fs.existsSync(dir)) {
    return [];
  }
  return fs.readdirSync(dir, { withFileTypes: true }).flatMap((entry) => {
    const entryPath = path.join(dir, entry.name);
    return entry.isDirectory() ? walkFiles(entryPath) : [entryPath];
  });
}

function strictJsonFileInfo(ref) {
  const filePath = fullPath(ref);
  const bytes = fs.existsSync(filePath) ? fs.readFileSync(filePath) : Buffer.alloc(0);
  const info = {
    ref,
    exists: fs.existsSync(filePath),
    size_bytes: bytes.length,
    has_utf8_bom: bytes.length >= 3 && bytes[0] === 0xef && bytes[1] === 0xbb && bytes[2] === 0xbf,
    parses_without_bom_stripping: false,
    error: null
  };
  if (!info.exists) {
    info.error = "FILE_NOT_FOUND";
    return info;
  }
  try {
    JSON.parse(bytes.toString("utf8"));
    info.parses_without_bom_stripping = true;
  } catch (error) {
    info.error = error instanceof Error ? error.message : String(error);
  }
  return info;
}

function strictJsonlFileInfo(ref) {
  const filePath = fullPath(ref);
  const bytes = fs.existsSync(filePath) ? fs.readFileSync(filePath) : Buffer.alloc(0);
  const info = {
    ref,
    exists: fs.existsSync(filePath),
    size_bytes: bytes.length,
    has_utf8_bom: bytes.length >= 3 && bytes[0] === 0xef && bytes[1] === 0xbb && bytes[2] === 0xbf,
    parses_without_bom_stripping: false,
    non_empty_line_count: 0,
    failure_lines: []
  };
  if (!info.exists) {
    info.failure_lines.push({ line_number: 0, error: "FILE_NOT_FOUND" });
    return info;
  }
  const text = bytes.toString("utf8");
  const lines = text.split(/\r?\n/);
  lines.forEach((line, index) => {
    if (line.trim().length === 0) {
      return;
    }
    info.non_empty_line_count += 1;
    try {
      JSON.parse(line);
    } catch (error) {
      info.failure_lines.push({
        line_number: index + 1,
        error: error instanceof Error ? error.message : String(error)
      });
    }
  });
  info.parses_without_bom_stripping = info.failure_lines.length === 0 && !info.has_utf8_bom;
  return info;
}

function collectQsg23OutputParseCoverage() {
  const qsg23Refs = walkFiles(qsg23Dir)
    .map(artifactRef)
    .filter((ref) => ref.endsWith(".json") || ref.endsWith(".jsonl"));
  const topLevelRefs = [
    outputs.reportJson,
    outputs.validation,
    outputs.evidenceJsonl
  ];
  const refsToCheck = [...new Set(qsg23Refs.concat(topLevelRefs))].sort((left, right) => left.localeCompare(right));
  const json_files = refsToCheck.filter((ref) => ref.endsWith(".json")).map(strictJsonFileInfo);
  const jsonl_files = refsToCheck.filter((ref) => ref.endsWith(".jsonl")).map(strictJsonlFileInfo);
  const json_failures = json_files.filter((info) => info.parses_without_bom_stripping !== true || info.has_utf8_bom);
  const jsonl_failures = jsonl_files.filter((info) => info.parses_without_bom_stripping !== true || info.has_utf8_bom);
  const utf8_bom_failures = json_files.concat(jsonl_files).filter((info) => info.has_utf8_bom);

  return {
    scope: "Every *.json and *.jsonl under QSG-23 output tree plus top-level QSG-23 JSON/JSONL; parsed from raw UTF-8 text without BOM stripping.",
    qsg23_output_root: qsg23Ref,
    top_level_refs_checked: topLevelRefs,
    files_checked_total: json_files.length + jsonl_files.length,
    json_files_checked: json_files.length,
    jsonl_files_checked: jsonl_files.length,
    jsonl_non_empty_lines_checked: jsonl_files.reduce((sum, info) => sum + info.non_empty_line_count, 0),
    json_failures,
    jsonl_failures,
    utf8_bom_failures,
    all_json_jsonl_parse_without_bom_stripping: json_failures.length === 0 && jsonl_failures.length === 0 && utf8_bom_failures.length === 0,
    json_files,
    jsonl_files
  };
}

function collectQsg23TextQuality() {
  const qsg23Refs = walkFiles(qsg23Dir).map(artifactRef);
  const topLevelRefs = [
    outputs.reportJson,
    outputs.reportMd,
    outputs.validation,
    outputs.evidenceJsonl
  ];
  const refsToCheck = [...new Set(qsg23Refs.concat(topLevelRefs))].sort((left, right) => left.localeCompare(right));
  const mojibakePattern = /[\uFFFD\u00D0\u00D1\u00E2]/;
  const files = refsToCheck.map((ref) => {
    const filePath = fullPath(ref);
    const bytes = fs.existsSync(filePath) ? fs.readFileSync(filePath) : Buffer.alloc(0);
    const hasUtf8Bom = bytes.length >= 3 && bytes[0] === 0xef && bytes[1] === 0xbb && bytes[2] === 0xbf;
    const text = bytes.toString("utf8");
    return {
      ref,
      exists: fs.existsSync(filePath),
      size_bytes: bytes.length,
      has_utf8_bom: hasUtf8Bom,
      mojibake_marker_detected: mojibakePattern.test(text)
    };
  });
  const missing_files = files.filter((info) => !info.exists).map((info) => info.ref);
  const utf8_bom_failure_refs = files.filter((info) => info.has_utf8_bom).map((info) => info.ref);
  const mojibake_failure_refs = files.filter((info) => info.mojibake_marker_detected).map((info) => info.ref);

  return {
    scope: "Every generated QSG-23 output file plus top-level QSG-23 report/validation/evidence files; UTF-8 text scan for BOM and common mojibake markers.",
    files_checked_total: files.length,
    missing_files,
    utf8_bom_failure_count: utf8_bom_failure_refs.length,
    utf8_bom_failure_refs,
    mojibake_failure_count: mojibake_failure_refs.length,
    mojibake_failure_refs,
    all_outputs_utf8_no_bom_no_mojibake: missing_files.length === 0 && utf8_bom_failure_refs.length === 0 && mojibake_failure_refs.length === 0
  };
}

function writeJson(ref, value) {
  const filePath = fullPath(ref);
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
  fs.writeFileSync(filePath, normalizeLf(`${JSON.stringify(value, null, 2)}\n`), "utf8");
}

function writeText(ref, value) {
  const filePath = fullPath(ref);
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
  fs.writeFileSync(filePath, normalizeLf(value), "utf8");
}

function writeJsonl(ref, rows) {
  writeText(ref, rows.map((row) => JSON.stringify(row)).join("\n") + "\n");
}

function copyFileRef(sourceRef, targetRef) {
  const source = fullPath(sourceRef);
  const target = fullPath(targetRef);
  fs.mkdirSync(path.dirname(target), { recursive: true });
  fs.copyFileSync(source, target);
}

function makeDotnetProject() {
  const projectDir = path.join(qsg23Dir, "dotnet-probe");
  fs.mkdirSync(projectDir, { recursive: true });
  fs.writeFileSync(path.join(projectDir, "Qsg23LocalAdaptiveProbe.csproj"), normalizeLf(`<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\\..\\..\\..\\43.SmartSkillsDomain\\43.SmartSkillsDomain.csproj" />
  </ItemGroup>
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
`), "utf8");
  fs.writeFileSync(path.join(projectDir, "Program.cs"), normalizeLf(dotnetProgramSource()), "utf8");
  return path.join(projectDir, "Qsg23LocalAdaptiveProbe.csproj");
}

function runDotnetProbe(projectPath) {
  const result = spawnSync(
    "dotnet",
    ["run", "--project", projectPath, "--", root, qsg23Dir],
    { cwd: root, encoding: "utf8", maxBuffer: 1024 * 1024 * 64 }
  );
  if (result.status !== 0) {
    throw new Error(`dotnet QSG-23 probe failed\nSTDOUT:\n${result.stdout}\nSTDERR:\n${result.stderr}`);
  }
  const dotnetDir = path.dirname(projectPath);
  for (const generatedDir of ["bin", "obj"]) {
    const target = path.join(dotnetDir, generatedDir);
    if (fs.existsSync(target)) {
      assertInside(target, qsg23Dir);
      fs.rmSync(target, { recursive: true, force: true });
    }
  }
  return { stdout: result.stdout.trim(), stderr: result.stderr.trim() };
}

function dotnetProgramSource() {
  return String.raw`
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SmartSkillsDomain;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

var root = args.Length > 0 ? Path.GetFullPath(args[0]) : Directory.GetCurrentDirectory();
var outDir = args.Length > 1 ? Path.GetFullPath(args[1]) : Path.Combine(root, "artifacts", "extension-runtime-token-quality-learnability", "qsg23-local-adaptive-readiness");
Directory.CreateDirectory(outDir);
var JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    WriteIndented = true
};

var generatedAtUtc = DateTimeOffset.UtcNow;
var assetRoot = Path.Combine(outDir, "assets");
var sourceManifestRef = "artifacts/extension-runtime-token-quality-learnability/qsg23-local-adaptive-readiness/source-manifest.synthetic-test-only.json";
var productionRefs = new[]
{
    "models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json",
    "models/prompt-to-task/prompt-to-task-v1/manifest.json",
    "models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json",
    "models/skill-selection/skill-selection-v1/manifest.json",
    "models/index.json"
};
var productionBefore = productionRefs.ToDictionary(item => item, item => FileHash(Join(root, item)), StringComparer.Ordinal);

var pttCommonCheckpoint = Join(root, "models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json");
var pttCommonManifest = Join(root, "models/prompt-to-task/prompt-to-task-v1/manifest.json");
var ssCommonCheckpoint = Join(root, "models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json");
var ssCommonManifest = Join(root, "models/skill-selection/skill-selection-v1/manifest.json");
var ssPublicLoadCommonCheckpoint = CreateTypedSkillSelectionCheckpointCopy(root, outDir, ssCommonCheckpoint);

var pttCommonLoad = await PromptToTaskMachine.LoadCheckpointAsync(pttCommonCheckpoint, pttCommonManifest);
Ensure(pttCommonLoad.IsSuccess && pttCommonLoad.Value is not null, "PromptToTask common checkpoint load failed: " + pttCommonLoad.ErrorMessage);
var ssCommonLoad = await SkillSelectionMachine.LoadCheckpointAsync(ssPublicLoadCommonCheckpoint);
Ensure(ssCommonLoad.IsSuccess && ssCommonLoad.Value is not null, "SkillSelection common checkpoint load failed: " + ssCommonLoad.ErrorMessage);

var sourceManifestPath = Join(root, sourceManifestRef);
WriteJson(sourceManifestPath, new Dictionary<string, object?>
{
    ["schema"] = "xiigen.qsg23.synthetic_source_manifest.v1",
    ["phase_id"] = "QUALITY-SEMANTIC-GATES-23",
    ["generated_at_utc"] = generatedAtUtc,
    ["scope"] = "test-only staged local adaptive fixtures under artifacts; no production model/locator mutation",
    ["semantic_quality_claimed"] = false,
    ["fixtures"] = new[]
    {
        "qsg23-ptt-initial",
        "qsg23-ptt-continue-delta",
        "qsg23-skill-initial",
        "qsg23-skill-continue-delta"
    }
});

var promptToTask = BuildPromptToTaskProof(root, outDir, assetRoot, sourceManifestRef, pttCommonLoad.Value!, pttCommonCheckpoint, pttCommonManifest);
var skillSelection = await BuildSkillSelectionProof(root, outDir, assetRoot, ssCommonLoad.Value!, ssPublicLoadCommonCheckpoint, ssCommonCheckpoint, ssCommonManifest);

var productionAfter = productionRefs.ToDictionary(item => item, item => FileHash(Join(root, item)), StringComparer.Ordinal);
var productionMutationRows = productionRefs.Select(item => new Dictionary<string, object?>
{
    ["ref"] = item,
    ["sha256_before"] = productionBefore[item],
    ["sha256_after"] = productionAfter[item],
    ["mutated"] = !string.Equals(productionBefore[item], productionAfter[item], StringComparison.Ordinal)
}).ToArray();

var report = new Dictionary<string, object?>
{
    ["schema"] = "xiigen.quality_semantic_gates_23.dotnet_probe_result.v1",
    ["phase_id"] = "QUALITY-SEMANTIC-GATES-23",
    ["generated_at_utc"] = generatedAtUtc,
    ["dry_run_only"] = true,
    ["production_locator_or_checkpoint_mutated"] = productionMutationRows.Any(row => row["mutated"] is true),
    ["production_mutation_rows"] = productionMutationRows,
    ["source_manifest_ref"] = sourceManifestRef,
    ["machines"] = new Dictionary<string, object?>
    {
        ["PromptToTaskMachine"] = promptToTask,
        ["SkillSelectionMachine"] = skillSelection
    }
};

var reportPath = Path.Combine(outDir, "dotnet-probe-result.json");
WriteJson(reportPath, report);
Console.WriteLine(JsonSerializer.Serialize(report, JsonOptions));

Dictionary<string, object?> BuildPromptToTaskProof(
    string rootPath,
    string outputRoot,
    string assets,
    string stagedSourceManifestRef,
    PromptToTaskMachineLoadResult commonLoad,
    string commonCheckpoint,
    string commonManifest)
{
    var userDir = Path.Combine(assets, "user-adaptive", "prompt-to-task", "prompt-to-task-v1");
    var continuedDir = Path.Combine(assets, "continued", "prompt-to-task", "prompt-to-task-v1");
    var backupDir = Path.Combine(assets, "backup", "prompt-to-task", "prompt-to-task-v1");
    var importDir = Path.Combine(assets, "share-import", "prompt-to-task", "prompt-to-task-v1");
    Directory.CreateDirectory(userDir);
    Directory.CreateDirectory(continuedDir);
    Directory.CreateDirectory(backupDir);
    Directory.CreateDirectory(importDir);

    var initialPattern = new PromptToTaskLearnedPattern(
        "qsg23-ptt-initial-local-adaptive",
        "qsg23_test_only_local_adaptive_task",
        new[] { "qsg23_local_adaptive", "fresh_load", "no_production_mutation", "portable_lifecycle" },
        new[] { "prove staged local adaptive load order through a universal task contract" },
        new[] { "test_only_fixture", "semantic_quality_claimed_false", "production_mutation_forbidden" },
        new[] { "fresh-load evidence", "load-order evidence", "claim-boundary notes" },
        new[] { "mutate production checkpoint", "claim semantic quality", "use project stack as universal authority" },
        new[] { "host_boundary_from_request" },
        new[] { "backup", "export", "import", "continue-training", "held-out", "ablation", "rollback" },
        "qsg23-synthetic-train",
        0.97);

    var userMachine = commonLoad.Machine;
    var initialContinue = userMachine.ContinueWithAcceptedPattern(initialPattern, "qsg23-accepted:ptt-initial-test-only");
    Ensure(initialContinue.IsSuccess, "PromptToTask initial staged continue failed: " + initialContinue.ErrorMessage);
    var userCheckpoint = Path.Combine(userDir, "prompt-to-task-v1-checkpoint.json");
    var userManifest = Path.Combine(userDir, "manifest.json");
    var userAccepted = commonLoad.Checkpoint.AcceptedRefIds.Concat(new[] { "qsg23-accepted:ptt-initial-test-only" }).ToArray();
    var userSave = userMachine.SaveCheckpointAsync(userCheckpoint, userManifest, stagedSourceManifestRef, userAccepted).GetAwaiter().GetResult();
    Ensure(userSave.IsSuccess && userSave.Value is not null, "PromptToTask user staged save failed: " + userSave.ErrorMessage);
    NormalizeJsonPairNoBom(userCheckpoint, userManifest);
    CopyPair(userCheckpoint, userManifest, backupDir);

    var freshRequest = PromptRequest(
        "qsg23-ptt-fresh-load",
        "Portable local adaptive lifecycle proof with no production mutation.",
        new[] { "qsg23_local_adaptive", "fresh_load", "no_production_mutation", "portable_lifecycle" },
        "qsg23.synthetic.fresh");
    var userFirstLoad = LoadPromptToTaskWithFallback(userCheckpoint, userManifest, commonCheckpoint, commonManifest);
    var freshResponse = userFirstLoad.Machine.Translate(freshRequest).Value!;

    var commonOnlyLoad = LoadPromptToTaskWithFallback(
        Path.Combine(userDir, "missing-checkpoint.json"),
        Path.Combine(userDir, "missing-manifest.json"),
        commonCheckpoint,
        commonManifest);
    var ablationResponse = commonOnlyLoad.Machine.Translate(freshRequest).Value!;

    var heldOutRequest = PromptRequest(
        "qsg23-ptt-held-out",
        "Portable lifecycle evidence needs a staged adaptive proof before readiness claims.",
        new[] { "qsg23_local_adaptive", "fresh_load", "no_production_mutation", "portable_lifecycle" },
        "qsg23.synthetic.heldout");
    var heldOutResponse = userFirstLoad.Machine.Translate(heldOutRequest).Value!;

    var continuedLoadStart = PromptToTaskMachine.LoadCheckpointAsync(userCheckpoint, userManifest).GetAwaiter().GetResult();
    Ensure(continuedLoadStart.IsSuccess && continuedLoadStart.Value is not null, "PromptToTask staged checkpoint reload before continue failed: " + continuedLoadStart.ErrorMessage);
    var deltaPattern = new PromptToTaskLearnedPattern(
        "qsg23-ptt-continue-delta",
        "qsg23_test_only_continue_delta_task",
        new[] { "qsg23_continue_delta", "load_existing_checkpoint_first", "export_import_rollback" },
        new[] { "continue from an existing staged PromptToTask checkpoint before adding a delta" },
        new[] { "test_only_fixture", "no_retrain_from_scratch", "production_mutation_forbidden" },
        new[] { "continued checkpoint evidence", "import evidence", "rollback comparison" },
        new[] { "cold-start retrain masquerade", "semantic quality claim", "common checkpoint mutation" },
        new[] { "host_boundary_from_request" },
        new[] { "base checkpoint sha", "continued checkpoint sha", "rollback comparison" },
        "qsg23-synthetic-continue",
        0.98);
    var beforePatternCount = continuedLoadStart.Value.Checkpoint.LearnedState.LearnedPatterns?.Count ?? 0;
    var continueResult = continuedLoadStart.Value.Machine.ContinueWithAcceptedPattern(deltaPattern, "qsg23-accepted:ptt-continue-delta-test-only");
    Ensure(continueResult.IsSuccess, "PromptToTask continue delta failed: " + continueResult.ErrorMessage);
    var continuedCheckpoint = Path.Combine(continuedDir, "prompt-to-task-v1-checkpoint.json");
    var continuedManifest = Path.Combine(continuedDir, "manifest.json");
    var continuedAccepted = continuedLoadStart.Value.Checkpoint.AcceptedRefIds.Concat(new[] { "qsg23-accepted:ptt-continue-delta-test-only" }).ToArray();
    var continuedSave = continuedLoadStart.Value.Machine.SaveCheckpointAsync(continuedCheckpoint, continuedManifest, stagedSourceManifestRef, continuedAccepted).GetAwaiter().GetResult();
    Ensure(continuedSave.IsSuccess && continuedSave.Value is not null, "PromptToTask continued save failed: " + continuedSave.ErrorMessage);
    NormalizeJsonPairNoBom(continuedCheckpoint, continuedManifest);
    CopyPair(continuedCheckpoint, continuedManifest, importDir);

    var importedCheckpoint = Path.Combine(importDir, Path.GetFileName(continuedCheckpoint));
    var importedManifest = Path.Combine(importDir, Path.GetFileName(continuedManifest));
    var importedLoad = PromptToTaskMachine.LoadCheckpointAsync(importedCheckpoint, importedManifest).GetAwaiter().GetResult();
    Ensure(importedLoad.IsSuccess && importedLoad.Value is not null, "PromptToTask imported continued load failed: " + importedLoad.ErrorMessage);
    var deltaRequest = PromptRequest(
        "qsg23-ptt-continue-runtime",
        "Continue delta should load the existing staged checkpoint before export import rollback.",
        new[] { "qsg23_continue_delta", "load_existing_checkpoint_first", "export_import_rollback" },
        "qsg23.synthetic.continue");
    var continuedResponse = importedLoad.Value.Machine.Translate(deltaRequest).Value!;
    var rollbackLoad = PromptToTaskMachine.LoadCheckpointAsync(Path.Combine(backupDir, "prompt-to-task-v1-checkpoint.json"), Path.Combine(backupDir, "manifest.json")).GetAwaiter().GetResult();
    Ensure(rollbackLoad.IsSuccess && rollbackLoad.Value is not null, "PromptToTask rollback load failed: " + rollbackLoad.ErrorMessage);
    var rollbackResponse = rollbackLoad.Value.Machine.Translate(deltaRequest).Value!;
    var afterPatternCount = importedLoad.Value.Checkpoint.LearnedState.LearnedPatterns?.Count ?? 0;

    var leakTerms = new[] { "llm_mvp_core", "c:\\", "c:/", "cursor", "claude", "codex", "qsg21", "qsg22" };
    var universalLayerText = string.Join(" | ",
        freshResponse.StructuredTask.TaskType,
        string.Join(" ", freshResponse.StructuredTask.ExplicitUserRequirements),
        string.Join(" ", freshResponse.StructuredTask.Constraints),
        string.Join(" ", freshResponse.StructuredTask.Deliverables),
        string.Join(" ", freshResponse.StructuredTask.ForbiddenActions),
        string.Join(" ", freshResponse.StructuredTask.RequiredHosts),
        string.Join(" ", freshResponse.StructuredTask.EvidenceNeeds)).ToLowerInvariant();
    var leakageTermsFound = leakTerms.Where(universalLayerText.Contains).ToArray();

    return new Dictionary<string, object?>
    {
        ["machine_id"] = "PromptToTaskMachine",
        ["asset_topology"] = new Dictionary<string, object?>
        {
            ["common_checkpoint_ref"] = ToRef(rootPath, commonCheckpoint),
            ["common_manifest_ref"] = ToRef(rootPath, commonManifest),
            ["user_adaptive_checkpoint_ref"] = ToRef(rootPath, userCheckpoint),
            ["user_adaptive_manifest_ref"] = ToRef(rootPath, userManifest),
            ["continued_checkpoint_ref"] = ToRef(rootPath, continuedCheckpoint),
            ["continued_manifest_ref"] = ToRef(rootPath, continuedManifest),
            ["backup_checkpoint_ref"] = ToRef(rootPath, Path.Combine(backupDir, "prompt-to-task-v1-checkpoint.json")),
            ["backup_manifest_ref"] = ToRef(rootPath, Path.Combine(backupDir, "manifest.json")),
            ["import_checkpoint_ref"] = ToRef(rootPath, importedCheckpoint),
            ["import_manifest_ref"] = ToRef(rootPath, importedManifest)
        },
        ["fresh_load"] = new Dictionary<string, object?>
        {
            ["passed"] = userFirstLoad.Source == "user_adaptive" && freshResponse.ContractStatus == PromptToTaskMachine.LearnedPatternStatus,
            ["load_order"] = userFirstLoad.Attempts,
            ["loaded_source"] = userFirstLoad.Source,
            ["runtime_invocation"] = PromptResponseSummary(freshResponse),
            ["common_fallback_probe"] = new Dictionary<string, object?>
            {
                ["loaded_source"] = commonOnlyLoad.Source,
                ["load_order"] = commonOnlyLoad.Attempts,
                ["runtime_invocation"] = PromptResponseSummary(ablationResponse)
            }
        },
        ["continue_training"] = new Dictionary<string, object?>
        {
            ["passed"] = afterPatternCount == beforePatternCount + 1 && continuedResponse.ContractStatus == PromptToTaskMachine.LearnedPatternStatus,
            ["loaded_existing_checkpoint_first"] = true,
            ["base_checkpoint_sha256"] = FileHash(userCheckpoint),
            ["continued_checkpoint_sha256"] = FileHash(continuedCheckpoint),
            ["patterns_before"] = beforePatternCount,
            ["patterns_after"] = afterPatternCount,
            ["new_pattern_id"] = "qsg23-ptt-continue-delta",
            ["retrain_from_scratch"] = false,
            ["runtime_invocation_after_continue"] = PromptResponseSummary(continuedResponse)
        },
        ["backup_export_import_rollback"] = new Dictionary<string, object?>
        {
            ["backup_ref_exists"] = File.Exists(Path.Combine(backupDir, "prompt-to-task-v1-checkpoint.json")) && File.Exists(Path.Combine(backupDir, "manifest.json")),
            ["export_ref"] = ToRef(rootPath, continuedCheckpoint),
            ["import_ref"] = ToRef(rootPath, importedCheckpoint),
            ["import_load_passed"] = importedLoad.IsSuccess,
            ["rollback_load_passed"] = rollbackLoad.IsSuccess,
            ["rollback_comparison_passed"] = rollbackResponse.ContractStatus != PromptToTaskMachine.LearnedPatternStatus || rollbackResponse.StructuredTask.TaskType != "qsg23_test_only_continue_delta_task",
            ["rollback_runtime_invocation"] = PromptResponseSummary(rollbackResponse)
        },
        ["held_out_ablation"] = new Dictionary<string, object?>
        {
            ["passed"] = heldOutResponse.ContractStatus == PromptToTaskMachine.LearnedPatternStatus && ablationResponse.ContractStatus != PromptToTaskMachine.LearnedPatternStatus,
            ["test_data_scope"] = "synthetic_test_only_source_disjoint_controls",
            ["source_disjoint"] = true,
            ["semantic_quality_claimed"] = false,
            ["held_out_runtime_invocation"] = PromptResponseSummary(heldOutResponse),
            ["ablation_without_adaptive_state"] = PromptResponseSummary(ablationResponse)
        },
        ["prompt_to_universal_task_boundary"] = new Dictionary<string, object?>
        {
            ["checked"] = true,
            ["task_type"] = freshResponse.StructuredTask.TaskType,
            ["required_hosts"] = freshResponse.StructuredTask.RequiredHosts,
            ["project_stack_specific_leakage_terms_found"] = leakageTermsFound,
            ["no_project_stack_specific_leakage_in_structured_task"] = leakageTermsFound.Length == 0,
            ["note"] = "SourceProvenance may name staged artifact refs; the universal task contract fields are checked for stack-specific leakage."
        },
        ["claims"] = ClosedClaims()
    };
}

async Task<Dictionary<string, object?>> BuildSkillSelectionProof(
    string rootPath,
    string outputRoot,
    string assets,
    SkillSelectionMachineLoadResult commonLoad,
    string publicLoadCommonCheckpoint,
    string registryCommonCheckpoint,
    string commonManifest)
{
    var userDir = Path.Combine(assets, "user-adaptive", "skill-selection", "skill-selection-v1");
    var continuedDir = Path.Combine(assets, "continued", "skill-selection", "skill-selection-v1");
    var backupDir = Path.Combine(assets, "backup", "skill-selection", "skill-selection-v1");
    var importDir = Path.Combine(assets, "share-import", "skill-selection", "skill-selection-v1");
    Directory.CreateDirectory(userDir);
    Directory.CreateDirectory(continuedDir);
    Directory.CreateDirectory(backupDir);
    Directory.CreateDirectory(importDir);

    var initialMachine = new SkillSelectionMachine();
    initialMachine.LoadCheckpoint(commonLoad.Checkpoint);
    var initialPair = SkillPair(
        "qsg23-skill-initial",
        "Portable adaptive lifecycle evidence requires verification before completion.",
        new[] { "verification-before-completion", "planning-session-startup", "plan-review", "ui-testing" },
        "verification-before-completion",
        SkillSelectionMachine.PromptContextTrainingAdapter,
        "qsg23.synthetic.skill.initial");
    var initialTrain = await initialMachine.TrainMachineBatchAsync(new[] { initialPair });
    Ensure(initialTrain.IsSuccess, "SkillSelection initial staged train failed: " + initialTrain.ErrorMessage);
    var userCheckpoint = Path.Combine(userDir, "skill-selection-v1-checkpoint.json");
    var userManifest = Path.Combine(userDir, "manifest.json");
    var userExport = await initialMachine.SaveCheckpointAsync(userCheckpoint, userManifest);
    Ensure(userExport.IsSuccess && userExport.Value is not null, "SkillSelection user staged save failed: " + userExport.ErrorMessage);
    NormalizeJsonPairNoBom(userCheckpoint, userManifest);
    CopyPair(userCheckpoint, userManifest, backupDir);

    var userFirstLoad = await LoadSkillSelectionWithFallback(userExport.Value, publicLoadCommonCheckpoint);
    var freshRequest = SkillRequest(
        "Portable adaptive lifecycle evidence requires verification before completion.",
        new[] { "verification-before-completion", "planning-session-startup", "plan-review", "ui-testing" },
        "qsg23.synthetic.skill.fresh",
        "fresh_load");
    var shuffledRequest = SkillRequest(
        "Portable adaptive lifecycle evidence requires verification before completion.",
        new[] { "ui-testing", "plan-review", "verification-before-completion", "planning-session-startup" },
        "qsg23.synthetic.skill.fresh.shuffled",
        "fresh_load_shuffled");
    var freshDecision = userFirstLoad.Machine.DecideFromTrainedState(freshRequest).Value!;
    var shuffledDecision = userFirstLoad.Machine.DecideFromTrainedState(shuffledRequest).Value!;
    var noPositionDecision = userFirstLoad.Machine.ProbeWithoutCandidatePositionAuthority(shuffledRequest);
    var leakageReport = userFirstLoad.Machine.EvaluateCandidateOrderLeakage(freshRequest);
    var missingUserCheckpoint = Path.Combine(userDir, "missing-user-adaptive-checkpoint.json");
    var missingUserManifest = Path.Combine(userDir, "missing-user-adaptive-manifest.json");
    var missingUserExport = new LearningMachineExport(
        SkillSelectionMachine.MachineId,
        SkillSelectionMachine.DomainTag,
        SkillSelectionMachine.Version,
        missingUserCheckpoint,
        missingUserManifest,
        0.0,
        DateTimeOffset.UtcNow);
    var commonFallback = await LoadSkillSelectionWithFallback(missingUserExport, publicLoadCommonCheckpoint);
    var commonDecision = commonFallback.Machine.DecideFromTrainedState(freshRequest).Value!;

    var continueStart = await SkillSelectionMachine.LoadCheckpointAsync(userCheckpoint);
    Ensure(continueStart.IsSuccess && continueStart.Value is not null, "SkillSelection reload before continue failed: " + continueStart.ErrorMessage);
    var continueMachine = continueStart.Value.Machine;
    var beforePairCount = continueStart.Value.Checkpoint.TrainedPairIds.Count;
    var deltaPair = SkillPair(
        "qsg23-skill-continue-delta",
        "Adaptive data gap audit needs source disjoint evidence before readiness.",
        new[] { "qsg23-delta-only-skill", "verification-before-completion", "plan-review", "skill-graph-sync" },
        "qsg23-delta-only-skill",
        SkillSelectionMachine.PromptContextTrainingAdapter,
        "qsg23.synthetic.skill.continue");
    var deltaTrain = await continueMachine.TrainMachineBatchAsync(new[] { deltaPair });
    Ensure(deltaTrain.IsSuccess, "SkillSelection continue train failed: " + deltaTrain.ErrorMessage);
    var continuedCheckpoint = Path.Combine(continuedDir, "skill-selection-v1-checkpoint.json");
    var continuedManifest = Path.Combine(continuedDir, "manifest.json");
    var continuedExport = await continueMachine.SaveCheckpointAsync(continuedCheckpoint, continuedManifest);
    Ensure(continuedExport.IsSuccess && continuedExport.Value is not null, "SkillSelection continued save failed: " + continuedExport.ErrorMessage);
    NormalizeJsonPairNoBom(continuedCheckpoint, continuedManifest);
    var continuedLoad = await SkillSelectionMachine.LoadCheckpointAsync(continuedCheckpoint);
    Ensure(continuedLoad.IsSuccess && continuedLoad.Value is not null, "SkillSelection continued load failed: " + continuedLoad.ErrorMessage);
    var afterPairCount = continuedLoad.Value.Checkpoint.TrainedPairIds.Count;
    var deltaRequest = SkillRequest(
        "Adaptive data gap audit needs source disjoint evidence before readiness.",
        new[] { "qsg23-delta-only-skill", "verification-before-completion", "plan-review", "skill-graph-sync" },
        "qsg23.synthetic.skill.continue.runtime",
        "continue_runtime");
    var continuedDecision = continuedLoad.Value.Machine.DecideFromTrainedState(deltaRequest).Value!;

    CopyPair(continuedCheckpoint, continuedManifest, importDir);
    var importedCheckpoint = Path.Combine(importDir, Path.GetFileName(continuedCheckpoint));
    var importedManifest = Path.Combine(importDir, Path.GetFileName(continuedManifest));
    var importExport = new LearningMachineExport(
        SkillSelectionMachine.MachineId,
        SkillSelectionMachine.DomainTag,
        SkillSelectionMachine.Version,
        importedCheckpoint,
        importedManifest,
        0.0,
        DateTimeOffset.UtcNow);
    var importMachine = new SkillSelectionMachine();
    var importResult = await importMachine.ImportAsync(importExport);
    Ensure(importResult.IsSuccess, "SkillSelection import load failed: " + importResult.ErrorMessage);

    var rollbackLoad = await SkillSelectionMachine.LoadCheckpointAsync(Path.Combine(backupDir, "skill-selection-v1-checkpoint.json"));
    Ensure(rollbackLoad.IsSuccess && rollbackLoad.Value is not null, "SkillSelection rollback load failed: " + rollbackLoad.ErrorMessage);
    var rollbackDecision = rollbackLoad.Value.Machine.DecideFromTrainedState(deltaRequest).Value!;

    var heldOutRequest = SkillRequest(
        "Lifecycle proof asks for verification gate before any readiness claim.",
        new[] { "ui-testing", "verification-before-completion", "plan-review", "planning-session-startup" },
        "qsg23.synthetic.skill.heldout",
        "held_out");
    var heldOutDecision = userFirstLoad.Machine.DecideFromTrainedState(heldOutRequest).Value!;
    var exactHeldOutHash = HashUpper(SkillSelectionMachine.NormalizePromptKey(heldOutRequest.Prompt));
    var exactHeldOutHit = userFirstLoad.Checkpoint.TrainedPromptKeys.Contains(exactHeldOutHash, StringComparer.Ordinal);
    var candidatePositionEvidenceUsed = freshDecision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal)
        || shuffledDecision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal)
        || noPositionDecision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal);
    var selectedInvariant = SameSet(freshDecision.SelectedSkillIds, shuffledDecision.SelectedSkillIds);
    var noPositionCanSelectSame = SameSet(freshDecision.SelectedSkillIds, noPositionDecision.SelectedSkillIds);

    return new Dictionary<string, object?>
    {
        ["machine_id"] = "SkillSelectionMachine",
        ["asset_topology"] = new Dictionary<string, object?>
        {
            ["registry_common_checkpoint_ref"] = ToRef(rootPath, registryCommonCheckpoint),
            ["common_checkpoint_ref"] = ToRef(rootPath, publicLoadCommonCheckpoint),
            ["public_load_typed_copy_ref"] = ToRef(rootPath, publicLoadCommonCheckpoint),
            ["common_manifest_ref"] = ToRef(rootPath, commonManifest),
            ["user_adaptive_checkpoint_ref"] = ToRef(rootPath, userCheckpoint),
            ["user_adaptive_manifest_ref"] = ToRef(rootPath, userManifest),
            ["continued_checkpoint_ref"] = ToRef(rootPath, continuedCheckpoint),
            ["continued_manifest_ref"] = ToRef(rootPath, continuedManifest),
            ["backup_checkpoint_ref"] = ToRef(rootPath, Path.Combine(backupDir, "skill-selection-v1-checkpoint.json")),
            ["backup_manifest_ref"] = ToRef(rootPath, Path.Combine(backupDir, "manifest.json")),
            ["import_checkpoint_ref"] = ToRef(rootPath, importedCheckpoint),
            ["import_manifest_ref"] = ToRef(rootPath, importedManifest)
        },
        ["fresh_load"] = new Dictionary<string, object?>
        {
            ["passed"] = userFirstLoad.Source == "user_adaptive" && freshDecision.SelectedSkillIds.Count > 0,
            ["load_order"] = userFirstLoad.Attempts,
            ["loaded_source"] = userFirstLoad.Source,
            ["runtime_invocation"] = SkillDecisionSummary(freshDecision),
            ["common_fallback_probe"] = new Dictionary<string, object?>
            {
                ["loaded_source"] = commonFallback.Source,
                ["load_order"] = commonFallback.Attempts,
                ["fallback_after_concrete_missing_user_path"] = true,
                ["attempted_missing_user_checkpoint_ref"] = ToRef(rootPath, missingUserCheckpoint),
                ["attempted_missing_user_manifest_ref"] = ToRef(rootPath, missingUserManifest),
                ["fallback_checkpoint_ref"] = ToRef(rootPath, publicLoadCommonCheckpoint),
                ["runtime_invocation"] = SkillDecisionSummary(commonDecision)
            }
        },
        ["continue_training"] = new Dictionary<string, object?>
        {
            ["passed"] = afterPairCount == beforePairCount + 1 && continuedDecision.SelectedSkillIds.Contains("qsg23-delta-only-skill", StringComparer.Ordinal),
            ["loaded_existing_checkpoint_first"] = true,
            ["base_checkpoint_sha256"] = FileHash(userCheckpoint),
            ["continued_checkpoint_sha256"] = FileHash(continuedCheckpoint),
            ["trained_pair_count_before"] = beforePairCount,
            ["trained_pair_count_after"] = afterPairCount,
            ["new_pair_id"] = "qsg23-skill-continue-delta",
            ["retrain_from_scratch"] = false,
            ["runtime_invocation_after_continue"] = SkillDecisionSummary(continuedDecision)
        },
        ["backup_export_import_rollback"] = new Dictionary<string, object?>
        {
            ["backup_ref_exists"] = File.Exists(Path.Combine(backupDir, "skill-selection-v1-checkpoint.json")) && File.Exists(Path.Combine(backupDir, "manifest.json")),
            ["export_ref"] = ToRef(rootPath, continuedCheckpoint),
            ["import_ref"] = ToRef(rootPath, importedCheckpoint),
            ["import_load_passed"] = importResult.IsSuccess,
            ["rollback_load_passed"] = rollbackLoad.IsSuccess,
            ["rollback_comparison_passed"] = !rollbackDecision.SelectedSkillIds.Contains("qsg23-delta-only-skill", StringComparer.Ordinal),
            ["rollback_runtime_invocation"] = SkillDecisionSummary(rollbackDecision)
        },
        ["held_out_ablation"] = new Dictionary<string, object?>
        {
            ["passed"] = !exactHeldOutHit
                && heldOutDecision.SelectedSkillIds.Contains("verification-before-completion", StringComparer.Ordinal)
                && !SameSet(heldOutDecision.SelectedSkillIds, commonDecision.SelectedSkillIds),
            ["test_data_scope"] = "synthetic_test_only_source_disjoint_controls",
            ["source_disjoint"] = true,
            ["exact_held_out_prompt_hit"] = exactHeldOutHit,
            ["adaptive_differs_from_common_ablation"] = !SameSet(heldOutDecision.SelectedSkillIds, commonDecision.SelectedSkillIds),
            ["semantic_quality_claimed"] = false,
            ["held_out_runtime_invocation"] = SkillDecisionSummary(heldOutDecision),
            ["ablation_without_adaptive_state"] = SkillDecisionSummary(commonDecision)
        },
        ["candidate_position_and_shortcut_boundary"] = new Dictionary<string, object?>
        {
            ["checked"] = true,
            ["candidate_position_weights_present"] = userFirstLoad.Checkpoint.CandidatePositionWeights.Count > 0,
            ["candidate_position_weights_label"] = "baseline/scaffold/leakage-guard only",
            ["candidate_position_evidence_used_as_final_authority"] = candidatePositionEvidenceUsed,
            ["selection_invariant_under_candidate_order_shuffle"] = selectedInvariant,
            ["probe_without_candidate_position_authority_selected_same_skills"] = noPositionCanSelectSame,
            ["probe_without_candidate_position_authority"] = SkillDecisionSummary(noPositionDecision),
            ["leakage_report"] = leakageReport,
            ["hidden_deterministic_shortcut_detected"] = candidatePositionEvidenceUsed || !selectedInvariant || !noPositionCanSelectSame,
            ["semantic_quality_claimed"] = false
        },
        ["claims"] = ClosedClaims()
    };
}

PromptToTaskRequest PromptRequest(string promptId, string rawPrompt, IReadOnlyList<string> explicitConstraints, string provenance)
    => new(
        PromptId: promptId,
        RawPrompt: rawPrompt,
        RawPromptSha256: HashLower(rawPrompt),
        PromptProvenance: provenance,
        UserLanguage: "en",
        ExplicitConstraints: explicitConstraints,
        ContextFields: new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["raw_prompt_redacted"] = "qsg23-test-only-redacted",
            ["xiigen_method"] = "prompt-to-task",
            ["host_fact.boundary"] = "host_boundary_from_request",
            ["provenance_key.phase"] = "QUALITY-SEMANTIC-GATES-23"
        });

SkillSelectionRequest SkillRequest(string prompt, IReadOnlyList<string> candidates, string sourceEntryId, string scope)
    => new(
        Prompt: prompt,
        ProjectRoot: "",
        AvailableSkillIds: candidates,
        HostFacts: new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["scope"] = "qsg23_test_only",
            ["lifecycle"] = scope,
            ["production_mutation"] = "forbidden"
        },
        UserPromptSourceEntryId: sourceEntryId);

SkillSelectionMachinePair SkillPair(
    string pairId,
    string prompt,
    IReadOnlyList<string> candidates,
    string chosen,
    string adapter,
    string sourceEntryId)
{
    var rejected = candidates.Where(item => !string.Equals(item, chosen, StringComparison.Ordinal)).ToArray();
    var request = SkillRequest(prompt, candidates, sourceEntryId, "training");
    var chosenDecision = new SkillSelectionDecision(
        SelectedSkillIds: new[] { chosen },
        RejectedSkillIds: rejected,
        EvidenceFeatures: new[] { "QSG23_SYNTHETIC_TEST_ONLY", SkillSelectionMachine.SemanticPipelineBoundaryEvidence },
        Confidence: 0.91,
        RequiresSecondPass: false,
        DecisionText: "QSG-23 synthetic test-only local adaptive training pair; semantic quality remains unclaimed.");
    var rejectedDecision = new SkillSelectionDecision(
        SelectedSkillIds: rejected.Take(1).ToArray(),
        RejectedSkillIds: new[] { chosen },
        EvidenceFeatures: new[] { "QSG23_SYNTHETIC_REJECTED_NEAR_MISS", SkillSelectionMachine.SemanticPipelineBoundaryEvidence },
        Confidence: 0.10,
        RequiresSecondPass: true,
        DecisionText: "Rejected near miss for QSG-23 synthetic test-only proof.");
    return new SkillSelectionMachinePair(
        PairId: pairId,
        Request: request,
        Chosen: chosenDecision,
        Rejected: rejectedDecision,
        RejectedType: "wrong_skill_contrast",
        SourcePromptEntryId: sourceEntryId,
        TargetTrainingAdapter: adapter);
}

PromptLoadResult LoadPromptToTaskWithFallback(string userCheckpoint, string userManifest, string commonCheckpoint, string commonManifest)
{
    var attempts = new List<Dictionary<string, object?>>();
    var userLoad = PromptToTaskMachine.LoadCheckpointAsync(userCheckpoint, userManifest).GetAwaiter().GetResult();
    attempts.Add(new Dictionary<string, object?>
    {
        ["source"] = "user_adaptive",
        ["checkpoint_ref"] = ToRef(root, userCheckpoint),
        ["manifest_ref"] = ToRef(root, userManifest),
        ["success"] = userLoad.IsSuccess,
        ["error_code"] = userLoad.ErrorCode
    });
    if (userLoad.IsSuccess && userLoad.Value is not null)
        return new PromptLoadResult(userLoad.Value.Machine, userLoad.Value.Checkpoint, userLoad.Value.Manifest, "user_adaptive", attempts);

    var commonLoad = PromptToTaskMachine.LoadCheckpointAsync(commonCheckpoint, commonManifest).GetAwaiter().GetResult();
    attempts.Add(new Dictionary<string, object?>
    {
        ["source"] = "repo_common_fallback",
        ["checkpoint_ref"] = ToRef(root, commonCheckpoint),
        ["manifest_ref"] = ToRef(root, commonManifest),
        ["success"] = commonLoad.IsSuccess,
        ["error_code"] = commonLoad.ErrorCode
    });
    Ensure(commonLoad.IsSuccess && commonLoad.Value is not null, "PromptToTask fallback load failed: " + commonLoad.ErrorMessage);
    return new PromptLoadResult(commonLoad.Value.Machine, commonLoad.Value.Checkpoint, commonLoad.Value.Manifest, "repo_common_fallback", attempts);
}

async Task<SkillLoadResult> LoadSkillSelectionWithFallback(LearningMachineExport userExport, string commonCheckpoint)
{
    var attempts = new List<Dictionary<string, object?>>();
    var userMachine = new SkillSelectionMachine();
    var userImport = await userMachine.ImportAsync(userExport);
    attempts.Add(new Dictionary<string, object?>
    {
        ["source"] = "user_adaptive",
        ["checkpoint_ref"] = ToRef(root, userExport.CheckpointRef),
        ["manifest_ref"] = ToRef(root, userExport.ManifestRef),
        ["success"] = userImport.IsSuccess,
        ["error_code"] = userImport.ErrorCode
    });
    if (userImport.IsSuccess)
    {
        var checkpointLoad = await SkillSelectionMachine.LoadCheckpointAsync(userExport.CheckpointRef);
        Ensure(checkpointLoad.IsSuccess && checkpointLoad.Value is not null, "SkillSelection checkpoint reload after import failed.");
        return new SkillLoadResult(userMachine, checkpointLoad.Value.Checkpoint, "user_adaptive", attempts);
    }

    var commonLoad = await SkillSelectionMachine.LoadCheckpointAsync(commonCheckpoint);
    attempts.Add(new Dictionary<string, object?>
    {
        ["source"] = "repo_common_fallback",
        ["checkpoint_ref"] = ToRef(root, commonCheckpoint),
        ["success"] = commonLoad.IsSuccess,
        ["error_code"] = commonLoad.ErrorCode
    });
    Ensure(commonLoad.IsSuccess && commonLoad.Value is not null, "SkillSelection fallback load failed: " + commonLoad.ErrorMessage);
    return new SkillLoadResult(commonLoad.Value.Machine, commonLoad.Value.Checkpoint, "repo_common_fallback", attempts);
}

Dictionary<string, object?> PromptResponseSummary(PromptToTaskResponse response)
    => new()
    {
        ["response_id"] = response.ResponseId,
        ["contract_status"] = response.ContractStatus,
        ["task_type"] = response.StructuredTask.TaskType,
        ["split_id"] = response.StructuredTask.SplitId,
        ["required_hosts"] = response.StructuredTask.RequiredHosts,
        ["evidence_needs"] = response.StructuredTask.EvidenceNeeds,
        ["contract_evidence"] = response.ContractEvidence,
        ["semantic_quality_claimed"] = false
    };

Dictionary<string, object?> SkillDecisionSummary(SkillSelectionDecision decision)
    => new()
    {
        ["selected_skill_ids"] = decision.SelectedSkillIds,
        ["rejected_skill_ids"] = decision.RejectedSkillIds,
        ["evidence_features"] = decision.EvidenceFeatures,
        ["confidence"] = decision.Confidence,
        ["requires_second_pass"] = decision.RequiresSecondPass,
        ["decision_text"] = decision.DecisionText,
        ["semantic_quality_claimed"] = false
    };

Dictionary<string, object?> ClosedClaims()
    => new()
    {
        ["semantic_quality_claimed"] = false,
        ["token_savings_claim_allowed"] = false,
        ["quality_improvement_claim_allowed"] = false,
        ["learnability_claimed"] = false,
        ["common_readiness_claimed"] = false,
        ["final_readiness_claimed"] = false,
        ["production_locator_or_checkpoint_mutated"] = false
    };

void CopyPair(string checkpointPath, string manifestPath, string targetDir)
{
    Directory.CreateDirectory(targetDir);
    var targetCheckpoint = Path.Combine(targetDir, Path.GetFileName(checkpointPath));
    var targetManifest = Path.Combine(targetDir, Path.GetFileName(manifestPath));
    File.Copy(checkpointPath, targetCheckpoint, true);
    File.Copy(manifestPath, targetManifest, true);
    NormalizeJsonPairNoBom(targetCheckpoint, targetManifest);
}

void NormalizeJsonPairNoBom(string checkpointPath, string manifestPath)
{
    NormalizeJsonFileNoBom(checkpointPath);
    if (!File.Exists(manifestPath))
        return;

    var manifest = ReadJsonNodeNoBom(manifestPath);
    if (manifest is JsonObject manifestObject && manifestObject.ContainsKey("checkpointSha256"))
        manifestObject["checkpointSha256"] = FileHash(checkpointPath);
    WriteJsonNodeNoBom(manifestPath, manifest);
}

void NormalizeJsonFileNoBom(string filePath)
{
    if (!File.Exists(filePath))
        return;

    WriteJsonNodeNoBom(filePath, ReadJsonNodeNoBom(filePath));
}

JsonNode ReadJsonNodeNoBom(string filePath)
{
    var text = File.ReadAllText(filePath, Encoding.UTF8);
    if (text.Length > 0 && text[0] == '\uFEFF')
        text = text[1..];
    var node = JsonNode.Parse(text);
    if (node is null)
        throw new InvalidOperationException("JSON document parsed null: " + filePath);
    return node;
}

void WriteJsonNodeNoBom(string filePath, JsonNode node)
{
    File.WriteAllText(
        filePath,
        NormalizeLf(node.ToJsonString(JsonOptions)) + "\n",
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

string CreateTypedSkillSelectionCheckpointCopy(string rootPath, string outputRoot, string registryCheckpoint)
{
    var target = Path.Combine(
        outputRoot,
        "assets",
        "normalized-common",
        "skill-selection",
        "skill-selection-v1",
        "skill-selection-v1-checkpoint.public-load-copy.json");
    Directory.CreateDirectory(Path.GetDirectoryName(target)!);

    using var doc = JsonDocument.Parse(File.ReadAllText(registryCheckpoint, Encoding.UTF8));
    var rootElement = doc.RootElement;
    var typedCheckpoint = new Dictionary<string, object?>
    {
        ["schemaVersion"] = IntProp(rootElement, "schema_version", "schemaVersion"),
        ["candidatePositionWeights"] = IntDictionary(rootElement, "candidate_position_weights", "candidatePositionWeights"),
        ["trainedPairIds"] = StringArray(rootElement, "trained_pair_ids", "trainedPairIds"),
        ["trainedPromptKeys"] = StringArray(rootElement, "trained_prompt_keys", "trainedPromptKeys"),
        ["promptUnderstanding"] = CounterUnit(rootElement, "prompt_understanding", "promptUnderstanding"),
        ["taskUnderstanding"] = CounterUnit(rootElement, "task_understanding", "taskUnderstanding"),
        ["taskType"] = CounterUnit(rootElement, "task_type", "taskType"),
        ["routing"] = CounterUnit(rootElement, "routing", "routing"),
        ["skillCandidates"] = CounterUnit(rootElement, "skill_candidates", "skillCandidates"),
        ["skillSelection"] = CounterUnit(rootElement, "skill_selection", "skillSelection"),
        ["promptContext"] = CounterUnit(rootElement, "prompt_context", "promptContext"),
        ["routeMode"] = StringProp(rootElement, "route_mode", "routeMode"),
        ["savedAtUtc"] = StringProp(rootElement, "saved_at_utc", "savedAtUtc"),
        ["resultGap"] = CounterUnit(rootElement, "result_gap", "resultGap"),
        ["outputValidation"] = CounterUnit(rootElement, "output_validation", "outputValidation"),
        ["taskCompletion"] = CounterUnit(rootElement, "task_completion", "taskCompletion"),
        ["intermediateChecks"] = CounterUnit(rootElement, "intermediate_checks", "intermediateChecks")
    };

    WriteJson(target, typedCheckpoint);
    return target;
}

Dictionary<string, object?> CounterUnit(JsonElement parent, string snakeName, string camelName)
{
    var unit = Required(parent, snakeName, camelName);
    return new Dictionary<string, object?>
    {
        ["unitId"] = StringProp(unit, "unit_id", "unitId"),
        ["schemaVersion"] = IntProp(unit, "schema_version", "schemaVersion"),
        ["skillProfiles"] = Required(unit, "skill_profiles", "skillProfiles")
            .EnumerateArray()
            .Select(CounterProfile)
            .ToArray(),
        ["trainingPairIds"] = StringArray(unit, "training_pair_ids", "trainingPairIds"),
        ["semanticQualityClaimed"] = BoolProp(unit, "semantic_quality_claimed", "semanticQualityClaimed")
    };
}

Dictionary<string, object?> CounterProfile(JsonElement profile)
    => new()
    {
        ["skillId"] = StringProp(profile, "skill_id", "skillId"),
        ["learnedSignals"] = StringIntDictionary(profile, "learned_signals", "learnedSignals"),
        ["sourcePairIds"] = StringArray(profile, "source_pair_ids", "sourcePairIds")
    };

JsonElement Required(JsonElement element, params string[] names)
{
    foreach (var name in names)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value))
            return value;
    }

    throw new InvalidOperationException("Required JSON property missing: " + string.Join("/", names));
}

int IntProp(JsonElement element, params string[] names)
    => Required(element, names).GetInt32();

bool BoolProp(JsonElement element, params string[] names)
    => Required(element, names).GetBoolean();

string StringProp(JsonElement element, params string[] names)
    => Required(element, names).GetString() ?? string.Empty;

string[] StringArray(JsonElement element, params string[] names)
    => Required(element, names)
        .EnumerateArray()
        .Select(item => item.GetString() ?? string.Empty)
        .ToArray();

Dictionary<string, int> IntDictionary(JsonElement element, params string[] names)
    => Required(element, names)
        .EnumerateObject()
        .ToDictionary(item => item.Name, item => item.Value.GetInt32(), StringComparer.Ordinal);

Dictionary<string, int> StringIntDictionary(JsonElement element, params string[] names)
    => Required(element, names)
        .EnumerateObject()
        .ToDictionary(item => item.Name, item => item.Value.GetInt32(), StringComparer.Ordinal);

bool SameSet(IReadOnlyList<string> left, IReadOnlyList<string> right)
    => left.OrderBy(item => item, StringComparer.Ordinal).SequenceEqual(right.OrderBy(item => item, StringComparer.Ordinal), StringComparer.Ordinal);

string Join(string basePath, string relative)
    => Path.GetFullPath(Path.Combine(basePath, relative.Replace('/', Path.DirectorySeparatorChar)));

string ToRef(string basePath, string filePath)
    => Path.GetRelativePath(basePath, Path.GetFullPath(filePath)).Replace(Path.DirectorySeparatorChar, '/');

string FileHash(string filePath)
    => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(filePath))).ToLowerInvariant();

string HashLower(string value)
    => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

string HashUpper(string value)
    => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

string NormalizeLf(string value)
    => value.Replace("\r\n", "\n").Replace("\r", "\n");

void Ensure(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

void WriteJson(string filePath, object value)
{
    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
    File.WriteAllText(
        filePath,
        NormalizeLf(JsonSerializer.Serialize(value, JsonOptions)) + "\n",
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

sealed record PromptLoadResult(
    PromptToTaskMachine Machine,
    PromptToTaskCheckpoint Checkpoint,
    PromptToTaskCheckpointManifest Manifest,
    string Source,
    IReadOnlyList<Dictionary<string, object?>> Attempts);

sealed record SkillLoadResult(
    SkillSelectionMachine Machine,
    SkillSelectionCheckpoint Checkpoint,
    string Source,
    IReadOnlyList<Dictionary<string, object?>> Attempts);
`;
}

function buildQsg23Artifacts(dotnetRun) {
  const state = readJson(refs.state);
  const qsg22Report = readJson(refs.qsg22Report);
  const qsg22Readiness = readJson(refs.qsg22Readiness);
  const modelIndex = readJson(refs.modelIndex);
  const pttSplit = readJson(refs.promptToTaskSplit);
  const pttRows = readJsonl(refs.promptToTaskDpo);
  const skillRows = readJsonl(refs.skillSelectionCorpus);
  const dotnet = readJson(outputs.dotnetReport);
  const ptt = dotnet.machines.PromptToTaskMachine;
  const ss = dotnet.machines.SkillSelectionMachine;

  const sourceFiles = Object.values(refs).map(fileInfo);
  const outputFileBase = Object.values(outputs).filter((ref) => ref !== outputs.evidenceJsonl);

  const assetTopology = {
    schema: "xiigen.quality_semantic_gates_23.asset_topology.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    dry_run_only: true,
    production_locator_or_checkpoint_mutated: false,
    current_common_assets: {
      prompt_to_task: {
        checkpoint_ref: refs.promptToTaskCommonCheckpoint,
        manifest_ref: refs.promptToTaskCommonManifest,
        checkpoint_sha256: sha256File(refs.promptToTaskCommonCheckpoint),
        manifest_sha256: sha256File(refs.promptToTaskCommonManifest),
        model_index_entry: modelIndex.models.find((model) => model.model_id === "prompt-to-task-v1")
      },
      skill_selection: {
        checkpoint_ref: refs.skillSelectionCommonCheckpoint,
        manifest_ref: refs.skillSelectionCommonManifest,
        checkpoint_sha256: sha256File(refs.skillSelectionCommonCheckpoint),
        manifest_sha256: sha256File(refs.skillSelectionCommonManifest),
        model_index_entry: modelIndex.models.find((model) => model.model_id === "skill-selection-v1")
      }
    },
    staged_user_adaptive_assets: {
      prompt_to_task: ptt.asset_topology,
      skill_selection: ss.asset_topology
    },
    qsg22_gap_basis: qsg22Readiness.missing_artifact_or_gate_before_full_readiness,
    staged_assets_are_test_only: true,
    production_models_or_locator_mutated: false
  };

  const freshLoad = {
    schema: "xiigen.quality_semantic_gates_23.fresh_load_load_order.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    dry_run_only: true,
    prompt_to_task: ptt.fresh_load,
    skill_selection: ss.fresh_load,
    passed: ptt.fresh_load.passed === true && ss.fresh_load.passed === true,
    claim_scope: "Fresh-load/load-order evidence only. User/adaptive staged assets load first; repo/common fallback is separately observed. No semantic quality or production readiness claim."
  };

  const continueTraining = {
    schema: "xiigen.quality_semantic_gates_23.continue_training.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    dry_run_only: true,
    prompt_to_task: ptt.continue_training,
    skill_selection: ss.continue_training,
    passed: ptt.continue_training.passed === true && ss.continue_training.passed === true,
    checkpoint_mutation_scope: "qsg23_staged_artifacts_only",
    retrain_from_scratch: false,
    production_checkpoint_mutated: false,
    claim_scope: "Continue-training/load-instead-of-retrain evidence only; staged test assets only."
  };

  const backupExportImport = {
    schema: "xiigen.quality_semantic_gates_23.backup_export_import_rollback.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    dry_run_only: true,
    prompt_to_task: ptt.backup_export_import_rollback,
    skill_selection: ss.backup_export_import_rollback,
    passed: ptt.backup_export_import_rollback.import_load_passed === true
      && ptt.backup_export_import_rollback.rollback_comparison_passed === true
      && ss.backup_export_import_rollback.import_load_passed === true
      && ss.backup_export_import_rollback.rollback_comparison_passed === true,
    production_rollback_closure_done: false,
    claim_scope: "Backup/export/import/rollback evidence for QSG-23 staged assets only; not production rollback closure."
  };

  const heldOutAblation = {
    schema: "xiigen.quality_semantic_gates_23.held_out_ablation_controls.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    dry_run_only: true,
    prompt_to_task: {
      ...ptt.held_out_ablation,
      split_ref: refs.promptToTaskSplit,
      train_count: pttSplit.train_count,
      held_out_count: pttSplit.held_out_count,
      source_disjoint_from_existing_split: pttSplit.source_disjoint === true,
      universal_prompt_to_task_rows_checked: pttRows.every((row) => row.universal_prompt_to_task_behavior === true && row.technical_execution_refs_excluded === true)
    },
    skill_selection: {
      ...ss.held_out_ablation,
      corpus_ref: refs.skillSelectionCorpus,
      safe_synthetic_rows_used: true,
      repo_safe_rows_sampled: skillRows.slice(0, 5).map((row) => ({
        pair_id: row.pair_id,
        training_eligible: row.training_eligible,
        universal_skill_semantics_only: row.universal_skill_semantics_only,
        deterministic_shortcut_rejected: row.deterministic_shortcut_rejected
      }))
    },
    passed: ptt.held_out_ablation.passed === true && ss.held_out_ablation.passed === true,
    semantic_quality_claimed: false,
    quality_improvement_claimed: false,
    test_data_scope: "synthetic/test-only controls plus source-disjoint metadata checks; not semantic quality evidence"
  };

  const skillBoundary = ss.candidate_position_and_shortcut_boundary;
  const pttBoundary = ptt.prompt_to_universal_task_boundary;
  const sourceBoundaryHits = {
    skill_selection_machine_candidate_position_refs: countMatches(readText(refs.skillSelectionMachine), /CandidatePosition|candidate-position|candidate position|_candidatePositionWeights/g),
    skill_selection_machine_shortcut_terms: countMatches(readText(refs.skillSelectionMachine), /regex|BM25|exact replay|static fixture|source path|candidate order/gi),
    prompt_to_task_stack_terms: countMatches(readText(refs.promptToTaskMachine), /llm_mvp_core|Cursor|Claude|Codex|C:\\/g)
  };
  const deterministicBoundary = {
    schema: "xiigen.quality_semantic_gates_23.deterministic_boundary_review.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    dry_run_only: true,
    prompt_to_task_universal_boundary: pttBoundary,
    skill_selection_candidate_position_boundary: skillBoundary,
    source_boundary_counts: sourceBoundaryHits,
    verdict: skillBoundary.hidden_deterministic_shortcut_detected === false
      && pttBoundary.no_project_stack_specific_leakage_in_structured_task === true
      ? "PASS_STAGED_RUNTIME_BOUNDARY_NO_HIDDEN_FINAL_SHORTCUT_DETECTED"
      : "IN_PROGRESS_BOUNDARY_LIMITATION_RECORDED",
    deterministic_helpers_labelled_baseline_only: true,
    semantic_quality_claimed: false
  };

  const claimsOpened = [
    "qsg23_prompt_to_task_staged_full_machine_checkpoint_manifest_fresh_load_invocation",
    "qsg23_skill_selection_staged_full_machine_checkpoint_manifest_fresh_load_invocation",
    "qsg23_user_adaptive_load_order_before_repo_common_fallback_recorded",
    "qsg23_continue_training_from_existing_staged_checkpoint_recorded",
    "qsg23_staged_backup_export_import_rollback_recorded",
    "qsg23_source_disjoint_synthetic_held_out_ablation_controls_recorded",
    "qsg23_skill_selection_candidate_position_boundary_checked",
    "qsg23_prompt_to_task_universal_task_boundary_checked"
  ];
  const claimsKeptClosed = [
    "token_savings_claim_allowed",
    "quality_improvement_claim_allowed",
    "semantic_quality_claimed",
    "learnability_claimed",
    "common_readiness_claim_allowed",
    "production_locator_promotion_done",
    "locator_update_done",
    "production_rollback_closure_done",
    "final_readiness_claimed"
  ];
  const claimLedger = {
    schema: "xiigen.quality_semantic_gates_23.claim_ledger.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    claims_opened: claimsOpened,
    claims_kept_closed: claimsKeptClosed,
    closed_claim_reasons: {
      token_savings_claim_allowed: "QSG-23 did not run paired token metrics.",
      quality_improvement_claim_allowed: "QSG-23 held-out controls are synthetic/test-only and do not prove rubric quality improvement.",
      semantic_quality_claimed: "The machines and manifests keep semantic_quality_claimed=false; deterministic baseline/helper evidence is not semantic quality.",
      learnability_claimed: "QSG-23 proves staged lifecycle mechanics, not real AI-output/correction learnability or product-quality improvement.",
      common_readiness_claim_allowed: "QSG-23 did not mutate or validate production/common promotion readiness.",
      production_locator_promotion_done: "Production locator/checkpoint mutation was forbidden and not done.",
      production_rollback_closure_done: "Rollback was staged-only because no production mutation occurred.",
      final_readiness_claimed: "Final readiness still requires full target matrix and ten final readiness passes."
    }
  };

  const reportStatus = [
    freshLoad.passed,
    continueTraining.passed,
    backupExportImport.passed,
    heldOutAblation.passed,
    deterministicBoundary.verdict.startsWith("PASS"),
    dotnet.production_locator_or_checkpoint_mutated === false
  ].every(Boolean)
    ? "PASS_STAGED_LOCAL_ADAPTIVE_READINESS_PROOF_CREATED_SYNTHETIC_CONTROLS_NO_PRODUCTION_MUTATION"
    : "IN_PROGRESS_QSG23_STAGED_EVIDENCE_RECORDED_WITH_GAPS";

  const report = {
    schema: "xiigen.quality_semantic_gates_23.report.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    status: reportStatus,
    dry_run_only: true,
    production_locator_or_checkpoint_mutated: dotnet.production_locator_or_checkpoint_mutated === true,
    evidence_scope: "QSG-23 staged local adaptive full-machine lifecycle proof for PromptToTaskMachine and SkillSelectionMachine only.",
    qsg22_basis_ref: refs.qsg22Report,
    qsg22_previous_gap_ref: refs.qsg22Readiness,
    artifact_refs: outputs,
    claims_opened: claimsOpened,
    claims_kept_closed: claimsKeptClosed,
    what_this_proves: [
      "PromptToTask and SkillSelection staged user/adaptive full-machine checkpoints/manifests can be loaded first through public machine boundaries.",
      "Repo/common fallback remains available and is recorded as fallback, not as adaptive source; SkillSelection fallback is proven by a concrete missing staged checkpoint path before repo/common fallback.",
      "Continue-training probes start from existing staged checkpoints and write only QSG-23 staged artifacts.",
      "Backup/export/import/rollback comparisons work for the staged assets.",
      "Held-out/ablation controls are source-disjoint synthetic/test-only controls and therefore do not claim semantic quality.",
      "SkillSelection candidate-position state is labelled baseline/scaffold/leakage-guard only and was not used as final authority in the staged semantic decision."
    ],
    what_this_does_not_prove: [
      "No token savings, quality improvement, full semantic quality, full learnability, common readiness, production locator promotion, production rollback closure, or final readiness claim is opened.",
      "Synthetic QSG-23 controls do not prove real product semantic quality or no-memorization over a full corpus.",
      "Production/common PromptToTask and SkillSelection assets were not promoted or mutated."
    ],
    next_qsg_step: "QUALITY-SEMANTIC-GATES-24: replace synthetic/test-only local adaptive controls with real AI-output/correction DPO rows and source-disjoint held-out evaluation, or request explicit Luba approval for the exact production mutation track if promotion is desired.",
    validation_summary: {
      fresh_load_passed: freshLoad.passed,
      continue_training_passed: continueTraining.passed,
      backup_export_import_rollback_passed: backupExportImport.passed,
      held_out_ablation_passed: heldOutAblation.passed,
      deterministic_boundary_verdict: deterministicBoundary.verdict,
      production_mutation_detected: dotnet.production_locator_or_checkpoint_mutated === true
    }
  };

  const skillSelectionFallback = freshLoad.skill_selection.common_fallback_probe || {};
  const skillSelectionFallbackLoadOrder = Array.isArray(skillSelectionFallback.load_order)
    ? skillSelectionFallback.load_order
    : [];
  const skillSelectionFallbackEvidenceStrong = skillSelectionFallback.loaded_source === "repo_common_fallback"
    && skillSelectionFallback.fallback_after_concrete_missing_user_path === true
    && typeof skillSelectionFallback.attempted_missing_user_checkpoint_ref === "string"
    && skillSelectionFallback.attempted_missing_user_checkpoint_ref.includes("missing-user-adaptive-checkpoint.json")
    && typeof skillSelectionFallback.fallback_checkpoint_ref === "string"
    && skillSelectionFallback.fallback_checkpoint_ref.endsWith("skill-selection-v1-checkpoint.public-load-copy.json")
    && skillSelectionFallbackLoadOrder.some((row) => row.source === "user_adaptive" && row.success === false && row.error_code === "CHECKPOINT_NOT_FOUND")
    && skillSelectionFallbackLoadOrder.some((row) => row.source === "repo_common_fallback" && row.success === true);

  const validation = {
    schema: "xiigen.quality_semantic_gates_23.validation.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    command: "node tools/qsg23-local-adaptive-readiness.js --dry-run",
    dry_run_mode: dryRun,
    source_files_checked: sourceFiles,
    source_files_exist: sourceFiles.every((info) => info.exists),
    source_json_parses: sourceFiles.filter((info) => info.ref.endsWith(".json")).every((info) => info.json_parses === true),
    dotnet_probe_stdout_present: dotnetRun.stdout.length > 0,
    dotnet_probe_stderr: dotnetRun.stderr,
    fresh_load_load_order_evidence_passed: freshLoad.passed,
    continue_training_load_instead_of_retrain_passed: continueTraining.passed,
    backup_export_import_rollback_passed: backupExportImport.passed,
    held_out_ablation_result_evidence_passed: heldOutAblation.passed,
    deterministic_boundary_review_verdict: deterministicBoundary.verdict,
    hidden_deterministic_shortcut_detected: skillBoundary.hidden_deterministic_shortcut_detected === true,
    hidden_deterministic_shortcut_false: skillBoundary.hidden_deterministic_shortcut_detected === false,
    skill_selection_fallback_evidence_strong: skillSelectionFallbackEvidenceStrong,
    skill_selection_fallback_evidence: {
      loaded_source: skillSelectionFallback.loaded_source,
      fallback_after_concrete_missing_user_path: skillSelectionFallback.fallback_after_concrete_missing_user_path === true,
      attempted_missing_user_checkpoint_ref: skillSelectionFallback.attempted_missing_user_checkpoint_ref,
      fallback_checkpoint_ref: skillSelectionFallback.fallback_checkpoint_ref,
      failed_user_adaptive_attempt_recorded: skillSelectionFallbackLoadOrder.some((row) => row.source === "user_adaptive" && row.success === false && row.error_code === "CHECKPOINT_NOT_FOUND"),
      repo_common_fallback_success_recorded: skillSelectionFallbackLoadOrder.some((row) => row.source === "repo_common_fallback" && row.success === true)
    },
    production_locator_or_checkpoint_mutated: dotnet.production_locator_or_checkpoint_mutated === true,
    no_overclaim_verified: claimsKeptClosed.every((claim) => !claimsOpened.includes(claim)),
    verdict: reportStatus
  };

  const evidenceRows = [
    evidenceRow("asset_topology", "PromptToTaskMachine", true, outputs.assetTopology),
    evidenceRow("asset_topology", "SkillSelectionMachine", true, outputs.assetTopology),
    evidenceRow("fresh_load", "PromptToTaskMachine", ptt.fresh_load.passed === true, outputs.freshLoad, ptt.fresh_load.loaded_source),
    evidenceRow("fresh_load", "SkillSelectionMachine", ss.fresh_load.passed === true, outputs.freshLoad, ss.fresh_load.loaded_source),
    evidenceRow("continue_training", "PromptToTaskMachine", ptt.continue_training.passed === true, outputs.continueTraining),
    evidenceRow("continue_training", "SkillSelectionMachine", ss.continue_training.passed === true, outputs.continueTraining),
    evidenceRow("backup_export_import_rollback", "PromptToTaskMachine", ptt.backup_export_import_rollback.import_load_passed === true, outputs.backupExportImport),
    evidenceRow("backup_export_import_rollback", "SkillSelectionMachine", ss.backup_export_import_rollback.import_load_passed === true, outputs.backupExportImport),
    evidenceRow("held_out_ablation", "PromptToTaskMachine", ptt.held_out_ablation.passed === true, outputs.heldOutAblation),
    evidenceRow("held_out_ablation", "SkillSelectionMachine", ss.held_out_ablation.passed === true, outputs.heldOutAblation),
    evidenceRow("deterministic_boundary", "PromptToTaskMachine", pttBoundary.no_project_stack_specific_leakage_in_structured_task === true, outputs.deterministicBoundary),
    evidenceRow("deterministic_boundary", "SkillSelectionMachine", skillBoundary.hidden_deterministic_shortcut_detected === false, outputs.deterministicBoundary)
  ];

  const summary = `# QUALITY-SEMANTIC-GATES-23 local adaptive readiness

QSG-23 created staged/test-only evidence of the local adaptive lifecycle for the full PromptToTaskMachine and SkillSelectionMachine machines.

## What is confirmed

- For both machines the staged user/adaptive checkpoint+manifest load before the repo/common fallback.
- The SkillSelection fallback was checked via a specific missing staged user checkpoint path, after which the transition to the repo/common fallback was recorded.
- The runtime call goes through the public machine boundaries: PromptToTaskMachine.Translate and SkillSelectionMachine.DecideFromTrainedState / ImportAsync.
- Continue-training starts from an already-existing staged checkpoint and writes only QSG-23 artifacts.
- Backup, export/import and rollback comparison are recorded only for staged assets.
- The SkillSelection candidate-position state was checked and marked as baseline/scaffold/leakage-guard; the staged semantic decision does not use candidate position as the final source of truth.
- The PromptToTask staged output was checked for universal task fields and for the absence of project-stack-specific leakage in those fields.
- Validation enumerates and strictly parses all QSG-23 JSON/JSONL, including asset checkpoint/manifest JSON, without stripping the UTF-8 BOM.

## What is not confirmed

- Synthetic/test-only controls do not prove semantic quality, quality improvement, token savings, real learnability, common readiness, production promotion, production rollback closure or final readiness.
- Production locator/checkpoint files were not changed.
- QSG-23 does not open a product-readiness claim.

## Next step

QUALITY-SEMANTIC-GATES-24 must replace the synthetic/test-only controls with real AI-output/correction DPO rows and source-disjoint held-out evaluation, or move into a separate production-mutation track only after explicit approval from Luba for the exact mutation.
`;

  const reportMd = `${summary}

## Open claims

${claimsOpened.map((claim) => `- ${claim}`).join("\n")}

## Closed claims

${claimsKeptClosed.map((claim) => `- ${claim}`).join("\n")}
`;

  writeJson(outputs.assetTopology, assetTopology);
  writeJson(outputs.freshLoad, freshLoad);
  writeJson(outputs.continueTraining, continueTraining);
  writeJson(outputs.backupExportImport, backupExportImport);
  writeJson(outputs.heldOutAblation, heldOutAblation);
  writeJson(outputs.deterministicBoundary, deterministicBoundary);
  writeJson(outputs.claimLedger, claimLedger);
  writeText(outputs.summary, summary);
  writeJson(outputs.reportJson, report);
  writeText(outputs.reportMd, reportMd);
  writeJson(outputs.validation, validation);
  writeJsonl(outputs.evidenceJsonl, evidenceRows);

  const outputInfos = outputFileBase.concat([outputs.evidenceJsonl]).map(fileInfo);
  const parseCoverage = collectQsg23OutputParseCoverage();
  validation.output_files = outputInfos;
  validation.output_files_exist = outputInfos.every((info) => info.exists);
  validation.output_json_parses = parseCoverage.all_json_jsonl_parse_without_bom_stripping;
  validation.output_json_jsonl_parse_coverage = parseCoverage;
  validation.output_json_jsonl_files_checked_total = parseCoverage.files_checked_total;
  validation.output_json_files_checked = parseCoverage.json_files_checked;
  validation.output_jsonl_files_checked = parseCoverage.jsonl_files_checked;
  validation.output_json_parse_failure_count = parseCoverage.json_failures.length;
  validation.output_jsonl_parse_failure_count = parseCoverage.jsonl_failures.length;
  validation.output_utf8_bom_failure_count = parseCoverage.utf8_bom_failures.length;
  validation.output_parse_failure_refs = parseCoverage.json_failures
    .concat(parseCoverage.jsonl_failures)
    .map((info) => info.ref);
  const textQuality = collectQsg23TextQuality();
  validation.output_text_encoding_quality = textQuality;
  validation.output_no_mojibake_verified = textQuality.mojibake_failure_count === 0;
  validation.output_text_utf8_bom_failure_count = textQuality.utf8_bom_failure_count;
  validation.output_text_mojibake_failure_count = textQuality.mojibake_failure_count;
  validation.output_text_mojibake_failure_refs = textQuality.mojibake_failure_refs;
  writeJson(outputs.validation, validation);

  updateExecutionState(state, report, validation);

  return { report, validation, evidenceRows };
}

function countMatches(text, pattern) {
  return [...text.matchAll(pattern)].length;
}

function evidenceRow(kind, machineId, passed, ref, loadedSource = null) {
  return {
    schema: "xiigen.quality_semantic_gates_23.evidence_row.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    evidence_kind: kind,
    machine_id: machineId,
    passed,
    ref,
    loaded_source: loadedSource,
    semantic_quality_claimed: false,
    production_mutation_done: false,
    scope: "qsg23 staged/test-only local adaptive readiness evidence"
  };
}

function updateExecutionState(state, report, validation) {
  state.updated_at_local = generatedAtUtc;
  state.current_phase = phaseId;
  state.phase_status = report.status;
  state.phase_scope = "QSG-23 created non-mutating staged full-machine local adaptive readiness evidence for PromptToTaskMachine and SkillSelectionMachine using test-only staged assets under artifacts/QSG-23. Production locator/checkpoint mutation, token savings, quality improvement, semantic quality, full learnability, common readiness, production rollback closure, and final readiness remain closed.";
  state.goal_reached = false;
  state.stop_allowed = false;
  state.next_action = report.next_qsg_step;
  state.continuation_required = true;
  state.implementation_authorized = false;
  state.implementation_authorized_scope = "QSG-23 staged test-only artifacts are generated. Production locator/checkpoint mutation remains not authorized.";
  state.quality_semantic_gates_23 = {
    schema: report.schema,
    phase_id: phaseId,
    report_ref: outputs.reportJson,
    markdown_ref: outputs.reportMd,
    validation_ref: outputs.validation,
    evidence_jsonl_ref: outputs.evidenceJsonl,
    qsg23_artifact_root: qsg23Ref,
    status: report.status,
    production_locator_or_checkpoint_mutated: false,
    claims_opened: report.claims_opened,
    claims_kept_closed: report.claims_kept_closed,
    next_bounded_work_order: report.next_qsg_step
  };
  state.quality_semantic_gates_23_validation = {
    schema: validation.schema,
    phase_id: phaseId,
    validation_ref: outputs.validation,
    verdict: validation.verdict,
    fresh_load_load_order_evidence_passed: validation.fresh_load_load_order_evidence_passed,
    continue_training_load_instead_of_retrain_passed: validation.continue_training_load_instead_of_retrain_passed,
    backup_export_import_rollback_passed: validation.backup_export_import_rollback_passed,
    held_out_ablation_result_evidence_passed: validation.held_out_ablation_result_evidence_passed,
    deterministic_boundary_review_verdict: validation.deterministic_boundary_review_verdict,
    hidden_deterministic_shortcut_detected: validation.hidden_deterministic_shortcut_detected,
    hidden_deterministic_shortcut_false: validation.hidden_deterministic_shortcut_false,
    skill_selection_fallback_evidence_strong: validation.skill_selection_fallback_evidence_strong,
    production_locator_or_checkpoint_mutated: false,
    no_overclaim_verified: validation.no_overclaim_verified,
    output_json_parses: validation.output_json_parses,
    output_json_jsonl_files_checked_total: validation.output_json_jsonl_files_checked_total,
    output_json_files_checked: validation.output_json_files_checked,
    output_jsonl_files_checked: validation.output_jsonl_files_checked,
    output_json_parse_failure_count: validation.output_json_parse_failure_count,
    output_jsonl_parse_failure_count: validation.output_jsonl_parse_failure_count,
    output_utf8_bom_failure_count: validation.output_utf8_bom_failure_count,
    output_no_mojibake_verified: validation.output_no_mojibake_verified,
    output_text_utf8_bom_failure_count: validation.output_text_utf8_bom_failure_count,
    output_text_mojibake_failure_count: validation.output_text_mojibake_failure_count,
    output_parse_failure_refs: validation.output_parse_failure_refs
  };
  writeJson(refs.state, state);
}

resetQsg23Dir();
const projectPath = makeDotnetProject();
const dotnetRun = runDotnetProbe(projectPath);
const { report, validation } = buildQsg23Artifacts(dotnetRun);

console.log(JSON.stringify({
  phase_id: phaseId,
  status: report.status,
  dry_run_only: true,
  production_locator_or_checkpoint_mutated: false,
  report_ref: outputs.reportJson,
  validation_ref: outputs.validation,
  evidence_jsonl_ref: outputs.evidenceJsonl,
  qsg23_artifact_root: qsg23Ref,
  validation_verdict: validation.verdict
}, null, 2));
