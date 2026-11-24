namespace JW_Management.Models
{
    using System.Diagnostics;
    using JW_Management.Controllers;
    using Microsoft.AspNetCore.Identity;
    using MySql.Simple;

    public class DbContext
    {
        public AppContext _appContext { get; set; }
        public Tenant _currentTenant => _appContext._currentTenant;
        private string _masterConnectionString => _appContext._tenantContext._masterConnectionString;
        private string _connectionString => _currentTenant.ConnectionString;
        
        public DbContext(AppContext appContext)
        {
            try
            {
                _appContext = appContext;
                using Database db = _masterConnectionString;
            }
            catch (Exception)
            {
                throw new Exception("Não foi possivel conectar á BD!");
            }
        }

        public bool ExecutarQuery(string SQL_Query)
        {
            try
            {
                Database db = _connectionString;

                db.Execute(SQL_Query);
                db.Connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;

            }
            return true;
        }

        #region Literatura
        //Obter todas as literaturas com os stocks especificos de um publicador e um filtro
        public List<Literatura> ObterLiteraturas(string filtro, int idpub)
        {
            List<Literatura> LstLiteratura = new();
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();
            List<Publicador> LstPublicador = ObterPublicadores(false);
            
            _appContext._fileContext.ObterCaminhoLiteratura();

            using (Database db = _connectionString)
            {
                string sql = "SELECT *, IFNULL((SELECT SUM(Quantidade) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura and l_movimentos.IdPublicador=" + idpub + "), 0) as Quantidade, IFNULL((SELECT Caminho from l_img where l_pubs.Referencia=l_img.Referencia), '') as Imagem FROM l_pubs where Descricao like '%" + filtro + "%' or Referencia like '%" + filtro + "%';";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstLiteratura.Add(new Literatura()
                    {
                        Stamp = result["STAMP"],
                        Id = result["Id"],
                        Referencia = result["Referencia"],
                        Data = result["Data"],
                        Imagem = "data:image/jpeg;base64," + Convert.ToBase64String(_appContext._fileContext.ObterFicheiro(_appContext._fileContext.ObterCaminhoLiteratura() + result["Imagem"])),
                        Descricao = result["Descricao"],
                        Quantidade = result["Quantidade"],
                        Tipo = LstTiposLiteratura.Where(g => g.Id == result["IdTipo"]).FirstOrDefault(new TipoLiteratura()),
                        Publicador = LstPublicador.Where(g => g.Id == idpub).FirstOrDefault(new Publicador())
                    });

                    if (LstLiteratura.Last().Tipo.Id == 7)
                    {
                        LstLiteratura.Last().Imagem = LstLiteratura.Last().URL;
                    }
                }
            }

            return LstLiteratura.OrderBy(l => l.Descricao.Trim()).ToList();
        }

        //Obter todas as literaturas com os stocks especificos entre duas datas
        public List<Literatura> ObterLiteraturas(int Mes, int Ano)
        {
            List<Literatura> LstLiteratura = new();
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            DateTime dInicial = DateTime.Parse("01/" + Mes + "/" + Ano + " 00:00:00");
            DateTime dFinal = DateTime.Parse(DateTime.DaysInMonth(Ano, Mes) + "/" + Mes + "/" + Ano + " 23:59:59");

            using (Database db = _connectionString)
            {
                string sql = "SELECT l_pubs.*, IFNULL((SELECT SUM(Quantidade) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura and Data < '" + dFinal.ToString("yyyy-MM-dd HH:mm:ss") + "' and IdPublicador = 0), 0) as Quantidade , IFNULL((SELECT SUM(CASE WHEN Quantidade > 0 THEN Quantidade ELSE 0 END) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura and Data between '" + dInicial.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + dFinal.ToString("yyyy-MM-dd HH:mm:ss") + "' and IdPublicador = 0), 0) as Entradas , IFNULL((SELECT SUM(CASE WHEN Quantidade < 0 THEN Quantidade ELSE 0 END) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura and Data between '" + dInicial.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + dFinal.ToString("yyyy-MM-dd HH:mm:ss") + "' and IdPublicador = 0), 0) as Saidas FROM l_pubs;";

                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstLiteratura.Add(new Literatura()
                    {
                        Stamp = result["STAMP"],
                        Id = result["Id"],
                        Referencia = result["Referencia"],
                        Data = result["Data"],
                        Descricao = result["Descricao"],
                        Quantidade = result["Quantidade"],
                        Entradas = result["Entradas"],
                        Saidas = result["Saidas"],
                        Linha = 0,
                        Tipo = LstTiposLiteratura.Where(g => g.Id == result["IdTipo"]).FirstOrDefault(new TipoLiteratura())
                    });
                }
            }

            return LstLiteratura.OrderBy(l => l.Descricao.Trim()).ToList();
        }

        //Obter uma literatura em especifico
        public Literatura ObterLiteratura(string STAMP)
        {
            List<Literatura> LstLiteratura = new();
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = _connectionString)
            {
                string sql = "SELECT *, IFNULL((SELECT SUM(Quantidade) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura and l_movimentos.IdPublicador=0), 0) as Quantidade, IFNULL((SELECT Caminho from l_img where l_pubs.Referencia=l_img.Referencia), '') as Imagem FROM l_pubs where STAMP='" + STAMP + "';";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstLiteratura.Add(new Literatura()
                    {
                        Stamp = result["STAMP"],
                        Id = result["Id"],
                        Referencia = result["Referencia"],
                        Data = result["Data"],
                        Imagem = "data:image/jpeg;base64," + Convert.ToBase64String(_appContext._fileContext.ObterFicheiro(_appContext._fileContext.ObterCaminhoLiteratura() + result["Imagem"])),
                        Descricao = result["Descricao"],
                        Quantidade = result["Quantidade"],
                        Tipo = LstTiposLiteratura.Where(g => g.Id == result["IdTipo"]).FirstOrDefault(new TipoLiteratura())
                    });
                }
            }

            return LstLiteratura.OrderBy(l => l.Descricao.Trim()).FirstOrDefault(new Literatura());
        }

        //Obter tipos de literatura
        public List<TipoLiteratura> ObterTiposLiteratura()
        {
            List<TipoLiteratura> LstTiposLiteratura = new();

            using (Database db = _connectionString)
            {
                string sql = "SELECT * FROM l_tipos;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstTiposLiteratura.Add(new TipoLiteratura()
                    {
                        Id = result["Id"],
                        Descricao = result["Descricao"]
                    });
                }
            }

            return LstTiposLiteratura;
        }

        //Obter estados de pedidos
        public List<EstadoPedido> ObterEstadosPedido()
        {
            List<EstadoPedido> LstEstadoPedido = new();

            using (Database db = _connectionString)
            {
                string sql = "SELECT * FROM l_estado_pedido;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstEstadoPedido.Add(new EstadoPedido()
                    {
                        Id = result["Id"],
                        Descricao = result["Descricao"]
                    });
                }
            }

            return LstEstadoPedido;
        }

        //Obter todos os grupos
        public List<Grupo> ObterGrupos()
        {
            List<Grupo> LstGrupos = new();
            List<Publicador> LstPublicadores = ObterPublicadores(false);

            using (Database db = _connectionString)
            {
                string sql = "SELECT * FROM l_grupos;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstGrupos.Add(new Grupo()
                    {
                        Id = result["Id"],
                        Nome = result["Nome"],
                        Responsavel = LstPublicadores.Where(p => p.Id == int.Parse(result["IdResponsavel"])).DefaultIfEmpty(new Publicador()).First(),
                        Ajudante = LstPublicadores.Where(p => p.Id == int.Parse(result["IdAjudante"])).DefaultIfEmpty(new Publicador()).First(),
                    });
                }
            }

            return LstGrupos;
        }

        //Obter todos os grupos
        public Grupo ObterGrupo(int id)
        {
            Grupo g = new();
            List<Publicador> LstPublicadores = ObterPublicadores(false);

            using (Database db = _connectionString)
            {
                string sql = "SELECT * FROM l_grupos where Id=" + id + ";";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    g = new Grupo()
                    {
                        Id = result["Id"],
                        Nome = result["Nome"],
                        Responsavel = LstPublicadores.Where(p => p.Id == int.Parse(result["IdResponsavel"])).DefaultIfEmpty(new Publicador()).First(),
                        Ajudante = LstPublicadores.Where(p => p.Id == int.Parse(result["IdAjudante"])).DefaultIfEmpty(new Publicador()).First(),
                    };
                }
            }

            return g;
        }

        //Adicionar literatura ou atualizar existente
        public bool AdicionarLiteratura(Literatura l)
        {

            string sql = "INSERT INTO l_pubs(Id, Referencia, Descricao, Data, IdTipo, STAMP) VALUES ";
            sql += ("('" + l.Id + "', '" + l.Referencia + "', '" + l.Descricao + "', '" + l.Data + "', '" + l.Tipo.Id + "', '" + l.Stamp + "') ");
            // sql += " ON DUPLICATE KEY UPDATE Id = VALUES(Id), Referencia = VALUES(Referencia), Descricao = VALUES(Descricao), IdTipo = VALUES(IdTipo);";

            return ExecutarQuery(sql);
        }

        //Apagar uma literatura e todos os movimentos associados
        public bool ApagarLiteratura(string stamp)
        {

            string sql = "DELETE FROM l_pubs where STAMP = '" + stamp + "';";
            sql += "DELETE FROM l_movimentos where StampLiteratura = '" + stamp + "';";

            return ExecutarQuery(sql);
        }


        //Obter tipos de periodicos
        public List<Literatura> ObterTipoPeriodicos()
        {
            List<Literatura> LstLiteratura = new();

            using (Database db = _connectionString)
            {
                string sql = "SELECT * from l_periodicos order by Id";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstLiteratura.Add(new Literatura()
                    {
                        Id = result["Id"],
                        Referencia = result["Referencia"],
                        Descricao = result["Descricao"]
                    });
                }
            }

            return LstLiteratura;
        }

        //Obter todos os periodicos em stocks
        public List<Literatura> ObterPeriodicos()
        {
            List<Literatura> LstLiteratura = new();
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = _connectionString)
            {
                string sql = "SELECT *, (SELECT Descricao FROM l_periodicos WHERE l_pubs.Referencia=l_periodicos.Referencia) as DescAdicional, IFNULL((SELECT SUM(Quantidade) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura and l_movimentos.IdPublicador=0), 0) as Quantidade, IFNULL((SELECT Caminho from l_img where l_pubs.Referencia=l_img.Referencia), '') as Imagem FROM l_pubs where IdTipo=7 and IFNULL((SELECT SUM(Quantidade) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura), 0) > 0 and (SELECT SUM(Quantidade) from l_pedidos_periodicos where l_pubs.Referencia = l_pedidos_periodicos.Referencia) > 0;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstLiteratura.Add(new Literatura()
                    {
                        Stamp = result["STAMP"],
                        Id = result["Id"],
                        Referencia = result["Referencia"],
                        Data = result["Data"],
                        Descricao = result["Descricao"],
                        Imagem = "data:image/jpeg;base64," + Convert.ToBase64String(_appContext._fileContext.ObterFicheiro(_appContext._fileContext.ObterCaminhoLiteratura() + result["Imagem"])),
                        Quantidade = result["Quantidade"],
                        DescricaoGeral = result["DescAdicional"],
                        QuantidadeFalta = ObterPeriodicos(result["STAMP"]).Sum(o => o.Quantidade),
                        Tipo = LstTiposLiteratura.Where(g => g.Id == result["IdTipo"]).FirstOrDefault(new TipoLiteratura())
                    });
                }
            }

            return LstLiteratura.ToList();
        }

        public List<Literatura> ObterPeriodicosData(string data, string referencia)
        {
            List<Literatura> LstLiteratura = new();
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = _connectionString)
            {
                string sql = "SELECT *, (SELECT Descricao FROM l_periodicos WHERE l_pubs.Referencia=l_periodicos.Referencia) as DescAdicional, IFNULL((SELECT SUM(Quantidade) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura and l_movimentos.IdPublicador=0), 0) as Quantidade, IFNULL((SELECT Caminho from l_img where l_pubs.Referencia=l_img.Referencia), '') as Imagem FROM l_pubs where IdTipo=7 and l_pubs.Data='" + data + "' and l_pubs.Referencia='" + referencia + "';";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstLiteratura.Add(new Literatura()
                    {
                        Stamp = result["STAMP"],
                        Id = result["Id"],
                        Referencia = result["Referencia"],
                        Data = result["Data"],
                        Descricao = result["Descricao"],
                        Imagem = "data:image/jpeg;base64," + Convert.ToBase64String(_appContext._fileContext.ObterFicheiro(_appContext._fileContext.ObterCaminhoLiteratura() + result["Imagem"])),
                        Quantidade = result["Quantidade"],
                        DescricaoGeral = result["DescAdicional"],
                        Tipo = LstTiposLiteratura.Where(g => g.Id == result["IdTipo"]).FirstOrDefault(new TipoLiteratura())
                    });
                }
            }

            return LstLiteratura.ToList();
        }
        //Obter todos os periodicos especificos de uma literatura para entregar aos publicador
        public List<Literatura> ObterPeriodicos(string stamp)
        {
            List<Literatura> LstLiteratura = new();
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = _connectionString)
            {
                string sql = "select l_pubs.STAMP, l_pubs.Id, l_pubs.Referencia, l_pubs.Descricao, l_pedidos_periodicos.Quantidade, l_pubs.IdTipo, sys_utilizadores.* from l_pedidos_periodicos inner join l_pubs on l_pubs.Referencia=l_pedidos_periodicos.Referencia left join sys_utilizadores on sys_utilizadores.IdUtilizador=l_pedidos_periodicos.IdPublicador\r\nwhere Quantidade>IFNULL((SELECT SUM(Quantidade) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura  and l_movimentos.IdPublicador=l_pedidos_periodicos.IdPublicador), 0) and l_pubs.STAMP='" + stamp + "';";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstLiteratura.Add(new Literatura()
                    {
                        Stamp = result["STAMP"],
                        Id = result["Id"],
                        Referencia = result["Referencia"],
                        Descricao = result["Descricao"],
                        Quantidade = result["Quantidade"],
                        Tipo = LstTiposLiteratura.Where(g => g.Id == result["IdTipo"]).FirstOrDefault(new TipoLiteratura()),
                        Publicador = ObterPublicador(int.Parse(result["IdUtilizador"]), false, false, true)
                    });
                }
            }

            return LstLiteratura.ToList();
        }

        //Obter Publicadores
        
        public List<Publicador> ObterPublicadores(int tenant, bool LoadGrupo)
        {
            List<Publicador> LstPublicador = new();
            List<Grupo> LstGrupos = new List<Grupo>();

            if (LoadGrupo) LstGrupos = ObterGrupos();

            using (Database db = _appContext._tenantContext._tenants.First(t => t.Id == tenant).ConnectionString)
            {
                string sql = "SELECT * FROM sys_utilizadores";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstPublicador.Add(new Publicador()
                    {
                        Id = result["IdUtilizador"],
                        Username = result["Username"],
                        Password = result["Password"],
                        Nome = result["Nome"],
                        Email = result["Email"],
                        Telemovel = result["Telemovel"],
                        TipoUtilizador = result["Tipo"],
                        Morada = result["Morada"]
                    });

                    if (LoadGrupo) LstPublicador.Last().Grupo = LstGrupos.Where(g => g.Id == int.Parse(result["IdGrupo"])).DefaultIfEmpty(new Grupo()).First();
                }
            }

            return LstPublicador;
        }
        public List<Publicador> ObterPublicadores(bool LoadGrupo)
        {
            List<Publicador> LstPublicador = new();
            List<Grupo> LstGrupos = new List<Grupo>();

            if (LoadGrupo) LstGrupos = ObterGrupos();

            using (Database db = _connectionString)
            {
                string sql = "SELECT * FROM sys_utilizadores";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstPublicador.Add(new Publicador()
                    {
                        Id = result["IdUtilizador"],
                        Username = result["Username"],
                        Password = result["Password"],
                        Nome = result["Nome"],
                        Email = result["Email"],
                        Telemovel = result["Telemovel"],
                        TipoUtilizador = result["Tipo"],
                        Morada = result["Morada"]
                    });

                    if (LoadGrupo) LstPublicador.Last().Grupo = LstGrupos.Where(g => g.Id == int.Parse(result["IdGrupo"])).DefaultIfEmpty(new Grupo()).First();
                }
            }

            return LstPublicador;
        }

        //Obter Publicador
        public Publicador ObterPublicador(int id, bool LoadMovimentos, bool LoadPedidos, bool LoadGrupos)
        {
            Publicador p = new();

            using (Database db = _connectionString)
            {
                string sql = "SELECT * FROM sys_utilizadores where IdUtilizador=" + id + ";";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    p = new Publicador()
                    {
                        Id = result["IdUtilizador"],
                        Username = result["Username"],
                        Password = result["Password"],
                        Nome = result["Nome"],
                        Email = result["Email"],
                        Telemovel = result["Telemovel"],
                        TipoUtilizador = result["Tipo"],
                        Morada = result["Morada"]
                    };
                    if (LoadMovimentos) p.Movimentos = ObterMovimentos(true, true, int.Parse(result["IdUtilizador"]), DateOnly.MinValue);
                    if (LoadPedidos)
                    {
                        p.Pedidos = ObterPedidosPeriodico(p.Id);
                        p.Pedidos.AddRange(ObterPedidosEspeciais(p.Id));
                    }
                    if (LoadGrupos) p.Grupo = ObterGrupo(int.Parse(result["IdGrupo"]));
                        }
            }

            return p;
        }

        //Adicionar publicador
        public bool AdicionarPublicador(Publicador p)
        {
            string sql = "INSERT INTO sys_utilizadores(IdUtilizador, Username, Password, Nome, Telemovel, Email, Morada, IdGrupo, Tipo) VALUES ";
            sql += ("('" + p.Id + "', '" + p.Username + "', '" + p.Password + "', '" + p.Nome + "', '" + p.Telemovel + "', '" + p.Email + "', '" + p.Morada + "', '" + p.Grupo.Id + "', '90') ON DUPLICATE KEY UPDATE Username=VALUES(Username), Password=VALUES(Password), Telemovel=VALUES(Telemovel), Email=VALUES(Email), Morada=VALUES(Morada), IdGrupo=VALUES(IdGrupo), Nome=VALUES(Nome);");

            return ExecutarQuery(sql);
        }

        //Apagar um publicador em especifico
        public bool ApagarPublicador(int id)
        {
            string sql = "DELETE FROM sys_utilizadores where IdUtilizador = '" + id + "';";
            sql += "DELETE FROM l_movimentos where IdPublicador = '" + id + "';";
            sql += "DELETE FROM l_pedidos_periodicos where IdPublicador = '" + id + "';";

            return ExecutarQuery(sql);
        }

        // Função (permissão) do utilizador: só chamada pelo controlador depois de confirmar que quem pede é Admin.
        public bool AtualizarTipoUtilizador(int id, int tipo) =>
            ExecutarQuery("UPDATE sys_utilizadores SET Tipo=" + tipo + " WHERE IdUtilizador=" + id + ";");

        // Grupos de serviço (só Admin). O Id novo é o máximo + 1 para não depender do AUTO_INCREMENT da tabela.
        private static string Esc(string? s) => (s ?? "").Trim().Replace("\\", "\\\\").Replace("'", "''");
        public bool GuardarGrupo(int id, string? nome, int idResponsavel, int idAjudante) =>
            id == 0
                ? ExecutarQuery("INSERT INTO l_grupos (Id, Nome, IdResponsavel, IdAjudante) SELECT IFNULL(MAX(Id), 0) + 1, '" + Esc(nome) + "', " + idResponsavel + ", " + idAjudante + " FROM l_grupos;")
                : ExecutarQuery("UPDATE l_grupos SET Nome='" + Esc(nome) + "', IdResponsavel=" + idResponsavel + ", IdAjudante=" + idAjudante + " WHERE Id=" + id + ";");
        // Os publicadores do grupo apagado passam a «sem grupo» (Id 0).
        public bool ApagarGrupo(int id) =>
            ExecutarQuery("UPDATE sys_utilizadores SET IdGrupo=0 WHERE IdGrupo=" + id + "; DELETE FROM l_grupos WHERE Id=" + id + ";");

        //Obter movimentos de um determinado tipo
        public List<Movimentos> ObterMovimentos(bool In, bool Out, int IdPub, DateOnly data)
        {
            List<Movimentos> LstMovimentos = new();
            DateOnly dF = data == DateOnly.MinValue ? DateOnly.MaxValue : data.AddDays(1);

            using (Database db = _connectionString)
            {
                string sql = "SELECT * FROM l_movimentos where " + (Out ? "IdPublicador=" + IdPub + " AND " : "") + "Data between '" + data.ToString("yyyy-MM-dd") + "' and '" + dF.ToString("yyyy-MM-dd") + "' AND(" + (In ? "Quantidade > 0" : "") + (In && Out ? " OR " : "") + (Out ? "Quantidade < 0" : "") + "); ";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstMovimentos.Add(new Movimentos()
                    {
                        Stamp = result["Id"],
                        Literatura = ObterLiteratura(result["StampLiteratura"]),
                        Quantidade = int.Parse(result["Quantidade"]),
                        DataMovimento = DateTime.Parse(result["Data"]),
                        Publicador = ObterPublicador(int.Parse(result["IdPublicador"]), false, false, false)
                    });
                }
            }

            return LstMovimentos;
        }

        //Adicionar movimentos
        public bool AdicionarMovimento(Movimentos m)
        {
            string sql = "INSERT INTO l_movimentos(Id, StampLiteratura, Quantidade, Data, IdPublicador) VALUES ";
            sql += ("('" + m.Stamp + "', '" + m.Literatura!.Stamp + "', '" + m.Quantidade + "', '" + m.DataMovimento.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + m.Publicador!.Id + "');");

            return ExecutarQuery(sql);
        }

        //Apagar um movimento em especifico
        public bool ApagarMovimento(string stamp)
        {

            string sql = "DELETE FROM l_movimentos where id = '" + stamp + "';";

            return ExecutarQuery(sql);
        }

        //Obter pedidos periodicos
        public List<Literatura> ObterPedidosPeriodico()
        {
            List<Literatura> LstLiteratura = new();
            List<Publicador> LstPublicador = ObterPublicadores(true);
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = _connectionString)
            {
                string sql = "SELECT l_pedidos_periodicos.*, l_periodicos.Descricao, IFNULL((SELECT Caminho from l_img where l_periodicos.Referencia=l_img.Referencia), '') as Imagem from l_pedidos_periodicos inner join l_periodicos on l_pedidos_periodicos.Referencia = l_periodicos.Referencia;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstLiteratura.Add(new Literatura()
                    {
                        Stamp = result["Id"],
                        Referencia = result["Referencia"],
                        Imagem = "data:image/jpeg;base64," + Convert.ToBase64String(_appContext._fileContext.ObterFicheiro(_appContext._fileContext.ObterCaminhoLiteratura() + result["Imagem"])),
                        Descricao = result["Descricao"],
                        Quantidade = int.Parse(result["Quantidade"]),
                        Tipo = LstTiposLiteratura.Where(g => g.Id == 7).FirstOrDefault(new TipoLiteratura()),
                        Publicador = LstPublicador.Where(p => p.Id == int.Parse(result["IdPublicador"]!)).DefaultIfEmpty(new Publicador()).First()
                    });
                }
            }

            return LstLiteratura;
        }

        //Obtem todos os pedidos periodicos de um utilizador
        public List<Literatura> ObterPedidosPeriodico(int IdPub)
        {
            List<Literatura> LstLiteratura = new();
            Publicador p = ObterPublicador(IdPub, false, false, false);
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = _connectionString)
            {
                string sql = "SELECT l_pedidos_periodicos.*, l_periodicos.Descricao from l_pedidos_periodicos inner join l_periodicos on l_pedidos_periodicos.Referencia = l_periodicos.Referencia Where l_pedidos_periodicos.IdPublicador=" + IdPub + ";";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstLiteratura.Add(new Literatura()
                    {
                        Stamp = result["Id"],
                        Referencia = result["Referencia"],
                        Descricao = result["Descricao"],
                        Quantidade = int.Parse(result["Quantidade"]),
                        Tipo = LstTiposLiteratura.Where(g => g.Id == 7).FirstOrDefault(new TipoLiteratura()),
                        Publicador = p
                    });
                }
            }

            return LstLiteratura;
        }

        //Adicionar periodicos
        public bool AdicionarPedidoPeriodico(Literatura l)
        {
            string sql = "INSERT INTO l_pedidos_periodicos(Id, IdPublicador, Quantidade, Referencia) VALUES ";
            sql += ("('" + l.Stamp + "', '" + l.Publicador.Id + "', '" + l.Quantidade + "', '" + l.Referencia + "') ON DUPLICATE KEY UPDATE Quantidade=Quantidade+" + l.Quantidade + ";");

            return ExecutarQuery(sql);
        }

        //Apagar um pedido periodico em especifico
        public bool ApagarPedidoPeriodico(string stamp)
        {

            string sql = "DELETE FROM l_pedidos_periodicos where id = '" + stamp + "';";

            return ExecutarQuery(sql);
        }

        //Obtem todos os pedidos especiais
        public List<Literatura> ObterPedidosEspeciais()
        {
            List<Literatura> LstLiteratura = new();
            List<Publicador> LstPublicador = ObterPublicadores(false);
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = _connectionString)
            {
                string sql = "SELECT l_pedidos_especiais.*, l_pubs.Referencia, l_pubs.Descricao, l_pubs.IdTipo, IFNULL((SELECT Caminho from l_img where l_pubs.Referencia=l_img.Referencia), '') as Imagem from l_pedidos_especiais inner join l_pubs on l_pedidos_especiais.StampLiteratura = l_pubs.STAMP;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstLiteratura.Add(new Literatura()
                    {
                        Stamp = result["Id"],
                        Referencia = result["Referencia"],
                        Imagem = "data:image/jpeg;base64," + Convert.ToBase64String(_appContext._fileContext.ObterFicheiro(_appContext._fileContext.ObterCaminhoLiteratura() + result["Imagem"])),
                        EstadoPedido = new EstadoPedido(result["Estado"]),
                        Descricao = result["Descricao"],
                        Quantidade = int.Parse(result["Quantidade"]),
                        Tipo = LstTiposLiteratura.Where(g => g.Id == result["IdTipo"]).FirstOrDefault(new TipoLiteratura()),
                        Publicador = LstPublicador.Where(p => p.Id == int.Parse(result["IdPublicador"]!)).DefaultIfEmpty(new Publicador()).First()
                    });
                }
            }

            return LstLiteratura;
        }


        //Obtem todos os pedidos especiais de um utilziador em especifico
        public List<Literatura> ObterPedidosEspeciais(int IdPub)
        {
            List<Literatura> LstLiteratura = new();
            Publicador p = ObterPublicador(IdPub, false, false, false);
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = _connectionString)
            {
                string sql = "SELECT l_pedidos_especiais.*, l_pubs.Referencia, l_pubs.Descricao, l_pubs.IdTipo from l_pedidos_especiais inner join l_pubs on l_pedidos_especiais.StampLiteratura = l_pubs.STAMP Where IdPublicador = " + IdPub + ";";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstLiteratura.Add(new Literatura()
                    {
                        Stamp = result["Id"],
                        Referencia = result["Referencia"],
                        EstadoPedido = new EstadoPedido(result["Estado"]),
                        Descricao = result["Descricao"],
                        Quantidade = int.Parse(result["Quantidade"]),
                        Tipo = LstTiposLiteratura.Where(g => g.Id == result["IdTipo"]).FirstOrDefault(new TipoLiteratura()),
                        Publicador = p
                    });
                }
            }

            return LstLiteratura;
        }


        //Obtem um pedido especial em especifico
        public Literatura ObterPedidoEspecial(string Stamp)
        {
            Literatura l = new();
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = _connectionString)
            {
                string sql = "SELECT l_pedidos_especiais.*, l_pubs.Referencia, l_pubs.Descricao, l_pubs.IdTipo from l_pedidos_especiais inner join l_pubs on l_pedidos_especiais.StampLiteratura = l_pubs.STAMP Where l_pedidos_especiais.Id = '" + Stamp + "';";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    l = new Literatura()
                    {
                        Stamp = result["Id"],
                        EstadoPedido = new EstadoPedido(result["Estado"]),
                        Referencia = result["Referencia"],
                        Descricao = result["Descricao"],
                        Quantidade = int.Parse(result["Quantidade"]),
                        Tipo = LstTiposLiteratura.Where(g => g.Id == result["IdTipo"]).FirstOrDefault(new TipoLiteratura()),
                        Publicador = ObterPublicador(int.Parse(result["IdPublicador"]), false, false, false)
                    };
                }
            }

            return l;
        }

        //Adicionar pedidos especiais
        public bool AdicionarPedidoEspecial(Literatura l, EstadoPedido e)
        {
            string sql = "INSERT INTO l_pedidos_especiais(Id, IdPublicador, Quantidade, StampLiteratura, Estado, Fornecido) VALUES ";
            sql += ("('" + DateTime.Now.Ticks.ToString() + "', '" + l.Publicador.Id + "', '" + l.Quantidade + "', '" + l.Stamp + "', '" + e.Descricao + "', " + e.Fornecido + ") ON DUPLICATE KEY UPDATE Quantidade=Quantidade+" + l.Quantidade + ";");

            return ExecutarQuery(sql);
        }

        //Atualiza o estado de um pedido
        public bool AtualizarEstadoPedidoEspecial(Literatura l, EstadoPedido e)
        {
            string sql = "UPDATE l_pedidos_especiais SET Estado='" + e.Descricao + "', Fornecido='" + (e.Fornecido ? 1 : 0) + "', Quantidade=" + l.Quantidade + " WHERE Id='" + l.Stamp + "';";

            return ExecutarQuery(sql);
        }

        //Apagar um pedido especial em especifico
        public bool ApagarPedidoEspecial(string stamp)
        {

            string sql = "DELETE FROM l_pedidos_especiais where id = '" + stamp + "';";

            return ExecutarQuery(sql);
        }

        //Inserir imagem
        public bool AdicionarImagemLiteratura(Literatura l, string FileName)
        {
            string sql = "DELETE FROM l_img WHERE Referencia='" + l.Referencia + "';\r\n";
            sql += "INSERT INTO l_img(Referencia, Caminho) VALUES ";
            sql += ("('" + l.Referencia + "', '" + FileName + "');");

            return ExecutarQuery(sql);
        }

        //Obter imagem
        public async Task<IFormFile> ObterImagemLiteratura(Literatura l, string URL)
        {
            using (HttpClient client = new HttpClient())
            {
                // Download the image bytes from the URL
                byte[] imageBytes = await client.GetByteArrayAsync(URL);

                // Create a MemoryStream from the downloaded image bytes
                using (MemoryStream memoryStream = new MemoryStream(imageBytes))
                {
                    // Create an IFormFile from the MemoryStream
                    IFormFile formFile = new FormFile(memoryStream, 0, imageBytes.Length, "image", Path.GetFileName(URL));

                    return formFile;
                }
            }
        }
        #endregion

        //REUNIOES
        #region Reunioes

        public List<Designacao> ObterDesignacoes(string Stamp)
        {
            List<Designacao> d = new();
            List<TipoDesignacao> t = ObterTiposDesignacao();
            List<Publicador> p = ObterPublicadores(false);

            using (Database db = _connectionString)
            {
                string sql = "SELECT * FROM r_designacoes WHERE StampReuniao='" + Stamp + "';";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    d.Add(new Designacao()
                    {
                        Stamp = result["Stamp"],
                        StampReuniao = result["StampReuniao"],
                        SemanaReuniao = result["Semana"],
                        NomePublicador = result["Publicador"],
                        NomeDesignacao = result["Designacao"],
                        NMin = result["Minutos"],
                        Local = result["Local"],
                        Publicador = p.Where(u => u.Id == result["IdPublicador"] && u.Id > 0).DefaultIfEmpty(new Publicador() { Nome = result["Publicador"] }).First(),
                        TipoDesignacao = t.Where(u => u.Id == result["IdTipo"] && u.Id > 0).DefaultIfEmpty(new TipoDesignacao() { Descricao = result["Designacao"] }).First(),
                    });
                }
            }

            return d;
        }

        public Designacao ObterDesignacao(string stamp)
        {
            List<Designacao> d = new();
            List<TipoDesignacao> t = ObterTiposDesignacao();
            List<Publicador> p = ObterPublicadores(false);

            using (Database db = _connectionString)
            {
                string sql = "SELECT * FROM r_designacoes WHERE Stamp='" + stamp + "';";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    d.Add(new Designacao()
                    {
                        Stamp = result["Stamp"],
                        StampReuniao = result["StampReuniao"],
                        SemanaReuniao = result["Semana"],
                        NomePublicador = result["Publicador"],
                        NomeDesignacao = result["Designacao"],
                        Local = result["Local"],
                        NMin = result["Minutos"],
                        Publicador = p.Where(u => u.Id == result["IdPublicador"] && u.Id > 0).DefaultIfEmpty(new Publicador() { Nome = result["Publicador"] }).First(),
                        TipoDesignacao = t.Where(u => u.Id == result["IdTipo"] && u.Id > 0).DefaultIfEmpty(new TipoDesignacao() { Descricao = result["Designacao"] }).First(),
                    });
                }
            }

            return d.DefaultIfEmpty(new Designacao()).First();
        }

        public bool AdicionarDesignacao(Designacao d)
        {

            string sql = "INSERT INTO r_designacoes(Stamp, StampReuniao, IdPublicador, IdTipo, Semana, Publicador,  Designacao,  Local) VALUES ";
            sql += ("('" + d.Stamp + "', '" + d.StampReuniao + "', '" + d.Publicador.Id + "', '" + d.TipoDesignacao.Id + "', '" + d.SemanaReuniao + "', '" + d.NomePublicador + "', '" + d.NomeDesignacao + "', '" + d.Local + "') ");

            return ExecutarQuery(sql);
        }

        public bool AdicionarDesignacoes(List<Designacao> LstDesignacoes)
        {
            if (LstDesignacoes.Count == 0) return false;
            
            string sql = "INSERT INTO r_designacoes(Stamp, StampReuniao, IdPublicador, IdTipo, Semana, Publicador,  Designacao,  Local, Minutos) VALUES ";

            foreach (var d in LstDesignacoes)
            {
                sql += ("('" + d.Stamp + "', '" + d.StampReuniao + "', '" + d.Publicador.Id + "', '" + d.TipoDesignacao.Id + "', '" + d.SemanaReuniao + "', '" + d.NomePublicador + "', '" + d.NomeDesignacao + "', '" + d.Local + "', '" + d.NMin + "'),\r\n ");
            }
            sql = sql.Remove(sql.Length - 4, 4) + ";";
            return ExecutarQuery(sql);
        }

        public bool AtualizarDesignacao(Designacao d)
        {
            string sql = "UPDATE r_designacoes SET IdPublicador = " + d.Publicador.Id + ", Publicador = '" + d.Publicador.Nome + "', Minutos = '" + d.NMin + "', Designacao = '" + d.NomeDesignacao + "' WHERE Stamp='" + d.Stamp + "';";

            return ExecutarQuery(sql);
        }
        public bool ApagarDesignacoes(List<string> Stamps)
        {
            string sql = "";

            foreach (var s in Stamps)
            {
                sql += ("DELETE FROM r_designacoes where Stamp='" + s + "';\r\n ");
            }

            return ExecutarQuery(sql);
        }

        public List<Reuniao> ObterReunioes(bool LoadDesignacoes)
        {
            List<Reuniao> r = new();

            using (Database db = _connectionString)
            {
                string sql = "SELECT DISTINCT StampReuniao, Semana FROM r_designacoes;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    r.Add(new Reuniao()
                    {
                        Stamp = result[0],
                        SemanaReuniao = result[1]
                    });

                    if (LoadDesignacoes) r.Last().Designacoes = ObterDesignacoes(r.Last().Stamp!).OrderBy(d => d.Stamp).ToList();

                }
            }

            return r;
        }
        public Reuniao ObterReuniao(string Stamp, bool LoadDesignacoes, bool LoadCanticos)
        {
            List<Reuniao> r = new();

            using (Database db = _connectionString)
            {
                string sql = "SELECT DISTINCT StampReuniao, Semana FROM r_designacoes Where StampReuniao='" + Stamp + "';";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    r.Add(new Reuniao()
                    {
                        Stamp = result[0],
                        SemanaReuniao = result[1]
                    });

                    if (LoadDesignacoes) r.Last().Designacoes = ObterDesignacoes(r.Last().Stamp!).OrderBy(d => d.Stamp).ToList();
                    if (LoadCanticos) r.Last().Canticos = ObterCanticos(r.Last().Stamp!).OrderBy(d => d.Stamp).ToList();

                }
            }

            return r.Any() ? r.First() : new Reuniao();
        }

        public bool AdicionarReuniao(string semana, string stamp)
        {
            List<TipoDesignacao> LstTipos = ObterTiposDesignacao();
            List<Designacao> LstDesignacoes = new();
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Some code or operation you want to measure

            foreach (var t in LstTipos)
            {
                for (int i = 1; i <= t.Salas; i++)
                {
                    LstDesignacoes.Add(new Designacao()
                    {
                        Stamp = DateTime.Now.Ticks + stopwatch.ElapsedTicks.ToString(),
                        StampReuniao = stamp,
                        SemanaReuniao = semana,
                        NomeDesignacao = t.DescricaoAdicional,
                        NomePublicador = "",
                        Publicador = new Publicador(),
                        TipoDesignacao = t,
                        NMin = t.NMin,
                        Local = i == 1 ? "Auditório" : "Sala " + i,
                    });
                }
            }

            for (int i = 0; i < 3; i++)
            {
                CriarCantico(stamp, new Cantico());
            }

            stopwatch.Stop();
            return AdicionarDesignacoes(LstDesignacoes);
        }

        public bool ApagarReunioes(List<string> Stamps)
        {
            string sql = "";

            foreach (var s in Stamps)
            {
                sql += ("DELETE FROM r_designacoes where StampReuniao='" + s + "';\r\n ");
                sql += ("DELETE FROM r_canticos where StampReuniao='" + s + "';\r\n ");
            }

            return ExecutarQuery(sql);
        }

        // Designações de um publicador em todas as semanas (para o «as minhas designações» do Início).
        public List<Designacao> ObterDesignacoesPublicador(int idPub)
        {
            var res = new List<Designacao>();
            var tipos = ObterTiposDesignacao();
            using (Database db = _connectionString)
            {
                using var result = db.Query("SELECT * FROM r_designacoes WHERE IdPublicador=" + idPub + ";");
                while (result.Read())
                {
                    res.Add(new Designacao()
                    {
                        Stamp = result["Stamp"], StampReuniao = result["StampReuniao"], SemanaReuniao = result["Semana"], NomeDesignacao = result["Designacao"],
                        NMin = int.TryParse(result["Minutos"], out var m) ? m : 0, Local = result["Local"],
                        TipoDesignacao = tipos.FirstOrDefault(t => t.Id.ToString() == result["IdTipo"]) ?? new TipoDesignacao { Descricao = result["Designacao"] }
                    });
                }
            }
            return res;
        }

        // ---------- Programa mensal (documentos JSON em r_programa) ----------
        private static string EscSql(string? s) => (s ?? "").Replace("\\", "\\\\").Replace("'", "''");

        public Dictionary<string, (string Dados, DateTime Atualizado)> ObterProgramaDocs(string tipo, IEnumerable<string> chaves)
        {
            var res = new Dictionary<string, (string, DateTime)>();
            var lista = chaves.Select(c => "'" + EscSql(c) + "'").ToList();
            if (lista.Count == 0) return res;

            using (Database db = _connectionString)
            {
                string sql = "SELECT Chave, Dados, timestamp FROM r_programa WHERE Tipo='" + EscSql(tipo) + "' AND Chave IN (" + string.Join(",", lista) + ");";
                using var result = db.Query(sql);
                while (result.Read()) res[result["Chave"]] = (result["Dados"], DateTime.Parse(result["timestamp"]));
            }
            return res;
        }

        public bool GuardarProgramaDoc(string tipo, string chave, string json)
        {
            return ExecutarQuery("INSERT INTO r_programa (Tipo, Chave, Dados) VALUES ('" + EscSql(tipo) + "', '" + EscSql(chave) + "', '" + EscSql(json) + "') ON DUPLICATE KEY UPDATE Dados=VALUES(Dados);");
        }

        // Progresso das reuniões numa só consulta: total de designações e quantas já têm publicador.
        public Dictionary<string, (int Total, int Atribuidas)> ObterProgressoReunioes()
        {
            var res = new Dictionary<string, (int, int)>();
            using (Database db = _connectionString)
            {
                using var result = db.Query("SELECT StampReuniao, COUNT(*) AS Total, SUM(IdPublicador > 0 OR Publicador <> '') AS Atrib FROM r_designacoes GROUP BY StampReuniao;");
                while (result.Read()) res[result["StampReuniao"]] = (int.Parse(result["Total"]), int.Parse(result["Atrib"] == "" ? "0" : result["Atrib"]));
            }
            return res;
        }

        // Cânticos: o catálogo local (sys_canticos) enche-se com os números e títulos vindos do Hourglass.
        public bool GuardarCanticos(IEnumerable<(int Numero, string Titulo)> canticos)
        {
            var linhas = canticos.Where(c => c.Numero > 0).DistinctBy(c => c.Numero)
                .Select(c => "(" + c.Numero + ", '" + EscSql(c.Titulo.Length > 50 ? c.Titulo[..50] : c.Titulo) + "')").ToList();
            if (linhas.Count == 0) return true;
            return ExecutarQuery("INSERT INTO sys_canticos (id, tema) VALUES " + string.Join(",", linhas) + " ON DUPLICATE KEY UPDATE tema=IF(VALUES(tema)='', tema, VALUES(tema));");
        }

        public bool CriarTipoDesignacao(TipoDesignacao t)
        {
            string T(string? s) => (s ?? "").Replace("\\", "\\\\").Replace("'", "''");
            return ExecutarQuery("INSERT INTO r_tipos (Id, Descricao, DescricaoAdicional, Minutos, SalasExtra) VALUES (" + t.Id + ", '" + T(t.Descricao) + "', '" + T(t.DescricaoAdicional) + "', " + t.NMin + ", " + (t.Salas > 1 ? 1 : 0) + ");");
        }

        public List<TipoDesignacao> ObterTiposDesignacao()
        {
            List<TipoDesignacao> t = new();

            using (Database db = _connectionString)
            {
                string sql = "SELECT * FROM r_tipos order by Id;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    t.Add(new TipoDesignacao()
                    {
                        Id = result["Id"],
                        Descricao = result["Descricao"],
                        DescricaoAdicional = result["DescricaoAdicional"],
                        NMin = result["Minutos"],
                        Salas = result["SalasExtra"] == "1" ? 2 : 1
                    });
                }
            }

            return t;
        }

        public List<Cantico> ObterCanticos()
        {
            List<Cantico> c = new();

            using (Database db = _connectionString)
            {
                string sql = "SELECT * FROM sys_canticos;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    c.Add(new Cantico()
                    {
                        Id = result["id"],
                        Nome = result["tema"],
                        TextoBiblico = result["texto"]
                    });
                }
            }

            return c;
        }
        public List<Cantico> ObterCanticos(string Stamp)
        {
            List<Cantico> c = ObterCanticos();
            List<Cantico> res = new();

            using (Database db = _connectionString)
            {
                string sql = "SELECT * FROM r_canticos WHERE StampReuniao='" + Stamp + "';";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    res.Add(c.Where(o => o.Id == result["id"]).DefaultIfEmpty(new Cantico()).First());
                    res.Last().Stamp = result["Stamp"];
                }
            }

            return res;
        }

        public bool CriarCantico(string StampReuniao, Cantico c)
        {
            return ExecutarQuery("INSERT INTO r_canticos (Stamp, Id, StampReuniao) VALUES ('" + c.Stamp + "', " + c.Id + ", '" + StampReuniao + "');");
        }

        public bool AtualizarCantico(Cantico c)
        {
            return ExecutarQuery("UPDATE r_canticos SET Id='" + c.Id + "' WHERE Stamp='" + c.Stamp + "';");
        }
        public bool ApagarCantico(Cantico c)
        {
            return ExecutarQuery("DELETE FROM r_canticos WHERE Stamp='" + c.Stamp + "';");
        }

        public List<Discurso> ObterDiscursoTemas()
        {
            List<Discurso> res = new();

            using (Database db = _connectionString)
            {
                string sql = "SELECT * FROM sys_discursos;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    res.Add(new Discurso()
                    {
                        NumDiscurso = result["NumDiscurso"],
                        TemaDiscurso = result["TemaDiscurso"]
                    });
                }
            }

            return res;
        }

        public Discurso ObterDiscursoTema(int id)
        {
            List<Discurso> res = new();

            using (Database db = _connectionString)
            {
                string sql = "SELECT * FROM sys_discursos where NumDiscurso=" + id + ";";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    res.Add(new Discurso()
                    {
                        NumDiscurso = result["NumDiscurso"],
                        TemaDiscurso = result["TemaDiscurso"]
                    });
                }
            }

            return res.DefaultIfEmpty(new Discurso()).First();
        }

        public List<Discurso> ObterDiscursos()
        {
            List<Discurso> res = new();
            List<Publicador> p = ObterPublicadores(false);

            using (Database db = _connectionString)
            {
                string sql = "SELECT * FROM r_discursos;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    res.Add(new Discurso()
                    {
                        Stamp = result["Stamp"],
                        Pub = p.Where(o => o.Id == result["IdPublicador"]).DefaultIfEmpty(new Publicador()).First(),
                        NomePublicador = result["NomePublicador"],
                        Congregacao = result["Congregacao"],
                        DataDiscurso = result["Data"],
                        NumDiscurso = result["NumDiscurso"],
                        TemaDiscurso = result["TemaDiscurso"]
                    });
                }
            }

            return res;
        }
        public Discurso ObterDiscurso(string stamp)
        {
            List<Discurso> res = new();
            List<Publicador> p = ObterPublicadores(false);

            using (Database db = _connectionString)
            {
                string sql = "SELECT * FROM r_discursos WHERE Stamp='" + stamp + "';";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    res.Add(new Discurso()
                    {
                        Stamp = result["Stamp"],
                        Pub = p.Where(o => o.Id == result["IdPublicador"]).DefaultIfEmpty(new Publicador()).First(),
                        NomePublicador = result["NomePublicador"],
                        Congregacao = result["Congregacao"],
                        DataDiscurso = result["Data"],
                        NumDiscurso = result["NumDiscurso"],
                        TemaDiscurso = result["TemaDiscurso"]
                    });
                }
            }

            return res.DefaultIfEmpty(new Discurso()).First();
        }


        public bool CriarDiscurso(Discurso d)
        {
            return ExecutarQuery("INSERT INTO r_discursos (Stamp, IdPublicador, NomePublicador, Congregacao, Data, NumDiscurso, TemaDiscurso) VALUES ('" + d.Stamp + "', " + d.Pub.Id + ", '" + d.NomePublicador + "', '" + d.Congregacao + "', '" + d.DataDiscurso.ToString("yyyyMMdd") + "', '" + d.NumDiscurso + "', '" + d.TemaDiscurso + "');");
        }

        public bool ApagarDiscurso(Discurso d)
        {
            return ExecutarQuery("DELETE FROM r_discursos WHERE Stamp='" + d.Stamp + "'");
        }
        #endregion
        
        
    }
}
