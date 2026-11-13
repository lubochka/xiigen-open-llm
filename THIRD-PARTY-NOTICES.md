# Third-Party Notices

This file lists the third-party components used by this project.

The project itself — LlmFromScratch (xiigen-open-llm) — is licensed under
**AGPL-3.0-only**, Copyright (C) 2026 Luba Karpenko, Genie Soft LTD. Nothing in
this file changes the license of the project's own source; it only records the
external components the project depends on and their licenses.

## Scope and honesty boundary

The entries below are the **direct** NuGet package references declared in the
project's `.csproj` files, as found by scanning the current source tree
(`PackageReference` inventory). This is **not** the complete dependency set:

- **Transitive dependencies are NOT listed yet.** Packages pulled in indirectly
  (for example, `SQLitePCLRaw.*`, which `Microsoft.Data.Sqlite` brings in) do not
  appear below.
- The **final direct + transitive denominator** must be produced by the
  maintainer by running `dotnet list package --include-transitive` on the release
  tree. That authoritative scan supersedes this preliminary direct-only inventory.

Every license below is marked **preliminary** — it reflects the commonly known
license family (MIT / Apache-2.0) for each package, **to be confirmed by the
maintainer against NuGet metadata**. No entry here has been individually verified
against upstream license files, so nothing below is stated as "verified".

## Direct package inventory

Snapshot date: 2026-07-23 (current source tree, branch `PrepareToGPL`).

### Microsoft.CodeAnalysis.CSharp

- **Version:** 4.12.0
- **Declared in:**
  - `12.SemanticCompiler/SemanticCompiler.csproj:13`
  - `13.CodeOptimizer/CodeOptimizer.csproj:13`
  - `21.AgentInfrastructure/AgentInfrastructure.csproj:8`
  - `22.AgentAlgorithms/AgentAlgorithms.csproj:8`
  - `23.OssEquivalents/OssEquivalents.csproj:8`
  - `34.Experiments/Experiments.csproj:13`
  - `35.ExperimentsV2/ExperimentsV2.csproj:13`
- **License:** MIT (preliminary, family MIT/Apache-2.0 — to be confirmed by the
  maintainer against NuGet metadata)
- **AGPL-3.0-only compatibility:** Permissive (MIT family) — compatible with
  AGPL-3.0-only.

### Microsoft.Data.Sqlite

- **Version:** 9.0.4
- **Declared in:**
  - `21.AgentInfrastructure/AgentInfrastructure.csproj:9`
  - `23.OssEquivalents/OssEquivalents.csproj:9`
- **License:** MIT (preliminary, family MIT/Apache-2.0 — to be confirmed by the
  maintainer against NuGet metadata)
- **AGPL-3.0-only compatibility:** Permissive (MIT family) — compatible with
  AGPL-3.0-only. Note: this package pulls in transitive `SQLitePCLRaw.*`
  components that are not yet listed here (see "Scope and honesty boundary").

### YamlDotNet

- **Version:** 16.3.0
- **Declared in:**
  - `23.OssEquivalents/OssEquivalents.csproj:10`
- **License:** MIT (preliminary, family MIT/Apache-2.0 — to be confirmed by the
  maintainer against NuGet metadata)
- **AGPL-3.0-only compatibility:** Permissive (MIT family) — compatible with
  AGPL-3.0-only.

### coverlet.collector

- **Version:** 6.0.2
- **Declared in:**
  - `Tests/LlmFromScratch.Tests.csproj:11`
  - `Tests.ApiDashboardWpf/Tests.ApiDashboardWpf.csproj:12`
  - `Tests/DesignConversion.ModelActivation.Tests/DesignConversion.ModelActivation.Tests.csproj:11`
- **License:** MIT (preliminary, family MIT/Apache-2.0 — to be confirmed by the
  maintainer against NuGet metadata)
- **AGPL-3.0-only compatibility:** Permissive (MIT family) — compatible with
  AGPL-3.0-only. Test-only dependency; not part of the runtime distribution.

### Microsoft.NET.Test.Sdk

- **Version:** 17.12.0
- **Declared in:**
  - `Tests/LlmFromScratch.Tests.csproj:12`
  - `Tests.ApiDashboardWpf/Tests.ApiDashboardWpf.csproj:13`
  - `Tests/DesignConversion.ModelActivation.Tests/DesignConversion.ModelActivation.Tests.csproj:12`
- **License:** MIT (preliminary, family MIT/Apache-2.0 — to be confirmed by the
  maintainer against NuGet metadata)
- **AGPL-3.0-only compatibility:** Permissive (MIT family) — compatible with
  AGPL-3.0-only. Test-only dependency; not part of the runtime distribution.

### xunit

- **Version:** 2.9.2
- **Declared in:**
  - `Tests/LlmFromScratch.Tests.csproj:13`
  - `Tests.ApiDashboardWpf/Tests.ApiDashboardWpf.csproj:14`
  - `Tests/DesignConversion.ModelActivation.Tests/DesignConversion.ModelActivation.Tests.csproj:13`
- **License:** Apache-2.0 (preliminary, family MIT/Apache-2.0 — to be confirmed by
  the maintainer against NuGet metadata)
- **AGPL-3.0-only compatibility:** Permissive (Apache-2.0 family) — compatible
  with AGPL-3.0-only. Test-only dependency; not part of the runtime distribution.

### xunit.runner.visualstudio

- **Version:** 2.8.2
- **Declared in:**
  - `Tests/LlmFromScratch.Tests.csproj:14`
  - `Tests.ApiDashboardWpf/Tests.ApiDashboardWpf.csproj:15`
  - `Tests/DesignConversion.ModelActivation.Tests/DesignConversion.ModelActivation.Tests.csproj:14`
- **License:** MIT (preliminary, family MIT/Apache-2.0 — to be confirmed by the
  maintainer against NuGet metadata)
- **AGPL-3.0-only compatibility:** Permissive (MIT family) — compatible with
  AGPL-3.0-only. Test-only dependency; not part of the runtime distribution.

## Summary

- Direct packages listed: **7**.
- All preliminary licenses fall in the permissive MIT / Apache-2.0 families, which
  are compatible with **AGPL-3.0-only**; count incompatible with AGPL-3.0-only:
  **0** of the direct set (preliminary — pending NuGet metadata confirmation).
- Transitive dependencies (for example `SQLitePCLRaw.*` behind
  `Microsoft.Data.Sqlite`) are **not yet listed**. The final direct + transitive
  denominator comes from `dotnet list package --include-transitive` run by the
  maintainer on the release tree.
