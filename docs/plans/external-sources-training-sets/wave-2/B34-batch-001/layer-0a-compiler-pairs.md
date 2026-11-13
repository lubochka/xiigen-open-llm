# Layer 0-A — Compiler Pairs
# Batch: B34-batch-001
# Scenario: B34 — Skills and guidance — teaching, mentoring, onboarding, tutorial vocabulary
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: compiler

---

## Method note

B34 is a CAPABILITY GAP batch. Guidance/mentoring vocabulary has no registered predicate:
teach, guide, mentor, coach, train (human learning sense), onboard, explain (tutorial
sense), walkthrough, tutorial, learning path, skill-building. When mixed with development
predicates (DOCUMENT, IMPLEMENT), those predicates fire and routing proceeds. The
"teach"/"guide"/"mentor" framing alone produces CapabilityGapWarnings.

---

## Pair B34B1-WDA-compiler-001 — pure guidance vocabulary: zero predicates

```
source_id: xiigen-b34-compiler-001
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: compiler
```

A:
  user_request: "Mentor the junior developers on clean code practices and guide them through the codebase."

B:
  DocumentId: "B34-compiler-001-prompt"
  DocumentTitle: "Junior developer mentoring on clean code"
  CompiledText: "Mentor the junior developers on clean code practices and guide them through the codebase."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"

  Predicate evidence:
    "mentor" — not in any predicate lexicon
    "junior developers" — not in any predicate lexicon
    "clean code" — not in any predicate lexicon
    "guide" — not in any predicate lexicon (guide ≠ DOCUMENT which needs document/write-up/specification)
    "codebase" — not in any predicate lexicon (codebase alone is not an IMPLEMENT term)
    Result: zero predicates → knowledge_edge

  CapabilityGapSignal:
    gap_id: "gap-skills-guidance-mentor-001"
    DomainId: "software-development"
    SCHEDULE_CAPABILITY_LEARNING: true

---

## Pair B34B1-WDA-compiler-002 — "document" + guidance: DOCUMENT fires

```
source_id: xiigen-b34-compiler-002
transition_type: documentation
phase: 6
sub_phase: 6.2
coverage_status: mixed_signal
pair_kind: compiler
```

A:
  user_request: "Document the onboarding guide and write a tutorial walkthrough for new developers."

B:
  DocumentId: "B34-compiler-002-prompt"
  DocumentTitle: "Onboarding guide and tutorial documentation"
  CompiledText: "Document the onboarding guide and write a tutorial walkthrough for new developers."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-skills-guidance-onboard-001: 'onboarding guide', 'tutorial', 'walkthrough' are gap vocabulary"

  Predicate evidence:
    "document" → DOCUMENT (explicit action verb)
    "guide" — gap (guide as a noun here, not an action verb — no predicate)
    "write" → DOCUMENT (write + documentation target)
    "tutorial" — gap (tutorial ≠ DOCUMENT unless paired with write/document)
    "walkthrough" — gap
    "new developers" — not in any predicate
    Result: ["DOCUMENT"] via document + write

  Teaching: "guide" as a noun (onboarding guide) does not fire a predicate. "guide" as
  an action verb also has no predicate. Only "document" and "write" fire DOCUMENT here.

---

## Pair B34B1-WDA-compiler-003 — "implement" + training: IMPLEMENT fires; training is gap

```
source_id: xiigen-b34-compiler-003
transition_type: feature_addition
phase: 4
sub_phase: 4.1
coverage_status: mixed_signal
pair_kind: compiler
```

A:
  user_request: "Implement the developer training module and build the skill-tracking system."

B:
  DocumentId: "B34-compiler-003-prompt"
  DocumentTitle: "Developer training module and skill-tracking system"
  CompiledText: "Implement the developer training module and build the skill-tracking system."
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-skills-guidance-train-001: 'training module', 'skill-tracking' are gap vocabulary"

  Predicate evidence:
    "implement" → IMPLEMENT
    "module" → IMPLEMENT (module as a software unit)
    "build" → IMPLEMENT
    "system" → IMPLEMENT
    "developer training" — "training" is gap; "developer" alone is not a predicate
    "skill-tracking" — gap
    Result: ["IMPLEMENT"] via implement + module + build + system
