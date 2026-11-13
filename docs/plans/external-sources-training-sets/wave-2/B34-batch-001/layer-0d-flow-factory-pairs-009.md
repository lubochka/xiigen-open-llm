# Layer 0-D — Flow Factory Pairs (009)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-009-001 — IMPLEMENT alone: build knowledge base

Predicates: ["IMPLEMENT"]
Request: "Build the internal knowledge base system with search and tagging."
Factory: ImplementationFlow41 (4.1)
TransitionType: feature_addition

---

## FF-009-002 — DOCUMENT alone: write knowledge base articles

Predicates: ["DOCUMENT"]
Request: "Write knowledge base articles on our engineering standards."
Factory: DocumentationFlow
TransitionType: documentation

---

## FF-009-003 — API_CONTRACT alone: LMS integration

Predicates: ["API_CONTRACT"]
Request: "Integrate the onboarding checklist with our HR system."
Factory: APIContractFlow33
TransitionType: documentation

---

## FF-009-004 — Gap factory: pure training request

Predicates: []
Request: "Design a training program for the engineering team."
Factory: GapHandler → gap-skills-guidance-train-001
Note: "design a training program" = curriculum design → gap. ARCHITECTURE not fired (no structural noun).

---

## FF-009-005 — IMPLEMENT+TEST+DOCUMENT+QA four-flow factory

Predicates: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"]
Request: "Build the mentorship tracker, write unit tests, document the matching algorithm, and QA the release."
Factory: ImplementationFlow41 + UnitTestingFlow51 + DocumentationFlow + QAGatingFlow73
TransitionType: feature_addition | QA last
