# External Training Rows — Batch 13
## Row IDs: EXT-251 through EXT-270
## Date: 2026-05-04
## Focus: Helm, Prometheus, WCF migration, Vue 3, Svelte, Cypress, Saga compensation, health checks, RFC 7235, additional families 1/2/3/4/5/6/7/8

---

## EXT-251

```json
{
  "rowId": "EXT-251",
  "sourceIds": ["HELM-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "helm-values-configuration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose this Kubernetes deployment task: deploy the payment-service Helm chart to the production environment with 5 replicas, a 512Mi memory limit, and the image tag 'v2.3.1'. The staging deployment uses 2 replicas and a 256Mi limit.",
    "b": "Two approaches: (A) Use 'helm upgrade --set replicaCount=5,resources.limits.memory=512Mi,image.tag=v2.3.1'. (B) Create a values-production.yaml file with the production-specific values and run 'helm upgrade -f values-production.yaml'.",
    "c": "Context: Helm values hierarchy. '--set' overrides are command-line flags that are not tracked in version control. '-f values-production.yaml' uses a file tracked in version control (GitOps). Audit requirement: production configuration changes must be traceable in the git history. '--set' changes are invisible in git history unless the command is also recorded in a CI/CD pipeline definition."
  },
  "function": "Produce the action items for the production Helm deployment.",
  "acceptedOutput": {
    "actionItems": [
      "Action 1: Create values-production.yaml in the chart's environments/production/ directory: replicaCount: 5; resources: { limits: { memory: 512Mi, cpu: '500m' }, requests: { memory: 256Mi, cpu: '100m' } }; image: { tag: v2.3.1 }",
      "Action 2: Commit values-production.yaml to the repository. This records the configuration change with the git author, timestamp, and commit message.",
      "Action 3: Run the deployment via CI/CD: helm upgrade payment-service ./charts/payment-service -f values/values-base.yaml -f values/values-production.yaml --namespace production --atomic --timeout 5m",
      "Action 4: The '--atomic' flag rolls back automatically if any pod fails to become ready within the timeout — prevents partial upgrades.",
      "Action 5: After deployment, verify: kubectl get pods -n production -l app=payment-service — expect 5 Running pods with the v2.3.1 image."
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Run: helm upgrade payment-service ./charts/payment-service --set replicaCount=5 --set resources.limits.memory=512Mi --set image.tag=v2.3.1 --namespace production"
    ]
  },
  "arbiters": ["helmConfigurationArbiter"],
  "whyAccepted": "The values file approach satisfies the audit requirement — every production configuration change is a git commit. The --set approach produces an untracked change: if the deployment fails and must be investigated, the exact configuration values used are not recoverable from git history alone.",
  "whyRejected": "The --set approach makes the production configuration invisible in git history. If the deployment is triggered by a human at the command line, the specific --set values are lost. The audit requirement ('traceable in git history') is violated.",
  "sourceAnchors": [
    {
      "sourceId": "HELM-DOCS",
      "url": "https://github.com/helm/helm",
      "section": "Values files — -f vs --set; GitOps values hierarchy; --atomic rollback on deployment failure",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-252

```json
{
  "rowId": "EXT-252",
  "sourceIds": ["PROMETHEUS-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "promql-alert-design",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Review this Prometheus alert rule: alert: HighErrorRate; expr: http_requests_total{status='500'} > 10; for: 0m; labels: { severity: critical }.",
    "b": "The intent is to alert when the HTTP 500 error rate is high. The metric 'http_requests_total' is a counter (cumulative total since process start).",
    "c": "Prometheus metric types: Counter — cumulative, monotonically increasing. A counter of 10 means 10 errors total since startup, not 10 errors per second. To get error rate, use the rate() function: rate(http_requests_total{status='500'}[5m]) — returns errors per second over the last 5 minutes. An alert on a raw counter > 10 fires immediately after 10 total errors since startup — even if the rate is 0.001 errors/second over hours."
  },
  "function": "Arbitrate: is this alert rule correct? What is the correct expression?",
  "acceptedOutput": {
    "verdict": "INCORRECT alert expression.",
    "problem": "http_requests_total is a counter (cumulative since startup). 'http_requests_total{status=\"500\"} > 10' fires after 10 total 500 errors since the process started — this fires once and stays fired permanently for any service that has ever had 10 errors. It measures total count, not rate.",
    "correctedRule": "alert: HighErrorRate\nexpr: |\n  rate(http_requests_total{status='500'}[5m])\n  /\n  rate(http_requests_total[5m])\n  > 0.05\nfor: 5m\nlabels:\n  severity: critical\nannotations:\n  summary: 'HTTP 500 error rate above 5%'",
    "explanation": "The corrected expression: rate() computes per-second rate over 5 minutes. Dividing 500-rate by total-rate gives error ratio (0.05 = 5%). The 'for: 5m' ensures the condition must persist for 5 minutes before alerting — eliminates transient spikes from triggering pages."
  },
  "rejectedOutput": {
    "verdict": "The alert is correct. 10 errors is a reasonable threshold for a critical alert. The 'for: 0m' ensures immediate alerting."
  },
  "arbiters": ["observabilityAlertArbiter"],
  "whyAccepted": "The corrected verdict is factually correct: http_requests_total is a counter. Alerting on a raw counter > 10 means the alert fires once (after 10 total errors) and never recovers — counters only increase. The alert would immediately become permanently firing for any service that processes meaningful traffic.",
  "whyRejected": "The rejected verdict does not identify the counter-vs-rate distinction. '10 errors is a reasonable threshold' is true for a rate (10/sec might be alertworthy), but not for a counter that represents all errors since startup. The 'for: 0m' means the alert fires instantly when counter > 10 — one deployment ago.",
  "sourceAnchors": [
    {
      "sourceId": "PROMETHEUS-DOCS",
      "url": "https://github.com/prometheus/prometheus",
      "section": "PromQL rate() — counter to rate conversion; error ratio alerting; for duration debounce",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-253

```json
{
  "rowId": "EXT-253",
  "sourceIds": ["DOTNET-MICROSERVICES"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "wcf-to-grpc-migration",
  "rowClass": "migration-decision",
  "input": {
    "a": "A .NET Framework 4.8 WCF (Windows Communication Foundation) service exposes 12 operations for internal inter-service communication. The team is migrating to .NET 8. WCF server is not supported in .NET 8. Should we migrate to gRPC or REST?",
    "b": "WCF service characteristics: all operations are synchronous request-response. 8 operations are simple CRUD (Create, Read, Update, Delete) over business entities. 4 operations are compute-intensive with large response payloads (1–5 MB). All clients are internal .NET services. No external clients. Estimated migration scope: 12 operations, 3 service contracts.",
    "c": "Migration options: (1) CoreWCF — open-source WCF port for .NET. Supports a subset of WCF bindings. Allows migration with minimal code changes. Not all WCF features supported. (2) gRPC — Protobuf binary serialization, HTTP/2, streaming support. Better performance for large payloads. Requires defining .proto files. (3) REST/HTTP — universal, tooling-rich, JSON. Higher serialization overhead for 1–5 MB payloads. Microsoft provides the WCF-to-gRPC migration guide."
  },
  "function": "Decide: CoreWCF, gRPC, or REST for the WCF service migration?",
  "acceptedOutput": {
    "decision": "MIGRATE to gRPC.",
    "rationale": "Three factors favor gRPC over REST and CoreWCF: (1) All clients are internal .NET services — gRPC tooling (protoc code generation) is well-integrated with .NET. No external clients means no REST universality requirement. (2) Large payloads (1–5 MB, 4 operations) — Protobuf binary is 30–50% smaller than JSON for structured data. At 5 MB JSON = 2.5–3.5 MB Protobuf. For frequently-called operations with large payloads, this is significant. (3) CoreWCF is a migration shortcut, not a destination — it supports a WCF subset and is not the long-term direction for .NET 8 services. gRPC is the Microsoft-recommended successor for internal WCF services.",
    "migrationPath": "Use the dotnet-grpc tool: dotnet grpc add-proto service.proto. Define .proto contracts matching the WCF service operations. Generate client and server stubs. Implement gRPC service methods. Replace WCF client proxies with generated gRPC client stubs."
  },
  "rejectedOutput": {
    "decision": "Migrate to CoreWCF. It allows minimal code changes and preserves the existing WCF programming model. gRPC requires significant refactoring of all 12 operations."
  },
  "arbiters": ["dotnetMigrationArbiter"],
  "whyAccepted": "CoreWCF is a transitional solution — it preserves the WCF programming model but not the performance or modern .NET idioms. Microsoft's own guidance recommends gRPC for internal service communication migration from WCF. The .proto contract definition effort for 12 operations is bounded and one-time.",
  "whyRejected": "CoreWCF's 'minimal code changes' benefit comes at the cost of carrying WCF patterns into a .NET 8 codebase. The WCF programming model (ServiceContracts, DataContracts, OperationContracts) is not idiomatic in .NET 8. Choosing CoreWCF defers the migration to gRPC/REST to a future migration cycle.",
  "sourceAnchors": [
    {
      "sourceId": "DOTNET-MICROSERVICES",
      "url": "https://github.com/dotnet-architecture/eShopOnContainers",
      "section": "WCF to gRPC migration — .NET 8 CoreWCF vs gRPC for internal 12-operation service",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-254

```json
{
  "rowId": "EXT-254",
  "sourceIds": ["VUE-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "vue3-composition-api-migration",
  "rowClass": "migration-decision",
  "input": {
    "a": "A Vue 2 codebase uses the Options API (data(), methods, computed, watch) throughout. The team is migrating to Vue 3. Should they migrate all components from Options API to Composition API, or keep Options API?",
    "b": "Codebase: 87 components. 15 components have complex state logic shared across multiple components (currently handled via mixins). 72 components are straightforward (local state, a few methods). Team: 3 senior Vue developers, 2 junior developers new to Vue.",
    "c": "Vue 3 Composition API: logic organized by concern (not by option type). Reusable composables replace mixins — explicit import vs implicit mixin merge. TypeScript support is significantly better with Composition API (reactive() and ref() have proper inference). Options API: still fully supported in Vue 3. Mixing both APIs in one codebase is valid — Vue 3 supports both simultaneously."
  },
  "function": "Decide: full Composition API migration or selective migration?",
  "acceptedOutput": {
    "decision": "SELECTIVE MIGRATION — migrate the 15 complex components, keep Options API for the 72 simple components.",
    "rationale": "The 15 complex components with shared state are the primary pain point in Vue 2 (mixin conflicts, unclear data source). Composition API composables solve this problem directly. The 72 simple components have no mixin issues and work correctly with Options API — migrating them adds risk without benefit. Vue 3 explicitly supports both APIs simultaneously. Junior developers can learn Composition API incrementally on the complex components before the full codebase uses it.",
    "migrationPriority": [
      "Priority 1: Extract the shared mixin logic into composables (e.g., useAuthState(), useCartItems()). This eliminates mixin merge conflicts across all 15 complex components at once.",
      "Priority 2: Migrate the 15 complex components to Composition API using the extracted composables.",
      "Priority 3 (optional): Migrate the 72 simple components if TypeScript inference improvement is a team goal. Not required for correctness."
    ],
    "gate": "All 15 complex components use composables. Zero mixin usages remain. All 87 components render correctly."
  },
  "rejectedOutput": {
    "decision": "Migrate all 87 components to Composition API immediately. Options API is the old way — the team should standardize on Composition API exclusively."
  },
  "arbiters": ["vueMigrationArbiter"],
  "whyAccepted": "Migrating 87 components simultaneously is a large, risky scope. Vue 3 supports both APIs — there is no technical requirement for exclusivity. Selective migration targets the actual pain point (mixin-heavy components) and minimizes regression risk for the 72 simple components.",
  "whyRejected": "'The old way' is not a migration criterion — Options API is fully supported and maintained in Vue 3. Migrating 72 working components to eliminate Options API introduces regression risk for zero functional improvement. The scope of 87 simultaneous migrations in an active codebase is difficult to execute without regressions.",
  "sourceAnchors": [
    {
      "sourceId": "VUE-DOCS",
      "url": "https://github.com/vuejs/core",
      "section": "Vue 3 Composition API — selective migration from Options API; composables replacing mixins; dual-API support",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-255

```json
{
  "rowId": "EXT-255",
  "sourceIds": ["SVELTE-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "svelte-reactive-declarations",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Review this Svelte component that displays a filtered and sorted product list.",
    "b": "Component A: <script> let products = []; let filterText = ''; let filtered = products.filter(p => p.name.includes(filterText)); let sorted = filtered.sort((a, b) => a.price - b.price); </script> Component B: <script> let products = []; let filterText = ''; $: filtered = products.filter(p => p.name.includes(filterText)); $: sorted = [...filtered].sort((a, b) => a.price - b.price); </script>",
    "c": "Svelte reactive declarations: the '$:' prefix makes a statement reactive — it re-runs when any referenced reactive value changes. In Component A, 'filtered' and 'sorted' are computed once at initialization. When 'filterText' changes, 'filtered' and 'sorted' do not update because they are plain variable assignments, not reactive declarations. The '$: filtered = ...' in Component B re-runs the filter whenever 'products' or 'filterText' changes."
  },
  "function": "Arbitrate: which component is correct?",
  "acceptedOutput": {
    "verdict": "Component B is correct.",
    "reasoning": "In Component A, 'filtered' and 'sorted' are plain JavaScript variable assignments. They compute once when the component initializes — at that point, 'products' is an empty array (products = []), so 'filtered' and 'sorted' are also empty arrays. When 'filterText' changes (user types in a search box), the assignment 'let filtered = products.filter(...)' does not re-execute — it is not reactive. Component B uses '$:' reactive declarations: '$: filtered = products.filter(...)' re-executes whenever 'products' or 'filterText' changes. The spread operator '[...filtered]' before sort prevents mutation of the 'filtered' array (Array.sort mutates in place).",
    "additionalFix": "The spread in Component B ('[...filtered].sort(...)') is correct. Component A's 'filtered.sort(...)' would mutate the 'filtered' array in place — for reactive declarations, mutation is problematic because Svelte tracks assignments, not mutations."
  },
  "rejectedOutput": {
    "verdict": "Both components are equivalent. Svelte automatically tracks all variable changes, so 'filtered' will update when 'filterText' changes in both components."
  },
  "arbiters": ["svelteReactivityArbiter"],
  "whyAccepted": "The rejected verdict contains a factual error: Svelte does NOT automatically track plain 'let' variable assignments as reactive. Only '$:' reactive declarations and Svelte store subscriptions are reactive. A plain 'let filtered = ...' is a JavaScript statement that executes once at component initialization.",
  "whyRejected": "Svelte's reactivity system is explicit: '$:' marks reactive statements. Plain 'let' assignments are not reactive in Svelte — this is a fundamental aspect of Svelte's compile-time reactivity model, not a runtime tracking behavior.",
  "sourceAnchors": [
    {
      "sourceId": "SVELTE-DOCS",
      "url": "https://github.com/sveltejs/svelte",
      "section": "Reactive declarations — $: prefix for reactive statements vs plain let assignment; array mutation",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-256

```json
{
  "rowId": "EXT-256",
  "sourceIds": ["CYPRESS-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "cypress-api-interception",
  "rowClass": "negative-example",
  "input": {
    "a": "Execute: write a Cypress end-to-end test that verifies the product list page displays items when the API returns products, and displays an empty state when the API returns an empty array. The API endpoint is GET /api/products.",
    "b": "Test requirements: (1) Scenario 1: API returns [{id:1, name:'Widget', price:9.99}] — product list shows one item with name and price. (2) Scenario 2: API returns [] — page shows 'No products found' text. Use cy.intercept to stub the API response.",
    "c": "Cypress cy.intercept: intercepts network requests matching a URL pattern and can stub the response. 'cy.intercept(\"GET\", \"/api/products\", { body: [...] })' stubs all matching requests to return the specified body. The intercept must be set up before the page loads. Route aliases (cy.intercept(...).as(\"alias\")) allow 'cy.wait(\"@alias\")' to ensure the intercept was matched before assertions run."
  },
  "function": "Implement the two Cypress test scenarios.",
  "acceptedOutput": {
    "implementation": "describe('Product List Page', () => {\n  it('displays products when API returns data', () => {\n    cy.intercept('GET', '/api/products', {\n      body: [{ id: 1, name: 'Widget', price: 9.99 }]\n    }).as('getProducts');\n    cy.visit('/products');\n    cy.wait('@getProducts');\n    cy.get('[data-testid=\"product-item\"]').should('have.length', 1);\n    cy.get('[data-testid=\"product-name\"]').should('contain.text', 'Widget');\n    cy.get('[data-testid=\"product-price\"]').should('contain.text', '9.99');\n  });\n\n  it('displays empty state when API returns empty array', () => {\n    cy.intercept('GET', '/api/products', { body: [] }).as('getProducts');\n    cy.visit('/products');\n    cy.wait('@getProducts');\n    cy.get('[data-testid=\"product-item\"]').should('not.exist');\n    cy.get('[data-testid=\"empty-state\"]').should('contain.text', 'No products found');\n  });\n});"
  },
  "rejectedOutput": {
    "implementation": "describe('Product List', () => {\n  it('shows products', () => {\n    cy.visit('/products');\n    cy.get('.product').should('exist');\n  });\n  it('shows empty state', () => {\n    cy.visit('/products');\n    cy.get('.empty-state').should('exist');\n  });\n});"
  },
  "arbiters": ["e2eTestingArbiter"],
  "whyAccepted": "The accepted implementation uses cy.intercept to control the API response for each scenario. The intercept is set up before cy.visit, so the stub is in place when the page loads. cy.wait('@getProducts') ensures the intercept was matched before assertions run. data-testid attributes are used for stable element selection.",
  "whyRejected": "The rejected implementation visits the page without stubbing the API — it tests against whatever the real API returns. The 'shows products' test will fail if the test database is empty; the 'shows empty state' test will fail if products exist. These tests have undetermined outcomes that depend on database state, not predictable stubs.",
  "sourceAnchors": [
    {
      "sourceId": "CYPRESS-DOCS",
      "url": "https://github.com/cypress-io/cypress",
      "section": "cy.intercept — API stub before page visit; .as alias for cy.wait; predictable test state",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-257

```json
{
  "rowId": "EXT-257",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "structured-log-parsing",
  "rowClass": "negative-example",
  "input": {
    "a": "Execute: parse structured log lines in the format 'TIMESTAMP LEVEL SERVICE: MESSAGE' where TIMESTAMP is ISO 8601, LEVEL is INFO/WARN/ERROR, SERVICE is alphanumeric, MESSAGE is free text. Return a list of dicts with keys: timestamp, level, service, message. Skip malformed lines and count them.",
    "b": "Function signature: parse_structured_logs(lines: list[str]) -> dict. Return: { 'records': list[dict], 'malformed_count': int }. Example line: '2026-05-04T10:30:00Z INFO payment-svc: Transaction 12345 processed successfully'",
    "c": "Regex pattern for the format: r'^(\\S+)\\s+(INFO|WARN|ERROR)\\s+(\\w[\\w-]*):\\s+(.+)$'. Group 1: timestamp, Group 2: level, Group 3: service (alphanumeric and hyphens), Group 4: message (rest of line). The colon after service name must be matched explicitly — some log lines may have ':' in the message body."
  },
  "function": "Implement parse_structured_logs returning records and malformed count.",
  "acceptedOutput": {
    "implementation": "import re\n\nLOG_PATTERN = re.compile(r'^(\\S+)\\s+(INFO|WARN|ERROR)\\s+([\\w][\\w-]*):\\s+(.+)$')\n\ndef parse_structured_logs(lines: list) -> dict:\n    records = []\n    malformed_count = 0\n    for line in lines:\n        line = line.strip()\n        if not line:\n            continue  # skip blank lines without counting as malformed\n        m = LOG_PATTERN.match(line)\n        if m:\n            records.append({\n                'timestamp': m.group(1),\n                'level': m.group(2),\n                'service': m.group(3),\n                'message': m.group(4)\n            })\n        else:\n            malformed_count += 1\n    return {'records': records, 'malformed_count': malformed_count}",
    "behavior": "For '2026-05-04T10:30:00Z INFO payment-svc: Transaction processed': records=[{timestamp:'2026-05-04T10:30:00Z', level:'INFO', service:'payment-svc', message:'Transaction processed'}], malformed_count=0."
  },
  "rejectedOutput": {
    "implementation": "def parse_structured_logs(lines):\n    records = []\n    for line in lines:\n        parts = line.split(' ', 3)\n        records.append({'timestamp': parts[0], 'level': parts[1], 'service': parts[2], 'message': parts[3] if len(parts) > 3 else ''})\n    return {'records': records, 'malformed_count': 0}"
  },
  "arbiters": ["logParsingArbiter"],
  "whyAccepted": "The regex-based implementation correctly handles: service names with hyphens (payment-svc), colons in message body (split-based parsing fails here), blank lines, and malformed line counting. The compiled regex is applied once per line.",
  "whyRejected": "The split-based implementation: (1) Never counts malformed lines (malformed_count always 0). (2) Fails for service names with hyphens: split(' ', 3) on '2026-05-04T10:30:00Z INFO payment-svc: message' gives parts=[..., 'INFO', 'payment-svc:', 'message'] — service becomes 'payment-svc:' (includes the colon) and message parsing is wrong. (3) Does not validate that LEVEL is INFO/WARN/ERROR.",
  "sourceAnchors": [
    {
      "sourceId": "BIGCODEBENCH",
      "url": "https://github.com/bigcode-project/bigcodebench",
      "section": "Task: parse_structured_logs — regex group extraction vs split with malformed handling",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-258

```json
{
  "rowId": "EXT-258",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "cross-cutting-pr-aggregation",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Aggregate results from four analysis agents for a pull request that adds distributed tracing to 8 service endpoints.",
    "b": "Agent results: Agent 1 (Test Agent): PASS — all existing tests pass. Agent 2 (Performance Agent): WARN — the tracing overhead adds 2–3ms to each instrumented endpoint. Agent 3 (Compatibility Agent): PASS — OpenTelemetry SDK version is compatible with all services. Agent 4 (Configuration Agent): WARN — the sampling rate is set to 100% (sample every request). For a high-traffic service (10,000 req/min), 100% sampling may generate excessive trace data.",
    "c": "Tracing policy: (1) All tests must pass. (2) Performance overhead must be documented but is acceptable up to 10ms for observability improvements. (3) Sampling rate must be configured per environment — 100% in development, 10% in staging, 1–5% in production. The PR configures 100% sampling with no environment-specific override."
  },
  "function": "Produce the aggregated result for this pull request.",
  "acceptedOutput": {
    "aggregatedResult": "BLOCK",
    "resultCode": "SAMPLING_RATE_MISCONFIGURATION",
    "summary": "The pull request is blocked on sampling rate configuration. 100% trace sampling in production (10,000 req/min) would generate 600,000 trace spans per minute — estimated 50–100 GB/day of trace data. The tracing policy requires 1–5% production sampling. The performance overhead (2–3ms) is within the acceptable 10ms threshold and is not a blocking issue.",
    "requiredActions": [
      "Add environment-specific sampling configuration: OTEL_TRACES_SAMPLER=parentbased_traceidratio, OTEL_TRACES_SAMPLER_ARG=0.01 for production (1%), 0.10 for staging, 1.0 for development",
      "Configure sampling via environment variables or a configuration map so each environment's deployment can override the rate",
      "Re-submit with environment-aware sampling configuration"
    ],
    "blockingAgent": "Configuration Agent (100% sampling rate without env override)",
    "passingAgents": ["Test Agent", "Compatibility Agent"],
    "advisoryAgents": ["Performance Agent (2-3ms overhead within 10ms limit)"]
  },
  "rejectedOutput": {
    "aggregatedResult": "PASS_WITH_ADVISORY",
    "summary": "Tests pass and SDK is compatible. Two warnings noted. Recommend adjusting sampling in a follow-up PR."
  },
  "arbiters": ["observabilityConfigArbiter"],
  "whyAccepted": "The sampling rate misconfiguration is a policy violation that becomes a production incident when deployed: 100% sampling at 10,000 req/min generates data at a rate that will exhaust trace storage within hours. The Configuration Agent WARN is actually a BLOCK per the stated policy (sampling must be environment-specific). PASS_WITH_ADVISORY defers a guaranteed production storage incident.",
  "whyRejected": "The rejected verdict treats a policy-violating configuration as advisory. The 'follow-up PR' pattern allows the misconfigured sampling rate to reach production. At 10,000 req/min and 100% sampling, the production impact (storage exhaustion, trace backend overload) will manifest within hours of deployment.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-BENCH",
      "url": "https://github.com/princeton-nlp/SWE-bench",
      "section": "Distributed tracing sampling rate — BLOCK for 100% production sampling; environment-specific configuration",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-259

```json
{
  "rowId": "EXT-259",
  "sourceIds": ["OPENAPI-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "B_ROUTING",
  "subdomain": "api-versioning-strategy",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We need to version our REST API. We have an existing v1 API in production and need to release v2 with breaking changes. Three options: URL path versioning (/api/v1/..., /api/v2/...), request header versioning (API-Version: 2), or content negotiation (Accept: application/vnd.myapi.v2+json). Which should we use?",
    "b": "Consumer profile: public API used by 200+ external developers, mobile apps (iOS, Android), and web frontends. Documentation is generated from OpenAPI specifications. Developer experience is a priority — the API should be easy to explore in a browser and test with curl.",
    "c": "Versioning approaches: URL path — visible in browser, bookmarkable, easy to route at load balancer/API gateway, OpenAPI spec per version is a separate URL. Header versioning — cleaner URLs, requires custom header knowledge, cannot be explored in browser without tooling. Content negotiation — HTTP standard (Accept header), most complex to implement, requires per-media-type response logic. Microsoft REST Guidelines and Google API Design Guide both recommend URL path versioning for public APIs."
  },
  "function": "Produce the command: which versioning strategy to use.",
  "acceptedOutput": {
    "command": "Use URL path versioning: /api/v1/... and /api/v2/...",
    "rationale": "For a public API with 200+ external developers: (1) Discoverability — developers can see the version in the URL, bookmark version-specific resources, and copy/paste URLs in Slack/email/documentation. Header versioning requires documentation to specify the header. (2) Browser exploration — developers testing the API in a browser without tooling see the version. curl commands in documentation include the version in the URL: curl https://api.example.com/api/v2/users. (3) Routing — API gateway can route /api/v1/* to v1 backend and /api/v2/* to v2 backend without header inspection. (4) OpenAPI — one spec per version at /api/v2/openapi.json is conventional and supported by all major spec tooling.",
    "coexistence": "Run v1 and v2 simultaneously. Announce v1 deprecation timeline (6–12 months). Provide migration guide from v1 to v2. Monitor v1 traffic for shutdown decision."
  },
  "rejectedOutput": {
    "command": "Use header versioning (API-Version: 2). URL path versioning pollutes the URL structure. Headers are the semantically correct place for version information."
  },
  "arbiters": ["apiVersioningArbiter"],
  "whyAccepted": "URL path versioning is the de facto standard for public APIs with developer audiences. Google, GitHub, Stripe, and Twilio all use URL path versioning. The 'semantically correct' argument for headers does not outweigh the developer experience advantages for a public API. Internal APIs can be more flexible on this choice.",
  "whyRejected": "Header versioning requires every developer to know the custom header name. A developer testing in a browser with no tooling cannot easily test different versions. Documentation examples must include header flags in every curl example. These are concrete UX disadvantages for a public developer API.",
  "sourceAnchors": [
    {
      "sourceId": "OPENAPI-SPEC",
      "url": "https://github.com/OAI/OpenAPI-Specification",
      "section": "API versioning — URL path vs header vs content negotiation; public API developer experience",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-260

```json
{
  "rowId": "EXT-260",
  "sourceIds": ["MICROSOFT-LEARN-ARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "saga-compensation-transactions",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A checkout saga has 3 steps: (1) Reserve inventory, (2) Charge payment, (3) Confirm order. Step 2 (charge payment) failed after step 1 succeeded. The saga coordinator must compensate. Two options: (A) retry step 2 indefinitely until it succeeds. (B) Execute compensation: cancel the inventory reservation, then mark the order as failed.",
    "b": "Failure context: the payment gateway returned HTTP 402 (Payment Required) — the customer's card was declined. This is a non-retriable business failure, not a transient network error.",
    "c": "Saga compensation transactions: each saga step has a compensating transaction that undoes the step's effect. For step 1 (Reserve inventory): compensating transaction is 'Release inventory reservation'. A 402 Payment Required response is a business rejection (insufficient funds, card declined) — retrying will not change the outcome. Compensation must release the reserved inventory to prevent stock being held indefinitely for a failed order."
  },
  "function": "Arbitrate: retry or compensate?",
  "acceptedOutput": {
    "verdict": "COMPENSATE — execute the compensating transaction for step 1.",
    "reasoning": "HTTP 402 Payment Required from the payment gateway indicates a business-level rejection (card declined, insufficient funds). This is not a transient error — retrying will not change the outcome unless the customer updates their payment method. Retrying indefinitely holds the inventory reservation open: if the item has limited stock, the reservation prevents other customers from purchasing. Compensation sequence: (1) Call inventory service: CancelReservation(reservationId). (2) Update order status to PAYMENT_DECLINED. (3) Notify customer of payment failure with instructions to update payment method.",
    "retriableVsNonRetriable": "Retriable errors (transient): 503 Service Unavailable, 429 Too Many Requests, network timeout. Non-retriable errors (business rejection): 402 Payment Required, 400 Bad Request with INVALID_CARD, 422 with CARD_EXPIRED. A saga coordinator must distinguish these to determine retry vs compensate."
  },
  "rejectedOutput": {
    "verdict": "Retry step 2 indefinitely. The payment may succeed if the customer updates their card. The inventory should remain reserved while retrying."
  },
  "arbiters": ["sagaOrchestrationArbiter", "compensationTransactionArbiter"],
  "whyAccepted": "Retrying a 402 indefinitely is incorrect: the card is declined — retrying the same charge request does not change the outcome. The inventory reservation is a limited resource: holding it for a declined payment prevents legitimate purchases. The saga must compensate and release the reservation.",
  "whyRejected": "'Retry until the customer updates their card' conflates the retry mechanism (automated, same request) with a user interaction (customer provides new card). Automated retry cannot trigger customer payment method update. The inventory hold duration for an indefinite retry has no bound — reservation could be held for days or indefinitely.",
  "sourceAnchors": [
    {
      "sourceId": "MICROSOFT-LEARN-ARCH",
      "url": "https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/saga/saga",
      "section": "Saga pattern — compensation transactions; retriable vs non-retriable error classification; 402 payment failure",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-261

```json
{
  "rowId": "EXT-261",
  "sourceIds": ["OPENASSISTANT-OASST1"],
  "sourceTrustTier": 2,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "security-incident-user-communication",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Translate this machine result into a user-facing message for affected customers: { status: 'SECURITY_INCIDENT_CONFIRMED', incidentType: 'UNAUTHORIZED_DATA_ACCESS', affectedDataTypes: ['email_address', 'hashed_password'], affectedUsersCount: 12500, detectionTimestamp: '2026-05-03T22:00:00Z', containmentTimestamp: '2026-05-04T01:30:00Z', externalExposureConfirmed: false }",
    "b": "Target audience: end users (customers) who may have been affected. The incident was contained within 3.5 hours. External exposure has not been confirmed — the unauthorized access was internal. The communication must be transparent, accurate, and actionable.",
    "c": "Security incident communication best practices: be factual (state what is known and what is unknown). Do not speculate. Provide concrete actions the user can take. Be specific about what data was affected and what was not. Avoid minimizing language ('minor incident') or alarmist language ('catastrophic breach')."
  },
  "function": "Produce the final user-facing security incident notification.",
  "acceptedOutput": {
    "userMessage": "Important security notice for your account",
    "body": "We are writing to inform you of a security incident that may have affected your account.\n\nWhat happened: On May 3, 2026, we detected unauthorized access to a subset of our user database. We contained the incident within 3.5 hours (by 1:30 AM UTC on May 4).\n\nWhat data was affected: Email addresses and hashed passwords were accessed. Your hashed password is not your actual password — it is a one-way cryptographic transformation. However, as a precaution, we recommend changing your password.\n\nWhat was NOT accessed: Payment information, full name, address, and other personal data were not in the affected database.\n\nWhat we do not yet know: We have not confirmed whether the accessed data was copied or transmitted outside our systems. Our investigation is ongoing.\n\nActions to take: (1) Change your password on this service. (2) If you use the same password on other services, change it there as well. (3) Watch for phishing emails — attackers who have email addresses may attempt phishing. We will never ask for your password via email.",
    "contactInfo": "Questions: security@example.com"
  },
  "rejectedOutput": {
    "userMessage": "We experienced a security incident. Some user data including email and hashed password may have been accessed. We take security seriously. Please change your password."
  },
  "arbiters": ["securityCommunicationArbiter"],
  "whyAccepted": "The accepted message provides: timeline (when detected, when contained), specific data types affected, specific data types not affected, honest statement of what is unknown, and concrete actionable steps. The hashed password explanation prevents user panic ('my password was stolen') while accurately describing what was accessed.",
  "whyRejected": "The rejected message is vague and not actionable. 'We take security seriously' is boilerplate that erodes trust when unaccompanied by specifics. 'May have been accessed' leaves the user uncertain about whether to act. No timeline, no specifics about what was not accessed, and no concrete steps beyond 'change your password'.",
  "sourceAnchors": [
    {
      "sourceId": "OPENASSISTANT-OASST1",
      "url": "https://huggingface.co/datasets/OpenAssistant/oasst1",
      "section": "Security incident communication — factual, specific, actionable customer notification with timeline",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-262

```json
{
  "rowId": "EXT-262",
  "sourceIds": ["GOOGLE-AIP"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "idempotency-key-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Our payment creation API is not idempotent. If a client retries a failed POST /payments request, it creates a duplicate payment. Two approaches: (A) AIP-148 idempotency key via X-Request-Id header. (B) Client-generated payment reference ID as a field in the request body.",
    "b": "The client generates a unique ID for each payment attempt. On retry, the same ID is sent. The server must detect the duplicate and return the original response without creating a duplicate payment.",
    "c": "AIP-148 (Idempotency Keys): clients send a unique key in the X-Request-Id (or Idempotency-Key) header. The server stores the key with the result. On retry with the same key, the server returns the cached result without re-executing. The key is separate from the business resource ID (payment ID). Request body field approach: include idempotency_key in the JSON body. Semantically equivalent but body fields are part of the resource representation, which may conflict with partial update semantics."
  },
  "function": "Arbitrate: AIP-148 header key or request body field for idempotency?",
  "acceptedOutput": {
    "verdict": "Use AIP-148 header-based idempotency key (X-Request-Id or Idempotency-Key).",
    "reasoning": "Three reasons to prefer the header approach: (1) Separation of concerns — the idempotency key is a protocol concern (retry deduplication), not a business data concern. It belongs in headers alongside other protocol metadata (Content-Type, Authorization), not in the JSON body that represents the payment. (2) Partial update compatibility — if the API later supports PATCH, a request body field for idempotency_key would appear in the resource schema and might be incorrectly interpreted as a patchable field. (3) AIP-148 convention — clients that know the convention (Stripe, Google Cloud) use the same header name and pattern across APIs, reducing per-API learning.",
    "implementation": "Server: store hash(X-Request-Id + user_id) → {payment_id, status, timestamp}. Expiry: 7–30 days (cover maximum retry window). On duplicate: return 200 with the original response body and the stored payment_id."
  },
  "rejectedOutput": {
    "verdict": "Use a request body field (idempotency_key). It is simpler — the key travels with the payment data and does not require custom header knowledge."
  },
  "arbiters": ["apiIdempotencyArbiter"],
  "whyAccepted": "The header approach separates protocol concerns from resource data. Stripe's Idempotency-Key header is the industry-standard pattern for payment APIs. The 'simpler' argument for body fields breaks down for PATCH requests and creates schema pollution in the payment resource representation.",
  "whyRejected": "A body field for idempotency conflates the retry protocol with the resource schema. The payment JSON schema would include a field (idempotency_key) that is not part of the payment entity — it is a client-generated key for the client's benefit. Headers are the correct place for such client-generated protocol metadata.",
  "sourceAnchors": [
    {
      "sourceId": "GOOGLE-AIP",
      "url": "https://google.aip.dev/148",
      "section": "AIP-148: Idempotency keys — X-Request-Id header vs body field; deduplication window; Stripe pattern",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-263

```json
{
  "rowId": "EXT-263",
  "sourceIds": ["NESTJS-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "nestjs-async-operation-pattern",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Our NestJS application needs to send a welcome email after user registration. The email sending should not block the registration HTTP response. Should we use NestJS EventEmitter (in-process events) or a message queue (RabbitMQ/Kafka) for this?",
    "b": "Current setup: single NestJS application. Email sending uses an external SMTP (Simple Mail Transfer Protocol) provider (200–500ms round trip). Registration endpoint: POST /users. The application is a monolith — there is no separate email service. Expected user registrations: 100/hour.",
    "c": "NestJS EventEmitter (event-emitter module): in-process event bus. Events are handled in the same process — if the process crashes, in-flight events are lost. No persistence. RabbitMQ/Kafka message queue: external process. Messages are persisted — survives process restart. Adds infrastructure dependency (message broker). For 100 registrations/hour (1.67/minute), email delivery is not time-critical."
  },
  "function": "Produce the command: EventEmitter or message queue for welcome email.",
  "acceptedOutput": {
    "command": "Use NestJS EventEmitter for welcome email at 100 registrations/hour.",
    "rationale": "At 100 registrations/hour, email delivery reliability requirements can be met by: (1) In-process EventEmitter for immediate non-blocking dispatch. (2) Retry logic within the event handler (3 retries with backoff for SMTP failures). (3) Logging failures for manual retry. A message queue adds RabbitMQ/Kafka infrastructure, operational overhead (monitoring, queue management), and deployment complexity — disproportionate for a 100/hour single-service scenario.",
    "implementation": "@Injectable()\nexport class UserService {\n  constructor(private eventEmitter: EventEmitter2) {}\n  async register(dto: RegisterDto) {\n    const user = await this.createUser(dto);\n    this.eventEmitter.emit('user.registered', new UserRegisteredEvent(user.id, user.email));\n    return user;\n  }\n}\n\n@OnEvent('user.registered')\nasync handleUserRegistered(event: UserRegisteredEvent) {\n  await this.emailService.sendWelcomeEmail(event.email);  // async, non-blocking to registration\n}",
    "upgradeThreshold": "If registrations grow to 10,000+/hour or the application scales to multiple instances (EventEmitter does not propagate across instances), migrate to a message queue."
  },
  "rejectedOutput": {
    "command": "Use RabbitMQ. Message queues are the correct pattern for any asynchronous operation. Event loss is unacceptable for welcome emails."
  },
  "arbiters": ["asyncOperationPatternArbiter"],
  "whyAccepted": "EventEmitter is appropriate for single-instance applications with low event volume and acceptable reliability through retry logic. The upgrade path to a message queue is clear and not premature — the current scale does not justify the operational overhead.",
  "whyRejected": "'Any asynchronous operation needs a queue' is an over-engineered rule. At 100/hour in a single instance, EventEmitter with retry is appropriate. Adding RabbitMQ for a 100/hour welcome email is introducing significant operational complexity (broker, queue monitoring, dead letter handling) for a non-critical operation.",
  "sourceAnchors": [
    {
      "sourceId": "NESTJS-ADVANCED",
      "url": "https://github.com/nestjs/nest",
      "section": "EventEmitter vs message queue — single-instance 100/hour async email vs RabbitMQ over-engineering",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-264

```json
{
  "rowId": "EXT-264",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "report-generation-with-sections",
  "rowClass": "negative-example",
  "input": {
    "a": "Execute: generate a text report from a list of section dicts. Each section has 'title' and 'content'. The report has a header ('Report: {report_title}'), then sections separated by '---', then a footer ('Generated: {timestamp}'). Line-wrap content at 80 characters.",
    "b": "Function signature: generate_report(report_title: str, sections: list[dict], timestamp: str) -> str. Each section dict: {'title': str, 'content': str}. Line wrap: use textwrap.fill(content, width=80).",
    "c": "Example: generate_report('Q1 Results', [{'title':'Revenue','content':'Revenue grew 15% year over year, driven by...'}], '2026-05-04') should produce: 'Report: Q1 Results\\n\\n## Revenue\\n<wrapped content>\\n\\n---\\n\\nGenerated: 2026-05-04'"
  },
  "function": "Implement generate_report with line-wrapped sections.",
  "acceptedOutput": {
    "implementation": "import textwrap\n\ndef generate_report(report_title: str, sections: list, timestamp: str) -> str:\n    lines = [f'Report: {report_title}', '']\n    for i, section in enumerate(sections):\n        lines.append(f'## {section[\"title\"]}')\n        lines.append(textwrap.fill(section['content'], width=80))\n        if i < len(sections) - 1:  # separator between sections, not after last\n            lines.append('')\n            lines.append('---')\n            lines.append('')\n    lines.append('')\n    lines.append('---')\n    lines.append('')\n    lines.append(f'Generated: {timestamp}')\n    return '\\n'.join(lines)",
    "behavior": "Produces correctly wrapped, separated sections with header and footer. The separator appears between sections AND before the footer."
  },
  "rejectedOutput": {
    "implementation": "def generate_report(report_title, sections, timestamp):\n    report = f'Report: {report_title}\\n\\n'\n    for section in sections:\n        report += f'## {section[\"title\"]}\\n{section[\"content\"]}\\n---\\n'\n    report += f'Generated: {timestamp}'\n    return report"
  },
  "arbiters": ["reportGenerationArbiter"],
  "whyAccepted": "The accepted implementation: (1) applies textwrap.fill for 80-character line wrapping, (2) places section separators between sections (not trailing the last section), (3) adds a final separator before the footer as specified. The intermediate separator logic (i < len(sections) - 1) prevents a trailing separator after the last section before the footer.",
  "whyRejected": "The rejected implementation: (1) does not apply line wrapping (content printed verbatim), (2) places '---' after every section including the last, producing double separators before the footer, (3) omits the blank lines around the separator. The output format is incorrect.",
  "sourceAnchors": [
    {
      "sourceId": "BIGCODEBENCH",
      "url": "https://github.com/bigcode-project/bigcodebench",
      "section": "Task: generate_report — textwrap.fill, section separators, header/footer formatting",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-265

```json
{
  "rowId": "EXT-265",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "accessibility-regression-detection",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Aggregate results from three analysis agents for a pull request that updates the product detail page layout.",
    "b": "Agent results: Agent 1 (Visual Test Agent): PASS — pixel comparison against baseline shows expected layout change. Agent 2 (Accessibility Agent): WARN — the new layout removes aria-label from the 'Add to Cart' button. The button now has no accessible name (only an icon, no text). Agent 3 (Performance Agent): PASS — Lighthouse performance score unchanged.",
    "c": "Accessibility policy: WCAG (Web Content Accessibility Guidelines) 2.1 AA compliance is required. WCAG Success Criterion 4.1.2 (Name, Role, Value): UI components must have a name that assistive technologies can compute. A button with no text and no aria-label has no accessible name — screen readers announce it as 'button' with no description. This is a WCAG 2.1 AA failure."
  },
  "function": "Produce the aggregated result for this pull request.",
  "acceptedOutput": {
    "aggregatedResult": "BLOCK",
    "resultCode": "WCAG_AA_VIOLATION",
    "summary": "The pull request introduces a WCAG 2.1 AA violation. The 'Add to Cart' button has no accessible name after the aria-label was removed — screen reader users hear 'button' with no description. This violates WCAG Success Criterion 4.1.2 (Name, Role, Value) and the project's AA compliance policy.",
    "requiredActions": [
      "Restore the aria-label to the 'Add to Cart' button: <button aria-label='Add Widget to cart'>",
      "Alternatively, add visible text alongside the icon: <button><CartIcon /><span>Add to cart</span></button>",
      "Re-run accessibility audit — zero SC 4.1.2 failures required before merge"
    ],
    "blockingAgent": "Accessibility Agent (WCAG 2.1 AA SC 4.1.2 violation)",
    "passingAgents": ["Visual Test Agent", "Performance Agent"]
  },
  "rejectedOutput": {
    "aggregatedResult": "PASS_WITH_ADVISORY",
    "summary": "Layout and performance pass. An accessibility warning was noted — recommend adding aria-label in a follow-up accessibility PR."
  },
  "arbiters": ["accessibilityComplianceArbiter"],
  "whyAccepted": "WCAG 2.1 AA compliance is a policy requirement. A WCAG SC 4.1.2 violation is not advisory — it is a compliance failure. The 'follow-up accessibility PR' pattern allows a known accessibility regression to reach production, removing an assistive technology user's ability to use the Add to Cart button.",
  "whyRejected": "Treating a WCAG AA violation as PASS_WITH_ADVISORY contradicts the stated compliance policy. An icon-only button with no accessible name is unusable by screen reader users — this is not a cosmetic issue. The 'follow-up PR' deferral is inappropriate for a regression that makes a core e-commerce action inaccessible.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-BENCH",
      "url": "https://github.com/princeton-nlp/SWE-bench",
      "section": "Accessibility regression — BLOCK for WCAG 2.1 AA SC 4.1.2 violation on icon-only button",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-266

```json
{
  "rowId": "EXT-266",
  "sourceIds": ["PYTHON-PEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "sentinel-value-pattern",
  "rowClass": "migration-decision",
  "input": {
    "a": "A Python library uses 'None' as a sentinel for 'value not provided' in several functions: def update_user(name=None, email=None, age=None). This conflicts with legitimate uses of None (e.g., clearing a field: update_user(email=None) should clear the email, but None also means 'not provided'). PEP 661 (Sentinel Values) provides a standard sentinel. Should we adopt it?",
    "b": "PEP 661 status: deferred as of Python 3.12 (not in stdlib). The pattern it formalizes: sentinel = object(); def update_user(name=sentinel, email=sentinel). Check: if email is sentinel: (not provided) vs if email is None: (provided as None, meaning 'clear').",
    "c": "Current codebase: 23 functions use None as a dual-purpose sentinel/value. The None-ambiguity bug has caused 4 production incidents where users intended to clear a field but the function interpreted None as 'not provided'. Alternative: use a module-level sentinel object (object()) without PEP 661."
  },
  "function": "Decide: migrate to sentinel pattern (module-level object()) to resolve None ambiguity.",
  "acceptedOutput": {
    "decision": "MIGRATE to module-level sentinel object — do not wait for PEP 661.",
    "rationale": "PEP 661 is deferred but the pattern it recommends (module-level sentinel = object()) is available now and is the standard pre-PEP idiom. Three reasons to migrate: (1) 4 production incidents caused by None ambiguity — the bug is documented and recurring. (2) 23 functions with the ambiguity = significant API surface. (3) The migration does not require PEP 661 — a module-level sentinel is 2 lines of code. When PEP 661 ships, migration to the stdlib sentinel is minimal.",
    "implementation": "# In a module-level utils.py:\nclass _Sentinel:\n    def __repr__(self): return '<NOT_PROVIDED>'\n    def __bool__(self): return False\nNOT_PROVIDED = _Sentinel()\n\n# In function definitions:\ndef update_user(name=NOT_PROVIDED, email=NOT_PROVIDED, age=NOT_PROVIDED):\n    if email is not NOT_PROVIDED:  # email was explicitly passed\n        user.email = email  # could be None (clear) or a string (update)",
    "gate": "All 23 functions migrated. The 4 documented incident scenarios now behave correctly: update_user(email=None) clears email; update_user() (no email arg) leaves email unchanged."
  },
  "rejectedOutput": {
    "decision": "Wait for PEP 661 to be accepted before adopting the sentinel pattern. The stdlib sentinel will be the canonical solution."
  },
  "arbiters": ["apiDesignArbiter"],
  "whyAccepted": "PEP 661 is deferred with no acceptance timeline. 4 production incidents have occurred in the meantime. The module-level object() sentinel solves the problem now and is the documented pattern prior to PEP 661. Waiting for a deferred PEP while production incidents occur is an incorrect prioritization.",
  "whyRejected": "PEP 661 deferred means no stdlib sentinel is available and no timeline exists. Waiting for a deferred PEP while known production bugs occur is not a valid strategy. The custom sentinel class is 2 lines and forwards-compatible with PEP 661 when it ships.",
  "sourceAnchors": [
    {
      "sourceId": "PYTHON-PEPS",
      "url": "https://peps.python.org/pep-0661/",
      "section": "PEP 661 — Sentinel values; module-level sentinel object() pre-PEP idiom; None ambiguity",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-267

```json
{
  "rowId": "EXT-267",
  "sourceIds": ["ANGULAR-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "angular-signal-vs-rxjs",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We have an Angular 17 component that manages a counter: it can increment, decrement, and reset. The current value is displayed in the template. Should we use an Angular Signal or an RxJS BehaviorSubject for the counter state?",
    "b": "Current code: private _count = new BehaviorSubject<number>(0); count$ = this._count.asObservable(); increment() { this._count.next(this._count.getValue() + 1); }. Template: {{ count$ | async }}",
    "c": "Angular Signals (Angular 17+): writable signals (signal(0)) store values with fine-grained change tracking. Reading a signal in a template ({{ count() }}) establishes a dependency — the template re-renders only when the signal changes. No async pipe required. RxJS BehaviorSubject: imperative stream management. getValue() breaks reactive patterns. Works with Angular async pipe or manual subscribe."
  },
  "function": "Produce the command: Signal or BehaviorSubject for counter state.",
  "acceptedOutput": {
    "command": "Use Angular Signal for counter state.",
    "implementation": "// In component:\ncount = signal(0);\nincrement() { this.count.update(c => c + 1); }\ndecrement() { this.count.update(c => c - 1); }\nreset() { this.count.set(0); }\n\n// In template:\n<span>{{ count() }}</span>\n<button (click)='increment()'>+</button>\n<button (click)='decrement()'>-</button>",
    "rationale": "For synchronous local component state, Signals are simpler than BehaviorSubject: (1) No async pipe in template — signals are synchronous values, not streams. (2) No getValue() — the signal is read with count() in the template and in component methods. (3) Fine-grained change detection — the template only re-renders the {{ count() }} binding when count changes, not the entire template. (4) BehaviorSubject is appropriate for streams (multiple values over time, async operations, operators). A counter is a synchronous state value — it fits the Signal model.",
    "rxjsRetention": "Keep BehaviorSubject/Observable for: async data fetching (HTTP), event streams (WebSocket), state shared across many components via services."
  },
  "rejectedOutput": {
    "command": "Keep BehaviorSubject. RxJS is the established Angular state management pattern. Signals are new and the ecosystem around them is still maturing."
  },
  "arbiters": ["angularStateManagementArbiter"],
  "whyAccepted": "Signals are the Angular team's recommended pattern for synchronous local component state in Angular 17+. For a counter (synchronous read/write state with no async operations), Signal is strictly simpler than BehaviorSubject — no async pipe, no getValue(), no stream semantics for a non-stream value.",
  "whyRejected": "The 'established pattern' argument is valid for RxJS streams where Signal is not applicable (async operations). For synchronous local state, BehaviorSubject is the wrong primitive — it introduces stream semantics for a value that is not a stream. Angular 17 Signals are stable, not experimental.",
  "sourceAnchors": [
    {
      "sourceId": "ANGULAR-DOCS",
      "url": "https://github.com/angular/angular",
      "section": "Signals — signal() for synchronous local state; update/set; vs BehaviorSubject for async streams",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-268

```json
{
  "rowId": "EXT-268",
  "sourceIds": ["REACT-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "react-server-components",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We're building a product catalog page in Next.js 14 with App Router. The page fetches 200 products from a database and renders a product grid. Some items have an 'Add to Cart' button. Should we make the product catalog page a React Server Component or a Client Component?",
    "b": "Next.js 14 App Router: all components are Server Components by default. Client Components require 'use client' directive. Data fetching in Server Components is done directly (no useEffect/useState required). The product grid is read-only. The 'Add to Cart' button needs onClick event handler and cart state.",
    "c": "React Server Components (RSC): run on the server, have direct database access, zero JavaScript shipped for the component to the client, cannot use browser APIs or event handlers. Client Components: run in the browser, can use useState/useEffect/onClick, require JavaScript to be sent to the client."
  },
  "function": "Produce the command: Server Component or Client Component for the product catalog page.",
  "acceptedOutput": {
    "command": "Make the ProductCatalogPage a Server Component. Make only the AddToCartButton a Client Component.",
    "rationale": "Component splitting by interactivity: the product grid (200 products × data fetching) is pure rendering — no user interaction. Server Component renders it with direct database access, no JavaScript shipped to the client for the grid. Only the AddToCartButton requires onClick and cart state — make it a Client Component with 'use client'. This pattern minimizes JavaScript bundle size: 200 product cards as Server Components = 0 JS shipped for the cards; 200 AddToCartButton Client Components = the button JS × 1 (shared).",
    "implementation": "// app/products/page.tsx (Server Component — no directive)\nexport default async function ProductCatalogPage() {\n  const products = await db.products.findMany({ limit: 200 });\n  return <div>{products.map(p => <ProductCard key={p.id} product={p} />)}</div>;\n}\n\n// components/AddToCartButton.tsx (Client Component)\n'use client';\nexport function AddToCartButton({ productId }) {\n  const { addToCart } = useCart();\n  return <button onClick={() => addToCart(productId)}>Add to Cart</button>;\n}"
  },
  "rejectedOutput": {
    "command": "Make the entire page a Client Component with 'use client'. This is simpler — all data fetching and rendering is in one component."
  },
  "arbiters": ["reactServerComponentArbiter"],
  "whyAccepted": "Pushing 'use client' to the leaf components (buttons) maximizes the Server Component surface. A full-page Client Component fetches data via useEffect (adds loading state complexity), ships the entire product grid's JavaScript to the client (unnecessary for static content), and misses the RSC data fetching model.",
  "whyRejected": "A full-page Client Component requires useEffect for data fetching, useState for products, loading state handling, and error handling — replacing the simpler async Server Component fetch. The entire product grid JavaScript is shipped to the client unnecessarily. The 'simpler' claim is incorrect — Client Component data fetching is more complex than Server Component async/await.",
  "sourceAnchors": [
    {
      "sourceId": "REACT-ADVANCED",
      "url": "https://github.com/facebook/react",
      "section": "Server Components vs Client Components — leaf component interactivity; zero-JS for static content",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-269

```json
{
  "rowId": "EXT-269",
  "sourceIds": ["DOTNET-MICROSERVICES"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "health-check-response-format",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Format the health check result for an ASP.NET Core service as an HTTP response. The service checks: (1) Database connection: healthy. (2) Redis cache: degraded (connection timeout). (3) Payment gateway: unhealthy (HTTP 503). HTTP status code and response body format to determine.",
    "b": "The health check endpoint is GET /health. Consumers: Kubernetes liveness/readiness probes, load balancer health checks, and a monitoring dashboard. Kubernetes readiness probe: if the endpoint returns non-2xx, Kubernetes stops routing traffic to the pod.",
    "c": "ASP.NET Core Health Checks middleware: produces a HealthReport with status (Healthy, Degraded, Unhealthy) per check and overall status. Standard response format: { status: 'Degraded', checks: [...] }. HTTP status mapping: Healthy → 200, Degraded → 200 (still serving), Unhealthy → 503. Kubernetes readiness probe: should return non-200 only when the service should stop receiving traffic. A degraded cache should not take the pod out of rotation — it may serve slower but is functional."
  },
  "function": "Produce the formatted health check HTTP response.",
  "acceptedOutput": {
    "httpStatus": 503,
    "contentType": "application/json",
    "body": {
      "status": "Unhealthy",
      "totalDuration": "00:00:01.234",
      "checks": [
        {
          "name": "database",
          "status": "Healthy",
          "duration": "00:00:00.012",
          "description": "SQL Server connection established"
        },
        {
          "name": "redis",
          "status": "Degraded",
          "duration": "00:00:00.890",
          "description": "Redis connection timeout — cache operating in degraded mode"
        },
        {
          "name": "payment-gateway",
          "status": "Unhealthy",
          "duration": "00:00:00.332",
          "description": "Payment gateway returned HTTP 503"
        }
      ]
    },
    "rationale": "Overall status is Unhealthy because at least one check is Unhealthy (payment gateway). HTTP 503 signals Kubernetes readiness probe to stop routing traffic to this pod — a service with no payment gateway is unable to process payments. The database is healthy. Redis degraded is noted but does not cause Unhealthy overall on its own."
  },
  "rejectedOutput": {
    "httpStatus": 200,
    "body": "OK"
  },
  "arbiters": ["healthCheckFormatArbiter"],
  "whyAccepted": "The structured health check response provides per-component status to the monitoring dashboard and the Kubernetes probe. HTTP 503 correctly signals readiness probe failure for a payment-processing service with no payment gateway. The degraded Redis status is visible in the response without causing a false readiness failure.",
  "whyRejected": "A plain 200 OK response hides that the payment gateway is down. The monitoring dashboard has no visibility into component status. Kubernetes continues routing payment requests to a pod that cannot process payments. The readiness probe is supposed to prevent this.",
  "sourceAnchors": [
    {
      "sourceId": "DOTNET-MICROSERVICES",
      "url": "https://github.com/dotnet-architecture/eShopOnContainers",
      "section": "Health check endpoint — per-component status, HTTP 503 for Unhealthy, Degraded stays 200, Kubernetes readiness",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-270

```json
{
  "rowId": "EXT-270",
  "sourceIds": ["IETF-RFCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "rfc7235-www-authenticate",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Format this authentication failure result as an HTTP 401 response: the request to GET /api/orders was rejected because the Bearer token in the Authorization header has expired.",
    "b": "Two format candidates: (A) HTTP 401 with body: { 'error': 'token_expired', 'message': 'Your access token has expired. Please refresh it.' }. (B) HTTP 401 with WWW-Authenticate header per RFC 7235 and body in RFC 6750 Bearer token error format.",
    "c": "RFC 7235: 401 Unauthorized responses MUST include a WWW-Authenticate header that specifies the authentication scheme required. RFC 6750 (Bearer Token Usage): WWW-Authenticate: Bearer realm='api', error='invalid_token', error_description='The access token expired'. The error parameter values are standardized: invalid_token, insufficient_scope, invalid_request. Clients that implement RFC 6750 parse the WWW-Authenticate header to determine the error type and can automatically refresh the token."
  },
  "function": "Produce the formatted HTTP 401 response for the expired token.",
  "acceptedOutput": {
    "httpStatus": 401,
    "headers": {
      "WWW-Authenticate": "Bearer realm=\"api\", error=\"invalid_token\", error_description=\"The access token has expired\"",
      "Content-Type": "application/json"
    },
    "body": {
      "error": "invalid_token",
      "error_description": "The access token has expired. Use the refresh token to obtain a new access token.",
      "docs_url": "https://api.example.com/docs/auth/token-refresh"
    },
    "rationale": "RFC 7235 requires WWW-Authenticate on 401 responses. RFC 6750 standardizes 'invalid_token' for expired/revoked tokens. OAuth2 clients that implement automatic token refresh parse the WWW-Authenticate header: if error='invalid_token', trigger refresh flow. The body provides the same information for clients that inspect the response body. docs_url links to the token refresh documentation."
  },
  "rejectedOutput": {
    "httpStatus": 401,
    "body": {
      "error": "token_expired",
      "message": "Your access token has expired. Please refresh it."
    }
  },
  "arbiters": ["httpAuthFormatArbiter", "rfcComplianceArbiter"],
  "whyAccepted": "The WWW-Authenticate header is required by RFC 7235 for 401 responses. The 'invalid_token' error code is RFC 6750 standard — OAuth2 clients use it for automatic token refresh decisions. The custom 'token_expired' code in the rejected output is not an RFC 6750 standard value — clients implementing the standard will not recognize it.",
  "whyRejected": "The rejected output omits the WWW-Authenticate header (violating RFC 7235) and uses a non-standard error code ('token_expired' vs RFC 6750 'invalid_token'). Clients implementing RFC 6750 automatic token refresh will not trigger the refresh flow because the error code is not in the standard set.",
  "sourceAnchors": [
    {
      "sourceId": "IETF-RFCS",
      "url": "https://www.rfc-editor.org/rfc/rfc7235",
      "section": "RFC 7235: WWW-Authenticate header required on 401; RFC 6750 invalid_token error for expired Bearer token",
      "license": "IETF Trust provisions"
    }
  ],
  "licenseConstraint": "IETF Trust provisions",
  "trainingStatus": "candidate"
}
```
