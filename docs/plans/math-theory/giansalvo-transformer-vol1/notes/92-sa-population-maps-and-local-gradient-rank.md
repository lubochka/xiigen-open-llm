---
title: "92. Глобальные S–A карты, границы predictor и локальный ранг"
source_pdf_pages: [330, 334]
book_pages: [294, 298]
batch: "007"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 92. Глобальные S–A карты, границы predictor и локальный ранг

[Индекс библиотеки](../README.md) · [§9.16](../source/0330-0334-ch09-sa-maps-and-low-rank-motors.md#section-9-16) · [§9.17](../source/0330-0334-ch09-sa-maps-and-low-rank-motors.md#section-9-17) · [Предыдущая карточка](91-signature-differential-chain-and-sa-motors.md) · [Следующая карточка](93-batch-gradient-rank-and-motor-observables.md)

## Какие величины составляют глобальную картину

[S–A balance, Definition 9.18](../source/0330-0334-ch09-sa-maps-and-low-rank-motors.md#definition-9-18), задаётся в пространстве параметров:

$$
\alpha_{\ell,h}
=\frac{\|M_a^{(\ell,h)}\|_F^2}
{\|M_s^{(\ell,h)}\|_F^2+\|M_a^{(\ell,h)}\|_F^2}\in[0,1].
$$

При нулевом знаменателе автор назначает $\alpha=0$. Этот специальный случай означает $M=0$, а не наличие ненулевого симметричного механизма. Параметрическая доля не зависит от входа, тогда как $\Gamma_s(X),\Gamma_a(X)$ зависят от того, какие направления вход активирует.

Глобальная карта $\mathcal A_{\mathrm{SA}}=(\alpha_{\ell,h})_{\ell,h}$ дополняется score-energy картами и сигнатурами. В [Definition 9.21](../source/0330-0334-ch09-sa-maps-and-low-rank-motors.md#definition-9-21) они собраны в

$$
\mathfrak D_\ell=
\bigl((\alpha_{\ell,h})_h,(\Gamma_s^{(\ell,h)})_h,
(\Gamma_a^{(\ell,h)})_h,\mu_\ell,(\rho_{\ell,h})_h\bigr).
$$

Такое совместное описание различает похожие attention signatures с разными score-side механизмами и похожие S–A доли с разными остатками. Само совпадение одной скалярной доли не доказывает функциональную избыточность.

Внутренний QK-gauge сохраняет $M$, а перестановка голов переставляет соответствующие ячейки. Это заявленная устойчивость описания; она не превращает номер головы в межслойную идентичность. [PDF 330](../source/0330-0334-ch09-sa-maps-and-low-rank-motors.md#pdf-0330).

## Каков статус карт BERT/SARS

[Figures 9.9–9.12](../source/0330-0334-ch09-sa-maps-and-low-rank-motors.md#figure-9-9) перенесены как исходные изображения: непрерывная и грубая карты, послойные энергии/остаток и иллюстрация связи с signature mass.

При этом [Remark 9.19](../source/0330-0334-ch09-sa-maps-and-low-rank-motors.md#remark-9-19) прямо описывает поставляемые таблицы как **reproducible template**, который следует заполнить точными эмпирическими значениями, когда появятся checkpoint-level arrays. Упоминание папки experiment/ в книге не означает, что эти массивы приложены к данному PDF или проверены библиотекой.

Поэтому рисунки нельзя использовать как уже подтверждённые измерения конкретного checkpoint, как benchmark улучшения или как основание для численных порогов pruning. Грубые категории symmetric/mixed/antisymmetric не заменяют исходную непрерывную долю и требуют явного правила порогов.

## Почему Proposition 9.20 не даёт общего монотонного закона

Источник печатает схематическое утверждение

$$
\Delta\Lambda_{\mathrm{dir}}^{(\ell,h)}
\ge c\,\Delta\alpha_{\ell,h}-C\,\Delta\rho_{\ell,h},
\qquad c,C\ge0,
$$

при локальной устойчивости softmax и дифференцируемости canonical projection. [Утверждение и proof sketch](../source/0330-0334-ch09-sa-maps-and-low-rank-motors.md#proposition-9-20).

**Редакторская оценка:** перечисленных условий недостаточно для положительного знака directional displacement. Гладкость обеспечивает существование линейного отклика и локальный контроль его величины, но не ориентацию отклика в выбранной barycentric coordinate.

В частности, при фиксированном $X$ могут существовать ненулевые кососимметричные вариации $K$ с $XKX^\top=0$. Добавление такой компоненты к $M_a$ способно увеличить parameter-side $\alpha$, оставляя logits, softmax и сигнатуру неизменными. Простой случай — вход ранга один, для которого $XKX^\top=0$ при любом кососимметричном $K$. Отсюда никакая строго положительная нижняя чувствительность к $\alpha$ не следует без условия наблюдаемости входом. Разрешённое автором $c=0$ не доказывает обещанный положительный сдвиг.

Кроме наблюдаемости потребовалось бы условие, что составной directional derivative направлен в нужный сектор сигнатуры. Контроль symmetric energy и residual сам по себе этого не даёт. В формуле также необходимо определить агрегат $\Lambda_{\mathrm{dir}}$ и смысл знакового $\Delta\rho$: обычная оценка ошибки использует модуль или норму, а уменьшение signed residual меняет правую часть в обратную сторону.

Поэтому в проектной библиотеке это сохранённое авторское условно-схематическое утверждение с неполным обоснованием, а не готовая гарантия: «рост antisymmetric fraction заставляет расти directional mass».

## Что действительно доказывается о локальном ранге

Для affine map $u=xW+b$:

$$
G_W=x^\top\delta,\qquad \delta=\partial L/\partial u.
$$

[Proposition 9.22](../source/0330-0334-ch09-sa-maps-and-low-rank-motors.md#proposition-9-22) корректно даёт $\operatorname{rank}G_W\le1$ и ранг суммы $r$ таких вкладов не более $r$.

Для $B$ masked-position строк decoder-градиент складывается в

$$
G_{W_D}=A^\top\Delta,\qquad
\operatorname{rank}G_{W_D}
\le\min(\operatorname{rank}A,\operatorname{rank}\Delta)
\le\min(B,d_{\mathrm{model}},D).
$$

Это начало [Theorem 9.23 на PDF 334](../source/0330-0334-ch09-sa-maps-and-low-rank-motors.md#theorem-9-23); дополнительное утверждение и доказательство находятся на [PDF 335](../source/0335-0341-ch09-motor-rank-anisotropy-and-bounds.md#theorem-9-23-continued), а их условия разобраны в [карточке 93](93-batch-gradient-rank-and-motor-observables.md).

$B$ здесь считает строки masked activations, а не автоматически число исходных последовательностей. Усреднение loss меняет общий масштаб, но не ранг. Bound может быть тривиальным при большом $B$; суммирование всех использований общих параметров и преобразование градиента optimizer не обязаны сохранять малый ранг отдельного вклада. Из этой алгебры не следует гарантированное ускорение обучения.

