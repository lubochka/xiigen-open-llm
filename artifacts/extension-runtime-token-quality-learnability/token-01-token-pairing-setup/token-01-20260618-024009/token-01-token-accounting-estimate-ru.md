# TOKEN-01: оценочный учет токенов

Run id: `token-01-20260618-024009`

Этот файл является только deterministic approximation/accounting estimate для setup-фазы. Он считает уже сохраненный prompt text из `artifacts/extension-runtime-token-quality-learnability/scenario-pack-token-quality-v1.jsonl` с помощью приближенного метода `xiigen-heuristic-word-punctuation/v1/heuristic-estimate`.

Оценка не считает host outputs, не сравнивает baseline/adaptive, не использует live host traces и не доказывает экономию токенов. Ее назначение: дать стабильный accounting baseline для будущих paired token runs, где каждая пара будет проверяться по одинаковому tokenizer/accounting version и только после quality non-regression.

## Границы оценки

- Входные данные: 12 сохраненных scenario prompts из scenario pack.
- Метод: deterministic word/punctuation approximation, а не vendor tokenizer и не live host usage meter.
- Сравнение baseline/adaptive: не выполнялось.
- Host coverage: Claude/Cursor/Codex покрыты в matrix setup, но этот estimate считает только prompt text сценариев.
- Savings claim: отсутствует; TOKEN-01 не разрешает заявлять token savings.
- Quality dependency: до любой интерпретации token delta нужен same-pair quality non-regression PASS.

## Оценка prompt tokens по сценариям

| Сценарий | Prompt hash | Оценка prompt tokens |
|---|---|---:|
| TQ-001-prompt-task-documentation | `sha256:f599dff02c7e15c8236fccbc2ad8f8ec0743ddf5c1f2dc6a2c0cbe884bce16d7` | 40 |
| TQ-002-prompt-task-interrupt | `sha256:6cdae328248b90679e7c26b0e7cc56bb4c8fd9e121430b8f3552cf69179214d9` | 34 |
| TQ-003-prompt-task-redaction | `sha256:4401a0d1294e7a3992e74127182a9a8ed7e14838d11dc388ea0d2aa8c03a47bb` | 33 |
| TQ-004-orchestration-runtime-proof | `sha256:f14b79556e6d39af03bbdae20314fb2b7d07c5b34a5cc2b69543747552f58f5e` | 43 |
| TQ-005-orchestration-token-pairing | `sha256:c3f886c224a9a25d6a72de30538c7608259413c188db80e3ab12550bf313998c` | 39 |
| TQ-006-orchestration-learning-lifecycle | `sha256:4016452c767263358aa2fb7fa043a253c1b186f9fe486c9faea73cadc94e5c3a` | 54 |
| TQ-007-skill-architecture-audit | `sha256:523c9713450f2267e86ac272e975ce07eaa85aae3489dadfab4b705ff3ab250d` | 42 |
| TQ-008-skill-debug-runtime | `sha256:129ac9443f3c57984c8ea82cc5a780b7fe2f45f75b583d5e4a746c047c8d9db4` | 37 |
| TQ-009-skill-learning-safety | `sha256:b3b857088fe575095161a85d33cf016768e1a506e0d23e7b0a29bf42929223be` | 40 |
| TQ-010-mixed-no-overclaim | `sha256:a1152cc2b139681267e4d9f1f153f7bda6ddb6a8b07c709f0a343ab8531e789e` | 42 |
| TQ-011-mixed-shortcut-audit | `sha256:73a567ed3f4a34c4dbe914e0a431ddd8ef726c15b2e75071177cebab3978325e` | 42 |
| TQ-012-mixed-final-readiness | `sha256:f77aef68c89593bb7e735f44064c12f5e7143eb0e69a874c78ae2ced05ea4956` | 51 |

## Что означает этот файл

Этот estimate доказывает только то, что для TOKEN-01 сохранен воспроизводимый приближенный accounting snapshot по prompt text сценариев. Он не доказывает снижение токенов, не доказывает качество adaptive behavior и не является readiness artifact.
