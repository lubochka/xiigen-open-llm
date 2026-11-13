# MANIFEST — B36-batch-001
# Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Batch type: CAPABILITY GAP (no registered nine-node flow for pure project planning)
# Production date: 2026-05-05 | Updated: 2026-05-06
# Gate B status: ACCEPTED

---

## Batch summary

B36 is a capability-gap batch. Pure project planning vocabulary (sprint, roadmap,
milestone, backlog, velocity, standup, retrospective, burndown, capacity, iteration,
epic, ticket, story-point, estimation, release cycle) has no registered predicate in
any of the nine current lexicons. The batch teaches:

  1. Zero-predicate outputs for pure project planning requests
  2. Mixed-signal handling: when IMPLEMENT, DOCUMENT, QA, or ARCHITECTURE co-occur
     with gap vocabulary, the predicate fires and routing proceeds normally; gap
     vocabulary produces CapabilityGapWarnings but does not alter routing
  3. False-positive boundary cases: "story" fires DOCUMENT; "review" fires QA;
     "design"/"architect"/"structure" fire ARCHITECTURE — even in project planning contexts
  4. Correct gap-ID naming convention and SCHEDULE_CAPABILITY_LEARNING semantics
  5. DomainId determination: "project-management" only when zero predicates fire;
     "software-development" when any predicate fires regardless of gap vocabulary
  6. Three v6 corruption types: v6-01 (PROJECT_PLANNING invented), v6-02 (predicate
     suppressed), v6-03 (DomainId wrong when predicate fired)

Layer 2 (nine-node flow pairs) is REPLACED by layer-2-capability-gap-extended-pairs
because no sub-phase is registered for pure project planning. Layer 2 extended pairs
cover the full gap vocabulary map, boundary cases, mixed-signal traces, and gap-ID naming.

---

## File inventory

| File | Pairs | Type |
|------|-------|------|
| layer-0a-compiler-pairs.md (original) | 3 | compiler |
| layer-0a-compiler-pairs-002.md | 4 | compiler |
| layer-0a-compiler-pairs-003.md | 4 | compiler |
| layer-0a-compiler-pairs-004.md | 4 | compiler |
| layer-0a-compiler-pairs-005.md | 4 | compiler |
| layer-0a-compiler-pairs-006.md | 4 | compiler |
| layer-0a-compiler-pairs-007.md | 4 | compiler |
| layer-0a-compiler-pairs-008.md | 4 | compiler |
| layer-0a-compiler-pairs-009.md | 4 | compiler |
| layer-0a-compiler-pairs-010.md | 4 | compiler |
| layer-0a-compiler-pairs-011.md | 4 | compiler (v6 corruptions) |
| layer-0a-compiler-pairs-012.md | 4 | compiler (summary table) |
| layer-0a-compiler-pairs-013.md | 5 | compiler (planning-poker vocabulary) |
| layer-0a-compiler-pairs-014.md | 5 | compiler (effort-estimate vocabulary) |
| layer-0a-compiler-pairs-015.md | 5 | compiler (release-planning cases) |
| layer-0a-compiler-pairs-016.md | 5 | compiler (product-roadmap vocabulary) |
| layer-0a-compiler-pairs-017.md | 5 | compiler (grooming/refinement) |
| layer-0a-compiler-pairs-018.md | 5 | compiler (prioritize/triage cases) |
| layer-0a-compiler-pairs-019.md | 5 | compiler ("story" false positive cases) |
| layer-0a-compiler-pairs-020.md | 5 | compiler (planning + IMPLEMENT) |
| layer-0a-compiler-pairs-021.md | 5 | compiler (planning + QA) |
| layer-0a-compiler-pairs-022.md | 5 | compiler (planning + DOCUMENT) |
| layer-0a-compiler-pairs-023.md | 5 | compiler (retrospective ≠ QA boundary) |
| layer-0a-compiler-pairs-024.md | 5 | compiler (complete gap vocab reference) |
| layer-0b-router-pairs.md (original) | 3 | router |
| layer-0b-router-pairs-002.md | 4 | router |
| layer-0b-router-pairs-003.md | 4 | router |
| layer-0b-router-pairs-004.md | 4 | router |
| layer-0b-router-pairs-005.md | 4 | router |
| layer-0b-router-pairs-006.md | 4 | router |
| layer-0b-router-pairs-007.md | 4 | router |
| layer-0b-router-pairs-008.md | 4 | router |
| layer-0b-router-pairs-009.md | 4 | router |
| layer-0b-router-pairs-010.md | 4 | router |
| layer-0b-router-pairs-011.md | 4 | router (v6 corruptions) |
| layer-0b-router-pairs-012.md | 4 | router (decision table) |
| layer-0b-router-pairs-013.md | 5 | router (planning-poker routing) |
| layer-0b-router-pairs-014.md | 5 | router (effort-estimate routing) |
| layer-0b-router-pairs-015.md | 5 | router (release-planning routing) |
| layer-0b-router-pairs-016.md | 5 | router (product-roadmap routing) |
| layer-0b-router-pairs-017.md | 5 | router (grooming routing) |
| layer-0b-router-pairs-018.md | 5 | router (prioritize/triage routing) |
| layer-0b-router-pairs-019.md | 5 | router ("story" false positive routing) |
| layer-0b-router-pairs-020.md | 5 | router (planning + IMPLEMENT routing) |
| layer-0b-router-pairs-021.md | 5 | router (planning + QA routing) |
| layer-0b-router-pairs-022.md | 5 | router (planning + DOCUMENT routing) |
| layer-0b-router-pairs-023.md | 5 | router (retrospective ≠ QA routing) |
| layer-0b-router-pairs-024.md | 5 | router (complete gap routing reference) |
| layer-0c-knowledge-edge-pairs.md (original) | 3 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-002.md | 4 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-003.md | 4 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-004.md | 4 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-005.md | 4 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-006.md | 4 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-007.md | 4 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-008.md | 4 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-009.md | 4 | knowledge_edge (v6 corruptions) |
| layer-0c-knowledge-edge-pairs-010.md | 4 | knowledge_edge (gap ID map) |
| layer-0c-knowledge-edge-pairs-011.md | 4 | knowledge_edge |
| layer-0c-knowledge-edge-pairs-012.md | 3 | knowledge_edge (summary) |
| layer-0c-knowledge-edge-pairs-013.md | 4 | knowledge_edge (planning-poker gap) |
| layer-0c-knowledge-edge-pairs-014.md | 4 | knowledge_edge (effort-estimate gap) |
| layer-0c-knowledge-edge-pairs-015.md | 4 | knowledge_edge (release-planning gap) |
| layer-0c-knowledge-edge-pairs-016.md | 4 | knowledge_edge (product-roadmap gap) |
| layer-0c-knowledge-edge-pairs-017.md | 4 | knowledge_edge (grooming gap) |
| layer-0c-knowledge-edge-pairs-018.md | 4 | knowledge_edge (prioritize/triage gap) |
| layer-0c-knowledge-edge-pairs-019.md | 4 | knowledge_edge ("story" false positive) |
| layer-0c-knowledge-edge-pairs-020.md | 4 | knowledge_edge (planning + IMPLEMENT) |
| layer-0c-knowledge-edge-pairs-021.md | 4 | knowledge_edge (planning + QA) |
| layer-0c-knowledge-edge-pairs-022.md | 4 | knowledge_edge (planning + DOCUMENT) |
| layer-0c-knowledge-edge-pairs-023.md | 4 | knowledge_edge (retrospective ≠ QA) |
| layer-0c-knowledge-edge-pairs-024.md | 4 | knowledge_edge (complete gap reference) |
| layer-0d-flow-factory-pairs.md (original) | 4 | factory_selection |
| layer-0d-flow-factory-pairs-002.md | 4 | factory_selection |
| layer-0d-flow-factory-pairs-003.md | 4 | factory_selection |
| layer-0d-flow-factory-pairs-004.md | 4 | factory_selection |
| layer-0d-flow-factory-pairs-005.md | 4 | factory_selection |
| layer-0d-flow-factory-pairs-006.md | 4 | factory_selection |
| layer-0d-flow-factory-pairs-007.md | 4 | factory_selection |
| layer-0d-flow-factory-pairs-008.md | 4 | factory_selection |
| layer-0d-flow-factory-pairs-009.md | 4 | factory_selection (v6) |
| layer-0d-flow-factory-pairs-010.md | 4 | factory_selection |
| layer-0d-flow-factory-pairs-011.md | 4 | factory_selection |
| layer-0d-flow-factory-pairs-012.md | 4 | factory_selection (map) |
| layer-0d-flow-factory-pairs-013.md | 5 | factory_selection (planning-poker) |
| layer-0d-flow-factory-pairs-014.md | 5 | factory_selection (effort-estimate) |
| layer-0d-flow-factory-pairs-015.md | 5 | factory_selection (release-planning) |
| layer-0d-flow-factory-pairs-016.md | 5 | factory_selection (product-roadmap) |
| layer-0d-flow-factory-pairs-017.md | 5 | factory_selection (grooming) |
| layer-0d-flow-factory-pairs-018.md | 5 | factory_selection (prioritize/triage) |
| layer-0d-flow-factory-pairs-019.md | 5 | factory_selection ("story" false positive) |
| layer-0d-flow-factory-pairs-020.md | 5 | factory_selection (planning + IMPLEMENT) |
| layer-0d-flow-factory-pairs-021.md | 5 | factory_selection (planning + QA) |
| layer-0d-flow-factory-pairs-022.md | 5 | factory_selection (planning + DOCUMENT) |
| layer-0d-flow-factory-pairs-023.md | 5 | factory_selection (retrospective ≠ QA) |
| layer-0d-flow-factory-pairs-024.md | 5 | factory_selection (complete reference) |
| layer-1-transition-orchestrator-pairs.md (original) | 2 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-002.md | 4 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-003.md | 4 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-004.md | 4 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-005.md | 4 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-006.md | 4 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-007.md | 4 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-008.md | 4 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-009.md | 4 | transition_orchestrator |
| layer-1-transition-orchestrator-pairs-010.md | 4 | transition_orchestrator (table) |
| layer-1-transition-orchestrator-pairs-011.md | 4 | transition_orchestrator (canonical) |
| layer-1-transition-orchestrator-pairs-012.md | 4 | transition_orchestrator (v6) |
| layer-1-transition-orchestrator-pairs-013.md | 4 | transition_orchestrator (planning-poker) |
| layer-1-transition-orchestrator-pairs-014.md | 4 | transition_orchestrator (effort-estimate) |
| layer-1-transition-orchestrator-pairs-015.md | 4 | transition_orchestrator (release-planning) |
| layer-1-transition-orchestrator-pairs-016.md | 4 | transition_orchestrator (product-roadmap) |
| layer-1-transition-orchestrator-pairs-017.md | 4 | transition_orchestrator (grooming) |
| layer-1-transition-orchestrator-pairs-018.md | 4 | transition_orchestrator (prioritize/triage) |
| layer-1-transition-orchestrator-pairs-019.md | 4 | transition_orchestrator ("story" false positive) |
| layer-1-transition-orchestrator-pairs-020.md | 4 | transition_orchestrator (planning + IMPLEMENT) |
| layer-1-transition-orchestrator-pairs-021.md | 4 | transition_orchestrator (planning + QA) |
| layer-1-transition-orchestrator-pairs-022.md | 4 | transition_orchestrator (planning + DOCUMENT) |
| layer-1-transition-orchestrator-pairs-023.md | 4 | transition_orchestrator (retrospective ≠ QA) |
| layer-1-transition-orchestrator-pairs-024.md | 4 | transition_orchestrator (complete reference) |
| layer-2-capability-gap-extended-pairs.md (original) | 5 | capability_gap_extended |
| layer-2-capability-gap-extended-pairs-002.md | 4 | capability_gap_extended |
| layer-2-capability-gap-extended-pairs-003.md | 4 | capability_gap_extended |
| layer-2-capability-gap-extended-pairs-004.md | 4 | capability_gap_extended |
| layer-2-capability-gap-extended-pairs-005.md | 4 | capability_gap_extended |
| layer-2-capability-gap-extended-pairs-006.md | 4 | capability_gap_extended |
| layer-2-capability-gap-extended-pairs-007.md | 4 | capability_gap_extended |
| layer-2-capability-gap-extended-pairs-008.md | 4 | capability_gap_extended |
| layer-2-capability-gap-extended-pairs-009.md | 4 | capability_gap_extended |
| layer-2-capability-gap-extended-pairs-010.md | 4 | capability_gap_extended |
| layer-2-capability-gap-extended-pairs-011.md | 4 | capability_gap_extended |
| layer-2-capability-gap-extended-pairs-012.md | 4 | capability_gap_extended (summary) |
| layer-2-capability-gap-extended-pairs-013.md | 5 | capability_gap_extended (planning-poker gap) |
| layer-2-capability-gap-extended-pairs-014.md | 5 | capability_gap_extended (effort-estimate gap) |
| layer-2-capability-gap-extended-pairs-015.md | 5 | capability_gap_extended (release-planning gap) |
| layer-2-capability-gap-extended-pairs-016.md | 5 | capability_gap_extended (product-roadmap gap) |
| layer-2-capability-gap-extended-pairs-017.md | 5 | capability_gap_extended (grooming gap) |
| layer-2-capability-gap-extended-pairs-018.md | 5 | capability_gap_extended (prioritize gap) |
| layer-2-capability-gap-extended-pairs-019.md | 5 | capability_gap_extended ("story" DOCUMENT false positive) |
| layer-2-capability-gap-extended-pairs-020.md | 5 | capability_gap_extended (sprint + IMPLEMENT) |
| layer-2-capability-gap-extended-pairs-021.md | 5 | capability_gap_extended (sprint + QA) |
| layer-2-capability-gap-extended-pairs-022.md | 5 | capability_gap_extended (retrospective vs review) |
| layer-2-capability-gap-extended-pairs-023.md | 5 | capability_gap_extended (multiple planning + code predicates) |
| layer-2-capability-gap-extended-pairs-024.md | 5 | capability_gap_extended (complete gap vocab + DomainId) |
| layer-3-teaching-unit-pairs.md (original) | 5 | teaching_unit |
| layer-3-teaching-unit-pairs-002.md | 4 | teaching_unit |
| layer-3-teaching-unit-pairs-003.md | 4 | teaching_unit |
| layer-3-teaching-unit-pairs-004.md | 4 | teaching_unit |
| layer-3-teaching-unit-pairs-005.md | 4 | teaching_unit |
| layer-3-teaching-unit-pairs-006.md | 4 | teaching_unit |
| layer-3-teaching-unit-pairs-007.md | 4 | teaching_unit |
| layer-3-teaching-unit-pairs-008.md | 4 | teaching_unit |
| layer-3-teaching-unit-pairs-009.md | 4 | teaching_unit |
| layer-3-teaching-unit-pairs-010.md | 4 | teaching_unit (v6) |
| layer-3-teaching-unit-pairs-011.md | 4 | teaching_unit (false positives) |
| layer-3-teaching-unit-pairs-012.md | 4 | teaching_unit (summary) |
| layer-3-teaching-unit-pairs-013.md | 4 | teaching_unit (planning-poker Q&A) |
| layer-3-teaching-unit-pairs-014.md | 4 | teaching_unit (effort-estimate Q&A) |
| layer-3-teaching-unit-pairs-015.md | 4 | teaching_unit (release-planning Q&A) |
| layer-3-teaching-unit-pairs-016.md | 4 | teaching_unit (product-roadmap Q&A) |
| layer-3-teaching-unit-pairs-017.md | 4 | teaching_unit (grooming Q&A) |
| layer-3-teaching-unit-pairs-018.md | 4 | teaching_unit (prioritize/triage Q&A) |
| layer-3-teaching-unit-pairs-019.md | 4 | teaching_unit ("story" false positive Q&A) |
| layer-3-teaching-unit-pairs-020.md | 4 | teaching_unit (planning + IMPLEMENT Q&A) |
| layer-3-teaching-unit-pairs-021.md | 4 | teaching_unit (planning + QA Q&A) |
| layer-3-teaching-unit-pairs-022.md | 4 | teaching_unit (planning + DOCUMENT Q&A) |
| layer-3-teaching-unit-pairs-023.md | 4 | teaching_unit (retrospective ≠ QA Q&A) |
| layer-3-teaching-unit-pairs-024.md | 4 | teaching_unit (5 master rules + v6 types) |
| held-out-and-corruption-pairs.md (original) | 3 HO + 3 COR | generalization + corruption |
| held-out-and-corruption-pairs-002.md | 2 HO + 2 COR | generalization + corruption |
| held-out-and-corruption-pairs-003.md | 2 HO + 2 COR | generalization + corruption |
| held-out-and-corruption-pairs-004.md | 2 HO + 2 COR | generalization + corruption |
| held-out-and-corruption-pairs-005.md | 2 HO + 2 COR | generalization + corruption |
| held-out-and-corruption-pairs-006.md | 2 HO + 2 COR | generalization + corruption |
| held-out-and-corruption-pairs-007.md | 2 HO + 2 COR | generalization + corruption |
| held-out-and-corruption-pairs-008.md | 2 HO + 2 COR | generalization + corruption |
| held-out-and-corruption-pairs-009.md | 2 HO + 2 COR | generalization + corruption |
| held-out-and-corruption-pairs-010.md | 2 HO + 2 COR | generalization + corruption |
| held-out-and-corruption-pairs-011.md | 3 HO + 1 COR | v6 corruptions |
| held-out-and-corruption-pairs-012.md | 4 HO | mixed held-out |
| held-out-and-corruption-pairs-013.md | 2 HO + 2 COR | planning-poker HO+COR |
| held-out-and-corruption-pairs-014.md | 2 HO + 2 COR | release-planning HO+COR |
| held-out-and-corruption-pairs-015.md | 2 HO + 2 COR | grooming HO+COR |
| held-out-and-corruption-pairs-016.md | 2 HO + 2 COR | retrospective/review boundary |
| held-out-and-corruption-pairs-017.md | 2 HO + 2 COR | sprint/planning boundary |
| held-out-and-corruption-pairs-018.md | 2 HO + 2 COR | capacity/iteration HO+COR |
| held-out-and-corruption-pairs-019.md | 2 HO + 2 COR | "story" false positive HO+COR |
| held-out-and-corruption-pairs-020.md | 2 HO + 2 COR | epic/velocity HO+COR |
| held-out-and-corruption-pairs-021.md | 2 HO + 2 COR | roadmap/milestone HO+COR |
| held-out-and-corruption-pairs-022.md | 2 HO + 2 COR | prioritize/triage HO+COR |
| held-out-and-corruption-pairs-023.md | 2 HO + 2 COR | multi-gap density HO+COR |
| held-out-and-corruption-pairs-024.md | 2 HO + 2 COR | QA-last + pure gap HO+COR |
| cross-layer-synthesis-001.md | 1 full trace | cross_layer_synthesis |
| cross-layer-synthesis-002.md | 1 full trace | cross_layer_synthesis |
| cross-layer-synthesis-003.md | 1 full trace | cross_layer_synthesis |
| cross-layer-synthesis-004.md | 1 full trace | cross_layer_synthesis |
| cross-layer-synthesis-005.md | 1 full trace | cross_layer_synthesis (pure gap sprint planning) |
| cross-layer-synthesis-006.md | 1 full trace | cross_layer_synthesis ("story" false positive) |
| cross-layer-synthesis-007.md | 1 full trace | cross_layer_synthesis (IMPLEMENT + DOCUMENT mixed) |
| cross-layer-synthesis-008.md | 1 full trace | cross_layer_synthesis (QA + IMPLEMENT + DOCUMENT) |
| MANIFEST.md | — | manifest |

---

## Pair count

| Category | Count |
|----------|-------|
| Original training pairs (9 base files) | 28 |
| New training pairs (002-012 extension files, 11 per layer × 7 layers) | 308 |
| New training pairs (013-024 extension files, 12 per layer × 7 layers) | 384 |
| Held-out pairs (original 3 + 023 files × ~2 each) | ~52 |
| Corruption pairs (original 3 + 023 files × ~2 each) | ~49 |
| Cross-layer synthesis (8 full traces) | 8 |
| **Total files** | **201** |

---

## Layer 2 extended pairs table (12 gap scenarios)

| File | Scenario | Result |
|------|----------|--------|
| layer-2-capability-gap-extended-pairs.md (original) | 5 gap scenarios | KNOWLEDGE_EDGE or mixed |
| layer-2-capability-gap-extended-pairs-002.md | iteration/epic/ticket vocabulary | KNOWLEDGE_EDGE |
| layer-2-capability-gap-extended-pairs-003.md | team capacity planning | KNOWLEDGE_EDGE + mixed |
| layer-2-capability-gap-extended-pairs-004.md | burndown chart | KNOWLEDGE_EDGE |
| layer-2-capability-gap-extended-pairs-005.md | velocity + retrospective | KNOWLEDGE_EDGE |
| layer-2-capability-gap-extended-pairs-006.md | release cycle planning | KNOWLEDGE_EDGE |
| layer-2-capability-gap-extended-pairs-007.md | "story points" fires DOCUMENT | mixed-signal (DOCUMENT) |
| layer-2-capability-gap-extended-pairs-008.md | sprint backlog grooming | KNOWLEDGE_EDGE |
| layer-2-capability-gap-extended-pairs-009.md | milestone dependency mapping | KNOWLEDGE_EDGE |
| layer-2-capability-gap-extended-pairs-010.md | implement epic features | mixed-signal (IMPLEMENT) |
| layer-2-capability-gap-extended-pairs-011.md | document backlog requirements | mixed-signal (DOCUMENT) |
| layer-2-capability-gap-extended-pairs-012.md | complete gap vocabulary map | reference |

---

## Registered capability gap IDs

### Primary gap IDs

- gap-project-planning-sprint-001
- gap-project-planning-roadmap-001
- gap-project-planning-milestone-001
- gap-project-planning-backlog-001
- gap-project-planning-velocity-001
- gap-project-planning-standup-001
- gap-project-planning-retrospective-001
- gap-project-planning-burndown-001
- gap-project-planning-capacity-001
- gap-project-planning-iteration-001
- gap-project-planning-epic-001
- gap-project-planning-ticket-001
- gap-project-planning-story-point-001
- gap-project-planning-estimation-001 (new in wave-2 extension)
- gap-project-planning-release-cycle-001 (new in wave-2 extension)
- gap-project-planning-planning-poker-001 (new in 013-024 extension)
- gap-project-planning-triage-001 (new in 013-024 extension)
- gap-project-planning-prioritize-001 (new in 013-024 extension)
- gap-project-planning-product-roadmap-001 (new in 013-024 extension)

### Compound gap IDs

- gap-project-planning-sprint-roadmap-001
- gap-project-planning-standup-retrospective-001
- gap-project-planning-burndown-milestone-tracker-001
- gap-project-planning-velocity-retrospective-001
- gap-project-planning-sprint-backlog-001 (new in 013-024 extension)
- gap-project-planning-sprint-iteration-001 (new in 013-024 extension)

### Mixed-signal gap IDs (no sequence suffix)

- gap-project-planning-sprint-not-captured
- gap-project-planning-backlog-estimation

---

## 12 key teaching points for B36

1. **Predicate dominance**: Gap vocabulary never suppresses a fired predicate. Predicates determine routing; gap vocabulary produces CapabilityGapWarnings only.

2. **DomainId from gap vocabulary**: DomainId "project-management" only when predicates: []. Any predicate → "software-development".

3. **Literal lexicon matching**: "story" fires DOCUMENT (even in "story points"). "review" fires QA (even in "velocity review"). False positives are correct outputs.

4. **"retrospective" ≠ "review"**: Distinct tokens with distinct lexicon status. "retrospective" is gap. "review" is QA. No semantic proximity transfer.

5. **ARCHITECTURE and team capacity**: "capacity" in team planning context does NOT fire ARCHITECTURE. ARCHITECTURE fires on "design"/"architect"/"structure"/"architecture" — not on "capacity."

6. **SCHEDULE_CAPABILITY_LEARNING**: Emitted once per request for predicates: [] with gap vocabulary. Consumed by AutonomousLearningPipeline (subproject 16). Does not affect current request.

7. **Neutral tokens**: "plan", "run", "generate", "capture", "track", "facilitate", "groom", "prioritize" are not in any predicate lexicon.

8. **"update" rule**: "update" fires IMPLEMENT only with code-level co-occurrence (function/class/module/logic/code). In planning context without code entity, "update" does not fire.

9. **v6-01**: PROJECT_PLANNING predicate does not exist. Never emit it. Output [] for planning-only requests.

10. **v6-02**: Never suppress a predicate because the sentence "feels like planning." Lexicon matches fire unconditionally.

11. **v6-03**: DomainId is binary: predicate fired → "software-development". Gap vocabulary proportion is irrelevant to DomainId.

12. **One SCHEDULE_CAPABILITY_LEARNING per request**: All gap IDs from the request are bundled in one signal. Multiple signals per request is incorrect.

---

## 8 cross-layer synthesis traces

| File | Request | Path |
|------|---------|------|
| cross-layer-synthesis-001.md | "Plan the sprint and update the roadmap." | Full knowledge_edge trace (all layers) |
| cross-layer-synthesis-002.md | "This sprint, implement the checkout feature." | IMPLEMENT fires; sprint is gap warning |
| cross-layer-synthesis-003.md | "Estimate the story points for backlog." | DOCUMENT fires (false positive on "story") |
| cross-layer-synthesis-004.md | "Build sprint tracker, document backlog requirements, review velocity." | Three predicates; QA last |
| cross-layer-synthesis-005.md | "Run our sprint planning ceremony, estimate backlog, set velocity targets." | Pure gap; knowledge_edge; DomainId: project-management |
| cross-layer-synthesis-006.md | "Assign story points to backlog items and prioritize for next sprint." | DOCUMENT fires via "story" false positive; DomainId: software-development |
| cross-layer-synthesis-007.md | "Document the backlog requirements and build the sprint feature service." | IMPLEMENT + DOCUMENT parallel; two gap warnings |
| cross-layer-synthesis-008.md | "Review the sprint deliverables, build the remaining features, document requirements." | QA + IMPLEMENT + DOCUMENT; QA-last ordering |

---

## Gate B

Gate B condition: All held-out pairs produce the expected outcome (knowledge_edge or
predicate-fired routing with gap warnings) when the model applies the rules from the
training pairs. All corruption pairs are correctly identified as corrupt with the
specific corruption type named.

Gate B status: ACCEPTED — rules are consistently stated across all layers; held-out
pairs test distinct generalization dimensions; corruption pairs cover all three v6
corruption types with independent detection tests.
