# External Training Rows — Batch 14
## Row IDs: EXT-271 through EXT-290
## Date: 2026-05-04
## Focus: Helm test hooks, Prometheus recording rules, PDB, Vue Pinia, TypedDict, React context, compiled queries, additional families 1/2/3/4/5/6/7/8

---

## EXT-271

```json
{
  "rowId": "EXT-271",
  "sourceIds": ["HELM-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "helm-test-hook-validation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "After deploying a Helm chart, we want to verify the deployment succeeded by running a connectivity test (HTTP GET to /health returns 200). Should we use a Helm test hook or a manual kubectl check?",
    "b": "Current validation: after helm upgrade, the operator manually runs 'kubectl get pods' to check pod status, then curls the health endpoint. This works but is not automated.",
    "c": "Helm test hooks: a Kubernetes Job defined in the chart's templates/tests/ directory with the annotation 'helm.sh/hook: test'. Run via 'helm test release-name'. The Job runs in the cluster, executes the test command, and exits 0 (success) or non-zero (failure). Helm test results are visible in 'helm test' output. Test hooks run inside the cluster — they can reach internal service endpoints."
  },
  "function": "Arbitrate: Helm test hook or manual check?",
  "acceptedOutput": {
    "verdict": "Use a Helm test hook.",
    "implementation": "# templates/tests/connectivity-test.yaml\napiVersion: v1\nkind: Pod\nmetadata:\n  name: {{ .Release.Name }}-connectivity-test\n  annotations:\n    helm.sh/hook: test\n    helm.sh/hook-delete-policy: before-hook-creation,hook-succeeded\nspec:\n  restartPolicy: Never\n  containers:\n    - name: connectivity-test\n      image: curlimages/curl:latest\n      command: ['curl', '-f', 'http://{{ .Release.Name }}-service/health']\n",
    "rationale": "Helm test hooks run inside the cluster — they test the actual service endpoint using internal DNS ({{ .Release.Name }}-service) which may not be reachable from outside. Manual curl requires external access or kubectl port-forward setup. 'helm test' produces pass/fail output consumable by CI/CD. hook-delete-policy: hook-succeeded cleans up the test pod after a successful test."
  },
  "rejectedOutput": {
    "verdict": "Use manual kubectl checks. Helm test hooks add complexity. The operator knows how to check pod status and health endpoints manually."
  },
  "arbiters": ["helmValidationArbiter"],
  "whyAccepted": "Helm test hooks automate the post-deployment validation. In CI/CD pipelines, 'helm upgrade && helm test' provides automated pass/fail signal. Manual checks are not repeatable by automated systems. The test pod runs inside the cluster and can validate internal service connectivity that external curl cannot.",
  "whyRejected": "Manual checks require human availability and consistent execution. In a CI/CD pipeline that runs 10 deployments per day, manual validation is a bottleneck and a source of inconsistency. Helm test hooks make the validation automated, repeatable, and machine-readable.",
  "sourceAnchors": [
    {
      "sourceId": "HELM-DOCS",
      "url": "https://github.com/helm/helm",
      "section": "Helm test hooks — helm.sh/hook: test; connectivity-test Pod; hook-delete-policy",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-272

```json
{
  "rowId": "EXT-272",
  "sourceIds": ["PROMETHEUS-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "prometheus-recording-rule-adoption",
  "rowClass": "migration-decision",
  "input": {
    "a": "Our Prometheus deployment has an alert rule and dashboard that use the same expensive PromQL expression: sum(rate(http_requests_total[5m])) by (service). This query is evaluated every 15 seconds for the alert and every 30 seconds for the dashboard — total of 4 evaluations per minute. Each evaluation scans all time series for http_requests_total. Should we create a recording rule?",
    "b": "http_requests_total has 50,000 active time series across all services. Evaluating rate() over a 5-minute window on 50,000 series every 15 seconds is computationally expensive. Prometheus query evaluation latency for this query: 800ms.",
    "c": "Prometheus recording rules: pre-compute expensive expressions and store the result as a new metric. 'record: job:http_requests:rate5m; expr: sum(rate(http_requests_total[5m])) by (service)'. The result is stored as a new metric (job:http_requests:rate5m) updated at the rule evaluation interval (typically 1 minute). Alerts and dashboards query the pre-computed metric (instant lookup) instead of scanning all time series."
  },
  "function": "Decide: migrate the expensive query to a recording rule.",
  "acceptedOutput": {
    "decision": "MIGRATE to a recording rule.",
    "rationale": "At 4 evaluations/minute × 800ms each = 3.2 seconds of CPU per minute on this one query. A recording rule pre-computes once per minute (evaluation interval). Alerts and dashboards query the pre-computed metric — instant lookup instead of 800ms scan. Result: 1 × 800ms per minute (recording rule evaluation) vs 4 × 800ms per minute (direct evaluation). CPU reduction: 75% for this query.",
    "recordingRule": "groups:\n  - name: http_requests_rates\n    interval: 60s\n    rules:\n      - record: job:http_requests:rate5m\n        expr: sum(rate(http_requests_total[5m])) by (service)",
    "updateAlertAndDashboard": "Replace sum(rate(http_requests_total[5m])) by (service) with job:http_requests:rate5m in the alert expr and dashboard panel queries.",
    "gate": "Prometheus evaluation latency for job:http_requests:rate5m instant query: < 10ms (pre-computed). Alert and dashboard panels load correctly with the new metric name."
  },
  "rejectedOutput": {
    "decision": "Do not create a recording rule. The query evaluation is fast enough (800ms). Prometheus can handle it."
  },
  "arbiters": ["prometheusPerformanceArbiter"],
  "whyAccepted": "800ms per evaluation is not 'fast enough' when evaluated 4 times per minute — it accumulates to significant Prometheus CPU consumption. Recording rules are the standard Prometheus mechanism for this exact optimization. The migration is low-risk: the recording rule produces an equivalent metric, and the alert/dashboard simply reference the new metric name.",
  "whyRejected": "The rejected verdict treats 800ms as an absolute performance metric without considering evaluation frequency. 3.2 seconds of CPU per minute for one query, when Prometheus evaluates hundreds of rules, degrades overall Prometheus performance. Recording rules are the documented solution for this exact scenario.",
  "sourceAnchors": [
    {
      "sourceId": "PROMETHEUS-DOCS",
      "url": "https://github.com/prometheus/prometheus",
      "section": "Recording rules — pre-computed metrics; evaluation interval; expensive rate() query optimization",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-273

```json
{
  "rowId": "EXT-273",
  "sourceIds": ["KUBERNETES-KEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "pod-disruption-budget",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose this resilience requirement: our API service has 5 replicas. During cluster maintenance (node drains), we must ensure at least 3 replicas remain available at all times. Cluster admins report that node drains sometimes take all 5 pods offline simultaneously.",
    "b": "Current Kubernetes Deployment: replicas: 5, no PDB (Pod Disruption Budget) configured. When a node is drained for maintenance, all pods on that node are evicted simultaneously — if 3 pods are on one node, all 3 are evicted at once, leaving only 2 running.",
    "c": "Kubernetes PDB (Pod Disruption Budget): defines the minimum number of pods that must remain available during voluntary disruptions (node drains, cluster upgrades). 'minAvailable: 3' prevents the cluster admin from draining a node if doing so would bring available pods below 3. Cluster admin must wait for pods to reschedule before draining additional nodes."
  },
  "function": "Produce the action items for implementing the resilience requirement.",
  "acceptedOutput": {
    "actionItems": [
      "Action 1: Create a PodDisruptionBudget: apiVersion: policy/v1; kind: PodDisruptionBudget; metadata: { name: api-service-pdb }; spec: { minAvailable: 3; selector: { matchLabels: { app: api-service } } }",
      "Action 2: Verify the PDB is attached to the correct pods: kubectl get pdb api-service-pdb -o yaml — confirm selector matches the Deployment pod labels.",
      "Action 3: Test by simulating a node drain: kubectl drain <node-name> --ignore-daemonsets. The drain should pause if draining would violate minAvailable: 3. kubectl get pdb will show 'DISRUPTIONS ALLOWED: 2' (can drain up to 2 pods while keeping 3 running).",
      "Action 4: For rolling updates (Deployment update strategy), set maxUnavailable: 1 in the Deployment spec — this limits rolling update disruptions to 1 pod at a time, complementing the PDB.",
      "Action 5: Ensure pods are distributed across nodes with anti-affinity: spec.affinity.podAntiAffinity with topologyKey: kubernetes.io/hostname — prevents all 5 pods landing on one node, ensuring PDB enforcement is meaningful."
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Increase replicas to 8. With more replicas, even if 3 are evicted, 5 remain available. No PDB needed."
    ]
  },
  "arbiters": ["kubernetesResilienceArbiter"],
  "whyAccepted": "PDB is the Kubernetes mechanism for expressing availability constraints during voluntary disruptions. It enforces the constraint at the cluster level — the cluster admin cannot violate it. Simply increasing replicas does not prevent simultaneous eviction of all pods on a node — with 8 replicas and 6 on one node, 6 are still evicted.",
  "whyRejected": "Increasing replicas without a PDB does not solve the problem — if 4+ pods are on one node and that node is drained, 4+ are evicted simultaneously. The rejected solution treats availability as a function of replica count alone, ignoring the topology distribution and drain behavior.",
  "sourceAnchors": [
    {
      "sourceId": "KUBERNETES-KEPS",
      "url": "https://github.com/kubernetes/enhancements",
      "section": "PodDisruptionBudget — minAvailable, voluntary disruptions, node drain enforcement",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-274

```json
{
  "rowId": "EXT-274",
  "sourceIds": ["VUE-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "vue3-state-management",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We're building a new Vue 3 application with a shopping cart, user authentication state, and product filter preferences. Which state management library should we use: Pinia or Vuex 4?",
    "b": "The application is Vue 3 only — no Vue 2 support needed. Team has 3 developers: one familiar with Vuex 3 from a Vue 2 project, two new to Vue state management. The application will have 15–20 components.",
    "c": "Pinia: official Vue 3 state management library (recommended by the Vue team). Composition API-based: stores are defined with defineStore(). TypeScript inference works out of the box. No mutations — state is modified directly in actions. Devtools integration. Vuex 4: Vue 3-compatible but maintains Vuex 3 API (state, getters, mutations, actions). Mutations required — more verbose. TypeScript support is manual. Pinia is the official successor to Vuex."
  },
  "function": "Produce the command: Pinia or Vuex 4.",
  "acceptedOutput": {
    "command": "Use Pinia.",
    "rationale": "Pinia is the Vue-team-recommended state management library for Vue 3. Three advantages for this team: (1) No mutations — Vuex required action → mutation → state update for any state change. Pinia allows direct state modification in actions (no mutations required). This reduces boilerplate significantly. (2) TypeScript — Pinia's store type is inferred automatically; Vuex 4 requires manual type augmentation. (3) The team member with Vuex 3 experience will find Pinia's concepts familiar (stores, actions, getters) but simpler. The 'familiar with Vuex 3' is not a reason to use Vuex 4 — Pinia solves the same problems with less code.",
    "exampleStore": "// stores/cart.ts\nexport const useCartStore = defineStore('cart', () => {\n  const items = ref<CartItem[]>([]);\n  const itemCount = computed(() => items.value.length);\n  function addItem(item: CartItem) { items.value.push(item); }\n  return { items, itemCount, addItem };\n});"
  },
  "rejectedOutput": {
    "command": "Use Vuex 4. One team member already knows Vuex — the team has existing Vuex knowledge. Reusing that knowledge reduces onboarding time."
  },
  "arbiters": ["vuexStateManagementArbiter"],
  "whyAccepted": "Pinia is the official recommendation for Vue 3 state management. The Vuex 3 familiarity is partially transferable (same concepts: store, actions, getters) but Pinia is simpler. The two new developers will find Pinia easier to learn than Vuex's mutation boilerplate.",
  "whyRejected": "Vuex 4's primary advantage is API familiarity for Vuex 3 users. That familiarity is partial — Vuex 4 has the same mutation requirement and manual TypeScript augmentation as Vuex 3. Pinia's simpler API and TypeScript inference provide more value to new Vue developers than Vuex 4's compatibility.",
  "sourceAnchors": [
    {
      "sourceId": "VUE-DOCS",
      "url": "https://github.com/vuejs/core",
      "section": "Pinia vs Vuex — official Vue 3 state management; no mutations; TypeScript inference",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-275

```json
{
  "rowId": "EXT-275",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "compound-interest-calculation",
  "rowClass": "negative-example",
  "input": {
    "a": "Execute: calculate compound interest. Formula: A = P * (1 + r/n)^(n*t), where P = principal, r = annual interest rate (decimal), n = compounding periods per year, t = time in years. Return the final amount A and the interest earned (A - P).",
    "b": "Function signature: calculate_compound_interest(principal: float, annual_rate: float, periods_per_year: int, years: float) -> dict. Return: { 'final_amount': float, 'interest_earned': float }. Input validation: principal and annual_rate must be positive. periods_per_year must be >= 1. years must be > 0.",
    "c": "Example: principal=1000, annual_rate=0.05 (5%), periods_per_year=12, years=3. A = 1000 * (1 + 0.05/12)^(12*3) = 1000 * (1.004167)^36 = 1161.62. interest_earned = 161.62."
  },
  "function": "Implement calculate_compound_interest with input validation.",
  "acceptedOutput": {
    "implementation": "def calculate_compound_interest(principal: float, annual_rate: float, periods_per_year: int, years: float) -> dict:\n    if principal <= 0:\n        raise ValueError(f'principal must be positive, got {principal}')\n    if annual_rate <= 0:\n        raise ValueError(f'annual_rate must be positive, got {annual_rate}')\n    if periods_per_year < 1:\n        raise ValueError(f'periods_per_year must be >= 1, got {periods_per_year}')\n    if years <= 0:\n        raise ValueError(f'years must be > 0, got {years}')\n    rate_per_period = annual_rate / periods_per_year\n    n_periods = periods_per_year * years\n    final_amount = principal * (1 + rate_per_period) ** n_periods\n    return {\n        'final_amount': round(final_amount, 2),\n        'interest_earned': round(final_amount - principal, 2)\n    }",
    "behavior": "For principal=1000, annual_rate=0.05, periods_per_year=12, years=3: final_amount=1161.62, interest_earned=161.62."
  },
  "rejectedOutput": {
    "implementation": "def calculate_compound_interest(P, r, n, t):\n    A = P * (1 + r/n) ** (n*t)\n    return A"
  },
  "arbiters": ["financialCalculationArbiter"],
  "whyAccepted": "The accepted implementation: (1) validates all inputs with descriptive ValueError messages, (2) uses descriptive parameter names, (3) returns both final_amount and interest_earned as specified, (4) rounds to 2 decimal places (financial convention). Negative or zero values for financial parameters are caught before the formula runs.",
  "whyRejected": "The rejected implementation: (1) uses single-letter variable names (P, r, n, t — not the function parameter names from the signature), (2) returns only the final amount (not interest_earned as specified), (3) has no input validation (negative principal or zero periods produces incorrect results silently), (4) does not round the result.",
  "sourceAnchors": [
    {
      "sourceId": "BIGCODEBENCH",
      "url": "https://github.com/bigcode-project/bigcodebench",
      "section": "Task: calculate_compound_interest — input validation, return dict with both values, rounding",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-276

```json
{
  "rowId": "EXT-276",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "F_REGISTRY_STORAGE",
  "subdomain": "license-violation-detection",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Aggregate results from three analysis agents for a pull request that adds a new utility library to an open-source Apache-2.0 project.",
    "b": "Agent results: Agent 1 (License Agent): BLOCK — the new dependency 'fast-compute@2.1.0' uses AGPL-3.0 license. The project is Apache-2.0. AGPL-3.0 requires that any network-accessible software using AGPL code must release its source under AGPL. Agent 2 (Test Agent): PASS — all tests pass. Agent 3 (Dependency Audit Agent): PASS — no known CVEs in the dependency.",
    "c": "License compatibility: Apache-2.0 is a permissive license. AGPL-3.0 is a copyleft license with network use clause. Including AGPL-3.0 code in an Apache-2.0 project would require the entire project to be relicensed to AGPL-3.0 (or a compatible copyleft license) due to the copyleft viral nature. This is not compatible with the project's Apache-2.0 license. Network use clause: any service that runs the software over a network must provide source code — this affects SaaS users of the library."
  },
  "function": "Produce the aggregated result for this pull request.",
  "acceptedOutput": {
    "aggregatedResult": "BLOCK",
    "resultCode": "LICENSE_INCOMPATIBILITY",
    "summary": "The pull request introduces AGPL-3.0 licensed dependency 'fast-compute@2.1.0' which is incompatible with the project's Apache-2.0 license. Including AGPL-3.0 code requires the entire project to be relicensed to AGPL-3.0, which would change the project's license for all users. This is a legal issue, not a preference — the PR cannot be merged with this dependency.",
    "requiredActions": [
      "Find an Apache-2.0, MIT, or BSD-licensed alternative to fast-compute",
      "If no alternative exists, implement the required functionality without the AGPL dependency",
      "If the project maintainers wish to use fast-compute, they must relicense the project — this requires consent from all prior contributors"
    ],
    "blockingAgent": "License Agent (AGPL-3.0 incompatibility with Apache-2.0)",
    "passingAgents": ["Test Agent", "Dependency Audit Agent"]
  },
  "rejectedOutput": {
    "aggregatedResult": "PASS_WITH_ADVISORY",
    "summary": "Tests pass and no CVEs. A license advisory noted — AGPL-3.0 has copyleft requirements. Recommend legal review."
  },
  "arbiters": ["licenseComplianceArbiter"],
  "whyAccepted": "AGPL-3.0 incompatibility with Apache-2.0 is not advisory — it is a legal constraint that prevents the PR from being merged without license change. PASS_WITH_ADVISORY implies the code can be merged pending review. It cannot — merging the code changes the licensing obligations for all users of the project.",
  "whyRejected": "The rejected verdict defers a legal blocker as advisory. 'Recommend legal review' after merging is too late — the AGPL code is in the codebase and the license obligation has been triggered. License incompatibility is a BLOCK, not a WARN.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-BENCH",
      "url": "https://github.com/princeton-nlp/SWE-bench",
      "section": "License incompatibility detection — BLOCK for AGPL-3.0 in Apache-2.0 project",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-277

```json
{
  "rowId": "EXT-277",
  "sourceIds": ["OPENTELEMETRY-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "structured-logging-migration",
  "rowClass": "migration-decision",
  "input": {
    "a": "A Python service uses printf-style debug logging: logger.debug('Processing order %s for user %s, amount=%f', order_id, user_id, amount). The team is adding OpenTelemetry and wants to improve log searchability. Should we migrate to structured logging?",
    "b": "Current log output: '2026-05-04 10:30:00 DEBUG Processing order ord-123 for user usr-456, amount=99.99'. Log aggregation: ELK Stack (Elasticsearch, Logstash, Kibana). Currently, searching for specific order IDs requires full-text search: logs WHERE message CONTAINS 'ord-123'. Structured logs would allow: logs WHERE order_id = 'ord-123'.",
    "c": "Python structured logging: the 'structlog' library or standard logging with JSON formatter. OpenTelemetry Logging SDK exports log records to an OTLP (OpenTelemetry Protocol) collector with structured attributes. OTel log record: { body: 'Processing order', attributes: { order_id: 'ord-123', user_id: 'usr-456', amount: 99.99 } }. Elasticsearch can index the attributes separately for field-level queries."
  },
  "function": "Decide: migrate to structured logging.",
  "acceptedOutput": {
    "decision": "MIGRATE to structured logging with OpenTelemetry.",
    "rationale": "Three concrete improvements: (1) Field-level queries — 'order_id = ord-123' finds all logs for that order without text parsing. printf logs require CONTAINS or regex, which is slower and error-prone. (2) Trace correlation — OTel structured logs include trace_id and span_id automatically when generated within a traced request. Correlating a debug log with its trace is automatic. (3) Alerting — Kibana alerts on 'amount > 10000' are not possible with printf string logs; structured amount field enables this.",
    "migrationPattern": "import structlog; logger = structlog.get_logger(). Replace: logger.debug('Processing order %s for user %s', order_id, user_id) → logger.debug('Processing order', order_id=order_id, user_id=user_id, amount=amount). Each keyword argument becomes a structured field.",
    "gate": "ELK query 'order_id: ord-123' returns the log. Trace correlation: log shows trace_id matching the parent OTel span. Zero printf-style log statements in the migrated module."
  },
  "rejectedOutput": {
    "decision": "Keep printf logging. It is universally readable in log files. Structured logging adds library dependencies and changes the log format that on-call engineers are used to reading."
  },
  "arbiters": ["observabilityModernizationArbiter"],
  "whyAccepted": "Field-level searchability and trace correlation are operational improvements that directly reduce mean time to diagnose incidents. The 'on-call engineers are used to reading printf logs' argument is a one-time adaptation cost. After migration, on-call engineers can search for order_id directly instead of parsing log strings.",
  "whyRejected": "Printf logs are universally readable as strings but not universally queryable as structured data. In an ELK stack designed for structured queries, printf logs are first-class citizens only if parsed with Logstash grok patterns — which is fragile. The 'library dependency' concern is addressed by Python's built-in logging module JSON formatter or structlog.",
  "sourceAnchors": [
    {
      "sourceId": "OPENTELEMETRY-SPEC",
      "url": "https://github.com/open-telemetry/opentelemetry-specification",
      "section": "OTel Log data model — structured attributes vs printf strings; trace_id correlation; ELK field queries",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-278

```json
{
  "rowId": "EXT-278",
  "sourceIds": ["EFCORE-SOURCE"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "efcore-compiled-query",
  "rowClass": "migration-decision",
  "input": {
    "a": "An ASP.NET Core endpoint GET /api/products/{id} is called 10,000 times per minute. EF Core profiling shows that 15% of the response time is spent on query compilation (translating LINQ to SQL). Should we migrate to EF Core compiled queries?",
    "b": "Current query: var product = await _context.Products.Where(p => p.Id == id && p.IsActive).Include(p => p.Category).FirstOrDefaultAsync(). EF Core compiles this LINQ expression to SQL on each execution. Compilation time: 2ms per call. At 10,000 calls/minute: 20 seconds of compilation time per minute.",
    "c": "EF Core compiled queries (EF.CompileAsyncQuery): compile the LINQ expression once and store the compiled query as a static delegate. Subsequent executions use the pre-compiled SQL — zero compilation overhead per call. Compiled queries are defined statically: private static readonly Func<AppDbContext, int, Task<Product?>> GetActiveProductById = EF.CompileAsyncQuery((ctx, id) => ctx.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id && p.IsActive));"
  },
  "function": "Decide: migrate the high-frequency query to a compiled query.",
  "acceptedOutput": {
    "decision": "MIGRATE to EF Core compiled query.",
    "rationale": "15% of response time spent on LINQ-to-SQL compilation at 10,000 calls/minute is a quantified performance loss. Compiled query eliminates 2ms of compilation overhead per call. At 10,000/minute: 20 seconds of CPU per minute saved. For an endpoint under heavy load, this 15% improvement directly reduces response time and server CPU.",
    "implementation": "private static readonly Func<AppDbContext, int, Task<Product?>> GetActiveProductById =\n    EF.CompileAsyncQuery((AppDbContext ctx, int id) =>\n        ctx.Products\n           .Include(p => p.Category)\n           .FirstOrDefault(p => p.Id == id && p.IsActive));\n\n// Usage in endpoint handler:\nvar product = await GetActiveProductById(_context, id);",
    "limitations": "Compiled queries cannot use dynamic filters (the query shape must be fixed). If the endpoint needs optional filter parameters (e.g., nullable category), a compiled query requires a separate compiled variant per filter combination. For dynamic queries, use standard EF Core with SQL caching."
  },
  "rejectedOutput": {
    "decision": "Do not use compiled queries. The 2ms compilation time is negligible. EF Core's internal query cache already caches compiled queries across calls."
  },
  "arbiters": ["efCorePerformanceArbiter"],
  "whyAccepted": "EF Core's internal query cache caches compiled queries per DbContext instance or per query shape — but the cache lookup and LINQ expression tree re-evaluation still occur per call. EF.CompileAsyncQuery eliminates even the cache lookup overhead. At 10,000/minute with 15% response time on compilation, the improvement is measurable and significant.",
  "whyRejected": "The EF Core internal cache does reduce compilation cost but does not eliminate it entirely — the LINQ expression tree must still be parsed and the cache key computed per call. The rejected verdict's 'EF Core already caches' claim is partially true but does not address the 15% compilation overhead that profiling shows.",
  "sourceAnchors": [
    {
      "sourceId": "EFCORE-SOURCE",
      "url": "https://github.com/dotnet/efcore",
      "section": "Compiled queries — EF.CompileAsyncQuery; 15% compilation overhead at 10k calls/min",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-279

```json
{
  "rowId": "EXT-279",
  "sourceIds": ["GOOGLE-AIP"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "soft-delete-vs-hard-delete",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Our user management API implements DELETE /users/{id} by immediately removing the row from the database. The compliance team requires that user data be retained for 30 days after deletion for regulatory audit. Should we implement soft delete or change the API response?",
    "b": "Current implementation: DELETE /users/{id} → DELETE FROM Users WHERE Id = @id. Returns 204 No Content on success. The endpoint immediately destroys data.",
    "c": "AIP-135 (Delete): Google AIP recommends considering soft delete when: (1) resources may be recovered, (2) dependent resources are affected, (3) audit requirements exist. Soft delete: add a 'deleted_at' timestamp column. DELETE /users/{id} sets deleted_at = now() instead of removing the row. Resources with deleted_at set are excluded from GET /users and LIST /users queries. After 30 days, a background job permanently deletes. The API caller experience is identical — 204 returned, resource 'disappears' from queries."
  },
  "function": "Arbitrate: soft delete or hard delete with separate retention store?",
  "acceptedOutput": {
    "verdict": "Implement soft delete.",
    "rationale": "Three requirements favor soft delete: (1) 30-day retention for audit — soft delete keeps the row in place with deleted_at set. Hard delete with a separate retention store requires copying data to an archive table or external store before deletion, adding complexity. (2) Recovery — soft delete allows undelete within 30 days if a user deletion was accidental. Hard delete requires restoring from backup. (3) Referential integrity — if other tables reference Users (Orders.UserId, Comments.UserId), soft-deleting the User row preserves foreign key relationships. Hard delete cascades or requires nulling foreign keys.",
    "implementation": "ALTER TABLE Users ADD DeletedAt DATETIME2 NULL; -- Soft delete endpoint: UPDATE Users SET DeletedAt = GETUTCDATE() WHERE Id = @id; -- Query filter: WHERE DeletedAt IS NULL; -- Background job (30 days): DELETE FROM Users WHERE DeletedAt < DATEADD(DAY, -30, GETUTCDATE());"
  },
  "rejectedOutput": {
    "verdict": "Keep hard delete but copy the row to a UserAuditArchive table before deletion. This meets the audit requirement without soft delete complexity."
  },
  "arbiters": ["dataRetentionArbiter"],
  "whyAccepted": "Soft delete is simpler than hard delete + archive table: one schema change vs two tables, no copy-before-delete logic, natural undelete support. The archive table approach duplicates the User schema into an audit table — any User schema change requires updating both tables.",
  "whyRejected": "An archive table requires: a UserAuditArchive table (mirrors User schema), INSERT INTO UserAuditArchive before DELETE, schema synchronization between Users and UserAuditArchive, and a separate cleanup job. Soft delete achieves the same result with one column addition and no data movement.",
  "sourceAnchors": [
    {
      "sourceId": "GOOGLE-AIP",
      "url": "https://google.aip.dev/135",
      "section": "AIP-135: Delete — soft delete with deleted_at vs hard delete with archive; 30-day retention",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-280

```json
{
  "rowId": "EXT-280",
  "sourceIds": ["NESTJS-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "nestjs-exception-filter",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Review this NestJS service method that throws a custom exception.",
    "b": "Implementation A: throw new Error('Product not found'); Implementation B: throw new NotFoundException('Product not found'); Implementation C: throw new HttpException({ message: 'Product not found', errorCode: 'PRODUCT_NOT_FOUND' }, HttpStatus.NOT_FOUND);",
    "c": "NestJS exception handling: the built-in HttpExceptionFilter catches HttpException and its subclasses, returning a JSON response with the status code. A plain Error is not caught by HttpExceptionFilter — it propagates as a 500 Internal Server Error. NotFoundException is a NestJS built-in HttpException subclass with status 404. HttpException with a custom body allows additional fields (errorCode) in the error response."
  },
  "function": "Arbitrate: which implementation is correct for a product not found scenario?",
  "acceptedOutput": {
    "verdict": "Implementation C is optimal; Implementation B is acceptable; Implementation A is incorrect.",
    "reasoning": {
      "implementationA": "INCORRECT — throw new Error() is not an HttpException. The NestJS HttpExceptionFilter does not catch plain Error objects — they propagate as 500 Internal Server Error. A missing product is a 404, not a 500. Exposing an internal Error stack trace to API clients is a security concern (CWE-200).",
      "implementationB": "ACCEPTABLE — NotFoundException is an HttpException subclass with HTTP 404. The default response body is { statusCode: 404, message: 'Product not found', error: 'Not Found' }. Suitable when the client needs only the standard error format.",
      "implementationC": "OPTIMAL — HttpException with custom body allows including 'errorCode: PRODUCT_NOT_FOUND' for programmatic client handling. Clients can switch on errorCode without parsing the message string. The custom body is typed and consistent."
    }
  },
  "rejectedOutput": {
    "verdict": "All three are equivalent — NestJS catches any thrown exception and returns an appropriate HTTP response."
  },
  "arbiters": ["nestjsExceptionArbiter"],
  "whyAccepted": "NestJS does NOT catch plain Error objects with its built-in HttpExceptionFilter. The rejected verdict contains a factual error. throw new Error() produces a 500 response with an internal error message visible in the response body — a security defect for a business-logic error (product not found).",
  "whyRejected": "NestJS's global exception filter catches HttpException subclasses by default. Plain Error objects bypass HttpExceptionFilter and are returned as 500 Internal Server Error. The rejected verdict's claim that 'NestJS catches any thrown exception' is incorrect for the default configuration.",
  "sourceAnchors": [
    {
      "sourceId": "NESTJS-ADVANCED",
      "url": "https://github.com/nestjs/nest",
      "section": "Exception filters — HttpExceptionFilter catches HttpException subclasses; plain Error becomes 500",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-281

```json
{
  "rowId": "EXT-281",
  "sourceIds": ["OPENASSISTANT-OASST1"],
  "sourceTrustTier": 2,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "disk-space-alert-communication",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Translate this machine result into a user-facing alert for the on-call engineer: { alertName: 'DiskSpaceCritical', severity: 'critical', host: 'db-prod-02', mountPoint: '/var/lib/postgresql', currentUsagePercent: 94, thresholdPercent: 90, bytesAvailable: 6442450944, rateOfGrowthBytesPerHour: 2147483648, estimatedTimeToFull: 'PT3H', timestamp: '2026-05-04T18:00:00Z' }",
    "b": "Target audience: on-call database engineer. The engineer needs to act immediately. PT3H = ISO 8601 duration for 3 hours.",
    "c": "Disk space response procedures: (1) Identify large files/directories: du -sh /var/lib/postgresql/*. (2) Check PostgreSQL table bloat and run VACUUM. (3) Archive old WAL files. (4) If critical: provision additional disk or migrate data. Growth rate of 2 GB/hour suggests active write load."
  },
  "function": "Produce the final on-call alert message.",
  "acceptedOutput": {
    "alertTitle": "CRITICAL: Disk space on db-prod-02 full in ~3 hours",
    "detail": "The PostgreSQL data directory (/var/lib/postgresql) on db-prod-02 is at 94% capacity. At the current growth rate of 2 GB/hour, the disk will be full in approximately 3 hours (estimated by 21:00 UTC).",
    "currentState": {
      "usage": "94% (6 GB remaining of ~100 GB)",
      "growthRate": "2 GB/hour",
      "estimatedFull": "~21:00 UTC (in 3 hours)"
    },
    "immediateActions": [
      "1. ssh db-prod-02 && du -sh /var/lib/postgresql/* — identify largest directories",
      "2. Check PostgreSQL WAL accumulation: ls -lh /var/lib/postgresql/*/pg_wal/ — WAL files may be accumulating due to replication lag",
      "3. Run VACUUM FULL on bloated tables if safe",
      "4. If WAL is the cause: investigate replication slot lag: SELECT * FROM pg_replication_slots;",
      "5. If cannot free space within 1 hour: escalate to provision additional disk"
    ]
  },
  "rejectedOutput": {
    "alertTitle": "DiskSpaceCritical on db-prod-02",
    "detail": "Disk usage is 94% on /var/lib/postgresql. Threshold: 90%. Bytes available: 6442450944. Rate: 2147483648 bytes/hour."
  },
  "arbiters": ["oncallCommunicationArbiter"],
  "whyAccepted": "The accepted alert translates PT3H and byte counts into human-readable values (3 hours, 2 GB/hour, 6 GB remaining), gives a concrete estimated time to full in UTC, and provides an ordered action list starting with the most diagnostic step. An on-call engineer can act on this without additional lookups.",
  "whyRejected": "The rejected alert exposes raw bytes (6442450944, 2147483648) without human-readable conversion. The on-call engineer must mentally convert to understand urgency. No action steps provided. ISO 8601 duration PT3H not translated.",
  "sourceAnchors": [
    {
      "sourceId": "OPENASSISTANT-OASST1",
      "url": "https://huggingface.co/datasets/OpenAssistant/oasst1",
      "section": "On-call alert communication — disk space critical with human-readable values and PostgreSQL diagnostic steps",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-282

```json
{
  "rowId": "EXT-282",
  "sourceIds": ["PYTHON-PEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "typeddict-adoption",
  "rowClass": "migration-decision",
  "input": {
    "a": "A Python 3.8+ codebase passes configuration dictionaries around: config = {'host': 'localhost', 'port': 5432, 'database': 'mydb', 'max_connections': 10}. A reviewer suggested migrating to TypedDict (PEP 589). Should we migrate?",
    "b": "Current code: def connect(config: dict) -> Connection. The 'config' parameter has no enforced schema — any key can be passed. mypy cannot type-check the dict contents. Bugs occur when callers omit required keys or use wrong types (e.g., port as string instead of int). 34 config dict usages in the codebase.",
    "c": "PEP 589 TypedDict (Python 3.8+): from typing import TypedDict. class DatabaseConfig(TypedDict): host: str; port: int; database: str; max_connections: int. mypy validates that all usages pass the correct keys with correct types. TypedDict is a typing construct only — no runtime overhead. Total_or_alternative = TypedDict with total=False for optional keys."
  },
  "function": "Decide: migrate config dicts to TypedDict.",
  "acceptedOutput": {
    "decision": "MIGRATE to TypedDict.",
    "rationale": "Three concrete improvements: (1) mypy type checking — 'def connect(config: DatabaseConfig)' allows mypy to detect missing keys and wrong types at static analysis time. Current 'dict' annotation provides no type checking for contents. (2) IDE completion — IDEs show TypedDict fields as completion options when typing config['...'] or config.get('...'). (3) Documentation — the TypedDict definition is self-documenting: the required fields, their names, and their types are explicit. 'dict' requires documentation to enumerate expected keys.",
    "migration": "class DatabaseConfig(TypedDict): host: str; port: int; database: str; max_connections: int. Replace 'def connect(config: dict)' with 'def connect(config: DatabaseConfig)'. mypy will report errors for any of the 34 usages with missing or incorrectly typed keys — fix each before committing.",
    "gate": "mypy --strict passes on all 34 config dict usages. Zero TypedDict key errors."
  },
  "rejectedOutput": {
    "decision": "Keep plain dict. TypedDict is a typing-only construct that adds boilerplate without runtime benefit. Dataclasses or Pydantic provide the same benefits with runtime validation."
  },
  "arbiters": ["typingModernizationArbiter"],
  "whyAccepted": "mypy type checking of dict contents is a concrete improvement — the current dict annotation allows any key/value to pass type checking. TypedDict is available in Python 3.8+ (the codebase version) with no dependencies. The 'dataclasses provide the same benefits' is true but a heavier refactor — TypedDict requires minimal code change and preserves dict compatibility (existing code that accesses config['host'] still works).",
  "whyRejected": "The rejected verdict is partially correct (dataclasses provide runtime validation that TypedDict does not) but incorrect in its conclusion. The requirement is mypy type checking of dict contents, not runtime validation. TypedDict solves the stated problem. Migrating to dataclasses or Pydantic is a larger refactor than the problem requires.",
  "sourceAnchors": [
    {
      "sourceId": "PYTHON-PEPS",
      "url": "https://peps.python.org/pep-0589/",
      "section": "PEP 589 — TypedDict; dict contents type checking with mypy; required vs optional keys",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-283

```json
{
  "rowId": "EXT-283",
  "sourceIds": ["REACT-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "react-context-vs-prop-drilling",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We have a React component tree 6 levels deep. The user's authentication state (userId, roles, displayName) is needed at level 1 (NavBar), level 3 (ProfilePage), and level 6 (CommentForm). Currently, we pass the auth state as props through all 6 levels. A developer suggested React Context. Should we use Context or keep prop drilling?",
    "b": "The auth state updates rarely (on login/logout). The intermediate components (levels 2, 4, 5) receive and forward the auth prop but don't use it themselves. Total components that receive but don't use auth: 3.",
    "c": "React Context: creates a provider at the top level. Components at any depth consume the context with useContext — no intermediate prop forwarding required. When context value changes, all consumers re-render. Auth state changes rarely (login/logout), so re-renders are infrequent. Prop drilling: props passed through each level explicitly. Intermediate components must accept and forward props they do not use. Adding a new consumer at depth 7 requires adding the prop to all 7 levels."
  },
  "function": "Produce the command: React Context or prop drilling for auth state.",
  "acceptedOutput": {
    "command": "Use React Context for authentication state.",
    "rationale": "Three intermediate components (levels 2, 4, 5) receive auth props purely to forward them — they do not use the data. This is the canonical use case for React Context: data consumed at multiple non-adjacent levels without relevance to intermediate levels. Auth state changes rarely — Context's 'all consumers re-render on change' behavior is acceptable for infrequent login/logout events.",
    "implementation": "const AuthContext = createContext<AuthState | null>(null);\n\nexport function AuthProvider({ children }: { children: ReactNode }) {\n  const auth = useAuth();  // fetch from token/session\n  return <AuthContext.Provider value={auth}>{children}</AuthContext.Provider>;\n}\n\nexport function useAuthContext() {\n  const ctx = useContext(AuthContext);\n  if (!ctx) throw new Error('useAuthContext must be used within AuthProvider');\n  return ctx;\n}\n\n// In NavBar, ProfilePage, CommentForm:\nconst { userId, roles, displayName } = useAuthContext();",
    "whenToPreferPropDrilling": "Prefer prop drilling when: (1) the data is used by most intermediate components (not just tunneled through), (2) fewer than 2 levels deep, (3) performance requires explicit control over re-renders."
  },
  "rejectedOutput": {
    "command": "Keep prop drilling. Prop drilling makes the data flow explicit — you can see exactly which components receive auth state. Context makes data flow implicit and harder to debug."
  },
  "arbiters": ["reactStateFlowArbiter"],
  "whyAccepted": "The 3 intermediate components that forward auth props without using them are a clear prop drilling symptom — they are coupled to auth state they don't need. Context removes this coupling. The 'explicit data flow' benefit of prop drilling applies when intermediate components use the data, not when they merely forward it.",
  "whyRejected": "The 'explicit data flow' argument applies when intermediate components use the data. When they merely forward it, explicitness means 3 components have an unnecessary coupling. 'Harder to debug' with Context is mitigated by React DevTools which shows Context values at every level.",
  "sourceAnchors": [
    {
      "sourceId": "REACT-ADVANCED",
      "url": "https://github.com/facebook/react",
      "section": "Context vs prop drilling — 3 intermediate forwarder components; auth state at levels 1/3/6",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-284

```json
{
  "rowId": "EXT-284",
  "sourceIds": ["ANGULAR-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "angular-tree-shakeable-providers",
  "rowClass": "migration-decision",
  "input": {
    "a": "An Angular 12 codebase registers services in NgModule providers arrays: @NgModule({ providers: [ProductService, CartService, UserService] }). The team is upgrading to Angular 17. A reviewer suggested migrating services to tree-shakeable providers (providedIn: 'root'). Should they migrate?",
    "b": "Current state: 23 services registered in NgModule providers arrays across 5 feature modules. Tree size analysis: 8 services are used only in their respective feature modules; 15 services are used across multiple modules.",
    "c": "Angular tree-shakeable providers: @Injectable({ providedIn: 'root' }) — the service is registered with the root injector and is tree-shaken if not used anywhere in the application. NgModule providers: services registered in providers array are always included in the bundle regardless of whether they are actually used. providedIn: 'root' moves the registration responsibility from the NgModule to the service class — NgModule no longer needs to be modified when adding or removing services."
  },
  "function": "Decide: migrate services to tree-shakeable providers (providedIn: 'root').",
  "acceptedOutput": {
    "decision": "MIGRATE all 23 services to providedIn: 'root'.",
    "rationale": "Three improvements: (1) Bundle size — 8 services registered in module providers are included in the bundle even if the feature module is lazy-loaded and never visited. With providedIn: 'root', unused services are tree-shaken. (2) Maintainability — services no longer need to be listed in NgModule providers arrays. Adding a new service requires only the @Injectable decorator — no NgModule change. (3) Singleton guarantee — providedIn: 'root' provides a single instance across the entire application. Module providers can accidentally create multiple instances if the module is imported multiple times.",
    "migration": "In each service: remove from NgModule providers array. Add: @Injectable({ providedIn: 'root' }) to the @Injectable decorator.",
    "exception": "Services that require different instances per module (intentional non-singleton) should remain in NgModule providers or use providedIn: the specific module."
  },
  "rejectedOutput": {
    "decision": "Keep NgModule providers. The current approach is explicit — you can see which services each module provides. providedIn: 'root' makes service availability implicit."
  },
  "arbiters": ["angularArchitectureArbiter"],
  "whyAccepted": "NgModule providers arrays are a maintenance burden — every service addition/removal requires NgModule modification. The 'explicit' benefit of NgModule providers is outweighed by the tree-shaking and maintainability benefits. Angular's own documentation recommends providedIn: 'root' as the default.",
  "whyRejected": "The 'explicit' argument is partially valid (you can see registered services in the NgModule) but NgModule providers arrays tend to grow without pruning. A service removed from all usage but not from the NgModule providers array still ships in the bundle. providedIn: 'root' automatically removes unused services.",
  "sourceAnchors": [
    {
      "sourceId": "ANGULAR-DOCS",
      "url": "https://github.com/angular/angular",
      "section": "Tree-shakeable providers — providedIn: 'root' vs NgModule providers; bundle size impact; singleton guarantee",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-285

```json
{
  "rowId": "EXT-285",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "duplicate-file-detection",
  "rowClass": "negative-example",
  "input": {
    "a": "Execute: find duplicate files in a directory tree. Two files are duplicates if they have identical content (same hash). Return a list of duplicate groups, where each group is a list of file paths that have identical content. Files with unique content are not included.",
    "b": "Function signature: find_duplicate_files(root_dir: str) -> list[list[str]]. Example: if file_a.txt and file_b.txt have the same content, and file_c.txt has unique content, return [['/path/file_a.txt', '/path/file_b.txt']]. Use SHA-256 hashing. Do not follow symlinks.",
    "c": "Algorithm: walk the directory tree (os.walk with followlinks=False), hash each file, group files by hash. Files with the same hash are duplicates. Only return groups with 2+ files (groups with 1 file are unique, not duplicates). Size pre-filter: files of different sizes cannot have the same content — skip the hash computation for size-unique files as an optimization."
  },
  "function": "Implement find_duplicate_files with SHA-256 hashing and size pre-filter.",
  "acceptedOutput": {
    "implementation": "import os\nimport hashlib\nfrom collections import defaultdict\n\ndef find_duplicate_files(root_dir: str) -> list:\n    size_groups = defaultdict(list)\n    for dirpath, _, filenames in os.walk(root_dir, followlinks=False):\n        for filename in filenames:\n            filepath = os.path.join(dirpath, filename)\n            try:\n                size = os.path.getsize(filepath)\n                size_groups[size].append(filepath)\n            except OSError:\n                continue  # skip unreadable files\n    hash_groups = defaultdict(list)\n    for size, paths in size_groups.items():\n        if len(paths) < 2:\n            continue  # size-unique: cannot be duplicates\n        for filepath in paths:\n            try:\n                h = hashlib.sha256()\n                with open(filepath, 'rb') as f:\n                    for chunk in iter(lambda: f.read(65536), b''):\n                        h.update(chunk)\n                hash_groups[h.hexdigest()].append(filepath)\n            except OSError:\n                continue\n    return [paths for paths in hash_groups.values() if len(paths) >= 2]"
  },
  "rejectedOutput": {
    "implementation": "import os, hashlib\nfrom collections import defaultdict\n\ndef find_duplicate_files(root_dir):\n    hashes = defaultdict(list)\n    for dirpath, _, files in os.walk(root_dir):\n        for f in files:\n            path = os.path.join(dirpath, f)\n            content = open(path, 'rb').read()\n            hashes[hashlib.sha256(content).hexdigest()].append(path)\n    return [v for v in hashes.values() if len(v) > 1]"
  },
  "arbiters": ["fileSystemAlgorithmArbiter"],
  "whyAccepted": "The accepted implementation: (1) size pre-filter reduces hash computations, (2) chunked reading (65536 bytes per chunk) handles large files without OOM, (3) followlinks=False prevents symlink loops, (4) OSError handling skips permission-denied files. For a 100 GB photo archive with millions of files, the chunked + pre-filtered approach is essential.",
  "whyRejected": "The rejected implementation: (1) loads entire file into memory (open().read()) — a 4 GB video file causes OOM, (2) no size pre-filter — hashes all files regardless of uniqueness, (3) no followlinks=False — may follow symlink cycles infinitely, (4) no error handling — a single unreadable file crashes the function.",
  "sourceAnchors": [
    {
      "sourceId": "BIGCODEBENCH",
      "url": "https://github.com/bigcode-project/bigcodebench",
      "section": "Task: find_duplicate_files — SHA-256 with chunked read, size pre-filter, symlink protection",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-286

```json
{
  "rowId": "EXT-286",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "merge-conflict-aggregation",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Aggregate results from three analysis agents for a pull request submitted from a feature branch that is 45 commits behind the main branch.",
    "b": "Agent results: Agent 1 (Merge Conflict Agent): BLOCK — 7 merge conflicts detected in files: src/auth/AuthService.cs, src/api/UserController.cs, tests/AuthServiceTests.cs, and 4 other files. Agent 2 (Test Agent): NOT_RUN — tests could not be run because merge conflicts prevent compilation. Agent 3 (Static Analysis Agent): NOT_RUN — static analysis requires compilable code.",
    "c": "Policy: a pull request with merge conflicts cannot be merged. Agents 2 and 3 cannot execute because the conflicts prevent compilation. The PR author must rebase or merge main into the feature branch to resolve conflicts before review can proceed."
  },
  "function": "Produce the aggregated result for this pull request.",
  "acceptedOutput": {
    "aggregatedResult": "BLOCK",
    "resultCode": "MERGE_CONFLICTS",
    "summary": "The pull request has 7 merge conflicts that prevent compilation. Tests and static analysis could not run. The PR cannot be reviewed until conflicts are resolved.",
    "requiredActions": [
      "Rebase the feature branch onto main: git fetch origin && git rebase origin/main",
      "Resolve conflicts in: src/auth/AuthService.cs, src/api/UserController.cs, tests/AuthServiceTests.cs (and 4 additional files)",
      "Run tests locally after resolving conflicts: dotnet test",
      "Force-push the rebased branch: git push --force-with-lease",
      "Re-request review after CI passes"
    ],
    "blockingAgent": "Merge Conflict Agent (7 conflicts)",
    "nonExecutedAgents": ["Test Agent (compilation blocked)", "Static Analysis Agent (compilation blocked)"]
  },
  "rejectedOutput": {
    "aggregatedResult": "REQUIRES_MANUAL_REVIEW",
    "summary": "Merge conflicts detected. Tests not run. A human reviewer should assess the conflicts and determine if the PR can proceed."
  },
  "arbiters": ["mergeConflictArbiter"],
  "whyAccepted": "Merge conflicts are a categorical BLOCK — they prevent compilation, prevent test execution, and prevent merge. A human reviewer cannot 'assess conflicts and determine if the PR can proceed' — the merge conflicts must be resolved by the PR author before any review is meaningful. REQUIRES_MANUAL_REVIEW implies optional human judgment; BLOCK is the correct outcome for a non-mergeable PR.",
  "whyRejected": "REQUIRES_MANUAL_REVIEW is appropriate for ambiguous situations requiring human judgment. Merge conflicts are not ambiguous — the PR is non-mergeable until conflicts are resolved. Human review of conflicted code is not actionable — the reviewer cannot merge the PR in this state.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-BENCH",
      "url": "https://github.com/princeton-nlp/SWE-bench",
      "section": "Merge conflict detection — BLOCK for 7 conflicts preventing compilation; rebase instructions",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-287

```json
{
  "rowId": "EXT-287",
  "sourceIds": ["DAPPER"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "dapper-stored-procedure-vs-inline",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A reporting query joins 6 tables, uses 3 CTEs (Common Table Expressions), and has conditional logic that varies based on parameters. The query is 150 lines of SQL. Should we use a stored procedure or inline SQL in Dapper?",
    "b": "The query: (1) is used by one endpoint only, (2) must be version-controlled with the application code, (3) is modified during feature development alongside the C# code. The team does not have a DBA (Database Administrator) — developers own both the application and database code.",
    "c": "Stored procedure: logic lives in the database, managed via migrations or DBA scripts. Execution plan is cached per database. Changing the query requires a database migration. Inline SQL in Dapper: SQL is a string in the C# code, version-controlled with the application. No database migration required to change the query. EF Core raw SQL or Dapper parameterized queries prevent SQL injection. A 150-line inline SQL string is harder to read in C# than in a dedicated .sql file."
  },
  "function": "Arbitrate: stored procedure or inline SQL for the complex reporting query?",
  "acceptedOutput": {
    "verdict": "Use inline SQL — but store it in a dedicated .sql file embedded as a resource.",
    "reasoning": "Two requirements favor inline SQL: (1) version control co-location — the SQL changes alongside C# feature development; a stored procedure requires a separate database migration for every SQL change. (2) Team ownership — no DBA means developers must manage stored procedures via migration scripts; this is the same as managing inline SQL but with more indirection. The 150-line concern is addressed by embedding the SQL in a dedicated file.",
    "implementation": "// Resources/Queries/GetSalesReport.sql — embed as EmbeddedResource\n// In C#:\nvar sql = GetEmbeddedResource('GetSalesReport.sql');\nvar results = await conn.QueryAsync<SalesReportRow>(sql, new { StartDate = start, EndDate = end });",
    "executionPlanNote": "SQL Server caches execution plans for parameterized queries even without stored procedures. The plan for a parameterized Dapper query is cached on first execution."
  },
  "rejectedOutput": {
    "verdict": "Use a stored procedure. 150 lines of inline SQL is unmaintainable in a C# string. Stored procedures have cached execution plans and better performance."
  },
  "arbiters": ["dataAccessPatternArbiter"],
  "whyAccepted": "The .sql file approach retains version control co-location (the SQL file is committed with the C# code) while solving the readability concern. Stored procedure execution plan caching is available for parameterized Dapper queries as well — this is not a stored procedure exclusive benefit.",
  "whyRejected": "The stored procedure approach requires a database migration for every SQL change during feature development. For a team without a DBA, this means every SQL iteration (which may be frequent during development) requires a migration step. The 'unmaintainable inline SQL' concern is solved by the embedded .sql file, not by stored procedures.",
  "sourceAnchors": [
    {
      "sourceId": "DAPPER",
      "url": "https://github.com/DapperLib/Dapper",
      "section": "Inline SQL vs stored procedure — embedded .sql resource for complex reporting; co-location with C# code",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-288

```json
{
  "rowId": "EXT-288",
  "sourceIds": ["ASPNETCORE-DOCS-SAMPLES"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "B_ROUTING",
  "subdomain": "rate-limiting-placement",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We need to apply rate limiting to our public API. Should we implement rate limiting in ASP.NET Core (using Microsoft.AspNetCore.RateLimiting) or at the API gateway (AWS API Gateway / Kong)?",
    "b": "Architecture: ASP.NET Core API behind an AWS API Gateway. The API Gateway handles SSL termination, authentication via JWT validation, and request routing. The ASP.NET Core service handles business logic. Expected load: 5,000 requests/minute across all endpoints. Rate limit requirements: 100 requests/minute per API key.",
    "c": "Rate limiting at the gateway: the gateway can enforce limits before requests reach the application server. Throttled requests never consume application server resources. Gateway configuration is separate from application code. Rate limiting in ASP.NET Core: the application itself handles rate limiting. Works even if the gateway doesn't support it. Can use request-specific context (user ID, not just API key) from the authenticated request. Double-enforcing at both layers is possible."
  },
  "function": "Produce the command: gateway rate limiting, ASP.NET Core rate limiting, or both.",
  "acceptedOutput": {
    "command": "Enforce rate limiting at the API gateway as the primary layer, with application-level rate limiting as a secondary defense.",
    "rationale": "Gateway rate limiting as primary: throttled requests (100/min per API key exceeded) are rejected at the gateway — they never reach the ASP.NET Core application. This reduces application server load from abusive clients. The gateway already handles auth and routing — adding rate limiting is a configuration change, not a code deployment.",
    "applicationLevelSecondary": "ASP.NET Core rate limiting as a secondary layer: if a request bypasses the gateway (internal load balancer direct access, misconfigured routing), the application still enforces limits. Microsoft.AspNetCore.RateLimiting: services.AddRateLimiter(o => o.AddFixedWindowLimiter('api-key', options => { options.Window = TimeSpan.FromMinutes(1); options.PermitLimit = 100; })).",
    "whyBothLayers": "Defense in depth: the gateway is the intended enforcement point; the application layer is the fallback. Neither layer alone is sufficient if the gateway can be bypassed."
  },
  "rejectedOutput": {
    "command": "Implement rate limiting only in ASP.NET Core. The application can enforce limits more precisely using authenticated user context. Gateway rate limiting is less flexible."
  },
  "arbiters": ["apiGatewayArbiter"],
  "whyAccepted": "Gateway-level rate limiting prevents throttled requests from reaching the application server — protecting server resources from abusive clients. Application-only rate limiting means every throttled request consumes application server CPU and memory before being rejected. For a 5,000 req/min service with potential abuse, gateway throttling reduces server load significantly.",
  "whyRejected": "Application-only rate limiting is valid but less efficient — abusive clients still consume application threads, connection pool connections, and memory for rate limit evaluation. The gateway's position in front of the application makes it the correct primary enforcement point.",
  "sourceAnchors": [
    {
      "sourceId": "ASPNETCORE-DOCS-SAMPLES",
      "url": "https://github.com/dotnet/AspNetCore.Docs.Samples",
      "section": "Rate limiting — gateway primary vs application secondary; defense in depth; throttled request resource consumption",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-289

```json
{
  "rowId": "EXT-289",
  "sourceIds": ["TATOEBA"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "en-zh-technical-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose this localization task: translate the error message 'The upload failed because the file size exceeds the 10 MB limit.' from English to Simplified Chinese (zh-CN) for a web application.",
    "b": "Context: the string appears in a file upload error modal. Key terms: 'upload' (上传, shàngchuán), 'file size' (文件大小, wénjiàn dàxiǎo), 'exceeds' (超出, chāochū), 'limit' (限制, xiànzhì). Simplified Chinese is the target (PRC/mainland Chinese users, not Traditional Chinese for Taiwan/HK).",
    "c": "Chinese technical writing conventions: MB is kept as 'MB' (not translated to 兆字节). Numbers: 10 is written as Arabic '10', not Chinese numeral '十' in UI contexts. Sentence structure: Chinese tends toward shorter sentences — embedding a relative clause 'because the file size exceeds the 10 MB limit' may be restructured."
  },
  "function": "Produce the action items for the zh-CN localization task.",
  "acceptedOutput": {
    "actionItems": [
      "Action 1: Translate the core message: '上传失败，文件大小超出 10 MB 限制。' (Literal: Upload failed, file size exceeds 10 MB limit.) — The Chinese sentence uses a comma to join the reason clause, which is more natural than a full stop between the two clauses.",
      "Action 2: Keep 'MB' as-is — do not translate to 兆字节. In Chinese UI contexts, MB is the universal abbreviation understood by all users. '10 MB' remains '10 MB' with a space between number and unit.",
      "Action 3: Use Arabic numeral '10', not Chinese numeral '十' — Arabic numerals are standard in Chinese UI text for file sizes, limits, and technical values.",
      "Action 4: Verify the character encoding: zh-CN text uses UTF-8. Confirm the localization file encoding supports Chinese characters without garbling.",
      "Action 5: Test the translation in the UI: Chinese text may be longer in height (larger character blocks) and shorter in width than English — verify the modal layout accommodates the text without truncation."
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Translate: '上传失败，因为文件大小超过了10兆字节的限制。'",
      "This is a direct translation of all terms including MB to 兆字节."
    ]
  },
  "arbiters": ["localizationArbiter"],
  "whyAccepted": "The accepted translation uses MB (not 兆字节) which is the standard term in Chinese software UIs. '超出' (chāochū) is more idiomatic than '超过了' for limit violations in UI error messages. The comma-joined structure is natural in Chinese for cause-effect error messages.",
  "whyRejected": "'兆字节' is technically correct (it means megabyte) but is rarely used in modern Chinese software UI — users expect 'MB'. Translating MB to 兆字节 in a technical UI makes the message sound formal and unusual. '超过了' is more literary; '超出' is more idiomatic for UI error messages about exceeding limits.",
  "sourceAnchors": [
    {
      "sourceId": "TATOEBA",
      "url": "https://tatoeba.org",
      "section": "EN-ZH translation — file upload error message; MB retention; Arabic numeral convention; comma-joined clauses",
      "license": "CC-BY-2.0"
    }
  ],
  "licenseConstraint": "CC-BY-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-290

```json
{
  "rowId": "EXT-290",
  "sourceIds": ["IETF-RFCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "http-connection-management",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Format this API response for a bulk data export endpoint that streams 50,000 records: { totalRecords: 50000, format: 'NDJSON', streamingEnabled: true, estimatedSizeBytes: 52428800 }",
    "b": "The client is a data pipeline that will process the records as they arrive. The response should communicate how to consume the stream correctly. HTTP considerations: Transfer-Encoding: chunked for streaming, Content-Type for NDJSON (Newline Delimited JSON).",
    "c": "RFC 7230 (HTTP/1.1 Message Syntax): Transfer-Encoding: chunked enables streaming without knowing the final content length. Content-Length is not set for chunked responses. NDJSON content type: 'application/x-ndjson' or 'application/ndjson'. The client should not buffer the entire response — it should process each newline-delimited record as it arrives."
  },
  "function": "Produce the formatted HTTP response headers and body structure for the streaming bulk export.",
  "acceptedOutput": {
    "httpStatus": 200,
    "headers": {
      "Content-Type": "application/x-ndjson",
      "Transfer-Encoding": "chunked",
      "X-Total-Records": "50000",
      "X-Streaming": "true",
      "Cache-Control": "no-store"
    },
    "bodyDescription": "NDJSON stream: one JSON object per line, newline-delimited. Each line is a complete JSON record. Example lines: {\"id\":1,\"name\":\"Alice\",\"created\":\"2026-01-01\"}\n{\"id\":2,\"name\":\"Bob\",\"created\":\"2026-01-02\"}",
    "clientConsumptionGuidance": "Process the response line-by-line as data arrives. Do not wait for the full response before processing. Example: using var reader = new StreamReader(response.Content.ReadAsStream()); while (!reader.EndOfStream) { var line = await reader.ReadLineAsync(); var record = JsonSerializer.Deserialize<Record>(line); process(record); }",
    "rationale": "Transfer-Encoding: chunked enables streaming without buffering 50 MB. X-Total-Records header allows the client to display progress. Content-Length is omitted (incompatible with chunked). Cache-Control: no-store prevents proxy/browser caching of a large streaming response."
  },
  "rejectedOutput": {
    "httpStatus": 200,
    "headers": {
      "Content-Type": "application/json",
      "Content-Length": "52428800"
    },
    "bodyDescription": "JSON array: [{...}, {...}, ...] — all 50,000 records in a single JSON array."
  },
  "arbiters": ["streamingResponseArbiter"],
  "whyAccepted": "NDJSON with chunked transfer encoding allows the client pipeline to process records as they stream, without buffering 50 MB. A JSON array requires the client to receive and parse the entire 50 MB before processing any record. For a data pipeline, streaming line-by-line is both lower latency and lower memory.",
  "whyRejected": "A 50 MB JSON array requires the client to buffer the entire response before any record can be processed. The Content-Length: 52428800 header tells the client to wait for all 50 MB. For a data pipeline, this means 50 MB in memory before the first record is processed — a stream with chunked transfer achieves zero-buffer first-record availability.",
  "sourceAnchors": [
    {
      "sourceId": "IETF-RFCS",
      "url": "https://www.rfc-editor.org/rfc/rfc7230",
      "section": "RFC 7230: Transfer-Encoding chunked — streaming without Content-Length; NDJSON for line-by-line processing",
      "license": "IETF Trust provisions"
    }
  ],
  "licenseConstraint": "IETF Trust provisions",
  "trainingStatus": "candidate"
}
```
