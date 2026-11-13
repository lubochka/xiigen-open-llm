# Codex Plan 12 - No Hidden Determinism

Artifact: `codex-plan-12-no-hidden-determinism.md`
Status: PLAN-ONLY, Codex-owned.
Encoding recovery note: recovered after cycle-42 status-sync encoding fault.

## Authority Anchor

```text
CURRENT TASK: Preserve no-hidden-determinism planning and finish 50 real review cycles.
NOT THE TASK: No implementation, tests, builds, git, training, Claude-file edits, or false cycle claims.
EVIDENCE-ONLY: Claude benchmark and old packets.
```

## Canon

The final authority for a graph edge is learned. Deterministic routers, counters, dictionaries, source order, file order, row order, candidate index, or provider names may generate candidates or eval baselines; they must not become final authority after cutover.

```csharp
public interface IDynamicGraphEdge
{
    string AuthorityUnitId { get; }
    string AuthorityUnitLevel { get; }
    bool DeterministicFinalAuthority { get; }
    string? BaselineUnitId { get; }
    string BaselineRole { get; }
    IReadOnlyList<string> ExcludedLeakageSignals { get; }
    string SourceProof { get; }
    string CutoverProof { get; }
    string AblationProofRef { get; }
}
```

No direct `ILogicalUnit` object reference is stored on the edge contract; the edge stores a canonical authority id.

## 9 Mechanism Inventory

Each deterministic mechanism must be classified before cutover. Candidate or
baseline mechanisms may stay only when they cannot become final edge authority.

| # | Mechanism | Deterministic risk | Disposition after cutover | Required proof |
|---|---|---|---|---|
| 1 | `MultiSubPhaseRouterUnit.Score` | final scoring by fixed heuristic | baseline/eval only | `ablation_delta > 0` and `baseline_retained_for_ablation=true` |
| 2 | `KeywordUnitScorer.Score` | keyword weight becomes hidden winner | candidate generator only | `keyword_score_used_as_final_authority=false` and `routing_source_excluded=true` |
| 3 | `SubPhaseRouterUnit` | static router path decides winner | baseline retained for ablation | `ResolveLatest.trained_authority_selected=true` only after `PASS_AS_TRAINED_FEATURE` |
| 4 | `OrchestrationScenarioPhasePlanner.PlanPhases` | fixed phase plan becomes final route | planner/candidate source only | `final_edge_authority_ref` points to trained authority, not planner output |
| 5 | `RoutingProfileTrainer.Train` | training profile hard-codes route | training helper only | `heldout_accuracy_delta > 0`, `reward_margin > 0`, `profile_bypasses_ablation_gate=false` |
| 6 | `KnnLogicalUnit` | nearest-neighbor lookup becomes winner | retrieval candidate source | `candidate_index_excluded=true` and `source_id_excluded=true` in `EdgeAuthorityInput` |
| 7 | `InMemoryLogicalUnitRegistry.ResolveLatest` | latest-wins hides unverified cutover | cutover mechanism only | `trained_authority_registered_after_pass=true` and `rollback_ref_present=true` |
| 8 | `BuildPathFreeScoringText` | prompt/source path leakage | text builder only | `exactPromptLeakageRate == 0.0`, `file_path_excluded=true`, `source_path_excluded=true` |
| 9 | confidence-floor / `KnowledgeEdge` | confidence threshold becomes final deny/allow | guard/candidate filter only | `confidence_guard_metric_ref_present=true` and `learned_authority_remains_final=true` |

`classified_mechanism_count == 9` is a Gate B requirement.

## Cutover Guard

Cutover from baseline to trained edge authority is allowed only when all
conditions below are true:

- `PASS_AS_TRAINED_FEATURE` is present from plan-11 measurement.
- Held-out accuracy improves and reward margin is positive.
- Fresh-load, continue-training, export/import, and rollback evidence exist.
- Baselines remain registered for ablation; they are not deleted as proof.
- Permutation negative tests prove candidate order does not change the final winner.
- `exactPromptLeakageRate == 0.0`.
- Source path, file path, candidate index, and provider name are excluded from `EdgeAuthorityInput`.
- `classified_mechanism_count == 9`.
- Legal `TeacherProvenance` is attached to any training data used for the learned authority.

## Gate B

- `DeterministicFinalAuthority=false` after learned cutover.
- `EdgeAuthorityInput` excludes leakage signals.
- `classified_mechanism_count == 9`.
- `cutover_guard_complete=true`: `PASS_AS_TRAINED_FEATURE`, retained baselines,
  permutation guard, leakage guard, legal `TeacherProvenance`, and rollback refs
  are all present.
- F8 cutover requires plan-11 two-PASS proof.
- Leakage and permutation checks re-run after cutover.
- Ablation proves the learned authority beats or justifies replacing the deterministic baseline.
- Current comparison-run packet records: 50/50; C49-C50 returned and integrated.

## Final Questions Only

None.
