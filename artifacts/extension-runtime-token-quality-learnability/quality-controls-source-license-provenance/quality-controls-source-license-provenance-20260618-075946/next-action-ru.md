# Следующее действие

Текущая фаза `QUALITY-CONTROLS-SOURCE-LICENSE-PROVENANCE` завершила локальный поиск provenance настолько, насколько позволяют разрешённые файлы.

Итог:
- source/doc coverage остается полным: 12/12 scenarios и 54/54 pair rows;
- source hash посчитан для всех строк, где source_ref указывает на существующий локальный файл;
- license proven остается 0/12 и 0/54;
- common training allowed остается 0/12 и 0/54;
- train/held-out split не назначен, потому что eligible rows = 0;
- strict QUALITY-03 остается NOT READY.

Следующий bounded work order:
1. Для каждого source_ref/doc_id предоставить явный локальный `source_license_ref` с `license_hash`, либо отдельный redacted permissive/open derived source packet с approval и hash.
2. После этого повторно классифицировать строки: COMMON_ALLOWED, USER_ADAPTIVE_ONLY, EVIDENCE_ONLY или QUARANTINE.
3. Только для строк с достаточной source/license/provenance сделать source-disjoint train/held-out split без общего `source_ref`, `doc_id`, `source_hash`, source group, prompt hash, path/title и nearest-neighbor leakage.
4. Только после этого запускать отдельный strict QUALITY-03 controls rerun.

Не делать на следующем шаге без отдельной авторизации: training, model/checkpoint mutation, locator update, target promotion, live-host readiness claims, token savings claims или final readiness claims.
