# P17F: component evidence matrix

| Component | Что теперь поддержано | Full readiness? | Что остается заблокировано |
|---|---|---:|---|
| `PromptToTask` | component lifecycle evidence accepted before P17F | `False` | Не является самостоятельным full API claim и не закрывает SkillSelection/dashboard/host readiness. |
| `OrchestrationReasoning` | component lifecycle evidence accepted before P17F | `False` | Не является самостоятельным full API claim и не закрывает SkillSelection/dashboard/host readiness. |
| `SkillSelection` | component runtime improvement and lifecycle import/export evidence accepted | `False` | Artifact locator is not runtime-wired/common/final promotion; evidence remains component-scoped and no-claim. |
| `API/firewall` | no-claim firewall still blocks unsupported full readiness claims | `False` | Full claim requires positive semantic-quality evidence for all components and no no-claim evidence. |

Вывод: P17D/P17E усилили SkillSelection component evidence, но не превратили его в full API readiness или semantic quality.
