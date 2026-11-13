# MANIFEST — B37-batch-001
# Scenario: B37 — Project managing — progress, coordination, and status reporting
# Batch type: CAPABILITY GAP (no registered nine-node flow for pure project managing)
# Production date: 2026-05-05
# Gate B status: ACCEPTED

---

## Batch summary

B37 is a capability-gap batch. Pure project managing vocabulary (track, coordinate,
blockers, dependencies [without API co-occurrence], stakeholder-update, escalation,
accountability, follow-up, action-items, on-track/off-track, RAG-status, RAID-log,
progress, status-report, manage, delivery-status, workstream) has no registered predicate
in any of the current predicate lexicons. The batch teaches:

  1. Zero-predicate outputs for pure project managing requests
  2. Critical boundaries: "track" ≠ TEST; "report" ≠ DOCUMENT; "status" ≠ DOCUMENT
  3. Context-sensitive predicate: "dependencies" fires API_CONTRACT ONLY with API
     co-occurrence (api, contract, endpoint, interface, integration, swagger, openapi,
     graphql, rest, grpc, webhook); without it, "dependencies" is gap vocabulary
  4. False-positive cases: "stakeholder" fires DOCUMENT; "structure" fires ARCHITECTURE;
     "define" fires DOCUMENT; "review" fires QA; "test" fires TEST — even in managing sentences
  5. Gap-ID domain distinction: "project-managing" (B37) vs "project-planning" (B36)
  6. DomainId rule reinforcement: "project-management" only when zero predicates fire;
     "software-development" when ANY predicate fires
  7. v6 corruption taxonomy: v6-01 (invent predicate), v6-02 (suppress DOCUMENT),
     v6-03 (wrong DomainId)
  8. Layer 2 type: capability-gap-extended-pairs (not nine-node pairs)
  9. SCHEDULE_CAPABILITY_LEARNING: one per zero-predicate request; not emitted when
     any predicate fires

Layer 2 (nine-node flow pairs) is REPLACED by layer-2-capability-gap-extended-pairs (NNN).

---

## File inventory: complete batch (201 files total including MANIFEST)

| File pattern | Files | Pairs per file | Type |
|-------------|-------|---------------|------|
| layer-0a-compiler-pairs-NNN.md (002-024) | 23 | 4-5 | compiler |
| layer-0b-router-pairs-NNN.md (002-024) | 23 | 4-5 | router |
| layer-0c-knowledge-edge-pairs-NNN.md (002-024) | 23 | 4-5 | knowledge_edge |
| layer-0d-flow-factory-pairs-NNN.md (002-024) | 23 | 4-5 | factory_selection |
| layer-1-transition-orchestrator-pairs-NNN.md (002-024) | 23 | 4-5 | transition_orchestrator |
| layer-2-capability-gap-extended-pairs-NNN.md (002-024) | 23 | 4-5 | capability_gap_extended |
| layer-3-teaching-unit-pairs-NNN.md (002-024) | 23 | 4-5 | teaching_unit |
| held-out-and-corruption-pairs-NNN.md (002-024) | 23 | 4-5 | held-out + corruption |
| cross-layer-synthesis-NNN.md (001-008) | 8 | 3-5 | cross_layer_synthesis |
| MANIFEST.md | 1 | — | manifest |

---

## Pair count

| Layer | Files | Approx pairs |
|-------|-------|-------------|
| Layer 0-A (compiler) | 23 | ~105 |
| Layer 0-B (router) | 23 | ~105 |
| Layer 0-C (knowledge edge) | 23 | ~105 |
| Layer 0-D (flow factory) | 23 | ~105 |
| Layer 1 (transition orchestrator) | 23 | ~105 |
| Layer 2 (capability gap extended) | 23 | ~105 |
| Layer 3 (teaching unit) | 23 | ~105 |
| Held-out and corruption | 23 | ~105 |
| Cross-layer synthesis | 8 | ~38 |
| **Total files (excl. MANIFEST)** | **192** | **~878 pairs** |

---

## Layer 2: capability-gap-extended-pairs file map

| File | Topic | Gap IDs featured |
|------|-------|-----------------|
| layer-2-capability-gap-extended-pairs-002.md | "track progress" vocabulary | track-progress-001, track-001, manage-delivery-001 |
| layer-2-capability-gap-extended-pairs-003.md | "coordinate blockers" | coordinate-blockers-001, coordinate-001, blockers-001 |
| layer-2-capability-gap-extended-pairs-004.md | RAG-status reporting | rag-status-001, status-report-001 |
| layer-2-capability-gap-extended-pairs-005.md | RAID log | raid-log-001, dependencies-team-001 |
| layer-2-capability-gap-extended-pairs-006.md | "stakeholder" DOCUMENT false positive | stakeholder-update-001 |
| layer-2-capability-gap-extended-pairs-007.md | "dependencies" context rule | dependencies-team-001 vs API_CONTRACT |
| layer-2-capability-gap-extended-pairs-008.md | Escalation + accountability | escalation-accountability-001, escalation-001, accountability-001 |
| layer-2-capability-gap-extended-pairs-009.md | Follow-up + action items | follow-up-001, action-items-001 |
| layer-2-capability-gap-extended-pairs-010.md | "manage delivery" with build | manage-delivery-001 (mixed-signal) |
| layer-2-capability-gap-extended-pairs-011.md | "define governance structure" | accountability-001, escalation-001 |
| layer-2-capability-gap-extended-pairs-012.md | Complete gap vocabulary map | all gap IDs |
| layer-2-capability-gap-extended-pairs-013.md | "maintain the risk log" gap | raid-log-001 |
| layer-2-capability-gap-extended-pairs-014.md | "process the change request" gap | manage-delivery-001, coordinate-001 |
| layer-2-capability-gap-extended-pairs-015.md | "create the project charter" gap | manage-delivery-001 |
| layer-2-capability-gap-extended-pairs-016.md | "update the decision log" gap | manage-delivery-001, accountability-001 |
| layer-2-capability-gap-extended-pairs-017.md | "run lessons-learned session" gap | manage-delivery-001 |
| layer-2-capability-gap-extended-pairs-018.md | "define accountability structure" → ARCHITECTURE fires | accountability-001 |
| layer-2-capability-gap-extended-pairs-019.md | "send stakeholder update" → DOCUMENT fires | stakeholder-update-001 |
| layer-2-capability-gap-extended-pairs-020.md | "API dependencies between services" → API_CONTRACT fires | dependencies-team-001 |
| layer-2-capability-gap-extended-pairs-021.md | "team dependencies blocking progress" gap | dependencies-team-001 |
| layer-2-capability-gap-extended-pairs-022.md | "track and document blockers for stakeholders" → DOCUMENT fires | blockers-001, stakeholder-update-001 |
| layer-2-capability-gap-extended-pairs-023.md | Multiple managing terms + code predicate disambiguation | escalation-001, accountability-001, raid-log-001 |
| layer-2-capability-gap-extended-pairs-024.md | Complete B37 gap vocabulary + boundary rule summary | all gap IDs |

---

## Capability gap IDs registered in this batch

### Primary gap IDs

| Gap ID | Vocabulary |
|--------|-----------|
| gap-project-managing-track-001 | track (standalone) |
| gap-project-managing-coordinate-001 | coordinate |
| gap-project-managing-blockers-001 | blockers |
| gap-project-managing-dependencies-team-001 | team/project dependencies (no API co-occurrence) |
| gap-project-managing-stakeholder-update-001 | stakeholder update intent (DOCUMENT false positive) |
| gap-project-managing-escalation-001 | escalation, escalate |
| gap-project-managing-accountability-001 | accountability, ownership, RACI |
| gap-project-managing-follow-up-001 | follow-up, follow up |
| gap-project-managing-action-items-001 | action items |
| gap-project-managing-track-status-001 | on-track, off-track, track status |
| gap-project-managing-rag-status-001 | RAG-status, RAG report |
| gap-project-managing-raid-log-001 | RAID log, RAID, R+A+I+D vocabulary |
| gap-project-managing-progress-001 | progress, progress update |
| gap-project-managing-status-report-001 | status report, weekly report |
| gap-project-managing-manage-delivery-001 | manage delivery, delivery status (mixed-signal) |
| gap-project-managing-work-stream-001 | workstream, work stream |

### Compound gap IDs

| Gap ID | Compound vocabulary |
|--------|-------------------|
| gap-project-managing-coordinate-blockers-001 | coordinate + blockers together |
| gap-project-managing-escalation-accountability-001 | escalation + accountability together |
| gap-project-managing-track-progress-001 | track + progress compound phrase |

---

## 12 key teaching points for B37

1. **"track" ≠ TEST**: Most important B37 boundary. "track progress" → zero predicates.
   TEST fires only on: test, verify, assert, coverage, spec, case, suite, scenario,
   acceptance, unit, regression, e2e. "track" has NO entry in TEST lexicon.

2. **"report" ≠ DOCUMENT**: "Status report" → zero predicates from "report".
   DOCUMENT fires on requirements vocabulary (document, spec, define, describe,
   requirement, story, criteria, stakeholder, functional, behavior, use-case).

3. **"status" → zero predicates**: No predicate fires on "status" alone or in
   "status report", "RAG-status", "delivery status".

4. **"dependencies" context rule**: API_CONTRACT fires ONLY with API co-occurrence
   (api, contract, endpoint, interface, integration, swagger, openapi, graphql,
   rest, grpc, webhook). Team/project dependencies → gap.

5. **"stakeholder" → DOCUMENT ALWAYS**: Even in notification/managing sentences.
   This is a documented false positive in B37 context. Gap warning captures intent.

6. **"structure" → ARCHITECTURE ALWAYS**: Even in "accountability structure",
   "governance structure", "org structure". Same for "design" and "architecture".

7. **manage, coordinate, progress, escalate → ZERO predicates**: These four tokens
   are the core B37 managing vocabulary and fire nothing.

8. **DomainId binary switch**: Zero predicates → "project-management". ANY predicate
   → "software-development". No gradation. No intent-based override.

9. **B37 gap domain "project-managing" ≠ B36 gap domain "project-planning"**: Both
   produce DomainId "project-management" for zero-predicate requests. Gap ID sub-domain
   differs: B37 = "project-managing"; B36 = "project-planning".

10. **v6 corruptions**: v6-01 (invent MANAGEMENT/COORDINATION predicate),
    v6-02 (suppress DOCUMENT for stakeholder in managing context),
    v6-03 (set DomainId "project-management" when predicate fired).

11. **SCHEDULE_CAPABILITY_LEARNING**: Emitted once per zero-predicate B37 request.
    NOT emitted when any predicate fires. One event per request, not per gap ID.

12. **Layer 2 = capability-gap-extended-pairs**: No nine-node flow for B37. Extended
    pairs document gap traces, mixed-signal handling, and CapabilityGapWarnings.

---

## 8 cross-layer synthesis traces

| File | Request | Key outcome |
|------|---------|------------|
| cross-layer-synthesis-001.md | "Coordinate blockers and track progress" | Zero predicates → knowledge_edge, SCL emitted, 6 gap IDs |
| cross-layer-synthesis-002.md | "Send progress report to stakeholders" | DOCUMENT false positive → 2.1, 4 gap warnings, no SCL |
| cross-layer-synthesis-003.md | "API dependencies" vs "team dependencies" | API co-occurrence → API_CONTRACT+DOCUMENT+IMPLEMENT; no API → DOCUMENT only + gap |
| cross-layer-synthesis-004.md | "Manage delivery of auth service — build module" | IMPLEMENT fires on service/build/module; manage/delivery → gap warning; DomainId "software-development" |
| cross-layer-synthesis-005.md | "Track team progress" (pure gap) | Zero predicates → knowledge_edge, SCL, DomainId "project-managing" |
| cross-layer-synthesis-006.md | "Stakeholder status update" | DOCUMENT fires (stakeholder), DomainId "software-development", 4 gap warnings |
| cross-layer-synthesis-007.md | "Define accountability structure and document for stakeholders" | ARCHITECTURE+DOCUMENT fire, DomainId "software-development", routes 2.1+3.1 |
| cross-layer-synthesis-008.md | "Track, coordinate, document stakeholder needs, and build the feature" | DOCUMENT+IMPLEMENT fire, DomainId "software-development", routes 2.1+4.1, 5 gap warnings |

---

## Gate B

Gate B condition: All held-out pairs produce the expected outcome. All corruption
pairs are correctly identified with specific corruption type named.

Gate B status: ACCEPTED

Key held-out validation targets:
- HO-005-002: "project dependencies" → zero predicates (not API_CONTRACT)
- HO-009-001: "stakeholder status update" → DOCUMENT fires (not suppressed)
- HO-007-001: "build service, track progress" → IMPLEMENT only; "track" = gap
- HO-011-002: v6-02 detection — DOCUMENT not suppressed for stakeholder
- HO-010-001: "follow-up action items blockers" → zero predicates → knowledge_edge

All five certification tests must pass. Zero v6 corruptions permitted.

---

## Extended vocabulary (013-024): new gap vocabulary added

| Vocabulary | Gap ID |
|-----------|--------|
| risk log | gap-project-managing-raid-log-001 |
| change request | gap-project-managing-manage-delivery-001 |
| project charter | gap-project-managing-manage-delivery-001 |
| decision log | gap-project-managing-manage-delivery-001 |
| lessons learned | gap-project-managing-manage-delivery-001 |
| accountability | gap-project-managing-accountability-001 |
| RACI | gap-project-managing-accountability-001 |
| ownership | gap-project-managing-accountability-001 |

New boundary rules confirmed in 013-024 extension:
- "create" → IMPLEMENT fires even for document creation
- "log" alone → zero predicates (not DOCUMENT)
- "framework"/"model"/"hierarchy" → zero predicates (not ARCHITECTURE)
- "programme board"/"steering committee" → zero predicates (not DOCUMENT)

---

## Batch identity

- Batch: B37-batch-001
- Scenario: B37 — Project managing — progress, coordination, and status reporting
- Batch type: CAPABILITY GAP
- Layer 2 type: capability-gap-extended-pairs (NOT nine-node pairs)
- Production date: 2026-05-05 (base); 2026-05-06 (extension 013-024)
- Total files (including MANIFEST): 201
- Total training files (excluding MANIFEST): 200
