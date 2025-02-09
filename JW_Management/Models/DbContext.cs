﻿namespace JW_Management.Models
{
    using System.Diagnostics;
    using JW_Management.Controllers;
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
            FileContext f = new();
            f.ObterCaminhoLiteratura();

            using (Database db = ConnectionString)
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
                        Imagem = "data:image/jpeg;base64," + Convert.ToBase64String(f.ObterFicheiro(f.ObterCaminhoLiteratura() + result["Imagem"])),
                        Descricao = result["Descricao"],
                        Quantidade = result["Quantidade"],
                        Tipo = LstTiposLiteratura.Where(g => g.Id == result["IdTipo"]).FirstOrDefault(new TipoLiteratura()),
                        Publicador = LstPublicador.Where(g => g.Id == idpub).FirstOrDefault(new Publicador())
                    });

                    if (LstLiteratura.Last().Tipo.Id == 7)
                    {
                        LstLiteratura.Last().Imagem = LstLiteratura.Last().GetUrl();
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

            using (Database db = ConnectionString)
            {
                string sql = "SELECT IFNULL(l_form_order.Linha,0) as Linha, l_pubs.*, IFNULL((SELECT SUM(Quantidade) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura and Data < '" + dFinal.ToString("yyyy-MM-dd HH:mm:ss") + "' and IdPublicador = 0), 0) as Quantidade , IFNULL((SELECT SUM(CASE WHEN Quantidade > 0 THEN Quantidade ELSE 0 END) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura and Data between '" + dInicial.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + dFinal.ToString("yyyy-MM-dd HH:mm:ss") + "' and IdPublicador = 0), 0) as Entradas , IFNULL((SELECT SUM(CASE WHEN Quantidade < 0 THEN Quantidade ELSE 0 END) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura and Data between '" + dInicial.ToString("yyyy-MM-dd HH:mm:ss") + "' and '" + dFinal.ToString("yyyy-MM-dd HH:mm:ss") + "' and IdPublicador = 0), 0) as Saidas FROM l_pubs left join l_form_order on l_pubs.Referencia=l_form_order.Referencia;";

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
                        Linha = result["Linha"],
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
            FileContext f = new();

            using (Database db = ConnectionString)
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
                        Imagem = "data:image/jpeg;base64," + Convert.ToBase64String(f.ObterFicheiro(f.ObterCaminhoLiteratura() + result["Imagem"])),
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

        //Obter estados de pedidos
        public List<EstadoPedido> ObterEstadosPedido()
        {
            List<EstadoPedido> LstEstadoPedido = new();

            using (Database db = ConnectionString)
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
            List<Publicador> LstPublicadores = ObterPublicadores(false);

            using (Database db = ConnectionString)
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
            FileContext f = new();

            using (Database db = ConnectionString)
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
                        Imagem = "data:image/jpeg;base64," + Convert.ToBase64String(f.ObterFicheiro(f.ObterCaminhoLiteratura() + result["Imagem"])),
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

            using (Database db = ConnectionString)
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
                        Publicador = ObterPublicador(int.Parse(result["IdUtilizador"]), false, false, true, false)
                    });
                }
            }

            return LstLiteratura.ToList();
        }

        //Obter Publicadores
        public List<Publicador> ObterPublicadores(bool LoadGrupo)
        {
            List<Publicador> LstPublicador = new();
            List<Grupo> LstGrupos = new List<Grupo>();

            if (LoadGrupo) LstGrupos = ObterGrupos();

            using (Database db = ConnectionString)
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
                        TipoUtilizador = result["Tipo"]
                    });

                    if (LoadGrupo) LstPublicador.Last().Grupo = LstGrupos.Where(g => g.Id == int.Parse(result["IdGrupo"])).DefaultIfEmpty(new Grupo()).First();
                }
            }

            return LstPublicador;
        }

        //Obter Publicador
        public Publicador ObterPublicador(int id, bool LoadMovimentos, bool LoadPedidos, bool LoadGrupos, bool LoadTerritorios)
        {
            Publicador p = new();

            using (Database db = ConnectionString)
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
                        TipoUtilizador = result["Tipo"]
                    };
                    if (LoadMovimentos) p.Movimentos = ObterMovimentos(true, true, int.Parse(result["IdUtilizador"]), DateOnly.MinValue);
                    if (LoadPedidos)
                    {
                        p.Pedidos = ObterPedidosPeriodico(p.Id);
                        p.Pedidos.AddRange(ObterPedidosEspeciais(p.Id));
                    }
                    if (LoadGrupos) p.Grupo = ObterGrupo(int.Parse(result["IdGrupo"]));
                    if (LoadTerritorios) p.Territorios = ObterTerritorios("", true, false, false, false).Where(t => t.UltimoMovimento.Publicador?.Id == p.Id).ToList();
                }
            }

            return p;
        }

        //Adicionar publicador
        public bool AdicionarPublicador(Publicador p)
        {
            string sql = "INSERT INTO sys_utilizadores(IdUtilizador, Username, Password, Nome, Telemovel, Email, IdGrupo, Tipo) VALUES ";
            sql += ("('" + p.Id + "', '" + p.Username + "', '" + p.Password + "', '" + p.Nome + "', '" + p.Telemovel + "', '" + p.Email + "', '" + p.Grupo.Id + "', '90') ON DUPLICATE KEY UPDATE Username=VALUES(Username), Password=VALUES(Password), Telemovel=VALUES(Telemovel), Email=VALUES(Email), IdGrupo=VALUES(IdGrupo), Nome=VALUES(Nome);");

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
            DateOnly dF = data == DateOnly.MinValue ? DateOnly.MaxValue : data.AddDays(1);

            using (Database db = ConnectionString)
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
                        Publicador = ObterPublicador(int.Parse(result["IdPublicador"]), false, false, false, false)
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
            FileContext f = new();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT l_pedidos_periodicos.*, l_periodicos.Descricao, IFNULL((SELECT Caminho from l_img where l_periodicos.Referencia=l_img.Referencia), '') as Imagem from l_pedidos_periodicos inner join l_periodicos on l_pedidos_periodicos.Referencia = l_periodicos.Referencia;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstLiteratura.Add(new Literatura()
                    {
                        Stamp = result["Id"],
                        Referencia = result["Referencia"],
                        Imagem = "data:image/jpeg;base64," + Convert.ToBase64String(f.ObterFicheiro(f.ObterCaminhoLiteratura() + result["Imagem"])),
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
            Publicador p = ObterPublicador(IdPub, false, false, false, false);
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = ConnectionString)
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
            FileContext f = new();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT l_pedidos_especiais.*, l_pubs.Referencia, l_pubs.Descricao, l_pubs.IdTipo, IFNULL((SELECT Caminho from l_img where l_pubs.Referencia=l_img.Referencia), '') as Imagem from l_pedidos_especiais inner join l_pubs on l_pedidos_especiais.StampLiteratura = l_pubs.STAMP;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstLiteratura.Add(new Literatura()
                    {
                        Stamp = result["Id"],
                        Referencia = result["Referencia"],
                        Imagem = "data:image/jpeg;base64," + Convert.ToBase64String(f.ObterFicheiro(f.ObterCaminhoLiteratura() + result["Imagem"])),
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
            Publicador p = ObterPublicador(IdPub, false, false, false, false);
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = ConnectionString)
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

            using (Database db = ConnectionString)
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
                        Publicador = ObterPublicador(int.Parse(result["IdPublicador"]), false, false, false, false)
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

        #region Territorios
        //Obter todos os territorios
        public List<Territorio> ObterTerritorios(string filtro, bool LoadUltimoMovimento, bool LoadMovimentos, bool LoadFicheiros, bool LoadMovimentoInOut)
        {
            List<Territorio> LstTerritorios = new();
            List<TipoTerritorio> LstTipos = ObterTiposTerritorio();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT *, (SELECT StampMovimento FROM t_movimentos where StampTerritorio=t_territorios.Stamp Order By StampMovimento DESC LIMIT 1) as StampMovimento FROM t_territorios WHERE Id like '%" + filtro + "%' OR Nome like '%" + filtro + "%';";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstTerritorios.Add(new Territorio()
                    {
                        Stamp = result["Stamp"],
                        Id = result["Id"],
                        Nome = result["Nome"],
                        Descricao = result["Descricao"],
                        Dificuldade = result["Dificuldade"] == "0" ? DificuldadeTerritorio.FACIL : result["Dificuldade"] == "1" ? DificuldadeTerritorio.MODERADO : DificuldadeTerritorio.DIFICIL,
                        Tipo = LstTipos.Where(t => t.Id == result["IdTipo"]).FirstOrDefault(new TipoTerritorio()),
                        Url = result["Url"]
                    });
                    if (LoadUltimoMovimento)
                    {
                        LstTerritorios.Last().UltimoMovimento = ObterMovimentoTerritorio(result["StampMovimento"]);
                    }
                    if (LoadMovimentos)
                    {
                        LstTerritorios.Last().Movimentos = ObterMovimentosTerritorio(result["Stamp"]);
                    }
                    if (LoadFicheiros)
                    {
                        LstTerritorios.Last().Anexos = ObterAnexosTerritorio(result["Stamp"]);
                    }
                    if (LoadMovimentoInOut)
                    {
                        LstTerritorios.Last().Registros = ObterMovimentosTerritorios(LstTerritorios.Last());
                    }
                }
            }

            return LstTerritorios.OrderBy(l => l.Id).ToList();
        }

        //Obter um territorio em especifico
        public Territorio ObterTerritorio(string stamp, bool LoadUltimoMovimento, bool LoadMovimentos, bool LoadFicheiros, bool LoadMovimentoInOut)
        {
            Territorio t = new();
            List<TipoTerritorio> LstTipos = ObterTiposTerritorio();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT *, (SELECT StampMovimento FROM t_movimentos where StampTerritorio=t_territorios.Stamp Order By StampMovimento DESC LIMIT 1) as StampMovimento FROM t_territorios where Stamp='" + stamp + "';";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    t = new Territorio()
                    {
                        Stamp = result["Stamp"],
                        Id = result["Id"],
                        Nome = result["Nome"],
                        Descricao = result["Descricao"],
                        Dificuldade = result["Dificuldade"] == "0" ? DificuldadeTerritorio.FACIL : result["Dificuldade"] == "1" ? DificuldadeTerritorio.MODERADO : DificuldadeTerritorio.DIFICIL,
                        Tipo = LstTipos.Where(t => t.Id == result["IdTipo"]).FirstOrDefault(new TipoTerritorio()),
                        Url = result["Url"]
                    };

                    if (LoadUltimoMovimento)
                    {
                        t.UltimoMovimento = ObterMovimentoTerritorio(result["StampMovimento"]);
                    }
                    if (LoadMovimentos)
                    {
                        t.Movimentos = ObterMovimentosTerritorio(result["Stamp"]);
                    }
                    if (LoadFicheiros)
                    {
                        t.Anexos = ObterAnexosTerritorio(result["Stamp"]);
                    }
                    if (LoadMovimentoInOut)
                    {
                        t.Registros = ObterMovimentosTerritorios(t);
                    }
                }
            }

            return t;
        }

        //Adicionar um territorio ou atualizar existente
        public bool AdicionarTerritorio(Territorio t)
        {

            string sql = "INSERT INTO t_territorios(Stamp, Id, Nome, Descricao, Dificuldade, Url) VALUES ";
            sql += ("('" + t.Stamp + "', '" + t.Id + "', '" + t.Nome + "', '" + t.Descricao + "', '" + (t.Dificuldade == DificuldadeTerritorio.FACIL ? "0" : t.Dificuldade == DificuldadeTerritorio.MODERADO ? "1" : "2") + "', '" + t.Url + "') ");
            sql += " ON DUPLICATE KEY UPDATE Id = VALUES(Id), Nome = VALUES(Nome), Dificuldade = VALUES(Dificuldade), Descricao = VALUES(Descricao), Url = VALUES(Url);";

            return ExecutarQuery(sql);
        }

        //Apagar um territorio e todos os movimentos asssociados
        public bool ApagarTerritorio(string stamp)
        {
            string sql = "DELETE FROM t_territorios where Stamp='" + stamp + "';";
            sql += "DELETE FROM t_movimentos where StampTerritorio='" + stamp + "';";
            sql += "DELETE FROM t_anexos where StampTerritorio='" + stamp + "';";

            return ExecutarQuery(sql);
        }

        //Obter todos os movimentos
        public List<RegistroTerritorio> ObterMovimentosTerritorios()
        {
            List<RegistroTerritorio> LstLinhaMovimentosTerritorio = new();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT     entrada.stampmovimento AS stampmovimento_entrada,     entrada.stampterritorio AS stampterritorio,     entrada.DataMovimento AS DataEntrada,     entrada.tipoMovimento AS tipoMovimento_entrada,     MIN(saida.stampmovimento) AS stampmovimento_saida,     MIN(saida.tipoMovimento) AS tipoMovimento_saida,     COALESCE(MIN(saida.DataMovimento), '0001-01-01') AS DataSaida,     entrada.IdPublicador FROM t_movimentos entrada LEFT JOIN t_movimentos saida ON entrada.stampterritorio = saida.stampterritorio     AND saida.tipoMovimento = 2	     AND saida.stampmovimento > entrada.stampmovimento WHERE entrada.tipoMovimento = 1 GROUP BY entrada.stampterritorio, entrada.stampmovimento, entrada.DataMovimento, entrada.tipoMovimento, entrada.IdPublicador ORDER BY entrada.stampterritorio, entrada.stampmovimento; ";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    Territorio t = ObterTerritorio(result["stampterritorio"], false, false, false, false);

                    MovimentosTerritorio e = new MovimentosTerritorio()
                    {
                        Stamp = result["stampmovimento_entrada"],
                        Territorio = t,
                        Publicador = ObterPublicador(result["IdPublicador"], false, false, false, false),
                        DataMovimento = result["DataEntrada"],
                        Tipo = result["tipoMovimento_entrada"] == "1" ? TipoMovimentoTerritorio.ENTRADA : TipoMovimentoTerritorio.SAIDA
                    };
                    MovimentosTerritorio s = new MovimentosTerritorio()
                    {
                        Stamp = result["stampmovimento_saida"],
                        Territorio = t,
                        DataMovimento = result["DataSaida"],
                        Tipo = result["tipoMovimento_saida"] == "1" ? TipoMovimentoTerritorio.ENTRADA : TipoMovimentoTerritorio.SAIDA
                    };

                    LstLinhaMovimentosTerritorio.Add(new RegistroTerritorio()
                    {
                        Entrada = e,
                        Saida = s
                    });
                }
            }

            return LstLinhaMovimentosTerritorio.ToList();
        }

        //Obter todos os movimentos
        public List<RegistroTerritorio> ObterMovimentosTerritorios(Territorio t)
        {
            List<RegistroTerritorio> LstLinhaMovimentosTerritorio = new();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT entrada.stampmovimento AS stampmovimento_entrada, entrada.stampterritorio AS stampterritorio, entrada.DataMovimento AS DataEntrada, entrada.tipoMovimento AS tipoMovimento_entrada, saida.stampmovimento AS stampmovimento_saida, saida.tipoMovimento AS tipoMovimento_saida,  COALESCE(saida.DataMovimento, '0001-01-01') AS DataSaida, entrada.IdPublicador FROM t_movimentos entrada LEFT JOIN t_movimentos saida ON entrada.stampterritorio = saida.stampterritorio  AND saida.tipoMovimento = 2  AND saida.stampmovimento > entrada.stampmovimento WHERE entrada.tipoMovimento = 1 and entrada.stampterritorio='" + t.Stamp + "' ORDER BY entrada.stampterritorio, entrada.stampmovimento;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    MovimentosTerritorio e = new MovimentosTerritorio()
                    {
                        Stamp = result["stampmovimento_entrada"],
                        Territorio = t,
                        Publicador = ObterPublicador(result["IdPublicador"], false, false, false, false),
                        DataMovimento = result["DataEntrada"],
                        Tipo = result["tipoMovimento_entrada"] == "1" ? TipoMovimentoTerritorio.ENTRADA : TipoMovimentoTerritorio.SAIDA
                    };
                    MovimentosTerritorio s = new MovimentosTerritorio()
                    {
                        Stamp = result["stampmovimento_saida"],
                        Territorio = t,
                        DataMovimento = result["DataSaida"],
                        Tipo = result["tipoMovimento_saida"] == "1" ? TipoMovimentoTerritorio.ENTRADA : TipoMovimentoTerritorio.SAIDA
                    };

                    LstLinhaMovimentosTerritorio.Add(new RegistroTerritorio()
                    {
                        Entrada = e,
                        Saida = s
                    });
                }
            }

            return LstLinhaMovimentosTerritorio.ToList();
        }

        //Obter todos os movimentos de um territorio
        public List<MovimentosTerritorio> ObterMovimentosTerritorio(string stamp)
        {
            List<MovimentosTerritorio> LstMovimentosTerritorio = new();
            Territorio t = ObterTerritorio(stamp, false, false, false, false);

            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM t_movimentos where StampTerritorio='" + stamp + "';";
                using var result = db.Query(sql);
                while (result.Read())
                {

                    LstMovimentosTerritorio.Add(new MovimentosTerritorio()
                    {
                        Stamp = result["StampMovimento"],
                        Territorio = t,
                        Publicador = ObterPublicador(result["IdPublicador"], false, false, false, false),
                        DataMovimento = result["DataMovimento"],
                        Tipo = result["TipoMovimento"] == "1" ? TipoMovimentoTerritorio.ENTRADA : TipoMovimentoTerritorio.SAIDA

                    });
                }
            }

            return LstMovimentosTerritorio.OrderBy(l => l.Stamp).ToList();
        }

        //Obter um movimento de um territorio
        public MovimentosTerritorio ObterMovimentoTerritorio(string stamp)
        {
            MovimentosTerritorio m = new();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM t_movimentos where StampMovimento='" + stamp + "';";
                using var result = db.Query(sql);
                while (result.Read())
                {

                    m = new MovimentosTerritorio()
                    {
                        Stamp = result["StampMovimento"],
                        Territorio = ObterTerritorio(result["StampTerritorio"], false, false, false, false),
                        Publicador = ObterPublicador(result["IdPublicador"], false, false, false, false),
                        DataMovimento = result["DataMovimento"],
                        Tipo = result["TipoMovimento"] == "1" ? TipoMovimentoTerritorio.ENTRADA : TipoMovimentoTerritorio.SAIDA

                    };
                }
            }

            return m;
        }

        //Adicionar um movimento a um territorio
        public bool AdicionarMovimentoTerritorio(MovimentosTerritorio m)
        {

            string sql = "INSERT INTO t_movimentos(StampMovimento, StampTerritorio, IdPublicador, DataMovimento,TipoMovimento) VALUES ";
            sql += ("('" + m.Stamp + "', '" + m.Territorio.Stamp + "', '" + m.Publicador.Id + "', '" + m.DataMovimento.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + (m.Tipo == TipoMovimentoTerritorio.ENTRADA ? "1" : "2") + "');");
            ;
            return ExecutarQuery(sql);
        }

        //Apagar um movimento de um territorio em especifico
        public bool ApagarMovimentoTerritorio(string stamp)
        {

            string sql = "DELETE FROM t_movimentos where StampMovimento = '" + stamp + "';";

            return ExecutarQuery(sql);
        }


        //Obter todos os anexos de um territorio
        public List<AnexosTerritorio> ObterAnexosTerritorio(string stamp)
        {
            List<AnexosTerritorio> LstAnexosTerritorio = new();
            Territorio t = ObterTerritorio(stamp, false, false, false, false);

            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM t_anexos where StampTerritorio='" + stamp + "';";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstAnexosTerritorio.Add(new AnexosTerritorio()
                    {
                        Stamp = result["StampAnexo"],
                        Territorio = t,
                        NomeFicheiro = result["Ficheiro"],
                        CaminhoFicheiro = result["Caminho"],
                        Extensao = result["Extensao"],
                        Descricao = result["Descricao"]
                    });
                }
            }

            return LstAnexosTerritorio;
        }

        //Obter um anexo em especifico
        public AnexosTerritorio ObterAnexo(string stamp)
        {
            AnexosTerritorio a = new();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM t_anexos where StampAnexo='" + stamp + "';";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    a = new AnexosTerritorio()
                    {
                        Stamp = result["StampAnexo"],
                        Territorio = ObterTerritorio(result["StampTerritorio"], false, false, false, false),
                        NomeFicheiro = result["Ficheiro"],
                        CaminhoFicheiro = result["Caminho"],
                        Extensao = result["Extensao"],
                        Descricao = result["Descricao"]
                    };
                }
            }

            return a;
        }

        //Adicionar um anexo a um territorio
        public bool AdicionarAnexoTerritorio(AnexosTerritorio a)
        {

            string sql = "INSERT INTO t_anexos(StampAnexo, StampTerritorio, Descricao, Ficheiro, Caminho, Extensao) VALUES ";
            sql += ("('" + a.Stamp + "', '" + a.Territorio.Stamp + "', '" + a.Descricao + "', '" + a.NomeFicheiro + "', '" + a.CaminhoFicheiro.Replace("\\", "\\\\") + "', '" + a.Extensao + "');");

            return ExecutarQuery(sql);
        }

        //Apagar um anexo a um territorio
        public bool ApagarAnexoTerritorio(string stamp)
        {

            string sql = "DELETE FROM t_anexos where StampAnexo = '" + stamp + "';";

            return ExecutarQuery(sql);
        }

        //Obter todos os tipos de territorio
        public List<TipoTerritorio> ObterTiposTerritorio()
        {
            List<TipoTerritorio> t = new();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM t_tipos;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    t.Add(new TipoTerritorio()
                    {
                        Id = result["Id"],
                        Descricao = result["Descricao"]
                    });
                }
            }

            return t;
        }

        #endregion

        //REUNIOES
        #region Reunioes

        public List<Designacao> ObterDesignacoes(string Stamp)
        {
            List<Designacao> d = new();
            List<TipoDesignacao> t = ObterTiposDesignacao();
            List<Publicador> p = ObterPublicadores(false);

            using (Database db = ConnectionString)
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

            using (Database db = ConnectionString)
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

            using (Database db = ConnectionString)
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

                    if (LoadDesignacoes) r.Last().Designacoes = ObterDesignacoes(r.Last().Stamp).OrderBy(d => d.Stamp).ToList();

                }
            }

            return r;
        }
        public Reuniao ObterReuniao(string Stamp, bool LoadDesignacoes, bool LoadCanticos)
        {
            List<Reuniao> r = new();

            using (Database db = ConnectionString)
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

                    if (LoadDesignacoes) r.Last().Designacoes = ObterDesignacoes(r.Last().Stamp).OrderBy(d => d.Stamp).ToList();
                    if (LoadCanticos) r.Last().Canticos = ObterCanticos(r.Last().Stamp).OrderBy(d => d.Stamp).ToList();

                }
            }

            return r.First();
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

        public List<TipoDesignacao> ObterTiposDesignacao()
        {
            List<TipoDesignacao> t = new();

            using (Database db = ConnectionString)
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

            using (Database db = ConnectionString)
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

            using (Database db = ConnectionString)
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

            using (Database db = ConnectionString)
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

            using (Database db = ConnectionString)
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

            using (Database db = ConnectionString)
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

            using (Database db = ConnectionString)
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
