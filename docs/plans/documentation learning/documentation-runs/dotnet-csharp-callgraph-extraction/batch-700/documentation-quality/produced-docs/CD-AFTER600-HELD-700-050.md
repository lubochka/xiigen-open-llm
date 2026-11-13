# Documentation: askapediaweb::Campaign.SaveCampaign

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\BL\Campaign.cs
- Symbol: askapediaweb::Campaign.SaveCampaign
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ if (campaign == null) throw new ArgumentNullException(nameof(campaign)); if (!campaign.QuestionId.HasValue && campaign.Question == null) throw new ArgumentException(nameof(campaign)); if (campaign.GatherLeads && campaign.AskLeadEmail.HasValue && !campaign.AskLeadEmail.Value && campaign.AskLeadPhone.HasValue && !campaign.AskLeadPhone.Value) throw new SaveEntityException("Phone or email field must be selected for lead form if campaign marked to gather leads."); if (campaign.GatherLeads && campaign.Question.Answers.All(x => !x.GatherLeads.HasValue || !x.GatherLeads.Value)) throw new SaveEntityException("At least one answer must selected to gather leads if campaign marked to gather leads."); if (campaign.GatherLeads && campaign.SendLeadsToEmail.HasValue && campaign.SendLeadsToEmail.Value && string.IsNullOrWhiteSpace(campaign.LeadsReceiverEmails)) throw new SaveEntityException("Leads receiv

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
