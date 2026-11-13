# Layer 2 — Capability Gap Extended Pairs (replaces nine-node layer)
# Batch: B37-batch-001
# Scenario: B37 — Project managing — progress, coordination, and status reporting
# Note: Layer 2 is replaced for gap batches. No nine-node flow executes for
#       pure project managing requests.
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: capability_gap_extended

---

## Pair B37B1-WDA-gap-001 — full project managing vocabulary map

```
source_id: xiigen-b37-gap-001
pair_kind: capability_gap_extended
```

A:
  Task: "Identify which project managing terms have no registered predicate
         and emit the correct capability gap ID for each."
  VocabularySet:
    - track (in project managing context)
    - coordinate
    - blockers
    - dependencies (without API co-occurrence)
    - stakeholder-update
    - escalation
    - accountability
    - follow-up
    - action-items
    - on-track / off-track
    - RAG-status
    - RAID-log
    - progress
    - status-report

B:
  GapVocabularyMap:
    - term: "track (project context)"
      predicate_match: null
      gap_id: "gap-project-managing-track-001"
      notes: "'track progress' is gap vocabulary. NOT the same as TEST which requires: test, verify, assert, validate, coverage."

    - term: "coordinate"
      predicate_match: null
      gap_id: "gap-project-managing-coordinate-001"
      notes: "Cross-team coordination; no predicate owns coordination vocabulary"

    - term: "blockers"
      predicate_match: null
      gap_id: "gap-project-managing-blockers-001"
      notes: "Project impediments; no predicate owns impediment vocabulary"

    - term: "dependencies (without API co-occurrence)"
      predicate_match: null
      gap_id: "gap-project-managing-dependencies-team-001"
      notes: "IMPORTANT BOUNDARY: 'dependencies' fires API_CONTRACT ONLY with API co-occurrence (api, endpoint, contract, interface, swagger, openapi, rest, graphql). Without those terms, 'dependencies' is gap vocabulary."

    - term: "stakeholder-update"
      predicate_match: "DOCUMENT (on 'stakeholder' if present as a single word)"
      gap_id: "gap-project-managing-stakeholder-update-001"
      notes: "BOUNDARY: 'stakeholder' fires DOCUMENT. 'stakeholder-update' as a compound may or may not fire depending on tokenization. The intent (status update to stakeholders) is project managing; the routing (DOCUMENT → 2.4) is requirements-focused."

    - term: "escalation"
      predicate_match: null
      gap_id: "gap-project-managing-escalation-001"
      notes: "Organizational escalation path; no predicate owns escalation vocabulary"

    - term: "accountability"
      predicate_match: null
      gap_id: "gap-project-managing-accountability-001"
      notes: "Organizational governance term; no predicate match"

    - term: "follow-up"
      predicate_match: null
      gap_id: "gap-project-managing-follow-up-001"
      notes: "Action tracking term; no predicate match"

    - term: "action-items"
      predicate_match: null
      gap_id: "gap-project-managing-action-items-001"
      notes: "Project ceremony output; 'action' alone not in any predicate lexicon"

    - term: "on-track / off-track"
      predicate_match: null
      gap_id: "gap-project-managing-track-status-001"
      notes: "RAG-adjacent status terms; no predicate match"

    - term: "RAG-status"
      predicate_match: null
      gap_id: "gap-project-managing-rag-status-001"
      notes: "Red/Amber/Green project health indicator; no predicate match"

    - term: "RAID-log"
      predicate_match: null
      gap_id: "gap-project-managing-raid-log-001"
      notes: "Risks/Assumptions/Issues/Dependencies log; no predicate match"

    - term: "progress"
      predicate_match: null
      gap_id: "gap-project-managing-progress-001"
      notes: "Project progress tracking; 'progress' not in any predicate lexicon"

    - term: "status-report"
      predicate_match: null
      gap_id: "gap-project-managing-status-report-001"
      notes: "'status' and 'report' alone do not fire DOCUMENT. 'report' is not a DOCUMENT term. DOCUMENT fires on: document, spec, define, describe, requirement, story, criteria, use-case, stakeholder, functional, behavior."

  TotalGapCount: 14
  TermsWithFalsePositivePredicate: ["stakeholder-update (via stakeholder→DOCUMENT)"]
  RecommendedAction: "SCHEDULE_CAPABILITY_LEARNING for all 14 gap IDs"

---

## Pair B37B1-WDA-gap-002 — "track" vs TEST: the critical boundary

```
source_id: xiigen-b37-gap-002
pair_kind: capability_gap_extended
```

A:
  Question: "Does 'track' fire the TEST predicate? A developer writes:
             'Track all the bugs and monitor test coverage.' Does TEST fire?"

B:
  Analysis:
    - term: "track"
      fires_TEST: false
      reason: "'track' is NOT in the TEST predicate lexicon. TEST fires on:
               test, verify, assert, validate, coverage, spec, case, suite,
               scenario, acceptance, unit, integration-test, regression, e2e."

    - term: "bugs"
      fires_TEST: false
      reason: "'bugs' is not in the TEST lexicon (QA lexicon may own bug-related
               terms, but 'bug' alone does not fire QA or TEST in this context)"

    - term: "monitor"
      fires_TEST: false
      reason: "'monitor' is not in the TEST lexicon"

    - term: "test coverage"
      fires_TEST: true
      reason: "'test' fires TEST; 'coverage' fires TEST. Both are in the TEST lexicon."

  CompiledPredicateIds: ["TEST"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-managing-track-001: 'track all the bugs' is project managing vocabulary; TEST fires only on 'test coverage'"

  Teaching point: "In 'Track all the bugs and monitor test coverage', TEST fires
  on 'test' and 'coverage' — NOT on 'track'. The model must not over-generalize
  and fire TEST on 'track' just because the sentence is about quality or bug tracking."

---

## Pair B37B1-WDA-gap-003 — status report with "report": DOCUMENT does NOT fire on "report"

```
source_id: xiigen-b37-gap-003
pair_kind: capability_gap_extended
```

A:
  CompiledText: "Generate a weekly status report and send it to the project sponsor."

B:
  PredicateAnalysis:
    - term: "generate"
      fires: null
      note: "'generate' is not in any predicate lexicon"

    - term: "weekly"
      fires: null
      note: "Time qualifier; no predicate"

    - term: "status"
      fires: null
      note: "'status' is NOT in DOCUMENT lexicon"

    - term: "report"
      fires: null
      note: "IMPORTANT: 'report' is NOT in the DOCUMENT predicate lexicon.
             DOCUMENT fires on: document, spec, specification, requirement,
             story, criteria, describe, define, functional, behavior, use-case,
             stakeholder. 'report' is a common false assumption — it does NOT fire."

    - term: "send"
      fires: null
      note: "'send' is not in any predicate lexicon"

    - term: "project sponsor"
      fires: null
      note: "'project' and 'sponsor' are not in any predicate lexicon.
             Note: 'stakeholder' fires DOCUMENT but 'project sponsor' does not."

  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "no_predicate_match_capability_gap"
  capability_gap_id: "gap-project-managing-status-report-001"

  Teaching point: "A status report request produces zero predicates. Despite
  containing 'report', DOCUMENT does not fire. The model must memorize the exact
  DOCUMENT lexicon and not assume 'report' belongs to it."

---

## Pair B37B1-WDA-gap-004 — gap-ID naming convention for project managing domain

```
source_id: xiigen-b37-gap-004
pair_kind: capability_gap_extended
```

A:
  Task: "State the gap-ID naming convention for project managing vocabulary."

B:
  Convention: "gap-{domain}-{term(s)}-{sequence}"
  Domain: "project-managing"
  Examples:
    - "gap-project-managing-track-001"
    - "gap-project-managing-coordinate-001"
    - "gap-project-managing-coordinate-blockers-001" (compound when multiple terms co-occur)
    - "gap-project-managing-escalation-accountability-001"
    - "gap-project-managing-dependencies-team-001" (qualified to distinguish from API dependencies)
  Distinction from B36:
    "B36 uses domain 'project-planning' (sprint, roadmap, milestone).
     B37 uses domain 'project-managing' (track, coordinate, blockers, escalation).
     The two domains are distinct gap registries. Do not mix gap-project-planning
     and gap-project-managing IDs."
  Note: The DomainId in the orchestrator transition is 'project-management' for both
     B36 and B37 (the shared project management domain). But the gap-ID domain segment
     is more specific: 'project-planning' vs 'project-managing' to distinguish the
     two gap vocabularies.
