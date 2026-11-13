# Аудит дополнения: документация и написание кода

Дата: 25.10.2026. Этап: подготовка учебного материала. Генерация китайской моделью, нативная проверка новых fixture и обучение **не выполнялись**.

## Что уже существовало

На снимке `xiigen-planning` / `llm_plan3` / `66cd68b247ad9d92570b37594fcb2cb5eb33a6cc` были девять родительских тем документации, шесть тем написания кода и шесть узких целей в `curriculum/doc-code.json`: null-only fallback, nullable COUNT и boolean disabled binding, каждая в вариантах документации и кода. Дополнение сохраняет существующие карточки и расширяет шесть других учебных границ до подробных микроцелей.

В новых файлах 12 микроцелей и 48 полных reference-пар. Есть 23 объявленных family_id; это **не утверждение о 23 независимых источниках**. Связанные документация/код имеют общее семейство; варианты одного LEFT JOIN fixture также объединены. `derived_from` и `leakage_group_ids` связывают дополнения с прежними опорными парами. Ряд новых примеров подробно уточняет прежние seeds, поэтому их нельзя объявлять независимым EXAM.

## Сопоставление с источниками

| Учебная граница | Источники | Что действительно поддерживает источник | Чего он не доказывает |
|---|---|---|---|
| Передача CancellationToken | RAG-027, RAG-028, RAG-029; прежние DOC-CS-02 / CODE-CS-02 | Видимый code/callee contract, точный runtime, одно основное смысловое отличие, caller/callee attribution | Что передача token физически отменяет транзакцию или что конкретный новый fixture уже исполнен |
| Эффекты вокруг await | Те же источники и прежние async examples | Сохранение error path и side effects, отсутствие выдуманного catch | Нативный outcome нового Program.cs без его запуска |
| Фильтр правой стороны LEFT JOIN | RAG-040, RAG-041; прежние DOC-SQL-02 / CODE-SQL-02 | Отличие объяснения от реализации, явные схемы/условия, семантика связей как предмет учебного материала | SQL Server execution plan, latency или blocking; старый текст корпуса не является автоматически runtime gold |
| Предикат UPDATE | RAG-041, RAG-042; прежние DOC-SQL-03 / CODE-SQL-02 | Связь результата с точным условием допустимой записи; недостаточный остаток | Что Stock>0 эквивалентно Stock>=qty; concurrency/rollback нескольких действий из одного UPDATE |
| Последний Observable | RAG-027, RAG-037, RAG-039; прежние DOC-NG-02 / CODE-NG-02 | Требование видимого контракта, project architecture, version discipline | Поведение Angular 16 по ADR про Angular 21/22; server rollback после unsubscribe |
| Область catchError | Те же источники и прежние Observable examples | Сохранение выбранной границы и error recovery как предмет материала | Результаты нового RxJS TestScheduler без его исполнения |

RAG-040 содержит маршрутизирующие CODE_Q-заготовки; его role — методология/контекст, а не доказательство истинности всех технических утверждений. RAG-041 содержит материал обработки предметных задач, включая ORM примеры; перенос ORM-фактов в T-SQL без fixture не выполнялся. Для новых примеров `metadata.source_role=methodology_not_runtime_gold`.

### Точные project references

Ранее закреплённые источники сохраняют свои идентификаторы и blob:

- [Правила составления документационных пар](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md), RAG-027, blob `454fadaf468eb9e31dc1bbb9be8f8508c234efa3`.
- [Версия стека и передача callee summaries](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md), RAG-028, blob `6f8f48d7125e24fed8d8caee69954857583ea8b1`.
- [Scheme planner: конфликт stock guard](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/external-sources-training-sets/clean-batch-001/node-03-scheme-planner-set.md), RAG-042, blob `1650c79a2e5a213eb57ba77701c3159f37ebb9a6`.
- [Angular runtime/config ADR](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md), RAG-037, blob `d182324879939f54ae447060f0f0fbf03b15e04f`.
- [Angular version ADR](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-VERSION-STANDALONE-STRATEGY.md), RAG-039, blob `a14589d9182228162b009ad8c30e7cd7a854d068`.

Новые внешние сайты, датасеты или репозитории для этого дополнения не скачивались. Ссылки на внешнюю документацию внутри старых ADR не рассматриваются как разрешение расширить границу источников.

## Подтверждённый конфликт в stock guard

В `node-03-scheme-planner-set.md` строка 127 предлагает условие `StockAvailable > 0`, а строка 147 — `StockAvailable >= requested_quantity`. Первое проверяет только ненулевой запас. При запасе 2 и запросе 3 оно разрешит списание ниже нуля, если иных ограничений нет. Это вывод из показанного предиката, не новое измерение обучения.

В дополнении эта разница стала предметом пар `DOC-SQL-GUARD` / `CODE-SQL-GUARD`. Условие задачи явно сообщает проверен ли положительный qty upstream, существуют ли CHECK/trigger и конкурентные соединения. Примеры не используют противоречивую старую строку как положительный эталон. Одиночный UPDATE не объявляется доказательством всей транзакции или сериализуемости.

## Версии и oracle

Сохранены профили родительских карточек: C# 10 / .NET 6; T-SQL / SQL Server 2019; Angular 16 / RxJS 7.8. Это фиксированные учебные профили, не рекомендации «самых новых» версий. Предусмотренный runner Observable использует JavaScript/TestScheduler и не заявляет Angular browser integration; новые API Angular и смена TypeScript в эти пары не входят.

Каждый reference содержит `metadata.oracle` с полным входом/ожиданием и `status=NOT_RUN`, `result_observed=false`. В карточках заданы точный каталог будущего harness, состав файлов и команда. Сами native oracle hosts создаются reviewer по этим договорам при проверке новых generated pairs; текущая поставка не выдаёт отсутствующие hosts за выполненные артефакты.

Для SQL Server требуется родной движок: SQLite не принимается как замена oracle. Для RxJS временная шкала виртуальная; sleeps, сеть и повторная подписка, скрывающая ошибку outer catchError, запрещены. Для C# TaskCompletionSource обеспечивает управляемый success/fault/cancel, затем ожидается wrapper; пустой trace до завершения не трактуется как failure процесса.

## Наследованные навыки и приоритет текущего задания

Прочитаны приложенные навыки planning-dpo-authoring, dpo-training-data, dpo-triples-to-experiment-pairs, experiment-data-integrity, plan-review, code-examination, promptops-cycle, human-language-gate. Полезные правила — traceable evidence, видимый контракт, одно значимое отличие, недопущение фиктивных результатов — применены.

Устаревшие положения не перенесены: фиксированный минимум прироста accuracy; объявление отрицательного общего delta провалом вопреки полезному другому показателю; обязанность заполнить положительный outcome просто потому, что прошла сессия; лимит текстового overlap, отбрасывающий точный minimal pair. Текущее указание пользователя выше: успех относится к измеренному полезному показателю и домену, регрессии показываются рядом, синтетические reference не получают измеренный outcome.

## Что проверено сейчас

- JSON и JSONL читаются; 12 objective IDs и 48 pair IDs уникальны.
- У каждой цели четыре полные контрастные пары; prompt не заменён метаданными.
- Все строки имеют reference split; ни одна не названа сгенерированной китайской моделью или train-ready.
- Все источники используют существующие RAG IDs.
- Родительские topic/profile/domain сохранены; DOC и CODE находятся в отдельных наборах.
- Карточки используют run IDs D301–D312 в порядке additions JSON; следующая цель берётся из канонического stream index, не дублируется старый ordinal.
- Обе стороны каждой пары имеют одинаковый формат ответа; rationale находится вне ответа.
- Проверены stop/next, штатное ожидание handle, неизменность истории, русскоязычный отчёт, текущая модель без override, B1 restore и запрет 0→0 как доказательства.

Смысловая проверка выполнена чтением самодостаточных fixture. Native compile/runtime, генерация Kiro и измерение обучения остаются отдельными невыполненными этапами; будущие observed результаты нельзя заполнять этими статическими ожиданиями.

## Исправления независимого ревью перед публикацией

Независимый reviewer нашёл неполный ответ в двух cancellation reference: теперь явно указаны число вызовов и возврат того же Task, включая условную true-ветвь. ID черновиков сохранены; опубликованные исторические результаты не изменялись.

В await-сценариях общий fault уточнён до `InvalidOperationException`; отдельная typed-catch пара дополнительно использует `FormatException`. Отмена создаётся `TaskCompletionSource.SetCanceled`. Faulted Task с `OperationCanceledException` исключена: иначе async wrapper может завершиться Canceled и безусловное обещание сохранения Task.Status было бы неверным. Эти ограничения теперь одинаково видны в prompt, парах, oracle и промпте генератора. Для чистоты fixture явно задано, что `trace.Add` не бросает исключений.

Команды oracle получили абсолютный `--out`, поскольку cwd при запуске остаётся каталогом preparation. RxJS runner обязан привязать `createRequire` к наблюдённому абсолютному `XIIGEN_RXJS_PACKAGE_JSON`, проверить версию 7.8.0 и загрузить runtime оттуда; наличие зависимости в другом checkout не считается автоматической доступностью для скрипта в `C:/x/lsets`. Несуществующая переменная/пакет — это ожидающая предпосылка, не утверждение об окружении.

Различены oracle для двух видов задач: CODE исполняет два кандидата, DOC исполняет единственный показанный код и сравнивает две гипотезы о нём. Русское описание не трактуется как компилируемый код. В `CODE-NG-LATEST` ученик явно получает имя switchMap; этот набор учит составлению указанного выражения, поэтому самостоятельный выбор оператора без подсказки ему не приписывается.
