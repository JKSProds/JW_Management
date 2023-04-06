namespace JW_Management.Models
{
    using Microsoft.AspNetCore.Identity;
    using MySql.Simple;

    public class DbContext
    {
        public string ConnectionString { get; set; }
        public DbContext(string ConnString)
        {
            this.ConnectionString = ConnString;

            try
            {
                Database db = ConnectionString;
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
                Database db = ConnectionString;

                var res = db.Execute(SQL_Query);
                db.Connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;

            }
            return true;
        }

        //Obter todas as literaturas com os stocks especificos de um publicador e um filtro
        public List<Literatura> ObterLiteraturas(string filtro, int idpub)
        {
            List<Literatura> LstLiteratura = new();
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();
            List<Publicador> LstPublicador = ObterPublicadores();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT *, IFNULL((SELECT SUM(Quantidade) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura and l_movimentos.IdPublicador=" + idpub + "), 0) as Quantidade FROM l_pubs where Descricao like '%" + filtro + "%' or Referencia like '%" + filtro + "%';";
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
                        Publicador = LstPublicador.Where(g => g.Id == idpub).FirstOrDefault(new Publicador())
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

            using (Database db = ConnectionString)
            {
                string sql = "SELECT *, IFNULL((SELECT SUM(Quantidade) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura), 0) as Quantidade FROM l_pubs where STAMP='" + STAMP + "';";
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

            using (Database db = ConnectionString)
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

        //Obter todos os grupos
        public List<Grupo> ObterGrupos()
        {
            List<Grupo> LstGrupos = new();
            List<Publicador> LstPublicadores = ObterPublicadores();

            using (Database db = ConnectionString)
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
            List<Publicador> LstPublicadores = ObterPublicadores();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM l_grupos where Id="+id+";";
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

            string sql = "INSERT INTO l_pubs(Id, Referencia, Descricao, IdTipo, STAMP) VALUES ";
            sql += ("('" + l.Id + "', '" + l.Referencia + "', '" + l.Descricao + "', '" + l.Tipo.Id + "', '" + l.Stamp + "') ");
            sql += " ON DUPLICATE KEY UPDATE Id = VALUES(Id), Referencia = VALUES(Referencia), Descricao = VALUES(Descricao), IdTipo = VALUES(IdTipo);";

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

            using (Database db = ConnectionString)
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

            using (Database db = ConnectionString)
            {
                string sql = "SELECT *, IFNULL((SELECT SUM(Quantidade) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura and l_movimentos.IdPublicador=0), 0) as Quantidade FROM l_pubs where IdTipo=7 and IFNULL((SELECT SUM(Quantidade) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura), 0) > 0;";
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
                        Tipo = LstTiposLiteratura.Where(g => g.Id == result["IdTipo"]).FirstOrDefault(new TipoLiteratura())
                    });
                }
            }

            return LstLiteratura.OrderBy(l => l.Descricao.Trim()).ToList();
        }

        //Obter todos os periodicos especificos de uma literatura para entregar aos publicador
        public List<Literatura> ObterPeriodicos(string stamp)
        {
            List<Literatura> LstLiteratura = new();
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = ConnectionString)
            {
                string sql = "select l_pubs.STAMP, l_pubs.Id, l_pubs.Referencia, l_pubs.Descricao, l_pedidos_periodicos.Quantidade, l_pubs.IdTipo, sys_utilizadores.* from l_pedidos_periodicos inner join l_pubs on l_pubs.Referencia=l_pedidos_periodicos.Referencia left join sys_utilizadores on sys_utilizadores.IdUtilizador=l_pedidos_periodicos.IdPublicador\r\nwhere Quantidade>IFNULL((SELECT SUM(Quantidade) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura  and l_movimentos.IdPublicador=l_pedidos_periodicos.IdPublicador), 0) and l_pubs.STAMP='" + stamp + "' order by Nome;";
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
                        Publicador = new Publicador() { Id = result["IdUtilizador"], Nome = result["Nome"] }
                    });
                }
            }

            return LstLiteratura.OrderBy(l => l.Descricao.Trim()).ToList();
        }

        //Obter Publicadores
        public List<Publicador> ObterPublicadores()
        {
            List<Publicador> LstPublicador = new();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM sys_utilizadores order by Nome;";
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
                        Telemovel = result["Telemovel"]
                    });
                }
            }

            return LstPublicador;
        }

        //Obter Publicador
        public Publicador ObterPublicador(int id, bool LoadMovimentos, bool LoadPedidos, bool LoadGrupos)
        {
            Publicador p = new();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM sys_utilizadores where IdUtilizador="+id+";";
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
                        Telemovel = result["Telemovel"]
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
            string sql = "INSERT INTO sys_utilizadores(IdUtilizador, Username, Password, Nome, Telemovel, Email, IdGrupo) VALUES ";
            sql += ("('" + p.Id + "', '" + p.Username + "', '" + p.Password + "', '" + p.Nome + "', '" + p.Telemovel + "', '" + p.Email + "', '" + p.Grupo.Id + "') ON DUPLICATE KEY UPDATE Username=VALUES(Username), Password=VALUES(Password), Telemovel=VALUES(Telemovel), Email=VALUES(Email), IdGrupo=VALUES(IdGrupo), Nome=VALUES(Nome);");

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

        //Obter movimentos de um determinado tipo 
        public List<Movimentos> ObterMovimentos(bool In, bool Out, int IdPub, DateOnly data)
        {
            List<Movimentos> LstMovimentos = new();
            Publicador p = ObterPublicador(IdPub,false,false,false);
            DateOnly dF = data == DateOnly.MinValue ? DateOnly.MaxValue : data.AddDays(1);

            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM l_movimentos where IdPublicador="+IdPub + " AND Data between '"+ data.ToString("yyyy-MM-dd") + "' and '"+ dF.ToString("yyyy-MM-dd") + "' AND  (" + (In ? "Quantidade > 0" : "") + (In && Out ? " OR ": "") + (Out ? "Quantidade < 0" : "") + ") ;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstMovimentos.Add(new Movimentos()
                    {
                        Stamp = result["Id"],
                        Literatura = ObterLiteratura(result["StampLiteratura"]),
                        Quantidade = int.Parse(result["Quantidade"]),
                        DataMovimento = DateTime.Parse(result["Data"]),
                        Publicador = p
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
            List<Publicador> LstPublicador = ObterPublicadores();
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT l_pedidos_periodicos.*, l_periodicos.Descricao from l_pedidos_periodicos inner join l_periodicos on l_pedidos_periodicos.Referencia = l_periodicos.Referencia;";
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
            Publicador p = ObterPublicador(IdPub, false, false,false);
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT l_pedidos_periodicos.*, l_periodicos.Descricao from l_pedidos_periodicos inner join l_periodicos on l_pedidos_periodicos.Referencia = l_periodicos.Referencia Where l_pedidos_periodicos.IdPublicador="+IdPub+";";
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
            sql += ("('" + l.Stamp + "', '" + l.Publicador.Id + "', '" + l.Quantidade + "', '" + l.Referencia + "') ON DUPLICATE KEY UPDATE Quantidade=Quantidade+"+l.Quantidade+";");

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
            List<Publicador> LstPublicador = ObterPublicadores();
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT l_pedidos_especiais.*, l_pubs.Referencia, l_pubs.Descricao, l_pubs.IdTipo from l_pedidos_especiais inner join l_pubs on l_pedidos_especiais.StampLiteratura = l_pubs.STAMP;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstLiteratura.Add(new Literatura()
                    {
                        Stamp = result["Id"],
                        Referencia = result["Referencia"],
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
            Publicador p = ObterPublicador(IdPub, false, false,false);
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT l_pedidos_especiais.*, l_pubs.Referencia, l_pubs.Descricao, l_pubs.IdTipo from l_pedidos_especiais inner join l_pubs on l_pedidos_especiais.StampLiteratura = l_pubs.STAMP Where IdPublicador = "+IdPub+";";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstLiteratura.Add(new Literatura()
                    {
                        Stamp = result["Id"],
                        Referencia = result["Referencia"],
                        Descricao = result["Descricao"],
                        Quantidade = int.Parse(result["Quantidade"]),
                        Tipo = LstTiposLiteratura.Where(g => g.Id == result["IdTipo"]).FirstOrDefault(new TipoLiteratura()),
                        Publicador = p
                    });
                }
            }

            return LstLiteratura;
        }

        //Adicionar pedidos especiais
        public bool AdicionarPedidoEspecial(Literatura l, EstadoPedido e)
        {
            string sql = "INSERT INTO l_pedidos_especiais(Id, IdPublicador, Quantidade, StampLiteratura, Estado, Fornecido) VALUES ";
            sql += ("('" + DateTime.Now.Ticks.ToString() + "', '" + l.Publicador.Id + "', '" + l.Quantidade + "', '" + l.Stamp + "', '"+e.Id+"', "+e.Fornecido+") ON DUPLICATE KEY UPDATE Quantidade=Quantidade+" + l.Quantidade + ";");

            return ExecutarQuery(sql);
        }

        //Atualiza o estado de um pedido
        public bool AtualizarEstadoPedidoEspecial(Literatura l, EstadoPedido e)
        {
            string sql = "UDPATE l_pedidos_especiais SET Estado='"+e.Id+"', Fornecido='"+e.Fornecido+"' WHERE Id='"+l.Stamp+"';";

            return ExecutarQuery(sql);
        }

        //Apagar um pedido especial em especifico
        public bool ApagarPedidoEspecial(string stamp)
        {

            string sql = "DELETE FROM l_pedidos_especiais where id = '" + stamp + "';";

            return ExecutarQuery(sql);
        }
    }
}
