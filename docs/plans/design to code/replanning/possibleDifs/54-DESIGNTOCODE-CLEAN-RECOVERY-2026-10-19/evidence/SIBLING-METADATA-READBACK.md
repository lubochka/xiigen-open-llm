# Исторические sibling subtree SHA: первичный metadata readback

Repo: lubochka/llm_mvp_core. Retrieval: GitHub contents API, 2026-10-19. Значения git tree SHA, не hashes артефактов обучения. Никаких download URL/token metadata в этом файле нет.

| Ref label | Exact commit | Path | Git tree SHA |
|---|---|---|---|
| clean | `e346b88a3cc1fcce48aefa626bfc7b00509eefd6` | `12.SemanticCompiler` | `cae2d9355e803e5b46ec52b7d4fe0f00b524af0f` |
| clean | `e346b88a3cc1fcce48aefa626bfc7b00509eefd6` | `34.Experiments` | `2efeec6d4f5911874b33c548593b7e16fc5550e3` |
| clean | `e346b88a3cc1fcce48aefa626bfc7b00509eefd6` | `35.ExperimentsV2` | `e1e8f881d6818bb926c806e3ea8fef5b2585528d` |
| DesignToCode | `8f20ceb8a9bfbadab2ad309226c47dd76e714548` | `12.SemanticCompiler` | `66eb13b71f12d4881b92fe551e24bdd62fcc74a7` |
| DesignToCode | `8f20ceb8a9bfbadab2ad309226c47dd76e714548` | `34.Experiments` | `82ffd0208fe99ecf1abc280598ecb32b81b67632` |
| DesignToCode | `8f20ceb8a9bfbadab2ad309226c47dd76e714548` | `35.ExperimentsV2` | `aa5c7d5d6f3a1ecb1f221755dae61f03141d62da` |
| language_translate | `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` | `12.SemanticCompiler` | `66eb13b71f12d4881b92fe551e24bdd62fcc74a7` |
| language_translate | `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` | `34.Experiments` | `82ffd0208fe99ecf1abc280598ecb32b81b67632` |
| language_translate | `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` | `35.ExperimentsV2` | `aa5c7d5d6f3a1ecb1f221755dae61f03141d62da` |
| external_domains | `3036a01acec938f0b83fa4ffbda26202cc35d639` | `12.SemanticCompiler` | `66eb13b71f12d4881b92fe551e24bdd62fcc74a7` |
| external_domains | `3036a01acec938f0b83fa4ffbda26202cc35d639` | `34.Experiments` | `82ffd0208fe99ecf1abc280598ecb32b81b67632` |
| external_domains | `3036a01acec938f0b83fa4ffbda26202cc35d639` | `35.ExperimentsV2` | `aa5c7d5d6f3a1ecb1f221755dae61f03141d62da` |

Три sibling ветки совпадают между собой для этих трёх subtrees. Clean имеет другие tree SHA. Это не доказывает различие алгоритмов: адресный diff трёх исходников E09StructuralKnn.cs/E25PatternClassifier.cs/E43ReplayBuffer.cs при одинаковом сохранении UTF-8 показал только пять строк AGPL/SPDX-заголовка в clean; соответствующие три raw May01 JSON совпадают. Проверка не распространяется на всё содержимое clean subtrees.

API source для каждой строки: `https://api.github.com/repos/lubochka/llm_mvp_core/contents?ref=<Exact commit>`.

