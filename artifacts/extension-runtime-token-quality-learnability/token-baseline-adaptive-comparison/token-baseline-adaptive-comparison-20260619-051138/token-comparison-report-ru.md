# TOKEN-BASELINE-ADAPTIVE-COMPARISON-PLAN-OR-FIRST-PAIR-01

Generated: 2026-06-19T05:11:38+03:00
Branch: codex/chat-gpt-5.5
Run root: artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/token-baseline-adaptive-comparison-20260619-051138

## Итог

Первая complete baseline/adaptive пара не создана. Текущая плановая матрица TOKEN-01..03 содержит 9 строк: Claude, Cursor и Codex по методам prompt-to-task, orchestration-plan и skill-selection. По этим 9 строкам сейчас 0/9 complete pairs.

Причина: в текущих safe evidence нет пары, где одновременно есть baseline_common_no_adaptive и adaptive_user_local_target для одного и того же pair_id, host, scenario, method, evidence class, tokenizer/accounting и redaction policy. Старые TOKEN-04 direct core API пары рассмотрены как historical/scoped diagnostics, но не повышены до текущих 9 строк: они не привязаны к текущим TOKEN-PAIR-* ids, adaptive effect/user-local target ref не доказан для этих строк, а quality gate не открыт.

## Что сделано

- Исправлен state human-readable Russian status: текущие human status поля больше не содержат последовательность из четырех вопросительных знаков.
- Подтверждена текущая обязательная матрица 9 token comparison pairs из TOKEN-01..03.
- Создана 9-строчная comparison matrix со статусами `missing` или `deferred`.
- Создан missing-input ledger для первой вероятной пары: `TOKEN-PAIR-CLAUDE-PROMPT-TO-TASK-001`.
- Создан точный следующий bounded work order для добора недостающего baseline counterpart без product/token/quality claim.

## Почему пара не complete

Ближайший кандидат: `TOKEN-PAIR-CLAUDE-PROMPT-TO-TASK-001`.

У него уже есть current adaptive-side narrow trace:
`TOKEN-03-CLAUDE-PROMPT-TO-TASK-NARROW-ADAPTIVE-TRACE-001`.

Но нет same-class baseline_common_no_adaptive trace для того же current pair_id. Поэтому сравнение не является complete pair, token pair count остается 0/9.

Вторая частичная строка: `TOKEN-PAIR-CODEX-PROMPT-TO-TASK-001`.
У нее есть baseline-side narrow installed-adapter trace, но нет adaptive_user_local_target trace.

Cursor строки deferred: безопасный Cursor launch/no-backend/no-model-metadata contract остается незакрытым.

## No-overclaim

Этот пакет не доказывает token savings, product savings, quality improvement, semantic quality, learnability, runtime readiness, target promotion, locator update, final readiness или product readiness. Он фиксирует только текущее отсутствие complete pair и следующий точный work order.

## Следующий bounded work order

`TOKEN-FIRST-PAIR-CLAUDE-PROMPT-TO-TASK-BASELINE-COMPANION-01`

Цель: найти или создать только недостающий baseline_common_no_adaptive counterpart для `TOKEN-PAIR-CLAUDE-PROMPT-TO-TASK-001` с тем же host/scenario/method/evidence class/tokenizer/redaction контрактом. Если exact counterpart не найден или не разрешен отдельной runtime authority, вернуть missing-input ledger, не создавать pair PASS и не заявлять savings.
