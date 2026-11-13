# Финальный независимый просмотр пакета61

**Дата:** 20 октября 2026. **Предмет:** staged документы в `deliverables/61-LEARNING-UNBLOCK-AND-EXPERIMENT-AUDIT-2026-10-20`, до завершения root README/source-scope. Stage не редактировался. Код, tests и обучение не запускались.

**Результат:** содержательные правила пользователя и актуальная политика20Oct согласованы. Обнаружены два конкретных документных уточнения, которые следует внести в финальную сборку; новых оснований останавливать разрешённую подготовку или запрашивать approval нет.

## R1. E2: bootstrap fallback не равен явному отказу

Место: `10-CURRENT-EXPERIMENT-OVERLAYS.md`, E2, строки61,69,88–105 текущего stage. Предложенный learned-support reader возвращает candidate с confidence0, затем сохранённый TrainedTranslationAdapter выбирает bootstrap. Это корректно описанный существующий переход, но итог нельзя автоматически записывать как `explicit refusal` или правильный перевод.

Первичный код:

- [BootstrapTranslationAdapter77–89](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/27.TranslationDomain/TranslationDirectionAdapters.cs#L77-L89) возвращает `OperationResult.Ok`, source в оболочке `[DIRECTION:BOOTSTRAP]` и confidence0.5. Здесь нет собственного явного сообщения об отказе.
- [TrainedTranslationAdapter124–139](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/27.TranslationDomain/TranslationDirectionAdapters.cs#L124-L139) при low-confidence candidate вызывает этот fallback. Proposed `unsupported-learned-source` adapter/reason не становится final adapter автоматически.
- [TranslationRoutingFlow85–97,121](https://github.com/lubochka/llm_mvp_core/blob/f5dd532045716495a2c7668573250e5d7cc46810/27.TranslationDomain/TranslationRoutingFlow.cs#L85-L121) уже отличает bootstrap: `Passed=false`, `NextAction=request_more_context`, `OverallVerdict=TRANSLATION_LOW_CONFIDENCE`. Следовательно отсутствие принятого перевода можно измерить по настоящему Flow, не выдумывая новый refusal API.

**Предлагаемый текст уточнения:**

> В этом варианте неподдержанный learned source вызывает существующий bootstrap fallback. Он не засчитывается как правильный перевод или явный человеческий отказ. `accepted_translation` определяется по собственному verdict `TRANSLATED` и non-bootstrap adapter; `wrong_accepted` — accepted translation, нарушающий фиксированный semantic gold. Bootstrap/low-confidence rate показывается отдельной колонкой. `explicit_refusal` учитывается только при действительно наблюдённом соответствующем структурированном или человеческом результате, которого один bootstrap wrapper не обеспечивает. Learned unsupported reason читается тем же диагностическим наблюдателем отдельно от final Flow adapter. Новый formatter отказа в этот A/B не добавляется: это изменило бы второй фактор.

Такой учёт сохраняет возможный полезный эффект «опасный неверный перевод больше не принят», но не приписывает модели способность сформулировать отказ. Рядом остаются coverage loss и фактически показанный пользователю bootstrap text.

## R2. Старый reference scan нужно явно пометить снимком

`inventory/source-reference-coverage.json` в просмотренном stage — исходный **bf9b543** scan: массив555 строк, у22 `protected_mentions=0`. У массива нет собственного snapshot header. Актуальный полный reference inventory внутри `experiment-card-audit-current.json` имеет555/555 соответствий и явно названный f5dd commit. Эти файлы не противоречат друг другу по существу, но без метки старый легко принять за текущий список пропусков.

**Предлагаемое исправление:** переименовать исторический файл в `source-reference-coverage-bf9b543.json` либо обернуть исходный массив envelope с `core_commit=bf9b543...`, `role=historical_reference_scan`. В README/source-scope связать его с текущим `experiment-card-audit-current.json`. Не удалять исторические свидетельства и не писать, что22 гипотезы отсутствуют на f5dd.

## Подтверждённые счётчики и целостность

Машинно перечитаны JSON stage; все корректно разбираются. В актуальном реестре:

| Проверка | Фактический счёт |
|---|---:|
| `cards` |125|
| Сумма `subcases` |470|
| Сумма `embedded_prompts` |133|
| `active_files` |24|
| `source_reference_inventory` |555|
| Актуальные прямые reference matches |555|
| Старый integrity manifest |248 файла|
| Latest changed integrity manifest |152 файла|
| Объединение уникальных protected design paths |249|
| Late semantic crosswalk |22 строки|

Все152 latest и248 old записи integrity содержат `equal=true`; старые и новые SHA относятся к своим снимкам. Общая фраза249 project documents корректна. 854 result files не выданы за полностью повторно прочитанный корпус. Наличие всех555 ссылок и133 текстовых правил не названо полной независимой валидацией470 subcases. Reference-style Markdown labels в просмотренных документах разрешаются; root ещё нормализует локальные ссылки и добавляет индексы.

## Содержательные проверки, пройденные без новых замечаний

- Native update, data/load/restore и consumer обученного состояния разнесены. Нет общего запрета на dependent consumer из-за0 новых updates. Предыдущая blanket-invalid оценка T явно отозвана в01/08/09/05.
- Общий approval per-card/SHA отменён. PREPARABLE означает самостоятельную подготовку; missing native SDK в T описан как конкретное наблюдение окружения, а не универсальная блокировка программы.
- Новые числа обучения не заявлены. Исторические успехи отделены от новых гипотез; 19/24 памяти не названо восстановлением Student.
- `04-NATIVE-UPDATE-ZERO-DELTA-RESULT` предлагает один наблюдатель в том же actual PAD A/B. Он не назначает отдельный fake/no-update run и не ослабляет historical continuation admission.
- E1 использует уже поддержанные `бесшумно/громко` и `дверь/люк`; проблемные `быстро/тихо/окно` не перенесены в новый материал. Admission остаётся единственным кодовым фактором. Локальные3 Verifier updates/16Verify честно отличены от полного11Unit-call варианта.
- E2 явно `NOT_IMPLEMENTED/NOT_RUN`, source support приобретается через настоящий Train. Родной reader до всех candidate routes не позволяет незаметно обойти приобретённую поддержку через wordmap/kNN. Это ещё не обещание наблюдаемого выигрыша.
- Все проверенные мини-карточки и промпты признают полезный измеренный delta по метрике/домену без обязательного accuracy gain; показывают остальные показатели и регрессии.

После R1 и R2 новых существенных замечаний по проверенному scope не осталось. Этот review не выдаётся за полный повторный source audit всех нейронных операций или854 исторических result files.
