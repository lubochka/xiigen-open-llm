# Layer 0-A — Compiler Pairs (031)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-031-001 — WalletService: IMPLEMENT alone

A: "Build a WalletService that manages user account balances."
B: Predicates: ["IMPLEMENT"] | feature_addition | DecisionCount: 1
"build" → IMPLEMENT. "account balances" is domain context.

---

## B15-compiler-031-002 — WalletService: IMPLEMENT+SECURITY

A: "Build a WalletService and protect all balance modification operations."
B: Predicates: ["IMPLEMENT", "SECURITY"] | feature_addition | DecisionCount: 2
"build" → IMPLEMENT. "protect" → SECURITY.

---

## B15-compiler-031-003 — WalletService: IMPLEMENT+ARCHITECTURE

A: "Build a WalletService and define the wallet bounded context."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | feature_addition | DecisionCount: 2
"build" → IMPLEMENT. "bounded context" → ARCHITECTURE.

---

## B15-compiler-031-004 — WalletService: IMPLEMENT+TEST

A: "Build a WalletService and create integration tests for the deposit flow."
B: Predicates: ["IMPLEMENT", "TEST"] | feature_addition | DecisionCount: 2
"build" → IMPLEMENT. "create integration tests" → TEST (sub-phase 5.1).

---

## B15-compiler-031-005 — WalletService: IMPLEMENT+SECURITY+TEST

A: "Build a WalletService, harden the withdrawal endpoint, and write unit tests."
B: Predicates: ["IMPLEMENT", "SECURITY", "TEST"] | feature_addition | DecisionCount: 3
"build" → IMPLEMENT. "harden" → SECURITY. "write unit tests" → TEST. Three predicates.
