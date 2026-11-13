# Layer 0-C — Knowledge-Edge Pairs (Extended)
# Batch: B32-batch-001
# Scenario: B32 — Vibe coding gap
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: knowledge_edge

---

## Pair B32B1-WDA-kedge-001 — pure vibe: KNOWLEDGE_EDGE

```
source_id: xiigen-b32-kedge-001
pair_kind: knowledge_edge
```

A:
  CompiledText: "Let's vibe code a prototype for the dashboard — just throw something together that feels right."
  CompiledPredicateIds: []
  KnownFailureMode: "no_predicate_match_capability_gap"

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics:
    capability_gap_id: "gap-vibe-coding-exploratory-001"
    capability_gap_description: "Request uses vibe coding vocabulary (vibe code, prototype, throw together, feels right) with no registered predicate. This describes a development style, not a development action."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  Warnings:
    - "knowledge_edge: capability_gap vocabulary detected: vibe, prototype, exploratory style"
    - "capability_gap_id: gap-vibe-coding-exploratory-001"

---

## Pair B32B1-WDA-kedge-002 — "prototype" alone: gap; "prototype" + "build": IMPLEMENT

```
source_id: xiigen-b32-kedge-002
pair_kind: knowledge_edge
```

A (Part 1): "Create a prototype for the checkout flow."
A (Part 2): "Vibe out a prototype for the checkout flow."

Part 1 predicate analysis:
  "create" → IMPLEMENT fires
  "prototype" — gap, but IMPLEMENT already fires via "create"
  Result: CompiledPredicateIds: ["IMPLEMENT"]

Part 2 predicate analysis:
  "vibe out" — gap vocabulary
  "prototype" — gap vocabulary
  No IMPLEMENT term present
  Result: CompiledPredicateIds: [] → KNOWLEDGE_EDGE

Teaching: "prototype" alone is gap vocabulary. It requires a co-occurring IMPLEMENT term
("create", "build", "implement", "write") to produce a predicate. "Vibe out a prototype"
with no action verb → knowledge_edge. "Create a prototype" → IMPLEMENT fires.

B (Part 1):
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings: ["gap-vibe-coding-style-001: 'prototype' style not captured"]

B (Part 2):
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  OverallVerdict: "KNOWLEDGE_EDGE"

---

## Pair B32B1-WDA-kedge-003 — "quick and dirty" + "build": IMPLEMENT fires; style is gap

```
source_id: xiigen-b32-kedge-003
pair_kind: knowledge_edge
```

A: "Build a quick and dirty solution for the email sender — nothing fancy, just get it working."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-vibe-coding-style-001: 'quick and dirty', 'nothing fancy', 'get it working' are vibe style vocabulary with no predicate"

Predicate analysis: "build" → IMPLEMENT; "solution" — not in predicate; "email sender" — not in predicate.
IMPLEMENT fires on "build". Vibe style vocabulary ("quick and dirty", "nothing fancy") is gap but
does not prevent routing. Teaching: quality/style qualifiers do not affect predicate compilation.
