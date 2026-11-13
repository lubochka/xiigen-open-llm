# Cross-Layer Synthesis (004)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: cross-layer-synthesis

---

## B17-CLS-004 — Seven-predicate maximum pipeline: Full integration cycle

**Input:**
"Build the enterprise payment connector, implement OAuth token refresh, secure client credentials, document the payment API interfaces, write integration tests, and QA the end-to-end payment flow."

---

### Layer 0-A (Compiler)
```
Triggers detected:
- "Build" → IMPLEMENT
- "implement" → IMPLEMENT (reinforces)
- "connector" → API_CONTRACT
- "OAuth" → AUTHORIZATION
- "token" → SECURITY
- "secure" → SECURITY (reinforces; fires once)
- "document" → DOCUMENT
- "write integration tests" → TEST
- "QA" → QA (Rule 21: abbreviation only)
- Integration vocabulary: connector, OAuth, token, API → 4.3 context

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT", "QA"]
(canonical order, QA last per Rule 21)
TransitionType: feature_addition
DomainId: software-development
```

---

### Layer 0-B (Router)
```
Routing decisions:
1. IMPLEMENT → 4.3 (IntegrationConnectorFlow43)
2. API_CONTRACT → 3.3 (APIContractFlow33)
3. SECURITY → 3.4 (SecurityAuditFlow34)
4. AUTHORIZATION → 3.4 (AuthorizationFlow34)
5. TEST → 5.1 (TestingFlow51)
6. DOCUMENT → 2.1 (DocumentationFlow21)
7. QA → 7.x (QAGatingFlow73)

DecisionCount: 7
UniqueSubPhases: 6 (4.3, 3.3, 3.4, 5.1, 2.1, 7.x — 3.4 counted once)
```

---

### Layer 0-C (Knowledge Edge)
```
All B17 rules active simultaneously:
- Rule 1: integrate absent (no v6-01 risk here — build verb present)
- Rule 4: connector → API_CONTRACT ✓
- Rule 6: token → SECURITY ✓
- Rule 7: OAuth → AUTHORIZATION ✓
- Rule 9: SECURITY+AUTHORIZATION → 3.4 (two decisions) ✓
- Rule 10: API_CONTRACT → 3.3 (not 4.3) ✓
- Rule 12: document → DOCUMENT ✓
- Rule 13: write tests → TEST ✓
- Rule 20: canonical ordering maintained ✓
- Rule 21: QA last ✓
- No GapHandler ✓
```

---

### Layer 0-D (Flow Factory)
```
Factory selection (7 flows):
1. IntegrationConnectorFlow43 (4.3) [probationary_accepted]
2. APIContractFlow33 (3.3) [deterministic_generic]
3. SecurityAuditFlow34 (3.4) [deterministic_generic]
4. AuthorizationFlow34 (3.4) [deterministic_generic]
5. TestingFlow51 (5.1) [deterministic_generic]
6. DocumentationFlow21 (2.1) [deterministic_generic]
7. QAGatingFlow73 (7.x) [deterministic_generic, position: last-absolute]

Only IntegrationConnectorFlow43 is probationary_accepted. All others are deterministic_generic.
```

---

### Layer 1 (Transition Orchestrator)
```
Orchestration plan:
Step 1 (parallel):
  ‖ IntegrationConnectorFlow43 (4.3) — build connector
  ‖ DocumentationFlow21 (2.1) — document interface (spec-driven, no code dependency)
Step 2 (parallel, after Step 1):
  ‖ APIContractFlow33 (3.3)
  ‖ SecurityAuditFlow34 (3.4)
  ‖ AuthorizationFlow34 (3.4)
Step 3 (after Step 1):
  TestingFlow51 (5.1) — tests require connector (depends on IntegrationConnectorFlow43)
Step 4 (absolute last):
  QAGatingFlow73 (7.x) — gates ALL completed flows

QAPosition: last-absolute (enforced)
Total orchestration steps: 4
```

---

### Layer 2 (Nine-Node 4.3 — EnterprisePaymentConnector)
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT", "QA"]
     DecisionCount: 7
N02: Enterprise payment connector — OAuth 2.0 PKCE, token refresh, charge/refund/webhook
N03: IEnterprisePaymentConnector (6 interface methods)
N06: Stripe charge/refund, OAuth token exchange, webhook HMAC validation
N08: PASS — all methods implemented, PKCE present, tests passing, docs complete
N09: EnterprisePaymentConnector complete
     Files generated: 10
     SecurityFlowRan: true, AuthorizationFlowRan: true, TestsIncluded: true, DocsIncluded: true
     QAVerdict: PASS
```

---

### Layer 3 (Teaching)
```
Key lessons from this trace (maximum pipeline):
1. All 7 B17 predicates fire simultaneously — this is the ceiling
2. DecisionCount 7 > UniqueSubPhases 6 (SECURITY+AUTHORIZATION share 3.4)
3. DocumentationFlow21 and IntegrationConnectorFlow43 run in parallel (spec-driven doc)
4. TestingFlow51 runs after IntegrationConnectorFlow43 (code dependency)
5. QAGatingFlow73 is always Step 4 (absolute last) — gates the entire pipeline
6. "QA" abbreviation fires QA; "quality assurance" written out does NOT
7. All 12 B17 rules are exercised in this single trace
8. probationary_accepted status only on IntegrationConnectorFlow43; others are deterministic_generic
```

---

**Full trace summary:**
Compiler → 7 predicates → Router → 7 decisions → Factory → 7 flows → Orchestrator → 4 steps (parallel where possible, sequential where dependent, QA last) → Nine-node: all flows run, N08 PASS → Teaching: maximum B17 pipeline, all rules active.

**This trace is the canonical reference for maximum B17 complexity. Every B17 rule is active simultaneously in this single work item.**
