# Сводка: source-license packet / provenance repair

Run: `quality-controls-source-license-packet-20260618-082436`

## Итог

Локальный поиск не нашел accepted source-license packet или explicit permissive/open derived source packet для текущих 12 scenarios / 54 pair rows.

## Счётчики

- Source/doc/hash coverage: 12/12 scenarios, 54/54 pair rows.
- License proven: 0/12 scenarios, 0/54 pair rows.
- Common training allowed: 0/12 scenarios, 0/54 pair rows.
- USER_ADAPTIVE_ONLY route: 7/12 scenarios, 21/54 pair rows.
- EVIDENCE_ONLY route: 5/12 scenarios, 33/54 pair rows.
- QUARANTINE route: 0/12 scenarios, 0/54 pair rows.
- Train/held-out split: 0 train / 0 held-out.
- Strict QUALITY-03: NOT READY.

## Что изменилось относительно прошлого quarantine

Прошлый статус `QUARANTINE_LICENSE_NOT_PROVEN` больше не смешивает разные безопасные non-common маршруты. Rows с private/user-local источниками вынесены в future `USER_ADAPTIVE_ONLY`; project-local/control rows вынесены в `EVIDENCE_ONLY`. Это не разрешает common training и не разрешает training сейчас.

## Следующее безопасное действие

`QUALITY-CONTROLS-SOURCE-LICENSE-PACKET-FILL-OR-DERIVED-PERMISSIVE-SOURCE-REVIEW`: заполнить source-license packet точными локальными license/ref/hash и approval evidence или создать новые derived universal rows из explicit permissive/open источников, затем заново прогнать source-license review, common-training classification, source-disjoint split и только потом strict QUALITY-03.
