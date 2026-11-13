---
title: "106. Граница доказанных training claims и вступление главы 11"
source_pdf_pages: [364, 367]
book_pages: [328, 331]
batch: "008"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 106. Граница доказанных training claims и вступление главы 11

[Индекс библиотеки](../README.md) · [Предыдущая карточка](105-enriched-crowding-and-empirical-scope.md) · [Следующая карточка](107-signature-orbits-and-slack-tightening.md)

## Что завершено в главе 10

§10.17 аккуратно отделяет algebraic splitting, first-order response и canonical-pair calculation от более сильных утверждений. Forward invariance compatibility regime, training-time inequality propagation, particular orbit emergence и NTK/edge-of-stability interpretations требуют дополнительных hypotheses. Неизбежность разложения $M=M_s+M_a$ — линейная алгебра; неизбежность конкретного распределения голов после обучения — другая задача. [PDF 364, §10.17](../source/0361-0364-ch10-empirical-status-and-synthesis.md#section-10-17).

Результаты нужно читать с сохранёнными условиями: согласованный softmax scale, fixed-$X$ versus full training derivative, differentiability полной signature map и push-forward actual factor update. Глава не устраняет эти различия одним названием two-flow. [Карточки 100](100-parameter-balance-evolution-and-induced-flow.md) и [101](101-joint-diagnostic-and-weak-signature-velocity.md).

## Вступительный обзор делает более сильные заявления

На PDF 365 сказано, что skew sector движется «until it reaches a fixed point», directional energy растёт с глубиной, а effective rank $W_O$ убывает. Затем дан список author-labelled proved/conditional/conjectural results с forward references. Это **статус, заявленный автором во вступлении**, а не доказательства этих statements на данной странице. Редакционная библиотека сохраняет формулировки и проверяет конкретные утверждения лишь когда доступны их hypotheses и proof. [Начало главы 11](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#chapter-11); [epistemic-status list](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#chapter-11-epistemic-status).

В частности, наличие gradient flow само по себе не означает convergence к fixed point или знак $\partial\mathcal L/\partial\Gamma<0$. Если loss зависит от многих directions параметров, частная производная по одному scalar observable требует определённой координатизации и контроля остальных переменных. Утверждение PDF 366 о выполнении (H2’) в gradient flow/NTK — часть обзорного обещания; без дальнейших assumptions его нельзя использовать как общий факт. [PDF 365–366, conditional item](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#chapter-11-epistemic-status).

В первом абзаце $M_a$ назван сектором score matrix. В уже установленной нотации это skew part канонической матрицы $M\in\mathbb R^{d\times d}$; token-score component равна $L_a=XM_aX^\top\in\mathbb R^{n\times n}$. Упоминание $\mathfrak{so}(d)$ относится к первой, не ко второй матрице. Английская формулировка сохранена буквально. [PDF 365](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#pdf-0365); [геометрия canonical pair](97-canonical-qk-sa-geometry-and-balance-definitions.md).

Depth и training time также различны: $\ell\mapsto\Gamma^{(\ell)}$ сравнивает разные layers в checkpoint, а $t\mapsto\Gamma^{(\ell)}(t)$ — изменение одного layer при обучении. Монотонность одного процесса не следует из другого. Static compatibility inequalities главы 8 уже условны и содержат известные gaps; их стрелки во вступлении не усиливают прежние proofs. [Connections to earlier chapters](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#chapter-11-earlier-connections); [карточка 80](80-conditional-compatibility-and-eigenplane-cancellation.md).

## Третье определение asymmetry index

Список связей с прежними главами на PDF 366 вводит

$$
\alpha_h=\frac{\lVert M_a\rVert_F}{\lVert M\rVert_F}.
$$

При $M\ne0$ и ортогональном S/A splitting это $\sqrt{\alpha_{\mathrm{param}}}$, а не сам squared parameter balance и не empirical score balance. Поэтому в библиотеке теперь необходимо явно различать:

| Имя в редакторском изложении | Формула | Место |
|---|---|---|
| $\alpha_{\mathrm{param}}$ | $\lVert M_a\rVert_F^2/\lVert M\rVert_F^2$ | Corollary 10.11, PDF 351 |
| $\alpha_{\mathrm{score}}$ | $\Gamma_a/(\Gamma_s+\Gamma_a+\varepsilon)$ | Empirical convention, PDF 354 |
| $\alpha_h$ в chapter-11 overview | $\lVert M_a\rVert_F/\lVert M\rVert_F$ | PDF 366 |

Одни и те же threshold values и derivatives нельзя переносить между ними. Для последнего ratio в $M=0$ также нужен отдельно заданный convention; этот overview его не задаёт. [PDF 366](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#chapter-11-earlier-connections); [100](100-parameter-balance-evolution-and-induced-flow.md), [102](102-sars-v8-aggregate-trace-and-score-conventions.md).

## Unresolved references и residual directional energy

Вступление содержит буквальные Chapter `??`, Remark `??`, Theorem `(??)` и Proposition `(??)`. Они сохранены в source. Упоминание $\Gamma_{\mathrm{res}}$ здесь качественное, со ссылкой на будущую Definition 11.43; точное operational definition нельзя восстановить догадкой из выражения «directional information not yet represented». Это также не прежний canonical attention residual $\rho$. [PDF 365–366](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#chapter-11-later-connections).

Фраза о conjectures «verified in Chapter 18» остаётся авторской forward reference. Текущий фрагмент не содержит проверки главы 18 и не превращает empirical verification в универсальное mathematical proof. Эта оговорка сохраняет удобство overview без подмены доказанного статуса. [Epistemic status](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#chapter-11-epistemic-status).

## Что действительно начинается здесь

§11.1 вводит orbit object, §11.2 предлагает читать training dynamics через observable gradients и slack, §11.3 различает два compact SARS traces. Эти локальные определения и условия доступны на PDF 366–367 и разобраны в следующей карточке; будущие statements перечислены в навигации как ссылки автора, а не как завершённые здесь доказательства. [§11.1](../source/0365-0367-ch11-signature-orbits-and-training-traces.md#section-11-1); [107](107-signature-orbits-and-slack-tightening.md).
