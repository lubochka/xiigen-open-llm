# Layer 0-B — Router Pairs (016)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-016-001 — [QA, DOCUMENT]: release notes documentation

PredicateSet: ["QA", "DOCUMENT"]
Scenario: ReleaseNotesDocumentation
Input: "Document the release notes for v3.2 and have the product owner sign off."

Routing decisions:
1. DOCUMENT → Phase 2.1 (DocumentationFlow)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: no_contribution
OutputFiles: [release_notes_v3_2.md, sign_off_register.md]

---

## B38-0B-016-002 — [QA, IMPLEMENT, DOCUMENT]: release notes with service

PredicateSet: ["QA", "IMPLEMENT", "DOCUMENT"]
Scenario: NotificationServiceReleaseNotes
Input: "Document the release notes for the notification service and review them with the team."

Routing decisions:
1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
2. DOCUMENT → Phase 2.1 (DocumentationFlow)
3. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 3
TransitionType: feature_addition

---

## B38-0B-016-003 — [QA, DOCUMENT]: release notes criteria review

PredicateSet: ["QA", "DOCUMENT"]
Scenario: ReleaseNotesCriteriaReview

Routing decisions:
1. DOCUMENT → Phase 2.1
2. QA → Phase 7.1 — QA last

DecisionCount: 2
TransitionType: no_contribution

---

## B38-0B-016-004 — Knowledge_edge: publish notes — no predicates

PredicateSet: []
Scenario: PublishReleaseNotes
Input: "Publish the release notes on the go-live day and distribute to all stakeholders."

Routing: knowledge_edge
CapabilityGapWarnings: ["go-live"]
DecisionCount: 1 (knowledge_edge sentinel)
TransitionType: N/A

Note: "publish" and "distribute" are not predicate triggers. Neither fires QA, IMPLEMENT, DOCUMENT, or SECURITY.
