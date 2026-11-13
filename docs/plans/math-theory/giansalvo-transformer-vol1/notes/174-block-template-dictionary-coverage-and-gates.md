---
title: "174. Шаблон блока: dictionary coverage, static sufficiency и диагностические gates"
source_pdf_pages: [509, 511]
book_pages: [473, 475]
batch: "011"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 174. Шаблон блока: dictionary coverage, static sufficiency и диагностические gates

[Индекс](../README.md)

## Формула и её уровень определённости

[Definition 14.7](../source/0505-0511-ch14-motivation-design-and-block.md#definition-14-7) задаёт

$$
X_{\ell+1}=\operatorname{LN}\bigl(X_\ell+
G_{\mathrm{att}}R_{\mathrm{att}}(X_\ell)+G_{\mathrm{ff}}R_{\mathrm{ff}}(X_\ell)\bigr),
\qquad R_{\mathrm{att}}=R_S+R_A+R_{\mathrm{mix}}.
$$

Это широкая спецификация: формулы сами по себе не определяют maps $R_S,R_A,R_{\mathrm{mix}}$, размерность gates или способ вычисления diagnostics. При $X\in\mathbb R^{n\times d}$ левое матричное умножение $GR(X)$ требует согласованного token-space оператора; feature-space gate действует справа. Скаляр или явно заданное покомпонентное умножение также возможны, но эти варианты нужно различать.

В [§14.3.1](../source/0505-0511-ch14-motivation-design-and-block.md#section-14-3-1) heads получают hard или soft роли $S,A,M$. Для hard partition слово «partition» предполагает попарную непересекаемость всех трёх множеств; в напечатанной строке явно указано лишь $\mathcal H_S\cap\mathcal H_A=\varnothing$. Для вероятностного soft assignment требуется дополнительно $\pi_{c,h}\ge0$, а не только сумма компонент 1. Эти условия полезно фиксировать при реализации.

## Здесь dictionary обозначает другой объект

[§14.2.3](../source/0505-0511-ch14-motivation-design-and-block.md#section-14-2-3) называет $K^\star$ эмпирическим covering number. Атомы в [§14.3.2](../source/0505-0511-ch14-motivation-design-and-block.md#section-14-3-2) — representatives $a_{\ell,k}$ в пространстве сигнатур.

В главе 13 dictionary состоял из skew-матриц $\Gamma_k\in\mathfrak{so}(d)$ и обсуждалось число межголовных block atoms. Это разные пространства и разные задачи. Равенство $K^\star=\binom H2$, даже если бы оно было доказано для прежней конструкции, не определяло бы covering number эмпирического распределения. Прежние ошибки span/rank отдельно разобраны в [164](164-dictionary-size-and-training-criterion.md).

Нужно сохранять: пространство сигнатур, metric, reference sample, веса наблюдений, допустимые центры и tolerance. Без них covering number не имеет единственного численного значения.

## Когда coverage residual тождественно равен нулю

Источник вводит

$$
\rho_\ell=\int\min_{h\in\mathcal H_\ell}
\operatorname{dist}(\sigma,\Sigma_{\ell,h})^2\,d\mu_\ell(\sigma).
$$

Если понимать «empirical signature geography» как меру на **тех же текущих heads**,

$$
\mu_\ell=\frac1H\sum_{j=1}^H\delta_{\Sigma_{\ell,j}},
$$

то для каждого слагаемого можно выбрать $h=j$ и получить $\rho_\ell=0$. Это верно даже при полном совпадении всех heads. Поэтому такая интерпретация не измеряет недостающее покрытие.

Нетривиальное значение получается, например, если reference measure включает состояния на разных входах/checkpoints, а центры являются ограниченным набором representatives, либо если reference geography задана независимо от текущего набора. Это возможные уточнения, а не дописанное авторское определение. Crowding может отдельно обнаружить дубликаты, но не делает вырожденный coverage term информативным.

## Static sufficiency — совместный диагностический порог

[Definition 14.8](../source/0505-0511-ch14-motivation-design-and-block.md#definition-14-8) требует одновременно

$$
\rho_\ell\le\rho^\star,
\qquad C_{\mathrm{orb}}^{(\ell)}\le C^\star,
\qquad\Gamma_{\mathrm{res}}^{(\ell)}\le\Gamma^\star.
$$

Автор прямо отличает это от оптимальности. Порог относится к выбранным данным и diagnostic resolution. Его нельзя автоматически переносить на task loss, новое распределение или минимальный бюджет. Ненормированная crowding sum зависит от числа голов; energy зависит от масштаба операторов; coverage зависит от metric. Три tolerance поэтому требуют самостоятельной калибровки.

## Gate не становится защитой только от присутствия alpha

[Пример gate](../source/0505-0511-ch14-motivation-design-and-block.md#display-0511-sigmoid-gate) имеет вид

$$
g_h=\sigma(b_0+b_1V_{\mathrm{marg},h}-b_2C_{\mathrm{orb},h}
+b_3\Gamma_{\mathrm{res},h}+b_4\alpha_h).
$$

Если coefficients обучаются без ограничений, знак влияния diagnostics не гарантирован. Даже при положительных $b_1,b_4$ отрицательный intercept или большой crowding term может сделать gate сколь угодно малым. Для формальной защиты важной головы нужна дополнительная нижняя граница или отдельное правило допуска.

Есть и вопрос времени измерения: marginal value и residual могут зависеть от gates текущего блока. Если gate немедленно зависит от этих же результатов, получается неявное уравнение, а не обычный последовательный forward. Источник допускает moving averages и checkpoint-level estimates; их использование с явным временем обновления устраняет эту неоднозначность. Замораживание input-dependent gate в одну константу на inference обычно меняет функцию, поэтому требует отдельной проверки.

[Figure 14.1](../source/0505-0511-ch14-motivation-design-and-block.md#figure-14-1) полезна как схема связей diagnostics и channels. Для точной реализации нужно также согласовать порядок residual, attention, FFN и LayerNorm: обнаруженное расхождение со стандартным pre-LN блоком разобрано в [177](177-prenorm-recovery-and-the-postnorm-template.md).
