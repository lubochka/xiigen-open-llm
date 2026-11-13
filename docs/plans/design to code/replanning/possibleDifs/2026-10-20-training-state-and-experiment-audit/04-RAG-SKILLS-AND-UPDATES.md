# RAG, навыки и присланные обновления

> **Окончательный срез аудита:** native code совпадает с прочитанным `62bcc8f`; адресная документная сверка доведена до core `ce890a48efda7f00d0ee040f2383cc2d06428ea6` и planning `3b625a83ee878906d42544516741849a16c04689`. В protected **291 Markdown / 128 файлов карточек**. N61 уже покрывают Mode1 и reference reuse; N-KU имеет опубликованную узкую таблицу ответов; N60-E2 конкретизировал writer/state/reader, но host не завершён. Числа 286/126/577 ниже — исходный воспроизводимый срез. [Поздние изменения и приоритет версий](06-CONCURRENT-UPDATES.md), [последняя дельта core](coverage/06-FINAL-CORE-DELTA-ce890a.md).

## Что использовано

Прочитан session RAG master index и usage guide; разобраны четыре NDJSON индекса: DPO — 201 запись, design — 140, merge — 33, IDE — 30, всего **404 записи**. Переходы проверялись и в отцовские директории, в том числе `dpo legal build`, `language translate learning`, `orchestrator experiment` и `PrepareToGPL`. Дерево каталогов является навигацией; его получение не означает, что содержимое каждого родительского файла прочитано.

Для архитектурного смысла использованы [ILearningMachine pattern notes][machine-notes], RP01 [SOURCE-AND-ARCHITECTURE][rp01], guides how-to-train/how-to-measure-quality и конкретные owners в коде. Для результатов — первичные JSON/отчёты и исходный experiment, а не только пересказ в RAG. Все ссылки закреплены версиями в [source ledger](05-SOURCES-AND-REVIEW.md).

Применены запрошенные навыки архитектуры, планирования, проверки плана/кода, prompt engineering и человеческой коммуникации. У больших current skill files прочитаны относящиеся к задаче разделы; не заявляется полное повторное прочтение тысяч строк всех навыков. Полностью прочитанные приложенные инструкции и решения по конфликтам перечислены в [attachment review](coverage/attachment-rules-review.md).

| Навык / семейство | Практический результат в этом пакете |
|---|---|
| Architect behavior / human-readable architecture | Причинная схема learner → state → reader; разные роли Unit/Flow/graph; исходники рядом с выводом |
| Planning startup / plan review / self validation | Exact small work order, requirements map, буквальные пути и статус готовности; не принимать исторический family-menu за runnable card |
| Code examination / generated code review | Чтение caller и consumer, отличие известного дефекта от текущего, source-copy diff и provenance |
| Prompt patch / promptops | Один factor, фиксированные inputs/metrics; каждый prompt повторяет действующее правило полезного выигрыша |
| Human-language gate / glossary | На первом месте эффект для обучения; имена классов служат доказательством. Domain, профиль, mechanism и architecture role не смешиваются |

## Как разрешены противоречащие старые требования

Прямые девять правил пользователя имеют приоритет над старой инструкцией. Это применяется как конкретное решение, а не новый процесс согласования.

| Старое требование | Решение текущей сессии |
|---|---|
| Обязательный прирост accuracy, +0.20 или «без любой регрессии» | Любое полезное измеренное улучшение — успех по его показателю/домену; все другие изменения рядом |
| kNN/словари/детерминированные scorers не могут доказывать runtime learning | Смотрим на actual learned state и влияние Train. Фиксированный gold-ответ не доказательство, но выученная память допустима |
| Перед каждым опытом ещё раз получить личное одобрение ID/SHA | Общая блокировка уже отменена current policy. Разрешённую подготовку выполняет исполнитель; пользователь не должен придумывать host |
| Cold restore/consumer без новых уроков всегда посторонний | Допустимы при точном learned artifact и source dependency; вывод относится к сохранности/использованию выученного |
| Сначала починить production и все tests, затем пробовать | Изолированный опыт; production transfer выбирается после фактического результата |
| Результаты писать под protected `docs/plans/replanning-possible-diffs` | Для этой сессии запрещено. Новый package назначает собственный `results/<ID>/`; старый адрес не используется |
| После положительного измерения автоматически merge | Нет. Пользователь выбирает перенос, после чего возникает отдельная полная цепь изменений |

Особенно важен [старый quality guide][old-quality-guide], где kNN/scorer/RegexDictionaryLookup исключаются по типу реализации. Его полезные требования собственных ответов и честного baseline сохранены; механический запрет learned memory не перенесён. То же относится к конфликтующему accuracy gate в measurement guidance и автоматическому merge в promptops.

## Учёт обновления, уже внесённого 18 октября

[required-changes][required] фиксирует разрешённое пользователем сохранение `phrase_mappings` и результат ingress 11/24 → 19/24 при росте ошибочного принятия 1 → 3. Текущий код действительно передаёт поле из [loader][translation-loader] в [ExecutorOutput][translation-continue]. Пакет использует это как baseline и не создаёт повторную работу с тем же названием.

N60 и отдельная N-KU-01 на head 62bcc уже содержат малые постановки и новые правила. В N-KU-01 уже открыто написано, что при k=3 эффект majority меток не доказывает качество structural distance. Это существующая честная граница, а не обнаруженный нами скрытый обман. Остаточный текст старых шаблонов не отменяет их новых верхних правил.

## Что здесь не сделано по существу поручения

Не изменены навыки и не создана новая политика для других задач. Не выполнен прежний полный product test gate, не выбран новый production recipient и не объединены все experiments в обязательный pipeline. Эти действия не нужны для документного аудита и противоречили бы просьбе сначала получить маленькое доказательство эффекта. Неопределённые или недоступные данные обозначены явно, а не заполнены вымышленными метриками.

[machine-notes]: https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/dpo%20legal%20build/03-ilearningmachine-pattern-notes.md
[old-quality-guide]: https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/dpo%20legal%20build/active-codex-execution-guide-03-how-to-measure-quality.md
[required]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/required-changes.md
[rp01]: https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/SOURCE-AND-ARCHITECTURE.md
[translation-continue]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/27.TranslationDomain.Tools/Program.cs#L4682-L4703
[translation-loader]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/27.TranslationDomain.Tools/Program.cs#L4442-L4470
