# Layer 0-C — Knowledge-Edge Pairs (009)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B36-0C-009-001 — v6 corruption types in B36: overview

Three v6 corruption types are documented for B36:

| Code | Name | Stage affected | Symptom |
|------|------|---------------|---------|
| v6-01 | PROJECT_PLANNING predicate invented | Compiler | Non-existent predicate in output |
| v6-02 | Predicate suppressed by planning context | Compiler | Missing predicate in output |
| v6-03 | DomainId "project-management" when predicate fired | Orchestrator | Wrong DomainId in output |

---

## B36-0C-009-002 — v6-01 detailed: PROJECT_PLANNING predicate invented

**What a v6-01 model does:**
Sees sprint/backlog/velocity vocabulary and invents a new predicate "PROJECT_PLANNING" to classify the request. Emits ["PROJECT_PLANNING"] instead of [].

**Detection test:**
- Input: "Plan the sprint and groom the backlog."
- Correct output: predicates: []
- v6-01 output: predicates: ["PROJECT_PLANNING"]

**Downstream effect:**
Router receives ["PROJECT_PLANNING"]. Router has no registered flow for PROJECT_PLANNING. Router crashes with FlowNotFoundException or routes to default (wrong).

**Fix:** Only emit predicates from the nine registered predicates. When zero registered predicates fire, output [].

---

## B36-0C-009-003 — v6-02 detailed: predicate suppressed by planning context

**What a v6-02 model does:**
Sees "review" in a planning sentence, determines the sentence is about sprint/backlog/velocity, and decides "the user doesn't really mean QA review here" — suppresses the QA predicate.

**Detection test:**
- Input: "Review the sprint velocity."
- Correct output: predicates: ["QA"]
- v6-02 output: predicates: [] (QA suppressed)

**Downstream effect:**
Router receives []. Routes to KNOWLEDGE_EDGE. DomainId: "project-management" (wrong). No QA gate.

**Fix:** Predicates are lexicon matches. Semantic context does not override lexicon matches. "review" fires QA unconditionally.

---

## B36-0C-009-004 — v6-03 detailed: DomainId "project-management" when predicate fired

**What a v6-03 model does:**
Fires a predicate correctly (e.g., IMPLEMENT) but then sets DomainId: "project-management" because the sentence contains many planning terms (sprint, backlog, epic).

**Detection test:**
- Input: "Implement the sprint features."
- Correct output: predicates: ["IMPLEMENT"], DomainId: "software-development"
- v6-03 output: predicates: ["IMPLEMENT"], DomainId: "project-management"

**Downstream effect:**
DomainId is wrong. Downstream systems (domain-specific routing, persona selection, response formatting) receive incorrect domain context. The implementation work may be formatted or routed inappropriately.

**Fix:** DomainId is "software-development" whenever any predicate fires. The planning vocabulary proportion is irrelevant.
