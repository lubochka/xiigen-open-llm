# Заключительная независимая сверка интеграции

Дата UTC: 2026-10-20T19:09:36.124389+00:00. Проверяющий: `/root/knn_history`.

**Вердикт: PASS_BOUNDED_DOCUMENT_INTEGRATION.** Проверены пять назначенных сводных файлов и их согласованность с двумя новыми карточками/промптами. Вывод относится к прочитанным байтам ниже. Он не является runtime certification, повторным историческим исследованием, проверкой всех 163 опытов или разрешением обучения/переноса в продукт.

## Подтверждённая согласованность

- JSON содержит 42 уникальных существующих ID с номерами1–42 и два новых уникальных предложения A1/A2: всего44. Первые8 имеют сохранённые измерительные статусы; в оставшейся последовательности ровно36 уникальных ID.
- Оставшийся порядок побайтно по значениям ID совпал с таблицей REGISTRY: №09–40 → A1 DPO-SHIFTED-CONTEXT-01 → A2 IDENTITY-TRAIN-INDEX-01 → №41–42. Следующий N63-AR-CAP, №09. №08 явно завершён и не возвращается из stale NEXT08. Исторические next-поля имеют source-назначение; неоднозначного поля next_id в current rows нет.
- README/REGISTRY/REQUIREMENTS/SOURCES одинаково различают native snapshot dd150aa и документный additive81141; позднее дополнение описано как7 файлов без native changes. Здесь проверена согласованность этой уже установленной командой provenance, а не повторно запрошен GitHub diff.
- Оба новых фактора остаются NOT_RUN/PREPARATION_REQUIRED; конкретизация identity NEEDS_NATIVE_HOST не противоречит общему статусу. Text blueprint и будущие команды не выданы за собранный runner. Старые измерения отделены от новых предложений.
- Default executor model сохранён; CODEX/Astra selection/quality и внешний китайский автор chosen/rejected — отдельные роли. Механические fixtures явно не приписываются внешней генерации. Новых model calls или обучения эта интеграция не утверждает.
- SC01 сохраняет отдельные Prepare/Calibrate/Compare с immutable root и новыми phase subdirs; Calibrate=A200 суммарно, Compare=A0/B200 со свежим общим donor1000. Скрытое продолжение calibration state и автоматический запуск трёх фаз запрещены. SFT optimizer не выдаётся за DPO continuation.
- Identity budget согласован: warm128×4=512 различных identities при4 независимых содержаниях; главные A/B по1×4; всего130 Train,520 предъявлений,8 независимых содержаний,96 Flow calls,2restore. Restore/index-build и Train плюс сумма сохраняются отдельными осями, не заявлен измеренный выигрыш.
- 0→0 не выдаётся за приобретённый навык; терминальная SC01 метрика отделена от donor anchors и teacher forcing. Полезные NLL/resource изменения не стираются, регрессии сохраняются. STOP/NEXT и protected read-only согласованы с текущей фазой документации.

## Найденный и устранённый integration drift

При первом чтении delivery содержал предпоследнюю SC01 карточку/промпт: A ошибочно описывался как обязательно достигший trainer, а teacher-forced response метрика называлась fullResponseExact. Автор уже уточнил early bridge CaptureAndValidate до StepVector и teacherForcedResponseExact/free-generation=N/A. Интегратор перенёс финальные тексты; заключительное сравнение подтвердило byte-equality всех5 delivery/neural Markdown с author freeze. Повторного runtime/source исследования для этого не выполнялось.

## Границы и незавершённая независимая работа

Новая MECHANISM-COVERAGE.md проверена здесь только на наличие и навигационную связь; её исторические соответствия не переаудированы. Late-delta материалы готовит соседний аудитор; эта сверка не приписывает себе его review. Авторская future fixture/host readiness остаётся непроверенной нативным выполнением. После изменения байтов настоящие hashes описывают прежнюю версию и не считаются аттестацией новой.

Состояние локальных ссылок пяти сводных файлов на момент receipt: все разрешились.

## SHA256 назначенных сводных файлов

| Файл | SHA256 | Bytes |
|---|---|---:|
| `README.md` | `40913788458d38e3e8df87c8b336672a6d46808a71859d12f910cb073096eaf8` | 27799 |
| `REGISTRY.md` | `961b08b2e5aed4a8c1da257e766e4b9342b4ce9866f8413b907b1d25a019cf2f` | 21805 |
| `REQUIREMENTS.md` | `85fb7cb03628b0eed066fe48d48e83912adf5c38db2868d507a2d278cf332ff1` | 9466 |
| `SOURCES-AND-REVIEW.md` | `2e1ac06709cc0f849b5db136de3e9c0509e242ddbd93399ec29f285ae1cd31a9` | 9806 |
| `experiment-audit/resolved-queue-current.json` | `2122b2452cd050dc2e246c163e6d8a3c7fc9fb11410d6942bcb2a515a010722d` | 46291 |

## SHA256 согласованных карточек и промптов

| Файл | SHA256 | Bytes |
|---|---|---:|
| `neural/02-DPO-SHIFTED-CONTEXT-01-CARD.md` | `78b04e6fd01b507a49bc5188a0246630ea4a1ca1b60112c3b598bbf1d10bf97b` | 27761 |
| `neural/03-SC01-EXECUTOR-PROMPT.md` | `42e8520191b44e6b86c433460e44c1794d8e153cc925219f3f53e3b5423aa163` | 11644 |
| `knn/identity-train-index-01/CARD.md` | `3771a5dc4cf08781d80b4d07be38b414572bf00b94e955dc40af4900e59056f6` | 14820 |
| `knn/identity-train-index-01/EXECUTOR-PROMPT.md` | `6bf8456c973f4d6ba114db866392b8c13695787158b4c850ac3c456cd6b5353f` | 8509 |

В этом заключительном действии создан только данный review-файл. Native build/test/train/inference, новые исторические поиски и изменения проверяемых документов не выполнялись.
