# Documentation: askapediaweb::User.CreateAnonymousForCompaign

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\BL\User.cs
- Symbol: askapediaweb::User.CreateAnonymousForCompaign
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ using (AskapediaDBContext db = new AskapediaDBContext()) { var compaign = db.Campaigns.FirstOrDefault(x => x.Id == compaignId); if (compaign == null) return null; AskUser newUser = new AskUser(); newUser.id = Guid.NewGuid(); newUser.Country = compaign.Country; newUser.City = compaign.City; newUser.Gender = compaign.Gender; if (compaign.MinAge.HasValue && compaign.MaxAge.HasValue) { newUser.BirthDate = new DateTime((DateTime.Now.Year - (compaign.MaxAge.Value + compaign.MinAge.Value) / 2), 1, 1); } newUser.Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())); newUser.TokenDateExpire = DateTime.Now.AddHours(24); db.Users.Add(newUser); db.SaveChanges(); return new LoginResponse { Token = newUser.Token, TokenExpiration = newUser.TokenDateExpire.Value }; } }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
