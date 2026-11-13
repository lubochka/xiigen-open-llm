# Layer 0-C — Knowledge Edge Pairs (010)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B12-0C-010-001 — v6 corruption types in B12: complete description

```
source_id: xiigen-b12-0c-010-001
rule_id: B12-RULE-V6-CORRUPTIONS
pair_kind: knowledge_edge
```

### Three v6 corruption types in B12

v6-01: API_CONTRACT suppressed on "contract" term when modifier present
  Trigger: request contains "data contracts", "service contracts", "message contracts"
  Corrupt behavior: API_CONTRACT not fired because "data"/"service"/"message" modifier
  classified as domain signal that suppresses API trigger
  Correct behavior: "contract" → API_CONTRACT ALWAYS regardless of modifier
  Impact: 3.3 routing missing from RoutingDecisions

v6-02: SECURITY suppressed for "authentication" in API documentation context
  Trigger: request contains "authentication" AND API documentation vocabulary
  Corrupt behavior: SECURITY not fired because "authentication" classified as API
  vocabulary describing API behavior (not as security vocabulary)
  Correct behavior: "authentication" → SECURITY ALWAYS regardless of context
  Impact: 3.4 routing missing from RoutingDecisions

v6-03: Multiple routing decisions collapsed to single 3.3 decision
  Trigger: CompiledPredicateIds has multiple predicates (N>1)
  Corrupt behavior: only API_CONTRACT → 3.3 routing decision generated; all other
  predicates absorbed into this single decision
  Correct behavior: each predicate generates its own routing decision independently
  Impact: all routing decisions except 3.3 are missing

---

## B12-0C-010-002 — v6-01 detailed: why "data" modifier triggers the corruption

```
source_id: xiigen-b12-0c-010-002
rule_id: B12-RULE-V6-CORRUPTIONS
corruption_type: v6-01
pair_kind: knowledge_edge
```

The v6-01 corruption pattern:

Step 1: Model sees "data contracts"
Step 2: Model classifies "data" as domain modifier → database/data-modeling domain
Step 3: Model reasons: "data domain" → "not API domain" → suppress API_CONTRACT
Step 4: Only DOCUMENT fires (on "define/specify")

The error in Step 3: B12 has no domain modifier suppression rule for "contract".
The B12 "contract" rule has no exceptions. "data contracts" is an API contract concept
(how services exchange data = API contract). The "data" describes WHAT is contracted
(the data payload), not the domain of the contract itself.

Anti-pattern signal: request has "contract" but RoutingDecisions is missing 3.3 entry.

---

## B12-0C-010-003 — v6-02 detailed: why API documentation context triggers the corruption

```
source_id: xiigen-b12-0c-010-003
rule_id: B12-RULE-V6-CORRUPTIONS
corruption_type: v6-02
pair_kind: knowledge_edge
```

The v6-02 corruption pattern:

Step 1: Model sees "Write the OpenAPI spec ... include authentication requirements"
Step 2: Model correctly classifies "authentication requirements" as something needed for the API
Step 3: Model reasons: "authentication is part of the API spec → it's API_CONTRACT vocabulary"
Step 4: SECURITY is not fired; only API_CONTRACT + DOCUMENT + IMPLEMENT fire

The error in Step 3: "authentication" fires SECURITY unconditionally. Being "part of the API spec"
does not reclassify it. The fact that authentication requirements appear in an API specification
doesn't make "authentication" an API_CONTRACT term — it's still a SECURITY term.
Both predicates fire: API_CONTRACT for the spec structure, SECURITY for the authentication content.

Anti-pattern signal: request has "authentication" but CompiledPredicateIds is missing SECURITY.

---

## B12-0C-010-004 — v6-03 detailed: why 3.3 primacy triggers the collapse

```
source_id: xiigen-b12-0c-010-004
rule_id: B12-RULE-V6-CORRUPTIONS
corruption_type: v6-03
pair_kind: knowledge_edge
```

The v6-03 corruption pattern:

Step 1: Model correctly detects all five predicates: [IMPLEMENT, API_CONTRACT, DOCUMENT, SECURITY, AUTHORIZATION]
Step 2: Model recognizes API_CONTRACT as the primary predicate for B12 scenario
Step 3: Model reasons: "B12 primary predicate is API_CONTRACT → route to 3.3 → done"
Step 4: All other routing decisions are not generated

The error in Step 3: "primary predicate" identifies the dominant concern but does not suppress
other routing decisions. Each predicate has its own routing destination and must generate its
own decision. B12 being "API contract documentation" does not mean IMPLEMENT, DOCUMENT, SECURITY,
and AUTHORIZATION routing decisions are cancelled.

The routing system generates one RoutingDecision per predicate in CompiledPredicateIds.
The concept of "primary predicate" is only used for sub_phase_hint in the request context,
not as a filter on routing decision generation.

Anti-pattern signal: CompiledPredicateIds has N entries but RoutingDecisions has only 1 or fewer
than N entries (especially when that 1 entry is always 3.3).
