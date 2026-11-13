# Layer 0-C — Knowledge Edge Pairs
# Batch: B34-batch-001
# Scenario: B34 — Skills and guidance gap
# Contract: KnowledgeEdgeHandlerUnit : ILogicalUnit<SubPhaseRouterResult, CapabilityGapSignal>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: knowledge_edge

---

## Pair B34B1-WDA-edge-001 — pure mentoring vocabulary: zero predicates

```
source_id: xiigen-b34-edge-001
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: knowledge_edge
```

A:
  user_request: "Coach the team on writing unit tests and mentor them through TDD practices."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  PhaseId: null
  SubPhaseId: null
  SourceStatus: "knowledge_edge"
  Confidence: 0.70

  CapabilityGapSignal:
    gap_id: "gap-skills-guidance-mentor-001"
    gap_vocabulary: ["coach", "mentor", "TDD practices"]
    DomainId: "software-development"
    SCHEDULE_CAPABILITY_LEARNING: true

  Predicate evidence:
    "coach" — not in any predicate lexicon
    "mentor" — not in any predicate lexicon
    "TDD" (Test-Driven Development) — not in any predicate lexicon ("test" would fire TEST,
      but "TDD" as an abbreviation does not trigger the TEST predicate)
    "writing unit tests" — "writing" alone might seem IMPLEMENT-adjacent, but "unit tests"
      without "write unit tests" as a direct action does not fire. The sentence structure
      is "coach on writing" (coaching about a practice) not "write unit tests" (direct command).
    Result: zero predicates → knowledge_edge

  Boundary note: "writing unit tests" in a mentoring/coaching context (coach the team ON
  writing) differs from "write unit tests for the service" (direct command). The orchestrator
  interprets the grammatical subject as the action — "coach" is the verb here, not "write".

---

## Pair B34B1-WDA-edge-002 — onboarding walkthrough: zero predicates

```
source_id: xiigen-b34-edge-002
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: knowledge_edge
```

A:
  user_request: "Walk the new engineer through the deployment pipeline and onboard them to the release process."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  PhaseId: null
  SubPhaseId: null
  SourceStatus: "knowledge_edge"
  Confidence: 0.70

  CapabilityGapSignal:
    gap_id: "gap-skills-guidance-onboard-001"
    gap_vocabulary: ["walk through", "onboard", "new engineer"]
    DomainId: "software-development"
    SCHEDULE_CAPABILITY_LEARNING: true

  Predicate evidence:
    "walk through" — not in any predicate lexicon (walkthrough ≠ DOCUMENT)
    "onboard" — not in any predicate lexicon
    "new engineer" — not in any predicate
    "deployment pipeline" — "deployment" not in predicate; "pipeline" alone not sufficient
    "release process" — "release" not in predicate (go-live/release is gap from B38)
    Result: zero predicates → knowledge_edge

---

## Pair B34B1-WDA-edge-003 — "explain" in tutorial context: zero predicates

```
source_id: xiigen-b34-edge-003
transition_type: unknown
phase: null
sub_phase: null
coverage_status: knowledge_edge
pair_kind: knowledge_edge
```

A:
  user_request: "Explain the authentication flow to the team and teach them the security model."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  PhaseId: null
  SubPhaseId: null
  SourceStatus: "knowledge_edge"
  Confidence: 0.70

  CapabilityGapSignal:
    gap_id: "gap-skills-guidance-mentor-001"
    gap_vocabulary: ["explain", "teach", "security model"]
    DomainId: "software-development"
    SCHEDULE_CAPABILITY_LEARNING: true

  Predicate evidence:
    "explain" — not in any predicate lexicon (explain ≠ DOCUMENT)
    "authentication flow" — "authentication" fires SECURITY? No: SECURITY requires
      action verbs like implement/enforce/configure/audit authentication. "Explain
      authentication" is guidance vocabulary. SECURITY fires on securing/implementing
      security measures, not explaining them.
    "teach" — not in any predicate lexicon
    "security model" — in a teaching context, SECURITY does not fire. The action is "explain",
      which has no predicate.
    Result: zero predicates → knowledge_edge

  Critical boundary: SECURITY fires when the action is implementing/enforcing/auditing
  security. When the action is explaining or teaching security concepts, SECURITY does
  not fire — "explain" is the verb and it has no predicate.
