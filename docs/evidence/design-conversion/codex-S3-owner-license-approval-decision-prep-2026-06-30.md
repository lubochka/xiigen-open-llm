# Codex S3 Owner/License Approval Decision Prep - 2026-06-30

## Claim Scope

This packet prepares the remaining external decision for the `login/cart` safe candidate route.

It does not approve legal use, owner authority, training rights, evaluation rights, a schema-valid source manifest, DPO labels, held-out measurements, checkpoint creation, upload, or promotion.

## Evidence Scope

Fresh source artifacts used for this packet:

- `training-data/design-conversion/real-s3-v1/codex-source-owner-license-blocker.json`
- `docs/evidence/design-conversion/codex-S3-owner-license-provenance-blocker-2026-06-30.md`
- Current Luba work order for this decision-prep packet

The prior blocker evidence says package metadata `ISC` is not enough to prove owner approval, provenance authority, training rights, or evaluation rights.

## Current Candidate Scope

Included safe candidate route:

- `login`
- `cart`

Excluded scene:

- `mail` remains excluded. This packet does not replace, approve, or reintroduce `mail`.

## Current Non-Approval Status

| Decision area | Current status |
|---|---|
| Owner/source approval | `approval_not_granted` |
| Training rights | `training_rights_not_granted` |
| Evaluation rights | `evaluation_rights_not_granted` |
| Provenance/source authority | `not_granted` |
| Screenshot or image use | `not_granted` |
| License decision beyond metadata | `not_granted` |
| Mail replacement or exclusion decision | `not_granted` |

This is a decision-prep artifact only.

## Accepted Evidence Luba Or Owner Could Provide

| Evidence option | Required content |
|---|---|
| Owner approval note | Who approved, authority role or owner basis, approval date, approved scope, covered source set, restrictions or exclusions. |
| License decision | License name or custom decision, license file or decision reference, allowed-use summary, restrictions, and whether package metadata `ISC` is accepted as sufficient or only supplemental. |
| Training rights approval | Explicit allowed or denied status for training use of `login/cart` candidate materials, allowed source materials, allowed model or dataset use, and restrictions. |
| Evaluation rights approval | Explicit allowed or denied status for evaluation or held-out use of `login/cart` candidate materials, allowed metrics or held-out use, and restrictions. |
| Provenance/source authority | Source owner or authority, source bundle identity, source origin reference, and chain from source bundle to the `login/cart` candidate route. |
| Screenshot or image use decision | Whether screenshots/images may be used, which image types are covered, allowed use, and redaction or exclusion rules. |
| Mail replacement or exclusion decision | Whether `mail` must remain excluded or may be replaced, and if replacement is allowed, the replacement source plus its license/provenance requirements. |

## Approval Evidence Schema For Future Validation

A future approval packet should include:

- `who_approved`
- `authority_role_or_owner_basis`
- `approval_date`
- `scope`, explicitly naming `login/cart` and the source bundle
- `training` rights as explicit allowed or denied
- `evaluation` rights as explicit allowed or denied
- `screenshots_or_images` as explicit allowed, denied, or not applicable
- license decision
- provenance/source authority
- covered source set
- restrictions, or an explicit statement that there are no restrictions
- `mail` decision: keep excluded or require a separately validated replacement path

## If The Decision Is Yes

The next bounded work order is not training.

Next bounded work order after approval is supplied:

1. Validate the approval packet for owner identity, date, scope, rights, source set, restrictions, screenshot/image authority, `mail` decision, and provenance/source authority.
2. Only if validation passes, attempt a schema-valid source manifest candidate for `login/cart`.
3. Do not automatically create DPO labels, held-out measurements, checkpoints, training, evaluation, upload, or promotion.

## If The Decision Is No

Keep the route excluded from training/evaluation, record the denial boundary, or seek a replacement source through a separate owner/license/provenance decision-prep packet.

## If There Is No Answer Or The Evidence Is Unclear

Recommended safe default:

- Keep blocked.
- Do not create a schema-valid source manifest.
- Do not create DPO labels.
- Do not create held-out measurements.
- Do not create checkpoints.
- Do not start training or evaluation.
- Do not upload or promote.

## Locks Preserved

| Lock | Value |
|---|---|
| `usable_for_training` | `false` |
| `schema_valid_source_manifest_created` | `false` |
| `promotion_upload` | `blocked` |
| `dpo_labels_created` | `false` |
| `held_out_measurement_created` | `false` |
| `checkpoint_manifest_created` | `false` |
| `training_run_started` | `false` |
| `evaluation_run_started` | `false` |
| `upload_or_promotion_started` | `false` |

## Downstream Worker Guard

A downstream worker must not treat this packet as approval.

Required guardrails:

- Package metadata `ISC` is not training-rights approval.
- Package metadata `ISC` is not evaluation-rights approval.
- Decision-prep is not approval.
- Approval validation must happen before schema/source/DPO/training/evaluation work.
- A schema-valid source manifest candidate, if later attempted, is still not automatic training permission.
- `mail` must not be reintroduced without an explicit replacement or exclusion decision.

## Mental Simulation

Luba reads the packet:

- The decision needed is precise: owner/license/provenance/training/evaluation authority for `login/cart`.
- If the decision is yes, the next step is approval validation, then only a source manifest candidate attempt.
- If the decision is no, the route stays excluded or a replacement source is sought.
- If there is no answer, the route stays blocked and no training/evaluation/upload work starts.

Downstream worker reads the packet:

- The worker must not infer approval, training rights, evaluation rights, DPO readiness, checkpoint readiness, or upload readiness.
- The worker must validate an approval packet before attempting source/schema/DPO/training/evaluation work.

Simulation verdict: PASS for decision-prep. The artifact does not upgrade the blocker into approval.

## Remaining Not-Proven Queue

- Authoritative owner or reviewer approval note.
- License decision covering the source bundle.
- Training rights approval.
- Evaluation rights approval.
- Provenance/source authority.
- Screenshots/images use decision.
- `mail` replacement or exclusion decision.
- Approval packet validation.
- Schema-valid source manifest candidate.
- DPO labels.
- Held-out measurement.
- Checkpoint, training, and evaluation.
- Upload or promotion approval.

## Forbidden Actions Confirmed

This packet did not make legal or owner decisions; did not grant approval; did not grant training rights; did not grant evaluation rights; did not create a schema-valid manifest; did not create DPO labels; did not run held-out measurements, checkpoints, training, upload, promotion, tests, builds, browser checks, e2e, render automation, git stage, commit, push, branch, or PR; and did not touch forbidden repos or non-codex artifacts.
