# Layer 0-D — Flow Factory Pairs
# Batch: B26-batch-001
# Scenario: B26 — QA security review, sub-phase 6.3
# Contract: FlowFactoryUnit : ILogicalUnit<SubPhaseRouterResult, FlowInstanceDescriptor>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: factory_selection

---

## Pair B26B1-WDA-factory-001 — SECURITY / 6.3 + QA / 7.3 → two deterministic_generic flows

```
source_id: xiigen-b26-factory-001
transition_type: feature_addition
phase: 6
sub_phase: 6.3
pair_kind: factory_selection
```

A:
  sub_phase: [6.3, 7.3]
  predicate: ["SECURITY", "QA"]
  compiled_text: "Audit the authentication layer for security vulnerabilities and QA the access control policies."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "SecurityReviewFlow63"
      predicate: SECURITY
      sub_phase: 6.3
      routing_note: "SECURITY → 6.3 (Security and Compliance Review)"
    - FlowId: "QATestingFlow73"
      predicate: QA
      sub_phase: 7.3
      routing_note: "QA → 7.3 (QA Testing and Validation)"
  SelectionReason: "Two predicates → two independent flow instances; both deterministic_generic"
  NodeCount_per_flow: 9

---

## Pair B26B1-WDA-factory-002 — SECURITY / 6.3 + API_CONTRACT / 3.3 → two flows

```
source_id: xiigen-b26-factory-002
transition_type: feature_addition
phase: 6
sub_phase: 6.3
pair_kind: factory_selection
```

A:
  sub_phase: [6.3, 3.3]
  predicate: ["SECURITY", "API_CONTRACT"]
  compiled_text: "Review the API endpoints for SQL injection vulnerabilities and enforce input sanitization."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "SecurityReviewFlow63"
      predicate: SECURITY
      sub_phase: 6.3
    - FlowId: "ApiContractDesignFlow33"
      predicate: API_CONTRACT
      sub_phase: 3.3
  SelectionReason: "Two predicates → two independent flow instances; both deterministic_generic"

---

## Pair B26B1-WDA-factory-003 — QA + DOCUMENT + SECURITY → three flows

```
source_id: xiigen-b26-factory-003
transition_type: feature_addition
phase: 6
sub_phase: 6.3
pair_kind: factory_selection
```

A:
  sub_phase: [7.3, 6.2, 6.3]
  predicate: ["QA", "DOCUMENT", "SECURITY"]
  compiled_text: "QA the release candidate and document all findings in the security review report."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "QATestingFlow73"
      predicate: QA
      sub_phase: 7.3
    - FlowId: "DocumentationFlow62"
      predicate: DOCUMENT
      sub_phase: 6.2
    - FlowId: "SecurityReviewFlow63"
      predicate: SECURITY
      sub_phase: 6.3
  SelectionReason: "Three predicates → three independent flow instances; all deterministic_generic"

---

## Pair B26B1-WDA-factory-004 — SECURITY weak-signal / "authentication" only → probationary_accepted

```
source_id: xiigen-b26-factory-004
transition_type: unknown
phase: 6
sub_phase: 6.3
pair_kind: factory_selection
```

A:
  sub_phase: 6.3
  predicate: SECURITY
  compiled_text: "Run a pentest on the authentication service before the release."
  signal_strength: "weak — single authentication term; gap action verb"

B:
  TrustState: probationary_accepted
  FlowId: "SecurityReviewFlow63"
  FlowLabel: "Security and Compliance Review Flow — sub-phase 6.3"
  SelectionReason: "SECURITY fires on 'authentication' alone (weak signal) with gap action
    verb 'pentest' — probationary_accepted (not full deterministic_generic)"
  NodeCount: 9
  CapabilityGapWarnings:
    - "gap-security-pentest-001: 'pentest' is gap vocabulary; routing confidence reduced"

  Trust state reasoning: A single security-domain noun ("authentication") without
  a security-action verb produces a weak SECURITY signal. probationary_accepted reflects
  the reduced routing confidence compared to requests with dense SECURITY vocabulary
  (audit + vulnerability + authentication + access control).
