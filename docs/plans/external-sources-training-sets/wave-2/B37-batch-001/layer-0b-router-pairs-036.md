# Layer 0-B — Router Pairs (036)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: router-pairs

---

## B37-0B-036-001 — Complete Round 2 routing reference table (025-036 vocabulary)

**Master routing table for Round 2 PM vocabulary:**

| Input pattern | Predicates | Route | DomainId |
|--------------|-----------|-------|---------|
| status report alone | [] | knowledge_edge | project-management |
| status report + stakeholders | [DOCUMENT] | 2.1 | software-development |
| status report + define | [DOCUMENT] | 2.1 | software-development |
| progress update alone | [] | knowledge_edge | project-management |
| progress update + stakeholders | [DOCUMENT] | 2.1 | software-development |
| milestone tracking alone | [] | knowledge_edge | project-management |
| milestone tracking + review | [QA] | QA sub-phase | software-development |
| Gantt chart maintenance | [] | knowledge_edge | project-management |
| Gantt chart + create | [IMPLEMENT] | 4.1 | software-development |
| resource allocation alone | [] | knowledge_edge | project-management |
| resource allocation + design/structure | [ARCHITECTURE] | 3.1 | software-development |
| capacity planning alone | [] | knowledge_edge | project-management |
| capacity planning + structure | [ARCHITECTURE] | 3.1 | software-development |
| dependency mapping (no API) | [] | knowledge_edge | project-management |
| dependency mapping + API | [API_CONTRACT] | 3.3 | software-development |
| critical path alone | [] | knowledge_edge | project-management |
| budget tracking alone | [] | knowledge_edge | project-management |
| vendor management alone | [] | knowledge_edge | project-management |
| SLA tracking alone | [] | knowledge_edge | project-management |
| SLA tracking + implement/service | [IMPLEMENT] | 4.1 | software-development |

---

## B37-0B-036-002 — Complete Round 2 SCL emission reference

**SCL emitted (zero predicates) — all Round 2 pure PM requests:**
All 11 vocabulary families + their combinations = knowledge_edge + SCL emitted.

**SCL NOT emitted — Round 2 false positive trigger co-occurrences:**
review, validate, stakeholder, define, document, criteria, structure, design, architecture, build, create, implement, write, service, module, API co-occurrence + dependencies.

---

## B37-0B-036-003 — Round 2 + Round 1 combined routing behaviour

**All B37 rounds (base 002-024 + Round 2 025-036) behave identically:**

1. Zero-predicate rule: unchanged — all PM vocabulary → knowledge_edge
2. False positive rules: unchanged — review, stakeholder, structure, define, create, etc.
3. API co-occurrence rule: unchanged — dependencies + API token → API_CONTRACT
4. DomainId rule: unchanged — zero predicates → project-management; any predicate → software-development
5. SCL rule: unchanged — one per zero-predicate request
6. Gap warning forwarding: unchanged — always forwarded, never suppressed
7. No new flow factories: unchanged — B37 is purely knowledge_edge for zero predicates

---

## B37-0B-036-004 — v6 corruptions for Round 2 vocabulary

**v6-01 examples (invented predicate):**
- "Track milestones" → ["MILESTONE_TRACKING"] — invented
- "Run capacity planning" → ["CAPACITY_PLANNING"] — invented
- "Manage vendors" → ["VENDOR_MANAGEMENT"] — invented
Correct for all three: Predicates: []

**v6-02 examples (suppress DOCUMENT):**
- "Send status report to stakeholders" → DOCUMENT suppressed — v6-02
- "Provide progress update to stakeholders" → DOCUMENT suppressed — v6-02
Correct: DOCUMENT fires on "stakeholders" — no suppression.

**v6-03 examples (wrong DomainId):**
- "Review milestone SLA tracking" → DomainId "project-management" despite QA firing — v6-03
- "Design capacity structure" → DomainId "project-management" despite ARCHITECTURE firing — v6-03
Correct: DomainId "software-development" when any predicate fires.

---

## B37-0B-036-005 — Round 2 router summary

**Summary: B37 Round 2 vocabulary (025-036) router behaviour**

1. All eleven new PM vocabulary families → zero predicates when used alone
2. False positives unchanged from base: review → QA; stakeholder → DOCUMENT; structure → ARCHITECTURE; create → IMPLEMENT; API + dependencies → API_CONTRACT
3. "report" rule (Rule 3) confirmed: "status report" / "progress report" → zero predicates
4. "track" rule (Rule 2) confirmed: "milestone tracking" / "SLA tracking" / "budget tracking" → zero predicates
5. "contract" boundary confirmed: "vendor contracts" alone → gap; "vendor API contract" → API_CONTRACT
6. DomainId unchanged: zero predicates → "project-management"; any predicate → "software-development"
7. No new factories added in Round 2
