# Code Documentation DPO Registry Guide
# Version: 1.0 - 2026-05-11
# Domain: Legacy .NET documentation DPO

---

## Required Pair Shape

Every pair must use this shape:

```text
Pair[ID]
A:
  code:
  call_context:
  known_callee_summaries:
  requested_documentation_level:
  app_side:
  runtime_family:
  target_framework:
  csharp_language_version:
  language_version_confidence:
  project_style:
  web_framework:
  web_framework_version:
  frontend_framework:
  frontend_framework_version:
  stack_label:
B:
  chosen:
  rejected:
Reason:
  why_chosen_is_correct:
  why_rejected_is_wrong:
Phrasing-B:
  alternate_correct_documentation:
Coverage:
  source_id:
  license_state:
  app_side:
  runtime_family:
  target_framework:
  csharp_language_version:
  language_version_confidence:
  project_style:
  web_framework:
  web_framework_version:
  frontend_framework:
  frontend_framework_version:
  stack_label:
  artifact_kind:
  documentation_level:
  rejected_type:
  overlap_ratio:
```

---

## Pair Types

### CD-A - Local Method Purpose

Chosen documents only behavior visible in the method.

Rejected invents business purpose, hidden caller context, or a feature name not
present in evidence.

### CD-B - Inputs, Outputs, And Error Paths

Chosen documents parameters, return values, `DataProcessResult`, null handling,
exceptions, or legacy error handling.

Rejected drops a failure branch or claims exceptions are thrown when the code
returns an error object.

### CD-C - Side Effects And Data Access

Chosen names writes, database calls, session state, cache writes, events, file
I/O, or external calls.

Rejected describes the method as pure/read-only or omits the side effect.

### CD-D - Framework Lifecycle

Chosen respects WebForms, MVC, Web API, WPF, WinForms, or ASP.NET Core
lifecycle.

Rejected applies the wrong lifecycle, for example treating `Page_Load` like an
MVC action.

### CD-E - Callee Propagation

Chosen uses accepted callee summaries and separates local behavior from
delegated behavior.

Rejected ignores callee summaries or attributes callee behavior to the wrong
method.

### CD-F - Component / Workflow Summary

Chosen documents class, controller, page, or module behavior from accumulated
evidence.

Rejected over-modernizes the code, invents missing services, or writes a generic
architecture summary.

### CD-G - Version-Aware Stack Routing

Chosen preserves server/client side, runtime family, target framework, inferred
C# language version, project style, and frontend framework version.

Rejected documents legacy code using the wrong platform vocabulary, for example
treating .NET Framework 4.6 WebApi2 plus AngularJS 1.4 as ASP.NET Core or
modern Angular.

### CD-H - Bottom-Up Call Transit

Chosen starts with inner methods, carries accepted callee summaries upward, and
separates local caller behavior from delegated callee behavior.

Rejected documents the caller directly from the entry point, invents callee
behavior, or drops the callee summary that the caller depends on.

---

## Rejected Annotation Rule

Every rejected side must contain:

```text
[WRONG: <specific weakness> - <what cannot be learned> - <functional consequence>.]
```

Example:

```text
[WRONG: missed side effect - the rejected documentation says the method only
validates input, but the code writes to session state and updates the repository.
Readers would deploy or refactor it as a pure validator and break persistence.]
```
