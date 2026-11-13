# Documentation: JwtAuthenticationAttribute.AuthenticateAsync

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\Filters\JwtAuthenticationAttribute.cs
- Symbol: JwtAuthenticationAttribute.AuthenticateAsync
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var request = context.Request; var authorization = request.Headers.Authorization; if (authorization == null || authorization.Scheme != "Bearer") return ; if (string.IsNullOrEmpty(authorization.Parameter)) { context.ErrorResult = new AuthenticationFailureResult("Missing Jwt Token", request); return ; } var token = authorization.Parameter; var principal = await AuthenticateJwtToken(token); if (principal == null) context.ErrorResult = new AuthenticationFailureResult("Invalid token", request); else context.Principal = principal; }

## Delegated Callee Behavior
- JwtAuthenticationAttribute.AuthenticateJwtToken: Accepted callee meaning for JwtAuthenticationAttribute.AuthenticateJwtToken: { UserBack userT; if (ValidateToken(token, out userT)) { // based on username to get more information from database in order to build local identity var claims = new List<Claim> { 

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
