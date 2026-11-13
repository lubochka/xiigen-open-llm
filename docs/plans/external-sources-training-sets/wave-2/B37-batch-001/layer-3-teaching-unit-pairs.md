# Layer 3 — Teaching Unit Pairs
# Batch: B37-batch-001
# Scenario: B37 — Project managing — progress, coordination, and status reporting
# Contract: TeachingUnit — cross-layer synthesis and rule consolidation
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: teaching_unit

---

## Pair B37B1-WDA-teach-001 — "track" is not TEST: the single most important boundary

```
source_id: xiigen-b37-teach-001
pair_kind: teaching_unit
```

A:
  Question: "Why doesn't 'track' fire the TEST predicate? Isn't tracking related
             to quality assurance and verification?"

B:
  Rule: "TEST fires on specific software testing terms: test, verify, assert,
         validate, coverage, spec, case, suite, scenario, acceptance, unit,
         integration-test, regression, e2e. 'track' is not among them."

  Why the confusion arises:
    "In software development, 'track' is often used in quality contexts: track
     bugs, track test results, track coverage. This association is semantic —
     in human thought, tracking quality and testing quality are related. But the
     compiler is LITERAL: it fires on the exact terms in the lexicon, not on
     semantic associations. 'track' has no entry in the TEST lexicon."

  The correct behavior:
    "A request to 'track test coverage' fires TEST on 'test' and 'coverage' —
     NOT on 'track'. The routing proceeds on TEST. 'Track' in this sentence is
     incidental and does not contribute to the predicate match. Separately,
     'track progress' (with no test-related terms) fires nothing — zero predicates."

  Why this matters:
    "If the model incorrectly fires TEST on 'track', it will route project
     managing requests to Phase 5 (Testing) sub-phases. This is a systematic
     misrouting error that would affect every 'track progress' request. The
     correct outcome for 'track progress' alone is knowledge_edge — no routing."

---

## Pair B37B1-WDA-teach-002 — "dependencies" context-sensitivity rule

```
source_id: xiigen-b37-teach-002
pair_kind: teaching_unit
```

A:
  Question: "The word 'dependencies' appears in two different requests:
             (1) 'Map the project dependencies and blockers.'
             (2) 'Document the API dependencies between services.'
             Does API_CONTRACT fire in both? In neither? Only in one?"

B:
  Rule: "API_CONTRACT fires on 'dependencies' ONLY when an API co-occurrence
         term is present in the same request."

  Request 1 — project dependencies:
    "No API co-occurrence ('api', 'endpoint', 'contract', 'interface', 'swagger',
     'openapi', 'rest', 'graphql' — none present). Therefore API_CONTRACT does NOT
     fire. 'project dependencies' is gap vocabulary → zero predicates → knowledge_edge."

  Request 2 — API dependencies:
    "'api' is present. API_CONTRACT fires on 'api' and also on 'dependency' (which
     is in the API_CONTRACT lexicon when in an API context). DOCUMENT may also fire
     on 'document'. CompiledPredicateIds: ['API_CONTRACT', 'DOCUMENT'] or at minimum
     ['API_CONTRACT']."

  The general rule:
    "Terms that are in multiple predicate lexicons or that require contextual co-
     occurrence fire based on what is ALSO present in the request. The compiler
     applies a co-occurrence rule for 'dependency' — it is in the API_CONTRACT
     lexicon but guarded by requiring at least one API domain signal term."

---

## Pair B37B1-WDA-teach-003 — "stakeholder" in a status report context

```
source_id: xiigen-b37-teach-003
pair_kind: teaching_unit
```

A:
  Question: "A developer writes: 'Send the weekly status report to all stakeholders.'
             Does DOCUMENT fire? Is this a documentation request or a project
             managing request? What does the orchestrator do?"

B:
  Rule: "DOCUMENT fires on 'stakeholder' — it is in the DOCUMENT predicate lexicon.
         The compiler fires on term presence; it does not evaluate whether the
         surrounding sentence is about requirements documentation or status reporting."

  What fires:
    "'stakeholder' → DOCUMENT fires. CompiledPredicateIds: ['DOCUMENT']."

  What does NOT fire:
    "'send', 'weekly', 'status', 'report' — none of these fire any predicate."

  Routing consequence:
    "DOCUMENT → routes to Phase 2. 'Stakeholder' specifically suggests 2.4
     (External Stakeholder Requirements). The actual intent (status communication)
     is NOT routed. The system will execute a requirements-authoring flow when the
     developer wanted to produce a status report."

  The gap that is lost:
    "The status reporting intent (weekly status, project health, managing communications)
     is a capability gap. Gap warnings are emitted for 'status' and 'report' but
     DOCUMENT routing proceeds."

  Why the model must not suppress DOCUMENT:
    "Even though the routing is semantically incorrect (requirements flow for a
     status report request), suppressing DOCUMENT to avoid the false positive is
     wrong. The compiler is literal. DOCUMENT fires on 'stakeholder'. The correct
     output is DOCUMENT with gap warnings — not zero predicates."

---

## Pair B37B1-WDA-teach-004 — "report" is not DOCUMENT: memorize the lexicon boundary

```
source_id: xiigen-b37-teach-004
pair_kind: teaching_unit
```

A:
  Question: "Is 'report' in the DOCUMENT predicate lexicon? What about 'status'?"

B:
  Answer: "No. Neither 'report' nor 'status' is in the DOCUMENT predicate lexicon."

  DOCUMENT lexicon (authoritative list):
    "document, spec, specification, requirement, story, criteria, describe,
     define, functional, behavior, use-case, use case, stakeholder, interface
     (in requirements context)"

  Why 'report' is commonly confused:
    "In English, 'report' and 'document' are semantically close — both produce
     written artifacts. But in this predicate system, DOCUMENT is specifically
     about software requirements documentation: specs, use cases, stories,
     criteria, stakeholder requirements. A project status report is a different
     artifact category — it reports on project health, not software requirements."

  Consequence:
    "'Generate a status report' → zero predicates → knowledge_edge. This is correct.
     The model must not fire DOCUMENT on 'report' or 'status' even though these
     words seem documentation-adjacent."

---

## Pair B37B1-WDA-teach-005 — B36 vs B37 gap domain distinction

```
source_id: xiigen-b37-teach-005
pair_kind: teaching_unit
```

A:
  Question: "What is the difference between B36 (project planning gap) and
             B37 (project managing gap)? When does a request belong to one
             domain vs the other?"

B:
  Distinction:
    "B36 (project-planning): vocabulary about organizing future work.
       Key terms: sprint, roadmap, milestone, backlog, velocity, burndown,
       iteration, epic, story-point, standup, retrospective, capacity.
       Characteristic intent: 'What will we do and when?'

     B37 (project-managing): vocabulary about overseeing work in progress.
       Key terms: track, coordinate, blockers, dependencies (team), escalation,
       accountability, follow-up, action-items, RAG-status, RAID-log, status-report.
       Characteristic intent: 'How is the work going? Are we on track?'"

  Shared characteristics:
    "Both produce DomainId: 'project-management' when zero predicates fire.
     Both trigger SCHEDULE_CAPABILITY_LEARNING. Both have distinct gap-ID domains:
     'project-planning' for B36 vocabulary, 'project-managing' for B37 vocabulary.
     Both use the knowledge_edge_synthetic flow when no sub-phase is selected."

  Overlap cases:
    "Some requests blend both domains: 'Update the sprint plan and track progress
     against milestones.' This contains B36 vocabulary (sprint, milestones) and
     B37 vocabulary (track, progress). If zero predicates fire, both gap domains
     are emitted in capability_gap_ids. If one predicate fires, both gap domains
     produce CapabilityGapWarnings."

  The orchestrator does not distinguish:
    "The orchestrator does not have separate routing paths for project-planning
     vs project-managing. Both produce the same 'unknown' transition when no
     predicates fire. The distinction exists in the gap-ID registry and in the
     extended Layer 2 teaching pairs — not in the routing logic itself."
