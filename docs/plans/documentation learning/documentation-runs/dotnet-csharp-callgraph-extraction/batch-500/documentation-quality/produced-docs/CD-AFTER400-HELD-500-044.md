# Documentation: HttpAuthenticationChallengeContextExtensions.ChallengeWith

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\Filters\HttpAuthenticationChallengeContextExtensions.cs
- Symbol: HttpAuthenticationChallengeContextExtensions.ChallengeWith
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ if (context == null) { throw new ArgumentNullException(nameof(context)); } context.Result = new AddChallengeOnUnauthorizedResult(challenge, context.Result); }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
