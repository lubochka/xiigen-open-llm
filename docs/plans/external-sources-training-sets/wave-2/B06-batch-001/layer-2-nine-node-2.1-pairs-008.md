# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 2.1 (008)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## Method note

File 008 covers AccessControlRequirements: roles and access (DOCUMENT+ARCHITECTURE).
4 roles defined. Dual predicate: DOCUMENT routes to 2.1, ARCHITECTURE routes to 3.1.

---

## B06-2.1-008-N01 — Node 01 DomainSplitterUnit: AccessControlRequirements

```
source_id: xiigen-b06-2.1-008-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
transition_type: documentation
phase: 2
sub_phase: 2.1
node_role: node-01-domain-splitter
scenario: AccessControlRequirements
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-B06-access-control-2.1"
  TransitionType: "documentation"
  SubPhaseId: "2.1"
  UserRequest: "Define the user roles and access control requirements for the checkout service, including the design of the permission model."
  PredicateIds: ["DOCUMENT", "ARCHITECTURE"]
  Note: This file covers the 2.1 (DOCUMENT) transition only. The 3.1 (ARCHITECTURE)
    transition is a separate flow not covered in B06 nine-node pairs.

B:
  DomainPackets:
    - PacketId: "B06-2.1-008-packet-document"
      DomainTag: "DOCUMENT"
      Focus: "User roles and access control requirements — 4 roles: Guest, Registered User, Business Account, Admin; permissions per role; checkout access rules"
      UserSignal: "user roles, access control, requirements, permission model"

    - PacketId: "B06-2.1-008-packet-architecture"
      DomainTag: "ARCHITECTURE"
      Focus: "Permission model architecture — role-based access control design"
      UserSignal: "design, permission model"

  PacketCount: 2
  Confidence: 0.92

---

## B06-2.1-008-N02 — Node 02 DomainTranslatorUnit: access roles

```
source_id: xiigen-b06-2.1-008-n02
contract_signature: DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
node_role: node-02-domain-translator
scenario: AccessControlRequirements
pair_kind: nine_node_flow
```

A: [DOCUMENT packet]

B:
  StatementId: "B06-2.1-008-stmt-document"
  TechnicalStatement: "4 user roles for checkout service: (1) Guest: no authentication required; access to: product browsing, cart, guest checkout flow; cannot access: order history, account settings, admin; (2) Registered User: authenticated; access to: all guest permissions + order history + saved addresses + saved payment methods + order management; (3) Business Account: authenticated business entity; extends Registered User with: bulk ordering access, purchase order (PO) payment method, invoicing, multi-user account management; (4) Admin: internal staff; access to: all user permissions + order management dashboard + refund processing + user account management + inventory management. Permission rules: BR-A01 (guest cannot access order history); BR-A02 (business account requires verified business registration); BR-A03 (admin access requires two-factor authentication); BR-A04 (role assignment is single-select; a user cannot be both Registered and Business simultaneously)."
  KeyTerms: ["user role", "Guest", "Registered User", "Business Account", "Admin", "RBAC", "permissions"]
  Confidence: 0.92

---

## B06-2.1-008-N03 — Node 03 SchemePlannerUnit: access role branches

```
source_id: xiigen-b06-2.1-008-n03
contract_signature: SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
node_role: node-03-scheme-planner
scenario: AccessControlRequirements
pair_kind: nine_node_flow
```

B:
  WorkBranches:
    - BranchId: "B06-2.1-008-branch-role-guest"      ; Scope: "Guest role permissions"       ; Priority: 1
    - BranchId: "B06-2.1-008-branch-role-registered" ; Scope: "Registered user permissions"  ; Priority: 2
    - BranchId: "B06-2.1-008-branch-role-business"   ; Scope: "Business account permissions" ; Priority: 3
    - BranchId: "B06-2.1-008-branch-role-admin"      ; Scope: "Admin permissions"            ; Priority: 4
    - BranchId: "B06-2.1-008-branch-rules"           ; Scope: "4 access control rules"       ; Priority: 5

  BranchCount: 5
  Confidence: 0.92

---

## B06-2.1-008-N09 — Node 09 FormatterUnit: AccessControlRequirements complete

```
source_id: xiigen-b06-2.1-008-n09
contract_signature: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
node_role: node-09-formatter
scenario: AccessControlRequirements
pair_kind: nine_node_flow
```

B:
  FlowId: "requirements-authoring-flow-2.1"
  OverallVerdict: "PASS"
  FinalHumanResult: |
    Access control requirements for checkout service (sub-phase 2.1):

    4 roles defined:
    ROLE-01 Guest: product browsing, cart, guest checkout. No history or account access.
    ROLE-02 Registered User: Guest permissions + order history, saved addresses/payments,
      order management.
    ROLE-03 Business Account: Registered User permissions + bulk orders, PO (Purchase Order)
      payment, invoicing, multi-user management. Requires verified registration (BR-A02).
    ROLE-04 Admin: all permissions + order dashboard, refund processing, user management,
      inventory. Requires 2FA (Two-Factor Authentication) (BR-A03).

    4 access control business rules (BR-A01 through BR-A04).
    Note: ARCHITECTURE predicate also fired — permission model architecture is documented
    in the 3.1 (Technical Architecture) flow output (out-of-scope for B06 2.1 pairs).
    Arbiter reviews: PASS (4 roles complete), PASS (rule atomicity).

  Metrics:
    role_count: "4"
    business_rule_count: "4"
    dual_predicate: "DOCUMENT+ARCHITECTURE — 2.1 and 3.1 transitions produced"
  Warnings: []
