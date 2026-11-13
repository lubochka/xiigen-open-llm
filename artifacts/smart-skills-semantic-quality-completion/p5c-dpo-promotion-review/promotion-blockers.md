# Promotion blockers

## Named-reference and todo sources

Five P5A rows are still named-reference/todo in the observed analysis state. They cannot become training-ready, and most cannot enter actual DPO materialization until parent/source acceptance:

- `P5A-CAND-BOTH-SKILLS-INVENTORY-001`
- `P5A-CAND-ORCH-DOC07956-001`
- `P5A-CAND-ORCH-UNIVERSAL-DEPS-001`
- `P5A-CAND-ORCH-NINE-NODE-FLOW-001`
- `P5A-CAND-ORCH-FLOW-ISOLATION-001`

`P5A-CAND-BOTH-SKILLS-INVENTORY-001` and `P5A-CAND-ORCH-DOC07956-001` are evidence-only, because their universal boundaries look useful but source acceptance is not complete.

## Same-source variants

P3 has two PromptToTask variants from the same `basic prompt.txt` source family. P4 has two OrchestrationReasoning variants from the same phase-transition source family. P5C selects one representative per family:

- keep as next-authoring candidate: `P3-PTT-DRAFT-001`
- merge/drop: `P3-PTT-DRAFT-002`
- keep as next-authoring candidate: `P4-ORCH-DPO-DRAFT-001`
- merge/drop: `P4-ORCH-DPO-DRAFT-002`

The merge/drop rows may inform rewrites, but they do not add independent source-disjoint evidence.

## Shortcut risks

The highest-risk rows are quarantined:

- `P5A-CAND-ORCH-UNIVERSAL-DEPS-001`: code filename, class/method names, namespace and CoNLL-U token leakage.
- `P5A-CAND-ORCH-NINE-NODE-FLOW-001`: exact-nine, node id, Project15Bridge, SemanticCompiler29 and class/file leakage.
- `P5A-CAND-ORCH-FLOW-ISOLATION-001`: test names, xUnit, Assert, run ids, temp paths and exactly-nine assertion leakage.

These rows need anonymized, non-code rewrites before any future authoring review can reconsider them.

## Near-duplicate concern

`P5A-CAND-ORCH-DOC07956-001` and `P5A-CAND-ORCH-NINE-NODE-FLOW-001` both point toward flow/station/branch orchestration. They can be useful only if their teaching boundaries are separated:

- doc07956 should teach multi-branch orchestration, pre-format review and targeted repair.
- NineNodeFlow should teach station-chain evidence carrying and review-before-claim.

Until that separation is accepted, NineNodeFlow remains quarantined and doc07956 remains evidence-only.

## Code and test leakage

Common DPO must not learn from C# syntax, `.cs` filenames, class names, method names, namespace names, test framework markers, exact node ids, source paths or local project terms. Any future rewrite must prove that the chosen answer survives after those markers are removed, and that marker-only baselines fail.
