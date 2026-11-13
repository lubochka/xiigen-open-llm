# Documentation: askapediaweb::UsersController.FacebookLogin

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\Controllers\UsersController.cs
- Symbol: askapediaweb::UsersController.FacebookLogin
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var fdetailsI = JsonConvert.DeserializeObject<FacebookDetails>(fdetails); var loginResult = BL.User.FacebookLogin(fdetailsI.email,fdetailsI.fullName, fdetailsI.accessToken); return loginResult == null ? new ActionResult<LoginResponse>(ERROR, null) : new ActionResult<LoginResponse>(OK, loginResult); }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
