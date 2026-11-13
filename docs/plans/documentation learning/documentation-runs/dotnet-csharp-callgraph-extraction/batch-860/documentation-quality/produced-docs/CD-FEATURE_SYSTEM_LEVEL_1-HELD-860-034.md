# Documentation: a1BreezeController.Clients

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\a1BreezeController.cs
- Symbol: a1BreezeController.Clients
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ UserBack myUser; if (!IsAuth(out myUser)) return new List<Client>().AsQueryable(); try { if (myUser.IsSystemManager==true || myUser.IsFilialManager == true ) { return _db.Context.Clients.Where(e => e.MainUserId==myUser.MainUserId).OrderBy(q => q.Adress); } if (myUser.IsClientManager==true) { string Client = myUser.Client; string[] Clients = Client.Split(','); int[] ClientsI = new int[Clients.Length]; for(var i=0;i<Clients.Length;i++) { ClientsI[i] = int.Parse(Clients[i]); } return _db.Context.Clients.Where(e => ClientsI.Contains(e.Id) && e.MainUserId==myUser.MainUserId).OrderBy(q => q.Adress); } else { return _db.Context.Clients.Where(e => e.MainUserId==myUser.MainUserId).OrderBy(q => q.Adress); } } catch(Exception em) { return new List<Client>().AsQueryable(); } }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
