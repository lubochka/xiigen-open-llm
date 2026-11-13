# Codex S3 Owner/License/Provenance Blocker Evidence - 2026-06-30

## Claim Scope

This packet records a blocked owner/license/provenance evidence state for the `login` and `cart` candidate route only.

It does not approve legal use, owner authority, training rights, evaluation rights, a schema-valid source manifest, DPO labels, held-out measurements, checkpoint creation, upload, or promotion.

## Evidence Scope

Read in this bounded pass:

- `training-data/design-conversion/real-s3-v1/codex-source-manifest.safe-candidate-route.json`
- `training-data/design-conversion/real-s3-v1/codex-source-pixel-privacy-review.json`
- `FigmaToCode-design-gen/package.json`
- `FigmaToCode-design-gen/README.md`
- `FigmaToCode-design-gen/LICENSE` presence check
- `FigmaToCode-design-gen/LICENSE.md` presence check
- `FigmaToCode-design-gen/COPYING` presence check

No raw local filesystem paths are recorded in this artifact.

## Metadata Found

| Evidence item | Result | Rights meaning |
|---|---|---|
| `package.json` | `license` metadata is `ISC` | Metadata only; not owner approval and not training/evaluation rights approval. |
| `README.md` | License section mentions `ISC` | Metadata only; not owner approval and not training/evaluation rights approval. |
| `LICENSE` | Missing in narrow root check | License file evidence remains incomplete. |
| `LICENSE.md` | Missing in narrow root check | License file evidence remains incomplete. |
| `COPYING` | Missing in narrow root check | License file evidence remains incomplete. |

## Current Route Scope

Current candidate route scenes:

- `login`
- `cart`

Excluded scene:

- `mail`, excluded by the safe-route artifact because privacy is not cleared.

## Blocking Decision

The package metadata license is `ISC`, but this packet keeps the route blocked.

Required distinction:

- `package_license_metadata=ISC`
- `license_approval_for_training=not_proven`
- `training_rights=blocked_pending_owner_approval`
- `evaluation_rights=blocked_pending_owner_approval`
- `provenance_source_authority_status=not_proven`
- `owner_or_reviewer_approval_status=not_proven`

Package metadata is not sufficient evidence for training or evaluation use of the candidate route assets.

## Required Approval Evidence

Before any downstream worker may create a schema-valid source manifest, DPO labels, training rows, evaluation rows, held-out measurements, checkpoints, upload, or promotion, the project needs authoritative evidence for all of the following:

- Owner approval note or reviewer authority note.
- License file or explicit license decision covering this source bundle.
- Training rights approval for `login` and `cart` source materials.
- Evaluation rights approval for `login` and `cart` source materials.
- Provenance source authority reference tying package metadata, screenshots, generated previews, and candidate route to an approved source.

## Locks Preserved

- `usable_for_training=false`
- `schema_valid_source_manifest_created=false`
- `promotion_upload=blocked`
- `dpo_labels_created=false`
- `held_out_measurement_created=false`
- `checkpoint_manifest_created=false`
- `training_run_started=false`
- `evaluation_run_started=false`
- `upload_or_promotion_started=false`

## Downstream Worker Simulation

Simulation: a downstream worker sees `package.json` with `license=ISC`.

Expected result: the worker must not proceed to source-manifest creation, DPO labels, training, evaluation, checkpoint creation, upload, or promotion.

Reason: `ISC` package metadata is local metadata evidence only. It does not prove owner approval, source authority, or training/evaluation rights for the route assets.

Verdict: PASS for this blocker packet. The packet prevents a metadata-to-rights upgrade.

## Remaining Not-Proven Queue

- Authoritative owner or reviewer approval note.
- License file or explicit license decision covering this source bundle.
- Training rights approval.
- Evaluation rights approval.
- Provenance source authority reference.
- Full privacy review over raw prompts, raw user ids, source files, secrets, private data, and screenshot capture authority.
- Schema source identity hash and stable source design id.
- Schema-valid source manifest eligibility.
- Schema-shaped DPO chosen/rejected labels.
- Held-out measurement with source-disjoint integrity proof.
- Checkpoint manifest, trained weights, and probes.
- Promotion/upload approval.

## Forbidden Actions Confirmed

This packet did not make legal or owner decisions; did not create a schema-valid source manifest; did not create DPO labels; did not run held-out measurements, checkpoints, training, upload, promotion, tests, builds, browser checks, e2e, render automation, git stage, commit, push, branch, or PR; and did not write source candidate files, schemas, package files, code files, test files, or claude-prefixed artifacts.
