# Codex S3 Source Manifest Authority Fields Review - 2026-06-30

claim_scope: bounded authority-fields review for selected scenes `mail`, `login`, and `cart`; no training readiness, no ingestion readiness, no DPO labels, no held-out measurement, no checkpoint manifest, no promotion, no upload.
evidence_scope: fresh source-manifest schema and candidate manifest reads; evidence packet boundary lines confirming `usable_for_training=false` and `promotion_upload=blocked`; narrow metadata reads for `FigmaToCode-design-gen` README/package/plugin manifest/license-file presence.
count_claimed: 3 selected scene authority reviews plus 1 bundle-level authority review; 0 training runs; 0 DPO label sets; 0 held-out measurements; 0 checkpoint manifests; 0 uploads/promotions; 0 sub-agent packets.
independent_evidence_count: 0 completed review/pass cycles; 0 sub-agent packets; 0 real training evidence records.
subagent_id_or_NONE: NONE
downgrade_needed: yes. The evidence supports only blocked authority-field review, not schema-valid source-manifest readiness or training readiness.

## Active Boundary

This review advances only the source-manifest authority-field layer after the candidate manifest PASS. The candidate manifest and evidence packet remain evidence only. They still say `usable_for_training=false`, `promotion_upload=blocked`, and the schema mismatch is intentional.

No DPO chosen/rejected labels were created. No held-out measurement, checkpoint manifest, training run, upload, promotion, tests/builds/browser/e2e, git action, code edit, test edit, schema edit, plan edit, package edit, or forbidden-repo edit was performed.

## Target Layer Classification

| layer | status | meaning |
| --- | --- | --- |
| project implementation | not_touched | no code/test/schema/package changes |
| tests/evidence | authority_review_artifact_only | this is a review artifact, not a test/build/e2e result |
| docs | codex_evidence_doc_created | this document records the authority-field review |
| plan artifact | not_touched | no plan was edited |
| skills/guides/agent files | not_touched | governance was read/applied; no repair requested |
| agent governance | applied | claim/evidence downgrade gates kept unproven fields blocked |
| external decision | blocked | provenance/license/privacy/training rights need Luba or owner authority |

## Authority Field Status Summary

| authority field | bundle status | why it remains blocked |
| --- | --- | --- |
| provenance refs | candidate_refs_present_but_authoritative_provenance_not_verified | selected refs exist in the candidate, but no authoritative provenance chain was proven |
| reviewer/owner authority ref | missing | no `reviewer_ref` or owner authority record was found |
| license/rights approval | blocked_pending_luba_or_owner_approval | package metadata declares `ISC`, but root license files were absent and package metadata alone is not training authority |
| training rights | not_proven | no authority record approves training use |
| evaluation rights | not_proven | no authority record approves evaluation use |
| privacy review | partial_not_proven | selected text scan evidence is not a full privacy review over prompts, user ids, secrets, private data, and screenshots |
| screenshot metadata | missing | screenshot hashes exist, but dimensions are `not_collected`, and capture time/authority are `not_proven` |
| source identity redaction | partial_not_proven | this review uses redacted/project-relative refs only, but schema source identity hash and full privacy proof are missing |
| schema-valid manifest eligibility | blocked_not_proven | required real-schema authority fields are still missing |
| usable for training | false | required authority fields are not proven |
| promotion/upload | blocked | upstream authority, DPO, held-out, checkpoint, and upload gates remain blocked |

## Scene Review

| scene | provenance refs | reviewer/owner | license/rights | training/eval rights | privacy | screenshots | source identity | schema-valid manifest | usable_for_training |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| mail | candidate refs only, not authoritative | missing | `ISC` package metadata only; approval not proven | not_proven / not_proven | partial_not_proven | metadata missing | partial_not_proven | false | false |
| login | candidate refs only, not authoritative | missing | `ISC` package metadata only; approval not proven | not_proven / not_proven | partial_not_proven; `Password` is classified only as visible UI label in inspected context | metadata missing | partial_not_proven | false | false |
| cart | candidate refs only, not authoritative | missing | `ISC` package metadata only; approval not proven | not_proven / not_proven | partial_not_proven | metadata missing | partial_not_proven | false | false |

## Real Schema Eligibility

`schema_valid_source_manifest_created=false`

Reasons:

- the candidate is a wrapper while `source-manifest.schema.json` has `additionalProperties=false`;
- stable authority-approved `source_design_id` is not proven;
- `source_design_tree.source_identity_hash` is missing;
- screenshots lack required dimensions, captured time, and license/authority refs;
- provenance lacks a proven reviewer/owner authority ref and authoritative source chain;
- license, training rights, and evaluation rights are not approved;
- privacy remains partial_not_proven;
- `evidence_status=real_source_manifest_ready` is unsupported.

## Downstream Worker Simulation

If a downstream worker tries to create a schema-valid source manifest from these files, the correct result is blocked. The worker may use the selected candidate refs as evidence inputs only. The worker must not treat the candidate wrapper, package metadata, or this authority review as training-ready material.

PASS condition for this slice: the next worker can see exactly which authority fields are present as candidate evidence, which are missing, and why `usable_for_training=false` remains locked.

## Remaining Not-Proven Queue

- authoritative provenance refs and reviewer/owner authority ref;
- license or rights approval beyond package metadata;
- training rights approval;
- evaluation rights approval;
- full privacy review for raw prompts, raw user ids, secrets, private data, and screenshot contents;
- screenshot dimensions, captured time, and license/authority refs;
- schema source identity hash and stable source design id;
- schema-valid source manifest eligibility;
- schema-shaped DPO chosen/rejected labels;
- held-out measurement with source-disjoint integrity proof;
- checkpoint manifest, trained weights, and probes;
- promotion/upload approval.

## Files Created

- `training-data/design-conversion/real-s3-v1/codex-source-manifest.authority-review.json`
- `docs/evidence/design-conversion/codex-S3-source-manifest-authority-fields-review-2026-06-30.md`

## Next Bounded Work Order

CURRENT TASK:
Resolve one external authority blocker for the selected `mail`, `login`, and `cart` source-material set: obtain or record an explicit owner/Luba authority decision for provenance reviewer/owner ref and training/evaluation rights, or keep the route `blocked_pending_luba_or_owner_approval`.

NOT THE TASK:
Do not create DPO labels, held-out measurements, checkpoint manifests, training runs, uploads, promotions, tests/builds/browser/e2e, git actions, code edits, schema edits, or plan edits.

VERIFICATION REQUIRED:
Fresh-read this authority review JSON, the candidate manifest, and the source-manifest schema. If authority is not explicitly provided, preserve `usable_for_training=false`, `schema_valid_source_manifest_created=false`, and `promotion_upload=blocked`.
