# Documentation: askapediaweb::AuthenticationFilter.AuthenticateAsync

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\Core\AuthenticationFilter.cs
- Symbol: askapediaweb::AuthenticationFilter.AuthenticateAsync
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ HttpRequestMessage request = context.Request; AuthenticationHeaderValue authorization = request.Headers.Authorization; if (authorization == null) return Task.CompletedTask; if (authorization.Scheme != "Bearer") return Task.CompletedTask; if (String.IsNullOrEmpty(authorization.Parameter)) { context.ErrorResult = new AuthenticationFailureResult("Missing token", request); return Task.CompletedTask; } var token = authorization.Parameter; var user = User.GetUserByToken(token); if (user == null) { context.ErrorResult = new AuthenticationFailureResult("Invalid token", request); return Task.CompletedTask; } if (user.TokenDateExpire < DateTime.Now) { context.ErrorResult = new AuthenticationFailureResult("Token expired", request); return Task.CompletedTask; } string role = user.IsAdmin == 1 ? "admin" : "user"; var identity = new GenericIdentity(user.Email ?? "Anonymous Identity", "Bearer"); iden

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
