# Documentation: NextStageJobScheduler.SelectMember

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\Services\NextStageJobScheduler.cs
- Symbol: NextStageJobScheduler.SelectMember
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var jmpairs = (from member in _context.Members from job in _context.Jobs where member.Filial == filial.Id.ToString() && job.MemberId == member.Id.ToString() select new {Job = job, Member = member}); var members = jmpairs.GroupBy(p => p.Member.Id).ToDictionary(g => g.First().Member, g => g.Select(i => i.Job).ToList()); var scores = members.ToDictionary(p => p.Key.Id, p => p.Value.Count(j => j.start <= de && j.end >= db)); var memberId = scores.OrderBy(p => p.Value).First().Key; return members.Keys.FirstOrDefault(m => m.Id == memberId); }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
