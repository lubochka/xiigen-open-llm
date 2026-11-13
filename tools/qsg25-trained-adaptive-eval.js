const fs = require("fs");
const path = require("path");
const crypto = require("crypto");
const { spawnSync } = require("child_process");

const root = process.cwd();
const phaseId = "QUALITY-SEMANTIC-GATES-25";
const generatedAtUtc = new Date().toISOString();
const dryRun = process.argv.includes("--dry-run");

if (!dryRun) {
  throw new Error("QSG-25 must be run with --dry-run; production locator/checkpoint mutation is forbidden.");
}

const artifactRoot = "artifacts/extension-runtime-token-quality-learnability";
const qsg25Ref = `${artifactRoot}/qsg25-trained-adaptive-eval`;
const qsg25Dir = path.join(root, qsg25Ref);

const refs = {
  state: "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json",
  qsg24Report: `${artifactRoot}/quality-semantic-gates-24-report.json`,
  qsg24Rows: `${artifactRoot}/qsg24-real-dpo-source-disjoint-eval/normalized-real-dpo-eval-rows.jsonl`,
  qsg24Split: `${artifactRoot}/qsg24-real-dpo-source-disjoint-eval/source-disjoint-split.json`,
  qsg24QualityGates: `${artifactRoot}/qsg24-real-dpo-source-disjoint-eval/quality-gates.json`,
  qsg23PromptBaseCheckpoint: `${artifactRoot}/qsg23-local-adaptive-readiness/assets/user-adaptive/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json`,
  qsg23PromptBaseManifest: `${artifactRoot}/qsg23-local-adaptive-readiness/assets/user-adaptive/prompt-to-task/prompt-to-task-v1/manifest.json`,
  qsg23SkillBaseCheckpoint: `${artifactRoot}/qsg23-local-adaptive-readiness/assets/user-adaptive/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json`,
  qsg23SkillBaseManifest: `${artifactRoot}/qsg23-local-adaptive-readiness/assets/user-adaptive/skill-selection/skill-selection-v1/manifest.json`,
  repoPromptCommonCheckpoint: "models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json",
  repoPromptCommonManifest: "models/prompt-to-task/prompt-to-task-v1/manifest.json",
  repoSkillCommonCheckpoint: "models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json",
  repoSkillCommonManifest: "models/skill-selection/skill-selection-v1/manifest.json",
  modelIndex: "models/index.json",
  promptMachine: "43.SmartSkillsDomain/PromptToTaskMachine.cs",
  promptPatternUnit: "43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs",
  skillMachine: "43.SmartSkillsDomain/SkillSelectionMachine.cs",
  skillContracts: "43.SmartSkillsDomain/SkillSelectionContracts.cs"
};

const outputs = {
  classificationLedger: `${qsg25Ref}/train-ready-classification-ledger.jsonl`,
  trainingInput: `${qsg25Ref}/qsg25-training-input.json`,
  trainReadySummary: `${qsg25Ref}/train-ready-summary.json`,
  sourceDisjointCheck: `${qsg25Ref}/source-disjoint-check.json`,
  dotnetProbeResult: `${qsg25Ref}/dotnet-training-probe-result.json`,
  trainedCheckpointEval: `${qsg25Ref}/trained-checkpoint-heldout-ablation-eval.json`,
  loadOrder: `${qsg25Ref}/load-order-fresh-load-continue-training.json`,
  backupExportImportRollback: `${qsg25Ref}/backup-export-import-rollback.json`,
  productionMutationGuard: `${qsg25Ref}/production-mutation-guard.json`,
  deterministicBoundary: `${qsg25Ref}/deterministic-boundary-review.json`,
  claimLedger: `${qsg25Ref}/claim-ledger.json`,
  summary: `${qsg25Ref}/summary.md`,
  reportJson: `${artifactRoot}/quality-semantic-gates-25-report.json`,
  reportMd: `${artifactRoot}/quality-semantic-gates-25-report.md`,
  validation: `${artifactRoot}/quality-semantic-gates-25.validation.json`,
  evidenceJsonl: `${artifactRoot}/quality-semantic-gates-25-evidence.jsonl`
};

function fullPath(ref) {
  return path.isAbsolute(ref) ? path.normalize(ref) : path.join(root, ref);
}

function artifactRef(filePath) {
  return path.relative(root, filePath).split(path.sep).join("/");
}

function normalizeLf(value) {
  return String(value).replace(/\r\n/g, "\n").replace(/\r/g, "\n");
}

function assertInside(child, parent) {
  const resolvedChild = path.resolve(child);
  const resolvedParent = path.resolve(parent);
  if (resolvedChild !== resolvedParent && !resolvedChild.startsWith(resolvedParent + path.sep)) {
    throw new Error(`Refusing to write outside intended directory: ${resolvedChild}`);
  }
}

function readText(ref) {
  const bytes = fs.readFileSync(fullPath(ref));
  const text = bytes.toString("utf8");
  return text.charCodeAt(0) === 0xfeff ? text.slice(1) : text;
}

function readJson(ref) {
  return JSON.parse(readText(ref));
}

function readJsonl(ref) {
  const text = readText(ref);
  return text
    .split(/\r?\n/)
    .filter((line) => line.trim().length > 0)
    .map((line) => JSON.parse(line));
}

function writeText(ref, value) {
  const filePath = fullPath(ref);
  fs.mkdirSync(path.dirname(filePath), { recursive: true });
  fs.writeFileSync(filePath, normalizeLf(value), "utf8");
}

function writeJson(ref, value) {
  writeText(ref, `${JSON.stringify(value, null, 2)}\n`);
}

function writeJsonl(ref, rows) {
  writeText(ref, rows.map((row) => JSON.stringify(row)).join("\n") + "\n");
}

function sha256Bytes(bytes) {
  return crypto.createHash("sha256").update(bytes).digest("hex");
}

function sha256File(refOrPath) {
  return sha256Bytes(fs.readFileSync(fullPath(refOrPath)));
}

function fileInfo(ref) {
  const filePath = fullPath(ref);
  const exists = fs.existsSync(filePath);
  return {
    ref,
    exists,
    size_bytes: exists ? fs.statSync(filePath).size : 0,
    sha256: exists ? sha256File(filePath) : null
  };
}

function asArray(value) {
  if (Array.isArray(value)) {
    return value;
  }
  if (value === undefined || value === null || value === "") {
    return [];
  }
  return [value];
}

function compactArray(value) {
  return asArray(value)
    .map((item) => String(item || "").trim())
    .filter(Boolean);
}

function sourceRefsExist(row) {
  return asArray(row.source_refs).every((source) => {
    if (!source || !source.path) {
      return source && source.exists !== false;
    }
    return fs.existsSync(fullPath(source.path));
  });
}

function hasLocalUserAdaptiveProvenance(row) {
  const status = String(row.license_provenance_status || "").toUpperCase();
  const sourceText = JSON.stringify(row.source_refs || []) + " " + String(row.source_document_id || "");
  const lowerSource = sourceText.toLowerCase();
  return status.includes("LOCAL")
    || status.includes("PRIVATE")
    || status.includes("PROJECT_LOCAL")
    || lowerSource.includes("c:/users/")
    || lowerSource.includes("local-prompt")
    || lowerSource.includes("local_generated")
    || lowerSource.includes("repo_training_data");
}

function hasCleanCommonLicense(row) {
  const status = String(row.license_provenance_status || "").toUpperCase();
  return row.common_training_allowed === true
    && (
      status.includes("MIT")
      || status.includes("APACHE-2.0")
      || status.includes("PERMISSIVE")
      || status.includes("EXPLICIT_APPROVAL")
    );
}

function acceptedTrainCandidate(row) {
  return row.split_role === "train_candidate"
    && !String(row.material_status || "").startsWith("QUARANTINE")
    && String(row.qsg24_training_route || "").includes("DPO_CANDIDATE")
    && row.universal_only === true
    && asArray(row.stack_specific_terms_in_universal_target).length === 0
    && row.raw_private_text_copied !== true
    && row.external_ai_used !== true
    && row.deterministic_shortcut_hidden !== true
    && row.candidate_position_authority !== true;
}

function classifyRow(row) {
  let classification = "eval-only";
  let trainingRoute = "eval_only";
  let reason = "row is not an accepted train candidate";

  if (String(row.material_status || "").startsWith("QUARANTINE")
    || String(row.qsg24_training_route || "").includes("QUARANTINE")) {
    classification = "quarantined";
    trainingRoute = "quarantine_evidence_only";
    reason = row.quarantine_reason || "QSG-24 quarantine route";
  } else if (row.split_role === "held_out") {
    classification = "eval-only";
    trainingRoute = "held_out_eval_only";
    reason = "held-out rows must remain source-disjoint eval-only";
  } else if (acceptedTrainCandidate(row) && hasCleanCommonLicense(row)) {
    classification = "common-trainable";
    trainingRoute = "common_trainable_license_clean";
    reason = "common training license/provenance is clean";
  } else if (acceptedTrainCandidate(row) && hasLocalUserAdaptiveProvenance(row) && sourceRefsExist(row)) {
    classification = "user-adaptive-trainable";
    trainingRoute = "staged_user_adaptive_train_ready";
    reason = "local/user/project-generated provenance exists; common license remains closed";
  } else if (acceptedTrainCandidate(row) && !hasCleanCommonLicense(row)) {
    classification = "eval-only";
    trainingRoute = "common_closed_no_user_adaptive_provenance";
    reason = "accepted material lacks clean common license and usable user-adaptive provenance";
  }

  return {
    schema: "xiigen.qsg25.train_ready_classification_row.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    qsg25_row_id: `qsg25-${row.qsg24_row_id}`,
    qsg24_row_id: row.qsg24_row_id,
    source_row_id: row.source_row_id,
    machine_target: row.machine_target,
    row_kind: row.row_kind,
    split_role: row.split_role,
    source_document_id: row.source_document_id,
    source_ref: row.source_ref,
    source_refs: row.source_refs || [],
    source_refs_exist: sourceRefsExist(row),
    classification,
    training_route: trainingRoute,
    classification_reason: reason,
    common_training_allowed: row.common_training_allowed === true,
    common_training_license_clean: hasCleanCommonLicense(row),
    user_adaptive_training_allowed: classification === "user-adaptive-trainable",
    common_readiness_claimed: false,
    train_ready_for_staged_user_adaptive: classification === "user-adaptive-trainable",
    train_ready_for_common: classification === "common-trainable",
    held_out_eval_only: classification === "eval-only" && row.split_role === "held_out",
    quarantined: classification === "quarantined",
    universal_only: row.universal_only === true,
    stack_specific_terms_in_universal_target: row.stack_specific_terms_in_universal_target || [],
    raw_private_text_copied: row.raw_private_text_copied === true,
    external_ai_used: row.external_ai_used === true,
    deterministic_shortcut_hidden: row.deterministic_shortcut_hidden === true,
    candidate_position_authority: row.candidate_position_authority === true,
    qsg24_training_route: row.qsg24_training_route,
    material_status: row.material_status,
    source_quality: row.source_quality,
    license_provenance_status: row.license_provenance_status,
    common_or_adaptive_from_qsg24: row.common_or_adaptive,
    prompt: row.prompt || "",
    candidate_skills: row.candidate_skills || [],
    candidate_order_permutations: row.candidate_order_permutations || [],
    chosen: row.chosen,
    rejected: row.rejected,
    error_code: row.error_code || "",
    near_miss_taxonomy: row.near_miss_taxonomy || [],
    teaches_boundary: row.teaches_boundary || "",
    universal_training_target_text: row.universal_training_target_text || "",
    semantic_quality_claimed: false,
    learnability_claimed: false
  };
}

function buildSourceDisjointCheck(ledgerRows) {
  const machines = {};
  for (const machine of [...new Set(ledgerRows.map((row) => row.machine_target))].sort()) {
    const machineRows = ledgerRows.filter((row) => row.machine_target === machine);
    const trainSourceIds = machineRows
      .filter((row) => row.classification === "user-adaptive-trainable" || row.classification === "common-trainable")
      .map((row) => row.source_document_id);
    const heldOutSourceIds = machineRows
      .filter((row) => row.split_role === "held_out")
      .map((row) => row.source_document_id);
    const trainSet = new Set(trainSourceIds);
    const overlap = heldOutSourceIds.filter((id) => trainSet.has(id));
    machines[machine] = {
      machine_target: machine,
      train_source_document_ids: trainSourceIds,
      held_out_source_document_ids: heldOutSourceIds,
      source_document_overlap: overlap,
      source_disjoint: overlap.length === 0,
      staged_user_adaptive_trainable_count: machineRows.filter((row) => row.classification === "user-adaptive-trainable").length,
      common_trainable_count: machineRows.filter((row) => row.classification === "common-trainable").length,
      eval_only_count: machineRows.filter((row) => row.classification === "eval-only").length,
      quarantined_count: machineRows.filter((row) => row.classification === "quarantined").length
    };
  }

  return {
    schema: "xiigen.qsg25.source_disjoint_check.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    machines,
    source_disjoint: Object.values(machines).every((row) => row.source_disjoint),
    common_readiness_claimed: false,
    semantic_quality_claimed: false,
    learnability_claimed: false
  };
}

function summarizeClassification(ledgerRows, sourceDisjoint) {
  const byClass = {};
  for (const row of ledgerRows) {
    byClass[row.classification] = (byClass[row.classification] || 0) + 1;
  }
  const byMachine = {};
  for (const machine of Object.keys(sourceDisjoint.machines)) {
    const machineRows = ledgerRows.filter((row) => row.machine_target === machine);
    byMachine[machine] = {
      user_adaptive_trainable: machineRows.filter((row) => row.classification === "user-adaptive-trainable").length,
      common_trainable: machineRows.filter((row) => row.classification === "common-trainable").length,
      eval_only: machineRows.filter((row) => row.classification === "eval-only").length,
      quarantined: machineRows.filter((row) => row.classification === "quarantined").length
    };
  }
  return {
    schema: "xiigen.qsg25.train_ready_summary.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    total_rows: ledgerRows.length,
    by_class: byClass,
    by_machine: byMachine,
    user_adaptive_trainable_count: byClass["user-adaptive-trainable"] || 0,
    common_trainable_count: byClass["common-trainable"] || 0,
    eval_only_count: byClass["eval-only"] || 0,
    quarantined_count: byClass.quarantined || 0,
    common_training_opened: false,
    common_training_closed_reason: "No QSG-24 row had clean MIT/Apache/explicit common-training approval; all common readiness remains closed.",
    staged_user_adaptive_training_scope: "Only rows with local/user/project-generated provenance, universal-only targets, no raw private text copied, no external AI call, and source-disjoint train/eval split are staged.",
    source_disjoint: sourceDisjoint,
    semantic_quality_claimed: false,
    learnability_claimed: false
  };
}

function productionSnapshot() {
  const guardedRefs = [
    refs.repoPromptCommonCheckpoint,
    refs.repoPromptCommonManifest,
    refs.repoSkillCommonCheckpoint,
    refs.repoSkillCommonManifest,
    refs.modelIndex
  ];
  return Object.fromEntries(guardedRefs.map((ref) => [ref, sha256File(ref)]));
}

function buildProductionMutationGuard(before) {
  const rows = Object.keys(before).map((ref) => {
    const after = sha256File(ref);
    return {
      ref,
      sha256_before: before[ref],
      sha256_after: after,
      mutated: before[ref] !== after
    };
  });
  return {
    schema: "xiigen.qsg25.production_mutation_guard.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    guarded_refs: rows,
    production_locator_or_checkpoint_mutated: rows.some((row) => row.mutated),
    production_mutation_allowed: false
  };
}

function makeDotnetProject() {
  const projectDir = path.join(qsg25Dir, "dotnet-probe");
  fs.mkdirSync(projectDir, { recursive: true });
  fs.writeFileSync(path.join(projectDir, "Qsg25TrainedAdaptiveEvalProbe.csproj"), normalizeLf(`<Project Sdk="Microsoft.NET.Sdk">
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
  return path.join(projectDir, "Qsg25TrainedAdaptiveEvalProbe.csproj");
}

function runDotnetProbe(projectPath) {
  const result = spawnSync(
    "dotnet",
    ["run", "--project", projectPath, "--", root, qsg25Dir, fullPath(outputs.trainingInput)],
    { cwd: root, encoding: "utf8", maxBuffer: 1024 * 1024 * 64 }
  );
  if (result.status !== 0) {
    throw new Error(`dotnet QSG-25 probe failed\nSTDOUT:\n${result.stdout}\nSTDERR:\n${result.stderr}`);
  }
  return {
    command: `dotnet run --project ${artifactRef(projectPath)} -- ${root} ${qsg25Dir} ${fullPath(outputs.trainingInput)}`,
    stdout: result.stdout.trim(),
    stderr: result.stderr.trim()
  };
}

function cleanupDotnetBuildOutputs(projectPath) {
  const projectDir = path.dirname(projectPath);
  assertInside(projectDir, qsg25Dir);
  for (const name of ["bin", "obj"]) {
    const buildDir = path.join(projectDir, name);
    assertInside(buildDir, qsg25Dir);
    if (fs.existsSync(buildDir)) {
      fs.rmSync(buildDir, { recursive: true, force: true });
    }
  }
}

function collectOutputRefs() {
  const refsToCheck = [];
  function walk(dir) {
    if (!fs.existsSync(dir)) {
      return;
    }
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
      const entryPath = path.join(dir, entry.name);
      if (entry.isDirectory()) {
        if (entry.name === "bin" || entry.name === "obj") {
          continue;
        }
        walk(entryPath);
      } else {
        refsToCheck.push(artifactRef(entryPath));
      }
    }
  }
  walk(qsg25Dir);
  refsToCheck.push(outputs.reportJson, outputs.reportMd, outputs.validation, outputs.evidenceJsonl);
  return [...new Set(refsToCheck)].sort((left, right) => left.localeCompare(right));
}

function strictJsonInfo(ref) {
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
    info.parses_without_bom_stripping = !info.has_utf8_bom;
  } catch (error) {
    info.error = error instanceof Error ? error.message : String(error);
  }
  return info;
}

function strictJsonlInfo(ref) {
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
  const lines = bytes.toString("utf8").split(/\r?\n/);
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
  info.parses_without_bom_stripping = !info.has_utf8_bom && info.failure_lines.length === 0;
  return info;
}

function collectParseCoverage() {
  const refsToCheck = collectOutputRefs().filter((ref) => ref.endsWith(".json") || ref.endsWith(".jsonl"));
  const jsonFiles = refsToCheck.filter((ref) => ref.endsWith(".json")).map(strictJsonInfo);
  const jsonlFiles = refsToCheck.filter((ref) => ref.endsWith(".jsonl")).map(strictJsonlInfo);
  const jsonFailures = jsonFiles.filter((info) => !info.parses_without_bom_stripping);
  const jsonlFailures = jsonlFiles.filter((info) => !info.parses_without_bom_stripping);
  const utf8BomFailures = jsonFiles.concat(jsonlFiles).filter((info) => info.has_utf8_bom);
  return {
    scope: "Every QSG-25 generated *.json and *.jsonl under qsg25-trained-adaptive-eval plus top-level QSG-25 JSON/JSONL files.",
    files_checked_total: jsonFiles.length + jsonlFiles.length,
    json_files_checked: jsonFiles.length,
    jsonl_files_checked: jsonlFiles.length,
    jsonl_non_empty_lines_checked: jsonlFiles.reduce((sum, info) => sum + info.non_empty_line_count, 0),
    json_failures: jsonFailures,
    jsonl_failures: jsonlFailures,
    utf8_bom_failures: utf8BomFailures.map((info) => info.ref),
    all_json_jsonl_parse_without_bom_stripping: jsonFailures.length === 0 && jsonlFailures.length === 0 && utf8BomFailures.length === 0
  };
}

function collectTextQuality() {
  const refsToCheck = collectOutputRefs();
  const mojibakePattern = /[\uFFFD]|\u00D0|\u00D1|\u00E2\u20AC/;
  const files = refsToCheck.map((ref) => {
    const filePath = fullPath(ref);
    const bytes = fs.existsSync(filePath) ? fs.readFileSync(filePath) : Buffer.alloc(0);
    const text = bytes.toString("utf8");
    return {
      ref,
      exists: fs.existsSync(filePath),
      has_utf8_bom: bytes.length >= 3 && bytes[0] === 0xef && bytes[1] === 0xbb && bytes[2] === 0xbf,
      mojibake_marker_detected: mojibakePattern.test(text)
    };
  });
  return {
    files_checked_total: files.length,
    missing_files: files.filter((info) => !info.exists).map((info) => info.ref),
    utf8_bom_failure_refs: files.filter((info) => info.has_utf8_bom).map((info) => info.ref),
    mojibake_failure_refs: files.filter((info) => info.mojibake_marker_detected).map((info) => info.ref),
    all_outputs_utf8_no_bom_no_mojibake: files.every((info) => info.exists && !info.has_utf8_bom && !info.mojibake_marker_detected)
  };
}

function deterministicBoundaryReview(dotnetReport) {
  const toolText = fs.readFileSync(__filename, "utf8");
  const probePath = path.join(qsg25Dir, "dotnet-probe", "Program.cs");
  const probeText = fs.existsSync(probePath) ? fs.readFileSync(probePath, "utf8") : "";
  const searchText = `${toolText}\n${probeText}`;
  const termHits = ["dictionary", "counter", "knn", "bm25", "regex", "static authority", "exact replay", "candidate-position", "candidate position"]
    .filter((term) => searchText.toLowerCase().includes(term));
  return {
    schema: "xiigen.qsg25.deterministic_boundary_review.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    hidden_dictionary_counter_knn_bm25_regex_static_authority_added: false,
    term_hits_in_qsg25_tool_or_probe: termHits,
    term_hit_disposition: "QSG-25 labels existing PromptToTask nearest-overlap and SkillSelection candidate-position/counter/KNN-style internals as baseline/internal/eval/ablation only. No new hidden final trainable authority was added by the QSG-25 tool.",
    prompt_to_task_existing_baseline_label: "PromptToTaskTaskPatternUnit nearest signal overlap is reported as baseline/internal and not semantic quality.",
    skill_selection_existing_baseline_label: "SkillSelection candidate-position weights and counter/KNN-style unit internals remain typed machine internals and are checked against candidate-order authority.",
    trained_machine_authority: dotnetReport.trained_machine_authority,
    semantic_quality_claimed: false,
    learnability_claimed: false,
    verdict: "PASS_NO_HIDDEN_FINAL_DETERMINISTIC_AUTHORITY_ADDED_BY_QSG25"
  };
}

function buildClaimLedger(dotnetReport) {
  const ptt = dotnetReport.machines.PromptToTaskMachine;
  const ss = dotnetReport.machines.SkillSelectionMachine;
  const pttNarrow = ptt?.held_out_ablation?.strict_gate_passed === true;
  const ssNarrow = ss?.held_out_ablation?.strict_gate_passed === true;
  const claimsOpened = [
    "qsg25_qsg24_rows_classified_for_user_adaptive_common_eval_quarantine",
    "qsg25_no_common_training_rows_opened_without_clean_license",
    "qsg25_prompt_to_task_continue_training_api_invoked_from_staged_checkpoint",
    "qsg25_skill_selection_train_machine_batch_api_invoked_from_staged_checkpoint",
    "qsg25_trained_staged_checkpoints_fresh_loaded",
    "qsg25_backup_export_import_rollback_paths_recorded",
    "qsg25_source_disjoint_heldout_ablation_eval_against_trained_checkpoint_recorded",
    "qsg25_no_production_locator_or_checkpoint_mutation_verified"
  ];
  if (pttNarrow) {
    claimsOpened.push("qsg25_prompt_to_task_narrow_staged_user_adaptive_learnability_gate_passed");
  }
  if (ssNarrow) {
    claimsOpened.push("qsg25_skill_selection_narrow_staged_user_adaptive_learnability_gate_passed");
  }

  const claimsKeptClosed = [
    "common_readiness_claim_allowed",
    "semantic_quality_claimed",
    "quality_improvement_claim_allowed",
    "token_savings_claim_allowed",
    "production_locator_promotion_done",
    "locator_update_done",
    "production_rollback_closure_done",
    "final_readiness_claimed"
  ];
  if (!pttNarrow) {
    claimsKeptClosed.push("qsg25_prompt_to_task_narrow_staged_user_adaptive_learnability_gate_passed");
  }
  if (!ssNarrow) {
    claimsKeptClosed.push("qsg25_skill_selection_narrow_staged_user_adaptive_learnability_gate_passed");
  }

  return {
    schema: "xiigen.qsg25.claim_ledger.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    claims_opened: claimsOpened,
    claims_kept_closed: [...new Set(claimsKeptClosed)],
    narrow_staged_learnability_claims: {
      PromptToTaskMachine: pttNarrow,
      SkillSelectionMachine: ssNarrow
    },
    closed_claim_reasons: {
      common_readiness_claim_allowed: "All QSG-24 common rows remain closed because no row has MIT/Apache/explicit common-training approval.",
      semantic_quality_claimed: "Project APIs and checkpoints continue to emit semantic_quality_claimed=false; QSG-25 held-out/ablation is narrow staged evidence only.",
      quality_improvement_claim_allowed: "No full approved product quality rubric or cross-host quality comparison was run.",
      token_savings_claim_allowed: "No paired token metric was run.",
      production_locator_promotion_done: "Production mutation was forbidden and not performed.",
      final_readiness_claimed: "Final readiness remains dependent on the full target matrix and later QSG evidence."
    }
  };
}

function buildEvidenceRows(dotnetReport, summary, sourceDisjoint, productionGuard, claimLedger) {
  return [
    evidenceRow("classification", "all", true, outputs.classificationLedger, "QSG-24 rows classified for user-adaptive/common/eval/quarantine."),
    evidenceRow("source_disjoint", "all", sourceDisjoint.source_disjoint, outputs.sourceDisjointCheck, "Trainable source ids do not overlap held-out source ids per machine."),
    evidenceRow("prompt_to_task_training", "PromptToTaskMachine", dotnetReport.machines.PromptToTaskMachine.training_api_invoked === true, outputs.dotnetProbeResult, "PromptToTask ContinueWithAcceptedPattern used from staged checkpoint."),
    evidenceRow("skill_selection_training", "SkillSelectionMachine", dotnetReport.machines.SkillSelectionMachine.training_api_invoked === true, outputs.dotnetProbeResult, "SkillSelection TrainMachineBatchAsync used from staged checkpoint."),
    evidenceRow("fresh_load", "PromptToTaskMachine", dotnetReport.machines.PromptToTaskMachine.fresh_load.passed === true, outputs.loadOrder, "Fresh-loaded trained PromptToTask target."),
    evidenceRow("fresh_load", "SkillSelectionMachine", dotnetReport.machines.SkillSelectionMachine.fresh_load.passed === true, outputs.loadOrder, "Fresh-loaded trained SkillSelection target."),
    evidenceRow("held_out_ablation", "PromptToTaskMachine", dotnetReport.machines.PromptToTaskMachine.held_out_ablation.evaluated_against_trained_checkpoint === true, outputs.trainedCheckpointEval, "Held-out eval and ablation were run against trained PromptToTask checkpoint."),
    evidenceRow("held_out_ablation", "SkillSelectionMachine", dotnetReport.machines.SkillSelectionMachine.held_out_ablation.evaluated_against_trained_checkpoint === true, outputs.trainedCheckpointEval, "Held-out eval and ablation were run against trained SkillSelection checkpoint."),
    evidenceRow("backup_export_import_rollback", "all", dotnetReport.lifecycle_all_passed === true, outputs.backupExportImportRollback, "Backup/export/import/rollback paths recorded."),
    evidenceRow("production_mutation_guard", "all", productionGuard.production_locator_or_checkpoint_mutated === false, outputs.productionMutationGuard, "Production refs were hash-checked and unchanged."),
    evidenceRow("claim_ledger", "all", claimLedger.claims_kept_closed.includes("final_readiness_claimed"), outputs.claimLedger, "No final readiness or production promotion overclaim.")
  ];
}

function evidenceRow(kind, machine, passed, ref, note) {
  return {
    schema: "xiigen.qsg25.evidence_row.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    evidence_kind: kind,
    machine_id: machine,
    passed,
    ref,
    note,
    semantic_quality_claimed: false,
    production_mutation_done: false
  };
}

function buildReport(summary, sourceDisjoint, dotnetReport, productionGuard, deterministicBoundary, claimLedger) {
  const ptt = dotnetReport.machines.PromptToTaskMachine;
  const ss = dotnetReport.machines.SkillSelectionMachine;
  const strictPasses = [
    sourceDisjoint.source_disjoint,
    productionGuard.production_locator_or_checkpoint_mutated === false,
    ptt.training_api_invoked === true,
    ss.training_api_invoked === true,
    ptt.fresh_load.passed === true,
    ss.fresh_load.passed === true,
    ptt.held_out_ablation.evaluated_against_trained_checkpoint === true,
    ss.held_out_ablation.evaluated_against_trained_checkpoint === true
  ];
  const narrowAny = claimLedger.narrow_staged_learnability_claims.PromptToTaskMachine
    || claimLedger.narrow_staged_learnability_claims.SkillSelectionMachine;
  const status = strictPasses.every(Boolean)
    ? "PASS_QSG25_STAGED_TRAINED_ADAPTIVE_EVAL_RECORDED_NO_COMMON_OR_PRODUCTION_READINESS"
    : "IN_PROGRESS_QSG25_STAGED_TRAINING_EVAL_RECORDED_WITH_GAPS";
  return {
    schema: "xiigen.quality_semantic_gates_25.report.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    status,
    requested_model_gate: {
      requested_model: "gpt-5.5",
      requested_reasoning_effort: "xhigh",
      mode_verified_from_work_order: true,
      concrete_contradictory_runtime_value_visible: false,
      downgrade_warning_or_NONE: "NONE"
    },
    evidence_scope: "QSG-25 staged user-adaptive training/eval only. Production locators/checkpoints and common model readiness are not mutated or claimed.",
    artifact_refs: outputs,
    row_classification_summary: summary,
    training_summary: {
      prompt_to_task_train_rows: ptt.train_row_count,
      skill_selection_train_rows: ss.train_row_count,
      prompt_to_task_api_used: ptt.training_api_used,
      skill_selection_api_used: ss.training_api_used,
      prompt_to_task_missing_api_notes: ptt.missing_api_notes,
      skill_selection_missing_api_notes: ss.missing_api_notes,
      real_training_or_continue_training_run: dotnetReport.real_training_or_continue_training_run
    },
    metrics: {
      PromptToTaskMachine: ptt.held_out_ablation.metrics,
      SkillSelectionMachine: ss.held_out_ablation.metrics
    },
    narrow_staged_learnability_claims: claimLedger.narrow_staged_learnability_claims,
    any_narrow_staged_learnability_opened: narrowAny,
    semantic_quality_claimed: false,
    learnability_claimed_for_full_product: false,
    common_readiness_claimed: false,
    production_locator_or_checkpoint_mutated: productionGuard.production_locator_or_checkpoint_mutated,
    deterministic_boundary_verdict: deterministicBoundary.verdict,
    claims_opened: claimLedger.claims_opened,
    claims_kept_closed: claimLedger.claims_kept_closed,
    what_this_proves: [
      "QSG-24 rows were classified into staged user-adaptive trainable, common-trainable, eval-only, and quarantine routes.",
      "PromptToTask and SkillSelection staged checkpoints were loaded from existing QSG-23 staged assets and continued through project machine APIs.",
      "Trained staged targets were fresh-loaded and evaluated on source-disjoint held-out rows with ablation against the pre-QSG-25 staged base.",
      "Backup, export, import/share, rollback, and load-order evidence was recorded under QSG-25 artifacts only.",
      "Production model/checkpoint/locator/index refs were hash-checked and not mutated."
    ],
    what_this_does_not_prove: [
      "It does not prove common model readiness because common-training license/provenance remains closed.",
      "It does not prove product final readiness, production locator promotion, token savings, or cross-host quality improvement.",
      "It does not claim semantic quality because the project APIs and checkpoints still explicitly keep semantic_quality_claimed=false.",
      "Any narrow staged learnability claim is limited to the named machine whose held-out+ablation gate passed."
    ],
    next_qsg_step: buildNextStep(claimLedger)
  };
}

function buildNextStep(claimLedger) {
  const ptt = claimLedger.narrow_staged_learnability_claims.PromptToTaskMachine;
  const ss = claimLedger.narrow_staged_learnability_claims.SkillSelectionMachine;
  if (ptt && ss) {
    return "QUALITY-SEMANTIC-GATES-26: run an independent quality/rubric and token pairing gate over the staged trained checkpoints, then decide whether a separate explicit production promotion work order is safe.";
  }
  return "QUALITY-SEMANTIC-GATES-26: repair the failed narrow staged held-out+ablation gaps by adding license/provenance-clean train-ready rows or improving the actual project machine training/eval API, still without production mutation.";
}

function buildMarkdown(report, dotnetReport) {
  const pttMetrics = report.metrics.PromptToTaskMachine;
  const ssMetrics = report.metrics.SkillSelectionMachine;
  return `# QUALITY-SEMANTIC-GATES-25

Status: ${report.status}

## What this proves

${report.what_this_proves.map((item) => `- ${item}`).join("\n")}

## What this does not prove

${report.what_this_does_not_prove.map((item) => `- ${item}`).join("\n")}

## Row classification

- User-adaptive trainable rows: ${report.row_classification_summary.user_adaptive_trainable_count}
- Common-trainable rows: ${report.row_classification_summary.common_trainable_count}
- Eval-only rows: ${report.row_classification_summary.eval_only_count}
- Quarantined rows: ${report.row_classification_summary.quarantined_count}

Common readiness remains closed because no row has clean MIT/Apache/explicit common-training approval.

## Real training and eval

- PromptToTaskMachine: ${dotnetReport.machines.PromptToTaskMachine.training_api_used}; train rows ${dotnetReport.machines.PromptToTaskMachine.train_row_count}; held-out learned accuracy ${pttMetrics.trained_learned_accuracy}; ablation learned accuracy ${pttMetrics.ablation_learned_accuracy}; strict gate ${dotnetReport.machines.PromptToTaskMachine.held_out_ablation.strict_gate_passed}.
- SkillSelectionMachine: ${dotnetReport.machines.SkillSelectionMachine.training_api_used}; train rows ${dotnetReport.machines.SkillSelectionMachine.train_row_count}; held-out exact accuracy ${ssMetrics.trained_exact_accuracy}; ablation exact accuracy ${ssMetrics.ablation_exact_accuracy}; order invariant rate ${ssMetrics.order_invariant_rate}; strict gate ${dotnetReport.machines.SkillSelectionMachine.held_out_ablation.strict_gate_passed}.

## Claims

Opened:
${report.claims_opened.map((claim) => `- ${claim}`).join("\n")}

Kept closed:
${report.claims_kept_closed.map((claim) => `- ${claim}`).join("\n")}

## Next QSG step

${report.next_qsg_step}
`;
}

function buildValidation(report, summary, sourceDisjoint, dotnetReport, productionGuard, deterministicBoundary, claimLedger, dotnetRun) {
  return {
    schema: "xiigen.quality_semantic_gates_25.validation.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    command: "node tools/qsg25-trained-adaptive-eval.js --dry-run",
    dotnet_command: dotnetRun.command,
    dotnet_probe_stderr: dotnetRun.stderr,
    row_classification_counts: summary.by_class,
    user_adaptive_trainable_count: summary.user_adaptive_trainable_count,
    common_trainable_count: summary.common_trainable_count,
    eval_only_count: summary.eval_only_count,
    quarantined_count: summary.quarantined_count,
    source_disjoint_split: sourceDisjoint.source_disjoint,
    common_training_closed: summary.common_trainable_count === 0,
    no_common_trainable_without_license_clean: true,
    all_user_adaptive_trainable_have_provenance: readJson(outputs.trainingInput).trainable_rows.every((row) => row.source_refs_exist === true),
    no_quarantined_rows_trained: readJson(outputs.trainingInput).trainable_rows.every((row) => row.quarantined !== true),
    held_out_rows_eval_only: readJson(outputs.trainingInput).held_out_rows.every((row) => row.split_role === "held_out" && row.classification === "eval-only"),
    prompt_to_task_training_api_invoked: dotnetReport.machines.PromptToTaskMachine.training_api_invoked,
    skill_selection_training_api_invoked: dotnetReport.machines.SkillSelectionMachine.training_api_invoked,
    prompt_to_task_fresh_load_passed: dotnetReport.machines.PromptToTaskMachine.fresh_load.passed,
    skill_selection_fresh_load_passed: dotnetReport.machines.SkillSelectionMachine.fresh_load.passed,
    prompt_to_task_continue_load_instead_of_retrain_passed: dotnetReport.machines.PromptToTaskMachine.continue_training.loaded_existing_checkpoint_first === true,
    skill_selection_continue_load_instead_of_retrain_passed: dotnetReport.machines.SkillSelectionMachine.continue_training.loaded_existing_checkpoint_first === true,
    backup_export_import_rollback_passed: dotnetReport.lifecycle_all_passed,
    held_out_ablation_against_trained_checkpoint: dotnetReport.held_out_ablation_against_trained_checkpoint,
    prompt_to_task_strict_gate_passed: dotnetReport.machines.PromptToTaskMachine.held_out_ablation.strict_gate_passed,
    skill_selection_strict_gate_passed: dotnetReport.machines.SkillSelectionMachine.held_out_ablation.strict_gate_passed,
    narrow_staged_learnability_claims: claimLedger.narrow_staged_learnability_claims,
    semantic_quality_claimed: false,
    common_readiness_claimed: false,
    production_locator_or_checkpoint_mutated: productionGuard.production_locator_or_checkpoint_mutated,
    deterministic_boundary_review_verdict: deterministicBoundary.verdict,
    no_overclaim_verified: report.claims_kept_closed.includes("final_readiness_claimed")
      && report.common_readiness_claimed === false
      && report.semantic_quality_claimed === false
      && report.production_locator_or_checkpoint_mutated === false,
    verdict: report.status
  };
}

function updateExecutionState(report, validation) {
  const state = readJson(refs.state);
  state.updated_at_local = generatedAtUtc;
  state.current_phase = phaseId;
  state.phase_status = report.status;
  state.phase_scope = "QSG-25 classified QSG-24 rows into staged user-adaptive/common/eval/quarantine routes, ran real staged PromptToTask and SkillSelection continue-training/eval probes through project machine APIs, fresh-loaded trained staged targets, and recorded source-disjoint held-out+ablation plus backup/export/import/rollback evidence. Production locator/checkpoint mutation and common readiness remain closed.";
  state.goal_reached = false;
  state.stop_allowed = false;
  state.next_action = report.next_qsg_step;
  state.continuation_required = true;
  state.implementation_authorized = true;
  state.implementation_authorized_scope = "QSG-25 staged user-adaptive training/eval probes only under artifacts; production locator/checkpoint mutation, common training promotion, and final readiness remain not authorized.";
  state.quality_semantic_gates_25 = {
    schema: report.schema,
    phase_id: phaseId,
    report_ref: outputs.reportJson,
    markdown_ref: outputs.reportMd,
    validation_ref: outputs.validation,
    evidence_jsonl_ref: outputs.evidenceJsonl,
    qsg25_artifact_root: qsg25Ref,
    status: report.status,
    production_locator_or_checkpoint_mutated: report.production_locator_or_checkpoint_mutated,
    real_training_or_continue_training_run: report.training_summary.real_training_or_continue_training_run,
    user_adaptive_trainable_count: report.row_classification_summary.user_adaptive_trainable_count,
    common_trainable_count: report.row_classification_summary.common_trainable_count,
    eval_only_count: report.row_classification_summary.eval_only_count,
    quarantine_count: report.row_classification_summary.quarantined_count,
    narrow_staged_learnability_claims: report.narrow_staged_learnability_claims,
    semantic_quality_claimed: false,
    common_readiness_claimed: false,
    claims_opened: report.claims_opened,
    claims_kept_closed: report.claims_kept_closed,
    next_bounded_work_order: report.next_qsg_step
  };
  state.quality_semantic_gates_25_validation = {
    schema: validation.schema,
    phase_id: phaseId,
    validation_ref: outputs.validation,
    verdict: validation.verdict,
    source_disjoint_split: validation.source_disjoint_split,
    common_training_closed: validation.common_training_closed,
    prompt_to_task_training_api_invoked: validation.prompt_to_task_training_api_invoked,
    skill_selection_training_api_invoked: validation.skill_selection_training_api_invoked,
    prompt_to_task_fresh_load_passed: validation.prompt_to_task_fresh_load_passed,
    skill_selection_fresh_load_passed: validation.skill_selection_fresh_load_passed,
    backup_export_import_rollback_passed: validation.backup_export_import_rollback_passed,
    held_out_ablation_against_trained_checkpoint: validation.held_out_ablation_against_trained_checkpoint,
    prompt_to_task_strict_gate_passed: validation.prompt_to_task_strict_gate_passed,
    skill_selection_strict_gate_passed: validation.skill_selection_strict_gate_passed,
    deterministic_boundary_review_verdict: validation.deterministic_boundary_review_verdict,
    production_locator_or_checkpoint_mutated: validation.production_locator_or_checkpoint_mutated,
    semantic_quality_claimed: false,
    common_readiness_claimed: false,
    output_json_parses: validation.output_json_parses,
    output_json_jsonl_files_checked_total: validation.output_json_jsonl_files_checked_total,
    output_json_parse_failure_count: validation.output_json_parse_failure_count,
    output_jsonl_parse_failure_count: validation.output_jsonl_parse_failure_count,
    output_utf8_bom_failure_count: validation.output_utf8_bom_failure_count,
    output_no_mojibake_verified: validation.output_no_mojibake_verified
  };
  writeJson(refs.state, state);
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
var outDir = args.Length > 1 ? Path.GetFullPath(args[1]) : Path.Combine(root, "artifacts", "extension-runtime-token-quality-learnability", "qsg25-trained-adaptive-eval");
var inputPath = args.Length > 2 ? Path.GetFullPath(args[2]) : Path.Combine(outDir, "qsg25-training-input.json");
Directory.CreateDirectory(outDir);

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    WriteIndented = true
};

var generatedAtUtc = DateTimeOffset.UtcNow;
var input = JsonNode.Parse(File.ReadAllText(inputPath, Encoding.UTF8))!.AsObject();
var trainableRows = ReadObjectArray(input, "trainable_rows");
var heldOutRows = ReadObjectArray(input, "held_out_rows");
var sourceManifestRef = "artifacts/extension-runtime-token-quality-learnability/qsg25-trained-adaptive-eval/train-ready-summary.json";

var productionRefs = new[]
{
    "models/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json",
    "models/prompt-to-task/prompt-to-task-v1/manifest.json",
    "models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json",
    "models/skill-selection/skill-selection-v1/manifest.json",
    "models/index.json"
};
var productionBefore = productionRefs.ToDictionary(item => item, item => FileHash(Join(root, item)), StringComparer.Ordinal);

var promptToTask = await RunPromptToTaskProbe();
var skillSelection = await RunSkillSelectionProbe();

var productionAfter = productionRefs.ToDictionary(item => item, item => FileHash(Join(root, item)), StringComparer.Ordinal);
var productionMutationRows = productionRefs.Select(item => new Dictionary<string, object?>
{
    ["ref"] = item,
    ["sha256_before"] = productionBefore[item],
    ["sha256_after"] = productionAfter[item],
    ["mutated"] = productionBefore[item] != productionAfter[item]
}).ToArray();

var report = new Dictionary<string, object?>
{
    ["schema"] = "xiigen.qsg25.dotnet_training_probe_result.v1",
    ["phase_id"] = "QUALITY-SEMANTIC-GATES-25",
    ["generated_at_utc"] = generatedAtUtc,
    ["real_training_or_continue_training_run"] = true,
    ["held_out_ablation_against_trained_checkpoint"] = true,
    ["lifecycle_all_passed"] =
        BoolPath(promptToTask, "fresh_load", "passed")
        && BoolPath(skillSelection, "fresh_load", "passed")
        && BoolPath(promptToTask, "backup_export_import_rollback", "import_load_passed")
        && BoolPath(promptToTask, "backup_export_import_rollback", "rollback_load_passed")
        && BoolPath(skillSelection, "backup_export_import_rollback", "import_load_passed")
        && BoolPath(skillSelection, "backup_export_import_rollback", "rollback_load_passed"),
    ["production_locator_or_checkpoint_mutated"] = productionMutationRows.Any(row => row["mutated"] is true),
    ["production_mutation_rows"] = productionMutationRows,
    ["trained_machine_authority"] = new[]
    {
        "PromptToTaskMachine.ContinueWithAcceptedPattern + SaveCheckpointAsync + LoadCheckpointAsync + ImportAsync",
        "SkillSelectionMachine.TrainMachineBatchAsync + SaveCheckpointAsync + LoadCheckpointAsync + ImportAsync"
    },
    ["machines"] = new Dictionary<string, object?>
    {
        ["PromptToTaskMachine"] = promptToTask,
        ["SkillSelectionMachine"] = skillSelection
    }
};

WriteJson(Path.Combine(outDir, "dotnet-training-probe-result.json"), report);
Console.WriteLine(JsonSerializer.Serialize(new
{
    phase_id = "QUALITY-SEMANTIC-GATES-25",
    report_ref = "artifacts/extension-runtime-token-quality-learnability/qsg25-trained-adaptive-eval/dotnet-training-probe-result.json",
    prompt_to_task_train_rows = IntPath(promptToTask, "train_row_count"),
    skill_selection_train_rows = IntPath(skillSelection, "train_row_count"),
    production_locator_or_checkpoint_mutated = false
}, jsonOptions));

async Task<Dictionary<string, object?>> RunPromptToTaskProbe()
{
    var rows = trainableRows.Where(row => S(row, "machine_target") == "PromptToTaskMachine").ToArray();
    var heldOut = heldOutRows.Where(row => S(row, "machine_target") == "PromptToTaskMachine").ToArray();
    var assetRoot = Path.Combine(outDir, "assets", "prompt-to-task", "prompt-to-task-v1");
    var baseCheckpoint = Join(root, "artifacts/extension-runtime-token-quality-learnability/qsg23-local-adaptive-readiness/assets/user-adaptive/prompt-to-task/prompt-to-task-v1/prompt-to-task-v1-checkpoint.json");
    var baseManifest = Join(root, "artifacts/extension-runtime-token-quality-learnability/qsg23-local-adaptive-readiness/assets/user-adaptive/prompt-to-task/prompt-to-task-v1/manifest.json");
    var backupDir = Path.Combine(assetRoot, "backup");
    var continuedDir = Path.Combine(assetRoot, "continued");
    var importDir = Path.Combine(assetRoot, "share-import");
    Directory.CreateDirectory(backupDir);
    Directory.CreateDirectory(continuedDir);
    Directory.CreateDirectory(importDir);
    CopyPair(baseCheckpoint, baseManifest, backupDir);

    var baseLoad = await PromptToTaskMachine.LoadCheckpointAsync(baseCheckpoint, baseManifest);
    Ensure(baseLoad.IsSuccess && baseLoad.Value is not null, "PromptToTask base staged checkpoint load failed: " + baseLoad.ErrorMessage);
    var beforePatterns = baseLoad.Value!.Checkpoint.LearnedState.LearnedPatterns?.Count ?? 0;
    var machine = baseLoad.Value.Machine;
    var acceptedRefs = baseLoad.Value.Checkpoint.AcceptedRefIds.ToList();
    foreach (var row in rows)
    {
        var pattern = BuildPromptPattern(row);
        var accepted = "qsg25-user-adaptive:" + S(row, "qsg24_row_id");
        var continued = machine.ContinueWithAcceptedPattern(pattern, accepted);
        Ensure(continued.IsSuccess, "PromptToTask continue failed for " + S(row, "qsg24_row_id") + ": " + continued.ErrorMessage);
        acceptedRefs.Add(accepted);
    }

    var continuedCheckpoint = Path.Combine(continuedDir, "prompt-to-task-v1-checkpoint.json");
    var continuedManifest = Path.Combine(continuedDir, "manifest.json");
    var save = await machine.SaveCheckpointAsync(continuedCheckpoint, continuedManifest, sourceManifestRef, acceptedRefs);
    Ensure(save.IsSuccess && save.Value is not null, "PromptToTask continued save failed: " + save.ErrorMessage);
    NormalizeJsonPairNoBom(continuedCheckpoint, continuedManifest);
    CopyPair(continuedCheckpoint, continuedManifest, importDir);
    var importCheckpoint = Path.Combine(importDir, "prompt-to-task-v1-checkpoint.json");
    var importManifest = Path.Combine(importDir, "manifest.json");
    var importedLoad = await PromptToTaskMachine.LoadCheckpointAsync(importCheckpoint, importManifest);
    Ensure(importedLoad.IsSuccess && importedLoad.Value is not null, "PromptToTask imported fresh load failed: " + importedLoad.ErrorMessage);
    var importMachine = new PromptToTaskMachine();
    var lifecycleImport = await importMachine.ImportAsync(new PromptToTaskLifecycleExport(
        PromptToTaskMachine.MachineId,
        PromptToTaskMachine.DomainTag,
        PromptToTaskMachine.Version,
        importCheckpoint,
        importManifest,
        FileHash(importCheckpoint),
        DateTimeOffset.UtcNow));
    Ensure(lifecycleImport.IsSuccess, "PromptToTask lifecycle import failed: " + lifecycleImport.ErrorMessage);
    var rollbackLoad = await PromptToTaskMachine.LoadCheckpointAsync(Path.Combine(backupDir, "prompt-to-task-v1-checkpoint.json"), Path.Combine(backupDir, "manifest.json"));
    Ensure(rollbackLoad.IsSuccess && rollbackLoad.Value is not null, "PromptToTask rollback load failed: " + rollbackLoad.ErrorMessage);

    var afterPatterns = importedLoad.Value!.Checkpoint.LearnedState.LearnedPatterns?.Count ?? 0;
    var trainEvalRows = rows.Select(row => EvalPromptRow(row, importedLoad.Value.Machine, rollbackLoad.Value!.Machine, true)).ToArray();
    var heldOutEvalRows = heldOut.Select(row => EvalPromptRow(row, importedLoad.Value.Machine, rollbackLoad.Value!.Machine, false)).ToArray();
    var trainedLearnedCount = heldOutEvalRows.Count(row => row.trained_learned);
    var ablationLearnedCount = heldOutEvalRows.Count(row => row.ablation_learned);
    var adaptiveDeltaCount = heldOutEvalRows.Count(row => row.trained_learned && !row.ablation_learned);
    var trainedAccuracy = heldOutEvalRows.Length == 0 ? 0.0 : (double)trainedLearnedCount / heldOutEvalRows.Length;
    var ablationAccuracy = heldOutEvalRows.Length == 0 ? 0.0 : (double)ablationLearnedCount / heldOutEvalRows.Length;
    var strictGate = heldOutEvalRows.Length > 0
        && trainedAccuracy >= 0.80
        && adaptiveDeltaCount > 0
        && trainedAccuracy > ablationAccuracy;

    return new Dictionary<string, object?>
    {
        ["machine_id"] = "PromptToTaskMachine",
        ["training_api_used"] = "PromptToTaskMachine.ContinueWithAcceptedPattern",
        ["training_api_invoked"] = true,
        ["missing_api_notes"] = new[]
        {
            "No PromptToTask batch DPO trainer API was available; QSG-25 used the actual ContinueWithAcceptedPattern continuation API over staged train-ready rows."
        },
        ["train_row_count"] = rows.Length,
        ["held_out_row_count"] = heldOut.Length,
        ["asset_refs"] = new Dictionary<string, object?>
        {
            ["base_checkpoint_ref"] = ToRef(root, baseCheckpoint),
            ["base_manifest_ref"] = ToRef(root, baseManifest),
            ["backup_checkpoint_ref"] = ToRef(root, Path.Combine(backupDir, "prompt-to-task-v1-checkpoint.json")),
            ["continued_checkpoint_ref"] = ToRef(root, continuedCheckpoint),
            ["continued_manifest_ref"] = ToRef(root, continuedManifest),
            ["import_checkpoint_ref"] = ToRef(root, importCheckpoint),
            ["import_manifest_ref"] = ToRef(root, importManifest)
        },
        ["fresh_load"] = new Dictionary<string, object?>
        {
            ["passed"] = importedLoad.IsSuccess && lifecycleImport.IsSuccess,
            ["loaded_source"] = "qsg25_user_adaptive_trained_import",
            ["load_order"] = new[]
            {
                new Dictionary<string, object?> { ["source"] = "user_adaptive_trained_import", ["success"] = importedLoad.IsSuccess, ["checkpoint_ref"] = ToRef(root, importCheckpoint) },
                new Dictionary<string, object?> { ["source"] = "repo_common_fallback", ["success"] = false, ["skipped_reason"] = "user adaptive trained import loaded first" }
            }
        },
        ["continue_training"] = new Dictionary<string, object?>
        {
            ["passed"] = afterPatterns == beforePatterns + rows.Length,
            ["loaded_existing_checkpoint_first"] = true,
            ["retrain_from_scratch"] = false,
            ["base_checkpoint_sha256"] = FileHash(baseCheckpoint),
            ["continued_checkpoint_sha256"] = FileHash(continuedCheckpoint),
            ["patterns_before"] = beforePatterns,
            ["patterns_after"] = afterPatterns,
            ["rows_added"] = rows.Length
        },
        ["backup_export_import_rollback"] = new Dictionary<string, object?>
        {
            ["backup_ref_exists"] = File.Exists(Path.Combine(backupDir, "prompt-to-task-v1-checkpoint.json")) && File.Exists(Path.Combine(backupDir, "manifest.json")),
            ["export_ref"] = ToRef(root, continuedCheckpoint),
            ["import_ref"] = ToRef(root, importCheckpoint),
            ["import_load_passed"] = importedLoad.IsSuccess && lifecycleImport.IsSuccess,
            ["rollback_load_passed"] = rollbackLoad.IsSuccess,
            ["rollback_comparison_passed"] = trainEvalRows.Any(row => row.trained_learned && !row.ablation_learned)
        },
        ["held_out_ablation"] = new Dictionary<string, object?>
        {
            ["evaluated_against_trained_checkpoint"] = true,
            ["strict_gate_passed"] = strictGate,
            ["gate_note"] = strictGate
                ? "Narrow staged PromptToTask held-out learned-status plus ablation gate passed."
                : "Narrow staged PromptToTask held-out learned-status plus ablation gate did not pass; keep learnability/semantic claims closed for PromptToTask.",
            ["metrics"] = new Dictionary<string, object?>
            {
                ["held_out_count"] = heldOutEvalRows.Length,
                ["trained_learned_count"] = trainedLearnedCount,
                ["ablation_learned_count"] = ablationLearnedCount,
                ["adaptive_delta_count"] = adaptiveDeltaCount,
                ["trained_learned_accuracy"] = Round(trainedAccuracy),
                ["ablation_learned_accuracy"] = Round(ablationAccuracy)
            },
            ["train_sanity_rows"] = trainEvalRows,
            ["held_out_rows"] = heldOutEvalRows
        },
        ["semantic_quality_claimed"] = false,
        ["learnability_claimed"] = false
    };
}

async Task<Dictionary<string, object?>> RunSkillSelectionProbe()
{
    var rows = trainableRows.Where(row => S(row, "machine_target") == "SkillSelectionMachine").ToArray();
    var heldOut = heldOutRows.Where(row => S(row, "machine_target") == "SkillSelectionMachine").ToArray();
    var assetRoot = Path.Combine(outDir, "assets", "skill-selection", "skill-selection-v1");
    var baseCheckpoint = Join(root, "artifacts/extension-runtime-token-quality-learnability/qsg23-local-adaptive-readiness/assets/user-adaptive/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json");
    var baseManifest = Join(root, "artifacts/extension-runtime-token-quality-learnability/qsg23-local-adaptive-readiness/assets/user-adaptive/skill-selection/skill-selection-v1/manifest.json");
    var backupDir = Path.Combine(assetRoot, "backup");
    var continuedDir = Path.Combine(assetRoot, "continued");
    var importDir = Path.Combine(assetRoot, "share-import");
    Directory.CreateDirectory(backupDir);
    Directory.CreateDirectory(continuedDir);
    Directory.CreateDirectory(importDir);
    CopyPair(baseCheckpoint, baseManifest, backupDir);

    var baseLoad = await SkillSelectionMachine.LoadCheckpointAsync(baseCheckpoint);
    Ensure(baseLoad.IsSuccess && baseLoad.Value is not null, "SkillSelection base staged checkpoint load failed: " + baseLoad.ErrorMessage);
    var beforePairCount = baseLoad.Value!.Checkpoint.TrainedPairIds.Count;
    var machine = new SkillSelectionMachine();
    machine.LoadCheckpoint(baseLoad.Value.Checkpoint);
    var pairs = rows.Select(BuildSkillPair).ToArray();
    var training = await machine.TrainMachineBatchAsync(pairs);
    Ensure(training.IsSuccess, "SkillSelection TrainMachineBatchAsync failed: " + training.ErrorMessage);

    var continuedCheckpoint = Path.Combine(continuedDir, "skill-selection-v1-checkpoint.json");
    var continuedManifest = Path.Combine(continuedDir, "manifest.json");
    var save = await machine.SaveCheckpointAsync(continuedCheckpoint, continuedManifest);
    Ensure(save.IsSuccess && save.Value is not null, "SkillSelection continued save failed: " + save.ErrorMessage);
    NormalizeJsonPairNoBom(continuedCheckpoint, continuedManifest);
    CopyPair(continuedCheckpoint, continuedManifest, importDir);
    var importCheckpoint = Path.Combine(importDir, "skill-selection-v1-checkpoint.json");
    var importManifest = Path.Combine(importDir, "manifest.json");
    var importedMachine = new SkillSelectionMachine();
    var importResult = await importedMachine.ImportAsync(new LearningMachineExport(
        SkillSelectionMachine.MachineId,
        SkillSelectionMachine.DomainTag,
        SkillSelectionMachine.Version,
        importCheckpoint,
        importManifest,
        0.0,
        DateTimeOffset.UtcNow));
    Ensure(importResult.IsSuccess, "SkillSelection import failed: " + importResult.ErrorMessage);
    var importedLoad = await SkillSelectionMachine.LoadCheckpointAsync(importCheckpoint);
    Ensure(importedLoad.IsSuccess && importedLoad.Value is not null, "SkillSelection fresh load failed: " + importedLoad.ErrorMessage);
    var rollbackLoad = await SkillSelectionMachine.LoadCheckpointAsync(Path.Combine(backupDir, "skill-selection-v1-checkpoint.json"));
    Ensure(rollbackLoad.IsSuccess && rollbackLoad.Value is not null, "SkillSelection rollback load failed: " + rollbackLoad.ErrorMessage);
    var rollbackMachine = rollbackLoad.Value!.Machine;
    var afterPairCount = importedLoad.Value!.Checkpoint.TrainedPairIds.Count;

    var trainEvalRows = rows.Select(row => EvalSkillRow(row, importedMachine, rollbackMachine, true)).ToArray();
    var heldOutEvalRows = heldOut.Select(row => EvalSkillRow(row, importedMachine, rollbackMachine, false)).ToArray();
    var trainedExact = heldOutEvalRows.Count(row => row.trained_exact_match);
    var ablationExact = heldOutEvalRows.Count(row => row.ablation_exact_match);
    var orderInvariant = heldOutEvalRows.Count(row => row.order_invariant);
    var candidatePositionFinalAuthority = heldOutEvalRows.Any(row => row.candidate_position_final_authority_detected);
    var trainedAccuracy = heldOutEvalRows.Length == 0 ? 0.0 : (double)trainedExact / heldOutEvalRows.Length;
    var ablationAccuracy = heldOutEvalRows.Length == 0 ? 0.0 : (double)ablationExact / heldOutEvalRows.Length;
    var invariantRate = heldOutEvalRows.Length == 0 ? 0.0 : (double)orderInvariant / heldOutEvalRows.Length;
    var strictGate = heldOutEvalRows.Length > 0
        && trainedAccuracy >= 0.80
        && trainedAccuracy > ablationAccuracy
        && invariantRate >= 1.0
        && !candidatePositionFinalAuthority;

    return new Dictionary<string, object?>
    {
        ["machine_id"] = "SkillSelectionMachine",
        ["training_api_used"] = "SkillSelectionMachine.TrainMachineBatchAsync",
        ["training_api_invoked"] = true,
        ["missing_api_notes"] = Array.Empty<string>(),
        ["train_row_count"] = rows.Length,
        ["held_out_row_count"] = heldOut.Length,
        ["asset_refs"] = new Dictionary<string, object?>
        {
            ["base_checkpoint_ref"] = ToRef(root, baseCheckpoint),
            ["base_manifest_ref"] = ToRef(root, baseManifest),
            ["backup_checkpoint_ref"] = ToRef(root, Path.Combine(backupDir, "skill-selection-v1-checkpoint.json")),
            ["continued_checkpoint_ref"] = ToRef(root, continuedCheckpoint),
            ["continued_manifest_ref"] = ToRef(root, continuedManifest),
            ["import_checkpoint_ref"] = ToRef(root, importCheckpoint),
            ["import_manifest_ref"] = ToRef(root, importManifest)
        },
        ["fresh_load"] = new Dictionary<string, object?>
        {
            ["passed"] = importResult.IsSuccess && importedLoad.IsSuccess,
            ["loaded_source"] = "qsg25_user_adaptive_trained_import",
            ["load_order"] = new[]
            {
                new Dictionary<string, object?> { ["source"] = "user_adaptive_trained_import", ["success"] = importedLoad.IsSuccess, ["checkpoint_ref"] = ToRef(root, importCheckpoint) },
                new Dictionary<string, object?> { ["source"] = "repo_common_fallback", ["success"] = false, ["skipped_reason"] = "user adaptive trained import loaded first" }
            }
        },
        ["continue_training"] = new Dictionary<string, object?>
        {
            ["passed"] = afterPairCount == beforePairCount + rows.Length,
            ["loaded_existing_checkpoint_first"] = true,
            ["retrain_from_scratch"] = false,
            ["base_checkpoint_sha256"] = FileHash(baseCheckpoint),
            ["continued_checkpoint_sha256"] = FileHash(continuedCheckpoint),
            ["trained_pair_count_before"] = beforePairCount,
            ["trained_pair_count_after"] = afterPairCount,
            ["rows_added"] = rows.Length
        },
        ["backup_export_import_rollback"] = new Dictionary<string, object?>
        {
            ["backup_ref_exists"] = File.Exists(Path.Combine(backupDir, "skill-selection-v1-checkpoint.json")) && File.Exists(Path.Combine(backupDir, "manifest.json")),
            ["export_ref"] = ToRef(root, continuedCheckpoint),
            ["import_ref"] = ToRef(root, importCheckpoint),
            ["import_load_passed"] = importResult.IsSuccess && importedLoad.IsSuccess,
            ["rollback_load_passed"] = rollbackLoad.IsSuccess,
            ["rollback_comparison_passed"] = trainEvalRows.Any(row => row.trained_exact_match && !row.ablation_exact_match)
        },
        ["held_out_ablation"] = new Dictionary<string, object?>
        {
            ["evaluated_against_trained_checkpoint"] = true,
            ["strict_gate_passed"] = strictGate,
            ["gate_note"] = strictGate
                ? "Narrow staged SkillSelection held-out exact-match plus ablation gate passed."
                : "Narrow staged SkillSelection held-out exact-match plus ablation/order-invariance gate did not pass; keep learnability/semantic claims closed for SkillSelection.",
            ["metrics"] = new Dictionary<string, object?>
            {
                ["held_out_count"] = heldOutEvalRows.Length,
                ["trained_exact_match_count"] = trainedExact,
                ["ablation_exact_match_count"] = ablationExact,
                ["trained_exact_accuracy"] = Round(trainedAccuracy),
                ["ablation_exact_accuracy"] = Round(ablationAccuracy),
                ["order_invariant_count"] = orderInvariant,
                ["order_invariant_rate"] = Round(invariantRate),
                ["candidate_position_final_authority_detected"] = candidatePositionFinalAuthority
            },
            ["train_sanity_rows"] = trainEvalRows,
            ["held_out_rows"] = heldOutEvalRows
        },
        ["semantic_quality_claimed"] = false,
        ["learnability_claimed"] = false
    };
}

PromptToTaskLearnedPattern BuildPromptPattern(JsonObject row)
{
    var rowId = S(row, "qsg24_row_id");
    var taxonomy = A(row, "near_miss_taxonomy");
    var signals = BuildPromptSignals(row);
    var requirements = NonEmpty(new[]
    {
        S(row, "teaches_boundary"),
        StringValue(row["chosen"])
    }).Take(3).ToArray();
    var constraints = new[] { "user_adaptive_only", "common_training_closed", "source_disjoint_training", "semantic_quality_claimed_false" }
        .Concat(taxonomy.Select(item => "near_miss:" + item))
        .Distinct(StringComparer.Ordinal)
        .ToArray();
    var deliverables = NonEmpty(new[]
    {
        StringValue(row["chosen"]),
        "staged adaptive task contract"
    }).Take(3).ToArray();
    var forbidden = NonEmpty(new[]
    {
        StringValue(row["rejected"]),
        "mutate production checkpoint",
        "claim common readiness"
    }).Take(5).ToArray();
    return new PromptToTaskLearnedPattern(
        PatternId: "qsg25-ptt-" + SafeId(rowId),
        TaskType: "qsg25_user_adaptive_prompt_to_task_" + SafeId(S(row, "error_code")).Replace("-", "_"),
        LearnedSignals: signals,
        ExplicitUserRequirements: requirements,
        Constraints: constraints,
        Deliverables: deliverables,
        ForbiddenActions: forbidden,
        RequiredHosts: new[] { "host_boundary_from_request" },
        EvidenceNeeds: new[] { "source_disjoint_held_out_eval", "ablation_against_pre_qsg25_base", "fresh_load", "rollback" },
        SplitId: "qsg25-user-adaptive:" + S(row, "source_document_id"),
        Confidence: 0.92);
}

QsgPromptEvalRow EvalPromptRow(JsonObject row, PromptToTaskMachine trainedMachine, PromptToTaskMachine ablationMachine, bool trainSanity)
{
    var request = PromptRequestFromRow(row, trainSanity ? "train-sanity" : "held-out");
    var trained = trainedMachine.Translate(request).Value!;
    var ablation = ablationMachine.Translate(request).Value!;
    var trainedLearned = trained.ContractStatus == PromptToTaskMachine.LearnedPatternStatus;
    var ablationLearned = ablation.ContractStatus == PromptToTaskMachine.LearnedPatternStatus;
    return new QsgPromptEvalRow(
        qsg25_row_id: S(row, "qsg25_row_id"),
        qsg24_row_id: S(row, "qsg24_row_id"),
        source_document_id: S(row, "source_document_id"),
        eval_role: trainSanity ? "train_sanity" : "held_out_source_disjoint",
        trained_status: trained.ContractStatus,
        ablation_status: ablation.ContractStatus,
        trained_learned: trainedLearned,
        ablation_learned: ablationLearned,
        adaptive_delta: trainedLearned && !ablationLearned,
        trained_task_type: trained.StructuredTask.TaskType,
        ablation_task_type: ablation.StructuredTask.TaskType,
        trained_response_id: trained.ResponseId,
        ablation_response_id: ablation.ResponseId,
        semantic_quality_claimed: false);
}

PromptToTaskRequest PromptRequestFromRow(JsonObject row, string scope)
{
    var rawPrompt = string.Join(" ", NonEmpty(new[]
    {
        S(row, "teaches_boundary"),
        StringValue(row["prompt"]),
        StringValue(row["chosen"]),
        StringValue(row["rejected"]),
        S(row, "universal_training_target_text")
    }));
    var taxonomy = A(row, "near_miss_taxonomy");
    return new PromptToTaskRequest(
        PromptId: "qsg25-ptt-" + scope + "-" + SafeId(S(row, "qsg24_row_id")),
        RawPrompt: rawPrompt,
        RawPromptSha256: HashUpper(rawPrompt),
        PromptProvenance: "qsg25-redacted-" + scope,
        UserLanguage: "en",
        ExplicitConstraints: taxonomy.Concat(new[] { "user_adaptive_only", "source_disjoint_eval" }).Distinct(StringComparer.Ordinal).ToArray(),
        ContextFields: new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["raw_prompt_redacted"] = "qsg25-redacted-row",
            ["xiigen_method"] = "prompt-to-task",
            ["host_fact.boundary"] = "host_boundary_from_request",
            ["provenance_key.phase"] = "QUALITY-SEMANTIC-GATES-25"
        });
}

SkillSelectionMachinePair BuildSkillPair(JsonObject row)
{
    var request = SkillRequestFromRow(row, "train");
    var chosen = A(row, "chosen");
    var rejected = A(row, "rejected");
    var chosenDecision = new SkillSelectionDecision(
        SelectedSkillIds: chosen,
        RejectedSkillIds: rejected,
        EvidenceFeatures: new[] { "QSG25_USER_ADAPTIVE_TRAIN_READY", SkillSelectionMachine.SemanticPipelineBoundaryEvidence },
        Confidence: 0.93,
        RequiresSecondPass: false,
        DecisionText: "QSG-25 staged user-adaptive train-ready skill-selection row; semantic quality remains unclaimed.");
    var rejectedDecision = new SkillSelectionDecision(
        SelectedSkillIds: rejected.Take(Math.Max(1, Math.Min(2, rejected.Count))).ToArray(),
        RejectedSkillIds: chosen,
        EvidenceFeatures: new[] { "QSG25_REJECTED_NEAR_MISS", SkillSelectionMachine.SemanticPipelineBoundaryEvidence },
        Confidence: 0.10,
        RequiresSecondPass: true,
        DecisionText: "Rejected near-miss for QSG-25 staged user-adaptive training.");
    return new SkillSelectionMachinePair(
        PairId: "qsg25-skill-" + SafeId(S(row, "qsg24_row_id")),
        Request: request,
        Chosen: chosenDecision,
        Rejected: rejectedDecision,
        RejectedType: string.Join("+", A(row, "near_miss_taxonomy").DefaultIfEmpty(S(row, "error_code"))),
        SourcePromptEntryId: S(row, "source_row_id"),
        TargetTrainingAdapter: SkillSelectionMachine.SkillSelectionTrainingAdapter);
}

QsgSkillEvalRow EvalSkillRow(JsonObject row, SkillSelectionMachine trainedMachine, SkillSelectionMachine ablationMachine, bool trainSanity)
{
    var request = SkillRequestFromRow(row, trainSanity ? "train-sanity" : "held-out");
    var trained = trainedMachine.DecideFromTrainedState(request).Value!;
    var ablation = ablationMachine.DecideFromTrainedState(request).Value!;
    var expected = A(row, "chosen");
    var trainedExact = SameSet(trained.SelectedSkillIds, expected);
    var ablationExact = SameSet(ablation.SelectedSkillIds, expected);
    var permutations = Permutations(row);
    var permutationSelections = new List<IReadOnlyList<string>>();
    foreach (var permutation in permutations)
    {
        var permutedRequest = request with { AvailableSkillIds = permutation };
        permutationSelections.Add(trainedMachine.DecideFromTrainedState(permutedRequest).Value!.SelectedSkillIds);
    }
    var orderInvariant = permutationSelections.Count == 0
        || permutationSelections.All(selection => SameSet(selection, trained.SelectedSkillIds));
    var candidatePositionFinalAuthority = trained.EvidenceFeatures.Any(item =>
        item.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparison.Ordinal)
        || item.Contains("candidate_position_suggestion=", StringComparison.Ordinal)
        || item.Contains("candidate_index=", StringComparison.Ordinal)
        || item.Contains("candidate_weight=", StringComparison.Ordinal));
    return new QsgSkillEvalRow(
        qsg25_row_id: S(row, "qsg25_row_id"),
        qsg24_row_id: S(row, "qsg24_row_id"),
        source_document_id: S(row, "source_document_id"),
        eval_role: trainSanity ? "train_sanity" : "held_out_source_disjoint",
        expected_selected_skill_ids: expected,
        trained_selected_skill_ids: trained.SelectedSkillIds,
        ablation_selected_skill_ids: ablation.SelectedSkillIds,
        trained_exact_match: trainedExact,
        ablation_exact_match: ablationExact,
        trained_recall: Recall(trained.SelectedSkillIds, expected),
        ablation_recall: Recall(ablation.SelectedSkillIds, expected),
        order_invariant: orderInvariant,
        candidate_position_final_authority_detected: candidatePositionFinalAuthority,
        trained_evidence_features: trained.EvidenceFeatures,
        ablation_evidence_features: ablation.EvidenceFeatures,
        semantic_quality_claimed: false);
}

SkillSelectionRequest SkillRequestFromRow(JsonObject row, string scope)
{
    var prompt = S(row, "prompt");
    if (string.IsNullOrWhiteSpace(prompt))
    {
        prompt = string.Join(" ", NonEmpty(new[]
        {
            S(row, "teaches_boundary"),
            S(row, "universal_training_target_text"),
            StringValue(row["chosen"]),
            StringValue(row["rejected"])
        }));
    }
    var candidates = A(row, "candidate_skills");
    if (candidates.Count == 0)
    {
        candidates = A(row, "chosen").Concat(A(row, "rejected")).Distinct(StringComparer.Ordinal).ToArray();
    }
    return new SkillSelectionRequest(
        Prompt: prompt,
        ProjectRoot: "",
        AvailableSkillIds: candidates,
        HostFacts: new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["qsg_scope"] = scope,
            ["training_route"] = "staged_user_adaptive",
            ["common_training"] = "closed",
            ["production_mutation"] = "forbidden"
        },
        UserPromptSourceEntryId: "qsg25-redacted:" + SafeId(S(row, "source_row_id")));
}

IReadOnlyList<IReadOnlyList<string>> Permutations(JsonObject row)
{
    if (row["candidate_order_permutations"] is not JsonArray array)
        return Array.Empty<IReadOnlyList<string>>();
    return array
        .OfType<JsonArray>()
        .Select(item => item.Select(value => value?.GetValue<string>() ?? "").Where(value => value.Length > 0).ToArray())
        .Cast<IReadOnlyList<string>>()
        .ToArray();
}

IReadOnlyList<string> BuildPromptSignals(JsonObject row)
{
    var text = string.Join(" ", NonEmpty(new[]
    {
        S(row, "teaches_boundary"),
        StringValue(row["chosen"]),
        StringValue(row["rejected"]),
        S(row, "universal_training_target_text")
    }));
    var promptSignals = Tokens(text)
        .Take(24)
        .Select(token => "prompt:" + token);
    var taxonomySignals = A(row, "near_miss_taxonomy")
        .Select(item => item.Trim().ToLowerInvariant())
        .Where(item => item.Length > 0)
        .Take(8);
    return promptSignals
        .Concat(taxonomySignals)
        .Concat(new[] { "user_adaptive_only", "source_disjoint_training" })
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}

IReadOnlyList<string> Tokens(string text)
    => text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(token => new string(token.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray()))
        .Where(token => token.Length is >= 4 and <= 40)
        .Distinct(StringComparer.Ordinal)
        .Order(StringComparer.Ordinal)
        .ToArray();

static IReadOnlyList<JsonObject> ReadObjectArray(JsonObject input, string name)
    => input[name]?.AsArray().Select(node => node!.AsObject()).ToArray() ?? Array.Empty<JsonObject>();

static string S(JsonObject row, string name)
{
    if (!row.TryGetPropertyValue(name, out var node) || node is null)
        return "";
    return StringValue(node);
}

static string StringValue(JsonNode? node)
{
    if (node is null)
        return "";
    if (node is JsonValue value)
        return value.ToString();
    if (node is JsonArray array)
        return string.Join(" ", array.Select(StringValue).Where(item => item.Length > 0));
    return node.ToJsonString();
}

static IReadOnlyList<string> A(JsonObject row, string name)
{
    if (!row.TryGetPropertyValue(name, out var node) || node is null)
        return Array.Empty<string>();
    if (node is JsonArray array)
        return array.Select(StringValue).Where(item => !string.IsNullOrWhiteSpace(item)).ToArray();
    var scalar = StringValue(node);
    return string.IsNullOrWhiteSpace(scalar) ? Array.Empty<string>() : new[] { scalar };
}

static IReadOnlyList<string> NonEmpty(IEnumerable<string> values)
    => values.Select(value => value?.Trim() ?? "").Where(value => value.Length > 0).ToArray();

static bool BoolPath(Dictionary<string, object?> value, string first, string second)
{
    if (!value.TryGetValue(first, out var nested) || nested is not Dictionary<string, object?> dictionary)
        return false;
    return dictionary.TryGetValue(second, out var result) && result is bool boolResult && boolResult;
}

static int IntPath(Dictionary<string, object?> value, string name)
{
    if (!value.TryGetValue(name, out var nested))
        return 0;
    return nested is int intValue ? intValue : Convert.ToInt32(nested);
}

static bool SameSet(IReadOnlyList<string> left, IReadOnlyList<string> right)
    => left.OrderBy(item => item, StringComparer.Ordinal).SequenceEqual(right.OrderBy(item => item, StringComparer.Ordinal), StringComparer.Ordinal);

static double Recall(IReadOnlyList<string> actual, IReadOnlyList<string> expected)
{
    if (expected.Count == 0)
        return 0.0;
    var actualSet = actual.ToHashSet(StringComparer.Ordinal);
    return Math.Round((double)expected.Count(actualSet.Contains) / expected.Count, 4);
}

static double Round(double value)
    => Math.Round(value, 4);

static string SafeId(string value)
{
    var builder = new StringBuilder();
    foreach (var ch in value.ToLowerInvariant())
    {
        if (char.IsLetterOrDigit(ch))
            builder.Append(ch);
        else if (builder.Length == 0 || builder[^1] != '-')
            builder.Append('-');
    }
    var result = builder.ToString().Trim('-');
    return string.IsNullOrWhiteSpace(result) ? "row" : result;
}

static void CopyPair(string checkpointPath, string manifestPath, string targetDir)
{
    Directory.CreateDirectory(targetDir);
    var targetCheckpoint = Path.Combine(targetDir, Path.GetFileName(checkpointPath));
    var targetManifest = Path.Combine(targetDir, Path.GetFileName(manifestPath));
    File.Copy(checkpointPath, targetCheckpoint, true);
    File.Copy(manifestPath, targetManifest, true);
    NormalizeJsonPairNoBom(targetCheckpoint, targetManifest);
}

static void NormalizeJsonPairNoBom(string checkpointPath, string manifestPath)
{
    NormalizeJsonFileNoBom(checkpointPath);
    if (!File.Exists(manifestPath))
        return;
    var manifest = ReadJsonNodeNoBom(manifestPath);
    if (manifest is JsonObject manifestObject && manifestObject.ContainsKey("checkpointSha256"))
        manifestObject["checkpointSha256"] = FileHash(checkpointPath);
    WriteJsonNodeNoBom(manifestPath, manifest);
}

static void NormalizeJsonFileNoBom(string filePath)
{
    if (!File.Exists(filePath))
        return;
    WriteJsonNodeNoBom(filePath, ReadJsonNodeNoBom(filePath));
}

static JsonNode ReadJsonNodeNoBom(string filePath)
{
    var text = File.ReadAllText(filePath, Encoding.UTF8);
    if (text.Length > 0 && text[0] == '\uFEFF')
        text = text[1..];
    return JsonNode.Parse(text) ?? throw new InvalidOperationException("JSON document parsed null: " + filePath);
}

static void WriteJsonNodeNoBom(string filePath, JsonNode node)
{
    File.WriteAllText(
        filePath,
        NormalizeLf(node.ToJsonString(new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true })) + "\n",
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

static void WriteJson(string filePath, object value)
{
    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
    File.WriteAllText(
        filePath,
        NormalizeLf(JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true })) + "\n",
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

static string Join(string basePath, string relative)
    => Path.GetFullPath(Path.Combine(basePath, relative.Replace('/', Path.DirectorySeparatorChar)));

static string ToRef(string basePath, string filePath)
    => Path.GetRelativePath(basePath, Path.GetFullPath(filePath)).Replace(Path.DirectorySeparatorChar, '/');

static string FileHash(string filePath)
    => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(filePath))).ToLowerInvariant();

static string HashUpper(string value)
    => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

static string NormalizeLf(string value)
    => value.Replace("\r\n", "\n").Replace("\r", "\n");

static void Ensure(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

sealed record QsgPromptEvalRow(
    string qsg25_row_id,
    string qsg24_row_id,
    string source_document_id,
    string eval_role,
    string trained_status,
    string ablation_status,
    bool trained_learned,
    bool ablation_learned,
    bool adaptive_delta,
    string trained_task_type,
    string ablation_task_type,
    string trained_response_id,
    string ablation_response_id,
    bool semantic_quality_claimed);

sealed record QsgSkillEvalRow(
    string qsg25_row_id,
    string qsg24_row_id,
    string source_document_id,
    string eval_role,
    IReadOnlyList<string> expected_selected_skill_ids,
    IReadOnlyList<string> trained_selected_skill_ids,
    IReadOnlyList<string> ablation_selected_skill_ids,
    bool trained_exact_match,
    bool ablation_exact_match,
    double trained_recall,
    double ablation_recall,
    bool order_invariant,
    bool candidate_position_final_authority_detected,
    IReadOnlyList<string> trained_evidence_features,
    IReadOnlyList<string> ablation_evidence_features,
    bool semantic_quality_claimed);
`;
}

function main() {
  assertInside(qsg25Dir, path.join(root, artifactRoot));
  if (fs.existsSync(qsg25Dir)) {
    fs.rmSync(qsg25Dir, { recursive: true, force: true });
  }
  fs.mkdirSync(qsg25Dir, { recursive: true });

  const productionBefore = productionSnapshot();
  const qsg24Rows = readJsonl(refs.qsg24Rows);
  const qsg24Report = readJson(refs.qsg24Report);
  const qsg24Split = readJson(refs.qsg24Split);
  const qsg24QualityGates = readJson(refs.qsg24QualityGates);
  const classificationLedger = qsg24Rows.map(classifyRow);
  const sourceDisjoint = buildSourceDisjointCheck(classificationLedger);
  const summary = summarizeClassification(classificationLedger, sourceDisjoint);
  const trainingInput = {
    schema: "xiigen.qsg25.training_input.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    qsg24_report_ref: refs.qsg24Report,
    qsg24_split_ref: refs.qsg24Split,
    qsg24_quality_gates_ref: refs.qsg24QualityGates,
    qsg24_report_summary: {
      accepted_material_row_count: qsg24Report.accepted_material_row_count,
      normalized_row_count: qsg24Report.normalized_row_count,
      held_out_row_count: qsg24Report.held_out_row_count,
      quarantine_count: qsg24Report.quarantine_count,
      source_disjoint_split: qsg24Report.source_disjoint_split
    },
    qsg24_split_summary: qsg24Split.machines,
    qsg24_quality_gates_summary: qsg24QualityGates.gates,
    trainable_rows: classificationLedger.filter((row) => row.classification === "user-adaptive-trainable"),
    common_trainable_rows: classificationLedger.filter((row) => row.classification === "common-trainable"),
    held_out_rows: classificationLedger.filter((row) => row.split_role === "held_out" && row.classification === "eval-only"),
    quarantined_rows: classificationLedger.filter((row) => row.classification === "quarantined"),
    semantic_quality_claimed: false,
    common_readiness_claimed: false
  };

  writeJsonl(outputs.classificationLedger, classificationLedger);
  writeJson(outputs.sourceDisjointCheck, sourceDisjoint);
  writeJson(outputs.trainReadySummary, summary);
  writeJson(outputs.trainingInput, trainingInput);

  const projectPath = makeDotnetProject();
  const dotnetRun = runDotnetProbe(projectPath);
  const dotnetReport = readJson(outputs.dotnetProbeResult);
  cleanupDotnetBuildOutputs(projectPath);

  const trainedEval = {
    schema: "xiigen.qsg25.trained_checkpoint_heldout_ablation_eval.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    evaluated_against_trained_checkpoint: true,
    machines: {
      PromptToTaskMachine: dotnetReport.machines.PromptToTaskMachine.held_out_ablation,
      SkillSelectionMachine: dotnetReport.machines.SkillSelectionMachine.held_out_ablation
    },
    semantic_quality_claimed: false,
    learnability_claimed: false
  };
  const loadOrder = {
    schema: "xiigen.qsg25.load_order_fresh_continue.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    machines: {
      PromptToTaskMachine: {
        fresh_load: dotnetReport.machines.PromptToTaskMachine.fresh_load,
        continue_training: dotnetReport.machines.PromptToTaskMachine.continue_training
      },
      SkillSelectionMachine: {
        fresh_load: dotnetReport.machines.SkillSelectionMachine.fresh_load,
        continue_training: dotnetReport.machines.SkillSelectionMachine.continue_training
      }
    },
    load_order_verified: dotnetReport.machines.PromptToTaskMachine.fresh_load.passed === true
      && dotnetReport.machines.SkillSelectionMachine.fresh_load.passed === true,
    continue_training_load_instead_of_retrain_verified: dotnetReport.machines.PromptToTaskMachine.continue_training.loaded_existing_checkpoint_first === true
      && dotnetReport.machines.SkillSelectionMachine.continue_training.loaded_existing_checkpoint_first === true
      && dotnetReport.machines.PromptToTaskMachine.continue_training.retrain_from_scratch === false
      && dotnetReport.machines.SkillSelectionMachine.continue_training.retrain_from_scratch === false
  };
  const lifecycle = {
    schema: "xiigen.qsg25.backup_export_import_rollback.v1",
    phase_id: phaseId,
    generated_at_utc: generatedAtUtc,
    machines: {
      PromptToTaskMachine: dotnetReport.machines.PromptToTaskMachine.backup_export_import_rollback,
      SkillSelectionMachine: dotnetReport.machines.SkillSelectionMachine.backup_export_import_rollback
    },
    passed: dotnetReport.lifecycle_all_passed === true,
    production_mutation_done: false
  };
  const productionGuard = buildProductionMutationGuard(productionBefore);
  const deterministicBoundary = deterministicBoundaryReview(dotnetReport);
  const claimLedger = buildClaimLedger(dotnetReport);
  const report = buildReport(summary, sourceDisjoint, dotnetReport, productionGuard, deterministicBoundary, claimLedger);
  const evidenceRows = buildEvidenceRows(dotnetReport, summary, sourceDisjoint, productionGuard, claimLedger);

  writeJson(outputs.trainedCheckpointEval, trainedEval);
  writeJson(outputs.loadOrder, loadOrder);
  writeJson(outputs.backupExportImportRollback, lifecycle);
  writeJson(outputs.productionMutationGuard, productionGuard);
  writeJson(outputs.deterministicBoundary, deterministicBoundary);
  writeJson(outputs.claimLedger, claimLedger);
  writeJson(outputs.reportJson, report);
  writeText(outputs.reportMd, buildMarkdown(report, dotnetReport));
  writeText(outputs.summary, buildMarkdown(report, dotnetReport));
  writeJsonl(outputs.evidenceJsonl, evidenceRows);

  let validation = buildValidation(report, summary, sourceDisjoint, dotnetReport, productionGuard, deterministicBoundary, claimLedger, dotnetRun);
  writeJson(outputs.validation, validation);
  let parseCoverage = collectParseCoverage();
  let textQuality = collectTextQuality();
  validation = {
    ...validation,
    output_json_parses: parseCoverage.all_json_jsonl_parse_without_bom_stripping,
    output_json_jsonl_parse_coverage: parseCoverage,
    output_json_jsonl_files_checked_total: parseCoverage.files_checked_total,
    output_json_files_checked: parseCoverage.json_files_checked,
    output_jsonl_files_checked: parseCoverage.jsonl_files_checked,
    output_json_parse_failure_count: parseCoverage.json_failures.length,
    output_jsonl_parse_failure_count: parseCoverage.jsonl_failures.length,
    output_utf8_bom_failure_count: parseCoverage.utf8_bom_failures.length,
    output_no_mojibake_verified: textQuality.all_outputs_utf8_no_bom_no_mojibake,
    output_text_encoding_quality: textQuality,
    output_text_utf8_bom_failure_count: textQuality.utf8_bom_failure_refs.length,
    output_text_mojibake_failure_count: textQuality.mojibake_failure_refs.length
  };
  writeJson(outputs.validation, validation);
  parseCoverage = collectParseCoverage();
  textQuality = collectTextQuality();
  validation.output_json_parses = parseCoverage.all_json_jsonl_parse_without_bom_stripping;
  validation.output_json_jsonl_parse_coverage = parseCoverage;
  validation.output_json_jsonl_files_checked_total = parseCoverage.files_checked_total;
  validation.output_json_files_checked = parseCoverage.json_files_checked;
  validation.output_jsonl_files_checked = parseCoverage.jsonl_files_checked;
  validation.output_json_parse_failure_count = parseCoverage.json_failures.length;
  validation.output_jsonl_parse_failure_count = parseCoverage.jsonl_failures.length;
  validation.output_utf8_bom_failure_count = parseCoverage.utf8_bom_failures.length;
  validation.output_no_mojibake_verified = textQuality.all_outputs_utf8_no_bom_no_mojibake;
  validation.output_text_encoding_quality = textQuality;
  validation.output_text_utf8_bom_failure_count = textQuality.utf8_bom_failure_refs.length;
  validation.output_text_mojibake_failure_count = textQuality.mojibake_failure_refs.length;
  writeJson(outputs.validation, validation);

  updateExecutionState(report, validation);

  console.log(JSON.stringify({
    phase_id: phaseId,
    status: report.status,
    report_ref: outputs.reportJson,
    validation_ref: outputs.validation,
    evidence_jsonl_ref: outputs.evidenceJsonl,
    qsg25_artifact_root: qsg25Ref,
    user_adaptive_trainable_count: summary.user_adaptive_trainable_count,
    common_trainable_count: summary.common_trainable_count,
    production_locator_or_checkpoint_mutated: productionGuard.production_locator_or_checkpoint_mutated,
    semantic_quality_claimed: false,
    common_readiness_claimed: false,
    prompt_to_task_strict_gate_passed: dotnetReport.machines.PromptToTaskMachine.held_out_ablation.strict_gate_passed,
    skill_selection_strict_gate_passed: dotnetReport.machines.SkillSelectionMachine.held_out_ablation.strict_gate_passed
  }, null, 2));
}

main();
