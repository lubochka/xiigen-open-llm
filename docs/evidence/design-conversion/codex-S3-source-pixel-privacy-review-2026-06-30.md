# Codex S3 Source Pixel Privacy Review - 2026-06-30

```text
claim_scope: bounded visible-pixel privacy review for selected login and cart candidate PNG refs only; no full privacy, legal, owner, source-manifest, DPO, held-out, checkpoint, upload, promotion, or training readiness claim
evidence_scope: safe-candidate route JSON; candidate JSON; six selected login/cart PNG refs; local visual inspection of those PNGs only
count_claimed: 6 selected PNG pixel reviews
independent_evidence_count: 6
subagent_id_or_NONE: pixel-privacy-worker
downgrade_needed: yes_visible_pixel_review_only_training_stays_blocked
```

## Scope

Current authority allowed only a bounded pixel-privacy review or explicit exclusion decision packet for `login` and `cart` candidate PNG refs from the safe-candidate-route. This artifact does not edit source candidate files and does not inspect `mail` PNGs.

The selected refs came from `training-data/design-conversion/real-s3-v1/codex-source-manifest.candidate.json` after reading `training-data/design-conversion/real-s3-v1/codex-source-manifest.safe-candidate-route.json`. The route already kept `login` and `cart` as candidate-only with `image_pixel_privacy=not_reviewed`; this packet narrows only the selected visible-pixel risk.

## Method

- Parsed the safe-candidate route JSON and candidate JSON.
- Selected only `expected_screenshot`, `html_preview`, and `react_preview` PNG refs for `login` and `cart`.
- Visually inspected each selected PNG locally.
- Read only metadata needed for existence, byte size, and dimensions.
- Saved no screenshots and no derived image copies.

## Pixel Privacy Results

| Scene | Role | Ref | Dimensions | Pixel review | Result | Notes |
|---|---|---|---|---|---|---|
| login | expected_screenshot | `FigmaToCode-design-gen/_tmp_debug_scenes/login expected.png` | 314x522 | performed | `pass_narrow_no_visible_pii_or_secrets` | Login UI labels only. `Password` is a placeholder/UI label; no secret value is visible. |
| login | html_preview | `FigmaToCode-design-gen/_tmp_debug_scenes/login html preview.png` | 683x419 | performed | `pass_narrow_no_visible_pii_or_secrets` | Developer preview chrome plus login UI labels. No visible email, phone, address, key, token, secret value, or private message. |
| login | react_preview | `FigmaToCode-design-gen/_tmp_debug_scenes/login react preview.png` | 885x681 | performed | `pass_narrow_no_visible_pii_or_secrets` | Developer/editor chrome, diagnostic text, and generic CSS/layout values. No visible PII or secret value. |
| cart | expected_screenshot | `FigmaToCode-design-gen/_tmp_debug_scenes/cart expected.png` | 291x523 | performed | `pass_narrow_no_visible_pii_or_secrets` | Generic shopping-cart fixture content with product names, prices, stock labels, subtotal, and checkout controls. |
| cart | html_preview | `FigmaToCode-design-gen/_tmp_debug_scenes/cart html preview.png` | 696x832 | performed | `pass_narrow_no_visible_pii_or_secrets` | Developer preview chrome, generic cart/subtotal text, and red layout-debug annotations. |
| cart | react_preview | `FigmaToCode-design-gen/_tmp_debug_scenes/cart react preview.png` | 897x832 | performed | `pass_narrow_no_visible_pii_or_secrets` | Developer/editor chrome, generic cart/subtotal text, red layout-debug annotations, and generic CSS/layout values. |

For all six selected PNG refs, obvious visible PII or secret risk was classified as not visible for emails, phones, addresses, API keys/tokens, secret values, and private messages.

## Route Decision

`login` and `cart` remain candidate-only and not training-ready.

```text
usable_for_training=false
promotion_upload=blocked
schema_valid_source_manifest_created=false
dpo_labels_created=false
held_out_measurement_created=false
checkpoint_manifest_created=false
training_run_started=false
upload_or_promotion_started=false
```

## Remaining Not-Proven Queue

- authoritative provenance and reviewer or owner authority ref
- license or rights approval beyond package metadata
- training rights approval
- evaluation rights approval
- full privacy review over raw prompts, raw user ids, source files, secrets, private data, and screenshot capture authority
- schema source identity hash and stable source_design_id
- schema-valid source manifest eligibility
- schema-shaped DPO chosen/rejected labels
- held-out measurement with source-disjoint integrity proof
- checkpoint manifest, trained weights, and probes
- promotion/upload approval

## Downstream Simulation

If a downstream worker tries to use `login` or `cart` images for training, the correct result is still blocked. This artifact provides bounded visible-pixel evidence only. It does not prove owner/license authority, training/evaluation rights, DPO labels, held-out integrity, checkpoint readiness, upload approval, or training readiness.

## Forbidden Actions Confirmation

- Did not edit source candidate files.
- Did not read `mail` PNGs.
- Did not save screenshots or derived image copies.
- Did not run browser/e2e/render automation.
- Did not run tests or builds.
- Did not create DPO labels.
- Did not run held-out measurements.
- Did not create checkpoints or start training.
- Did not upload or promote.
- Did not stage, commit, push, branch, or open a PR.
- Did not make license or owner decisions.
