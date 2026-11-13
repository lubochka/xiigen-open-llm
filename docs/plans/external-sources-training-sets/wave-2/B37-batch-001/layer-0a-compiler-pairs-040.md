# Layer 0-A — Compiler Pairs (040)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-040-001 — PM+IMPLEMENT combo: "build" fires even in managing context

**Input:** "Manage the delivery of the new reporting service."

Token scan:
- "Manage" → gap
- "delivery" → gap
- "reporting service" → "service" fires IMPLEMENT (Rule 9)

Predicates: ["IMPLEMENT"]
DomainId: "software-development"

**Rule confirmed:** "service" fires IMPLEMENT regardless of managing context. "Manage" does not suppress it.

---

## B37-0A-040-002 — PM+IMPLEMENT combo: "module" fires even with tracking verbs

**Input A:** "Track the delivery of the payment module."
- "Track" → NOT TEST (Rule 2) → gap
- "delivery" → gap
- "payment module" → "module" fires IMPLEMENT

Predicates: ["IMPLEMENT"]
DomainId: "software-development"

**Input B:** "Track the delivery milestones."
- "Track" → gap
- "delivery milestones" → gap

Predicates: []
DomainId: "project-management"

**Contrast:** "module" fires IMPLEMENT; "milestones" does not.

---

## B37-0A-040-003 — "build the tracking system" — IMPLEMENT + gap

**Input:** "Coordinate the team and build the tracking system."

Token scan:
- "Coordinate" → gap
- "team" → gap
- "build" → IMPLEMENT fires
- "tracking system" → "tracking" alone → gap; "system" → not in predicate lexicon

Predicates: ["IMPLEMENT"]
DomainId: "software-development"
GapIds: gap-project-managing-coordinate-001 (forwarded)

---

## B37-0A-040-004 — PM+DOCUMENT combo: "document [artifact]" rule

**Input A:** "Document the escalation path." → DOCUMENT fires on "document"
**Input B:** "Document the communication plan." → DOCUMENT fires on "document"
**Input C:** "Document the handover process." → DOCUMENT fires on "document"
**Input D:** "Document the sign-off criteria." → DOCUMENT fires on "document" AND "criteria"

All four: Predicates: ["DOCUMENT"] — DomainId: "software-development"

**Rule:** Any "document [PM artifact]" → DOCUMENT fires. The PM artifact itself remains gap vocabulary — only "document" fires the predicate.

---

## B37-0A-040-005 — Complete PM+predicate combo table for 037-040 vocabulary

| Input pattern | Predicates | DomainId |
|--------------|-----------|---------|
| escalation process alone | [] | project-management |
| document escalation process | [DOCUMENT] | software-development |
| steering committee alone | [] | project-management |
| steering committee + stakeholders | [DOCUMENT] | software-development |
| change control board alone | [] | project-management |
| document CCB criteria | [DOCUMENT] | software-development |
| scope creep alone | [] | project-management |
| define scope + creep | [DOCUMENT] | software-development |
| risk mitigation alone | [] | project-management |
| implement risk mitigation | [IMPLEMENT] | software-development |
| quality gate alone | [] | project-management |
| review quality gate | [QA] | software-development |
| sign-off alone | [] | project-management |
| handover document | [DOCUMENT] | software-development |
| post-mortem alone | [] | project-management |
| document post-mortem | [DOCUMENT] | software-development |
