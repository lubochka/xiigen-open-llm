# Layer 2 — Nine-Node 4.3 Pairs (022)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-022-001 — AvalaraTaxConnector: nine-node IntegrationConnectorFlow43

**ConnectorName:** AvalaraTaxConnector
**Input predicates:** ["IMPLEMENT", "API_CONTRACT"]
**TransitionType:** feature_addition

### N01 — DomainSplitterUnit
```
Task: "Build the Avalara tax connector and implement the tax calculation, commit, and void transaction API endpoints."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
```

### N02 — DomainTranslatorUnit
```
Connector: AvalaraTaxConnector
API: Avalara AvaTax REST API v2
Scope: Three handlers — TaxCalculationHandler, TransactionCommitHandler, TransactionVoidHandler
Authentication: Basic auth (username:password base64) or bearer token
```

### N03 — SchemePlannerUnit
```
IAvalaraTaxConnector:
  - CalculateTaxAsync(request: TaxRequest) → OperationResult<TaxResult>
  - CommitTransactionAsync(transactionId: string) → OperationResult<CommitResult>
  - VoidTransactionAsync(transactionId: string, reason: VoidReason) → OperationResult<VoidResult>
```

### N04 — DomainProcessorUnit
```
Dependencies: HttpClient (injected)
Config: AvalaraAccountId, AvalaraLicenseKey, AvalaraCompanyCode, AvalaraApiBaseUrl
```

### N05 — ApplierUnit
```
Files:
1. AvalaraTaxConnector.cs
2. IAvalaraTaxConnector.cs
3. AvalaraModels.cs (TaxRequest, TaxResult, CommitResult, VoidResult, VoidReason)
4. AvalaraConfig.cs
```

### N06 — ResultTranslatorUnit
```
CalculateTaxAsync: POST /api/v2/transactions/create (type: SalesOrder for estimate, SalesInvoice for commit)
CommitTransactionAsync: POST /api/v2/companies/{companyCode}/transactions/{transactionCode}/commit
VoidTransactionAsync: POST /api/v2/companies/{companyCode}/transactions/{transactionCode}/void
```

### N07 — ArbiterSelectorUnit
```
ArbiterRequests:
  - completeness: 3 handlers implemented
  - transaction-type: SalesOrder vs SalesInvoice distinction correct
```

### N08 — ArbiterUnit
```
Verdict: PASS
Findings:
  - CalculateTaxAsync: SalesOrder estimate and SalesInvoice commit — PASS
  - CommitTransactionAsync: commit endpoint called correctly — PASS
  - VoidTransactionAsync: void with reason code — PASS
```

### N09 — FormatterUnit
```
AvalaraTaxConnector complete.
Files generated: 4
ConnectorName: AvalaraTaxConnector
SubPhase: 4.3
```

---

## B17-L2-022-002 — Gate B for AvalaraTaxConnector

**Gate B:** Three handlers implemented, SalesOrder vs SalesInvoice transaction type distinction correct, void with reason code present, filesGenerated >= 3.

**Assertion:** N08 verdict = PASS AND filesGenerated >= 3 AND transactionTypeDistinctionCorrect = true.
