# Codex Plan 04 - DynamicGraph Machine

Artifact: `codex-plan-04-dynamicgraph-machine.md`
Status: PLAN-ONLY, Codex-owned.
Encoding recovery note: recovered after cycle-42 status-sync encoding fault. This ASCII file restates the active canonical graph-machine contract.

## Authority Anchor

```text
CURRENT TASK: Improve the Codex ILearningDynamicGraph plan and finish 50 real review cycles.
NOT THE TASK: No implementation, tests, builds, git, training, Claude-file edits, or false cycle claims.
EVIDENCE-ONLY: Claude benchmark, old packets, old overlays, and foreign artifacts.
```

## Canon

ILearningDynamicGraph is an L5 `ILearningMachine<DynamicGraphDomain>`. It is not a sixth learning level. V1 uses explicit node and edge objects plus topology-build results; it does not use an adjacency matrix as the primary representation.

```csharp
public enum GraphEdgeKind
{
    FanOut,
    FanIn,
    ManyToMany,
    MultiOutput,
    ReturnLoop
}

public interface IDynamicGraphEdge
{
    string FromNodeId { get; }
    IReadOnlyList<string> ToNodeIds { get; }
    GraphEdgeKind EdgeKind { get; }
    string AuthorityUnitId { get; }
    string AuthorityUnitLevel { get; }
    bool ParallelEligible { get; }
    bool DeterministicFinalAuthority { get; }
    string? BaselineUnitId { get; }
    string BaselineRole { get; }
    IReadOnlyList<string> ExcludedLeakageSignals { get; }
    string SourceProof { get; }
    string CutoverProof { get; }
    string AblationProofRef { get; }
}

public OperationResult<BuildDynamicTopologyResult> BuildDynamicTopology(
    DynamicGraphBuildSignal signal,
    TopologyBuildBudget budget);
```

Baseline/fallback state is represented by `BaselineUnitId` and `BaselineRole`, not by a null final authority. The edge final authority is a learned unit id and must not receive candidate index, source path, row order, file order, or deterministic router output as authority input.

## Gate B

- `GraphEdgeKind` contains exactly the canonical five values.
- `IDynamicGraphEdge.AuthorityUnitId` is non-null in the target contract.
- `DeterministicFinalAuthority=false` after learned cutover.
- `BuildDynamicTopology(signal,budget)` returns topology, budget proof, failure reason when budget is exceeded, and mutation evidence.
- No V1 plan section reintroduces adjacency matrix as the source of authority.
- Edge leakage/permutation/ablation checks pass before F8 cutover.
- Current comparison-run packet records: 50/50; C49-C50 returned and integrated
  in the current-run ledger.

## Final Questions Only

None.
