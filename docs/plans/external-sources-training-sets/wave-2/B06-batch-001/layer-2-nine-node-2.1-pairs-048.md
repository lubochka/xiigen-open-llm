# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 2.1 (048)
# Batch: B06-batch-001 | Scenario: B06 — E-commerce business rules and order management
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## B06-2.1-048-N01 — DomainSplitterUnit: Round3TransitionRef requirements scenario

```
source_id: xiigen-b06-2.1-048-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
node_role: node-01-domain-splitter
scenario: Round3TransitionRefRequirements
pair_kind: nine_node_flow
```

A (Node 01 input — always SelectedLogicalTransition from Layer 1):
  Type: "SelectedLogicalTransition"
  TransitionId: "transition-Round3TransitionRef-048"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  DomainId: "software-development"
  UserRequest: "Define the Round3TransitionRef requirements."

B:
  DomainPackets:
    - PacketId: "round3transitionref-packet-048"; DomainTag: "DOCUMENT"; SubPhaseSignal: "2.1"
  Rule: "N01 always receives SelectedLogicalTransition from Layer 1, not the raw user request."

---

## B06-2.1-048-N02 — DomainTranslatorUnit: converts DomainPacket to TechnicalStatement

```
source_id: xiigen-b06-2.1-048-n02
node_role: node-02-domain-translator
scenario: Round3TransitionRefRequirements
pair_kind: nine_node_flow
```

Input: DomainPacket with DomainTag: "DOCUMENT", SubPhaseSignal: "2.1"

Output:
  TechnicalStatement:
    Content: "Round3TransitionRef requirements — use cases, acceptance criteria, business rules"
    KeyTerms: ["Round3TransitionRef", "requirements", "business rules"]
    Confidence: 0.92

---

## B06-2.1-048-N09 — FormatterUnit: nine-node chain summary

```
source_id: xiigen-b06-2.1-048-n09
node_role: node-09-formatter
scenario: Round3TransitionRefRequirements
pair_kind: nine_node_flow
```

B:
  FlowId: "requirements-authoring-flow-2.1"
  OverallVerdict: "PASS"
  NineNodeChainSummary:
    N01: DomainSplitterUnit — receives SelectedLogicalTransition; splits into DomainPackets
    N02: DomainTranslatorUnit — converts DomainPacket to TechnicalStatement
    N03: SchemePlannerUnit — creates WorkBranches for Round3TransitionRef requirements scope
    N04: DomainProcessorUnit — processes each branch; writes use cases and acceptance criteria
    N05: ApplierUnit — consolidates processed results into RequirementsDocument
    N06: ResultTranslatorUnit — formats in UC-NNN format with Given/When/Then criteria
    N07: ArbiterSelectorUnit — selects RequirementsCompletenessArbiter
    N08: ArbiterUnit — validates >= 2 use cases; >= 1 criterion; business rules present
    N09: FormatterUnit — produces final HumanResult with OverallVerdict
  InvariantRules:
    "N01 A = SelectedLogicalTransition from Layer 1 (not raw user request)"
    "N09 OverallVerdict = PASS only if all ArbiterUnit checks pass"
    "SourceDocumentId propagated through all 9 nodes"
  Metrics:
    transition_type: "documentation"
    domain: "Round3TransitionRef"
    DomainId: "software-development"
  Warnings: []
