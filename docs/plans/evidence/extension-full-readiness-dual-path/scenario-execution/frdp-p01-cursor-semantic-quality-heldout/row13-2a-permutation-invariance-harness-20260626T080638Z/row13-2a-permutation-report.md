# Row13.2a Permutation/Invariance Harness

Status: `ROW13_2A_PERMUTATION_PASS`
Runtime: `SkillSelectionMachine.LoadCheckpointAsync + SkillSelectionMachine.DecideFromTrainedState`
permutationInvariant: `true`
candidate-position dependence detected: `false`
no checkpoint mutation: `true`
Row13.2 can proceed: `true`

Exact held-out prompt text was unavailable in Row13.1c; Row13.2a used one source-disjoint Row13.1a training prompt variant per target skill as a proxy permutation test only.

| row | expected | original | reversed | rotated | invariant | expected across permutations |
| --- | --- | --- | --- | --- | --- | --- |
| P24-SS-HOLDOUT-E2E-MATRIX-001 | e2e-test-matrix | e2e-test-matrix | e2e-test-matrix | e2e-test-matrix | True | True |
| P24-SS-HOLDOUT-NO-PRODUCT-DECISION-001 | no-product-decisions | no-product-decisions | no-product-decisions | no-product-decisions | True | True |
| P24-SS-HOLDOUT-OUTPUT-SKEPTICISM-001 | output-skepticism | output-skepticism | output-skepticism | output-skepticism | True | True |
| P24-SS-HOLDOUT-SELF-VERIFICATION-001 | self-verification | self-verification | self-verification | self-verification | True | True |

Read-back phrases: original, reversed, rotated, candidate-position, permutationInvariant, no checkpoint mutation.
Proxy permutation/invariance harness only; no product readiness, token savings, full semantic quality, extension readiness, training, mutation, promotion, or locator update claimed.
