# Documentation: LoginController.contact

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\LoginController.cs
- Symbol: LoginController.contact
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ IActionResult response = Unauthorized(); if (phone != null && phone.Length > 5 && phone.Length <= 20) { Contacts contact = new Contacts(); contact.Phone = phone; contact.Updated = DateTime.Now; _context.Contacts.Add(contact); _context.SaveChanges(); response = Ok(new { }); } return response; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
