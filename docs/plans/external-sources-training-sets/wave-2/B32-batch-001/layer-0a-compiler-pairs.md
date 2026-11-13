# Layer 0-A — Compiler Pairs
# Batch: B32-batch-001
# Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: compiler

---

## Method note

B32 is a CAPABILITY GAP batch. "Vibe coding" vocabulary has no registered predicate:
vibe, exploratory, prototype, scaffold (without IMPLEMENT co-occurrence), "just make
it work", "good enough", "throw something together", "quick and dirty", intuition-driven,
creative mode. The gap exists because these terms describe a development STYLE rather
than a development ACTION — and the predicate system fires on actions, not styles.

When vibe-coding requests contain development action terms (build, implement, write,
create), those predicates fire and routing proceeds normally. The "vibe" framing is
gap vocabulary that produces CapabilityGapWarnings.

---

## Pair B32B1-WDA-compiler-001 — pure vibe vocabulary: zero predicates

```
source_id: xiigen-b32-compiler-001
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: compiler
```

A:
  user_request: "Let's vibe code a prototype for the dashboard — just throw something together that feels right."

B:
  DocumentId: "B32-compiler-001-prompt"
  DocumentTitle: "Vibe coding dashboard prototype"
  CompiledText: "Let's vibe code a prototype for the dashboard — just throw something together that feels right."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

Predicate evidence:
  "vibe code" — not in any predicate lexicon
  "prototype" — not in IMPLEMENT (prototype is gap vocabulary in this system)
  "dashboard" — not in UI lexicon (dashboard ≠ screen, page, interface, form, component)
  "throw something together" — not in any predicate
  "feels right" — not in any predicate
  Result: CompiledPredicateIds: [], knowledge_edge
  Capability gap ID: gap-vibe-coding-exploratory-001

---

## Pair B32B1-WDA-compiler-002 — vibe with UI co-signal: UI fires

```
source_id: xiigen-b32-compiler-002
transition_type: feature_addition
phase: 1
sub_phase: 1.1
coverage_status: mixed_signal
pair_kind: compiler
```

A:
  user_request: "Vibe code a quick screen for the user profile — make the interface feel intuitive."

B:
  DocumentId: "B32-compiler-002-prompt"
  DocumentTitle: "Vibe-coded user profile screen with intuitive interface"
  CompiledText: "Vibe code a quick screen for the user profile — make the interface feel intuitive."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-vibe-coding-style-001: 'vibe code', 'intuitive', 'feel' are vibe vocabulary with no predicate"

Predicate evidence:
  "vibe code" — gap vocabulary
  "screen" → UI fires on "screen"
  "interface" → UI fires on "interface"
  "intuitive" — not in any predicate lexicon
  "feel" — not in any predicate lexicon
  Result: CompiledPredicateIds: ["UI"] via screen + interface

---

## Pair B32B1-WDA-compiler-003 — vibe with IMPLEMENT co-signal: IMPLEMENT fires

```
source_id: xiigen-b32-compiler-003
transition_type: feature_addition
phase: 4
sub_phase: 4.1
coverage_status: mixed_signal
pair_kind: compiler
```

A:
  user_request: "Exploratory prototype — just build something quick for the order summary, good enough to demo."

B:
  DocumentId: "B32-compiler-003-prompt"
  DocumentTitle: "Exploratory order summary prototype build"
  CompiledText: "Exploratory prototype — just build something quick for the order summary, good enough to demo."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-vibe-coding-exploratory-001: 'exploratory', 'prototype', 'good enough', 'demo' are vibe vocabulary"

Predicate evidence:
  "exploratory" — gap vocabulary
  "prototype" — gap vocabulary
  "build" → IMPLEMENT fires on "build"
  "something" — not in any predicate
  "good enough" — gap vocabulary
  "demo" — not in any predicate
  Result: CompiledPredicateIds: ["IMPLEMENT"] via "build"
