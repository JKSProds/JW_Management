using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Simple;

namespace JW_Management.Models
{
    public class JW_ManagementContext
    {
        public string ConnectionString { get; set; }

        public JW_ManagementContext(string connectionString)
        {
            this.ConnectionString = connectionString;
            CheckDatabase("sys_jw");
        }

        private void CheckDatabase(string DB_Nome)
        {

            Database db = ConnectionString;

            try
            {
                db.Connection.ChangeDatabase(DB_Nome);
            }
            catch (Exception)
            {
                db.Execute(@"DROP TABLE IF EXISTS `@0`.`dat_discursos`;
                            CREATE TABLE  `@0`.`dat_discursos` (
                              `IdDiscurso` int(10) unsigned NOT NULL auto_increment,
                              `NomeOrador` varchar(100) NOT NULL,
                              `CongregacaoOrador` varchar(100) default NULL,
                              `IdTemaDiscurso` int(10) unsigned NOT NULL,
                              `ContactoOrador` varchar(45) default NULL,
                              `EmailOrador` varchar(100) default NULL,
                              `Dentro_Fora` tinyint(1) NOT NULL default '0' COMMENT 'IN 0 - Saida 1',
                              `DataDiscurso` datetime NOT NULL,
                              `Observacoes` varchar(1024) default NULL,
                              PRIMARY KEY  USING BTREE (`IdDiscurso`,`IdTemaDiscurso`)
                            ) ENGINE=InnoDB DEFAULT CHARSET=utf8;
                            DROP TABLE IF EXISTS `@0`.`dat_temas`;
                            CREATE TABLE  `@0`.`dat_temas` (
                              `IdTemaDiscurso` int(10) unsigned NOT NULL auto_increment,
                              `TemaDiscurso` varchar(1024) NOT NULL,
                              PRIMARY KEY  USING BTREE (`IdTemaDiscurso`)
                            ) ENGINE=InnoDB DEFAULT CHARSET=utf8;", DB_Nome);
            }
            ConnectionString += "database=" + DB_Nome;
        }

        public List<CalendarEvent> ConverterDiscursosEventos(List<Discurso> discursos)
        {
            List<CalendarEvent> LstEventos = new List<CalendarEvent>();

            foreach (var item in discursos)
            {
                LstEventos.Add(new CalendarEvent
                {
                    id = item.IdDiscurso,
                    title = item.NomeOrador,
                    tema = item.TemaDiscurso,
                    obs = item.Observacoes,
                    congregacao = item.CongregacaoOrador,
                    contacto = item.ContactoOrador,
                    email = item.EmailOrador,
                    start = item.DataDiscurso,
                    end = item.DataDiscurso,
                    allDay = false,
                    color = (item.Dentro_Fora ? "#33FF77" : "#3371FF")
                });
            }

            return LstEventos;
        }
        public List<Discurso> ObterListaDiscursos()
        {
            List<Discurso> LstDiscursos = new List<Discurso>();

            Database db = ConnectionString;

            using (var result = db.Query("SELECT * FROM dat_discursos INNER JOIN dat_temas ON dat_discursos.IdTemaDiscurso=dat_temas.IdTemaDiscurso;"))
            {
                while (result.Read())
                {
                    LstDiscursos.Add(new Discurso()
                    {
                        IdDiscurso = result["IdDiscurso"],
                        NomeOrador = result["NomeOrador"],
                        CongregacaoOrador = result["CongregacaoOrador"].ToString().Count() < 0 ? "" : result["CongregacaoOrador"],
                        ContactoOrador = result["ContactoOrador"].ToString().Count() < 0 ? "" : result["ContactoOrador"],
                        EmailOrador = result["EmailOrador"].ToString().Count() < 0 ? "" : result["EmailOrador"],
                        DataDiscurso = result["DataDiscurso"],
                        IdTemaDiscurso = result["IdTemaDiscurso"],
                        TemaDiscurso = result["TemaDiscurso"],
                        Dentro_Fora = result["Dentro_Fora"],
                        Observacoes = result["Observacoes"].ToString().Count() < 0 ? "" : result["Observacoes"]

                });
                }
            }

            return LstDiscursos;
        }
        public Discurso ObterDiscurso(int id)
        {
            Discurso res = new Discurso();

            Database db = ConnectionString;

            using (var result = db.Query("SELECT * FROM dat_discursos INNER JOIN dat_temas ON dat_discursos.IdTemaDiscurso=dat_temas.IdTemaDiscurso WHERE IdDiscurso=" + id + " ;"))
            {
                result.Read();
                    res = new Discurso()
                    {
                        IdDiscurso = result["IdDiscurso"],
                        NomeOrador = result["NomeOrador"],
                        CongregacaoOrador = result["CongregacaoOrador"].ToString().Count() < 0 ? "" : result["CongregacaoOrador"],
                        ContactoOrador = result["ContactoOrador"].ToString().Count() < 0 ? "" : result["ContactoOrador"],
                        EmailOrador = result["EmailOrador"].ToString().Count() < 0 ? "" : result["EmailOrador"],
                        DataDiscurso = result["DataDiscurso"],
                        IdTemaDiscurso = result["IdTemaDiscurso"],
                        TemaDiscurso = result["TemaDiscurso"],
                        Dentro_Fora = result["Dentro_Fora"],
                        Observacoes = result["Observacoes"].ToString().Count() < 0 ? "" : result["Observacoes"]

                    };
            }

            return res;
        }
        public void ApagarDiscurso (int id)
        {
            Database db = ConnectionString;

            db.Execute("DELETE FROM dat_discursos where IdDiscurso=" + id);
        }
        public void AtualizarDiscurso(Discurso discurso, int id)
        {
            Database db = ConnectionString;

            db.Execute("UPDATE dat_discursos SET NomeOrador='" + discurso.NomeOrador + "', CongregacaoOrador='" + discurso.CongregacaoOrador + "', ContactoOrador='" + discurso.ContactoOrador + "', DataDiscurso='" + discurso.DataDiscurso.ToString("yyyy-MM-dd HH:mm:ss") + "', Observacoes='" + discurso.Observacoes + "', IdTemaDiscurso=" + discurso.IdTemaDiscurso + ", Dentro_Fora="+discurso.Dentro_Fora+", EmailOrador='"+discurso.EmailOrador+"' WHERE IdDiscurso=" + id + ";");
        }
        public void CriarDiscurso(Discurso discurso)
        {
            Database db = ConnectionString;

            db.Execute("INSERT INTO dat_discursos (NomeOrador, CongregacaoOrador, ContactoOrador, EmailOrador, DataDiscurso, Observacoes, IdTemaDiscurso, Dentro_Fora) VALUES ('" + discurso.NomeOrador + "', '" + discurso.CongregacaoOrador + "', '" + discurso.ContactoOrador + "', '" + discurso.EmailOrador + "', '" + discurso.DataDiscurso.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + discurso.Observacoes + "', " + discurso.IdTemaDiscurso + ", "+ discurso.Dentro_Fora +");");
        }
        public List<TemaDiscurso> ObterListaTemas()
        {
            List<TemaDiscurso> LstTemaDiscurso = new List<TemaDiscurso>();

            Database db = ConnectionString;

            using (var result = db.Query("SELECT * FROM dat_temas;"))
            {
                while (result.Read())
                {
                    LstTemaDiscurso.Add(new TemaDiscurso()
                    {
                        Id = result["IdTemaDiscurso"],
                        Tema = result["TemaDiscurso"],
                    });
                }
            }

            return LstTemaDiscurso;
        }

    }
}
