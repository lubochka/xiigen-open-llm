> **Финальный статус после свежего planning9d311:** прочитаны ещё20 Markdown, SKNN признан дубликатом AST и оставлен только для review; новый отдельный фактор один — BPE. Ранние receipts ниже сохраняют свои pins и не отменяют эту дедупликацию. [Согласование](18-concurrent-reconciliation.md).

# Источники, охват, новизна и границы проверки

Дата:20.10.2026. Все выводы привязаны к перечисленным объектам Git, а не к подвижному HEAD. Публикация добавит новые документы только в planning/possibleDifs; не изменит перечисленные источники.

## Закреплённые версии

| Источник | Commit / tree | Что проверялось |
|---|---|---|
| [core](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/README.md) | `883496bca18113bf37e0afe683c75b05a0075048` | Текущие source chains, experiment cards/results/skills |
| [planning](https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/README.md) | `6bc4b6fba8f14870f5dc4966cf5a344754962d66` | Предыдущие находки, RAG-навигация, RP01 и новизна |
| possibleDifs subtree | `5079e5f6505c79265e47c396febd68b72b8e42ef` | 1460 entries, 1313 файлов, 747 Markdown; tree не усечено |
| protected subtree | `41587e4a86b38928e4f4d642a8f1a0f2d35bfcf1` | 1392 entries, 1253 файлов, 334 Markdown; tree не усечено |
| Исходники сохранённого N62-R1 | `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39` | Это source pin запуска, а883496 — снимок, из которого прочитаны его результаты |

В локальном корпусе для этой версии пакета **747 planning Markdown и 334 protected Markdown**. Материализация файлов и поиск строк не означают ручного содержательного прочтения каждой строки. Все153 experiment cards проверены структурно; selected source chains и новые карточки проверены содержательно. 125 P-файлов остаются справочными семействами, не125 готовыми исполнимыми экспериментами. 470 matrix IDs не равны470 runtime-аттестациям.

Первый поиск новых гипотез охватывал747+172 Markdown; его квитанция сохранена. Затем получены все334 protected Markdown, проверено совпадение334/334 Git blob SHA и завершена адресная дедупликация по полному корпусу. Отдельные точные факторы двух новых карточек не найдены; overlap BPE уже известен и явно отделён от новизны objective experiment. [Финальный review](reviews/full-corpus-novelty-review.md), [полный readback receipt](evidence/protected-complete-materialization-receipt.json), [поисковые запросы и совпадения](evidence/protected-novelty-search-receipt.json). Большие JSON/CSV из прежних planning inventories не выдаются за новые измерения. [Пофайловый card index](card-review.json) сохраняет исходные Git blob SHA, URL и точные строки доказательств по каждому правилу; повторяющиеся цитаты опущены, источник доступен по URL. [Машинный scope](audit-index.json).

## RAG и исходный RP01

| Указатель | Записей | Граница |
|---|---:|---|
| [09.09.26WorkLeft-RAG-01-dpo-legal-build.ndjson](https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-RAG-01-dpo-legal-build.ndjson) | 201 | Навигационный RAG-индекс, не замена исходникам |
| [09.09.26WorkLeft-RAG-02-design-to-code.ndjson](https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-RAG-02-design-to-code.ndjson) | 140 | Навигационный RAG-индекс, не замена исходникам |
| [09.09.26WorkLeft-RAG-03-merge-and-branches.ndjson](https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-RAG-03-merge-and-branches.ndjson) | 33 | Навигационный RAG-индекс, не замена исходникам |
| [09.09.26WorkLeft-RAG-04-vs-vscode-continue.ndjson](https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-RAG-04-vs-vscode-continue.ndjson) | 30 | Навигационный RAG-индекс, не замена исходникам |

Из404 ссылок не делается утверждение о404 заново прочитанных исходниках. Содержательно использованы [алгоритм создания ДПО](https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/%D0%90%D0%BB%D0%B3%D0%BE%D1%80%D0%B8%D1%82%D0%BC%20%D1%81%D0%BE%D0%B7%D0%B4%D0%B0%D0%BD%D0%B8%D1%8F%20%D0%94%D0%9F%D0%9E.md), [integration training-into-work-program](https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-INTEGRATION-01-training-into-work-program.md), RP01 [README](https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/README.md), fast-learning contract, recovery/code audit, causal-diagnostic pack, source/architecture и learning-examination. Точные соответствия исходным вопросам — в [карте осей](14-requested-axis-crosswalk.md). RAG-требование отделено от факта исполнения кода.

## Три уровня числовых доказательств

1. **Фактический JSON прочитан:** N62-R1 A/B и исторические E09/E22/E25/E43 JSON. Суммы/дельты N62 пересчитаны из сохранённых полей, не новым learner run. Отдельно прочитаны первичные итоговые `scientific-result.json` C#/ENRU RP01 на историческом83590a и producer того же commit: это actual JSON payload, но не повторное независимое чтение всех вложенных score/grade/checkpoint файлов. [RP01 и точная граница readback](16-RP01-primary-readback.md).
2. **Первичный отчёт прочитан, LFS payload недоступен в полученном Git blob:** N-KU, frozen transpose, BPE no-match. Их положительные результаты сохранены как сведения этих отчётов, без ложного утверждения о независимом разборе всей raw полезной нагрузки. Raw-link с LFS pointer не маскируется под actual JSON.
3. **Аналитическое ожидание:** новые SKNN/BPE карточки, cosine reachability и арифметика k=3. Это выводы из source/заданных fixtures; measured success новых карточек отсутствует.

[measured-results.json](measured-results.json) отдельно маркирует каждый набор. Успех наблюдаемой полезной оси не требует accuracy, но причинная величина, переносимость и обобщение требуют собственных доказательств. Никаких новых прогонов ради этой публикации.

## Что новое, а что сохраняется как известное

| Деталь | Вердикт | Отличие от близкой существующей темы |
|---|---|---|
| SKNN-SIBLING-PREP-01 | После concurrent9d311: REFERENCE_ONLY, дубликат фактора существующего AST-SIB-CACHE-01 | Один scoped cache подготовки actual Train; P26.4 меняет использование SiblingCount в distance, N53/N57 — семантику позиции, KH-COST — двойной parse другого FileAction learner |
| BPE-REALIZED-MERGE-COUNT-01 | Новый отдельный objective experiment внутри уже известного BPE-семейства; ещё не запускался | Overlapping count уже известен. Новая часть — изолированное A/B иной цели выбора merges с заранее объявленным выигрышем/проигрышем профилей; weighted/no-match сохраняют исходную цель |
| N62-R1: GC+1 и неправомерная причинная формулировка | Свежая поправка к четвёртому сохранённому результату | Это корректировка доказательного вывода, не новая алгоритмическая гипотеза и не повод повторить опыт |
| Пять missing skill paths | Адресный handoff-пробел, добавлены пять prompts | Не объявляется отсутствием всех правил во всех153 карточках |
| KH-COST learner/ordinal; семь P54 exclusions | Ранее известные конфликты, подтверждены текущим чтением | В новом snapshot ещё не устранены в самом protected тексте; additive resolution сохраняет пользователя выше старых ограничений |
| Cosine beyond horizon | Подтверждённая достижимость, известная гипотеза | Existing clamp/unclamped experiment уже есть; новый ID не создаётся |
| Language keys, retokenization, correction visibility, unused siblings, replay capacity, Flow identity | Известные детали, source-check и experiment pointers сохранены | Совпадение нового названия не делает старый фактор новым |
| Q/K initialization scale | Недостаточно обоснованный lead; в очередь не включён | Нет показанного плохого attention-state, измерения эффекта или отличия от успешного пути; не выдаётся за defect |

Квитанции: [поиск SKNN](evidence/knn-sibling-novelty-receipt.json), [16 чтений соседних веток](evidence/knn-history-branch-source-receipt.json), [реальный E09 transfer commit](evidence/e09-transfer-commit-receipt.json). Проверка четырёх выбранных файлов на четырёх branches не равна поиску всех checkpoints и всех успешных запусков во всех ветках. Абсолютная новизна по неизвестным веткам не заявляется.

## Проверка новых документов

Новые карточки раздельно проверены другим агентом по настоящим native bodies, счётчикам, readout, paths и семантике одного фактора. Замечания исправлены **в документации до будущего запуска**: SKNN resume/build imports/terminal scoring/warm parser; BPE stream rewind/build imports/no-replay/result classification. Root отдельно проверил fixture BPE и native Save/Load. [SKNN review](reviews/SKNN-document-review.md), [BPE review](reviews/BPE-document-review.md), [интеграционный review](reviews/integration-document-review.md).

Статус проверки — **DOCUMENT_REVIEWED, BUILD_AND_RUNTIME_UNVERIFIED**. Не было30/50/любого иного числа выдуманных прогонов. Не проверялись native сборка, установленный SDK, package access, checkpoint availability, фактические gains новых опытов. Последняя публикационная проверка касается только наличия файлов, ссылок, согласования реестра, данных и ограничения target path.

## Сохранённые ограничения

Новые build/train/inference calls:0. Изменения product source:0. Изменения protected:0. Удаления прошлых опытов:0. Текущее разрешение — документы в planning. Следующий ID существующей очереди05 N62-R2; её запуск отдельно не поручался. ROPE/AST/NG01 сохранены; BPE добавлен послеNG01; reader16 IDENTITY и32 KN-ROUTER стоят последними. Всего36строк/32pending; SKNN не отдельный запуск.

STOP/NEXT выше report/hash/review/commit. Кандидаты четырёх измеренно полезных изменений зарегистрированы в [02](02-learning-results-and-candidates.md); новые гипотезы не названы успешными без результатов. Полная продуктовая цепочка остаётся решением пользователя.


## Финальное расширение planning до9d311

Перед публикацией прочитаны20 новых Markdown пакета63; прежний747-file scope относится к6bc4, полный известный planning scope теперь767 Markdown. Обнаружен дубликатSKNN/AST и сохранён существующийcanonicalAST. ТолькоBPE остаётся новым отдельным фактором. [Полное согласование](18-concurrent-reconciliation.md), [сравнениеAST/SKNN](reviews/concurrent-SKNN-reconciliation.md), [единственная36-row очередь](05-execution-registry.md). Ранние source/novelty receipts сохраняют свой историческийpin; их ограниченный вердикт не выдаётся за отсутствие дубликата в позднем9d311.
