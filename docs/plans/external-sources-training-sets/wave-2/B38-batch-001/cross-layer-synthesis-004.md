# Cross-Layer Synthesis (004)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## B38-CLS-004 — QA+IMPLEMENT+DOCUMENT+SECURITY: Full release gate pipeline

Scenario: FullReleaseGatePipeline
Input: "Review the payment service deployment, validate the release criteria, audit the security controls, and approve the go-live."
Full trace: four predicates, four routing decisions, QA last.

---

### LAYER 0-A: Compiler

Input vocabulary analysis:
- "Review" → QA
- "payment service" → IMPLEMENT (service fires IMPLEMENT always)
- "deployment" → GAP (rule 4)
- "validate" → QA
- "release criteria" → DOCUMENT (criteria fires DOCUMENT)
- "audit" → QA (release context) + SECURITY co-fires (rule 19: security + audit)
- "security controls" → SECURITY ("security" fires SECURITY)
- "controls" → SECURITY context
- "approve" → QA
- "go-live" → GAP (rule 5)

PredicateSet: ["QA", "IMPLEMENT", "DOCUMENT", "SECURITY"]
CapabilityGapWarnings: ["deployment", "go-live"]
DomainId: software-development

---

### LAYER 0-B: Router

Routing decisions:
1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
2. DOCUMENT → Phase 2.1 (DocumentationFlow)
3. SECURITY → Phase 3.4 (SecurityAuditFlow)
4. QA → Phase 7.1 (ReleaseGateFlow71) — QA last (absolute)

DecisionCount: 4
TransitionType: feature_addition (IMPLEMENT present)

CapabilityGapWarnings "deployment" and "go-live" do not generate routing decisions.

---

### LAYER 0-C: Knowledge Edge

Knowledge edges:
- "service" fires IMPLEMENT (rule 2)
- "criteria" fires DOCUMENT (rule 3)
- "deployment" is gap (rule 4)
- "go-live" is gap (rule 5)
- "security" + "audit" → SECURITY co-fires (rule 19)
- Four predicates: no knowledge_edge (predicates fired)

---

### LAYER 0-D: Flow Factory

Factory creates four flows:
1. BackendServiceFlow41 (Phase 4.1, order 1)
2. DocumentationFlow (Phase 2.1, order 2)
3. SecurityAuditFlow (Phase 3.4, order 3)
4. ReleaseGateFlow71 (Phase 7.1, order 4 — QA last)

Gate B: not required for all four flows (deterministic_generic)
scenario_transition_type: feature_addition

---

### LAYER 1: Transition Orchestrator

Orchestration (canonical four-flow order: IMPLEMENT → DOCUMENT → SECURITY → QA):
1. BackendServiceFlow41: payment service reviewed; build artifact examined
   → payment_service_review.md, implementation_audit.md
2. DocumentationFlow: release criteria documented
   → release_criteria_specification.md (references service review)
3. SecurityAuditFlow: security controls audited; compliance checked
   → security_posture_report.md, compliance_summary.md
4. ReleaseGateFlow71 (QA — last): all prior outputs as evidence; gate decision made
   → release_gate_decision.md

TransitionType: feature_addition
Gap warnings: "deployment" and "go-live" attached to orchestration record

---

### LAYER 2: Nine-Node Flow (7.1)

N01: ["QA","IMPLEMENT","DOCUMENT","SECURITY"] confirmed; gap warnings: deployment, go-live
N02: Four routing decisions
N03-N08: All four flows executed in canonical order
N09 Result:
- TransitionType: feature_addition
- Files: [payment_service_review.md, implementation_audit.md, release_criteria_specification.md, security_posture_report.md, compliance_summary.md, release_gate_decision.md]
- FileCount: 6
- ResultState: full_release_gate_passed
- DecisionCount: 4
- CapabilityGapWarnings: ["deployment", "go-live"]

---

### LAYER 3: Teaching Points

1. Four predicates from one input sentence.
2. Each predicate fires from a different vocabulary domain (service/criteria/security/review).
3. Two gap warnings co-exist with four active predicates (gap warnings do not reduce predicate set).
4. QA is absolutely last — the gate decision benefits from all prior phase outputs.
5. feature_addition because IMPLEMENT (service) is present.
6. DecisionCount = 4 = len(PredicateSet).
7. FileCount = 6 (most files of any single-scenario B38 output).
8. The full four-predicate scenario is the richest B38 path.
