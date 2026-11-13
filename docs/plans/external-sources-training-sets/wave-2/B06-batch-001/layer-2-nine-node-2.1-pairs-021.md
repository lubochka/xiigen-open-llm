# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 2.1 (021)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## Method note

File 021: DataPrivacyRequirements — DOCUMENT+SECURITY framing (DOCUMENT alone).
TransitionType: documentation.

---

## B06-2.1-021-N01 — DomainSplitterUnit: DataPrivacyRequirements

```
source_id: xiigen-b06-2.1-021-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
node_role: node-01-domain-splitter
scenario: DataPrivacyRequirements
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-privacy-021-001"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  PredicateIds: ["DOCUMENT"]
  UserRequest: "Define the data privacy requirements for the checkout flow, covering GDPR consent, PII handling, and data minimization."

B:
  DomainPackets:
    - PacketId: "privacy-021-packet-document"
      DomainTag: "DOCUMENT"
      Focus: "Data privacy requirements: GDPR consent collection, PII field handling, data minimization rules, retention policy, right-to-erasure"
      SubPhaseSignal: "2.1"
  PacketCount: 1

---

## B06-2.1-021-N02 — DomainTranslatorUnit: data privacy statement

```
source_id: xiigen-b06-2.1-021-n02
node_role: node-02-domain-translator
scenario: DataPrivacyRequirements
pair_kind: nine_node_flow
```

B:
  StatementId: "privacy-021-stmt"
  TechnicalStatement: "Data privacy requirements: (1) UC-P001 GDPR (General Data Protection Regulation) consent at checkout: explicit consent for marketing; separate consent for analytics; consent recorded with timestamp; (2) UC-P002 PII (Personally Identifiable Information) field handling: card number masked after entry; address fields not logged in analytics events; (3) UC-P003 Data minimization: only required fields collected; optional fields clearly marked; (4) UC-P004 Right-to-erasure trigger: account deletion request initiates anonymization of checkout history; (5) Business rules: retention period 7 years for financial records; 30 days for analytics events; (6) Acceptance criteria: consent checkbox required before order submission; masked card number shown on confirmation."
  Confidence: 0.92

---

## B06-2.1-021-N03 — SchemePlannerUnit: data privacy branches

```
source_id: xiigen-b06-2.1-021-n03
node_role: node-03-scheme-planner
scenario: DataPrivacyRequirements
pair_kind: nine_node_flow
```

B:
  WorkBranches:
    - BranchId: "privacy-021-uc"; Scope: "4 privacy use cases"; Priority: 1
    - BranchId: "privacy-021-rules"; Scope: "Retention and anonymization rules"; Priority: 2
    - BranchId: "privacy-021-ac"; Scope: "Consent acceptance criteria"; Priority: 3
  BranchCount: 3; Confidence: 0.92

---

## B06-2.1-021-N09 — FormatterUnit: DataPrivacyRequirements complete

```
source_id: xiigen-b06-2.1-021-n09
node_role: node-09-formatter
scenario: DataPrivacyRequirements
pair_kind: nine_node_flow
```

B:
  FlowId: "requirements-authoring-flow-2.1"
  OverallVerdict: "PASS"
  FinalHumanResult: |
    Data privacy requirements (sub-phase 2.1) — TransitionType: documentation:
    UC-P001: GDPR consent — marketing + analytics; recorded with timestamp.
    UC-P002: PII handling — card masked; address excluded from analytics events.
    UC-P003: Data minimization — required fields only; optional fields labeled.
    UC-P004: Right-to-erasure — account deletion → anonymize checkout history.
    Business rules: financial records 7 years; analytics events 30 days.
    Acceptance criteria: consent required before submission; masked card on confirmation.
  Metrics:
    use_case_count: "4"
    transition_type: "documentation"
  Warnings: []
