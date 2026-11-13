# Layer 0-D — Flow Factory Selection Pairs (024)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-024-001 — Trust state reference

| TrustState | Condition | Flow Selected |
|------------|-----------|---------------|
| deterministic_generic | No registered candidate flow | Fallback (universal) |
| probationary_accepted | Candidate registered, Gate B passed | Candidate |
| probationary_rejected | Candidate registered, Gate B failed | Fallback (universal) |
| trusted_learned | Promoted past threshold (≥25 evidence pairs) | Candidate |

---

## B15-factory-024-002 — Gate B criteria for promotion from probationary to trusted_learned

Gate B passes when ALL of:
1. All 9 nodes covered (N01 through N09)
2. min_confidence ≥ 0.89 across all nodes
3. Promotion threshold reached: ≥ 25 production evidence pairs

Gate B fails when ANY of:
1. Any node missing coverage
2. Any node confidence below 0.89
3. Evidence pairs below threshold (threshold check only applies to promotion, not Gate B pass/fail for selection)

---

## B15-factory-024-003 — Factory call count matches predicate count

Rule: Factory is called exactly once per unique predicate after deduplication.
Example: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST"] → 4 factory calls → 4 flow selections.
Anti-pattern: Calling factory once for all predicates and returning all flows → incorrect.
Each factory call independently queries the flow registry for its sub-phase.

---

## B15-factory-024-004 — TransitionType passthrough scope

TransitionType: refactoring only applies to ImplementationFlow41 (sub-phase 4.1) execution.
Other flows (APIContractFlow33, ArchitectureFlow31, SecurityAuditFlow, TestingFlow51, DocumentationFlow21) are not modified by TransitionType.
Factory passes TransitionType as metadata to ImplementationFlow41 only.

---

## B15-factory-024-005 — Complete universal flow ID reference for B15

| Sub-phase | Universal Flow ID |
|-----------|-------------------|
| 2.1 | universal-nine-node-documentation-flow-2.1 |
| 3.1 | universal-nine-node-architecture-flow-3.1 |
| 3.3 | universal-nine-node-contract-flow-3.3 |
| 4.1 | universal-nine-node-development-flow-4.1 |
| 5.1 | universal-nine-node-testing-flow-5.1 |
| 5.3 | universal-nine-node-e2e-testing-flow-5.3 |
| security | universal-security-audit-flow |
| 7.1 | universal-qa-gating-flow-7.1 |
| 7.3 | universal-release-gate-flow-7.3 |
