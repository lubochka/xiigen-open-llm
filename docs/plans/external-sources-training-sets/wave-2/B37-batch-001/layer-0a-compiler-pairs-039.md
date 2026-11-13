# Layer 0-A — Compiler Pairs (039)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-039-001 — "sign-off process" is zero-predicate vocabulary

**Input A:** "Manage the sign-off process for all deliverables."

Token scan:
- "Manage" → gap
- "sign-off process" → zero predicates → gap
- "deliverables" → zero predicates → gap

Predicates: []
DomainId: "project-management"

**Input B:** "Document the sign-off process for stakeholders."
- "Document" → DOCUMENT fires
- "sign-off process" → gap
- "stakeholders" → DOCUMENT fires

Predicates: ["DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-039-002 — "sign-off" alone vs "sign off document"

**Input A:** "Obtain sign-off from the programme board."
- "Obtain" → gap
- "sign-off" → zero predicates → gap
- "programme board" → gap
- Predicates: []

**Input B:** "Create a sign-off document for delivery approval."
- "Create" → IMPLEMENT fires
- "sign-off document" → "document" → DOCUMENT fires
- "delivery approval" → gap

Predicates: ["IMPLEMENT", "DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-039-003 — "handover document" fires DOCUMENT

**Input A:** "Prepare the handover for the next team."
- "Prepare" → gap
- "handover" → zero predicates → gap
- "team" → gap
- Predicates: []
- DomainId: "project-management"

**Input B:** "Prepare the handover document for the next team."
- "Prepare" → gap
- "handover document" → "document" fires DOCUMENT (Rule 8)
- "team" → gap

Predicates: ["DOCUMENT"]
DomainId: "software-development"

**Key rule:** "handover" alone → gap. "handover document" → DOCUMENT fires on "document" token.

---

## B37-0A-039-004 — "post-mortem" is zero-predicate vocabulary

**Input A:** "Run the post-mortem for the programme delivery."
- "Run" → gap
- "post-mortem" → zero predicates → gap
- "programme delivery" → gap
- Predicates: []

**Input B:** "Document the post-mortem findings for stakeholders."
- "Document" → DOCUMENT fires
- "post-mortem findings" → gap
- "stakeholders" → DOCUMENT fires

Predicates: ["DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-039-005 — "post-mortem" vs "retrospective" — both zero-predicate

**Input A:** "Run the post-mortem."
**Input B:** "Facilitate the retrospective."
**Input C:** "Coordinate the lessons-learned session."

All three:
- All managing vocabulary → gap
- Predicates: [] for all three
- DomainId: "project-management" for all three
- SCL emitted for all three

**Contrast:** "Document the retrospective findings." → DOCUMENT fires → ["DOCUMENT"] → software-development.
