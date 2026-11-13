# Codex S3 Source Privacy Review - 2026-06-30

claim_scope: bounded privacy/source-content review for selected candidate scenes `mail`, `login`, and `cart`; selected text refs scanned and selected PNG refs checked by metadata only; no image pixel review, no source edits, no DPO labels, no held-out measurement, no checkpoint/training, no promotion/upload, no schema-valid source manifest.

evidence_scope: fresh parse of the candidate manifest and authority review; targeted scan of selected text refs under `FigmaToCode-design-gen`; representative context checks for numeric false positives and the login password UI label; metadata-only existence/byte checks for selected PNG refs.

count_claimed: 3 selected scene privacy reviews, 15 selected text refs scanned, 9 selected PNG refs metadata-checked, 0 image pixel reviews, 0 training runs, 0 DPO label sets, 0 held-out measurements, 0 checkpoint manifests, 0 uploads/promotions, 0 sub-agent packets.

independent_evidence_count: 1 bounded scanner pass, 5 representative context-read groups, 2 visible-text extraction passes, 1 metadata-only PNG check, 0 sub-agent packets.

subagent_id_or_NONE: NONE

downgrade_needed: true

## Result

This slice narrows selected text privacy risk but does not clear the bundle for training.

Bundle status:
- `usable_for_training=false`
- `promotion_upload=blocked`
- `schema_valid_source_manifest_created=false`
- `image_pixel_privacy=not_reviewed`
- `privacy_text_scan=partial_needs_manual_privacy_authority`

Scene status:
- `mail`: `needs_manual_review_person_name_like_ui_strings_present`. No accepted secrets, emails, phones, addresses, local absolute paths, raw prompts, raw user-id markers, cookies, API keys/tokens, or password/secret values were found in the selected text refs. The source JSON contains 5 person-name-like visible UI strings, recorded only by redacted JSON paths in the JSON artifact.
- `login`: `pass_narrow_no_secrets_found`. `Password` appears as UI-label-only text in the source JSON and generated HTML; no secret value was present in the inspected contexts.
- `cart`: `pass_narrow_no_secrets_found`. Title-case visible text was product/status context, not accepted as person-name findings.

## Inputs Checked

Authority inputs:
- `training-data/design-conversion/real-s3-v1/codex-source-manifest.candidate.json`
- `training-data/design-conversion/real-s3-v1/codex-source-manifest.authority-review.json`

Selected text refs scanned:
- `FigmaToCode-design-gen/tests/fixtures/scene-replay/mail-fixture.json`
- `FigmaToCode-design-gen/tests/fixtures/generated/mail.html`
- `FigmaToCode-design-gen/tests/fixtures/generated/mail.css`
- `FigmaToCode-design-gen/tests/fixtures/generated/mail-layout-report.json`
- `FigmaToCode-design-gen/tests/fixtures/scene-replay-approved/mail-approved.json`
- `FigmaToCode-design-gen/tests/fixtures/scene-replay/login-fixture.json`
- `FigmaToCode-design-gen/tests/fixtures/generated/login.html`
- `FigmaToCode-design-gen/tests/fixtures/generated/login.css`
- `FigmaToCode-design-gen/tests/fixtures/generated/login-layout-report.json`
- `FigmaToCode-design-gen/tests/fixtures/scene-replay-approved/login-approved.json`
- `FigmaToCode-design-gen/tests/fixtures/scene-replay/cart-fixture.json`
- `FigmaToCode-design-gen/tests/fixtures/generated/cart.html`
- `FigmaToCode-design-gen/tests/fixtures/generated/cart.css`
- `FigmaToCode-design-gen/tests/fixtures/generated/cart-layout-report.json`
- `FigmaToCode-design-gen/tests/fixtures/scene-replay-approved/cart-approved.json`

Selected PNG refs metadata-checked only:
- `FigmaToCode-design-gen/_tmp_debug_scenes/mail expected.png`
- `FigmaToCode-design-gen/_tmp_debug_scenes/mail html preview.png`
- `FigmaToCode-design-gen/_tmp_debug_scenes/mail react preview.png`
- `FigmaToCode-design-gen/_tmp_debug_scenes/login expected.png`
- `FigmaToCode-design-gen/_tmp_debug_scenes/login html preview.png`
- `FigmaToCode-design-gen/_tmp_debug_scenes/login react preview.png`
- `FigmaToCode-design-gen/_tmp_debug_scenes/cart expected.png`
- `FigmaToCode-design-gen/_tmp_debug_scenes/cart html preview.png`
- `FigmaToCode-design-gen/_tmp_debug_scenes/cart react preview.png`

## Privacy Findings

Accepted text findings:
- `accepted_secret_or_credential_hits=0`
- `accepted_email_hits=0`
- `accepted_phone_hits=0`
- `accepted_address_hits=0`
- `accepted_local_absolute_path_hits=0`
- `accepted_raw_prompt_hits=0`
- `accepted_raw_user_id_hits=0`
- `mail_person_name_like_visible_strings=present_redacted_needs_owner_or_privacy_authority`

Context triage:
- Phone-like regex candidates were manually sampled and classified as layout/debug numeric values such as alpha, stroke/radius, position, opacity, RGBA, and drift decimal fields. They were not accepted as phone-number privacy hits.
- `Password` in `login` is UI-label-only: source JSON `$.nodes[16].sceneInput.characters` and generated HTML line 290 visible text. No password value or captured secret was found.
- `cart` title-case strings were product or status labels by context.

## Downstream Simulation

A downstream worker can use this artifact to see the narrow text-scan result, but still cannot create a schema-valid training source manifest. `mail` has a remaining person-name-like privacy authority question, images were not pixel-reviewed, and the parent authority blockers remain open.

## Remaining Not-Proven Queue

- Authoritative provenance and reviewer/owner authority ref.
- License or rights approval beyond package metadata.
- Training rights approval.
- Evaluation rights approval.
- Owner/privacy authority decision for `mail` person-name-like UI strings or source replacement.
- Full privacy review over raw prompts, raw user IDs, secrets, private data, and screenshot contents.
- Image pixel privacy review or explicit decision to exclude images.
- Screenshot dimensions, captured UTC, and license/authority refs.
- Schema source identity hash and stable source design ID.
- Schema-valid source manifest eligibility.
- Schema-shaped DPO chosen/rejected labels.
- Held-out measurement with source-disjoint integrity proof.
- Checkpoint manifest, trained weights, and probes.
- Promotion/upload approval.

## Forbidden Actions Confirmed

- Source candidate files edited: false.
- DPO labels created: false.
- Held-out measurements created: false.
- Checkpoint/training created: false.
- Promotion/upload attempted: false.
- Tests/builds/browser/e2e run: false.
- Git stage/commit/push/branch/PR: false.
- Forbidden worktrees touched: false.
