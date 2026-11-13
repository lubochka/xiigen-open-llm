# Layer 0-B — Router Pairs
# Batch: B26-batch-001
# Scenario: B26 — QA security review, sub-phase 6.3
# Contract: PromptRoutingInputRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRouterResult>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: router

---

## Pair B26B1-WDA-router-001 — SECURITY + QA → 6.3 + 7.3

```
source_id: xiigen-b26-router-001
transition_type: feature_addition
phase: 6
sub_phase: 6.3
pair_kind: router
```

A:
  compiled_predicate_ids: ["SECURITY", "QA"]
  compiled_text: "Audit the authentication layer for security vulnerabilities and QA the access control policies."

B:
  RoutingDecisions:
    - predicate: SECURITY
      sub_phase: 6.3
      sub_phase_label: "Security and Compliance Review"
      confidence: 0.92
      routing_note: "'audit' + 'authentication' + 'vulnerabilities' + 'access control' → SECURITY → 6.3"
    - predicate: QA
      sub_phase: 7.3
      sub_phase_label: "QA Testing and Validation"
      confidence: 0.92
      routing_note: "'QA' literal → QA predicate → 7.3"

  TransitionType: feature_addition
  PrimarySubPhaseId: 6.3
  AllSubPhaseIds: [6.3, 7.3]

  Teaching: SECURITY routes to 6.3 (Security and Compliance Review) in a review/audit
  context — distinct from 5.2 (Security Testing) which applies to test-suite writing.
  QA routes to 7.3 independently.

---

## Pair B26B1-WDA-router-002 — SECURITY + API_CONTRACT → 6.3 + 3.3

```
source_id: xiigen-b26-router-002
transition_type: feature_addition
phase: 6
sub_phase: 6.3
pair_kind: router
```

A:
  compiled_predicate_ids: ["SECURITY", "API_CONTRACT"]
  compiled_text: "Review the API endpoints for SQL injection vulnerabilities and enforce input sanitization."

B:
  RoutingDecisions:
    - predicate: SECURITY
      sub_phase: 6.3
      sub_phase_label: "Security and Compliance Review"
      confidence: 0.92
      routing_note: "'injection' + 'vulnerabilities' + 'enforce' + 'sanitization' → SECURITY → 6.3"
    - predicate: API_CONTRACT
      sub_phase: 3.3
      sub_phase_label: "API and Integration Design"
      confidence: 0.92
      routing_note: "'API endpoints' → API_CONTRACT → 3.3"

  TransitionType: feature_addition
  PrimarySubPhaseId: 6.3
  AllSubPhaseIds: [6.3, 3.3]

---

## Pair B26B1-WDA-router-003 — QA + DOCUMENT + SECURITY → 7.3 + 6.2 + 6.3

```
source_id: xiigen-b26-router-003
transition_type: feature_addition
phase: 6
sub_phase: 6.3
pair_kind: router
```

A:
  compiled_predicate_ids: ["QA", "DOCUMENT", "SECURITY"]
  compiled_text: "QA the release candidate and document all findings in the security review report."

B:
  RoutingDecisions:
    - predicate: QA
      sub_phase: 7.3
      sub_phase_label: "QA Testing and Validation"
      confidence: 0.92
      routing_note: "'QA' → 7.3"
    - predicate: DOCUMENT
      sub_phase: 6.2
      sub_phase_label: "Documentation and Knowledge Base"
      confidence: 0.92
      routing_note: "'document' → DOCUMENT → 6.2"
    - predicate: SECURITY
      sub_phase: 6.3
      sub_phase_label: "Security and Compliance Review"
      confidence: 0.92
      routing_note: "'security review' → SECURITY → 6.3"

  TransitionType: feature_addition
  PrimarySubPhaseId: 6.3
  AllSubPhaseIds: [7.3, 6.2, 6.3]
