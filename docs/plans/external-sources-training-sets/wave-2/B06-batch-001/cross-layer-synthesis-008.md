# Cross-Layer Synthesis (008)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## Synthesis note

File 008 traces the full layer-by-layer execution for the maximum four-predicate scenario
with a new domain set:
"Write the fraud detection requirements, design the architecture for the risk engine,
implement the fraud scoring service, and document the test specifications."
This fires DOCUMENT + ARCHITECTURE + IMPLEMENT + (TEST variant of DOCUMENT),
producing routing to 2.1 + 3.1 + 4.1 with DOCUMENT picking up test spec authoring in 2.1.

---

## Synthesis Trace — ARCHITECTURE + IMPLEMENT + DOCUMENT four-predicate sweep

```
source_id: xiigen-b06-synthesis-008
pair_kind: cross_layer_synthesis
scenario: "FraudDetectionFullCycle — DOCUMENT + ARCHITECTURE + IMPLEMENT (new domain)"
```

### Input
UserRequest: "Write the fraud detection requirements, design the architecture for the risk engine, implement the fraud scoring service, and document the test specifications."
TransitionId: "transition-B06-synthesis-008"
Domain subjects: fraud detection, risk engine, fraud scoring, test specifications

---

### Layer 0-A — Compiler

Layer: 0-A
Unit: PredicateCompilerUnit
Input: the full request above

Scan results:
  "Write" → IMPLEMENT (Rule I-01: write fires IMPLEMENT)
  "fraud" → not in any lexicon (domain noun)
  "detection" → not in any lexicon (domain noun)
  "requirements" → DOCUMENT (Rule 3: requirements fires DOCUMENT)
  "design" → ARCHITECTURE (Rule A-01: design fires ARCHITECTURE)
  "the" → not in any lexicon
  "architecture" → ARCHITECTURE (Rule A-02: architecture fires ARCHITECTURE)
  "for" → not in any lexicon
  "risk" → not in any lexicon (domain noun)
  "engine" → not in any lexicon (domain noun)
  "implement" → IMPLEMENT (Rule I-04: implement fires IMPLEMENT)
  "fraud" → (already scanned — domain noun, no predicate)
  "scoring" → not in any lexicon (domain noun)
  "service" → not in any lexicon (domain noun)
  "and" → not in any lexicon
  "document" → DOCUMENT (Rule 1: document fires DOCUMENT)
  "test" → not in any lexicon (not a DOCUMENT term in current lexicon)
  "specifications" → DOCUMENT (Rule 2: spec/specification fires DOCUMENT)

DOCUMENT signals: ["requirements", "document", "specifications"]
ARCHITECTURE signals: ["design", "architecture"]
IMPLEMENT signals: ["Write", "implement"]

Output:
  CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]
  Confidence: 0.95
  KnowledgeEdge: false

Key teachings:
  "Write ... requirements" fires both IMPLEMENT (from "Write") and DOCUMENT (from "requirements").
  "design the architecture" fires both ARCHITECTURE tokens independently.
  "implement" fires IMPLEMENT again (redundant but correct — both IMPLEMENT signals recorded).
  "test" is NOT a DOCUMENT term in the current lexicon; "specifications" IS a DOCUMENT term.
  Three predicates — not four. "test" alone is not sufficient for a TEST predicate in B06.
  Domain nouns (fraud, detection, risk, engine, scoring) add zero predicates.
  "specifications" = "spec/specification" — Rule 2, fires DOCUMENT.

v6-01 risk: DOCUMENT missed from "requirements" or "specifications".
v6-02 risk: IMPLEMENT suppressed because "write" precedes "requirements".
v6-03 risk: ARCHITECTURE routing missed when "design + architecture" both appear.

Reference: layer-0a-compiler-pairs-024.md

---

### Layer 0-B — Router

Layer: 0-B
Unit: PredicateRouterUnit
Input: CompiledPredicateIds: ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"]

Routing logic:
  DOCUMENT → 2.1
  ARCHITECTURE → 3.1
  IMPLEMENT → 4.1
  No "stakeholder" → 2.4 NOT triggered
  No "UI" → 1.1 NOT triggered
  DecisionCount: 3

Output:
  RoutingDecisions:
    - SubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    - SubPhaseId: "3.1"; EvidencePredicates: ["ARCHITECTURE"]
    - SubPhaseId: "4.1"; EvidencePredicates: ["IMPLEMENT"]

CanonicalOrdering: 2.1 → 3.1 → 4.1

Key teaching:
  Three predicates → three routing decisions → three concurrent flows.
  No stakeholder term → 2.4 absent.
  No UI term → 1.1 absent.
  This is the DOCUMENT + ARCHITECTURE + IMPLEMENT three-predicate combination.

Reference: layer-0b-router-pairs-024.md

---

### Layer 0-D — Flow Factory

Layer: 0-D
Unit: ILogicalFlowFactory

Input:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "3.1", SubPhaseId: "4.1"]

Output:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
    FallbackUsed: false
    Warnings: []

  Flow_3_1:
    SelectedFlowId: "universal-nine-node-development-flow-3.1"
    FlowAlias: "ArchitectureFlow31"
    TrustState: "trusted_learned"
    FallbackUsed: false
    Warnings: []

  Flow_4_1:
    SelectedFlowId: "universal-nine-node-development-flow-4.1"
    FlowAlias: "ImplementationFlow41"
    TrustState: "trusted_learned"
    FallbackUsed: false
    Warnings: []

Key teaching:
  ILogicalFlowFactory always produces a valid flow — never null, never halts.
  Three routing decisions → three distinct flows.
  All three are trusted_learned. No fallback required.

Reference: layer-0d-flow-factory-pairs-024.md

---

### Layer 1 — Transition Orchestrator

Layer: 1
Unit: LogicalTransitionOrchestratorUnit

TransitionType determination:
  IMPLEMENT present → feature_addition (unconditional override)
  ARCHITECTURE present → would be documentation if alone, but IMPLEMENT overrides
  DOCUMENT present → would be documentation if alone, but IMPLEMENT overrides
  All three predicates present → feature_addition on ALL transitions

Output:
  Transition_2_1:
    TransitionType: "feature_addition"
    DomainId: "software-development"
    SubPhaseId: "2.1"
    SelectedFlowId: "universal-nine-node-development-flow-2.1"

  Transition_3_1:
    TransitionType: "feature_addition"
    DomainId: "software-development"
    SubPhaseId: "3.1"
    SelectedFlowId: "universal-nine-node-development-flow-3.1"

  Transition_4_1:
    TransitionType: "feature_addition"
    DomainId: "software-development"
    SubPhaseId: "4.1"
    SelectedFlowId: "universal-nine-node-development-flow-4.1"

Key teaching:
  IMPLEMENT presence forces feature_addition on ALL transitions, including 2.1 and 3.1.
  DOCUMENT+ARCHITECTURE alone would be documentation — but IMPLEMENT changes it.
  DomainId: software-development on all three transitions (Rule 17).
  This propagation rule is unconditional: IMPLEMENT anywhere = feature_addition everywhere.

Reference: layer-1-transition-orchestrator-pairs-024.md

---

### Layer 2 — Three Concurrent Nine-Node Flows

Flow_2_1 (RequirementsAuthoringFlow21) — Fraud Detection Requirements + Test Specs:
  N01 input: SelectedLogicalTransition (TransitionType: feature_addition, SubPhaseId: 2.1)
  N02: Decompose into fraud detection requirements scope
       Scope includes: fraud detection requirements AND test specifications (both in 2.1 scope)
  N03: Plan: fraud rule requirements, chargeback detection rules, test specification outline
  N04: Domain processor — fraud detection domain (software-development)
  N05: Apply requirements authoring rules
  N06: Translate to RequirementsDoc format (requirements + test spec sections)
  N07: Select arbiter for requirements and test spec quality validation
  N08: Arbiter validates completeness of both requirements and test specifications
  N09 Verdict: PASS
  Artifacts: FraudDetectionRequirementsDoc + TestSpecificationsDoc
  Note: "test specifications" authoring is in 2.1 scope (DOCUMENT) — not a separate routing decision

Flow_3_1 (ArchitectureFlow31) — Risk Engine Architecture:
  N01 input: SelectedLogicalTransition (TransitionType: feature_addition, SubPhaseId: 3.1)
  N02: Decompose into risk engine architecture scope
  N03: Plan: RiskEngine component diagram, FraudScoring service interface, data flow ADR
  N04: Domain processor — risk engine architecture (software-development)
  N05: Apply architecture authoring rules
  N06: Translate to ArchitectureDoc format (component diagrams, ADRs)
  N07: Select arbiter for architecture quality validation
  N08: Arbiter validates architectural completeness and decision record quality
  N09 Verdict: PASS
  Artifacts: RiskEngineArchitectureDoc, FraudScoringServiceADR

Flow_4_1 (ImplementationFlow41) — Fraud Scoring Service Implementation:
  N01 input: SelectedLogicalTransition (TransitionType: feature_addition, SubPhaseId: 4.1)
  N02: Decompose into fraud scoring service implementation scope
  N03: Plan: FraudScoringEngine, RuleEvaluator, ScoreThresholdManager, AlertDispatcher
  N04: Domain processor — fraud scoring implementation (software-development)
  N05: Apply implementation authoring rules
  N06: Translate to CodeArtifact format
  N07: Select arbiter for implementation quality validation
  N08: Arbiter validates code structure, coverage, and integration points
  N09 Verdict: PASS
  Artifacts: FraudScoringService implementation (4 components), unit tests

All three flows run concurrently. Each flow's nine nodes execute independently.
RequirementsAuthoringFlow21 handles both requirements and test specifications (both are 2.1 scope).

Reference: layer-2-nine-node-2.1-pairs-024.md

---

### Synthesis Summary

Predicates: DOCUMENT + ARCHITECTURE + IMPLEMENT
Routing decisions: 3 (→ 2.1 + 3.1 + 4.1)
TransitionType: feature_addition (IMPLEMENT present — overrides all other transitions)
DomainId: software-development on all three transitions (Rule 17)
Flows: 3 concurrent flows (RequirementsAuthoringFlow21 + ArchitectureFlow31 + ImplementationFlow41)
N09 verdicts: PASS × 3
Canonical ordering: 2.1 → 3.1 → 4.1

Key teachings:
  "Write ... requirements" fires both IMPLEMENT (write) and DOCUMENT (requirements)
  "design the architecture" fires both ARCHITECTURE tokens — two signals, one predicate
  "test" alone is NOT a DOCUMENT term; "specifications" IS a DOCUMENT term (Rule 2)
  IMPLEMENT overrides TransitionType to feature_addition on ALL transitions unconditionally
  DOCUMENT+ARCHITECTURE+IMPLEMENT → 3 routing decisions → 3 concurrent flows
  RequirementsAuthoringFlow21 is domain-agnostic: works for fraud detection same as checkout
  Test specification authoring stays in 2.1 scope (DOCUMENT) — no separate routing decision
  Domain nouns (fraud, detection, risk, engine, scoring) add zero predicates

Corruption checklist:
  v6-01: Do not miss DOCUMENT despite strong IMPLEMENT+ARCHITECTURE presence
  v6-01: Do not miss "specifications" as a DOCUMENT term
  v6-02: Do not suppress IMPLEMENT from "Write" when "requirements" follows it
  v6-03: Do not route 3.1 or 4.1 before 2.1 (canonical ordering violation)
  v6-03: Do not use "documentation" TransitionType when IMPLEMENT is present
  v6-03: Do not add extra sub-phase for "fraud detection" domain (2.1 only)

---

### Full Cross-Layer Synthesis Comparison Table (Syntheses 001–008)

```
source_id: xiigen-b06-synthesis-comparison-full
pair_kind: cross_layer_synthesis_summary
```

Synthesis | Scenario                           | Predicates              | Routing Decisions    | TransitionType   | Flows
----------|------------------------------------|-------------------------|----------------------|------------------|------
001       | DOCUMENT alone (checkout)          | DOCUMENT                | 1 (→ 2.1)            | documentation    | 1
002       | DOCUMENT + IMPLEMENT (checkout)    | DOCUMENT, IMPLEMENT     | 2 (→ 2.1, 4.1)       | feature_addition | 2
003       | DOCUMENT + ARCHITECTURE            | DOCUMENT, ARCHITECTURE  | 2 (→ 2.1, 3.1)       | documentation    | 2
004       | All four (checkout redesign)       | DOC, ARCH, IMPL, UI     | 4 (→ 2.1, 3.1, 4.1, 1.1) | feature_addition | 4
005       | DOCUMENT + IMPLEMENT (billing)     | DOCUMENT, IMPLEMENT     | 2 (→ 2.1, 4.1)       | feature_addition | 2
006       | Partial DOCUMENT + gap             | DOCUMENT (partial)      | 1 (→ 2.1)            | documentation    | 1 (warning)
007       | DOCUMENT → 2.1 + 2.4 stakeholder   | DOCUMENT                | 2 (→ 2.1, 2.4)       | documentation    | 2
008       | DOC + ARCH + IMPL (fraud)          | DOCUMENT, ARCH, IMPLEMENT | 3 (→ 2.1, 3.1, 4.1) | feature_addition | 3

Key observations:
  1. IMPLEMENT present → feature_addition unconditionally (syntheses 002, 004, 005, 008)
  2. DOCUMENT + ARCHITECTURE without IMPLEMENT → documentation (synthesis 003)
  3. Stakeholder signal adds a routing decision without adding a predicate (synthesis 007)
  4. Partial predicate fires DOCUMENT with reduced confidence but does not halt pipeline (synthesis 006)
  5. Domain nouns never add predicates — only lexicon terms add predicates
  6. Routing decision count ≠ predicate count (stakeholder case: 1 predicate → 2 decisions)
  7. RequirementsAuthoringFlow21 is domain-agnostic: used for all 2.1 routing regardless of domain
  8. DomainId is always software-development regardless of business domain (Rule 17)
