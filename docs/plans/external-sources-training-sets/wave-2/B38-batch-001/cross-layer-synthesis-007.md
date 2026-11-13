# Cross-Layer Synthesis (007)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## B38-CLS-007 — QA+DOCUMENT+IMPLEMENT for hotfix with release notes

Scenario: HotfixWithReleaseNotes
Input: "Review the hotfix to the auth service, document the release notes and exit criteria, and sign off."
Full trace: three predicates, three routing decisions, feature_addition.

---

### LAYER 0-A: Compiler

Input vocabulary analysis:
- "Review" → QA (review is QA lexicon core)
- "hotfix" → no predicate (delivery context noun — rule: hotfix is context, not IMPLEMENT)
- "auth service" → IMPLEMENT (service fires IMPLEMENT always — rule 2)
- "document" → DOCUMENT (document fires DOCUMENT)
- "release notes" → context phrase (notes are the content; "document" is the predicate trigger)
- "exit criteria" → DOCUMENT (criteria fires DOCUMENT — rule 3; consolidates with "document")
- "sign off" → QA (sign-off fires QA — rule 23; consolidates with "review")

PredicateSet: ["QA", "IMPLEMENT", "DOCUMENT"]
CapabilityGapWarnings: []
DomainId: software-development

---

### LAYER 0-B: Router

Routing decisions:
1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
2. DOCUMENT → Phase 2.1 (DocumentationFlow)
3. QA → Phase 7.1 (ReleaseGateFlow71) — QA last (absolute rule 12)

RoutingTable:
| Predicate | Phase | Flow | Order |
|---|---|---|---|
| IMPLEMENT | 4.1 | BackendServiceFlow41 | 1 |
| DOCUMENT | 2.1 | DocumentationFlow | 2 |
| QA | 7.1 | ReleaseGateFlow71 | 3 |

DecisionCount: 3
TransitionType: feature_addition (IMPLEMENT present)

---

### LAYER 0-C: Knowledge Edge

Knowledge edges in this scenario:
- "auth service" → IMPLEMENT fires (rule 2 knowledge edge — service always fires IMPLEMENT)
- "exit criteria" → DOCUMENT fires (rule 3 knowledge edge — criteria always fires DOCUMENT)
- "hotfix" → no predicate (boundary: hotfix does not fire IMPLEMENT)
- No gap vocabulary; no knowledge_edge triggered
- FLOW_EXECUTING (three predicates fired)

---

### LAYER 0-D: Flow Factory

Three flows created:
1. BackendServiceFlow41 (Phase 4.1, IMPLEMENT, order 1)
2. DocumentationFlow (Phase 2.1, DOCUMENT, order 2)
3. ReleaseGateFlow71 (Phase 7.1, QA, order 3 — last)

Gate B: not required (all deterministic_generic)
scenario_transition_type: feature_addition

---

### LAYER 1: Transition Orchestrator

Orchestration:
1. BackendServiceFlow41 executes (IMPLEMENT phase):
   - Scope: auth service hotfix code review
   - Output: auth_service_hotfix_review.md
2. DocumentationFlow executes (DOCUMENT phase):
   - Scope: release notes + exit criteria documentation
   - Output: release_notes_hotfix.md, exit_criteria_doc.md
3. ReleaseGateFlow71 executes (QA phase — last):
   - Input: all prior outputs as evidence
   - Output: hotfix_release_sign_off.md

TransitionType: feature_addition

---

### LAYER 2: Nine-Node Flow (7.1)

N01: ["QA","IMPLEMENT","DOCUMENT"] confirmed; feature_addition
N02: DomainStatement: "Hotfix release gate with release notes and exit criteria documentation"
N03-N08: Three flow outputs as evidence; gate evaluation complete
N09 Result:
- TransitionType: feature_addition
- Files: [auth_service_hotfix_review.md, release_notes_hotfix.md, exit_criteria_doc.md, hotfix_release_sign_off.md]
- FileCount: 4 (one per output across all flows)
- ResultState: hotfix_with_release_notes_approved
- DecisionCount: 3

Note: FileCount is 4 because DocumentationFlow produces two documents. FileCount > DecisionCount is valid.

---

### LAYER 3: Teaching Points

1. QA+DOCUMENT+IMPLEMENT from a hotfix scenario: "service" forces IMPLEMENT; "criteria" forces DOCUMENT; QA terms fire QA.
2. "hotfix" is a context noun — it does NOT force IMPLEMENT. Only "service"/"build"/"pipeline" force IMPLEMENT.
3. Three predicates, three routing decisions, feature_addition because IMPLEMENT is present.
4. QA is last (rule 12 — absolute). IMPLEMENT executes first, DOCUMENT second, QA last.
5. The QA gate receives all prior flow outputs as evidence (service review + documentation).
6. FileCount (4) can exceed DecisionCount (3) when a single flow produces multiple output files.
