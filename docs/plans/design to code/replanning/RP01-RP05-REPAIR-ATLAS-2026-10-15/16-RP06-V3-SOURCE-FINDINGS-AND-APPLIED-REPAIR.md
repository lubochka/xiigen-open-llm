# 16 · RP06 direct-source findings F10–F14 and applied nineteen-card repair

**15 October 2026; planning change only.** K inspected current branch 58d12ecfe7af05e3221f984bf6c4c67d82fe3b96. RP01–RP05 Atlas old findings and RP06 V2 F01–F09 are preserved. Do NOT claim F10–F14 are confirmed runtime bugs, measured learning effects, native installations or independent Astra reviews.

## F10 (NEW) — verify DTC dataset gate before claiming earliest single lesson

K/44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs lines 213–217 defaults MinimumTrainRecords=50 and MinimumHeldOutRecords=50, builds/uses count gate at 664 and 1578–1606. K/44.DesignConversionDomain/DesignConversionTrainingPipeline.cs lines 16–22 defaults RequireDatasetCountGate=true, examines it at 828 and 975. Consequence: 'one new accepted pair trains' is conditional on proven legal trainer route, not automatically the default DTC dataset path. Source-proven other RP01 E1 path may work, but is not verified by this source-only review. **Fix applied to tasks 04A/04B and exam 06B**: trace actual route/options and count before optimizer; no lowering gate or fabricated heldout. GOOD actual mutable weight step and cold reader; BAD renamed checkpoint with blocked gate; EDGE need genuine source-disjoint bulk or an authorized different route.

## F11 (NEW) — real critic learning must not be inferred from scaffold

K/44.DesignConversionDomain/DesignConversionTrainedUnits.cs lines 11–16,62–64 declares critic as evidence-only runtime scaffold, lines 117–145 and 162–225 pass optional learned correction preferences to improver rule selection, lines 269–272 include nonlearned warning. **Fix to 02B/03A/03B/06B:** source proof of trained preference origin, owning optimizer/checkpoint, actual consumer and measurable corrected HTML/CSS choice. GOOD same-law unknown mobile frame improves with true parameter; BAD input verdict score described as trained visual intelligence; EDGE missing learned reader => NO_TRAINABLE_RECIPIENT rather than false PASS.

## F12 (refinement of RP06 V2 F04) — raw upload exists, external delivery remains open

K/25.ModelServer/DesignConversion/DesignConversionModelServerAspNetEndpointMapper.cs 21–36 has 8MiB JSON cap and local refs-only no public deployment. .../DesignConversionModelServerAspNetHostStartup.cs 303–307,371–446 maps a raw artifact endpoint at 442; .../DesignConversionModelServerArtifactUploadHttpHandler.cs 42–46,112–147,265 is transport-neutral with admission. **Fix 01A/02A/00B:** inspect Figma/Canva native producer→platform auth/tenant→upload sha→status/HTML/CSS; do not assert raw path missing or native installed. GOOD verified actual external route; BAD local handler treated as Flow47; EDGE too-large/unauthorized payload denied or verified server-owned referenced route.

## F13 (refinement of RP06 V2 F02) — IDE training label vs real optimizer

K/42.ProjectWikiDomain/ProjectWikiPluginOrchestrationAdapter.cs 54–74,111–164 and 238–280 shows optional ILogicalOrchestrator vs BOOTSTRAP_FALLBACK, and SelectedUnits/AllowTraining can mention ProjectWikiTrainingOrchestrator without invoking it. **Fix 05A/05B/05D/06C:** distinguish local plan, native installed host, actual admitted optimizer state mutation and independent gain. GOOD cold accepted new-state IDE outcome; BAD SelectedUnits name claimed trained; EDGE no host → LOCAL_ONLY.

## F14 (refinement of RP06 V2 F01) — local model activation ≠ public product

K/25.ModelServer/DesignConversion/DesignConversionModelServerModelActivationHandler.cs 329–348 and 436–492 includes ProductReady=false and public_http_endpoint_verified=false in relevant boundary metadata. **Fix 02A/04B/06B:** separate valid cold local model reader vs deployed/native Figma/Canva adoption. GOOD both receipts separately; BAD local manifest promoted to public native product; EDGE incompatible encoder/index must deny activation.

## Scope of actual applied repair

All nineteen EXISTING tasks RP06.00A/B,01A/B,02A/B,03A/B,04A/B,05A–D,06A–D,07A were changed IN PLACE to have a detailed V3 current/NOT task, domain-specific primary source, role-aware 3–4 actual skills, exact steps, reasoned GOOD/BAD/EDGE, functional independent learning exam, a minimal deterministic invariant, failure route, subworkers with full prompt and no-false-PASS output. Obsolete 12-common skill mandates in 06B/C/D and other tasks were removed, and original full task prompts retained. No results/Rxx or RP01–RP05 code modified; no experiments, teacher/judge calls, LoRA training or Astra independent review performed.

**Owner trigger:** RP06 00B/06A/02B local reader/holdout → 04A exact DPO → 04B legal count-compatible trainer/cold readback → 06B/06D actual gain, with independent native Figma 01A/02A/03A and IDE 05A/05B/05C lanes. Full instructions: [RP06 V3 runbook](../Kiro-Xiigen-RP06-DTC-IDE-Learning-Delivery-Slice-Pack/RP06-V3-DIRECT-CODE-REVIEW-AND-EXECUTION-2026-10-15.md).
