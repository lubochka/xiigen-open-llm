# Held-Out and Corruption Pairs
# Batch: B37-batch-001
# Scenario: B37 — Project managing — progress, coordination, and status reporting
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05

---

## Held-Out Pairs

---

### Held-Out B37B1-HO-001 — RAG status request: pure gap

```
source_id: xiigen-b37-ho-001
held_out_reason: "Tests generalization to RAG-status vocabulary not in training pairs"
expected_outcome: knowledge_edge
```

A:
  user_request: "Update the project RAG status to Amber and log the new risks in the RAID."
  Task: "Compile predicates and determine routing."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"
  OverallVerdict: "KNOWLEDGE_EDGE"
  capability_gap_ids:
    - "gap-project-managing-rag-status-001"
    - "gap-project-managing-raid-log-001"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"

Predicate analysis:
  "update" — not in IMPLEMENT lexicon (no code-level co-occurrence)
  "project" — not in any predicate lexicon
  "RAG status" — not in any predicate lexicon ("RAG" is Red/Amber/Green)
  "Amber" — not in any predicate lexicon
  "log" — not in any predicate lexicon (log ≠ TEST which requires test/verify/assert)
  "risks" — not in SECURITY lexicon (SECURITY fires on: security, authentication,
    authorization, encrypt, vulnerability, threat, injection, token, permission,
    access-control. "risks" alone does not fire SECURITY.)
  "RAID" — not in any predicate lexicon
  Result: zero predicates → KNOWLEDGE_EDGE

---

### Held-Out B37B1-HO-002 — accountability with ARCHITECTURE boundary

```
source_id: xiigen-b37-ho-002
held_out_reason: "Tests that 'structure' in 'accountability structure' does not fire ARCHITECTURE"
expected_outcome: knowledge_edge (ARCHITECTURE does NOT fire on 'accountability structure')
```

A:
  user_request: "Define the accountability structure for the delivery team and assign owners to each work stream."
  Task: "Compile predicates and determine routing."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  CapabilityGapWarnings:
    - "gap-project-managing-accountability-001: 'accountability structure' not captured"
    - "gap-project-managing-work-stream-001: 'work stream' and 'owners' not captured"

Predicate analysis:
  "define" → DOCUMENT fires on "define"
  "accountability" — not in any predicate lexicon
  "structure" — IS in ARCHITECTURE lexicon → ARCHITECTURE fires!

  Correction: "structure" fires ARCHITECTURE. "define" fires DOCUMENT.
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]

  Teaching point: "structure" in "accountability structure" fires ARCHITECTURE
  even though the intent is organizational governance, not system architecture.
  The compiler is literal. ARCHITECTURE fires on "structure" regardless of whether
  the structure being described is a software component or an org chart.

Corrected B:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  SourceStatus: "ready"
  KnownFailureMode: null
  CapabilityGapWarnings:
    - "gap-project-managing-accountability-001: organizational accountability intent not captured; ARCHITECTURE fires on 'structure'"
    - "gap-project-managing-work-stream-001: 'work stream' and 'assign owners' not captured"

Routing consequence:
  DOCUMENT → 2.4 (stakeholder requirements, 'define' + potential stakeholder signal)
  ARCHITECTURE → Phase 3 sub-phases
  Two routing decisions. The actual intent (org structure for accountability) is
  not served by either routing — both are false positives in intent terms.

---

### Held-Out B37B1-HO-003 — "on-track" and "follow-up": pure gap

```
source_id: xiigen-b37-ho-003
held_out_reason: "Tests generalization to on-track/follow-up vocabulary"
expected_outcome: knowledge_edge
```

A:
  user_request: "Confirm which workstreams are on-track and schedule follow-ups for any that are at risk."
  Task: "Compile predicates and determine routing."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"
  OverallVerdict: "KNOWLEDGE_EDGE"
  capability_gap_ids:
    - "gap-project-managing-track-status-001"
    - "gap-project-managing-follow-up-001"
  recommended_action: "SCHEDULE_CAPABILITY_LEARNING"

Predicate analysis:
  "confirm" — not in any predicate lexicon
  "workstreams" — not in any predicate lexicon
  "on-track" — not in any predicate lexicon ("track" is not TEST)
  "schedule" — not in any predicate lexicon
  "follow-ups" — not in any predicate lexicon
  "at risk" — "risk" not in SECURITY lexicon in this context
  Result: zero predicates → KNOWLEDGE_EDGE

---

## Corruption Pairs

---

### Corruption B37B1-COR-001 — v6-01: TEST fired on "track" (incorrect)

```
source_id: xiigen-b37-cor-001
corruption_type: "Corruption-v6-01: knowledge edge suppression — TEST falsely fired on 'track'"
```

A:
  CompiledText: "Track progress across all active projects and provide a status report to stakeholders by Friday."
  CompiledPredicateIds: ["DOCUMENT"]  (correct: stakeholder fires DOCUMENT)
  SourceStatus: "ready"

CORRUPT B (do not reproduce):
  CompiledPredicateIds: ["DOCUMENT", "TEST"]
  SourceStatus: "ready"

Why this is corrupt:
  "The corrupt output adds TEST to the compiled predicates because the request
   is about 'tracking' — a concept the model associates with monitoring and
   verification. But 'track' is NOT in the TEST predicate lexicon. TEST fires on:
   test, verify, assert, validate, coverage, spec, case, suite, scenario, acceptance.
   'track' and 'progress' are gap vocabulary. The corrupt model over-associated
   'track progress' with quality monitoring and incorrectly fired TEST."

Correct B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null
  CapabilityGapWarnings:
    - "gap-project-managing-track-progress-001: 'track progress' not captured; DOCUMENT fires on 'stakeholder' only"
    - "gap-project-managing-status-report-001: 'status report' intent not captured by DOCUMENT routing"

---

### Corruption B37B1-COR-002 — v6-01: API_CONTRACT fired on "dependencies" without API co-occurrence

```
source_id: xiigen-b37-cor-002
corruption_type: "Corruption-v6-01: knowledge edge suppression — API_CONTRACT falsely fired on project dependencies"
```

A:
  CompiledText: "Coordinate the cross-team dependencies and resolve the blockers preventing the release."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"

CORRUPT B (do not reproduce):
  CompiledPredicateIds: ["API_CONTRACT"]
  SourceStatus: "ready"
  RoutingDecisions:
    - PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false

Why this is corrupt:
  "The corrupt output fires API_CONTRACT on 'dependencies' — but 'dependencies' in
   'cross-team dependencies' is NOT an API contract term. API_CONTRACT requires an
   API domain co-occurrence: api, endpoint, contract, interface, swagger, openapi,
   rest, graphql. None of those are present. The corrupt model over-generalized from
   the rule that 'dependencies' fires API_CONTRACT when in an API context, and
   incorrectly applied it to a project management sentence about team relationships."

Correct B:
  RoutingDecisions:
    - DocumentId: "B37-compiler-002-prompt"
      SourceStatus: "knowledge_edge: no routing predicate matched any registered sub-phase"
      PredictedPhaseId: null
      PredictedSubPhaseId: null
      Confidence: 0.70
      NearestWrongSubPhaseScore: 0.0
      EvidencePredicates: []
      KnowledgeEdge: true
      FailureClassification: "no_predicate_match_capability_gap"
  DecisionCount: 1

---

### Corruption B37B1-COR-003 — v6-03: DomainId "project-management" when IMPLEMENT fires

```
source_id: xiigen-b37-cor-003
corruption_type: "Corruption-v6-03: domain signal collapse — project-managing gap vocabulary bleeds into DomainId"
```

A:
  TransitionId: "transition-B37-t1-002-4.1-feature_addition"
  TransitionType: "feature_addition"
  CompiledPredicateIds: ["IMPLEMENT"]
  PredictedSubPhaseId: "4.1"
  UserRequest: "Manage the delivery of the authentication module — build the service, track progress, and escalate blockers."

CORRUPT B (do not reproduce):
  TransitionType: "feature_addition"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "project-management"
  ContextCarryForward: true

Why this is corrupt:
  "The corrupt output sets DomainId to 'project-management' because the request
   contains project managing vocabulary ('manage the delivery', 'track progress',
   'escalate blockers'). This is the same corruption type as B36-COR-003, applied
   to B37 managing vocabulary instead of B36 planning vocabulary. DomainId
   'project-management' is ONLY valid when zero predicates fire. When IMPLEMENT
   fires, DomainId must be 'software-development'. The managing vocabulary produces
   CapabilityGapWarnings — it does not change the DomainId."

Correct B:
  TransitionId: "transition-B37-t1-002-4.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "software-development"
  PredicateIds: ["IMPLEMENT"]
  ContextCarryForward: true
  Metadata:
    transition_classification_basis: "IMPLEMENT predicate (build, service, module) classifies as feature_addition; 'manage', 'track progress', 'escalate blockers' are capability gap vocabulary and do not alter the classification"
    domain_signal: "backend_implementation"
    capability_gap_warnings:
      - "gap-project-managing-track-progress-001"
      - "gap-project-managing-escalation-001"
      - "gap-project-managing-manage-delivery-001"
