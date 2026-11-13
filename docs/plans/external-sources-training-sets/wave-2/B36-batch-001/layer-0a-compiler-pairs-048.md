# Layer 0-A — Compiler Pairs (048)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## B36-0A-048-001 — "MoSCoW" + "now-next-later" mega gap

A:
  run_id: "B36-0A-048-001"
  user_request: "Use MoSCoW (Must/Should/Could/Won't) and now-next-later to organize the roadmap."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-moscow-001"
    - "gap-project-planning-now-next-later-001"
    - "gap-project-planning-product-roadmap-001"
  Teaching: "use" and "organize" are neutral. Three gap terms. DomainId: "project-management".
    SCHEDULE_CAPABILITY_LEARNING emitted with all three IDs bundled in one signal.

---

## B36-0A-048-002 — "hypothesis" + "experiment" + "test" fires TEST

A:
  run_id: "B36-0A-048-002"
  user_request: "Formulate the hypothesis, run the experiment, and test the output."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: ["TEST"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-hypothesis-001"
    - "gap-project-planning-experiment-001"
  Teaching: "test" fires TEST. "formulate", "run" are neutral. "hypothesis" and "experiment"
    are gap vocabulary. DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-048-003 — "RICE scoring" + "OKRs" + "KPIs" triple gap

A:
  run_id: "B36-0A-048-003"
  user_request: "Align RICE scoring (Reach, Impact, Confidence, Effort) with OKRs and KPIs for Q4."
  Task: "Identify predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-rice-scoring-001"
    - "gap-project-planning-okr-001"
    - "gap-project-planning-kpi-001"
  Teaching: "align" is neutral. RICE, OKRs, and KPIs are all gap vocabulary.
    DomainId: "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-0A-048-004 — "story splitting" + "define" fires DOCUMENT (not IMPLEMENT)

A:
  run_id: "B36-0A-048-004"
  user_request: "Define the split stories for the checkout epic."
  Task: "Identify predicates. 'define' and 'stories' tokens."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-story-splitting-001"
    - "gap-project-planning-epic-001"
  Teaching: "define" fires DOCUMENT. "stories" fires DOCUMENT (false positive).
    Same predicate, two triggers. No code entity present → IMPLEMENT does NOT fire.
    DomainId: "software-development". SCHEDULE_CAPABILITY_LEARNING NOT emitted.

---

## B36-0A-048-005 — complete DomainId reference for Round 3 vocabulary

A:
  run_id: "B36-0A-048-005"
  Task: "State the complete DomainId reference for all Round 3 (037–060) gap vocabulary."

B:
  DomainIdReference_Round3:
    Zero predicates fire → DomainId: "project-management" for ALL gap vocabulary, including:
      scrum ceremony, daily standup, sprint demo, backlog refinement, product increment,
      MVP, launch readiness, go-live checklist, feature flag (no code verb),
      OKRs, KPIs, north-star metric, impact mapping, t-shirt sizing, RICE scoring,
      MoSCoW, now-next-later, hypothesis, experiment (planning context)

    Any predicate fires → DomainId: "software-development" regardless of how many gap terms
      are present in the same sentence.

    Hard rule: "gap vocabulary proportion has zero effect on DomainId determination."
