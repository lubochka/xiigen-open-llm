# Documentation: quantabizContext.OnModelCreating

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Models\quantabizContext.cs
- Symbol: quantabizContext.OnModelCreating
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ modelBuilder.Entity<ActionToStatesDefs>(entity => { entity.HasIndex(e => new { e.ActionId, e.StateId }) .HasName("ActionsToState"); entity.Property(e => e.Id) .HasColumnName("id") .HasDefaultValueSql("(newid())"); entity.Property(e => e.ActionId) .HasColumnName("actionId") .HasMaxLength(50); entity.Property(e => e.Addition).HasColumnName("addition"); entity.Property(e => e.Cost).HasColumnName("cost"); entity.Property(e => e.LevelId) .HasColumnName("levelId") .HasMaxLength(50); entity.Property(e => e.MinValue).HasColumnName("minValue"); entity.Property(e => e.Regularity) .HasColumnName("regularity") .HasMaxLength(50); entity.Property(e => e.StateId) .HasColumnName("stateId") .HasMaxLength(50); }); modelBuilder.Entity<Actions>(entity => { entity.HasIndex(e => new { e.ParentId, e.Language }) .HasName("ActionsParent"); entity.Property(e => e.Id) .HasColumnName("id") .HasDefaultValueSql("(n

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
