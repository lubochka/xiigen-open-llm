# Layer 0-A — Compiler Pairs (057)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-057-001 — "program increment" + "value stream" combined

**Input:** "Map value streams for the program increment delivery."

Token scan:
- "Map" → gap; "value streams" → gap; "program increment" → gap; "delivery" → gap
Predicates: []
DomainId: "project-management"
SCL: emitted

---

## B37-0A-057-002 — "portfolio management" + "waterfall" + "agile" combined

**Input:** "Manage portfolio management across waterfall and agile transformation workstreams."

Token scan:
- All tokens: portfolio management, waterfall, agile transformation, workstreams = gap
Predicates: []
DomainId: "project-management"
SCL: emitted

---

## B37-0A-057-003 — "project closure" + "post-mortem" + "benefits realization"

**Input:** "Run project closure, facilitate the post-mortem, and track benefits realization."

Token scan: all gap vocabulary.
Predicates: []
DomainId: "project-management"
SCL: emitted

---

## B37-0A-057-004 — "work breakdown structure" is the only Round 4 self-firing predicate

**Rule clarification:**

All Round 4 vocabulary (049-060) is zero-predicate when used alone, EXCEPT:
- "work breakdown structure" → "structure" fires ARCHITECTURE

This is the ONLY Round 4 term that self-fires a predicate (via "structure" token).

All other Round 4 terms (waterfall, agile transformation, SAFe, program increment, portfolio management,
value stream, flow state, project closure, benefits realization, KPI monitoring) = zero predicates alone.

---

## B37-0A-057-005 — "flow state" + "SAFe" + "program increment" combined

**Input:** "Monitor flow state, coordinate SAFe ceremonies, and track program increment objectives."

Token scan:
- "Monitor" → gap; "flow state" → gap; "coordinate" → gap
- "SAFe ceremonies" → gap; "track" → NOT TEST → gap; "program increment" → gap; "objectives" → gap

Predicates: []
DomainId: "project-management"
SCL: emitted
