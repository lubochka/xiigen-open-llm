# Проверка пакета и закрытие замечаний

**Граница этого снимка:** ниже описано объединение40 ID до параллельного planningd7. Финальное состояние —42 ID,7 завершённых,35 будущих; порядок и новые NEXT заданы в [последнем дополнении](12-concurrent-planning-d7.md). Старое FROZEN→ATTN из этого снимка не применяется.

Это статическая проверка документов, источников и написанного в приложениях экспериментального кода. Она не означает успешную сборку или выполненное обучение. Пакет сохраняет исходные review-отчёты, включая замечания до исправлений; текущий статус дан здесь и в поздней сверке.

| Область | Найденная проблема | Исправление и граница |
|---|---|---|
| JSON host: собственный код | Проверка только исходного продукта не доказывала, какая arm-сборка исполняется | В authored host проверяются собственная assembly/PDB, девять arm-source files и Program.cs, фактические runtime DLL и загруженные зависимости. Parent Directory.Build imports отключены до SDK. |
| JSON host: запуск | Несколько команд могли допускать продолжение после ошибки | Единственный run.py задаёт шесть последовательных шагов, timeout, receipts и остановку при первом nonzero; нет B после failed A. |
| JSON: качество и gold | Не хватало полной четырёхсостоянийной панели и границы observer | Comparator сохраняет A0/A1/B0/B1, ΔA/ΔB/разность дельт и buckets; gold читает post-output comparator. State mismatch сохраняет наблюдения и ограничивает атрибуцию. |
| Учебное окно | Общее исключение «сериализации» могло убрать настоящий native учебный труд | В контракте оставлены parsing, guards, штатные exports/publication/serialization, входящие в заданный Train/StepVector. Исключаются только setup и добавленные observer/report операции вне этого окна. |
| ATTN observer | Чтение logits и вычисление метрик могли смешиваться с diagnostic forward cost | Forward окно содержит только назначенные вызовы; ToArray, NLL и обработка результатов выполняются после него. Бюджет не увеличен. Rank tie-break определён. |
| KN reader | На core827 cost-window окружал Train, хотя фактор меняет reader | Исходный точный overlay сохранён как история; в core6e3 нужный readerCosts уже внесён. Второй observer не назначен. |
| NEXT и позиции | Параллельные пакеты содержали две очереди по 38 ID, а LF-03 уже завершился | Объединение 40 ID, done1–7, NEXT=N63-INGRESS-LENGTH. Peer FROZEN→ATTN и собственные37→38→39 заданы буквально; все pending output roots уникальны. |
| E-OPT / LF-03 | Слишком сильная причинная трактовка и неполное описание provenance | Сохранены measured gains и regressions, отделены state receipts от historical binary custody; учтён опубликованный EOPT erratum. LF03 добавлен кандидатом, без rerun. |
| Материалы доказательств | Служебный manifest содержал временные подписанные download URLs | Такой manifest исключён до публикации. Публикуются только source paths, immutable refs, hashes, extracted/recomputed evidence. |

Исходные независимые чтения: [история и новые факторы](reviews/historical-independent-review.md), [механизмы и JSON](reviews/mechanisms-independent-review.md), [каталог](reviews/catalog-independent-review.md), [EOPT source](reviews/EOPT-independent-source-review.md). Адресный повторный review исправленного JSON: [закрытие R1–R6](reviews/JSON-final-static-review.md). Эти отчёты имеют собственные временные границы: упоминание позиций 35–38 и NEXT LF-03 в раннем review не является текущей очередью.

Последняя peer-дедупликация и exact metadata: [eaed review](reviews/planning-eaed-peer-delta.md), [JSON](evidence/planning-eaed-peer-delta.json). Поздний core readback: [6e3 review](reviews/core6e3-late-delta.md), [JSON](evidence/core6e3-late-delta.json). Для назначения исполнителю действуют только текущие [реестр](04-execution-registry.md), [карта выходов](output-path-map.json), карточка и [контракт](05-execution-contract.md).

Проверка публикационного состава включает JSON parsing, уникальность 40 ID и 33 roots, согласованность NEXT, отсутствие LF03 среди будущих назначений, относительные ссылки, отсутствие подписанных URLs и ограничение изменяемых путей папкой possibleDifs. Старые опыты и raw сохраняются. Чтение/пересчёт сохранённых файлов не является новым экспериментом.

Финальная адресная проверка объединённой очереди и переходов: [итоговый review](reviews/final-merged-static-review.md). Существенных оставшихся конфликтов в проверенной области не найдено; build/runtime этим не аттестованы.
