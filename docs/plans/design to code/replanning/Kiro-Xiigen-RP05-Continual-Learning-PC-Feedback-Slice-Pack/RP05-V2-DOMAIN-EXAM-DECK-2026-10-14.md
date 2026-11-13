# RP05 V2 — Экзамены на реальные законы семи доменов и эффективность гибридных алгоритмов

**Все ниже — обучающие ПРИМЕРЫ ПЛАНИРОВАНИЯ, НЕ реальные training pairs, НЕ sealed test data, НЕ измеренные ответы Student.** Нужны отдельные новые source-family-disjoint задания, заморозка oracle и рубрик *до* наблюдения trial. Семь предметных паспортов покрываются программой; **ни один обязательный all-seven gate не предшествует самому первому C# B1**. Перенос на иной домен нельзя объявить, если Student не обслуживает его.

| Домен / владелец | GOOD — правильный закон и полезная способность | BAD — неверный результат и почему он плох | EDGE / фальсификатор |
|---|---|---|---|
| **C# / RP01+RP05** | Для nullable s корректно объяснить последовательность s?.Length ?? 0 с учётом AST, языка и фактического исходника; ?. условно обращается к члену, ?? даёт fallback | Утверждать, что ?. автоматически заменяет null на 0 или ?? вызывает Length; путает разные операторы | nullable overload, новая C# версия, иной независимый кодовый проект, допустимая перефразировка |
| **SQL / RP02** | LEFT JOIN/IS NULL и три значения TRUE/FALSE/UNKNOWN, dialect+schema+duplicate cardinality точно сохраняются | Объявить NOT IN с NULL в подзапросе эквивалентным NOT EXISTS для всех диалектов; NULL меняет семантику | NULL ключи, пустая выборка, дубликаты, смешанные диалекты — UNKNOWN и запрос уточнения |
| **EN→RU / RP02** | “no more than five” → “не более пяти”; связанный женский референт требует согласованных местоимения/прилагательного/формы в допустимом синтаксисе | “не менее пяти” меняет неравенство; ближайший по embedding мужской вариант, нарушающий связанную предметную референцию, незаконен | неоднозначная анафора, слова с различной грамматической/биологической категорией, исключения, неопределённая роль следующего слова — abstain |
| **Angular / documentation** | (click) — event binding, [value] — property binding в указанной версии Angular/template context | Назвать [click] обработчиком события без подтверждения контекста; смешивает связывания | template scope, alias, version transition, independent project |
| **Orchestration / Flow** | При DAG A→B запустить B только после законного завершения A, side effect лишь при авторизованном selected path; cancellation bounded | Выбрать быстрый результат запрещённой ветви: короткое время без законности не успех | нет пути, cycle, tenant fence, timeout, rollback и стоимость проигравших ветвей |
| **DTC Figma/Canva→HTML/CSS** | Семантический DOM, source order/RTL, keyboard focus, responsive 320/768/1440 и законные assets в реальном client | Идентичный скриншот 1440 при overflow 320 / сломанном focus нельзя считать качеством | недоступный font, конфликт design tree vs pixels, private asset, изменённый breakpoint |
| **DPO Builder / RP05** | Одинаковые x+context+source-law; модель из внешнего OPEN Chinese provider пишет ОБЕ финальные y+ и y−; CODEX Astra 6 UltraCode выбирает human case и отдельно судит ту же пару | UserCorrection или Student response маркируется «Chinese rejected», косметический negative, другой context, сам себе судья | TIE, BOTH_BAD, UNJUDGEABLE, правовой источник закрыт, truncation теряет отрицание → quarantine |

## Domain mapping и смысловое kNN (не просто поиск соседей)
Для каждой семьи материализовать domain×intent×language/stack×version×rights×law; сравнить обычное **predicted** назначение домена с **oracle** доменом. Не скрывать MIXED/UNKNOWN/новые законы. Токен/вектор должен хранить применимые синтаксические роли (verb/preposition/adjective/subject), привязку референта, род/число при применимых языковых правилах, отрицание, сравнение, закон SQL NULL, API framework version, кодовую операцию, источник и права. Обучаемая LoRA параметризация отличается от encoder/semantic projection/codebook/metric/index; идентичная размерность двух несовместимых кодовых книг не равна идентичному смыслу.

На одной и той же exact lawful eligible pool сравнить k=1, fixed k>1 c **реальной** законной агрегацией/переранжировкой нескольких источников, и query/domain-adaptive k=f(domain,law,eligibleCount,density,margin,uncertainty,budget,rights). Проверить, что не менее двух релевантных разрешённых соседей способны изменить *типизированный полезный выход*, а не только ссылку на evidence. Затем отдельно изучить ANN vs exact recall/quality и цену reindex, включая миграцию metric/version. Нулевая разрешённая база → abstain/fallback, а не кросс-доменная галлюцинация. Конкретное hits[0] — текущее ограничение одного consumer, а не рецепт общей предметной метрики.

## Четыре изолированных causal-factor экзамена
**Unit:** заморозить Flow/Graph/Orchestrator, оценить many→many typed associations и lawful output для неизвестных входных семейств; reorder/fan-in не должен уничтожать OutputRefs.

**Flow:** заморозить Unit outputs и менять лишь learned relationship/correction, сравнить следующий Unit и доменную полезность. Static wiring сам по себе не обученное состояние.

**Orchestrator:** заморозить allowed DAG, candidate pool, solver, permissions и side effects; сравнить learned route/stop/fanout с маленьким перечисленным lawful feasible alternative; включить wasted/cancelled branch PC.

**DynamicGraph:** заморозить lower roles, применить подлинный learned higher-order relation update и проверить новую составную задачу, не встречавшуюся в training families; static extra edges не считаются приростом.

**LoRA:** показать actual A/B recipient change→logit/typed consumer output; отдельно от изменения semantic token projection и kNN. Сравнить deterministic-only, learned-only-with-hard-laws и hybrid по одинаковому полному ресурсу, then только причинно обоснованные маленькие взаимодействия 2×2.

## Регистрация независимого эксперимента
До начала: exam_id, исходный git/model-state digests P/C0/T, independent source-family IDs train/dev/sealed, law+version, chosen primary lawful utility, negative rubric, OOD/abstain, named model state owner, same prompt/decoder/rights, allowed time/PC budget, meaningful gain threshold и retention bound, judge blindness, stop rule. Экзаменатор не может видеть набор для обучения и не должен менять ожидания после T.

Для каждого family_id: input/source digest, output, domain expected/predicted, law checks, lawful success, output type+provenance, time, CPU, simultaneous RAM/VRAM, model+index+rollback disk, failures. Для каждого bulk: новый B0→B1→B2, paired C0→T gain, engineering P→C0, worst domain и забывание, costs of all provider roles/failed work, uncertainty grouped by independent families. Небольшой pilot полезен для обнаружения дефектов, но не даёт права заявить общий статистический прирост.
