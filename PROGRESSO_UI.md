# Progresso da renovação de interface

Última atualização: 2026-11-05. Âmbito: Literatura, Publicadores, Configuração, navbar, Início e Login.
Fora de âmbito por agora: **Territórios, Reuniões e Congressos** (não foram tocados).

## Direção de design

- Mundo: «secretária de escriba». Papel quente (`--paper`), tinta azul-petróleo (`--brand`), tipo Figtree (auto-alojada em `wwwroot/fonts`).
- Bulma 0.9.4 mantém-se como base; `wwwroot/css/site.css` é a camada de design (tokens em `:root`, carregada por último). As cores `is-info/is-primary/is-success/is-danger` do Bulma foram remapeadas, por isso os ecrãs ainda não migrados ficam coerentes.
- Componentes reutilizáveis: `.page-head`, `.toolbar` + `.chips`, `.items/.item` (substitui as tabelas, funciona igual em telemóvel), `.seg` (separadores), `.stepper`, `.modal`, `.notice`, `.empty-state`, `.panel-card`.
- Tabelas `b-table/has-mobile-cards` substituídas por listas de cartões; ícones de botões têm `aria-label`.

## Minimizar refrescos de página

Utilitários em `wwwroot/js/app.js` (sem dependências, usam `fetch` e delegação de eventos):

| Utilitário | Função |
|---|---|
| `App.api` | pedidos HTTP (corpo urlencoded como o `$.ajax` anterior); deteta sessão expirada e acesso negado |
| `App.refresh(sel)` | substitui só uma região da página com a versão do servidor (preserva o separador ativo) |
| `App.removeItem` | remove um item com animação, sem pedir nada |
| `App.filtro` | pesquisa e filtro por tipo instantâneos no browser (URL atualizado com `replaceState`) |
| `App.typeahead` | sugestões de publicações (substitui o jQuery UI autocomplete nos ecrãs migrados) |
| `App.confirm` | confirmação genérica (substitui ~10 modais duplicados) |
| `App.modal`, `App.on`, `App.run` | modais acessíveis (Esc, foco), ações por `data-action`, botão em loading + erro amigável |

Regra: nenhum ecrã migrado usa `window.location.reload()`. Só há navegação real ao criar um publicador (abre a ficha).

## Estado por ecrã

| Ecrã | Estado | Notas |
|---|---|---|
| `_Layout` / navbar | Feito | Sticky, destaque do módulo ativo, avatar com iniciais, menu de utilizador. Removida a imagem externa «Made with Bulma». Modo `ViewData["Bare"]` sem navbar (usado no login) |
| Literatura / Index (Stock) | Feito | Filtro instantâneo + chips por tipo; acertar quantidade, imagem, criar e apagar sem recarregar |
| Literatura / Entradas e Saídas | Feito | Navegação por dia sem recarregar; partials `_BarraData`, `_Movimentos`, `_ScriptMovimentos` |
| Literatura / Pedidos | Feito | Separadores, filtro por publicação no browser, estado por chips; email de destino lembrado no dispositivo |
| Literatura / Periodicos e Periodico | Feito | Semáforo de urgência; finalizar/entregar remove o item no sítio |
| Publicadores / Index | Feito | Pesquisa instantânea; apagar no sítio |
| Publicadores / Publicador | Feito | Guardar sem recarregar; ver «Correções» |
| Configuração | Feito | Cartões por secção, avisos claros |
| Home / Index | Feito | Saudação, resumo de stock carregado em segundo plano (`/Home/Resumo`), módulos com atalhos |
| Home / Login (+ Auth, Error) | Feito | Painel de marca + formulário; congregações renderizadas no servidor (sem pedido AJAX) |
| Territórios, Reuniões, Congressos | **Por fazer** | Não alterados; herdam as cores novas via remapeamento do Bulma |

## Correções de código (comportamento preservado)

- `HomeController.Login`: `ReturnUrl` só aceita URLs locais (evitava redirecionamento aberto); mensagem única de erro (já não revela se o utilizador existe); o ciclo antigo acrescentava sempre «Utilizador não encontrado».
- `LiteraturaController`: respostas `Json(StatusCode(...))` devolviam sempre 200 mesmo em falha; agora devolvem 200/500 reais (`Resultado`). `Movimentos` POST validava depois de consultar a BD. `PesquisarPeriodicos` deixa de rebentar com tipo inválido. Upload de imagem só aceita `.jpg/.jpeg/.png/.webp/.gif`. `OrderBy().OrderBy()` passou a `ThenBy` (mesmo resultado).
- `Publicador.cshtml`: o hash da palavra-passe já não é enviado para o HTML (campo vazio = manter); botões de telefone/morada dependiam do email.
- `PublicadoresController`: `RedirectToAction` passava o id como `routeValues` incorreto.

## Por validar e pendências

- **Não foi possível correr a aplicação**: não há `appsettings` nem MySQL neste ambiente. Validado: build limpo (inclui compilação Razor), detetor de design sem achados, e CSS/JS de lista, modal, stepper e login verificados num browser com marcação estática equivalente. **Falta testar com dados reais** os fluxos: criar/apagar publicação, acertar quantidade, upload de imagem (Dropzone), entradas de periódicos (JW), saídas, pedidos, envio de email, guardar publicador.
- `ObterLiteraturas` embute cada imagem em base64 no HTML (peso grande no Stock e em cada `App.refresh`). Melhoria sugerida: servir imagens por endpoint com cache.
- `ObterMovimentos` faz uma consulta por movimento (N+1) e o SQL é montado por concatenação em `DbContext`; fora do âmbito desta fase.
- Os ecrãs antigos ainda dependem de jQuery, jQuery UI (tema `blitzer`), Bulma JS e bulma-calendar; só se podem retirar do `_Layout` depois de migrar Territórios/Reuniões/Congressos.
- Uma atualização do skill de design está disponível (v4.8.0); não foi aplicada.
