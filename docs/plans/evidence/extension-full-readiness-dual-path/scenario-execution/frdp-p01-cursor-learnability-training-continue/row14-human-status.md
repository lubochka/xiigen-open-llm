# Row14 Human Status

Status: `ROW14_SCOPED_ACCEPTED / LEARNABILITY_FRESH_LOAD_REVIEW_PASS`

Scenario: Row14 `frdp-p01-cursor-learnability-training-continue`

Generated UTC: `2026-06-26T11:05:01Z`

## Human Summary

- Row14.1 backup/export preflight: PASS. Checkpoint hash `6b711271fd3f1c792fdaecf6610bed2a7791adfc4a2f4d04eb4a69dc6783419f`; manifest hash `4dc0b69deecd73645c681fe193fb58fb3febdf106dc3de5f664df850d01f5c7d`.
- Row14.2 DPO rows review: PASS. There are `2` usable rows for `client-adaptive-learning-loop`; overlap with Row13 is `0/56`.
- Row14.3 staged continue-training/export review: PASS. Base `8` -> staged `10` pairs, fresh-load true. Staged checkpoint hash `c0f3047e80610f3852928c04d2ecd8f89857d5e20726b2dc391892ff73ac9dbb`; staged manifest hash `4ae155818ac6351009626a37f22de56202ec1133c8a2728ab39be2f4dba90475`.
- Row14.4 held-out/fresh-load eval review: PASS. selected=expected=`client-adaptive-learning-loop`, pass=true, accuracy=1.0.

## Evidence Basis

- Row14.1 report: `row14-1-backup-export-preflight-no-training-20260626T102311Z/row14-1-backup-export-preflight-report.md`
- Row14.1 JSON: `row14-1-backup-export-preflight-no-training-20260626T102311Z/row14-1-backup-export-preflight-report.json`
- Row14.2 report: `row14-2-dpo-rows-20260626T102909Z/row14-2-dpo-rows-report.md`
- Row14.2 JSON: `row14-2-dpo-rows-20260626T102909Z/row14-2-dpo-summary.json`
- Row14.2 rows: `row14-2-dpo-rows-20260626T102909Z/row14-2-skillselection-dpo-rows.jsonl`
- Row14.3 report: `row14-3-staged-continue-training-export-20260626T103934Z/row14-3-staged-continue-training-export-report.md`
- Row14.3 JSON: `row14-3-staged-continue-training-export-20260626T103934Z/row14-3-staged-continue-training-export-report.json`
- Row14.3 validation: `row14-3-staged-continue-training-export-20260626T103934Z/row14-3-staged-continue-training-export-validation.json`
- Row14.4 report: `row14-4-heldout-fresh-load-eval-20260626T105431Z/row14-4-heldout-fresh-load-report.md`
- Row14.4 JSON: `row14-4-heldout-fresh-load-eval-20260626T105431Z/row14-4-heldout-fresh-load-report.json`
- Row14.4 validation: `row14-4-heldout-fresh-load-eval-20260626T105431Z/row14-4-heldout-fresh-load-validation.json`
- Row14.4 results: `row14-4-heldout-fresh-load-eval-20260626T105431Z/row14-4-heldout-fresh-load-results.jsonl`

## Counters

- Total scenarios: `28`
- Accepted narrow/scoped Row12 + Row13 + Row14: `3`
- Product readiness full pass: `0`
- Full joined pass: `0`

## Next Scenario

Next scenario after Row14 packaging: Row15 `frdp-p01-cursor-dpo-quality-provenance`.

This packet does not mark product done.

## Non-Claims

No product readiness is claimed.
No token savings are claimed.
No full semantic quality is claimed.
No training readiness is claimed.
No checkpoint mutation or promotion is claimed.
No locator update is claimed.
No full extension readiness is claimed.

This packaging packet did not run eval, UI, training, checkpoint/model mutation, locator update, promotion, stage, commit, or push.
