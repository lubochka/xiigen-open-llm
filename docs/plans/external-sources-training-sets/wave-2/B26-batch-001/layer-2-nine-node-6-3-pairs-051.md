# Layer 2 — Nine-Node Flow Pairs (051): ProbationaryRejected — ZeroTrust Write-Test Context
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# SubPhase: 6.3 (attempted) → probationary_rejected | Flow: SecurityComplianceReviewFlow63 attempted
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## N051 — Probationary Rejected: Write-Tests Context Routed to 6.3 — Zero Trust

### Node 1 — DomainSplitter
A = SelectedLogicalTransition: SecurityComplianceReviewFlow63 (attempted)
Context mismatch detected: "write tests asserting Zero Trust mTLS enforcement" = test-writing context
DomainSplitter sets ProbationaryWarning: routed to 6.3 but context indicates 5.2 (SecurityTestingFlow52).
TrustState: probationary_accepted (context mismatch)

### Node 2 — DomainTranslator
Note: Probationary context propagated. Test-writing context identified.
TranslatedAuditBoundaries: N/A — context mismatch prevents full translation in 6.3 frame.

### Node 3 — SchemePlanner
Schema deferred — probationary rejection expected at Node 8.

### Node 4 — DomainProcessor
Processing halted — probationary rejection path active.

### Node 5 — Applier
N/A — probationary rejection path.

### Node 6 — ResultTranslator
N/A — probationary rejection path.

### Node 7 — ArbiterSelector
Arbiter: ContextArbiter (probationary_accepted with context mismatch → ContextArbiter selected)
Config: reject on context mismatch between write-tests action and 6.3 audit sub-phase.

### Node 8 — Arbiter
OverallVerdict: probationary_rejected
Detail: Request is "write tests asserting mTLS enforcement" — this is SecurityTestingFlow52 (5.2), not SecurityComplianceReviewFlow63 (6.3). Routing mismatch confirmed by ContextArbiter.

### Node 9 — Formatter
status: "probationary_rejected — re-route to SecurityTestingFlow52 (5.2)"
output_status: "probationary_rejected"
file: N/A
Teaching: Write-tests context for security → always 5.2. Probationary routing to 6.3 is rejected by ContextArbiter.
