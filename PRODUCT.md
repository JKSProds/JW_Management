# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Utilizador principal (confirmado): o servo de literatura de uma congregação, que regista entradas, saídas, pedidos e inventário de literatura, muitas vezes de pé, no armazém ou no Salão do Reino.

Outros públicos que o código sugere, por confirmar: gestores (Admin, Assistente, Coordenador, Secretário, Serviço) que gerem publicadores e o programa das reuniões, e publicadores comuns que consultam as suas designações.

## Product Purpose

Gestor Congregacional: aplicação web para gerir a literatura, os publicadores e o programa das reuniões de uma congregação. O programa e as designações vêm do Hourglass, geram-se PDF e enviam-se por email.

## Positioning

Camada de leitura e distribuição sobre o Hourglass: o Hourglass continua a ser a fonte dos dados e a aplicação só lê dele (nunca escreve). Não é, por agora, o sítio onde se edita o programa.

## Operating Context

- Cada congregação é um tenant com base de dados própria e configuração de email própria.
- O acesso à API do Hourglass faz-se com a sessão do utilizador, colada na aplicação (pedido cURL ou exportação) e guardada só em memória.
- A autenticação faz-se por OIDC (passkey), com login por palavra-passe como alternativa.

## Capabilities and Constraints

- Idioma: português de Portugal em todo o texto, datas e números.
- Uma base de dados por congregação (multi-tenant).
- Módulos: Literatura, Publicadores e grupos, Reuniões e Programa (PDF e email), Configuração.
- Territórios e Congressos foram retirados do produto e não devem voltar à interface.
- Por decidir: se a aplicação deve vir a editar o programa em vez de só o ler do Hourglass.

## Evidence on Hand

Não há testemunhos, métricas nem casos de estudo. Não inventar nenhum. Os dados reais (nomes, emails, telefones de publicadores) são confidenciais e não devem aparecer em exemplos ou mock-ups.

## Product Principles

1. Só leitura no Hourglass: nunca escrever na fonte externa.
2. Ferramenta de trabalho antes de vitrine: rapidez e clareza no uso repetido, em especial para o servo de literatura.
3. Português claro e direto, sem jargão técnico visível ao utilizador.
4. Cada congregação isolada: dados, configuração e email nunca se misturam entre tenants.

## Accessibility & Inclusion

Nenhum requisito específico foi estabelecido. Fica registado só o mínimo geral de interface acessível; não foi pedida uma norma concreta.
