# Layer 2 — Nine-Node 4.3 Pairs (011)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: nine-node-4.3

---

## B17-L2-011-001 — DocuSignConnector: IMPLEMENT + API_CONTRACT + DOCUMENT

**ConnectorName:** DocuSignConnector
**Input predicates:** ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
**TransitionType:** feature_addition

### N01 — Input node
```
Task: "Build the DocuSign e-signature connector, document the signing workflow API interface, and QA the end-to-end signing flow."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT", "QA"]
SubPhase: 4.3 (IMPLEMENT), 3.3 (API_CONTRACT), 2.1 (DOCUMENT), 7.x (QA)
```

### N02 — Domain statement
```
Connector: DocuSignConnector
API: DocuSign eSignature REST API v2.1
Scope: Create envelope, send for signature, track status, handle webhook (Connect) callback
Auth: OAuth 2.0 JWT (JSON Web Token) Bearer Grant (service account)
```

### N03 — Interface contract
```
IDocuSignConnector:
  - CreateAndSendEnvelope(document: DocumentPayload, signers: Signer[])
    → OperationResult<EnvelopeResult>
  - GetEnvelopeStatus(envelopeId: string) → OperationResult<EnvelopeStatus>
  - HandleConnectWebhook(payload: string, hmacKey: string) → OperationResult<ConnectEvent>
  - DownloadSignedDocument(envelopeId: string, documentId: string) → OperationResult<byte[]>
```

### N04 — Dependency resolution
```
Dependencies: HttpClient, IDocuSignAuthService (JWT grant), IDocuSignConfig
Config: AccountId, IntegrationKey, RsaPrivateKey (stored in secrets manager), WebhookHmacKey
```

### N05 — Implementation plan
```
Files:
1. DocuSignConnector.cs
2. IDocuSignConnector.cs
3. DocuSignJwtAuthService.cs — JWT Bearer grant token generation
4. DocuSignModels.cs
5. DocuSignConfig.cs
6. DocuSignConnector.md — signing workflow documentation
7. DocuSignAPI-contract.yaml — OpenAPI specification
```

### N06 — Implementation execution
```
CreateAndSendEnvelope: POST /v2.1/accounts/{accountId}/envelopes
GetEnvelopeStatus: GET /v2.1/accounts/{accountId}/envelopes/{envelopeId}
HandleConnectWebhook: Validate HMAC signature, parse event type
DownloadSignedDocument: GET /v2.1/accounts/{accountId}/envelopes/{envelopeId}/documents/{documentId}
```

### N07 — Validation
```
Validate: JWT grant token generation with RSA private key
Validate: RSA private key stored in secrets manager, not config file
Validate: Connect webhook HMAC validation present
Validate: Documentation includes signing workflow sequence diagram
```

### N08 — Verdict
```
Verdict: PASS
DocuSignConnector complete with envelope management, Connect webhook, and API documentation.
```

### N09 — Output
```
DocuSignConnector + API documentation complete.
Files generated: 7
ConnectorName: DocuSignConnector
SubPhase: 4.3 (primary), 2.1 (documentation generated)
DocumentationIncluded: true
```

---

## B17-L2-011-002 — Gate B for DocuSignConnector

**Gate B:** Four interface methods implemented, RSA key in secrets manager, HMAC webhook validation, documentation includes sequence diagram, filesGenerated >= 6.
