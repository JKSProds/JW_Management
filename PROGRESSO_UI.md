# Progresso da renovação de interface

Última atualização: 2026-11-05. Âmbito: Literatura, Publicadores, Configuração, navbar, Início e Login.
Fora de âmbito por agora: **Territórios, Reuniões e Congressos** (não foram tocados).

## Direção de design

- Mundo: gabinete administrativo. Cinzentos frios (`--paper`), azul-tinta (`--brand`), tipo Figtree (auto-alojada em `wwwroot/fonts`).
- Bulma 0.9.4 mantém-se como base; `wwwroot/css/site.css` é a camada de design (tokens em `:root`, carregada por último). As cores `is-info/is-primary/is-success/is-danger` do Bulma foram remapeadas, por isso os ecrãs ainda não migrados ficam coerentes.
- Componentes reutilizáveis: `.page-head`, `.toolbar` + `.chips`, `.items/.item` (substitui as tabelas, funciona igual em telemóvel), `.seg` (separadores), `.stepper`, `.modal`, `.notice`, `.empty-state`, `.panel-card`.
- Tabelas `b-table/has-mobile-cards` substituídas por listas de cartões; ícones de botões têm `aria-label`.

## Revisão 2 (2026-11-05)

- Paleta passou de papel quente/petróleo para cinzentos frios e azul-tinta.
- Barra de estatísticas (`_Stats.cshtml`) no topo de cada ecrã migrado; segue o filtro e as alterações em Stock, Publicadores e Periódicos (`App.stats`).
- Navbar: menus com borda, respiração e ponte invisível sobre o intervalo (já não fecham ao mover o rato); também abrem por clique/toque.
- Login: a lista de congregações é relida se estiver vazia (era lida só no arranque) e há pedido AJAX de reserva com aviso de erro.

- Visual plano (revisão 3): sem sombras nem gradientes nos cartões, raios de 6-10px, sem elevação ao passar o rato; sombra só nos elementos flutuantes (menus, modais, avisos).

## Revisão 4 (2026-11-05)

Aplicadas as 12 sugestões visuais: miniaturas com proporção fixa e ícone por tipo quando não há imagem; quantidade em destaque com barra de stock (relativa ao maior stock da lista; âmbar até 20%, vermelha a zero) e setas nas entradas/saídas; cabeçalhos sem o quadrado de ícone; estatísticas sem fundo nos ícones; filtros em texto com sublinhado; navbar alinhada com o conteúdo, módulo ativo sublinhado e menu móvel em painel lateral; Início plano com resumo (stock, exemplares por entregar, pedidos por enviar, periódicos urgentes e lista «Atenção», via `/Home/Resumo`); login com painel de marca mais estreito; erros junto ao campo (`App.campo`); linhas compactas a partir de 1024px; tabelas, separadores e cartões dos ecrãs antigos alinhados com o novo estilo.

## Revisão 5 (2026-11-05)

- Ícones sem cruz: `clipboard-list` (pedidos), `box-open`, `id-card`, `people-roof`, etc.
- Login: deixou de ter seletor de congregação. O utilizador é procurado em todas as congregações; se existir em mais de uma, o ecrã pede para escolher (e volta a pedir a palavra-passe). Cartão centrado, sem painel lateral. Página inicial sem sessão centrada com o botão de entrada.
- «A entregar em breve» passou a menos de 31 dias (Início e Periódicos).
- JW Hub: aceita o comando «Copiar como cURL» e extrai a cookie; o envio passou a POST (a cookie já não vai no URL).

## Revisão 6 (2026-11-05)

- Tema claro/escuro em toda a app: tokens em `:root` e `:root[data-theme="dark"]`, escolha guardada em `localStorage` (`tema`), por omissão segue o sistema. Botão na navbar e no login. As cores fixas do Bulma, jQuery UI, calendário, Swal e mapas (peças Leaflet) foram ligadas aos tokens.
- Territórios migrados para o novo design.
- Hourglass: ver nota abaixo.

### Hourglass (investigação)
Fontes públicas indicam que o Hourglass não tem API pública; permite exportar os dados da congregação em PDF, DOCX, CSV ou JSON e importar de KHS, NW Scheduler e TSWIN (https://www.hourglass-app.com/en/features/). Caminho seguro: importar um ficheiro exportado. Falta um ficheiro de exemplo (anonimizado) para escrever o leitor; não se deve ligar com as credenciais do utilizador.

## Importação do Hourglass (reuniões)

Reuniões > «Importar do Hourglass». O utilizador cola um pedido «Copiar como cURL» do Hourglass; o servidor extrai a sessão e faz **só GET** (`HourglassContext`), sem a guardar nem registar. Endereços usados (API interna da aplicação web, não documentada): `fsreport/users` (id e nome), `scheduling/mm/meeting/{datas}?no_subs=true` (partes e cânticos), `scheduling/mm/schedule/{datas}` (quem tem cada parte) e `scheduling/wm/schedule/{datas}` (fim de semana e discurso público).
Cada semana é criada com `AdicionarReuniao` (reunião-base a partir de `r_tipos`) e preenchida; há pré-visualização com o tipo local de cada parte editável (a escolha fica guardada no browser) e correspondência de nomes aos publicadores (sem correspondência fica só o nome, como no Excel). A tabela `r_tipos` pode estar vazia: os tipos em falta são **criados a partir do Hourglass** na importação (nome estável por tipo de parte, minutos e sala adicional), com Ids por separador: 1-99 Tesouros/presidente/oração inicial, 101+ Ministério, 201+ Vida cristã, 301+ presidente do fim de semana, 401+ A Sentinela. Os Ids não correspondem aos campos do PDF S-140 antigo, que por isso pode ficar por preencher. O fim de semana mostra-se como sábado e domingo e o dia do discurso escolhe-se na pré-visualização (domingo por omissão). Por ser API não documentada, pode deixar de funcionar se o Hourglass a alterar.

## Programa mensal (PDF) e fim do S-140

O formulário S-140 foi removido (ação, método, botões e `S-140_TPO.pdf`). Em Reuniões, «Gerar programa» cria um PDF A4 horizontal no estilo do quadro de atividades da congregação (uma coluna por semana: reunião de semana, fim de semana, A Sentinela, indicadores e sistema sonoro), com os dados lidos do Hourglass (`HourglassPrograma`, `ProgramaPdf`). Semanas de eventos (assembleia de circuito, memorial…) aparecem a amarelo. A limpeza vem de `scheduling/other/clean` (atribuída ao responsável do grupo, mostra-se «Grupo N») e o dirigente de A Sentinela, que o Hourglass devolve vazio, usa um valor por omissão editável no ecrã (Filipe Gonçalves).

## Sincronização de publicadores com o Hourglass

Publicadores > «Sincronizar com o Hourglass» (`HourglassSincronizador`, só leitura no Hourglass): liga cada utilizador a um publicador por email ou nome, e propõe preencher email, telemóvel, morada (`fsreport/addresses`) e grupo (pelo responsável do grupo, ou pelo número), ou criar quem falta (como «Publicador», sem palavra-passe). Por omissão só se preenche o que está vazio; diferenças em dados já preenchidos ficam por marcar. Não altera funções nem palavras-passe. Dados do Hourglass sem coluna na aplicação (nascimento, batismo, nomeação, estado) não são guardados.

## Revisão 7

- Nomes do Hourglass: usa-se o **nome preferido** (`descriptor`; sem ele, primeiro e último nome) no programa, na importação de reuniões e como nome de novos publicadores.
- Alinhamento dos controlos em modais e painéis: inputs, selects, botões e ícones partilham a mesma altura (`--control-h`); removida a dupla margem do Bulma nas grelhas de campos.
- Sincronização de publicadores: seleção em bloco (tudo, nada, só email, só telemóvel, só morada, só grupo, só novos). «Tudo» substitui também dados já preenchidos.
- Instruções do Hourglass: o pedido a copiar é o que tem o intervalo de datas no nome (ex.: `2026-11-01_2026-11-30`).

## Revisão 8

- O Bulma carregado é a **1.x** (variáveis `--bulma-*`): no tema escuro aplicam-se as variáveis de escuro do próprio Bulma, o que corrige os menus brancos da navbar. Itens dos menus com `!important` por causa da regra `:not()` do Bulma.
- Notificações próprias (`notify`/`notifyError`) com as cores do tema, no lugar do SweetAlert2 (removido do layout). O GSTC (gráfico do Calendário) só carrega nessa página: dava erro de sintaxe no Safari em todas as outras.
- Pedidos reescrito: pesquisa, filtro por estado com contagens, agrupar por publicador/publicação, seleção em bloco (visíveis, grupo ou individual) com barra de ações (marcar como Pendente/Enviado/Fornecido, apagar), botão «Avançar» ao estado seguinte, tudo sem recarregar. Novos endpoints `PedidoEstado` e `PedidosApagar` (ids só numéricos).
- Login com passkey: falhas do OIDC passam a ficar registadas no servidor (`[OIDC] Falha no início de sessão`).

## Revisão 9: Reuniões simplificadas e programas

- Importação Excel removida (ação, método, pacote EPPlus). Territórios escondidos da navbar e do Início (código mantido).
- **Programa mensal guardado**: a tabela `r_programa` (migração V_1.7) guarda como JSON as semanas, o serviço de campo e o testemunho público de cada mês. «Obter do Hourglass» lê e guarda uma vez; os PDF geram-se depois **sem cookie** (`ProgramaServico`, `/Reunioes/ProgramaPdf`).
- **Três PDF modernos** (`PdfProgramas`): quadro mensal A4 horizontal (numa página, com cânticos, indicadores, sistema sonoro e limpeza), serviço de campo e testemunho público A4 vertical.
- Cânticos: números e títulos vêm do Hourglass (programa e importação de reuniões) e enchem o catálogo `sys_canticos`.
- Endpoints do Hourglass usados (GET): `scheduling/other/fm` (serviço de campo), `scheduling/fsmeetings/locations`, `scheduling/pw/schedule/{datas}`, `scheduling/pw/locations?disabled=true`; instantes do testemunho em UTC, convertidos para a hora de Lisboa.
- Interface: Semanas (lista com progresso e filtros), Reunião (linhas com gravação automática e filtro «por atribuir»), Programa e PDF (substitui o Calendário), Discursos.

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
| Territórios (Index, Território, Registos) | Feito | Estados disponível/atribuído/+90 dias, atribuir e devolver com envio opcional por email/SMS, anexos com lista viva, leitor QR partilhado (`App.qr`) |
| Reuniões (Início, Reunião, Discursos, Calendário) | Feito | Cartões de designação com guardar/apagar sem recarregar, importação do Hourglass e programa mensal em PDF; o gráfico Gantt do Calendário não segue o tema escuro |
| Congressos | **Por fazer** | Não alterado; segue as cores e o tema via remapeamento do Bulma |

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
