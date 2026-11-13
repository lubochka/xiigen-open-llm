import fs from 'node:fs';
import { describeXiigenCodexEntrypoint } from 'file:///C:/Users/lubao/.codex/plugins/cache/xiigen-local/codex-xiigen-smart-skills/0.1.0+codex.runtime04e-20260616-231500/src/codex-entrypoint.js';
const evidenceDir = 'C:/Projects/llm_mvp_core/docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/codex-path-a-api-boundary-20260627-223748';
const metadata = describeXiigenCodexEntrypoint();
fs.writeFileSync(`${evidenceDir}/host-boundary-metadata.json`, JSON.stringify({
  entrypointId: metadata.entrypointId,
  sourceInvokesCoreEndpoint: metadata.sourceInvokesCoreEndpoint,
  deterministicSkillSelectionAuthorityInHost: metadata.deterministicSkillSelectionAuthorityInHost,
  dpoGenerationInHost: metadata.dpoGenerationInHost,
  learnedModelMutationInHost: metadata.learnedModelMutationInHost,
  claim: 'Host adapter metadata says it invokes core and does not add deterministic skill selection authority in host.'
}, null, 2), 'utf8');