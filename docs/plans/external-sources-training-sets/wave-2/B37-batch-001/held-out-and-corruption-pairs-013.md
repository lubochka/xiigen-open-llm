# Held-Out and Corruption Pairs (013)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-013-001 — HO: "risk log" maintenance → knowledge_edge

**Held-out request:** "Maintain and monitor the project risk log for the delivery programme."

**Expected output:**
- "Maintain" → not in any predicate lexicon
- "risk log" → gap
- "monitor" → not in any predicate lexicon
- "delivery programme" → gap
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: gap-project-managing-raid-log-001, gap-project-managing-manage-delivery-001

---

## B37-HO-013-002 — COR: "track" fires TEST on "risk log" (wrong)

**Corrupted output:** "Track all risks in the risk log." → ["TEST"] — "track" fires TEST

**Why this is wrong:** Rule 2 — "track" does NOT fire TEST. TEST fires on: test, verify, assert, coverage, acceptance, unit, regression, e2e. "track" has NO entry in the TEST lexicon.

**Correct output:** [] (zero predicates)

**Corruption label:** v6-01 — inventing TEST predicate from "track"

---

## B37-HO-013-003 — HO: "review risk log" → QA fires

**Request:** "Review all entries in the risk log before the milestone meeting."

**Expected output:**
- "Review" → QA fires (Rule 19)
- "risk log" → gap
- Predicates: ["QA"]
- DomainId: "software-development"
- Route: ReviewFlow (QA sub-phase)
- Gap ID: gap-project-managing-raid-log-001

---

## B37-HO-013-004 — COR: "document risk log" → DOCUMENT fires but gap warning suppressed (wrong)

**Request:** "Document all risks in the risk log."

**v6 corrupt output:** Predicates: ["DOCUMENT"] — DOCUMENT fires correctly, BUT gap warning gap-project-managing-raid-log-001 is NOT forwarded.

**Why this is wrong:** Gap warnings must always be forwarded (Rule: gap warnings never suppressed). Even when predicates fire, gap vocabulary present in the sentence generates gap warnings.

**Correct output:** Predicates: ["DOCUMENT"], CapabilityGapWarnings: [gap-project-managing-raid-log-001]

---

## B37-HO-013-005 — HO: risk log held-out certification summary

| Case | Request | Expected predicates | Expected DomainId |
|------|---------|-------------------|-----------------|
| HO-013-001 | "Maintain the risk log" | [] | project-management |
| HO-013-002 | "Track risks in risk log" | [] | project-management |
| HO-013-003 | "Review risk log entries" | [QA] | software-development |
| HO-013-004 | "Escalate risks from risk log" | [] | project-management |
| HO-013-005 | "Document risks in risk log" | [DOCUMENT] | software-development |

All five must pass. Zero v6 corruptions permitted.
