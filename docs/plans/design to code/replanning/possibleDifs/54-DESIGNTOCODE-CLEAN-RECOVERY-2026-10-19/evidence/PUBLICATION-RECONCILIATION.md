# Сверка параллельной публикации перед обновлением ветки

19 октября 2026. После финального основного review другой участник опубликовал пакет **2026-10-19-designtocode-clean-causal-audit** в commit 6b97c01deecc0382e1a5b5c68b10d520a483c1f2. Его изменения сохранены полностью; общий README берётся именно из этого parent перед добавлением нашей навигации.

Получены ещё **15 Markdown-версий**: 14 новых путей и новая версия общего README. Основной [CORPUS-MANIFEST.json](CORPUS-MANIFEST.json) остаётся снимком **743 версий / 733 repo-paths до b95278b**; новая [дельта](PUBLICATION-DELTA-MANIFEST.json) доводит проверенный текстовый набор до **758 версий / 747 repo-paths**. Этот подсчёт не превращается в утверждение о ручном построчном review всех файлов.

## Содержательные пересечения

| Новый peer owner | Как соотносится с нашим пакетом |
|---|---|
| [U-DTC-1b и U-DTC-3](https://github.com/lubochka/xiigen-planning/blob/6b97c01deecc0382e1a5b5c68b10d520a483c1f2/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-19-designtocode-clean-causal-audit/02-NEW-LEARNED-DESIGN-STATE.md) | U-DTC-1b — отсутствие vector mean/dimension в ordinary **local Critic**; U-DTC-3 — восстановление суммы из округлённого среднего. Это полезные отдельные проверки, а не найденные нами whole-checkpoint whitelist refusal или **neural policy** RuntimePair conditioning |
| [Доступность runtime pair, peer02 §6](https://github.com/lubochka/xiigen-planning/blob/6b97c01deecc0382e1a5b5c68b10d520a483c1f2/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-19-designtocode-clean-causal-audit/02-NEW-LEARNED-DESIGN-STATE.md#L144-L158) | Peer уже сравнил synthetic/real стороны, наличие pair и fallback. CLEAN-CTX01 дополнительно сравнивает содержимое training source prompt с runtime identity prompt; прежнее исследование route признано ближайшим владельцем |
| [N-ADJOINT-01](https://github.com/lubochka/xiigen-planning/blob/6b97c01deecc0382e1a5b5c68b10d520a483c1f2/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-19-designtocode-clean-causal-audit/03-NEW-NUMERICAL-DELTA.md) | Совпадает с numerical delta scalar→SIMD gradA, которую наша глава03 уже рассматривала без заявки на новизну. Детальный численный опыт принадлежит этому peer; нашего нового finding здесь нет |
| [Review Units, строки 42–54](https://github.com/lubochka/xiigen-planning/blob/6b97c01deecc0382e1a5b5c68b10d520a483c1f2/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-19-designtocode-clean-causal-audit/evidence/review/review-units.md#L42-L54) | **StageLearnedWeights уже упомянут** как caller FromCheckpoint для проверки округления. Поэтому после этой дельты нельзя говорить о нуле exact совпадений по имени метода. Последствие whitelist-invalid → отказ всего старого checkpoint там не разобрано |
| [Свежая карта всех механизмов](https://github.com/lubochka/xiigen-planning/blob/6b97c01deecc0382e1a5b5c68b10d520a483c1f2/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-19-designtocode-clean-causal-audit/04-ALL-MECHANISMS-CROSSWALK.md) | Дополняет прежний общий crosswalk. Её гипотезы о PRON, phrase spans, loop labels, feature maps и rounding не присваиваются как новые |

Поиск во всех 15 текстовых версиях не обнаружил CHECKPOINT_LEARNED_WEIGHTS_INVALID, srcenc, design-conversion-runtime-pair, BuildPolicyPrompt или RootOwnershipBinding. Root дополнительно прочитал причинные описания U-DTC-1b/U-DTC-3/N-ADJOINT-01 и source-review контекст StageLearnedWeights. Нулевой match по другим ключам не заменяет это смысловое разграничение.

Для **CLEAN-CP01** сохраняется новая конкретная consequence/import chain поверх ранее известного feature retirement. Для **CLEAN-CTX01** сохраняется другая condition/caller цепочка, частично унаследованная от CLEAN. Никакая из них не объявляется измеренной причиной деградации RP01. Планы source condition не получают score/category labels из чужого локального learner sidecar.

## Проверка дополнения

Дополнительная независимая сверка дельты: **PASS_BOUNDED — DELTA3 NOVELTY RECONCILIATION**. Reviewer подтвердил разграничение U-DTC-1b/U-DTC-3, прежний вклад peer02 §6 и владельца N-ADJOINT-01. Упоминание StageLearnedWeights учтено явно. Новых блокирующих замечаний к двум дополнительным цепочкам нет. Предыдущий основной PASS_WITH_LIMITS относится к ранее рассмотренной сборке до этой дельты; нынешнее дополнение фиксирует отдельный review, не подменяет его прежним.

Новые документы peer не редактировались, их скрипты не запускались. Изменение родительского commit используется для сохранения параллельной работы; предметное обучение и quality измерения этим действием не выполняются.

