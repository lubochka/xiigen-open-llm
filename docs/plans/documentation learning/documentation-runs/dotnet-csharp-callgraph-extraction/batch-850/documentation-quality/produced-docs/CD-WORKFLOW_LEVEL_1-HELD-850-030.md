# Documentation: askapediaweb::QuestionsController.GetCompaignQuestionById

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\Controllers\QuestionsController.cs
- Symbol: askapediaweb::QuestionsController.GetCompaignQuestionById
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var question = Campaign.GetCampaign(id).ToQuestionViewModel(); //if user is not anonymous just return compaign question if (User.Identity.IsAuthenticated) return question; //else create anonymous user and return question with token var anonymousUser = BL.User.CreateAnonymousForCompaign(id); question.Token = anonymousUser.Token; question.TokenExpiration = anonymousUser.TokenExpiration; return question; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
