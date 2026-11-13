# External Training Rows — Batch 5
## Row IDs: EXT-091 through EXT-110
## Date: 2026-05-04
## Focus: Families 3, 4, 6, 7 (gap closure); CWE entries 11–15; Groups 8–15 continued

---

## EXT-091

```json
{
  "rowId": "EXT-091",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "function-implementation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Implement this function specification: parse_log_entries(log_text: str) → list[dict]. The function receives a multi-line log string where each line has format 'YYYY-MM-DDTHH:MM:SSZ LEVEL message'. It must return a list of dicts with keys: timestamp (datetime), level (str), message (str). Lines that do not match the format must be silently skipped.",
    "b": "Failing test cases: parse_log_entries('2026-05-04T13:00:00Z INFO Server started') == [{'timestamp': datetime(2026,5,4,13,0,0), 'level': 'INFO', 'message': 'Server started'}]; parse_log_entries('INVALID LINE\\n2026-05-04T13:01:00Z ERROR Timeout') == [{'timestamp': datetime(2026,5,4,13,1,0), 'level': 'ERROR', 'message': 'Timeout'}]",
    "c": "Context: BigCodeBench function implementation task. The implementation must handle: (1) correct ISO timestamp parsing, (2) silent skip of malformed lines, (3) multi-word messages (message may contain spaces). The returned dicts must use datetime objects, not strings, for the timestamp field."
  },
  "function": "Produce the domain result: the implemented function that passes both test cases.",
  "acceptedOutput": {
    "implementation": "import re\nfrom datetime import datetime\n\ndef parse_log_entries(log_text: str) -> list[dict]:\n    pattern = re.compile(\n        r'^(\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}Z)\\s+(\\S+)\\s+(.+)$'\n    )\n    results = []\n    for line in log_text.splitlines():\n        match = pattern.match(line.strip())\n        if match:\n            ts_str, level, message = match.groups()\n            results.append({\n                'timestamp': datetime.strptime(ts_str, '%Y-%m-%dT%H:%M:%SZ'),\n                'level': level,\n                'message': message\n            })\n    return results",
    "testVerification": "Both test cases pass. parse_log_entries with valid line returns list with one dict, timestamp is datetime object. parse_log_entries with INVALID LINE + valid line returns list with one dict (invalid line silently skipped)."
  },
  "rejectedOutput": {
    "implementation": "def parse_log_entries(log_text):\n    results = []\n    for line in log_text.split('\\n'):\n        parts = line.split(' ', 2)\n        if len(parts) == 3:\n            results.append({'timestamp': parts[0], 'level': parts[1], 'message': parts[2]})\n    return results"
  },
  "arbiters": ["implementationCorrectnessArbiter"],
  "whyAccepted": "The accepted implementation uses a regex to correctly parse the ISO timestamp (including the T separator and Z suffix), returns datetime objects (not strings), and skips malformed lines silently. splitlines() handles all line ending variants.",
  "whyRejected": "The rejected implementation uses split(' ', 2) which fails if the timestamp contains unexpected whitespace, returns the timestamp as a string instead of a datetime object (test will fail on equality check), and does not skip invalid lines — it accepts any 3-token line including INVALID LINE if it has two spaces.",
  "sourceAnchors": [
    {
      "sourceId": "BIGCODEBENCH",
      "url": "https://github.com/bigcode-project/bigcodebench",
      "section": "Dataset format — function signature plus docstring plus test suite",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-092

```json
{
  "rowId": "EXT-092",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "function-implementation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Implement: retry_with_backoff(func: Callable, max_retries: int = 3, base_delay: float = 0.5) → Any. Calls func(). If func raises an exception, waits base_delay * 2^(attempt-1) seconds and retries. Returns the result on success. Raises the last exception after max_retries failures. Does not retry on KeyboardInterrupt or SystemExit.",
    "b": "Test: retry_with_backoff(lambda: 42) == 42. retry_with_backoff(failing_func, max_retries=2) raises ValueError after 2 retries. Delays between retries should be approximately 0.5s and 1.0s for base_delay=0.5.",
    "c": "Context: BigCodeBench implementation task. Key correctness criteria: (1) exception re-raise after max_retries, not on first failure, (2) exponential delay formula, (3) KeyboardInterrupt and SystemExit must not be caught and retried — they must propagate immediately."
  },
  "function": "Produce the implementation that passes the retry, re-raise, and non-retry-on-system-exit criteria.",
  "acceptedOutput": {
    "implementation": "import time\nfrom typing import Callable, Any\n\ndef retry_with_backoff(func: Callable, max_retries: int = 3, base_delay: float = 0.5) -> Any:\n    last_exc = None\n    for attempt in range(1, max_retries + 1):\n        try:\n            return func()\n        except (KeyboardInterrupt, SystemExit):\n            raise\n        except Exception as e:\n            last_exc = e\n            if attempt < max_retries:\n                time.sleep(base_delay * (2 ** (attempt - 1)))\n    raise last_exc",
    "delaySequence": "attempt 1 fails → sleep 0.5s; attempt 2 fails → sleep 1.0s; attempt 3 fails → raise. For max_retries=2: attempt 1 fails → sleep 0.5s; attempt 2 fails → raise."
  },
  "rejectedOutput": {
    "implementation": "import time\n\ndef retry_with_backoff(func, max_retries=3, base_delay=0.5):\n    for i in range(max_retries):\n        try:\n            return func()\n        except Exception:\n            time.sleep(base_delay * (2 ** i))\n    return func()"
  },
  "arbiters": ["implementationCorrectnessArbiter"],
  "whyAccepted": "The accepted implementation correctly: re-raises KeyboardInterrupt and SystemExit before the retry loop catches them; stores last_exc to re-raise the original exception; only sleeps when another retry will follow (attempt < max_retries); raises the last exception after exhausting retries.",
  "whyRejected": "The rejected implementation has three bugs: (1) calls func() one extra time after max_retries loop exhausts, potentially raising an unhandled exception outside the try block; (2) sleeps after the final failed attempt (wasted delay); (3) does not re-raise KeyboardInterrupt — it would retry on Ctrl+C, making the function uninterruptible.",
  "sourceAnchors": [
    {
      "sourceId": "BIGCODEBENCH",
      "url": "https://github.com/bigcode-project/bigcodebench",
      "section": "Retry and backoff task instances",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-093

```json
{
  "rowId": "EXT-093",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "result-aggregation-with-conflict",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Aggregate these three domain results for a password-change request. Detect and surface any cross-result inconsistency before producing resultI.",
    "b": "auth_result = { status: 'SUCCESS', userId: 'usr_991', oldPasswordVerified: true, sessionInvalidated: true }; data_result = { status: 'SUCCESS', userId: 'usr_992', passwordUpdated: true, updatedAt: '2026-05-04T13:10:00Z' }; notification_result = { status: 'SUCCESS', notifiedEmail: 'user@example.com', notificationType: 'PASSWORD_CHANGED' }.",
    "c": "Context: domainResultsArray_to_resultI. The aggregation stage must detect cross-result inconsistencies before forwarding to arbitration. auth_result.userId is 'usr_991' and data_result.userId is 'usr_992' — these do not match. One of the processors operated on the wrong user record."
  },
  "function": "Aggregate the three domain results. Detect the userId mismatch. Produce a resultI that surfaces the inconsistency rather than suppressing it.",
  "acceptedOutput": {
    "resultI": {
      "requestId": "req_password_change",
      "overallStatus": "INCONSISTENCY_DETECTED",
      "inconsistency": {
        "field": "userId",
        "auth_result_value": "usr_991",
        "data_result_value": "usr_992",
        "severity": "CRITICAL",
        "description": "Authentication and data processors operated on different user IDs. The password change may have been applied to the wrong user record."
      },
      "arbitrationTargets": ["crossResultConsistencyArbiter"],
      "reprocessCandidate": true,
      "reprocessScope": "Identify which userId is correct for this request. Verify whether the data_result password update was applied to usr_992 or usr_991. Roll back if the update was applied to the wrong user."
    }
  },
  "rejectedOutput": {
    "resultI": {
      "overallStatus": "SUCCESS",
      "userId": "usr_991",
      "passwordUpdated": true,
      "sessionInvalidated": true,
      "notified": true
    }
  },
  "arbiters": ["crossResultConsistencyArbiter"],
  "whyAccepted": "The aggregation stage must detect cross-result inconsistencies because downstream nodes (arbiter, formatter) do not receive the individual domain results — they only see the resultI. A userId mismatch is a critical security inconsistency that must be surfaced, not resolved by picking one value.",
  "whyRejected": "The rejected output picks usr_991 from auth_result and marks the result SUCCESS, discarding the mismatch. If the password update was applied to usr_992 (wrong user), the rejected resultI sends the formatter a success message for the wrong user's account modification.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-BENCH",
      "url": "https://github.com/swe-bench/SWE-bench",
      "section": "Multi-step task result consistency — cross-result field validation",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-094

```json
{
  "rowId": "EXT-094",
  "sourceIds": ["OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "result-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Translate this security audit resultI for a security reviewer audience. The reviewer is a professional security engineer who needs a complete technical assessment, not a summary.",
    "b": "resultI = { overallStatus: 'FAIL', findings: [ { arbiter: 'jwtValidationArbiter', severity: 'CRITICAL', finding: 'alg:none accepted', location: 'auth/jwt.service.ts:line 42', cweReference: 'CWE-347', asvs: 'V3.5.1' }, { arbiter: 'sqlInjectionArbiter', severity: 'HIGH', finding: 'string interpolation in query', location: 'data/user.repository.ts:line 88', cweReference: 'CWE-89', asvs: 'V5.3.4' } ], passedArbiters: ['tlsConfigArbiter', 'passwordStorageArbiter'] }",
    "c": "Context: Node 6 (result translator) for security reviewer audience. Security reviewers need: exact location references, CWE mappings, ASVS control references, severity classifications, and the complete list of what passed (to confirm coverage). Internal arbiter names are meaningful to this audience and should be preserved."
  },
  "function": "Translate the security audit resultI for a security reviewer. Preserve all technical identifiers. Format as a structured security finding report.",
  "acceptedOutput": {
    "finalMachineResult": {
      "summary": "Security audit: 2 critical/high findings, 2 controls passed.",
      "findings": [
        {
          "severity": "CRITICAL",
          "title": "JWT algorithm restriction not enforced (alg:none accepted)",
          "location": "auth/jwt.service.ts:42",
          "cwe": "CWE-347 (Improper Verification of Cryptographic Signature)",
          "asvsControl": "ASVS V3.5.1",
          "impact": "Unsigned tokens accepted as valid. Attacker can forge any token without a secret key.",
          "remediation": "Add server-side algorithm allowlist. Reject any token with alg:none or unexpected algorithm."
        },
        {
          "severity": "HIGH",
          "title": "SQL injection via string interpolation",
          "location": "data/user.repository.ts:88",
          "cwe": "CWE-89 (SQL Injection)",
          "asvsControl": "ASVS V5.3.4",
          "impact": "Attacker-controlled input included in SQL query. Authentication bypass, data exfiltration possible.",
          "remediation": "Replace string interpolation with parameterized queries or ORM query builder."
        }
      ],
      "passed": [
        { "control": "TLS configuration", "arbiter": "tlsConfigArbiter" },
        { "control": "Password storage", "arbiter": "passwordStorageArbiter" }
      ]
    }
  },
  "rejectedOutput": {
    "finalMachineResult": "The security audit found 2 issues. One involves authentication tokens and one involves database queries. Two checks passed. Please fix the issues before deployment."
  },
  "arbiters": ["resultTranslationArbiter"],
  "whyAccepted": "The security reviewer needs: exact file locations and line numbers, CWE references for tracking, ASVS control identifiers for compliance mapping, impact statements, and remediation direction. The accepted translation preserves all of this for a professional audience.",
  "whyRejected": "The rejected output strips all technical detail. A security engineer reading 'one involves authentication tokens' cannot open a ticket, assign severity, reference a standard, or know where to look. Translation for a technical audience must not reduce technical content.",
  "sourceAnchors": [
    {
      "sourceId": "OWASP-ASVS",
      "url": "https://github.com/OWASP/ASVS",
      "section": "Security finding report format — severity, CWE, ASVS control mapping",
      "license": "CC-BY-SA-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-SA-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-095

```json
{
  "rowId": "EXT-095",
  "sourceIds": ["DIATAXIS"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "H_GOVERNANCE",
  "subdomain": "audience-adaptation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Format this approved finalMachineResult for a product manager audience. The product manager needs to understand what was delivered and what the user experience impact is — not the technical implementation.",
    "b": "finalMachineResult = { summary: 'Your account has been created and you are now signed in.', detail: 'A welcome email is on its way to user@example.com.', userFacingData: { email: 'user@example.com' } }",
    "c": "Context: Node 9 (formatter). The formatter produces different outputs for different audiences from the same approved finalMachineResult. For a product manager: business language, user experience framing, no technical detail. For a developer: technical detail. For a security reviewer: security implications. The content does not change; the shape and emphasis do."
  },
  "function": "Format the approved finalMachineResult for a product manager. Use business language. Describe the user experience delivered, not the technical steps.",
  "acceptedOutput": {
    "formattedOutput": {
      "audience": "product_manager",
      "title": "New Account Registration Flow — Completed Successfully",
      "userExperienceSummary": "A new user completed registration and is immediately signed in. The onboarding email has been queued for delivery to the address they provided.",
      "deliveredValue": [
        "User can now access the platform without re-entering credentials.",
        "Onboarding communication is triggered automatically.",
        "No friction introduced — registration to signed-in state in a single flow."
      ],
      "metrics": {
        "registrationFlowCompleted": true,
        "immediateSignIn": true,
        "onboardingEmailTriggered": true
      }
    }
  },
  "rejectedOutput": {
    "formattedOutput": "SUCCESS. userId: usr_4829. sessionToken issued. tokenExpiry: 2026-05-04T14:30:00Z. welcomeEmail job_email_991 QUEUED."
  },
  "arbiters": ["formatterAudienceArbiter"],
  "whyAccepted": "The product manager output uses business language, describes user experience impact, and presents outcomes in terms a PM can act on (is the flow complete? is email triggered?). No technical identifiers, no status codes, no system internals.",
  "whyRejected": "The rejected output is a raw field dump — identical to what would be rejected at the result translation stage. The formatter's job is audience adaptation, not relay of internal state. A PM cannot interpret 'tokenExpiry: 2026-05-04T14:30:00Z' in a business context.",
  "sourceAnchors": [
    {
      "sourceId": "DIATAXIS",
      "url": "https://github.com/evildmp/diataxis-documentation-framework",
      "section": "Audience-oriented content — matching format to reader need",
      "license": "CC-BY-SA-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-SA-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-096

```json
{
  "rowId": "EXT-096",
  "sourceIds": ["NIST-SSDF"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "H_GOVERNANCE",
  "subdomain": "vulnerability-response",
  "rowClass": "migration-decision",
  "input": {
    "a": "Reprocessing decision: a dependency scanner found that the application uses log4j 2.14.1. A CVE has been published: CVE-2021-44228 (Log4Shell) affects log4j versions 2.0-beta9 through 2.14.1 with CVSS score 10.0 (Critical). The application is a production API serving external users.",
    "b": "History: version audit run on 2026-05-04. Finding: pom.xml declares log4j-core:2.14.1. NIST-SSDF RV.1.1 requires identifying vulnerabilities before they are exploited. RV.1.3 requires confirming exploitability. CVE-2021-44228 is exploitable via JNDI lookup in log messages — if user-controlled data is ever logged, the application is exploitable.",
    "c": "Context: NIST-SSDF practice RV (Respond to Vulnerabilities). RV.1.1: identify vulnerabilities, including in dependencies. RV.1.3: confirm exploitability in the specific application context before prioritizing. RV.2.1: plan and implement the response. The reprocessing decision must specify urgency and the minimum safe version."
  },
  "function": "Produce the reprocessing decision for CVE-2021-44228 in the specified application. Include exploitability confirmation requirement, urgency classification, and minimum safe version.",
  "acceptedOutput": {
    "reprocessingDecision": "IMMEDIATE ACTION REQUIRED.",
    "exploitabilityConfirmation": "Confirm whether any user-controlled input is passed to a log statement using log4j. If yes: exploitability is confirmed and the window for exploitation is the time until the patch is deployed. If no: risk is reduced but the vulnerable library is still present and logging behavior may change. Do not treat 'no user-controlled input currently logged' as 'not exploitable' without code verification.",
    "urgency": "CRITICAL — CVSS 10.0. Deploy the patch before the next business day regardless of exploitability confirmation result.",
    "response": [
      "Update log4j-core to 2.17.1 or later in pom.xml (2.17.1 addresses CVE-2021-44228 and subsequent CVEs CVE-2021-45046 and CVE-2021-45105).",
      "Do not update to 2.15.0 or 2.16.0 — these versions partially patch CVE-2021-44228 but are vulnerable to follow-on CVEs.",
      "Rebuild and redeploy the application.",
      "Verify the update: run mvn dependency:tree and confirm log4j-core version is 2.17.1 in the effective POM."
    ]
  },
  "rejectedOutput": {
    "reprocessingDecision": "Monitor the situation. If exploitability cannot be confirmed in the next sprint, deprioritize until the next planned dependency update cycle."
  },
  "arbiters": ["vulnerabilityResponseArbiter"],
  "whyAccepted": "NIST-SSDF RV.1.3 requires confirming exploitability but does not authorize deferring the patch pending confirmation. A CVSS 10.0 vulnerability in a production system must be patched immediately. Exploitability confirmation informs incident response scope, not patch urgency.",
  "whyRejected": "The rejected output defers a CVSS 10.0 patch to a sprint cycle, treating exploitability confirmation as a prerequisite for action. For critical-severity publicly known CVEs in production systems, confirmation is performed in parallel with patching, not before it.",
  "sourceAnchors": [
    {
      "sourceId": "NIST-SSDF",
      "url": "https://csrc.nist.gov/projects/ssdf",
      "section": "RV.1.1 and RV.1.3 — Identify and confirm vulnerability exploitability",
      "license": "Public domain"
    }
  ],
  "licenseConstraint": "none",
  "trainingStatus": "candidate"
}
```

---

## EXT-097

```json
{
  "rowId": "EXT-097",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "injection",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this result for input validation: a REST endpoint receives a JSON body with a page parameter for pagination. The endpoint validates only that page is not null and is a positive integer, then passes it directly to the database query as OFFSET page * 20.",
    "b": "page = request.json['page']; if page is None or page < 1: abort(400); query = 'SELECT * FROM products LIMIT 20 OFFSET ' + str(page * 20) — page is validated for sign but its upper bound is not constrained.",
    "c": "Context: CWE-20 Improper Input Validation — rank 6 in some CWE Top 25 variants. An unbounded page parameter allows a caller to request OFFSET 20 * 9999999 = 199,999,980, forcing the database to scan 200 million rows to satisfy the query. This is a denial-of-service vector via resource exhaustion, not an injection attack. The validation is incomplete — positive does not imply reasonable."
  },
  "function": "Apply inputValidationCompletenessArbiter: verify that numeric inputs used as database pagination parameters have both a lower bound and an upper bound check.",
  "acceptedOutput": {
    "decision": "REJECT on CWE-20. page is validated for positivity but has no upper bound. page=9999999 is accepted and produces OFFSET 199,999,980. On a table with 10,000 rows, the database scans all 10,000 rows and returns zero results — but on a table with 200 million rows, the query may run for minutes. This is a resource exhaustion denial-of-service vector requiring no authentication if the endpoint is public.",
    "reprocessInstruction": "Add an upper-bound check: MAX_PAGE = 1000; if page > MAX_PAGE: abort(400, 'Page number exceeds maximum'). Also consider using cursor-based pagination to eliminate offset scanning entirely."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The endpoint rejects null and negative values, which prevents the most obvious invalid inputs. Positive integers are semantically valid page numbers."
  },
  "arbiters": ["inputValidationCompletenessArbiter"],
  "whyAccepted": "CWE-20 explicitly covers incomplete validation — verifying one property (positivity) while leaving another (upper bound) unchecked. 'Semantically valid' must include reasonableness bounds for values used in resource-consuming operations.",
  "whyRejected": "The rejected output evaluates semantic validity in isolation from operational impact. A positive integer is semantically valid as a page number but operationally dangerous as an unbounded OFFSET multiplier.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/20.html",
      "section": "CWE-20 Extended Description — incomplete validation leaving attack surface",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-098

```json
{
  "rowId": "EXT-098",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "numeric-safety",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this C result: a function calculates a buffer size by multiplying a user-supplied count by the size of a struct, then passes the result to malloc. Both count and sizeof(Record) are unsigned 32-bit integers.",
    "b": "uint32_t count = get_record_count_from_input(); uint32_t size = count * sizeof(Record); char* buf = malloc(size); — if sizeof(Record) is 16 and count is 268435457 (2^28 + 1), the multiplication overflows uint32_t and produces 16 rather than 4,294,967,312.",
    "c": "Context: CWE-190 Integer Overflow or Wraparound — rank 11 in CWE Top 25 2023. Integer overflow in size calculation before malloc produces an allocation smaller than the intended size. Subsequent writes into the buffer overflow the undersized allocation, enabling heap exploitation."
  },
  "function": "Apply integerOverflowArbiter: detect multiplication of user-controlled values used as allocation sizes without overflow protection.",
  "acceptedOutput": {
    "decision": "REJECT. count * sizeof(Record) is computed in uint32_t. If count exceeds UINT32_MAX / sizeof(Record) (268435455 for sizeof(Record)=16), the multiplication overflows and produces a value smaller than the true product. malloc allocates a small buffer; subsequent record writes overflow the heap buffer. CWE-190 applies.",
    "reprocessInstruction": "Check for overflow before the multiplication: if (count > SIZE_MAX / sizeof(Record)) { return ERROR_SIZE_OVERFLOW; } size_t size = (size_t)count * sizeof(Record); Use size_t (native word size) not uint32_t for the size calculation to avoid truncation on 64-bit platforms."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. uint32_t is an unsigned type. Unsigned integer overflow is well-defined in C (modular wraparound) and does not produce undefined behavior."
  },
  "arbiters": ["integerOverflowArbiter"],
  "whyAccepted": "Well-defined modular wraparound is the mechanism of the exploit, not a mitigation. The overflow produces a valid (but wrong) small number that is passed to malloc. The resulting allocation is correct C — it allocates the small number of bytes. The bug is that the programmer intended a large allocation and got a small one.",
  "whyRejected": "The rejected output correctly notes that unsigned overflow is well-defined but draws the wrong conclusion. Well-defined behavior that produces the wrong allocation size is still a security vulnerability. CWE-190 applies to the incorrect size calculation, not to undefined behavior.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/190.html",
      "section": "CWE-190 Extended Description — allocation size overflow",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-099

```json
{
  "rowId": "EXT-099",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "file-upload",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this file upload result: an endpoint accepts user file uploads and validates the uploaded content by checking the file extension (.jpg, .png, .gif are allowed). The file is saved to the server's web root directory using the original filename provided by the client.",
    "b": "uploaded_filename = request.files['file'].filename; if not uploaded_filename.endswith(('.jpg', '.png', '.gif')): abort(400); save_path = os.path.join('/var/www/html/uploads/', uploaded_filename); request.files['file'].save(save_path)",
    "c": "Context: CWE-434 Unrestricted Upload of File with Dangerous Type — rank 12 in CWE Top 25 2023. Two attack vectors: (1) extension check bypassed with double extension (shell.php.jpg); (2) original filename used in path construction enables path traversal (../../index.php)."
  },
  "function": "Apply fileUploadArbiter: verify that file extension validation is suffix-resistant, that MIME type is checked from file content (not filename), and that saved filenames are server-generated, not client-supplied.",
  "acceptedOutput": {
    "decision": "REJECT. Two vulnerabilities. Vulnerability 1: extension check uses endswith which accepts shell.php.jpg — the file extension is .jpg but the file may contain PHP code. The web server may execute it as PHP if configured to process all files in the uploads directory. Vulnerability 2: the original client-supplied filename is used in the save path without sanitization. A filename of '../../etc/cron.d/backdoor' would save the file outside the uploads directory.",
    "reprocessInstruction": "Fix 1: Generate a UUID filename server-side. Never use the client-supplied filename in the filesystem path. Fix 2: Validate MIME type from file content using python-magic or equivalent: if magic.from_buffer(file_content, mime=True) not in ('image/jpeg', 'image/png', 'image/gif'): abort(400). Fix 3: Serve uploads from a directory that is not in the web root, preventing direct HTTP access. All three fixes are required."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The endpoint validates the file extension, which prevents non-image files from being uploaded. The allowed extensions are image-only."
  },
  "arbiters": ["fileUploadArbiter"],
  "whyAccepted": "Extension validation is bypassed by double extensions and does not verify file content. Path traversal via client-supplied filenames is a separate vulnerability. Both vectors are confirmed exploitable on common configurations. CWE-434 requires content-based validation, not name-based validation.",
  "whyRejected": "The rejected output evaluates only the extension allowlist. It does not evaluate the double-extension bypass, the MIME type content gap, or the path traversal via client filename. Incomplete validation is not acceptable validation for file upload endpoints.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/434.html",
      "section": "CWE-434 Extended Description — double extension bypass and path traversal",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-100

```json
{
  "rowId": "EXT-100",
  "sourceIds": ["KEEP-A-CHANGELOG"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "H_GOVERNANCE",
  "subdomain": "changelog-formatting",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Format these release changes as a Keep a Changelog 1.0.0 compliant CHANGELOG.md entry for version 2.4.0 released on 2026-05-04.",
    "b": "Changes in this release: (1) Added retry-with-backoff to the API client. (2) Fixed: null pointer crash in UserSessionService when the session expires during a concurrent request. (3) Deprecated: the v1 authentication endpoint /api/v1/auth — will be removed in v3.0.0. (4) Security: patched CWE-352 CSRF vulnerability on the account email change endpoint.",
    "c": "Context: Keep a Changelog 1.0.0 format. Sections in order: Added, Changed, Deprecated, Removed, Fixed, Security. Date format: YYYY-MM-DD. Version header links to diff. Each entry is a list item. Security section entries reference CVE or CWE where applicable."
  },
  "function": "Format the release changes as a Keep a Changelog 1.0.0 compliant CHANGELOG.md entry.",
  "acceptedOutput": {
    "changelogEntry": "## [2.4.0] - 2026-05-04\n\n### Added\n- Retry-with-backoff support in API client: configurable MaxRetries, BaseDelayMs, and jitter. See RetryPolicy documentation.\n\n### Deprecated\n- `POST /api/v1/auth` — use `POST /api/v2/auth` instead. Will be removed in v3.0.0.\n\n### Fixed\n- Null pointer crash in `UserSessionService` when a session expires during a concurrent request.\n\n### Security\n- Patched CSRF vulnerability (CWE-352) on `POST /api/account/email`. Added `SameSite=Strict` cookie attribute. Affected versions: 2.0.0–2.3.x.\n\n[2.4.0]: https://github.com/example/app/compare/v2.3.0...v2.4.0"
  },
  "rejectedOutput": {
    "changelogEntry": "Version 2.4.0 (May 4, 2026)\n\nNew features:\n- Added retry logic\n- Fixed a crash\n- Deprecated old auth endpoint\n- Security fix"
  },
  "arbiters": ["changelogFormatArbiter"],
  "whyAccepted": "The accepted entry uses Keep a Changelog section headers (Added, Deprecated, Fixed, Security), ISO 8601 date, backtick-quoted identifiers, the CWE reference in the Security entry, affected version range, and a compare link. All Keep a Changelog 1.0.0 conventions are present.",
  "whyRejected": "The rejected entry uses prose date format ('May 4, 2026' rather than ISO 8601), informal section headers ('New features'), omits the CWE reference, combines multiple concerns per entry, and has no compare link. Not parseable by changelog tooling.",
  "sourceAnchors": [
    {
      "sourceId": "KEEP-A-CHANGELOG",
      "url": "https://github.com/olivierlacan/keep-a-changelog",
      "section": "Keep a Changelog 1.0.0 specification — section order, date format, version links",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-101

```json
{
  "rowId": "EXT-101",
  "sourceIds": ["NODEJS-CORE"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "event-emitter-lifecycle",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this Node.js result: a class creates a new EventEmitter inside each HTTP request handler and registers a listener on the process 'exit' event to flush pending events before shutdown. The listener is never removed after the request completes.",
    "b": "app.get('/api/data', (req, res) => { const emitter = new EventEmitter(); process.on('exit', () => emitter.flush()); /* ... use emitter ... */ res.json(result); }); — each request registers a new 'exit' listener on process. After 10 requests, 10 listeners exist. Node.js warns when listener count exceeds 10 (MaxListenersExceededWarning).",
    "c": "Context: Node.js EventEmitter listener leak. process.on('exit', fn) accumulates listeners per request because they are never removed. Memory used by closed-over request state (emitter, pending events) is retained as long as the listener exists. In a long-running server this is a memory leak that grows with request count."
  },
  "function": "Apply eventEmitterLeakArbiter: detect listener registration inside a short-lived scope (request handler) on a long-lived emitter (process) without corresponding removal.",
  "acceptedOutput": {
    "decision": "REJECT. process.on('exit', handler) inside a request handler adds a listener to the process-level emitter on every request. The listener is never removed with process.removeListener() or process.off(). Each request grows the listener array. Node.js emits MaxListenersExceededWarning at 11 listeners and suppresses subsequent warnings. The listeners retain references to request-scoped emitter instances, preventing GC.",
    "reprocessInstruction": "Either: (1) register the exit handler once at server startup outside the request handler, with a reference to a shared flush queue that all emitters contribute to; or (2) register and remove per-request: const exitHandler = () => emitter.flush(); process.on('exit', exitHandler); ... res.json(result); process.removeListener('exit', exitHandler); — remove must happen whether the request succeeds or fails."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The process 'exit' event fires only once before process termination. The listeners will only execute once and then the process ends, so accumulated listeners cause no functional problem."
  },
  "arbiters": ["eventEmitterLeakArbiter"],
  "whyAccepted": "The exit event fires once, so the functions execute once — that is correct. But the leak is the accumulation of listener references over the server's lifetime, not the exit behavior. Each listener retains a closure over request state, preventing garbage collection of those objects while the server runs.",
  "whyRejected": "The rejected output evaluates exit-time behavior (correct — handlers fire once on exit) but ignores runtime behavior (incorrect — listeners accumulate in memory throughout the server's lifetime). The problem is memory pressure over time, not exit-handler correctness.",
  "sourceAnchors": [
    {
      "sourceId": "NODEJS-CORE",
      "url": "https://github.com/nodejs/node",
      "section": "EventEmitter documentation — listener lifecycle and MaxListenersExceededWarning",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-102

```json
{
  "rowId": "EXT-102",
  "sourceIds": ["JASON-TAYLOR-CLEANARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "clean-architecture",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this Clean Architecture result for a .NET application: a CQRS query handler directly instantiates an HttpClient to call an external weather API and returns the result to the controller.",
    "b": "public class GetWeatherQueryHandler : IRequestHandler<GetWeatherQuery, WeatherDto> { public async Task<WeatherDto> Handle(GetWeatherQuery request, CancellationToken ct) { using var client = new HttpClient(); var response = await client.GetAsync('https://api.weather.com/v1/forecast?city=' + request.City); return JsonSerializer.Deserialize<WeatherDto>(await response.Content.ReadAsStringAsync()); } }",
    "c": "Context: Clean Architecture — query handlers should have no knowledge of infrastructure concerns. An IRequestHandler contains domain or application logic. HttpClient is an infrastructure concern and must be injected via an interface (IWeatherService or IWeatherApiClient) defined in the application layer, with the HttpClient-backed implementation in the infrastructure layer."
  },
  "function": "Apply cleanArchitectureArbiter: detect infrastructure dependencies (HttpClient, database connections, external SDK clients) instantiated directly inside application-layer handlers.",
  "acceptedOutput": {
    "decision": "REJECT. Two violations. Violation 1: HttpClient is instantiated directly with 'using var client = new HttpClient()'. This is the known socket exhaustion anti-pattern — short-lived HttpClient instances do not release TCP connections promptly, exhausting the available socket pool under load. Violation 2: The query handler has a direct dependency on an external HTTP API, making it untestable without network access and coupled to the infrastructure layer. Both violate Clean Architecture's dependency rule (application layer must not depend on infrastructure).",
    "reprocessInstruction": "Define IWeatherService in the Application layer: Task<WeatherDto> GetForecastAsync(string city). Implement WeatherApiClient : IWeatherService in the Infrastructure layer using IHttpClientFactory injection. Inject IWeatherService into the handler constructor. Register WeatherApiClient with services.AddHttpClient<IWeatherService, WeatherApiClient>()."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The handler uses using var to dispose the HttpClient correctly after use, preventing resource leaks."
  },
  "arbiters": ["cleanArchitectureArbiter"],
  "whyAccepted": "using var disposes the HttpClient object but does not close the underlying TCP connection — socket TIME_WAIT state persists for 240 seconds regardless of disposal. More critically, the architectural violation (infrastructure dependency in application layer) is not addressed by disposal correctness.",
  "whyRejected": "Correct disposal of an incorrectly used infrastructure dependency does not make the usage correct. The rejected output evaluates only resource disposal, not architectural placement.",
  "sourceAnchors": [
    {
      "sourceId": "JASON-TAYLOR-CLEANARCH",
      "url": "https://github.com/jasontaylordev/CleanArchitecture",
      "section": "Clean Architecture layers — dependency rule and infrastructure isolation",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-103

```json
{
  "rowId": "EXT-103",
  "sourceIds": ["MCP-SERVERS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "B_ROUTING_SELECTION",
  "subdomain": "mcp-server-tool-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Design the tool interface for an MCP server that gives an LLM agent access to a customer database. The agent must be able to: look up a customer by email, update the customer's subscription tier, and list all customers on a given tier.",
    "b": "The database is PostgreSQL. The agent will run in a customer support context where a human agent is supervising but not always reviewing each query. The tools must be safe for LLM use — they must not allow arbitrary SQL.",
    "c": "Context: MCP server tool design. Each tool should have one purpose, clear input schema with constraints, and should not expose raw database access. Write operations (subscription update) require explicit parameter constraints to prevent accidental bulk updates."
  },
  "function": "Define the three MCP tool schemas as JSON. Each tool must have: name, description, input schema with required fields and types, and any constraints that prevent misuse.",
  "acceptedOutput": {
    "tools": [
      {
        "name": "lookup_customer_by_email",
        "description": "Find a customer record by their exact email address. Returns customer ID, name, email, and current subscription tier.",
        "inputSchema": {
          "type": "object",
          "required": ["email"],
          "properties": {
            "email": { "type": "string", "format": "email", "description": "Exact email address to look up" }
          }
        }
      },
      {
        "name": "update_customer_subscription",
        "description": "Update the subscription tier for one specific customer. Requires the customer ID from lookup_customer_by_email. Cannot update multiple customers in one call.",
        "inputSchema": {
          "type": "object",
          "required": ["customer_id", "new_tier"],
          "properties": {
            "customer_id": { "type": "string", "pattern": "^cust_[a-zA-Z0-9]+$", "description": "Customer ID from lookup result" },
            "new_tier": { "type": "string", "enum": ["free", "starter", "pro", "enterprise"], "description": "Target subscription tier" }
          }
        }
      },
      {
        "name": "list_customers_by_tier",
        "description": "List customers on a specific tier. Returns up to 50 results. Use customer_id from results for individual lookups.",
        "inputSchema": {
          "type": "object",
          "required": ["tier"],
          "properties": {
            "tier": { "type": "string", "enum": ["free", "starter", "pro", "enterprise"] },
            "limit": { "type": "integer", "minimum": 1, "maximum": 50, "default": 20 }
          }
        }
      }
    ]
  },
  "rejectedOutput": {
    "tools": [
      {
        "name": "query_database",
        "description": "Execute a SQL query against the customer database.",
        "inputSchema": {
          "type": "object",
          "required": ["sql"],
          "properties": {
            "sql": { "type": "string", "description": "SQL query to execute" }
          }
        }
      }
    ]
  },
  "arbiters": ["mcpToolDesignArbiter"],
  "whyAccepted": "The accepted tools expose bounded operations with explicit input constraints: email format validation, customer_id pattern matching, tier enum constraints, result limit cap. An LLM agent using these tools cannot execute arbitrary SQL, cannot update multiple customers accidentally, and cannot return unbounded result sets.",
  "whyRejected": "A raw SQL tool gives the LLM unrestricted database access. In a supervised customer support context this is unacceptable — the LLM could generate a DELETE statement, a bulk UPDATE without WHERE clause, or a data exfiltration query. MCP tools must encode the safety constraints of the operation, not delegate them to the LLM.",
  "sourceAnchors": [
    {
      "sourceId": "MCP-SERVERS",
      "url": "https://github.com/modelcontextprotocol/servers",
      "section": "Tool design guidelines — bounded operations and input schema constraints",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-104

```json
{
  "rowId": "EXT-104",
  "sourceIds": ["LANGCHAIN-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "chain-composition",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "I need a pipeline that: (1) classifies a support ticket into one of three categories (billing, technical, account), (2) generates a draft reply tailored to the category, and (3) checks the draft for policy compliance before sending. Each step must run in sequence — the draft cannot be generated before the category is known, and the compliance check cannot run before the draft exists.",
    "b": "Available LangChain components: ChatOpenAI (for LLM calls), PromptTemplate, StrOutputParser, RunnableSequence, RunnableParallel. The pipeline must be composable and each step must be independently testable.",
    "c": "Context: LangChain LCEL (LangChain Expression Language) chain composition. Sequential dependencies require RunnableSequence (|). Parallel independent steps can use RunnableParallel. The three steps described are strictly sequential — each step's output is the next step's input."
  },
  "function": "Produce the LangChain LCEL chain definition that implements the three-step sequential pipeline. Show the chain composition and the input/output types at each step.",
  "acceptedOutput": {
    "chain": "from langchain_core.prompts import ChatPromptTemplate\nfrom langchain_core.output_parsers import StrOutputParser\nfrom langchain_openai import ChatOpenAI\n\nllm = ChatOpenAI(model='gpt-4o-mini')\nparser = StrOutputParser()\n\nclassify_prompt = ChatPromptTemplate.from_template(\n    'Classify this support ticket into exactly one category: billing, technical, or account.\\nTicket: {ticket}\\nCategory:'\n)\n\ndraft_prompt = ChatPromptTemplate.from_template(\n    'Write a helpful draft reply for a {category} support issue.\\nTicket: {ticket}\\nDraft:'\n)\n\ncompliance_prompt = ChatPromptTemplate.from_template(\n    'Check this draft reply for policy compliance. Return APPROVED or REJECTED with reason.\\nDraft: {draft}\\nVerdict:'\n)\n\n# Step 1: Classify. Step 2: Draft (receives ticket + category). Step 3: Compliance check.\nchain = (\n    {\"category\": classify_prompt | llm | parser, \"ticket\": lambda x: x[\"ticket\"]}\n    | draft_prompt | llm | parser\n    | (lambda draft: {\"draft\": draft})\n    | compliance_prompt | llm | parser\n)"
  },
  "rejectedOutput": {
    "chain": "RunnableParallel(classify=classify_chain, draft=draft_chain, compliance=compliance_chain)"
  },
  "arbiters": ["chainCompositionArbiter"],
  "whyAccepted": "The accepted chain uses sequential composition (| operator) where each step's output feeds the next. The category from step 1 flows into step 2's prompt alongside the original ticket. The draft from step 2 flows into step 3's compliance prompt. Dependencies are explicit.",
  "whyRejected": "RunnableParallel runs all three chains simultaneously. The draft chain runs before the category is known (it has no input from the classify chain), and the compliance chain runs before the draft exists. The parallel composition ignores the stated sequential dependency requirement.",
  "sourceAnchors": [
    {
      "sourceId": "LANGCHAIN-DOCS",
      "url": "https://github.com/langchain-ai/docs",
      "section": "LCEL — RunnableSequence vs RunnableParallel composition",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-105

```json
{
  "rowId": "EXT-105",
  "sourceIds": ["NIST-NVD"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "vulnerability-prioritization",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Arbitrate this vulnerability prioritization result: a security team received scan output showing a CVSS 9.8 (Critical) vulnerability in a third-party library. The team deprioritized it because: 'the library is used only for XML formatting of internal report files that are never sent over the network.'",
    "b": "CVE-XXXX-1234: CVSS 9.8 Critical. Attack vector: Network. The vulnerable library function parses externally-supplied XML. The application uses the library to format internal audit reports from a database query result — the XML input is built by the application, not received from users.",
    "c": "Context: NIST NVD CVSS scoring and exploitability assessment. CVSS base score is context-free — it represents the worst-case severity assuming the vulnerable feature is exposed as described in the CVE. The team's exploitability assessment (internally-generated XML, no network exposure) may reduce the effective risk. NIST-SSDF RV.1.3 requires confirming exploitability before reprioritizing."
  },
  "function": "Apply vulnerabilityPrioritizationArbiter: verify that the deprioritization is based on a confirmed exploitability assessment with documented evidence, not an assumption.",
  "acceptedOutput": {
    "decision": "ACCEPT the deprioritization — conditionally. The team's assessment (XML is application-generated, not user-supplied) reduces the attack vector from 'Network' to 'None' in the application's specific context, which would reduce the effective CVSS from 9.8 to approximately 0. However, the decision must be documented with two additional confirmations: (1) code review evidence that the library is called only with application-generated XML strings, and (2) a search for any code path that could reach the library function with user-controlled input (even indirect paths through log parsing, configuration files, or third-party API responses). If both confirmations pass, the deprioritization is justified. Review again if the library's usage scope changes.",
    "documentationRequired": "Create a risk acceptance record: CVE, assessment date, code paths reviewed, confirming engineer, re-review trigger condition."
  },
  "rejectedOutput": {
    "decision": "REJECT the deprioritization. CVSS 9.8 is Critical. All Critical-severity CVEs must be patched regardless of exploitability context."
  },
  "arbiters": ["vulnerabilityPrioritizationArbiter"],
  "whyAccepted": "NIST-SSDF RV.1.3 specifically requires confirming exploitability before prioritizing response. Context-based deprioritization with documented evidence is exactly what RV.1.3 enables. The accepted output validates the deprioritization while requiring the evidence to be documented.",
  "whyRejected": "Blanket patch-all-Critical policies without exploitability assessment produce alert fatigue and waste remediation capacity on zero-risk findings. NIST-SSDF explicitly calls for exploitability confirmation as a step before response planning. A policy that ignores RV.1.3 is itself a governance gap.",
  "sourceAnchors": [
    {
      "sourceId": "NIST-NVD",
      "url": "https://nvd.nist.gov/vuln/data-feeds",
      "section": "CVSS contextual scoring and NIST-SSDF RV.1.3 exploitability confirmation",
      "license": "Public domain"
    }
  ],
  "licenseConstraint": "none",
  "trainingStatus": "candidate"
}
```

---

## EXT-106

```json
{
  "rowId": "EXT-106",
  "sourceIds": ["KUBERNETES-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "A_TOPOLOGY",
  "subdomain": "resource-limits",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Decompose this deployment command: deploy the payment service with 3 replicas to production. The payment service team has not specified CPU or memory requests/limits.",
    "b": "kubectl apply -f payment-service-deployment.yaml — the deployment manifest has no resources section in the container spec.",
    "c": "Context: Kubernetes resource requests and limits — CC-BY 4.0. A deployment with no resource requests is scheduled by Kubernetes as a BestEffort QoS class. Under node memory pressure, BestEffort pods are the first to be evicted. A payment service evicted under memory pressure causes transaction failures. The deployment must be blocked pending resource specification."
  },
  "function": "Decompose the deployment command into action items. Identify the missing resource specification and produce the action items that must complete before the deploy proceeds.",
  "acceptedOutput": {
    "actionItems": [
      "BLOCK: Do not deploy until resources are specified. The payment-service deployment has no CPU/memory requests or limits. Without requests: (a) the pod is BestEffort QoS and will be the first evicted under memory pressure; (b) the scheduler cannot make informed placement decisions.",
      "Action 1: Request the payment service team to profile the service under load. Minimum data required: p99 CPU usage at peak load, p99 memory at peak load, startup CPU/memory spike.",
      "Action 2: Set resource requests at p99 peak + 20% headroom. Set limits at 2x the requests for CPU (burstable) and 1.5x for memory (hard limit to prevent OOM kill from cascading).",
      "Action 3: After the team provides measurements, add the resources section to the manifest and re-run kubectl apply.",
      "Action 4: After deployment, monitor with kubectl top pods -n production to verify actual usage is below the requests."
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Run kubectl apply -f payment-service-deployment.yaml",
      "Monitor the pod with kubectl get pods -w",
      "Add resource limits later in a follow-up PR."
    ]
  },
  "arbiters": ["deploymentReadinessArbiter"],
  "whyAccepted": "Resource specification is a prerequisite for safe production deployment of a payment service. 'Add later' is not a valid deployment plan for a service whose eviction causes financial transaction failures.",
  "whyRejected": "The rejected output deploys first and monitors, deferring the resource specification. BestEffort QoS pods are evicted before the operator can observe and respond. The 'add later' plan will be executed after the first production incident.",
  "sourceAnchors": [
    {
      "sourceId": "KUBERNETES-DOCS",
      "url": "https://github.com/kubernetes/website",
      "section": "Resource management — QoS classes and BestEffort eviction priority",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-107

```json
{
  "rowId": "EXT-107",
  "sourceIds": ["ASPNETCORE-SAMPLES"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "middleware-pipeline",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose this ASP.NET Core middleware configuration requirement: the API must authenticate requests before routing, rate-limit authenticated users before reaching controllers, and log all request/response pairs including the user identity.",
    "b": "Current Startup.Configure: app.UseRouting(); app.UseAuthentication(); app.UseAuthorization(); app.UseEndpoints(...) — rate limiting and request/response logging are not yet added.",
    "c": "Context: ASP.NET Core middleware pipeline order is significant. Authentication must run before authorization. Rate limiting should run after authentication (so it can rate-limit by identity, not by IP). Request logging must wrap the entire pipeline to capture both authenticated identity and response status."
  },
  "function": "Produce the action items: the correct middleware registration order and the reason each position is required.",
  "acceptedOutput": {
    "actionItems": [
      "Add middleware in this exact order:",
      "1. app.UseRequestLogging() — must be first to capture the entire request lifecycle including all subsequent middleware timing. Implement as a custom middleware that starts a stopwatch, captures the incoming request, calls await next(context), then logs response status and elapsed time. Captures identity from HttpContext.User after authentication runs (because logging the response happens after auth).",
      "2. app.UseRouting() — must precede authentication so the route is known before auth decisions are made.",
      "3. app.UseAuthentication() — populates HttpContext.User. Must run before UseAuthorization and before rate limiter (which will key on user identity).",
      "4. app.UseAuthorization() — checks permissions after identity is established.",
      "5. app.UseRateLimiter() — runs after authentication so rate limit policies can reference HttpContext.User.Identity.Name. A rate limiter before authentication can only key on IP address.",
      "6. app.UseEndpoints(endpoints => { endpoints.MapControllers().RequireAuthorization(); })"
    ]
  },
  "rejectedOutput": {
    "actionItems": [
      "Add app.UseRateLimiter() before app.UseRouting()",
      "Add app.UseRequestLogging() at the end of the pipeline after endpoints"
    ]
  },
  "arbiters": ["middlewareOrderArbiter"],
  "whyAccepted": "The accepted order satisfies all three requirements: logging wraps the full pipeline (captures identity and response), authentication precedes rate limiting (enables identity-based limits), and routing precedes authentication (enables route-based auth policies).",
  "whyRejected": "The rejected order places rate limiting before routing and authentication — the rate limiter cannot key on user identity because authentication has not run. Logging after endpoints misses the response time of all preceding middleware and cannot capture the authenticated identity because HttpContext.User is not yet populated when the request arrives.",
  "sourceAnchors": [
    {
      "sourceId": "ASPNETCORE-SAMPLES",
      "url": "https://github.com/dotnet/aspnetcore",
      "section": "Middleware pipeline ordering — authentication, authorization, rate limiting",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-108

```json
{
  "rowId": "EXT-108",
  "sourceIds": ["REACT-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "react-state-management",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this React state management result: a component derives 'filteredItems' by calling items.filter() directly in the render function body, outside of any useMemo hook. The items array is a prop with 5,000 elements and the filter is a string comparison against all elements.",
    "b": "function ProductList({ items, searchTerm }) { const filteredItems = items.filter(item => item.name.toLowerCase().includes(searchTerm.toLowerCase())); return <ul>{filteredItems.map(...)}</ul>; } — the filter runs on every render of ProductList, including renders triggered by unrelated parent state changes.",
    "c": "Context: React docs (react.dev) — useMemo for expensive computations. Filtering 5,000 items on every render including unrelated parent re-renders is a performance problem. The correct pattern is useMemo with [items, searchTerm] as dependencies, which caches the filtered result and recomputes only when items or searchTerm changes."
  },
  "function": "Apply reactComputedValueArbiter: detect expensive computations in render body that should be memoized, with cost proportional to input size.",
  "acceptedOutput": {
    "decision": "REJECT. items.filter() over 5,000 elements runs on every render cycle, including renders caused by parent component state changes unrelated to items or searchTerm. This blocks the main thread for each re-render. The computation's result depends only on items and searchTerm — it should be cached until either changes.",
    "reprocessInstruction": "Wrap in useMemo: const filteredItems = useMemo(() => items.filter(item => item.name.toLowerCase().includes(searchTerm.toLowerCase())), [items, searchTerm]); The filter now runs only when items or searchTerm changes, not on unrelated parent re-renders."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. React's virtual DOM diffing will prevent unnecessary DOM updates. Even if the filter runs on every render, React will only update DOM nodes that actually changed."
  },
  "arbiters": ["reactComputedValueArbiter"],
  "whyAccepted": "Virtual DOM diffing prevents DOM updates, not JavaScript execution. The filter over 5,000 items runs synchronously on the main thread before diffing begins. For large arrays, this can exceed 16ms per frame, causing visible jank. Diffing optimization and computation optimization are independent.",
  "whyRejected": "The rejected output conflates DOM update prevention (what virtual DOM diffing does) with computation prevention (what memoization does). They address different costs at different points in the rendering pipeline.",
  "sourceAnchors": [
    {
      "sourceId": "REACT-DOCS",
      "url": "https://github.com/reactjs/react.dev",
      "section": "useMemo — when to use for expensive computations",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-109

```json
{
  "rowId": "EXT-109",
  "sourceIds": ["GOOD-DOCS-TEMPLATES"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "H_GOVERNANCE",
  "subdomain": "how-to-guide-formatting",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Format this approved result as a how-to guide for a developer who needs to add retry-with-backoff to their existing API client. The guide must be actionable — the reader should be able to complete the task without referring to any other document.",
    "b": "Approved result: RetryPolicy class documentation plus the implementation from EXT-092 (retry_with_backoff function with max_retries, base_delay, exponential backoff, KeyboardInterrupt passthrough).",
    "c": "Context: Diátaxis how-to guide format (task-oriented). A how-to guide starts from where the reader is now, tells them exactly what to do, and ends at a specific accomplished state. It does not explain why the design is the way it is (that is explanation documentation). It does not teach from scratch (that is a tutorial)."
  },
  "function": "Format the retry implementation as a how-to guide. Include: prerequisite state, numbered steps, a working code example, and a verification step.",
  "acceptedOutput": {
    "howToGuide": "## How to add retry-with-backoff to your API client\n\n**Before you start:** You have a Python function that makes an API call and may raise an exception on failure.\n\n### Steps\n\n1. Copy this function into your project:\n\n```python\nimport time\nfrom typing import Callable, Any\n\ndef retry_with_backoff(func: Callable, max_retries: int = 3, base_delay: float = 0.5) -> Any:\n    last_exc = None\n    for attempt in range(1, max_retries + 1):\n        try:\n            return func()\n        except (KeyboardInterrupt, SystemExit):\n            raise\n        except Exception as e:\n            last_exc = e\n            if attempt < max_retries:\n                time.sleep(base_delay * (2 ** (attempt - 1)))\n    raise last_exc\n```\n\n2. Wrap your existing API call:\n\n```python\n# Before\nresult = my_api_client.get_user(user_id)\n\n# After\nresult = retry_with_backoff(lambda: my_api_client.get_user(user_id))\n```\n\n3. Verify it works:\n\n```python\ncall_count = 0\ndef flaky(): global call_count; call_count += 1; raise ValueError if call_count < 3 else return 'ok'\nassert retry_with_backoff(flaky, max_retries=3) == 'ok'\nassert call_count == 3\n```\n\n**Result:** Your API call now retries up to 3 times with exponential backoff (0.5s, 1.0s) before raising."
  },
  "rejectedOutput": {
    "howToGuide": "## Retry-with-backoff\n\nRetry-with-backoff is a resilience pattern that retries failed operations with increasing delays. Exponential backoff prevents thundering herd by spreading retry attempts over time. The jitter variant adds randomness to further distribute load. This implementation uses the Python standard library...\n\n[continues with explanation of the pattern for 500 words]"
  },
  "arbiters": ["documentationFormatArbiter"],
  "whyAccepted": "The accepted guide starts from the reader's current state (have a function that may fail), provides numbered steps with working code, and ends at a verified accomplished state. It contains no explanation of why exponential backoff works — that belongs in explanation documentation.",
  "whyRejected": "The rejected output is explanation documentation — it describes the pattern, its properties, and its rationale. A developer who needs to add retry now does not need to understand thundering herd theory. Wrong documentation type for a how-to request.",
  "sourceAnchors": [
    {
      "sourceId": "GOOD-DOCS-TEMPLATES",
      "url": "https://gitlab.com/tgdp/templates",
      "section": "How-to guide template — prerequisite state, numbered steps, verification",
      "license": "MIT-0"
    }
  ],
  "licenseConstraint": "MIT-0",
  "trainingStatus": "candidate"
}
```

---

## EXT-110

```json
{
  "rowId": "EXT-110",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "race-condition",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Arbitrate this result: a file upload handler checks whether the uploaded file is a valid image by reading the first bytes to verify the magic number, then passes the confirmed file path to an image processing library. Between the check and the processing call, the file path is stored in a temporary variable.",
    "b": "def handle_upload(filepath): if not is_valid_image(filepath): raise ValidationError(); process_image(filepath) — the file at filepath could be replaced between the is_valid_image check and the process_image call on systems where the temp directory is world-writable.",
    "c": "Context: CWE-367 Time-of-check Time-of-use (TOCTOU) race condition. On POSIX systems with a world-writable temp directory, an attacker process can replace the file between the check and the use. The validation passes on the real image; processing executes on the attacker's replacement file. This is the classic TOCTOU pattern."
  },
  "function": "Apply toctouArbiter: detect time-of-check to time-of-use race conditions in file validation workflows. Produce the correct atomic pattern.",
  "acceptedOutput": {
    "decision": "REJECT. TOCTOU race condition between is_valid_image(filepath) and process_image(filepath). If the upload temp directory is world-writable (default on many POSIX systems), an attacker can replace the file in the window between the two calls. The validation result applies to the file at the time of checking; process_image acts on the file at the time of use — these can be different files.",
    "reprocessInstruction": "Open the file once, hold an exclusive file descriptor, validate from the file descriptor, and pass the file descriptor to processing — do not close and reopen. In Python: with open(filepath, 'rb') as f: validate_from_fd(f); f.seek(0); process_from_fd(f). Alternatively, move the validated file to a non-world-writable directory using os.rename() atomically after validation, then process from the new path."
  },
  "rejectedOutput": {
    "decision": "ACCEPT. The file is validated before processing and the validation failure raises an exception that prevents invalid files from reaching process_image."
  },
  "arbiters": ["toctouArbiter"],
  "whyAccepted": "The validation and the use are not atomic. Between two separate file operations on the same path, the file at that path can change. The exception correctly blocks invalid files at the time of checking, but does not block files that become invalid (or malicious) after checking.",
  "whyRejected": "The rejected output evaluates the validation logic (correct — bad files raise exceptions) without evaluating the time window between check and use. TOCTOU is a temporal vulnerability; evaluating only the validation logic misses the attack window.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/367.html",
      "section": "CWE-367 TOCTOU — file path race condition in validation workflows",
      "license": "MITRE CWE Terms of Use",
      "requiredNotice": "CWE™ is a trademark of The MITRE Corporation"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```
