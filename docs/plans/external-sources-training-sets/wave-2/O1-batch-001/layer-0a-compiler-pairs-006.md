# Layer 0-A — Compiler Pairs (006)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: compiler

---

## O1-compiler-006-001 — IMPLEMENT+API_CONTRACT from "implement" + "api endpoints"

A: "Implement the REST API endpoints for the tenant provisioning service."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "implement"
  API_CONTRACT matched on: "rest", "api", "endpoints"
  AUTHORIZATION matched on: "tenant"
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-006-002 — IMPLEMENT+API_CONTRACT from "build" + "swagger"

A: "Build the service layer and generate the Swagger contract for each endpoint."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "build"
  API_CONTRACT matched on: "swagger", "contract", "endpoint"
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-006-003 — IMPLEMENT+API_CONTRACT from "create" + "graphql"

A: "Create the GraphQL schema and resolvers for the permissions management API."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "create" (code context — "create the schema and resolvers")
  API_CONTRACT matched on: "graphql", "schema", "api"
  AUTHORIZATION matched on: "permissions"
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-006-004 — IMPLEMENT+API_CONTRACT from "add" + "webhook"

A: "Add webhook delivery endpoints for tenant-scoped authentication events."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "add"
  API_CONTRACT matched on: "webhook", "endpoints"
  SECURITY matched on: "authentication"
  AUTHORIZATION matched on: "tenant"
  TransitionType: feature_addition — IMPLEMENT fires

---

## O1-compiler-006-005 — IMPLEMENT+API_CONTRACT from "scaffold" + "grpc"

A: "Scaffold the gRPC (Remote Procedure Call) interface for the internal auth service."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] | TransitionType: feature_addition

Lexicon evidence:
  IMPLEMENT matched on: "scaffold"
  API_CONTRACT matched on: "grpc", "interface" (API context)
  SECURITY matched on: "auth"
  gRPC expansion: gRPC = Google Remote Procedure Call
  TransitionType: feature_addition — IMPLEMENT fires
