# Layer 0-D — Flow Factory Pairs (006)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-006-001 — API_CONTRACT factory: LMS integration

Predicates: ["API_CONTRACT"]
Request: "Integrate the internal LMS with the corporate identity provider."
Factory: APIContractFlow33
TransitionType: documentation (no IMPLEMENT)

---

## FF-006-002 — IMPLEMENT+API_CONTRACT+DOCUMENT factory

Predicates: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
Request: "Build the training notification service, integrate with SendGrid, and document the email templates."
Factory: APIContractFlow33 + ImplementationFlow41 + DocumentationFlow
TransitionType: feature_addition

---

## FF-006-003 — QA sub-phase 7.1 in skills context

Predicates: ["QA"]
Request: "QA the training platform release to the staging environment."
Factory: ReleaseGateFlow71 (sub-phase 7.1 — staging/release vocabulary → 7.1, not 7.3)
TransitionType: no contribution

---

## FF-006-004 — ARCHITECTURE+IMPLEMENT+QA factory

Predicates: ["ARCHITECTURE", "IMPLEMENT", "QA"]
Request: "Design the learning platform architecture, build the course service, and QA before launch."
Factory: ArchitectureFlow31 + ImplementationFlow41 + QAGatingFlow73
TransitionType: feature_addition | QA last

---

## FF-006-005 — v6 corruption: DOCUMENT invented for "explain"

v6 corrupt: "Explain the architecture" → Predicates: ["DOCUMENT"] → DocumentationFlow
Correct: Predicates: [] → GapHandler → gap-skills-guidance-mentor-001
Error type: v6-02 (gap suppression). "explain" ≠ DOCUMENT trigger. Explanation → gap always.
