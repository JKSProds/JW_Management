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

        public List<Literatura> ObterPeriodicos(string stamp)
        {
            List<Literatura> LstLiteratura = new();
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = ConnectionString)
            {
                string sql = "select l_pubs.STAMP, l_pubs.Id, l_pubs.Referencia, l_pubs.Descricao, l_periodicos.Quantidade, l_pubs.IdTipo, sys_utilizadores.* from l_periodicos inner join l_pubs on l_pubs.Referencia=l_periodicos.Referencia left join sys_utilizadores on sys_utilizadores.IdUtilizador=l_periodicos.IdPublicador\r\nwhere Quantidade>IFNULL((SELECT SUM(Quantidade) from l_movimentos where l_pubs.STAMP=l_movimentos.StampLiteratura  and l_movimentos.IdPublicador=l_periodicos.IdPublicador), 0) and l_pubs.STAMP='" + stamp + "' order by Nome;";
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

        public bool NovaLiteratura(Literatura l)
        {

            string sql = "INSERT INTO l_pubs(Id, Referencia, Descricao, IdTipo, STAMP) VALUES ";
            sql += ("('" + l.Id + "', '" + l.Referencia + "', '" + l.Descricao + "', '" + l.Tipo.Id + "', '" + l.Stamp + "') ");
            sql += " ON DUPLICATE KEY UPDATE Id = VALUES(Id), Referencia = VALUES(Referencia), Descricao = VALUES(Descricao), IdTipo = VALUES(IdTipo);";

            return ExecutarQuery(sql);
        }

        public bool ApagarLiteratura(string stamp)
        {

            string sql = "DELETE FROM l_pubs where STAMP = '" + stamp + "';";
            sql += "DELETE FROM l_movimentos where StampLiteratura = '" + stamp + "';";

            return ExecutarQuery(sql);
        }


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
                        Nome = result["Nome"]
                    });
                }
            }

            return LstPublicador;
        }

        public List<Movimentos> ObterMovimentos(int tipo)
        {
            List<Movimentos> LstMovimentos = new();
            List<Publicador> LstPublicador = ObterPublicadores();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM l_movimentos where " + (tipo == 1 ? "Quantidade > 0" : "Quantidade < 0") + ";";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstMovimentos.Add(new Movimentos()
                    {
                        Stamp = result["Id"],
                        Literatura = ObterLiteratura(result["StampLiteratura"]),
                        Quantidade = int.Parse(result["Quantidade"]),
                        DataMovimento = DateTime.Parse(result["Data"]),
                        Publicador = LstPublicador.Where(g => g.Id == result["IdPublicador"]).FirstOrDefault(new Publicador())
                    });
                }
            }

            return LstMovimentos;
        }

        public bool AdicionarMovimento(Movimentos m)
        {
            string sql = "INSERT INTO l_movimentos(Id, StampLiteratura, Quantidade, Data, IdPublicador) VALUES ";
            sql += ("('" + m.Stamp + "', '" + m.Literatura.Stamp + "', '" + m.Quantidade + "', '" + m.DataMovimento.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + m.Publicador.Id + "');");

            return ExecutarQuery(sql);
        }
        public bool ApagarMovimento(string stamp)
        {

            string sql = "DELETE FROM l_movimentos where id = '" + stamp + "';";

            return ExecutarQuery(sql);
        }
    }
}
