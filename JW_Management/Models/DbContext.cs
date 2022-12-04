namespace JW_Management.Models
{
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

        public List<Literatura> ObterLiteratura(int idGrupo, string filtro)
        {
            List<Literatura> LstLiteratura = new List<Literatura>();
            List<Grupo> LstGrupos = ObterGrupos();
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM dat_literatura where IdGrupo="+idGrupo+" and Descricao like '%"+filtro+"%';";
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
                        Grupo = LstGrupos.Where(g => g.Id == result["IdGrupo"]).FirstOrDefault(new Grupo()),
                        Tipo = LstTiposLiteratura.Where(g => g.Id == result["IdTipo"]).FirstOrDefault(new TipoLiteratura())
                    });
                }
            }

            return LstLiteratura.OrderBy(l => l.Descricao.Trim()).ToList();
        }

        public Literatura ObterLiteratura(string STAMP)
        {
            List<Literatura> LstLiteratura = new List<Literatura>();
            List<Grupo> LstGrupos = ObterGrupos();
            List<TipoLiteratura> LstTiposLiteratura = ObterTiposLiteratura();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM dat_literatura where STAMP='" + STAMP + "';";
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
                        Grupo = LstGrupos.Where(g => g.Id == result["IdGrupo"]).FirstOrDefault(new Grupo()),
                        Tipo = LstTiposLiteratura.Where(g => g.Id == result["IdTipo"]).FirstOrDefault(new TipoLiteratura())
                    });
                }
            }

            return LstLiteratura.OrderBy(l => l.Descricao.Trim()).FirstOrDefault(new Literatura());
        }

        public bool AtualizarLiteratura(Literatura l)
        {

            string sql = "INSERT INTO dat_literatura(Id, Referencia, Descricao, Quantidade, IdGrupo, IdTipo) VALUES ";
            sql += ("('" + l.Id + "', '" + l.Referencia + "', '" + l.Descricao + "', '" + l.Quantidade + "', '" + l.Grupo.Id + "', '" + l.Tipo.Id + "') ");
            sql += " ON DUPLICATE KEY UPDATE Id = VALUES(Id), Referencia = VALUES(Referencia), Descricao = VALUES(Descricao), Quantidade = VALUES(Quantidade), IdGrupo = VALUES(IdGrupo), IdTipo = VALUES(IdTipo);";

            Database db = ConnectionString;

            var res = db.Execute(sql);
            db.Connection.Close();

            return res > 0;
        }


        public List<Grupo> ObterGrupos()
        {
            List<Grupo> LstGrupos = new List<Grupo>();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM dat_grupos;";
                using var result = db.Query(sql);
                while (result.Read())
                {
                    LstGrupos.Add(new Grupo()
                    {
                        Id = result["Id"],
                        Responsavel = result["Responsavel"],
                        Ajudante = result["Ajudante"],
                        Nome = result["Nome"]
                    });
                }
            }

            return LstGrupos;
        }
        public List<TipoLiteratura> ObterTiposLiteratura()
        {
            List<TipoLiteratura> LstTiposLiteratura = new List<TipoLiteratura>();

            using (Database db = ConnectionString)
            {
                string sql = "SELECT * FROM dat_tipos_literatura;";
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
    }
}
