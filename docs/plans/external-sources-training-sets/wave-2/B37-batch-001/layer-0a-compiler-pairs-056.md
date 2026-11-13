# Layer 0-A — Compiler Pairs (056)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-056-001 — "scope creep" in agile context — zero predicates

**Input:** "Monitor scope creep during agile transformation and manage SAFe adoption."

Token scan:
- "Monitor" → gap; "scope creep" → gap; "agile transformation" → gap; "manage" → gap; "SAFe adoption" → gap
Predicates: []
DomainId: "project-management"
SCL: emitted

---

## B37-0A-056-002 — "change control board" + "steering committee" combined

**Input:** "Escalate scope creep to the change control board and steering committee."

Token scan:
- "Escalate" → gap; "scope creep" → gap; "change control board" → gap; "steering committee" → gap
Predicates: []
DomainId: "project-management"
SCL: emitted

---

## B37-0A-056-003 — "risk mitigation" + "quality gate" combined — zero predicates

**Input:** "Run risk mitigation activities and enforce quality gates before delivery."

Token scan:
- "Run" → gap; "risk mitigation" → gap; "activities" → gap
- "enforce" → gap; "quality gates" → gap; "delivery" → gap
Predicates: []
DomainId: "project-management"
SCL: emitted

---

## B37-0A-056-004 — "handover document" + "sign-off process" — DOCUMENT fires

**Input:** "Prepare the handover document and manage the sign-off process."

Token scan:
- "Prepare" → gap; "handover document" → "document" fires DOCUMENT
- "manage" → gap; "sign-off process" → gap
Predicates: ["DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-056-005 — "post-mortem" + "lessons learned" combined — zero predicates

**Input:** "Run the post-mortem and coordinate the lessons-learned session."

Token scan:
- "Run" → gap; "post-mortem" → gap
- "coordinate" → gap; "lessons-learned session" → gap
Predicates: []
DomainId: "project-management"
SCL: emitted
