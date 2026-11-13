# Layer 0-C — Knowledge Edge Pairs (021)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-021-001 — PartnerAPI: "integration" fires API_CONTRACT ONLY with "api" co-occurrence

```
source_id: xiigen-b12-0c-021-001
rule_id: B12-RULE-INTEGRATION-CO-OCCURRENCE
pair_kind: knowledge_edge
```

A:
  Case 1: "Define the partner API contract for third-party integration."
  Case 2: "Define the partner integration onboarding process."

B:
  Case 1: CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
    "API" → API_CONTRACT fires (API co-occurrence for "integration")
    "contract" → API_CONTRACT fires
    "integration" → API_CONTRACT fires (api co-occurrence present)
    Result: ["API_CONTRACT", "DOCUMENT"]

  Case 2: CompiledPredicateIds: ["DOCUMENT"]
    "define" → DOCUMENT fires
    "integration" — no API co-occurrence → API_CONTRACT does NOT fire
    No "api", "rest", "endpoint", "contract", etc.
    Result: ["DOCUMENT"] only

  Teaching: "integration" is conditional. It fires API_CONTRACT only when an API term
  is present in the same request. Core Rule 17: "integrate" requires co-occurrence.

---

## B12-0C-021-002 — PartnerAPI: "design" + "interface" dual-fire for external APIs

```
source_id: xiigen-b12-0c-021-002
rule_id: B12-RULE-INTERFACE-DUAL
pair_kind: knowledge_edge
```

A:
  CompiledText: "Design the partner integration interface and document the API contract for data exchange."

B:
  CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "design" → ARCHITECTURE fires
    "interface" → fires BOTH ARCHITECTURE (system design: "design" present) AND API_CONTRACT (api context: "API" present)
    "document" → DOCUMENT fires
    "API" → API_CONTRACT fires
    "contract" → API_CONTRACT fires
  Result: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"]
  Teaching: "interface" + "design" + "API" = classic dual-fire case.
  All three predicates fire. DecisionCount = 3 (3.1 + 3.3 + 2.1).

---

## B12-0C-021-003 — PartnerAPI: "publish" is NOT an IMPLEMENT verb

```
source_id: xiigen-b12-0c-021-003
rule_id: B12-RULE-PUBLISH-NOT-IMPLEMENT
pair_kind: knowledge_edge
```

A:
  CompiledText: "Publish the OpenAPI (Swagger) specification for the partner integration API."

B:
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  Predicate analysis:
    "OpenAPI" → API_CONTRACT fires
    "Swagger" → API_CONTRACT fires
    "specification" → DOCUMENT fires
    "API" → API_CONTRACT fires
    "publish" → NOT IMPLEMENT (publication/release verb, not code-creation verb)
  TransitionType: documentation (IMPLEMENT does NOT fire)
  Teaching: "publish" means to release or make available — it is a release-management verb.
  It does NOT create new code or specifications. Contrast:
    "write the OpenAPI spec" → IMPLEMENT fires (creating new artifact)
    "publish the OpenAPI spec" → IMPLEMENT does NOT fire (releasing existing artifact)
