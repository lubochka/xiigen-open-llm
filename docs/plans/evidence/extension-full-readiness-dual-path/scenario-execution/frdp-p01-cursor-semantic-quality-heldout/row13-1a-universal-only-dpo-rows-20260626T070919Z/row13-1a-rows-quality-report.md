# Row13.1a Universal-Only DPO Rows Quality Report

Status: ROW13_1A_DPO_ROWS_PASS

Boundary: rows-only artifact for Row13.1b candidate training/export. No training performed. No checkpoint mutation. No production/common promotion. No eval/UI rerun. No quality improvement claimed.

## Source Boundary

The rows are universal-only synthetic preference rows for the four failed Row13.1 skills: `e2e-test-matrix`, `no-product-decisions`, `output-skepticism`, and `self-verification`.

Source-disjoint rule: the rows do not copy held-out exact prompt text, row ids, pair ids, source refs, paths, filenames, candidate order, or vendor output. They use only the universal failure category and available skill contrast.

No vendor output rule: no vendor output was copied, paraphrased, ranked, or distilled into the chosen or rejected examples.

Quarantine decision: all 8 rows are accepted as usable Row13.1b candidate-training input only. The original held-out rows remain quarantined for evaluation. These rows do not authorize common/prod promotion.

## Skill Examples

### e2e-test-matrix

Rows: `ROW13_1A_DPO_E2E_TEST_MATRIX_001`, `ROW13_1A_DPO_E2E_TEST_MATRIX_002`

What the model should learn: select `e2e-test-matrix` when the user asks for an end to end coverage matrix spanning success paths, retries, delayed work, recovery, boundary inputs, and security expectations before implementation.

What the model should reject: selecting adjacent review or integrity skills as the primary answer when the requested artifact is the matrix itself. `test-integrity`, `implementation-integrity`, and `plan-review` are not the central behavior for authoring scenario coverage.

### no-product-decisions

Rows: `ROW13_1A_DPO_NO_PRODUCT_DECISIONS_001`, `ROW13_1A_DPO_NO_PRODUCT_DECISIONS_002`

What the model should learn: select `no-product-decisions` when a task asks the agent to choose user-visible behavior, public API direction, workflow policy, or architecture/product direction without a current human owner decision.

What the model should reject: treating the task as implementation, startup, or claim verification when the real boundary is that the agent must not make the product decision itself.

### output-skepticism

Rows: `ROW13_1A_DPO_OUTPUT_SKEPTICISM_001`, `ROW13_1A_DPO_OUTPUT_SKEPTICISM_002`

What the model should learn: select `output-skepticism` when a completion, pass, readiness, or quality claim may outrun the evidence, depend on a proxy marker, be stale, or conflict with available facts.

What the model should reject: packaging the phase or requiring generic verification as the primary skill when the immediate need is to downgrade unsupported claims to the narrow evidence actually shown.

### self-verification

Rows: `ROW13_1A_DPO_SELF_VERIFICATION_001`, `ROW13_1A_DPO_SELF_VERIFICATION_002`

What the model should learn: select `self-verification` when the implementer who changed an artifact must independently verify that change using fresh cases, read-back, expected-versus-actual comparison, blast radius classification, and cascade tracing.

What the model should reject: selecting test-only, skepticism-only, or implementation-boundary skills when the work order requires the changer to produce self-owned post-change proof.

## Counts

- Rows total: 8
- Rows per skill: 2 each
- Usable rows: 8
- Quarantined rows: 0
- Universal-only: true
- Source-disjoint: true
- No vendor output: true
- No training performed: true
- No checkpoint mutation: true

## Validation

- JSONL parse: pass
- JSON summary parse: pass
- Required fields present on every row: pass
- Required phrase read-back: `universal-only`, `source-disjoint`, `no vendor output`, `quarantine`, `no checkpoint mutation`
- Skill read-back: `e2e-test-matrix`, `no-product-decisions`, `output-skepticism`, `self-verification`

## Next Small Step

Row13.1b candidate training/export can use these rows as candidate-only input, with held-out Row13.1 rows kept quarantined for evaluation.
