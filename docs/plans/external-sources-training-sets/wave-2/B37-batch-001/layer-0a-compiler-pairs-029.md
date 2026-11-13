# Layer 0-A — Compiler Pairs (029)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-029-001 — "resource allocation" + "dependencies" combined

**Input A:** "Manage resource allocation and resolve team dependencies."

Token scan:
- "Manage" → gap
- "resource allocation" → gap
- "resolve" → not in predicate lexicon → gap
- "team dependencies" → no API co-occurrence → gap (Rule 5)

Predicates: []
DomainId: "project-management"

**Input B:** "Manage resource allocation and resolve API dependencies between services."

Token scan:
- "Manage" → gap
- "resource allocation" → gap
- "resolve" → gap
- "API dependencies" → "API" = API co-occurrence → API_CONTRACT fires (Rule 5)
- "services" → "service" → IMPLEMENT fires (Rule 9)

Predicates: ["API_CONTRACT", "IMPLEMENT"]
DomainId: "software-development"
GapIds forwarded: manage-delivery-001

---

## B37-0A-029-002 — "budget tracking" with "define" false positive

**Input A:** "Track budget for the next sprint."

Token scan:
- "Track" → NOT TEST → gap
- "budget" → gap
- "sprint" → not in predicate lexicon → gap

Predicates: []
DomainId: "project-management"

**Input B:** "Define the budget tracking criteria and document variance thresholds."

Token scan:
- "Define" → DOCUMENT fires
- "budget tracking" → gap
- "criteria" → DOCUMENT fires
- "document" → DOCUMENT fires
- "variance thresholds" → gap

Predicates: ["DOCUMENT"]
DomainId: "software-development"

---

## B37-0A-029-003 — "milestone tracking" + "review" false positive

**Input:** "Review milestone tracking data and report findings to the programme board."

Token scan:
- "Review" → QA fires
- "milestone tracking" → gap
- "report" → NOT DOCUMENT (Rule 3) → gap
- "findings" → gap
- "programme board" → zero predicates → gap

Predicates: ["QA"]
DomainId: "software-development"
GapIds forwarded: manage-delivery-001, progress-001, status-report-001

---

## B37-0A-029-004 — "SLA tracking" + "implement" false positive

**Input:** "Implement the SLA tracking dashboard for vendor compliance reporting."

Token scan:
- "Implement" → IMPLEMENT fires (Rule 9)
- "SLA tracking" → gap vocabulary
- "dashboard" → not standalone predicate trigger (no UI predicate in this system)
- "vendor compliance" → gap
- "reporting" → NOT DOCUMENT → gap

Predicates: ["IMPLEMENT"]
DomainId: "software-development"
GapIds forwarded: track-001, manage-delivery-001

---

## B37-0A-029-005 — "dependency mapping" + "design" false positive

**Input:** "Design the dependency mapping workflow for the cross-functional programme."

Token scan:
- "Design" → ARCHITECTURE fires (Rule 7)
- "dependency mapping" → gap (no API co-occurrence on "dependency" here)
- "workflow" → not in predicate lexicon → gap
- "cross-functional" → gap
- "programme" → gap

Predicates: ["ARCHITECTURE"]
DomainId: "software-development"
GapIds forwarded: dependencies-team-001, manage-delivery-001
