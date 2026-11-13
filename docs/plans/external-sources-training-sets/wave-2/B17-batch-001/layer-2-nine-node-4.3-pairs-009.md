# Layer 2 — Nine-Node 4.3 Pairs (009)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: nine-node-4.3

---

## B17-L2-009-001 — probationary_rejected: SalesforceConnector candidate fails Gate B

**ConnectorName:** SalesforceConnector
**Scenario:** Candidate flow submitted, Gate B fails, fallback to deterministic_generic

### N01 — Input node
```
Task: "Build the Salesforce CRM connector for contact and opportunity management."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
```

### N02 — Domain statement
```
Connector: SalesforceConnector
API: Salesforce REST API v58.0
Scope: Contact CRUD (Create, Read, Update, Delete), Opportunity management
Auth: OAuth 2.0 Connected App (Username-Password flow or JWT Bearer)
```

### N03 — Interface contract (candidate flow's proposed contract)
```
ISalesforceConnector:
  - CreateContact(contact: SalesforceContact) → OperationResult<string> // returns record Id
  - GetContact(contactId: string) → OperationResult<SalesforceContact>
  - UpdateContact(contactId: string, updates: SalesforceContactUpdate) → OperationResult<bool>
  - CreateOpportunity(opp: SalesforceOpportunity) → OperationResult<string>
```

### N04-N06 — Implementation execution (candidate flow)
```
Candidate generated files:
1. SalesforceConnector.cs — partial (CreateContact, GetContact only; UpdateContact missing body)
2. ISalesforceConnector.cs — interface defined
3. SalesforceModels.cs — DTOs
```

### N07 — Validation (Gate B)
```
Gate B checks:
✓ ISalesforceConnector interface defined
✗ UpdateContact method body missing (empty implementation)
✗ CreateOpportunity not implemented (stub only)
✗ OAuth token refresh not present
Gate B result: FAIL
```

### N08 — Verdict
```
Verdict: PASS (work item passes — the task was executed)
CandidateFlowVerdict: probationary_rejected (Gate B failed for candidate flow)
FallbackActivated: true → deterministic_generic used instead
```

**Important distinction:**
- N08 work item verdict: PASS (the nine-node flow ran to completion, fallback was used)
- Candidate flow status: probationary_rejected (Gate B failed for the candidate)
- These are two separate verdicts

### N09 — Output
```
SalesforceConnector generated via deterministic_generic fallback.
CandidateFlowStatus: probationary_rejected
FallbackUsed: deterministic_generic
Files generated: 4 (deterministic_generic output)
SubPhase: 4.3
```

---

## B17-L2-009-002 — Teaching point: probationary_rejected vs work item PASS

**The nine-node flow always completes.** When a candidate flow is probationary_rejected, the nine-node flow activates the fallback (deterministic_generic) and continues to N09. The work item is completed.

**probationary_rejected means:** The candidate flow's output failed Gate B → it cannot replace deterministic_generic → it remains in probationary_rejected state → future invocations also use fallback.

**The output at N09 is still a valid connector implementation** (from deterministic_generic). The candidate flow failure is a meta-level factory learning event, not an application-level failure.
