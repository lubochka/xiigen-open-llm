# RP03 · 16.10.2026 · опубликованный ремонт и проверка чтением

**Текущий статус: PUBLISHED_AND_SCOPED_READBACK_VERIFIED / AUTHOR_AUDIT_ONLY.** Первоначальные пометки `REPAIR IN PROGRESS` в [17](17-RP03-FIRST-LESSON-READINESS-REPAIR-2026-10-16.md) и future-publication в [18](18-RP03-OCT16-APPLIED-REPAIR-AND-REVIEW-SCOPE.md)/[19](19-RP03-STARTUP-DRIFT-REPAIR-2026-10-16.md) относятся к моменту подготовки. Этот receipt подтверждает состоявшуюся публикацию их адресных carrier repairs, но **не** product/code/learning acceptance.

## Фактическая публикация

[PR №15](https://github.com/lubochka/xiigen-planning/pull/15) слит в `llm_plan3` с результатом `merged=true`. [Merge commit](https://github.com/lubochka/xiigen-planning/commit/46d80f5f2f4d00f83683a5962fa53a2a7a4eed66): **46d80f5f2f4d00f83683a5962fa53a2a7a4eed66**. GitHub commit read подтвердил parents `c0f788ec5f3818b817bcd7775039509721427f51` (актуальная целевая ветка) и `00d7db4ab6de307b386bc9f5084ebe918049929a` (наш candidate), tree `aa732b46451ed831894ccbdf6113c851ae390847`. История и параллельные изменения не перезаписывались force-push.

PR содержал **21 изменённый документ**, два candidate commits: `7de1dce5357a971c955563860e6cb55287ff5e11` (основной ремонт) и `00d7db4ab6de307b386bc9f5084ebe918049929a` (startup drift). Ранее [17] опубликован отдельным `5f11281d50463535050bef641a46cb87c978ff3c`. Этот receipt — дополнительная документная запись, не повтор публикации PR.

## Что расширено в findings

| ID | Проверенный вывод | Адресная правка |
|---|---|---|
| O16-01 | Остаточный prompt conflict: experimental Candidate должен был пройти ordinary serving до cold proof | Trial update/cold и ordinary scoped admission разделены; устранён цикл exam/adoption. |
| O16-02 | Rank flip ошибочно требовался после одного численного update | PARAMETER/SCORE/RANK/COLD/GAIN/ADOPTION отдельно; есть численный пример корректного шага без argmax flip. |
| O16-03 | В просмотренном ExecuteAsync L2 request создаётся с пустым Metadata | Найти реальный immutable feature/law/coordinate reader; отсутствие глобального version owner НЕ объявлено доказанным. |
| O16-04 | Stale five/nine child navigation | Единый V7 состав15parents/9children. |
| O16-05 | GOOD-example мог быть прочитан как разные held-out cases между C0 и T | Явно одинаковые cases для arms, независимые от TRAIN/OP_DEV; paired gains/losses. |
| O16-06 | 03A ссылался на RP01 V6 вместо текущего dispatch | Проверенный RP01-EXECUTE-NOW; его существующий Main500 и scoped simulation policy не изменены. |
| O16-07 | INSTALL-AND-START действительно направлял в tasks/v6/V6.2 | Переписан current startup, forwarding AGENTS и task index на V7; нет alternate execution route. |

O16-01/03 конкретизируют известные N26–N29, не считаются новыми runtime bugs. O16-05 — неоднозначность промпта, не доказательство реально выполненного confounded experiment. Подозрение о систематической подгонке тестов по истории этой ревизией не доказано.

## Реальная проверка ПОСЛЕ merge

Следующие файлы повторно прочитаны из immutable merge SHA выше, а не из подготовленного текста:

| Файл внутри RP03 pack | Readback scope | Returned blob |
|---|---|---|
| README.md | Весь файл | `a2b3724c18f9c9cb9e23689eeb6ab2a955c56c3d` |
| PACKAGE-MANIFEST.json | 1–40;15ID/paths,9childpaths, early trial/exam route | `b694ab707046804625f3f783d2c2fb0e32a09257` |
| tasks/v7/RP03.02B.md | 16–60; numeric update, no rank gate, independent signal, real reader | `eb2e71ffeb53ec3a9db8befc62d9db27305189ee` |
| tasks/v7/RP03.02D.md | 15–40; trial cold, coherent ledger, live tuple unchanged | `cbf154bb4789c48ee04374a73fae54797276ee7d` |
| tasks/v7/RP03.03B.md | 1–34; matched cases, trial scope, bulk/domain/PC protocol | `cef31ee6f1dc412c20b49c2affdba1c514590485` |
| INSTALL-AND-START.md | 1–34, весь current short startup | `8dc1a8cdf06215a39736a093d3612a66037d9d07` |

Manifest inventory перечисляет15parents/9children и сохраняет их ID. Это не утверждение, что выполнен полный semantic re-audit каждой из24cards: непосредственно переписаны **02A/02B/02C/02D/03A/03B/03C и child00C-A**. Остальные8parents+8children сохранены; [functional crosswalk](../Kiro-Xiigen-RP03-Typed-ManyToMany-Flow-Correction-Slice-Pack/RP03-V7-OCT16-FUNCTIONAL-CROSSWALK.md) указывает их owners и bounded review scope. Подтверждённый GitHub merge и чтение ключевых carriers не подменяют runtime proof.

## Практический текущий маршрут

**02A + реальный00C-A →02B shared-scorer trial update →02D new-PID trial cold**. 00B/01A→01B→01C typed lane и00A/00C-B domain preparation параллельны. 02C BUILD/JOIN и03B trial-quality exam доступны до promotion. 02C ADOPT — после настоящей scoped acceptance;03C — ordinary Host/PC. Не ждать Student DPO, всех семи domains, Windows Actions или mandatory rank flip для первого настоящего Flow step.

Domain/kNN semantic laws не отброшены: первая Flow causal comparison фиксирует их версию, а RP02 развивается параллельно. C#/EN→RU/SQL pilot; ORCH/DESIGN/ANGULAR/DPO — nonblocking extensions. DPO human-role selection=CODEX Astra6UltraCode, both final sides=external open Chinese model, exact-pair quality=separate CODEX Astra6UltraCode. Actual invocation receipts обязательны; Flow preference не Student DPO.

## Чего не было

**0 новых product tests; Core/weights не изменены этим ремонтом; Flow/Student training NOT_RUN; domain/PC gains NOT_MEASURED; separate Astra review NOT_PERFORMED.** Source/planning/exam tasks не получают coverage-квоты; реальные изменённые deterministic rights/type/once-only/state boundaries сохраняют минимальную адресную проверку. Independent domain exams, а не процент software coverage, должны подтвердить обучение.

[Вход RP03](../Kiro-Xiigen-RP03-Typed-ManyToMany-Flow-Correction-Slice-Pack/README.md) · [единый dispatcher](../Kiro-Xiigen-RP03-Typed-ManyToMany-Flow-Correction-Slice-Pack/RP03-V7-LEARNING-FIRST-DISPATCH-2026-10-15.md) · [полная методика алгоритма/экзаменов](../Kiro-Xiigen-RP03-Typed-ManyToMany-Flow-Correction-Slice-Pack/RP03-V7-ALGORITHM-AND-EXAMS.md).
