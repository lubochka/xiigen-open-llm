# Documentation: JwtAuthenticationAttribute.AuthenticateJwtToken

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\Filters\JwtAuthenticationAttribute.cs
- Symbol: JwtAuthenticationAttribute.AuthenticateJwtToken
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ UserBack userT; if (ValidateToken(token, out userT)) { // based on username to get more information from database in order to build local identity var claims = new List<Claim> { new Claim(ClaimTypes.Name, userT.Name), new Claim("userNum", userT.Id.ToString()), new Claim("Filial", userT.Filial), new Claim("isFilialManager", userT.IsFilialManager.ToString()), new Claim("IsClientManager", userT.IsClientManager.ToString()), new Claim("Client", userT.Client), new Claim("IsSystemManager", userT.IsSystemManager.ToString()), new Claim("MainUserId", userT.MainUserId.ToString()), new Claim("CompanyName", userT.CompanyName), new Claim("ManageHimself", userT.ManageHimself.ToString()) }; var identity = new ClaimsIdentity(claims, "Jwt"); IPrincipal user = new ClaimsPrincipal(identity); return System.Threading.Tasks.Task.FromResult(user); } return System.Threading.Tasks.Task.FromResult<IPrincipal>(nu

## Delegated Callee Behavior
- JwtAuthenticationAttribute.ValidateToken: Accepted callee meaning for JwtAuthenticationAttribute.ValidateToken: { uset = new UserBack(); var simplePrinciple = JwtManager.GetPrincipal(token); var identity = simplePrinciple.Identity as ClaimsIdentity; if (identity == null) return false; if (!i

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
