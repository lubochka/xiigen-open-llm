# Layer 3 — Teaching-Unit Pairs (002)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: teaching-unit-pairs

---

## B37-L3-002-001 — Teaching: "dependencies" context-sensitivity rule

**Concept:** The "dependencies" predicate rule (Rule 5) requires API co-occurrence for API_CONTRACT to fire. Without API co-occurrence, "dependencies" fires nothing.

**Why this rule exists:**
"Dependencies" in software development most commonly refers to software/API dependencies (libraries, services, contracts). However, in project management, "dependencies" refers to team, cross-project, or delivery dependencies. The rule separates these by requiring an explicit API co-occurrence signal.

**Learning pair — correct:**
Q: What does "project team dependencies" fire?
A: Nothing. Zero predicates. gap-project-managing-dependencies-team-001. DomainId "project-management".

**Learning pair — correct:**
Q: What does "API endpoint dependencies" fire?
A: API_CONTRACT. DomainId "software-development".

---

## B37-L3-002-002 — Teaching: API co-occurrence list (exam-level)

**Complete API co-occurrence list for Rule 5:**
api, contract, endpoint, interface, integration, swagger, openapi, graphql, rest, grpc, webhook

**Mnemonic: "ACEIISG-ORGW"** — api, contract, endpoint, interface, integration, swagger, graphql, openapi, rest, grpc, webhook

**Test questions:**
Q: Does "service dependencies" trigger API_CONTRACT?
A: No. "service" is not in the API co-occurrence list.

Q: Does "REST dependencies" trigger API_CONTRACT?
A: Yes. "rest" is in the API co-occurrence list.

Q: Does "backend integration dependencies" trigger API_CONTRACT?
A: Yes. "integration" is in the API co-occurrence list.

Q: Does "delivery dependencies" trigger API_CONTRACT?
A: No. "delivery" is not in the API co-occurrence list.

---

## B37-L3-002-003 — Teaching: "dependencies" in RAID context

**Input:** "Document the project dependencies and risks in the RAID log."

**Token-by-token:**
- "Document" → DOCUMENT fires
- "project dependencies" → no API co-occurrence → gap (not API_CONTRACT)
- "risks" → gap (RAID-R)
- "RAID log" → gap

Result: ["DOCUMENT"]

CapabilityGapWarnings: gap-project-managing-dependencies-team-001, gap-project-managing-raid-log-001

**Teaching point:** "Document" fires DOCUMENT. "project dependencies" does NOT fire API_CONTRACT. The RAID context does not change the API co-occurrence rule.

---

## B37-L3-002-004 — Teaching: escalating dependency vocabulary

**The "dependencies" chain:**
Level 0: "dependencies" alone → needs context
Level 1: + project/team/delivery/workstream → gap only
Level 2: + API/contract/endpoint/interface/integration → API_CONTRACT fires
Level 3: + document → DOCUMENT also fires
Level 4: + build/service/module → IMPLEMENT also fires

**Each level adds a predicate. None of the managing words in Level 1 add a predicate.**

---

## B37-L3-002-005 — Teaching: exam-level "dependencies" decision tree

```
"dependencies" in sentence?
├── Yes
│   ├── API co-occurrence present? (api/contract/endpoint/interface/integration/swagger/openapi/graphql/rest/grpc/webhook)
│   │   ├── Yes → API_CONTRACT fires
│   │   └── No → gap: gap-project-managing-dependencies-team-001
│   └── Other predicate tokens present? (document/define/etc)
│       ├── Yes → those predicates fire + gap-project-managing-dependencies-team-001 as warning
│       └── No → [] + gap-project-managing-dependencies-team-001
└── No → "dependencies" not relevant
```
