# Аудит после Q003: состояние артефактов и границы переноса

3 ноября 2026. Канонический пакет core: [аудит154](https://github.com/lubochka/llm_mvp_core/blob/f3b64f87159193c2897092d6e571b2b65e3b75c4/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-154-postq003-artifact-and-transfer-audit-2026-11-03.md), [план155](https://github.com/lubochka/llm_mvp_core/blob/f3b64f87159193c2897092d6e571b2b65e3b75c4/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-155-postq003-addressed-repair-plan-2026-11-03.md), [покрытие156](https://github.com/lubochka/llm_mvp_core/blob/f3b64f87159193c2897092d6e571b2b65e3b75c4/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-156-postq003-card-coverage-2026-11-03.md), [договор50](https://github.com/lubochka/llm_mvp_core/blob/f3b64f87159193c2897092d6e571b2b65e3b75c4/docs/plans/replanning-possible-diffs/experimets/codex-50-postq003-binary-and-historical-evidence-contract-2026-11-03.md). Исходный срез: core7ce6b14c / planningeabb34a; учтены параллельные изменения до corec1f3 / planningf003, включая отчёт579d и передачу завершённого Q004. Пакет завершает документный аудит и усиливает условия приёмки. Новое обучение и продуктовые исправления не выполнялись.

## Установленные факты

| Проверка | Результат | Последствие |
|---|---|---|
| Полная дельта187b54b в RP01-MICRO | 3502 пути =3469 modified +33 added. Из modified:3401 current=187,49 restored,19 other | 3401 — сохраняющаяся Git-версия; содержимое всех этих LFS objects не скачивалось. Каждый потребляемый donor проверяется адресно |
|19 other paths /10 уникальных OID | Конкретный конфликтный текст длиной289 bytes реконструирован независимо; SHA256/size равны current LFS OID/size | Корректный внешний pointer адресует содержимое, не являющееся checkpoint; проверять materialized bytes |
|Shared P11/P12 S1.weights | Manifest/original:320620 bytes; current:320618 и другой OID | Восстановить donors существующих29-D3/29-W1; исторические code fixes сохраняются, manifest не подгоняется |
|Q002/Q003 |49 pointers равны original; этот аудит не скачивал LFS payload заново | Binary restore не доказывает нового выигрыша: у обоих опытов A/B остаются2/4→2/4 |
|Q002 index | Устаревшее DPO0 исправлено: recovery800SFT+8DPO завершён, прежняя800SFT partial сохранена | Известны1600SFT+8DPO; прочая историческая цена не сверена; transfer NOT_GRANTED |
|P03/Q004 |T1 NLLΔ−0.2305984497; accuracy2/4→1/4; средняя NLL панелиΔ+0.1023478508 | Частная польза и регрессия сообщаются вместе. Полный final state не опубликован в P0301; повтор Train для его воссоздания не назначен |
|Поручения P127/P131 | Для P127 назначен внешний manifest15 result files; P131 InputRoot исправлен на/evidence | Реальный native verifier ещё должен реализовать authority/layout. Документы не равны C# acceptance |

Точные paths, hashes, исходные rows и воспроизводимый Python доступны в [evidence package](https://github.com/lubochka/llm_mvp_core/tree/f3b64f87159193c2897092d6e571b2b65e3b75c4/experiments-v2/_audits/postq003-2026-11-03). Свежий LFS download, native Load, dotnet/pwsh acceptance — UNRUN. Исторические raw/source/budgets сохранены.

## Что углублено в карточках

Карточки checkpoint, LoRA/координат, DPO objective и аудита получили конкретные положительные и отрицательные случаи: правильный pointer к конфликтному тексту; чужой payload того же размера; receipt вместо полного final state; подменённый historical пакет вместе с его локальными hashes; неправильный parent root P131; смешение attempt/phase/denominator. Исполнение остаётся у существующего владельца core, в его назначенном бюджете. Отсутствующие bytes не создают нового научного плана.

Параллельный153 о null/empty Output P115 и метриках P127 сохраняется. Перенос/Q01 остаётся135-R01-CONCURRENCY. Записанная попытка, сохранность, измеренная польза и приёмка продукта имеют отдельные статусы. Счётчик RESULT_RECORDED не означает научную приёмку131 опыта; исторический NEXT не разрешает повтор PRIMARY. В передаче c1f3 родитель отмечает4/131 последовательных проверок, следующим по порядку Q005, ещё не назначенный. Этот аудит сохраняет данный порядок.
