---
title: "71. Каноническая замена, точность словаря и проверка границы output drift"
source_pdf_pages: [280, 282]
book_pages: [244, 246]
batch: "006"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 71. Каноническая замена, точность словаря и проверка границы output drift

[Индекс библиотеки](../README.md) · [Исходник §7.40](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#section-7-40) · [Метрика и transport](69-hellinger-wasserstein-layer-geometry.md)

## Как геометрическое объяснение проверяется вмешательством

Definition 7.43 заменяет attention map её ближайшей точкой фиксированного canonical hull:

$$
\widehat S_{\ell,h}(X)=\Pi_8(S_{\ell,h}(X)).
$$

Все остальные компоненты модели сохраняются. Полученный $M_c$ сравнивается с исходным $M$. Это проверяет, насколько словарь сохраняет функцию модели, когда из внимания удалена residual part. [Definition 7.43](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#definition-7-43), [Figure 7.18](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#figure-7-18).

Локально

$$
S-\Pi_8(S)=R_8(S),\qquad
\|R_8(S)\|_F=\rho_8(S).
$$

Это точная ошибка приближения **матрицы внимания**. Ошибка задачи дополнительно зависит от values, output projection, следующих слоёв и readout.

Для измерения надо различать замену одной головы, нескольких голов и всей сети. При выполнении всей модифицированной сети более ранние замены меняют hidden states следующих слоёв. Вычисление проекции на фактических новых states и подстановка cached attention maps исходного прохода — разные вмешательства. Эта операционная деталь не расписана алгоритмом в Definition 7.43 и должна быть зафиксирована при реализации.

## Ошибка Proposition 7.44: atom не обязательно является вершиной

Автор пишет, что каждый фиксированный $D_a\in\mathcal D_8(n)$ является vertex $K_8(n)$. Это неверно для полного словаря с closed atom и всеми directional atoms. Обозначим

$$
D_j=\mathbf1e_j^\top,\qquad
U=\frac1n\mathbf1\mathbf1^\top.
$$

При $n\ge2$ имеем

$$
U=\frac1n\sum_{j=1}^{n}D_j.
$$

Directional matrices различны, а closed atom $U$ является их нетривиальной convex mixture. Следовательно, $U$ не extreme point их hull. Это явный редакторский контрпример к первой фразе proof. [Proposition 7.44](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#proposition-7-44).

Корректные операции следует различать:

| Операция | Что подставляется | Что нужно дополнительно |
|---|---|---|
| Fixed atom replacement | Один заранее выбранный $D_a$ для всех входов | Atom может быть внутренней точкой/точкой грани hull |
| Vertex replacement | Фиксированная или выбранная extreme point hull | Проверка, что это действительно vertex |
| Nearest canonical projection | $\Pi_8(S)$ для текущего $S$ | Зафиксированные hull, norm и solver |

Fixed atom replacement не является автоматически частным случаем **правила ближайшей проекции**. Например, если $S=D_b\in K_8$, то $\Pi_8(S)=D_b$; выбор другого $D_a$ уже не совпадает с этим результатом. Обе операции входят в более широкий класс canonical interventions, но исходная Definition 7.43 задаёт конкретное правило.

## Что действительно следует из локальной ошибки

При фиксированном представлении $Z$ и value-output операторе $N$:

$$
\|(S-\widehat S)ZN\|_F
\le\|S-\widehat S\|_F\,\|ZN\|_{\rm op}
=\rho_8(S)\|ZN\|_{\rm op}.
$$

Это редакторское пояснение того, откуда берётся local head-output constant. Небольшой residual может дать значимую ошибку при большом $\|ZN\|_{\rm op}$ или усилении в следующих блоках. Поэтому одна геометрическая coverage без масштабов downstream computation не гарантирует сохранения ответа.

## Proposition 7.45: общий product требует уточнения

В источнике напечатано

$$
\|M(X)-M_c(X)\|_F
\le
\left(\prod_{r=1}^{L}J_r(X)\right)
\sum_{\ell,h}a_{\ell,h}(X)\rho_8(S_{\ell,h}(X)).
$$

Автор связывает $J_r$ с распространением возмущений по слоям, а $a_{\ell,h}$ — с локальной head-output ошибкой. Треугольное неравенство действительно допускает безопасное **суммирование** ошибок, но не даёт общий product всех $J_r$ без дополнительного условия. [Proposition 7.45 и proof](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#proposition-7-45).

Чтобы увидеть проблему, запишем обычную редакторскую рекурсию:

$$
e_\ell\le J_\ell e_{\ell-1}+\delta_\ell,\qquad e_0=0,
$$

где $\delta_\ell$ — ошибка, внесённая именно на слое $\ell$. Раскрытие даёт

$$
e_L\le
\sum_{\ell=1}^{L}
\left(\prod_{r=\ell+1}^{L}J_r\right)\delta_\ell.
$$

Ошибка последнего слоя имеет пустой product, равный 1. Более ранние слои её не уменьшают: она ещё не существовала, когда вход проходил эти слои.

Например, при $L=2$, $\delta_1=0$, $\delta_2>0$, $J_1=0$ и $J_2=1$ допустима конечная ошибка $e_2=\delta_2$. Общий product $J_1J_2$ равен нулю и не может её ограничивать. Это контрпример к использованному правилу распространения при заявленных общих условиях, а не новый эмпирический запуск Transformer.

При согласованно ограниченных локальных ошибках

$$
\delta_\ell
\le\sum_h a_{\ell,h}\rho_8(S_{\ell,h})
$$

получается безопасная редакторская форма

$$
e_L\le
\sum_{\ell=1}^{L}
\left(\prod_{r=\ell+1}^{L}J_r\right)
\sum_h a_{\ell,h}\rho_8(S_{\ell,h}).
$$

Константы и residuals должны относиться к одной доказанной схеме сравнения — например, подходящим hybrid states или равномерным границам вдоль интерполяции. Нельзя смешивать local residual из одного состояния с Jacobian bounds, установленными только для другого.

Если все $J_r\ge1$, каждый tail product не превосходит полного product, и авторская более грубая форма следует. Другой безопасный вариант — заменить $J_r$ на $\max(1,J_r)$. Источник не указывает это условие явно.

## Когда возможно суммирование по квадратам

Без дополнительных ограничений aligned errors складываются по нормам: для одинаковых векторов суммарная норма растёт линейно с их количеством. Чтобы заменить weighted $\ell^1$ на weighted $\ell^2$, нужны условия на **уже распространённые** ошибки, например

$$
\left\|\sum_j z_j\right\|^2
\le C\sum_j\|z_j\|^2.
$$

Это условие quasi-orthogonality/coherence, а не автоматическое следствие разных номеров heads. Source правильно обозначает refined bound как conditional. Ссылка на Khintchine-type reasoning не задаёт без дополнительных случайных предположений детерминированную гарантию для каждого входа.

## Fidelity и потерянное направление residual

§7.40.1 предлагает

$$
\eta(S)=1-\frac{\rho_8(S)}{\|S\|_F}.
$$

Матрица projection плюс число $\rho$ не восстанавливают направление $R_8$. Поэтому это lossy compression. Вся пара $(\Pi_8(S),\rho_8(S))$ содержит больше информации, чем только family signature $(\Lambda,\rho)$, если агрегация atom weights теряет расположение конкретных atoms. [§7.40.1](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#section-7-40-1).

Для row-stochastic $S$ знаменатель ненулевой. Если $U$ входит в hull, то

$$
0\le\rho_8(S)\le\|S-U\|_F,\qquad
\|S-U\|_F^2=\|S\|_F^2-1.
$$

Следовательно, для такого словаря $0\le\eta(S)\le1$. При произвольном ограниченном словаре без $U$ эта гарантия требует повторной проверки: например, для $S=U$ и hull из единственной permutation matrix при $n\ge3$ отношение residual к $\|S\|_F$ может превышать 1.

## Что остаётся исследовательской гипотезой

Conjecture 7.46 связывает coverage с устойчивостью к canonical replacement **в тренде на семействе моделей**, а не как монотонную теорему для любой пары моделей. Масштабы value/readout и propagation constants объясняют, почему одинаковая coverage может сопровождаться разным output drift. [Conjecture 7.46](../source/0277-0284-ch07-metric-geometry-and-canonical-replacement.md#conjecture-7-46).

Предложенный penalty

$$
\mathcal L_{\rm task}
+\beta\sum_{\ell,h}\mathbb E_X\rho_8(S_{\ell,h}(X))^2
$$

— design idea. Он может притягивать внимание к выбранному hull, но может также подавить полезный механизм вне словаря. Здесь не доказаны улучшение обобщения, сокращение sample budget или ускорение обучения. Эти результаты потребуют отдельных task measurements.

