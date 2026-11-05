/* Gestor Congregacional - utilitários de interface partilhados.
   Sem dependências: usa fetch e delegação de eventos, por isso continua a
   funcionar depois de uma região da página ser substituída (App.refresh). */
(function () {
    'use strict';

    // ---------- Notificações (SweetAlert2 continua a ser usado pelos ecrãs antigos) ----------
    function toast(icon, title) {
        Swal.mixin({ toast: true, position: 'top-end', showConfirmButton: false, timer: 4000, timerProgressBar: true })
            .fire({ icon: icon, title: title });
    }
    window.notify = function (m) { toast('success', m); };
    window.notifyError = function (m) { toast('error', m); };

    const App = window.App = {};
    const $ = (s, r) => (r || document).querySelector(s);
    const $$ = (s, r) => Array.from((r || document).querySelectorAll(s));
    App.$ = $; App.$$ = $$;

    // ---------- HTTP ----------
    // Corpo urlencoded em todos os métodos, como o $.ajax que os controllers já esperam.
    App.api = async function (method, url, data) {
        const opt = { method: method, headers: { 'X-Requested-With': 'XMLHttpRequest' }, credentials: 'same-origin' };
        const qs = data ? new URLSearchParams(data).toString() : '';
        if (method === 'GET') { if (qs) url += (url.includes('?') ? '&' : '?') + qs; }
        else opt.body = qs;
        const r = await fetch(url, opt);
        if (r.redirected && /\/Home\/Login/i.test(r.url)) { location.href = r.url; throw new Error('sessão expirada'); }
        if (!r.ok || (r.redirected && /\/Home\/Error/i.test(r.url))) throw new Error('HTTP ' + r.status); // acesso negado redireciona para /Home/Error
        const ct = r.headers.get('content-type') || '';
        return ct.includes('json') ? r.json() : r.text();
    };

    // Executa uma acção com o botão em loading e erro amigável.
    App.run = async function (btn, fn, erro) {
        if (btn) { if (btn.classList.contains('is-loading')) return; btn.classList.add('is-loading'); }
        try { return await fn(); }
        catch (e) { if (e.message !== 'sessão expirada') notifyError(erro || 'Ocorreu um erro. Tente novamente.'); }
        finally { if (btn) btn.classList.remove('is-loading'); }
    };

    // Substitui o conteúdo de uma região com a versão actual do servidor (sem recarregar a página).
    App.refresh = async function (sel, url) {
        const r = await fetch(url || location.href, { headers: { 'X-Requested-With': 'XMLHttpRequest' }, credentials: 'same-origin' });
        const doc = new DOMParser().parseFromString(await r.text(), 'text/html');
        const novo = $(sel, doc), atual = $(sel);
        if (!novo || !atual) return;
        // Preserva o separador/painel que o utilizador tinha aberto.
        const hid = $$('[id][hidden], [id][role=tabpanel]', atual).map(function (e) { return [e.id, e.hidden]; });
        const tabs = $$('[role=tab]', atual).map(function (e) { return [e.id, e.getAttribute('aria-selected')]; });
        atual.innerHTML = novo.innerHTML;
        hid.forEach(function (h) { const e = document.getElementById(h[0]); if (e) e.hidden = h[1]; });
        tabs.forEach(function (t) { const e = document.getElementById(t[0]); if (e) e.setAttribute('aria-selected', t[1]); });
        atual.dispatchEvent(new CustomEvent('refreshed', { bubbles: true }));
    };

    // Remove um item com animação e avisa quando a lista ficar vazia.
    App.removeItem = function (el) {
        const lista = el.parentElement;
        el.classList.add('is-leaving');
        setTimeout(function () { el.remove(); lista.dispatchEvent(new CustomEvent('changed', { bubbles: true })); }, 250);
    };

    // ---------- Modais ----------
    let ultimoFoco = null;
    const modal = App.modal = function (id) {
        const el = typeof id === 'string' ? document.getElementById(id) : id;
        return {
            el: el,
            open: function () {
                ultimoFoco = document.activeElement;
                el.classList.add('is-active'); document.documentElement.classList.add('is-clipped');
                const f = $('[autofocus], input:not([type=hidden]):not([disabled]), select', el);
                if (f) setTimeout(function () { f.focus(); }, 60);
                el.dispatchEvent(new CustomEvent('opened'));
            },
            close: function () {
                el.classList.remove('is-active');
                if (!$('.modal.is-active')) document.documentElement.classList.remove('is-clipped');
                if (ultimoFoco && ultimoFoco.focus) ultimoFoco.focus();
                el.dispatchEvent(new CustomEvent('closed'));
            }
        };
    };

    // Confirmação genérica: await App.confirm({ title, text, ok, danger }) -> boolean
    App.confirm = function (o) {
        return new Promise(function (resolve) {
            const el = document.createElement('div');
            el.className = 'modal';
            el.setAttribute('role', 'alertdialog'); el.setAttribute('aria-modal', 'true');
            el.innerHTML = '<div class="modal-background"></div><div class="modal-card"><header class="modal-card-head"><p class="modal-card-title"></p></header>' +
                '<section class="modal-card-body"><p class="modal-body-text"></p></section>' +
                '<footer class="modal-card-foot"><button type="button" class="button" data-r="0">Cancelar</button><button type="button" class="button" data-r="1" autofocus></button></footer></div>';
            $('.modal-card-title', el).textContent = o.title || 'Confirmar';
            $('.modal-body-text', el).textContent = o.text || '';
            const ok = $('[data-r="1"]', el);
            ok.textContent = o.ok || 'Confirmar';
            ok.classList.add(o.danger === false ? 'is-info' : 'is-danger');
            document.body.appendChild(el);
            const m = modal(el);
            const fim = function (v) { m.close(); setTimeout(function () { el.remove(); }, 200); resolve(v); };
            el.addEventListener('click', function (e) {
                if (e.target.matches('.modal-background')) fim(false);
                const b = e.target.closest('[data-r]'); if (b) fim(b.dataset.r === '1');
            });
            m.open();
        });
    };

    // ---------- Acções delegadas ----------
    const acoes = {};
    App.on = function (nome, fn) { acoes[nome] = fn; };

    document.addEventListener('click', function (e) {
        const t = e.target;
        const abrir = t.closest('[data-open]');
        if (abrir) { e.preventDefault(); modal(abrir.dataset.open).open(); return; }
        const fechar = t.closest('[data-close]');
        if (fechar) { const m = fechar.closest('.modal'); if (m) modal(m).close(); return; }
        if (t.matches('.modal-background')) { const m = t.closest('.modal'); if (m && !m.dataset.persist) modal(m).close(); return; }

        const passo = t.closest('[data-step]');
        if (passo) {
            const inp = $('input', passo.closest('.stepper'));
            const min = inp.min === '' ? -Infinity : +inp.min;
            inp.value = Math.max(min, (parseInt(inp.value, 10) || 0) + (+passo.dataset.step));
            inp.dispatchEvent(new Event('change', { bubbles: true }));
            return;
        }

        const tab = t.closest('.seg [role=tab]');
        if (tab) {
            $$('[role=tab]', tab.parentElement).forEach(function (b) {
                const on = b === tab; b.setAttribute('aria-selected', on);
                const p = document.getElementById(b.getAttribute('aria-controls')); if (p) p.hidden = !on;
            });
            return;
        }

        const a = t.closest('[data-action]');
        if (a && acoes[a.dataset.action]) { e.preventDefault(); acoes[a.dataset.action](a, e); }
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') { const m = $$('.modal.is-active').pop(); if (m && !m.dataset.persist) modal(m).close(); }
        if (e.key === 'Enter' && e.target.matches('.modal input:not([type=hidden])') && !e.target.closest('.ta')) {
            const prim = $('[data-submit]', e.target.closest('.modal')); if (prim) { e.preventDefault(); prim.click(); }
        }
    });

    // ---------- Menus da navbar ----------
    // Em ecrãs largos o menu abre ao passar o rato (CSS) e também ao clicar/tocar, para funcionar em tablets.
    document.addEventListener('click', function (e) {
        const link = e.target.closest('.app-nav .has-dropdown > .navbar-link');
        $$('.app-nav .has-dropdown.is-active').forEach(function (d) { if (!link || d !== link.parentElement) d.classList.remove('is-active'); });
        if (link && window.matchMedia('(min-width: 1024px)').matches) { e.preventDefault(); link.parentElement.classList.toggle('is-active'); }
    });

    // Atualiza os números de uma barra de estatísticas: App.stats({ sPub: 12, ... }).
    App.stats = function (v) { Object.keys(v).forEach(function (k) { const e = document.getElementById(k); if (e) e.textContent = v[k]; }); };

    // ---------- Filtro instantâneo (sem pedir a página ao servidor) ----------
    // Itens com data-search (texto em minúsculas) e data-type; chips com data-type.
    App.filtro = function (o) {
        const lista = $(o.lista), inp = $(o.input), chips = o.chips ? $(o.chips) : null, vazio = o.vazio ? $(o.vazio) : null;
        let tipo = o.tipo || '0';
        const norm = function (s) { return (s || '').toLowerCase().normalize('NFD').replace(/[̀-ͯ]/g, ''); };
        function aplicar() {
            const q = norm(inp.value.trim()); let n = 0;
            $$('.item', lista).forEach(function (i) {
                const ok = (tipo === '0' || i.dataset.type === tipo) && (!q || norm(i.dataset.search).includes(q));
                i.classList.toggle('is-hidden', !ok); if (ok) n++;
            });
            if (vazio) vazio.hidden = n > 0;
            if (o.contador) $(o.contador).textContent = n;
            if (o.aoAplicar) o.aoAplicar($$('.item:not(.is-hidden)', lista));
            const u = new URL(location.href);
            q ? u.searchParams.set('filtro', inp.value.trim()) : u.searchParams.delete('filtro');
            tipo !== '0' ? u.searchParams.set('tipo', tipo) : u.searchParams.delete('tipo');
            history.replaceState(null, '', u);
        }
        let t; inp.addEventListener('input', function () { clearTimeout(t); t = setTimeout(aplicar, 120); });
        if (chips) chips.addEventListener('click', function (e) {
            const c = e.target.closest('.chip'); if (!c) return;
            tipo = c.dataset.type; $$('.chip', chips).forEach(function (x) { x.setAttribute('aria-pressed', x === c); }); aplicar();
        });
        lista.addEventListener('refreshed', aplicar); lista.addEventListener('changed', aplicar);
        aplicar();
        return { aplicar: aplicar };
    };

    // ---------- Pesquisa com sugestões (substitui o jQuery UI autocomplete) ----------
    // o: { input, url, params(termo), mapa(item)->{ref,texto,valor}, aoEscolher(item) }
    App.typeahead = function (o) {
        const inp = typeof o.input === 'string' ? $(o.input) : o.input;
        const wrap = inp.parentElement; wrap.classList.add('ta');
        const ul = document.createElement('ul'); ul.className = 'ta-list'; ul.hidden = true; ul.setAttribute('role', 'listbox');
        wrap.appendChild(ul);
        inp.setAttribute('autocomplete', 'off');
        let itens = [], sel = -1, seq = 0, t;
        function mostrar() {
            ul.innerHTML = '';
            if (!itens.length) { ul.innerHTML = '<li class="empty">Sem resultados</li>'; ul.hidden = false; return; }
            itens.forEach(function (it, i) {
                const li = document.createElement('li'); li.setAttribute('role', 'option');
                li.setAttribute('aria-selected', i === sel);
                if (it.ref) { const r = document.createElement('span'); r.className = 'ref'; r.textContent = it.ref; li.appendChild(r); }
                const x = document.createElement('span'); x.textContent = it.texto; li.appendChild(x);
                li.addEventListener('mousedown', function (e) { e.preventDefault(); escolher(i); });
                ul.appendChild(li);
            });
            ul.hidden = false;
        }
        function escolher(i) {
            const it = itens[i]; if (!it) return;
            inp.value = (it.ref ? '[' + it.ref + '] ' : '') + it.texto; ul.hidden = true; o.aoEscolher(it);
        }
        inp.addEventListener('input', function () {
            o.aoEscolher(null); clearTimeout(t);
            const termo = inp.value.trim(); if (!termo) { ul.hidden = true; return; }
            t = setTimeout(async function () {
                const meu = ++seq;
                try {
                    const d = await App.api('GET', o.url, o.params(termo));
                    if (meu !== seq) return; // resposta antiga
                    itens = d.map(o.mapa); sel = -1; mostrar();
                } catch (e) { notifyError('Ocorreu um erro na pesquisa.'); }
            }, 200);
        });
        inp.addEventListener('keydown', function (e) {
            if (ul.hidden) return;
            if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
                e.preventDefault(); sel = (sel + (e.key === 'ArrowDown' ? 1 : -1) + itens.length) % itens.length; mostrar();
            } else if (e.key === 'Enter' && sel >= 0) { e.preventDefault(); e.stopPropagation(); escolher(sel); }
            else if (e.key === 'Escape') { e.stopPropagation(); ul.hidden = true; }
        });
        inp.addEventListener('blur', function () { setTimeout(function () { ul.hidden = true; }, 120); });
        return { limpar: function () { inp.value = ''; ul.hidden = true; o.aoEscolher(null); } };
    };

    // Pesquisa de publicações usada em Entradas, Saídas e Pedidos.
    App.pesquisaPublicacao = function (input, stampEl, periodico) {
        return App.typeahead({
            input: input, url: '/Literatura/Literaturas/',
            params: function (t) { return { filtro: t, periodico: !!periodico }; },
            mapa: function (i) { return { ref: i.referencia, texto: i.descricao, valor: i.stamp }; },
            aoEscolher: function (i) { $(stampEl).value = i ? i.valor : ''; }
        });
    };

    // Movimento de saída: mantém o par de registos que a base de dados já usava.
    App.saida = async function (stamp, qtd, idpub) {
        await App.api('POST', '/Literatura/Movimentos/', { stamp: stamp, qtd: -qtd, idpub: 0 });
        await App.api('POST', '/Literatura/Movimentos/', { stamp: stamp, qtd: qtd, idpub: idpub });
    };

    // Seleccionar o publicador "Loja" (id definido pelo ecrã).
    App.lerInt = function (sel) { return parseInt($(sel).value, 10) || 0; };
})();
