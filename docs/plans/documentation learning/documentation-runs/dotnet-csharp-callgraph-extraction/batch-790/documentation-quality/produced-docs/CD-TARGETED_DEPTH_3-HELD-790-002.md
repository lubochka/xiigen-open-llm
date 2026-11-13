# Documentation: askapediaweb::Campaign.GetCampaign

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\BL\Campaign.cs
- Symbol: askapediaweb::Campaign.GetCampaign
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ using (AskapediaDBContext db = new AskapediaDBContext()) { var campaign = db.Campaigns.Include(x => x.Question) .Include(x => x.Question.Answers) .FirstOrDefault(x => x.Id == campaignId); return campaign.ToViewModel(); } }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
