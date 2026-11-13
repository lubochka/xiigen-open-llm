---
title: "91. Дифференциальная цепь до сигнатуры и S–A motor"
source_pdf_pages: [328, 330]
book_pages: [292, 294]
supplementary_source_pdf_pages: [302]
batch: "007"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 91. Дифференциальная цепь до сигнатуры и S–A motor

[Индекс библиотеки](../README.md) · [Proposition 9.16](../source/0325-0329-ch09-headwise-recursion-and-score-motors.md#proposition-9-16) · [Proposition 9.17](../source/0325-0329-ch09-headwise-recursion-and-score-motors.md#proposition-9-17) · [Предыдущая карточка](90-block-synthesis-depth-recursion-and-residual-bounds.md) · [Следующая карточка](92-sa-population-maps-and-local-gradient-rank.md)

## От параметров к изменению наблюдаемой сигнатуры

В §9.14 используется $M=W_QW_K^\top$ и немасштабированная запись $L=XMX^\top$. Дифференциалы равны

$$
dM=dW_QW_K^\top+W_QdW_K^\top,
$$

$$
dL=dXMX^\top+XdMX^\top+XMdX^\top.
$$

Если $X$ фиксирован, остаётся только средний член. Далее вариация проходит через row-softmax и отображение сигнатуры:

$$
d\Sigma
=D\Sigma(S)\,D\operatorname{softmax}_L
\left[X(dW_QW_K^\top+W_QdW_K^\top)X^\top\right].
$$

Это цепное правило, а не отдельная динамическая гипотеза. [Proposition 9.16 и proof](../source/0325-0329-ch09-headwise-recursion-and-score-motors.md#proposition-9-16).

Размерности: $X\in\mathbb R^{n\times d}$, $M,dM\in\mathbb R^{d\times d}$, $L,dL,S,dS\in\mathbb R^{n\times n}$. Если реализация использует logits $XMX^\top/\sqrt{d_k}$, их дифференциал также делится на $\sqrt{d_k}$. Масштаб нельзя потерять при переходе от §9.9 к соглашению §9.14.

При изменении более ранних слоёв $dX$ обычно не равен нулю. Поэтому фиксированный-input bound описывает локальную QK-вариацию; он не является полной производной сетевой сигнатуры по всем параметрам.

## Требуется дифференцируемость всей сигнатуры

В книге условие сформулировано через дифференцируемую canonical projection на активной грани. Для $\Sigma(S)=(\Lambda(S),\rho(S))$ следует отдельно проверить:

- гладкость самой проекции на выбранном участке;
- единственность и локальную гладкость правила выбора barycentric coefficients $\Lambda$;
- существование производной residual distance $\rho$.

Даже гладкая проекция не гарантирует гладкость нормы остатка в точке $\rho=0$. В простейшем редакторском примере проекции на единственную точку остаток равен $\|S-S_0\|$, недифференцируемый при $S=S_0$. Это показывает недостаточность одного условия о проекции; пример относится к ограниченному словарю и не утверждает, что любая точка полного canonical hull недифференцируема.

При смене активной грани или неоднозначном tie-break обычный Jacobian может отсутствовать. Тогда нужны направленные производные или иной явно заданный generalized differential; source-формулу нельзя безусловно применять через такой переход. См. также [карточку 70](70-training-paths-and-coupling-observables.md).

## Forward variation и backward covector — разные объекты

$dM$ задаёт выбранное изменение параметров, а $G_M=\partial L_m/\partial M$ — чувствительность loss. Их связывает $dL_m=\langle G_M,dM\rangle_F$, но равенства $dM=G_M$ нет без отдельного выбора шага обучения.

Если $g_m^L$ — поднятый score-covector размера $n\times n$, то

$$
G_M=X^\top g_m^LX.
$$

При локальном одном query-вкладе $g_m^L$ имеет ненулевую строку $m$. При loss, уже распространившемся через следующие attention-блоки, нужно суммировать вклады всех активных query-строк. [§9.15](../source/0325-0329-ch09-headwise-recursion-and-score-motors.md#section-9-15).

## Точное ортогональное S–A разложение

Для $P_s(A)=(A+A^\top)/2$, $P_a(A)=(A-A^\top)/2$,

$$
G_M=G_s+G_a,\qquad G_s=P_s(G_M),\quad G_a=P_a(G_M),
\qquad \langle G_s,G_a\rangle_F=0.
$$

Это ортогональные проекции на пространства симметричных и кососимметричных матриц. Они дают чувствительность по допустимым вариациям соответствующих частей $M_s,M_a$. [Proposition 9.17](../source/0325-0329-ch09-headwise-recursion-and-score-motors.md#proposition-9-17).

Однако ортогональность в пространстве $M$ не делает независимыми обновления факторов $W_Q,W_K$. По цепному правилу $\nabla_{W_Q}L=G_MW_K$ и $\nabla_{W_K}L=G_M^\top W_Q$; геометрия факторизации и optimizer влияют на фактический $dM$. Последующий разбор двух потоков находится в [карточке 99](99-two-flow-gradient-and-factorised-training.md).

## Не смешивать энергии scores и нормы gradients

Автор вводит

$$
\Gamma_s(X)=\|XM_sX^\top\|_F^2,\qquad
\Gamma_a(X)=\|XM_aX^\top\|_F^2.
$$

В [§8.11.1 на PDF 302](../source/0301-0305-ch08-compatibility-bounds-and-signature-transport.md#section-8-11-1) $\Gamma$ обозначает $\|XM_aX^\top\|_F$ **без квадрата**, тогда как здесь $\Gamma_a$ — квадрат этой нормы. При одинаковых $X,M_a$ и масштабировании связь равна $\Gamma_a^{(9)}=(\Gamma^{(8)})^2$. Численные пороги и размерность коэффициентов нельзя переносить между этими обозначениями без пересчёта. Авторская фраза о той же directional energy сохранена в source.

Это **forward score energies**, зависящие от входа. Они не равны $\|G_s\|_F^2,\|G_a\|_F^2$, которые измеряют backward covector. Сигнатура, score-energy, parameter-side S–A balance и motor norm отвечают на разные вопросы. [Формулы PDF 329](../source/0325-0329-ch09-headwise-recursion-and-score-motors.md#display-9-sa-score-energies).

[Figure 9.8](../source/0330-0334-ch09-sa-maps-and-low-rank-motors.md#figure-9-8) сохранён с исходным наложением текста и буквальным \n. Его математический смысл восстанавливается из точных формул, а не из исправленной перерисовки.

