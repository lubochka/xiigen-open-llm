# External Training Rows — Batch 9
## Row IDs: EXT-171 through EXT-190
## Date: 2026-05-04
## Focus: CI/CD, Docker, Kafka, additional CWE entries, React/Angular additional, families 3/4/7/8

---

## EXT-171

```json
{
  "rowId": "EXT-171",
  "sourceIds": ["GITHUB-ACTIONS-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "ci-pipeline-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose this CI requirement: run tests for a Node.js library against Node.js versions 18, 20, and 22 on Ubuntu and Windows. All combinations must pass before the release job can run.",
    "b": "Current workflow: one job that runs 'npm test' on ubuntu-latest with Node.js 20 only. The library needs to be verified against 3 Node.js versions × 2 OS = 6 combinations.",
    "c": "Context: GitHub Actions matrix strategy. A matrix strategy generates one job per combination of the matrix values, running all combinations in parallel. All matrix jobs must succeed before downstream jobs (release) can run. Sequential execution of 6 combinations would take 6 × (test time). Parallel matrix reduces wall time to approximately 1× test time."
  },
  "function": "Produce the action items for converting the single test job to a matrix strategy covering 6 OS/Node.js combinations.",
  "acceptedOutput": {
    "actionItems": [
      "Action 1: Add a matrix strategy to the test job: strategy: { matrix: { node-version: [18, 20, 22], os: [ubuntu-latest, windows-latest] } } runs-on: ${{ matrix.os }}",
      "Action 2: Replace the hardcoded Node.js version with the matrix variable in the setup-node step: with: { node-version: ${{ matrix.node-version }} }",
      "Action 3: The test step is unchanged: npm ci && npm test",
      "Action 4: Make the release job depend on the test matrix completing: needs: [test]. GitHub Actions waits for all matrix job variants to succeed before starting needs-dependent jobs.",
      "Action 5: If a specific combination is expected to fail (known incompatibility), use matrix.exclude to skip it: exclude: [{ os: windows-latest, node-version: 18 }] — document why the exclusion exists."
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Create 6 separate jobs: test-ubuntu-node18, test-ubuntu-node20, test-ubuntu-node22, test-windows-node18, test-windows-node20, test-windows-node22.",
      "Each job has the same steps."
    ]
  },
  "arbiters": ["ciPipelineDesignArbiter"],
  "whyAccepted": "The matrix strategy generates all 6 combinations from a single job definition. Adding a new Node.js version or OS requires changing one value in the matrix array. The 6-job manual approach requires duplicating the job definition 6 times and manually updating all copies when the steps change.",
  "whyRejected": "6 separate job definitions are functionally equivalent but require 6× the maintenance. If the test steps change (new npm script, different caching strategy), all 6 must be updated identically. A matrix strategy guarantees consistent steps across all combinations.",
  "sourceAnchors": [
    {
      "sourceId": "GITHUB-ACTIONS-ADVANCED",
      "url": "https://github.com/actions/runner",
      "section": "Matrix strategy — node-version × os combinations; needs for downstream job dependency",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-172

```json
{
  "rowId": "EXT-172",
  "sourceIds": ["DOCKER-BEST-PRACTICES"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "container-image-optimization",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose this container build requirement: build a Go HTTP API server into a Docker image that is as small as possible. The Go binary is statically compiled. The current image is 850 MB based on golang:1.21.",
    "b": "Current Dockerfile: FROM golang:1.21; WORKDIR /app; COPY . .; RUN go build -o server .; EXPOSE 8080; CMD ['./server'] — single-stage build using the full Go toolchain image.",
    "c": "Context: Docker multi-stage build. The golang:1.21 image includes the Go compiler, standard library source, and OS utilities needed for compilation. The final binary does not need any of these — it only needs the static binary. A multi-stage build uses golang:1.21 as the build stage and scratch (or distroless) as the final stage, copying only the binary. The result is approximately 10–15 MB instead of 850 MB."
  },
  "function": "Produce the action items for converting the single-stage Dockerfile to a multi-stage build with a scratch final stage.",
  "acceptedOutput": {
    "actionItems": [
      "Action 1: Add build flags for a static binary: RUN CGO_ENABLED=0 GOOS=linux go build -o server -ldflags='-w -s' . — CGO_ENABLED=0 produces a statically linked binary with no libc dependency. -ldflags='-w -s' strips debug info and symbol table.",
      "Action 2: Structure the Dockerfile as two stages: FROM golang:1.21 AS builder; WORKDIR /app; COPY go.mod go.sum ./; RUN go mod download; COPY . .; RUN CGO_ENABLED=0 GOOS=linux go build -o server -ldflags='-w -s' .; FROM scratch; COPY --from=builder /app/server /server; EXPOSE 8080; ENTRYPOINT ['/server']",
      "Action 3: Separate 'go mod download' from 'COPY . .' — this caches the module download layer. If only application code changes (not go.mod), Docker reuses the cached module layer. Only add 'COPY go.mod go.sum' first if using go modules.",
      "Action 4: If the application requires TLS certificates (HTTPS), the scratch image has no CA certificates. Add: COPY --from=builder /etc/ssl/certs/ca-certificates.crt /etc/ssl/certs/ before the ENTRYPOINT.",
      "Action 5: Verify the image size: docker build -t server:v1 .; docker image ls server — expected final size: 10–15 MB (binary only) vs 850 MB (full toolchain)."
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Change FROM golang:1.21 to FROM golang:1.21-alpine.",
      "This reduces the image size significantly."
    ]
  },
  "arbiters": ["containerOptimizationArbiter"],
  "whyAccepted": "The accepted decomposition reduces the image to the binary only (10–15 MB). The alpine approach reduces the base image to ~300 MB (golang:alpine includes Go toolchain plus Alpine OS) but does not eliminate the toolchain from the final image. Multi-stage + scratch is the correct approach for Go static binaries.",
  "whyRejected": "golang:1.21-alpine reduces the image from 850 MB to approximately 300–400 MB (Alpine is smaller than Debian but still includes the full Go toolchain in the final image). The requirement is 'as small as possible' — multi-stage + scratch achieves 10–15 MB vs alpine's 300 MB.",
  "sourceAnchors": [
    {
      "sourceId": "DOCKER-BEST-PRACTICES",
      "url": "https://github.com/docker/docs",
      "section": "Multi-stage builds — builder stage with Go toolchain; scratch final stage for static binary",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-173

```json
{
  "rowId": "EXT-173",
  "sourceIds": ["KAFKA-KIPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "kafka-consumer-assignment",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We have a Kafka topic with 12 partitions and a consumer group with 3 consumer instances. We're seeing significant rebalancing when one consumer restarts — all 12 partitions are reassigned to all 3 consumers on every rebalance. Should we change the partition assignment strategy?",
    "b": "Current configuration: partition.assignment.strategy=range (the default). The team experiences 3–5 second rebalancing pauses when any consumer restarts. All consumers subscribe to the same topic.",
    "c": "Context: Kafka consumer group partition assignment strategies. Range assignment: sorts partitions by number, sorts consumers by ID, assigns ranges. On rebalance, all partitions are reassigned from scratch. Sticky assignment (StickyAssignor or CooperativeStickyAssignor): preserves existing assignments when possible; only reassigns partitions from the leaving/joining consumer. CooperativeStickyAssignor supports incremental cooperative rebalancing — non-affected consumers continue consuming during rebalance."
  },
  "function": "Produce the command: which partition assignment strategy to use and what configuration change is required.",
  "acceptedOutput": {
    "command": "Switch to CooperativeStickyAssignor.",
    "rationale": "Three improvements over Range: (1) Stickiness — when one of the 3 consumers restarts, CooperativeStickyAssignor reassigns only that consumer's 4 partitions. The other 2 consumers retain their 8 partitions and continue processing without interruption. (2) Incremental cooperative rebalance — consumers do not stop processing during rebalance; they release and acquire partitions incrementally. The 3–5 second full stop is eliminated. (3) Balanced distribution — 12 partitions ÷ 3 consumers = 4 partitions each, same as Range for uniform consumer count.",
    "configurationChange": "In each consumer's configuration: partition.assignment.strategy=org.apache.kafka.clients.consumer.CooperativeStickyAssignor. This requires all consumers in the group to use the same strategy — update all 3 consumer instances before any rebalance occurs.",
    "migrationNote": "Migrating from Range to CooperativeStickyAssignor requires a rolling upgrade: temporarily set partition.assignment.strategy=org.apache.kafka.clients.consumer.CooperativeStickyAssignor,org.apache.kafka.clients.consumer.RangeAssignor (both strategies) during the rollout, then remove RangeAssignor after all consumers are updated."
  },
  "rejectedOutput": {
    "command": "Switch to StickyAssignor (non-cooperative). It will keep partition assignments stable across rebalances."
  },
  "arbiters": ["kafkaAssignmentArbiter"],
  "whyAccepted": "StickyAssignor is an improvement over Range but still uses an eager rebalance protocol — all consumers stop processing during rebalance. CooperativeStickyAssignor uses incremental rebalance — unaffected consumers continue processing. For a 3–5 second pause elimination goal, CooperativeStickyAssignor is the correct choice.",
  "whyRejected": "StickyAssignor reduces partition movement (stickiness) but does not eliminate the processing pause (eager protocol). The user's problem is the 3–5 second rebalancing pause, not just partition movement. StickyAssignor addresses movement but not the pause.",
  "sourceAnchors": [
    {
      "sourceId": "KAFKA-KIPS",
      "url": "https://github.com/apache/kafka/tree/trunk/proposals",
      "section": "KIP-429 Kafka Consumer Incremental Rebalance — CooperativeStickyAssignor vs eager rebalance",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-174

```json
{
  "rowId": "EXT-174",
  "sourceIds": ["REACT-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "react-memoization",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this React result: a developer wrapped every function in a component with useCallback after reading that 'useCallback prevents unnecessary re-renders'. There are 8 functions in the component, 6 of which are used only inside event handlers (onClick, onChange) and are not passed as props to child components.",
    "b": "const handleClick = useCallback(() => setCount(c => c+1), []); const handleChange = useCallback((e) => setName(e.target.value), []); ... (6 more similar useCallback wrappings) — all 8 functions wrapped with useCallback regardless of whether they are passed to memoized child components.",
    "c": "Context: React useCallback optimization. useCallback is useful when: (1) a function is passed as a prop to a child component wrapped with React.memo, (2) a function is a dependency of a useEffect/useMemo. For functions used only as inline event handlers (not passed to children), useCallback adds overhead without benefit: it creates a memoization closure, stores the callback reference, and runs the dependency comparison on every render. For 6 inline handlers, this is 6 useless closures per render."
  },
  "function": "Apply reactMemoizationArbiter: detect useCallback usage on functions that are not passed to memoized children and not used as effect dependencies. Identify which uses are correct and which are overhead.",
  "acceptedOutput": {
    "decision": "REJECT the blanket useCallback application for 6 of 8 functions. useCallback is unnecessary overhead for event handlers that are not passed as props to memoized child components. The 6 inline event handlers (onClick, onChange) do not need useCallback: creating a new function on each render has negligible cost; re-creating an onClick handler does not cause child re-renders unless the child is wrapped in React.memo and receives the function as a prop.",
    "reprocessInstruction": "Remove useCallback from the 6 inline event handlers. Keep useCallback only for functions that are: (1) passed as props to React.memo-wrapped child components, or (2) used in a useEffect/useCallback/useMemo dependency array. After removal, verify with React DevTools Profiler that the re-render behavior is unchanged — for components whose children are not memoized, removing useCallback has no effect on render frequency."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. useCallback is a performance optimization. More memoization is always better than less."
  },
  "arbiters": ["reactMemoizationArbiter"],
  "whyAccepted": "useCallback is a tool, not a universal improvement. Every useCallback call creates a closure, allocates memory for the memoized reference, and runs a dependency comparison on every render. For functions that are never compared by reference (not passed to memo'd children), this is pure overhead.",
  "whyRejected": "The rejected output applies a universal principle ('more memoization is better') that is incorrect for React. React's documentation explicitly notes that premature memoization adds overhead. The correct guidance is: profile first, memoize where reference equality matters.",
  "sourceAnchors": [
    {
      "sourceId": "REACT-ADVANCED",
      "url": "https://github.com/reactjs/react.dev",
      "section": "useCallback — when to use; inline event handlers do not benefit without React.memo children",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0 — attribution required",
  "trainingStatus": "candidate"
}
```

---

## EXT-175

```json
{
  "rowId": "EXT-175",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "information-exposure",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this API error handling result: a REST API returns detailed exception stack traces in JSON error responses in the production environment. The stack trace includes file paths, line numbers, library version information, and internal function names.",
    "b": "HTTP 500 response body: { 'error': 'NullReferenceException', 'message': 'Object reference not set to an instance of an object.', 'stackTrace': 'at UserService.GetUser(Int32 userId) in /app/src/services/UserService.cs:line 42\\n at OrderController.GetOrder(Int32 id) in /app/src/controllers/OrderController.cs:line 18\\n at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor...', 'frameworkVersion': '.NET 8.0.5', 'aspnetVersion': '8.0.5' }",
    "c": "Context: CWE-200 Exposure of Sensitive Information to an Unauthorized Actor (rank 5 in some CWE Top 25 variants). Stack traces in production responses expose: internal file structure and project layout, technology stack and exact version (attackers can target known CVEs for .NET 8.0.5), internal class and method names (useful for targeted attacks), and internal implementation details."
  },
  "function": "Apply informationExposureArbiter: detect stack trace disclosure in production API responses. Enumerate what information is exposed and produce the safe error format.",
  "acceptedOutput": {
    "decision": "REJECT. CWE-200 — stack trace disclosure in production API response exposes: (1) internal file paths and project structure (/app/src/services/UserService.cs — reveals project layout); (2) exact framework versions (.NET 8.0.5, ASP.NET 8.0.5 — allows targeted CVE exploitation if a known vulnerability exists in these exact versions); (3) internal class and method names (UserService.GetUser, OrderController.GetOrder — useful for social engineering and targeted testing); (4) the fact that the error is a NullReferenceException (reveals a programming error that may indicate data validation gaps).",
    "reprocessInstruction": "Production error responses must not include stack traces, framework versions, or internal identifiers. Safe format: { 'error': { 'code': 'INTERNAL_ERROR', 'message': 'An unexpected error occurred. Please contact support if this persists.', 'referenceId': 'err_7f8a2b1c' } }. Log the full stack trace server-side with the referenceId so the support team can correlate. In ASP.NET Core: configure app.UseExceptionHandler() in production and disable app.UseDeveloperExceptionPage() (which is enabled only in Development environment)."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. Stack traces help developers debug production issues. The information is technical and not immediately useful to attackers."
  },
  "arbiters": ["informationExposureArbiter"],
  "whyAccepted": "Stack traces help developers but also provide actionable intelligence to attackers. Exact version information enables targeted CVE exploitation. 'Not immediately useful' is not the standard — CWE-200 applies when sensitive information is disclosed, regardless of the effort required to exploit it.",
  "whyRejected": "The rejected output evaluates from the developer's perspective (useful for debugging). The arbiter evaluates from the attacker's perspective (useful for reconnaissance). Both uses of the information are real — the rejected output ignores one of them.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/200.html",
      "section": "CWE-200 Information Exposure — stack trace in production response; referenceId pattern for safe error correlation",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-176

```json
{
  "rowId": "EXT-176",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "resource-exhaustion",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this API rate limiting result: a public API endpoint accepts a JSON body with an 'items' array. The API processes each item in the array synchronously. There is no validation on the number of items in the array. The endpoint is publicly accessible.",
    "b": "POST /api/process — handler processes request.body.items.forEach(item => processItem(item)). A request with 100,000 items would process 100,000 items synchronously, blocking the event loop for the duration. The API has no array length validation and no request body size limit.",
    "c": "Context: CWE-400 Uncontrolled Resource Consumption (rank 4 in some CWE Top 25 variants). An unbounded input array allows a single request to consume arbitrarily large CPU and memory. On a Node.js server, a 100,000-item synchronous forEach blocks the event loop for all other requests. Multiple concurrent requests of this type exhaust available CPU. This is an amplification attack: one attacker request consumes resources equivalent to thousands of normal requests."
  },
  "function": "Apply resourceConsumptionArbiter: detect unbounded array inputs used in synchronous processing without length or body size validation.",
  "acceptedOutput": {
    "decision": "REJECT. CWE-400 — uncontrolled resource consumption via unbounded array input. A request with 100,000 items consumes CPU proportional to N (linear), memory proportional to N (items held in memory during processing), and blocks the Node.js event loop for the duration of forEach. A single attacker request can cause a denial-of-service for all concurrent users.",
    "reprocessInstruction": "Three controls required: (1) Body size limit: configure express limit option: app.use(express.json({ limit: '1mb' })). This limits the raw request body size, bounding the maximum array size indirectly. (2) Array length validation: const MAX_ITEMS = 100; if (req.body.items.length > MAX_ITEMS) return res.status(400).json({ error: 'Maximum 100 items per request' }); (3) Rate limiting: apply express-rate-limit on this endpoint — even with item limits, rate limiting prevents bursts of maximum-size requests. All three controls are required; each addresses a different attack vector."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. Large requests take longer to process but do not cause errors. The server will return a result eventually."
  },
  "arbiters": ["resourceConsumptionArbiter"],
  "whyAccepted": "'Will return a result eventually' is the attacker's desired outcome. The server processing one 100,000-item request for 30 seconds is 30 seconds of denial-of-service for all other users. CWE-400 does not require that the server crash — it requires that resource consumption is uncontrolled.",
  "whyRejected": "The rejected output evaluates whether the server produces a correct result (it does) but not whether it remains available to other users while processing (it is not). Resource consumption attacks target availability, not correctness.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/400.html",
      "section": "CWE-400 Uncontrolled Resource Consumption — unbounded array input; body size limit and array length validation",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-177

```json
{
  "rowId": "EXT-177",
  "sourceIds": ["OPENAPI-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "B_ROUTING_SELECTION",
  "subdomain": "async-operation-completion",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Arbitrate this API design result: a report generation API endpoint returns 202 Accepted and a job ID. The API documentation instructs clients to poll GET /reports/{jobId}/status every 5 seconds until status is 'complete'. The report generation takes 30–90 seconds.",
    "b": "POST /reports → 202 { 'jobId': 'job_abc123' }. GET /reports/job_abc123/status → { 'status': 'processing' } or { 'status': 'complete', 'downloadUrl': '...' }. Clients are SDKs and server-side integrations (not browsers).",
    "c": "Context: OpenAPI 3.0 async operation patterns. Polling works but causes unnecessary load: a 30-second job with 5-second polling = 6 status requests per job. If 1,000 jobs run concurrently: 6,000 status requests for work that could be delivered via webhook. Webhooks require the client to expose an endpoint; polling is simpler for clients that cannot receive webhooks. Both patterns must be offered for SDK clients with different deployment models."
  },
  "function": "Apply asyncOperationArbiter: evaluate the polling-only design against the webhook alternative. Produce the recommendation for an API serving both SDK types.",
  "acceptedOutput": {
    "decision": "ACCEPT polling with advisory — but recommend adding webhook support for production-scale clients.",
    "pollingAnalysis": "Polling every 5 seconds for a 30–90s job is: 6–18 requests per job. At 1,000 concurrent jobs: 6,000–18,000 status requests per minute to an endpoint that returns cached state (no computation). This is acceptable at current scale but grows linearly with concurrent job count.",
    "webhookRecommendation": "Add optional webhook notification: POST /reports accepts an optional 'callbackUrl' in the request body. When the report completes, the server POSTs to callbackUrl with { 'jobId': '...', 'status': 'complete', 'downloadUrl': '...' }. Clients that provide a callbackUrl do not need to poll. Clients without a callbackUrl continue polling as before. OpenAPI 3.0 models this as a Callback Object on the POST /reports operation.",
    "reprocessInstruction": "No immediate change required for the polling path. Add the callback URL parameter in the next API version: POST /reports body: { reportParams: {...}, callbackUrl?: string }. Document both interaction patterns in the OpenAPI spec."
  },
  "rejectedOutput": {
    "decision": "REJECT polling. It generates unnecessary server load. Switch to webhooks immediately."
  },
  "arbiters": ["asyncOperationArbiter"],
  "whyAccepted": "Polling and webhooks serve different client deployment models. SDK clients running in serverless functions or behind firewalls cannot receive webhooks — polling is their only option. Mandating webhooks immediately would break these clients. The accepted response adds webhook support as an opt-in enhancement without removing polling.",
  "whyRejected": "The rejected output mandates webhooks without acknowledging that some SDK clients cannot receive them. 'Immediately' forces a breaking change to clients that depend on polling. The load concern is valid but does not justify breaking existing clients.",
  "sourceAnchors": [
    {
      "sourceId": "OPENAPI-SPEC",
      "url": "https://github.com/OAI/OpenAPI-Specification",
      "section": "Callback Object — async operation webhook pattern alongside polling; OpenAPI 3.0 Callback",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-178

```json
{
  "rowId": "EXT-178",
  "sourceIds": ["IETF-RFCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "B_ROUTING_SELECTION",
  "subdomain": "http-error-response-format",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Format this API error result as an RFC 7807 Problem Details response. The error: a user attempted to transfer funds from an account with insufficient balance.",
    "b": "Error context: { errorType: 'INSUFFICIENT_BALANCE', accountId: 'acc_4821', availableBalance: 42.50, requestedAmount: 100.00, currency: 'USD' }",
    "c": "Context: RFC 7807 Problem Details for HTTP APIs (IETF Trust provisions). RFC 7807 defines a standard JSON format for HTTP API error responses: type (URI identifying the problem type), title (human-readable summary), status (HTTP status code), detail (human-readable explanation of this specific occurrence), instance (URI of the specific occurrence). Content-Type must be application/problem+json."
  },
  "function": "Format the error as an RFC 7807 Problem Details response. Include all required fields and the domain-specific extension fields.",
  "acceptedOutput": {
    "httpResponse": {
      "status": 422,
      "headers": { "Content-Type": "application/problem+json" },
      "body": {
        "type": "https://api.example.com/problems/insufficient-balance",
        "title": "Insufficient Balance",
        "status": 422,
        "detail": "The account does not have sufficient funds for this transfer. Available: $42.50 USD. Requested: $100.00 USD.",
        "instance": "/transfers/req_transfer_88421",
        "availableBalance": 42.50,
        "requestedAmount": 100.00,
        "currency": "USD"
      }
    }
  },
  "rejectedOutput": {
    "httpResponse": {
      "status": 400,
      "body": { "error": "INSUFFICIENT_BALANCE", "message": "Not enough funds." }
    }
  },
  "arbiters": ["httpErrorFormatArbiter"],
  "whyAccepted": "The accepted response: uses 422 Unprocessable Entity (correct — the request is well-formed but cannot be processed due to business logic); includes all RFC 7807 fields (type, title, status, detail, instance); uses Content-Type: application/problem+json; includes domain-specific extension fields (availableBalance, requestedAmount) as RFC 7807 allows. The type URI can be followed by a client to read documentation about this problem type.",
  "whyRejected": "The rejected response uses 400 Bad Request (semantically incorrect — the request format is correct, the problem is a business rule violation); the error code 'INSUFFICIENT_BALANCE' is not a URI (not RFC 7807 compliant); 'Not enough funds' provides no actionable numbers. RFC 7807 clients cannot detect this as a Problem Details response because the Content-Type is wrong.",
  "sourceAnchors": [
    {
      "sourceId": "IETF-RFCS",
      "url": "https://www.rfc-editor.org/rfc/rfc7807",
      "section": "RFC 7807 Problem Details — type URI, title, status, detail, instance; application/problem+json",
      "license": "IETF Trust provisions"
    }
  ],
  "licenseConstraint": "IETF Trust provisions — document text usable for training",
  "trainingStatus": "candidate"
}
```

---

## EXT-179

```json
{
  "rowId": "EXT-179",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "function-implementation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Implement: safe_json_loads(raw: str) -> tuple[dict | list | None, str | None]. Parses raw as JSON. Returns (parsed_value, None) on success. Returns (None, error_message) on failure. Never raises an exception. Handles: valid JSON objects, valid JSON arrays, invalid JSON syntax, empty string, and None input.",
    "b": "Test: safe_json_loads('{\"key\": 1}') == ({'key': 1}, None). safe_json_loads('[1,2,3]') == ([1,2,3], None). safe_json_loads('invalid') == (None, 'invalid JSON: Expecting value: line 1 column 1 (char 0)'). safe_json_loads('') == (None, 'empty input'). safe_json_loads(None) == (None, 'input is None').",
    "c": "Context: BigCodeBench function — safe JSON parser with explicit error tuple. The function must handle None and empty string as distinct from invalid JSON, since they require different error messages. The error message for invalid JSON must include the original json.JSONDecodeError message (not just a generic 'parse error')."
  },
  "function": "Implement safe_json_loads with the tuple return pattern and specific error messages for each failure case.",
  "acceptedOutput": {
    "implementation": "import json\nfrom typing import Union\n\ndef safe_json_loads(raw: Union[str, None]) -> tuple[Union[dict, list, None], Union[str, None]]:\n    if raw is None:\n        return (None, 'input is None')\n    if not isinstance(raw, str) or raw.strip() == '':\n        return (None, 'empty input')\n    try:\n        parsed = json.loads(raw)\n        return (parsed, None)\n    except json.JSONDecodeError as e:\n        return (None, f'invalid JSON: {e}')",
    "notes": "raw.strip() == '' catches both empty string and whitespace-only strings. json.JSONDecodeError inherits from ValueError — catching JSONDecodeError specifically avoids masking other ValueErrors. f'invalid JSON: {e}' uses the str representation of JSONDecodeError which includes 'Expecting value: line 1 column 1 (char 0)'."
  },
  "rejectedOutput": {
    "implementation": "import json\n\ndef safe_json_loads(raw):\n    try:\n        return (json.loads(raw), None)\n    except:\n        return (None, 'parse error')"
  },
  "arbiters": ["implementationCorrectnessArbiter"],
  "whyAccepted": "The accepted implementation: handles None before attempting json.loads (json.loads(None) raises TypeError, not JSONDecodeError); distinguishes empty string from invalid JSON; uses the specific JSONDecodeError message (not generic 'parse error'); uses bare except avoidance.",
  "whyRejected": "The rejected implementation: passes None to json.loads (raises TypeError which is caught by bare except — same return as 'parse error', but the test expects 'input is None'); does not distinguish empty string from invalid JSON; uses bare except; returns generic 'parse error' instead of the JSONDecodeError message.",
  "sourceAnchors": [
    {
      "sourceId": "BIGCODEBENCH",
      "url": "https://github.com/bigcode-project/bigcodebench",
      "section": "Safe JSON parser task — tuple return, None/empty/invalid distinction",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-180

```json
{
  "rowId": "EXT-180",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "priority-ranked-aggregation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Aggregate these four domain results from a code quality scan. Apply priority ranking: security vulnerabilities > correctness errors > performance issues > style warnings. The highest-priority issue class determines the overall status.",
    "b": "sast_result = { status: 'FAIL', findings: [{ type: 'security', severity: 'high', cwe: 'CWE-89', message: 'SQL injection in user search query' }] }; unit_test_result = { status: 'SUCCESS', passed: 144, failed: 0 }; perf_result = { status: 'WARN', findings: [{ type: 'performance', message: 'N+1 query detected in UserRepository.GetAll()' }] }; style_result = { status: 'WARN', findings: [{ type: 'style', rule: 'naming-convention', count: 3 }] }",
    "c": "Context: priority-ranked aggregation. The overall status must reflect the highest-priority issue present: security HIGH → overall status SECURITY_BLOCK. The correctness (tests) passed. Performance and style are advisory. The resultI must make the security blocker immediately visible as the primary concern."
  },
  "function": "Aggregate the four scan results with priority ranking. Produce a resultI where the security finding is the primary issue and the other results are secondary.",
  "acceptedOutput": {
    "resultI": {
      "requestId": "req_code_scan_pr_5512",
      "overallStatus": "SECURITY_BLOCK",
      "priorityLevel": 1,
      "primaryBlocker": {
        "tool": "sast",
        "type": "security",
        "severity": "high",
        "cwe": "CWE-89",
        "message": "SQL injection in user search query",
        "mergeBlocked": true,
        "priorityReason": "Security vulnerabilities override all other status levels"
      },
      "secondaryFindings": [
        { "tool": "unit_tests", "status": "SUCCESS", "passed": 144, "failed": 0, "priority": 2 },
        { "tool": "perf_scan", "status": "WARN", "finding": "N+1 query in UserRepository.GetAll()", "priority": 3, "blocksMerge": false },
        { "tool": "style_scan", "status": "WARN", "finding": "3 naming convention violations", "priority": 4, "blocksMerge": false }
      ],
      "arbitrationTargets": ["codeQualityArbiter"],
      "mergeReady": false
    }
  },
  "rejectedOutput": {
    "resultI": {
      "overallStatus": "WARN",
      "reason": "Security finding detected and style/performance warnings present. Tests passed."
    }
  },
  "arbiters": ["codeQualityArbiter"],
  "whyAccepted": "SECURITY_BLOCK is the correct status for a high-severity security finding — it is distinct from WARN and makes the blocking nature and reason immediately clear. The priority-ranked structure allows downstream handlers to display the security finding prominently while still showing the full scan results.",
  "whyRejected": "WARN does not communicate that a security finding blocks merge. A developer seeing 'WARN' may treat it as advisory. CWE-89 SQL injection is a merge blocker in any reasonable CI policy, not a warning to be addressed at leisure.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-BENCH",
      "url": "https://github.com/swe-bench/SWE-bench",
      "section": "Priority-ranked scan result aggregation — security overrides performance and style findings",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-181

```json
{
  "rowId": "EXT-181",
  "sourceIds": ["MICROSOFT-LEARN-ARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "A_TOPOLOGY",
  "subdomain": "retry-storm",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this retry configuration result: a microservice has 10 instances. Each instance calls an upstream dependency with a 3-retry configuration and a fixed 1-second delay between retries. The upstream dependency goes down for 30 seconds.",
    "b": "Retry config: maxRetries=3, delayMs=1000 (fixed, no jitter). When the upstream fails: each of 10 instances retries 3 times at t=1s, t=2s, t=3s after initial failure. For an incoming request rate of 100 requests/second: 100 req/s × 10 instances × 3 retries = 3,000 retry requests hitting the recovering upstream simultaneously.",
    "c": "Context: retry storm / thundering herd. Fixed delays cause all retries to synchronize — all 10 instances retry at exactly t=1s, then t=2s, then t=3s. When the upstream begins recovering, it receives 3,000 simultaneous requests instead of the normal 100/second, potentially causing it to fail again. Jitter distributes retry times randomly, preventing synchronization."
  },
  "function": "Apply retryStormArbiter: detect fixed-delay retry configurations in multi-instance deployments that cause synchronized retry storms. Produce the jitter pattern.",
  "acceptedOutput": {
    "decision": "REJECT fixed delay retry. Synchronized retries in a 10-instance deployment amplify the load on the recovering upstream by 30× (3,000 retries vs 100/s normal). When the upstream recovers, synchronized retries cause a new spike that may prevent stable recovery.",
    "reprocessInstruction": "Add jitter to the retry delay. Two options: (1) Full jitter: delay = random(0, base_delay_ms * 2^attempt). Each instance selects an independent random delay — 10 instances distribute their retries across the delay window. (2) Exponential backoff with jitter: delay = random(0, min(cap_ms, base_delay_ms * 2^attempt)). This caps the maximum delay and adds randomness. Also add a circuit breaker — if retry attempts consistently fail, open the circuit and stop retrying for a recovery period (e.g., 30 seconds). Circuit breakers prevent retry storms independently of jitter."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. Retries with fixed delays are predictable and easy to configure. The upstream will recover between retry attempts."
  },
  "arbiters": ["retryStormArbiter"],
  "whyAccepted": "Predictability in fixed delays is the mechanism of the storm — all instances predict the same retry times. 'The upstream will recover between retry attempts' assumes a 1-instance deployment; 10 instances retrying simultaneously provides no recovery window.",
  "whyRejected": "The rejected output evaluates single-instance retry behavior (predictable, recoverable) and applies it to a 10-instance system. Retry storm is a system-level property that does not appear in single-instance analysis.",
  "sourceAnchors": [
    {
      "sourceId": "MICROSOFT-LEARN-ARCH",
      "url": "https://learn.microsoft.com/en-us/azure/architecture/patterns/retry",
      "section": "Retry pattern — jitter for distributed deployments; thundering herd with fixed delays",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0 — attribution required",
  "trainingStatus": "candidate"
}
```

---

## EXT-182

```json
{
  "rowId": "EXT-182",
  "sourceIds": ["REACT-CORE"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "react-context-performance",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this React Context result: a ThemeContext.Provider is placed at the root of the application and provides the theme value as an object literal: <ThemeContext.Provider value={{ primary: '#007bff', secondary: '#6c757d', mode: 'light' }}>. The provider wraps the entire application.",
    "b": "function App() { return (<ThemeContext.Provider value={{ primary: '#007bff', secondary: '#6c757d', mode: 'light' }}><Router /><ThemeContext.Provider>); } — the value object literal is created on every App render.",
    "c": "Context: React Context re-render behavior. When a Context value changes (including reference changes), all consumers re-render. An object literal { ... } creates a new object reference on every render even if the values are identical. App renders when any parent state changes — on every App render, a new theme object is created, causing all ThemeContext consumers to re-render unnecessarily."
  },
  "function": "Apply contextValueArbiter: detect Context Provider value objects that are recreated on every render, causing unnecessary consumer re-renders.",
  "acceptedOutput": {
    "decision": "REJECT. The theme value object literal is recreated on every App render. React compares Context values by reference (Object.is). A new object on each render always fails the reference equality check, causing all ThemeContext consumers to re-render on every App render — even when the theme has not changed. For a theme that rarely changes, this causes unnecessary re-renders across all themed components on every unrelated App re-render.",
    "reprocessInstruction": "Two options: (1) Memoize the value: const themeValue = useMemo(() => ({ primary: '#007bff', secondary: '#6c757d', mode: themeMode }), [themeMode]); — the value is recreated only when themeMode changes. (2) Move the theme object to a stable location: const DEFAULT_THEME = { primary: '#007bff', secondary: '#6c757d', mode: 'light' as const }; <ThemeContext.Provider value={DEFAULT_THEME}> — a module-level constant has a stable reference."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The theme values are constant strings. The object has the same values on every render."
  },
  "arbiters": ["contextValueArbiter"],
  "whyAccepted": "React compares by reference, not by value equality. Same values in a new object === new reference === context change === all consumers re-render. The object's values being constant does not prevent the reference from being new.",
  "whyRejected": "The rejected output evaluates value equality (correct — the string values are constant) but applies it to reference equality (incorrect — React uses Object.is for context comparison, which is reference equality for objects).",
  "sourceAnchors": [
    {
      "sourceId": "REACT-CORE",
      "url": "https://github.com/facebook/react",
      "section": "Context — value comparison by reference; useMemo to stabilize object value reference",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-183

```json
{
  "rowId": "EXT-183",
  "sourceIds": ["ANGULAR-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "angular-change-detection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Arbitrate this Angular performance result: a large data grid component with 500 rows uses the default ChangeDetectionStrategy.Default. The component receives a products array as an @Input. The parent component updates an unrelated UI state (sidebar open/closed) every 200ms.",
    "b": "@Component({ selector: 'app-product-grid', changeDetection: ChangeDetectionStrategy.Default }) class ProductGridComponent { @Input() products: Product[]; } — default strategy runs change detection on every event in the application, including the sidebar state changes.",
    "c": "Context: Angular change detection strategies. Default: Angular checks the entire component tree on every application event (mousemove, setTimeout, HTTP response). OnPush: Angular only checks the component when: (1) its Input references change, (2) an event originates from the component or its children, (3) an async pipe emits, or (4) ChangeDetectorRef.markForCheck() is called. For a grid that re-renders 500 rows, running change detection on every sidebar event wastes CPU."
  },
  "function": "Apply changeDetectionArbiter: evaluate whether Default strategy is appropriate for this data grid. Produce the OnPush migration requirements.",
  "acceptedOutput": {
    "decision": "REJECT Default strategy for this component. The data grid runs change detection on every application event including sidebar open/close events that are unrelated to the grid's data. With 500 rows and 200ms sidebar events: change detection runs 5 times per second on the grid, checking 500 row components each time, even when products has not changed.",
    "reprocessInstruction": "Migrate to ChangeDetectionStrategy.OnPush: @Component({ changeDetection: ChangeDetectionStrategy.OnPush }). Prerequisites: (1) products Input must use immutable updates — if the parent pushes a new array reference, OnPush detects the change. If the parent mutates the existing array, OnPush will not detect it. (2) Any async data source must use the async pipe or ChangeDetectorRef.markForCheck(). (3) Template event bindings (click, keyup) on the grid still trigger change detection for the grid component — no change needed there."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. Angular's change detection is optimized and runs efficiently for most components."
  },
  "arbiters": ["changeDetectionArbiter"],
  "whyAccepted": "Angular's change detection is fast per-check but the problem is frequency: 5 checks/second × 500 rows × row complexity = measurable CPU cost. 'Optimized' refers to the algorithm efficiency, not to the appropriateness of running the algorithm when inputs have not changed.",
  "whyRejected": "The rejected output applies a general performance claim ('runs efficiently') without evaluating the specific check frequency (5/second from sidebar events) and check scope (500 rows). Efficiency per check does not help when the check is unnecessarily frequent.",
  "sourceAnchors": [
    {
      "sourceId": "ANGULAR-DOCS",
      "url": "https://github.com/angular/angular",
      "section": "Change detection OnPush — triggers: Input reference change, component events, async pipe, markForCheck",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-184

```json
{
  "rowId": "EXT-184",
  "sourceIds": ["TATOEBA"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "multilingual-translation-quality",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Translate the German sentence 'Der Zug hat Verspätung wegen eines technischen Defekts.' into English. Produce the action items for verifying the translation quality.",
    "b": "Source: German. Target: English. The sentence contains: 'hat Verspätung' (has delay / is delayed), 'wegen' (because of / due to), 'eines technischen Defekts' (a technical defect). The word 'Defekt' is a loanword in German from French/Latin, equivalent to 'defect' or 'fault' in English.",
    "c": "Context: Tatoeba German-English corpus (CC-BY 2.0). The translation must: preserve the sentence's passive meaning (the train is the subject affected by the delay), use natural English phrasing for a delay announcement, and select the appropriate English register (neutral/formal, as appropriate for public transport announcements)."
  },
  "function": "Produce the translation and the quality verification action items for each component.",
  "acceptedOutput": {
    "actionItems": [
      "Translate: 'The train is delayed due to a technical fault.'",
      "Verify 1 — tense/voice: 'has delay' (hat Verspätung) translates as 'is delayed' in English (stative passive, present tense). Alternate: 'The train has been delayed' (present perfect, also correct). 'Has delay' is not idiomatic English.",
      "Verify 2 — causal connector: 'wegen' = 'due to' (formal) or 'because of' (informal). 'Due to' is correct for a public transport announcement. 'On account of' is also correct but more formal.",
      "Verify 3 — noun phrase: 'eines technischen Defekts' = 'a technical fault' or 'a technical defect'. Both are acceptable. 'Fault' is more common in British English transport announcements; 'defect' is more common in American English.",
      "Verify 4 — article: 'eines' is the genitive article, not the main noun. The English equivalent is 'a technical fault' (indefinite article). No article is missing in the translation."
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Translate: 'The train has a delay because of technical defect.'",
      "Verify: The meaning is preserved."
    ]
  },
  "arbiters": ["translationQualityArbiter"],
  "whyAccepted": "The accepted translation uses idiomatic English ('is delayed due to') and verifies each component against the German source. 'The train has a delay' is a calque (word-for-word translation) — grammatically correct but not idiomatic in English transport announcements.",
  "whyRejected": "The rejected translation is a direct calque of 'hat Verspätung' → 'has a delay' (unidiomatic) and omits the article before 'technical defect'. The verification step 'meaning is preserved' is not decomposed into falsifiable component checks.",
  "sourceAnchors": [
    {
      "sourceId": "TATOEBA",
      "url": "https://tatoeba.org",
      "section": "German-English sentence pair — train delay with causal connector",
      "license": "CC-BY-2.0"
    }
  ],
  "licenseConstraint": "CC-BY-2.0 — attribution required: Tatoeba (tatoeba.org)",
  "trainingStatus": "candidate"
}
```

---

## EXT-185

```json
{
  "rowId": "EXT-185",
  "sourceIds": ["GITHUB-ACTIONS-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "secret-exposure",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this GitHub Actions workflow result: a deployment workflow logs the deployment configuration before running the deploy script. The configuration echo includes the API key from a GitHub Secret.",
    "b": "- name: Log deployment config\n  run: echo \"Deploying to ${{ secrets.API_ENDPOINT }} with key ${{ secrets.API_KEY }}\"\n- name: Deploy\n  run: ./deploy.sh",
    "c": "Context: GitHub Actions secret handling. GitHub Actions automatically masks known secret values in logs (replaces them with ***). However, the masking applies to the exact secret value as stored. If the secret is modified (URL-encoded, base64-encoded, split across echo arguments), the masking may not recognize the modified form. Additionally, third-party log collection tools, build artifact uploads, or debugging sessions may capture the raw log before masking is applied."
  },
  "function": "Apply secretExposureArbiter: detect explicit echo of secret values in CI workflow steps. Assess the masking reliability and produce the safe pattern.",
  "acceptedOutput": {
    "decision": "REJECT. Explicit echo of ${{ secrets.API_KEY }} in a run step. GitHub Actions masks the exact secret value in logs, but: (1) the echo step creates a command line that includes the secret value — any tool or step that captures command arguments (e.g., process listing, audit logs) may capture the unmasked value; (2) if the secret contains characters that get URL-encoded or shell-escaped in the log output, the masking regex may not match the modified form; (3) third-party CI log aggregators receive the raw log stream and may not apply masking.",
    "reprocessInstruction": "Never echo secret values, even to verify they are set. To verify a secret is present without exposing it: run: '[ -n \"$API_KEY\" ] && echo \"API_KEY is set\" || echo \"ERROR: API_KEY is not set\"' env: API_KEY: ${{ secrets.API_KEY }}. This confirms presence without revealing the value. If you need to log the API endpoint (not the key): echo \"Deploying to ${{ secrets.API_ENDPOINT }}\" — endpoint URLs are generally not secret values."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. GitHub Actions automatically masks secret values in workflow logs. The secrets will appear as *** in the output."
  },
  "arbiters": ["secretExposureArbiter"],
  "whyAccepted": "GitHub's masking is a best-effort control, not a guarantee. The masking applies to the log output visible in the GitHub UI. It does not protect against: process argument capture, third-party log aggregators, build artifact uploads, or masking failures when secrets are transformed. The correct approach is to not echo secrets, not to rely on masking.",
  "whyRejected": "The rejected output presents GitHub's masking as a reliable protection. 'Will appear as ***' is true for the GitHub UI under normal conditions — it is not a security guarantee for all possible log capture paths.",
  "sourceAnchors": [
    {
      "sourceId": "GITHUB-ACTIONS-ADVANCED",
      "url": "https://github.com/actions/runner",
      "section": "Secret masking — echo of secret value is a risk even with masking; presence check pattern",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-186

```json
{
  "rowId": "EXT-186",
  "sourceIds": ["DOCKER-BEST-PRACTICES"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "container-build-efficiency",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this Docker build result: a Node.js application Dockerfile copies the entire source directory before running 'npm install'. Application code changes rebuild the npm install layer on every build.",
    "b": "FROM node:20-alpine\nWORKDIR /app\nCOPY . .\nRUN npm ci\nCMD [\"node\", \"server.js\"] — COPY . . copies all source files including package.json before npm install.",
    "c": "Context: Docker layer caching. Docker caches each instruction as a layer. If a layer's inputs change, that layer and all subsequent layers are invalidated. COPY . . copies all source files — any change to any .js file invalidates the copy layer, which invalidates the npm install layer. package.json and package-lock.json change much less frequently than source code. Separating the two copies lets Docker cache the npm install layer separately from the source code."
  },
  "function": "Apply dockerLayerCachingArbiter: detect COPY patterns that invalidate expensive install layers on every source code change. Produce the optimized layer order.",
  "acceptedOutput": {
    "decision": "REJECT current layer order. COPY . . before npm ci causes npm install to re-run on every source code change. npm ci for a typical Node.js project takes 30–120 seconds. With 10 developers making 5 commits each per day: 50 CI builds × 60 seconds npm install = 50 minutes of unnecessary npm installs per day.",
    "reprocessInstruction": "Separate package files from source code with two COPY instructions: FROM node:20-alpine; WORKDIR /app; COPY package.json package-lock.json ./; RUN npm ci; COPY . .; CMD ['node', 'server.js']. Layer order: (1) copy package.json+lock (changes rarely); (2) run npm ci — cached unless package.json changes; (3) copy source code — changes frequently but does not invalidate the npm install layer. .dockerignore should include node_modules to prevent copying the local modules."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. COPY . . is simpler and copies everything the application needs. npm install will re-run when needed."
  },
  "arbiters": ["dockerLayerCachingArbiter"],
  "whyAccepted": "npm install re-running 'when needed' mischaracterizes the situation — it re-runs on every source code change, which happens on every commit. This is unnecessary for changes that do not modify dependencies. Layer cache optimization is a correctness concern (reproducible builds) and an efficiency concern (CI build time).",
  "whyRejected": "The rejected output evaluates simplicity (correct — COPY . . is one instruction instead of two) without evaluating cache effectiveness (incorrect — the simpler pattern discards the most valuable cached layer on every build).",
  "sourceAnchors": [
    {
      "sourceId": "DOCKER-BEST-PRACTICES",
      "url": "https://github.com/docker/docs",
      "section": "Layer caching — package.json copy before source to cache npm install layer",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-187

```json
{
  "rowId": "EXT-187",
  "sourceIds": ["IETF-RFCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "B_ROUTING_SELECTION",
  "subdomain": "http-response-semantics",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Format this approved DELETE operation result as an HTTP response. The operation: the user deleted their saved address with ID 'addr_4821'. The address was deleted successfully. Nothing is returned to the user after deletion.",
    "b": "Approved result: { operationType: 'DELETE', resourceId: 'addr_4821', status: 'SUCCESS', resourceExists: false }",
    "c": "Context: RFC 9110 HTTP semantics. For a successful DELETE with no response body: 204 No Content is the correct status code — 'the server has fulfilled the request and there is no additional content to send in the response.' 200 OK with an empty body is technically correct but non-idiomatic for DELETE. 200 OK with a body (e.g., { success: true }) adds unnecessary payload for a destructive operation that naturally has no meaningful response content."
  },
  "function": "Format the DELETE success as an HTTP response. Select the correct status code per RFC 9110 and explain the choice.",
  "acceptedOutput": {
    "httpResponse": {
      "status": 204,
      "body": null,
      "headers": { "Content-Length": "0" },
      "rationale": "204 No Content is the RFC 9110 standard response for a successful DELETE where no body needs to be returned. The client's intent (delete the resource) is confirmed by the 204 status. No body is required or expected — the resource no longer exists. Content-Length: 0 informs the client not to wait for a body."
    }
  },
  "rejectedOutput": {
    "httpResponse": {
      "status": 200,
      "body": { "success": true, "deletedId": "addr_4821" }
    }
  },
  "arbiters": ["httpResponseSemanticsArbiter"],
  "whyAccepted": "204 No Content is the standard response for successful DELETE per RFC 9110 Section 15.3.5. Returning 200 with { success: true } is non-idiomatic and adds a response body to a destructive operation that has nothing meaningful to return. Clients must parse and discard the body; 204 signals unambiguously that nothing needs to be parsed.",
  "whyRejected": "200 with a body is not incorrect (RFC 9110 permits it) but it is not idiomatic for DELETE. The body adds parsing overhead for no semantic benefit — the 204 status code already communicates success. The deletedId field restates information the client already knows (it issued the DELETE for addr_4821).",
  "sourceAnchors": [
    {
      "sourceId": "IETF-RFCS",
      "url": "https://www.rfc-editor.org/rfc/rfc9110",
      "section": "RFC 9110 Section 15.3.5 — 204 No Content for successful DELETE with no response body",
      "license": "IETF Trust provisions"
    }
  ],
  "licenseConstraint": "IETF Trust provisions — document text usable for training",
  "trainingStatus": "candidate"
}
```

---

## EXT-188

```json
{
  "rowId": "EXT-188",
  "sourceIds": ["PYTHON-PEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "language-feature-adoption",
  "rowClass": "migration-decision",
  "input": {
    "a": "Reprocessing decision: a Python 3.12 codebase uses TypeVar extensively for generic type annotations. The team wants to adopt PEP 695 (Type Parameter Syntax, available in Python 3.12+) for cleaner generic syntax. Should they migrate existing TypeVar usage to PEP 695 syntax?",
    "b": "Current code: T = TypeVar('T'); K = TypeVar('K'); V = TypeVar('V'); def first(items: list[T]) -> T: ...; def make_map(keys: list[K], values: list[V]) -> dict[K, V]: ... PEP 695 equivalent: def first[T](items: list[T]) -> T: ...; def make_map[K, V](keys: list[K], values: list[V]) -> dict[K, V]: ...",
    "c": "Context: PEP 695 type parameter syntax (Python 3.12). The new syntax: (1) eliminates the TypeVar declaration boilerplate; (2) makes type parameter scope explicit (scoped to the function/class); (3) is not backward compatible — PEP 695 syntax is a syntax error in Python 3.11 and earlier. Migration is appropriate for 3.12+ codebases."
  },
  "function": "Produce the reprocessing decision: whether to migrate TypeVar to PEP 695 syntax, and the migration scope.",
  "acceptedOutput": {
    "reprocessingDecision": "MIGRATE — recommended for Python 3.12+ codebases.",
    "benefits": [
      "Eliminates module-level TypeVar declarations — type parameters are declared inline with the function/class.",
      "Makes scoping explicit — T in def first[T](...) is scoped to that function; no risk of TypeVar reuse across unrelated functions.",
      "Reduces boilerplate for parameterized generics."
    ],
    "migrationScope": "Migrate function and class definitions that use TypeVars. Priority: (1) simple single-TypeVar functions (highest clarity gain); (2) multi-TypeVar functions; (3) generic classes (TypeVar in class body → type parameter in class declaration). Do not migrate TypeVars that are used as constraints with bound= or constraints= — PEP 695 supports these but the syntax differs: def first[T: int](...) for bound constraints.",
    "prerequisite": "Confirm the minimum supported Python version is 3.12 across all deployment environments. PEP 695 syntax is a SyntaxError in Python 3.11. If any environment still runs 3.11, defer migration."
  },
  "rejectedOutput": {
    "reprocessingDecision": "Do not migrate. TypeVar syntax is established and supported. PEP 695 is too new to adopt in production code."
  },
  "arbiters": ["languageFeatureMigrationArbiter"],
  "whyAccepted": "PEP 695 is stable in Python 3.12 (not experimental). 'Too new' without a specific concern (compatibility, tooling support, team familiarity) is not a technical argument. The migration is low-risk: it is a purely syntactic change with no behavior difference, and mypy/pyright fully support PEP 695 in current versions.",
  "whyRejected": "The rejected output uses 'too new' as a blocking criterion without evaluating what 'new' means: stability (stable in 3.12), tooling support (mypy 1.5+, pyright current), team effort (mechanical rename). These should be evaluated, not assumed to be problematic.",
  "sourceAnchors": [
    {
      "sourceId": "PYTHON-PEPS",
      "url": "https://peps.python.org/pep-0695/",
      "section": "PEP 695 Type Parameter Syntax — TypeVar replacement; Python 3.12 stable feature",
      "license": "Public domain"
    }
  ],
  "licenseConstraint": "none",
  "trainingStatus": "candidate"
}
```

---

## EXT-189

```json
{
  "rowId": "EXT-189",
  "sourceIds": ["OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "result-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Translate this security audit resultI for a non-technical executive audience. The executive needs to understand business risk and required investment — not technical implementation details.",
    "b": "resultI = { overallStatus: 'FAIL', findings: [ { arbiter: 'sqlInjectionArbiter', severity: 'CRITICAL', finding: 'String interpolation in user search query at data/user.repository.ts:88', cweReference: 'CWE-89', asvsControl: 'V5.3.4' }, { arbiter: 'passwordStorageArbiter', severity: 'HIGH', finding: 'SHA-1 password hashing in auth/password.service.ts:31', cweReference: 'CWE-916', asvsControl: 'V2.4.5' } ], remediationEstimate: { effortDays: 3, riskIfUnpatched: 'Data breach risk — user records and credentials exposed' } }",
    "c": "Context: Node 6 result translator for executive audience. Executives need: business risk in plain language, regulatory/compliance exposure, cost to fix, and a decision prompt. They do not need CWE references, file paths, or ASVS control numbers."
  },
  "function": "Translate the security audit resultI for an executive. Use plain business language. Quantify risk and remediation investment.",
  "acceptedOutput": {
    "finalMachineResult": {
      "audience": "executive",
      "headline": "Security audit: 2 critical issues found requiring immediate attention.",
      "businessRisk": "The application has two vulnerabilities that could allow an attacker to read or steal user data, including passwords. If exploited before patching, the company faces potential data breach liability, regulatory reporting obligations (GDPR, CCPA), and reputational damage.",
      "immediateActions": [
        "Pause new feature development on the affected components until the issues are patched.",
        "Schedule 3 days of engineering time to implement the fixes (estimated remediation: 3 engineer-days).",
        "After patching, conduct a verification scan to confirm the issues are resolved."
      ],
      "investmentRequired": "3 engineer-days. Estimated cost: [calculated from team rate]. Cost of a data breach: median $4.88M (IBM Cost of Data Breach Report 2024).",
      "timeline": "These issues should be fixed before the next production deployment."
    }
  },
  "rejectedOutput": {
    "finalMachineResult": "The audit found SQL injection (CWE-89) at V5.3.4 and weak password hashing (CWE-916) at V2.4.5. These are critical and high severity respectively. The team should patch these per the ASVS requirements."
  },
  "arbiters": ["resultTranslationArbiter"],
  "whyAccepted": "The executive translation: explains risk in business terms (data breach liability, regulatory exposure), omits technical identifiers (CWE-89, V5.3.4), quantifies the investment (3 engineer-days), provides a cost comparison (breach cost), and gives a clear decision prompt (pause new features, schedule fix).",
  "whyRejected": "The rejected output uses CWE references, ASVS control numbers, and technical severity terminology that are meaningful to security engineers but opaque to executives. 'Should patch per ASVS requirements' does not communicate urgency or business consequence.",
  "sourceAnchors": [
    {
      "sourceId": "OWASP-ASVS",
      "url": "https://github.com/OWASP/ASVS",
      "section": "Security audit result translation for executive audience — business risk vs technical finding detail",
      "license": "CC-BY-SA-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-SA-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-190

```json
{
  "rowId": "EXT-190",
  "sourceIds": ["NESTJS-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "A_TOPOLOGY",
  "subdomain": "module-circular-dependency",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this NestJS module dependency result: UserModule imports OrderModule (to access OrderService for user order history). OrderModule imports UserModule (to access UserService for order owner validation). Both modules use forwardRef() to resolve the circular dependency.",
    "b": "@Module({ imports: [forwardRef(() => OrderModule)] }) export class UserModule {} @Module({ imports: [forwardRef(() => UserModule)] }) export class OrderModule {} — circular module dependency resolved with forwardRef() on both sides.",
    "c": "Context: NestJS forwardRef() and circular dependencies. forwardRef() is a workaround for circular module dependencies in NestJS. It works by deferring the module reference until after modules are initialized. While it resolves the initialization error, it is a code smell: the circular dependency indicates that the two modules have high coupling and the dependency direction violates a module boundary principle. If UserModule and OrderModule depend on each other, they are effectively one module."
  },
  "function": "Apply circularModuleDepArbiter: evaluate whether forwardRef() is the correct resolution for the UserModule-OrderModule circular dependency. Produce the architectural resolution.",
  "acceptedOutput": {
    "decision": "REJECT forwardRef() as a permanent solution. forwardRef() resolves the initialization error but does not address the underlying coupling. UserModule needing OrderService and OrderModule needing UserService indicates that the two modules share concerns that should live in a third module or be restructured.",
    "reprocessInstruction": "Two architectural options: (1) Extract a SharedModule containing OrderUserValidationService (validates an order belongs to a user without UserModule knowing about orders or vice versa). UserModule and OrderModule both import SharedModule. No circular dependency. (2) Dependency inversion: if OrderModule needs to validate ownership, inject a UserValidationPort interface in OrderModule. UserModule implements the interface and registers the implementation. OrderModule depends on the interface, not on UserModule directly. Choose option based on: if the shared logic is simple and grows — SharedModule; if OrderModule must remain independent — dependency inversion."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. forwardRef() is the NestJS-documented solution for circular dependencies. Both modules use it correctly."
  },
  "arbiters": ["circularModuleDepArbiter"],
  "whyAccepted": "forwardRef() being documented as a solution does not make circular module dependencies architecturally correct. NestJS documentation describes it as a workaround for cases where circular dependencies are unavoidable — it explicitly recommends avoiding circular dependencies in favor of module restructuring.",
  "whyRejected": "The rejected output treats 'documented workaround' as 'correct design'. forwardRef() is documented as a last resort, not a first-class pattern. A UserModule that imports OrderModule that imports UserModule is a coupled design regardless of how the initialization is resolved.",
  "sourceAnchors": [
    {
      "sourceId": "NESTJS-ADVANCED",
      "url": "https://github.com/nestjs/nest",
      "section": "Circular dependencies — forwardRef() as workaround; SharedModule extraction as architectural resolution",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```
