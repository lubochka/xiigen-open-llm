# Cross-Layer Synthesis (004)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## S004 — End-to-end trace: Four-predicate — "Design infrastructure, implement IaC, validate connectivity, document ADRs, get QA sign-off"

### Layer 0-A
Input: "Design the zero-trust network infrastructure, implement the Terraform modules, validate the mTLS connectivity, document the ADRs, and get QA sign-off before production."
  "design" → ARCHITECTURE | "zero-trust network infrastructure" → ARCHITECTURE + SECURITY ("zero-trust") | "implement Terraform modules" → IMPLEMENT | "validate" → TEST | "mTLS connectivity" → SECURITY ("mTLS") | "document the ADRs" → DOCUMENT + ARCHITECTURE ("ADRs") | "QA sign-off" → QA
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY", "IMPLEMENT", "TEST", "DOCUMENT", "QA"]

### Layer 0-B
  ARCHITECTURE → 3.5 | SECURITY → 6.3 (audit existing) | IMPLEMENT → 4.1 | TEST → 5.1 | DOCUMENT → 6.2 | QA → 7.3
  DecisionSet: Six decisions, six sub-phases

### Layer 0-D
  FlowFactory.CreateMulti([...]) → [InfrastructureArchitectureFlow35, SecurityAuditFlow63, ImplementationFlow41, UnitTestingFlow51, DocumentationFlow62, QAGatingFlow73]
  Six concurrent flows

### Layer 1
  TransitionType: feature_addition (IMPLEMENT present) | Sequential: ARCHITECTURE → SECURITY → IMPLEMENT → TEST → DOCUMENT → QA gate

### Layer 2 (InfrastructureArchitectureFlow35)
  Zero-trust network: Istio STRICT mTLS, NetworkPolicy deny-all, egress gateway allowlist
  Node 7: PASS

### Layer 3
  Teaching: "zero-trust" fires SECURITY alongside ARCHITECTURE. "validate mTLS" fires TEST. "ADRs" fires ARCHITECTURE independently. Six-predicate requests create six concurrent flows. QA always last.

### Cross-layer invariant
  Six predicates → six flows. No cap on predicate count. IMPLEMENT drives feature_addition. QA gating always last in sequential ordering. "zero-trust" and "mTLS" fire SECURITY independently of ARCHITECTURE.

