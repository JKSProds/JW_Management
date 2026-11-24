---
name: Gestor Congregacional
description: Gabinete administrativo em cinzentos frios e azul-tinta, plano, direto e legível, para um balcão de secretaria digital.
colors:
  paper: "#f3f5f9"
  surface: "#ffffff"
  line: "#e2e7ef"
  line-strong: "#c9d1de"
  ink: "#151e2d"
  ink-soft: "#465165"
  ink-faint: "#637085"
  brand: "#2a4dd0"
  brand-strong: "#1e3a9f"
  brand-tint: "#e8edfb"
  ok: "#1f7a4d"
  ok-tint: "#e1f3ea"
  warn: "#a35a06"
  warn-tint: "#fdf0dd"
  bad: "#c0372b"
  bad-tint: "#fbe6e3"
typography:
  display:
    fontFamily: "Figtree, ui-sans-serif, system-ui, sans-serif"
    fontSize: "1.85rem"
    fontWeight: 780
    lineHeight: 1.1
    letterSpacing: "-0.02em"
  body:
    fontFamily: "Figtree, ui-sans-serif, system-ui, sans-serif"
    fontSize: "1rem"
    fontWeight: 400
  label:
    fontFamily: "Figtree, ui-sans-serif, system-ui, sans-serif"
    fontSize: "0.82rem"
    fontWeight: 600
rounded:
  sm: "6px"
  md: "8px"
components:
  button-primary:
    backgroundColor: "{colors.brand}"
    textColor: "#ffffff"
    rounded: "{rounded.sm}"
    height: "44px"
  button-primary-hover:
    backgroundColor: "{colors.brand-strong}"
  button-light:
    backgroundColor: "{colors.brand-tint}"
    textColor: "{colors.brand-strong}"
    rounded: "{rounded.sm}"
    height: "44px"
  input:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.ink}"
    rounded: "{rounded.sm}"
    height: "44px"
  card:
    backgroundColor: "{colors.surface}"
    rounded: "{rounded.md}"
---

# Design System: Gestor Congregacional

## Overview

**Creative North Star: "O Balcão de Secretaria"**

Uma ferramenta de trabalho diária, prática e sem cerimónia. O ecrã faz o que o balcão de uma secretaria faz: tem tudo à mão, ordenado, legível de relance e sem nada que distraia da tarefa. A superfície é plana e fria, o azul-tinta aparece onde há ação, e os números e estados dizem o que importa.

A interface serve sobretudo o servo de literatura, de pé no armazém ou no Salão do Reino, muitas vezes no telemóvel, e depois os gestores que tratam do programa e dos publicadores no computador. A densidade é a de uma lista de trabalho: linhas com miniatura, nome, metadados e ações, em vez de cartões decorativos.

O sistema é construído sobre o Bulma 0.9.4 com uma camada própria em `wwwroot/css/site.css`, com tema claro e escuro.

**Key Characteristics:**
- Plano em repouso: sem sombras, a profundidade vem de bordas finas e de camadas tonais.
- Um único acento (azul-tinta), reservado à ação e ao estado selecionado.
- Controlos firmes e diretos, com altura mínima de toque de 44 px.
- Números tabulares e estados com cor semântica (ok, aviso, erro) sempre acompanhados de texto ou ícone.
- Tema claro e escuro com os mesmos papéis de cor.

## Colors

Uma paleta de cinzentos frios com um só azul-tinta como acento e três cores de estado.

### Primary
- **Azul-tinta** (#2a4dd0): botões primários, ligações, item de menu ativo e filtro selecionado. No tema escuro desloca-se para #8aa2ff no texto e mantém #3b5bdb como fundo sólido.
- **Azul-tinta fundo** (#e8edfb): fundo de botões suaves, item ativo e passagem do rato.

### Neutral
- **Papel frio** (#f3f5f9): fundo da página.
- **Superfície** (#ffffff): cartões, listas, campos e modais.
- **Linha** (#e2e7ef) e **linha forte** (#c9d1de): bordas de superfícies e de controlos.
- **Tinta** (#151e2d), **tinta suave** (#465165), **tinta ténue** (#637085): texto principal, secundário e de apoio.

### Semantic
- **Verde de estado** (#1f7a4d, fundo #e1f3ea): concluído, completo.
- **Âmbar de aviso** (#a35a06, fundo #fdf0dd): por atribuir, atenção.
- **Vermelho de erro** (#c0372b, fundo #fbe6e3): erro e ações destrutivas.

### Named Rules
**The One Voice Rule.** O azul-tinta é a única cor de acento. Se um ecrã precisa de uma segunda cor para se fazer entender, o problema é a estrutura, não a paleta.

**The Never-Color-Alone Rule.** Estado nunca se comunica só por cor: leva sempre texto ou ícone.

## Typography

**Display Font:** Figtree (variável, 300–900, com *fallback* para fontes de sistema)
**Body Font:** Figtree
**Label/Mono Font:** Figtree; números em `font-variant-numeric: tabular-nums`

**Character:** uma sans-serif humanista e limpa, de peso firme nos títulos e neutra no corpo. Dá legibilidade a quem trabalha de pé e com pressa.

### Hierarchy
- **Display** (780, 1.85 rem, 1.1): título de página (`.page-head h1`), espaçamento de letras −0.02 em.
- **Title** (650–750, ≈1.05–1.2 rem): título de cartão e de secção, números das estatísticas (1.2 rem, 750).
- **Body** (400, 1 rem; 15 px no telemóvel e 16 px em ecrãs ≥768 px): texto corrido e listas.
- **Label** (600, 0.82 rem): etiquetas de campos; notas de apoio em 0.8 rem (`.field-hint`) na cor ténue.

### Named Rules
**The Tabular Numbers Rule.** Quantidades, minutos e contagens usam números tabulares para alinhar em coluna.

## Layout

Contentor central de 1100 px, com cabeçalho de página (título, ícone e ações), barra de filtros e conteúdo em listas. A navegação é uma barra superior com menus suspensos, que no telemóvel passa a menu lateral. As estatísticas ficam numa barra horizontal que passa a duas colunas abaixo de 640 px.

O ritmo vertical é de 1 a 1.25 rem entre blocos. As listas de itens ocupam a largura toda e as tabelas largas (modal de importação) deslizam na horizontal em vez de partir o ecrã.

## Elevation & Depth

Plano por defeito: `--shadow: none`. A profundidade vem de bordas de 1 px (`line`), de fundos tonais (papel contra superfície) e de um véu (`overlay`) por trás dos modais.

### Shadow Vocabulary
- **Pop** (`box-shadow: 0 8px 24px rgba(21,30,45,.14)`; no escuro `rgba(0,0,0,.5)`): só menus suspensos, listas de autocompletar e outros elementos que flutuam acima da página.

### Named Rules
**The Flat-By-Default Rule.** Cartões, listas e campos não têm sombra. Só o que flutua tem.

## Shapes

Cantos suaves e contidos: 6 px nos controlos (botões, campos, filtros) e 8 px nas superfícies (cartões, barra de estatísticas, grupos). Bordas finas de 1 px. Os ícones de ação são quadrados arredondados de 44 px; a marca da aplicação tem 34 px com 9 px de raio.

## Components

### Buttons
- **Shape:** cantos de 6 px, altura mínima 44 px, peso 650.
- **Primary:** fundo azul-tinta (#2a4dd0), texto branco; no hover escurece para #1e3a9f.
- **Light:** fundo azul-tinta suave (#e8edfb), texto azul escuro; para ações secundárias da mesma família.
- **Outlined / Ghost:** contorno azul ou transparente com texto cinzento, para ações terciárias.
- **Estado:** `Success` e `Danger` com fundos sólidos de verde e vermelho; `.is-loading` esconde o texto e mostra o indicador.
- **Icon button:** 44 px quadrado, contorno `line-strong`; no hover pinta-se de azul-tinta, verde ou vermelho conforme a ação.

### Chips / filtros
- **Estilo:** texto simples com sublinhado de 2 px no selecionado (`aria-pressed`), em azul-tinta.
- **Modo de escolha em formulário:** botões com contorno e, quando selecionados, fundo azul sólido e texto branco.

### Cards / Containers
- **Corner Style:** 8 px.
- **Background:** superfície branca sobre o papel frio.
- **Shadow Strategy:** nenhuma.
- **Border:** 1 px `line`.
- **Internal Padding:** 1.25 rem em `.panel-card`.

### Inputs / Fields
- **Style:** contorno `line-strong` de 1 px, fundo superfície, cantos de 6 px, altura mínima 44 px.
- **Focus:** contorno e anel de 1 px em azul-tinta.
- **Etiquetas e ajuda:** etiqueta de 0.82 rem em tinta suave; nota de apoio de 0.8 rem em tinta ténue.

### Navigation
- Barra superior de 3.75 rem sobre superfície, com texto em tinta suave e peso 550. O item com o rato por cima ou ativo ganha sublinhado interior de 2 px em azul-tinta, ou fundo azul suave quando é item ativo de menu. Menus suspensos com sombra Pop. No telemóvel a barra recolhe num menu e os submenus abrem com um filete lateral.

### Statbar
- Faixa horizontal com 3 a 4 indicadores. Ícone em azul-tinta (ou cor de estado), valor em 1.2 rem/750 e legenda de 0.76 rem. Separada por filetes de 1 px.

### Lista de itens (`.item`)
- Linha com miniatura ou avatar, nome, metadados e ações à direita. Uma borda de 1 px que reforça no hover. É o padrão central de Literatura, Publicadores e Reuniões.

### Avisos (`.notice`)
- Faixa com ícone e texto, em tons de estado (`ok`, `warn`, `bad`) sobre os fundos de tom; usa `role="status"` ou `role="alert"`.

### Modais
- Cartão sobre véu `overlay`, cabeçalho com título e botão de fechar, corpo com deslocamento próprio e rodapé de ações. A variante `.is-wide` (até 940 px) serve a importação do Hourglass.

## Do's and Don'ts

### Do:
- **Do** usar o azul-tinta (#2a4dd0) só para ação, ligação e estado selecionado.
- **Do** manter superfícies planas, com borda de 1 px (`line`) em vez de sombra.
- **Do** acompanhar todo o estado de cor com texto ou ícone.
- **Do** manter alvos de toque de pelo menos 44 px (`--touch`) nos controlos e nos botões de ícone.
- **Do** usar números tabulares em quantidades, minutos e contagens.
- **Do** definir as cores por tokens e dar o equivalente no tema escuro.
- **Do** escrever o texto da interface em português de Portugal.

### Don't:
- **Don't** usar emojis na interface (instrução geral da organização); usar ícones do conjunto existente.
- **Don't** pôr sombra em cartões, listas ou campos em repouso; reservar a sombra Pop ao que flutua.
- **Don't** introduzir um segundo acento de cor para dar ênfase.
- **Don't** mostrar dados reais de publicadores (nomes, emails, telefones) em exemplos ou mock-ups.
- **Don't** reintroduzir módulos retirados (Territórios, Congressos) na navegação ou no CSS.
