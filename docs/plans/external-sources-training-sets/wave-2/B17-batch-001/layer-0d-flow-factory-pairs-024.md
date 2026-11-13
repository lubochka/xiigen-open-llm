# Layer 0-D — Flow Factory Pairs (024)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Complete factory reference for B17 connectors 013-024

---

## B17-0D-024-001 — Factory decision tree (complete for B17)

```
Predicate set arrives at factory
  ↓
For each predicate in canonical order:
  IMPLEMENT + integration vocabulary → 4.3
    Check: Is IntegrationConnectorFlow43 in probationary_accepted or trusted_learned?
      Yes → USE that status
      No → USE deterministic_generic (fallback)

  API_CONTRACT → 3.3
    Always deterministic_generic → APIContractFlow33

  SECURITY → 3.4
    Always deterministic_generic → SecurityAuditFlow34

  AUTHORIZATION → 3.4
    Always deterministic_generic → AuthorizationFlow34

  TEST → 5.1
    Always deterministic_generic → TestingFlow51

  DOCUMENT → 2.1
    Always deterministic_generic → DocumentationFlow21

  QA → 7.x (ALWAYS LAST)
    Always deterministic_generic → QAGatingFlow73
```

---

## B17-0D-024-002 — Gate B requirements for IntegrationConnectorFlow43 promotion

For a connector candidate to advance from deterministic_generic to probationary_accepted:

**Gate B conditions:**
1. Nine-node flow executes without error on N01 through N09
2. N08 (ArbiterUnit) verdict = PASS
3. All required connector interface methods implemented (minimum 2 handlers)
4. No N08 abort (probationary_rejected)

**Gate B conditions for probationary_accepted → trusted_learned:**
1. Gate B passes across ≥ 9 distinct connector evidence instances
2. Minimum confidence at N08: 0.90 across all instances
3. No production failures in the probationary period

---

## B17-0D-024-003 — Factory output schema reference

```
FactoryOutput: {
  SelectedFlows: [
    {
      subPhase: string,       // "4.3", "3.3", "3.4", "5.1", "2.1", "7.x"
      flow: string,           // FlowId
      status: string,         // "deterministic_generic", "probationary_accepted", "trusted_learned"
      position?: string       // "last-absolute" for QAGatingFlow73 only
    }
  ],
  TransitionType: string,     // "feature_addition", "refactoring", "documentation", "no_contribution"
  FallbackUsed: boolean,
  RejectedCandidateFlowId?: string  // present only if probationary_rejected
}
```

---

## B17-0D-024-004 — Factory anti-patterns for B17 connectors 013-024

| Anti-pattern | Description | Correction |
|---|---|---|
| Selecting IntegrationConnectorFlow43 for API_CONTRACT alone | 4.3 requires IMPLEMENT | Only select 4.3 when IMPLEMENT fires |
| Not selecting AuthorizationFlow34 when AUTHORIZATION fires | Missing flow for AUTHORIZATION predicate | AUTHORIZATION → AuthorizationFlow34 (3.4) always |
| Selecting QAGatingFlow73 in non-last position | QA is always last | Enforce `position: last-absolute` |
| Selecting IntegrationConnectorFlow43 for "integrate" (no IMPLEMENT) | "integrate" fires API_CONTRACT not IMPLEMENT | No 4.3 without IMPLEMENT |
| Inventing a GapHandler flow | No GapHandler exists in B17 | If no predicate fires, no flow is selected |
