# External Training Rows — Batch 10
## Row IDs: EXT-191 through EXT-210
## Date: 2026-05-04
## Focus: Terraform/Ansible IaC, additional CWE, Python PEPs, React concurrent, circuit breaker, API design, additional families 3/4/5/6/8

---

## EXT-191

```json
{
  "rowId": "EXT-191",
  "sourceIds": ["ANSIBLE-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "idempotent-deployment-automation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose this deployment requirement: deploy the application configuration file /etc/myapp/config.ini to 40 servers. The file must be owned by user 'myapp', group 'myapp', mode 0640. If the file already exists with the correct content, do not restart the service. If the file changes, restart the myapp service.",
    "b": "Current approach: use the shell module to run 'scp config.ini user@host:/etc/myapp/' in a loop across all 40 servers. The service is always restarted after deployment regardless of whether the file changed.",
    "c": "Context: Ansible idempotency. The 'copy' module is idempotent — it computes a checksum of the source and destination files. If the checksums match, no change is made and the task is marked 'ok' (not 'changed'). Ansible handlers are triggered only when a task reports 'changed'. If the file does not change, the handler (service restart) is not triggered. The shell module with scp is not idempotent — it always copies and therefore always triggers the restart."
  },
  "function": "Produce the action items for converting the scp-based deployment to an idempotent Ansible playbook.",
  "acceptedOutput": {
    "actionItems": [
      "Action 1: Replace the shell/scp task with the Ansible 'copy' module: - name: Deploy application config; copy: src: config.ini; dest: /etc/myapp/config.ini; owner: myapp; group: myapp; mode: '0640'; notify: Restart myapp service",
      "Action 2: Define the handler: handlers: - name: Restart myapp service; service: name: myapp; state: restarted — handlers only execute when at least one task that notified them reports 'changed'.",
      "Action 3: Set the inventory to target all 40 servers: hosts: myapp_servers — Ansible runs the play against all hosts in the group in parallel (forks: 10 by default, or configure forks: 40 for full parallelism).",
      "Action 4: Add a pre-task to verify the destination directory exists: - name: Ensure config directory exists; file: path: /etc/myapp; state: directory; owner: myapp; group: myapp; mode: '0750'",
      "Action 5: Run with --check --diff first to preview what would change: ansible-playbook deploy.yml --check --diff — this shows a unified diff of file changes without applying them."
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Use the shell module: shell: scp config.ini myapp@{{ inventory_hostname }}:/etc/myapp/config.ini",
      "Always run: service: name: myapp; state: restarted after the copy."
    ]
  },
  "arbiters": ["deploymentAutomationArbiter"],
  "whyAccepted": "The copy module is idempotent — 40 deployments where the config has not changed produce zero restarts and zero service disruptions. The scp approach unconditionally restarts the service on all 40 servers on every run, causing unnecessary disruption. Handlers with notify implement conditional restart correctly.",
  "whyRejected": "scp via shell module is not idempotent. Every playbook run triggers a service restart on all 40 servers regardless of whether the configuration changed. This violates the stated requirement: 'if the file already exists with the correct content, do not restart the service.'",
  "sourceAnchors": [
    {
      "sourceId": "ANSIBLE-DOCS",
      "url": "https://github.com/ansible/ansible",
      "section": "copy module — idempotent file deployment; handlers — conditional execution on changed",
      "license": "GPL-3.0"
    }
  ],
  "licenseConstraint": "GPL-3.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-192

```json
{
  "rowId": "EXT-192",
  "sourceIds": ["TERRAFORM-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "iac-resource-management",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We need to provision an AWS VPC (Virtual Private Cloud) with 3 public subnets and 3 private subnets across 3 availability zones. Should we write Terraform modules or use a flat main.tf with all resources listed directly?",
    "b": "The team has one environment today (production). There are plans to add staging and development environments with the same VPC topology but different CIDR (Classless Inter-Domain Routing) ranges.",
    "c": "Context: Terraform modules encapsulate reusable resource groups behind input variables and output values. A flat main.tf lists all resources directly in one file. For a single environment, a flat file is readable. For multiple environments with the same topology, a module allows: module 'vpc' { source = './modules/vpc'; cidr_block = '10.0.0.0/16'; availability_zones = ['us-east-1a','us-east-1b','us-east-1c'] } — one module definition, three environment instantiations."
  },
  "function": "Produce the command: module or flat file, and what structure to use.",
  "acceptedOutput": {
    "command": "Create a reusable VPC module.",
    "rationale": "Three environments with the same VPC topology means three module instantiations, not three copies of 18+ resource definitions. The module accepts variables: cidr_block, availability_zones, environment_name. Each environment directory (environments/prod/, environments/staging/, environments/dev/) contains a main.tf that instantiates the module with environment-specific values. Changes to VPC topology (add a NAT Gateway, modify subnet sizing) are made once in the module and applied to all environments.",
    "structure": "modules/vpc/main.tf — resource definitions for VPC, subnets, route tables, internet gateway. modules/vpc/variables.tf — input variables: cidr_block, availability_zones, environment_name. modules/vpc/outputs.tf — output values: vpc_id, public_subnet_ids, private_subnet_ids. environments/prod/main.tf — module instantiation with prod values. environments/staging/main.tf — module instantiation with staging values.",
    "antiPattern": "Do not use 'count' or 'for_each' on the root module to create multiple environments — Terraform state becomes entangled. Separate directories with separate state files (or separate workspaces) keep environment state isolated."
  },
  "rejectedOutput": {
    "command": "Write a flat main.tf with all resources listed directly. Modules add unnecessary complexity for a single environment. If you add environments later, you can refactor."
  },
  "arbiters": ["iacDesignArbiter"],
  "whyAccepted": "The staging and development environments are already planned. Refactoring from flat to module later requires moving 18+ resources, updating state references, and coordinating a potentially destructive plan. Starting with a module eliminates this migration cost. The module approach is not premature abstraction — the requirement explicitly states multiple environments are planned.",
  "whyRejected": "The 'refactor later' argument is valid only when future requirements are speculative. Here the requirements explicitly state 3 environments with the same topology are planned. Deferring modularity guarantees a refactor that risks state corruption and resource re-creation.",
  "sourceAnchors": [
    {
      "sourceId": "TERRAFORM-DOCS",
      "url": "https://github.com/hashicorp/terraform",
      "section": "Modules — reusable infrastructure components; separate state per environment",
      "license": "MPL-2.0"
    }
  ],
  "licenseConstraint": "MPL-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-193

```json
{
  "rowId": "EXT-193",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "null-pointer-dereference-detection",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Evaluate this implementation of a user profile lookup service.",
    "b": "Two implementations submitted: Implementation A: public string GetDisplayName(int userId) { var user = _repository.FindById(userId); return user.DisplayName; } Implementation B: public OperationResult<string> GetDisplayName(int userId) { var user = _repository.FindById(userId); if (user == null) { return OperationResult<string>.Fail('User not found', 'USER_NOT_FOUND'); } return OperationResult<string>.Ok(user.DisplayName); }",
    "c": "Repository contract: FindById returns null if the user does not exist. CWE-476 (NULL Pointer Dereference): accessing a member of a null reference causes a NullReferenceException at runtime. The null check must occur before any member access on the returned reference. The userId may be valid (user exists) or invalid (user was deleted, ID is stale)."
  },
  "function": "Arbitrate: which implementation is correct and why.",
  "acceptedOutput": {
    "verdict": "Implementation B is correct.",
    "reasoning": "The repository contract specifies FindById returns null for non-existent users. Implementation A dereferences 'user.DisplayName' without checking whether 'user' is null — this is CWE-476 (NULL Pointer Dereference). For any userId that does not correspond to an existing user, Implementation A throws NullReferenceException at runtime. Implementation B checks for null before member access and returns a structured failure result. The caller can handle 'USER_NOT_FOUND' without catching exceptions.",
    "cweReference": "CWE-476: NULL Pointer Dereference — The software dereferences a pointer that it expects to be valid but is NULL, typically causing a crash or exit."
  },
  "rejectedOutput": {
    "verdict": "Implementation A is acceptable. The repository should throw an exception if the user is not found rather than returning null. The caller should not need to check for null."
  },
  "arbiters": ["nullSafetyArbiter", "cweComplianceArbiter"],
  "whyAccepted": "The repository contract as stated returns null — this is the interface the implementation must handle. Arguing that the repository 'should' throw is a contract redesign, not an implementation decision. The code as written will crash on null input. CWE-476 is a factual classification of the defect in Implementation A.",
  "whyRejected": "Shifting blame to the repository contract does not fix the crash in Implementation A. If the repository does return null (as stated), Implementation A has a CWE-476 defect. The defensive implementation B is correct regardless of whether the repository design is ideal.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/476.html",
      "section": "CWE-476: NULL Pointer Dereference — member access before null check",
      "license": "MITRE notice required"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-194

```json
{
  "rowId": "EXT-194",
  "sourceIds": ["PYTHON-PEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "language-feature-adoption",
  "rowClass": "migration-decision",
  "input": {
    "a": "A Python 3.8 codebase uses the following pattern in several while loops: while True: chunk = file.read(8192); if not chunk: break; process(chunk). The team is upgrading to Python 3.8+ and a reviewer suggested using the walrus operator (PEP 572). Should we migrate?",
    "b": "PEP 572 introduced the walrus operator ':=' in Python 3.8. It allows assignment within expressions. The pattern 'while chunk := file.read(8192): process(chunk)' is semantically equivalent to the while True / break pattern.",
    "c": "Codebase metrics: 23 occurrences of the while True / assignment / if not x: break pattern. Team Python proficiency: mixed (senior engineers familiar with PEP 572, junior engineers unfamiliar). Code review policy: all changes require review. Migration risk: low — the walrus operator is syntactic sugar; the compiled bytecode is equivalent."
  },
  "function": "Decide: migrate to walrus operator or preserve existing while-True-break pattern.",
  "acceptedOutput": {
    "decision": "MIGRATE with team education gate.",
    "rationale": "PEP 572 walrus operator eliminates the while True / assignment / if not x: break triple-statement pattern. The migration is: (1) mechanically safe — semantically equivalent, same bytecode; (2) improves readability for Python 3.8+ readers — the loop termination condition is explicit in the while clause rather than hidden inside the loop body; (3) 23 occurrences are a reasonable migration scope. Gate B: all 23 occurrences migrated, tests green, and a 30-minute PEP 572 team walkthrough completed before merging.",
    "migrationPattern": "Before: while True: chunk = file.read(8192); if not chunk: break; process(chunk) — After: while chunk := file.read(8192): process(chunk)",
    "caveat": "Do not apply walrus indiscriminately. Use it only for the assignment-then-test pattern where the assigned variable is used immediately. Avoid nested walrus expressions that reduce readability."
  },
  "rejectedOutput": {
    "decision": "DO NOT MIGRATE. The existing pattern is universally understood Python. The walrus operator is unfamiliar to junior engineers and reduces readability for newcomers. Preserve the current pattern."
  },
  "arbiters": ["languageModernizationArbiter"],
  "whyAccepted": "The walrus operator is part of Python 3.8 standard — it is not an obscure third-party pattern. The team education gate addresses the junior familiarity gap without blocking the migration. Keeping 23 occurrences of a pattern that Python 3.8 has a cleaner replacement for accumulates minor readability debt.",
  "whyRejected": "The 'unfamiliar to junior engineers' argument applies to any language feature introduced after the junior engineers learned Python. The solution is education, not indefinite preservation of pre-3.8 patterns in a 3.8+ codebase.",
  "sourceAnchors": [
    {
      "sourceId": "PYTHON-PEPS",
      "url": "https://peps.python.org/pep-0572/",
      "section": "PEP 572 — Assignment Expressions (walrus operator); while loop pattern",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-195

```json
{
  "rowId": "EXT-195",
  "sourceIds": ["KUBERNETES-KEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "kubernetes-autoscaling",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Our Kubernetes deployment processes a message queue. We want to autoscale the number of pods based on the number of messages in the queue, not CPU or memory. Which autoscaling mechanism should we use?",
    "b": "Current setup: HPA (Horizontal Pod Autoscaler) configured with CPU target 70%. The queue depth (number of unprocessed messages) is available as a custom metric exposed via Prometheus and the Prometheus Adapter. Queue depth correlates directly with processing backlog — CPU utilization does not (processors are I/O bound, not CPU bound).",
    "c": "Context: Kubernetes HPA supports three metric source types: Resource (CPU/memory), Pods (per-pod custom metrics), and External (external system metrics like queue depth). KEDA (Kubernetes Event-Driven Autoscaling) is a purpose-built autoscaler for event-driven workloads — it can scale to zero when the queue is empty and scale out based on queue depth natively without the Prometheus Adapter intermediary."
  },
  "function": "Produce the command: which autoscaling mechanism to use for queue-depth-based scaling.",
  "acceptedOutput": {
    "command": "Deploy KEDA (Kubernetes Event-Driven Autoscaling) with a ScaledObject targeting the message queue.",
    "rationale": "KEDA is purpose-built for event-driven scaling: (1) Scale-to-zero — when queue depth is 0, KEDA scales the deployment to 0 pods. HPA minimum is 1. Scale-to-zero eliminates cost for idle periods. (2) Native queue integration — KEDA has built-in scalers for RabbitMQ, Apache Kafka, Azure Service Bus, AWS SQS, and others. No Prometheus Adapter required. (3) Queue-depth math — KEDA ScaledObject: triggers: type: rabbitmq; metadata: queueName: my-queue; queueLength: '50' — target 50 messages per pod. If queue depth is 500, KEDA scales to 10 pods.",
    "hpaAlternative": "HPA with External metric type and Prometheus Adapter works but requires: Prometheus scraping the queue exporter, Prometheus Adapter translating the metric to Kubernetes External metrics API, HPA consuming the External metric. Four moving parts vs KEDA's one ScaledObject."
  },
  "rejectedOutput": {
    "command": "Keep the CPU-based HPA. Add a sidecar that artificially consumes CPU proportional to queue depth, so CPU utilization reflects queue pressure. HPA will then scale based on CPU, which correlates indirectly with queue depth."
  },
  "arbiters": ["kubernetesAutoscalingArbiter"],
  "whyAccepted": "KEDA is the canonical Kubernetes solution for queue-depth-based autoscaling. It is a CNCF (Cloud Native Computing Foundation) project, graduated in 2023. The CPU-proportional sidecar approach is an anti-pattern — it adds an artificial dependency between CPU consumption and queue depth, introduces CPU cost for the sidecar itself, and makes the scaling behavior opaque.",
  "whyRejected": "The CPU-sidecar approach inverts causality: it makes CPU a proxy metric for queue depth instead of measuring queue depth directly. If the proportionality constant is wrong, scaling is wrong. KEDA measures the actual metric of interest directly.",
  "sourceAnchors": [
    {
      "sourceId": "KUBERNETES-KEPS",
      "url": "https://github.com/kubernetes/enhancements",
      "section": "KEP-2021 KEDA integration — External metrics API and event-driven autoscaling",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-196

```json
{
  "rowId": "EXT-196",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "typed-csv-parsing",
  "rowClass": "negative-example",
  "input": {
    "a": "Execute: parse a CSV (Comma-Separated Values) file where column 'age' is integer and column 'score' is float. Return a list of records as typed dictionaries. Raise ValueError with column name and value if a type conversion fails.",
    "b": "Function signature: parse_typed_csv(filepath: str, schema: dict[str, type]) -> list[dict]. schema = {'name': str, 'age': int, 'score': float}. Input file: name,age,score\nAlice,30,9.5\nBob,twenty-two,8.0\nCarol,28,invalid",
    "c": "Type coercion context: int('thirty') raises ValueError. The function must identify which row and column caused the failure and propagate a meaningful error. Silent coercion to None is incorrect — the schema specifies required types."
  },
  "function": "Implement parse_typed_csv returning typed records, raising ValueError with context on coercion failure.",
  "acceptedOutput": {
    "implementation": "def parse_typed_csv(filepath: str, schema: dict) -> list[dict]:\n    records = []\n    with open(filepath, newline='') as f:\n        reader = csv.DictReader(f)\n        for row_num, row in enumerate(reader, start=2):  # start=2: row 1 is header\n            typed_row = {}\n            for col, col_type in schema.items():\n                raw = row.get(col)\n                try:\n                    typed_row[col] = col_type(raw)\n                except (ValueError, TypeError) as e:\n                    raise ValueError(f'Row {row_num}, column {col!r}: cannot convert {raw!r} to {col_type.__name__}') from e\n            records.append(typed_row)\n    return records",
    "behavior": "For input above: row 3 'age'='twenty-two' raises ValueError: Row 3, column 'age': cannot convert 'twenty-two' to int. Processing stops at first error."
  },
  "rejectedOutput": {
    "implementation": "def parse_typed_csv(filepath, schema):\n    records = []\n    with open(filepath) as f:\n        reader = csv.DictReader(f)\n        for row in reader:\n            typed_row = {}\n            for col, col_type in schema.items():\n                try:\n                    typed_row[col] = col_type(row[col])\n                except:\n                    typed_row[col] = None  # coercion failed, use None\n            records.append(typed_row)\n    return records"
  },
  "arbiters": ["typedDataParsingArbiter"],
  "whyAccepted": "The accepted implementation raises ValueError with row number and column name when type coercion fails. The error message identifies exactly which value caused the failure. Row numbering starts at 2 (accounting for the header row) so the error maps to the CSV line number.",
  "whyRejected": "Silently converting coercion failures to None violates the schema contract — the caller cannot distinguish 'age was missing' from 'age was present but unparseable'. A bare 'except:' clause catches all exceptions including KeyboardInterrupt. The rejected output produces corrupted data silently.",
  "sourceAnchors": [
    {
      "sourceId": "BIGCODEBENCH",
      "url": "https://github.com/bigcode-project/bigcodebench",
      "section": "Task: parse_typed_csv — typed coercion with explicit error propagation vs silent None substitution",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-197

```json
{
  "rowId": "EXT-197",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "api-contract-change-review",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Aggregate results from three analysis agents for this pull request: it renames the 'user_id' field to 'userId' in the JSON response of the GET /users/{id} endpoint.",
    "b": "Agent results: Agent 1 (API Contract Agent): BREAKING_CHANGE — field name change in response body is a breaking change for existing clients that parse 'user_id'. Agent 2 (Test Agent): PASS — all automated tests pass. Agent 3 (Documentation Agent): PASS — API documentation updated to reflect 'userId'.",
    "c": "Context: API versioning policy. The service is at v1 with external consumers. Breaking changes to v1 require either: (a) a new API version (v2) with migration period, or (b) a field alias period where both 'user_id' and 'userId' are returned simultaneously. The tests pass because the tests were also updated to use 'userId' — they do not test backward compatibility for existing consumers."
  },
  "function": "Produce the aggregated result for this pull request.",
  "acceptedOutput": {
    "aggregatedResult": "REQUIRES_MANUAL_REVIEW",
    "resultCode": "BREAKING_API_CHANGE",
    "summary": "The field rename from 'user_id' to 'userId' is a breaking change for all v1 API consumers. Automated tests pass because they were updated alongside the change — they do not validate backward compatibility. The change cannot be merged without either: (1) introducing API versioning (v2 endpoint), or (2) a transition period returning both field names. Manual review with the API governance team is required before merge.",
    "blockingAgent": "API Contract Agent",
    "nonBlockingFindings": ["Documentation updated correctly", "Internal tests green"]
  },
  "rejectedOutput": {
    "aggregatedResult": "PASS",
    "summary": "2 of 3 agents passed. The API Contract Agent flagged a concern, but tests and documentation are green. Merge approved."
  },
  "arbiters": ["apiContractArbiter", "breakingChangeArbiter"],
  "whyAccepted": "A single BREAKING_CHANGE verdict from the API Contract Agent is sufficient to block merge — it represents a semantic contract violation that automated tests cannot detect if the tests were updated concurrently. The 2-of-3 majority rule does not apply when one agent identifies a policy violation (breaking API contract). REQUIRES_MANUAL_REVIEW is the correct aggregation outcome for breaking changes.",
  "whyRejected": "The rejected output applies a majority-vote rule where any 2/3 pass means merge approval. This is incorrect for breaking change verdicts — they are not equivalent to test failures that can be outvoted. A single breaking change finding requires escalation regardless of other agent verdicts.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-BENCH",
      "url": "https://github.com/princeton-nlp/SWE-bench",
      "section": "API contract change — breaking field rename; test-updated-alongside anti-pattern",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-198

```json
{
  "rowId": "EXT-198",
  "sourceIds": ["OPENTELEMETRY-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "distributed-trace-propagation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Our microservices system currently uses Zipkin's B3 trace propagation format (X-B3-TraceId, X-B3-SpanId headers). We're adopting OpenTelemetry. Should we migrate to W3C TraceContext (traceparent header) or keep B3?",
    "b": "Current state: 12 services use B3 headers. 8 services have been migrated to OpenTelemetry SDK. 4 services are still on Zipkin client. The Zipkin backend remains the trace storage. New services are being written with OpenTelemetry SDK.",
    "c": "Context: W3C TraceContext (RFC 7540 / W3C Recommendation) is the standard propagation format for OpenTelemetry. B3 is Zipkin's proprietary format, supported by OpenTelemetry as a legacy propagator. OpenTelemetry SDK supports both propagation formats simultaneously via composite propagator. During migration, services that receive traceparent but emit B3 will break the trace chain — parent-child relationships are lost."
  },
  "function": "Arbitrate: which propagation format to standardize on, and what migration sequence is required.",
  "acceptedOutput": {
    "verdict": "Migrate to W3C TraceContext (traceparent) as the primary format, with B3 as a temporary fallback during migration.",
    "migrationSequence": [
      "Phase 1: Configure all OpenTelemetry SDK services with a composite propagator that reads and writes both W3C traceparent and B3. This ensures services can inter-operate with both legacy and migrated services: OTEL_PROPAGATORS=tracecontext,b3multi",
      "Phase 2: Migrate the remaining 4 Zipkin client services to OpenTelemetry SDK with composite propagator. All 12 services now emit both formats.",
      "Phase 3: Remove B3 from the composite propagator once all services are OpenTelemetry SDK. Final state: OTEL_PROPAGATORS=tracecontext",
      "Phase 4: Migrate the Zipkin backend to Jaeger or another OTLP (OpenTelemetry Protocol)-native backend, or deploy an OpenTelemetry Collector that exports to Zipkin during the transition."
    ],
    "antiPattern": "Do not do a hard cutover — switching all services from B3 to W3C simultaneously breaks traces for any in-flight request that crosses a service boundary during the cutover window."
  },
  "rejectedOutput": {
    "verdict": "Keep B3. OpenTelemetry supports B3 propagation. There is no reason to migrate propagation formats — the Zipkin backend already works with B3."
  },
  "arbiters": ["distributedTracingArbiter", "migrationSequenceArbiter"],
  "whyAccepted": "W3C TraceContext is the OpenTelemetry native format and an IETF (Internet Engineering Task Force) standard. Staying on B3 long-term couples the system to Zipkin's proprietary format and requires the B3 legacy propagator in every service indefinitely. The composite propagator approach (both formats simultaneously) allows a zero-downtime migration.",
  "whyRejected": "B3 is a proprietary Zipkin format. Keeping it permanently means all future services must be configured with the legacy B3 propagator. W3C TraceContext is supported by every major observability vendor and is the interoperability standard for distributed tracing.",
  "sourceAnchors": [
    {
      "sourceId": "OPENTELEMETRY-SPEC",
      "url": "https://github.com/open-telemetry/opentelemetry-specification",
      "section": "Propagators — W3C TraceContext vs B3; composite propagator for migration",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-199

```json
{
  "rowId": "EXT-199",
  "sourceIds": ["REACT-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "react-concurrent-mode",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Our React search input calls an API on every keystroke. Users with fast typing see the UI lag because every keystroke triggers a state update and re-render synchronously. How should we handle this?",
    "b": "Current implementation: const [query, setQuery] = useState(''); const handleChange = (e) => { setQuery(e.target.value); fetchResults(e.target.value); }. The input updates and the result list re-renders synchronously on every keystroke.",
    "c": "Context: React 18 concurrent mode provides two mechanisms: (1) useTransition — marks a state update as non-urgent; React can interrupt it to process higher-priority updates (user input). (2) useDeferredValue — creates a deferred copy of a value that lags behind the primary value during heavy rendering. Both allow the input to remain responsive while deferring the expensive result list re-render. Alternative: debounce (delay API call by 300ms)."
  },
  "function": "Produce the command: which mechanism to use and what the implementation looks like.",
  "acceptedOutput": {
    "command": "Use useTransition to mark the search result update as non-urgent.",
    "implementation": "const [query, setQuery] = useState('');\nconst [results, setResults] = useState([]);\nconst [isPending, startTransition] = useTransition();\n\nconst handleChange = (e) => {\n  const value = e.target.value;\n  setQuery(value);  // urgent: input must update immediately\n  startTransition(() => {\n    fetchResults(value).then(data => setResults(data));  // non-urgent: can be interrupted\n  });\n};\n\n// In JSX: {isPending && <Spinner />} — show loading indicator during transition",
    "rationale": "useTransition tells React that the setResults update inside startTransition is non-urgent. If the user types another character before the fetch completes, React can interrupt the in-progress setResults update and start fresh with the new query value. The input (setQuery) remains urgent and always updates immediately.",
    "debounceComparison": "Debounce delays the API call by a fixed time (300ms). useTransition does not delay — it starts immediately but yields to higher-priority updates. For fast networks, useTransition results appear faster than debounce; for slow networks, debounce reduces API call volume. Combine both for optimal behavior."
  },
  "rejectedOutput": {
    "command": "Add a debounce: const debouncedFetch = debounce(fetchResults, 300). Replace fetchResults(value) with debouncedFetch(value). This delays the API call by 300ms after the last keystroke."
  },
  "arbiters": ["reactConcurrencyArbiter"],
  "whyAccepted": "useTransition addresses the UI lag problem at its source — React concurrent mode allows the input update (urgent) to preempt the result render (non-urgent). Debounce reduces API call frequency but does not fix the synchronous re-render lag during typing. useTransition is the React 18 idiomatic solution for this exact scenario.",
  "whyRejected": "Debounce reduces unnecessary API calls but does not address the render performance problem. If the result list has 100 items and re-rendering takes 50ms, debounce still causes a 50ms freeze after each debounce timeout. useTransition eliminates the freeze by making the render interruptible.",
  "sourceAnchors": [
    {
      "sourceId": "REACT-ADVANCED",
      "url": "https://github.com/facebook/react",
      "section": "useTransition — non-urgent state updates; startTransition for concurrent rendering",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-200

```json
{
  "rowId": "EXT-200",
  "sourceIds": ["MICROSOFT-LEARN-ARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "resilience-pattern-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Our payment gateway integration is failing intermittently — about 5% of calls fail with a 503 (Service Unavailable) response. We're retrying 3 times with exponential backoff. The failure rate has increased to 15% over the past hour. Diagnose and prescribe the resilience pattern change.",
    "b": "Current configuration: RetryPolicy: maxAttempts=3, backoff=exponential (100ms, 200ms, 400ms). Circuit breaker: not configured. Monitoring shows: payment gateway CPU at 90%, error rate rising. Our service is making 1,000 calls/minute to the gateway.",
    "c": "Context: Circuit breaker pattern. When a downstream service shows elevated error rates, retries amplify load on an already-stressed system (retry storm). A circuit breaker monitors failure rate; when it exceeds a threshold, the breaker opens and requests fail immediately without calling the downstream service. This gives the downstream service time to recover. After a configured timeout, the breaker enters half-open state and allows test requests through."
  },
  "function": "Arbitrate: what is the diagnosis and what resilience pattern change is required?",
  "acceptedOutput": {
    "diagnosis": "Retry storm. The payment gateway is at 90% CPU. Our service is making 1,000 calls/minute, each failing 15% of the time. Each failure triggers 3 retries, multiplying the actual call volume to up to 4,000 calls/minute (1 initial + 3 retries × failure rate). This amplified load is driving the gateway further into saturation, increasing the failure rate from 5% to 15%.",
    "prescription": "Add a circuit breaker: failureThreshold=50%, samplingDuration=60s, openDuration=30s. When 50% of calls in the last 60 seconds fail, open the circuit. For the next 30 seconds, all payment calls fail immediately without hitting the gateway — allowing it to recover. After 30 seconds, enter half-open state: allow 10 test requests. If they succeed, close the circuit. If they fail, reopen for another 30 seconds.",
    "retryAdjustment": "Reduce retry attempts from 3 to 1 during the circuit breaker deployment. With a circuit breaker in place, retries are only appropriate for transient single-request failures, not systemic overload."
  },
  "rejectedOutput": {
    "diagnosis": "The backoff intervals are too short. Increase maxAttempts to 5 and extend backoff to 1s, 2s, 4s, 8s, 16s. This gives the gateway more time to recover between retries."
  },
  "arbiters": ["resiliencePatternArbiter", "retryStormArbiter"],
  "whyAccepted": "The diagnosis correctly identifies the retry storm. 1,000 calls/minute × 15% failure × 3 retries = up to 450 additional calls/minute from retries alone. Increasing retry count or extending backoff worsens the storm by keeping retry pressure on the gateway longer. A circuit breaker breaks the amplification loop by stopping calls entirely when the gateway is saturated.",
  "whyRejected": "Increasing retries from 3 to 5 with longer backoffs would increase the total call amplification from 3× to 5× of the failure rate. This makes the retry storm significantly worse. The rejected output treats a systemic overload as a timing problem, which it is not.",
  "sourceAnchors": [
    {
      "sourceId": "MICROSOFT-LEARN-ARCH",
      "url": "https://learn.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker",
      "section": "Circuit breaker pattern — failure threshold, open/half-open/closed states; retry storm diagnosis",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-201

```json
{
  "rowId": "EXT-201",
  "sourceIds": ["ASPNETCORE-DOCS-SAMPLES"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "B_ROUTING",
  "subdomain": "aspnetcore-request-pipeline",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "In our ASP.NET Core Minimal API (Application Programming Interface), we need to validate that every request to /api/orders includes a valid X-Tenant-Id header. Should we use endpoint filters or middleware for this validation?",
    "b": "Current setup: Minimal API with endpoints registered via app.MapGet, app.MapPost. The tenant validation logic checks the X-Tenant-Id header against a list of registered tenant IDs in the database. The validation should apply only to /api/orders endpoints, not to /health or /metrics.",
    "c": "Context: ASP.NET Core request pipeline. Middleware: runs for all requests matching the middleware registration position. Endpoint filters: run only for specific endpoints or endpoint groups. For cross-cutting concerns that apply to a subset of endpoints, endpoint filters avoid the conditional logic required in middleware ('if request is to /api/orders, validate'). IEndpointFilter executes after route matching, so the endpoint is known when the filter runs."
  },
  "function": "Produce the command: endpoint filter or middleware, with implementation sketch.",
  "acceptedOutput": {
    "command": "Use an endpoint filter applied to the /api/orders route group.",
    "implementation": "var ordersGroup = app.MapGroup('/api/orders').AddEndpointFilter<TenantValidationFilter>();\nordersGroup.MapGet('/', GetOrders);\nordersGroup.MapPost('/', CreateOrder);\n\npublic class TenantValidationFilter : IEndpointFilter {\n    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {\n        var tenantId = context.HttpContext.Request.Headers['X-Tenant-Id'].FirstOrDefault();\n        if (string.IsNullOrEmpty(tenantId) || !await IsValidTenant(tenantId)) {\n            return Results.Problem('Invalid or missing X-Tenant-Id', statusCode: 401);\n        }\n        return await next(context);\n    }\n}",
    "rationale": "The filter applies to all endpoints in the ordersGroup without conditional path-matching logic. /health and /metrics are not in the group and are unaffected. Adding a new order endpoint via ordersGroup.MapPut automatically inherits the filter."
  },
  "rejectedOutput": {
    "command": "Use middleware. app.Use(async (context, next) => { if (context.Request.Path.StartsWithSegments('/api/orders')) { /* validate */ } else { await next(); } })."
  },
  "arbiters": ["aspnetCorePipelineArbiter"],
  "whyAccepted": "Endpoint filters are the idiomatic ASP.NET Core Minimal API mechanism for per-endpoint or per-group validation. The route group approach ensures the validation is co-located with the endpoint registrations — visible to any developer adding a new order endpoint. The middleware approach requires remembering to update the path condition for every new endpoint.",
  "whyRejected": "Middleware with StartsWithSegments is a path-matching hack that bypasses the routing system. It runs before route matching, requires manual path maintenance, and applies to the entire request pipeline rather than specific endpoint registrations.",
  "sourceAnchors": [
    {
      "sourceId": "ASPNETCORE-DOCS-SAMPLES",
      "url": "https://github.com/dotnet/AspNetCore.Docs.Samples",
      "section": "Minimal API endpoint filters — IEndpointFilter, MapGroup, per-group filter registration",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-202

```json
{
  "rowId": "EXT-202",
  "sourceIds": ["NESTJS-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "nestjs-pipeline-execution-order",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A NestJS developer placed authentication logic in an interceptor, authorization logic in a guard, and request logging in middleware. Review this placement for correctness.",
    "b": "Code structure: @UseInterceptors(AuthInterceptor) on the controller — AuthInterceptor checks JWT (JSON Web Token) validity. @UseGuards(RbacGuard) on the handler — RbacGuard checks user roles. GlobalMiddleware registered in main.ts — logs request method, path, and duration.",
    "c": "NestJS execution order: Middleware → Guards → Interceptors (before) → Pipes → Handler → Interceptors (after) → Exception Filters. Guards run after middleware but before interceptors. A guard's purpose is to determine whether a request should proceed (authorization). An interceptor's purpose is to transform the request/response or add behavior (logging, caching, serialization). Placing authentication in an interceptor means it runs after guards — a guard could allow a request through before authentication is verified."
  },
  "function": "Arbitrate: is the placement correct? If not, what is the correct placement?",
  "acceptedOutput": {
    "verdict": "INCORRECT placement. Authentication is in the wrong layer.",
    "correctPlacement": {
      "middleware": "Request logging — correct. Middleware runs first, has access to raw request/response, appropriate for cross-cutting logging.",
      "guard": "Authentication (JWT validation) — must move from interceptor to a guard (e.g., JwtAuthGuard). Guards run before interceptors and before the handler. Authentication must occur before authorization (RbacGuard) can run.",
      "guard_order": "Guard execution order: NestJS runs guards in the order they are declared. @UseGuards(JwtAuthGuard, RbacGuard) — JwtAuthGuard runs first, validates JWT and attaches user to request context. RbacGuard runs second, reads user.roles from request context set by JwtAuthGuard.",
      "interceptor": "Response transformation, caching, or timing — appropriate for interceptors. Remove authentication from interceptor."
    }
  },
  "rejectedOutput": {
    "verdict": "The placement is acceptable. Authentication in an interceptor still runs before the handler, so the request is authenticated before business logic executes."
  },
  "arbiters": ["nestjsPipelineArbiter", "securityLayeringArbiter"],
  "whyAccepted": "The NestJS execution order is Guards → Interceptors, not Interceptors → Guards. With authentication in an interceptor and authorization in a guard, the RbacGuard (authorization) runs before the AuthInterceptor (authentication). A request could pass the RbacGuard without being authenticated, then fail authentication in the interceptor — but the guard has already evaluated roles on an unauthenticated request context.",
  "whyRejected": "The rejected verdict assumes interceptors run before handlers, which is true, but misses that guards run before interceptors. The security consequence is real: authorization (RbacGuard) evaluates before authentication (AuthInterceptor) completes, creating a window where unauthorized access may pass the authorization check.",
  "sourceAnchors": [
    {
      "sourceId": "NESTJS-ADVANCED",
      "url": "https://github.com/nestjs/nest",
      "section": "Request lifecycle — Middleware, Guards, Interceptors execution order; authentication in guards",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-203

```json
{
  "rowId": "EXT-203",
  "sourceIds": ["GOOGLE-AIP"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "api-resource-revision-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We're designing an API for document management. Documents can be revised multiple times. We need to access both the current version and historical versions. Should we use AIP-162 resource revisions or a separate 'document_versions' resource?",
    "b": "Requirements: GET /documents/{id} returns the current document. GET /documents/{id}@{revision_id} returns a specific historical revision. LIST /documents/{id}/revisions lists all revisions. Revision creation is implicit — every update to a document creates a revision. There are no deletions of revisions.",
    "c": "Context: Google AIP-162 (API Improvement Proposals 162) defines a standard pattern for resource revisions. The revision is addressed via a '@' separator in the resource name: GET /documents/doc-123@abc456. A separate 'document_versions' resource would use GET /documents/doc-123/versions/v5. AIP-162 is optimized for the case where revisions are read-only historical snapshots of the primary resource."
  },
  "function": "Arbitrate: AIP-162 revision pattern or separate document_versions resource?",
  "acceptedOutput": {
    "verdict": "Use AIP-162 resource revisions.",
    "rationale": "The requirements exactly match the AIP-162 use case: (1) Revisions are read-only historical snapshots — no revision deletions. (2) The primary resource (current document) and its revisions share the same data schema. (3) Revision access via '@' suffix (GET /documents/doc-123@abc456) is semantically clear — the '@revision_id' distinguishes revision access from sub-resource access ('/'). (4) LIST /documents/doc-123/revisions is defined by AIP-162 and aligns with the stated requirement.",
    "implementation": "Revision ID generation: use the commit hash or a UUID generated at update time. Expose: GET /documents/{name} — current revision. GET /documents/{name}@{revision_id} — specific revision. POST /documents/{name}:tag — optional: name a specific revision. LIST /documents/{name}/revisions — list all revisions with metadata (revision_id, create_time, author)."
  },
  "rejectedOutput": {
    "verdict": "Use a separate document_versions resource: GET /documents/{id}/versions/{version_number}. This is more RESTful (Representational State Transfer) — sub-resources are a standard REST pattern. AIP-162 is a Google-specific convention."
  },
  "arbiters": ["apiDesignArbiter"],
  "whyAccepted": "AIP-162 is designed for exactly this case. The '@' separator unambiguously signals 'this is a historical snapshot of this resource', which a '/versions/' sub-resource does not convey. AIP-162 is not 'just Google' — it is a published open standard that multiple API design guides cite. The separate versions resource requires versioning the URL structure and managing a parallel resource type.",
  "whyRejected": "A 'document_versions' sub-resource is a valid alternative but introduces a separate resource type with its own CRUD (Create, Read, Update, Delete) surface. For read-only historical snapshots of a primary resource, a separate resource type adds complexity without benefit. AIP-162 exists precisely to standardize this common pattern.",
  "sourceAnchors": [
    {
      "sourceId": "GOOGLE-AIP",
      "url": "https://google.aip.dev/162",
      "section": "AIP-162: Resource revisions — @revision_id addressing, LIST revisions, implicit revision on update",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-204

```json
{
  "rowId": "EXT-204",
  "sourceIds": ["GITHUB-ACTIONS-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "ci-credential-management",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A developer stored an AWS access key and secret in GitHub Actions repository secrets (AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY) for a deployment workflow that pushes to S3. Review this credential management approach.",
    "b": "Workflow: jobs: deploy: steps: - uses: aws-actions/configure-aws-credentials@v4; with: aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}; aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}; region: us-east-1",
    "c": "Context: GitHub Actions OIDC (OpenID Connect) federation. GitHub Actions supports OIDC tokens — short-lived JWT tokens issued per workflow run. AWS IAM (Identity and Access Management) can be configured to accept GitHub OIDC tokens via an IAM Identity Provider. A GitHub Actions workflow using OIDC does not need any stored credentials — AWS generates a short-lived role session token for each workflow run. The IAM role trust policy restricts which GitHub repository/branch can assume it."
  },
  "function": "Arbitrate: is the long-lived access key approach correct? What is the recommended alternative?",
  "acceptedOutput": {
    "verdict": "ANTI-PATTERN. Long-lived AWS access keys in repository secrets are a security risk.",
    "risks": [
      "Credential longevity: Long-lived access keys do not expire unless manually rotated. If compromised (secret exfiltration, repository fork secret exposure), the key remains valid until rotated.",
      "Scope: The access key carries the permissions of the IAM user — often broader than the specific S3 bucket the workflow needs.",
      "Rotation burden: Key rotation requires updating the secret in every repository that uses it."
    ],
    "recommendation": "Migrate to GitHub Actions OIDC federation: (1) Create an IAM Identity Provider in AWS for GitHub Actions (sts.amazonaws.com, OIDC issuer: token.actions.githubusercontent.com). (2) Create an IAM role with a trust policy: StringLike condition on token.actions.githubusercontent.com:sub: repo:myorg/myrepo:ref:refs/heads/main. (3) In workflow: permissions: id-token: write; uses: aws-actions/configure-aws-credentials@v4; with: role-to-assume: arn:aws:iam::123456789:role/github-actions-deploy; aws-region: us-east-1 — no stored secrets required."
  },
  "rejectedOutput": {
    "verdict": "The long-lived key approach is acceptable. GitHub secrets are encrypted at rest and not exposed in logs. Rotate the key every 90 days per security policy."
  },
  "arbiters": ["credentialManagementArbiter", "ciSecurityArbiter"],
  "whyAccepted": "OIDC federation eliminates the entire credential storage risk class. No stored credentials means no credential leakage, no rotation burden, and no long-lived key exposure window. The 90-day rotation policy in the rejected output still leaves a 90-day exposure window if the key is compromised.",
  "whyRejected": "GitHub secrets encryption and log masking reduce exposure but do not eliminate it. The 90-day rotation window is a 90-day exposure window. OIDC tokens are valid for minutes, not months. The rejected verdict accepts a preventable risk class.",
  "sourceAnchors": [
    {
      "sourceId": "GITHUB-ACTIONS-ADVANCED",
      "url": "https://github.com/actions/runner",
      "section": "OIDC federation — GitHub Actions identity provider, IAM role trust policy, no stored credentials",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-205

```json
{
  "rowId": "EXT-205",
  "sourceIds": ["OPENASSISTANT-OASST1"],
  "sourceTrustTier": 2,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "deployment-failure-user-communication",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Translate this machine result into a user-facing message: { status: 'DEPLOYMENT_FAILED', errorCode: 'IMAGE_PULL_BACKOFF', affectedService: 'payment-api', retryCount: 3, lastAttemptTimestamp: '2026-05-04T14:22:00Z' }",
    "b": "Target audience: operations team member who triggered the deployment from a web dashboard. The operations team member is technical but does not read Kubernetes internals daily.",
    "c": "Context: ImagePullBackOff in Kubernetes means the container runtime could not pull the specified container image. Common causes: wrong image tag, image registry authentication failure, image not pushed before deployment triggered. The retryCount=3 means Kubernetes has attempted 3 times with exponential backoff."
  },
  "function": "Produce the final user-facing message for the deployment failure result.",
  "acceptedOutput": {
    "userMessage": "Deployment failed: payment-api",
    "detail": "The deployment could not start because Kubernetes was unable to pull the container image (ImagePullBackOff). After 3 attempts, the image is still unavailable.",
    "likelyCauses": [
      "The image tag specified in the deployment does not exist in the registry",
      "The image was not pushed to the registry before the deployment was triggered",
      "Registry authentication credentials have expired or are incorrect"
    ],
    "nextSteps": [
      "Verify the image tag exists: docker pull <image>:<tag>",
      "Check the registry credentials in the Kubernetes secret: kubectl get secret registry-credentials -o yaml",
      "Re-trigger the deployment after confirming the image is available"
    ],
    "timestamp": "2026-05-04T14:22:00Z"
  },
  "rejectedOutput": {
    "userMessage": "Error: IMAGE_PULL_BACKOFF on payment-api after 3 retries at 2026-05-04T14:22:00Z. Check Kubernetes pod logs."
  },
  "arbiters": ["userCommunicationArbiter"],
  "whyAccepted": "The accepted message translates the internal error code (IMAGE_PULL_BACKOFF) into its meaning, provides actionable next steps, and lists likely causes in order of frequency. An operations team member can act on this message without reading Kubernetes documentation.",
  "whyRejected": "The rejected message exposes the raw error code and instructs the user to check pod logs — which would show the same ImagePullBackOff error in a different format. It does not explain what ImagePullBackOff means, why it happens, or what to do about it.",
  "sourceAnchors": [
    {
      "sourceId": "OPENASSISTANT-OASST1",
      "url": "https://huggingface.co/datasets/OpenAssistant/oasst1",
      "section": "Operations team deployment communication — machine error to human-readable action steps",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-206

```json
{
  "rowId": "EXT-206",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "batch-validation-with-accumulation",
  "rowClass": "negative-example",
  "input": {
    "a": "Execute: validate a batch of email addresses. Return all invalid addresses with their specific error reason (empty, malformed format, domain missing). Do not stop at the first invalid address — collect all errors.",
    "b": "Function signature: validate_email_batch(emails: list[str]) -> ValidationResult. ValidationResult has fields: valid_count: int, invalid_entries: list[dict] where each dict has 'email' and 'reason'.",
    "c": "Validation rules: (1) Empty string or None → reason: 'empty'. (2) No '@' character → reason: 'no_at_symbol'. (3) No domain after '@' → reason: 'missing_domain'. (4) Otherwise: valid. Order of validation: check empty first, then format checks."
  },
  "function": "Implement validate_email_batch with full error accumulation.",
  "acceptedOutput": {
    "implementation": "def validate_email_batch(emails: list) -> dict:\n    valid_count = 0\n    invalid_entries = []\n    for email in emails:\n        if not email:\n            invalid_entries.append({'email': email, 'reason': 'empty'})\n        elif '@' not in email:\n            invalid_entries.append({'email': email, 'reason': 'no_at_symbol'})\n        elif email.split('@')[1] == '':\n            invalid_entries.append({'email': email, 'reason': 'missing_domain'})\n        else:\n            valid_count += 1\n    return {'valid_count': valid_count, 'invalid_entries': invalid_entries}",
    "behavior": "For input ['alice@example.com', '', 'bob', 'carol@', 'dave@corp.com']: valid_count=2, invalid_entries=[{'email':'','reason':'empty'},{'email':'bob','reason':'no_at_symbol'},{'email':'carol@','reason':'missing_domain'}]"
  },
  "rejectedOutput": {
    "implementation": "def validate_email_batch(emails):\n    for email in emails:\n        if not email:\n            raise ValueError(f'Empty email found')\n        if '@' not in email:\n            raise ValueError(f'Invalid email: {email}')\n    return {'valid_count': len(emails), 'invalid_entries': []}"
  },
  "arbiters": ["batchValidationArbiter"],
  "whyAccepted": "The accepted implementation accumulates all errors before returning. A batch of 100 emails with 5 invalid entries produces a complete error report in one pass. The caller can display all errors to the user at once.",
  "whyRejected": "The rejected implementation raises ValueError at the first invalid email and stops. For a batch of 100 emails, the caller must fix one error, re-submit, discover the next error, fix it, and repeat — up to 100 round trips. The rejected implementation also misclassifies the missing_domain case (it would fall through to the valid_count return incorrectly).",
  "sourceAnchors": [
    {
      "sourceId": "BIGCODEBENCH",
      "url": "https://github.com/bigcode-project/bigcodebench",
      "section": "Task: validate_email_batch — full error accumulation vs fail-fast on first error",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-207

```json
{
  "rowId": "EXT-207",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "multi-file-refactor-review",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Aggregate results from four analysis agents for a pull request that renames the Python module 'utils' to 'helpers' across the codebase.",
    "b": "Agent results: Agent 1 (Import Scanner): PARTIAL_SUCCESS — 47 of 49 import statements updated. 2 files in the 'legacy/' directory still import from 'utils'. Agent 2 (Test Agent): FAIL — 3 tests import from 'utils' directly and fail with ModuleNotFoundError. Agent 3 (Type Checker): PASS — mypy passes. Agent 4 (Documentation Agent): PASS — module references in docs updated.",
    "c": "Context: a rename refactor requires 100% reference update coverage to be mergeable. Partial updates leave the codebase in an inconsistent state — some code imports from 'utils' (deleted), some from 'helpers' (new). The 2 legacy files and 3 test files that still reference 'utils' will cause ImportError at runtime and test failures."
  },
  "function": "Produce the aggregated result for this pull request.",
  "acceptedOutput": {
    "aggregatedResult": "BLOCK",
    "resultCode": "INCOMPLETE_RENAME",
    "summary": "The module rename is incomplete. 5 files (2 in legacy/, 3 test files) still import from the old 'utils' module. These will fail with ModuleNotFoundError at runtime and in the test suite. The PR cannot be merged until all 49 import references are updated.",
    "requiredActions": [
      "Update imports in legacy/report_generator.py and legacy/export_handler.py",
      "Update imports in tests/test_data_processor.py, tests/test_formatter.py, and tests/test_exporter.py",
      "Re-run tests: all 3 failing tests must pass before merge"
    ],
    "passingAgents": ["Type Checker", "Documentation Agent"],
    "failingAgents": ["Import Scanner (2 files missing)", "Test Agent (3 tests failing)"]
  },
  "rejectedOutput": {
    "aggregatedResult": "PARTIAL_SUCCESS",
    "summary": "95.9% of imports updated (47/49). Tests fail in 3 files. Recommend merging with a follow-up ticket to fix legacy imports."
  },
  "arbiters": ["refactorCompletenessArbiter"],
  "whyAccepted": "A rename refactor with missing references is not a partial success — it leaves the codebase in a broken state where importing the old module name causes ImportError. The 3 test failures confirm the breakage. A BLOCK verdict is required. 'Follow-up ticket' is not an acceptable outcome for a refactor that breaks tests.",
  "whyRejected": "95.9% coverage sounds high, but a 4.1% gap in a rename refactor produces 100% failure for any code path that touches the 5 unmigrated files. Percentage coverage is the wrong metric for rename completeness — the only correct metric is zero remaining references.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-BENCH",
      "url": "https://github.com/princeton-nlp/SWE-bench",
      "section": "Multi-file rename refactor — import coverage completeness; BLOCK vs PARTIAL_SUCCESS for incomplete rename",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-208

```json
{
  "rowId": "EXT-208",
  "sourceIds": ["PROJECT-GUTENBERG"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "ocr-artifact-normalization",
  "rowClass": "negative-example",
  "input": {
    "a": "Execute: remove OCR (Optical Character Recognition) artifacts from a Project Gutenberg plain text file. Common artifacts: 'rn' substituted for 'm' (e.g., 'cornrnand' instead of 'command'), 'l' substituted for '1' (e.g., 'l00' instead of '100'), ligature characters (fi, fl rendered as 'ﬁ', 'ﬂ').",
    "b": "Input text excerpt: 'The systern rnust process l00 cornrnands per second. The ﬁle contains ﬂow control logic.' Expected output: 'The system must process 100 commands per second. The file contains flow control logic.'",
    "c": "Processing constraints: the substitution must be contextual — 'rn' in legitimate words like 'earn' or 'barn' must not be changed. Only 'rn' sequences where 'm' is the statistically more probable letter (based on language model context) should be replaced. Ligature normalization (ﬁ→fi, ﬂ→fl) is always safe — ligatures have no standalone semantic meaning."
  },
  "function": "Implement the OCR normalization pipeline for the given text excerpt.",
  "acceptedOutput": {
    "implementation": "Step 1 — Ligature normalization (always safe, no context required): text = text.replace('\\uFB01', 'fi').replace('\\uFB02', 'fl')  # U+FB01=ﬁ, U+FB02=ﬂ\nStep 2 — Digit substitution (contextual: 'l' before or after digits): import re; text = re.sub(r'(?<=\\d)l|l(?=\\d)', '1', text)  # 'l00' → '100', 'l0l' → '101'\nStep 3 — 'rn'→'m' substitution (requires spell-check or language model): flag all 'rn' occurrences for review — do not blindly replace. Use pyspellchecker or a unigram frequency model: replace 'rn' with 'm' only if the corrected word has higher frequency than the original.",
    "output": "The system must process 100 commands per second. The file contains flow control logic.",
    "note": "Ligatures and digit-context substitutions are deterministic. The rn→m substitution requires a language model to avoid corrupting legitimate 'rn' sequences (e.g., 'burn', 'earn', 'intern')."
  },
  "rejectedOutput": {
    "implementation": "text = text.replace('rn', 'm').replace('l', '1').replace('\\uFB01', 'fi').replace('\\uFB02', 'fl')",
    "output": "The system must process 100 commands per second. The fie contains flow control logic."
  },
  "arbiters": ["textNormalizationArbiter"],
  "whyAccepted": "The accepted implementation applies safe substitutions (ligatures, digit-context) deterministically and flags ambiguous substitutions ('rn'→'m') for language-model adjudication. The result is correct normalization without corruption of legitimate text.",
  "whyRejected": "The rejected implementation blindly replaces all 'l' with '1' — converting 'The file' to 'The 1ie' and 'flow' to '1ow'. Blind 'rn'→'m' replacement would corrupt 'burn' to 'bum', 'barn' to 'bam', 'intern' to 'intem'. Global string replacement without context is wrong for OCR normalization.",
  "sourceAnchors": [
    {
      "sourceId": "PROJECT-GUTENBERG",
      "url": "https://www.gutenberg.org",
      "section": "OCR artifact normalization — ligature substitution, digit-context l→1, contextual rn→m",
      "license": "Public domain"
    }
  ],
  "licenseConstraint": "Public domain",
  "trainingStatus": "candidate"
}
```

---

## EXT-209

```json
{
  "rowId": "EXT-209",
  "sourceIds": ["PYTHON-PEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "structural-pattern-matching-adoption",
  "rowClass": "migration-decision",
  "input": {
    "a": "A Python 3.10 codebase has a function that dispatches on a command type using an if-elif chain with 8 branches. A reviewer suggested migrating to structural pattern matching (PEP 634). Should the team migrate?",
    "b": "Current code: def handle_command(cmd): if cmd['type'] == 'create': ... elif cmd['type'] == 'update': ... elif cmd['type'] == 'delete': ... elif cmd['type'] == 'list': ... elif cmd['type'] == 'get': ... elif cmd['type'] == 'batch_create': ... elif cmd['type'] == 'batch_delete': ... elif cmd['type'] == 'import': ... else: raise ValueError(f'Unknown command: {cmd[\"type\"]}')",
    "c": "PEP 634 structural pattern matching (Python 3.10+): match cmd: case {'type': 'create', **rest}: ... case {'type': 'update', **rest}: ... — supports destructuring, guard clauses, and wildcard patterns. The 'case _:' pattern replaces the else branch. Pattern matching also enables destructuring the matched dict in the case clause, reducing boilerplate for commands that need to extract additional fields."
  },
  "function": "Decide: migrate to structural pattern matching or preserve the if-elif chain.",
  "acceptedOutput": {
    "decision": "MIGRATE.",
    "rationale": "The 8-branch if-elif chain on dict key equality is the primary use case structural pattern matching was designed for. Three concrete improvements: (1) Exhaustiveness — with a wildcard 'case _:' branch, the reader sees all handled cases in one structure. Adding a new command type requires a new 'case' clause, not an elif at the correct depth. (2) Destructuring — 'case {\"type\": \"create\", \"name\": name, \"schema\": schema}:' extracts fields in the pattern without additional dict access. (3) Guard clauses — 'case {\"type\": \"batch_delete\"} if len(cmd[\"ids\"]) <= 100:' adds conditions without nested if-statements inside the branch.",
    "migrationPattern": "Before: if cmd['type'] == 'create': create_resource(cmd['name'], cmd['schema']) — After: case {'type': 'create', 'name': name, 'schema': schema}: create_resource(name, schema)",
    "gate": "All 8 branches migrated, existing tests pass, match exhaustiveness verified (case _ raises ValueError for unknown types)."
  },
  "rejectedOutput": {
    "decision": "DO NOT MIGRATE. The if-elif chain is universally readable across Python 2, 3.9, and 3.10+. Pattern matching is Python 3.10+ only and not all team members are familiar with it. The existing code is clear and maintainable."
  },
  "arbiters": ["languageModernizationArbiter"],
  "whyAccepted": "The codebase is already Python 3.10 — the version compatibility argument does not apply. The 8-branch elif dispatch on a single dict key is precisely the structural pattern matching use case. The destructuring benefit eliminates repeated cmd['field'] access inside each branch.",
  "whyRejected": "The Python 3.10+ restriction is already satisfied. The 'team familiarity' argument is a one-time learning cost, not a permanent constraint. An 8-branch elif chain grows in complexity as commands are added; structural pattern matching scales better because each case is self-contained.",
  "sourceAnchors": [
    {
      "sourceId": "PYTHON-PEPS",
      "url": "https://peps.python.org/pep-0634/",
      "section": "PEP 634 — Structural Pattern Matching; mapping patterns; match vs if-elif dispatch",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-210

```json
{
  "rowId": "EXT-210",
  "sourceIds": ["IETF-RFCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "json-number-precision",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A financial API returns transaction amounts as JSON numbers: { 'amount': 9007199254740993, 'currency': 'USD' }. A code reviewer flagged this as a potential precision problem. Evaluate the reviewer's concern.",
    "b": "The value 9007199254740993 is a JavaScript safe integer boundary case. RFC 8259 (JSON) does not restrict number size, but IEEE 754 double-precision (64-bit) floating point — used by most JSON parsers — can only represent integers exactly up to 2^53 = 9007199254740992. The value 9007199254740993 (2^53 + 1) cannot be represented exactly in a double.",
    "c": "Context: JSON number parsing. Python's json.loads preserves integer precision for integer-valued JSON numbers. JavaScript's JSON.parse converts all numbers to IEEE 754 double — 9007199254740993 parses as 9007199254740992 (loses 1 unit). Java's Jackson deserializes to long (64-bit integer, preserves precision) or double (loses precision) depending on the target type."
  },
  "function": "Arbitrate: is the reviewer's concern valid? What is the correct fix?",
  "acceptedOutput": {
    "verdict": "VALID CONCERN. The API has a cross-language precision defect for clients using IEEE 754 double-precision parsing.",
    "affectedClients": "Any JavaScript client using JSON.parse will silently lose 1 unit of precision on this specific value. For financial amounts, a 1-unit error in dollars is acceptable, but a 1-unit error in cents (if the value is in cents) represents $0.01 loss per transaction.",
    "fix": "Represent financial amounts as strings in JSON: { 'amount': '9007199254740993', 'currency': 'USD' }. The consuming client parses the string to its native decimal type (Python Decimal, Java BigDecimal, JavaScript BigInt). Alternatively, use a sub-unit integer within safe range (e.g., amounts in millicents, capped at well under 2^53) and document the unit explicitly.",
    "rfcReference": "RFC 8259 Section 6: 'Since software that implements IEEE 754 binary64 is extremely common... good interoperability can be achieved by implementations that expect no more precision or range than these provide.' Financial APIs must not rely on this interoperability for values exceeding 2^53."
  },
  "rejectedOutput": {
    "verdict": "The reviewer's concern is theoretical. In practice, modern JSON parsers handle large integers correctly. The API is fine as-is."
  },
  "arbiters": ["apiPrecisionArbiter", "rfcComplianceArbiter"],
  "whyAccepted": "The concern is not theoretical — it is a documented IEEE 754 limitation. JavaScript's JSON.parse silently truncates integers above 2^53. For any client using the browser's built-in JSON.parse (which is the default in every JavaScript frontend), the value 9007199254740993 parses as 9007199254740992. In a financial context, silent precision loss is a defect, not a theoretical concern.",
  "whyRejected": "The rejected verdict dismisses the concern without engaging with the specific value (above 2^53) or the specific parser behavior (JavaScript's IEEE 754 coercion). 'Modern parsers handle large integers correctly' is false for JavaScript's built-in JSON.parse — it uses IEEE 754 double for all numbers.",
  "sourceAnchors": [
    {
      "sourceId": "IETF-RFCS",
      "url": "https://www.rfc-editor.org/rfc/rfc8259",
      "section": "RFC 8259 Section 6 — JSON number interoperability note; IEEE 754 double precision limit for integers",
      "license": "IETF Trust provisions"
    }
  ],
  "licenseConstraint": "IETF Trust provisions",
  "trainingStatus": "candidate"
}
```
