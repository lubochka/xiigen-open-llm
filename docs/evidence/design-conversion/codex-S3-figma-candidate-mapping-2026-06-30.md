# Codex S3 Figma Candidate Mapping - 2026-06-30

claim_scope: mapping-only evidence packet for `C:\Projects\FigmaToCode-design-gen` candidate materials against the `real-s3-v1` schemas. This packet does not ingest, copy, hash, train, upload, stage, commit, push, or claim readiness.
evidence_scope: current-turn targeted reads of the Figma candidate fixture/debug/session paths named in the caller discovery packet, plus `real-s3-v1` README/schema files and the caller discovery packet. PNG files were listed by path only; binaries were not opened or hashed.
count_claimed: 3 concrete source/generated/PNG-path/layout-report material tuples by path presence; 4 partial source/debug material groups; 0 ingestion-ready tuples.
independent_evidence_count: 0 review/pass cycles claimed; 0 sub-agent packets; mapping is based on current-worker targeted command outputs only.
subagent_id_or_NONE: NONE
downgrade_needed: yes. The evidence supports only candidate material mapping and schema gap identification, not a real-s3 bundle, not DPO readiness, not held-out measurement, not checkpoint readiness, and not promotion.

figma_candidate_mapping_done=true
promotion_upload=blocked
missing_provenance_or_license=true
missing_dpo_teacher_labels=true
missing_real_heldout_measurement=true
missing_checkpoint_manifest=true

## Boundaries

- Worked only under `C:\Projects` paths.
- Read-only in `C:\Projects\FigmaToCode-design-gen`.
- Did not touch `forbidden_user_xiigen_path`.
- Did not touch `forbidden_platform_repo_path`.
- Did not edit caller repos.
- Did not copy files into a training-data bundle.
- Did not hash binaries, open huge binaries, run browser/e2e/tests/builds/training/upload.
- Did not stage, commit, push, switch branches, create PRs, or run `gh`.
- Did not edit `artifacts/**` or package files.
- Wrote only this packet: `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-S3-figma-candidate-mapping-2026-06-30.md`.

## Schema Requirements Checked

### source-manifest.schema.json

Present as candidate material only:

- Local source tree refs exist for all seven scenes under `tests\fixtures\scene-replay\*-fixture.json`.
- The source tree shape is Figma-like JSON: inspected `login-fixture.json` contains `generatedAt`, `nodeCount`, `nodes`, and nested `sceneInput` objects with Figma node fields.
- Candidate screenshot paths exist for `travel`, `news`, `mail`, `login`, `form`, and `cart` under `_tmp_debug_scenes`.
- `manifest.json` is generic Figma plugin metadata, not a source manifest.

Missing for a real source manifest:

- `schema_version`, `bundle_id`, `dataset_id`, `created_utc`.
- Stable redacted `source_design_id`.
- Bundle-local `source_design_tree.tree_ref`, `tree_sha256`, and `source_identity_hash`; hashing was forbidden in this worker.
- Screenshot `width`, `height`, `sha256`, `captured_utc`, and `license_or_authority_ref`; PNGs were listed only.
- `provenance.source_manifest_ref`, `source_uri_or_local_ref`, `source_sha256`, `provenance_refs`, `reviewer_ref`, and raw-id/path flags.
- `license` / authority fields proving training and evaluation rights.
- `privacy` object proving no raw prompt, raw filesystem path, raw user id, or secrets in the eventual bundle.
- `evidence_status=real_source_manifest_ready`.

source_manifest_fields_present: candidate local source-tree paths, candidate PNG paths for six scenes, generic plugin manifest metadata.
source_manifest_fields_missing: redacted bundle refs, hashes, provenance, license/authority, privacy proof, screenshot metadata, ready evidence status.

### dpo-rows.schema.json

Present as label-adjacent or candidate-result material only:

- `mail`, `login`, and `cart` have generated HTML/CSS files.
- `mail`, `login`, and `cart` have layout-report JSON with visual QA counts.
- `tests\fixtures\scene-replay-approved\*-approved.json` files exist, and an inspected `login-approved.json` contains approved CSS-style mappings.
- Session fixtures contain `score`, `userFeedback`, `bestSessionId`, and Cart/Mature Cart session metadata.

Missing for real DPO rows:

- No `dpo-rows.jsonl` or schema-shaped DPO row was found.
- No `chosen` / `rejected` safe candidate refs with `result_hash`, `visual_score`, and `accepted`.
- No `teacher_labels.preferred_result_ref`, `rejected_result_ref`, `preference_label`, `score_margin`, `pair_family`, or `defect_categories`.
- No DPO `prompt` object with hashed source identity/structure and raw-prompt guard.
- No `teaching_point`, `curriculum_tier`, `validation_outcome`, metadata guard fields, or `leakage_guard`.
- Session `score`/`userFeedback` values are not enough to satisfy chosen/rejected and teacher-label requirements.

missing_dpo_teacher_labels=true

### heldout-measurements.schema.json

Present as QA/debug material only:

- `mail-layout-report.json`: `total=78`, `found=57`, `missing=21`, `sizeCorrect=48`, `posCorrect=41`, `posOff50=6`.
- `login-layout-report.json`: `total=25`, `found=17`, `missing=8`, `sizeCorrect=16`, `posCorrect=10`, `posOff50=3`.
- `cart-layout-report.json`: `total=84`, `found=64`, `missing=20`, `sizeCorrect=26`, `posCorrect=26`, `posOff50=30`.

Missing for real held-out measurement:

- No `measurement_id`, `dataset_id`, `unit_id`, or `checkpoint_ref`.
- No `measurement_gate_status` from a real held-out run.
- No schema-shaped `held_out_metrics` with held-out record count, quality score, required threshold, runtime, cost, RAM, and evidence refs.
- No `integrity_proof` for source-disjointness, anti-leakage, hidden determinism, or ablation.
- No `thresholds`, `runtime_cost`, or `evidence_status=real_held_out_measurement_ready`.
- Layout reports are QA/debug evidence, not held-out measurement objects.

missing_real_heldout_measurement=true

### checkpoint-manifest.schema.json

Present:

- `C:\Projects\FigmaToCode-design-gen\manifest.json` exists and was read as generic Figma plugin metadata.

Missing for real checkpoint manifest:

- No checkpoint manifest matching the real-s3 schema was found.
- No `checkpoint_ref`, `checkpoint_sha256`, `machine_id`, `version`, or `exported_utc`.
- No real trained weights refs/hashes or unit manifests.
- No fresh-load probe.
- No continue-training probe.
- No training dataset id, train row count, held-out row count, or measurement gate refs.
- No evidence that raw model upload was avoided except this worker's own no-upload boundary.

missing_checkpoint_manifest=true

## Candidate Tuple Matrix

| scene | source_tree_ref | generated_html_ref | generated_css_ref | expected_screenshot_ref | actual_preview_ref | qa_report_ref | available_label_signal | tuple_ready_for_ingestion | classification |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| mail | `tests\fixtures\scene-replay\mail-fixture.json` | `tests\fixtures\generated\mail.html` | `tests\fixtures\generated\mail.css` | `_tmp_debug_scenes\mail expected.png` | `_tmp_debug_scenes\mail html preview.png`; `_tmp_debug_scenes\mail react preview.png` | `tests\fixtures\generated\mail-layout-report.json` | approved CSS map path and layout counts; no schema DPO labels | tuple_ready_for_ingestion=false | concrete_material_tuple_schema_gaps |
| login | `tests\fixtures\scene-replay\login-fixture.json` | `tests\fixtures\generated\login.html` | `tests\fixtures\generated\login.css` | `_tmp_debug_scenes\login expected.png` | `_tmp_debug_scenes\login html preview.png`; `_tmp_debug_scenes\login react preview.png` | `tests\fixtures\generated\login-layout-report.json` | inspected approved CSS map and layout counts; no schema DPO labels | tuple_ready_for_ingestion=false | concrete_material_tuple_schema_gaps |
| cart | `tests\fixtures\scene-replay\cart-fixture.json` | `tests\fixtures\generated\cart.html` | `tests\fixtures\generated\cart.css` | `_tmp_debug_scenes\cart expected.png` | `_tmp_debug_scenes\cart html preview.png`; `_tmp_debug_scenes\cart react preview.png` | `tests\fixtures\generated\cart-layout-report.json` | session score/userFeedback and layout counts; no schema DPO labels | tuple_ready_for_ingestion=false | concrete_material_tuple_schema_gaps |
| travel | `tests\fixtures\scene-replay\travel-fixture.json` | missing | missing | `_tmp_debug_scenes\travel expected.png` | `_tmp_debug_scenes\travel html preview.png`; `_tmp_debug_scenes\travel react preview.png` | missing | approved CSS map path/debug history only; no schema DPO labels | tuple_ready_for_ingestion=false | partial_material_group_missing_generated_and_qa |
| news | `tests\fixtures\scene-replay\news-fixture.json` | missing | missing | `_tmp_debug_scenes\news expected.png` | `_tmp_debug_scenes\news html preview.png`; `_tmp_debug_scenes\news react preview.png` | missing | approved CSS map path/debug history only; no schema DPO labels | tuple_ready_for_ingestion=false | partial_material_group_missing_generated_and_qa |
| form | `tests\fixtures\scene-replay\form-fixture.json` | missing | missing | `_tmp_debug_scenes\form expected.png` | `_tmp_debug_scenes\form html preview.png`; `_tmp_debug_scenes\form react preview.png` | missing | approved CSS map path/debug history only; no schema DPO labels | tuple_ready_for_ingestion=false | partial_material_group_missing_generated_and_qa |
| FAQ | `tests\fixtures\scene-replay\FAQ-fixture.json` | missing | missing | missing | missing | missing | approved JSON path only; no schema DPO labels | tuple_ready_for_ingestion=false | partial_source_only_group |

## Tuple Gap Classification

For every tuple above:

- tuple_ready_for_ingestion=false
- missing_provenance_or_license=true
- missing_dpo_teacher_labels=true
- missing_real_heldout_measurement=true
- missing_checkpoint_manifest=true
- promotion_upload=blocked

Exact missing data:

- Source-manifest gap: redacted source design id/ref, bundle-local artifact refs, SHA-256 fields, screenshot dimensions/capture metadata, provenance refs, reviewer ref, license/authority, privacy/redaction proof, and ready evidence status.
- DPO gap: schema-shaped chosen/rejected rows, safe result refs and hashes, teacher labels, teaching point, curriculum tier, validation outcome, metadata scope guards, and leakage guard.
- Held-out gap: real held-out dataset split, checkpoint-linked measurement, quality threshold, source-disjoint/integrity proof, ablation proof, runtime/cost budget, and measurement evidence refs.
- Checkpoint gap: real checkpoint/weights refs and hashes, learned-weight evidence, fresh-load probe, continue-training probe, training/held-out row counts, measurement gate refs, and checkpoint evidence status.

Exact next mapping/collection actions:

1. Create an authorized source-manifest collection order for selected scenes, including license/authority/provenance review, privacy review, bundle-local refs, screenshot metadata, and hashes. This worker did not perform hashing.
2. Create a DPO-row construction order only after authority is established: define pair family, chosen/rejected candidate refs, teacher labels, score margin rules, validation outcome, metadata guards, and leakage guard.
3. Create a held-out measurement order only after source-disjoint splits and a checkpoint candidate exist; layout reports may be supporting QA evidence but not the held-out measurement object.
4. Create a checkpoint-manifest order only after real training/checkpoint evidence exists and fresh-load/continue-training probes are authorized and run.
5. Keep `promotion_upload=blocked` until source manifest, DPO rows, held-out measurement, and checkpoint manifest all have independent real evidence.

## Validation

validation_status: passed_current_worker_checks

- read_back_packet=passed.
- required_literals_search=passed.
- direct_content_whitespace_check=passed: file was non-empty; no trailing whitespace lines; no tab lines.
- path_limited_git_diff_check=passed: `git diff --check -- docs/evidence/design-conversion/codex-S3-figma-candidate-mapping-2026-06-30.md`.
