# Codex External Calls Mock Technical Debt Plan

## Authority

Path: `C:\Users\lubao\Documents\xiigen\llm mvp\DPO LEARNING\codex-external-calls-mock.md`

This is the only allowed technical-debt plan for the active DPO Legal Learning execution plan.

## Binding Rule

During implementation, every external API/resource code path must use a mock response instead of a real external call. The implementation must include a unit test that asserts exactly the expected mock response/result.

## Required Mock Evidence Shape

For each external call replaced by a mock, record:

| field | required value |
| --- | --- |
| implementation artifact | exact file path and symbol once implementation exists |
| real external resource | API/resource name, endpoint, SDK, or service category |
| mock response/result | exact literal/object asserted by the unit test |
| unit test | exact test file path and test name |
| replacement gate | what evidence is required before replacing the mock with the real call |
| status | `mock_active`, `ready_for_real_call_review`, or `real_call_enabled_after_future_approval` |

## Default Mock Standard

Unless a narrower work order defines a scenario-specific result, use a deterministic mock response/result that can be asserted exactly, for example:

```json
{
  "status": "mocked",
  "result": "DPO_LEGAL_EXTERNAL_MOCK_OK",
  "source": "codex-external-calls-mock.md"
}
```

The unit test must assert the exact `status`, `result`, and `source` values, not only non-null output.

## Current Entries

| implementation artifact | real external resource | mock response/result | unit test | replacement gate | status |
| --- | --- | --- | --- | --- | --- |
| `C:\Projects\DPOLegalLearning\CanvaToCode\src\platform\design-conversion-client.ts` `DesignConversionClient.submitConversion` using injected `fetchImpl` | Design conversion platform API `POST /conversion/submit` | `{"jobId":"mock-design-conversion-job","status":"mocked","result":"DPO_LEGAL_EXTERNAL_MOCK_OK","source":"codex-external-calls-mock.md"}` | `C:\Projects\DPOLegalLearning\CanvaToCode\tests\design-conversion-client.test.ts` `uses the injected fetchImpl mock for the external design conversion submit result` | Future Luba instruction authorizes real call, source authority is rechecked, source branch mergeability is validated, credentials/secrets are not captured, and a non-mock integration gate proves the real API result without overclaiming mock evidence. | `mock_active` |
| `C:\Projects\DPOLegalLearning\CanvaToCode\src\platform\design-conversion-client.ts` `DesignConversionClient.getConversionJob` using injected `fetchImpl` | Design conversion platform API `GET /conversion/jobs/{jobId}` | `{"jobId":"mock-design-conversion-job","status":"mocked","result":"DPO_LEGAL_EXTERNAL_MOCK_OK","source":"codex-external-calls-mock.md"}` | `C:\Projects\DPOLegalLearning\CanvaToCode\tests\design-conversion-client.test.ts` `uses the injected fetchImpl mock for the external design conversion job lookup result` | Future Luba instruction authorizes real call, source authority is rechecked, source branch mergeability is validated, credentials/secrets are not captured, and a non-mock integration gate proves the real API result without overclaiming mock evidence. | `mock_active` |
| `C:\Projects\DPOLegalLearning\FigmaToCode-design-gen\src\ui\components\design-gen\providers\OpenAIComparisonProvider.ts` `OpenAIComparisonProvider.compareAndPlan` direct `fetch` | OpenAI Chat Completions API `POST https://api.openai.com/v1/chat/completions` | Raw mock response `{"choices":[{"message":{"content":"{\"score\":88,\"ops\":[{\"op\":\"set-fill\",\"nodeId\":\"zone-bg\",\"color\":{\"r\":0.1,\"g\":0.2,\"b\":0.3}},{\"op\":\"set-text\",\"nodeId\":\"txt-headline\",\"content\":\"DPO Legal Mock\"},{\"op\":\"teleport\",\"nodeId\":\"bad-op\"}],\"notes\":\"Mock comparison says header color and headline should change.\",\"annotation\":[\"replace background\",\"update headline\"],\"tokens\":{\"fonts\":[{\"name\":\"heading\",\"family\":\"Inter\",\"size\":24,\"weight\":\"Bold\"}],\"colors\":[{\"name\":\"primary\",\"hex\":\"#123456\"}],\"spacing\":{\"gap\":16}}}"}}]}`; asserted normalized result `{"success":true,"score":88,"ops":[{"op":"set-fill","nodeId":"zone-bg","color":{"r":0.1,"g":0.2,"b":0.3}},{"op":"set-text","nodeId":"txt-headline","content":"DPO Legal Mock"}],"notes":"Mock comparison says header color and headline should change.","annotation":["replace background","update headline"],"tokens":{"fonts":[{"name":"heading","family":"Inter","size":24,"weight":"Bold"}],"colors":[{"name":"primary","hex":"#123456"}],"spacing":{"gap":16}}}` | `C:\Projects\DPOLegalLearning\FigmaToCode-design-gen\tests\test-openai-comparison-fetch.test.ts` `uses a deterministic mock response for compareAndPlan and asserts the exact result` | Future Luba instruction authorizes real call, source authority is rechecked, source branch mergeability is validated, credentials/secrets are not captured, and a non-mock integration gate proves the real OpenAI result without overclaiming mock evidence. | `mock_active` |

## Replacement Rule

Replacing a mock with a real external resource call requires a future Luba instruction and must pass the active plan gates: source authority latest-updated re-check, separate `C:\Projects` working directory for `DPOLegalLearning`, merge/PR no-conflict validation into the original source authority branch before commit/push, and no overclaim of readiness from mock-only evidence.
