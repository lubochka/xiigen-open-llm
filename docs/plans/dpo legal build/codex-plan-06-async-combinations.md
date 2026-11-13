# Codex Plan 06 - Async Combinations

Artifact: `codex-plan-06-async-combinations.md`
Status: PLAN-ONLY, Codex-owned.
Encoding recovery note: recovered after cycle-42 status-sync encoding fault.

## Authority Anchor

```text
CURRENT TASK: Preserve async/fan-in/fan-out planning for ILearningDynamicGraph and finish 50 real review cycles.
NOT THE TASK: No code, tests, builds, git, training, Claude-file edits, or false cycle claims.
EVIDENCE-ONLY: Claude benchmark and old packets.
```

## Canon

Async graph execution supports fan-out, fan-in, many-to-many, and return-loop flows without hiding a deterministic winner. Async branches may produce candidates, evidence, and measurements; the final edge decision belongs to the learned edge authority and is checked by leakage/permutation gates.

Required bridge records: `GraphAsyncBranchResult`, `GraphFanInMergeInput`, `GraphManyToManyRoute`, `EdgeAuthorityInput`, `GraphDpoPair`, `GeneratedUiOutputFeedbackEvent`.

`EdgeAuthorityInput` excludes candidate index, source path, file order, row order, and deterministic router score when those signals would become shortcut authority.

## Gate B

- Fan-out with one target is a degenerate one-to-one case; it does not create a separate authority rule.
- Fan-in merge records all branch ids and source hashes without leaking order as authority.
- Many-to-many routing records source/target ids and legal/provenance proof.
- Async readability examples A/B/C/D remain examples, not hidden gates.
- `GeneratedUiOutputFeedbackEvent` can enter the DPO builder only after legal provenance and source split checks.
- Current comparison-run packet records: 50/50; C49-C50 returned and integrated
  in the current-run ledger.

## Final Questions Only

None.
