# Layer 0-A — Compiler Pairs (045)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-045-001 — "quality gate" vocabulary: "QA" co-occurrence fires QA

**Input A:** "Enforce the quality gate before deployment." → Predicates: []
**Input B:** "Review quality gate criteria before deployment." → ["QA", "DOCUMENT"]
**Input C:** "Define quality gate acceptance criteria." → ["DOCUMENT"] — "define" + "criteria"
**Input D:** "Build the quality gate monitoring service." → ["IMPLEMENT"]

**Rule:** "quality gate" alone → zero predicates. "QA" fires only when "review" or "validate" present.

---

## B37-0A-045-002 — "sign-off process" full vocabulary scan

**Input:** "Coordinate sign-off, track approvals, and manage delivery across programmes."

Token scan:
- "Coordinate" → gap
- "sign-off" → gap
- "track" → NOT TEST → gap
- "approvals" → gap
- "manage" → gap
- "delivery" → gap
- "programmes" → gap

Predicates: []
DomainId: "project-management"
GapIds: coordinate-001, manage-delivery-001, track-001
SCL: emitted

---

## B37-0A-045-003 — "post-mortem" with predicate triggers

**Input A:** "Facilitate the post-mortem." → Predicates: []
**Input B:** "Document the post-mortem outcomes for stakeholders." → ["DOCUMENT"]
**Input C:** "Review post-mortem findings." → ["QA"]
**Input D:** "Define post-mortem criteria." → ["DOCUMENT"]
**Input E:** "Write the post-mortem report." → ["DOCUMENT"] — "write" fires; "report" alone → gap

All confirm: "post-mortem" = gap. Predicate from co-occurring token.

---

## B37-0A-045-004 — "handover document" boundary — "handover" vs "handover document"

**Input A:** "Manage the handover." → Predicates: [] — "handover" alone → gap
**Input B:** "Manage the handover document." → ["DOCUMENT"] — "document" fires
**Input C:** "Write the handover document." → ["DOCUMENT"] — "write" + "document" fire (one DOCUMENT)
**Input D:** "Prepare the handover plan." → Predicates: [] — "plan" alone → gap

**Rule:** "handover" without "document" → gap. "handover document" → DOCUMENT on "document".

---

## B37-0A-045-005 — PM+IMPLEMENT combo: coordinating vs building a reporting service

**Input A:** "Coordinate delivery of the reporting service." → ["IMPLEMENT"] — "service" fires
**Input B:** "Coordinate delivery milestones." → [] — "milestones" alone → gap
**Input C:** "Track reporting progress." → [] — "reporting" + "progress" → gap
**Input D:** "Build the progress reporting service." → ["IMPLEMENT"] — "build" + "service" fire

**Key distinction:** Managing verbs (coordinate, track, manage) do NOT fire predicates. The noun "service" fires IMPLEMENT regardless of the managing verb.
