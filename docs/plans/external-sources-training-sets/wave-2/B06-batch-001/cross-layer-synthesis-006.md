# Cross-Layer Synthesis (006)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## Synthesis note

File 006 traces the full layer-by-layer execution for the gap (knowledge-edge) case:
"Plan the sprint for checkout requirements."
This request contains a DOCUMENT term ("requirements") embedded in a gap-verb sentence
("plan the sprint"), producing a partial-predicate DOCUMENT firing and a knowledge-edge
detection at Layer 0-C.

---

## Synthesis Trace — Gap: partial DOCUMENT + knowledge-edge detection

```
source_id: xiigen-b06-synthesis-006
pair_kind: cross_layer_synthesis
scenario: "ProjectPlanningGap — partial DOCUMENT, project-planning capability gap"
```

### Input
UserRequest: "Plan the sprint for checkout requirements."
TransitionId: "transition-B06-synthesis-006"
Domain subject: checkout / sprint planning

---

### Layer 0-A — Compiler

Layer: 0-A
Unit: PredicateCompilerUnit
Input: "Plan the sprint for checkout requirements."

Scan results:
  "Plan" → not in DOCUMENT lexicon; not in IMPLEMENT lexicon
           (project-planning verb; not a recognized predicate trigger)
  "the" → not in any lexicon
  "sprint" → not in any lexicon (Agile term; not a predicate term)
  "for" → not in any lexicon
  "checkout" → not in any lexicon (domain noun)
  "requirements" → DOCUMENT (Rule 3: requirements fires DOCUMENT)

DOCUMENT signals: ["requirements"]
IMPLEMENT signals: [] (none)
ARCHITECTURE signals: [] (none)
UI signals: [] (none)

Output:
  CompiledPredicateIds: ["DOCUMENT"]
  Confidence: 0.61
  KnowledgeEdge: false
  Note: Partial DOCUMENT — single term fires. "Plan" is a gap verb (not a DOCUMENT verb).

Key teaching (partial predicate rule):
  "requirements" fires DOCUMENT even though the dominant verb "plan" is not in any lexicon.
  The compiler scans all tokens; a single DOCUMENT term is sufficient to fire DOCUMENT.
  This is a partial-predicate result: DOCUMENT fires with reduced confidence (0.61 vs 0.95+).

v6-01 risk: "requirements" missed — DOCUMENT not fired despite being present.
  This would produce CompiledPredicateIds: [] (knowledge-edge — no predicates at all).
  Correct result: DOCUMENT fires from "requirements" even in a gap-verb sentence.

Reference: layer-0a-compiler-pairs-018.md

---

### Layer 0-B — Router

Layer: 0-B
Unit: PredicateRouterUnit
Input: CompiledPredicateIds: ["DOCUMENT"]

Routing logic:
  DOCUMENT → 2.1
  No "stakeholder" → 2.4 NOT triggered
  DecisionCount: 1

Output:
  RoutingDecisions:
    - SubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]

Note: Despite "plan" and "sprint" appearing in the request, routing is to 2.1 (business
requirements). The router acts on compiled predicates only — not on raw request text.

Reference: layer-0b-router-pairs-018.md

---

### Layer 0-C — Knowledge-Edge Detector

Layer: 0-C
Unit: KnowledgeEdgeDetectorUnit

Input:
  CompiledPredicateIds: ["DOCUMENT"]
  UserRequest: "Plan the sprint for checkout requirements."
  PartialPredicateFlag: true (confidence below threshold 0.75)

Analysis:
  Capability gap identified: "plan" and "sprint" signal project-planning intent.
  The system handles requirements authoring (2.1), not sprint planning.
  Sprint planning is a project-management capability, not a development flow capability.

Output:
  OverallVerdict: KNOWLEDGE_EDGE_TEACHING
  CapabilityGapId: "project-planning-gap-001"
  Cases:
    - Predicates: ["DOCUMENT"] (from "requirements")
      Result: "DOCUMENT fires (partial); routing proceeds to 2.1"
      CapabilityGap: "sprint-planning: no flow for sprint planning exists in current capability set"
      Recommendation: "System will author the checkout requirements; sprint planning is out of scope"

Key teaching:
  DOCUMENT fires from "requirements" (partial predicate rule) — pipeline does NOT halt.
  The knowledge-edge detector notes the capability gap but does not block execution.
  Routing proceeds to 2.1 for the requirements authoring portion of the request.
  The "plan the sprint" portion is flagged as a capability gap — no sprint planning flow exists.

Reference: layer-0c-knowledge-edge-pairs-018.md

---

### Layer 0-D — Flow Factory

Layer: 0-D
Unit: ILogicalFlowFactory

Input:
  RoutingDecisions: [SubPhaseId: "2.1"]

Output:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
    FallbackUsed: false
    Warnings: ["partial-predicate-low-confidence", "capability-gap-project-planning"]

Key teaching:
  ILogicalFlowFactory always produces a valid flow — never null, never halts.
  Despite partial predicate confidence, RequirementsAuthoringFlow21 is selected.
  Two warnings are propagated to indicate partial confidence and capability gap.

Reference: layer-0d-flow-factory-pairs-018.md

---

### Layer 1 — Transition Orchestrator

Layer: 1
Unit: LogicalTransitionOrchestratorUnit

TransitionType determination:
  DOCUMENT only → documentation
  No IMPLEMENT, no UI, no ARCHITECTURE present

Output:
  Transition_2_1:
    TransitionType: "documentation"
    DomainId: "software-development"
    SubPhaseId: "2.1"
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    Warnings: ["partial-predicate-low-confidence", "capability-gap-project-planning"]

Key teaching:
  DOCUMENT alone → TransitionType: documentation (standard rule, unaffected by capability gap).
  Warnings propagate through the transition but do not change TransitionType.
  DomainId: software-development (Rule 17 — always, regardless of business domain).

Reference: layer-1-transition-orchestrator-pairs-018.md

---

### Layer 2 — Single Nine-Node Flow (Partial Confidence)

Flow_2_1 (RequirementsAuthoringFlow21) — Checkout Requirements (Partial Confidence):
  N01 input: SelectedLogicalTransition (TransitionType: documentation, SubPhaseId: 2.1)
             Warnings include: partial-predicate-low-confidence
  N02: Decompose into checkout requirements scope
       Note: Sprint planning scope is explicitly excluded (capability gap)
  N03: Plan requirements authoring for checkout flow
  N04: Domain processor — checkout business requirements
  N05: Apply requirements authoring rules
  N06: Translate to RequirementsDoc format
  N07: Select arbiter for requirements quality validation
  N08: Arbiter validates with reduced threshold due to partial predicate confidence
  N09 Verdict: PASS (with warning)
  Artifacts: CheckoutRequirementsDoc (reduced scope: requirements only, no sprint plan)
  Note: N09 passes but records capability gap warning in output

Key teaching:
  Pipeline executes fully even for partial-predicate requests.
  The nine-node flow produces requirements artifacts for the in-scope portion.
  Sprint planning artifact is not produced (capability gap).

Reference: layer-2-nine-node-2.1-pairs-018.md

---

### Synthesis Summary

Predicates: DOCUMENT (partial — from "requirements" only)
Routing decisions: 1 (→ 2.1)
TransitionType: documentation
DomainId: software-development (Rule 17)
Flow: 1 flow (RequirementsAuthoringFlow21)
N09 verdict: PASS (with capability gap warning)
Knowledge-edge: KNOWLEDGE_EDGE_TEACHING (sprint planning capability gap flagged)

Key teachings:
  Partial predicate rule: "requirements" fires DOCUMENT even in a gap-verb sentence
  Gap verbs ("plan", "sprint") do not add predicates — they produce capability gap warnings
  DOCUMENT from partial predicate → routing to 2.1, TransitionType: documentation
  Knowledge-edge detection does NOT halt execution — pipeline continues
  ILogicalFlowFactory never returns null, even for partial-predicate cases
  Warnings propagate layer-by-layer but do not change TransitionType or routing
  The in-scope portion (requirements authoring) is fulfilled; gap portion (sprint planning) is flagged

Contrast with full DOCUMENT case:
  Full DOCUMENT ("Document the checkout requirements"): Confidence 0.95+, no warnings, PASS clean
  Partial DOCUMENT ("Plan the sprint for requirements"): Confidence 0.61, 2 warnings, PASS with gap

---

### Comparison: Knowledge-Edge vs Partial Predicate vs Full Predicate

Scenario                            | Predicates | Confidence | KE verdict      | Routing | Flow executes
------------------------------------|------------|------------|-----------------|---------|---------------
Full DOCUMENT ("document the req")  | DOCUMENT   | 0.95       | N/A (not KE)    | 2.1     | Yes, clean
Partial DOCUMENT ("plan the sprint")| DOCUMENT   | 0.61       | KE_TEACHING     | 2.1     | Yes, with warning
True gap ("do the thing")           | []         | 0.00       | KNOWLEDGE_EDGE  | none    | No (empty predicate set)

Key distinction: Partial predicate fires DOCUMENT from a single term — pipeline runs.
True gap fires nothing — pipeline has no routing decisions.
