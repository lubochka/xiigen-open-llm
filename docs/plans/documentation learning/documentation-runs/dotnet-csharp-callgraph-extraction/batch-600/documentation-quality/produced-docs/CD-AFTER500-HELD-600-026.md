# Documentation: Configuration.Seed

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\Migrations\Configuration.cs
- Symbol: Configuration.Seed
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ // This method will be called after migrating to the latest version. // You can use the DbSet<T>.AddOrUpdate() helper extension method // to avoid creating duplicate seed data. E.g. // // context.People.AddOrUpdate( // p => p.FullName, // new Person { FullName = "Andrew Peters" }, // new Person { FullName = "Brice Lambson" }, // new Person { FullName = "Rowan Miller" } // ); // }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
