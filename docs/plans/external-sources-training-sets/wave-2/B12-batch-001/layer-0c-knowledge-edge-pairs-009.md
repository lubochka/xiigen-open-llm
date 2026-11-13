# Layer 0-C — Knowledge Edge Pairs (009)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-009-001 — Rule: API_CONTRACT alone → 3.3; DOCUMENT adds 2.x routing

```
source_id: xiigen-b12-0c-009-001
rule_id: B12-RULE-API-CONTRACT-ROUTING
pair_kind: knowledge_edge
```

### The Rule (Core Rule 13 elaborated)

API_CONTRACT alone → routes ONLY to 3.3 (documentation TransitionType, single decision)
API_CONTRACT + DOCUMENT → routes to 3.3 AND 2.x (two routing decisions)

When DOCUMENT fires alongside API_CONTRACT:
  - API_CONTRACT → 3.3 (APIContractFlow33)
  - DOCUMENT → 2.1 (DocumentationFlow21)
  Both run; FlowOrder: [APIContractFlow33, DocumentationFlow21]

The DOCUMENT predicate adds the Phase 2 (documentation) routing decision.
API_CONTRACT by itself does not route to Phase 2.

---

## B12-0C-009-002 — API_CONTRACT alone: when does it fire without DOCUMENT?

```
source_id: xiigen-b12-0c-009-002
rule_id: B12-RULE-API-CONTRACT-ROUTING
pair_kind: knowledge_edge
```

API_CONTRACT can theoretically fire alone (without DOCUMENT) when a request uses:
  - Only API vocabulary with no documentation verbs
  - Implicit documentation (API terms without explicit "define/specify/document/describe")

Example of API_CONTRACT firing without DOCUMENT:
  "Update the REST API endpoint for the cart service"
  → "REST API" → API_CONTRACT fires
  → "update" → not in DOCUMENT lexicon (documentation verbs are: document, define, specify, describe, annotate, write spec)
  → CompiledPredicateIds: ["API_CONTRACT"] only (if "update" does not trigger DOCUMENT)

However, in most B12 scenarios, documentation verbs co-occur with API vocabulary,
making [API_CONTRACT, DOCUMENT] the most common pattern.

The B12 scenario is API contract DOCUMENTATION — by definition, documentation verbs will
almost always appear alongside API vocabulary. In practice, pure [API_CONTRACT] without
DOCUMENT is rare in well-formed B12 requests.

---

## B12-0C-009-003 — DOCUMENT routes to Phase 2, API_CONTRACT routes to Phase 3.3

```
source_id: xiigen-b12-0c-009-003
rule_id: B12-RULE-API-CONTRACT-ROUTING
pair_kind: knowledge_edge
```

The phase mapping reminder:
  DOCUMENT → Phase 2 (documentation sub-phase)
    Phase 2.1 = DocumentationFlow21 (standard documentation entry)
    Phase 2.4 = DocumentationFlow24 (alternate documentation routing — less common)
  API_CONTRACT → Phase 3.3 (API contract design)
  ARCHITECTURE → Phase 3.1 (system architecture)
  IMPLEMENT → Phase 4.3 (implementation)
  SECURITY → Phase 3.4 (security)
  AUTHORIZATION → Phase 3.4 (authorization)

DOCUMENT is always a Phase 2 routing decision.
API_CONTRACT is always a Phase 3.3 routing decision.
They do not conflict — they add separate routing decisions to different phases.

When [API_CONTRACT, DOCUMENT] fires together:
  RoutingDecisions: [{3.3, APIContractFlow33}, {2.1, DocumentationFlow21}]
  This is TWO routing decisions to TWO different phases.

---

## B12-0C-009-004 — Complete routing decision table for API_CONTRACT-containing predicates

```
source_id: xiigen-b12-0c-009-004
rule_id: B12-RULE-API-CONTRACT-ROUTING
pair_kind: knowledge_edge
```

All routing decisions when API_CONTRACT is primary:

CompiledPredicateIds → Routing decisions → FlowOrder

[API_CONTRACT]:
  → [{3.3}]
  → [APIContractFlow33]
  DecisionCount: 1, TransitionType: documentation

[API_CONTRACT, DOCUMENT]:
  → [{3.3}, {2.1}]
  → [APIContractFlow33, DocumentationFlow21]
  DecisionCount: 2, TransitionType: documentation

[ARCHITECTURE, API_CONTRACT, DOCUMENT]:
  → [{3.1}, {3.3}, {2.1}]
  → [ArchitectureFlow31, APIContractFlow33, DocumentationFlow21]
  DecisionCount: 3, TransitionType: documentation

[API_CONTRACT, DOCUMENT, SECURITY]:
  → [{3.3}, {2.1}, {3.4}]
  → [APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34]
  DecisionCount: 3, TransitionType: documentation

[IMPLEMENT, API_CONTRACT, DOCUMENT]:
  → [{4.3}, {3.3}, {2.1}]
  → [ImplementationFlow43, APIContractFlow33, DocumentationFlow21]
  DecisionCount: 3, TransitionType: feature_addition

[IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY]:
  → [{4.3}, {3.3}, {2.1}, {3.4}]
  → [ImplementationFlow43, APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34]
  DecisionCount: 4, TransitionType: feature_addition

[IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION]:
  → [{4.3}, {3.3}, {2.1}, {3.4-SECURITY}, {3.4-AUTHORIZATION}]
  → [ImplementationFlow43, APIContractFlow33, DocumentationFlow21, SecurityAuditFlow34, AuthorizationFlow34]
  DecisionCount: 5, TransitionType: feature_addition
