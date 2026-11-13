# Layer 2 — Nine-Node Flow Pairs (023)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: nine_node_flow (E2ETestingFlow53)
# Flow: ProbationaryRejected | Predicate: TEST+UI+QA | accessibility flow REJECTED

---

## B39-nine-023 — Probationary accessibility flow — REJECTED

Request: "Write accessibility tests that validate the ARIA roles for the modal, then certify WCAG 2.1 AAA compliance, and QA sign-off for production release to staging."
Predicate set: ["TEST", "UI", "QA"] | TrustState: probationary → REJECTED

### Flow: E2ETestingFlow53 + UIInteractionFlow15 + QAGatingFlow73 (attempted)

**Node 1 — DomainSplitter**: SelectedLogicalTransition: E2ETestingFlow53
Target: Modal — ARIA roles, WCAG 2.1 AAA compliance claim, QA gate for staging deployment.
Issue detected at Node 1: "WCAG 2.1 AAA" (triple-A) is a higher standard than B39's default WCAG 2.1 AA. The B39 QA gate only certifies AA compliance. AAA certification is out of scope.

**Node 2 — DomainTranslator**:
  Attempted: ARIA roles validation ✓ (feasible).
  Attempted: WCAG 2.1 AAA certification — FAIL (out of scope; AAA requires contrast ratio 7:1 and additional success criteria not covered in B39 test suite).
  Attempted: QA sign-off for staging → "staging" vocabulary → this shifts QA routing to 7.1 (release pipeline), not 7.3 (feature acceptance gate). Routing conflict detected.

**Node 3 — SchemePlanner**: Flow enters probationary state due to two scope violations:
  (1) WCAG 2.1 AAA out of scope for E2ETestingFlow53 (B39 only certifies AA).
  (2) "staging deployment" shifts QA to 7.1 but the flow was instantiated as QAGatingFlow73.

**Nodes 4–8**: Skipped — probationary rejection before domain processing.

**Node 9 — FormatterUnit**:
  ProbationaryVerdict: REJECTED
  RejectionReasons:
    (1) AAA_OUT_OF_SCOPE: B39 certifies WCAG 2.1 AA only. AAA certification requires a separate flow not in B39.
    (2) QA_ROUTING_CONFLICT: "production release to staging" vocabulary → QA should route to 7.1 (ReleaseGateFlow71) not 7.3 (QAGatingFlow73). Re-instantiate with correct QA sub-phase.
  Recommendation: Rewrite request as "Write accessibility tests for WCAG 2.1 AA compliance on the modal and QA sign-off before merge" → TEST+UI+QA → 5.3+1.5+7.3 (correct flow).
  OutputStatus: rejected_probationary

### Teaching Point

"certify WCAG 2.1 AAA" is not in B39 scope. "production release to staging" shifts QA from 7.3 to 7.1. Both scope violations cause probationary rejection.
