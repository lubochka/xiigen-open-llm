# Wave 2 Center Phase Batch 001 - Held-Out Manifest v1
# Date: 2026-05-05

---

## Purpose

Define the held-out conformance seed for the first Wave 2 center-phase batch.

These rows are not training rows. They are reserved for checking whether the
`sub-phase-router` can classify unseen center-phase material without drifting
to a neighboring sub-phase.

---

## Evaluation Surface

```text
Logical unit role: sub-phase-router
Input shape: SubPhaseRouterSourceDocument
Output shape: LogicalUnitResult<SubPhaseRoutingDecision>
Default flags:
  UseCompiledPredicateRepresentation = true
  UseDomainScopedRetrieval = true
  UseConfidenceFloor = true
  UseArbiterAuthority = true
  UseKnowledgeEdgeDetection = true
  UseInternalFirstExecution = true
  RequireExternalAiApproval = true
```

---

## Held-Out Rows

| held_out_id | source_document_id | expected_phase_id | expected_sub_phase_id | category | minimum_confidence | maximum_wrong_sub_phase_score | expected_knowledge_edge | expected_conflict_handling |
|---|---|---|---|---|---:|---:|---|---|
| W2C-HLD-001 | W2C-3.1-H-001 | phase-3-technical-design | 3.1-architecture-and-component-topology | correct flow selection | 0.70 | 0.49 | none | none |
| W2C-HLD-002 | W2C-3.3-H-002 | phase-3-technical-design | 3.3-api-contract-definition | correct flow selection | 0.70 | 0.49 | none | none |
| W2C-HLD-003 | W2C-3.4-H-003 | phase-3-technical-design | 3.4-security-and-authorization-model | conflict detection | 0.70 | 0.49 | none | harmless logout must not be accepted |
| W2C-HLD-004 | W2C-4.1-H-004 | phase-4-implementation | 4.1-backend-services | edge cases | 0.70 | 0.49 | none | missing evidence anchor must remain visible |
| W2C-HLD-005 | W2C-4.3-H-005 | phase-4-implementation | 4.3-integration-and-provider-connectors | cross-sub-phase constraint | 0.70 | 0.49 | none | wrong arbiter family must not be selected |

---

## Pass Conditions

This held-out seed passes only when all rows satisfy every condition below.

```text
5/5 rows return OperationResult success.
5/5 rows predict the expected phase id.
5/5 rows predict the expected sub-phase id.
5/5 rows produce confidence >= 0.70.
5/5 rows keep nearest wrong sub-phase score < 0.50.
0 rows create a source knowledge edge.
0 rows create a router knowledge edge.
```

Conflict rows have a downstream guard:

```text
conflict detection rows must not be rewritten into harmless formatter language
```

The downstream guard is recorded here because the router row is the first place
where the conflict must remain attached to the correct sub-phase.

---

## Promotion Status

This manifest is a conformance seed, not a full held-out set.

It contributes:

```text
5 held-out documents toward the 620-document sub-phase held-out target
```

The full v4 corpus remains incomplete until every sub-phase owns 20 held-out
documents and passes at least 17/20 with zero conflict leakage.
