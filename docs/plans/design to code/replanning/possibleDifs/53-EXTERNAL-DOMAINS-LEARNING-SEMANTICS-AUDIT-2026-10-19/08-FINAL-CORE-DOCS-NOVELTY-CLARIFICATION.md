# Окончательное уточнение новизны: пять новых гипотез и одна конкретная локализация

**Финальный документальный срез — 19 октября 2026, core `358af351e22a92a3f7938d1f9221d5aeb7ddb053`.** Во время проверки подготовленного Git tree соседний автор обновил только документы защищённого каталога. C# исходники не изменились; все выводы о коде по-прежнему относятся к зафиксированному `44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76`. Записей в core эта работа не выполняет.

## Что уточнилось

В позднем обновлении — **13 Markdown версий: 11 замен и 2 новых пути**. Их точные bytes сверены с Git blob SHA. Общий поисковый охват теперь **689 уникальных Markdown: 415 planning и 274 core**. Корпуса 661, 671 и 687 в предыдущих receipts остаются историческими этапами. Эта глава уточняет итоговые утверждения о новизне; она не отменяет первичные исходники и результаты probes.

| Сопоставление | Общее предыдущее знание | Что установлено здесь | Финальная классификация |
|---|---|---|---|
| KU-02 и NK-03 | Отбор допустимых кандидатов после необратимого отсечения способен потерять правильный вариант | `DeterministicRegistryEdgeCandidateProvider` применяет whole-registry relative floor до `edge.ToNodeIds`; доказан конкретный bound crossing и остановка до learned inference | Новый конкретный owner/trigger в известной общей семье. Не присваиваем авторство всей идеи eligibility-first |
| PT-01 и открытый duplicate lead | Копия победителя способна превратить top-two сравнение в сравнение с тем же решением. Соседний текст оставляет `OPEN_RECIPIENT` | Установлены `ContinueWithAcceptedPattern`, принятый семантический повтор с другим ID, selector загруженной PromptToTask memory, margin0 и последующий generic fallback; заданы отрицательный контроль и cold проверка | **PRIOR_GENERIC_LEAD + NEW_CONCRETE_OWNER_TRIGGER_CONSUMER_LOCALIZATION**. Общая идея duplicate/margin больше не заявляется как новая |

Предыдущее NK-03 относится к `KnnSkillFilter`: global topK → `skillCandidates` intersection, с отдельно подключаемым context consumer. Наш KU-02 меняет поддержку edge candidates через относительный порог. У них различаются классы, пороговая операция и вызывающая цепь; proposed A/B сохраняет исходные raw scores. [Первичный поздний рецепт NK-03](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/12-additions-wiki-sql-knn-tensor.md#L105-L113).

Предыдущая запись о duplicate прямо формулирует invariant и top-two margin, но не называет подтверждённый native recipient. Это содержательный prior lead, даже при отсутствии точного `PromptToTask` имени. Поэтому одна лишь проверка отсутствия class names была бы недостаточна. Наш вклад PT-01 — локализация работающей по исходникам цепочки и более конкретный эксперимент; изменение learned memory через accepted continuation отличается от абстрактного копирования ranked input. [Открытый duplicate lead](https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/13-additions-feedback-bpe.md#L74-L80).

## Как считать результаты пакета

Сохраняются **шесть конкретных механизмов с источниками и опытами**: KU-01, KU-02, PT-01, PT-02, AUT-WEIGHT-01, AUT-REPLAY-02. Для строгого счёта новизны это **пять новых конкретных гипотез плюс одна новая локализация ранее открытого общего предположения**. Нового native доказательства улучшения обучения нет ни у одного пункта. Приоритетные recovery-опыты P0–P5, известные Unit риски и исторические успехи не включаются в счёт новых гипотез.

Содержательная поздняя проверка ограничена изменёнными документами и ближайшими пересечениями. Она не является полным новым чтением всех 689 документов. Последующие внешние изменения находятся за границей этого зафиксированного среза.

[Точные источники и hashes](evidence/novelty/core-final-delta-ledger.json) · [Независимая адресная проверка](evidence/review/core-late-delta-review.md) · [Основной пакет](README.md) · [Начать с восстановления external_domains](00-EXTERNAL-DOMAINS-RECOVERY-FIRST.md)
