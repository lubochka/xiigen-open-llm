# Layer 0-B — Router Pairs (021)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-021-001 — IMPLEMENT → ImplementationFlow41

A: Predicates: ["IMPLEMENT"] | Request: "Build the reporting service that generates daily sales summaries and revenue breakdowns."
Router:
  - PredictedSubPhaseId: "4.1" | Confidence: 0.92 | DecisionCount: 1

---

## B15-router-021-002 — IMPLEMENT+DOCUMENT → two flows

A: Predicates: ["IMPLEMENT", "DOCUMENT"] | Request: "Build the reporting service and document the report generation API."
Router:
  - ImplementationFlow41 (IMPLEMENT) — sub-phase 4.1
  - DocumentationFlow21 (DOCUMENT) — sub-phase 2.1
  DecisionCount: 2

---

## B15-router-021-003 — IMPLEMENT+API_CONTRACT → two flows

A: Predicates: ["IMPLEMENT", "API_CONTRACT"] | Request: "Implement the reporting service and define the service contracts for report request and response schemas."
Router:
  - APIContractFlow33 | ImplementationFlow41
  DecisionCount: 2 | Routing note: "service contracts" fires API_CONTRACT on "contract" keyword.

---

## B15-router-021-004 — IMPLEMENT+API_CONTRACT+DOCUMENT → three flows

A: Predicates: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"] | Request: "Build the reporting service, integrate with the analytics data warehouse API, and document the report schemas."
Router:
  - APIContractFlow33 | ImplementationFlow41 | DocumentationFlow21
  DecisionCount: 3

---

## B15-router-021-005 — v6-03 corruption: three routing decisions collapsed to IMPLEMENT-only

v6 corrupt: Predicates: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"] → DecisionCount: 1 (only ImplementationFlow41)
Correct: DecisionCount: 3 — APIContractFlow33 + ImplementationFlow41 + DocumentationFlow21
Error: v6-03. Three predicates fired — router must produce three independent decisions. Selecting only IMPLEMENT is the multi-branch collapse error.
