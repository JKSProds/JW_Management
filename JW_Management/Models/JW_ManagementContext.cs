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
                db.Execute(@"CREATE DATABASE IF NOT EXISTS `@0` /*!40100 DEFAULT CHARACTER SET utf8 */;
                            DROP TABLE IF EXISTS `@0`.`dat_discursos`;
                            CREATE TABLE  `@0`.`dat_discursos` (
                              `IdDiscurso` int(10) unsigned NOT NULL auto_increment,
                              `NomeOrador` varchar(100) NOT NULL,
                              `CongregacaoOrador` varchar(100) default NULL,
                              `IdTemaDiscurso` int(10) unsigned NOT NULL,
                              `ContactoOrador` varchar(45) default NULL,
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
    }
}
