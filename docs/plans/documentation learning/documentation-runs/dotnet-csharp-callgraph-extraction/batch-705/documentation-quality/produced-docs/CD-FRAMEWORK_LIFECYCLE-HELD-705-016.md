# Documentation: askapediaweb::CompaignsController.SaveCompaign

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\Controllers\CompaignsController.cs
- Symbol: askapediaweb::CompaignsController.SaveCompaign
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ try { var id = Campaign.SaveCampaign(compaign, UserId.Value); return new ActionResult(OK, new { id }); } catch (Exception ex) { return new ActionResult(ERROR, null); } }

## Delegated Callee Behavior
- askapediaweb::Campaign.SaveCampaign: Accepted callee meaning for askapediaweb::Campaign.SaveCampaign: { if (campaign == null) throw new ArgumentNullException(nameof(campaign)); if (!campaign.QuestionId.HasValue && campaign.Question == null) throw new ArgumentException(nameof(campai

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
