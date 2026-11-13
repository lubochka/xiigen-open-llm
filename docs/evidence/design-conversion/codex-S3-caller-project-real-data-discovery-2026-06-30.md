# Codex S3 caller-project real-data discovery - 2026-06-30

claim_scope: read-only discovery packet for DesignConversion caller-project real-data evidence under the explicitly allowed `C:\Projects` paths. This packet does not ingest data, does not run tests/builds, does not claim readiness, and does not claim upload/training/promotion.
evidence_scope: `Test-Path`, path-limited `rg --files`, selected `rg -n`, and small text reads over `C:\Projects\FigmaToCode-design-gen`, `C:\Projects\CanvaToCode`, `C:\Projects\xiigen-flows-dot-net-angular-DesignToCode-s1b`, and `C:\Projects\llm_mvp_core` real-s3 scaffold/docs. PNG/JPG/binary assets were listed by path only, not opened.
count_claimed: 0 `usable_for_real_s3_bundle_now`; 1 caller project with `candidate_needs_mapping`.
independent_evidence_count: 8 read-only discovery command groups before packet write; validation is recorded separately below.
subagent_id_or_NONE: NONE
downgrade_needed: yes. The evidence supports only: FigmaToCode contains mapping-needed design-conversion fixture/debug materials; no inspected caller project currently provides a complete real-s3-v1 bundle with provenance/license, DPO rows, held-out measurement, and checkpoint/weights manifest.

figma_project_scanned=true
canva_project_scanned=true
usable_for_real_s3_bundle_now=false
candidate_needs_mapping=true
promotion_upload=blocked
C:\Users\lubao\Documents\xiigen_touch_forbidden=true

## Boundaries

- Worked only under `C:\Projects`.
- Did not touch `C:\Users\lubao\Documents\xiigen`.
- Did not touch `C:\Projects\xiigen-platform-dot-net-angular`.
- Did not edit caller repos.
- Did not edit `artifacts/**` or package files.
- Did not stage, commit, push, branch switch, create PR, run `gh`, browser/e2e, external training, or model upload.
- Did not run tests or builds.
- Wrote only this packet: `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-S3-caller-project-real-data-discovery-2026-06-30.md`.

## Scan Summary

All allowed target directories existed:

- `C:\Projects\FigmaToCode-design-gen`
- `C:\Projects\CanvaToCode`
- `C:\Projects\xiigen-flows-dot-net-angular-DesignToCode-s1b`
- `C:\Projects\llm_mvp_core`

The `real-s3-v1` scaffold in `C:\Projects\llm_mvp_core\training-data\design-conversion\real-s3-v1` is schema-only. Its README says it has no real source assets, DPO rows, trained weights, or held-out results, and keeps `promotion_upload=blocked`.

## Candidate Classification

### C:\Projects\FigmaToCode-design-gen

classification: `candidate_needs_mapping`

Rationale: this project contains source-like Figma scene fixtures, generated HTML/CSS, expected/actual preview screenshots, and layout/debug reports. It does not currently provide a complete real-s3-v1 bundle because provenance/license/authority, redacted source manifest, DPO JSONL rows, held-out measurement, and checkpoint/weights manifest are not present in the inspected candidate set.

Observed source design tree / original tree exports:

- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\scene-replay\travel-fixture.json`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\scene-replay\news-fixture.json`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\scene-replay\mail-fixture.json`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\scene-replay\login-fixture.json`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\scene-replay\form-fixture.json`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\scene-replay\FAQ-fixture.json`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\scene-replay\cart-fixture.json`
- `C:\Projects\FigmaToCode-design-gen\_tmp_debug_scenes\mail css-debug-history.json` contains serialized Figma node JSON in debug history search hits.
- Small read of `login-fixture.json` showed `sceneInput` with Figma-like node fields such as `id`, `parentId`, `name`, `type`, `x`, `y`, `width`, `height`, `fills`, `strokes`, and layout fields.

Observed generated HTML/CSS result:

- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\generated\mail.html`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\generated\mail.css`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\generated\login.html`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\generated\login.css`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\generated\cart.html`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\generated\cart.css`
- Small reads of `login.html` and `login.css` showed generated markup/CSS for the login scene.

Observed design screenshots / exports, listed only:

- `C:\Projects\FigmaToCode-design-gen\_tmp_debug_scenes\travel expected.png`
- `C:\Projects\FigmaToCode-design-gen\_tmp_debug_scenes\travel html preview.png`
- `C:\Projects\FigmaToCode-design-gen\_tmp_debug_scenes\travel react preview.png`
- Same pattern was listed for `news`, `mail`, `login`, `form`, and `cart`.

Observed rendered QA screenshots or visual diffs:

- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\generated\mail-layout-report.json`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\generated\login-layout-report.json`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\generated\cart-layout-report.json`
- Small read of `login-layout-report.json` showed QA-like counts and diffs: `total`, `found`, `missing`, `sizeCorrect`, `posCorrect`, and per-node `expectedPos`/`actualPos`.
- `C:\Projects\FigmaToCode-design-gen\docs\GEOMETRIC-LEARNING-SYSTEM.md` describes debug histories + expected PNGs as a labeled training dataset, but this is a plan/analysis doc, not a real-s3 bundle artifact.

Observed DPO/teacher labels/chosen-rejected rows:

- `insufficient_or_placeholder`.
- Direct category scan found no DPO/chosen/rejected/teacher-label files in the Figma candidate file set.
- `tests\fixtures\sessions\*.json` search hits contain `score` and `userFeedback` values such as `good`/`bad`, but these are not current `real-s3-v1` `dpo-rows.jsonl` rows with chosen/rejected safe refs, teacher labels, curriculum tier, validation outcome, provenance, and leakage guard.

Observed held-out measurement metrics:

- `insufficient_or_placeholder`.
- Layout reports are visual QA/debug evidence, not held-out measurement objects matching `heldout-measurements.schema.json`.

Observed checkpoint/weights manifests:

- `insufficient_or_placeholder`.
- Only a generic `manifest.json` was found by checkpoint/manifest name scan; no real checkpoint manifest or learned weights evidence was found.

### C:\Projects\CanvaToCode

classification: `insufficient_or_placeholder`

Rationale: this project is a Canva text adapter and platform client. It has code and tests that can build a Canva source-tree payload with HTML/CSS strings, but the inspected repository does not contain persisted real Canva design exports, generated HTML/CSS artifact files, screenshots, visual diffs, DPO rows, held-out measurements, or checkpoint/weights manifests.

Observed source/design evidence:

- `C:\Projects\CanvaToCode\README.md` describes a text-only Canva adapter and says layout, frames, vectors, and full design generator are out of scope.
- `C:\Projects\CanvaToCode\tests\platform-payload.test.ts` small read showed a test payload with `sourceKind: 'canva'`, `sourceTree` entries `canva-selection.json`, `generated.html`, and `generated.css`.
- The same test asserts `screenshots` is empty and records `screenshotsUnavailableReason: 'Canva text adapter does not expose a design export screenshot API.'`

Observed generated HTML/CSS result:

- No persisted generated HTML/CSS artifact files were found by the targeted file scan.
- HTML/CSS strings are present inside tests only.

Observed DPO/held-out/checkpoint evidence:

- No DPO/chosen/rejected/teacher labels, held-out measurement, or checkpoint/weights manifest evidence was found in the targeted scans.

### C:\Projects\xiigen-flows-dot-net-angular-DesignToCode-s1b

classification: `not_relevant` for caller-project real-data bundle; `insufficient_or_placeholder` for S3 evidence.

Rationale: FLOW-47 is an intake/status flow and not a caller project source-data bundle. It was scanned only because it was explicitly allowed if needed. It supplies contracts and flow metadata, not real design-conversion training/evaluation assets.

Observed evidence:

- `flows\FLOW-47-design-conversion-intake\flow.manifest.json` reports `readinessStatus: real-core-wired-untrained-scaffold`.
- The same manifest says real metadata-only render/compare providers are wired, but the core remains untrained and has no learned ML/training promotion yet.
- `flows\FLOW-47-design-conversion-intake\README.md` says FLOW-47 is a minimal platform-facing scaffold and does not render, train, infer, or call real design providers.

### C:\Projects\llm_mvp_core real-s3 scaffold and validator docs

classification: `insufficient_or_placeholder`

Rationale: the local training-data scaffold defines expected future bundle shapes, but does not contain real caller-project evidence.

Observed evidence:

- `training-data\design-conversion\real-s3-v1\README.md` says the scaffold contains only JSON Schema placeholders for future source manifest, DPO rows, held-out measurements, and checkpoint manifest.
- `source-manifest.schema.json` requires a real/redacted source design id/ref, source kind, source design tree, screenshots, provenance, license, privacy, and evidence status.
- `dpo-rows.schema.json` requires chosen/rejected safe candidate refs, teacher labels, curriculum tier, validation outcome, metadata, and leakage guard.
- `heldout-measurements.schema.json` requires real held-out metrics, integrity proof, thresholds, runtime cost, evidence status, and `promotion_upload`.
- `checkpoint-manifest.schema.json` requires checkpoint/weights refs and hashes, learned weights evidence, fresh-load and continue-training probes, row counts, measurement gate refs, and upload status.

## Result

usable_for_real_s3_bundle_now=false

candidate_needs_mapping=true

Candidate needing mapping:

- `C:\Projects\FigmaToCode-design-gen`

No candidate should be promoted to ingestion until a mapping worker proves scene-level tuples, authority/provenance, redaction boundaries, and missing evidence status. The current evidence does not prove real DPO rows, held-out measurements, real trained weights, fresh-load, continue-training, upload, or readiness.

## Next Bounded Work Order

CURRENT TASK:
Map only the FigmaToCode candidate materials into a proposed real-s3-v1 mapping report. Do not ingest, copy, hash binaries, upload, train, test, or build.

NOT THE TASK:
Do not create `source-manifest.json`, `dpo-rows.jsonl`, `heldout-measurements.json`, or `checkpoint-manifest.json`. Do not edit caller repos. Do not claim readiness or usable bundle status.

EVIDENCE-ONLY:
This worker produces a mapping/evidence packet only. Any source path that cannot be mapped to real-s3 schema requirements must stay `candidate_needs_mapping` or `insufficient_or_placeholder`.

required_skills_and_gates:
Load `AGENTS.md` honesty/claim guard, this packet, `training-data/design-conversion/real-s3-v1/README.md`, and the four real-s3 schemas before mapping. Apply claim-verification and no-readiness gates.

context_files_to_read:
- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-S3-caller-project-real-data-discovery-2026-06-30.md`
- `C:\Projects\llm_mvp_core\training-data\design-conversion\real-s3-v1\README.md`
- `C:\Projects\llm_mvp_core\training-data\design-conversion\real-s3-v1\source-manifest.schema.json`
- `C:\Projects\llm_mvp_core\training-data\design-conversion\real-s3-v1\dpo-rows.schema.json`
- `C:\Projects\llm_mvp_core\training-data\design-conversion\real-s3-v1\heldout-measurements.schema.json`
- `C:\Projects\llm_mvp_core\training-data\design-conversion\real-s3-v1\checkpoint-manifest.schema.json`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\scene-replay\*.json`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\scene-replay-approved\*.json`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\generated\*.html`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\generated\*.css`
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\generated\*-layout-report.json`
- `C:\Projects\FigmaToCode-design-gen\_tmp_debug_scenes\* css-debug-history.json`
- `C:\Projects\FigmaToCode-design-gen\_tmp_debug_scenes\*.png` as listed paths only unless a later ingestion/hash order explicitly permits binary reads.
- `C:\Projects\FigmaToCode-design-gen\tests\fixtures\sessions\*.json` for possible score/userFeedback mapping, with no DPO claim unless chosen/rejected and teacher-label requirements are explicitly satisfied.

implementation_or_artifact_details:
Write only `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-S3-figma-candidate-mapping-2026-06-30.md`. The report must include one row per candidate scene/material tuple with fields: source_tree_ref, generated_html_ref, generated_css_ref, expected_screenshot_ref, actual_preview_ref, qa_report_ref, available_label_signal, missing_for_source_manifest, missing_for_dpo_rows, missing_for_heldout_measurement, missing_for_checkpoint_manifest, classification.

original_task_requirements_to_cover:
For every inspected Figma candidate, classify source design tree/original tree exports, generated HTML/CSS, design screenshots/exports, rendered QA screenshots or visual diffs, DPO/teacher labels/chosen-rejected rows, held-out measurement metrics, and checkpoint/weights manifests.

mental_simulation_required:
For each proposed scene tuple, mentally simulate whether it could pass the real-s3 schemas without inventing provenance/license/DPO/held-out/checkpoint data. If any required schema field has no inspected evidence, mark the tuple `candidate_needs_mapping` or `insufficient_or_placeholder`, not usable.

good_examples:
`login-fixture.json` + `login.html` + `login.css` + `login-layout-report.json` + `login expected.png/html preview.png/react preview.png` is a mapping candidate, not usable now, until authority/provenance/DPO/held-out/checkpoint gaps are solved.

bad_examples:
Calling `tests\fixtures\sessions\ten-sessions-mature.json` a DPO dataset because it has `score` and `userFeedback`; calling layout-report counts a held-out measurement; calling `manifest.json` a checkpoint manifest without learned weights/fresh-load/continue-training evidence.

allowed_files:
Read only the Figma candidate paths above and the llm_mvp_core real-s3 scaffold/schema/docs. Write only the mapping packet named above.

forbidden_files:
Do not touch `C:\Users\lubao\Documents\xiigen`, `C:\Projects\xiigen-platform-dot-net-angular`, caller repo files, package files, `artifacts/**`, or training-data bundle files.

allowed_commands:
`Test-Path`, `rg --files`, selected `rg -n`, `Get-Content -TotalCount` for small text/JSON/MD/HTML/CSS files, and path-limited `git diff --check` on the mapping packet only.

forbidden_commands:
No nested agents, no `codex.exe`, no tests/builds, no `gh`, no browser/e2e, no staging/commit/push/branch switch/PR, no external training/model upload, no ingestion/copy into bundle.

verification_required:
Read back the mapping packet, search required literals, run direct content/whitespace check, and run path-limited `git diff --check -- docs/evidence/design-conversion/codex-S3-figma-candidate-mapping-2026-06-30.md`.

return_packet_schema:
Return in Russian with `claim_scope`, `evidence_scope`, `count_claimed`, `independent_evidence_count`, `subagent_id_or_NONE`, `downgrade_needed`, `changed_file`, `discovery_results`, `validation`, `next_bounded_work_order`, and `forbidden_actions_confirmed`.

claim_scope/evidence_scope/count_claimed/independent_evidence_count/subagent_id_or_NONE/downgrade_needed:
Required in both the mapping packet and final return. Do not claim usable bundle status unless the exact schema requirements are proven by inspected evidence.

## Validation

validation_status: passed_final_worker_validation

Validation commands required for this packet:

- read back packet;
- search required literals;
- direct content/whitespace check packet;
- path-limited `git diff --check -- docs/evidence/design-conversion/codex-S3-caller-project-real-data-discovery-2026-06-30.md`.
