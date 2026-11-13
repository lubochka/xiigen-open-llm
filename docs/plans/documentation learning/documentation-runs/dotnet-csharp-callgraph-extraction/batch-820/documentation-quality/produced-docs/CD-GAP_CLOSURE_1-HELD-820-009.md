# Documentation: askapediaweb::QuestionExtensions.ToViewModel

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\BL\AutoMapper\QuestionExtensions.cs
- Symbol: askapediaweb::QuestionExtensions.ToViewModel
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ return AutoMapperConfig.Mapper.Map<AskQuestion, QuestionViewModel>(entity); }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
