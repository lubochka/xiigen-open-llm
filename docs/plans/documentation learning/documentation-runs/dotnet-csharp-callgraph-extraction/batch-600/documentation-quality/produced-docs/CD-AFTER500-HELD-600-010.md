# Documentation: NotificationController.OnAuthorization

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Controllers\NotificationController.cs
- Symbol: NotificationController.OnAuthorization
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var contextFeatures = context.HttpContext.Features; var formFeature = contextFeatures.Get<IFormFeature>(); if (formFeature == null || formFeature.Form == null) { // Setting length limit when the form request is not yet being read contextFeatures.Set<IFormFeature>(new FormFeature(context.HttpContext.Request, _formOptions)); } }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
