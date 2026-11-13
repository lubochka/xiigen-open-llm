# Layer 1 — Transition Orchestrator Pairs (005)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-005-001 — QA sub-phase: 7.1 vs 7.3 in B34

7.1 (ReleaseGateFlow71): staging/deployment/release vocabulary
- "QA the training platform release to staging" → 7.1
7.3 (QAGatingFlow73): feature acceptance gate (default)
- "QA the onboarding workflow before launch" → 7.3
- "QA sign-off on the certification feature" → 7.3

---

## TO-005-002 — "guide through implementing" fires IMPLEMENT

Request: "Guide the team through implementing the new onboarding API."
Predicates: ["IMPLEMENT"] (implementing = embedded IMPLEMENT action inside guidance frame)
Orchestration: ImplementationFlow41
TransitionType: feature_addition
Key rule: The embedded verb "implementing" is the action, not "guide." IMPLEMENT fires.

---

## TO-005-003 — DOCUMENT alone in guidance context

Request: "Document the mentorship program policies and procedures."
Predicates: ["DOCUMENT"]
Orchestration: DocumentationFlow
TransitionType: documentation
No gap: "document" fires DOCUMENT; guidance context of "mentorship program" is merely the object.

---

## TO-005-004 — Gap: "help the team adopt" → gap

Request: "Help the team adopt the new CI/CD practices."
Predicates: [] | GapHandler → gap-skills-guidance-train-001
Evidence: "help adopt" = facilitation/adoption vocabulary → gap. "CI/CD" alone ≠ predicate.

---

## TO-005-005 — IMPLEMENT+DOCUMENT: guidance context suppressed

Request: "Coach the team through the process of building and documenting the new onboarding tool."
Predicates: ["IMPLEMENT", "DOCUMENT"] (build + document embedded inside "coach through")
Orchestration: ImplementationFlow41 ‖ DocumentationFlow
TransitionType: feature_addition
"coach through" = guidance frame; embedded actions "building" + "documenting" fire predicates.
