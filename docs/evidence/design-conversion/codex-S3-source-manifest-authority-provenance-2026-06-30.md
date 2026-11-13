# Codex S3 Source Manifest Authority and Provenance Intake - 2026-06-30

claim_scope: bounded source-manifest authority/provenance intake artifact for selected Figma candidate scenes `mail`, `login`, and `cart`; no ingestion, no training, no DPO labels, no held-out measurement, no checkpoint manifest, no promotion, no upload.
evidence_scope: current-turn reads of the mapping packet, source-manifest schema, selected `mail`/`login`/`cart` candidate refs under `FigmaToCode-design-gen`, selected candidate file hashes, repository README/package/plugin metadata, session prompt, AGENTS/project rules, and exact project skill-file location checks.
count_claimed: 3 selected candidate scene intake records; 24 selected candidate refs present and hashed; 0 training/evaluation/review/pass cycles; 0 sub-agent packets.
independent_evidence_count: 0 completed review/pass cycles claimed; 0 sub-agent packets.
subagent_id_or_NONE: NONE
downgrade_needed: yes. The evidence supports only candidate source-manifest intake and blocker reduction, not training readiness.

source_manifest_authority_provenance_intake_created=true
selected_scenes=mail,login,cart
tuple_ready_for_ingestion=false
usable_for_training=false
promotion_upload=blocked
schema_valid_real_source_manifest=false

## Active Task Boundary

This artifact advances only the source-manifest authority/provenance collection gate after the mapping packet. It does not train, upload, promote, run browser/e2e, run builds/tests, create DPO chosen/rejected labels, create held-out measurements, create checkpoint manifests, stage, commit, push, branch, or edit forbidden repos.

The mapping packet is evidence only. Its relevant boundary remains:

- selected scenes `mail`, `login`, and `cart` have concrete candidate material tuples by path presence.
- `tuple_ready_for_ingestion=false`.
- next authorized project layer is source-manifest authority/provenance intake only.

## Files Created

- `training-data/design-conversion/real-s3-v1/codex-source-manifest.candidate.json`
- `docs/evidence/design-conversion/codex-S3-source-manifest-authority-provenance-2026-06-30.md`

## Target-Layer Classification

| layer | classification | result |
| --- | --- | --- |
| project implementation | not touched | no code/test/package files changed |
| tests/evidence | touched | created an evidence packet and candidate intake JSON only |
| docs | touched | created this codex-prefixed evidence packet only |
| plan artifact | not touched | no plan text was edited |
| skills/guides/agent files | not touched | project rules were read/applied where available; no governance repair was requested or made |
| agent governance | applied | no readiness/count/sub-agent claims are made without matching evidence |
| external decision | blocked | provenance/license/privacy authority remains not proven |

## Schema Shape Decision

The source-manifest schema is a real source-design manifest shape. It requires fields such as `schema_version`, `bundle_id`, `dataset_id`, `created_utc`, `source_design_id`, `source_design_tree`, `screenshots`, `provenance`, `license`, `privacy`, and `evidence_status`.

The requested intake artifact must also preserve scene id, generated HTML/CSS refs, expected/preview image refs, QA/layout report refs, and `usable_for_training=false`. Those fields are not accepted by the schema because the schema has `additionalProperties=false`.

Therefore the JSON artifact is intentionally a candidate intake wrapper, not a schema-valid real source manifest. A validator should parse the JSON but reject it for source-manifest schema readiness and training readiness.

schema_gap:

- scene-level candidate refs are needed for the next worker but are not schema fields.
- generated HTML/CSS and QA layout reports are candidate material, not source-manifest authority evidence.
- screenshot dimensions and capture authority were not collected.
- provenance source refs, reviewer refs, license/authority refs, and privacy proof are not proven.
- `evidence_status=real_source_manifest_ready` is not supported.

## Selected Candidate Scene Intake

| scene | source tree | generated refs | expected/preview refs | QA/layout report | label-adjacent ref | usable for training |
| --- | --- | --- | --- | --- | --- | --- |
| mail | present, hashed | HTML/CSS present, hashed | expected/html preview/react preview present, hashed | total 78, found 57, missing 21, sizeCorrect 48, posCorrect 41, posOff50 6 | approved CSS map present, not DPO label | false |
| login | present, hashed | HTML/CSS present, hashed | expected/html preview/react preview present, hashed | total 25, found 17, missing 8, sizeCorrect 16, posCorrect 10, posOff50 3 | approved CSS map present, not DPO label | false |
| cart | present, hashed | HTML/CSS present, hashed | expected/html preview/react preview present, hashed | total 84, found 64, missing 20, sizeCorrect 26, posCorrect 26, posOff50 30 | approved CSS map present, not DPO label | false |

Hashes were collected only for the selected candidate refs listed in the JSON. No binaries outside the selected candidate set were hashed.

## Provenance, License, Privacy Status

provenance_status: not_verified
license_status: not_approved_for_training
privacy_status: not_fully_proven
usable_for_training: false
promotion_upload: blocked

Evidence found:

- `FigmaToCode-design-gen/package.json` exists and declares package license `ISC`.
- `FigmaToCode-design-gen/README.md` exists and describes the plugin/project.
- `FigmaToCode-design-gen/manifest.json` exists and is plugin metadata, not a source manifest.
- `FigmaToCode-design-gen/LICENSE` was not present.
- `FigmaToCode-design-gen/LICENSE.md` was not present.

Boundary:

- Package metadata is not enough to approve training/evaluation authority.
- No owner/reviewer authority record was found.
- No full privacy review over raw prompts, raw user ids, private data, secrets, or screenshot contents was completed.
- The selected text scan did not find raw filesystem path markers, raw user id markers, or secret markers in the selected non-PNG refs. The login scene contains the literal UI label `Password`; the inspected snippets classify it as visible UI placeholder/label text, not a captured secret. This does not complete privacy review.

## Validator Simulation

If a validator consumes `codex-source-manifest.candidate.json`:

1. JSON parse should pass.
2. Real source-manifest schema validation should fail because this is a candidate wrapper and because authority fields are not complete.
3. Training readiness should fail because `usable_for_training=false`.
4. Promotion/upload should remain blocked because `promotion_upload=blocked`.

This is the intended result. The artifact reduces the next-worker search space without converting schema evidence into training evidence.

## Remaining Not-Proven Queue

- authoritative provenance source refs and reviewer ref.
- license/authority approval for training and evaluation.
- full privacy review, including raw prompts, raw user ids, secrets, private data, and screenshot contents.
- schema-shaped source manifest after authority is proven.
- DPO chosen/rejected labels and teacher-label rules.
- held-out measurement with source-disjoint integrity proof.
- checkpoint manifest with trained-weight evidence and fresh-load/continue-training probes.
- promotion/upload decision after all required upstream gates pass.

## Next Bounded Work Order

CURRENT TASK:
Validate and complete source-manifest authority fields for selected scenes `mail`, `login`, and `cart` only.

NOT THE TASK:
Do not train, upload, promote, create DPO labels, create held-out metrics, create checkpoint manifests, run browser/e2e/build/test commands, or edit forbidden repos.

EVIDENCE-ONLY:
Use this intake packet and the candidate JSON as bounded candidate evidence only. Do not treat package license metadata or file presence as training authority.

allowed_files:
Read this evidence packet, the candidate JSON, the source-manifest schema, and only selected candidate/provenance refs for `mail`, `login`, and `cart`. Write only the next explicitly authorized codex-prefixed source-manifest authority artifact.

verification_required:
Prove or keep blocked each required schema authority field: provenance refs, reviewer ref, license/authority ref, privacy proof, screenshot metadata, and redacted source identity. If any field is not proven, keep `usable_for_training=false` and `promotion_upload=blocked`.

return_packet_schema:
claim_scope:
evidence_scope:
count_claimed:
independent_evidence_count:
subagent_id_or_NONE:
downgrade_needed:
files_changed:
provenance_license_privacy_status:
schema_validation:
remaining_not_proven_queue:
forbidden_actions_confirmed:

## Forbidden Actions Confirmed

- did not train.
- did not upload.
- did not promote.
- did not run browser/e2e.
- did not run dotnet/npm builds or tests.
- did not create DPO chosen/rejected labels.
- did not create held-out measurements.
- did not create checkpoint manifests.
- did not stage, commit, push, branch, switch, merge, rebase, reset, or open a PR.
- did not edit forbidden repos.
- did not copy candidate files into training-data.

## Validation Results

- json_parse=passed for `training-data/design-conversion/real-s3-v1/codex-source-manifest.candidate.json`.
- raw_path_scan=passed for both created files using raw local path and forbidden-repo path patterns.
- usable_for_training_true_scan=passed; no true-valued usable_for_training assignment was found.
- json_usable_for_training_false_count=4.
- promotion_upload_blocked_hits=4.
- remaining_blocker_literal_hits=18.
- standard path-limited `git diff --check` exited clean but did not inspect untracked file contents.
- `git diff --check --no-index` against `NUL` produced no whitespace warning output for either file; exit code was 1 because the files differ from the empty device.
- manual_whitespace_check=passed for trailing whitespace and tab characters.
- path-limited git status shows both created files are untracked; no staging or git write action was performed.
