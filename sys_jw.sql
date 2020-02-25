-- MySQL dump 10.10
--
-- Host: 10.0.0.241    Database: sys_jw
-- ------------------------------------------------------
-- Server version	5.7.29

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `sys_jw`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `sys_jw` /*!40100 DEFAULT CHARACTER SET latin1 */;

USE `sys_jw`;

--
-- Table structure for table `dat_discursos`
--

DROP TABLE IF EXISTS `dat_discursos`;
CREATE TABLE `dat_discursos` (
  `IdDiscurso` int(10) unsigned auto_increment,
  `NomeOrador` varchar(100),
  `CongregacaoOrador` varchar(100),
  `IdTemaDiscurso` int(10) unsigned,
  `ContactoOrador` varchar(45),
  `EmailOrador` varchar(100),
  `Dentro_Fora` tinyint(1) DEFAULT '0',
  `DataDiscurso` datetime,
  `Observacoes` varchar(1024),
  PRIMARY KEY (`IdDiscurso`,`IdTemaDiscurso`)
)/*! engine=InnoDB */;

--
-- Dumping data for table `dat_discursos`
--


/*!40000 ALTER TABLE `dat_discursos` DISABLE KEYS */;
LOCK TABLES `dat_discursos` WRITE;
UNLOCK TABLES;
/*!40000 ALTER TABLE `dat_discursos` ENABLE KEYS */;

--
-- Table structure for table `dat_temas`
--

DROP TABLE IF EXISTS `dat_temas`;
CREATE TABLE `dat_temas` (
  `IdTemaDiscurso` int(10) unsigned auto_increment,
  `TemaDiscurso` varchar(1024),
  PRIMARY KEY (`IdTemaDiscurso`)
)/*! engine=InnoDB */;

--
-- Dumping data for table `dat_temas`
--


/*!40000 ALTER TABLE `dat_temas` DISABLE KEYS */;
LOCK TABLES `dat_temas` WRITE;
INSERT INTO `dat_temas` VALUES (1,'Quão Bem conhece a Deus?'),(2,'Será Um dos Sobreviventes dos Últimos Dias?'),(3,'Sirvamos com a Organização Unificada de Jeová'),(4,'Evidencias de Deus no Mundo que Nos Rodeia'),(5,'Vida Familiar que Anima o Coração'),(6,'O Dilúvio dos dias de Noé tem Significado para Nós'),(7,'Misericórdia, Qualidade Dominante dos Verdadeiros Cristãos'),(8,'Viva, não para Si mesmo, mas para Fazer a Vontade de Deus'),(9,'Nunca Fique Obtuso no Ouvir'),(10,'Comportemo-nos Honestamente em Todas as Ocasiões'),(11,'Não Fazemos Parte do Mundo - Em Imitação de Cristo'),(12,'O Respeito Pela Autoridade Serve de Protecção'),(13,'O Conceito Piedoso Sobre o Sexo e o Casamento'),(14,'Um Povo Limpo Honra a Jeová'),(15,'Como Cristãos, Importamo-nos com os Outros'),(16,'Continue a Desenvolver-se em sua Relação com Deus'),(17,'Glorifiquemos a Deus com Tudo o Que Temos'),(18,'Faz Realmente de Jeová a Sua Fortaleza?'),(19,'Como Poderá Saber Seu Futuro?'),(20,'Chegou o Tempo de Deus Governar o Mundo?'),(21,'Onde se Enquadra Você no Arranjo do Reino?'),(22,'Está Contente com as Provisões de Jeová?'),(23,'A Vida Tem Objectivo'),(24,'O Que o Governo de Deus Pode Fazer por Nós'),(25,'Resista ao Espirito do Mundo'),(26,'Será que Deus Considera Você Importante?'),(27,'Como Dar Bom Inicio ao Casamento'),(28,'Mostre Respeito e Amor no Seu Casamento'),(29,'Responsabilidades e Recompensas dos Pais'),(30,'Comunicação - Dentro da Família e com Deus'),(31,'Felizes Embora Famintos - Como Pode Ser?'),(32,'Como Lidar com as Ansiedades da Vida'),(33,'O Que Há Por Trás do Espirito de Rebeldia?'),(34,'Foi Marcado Para Sobreviver?'),(35,'Poderá Viver Para Sempre? Viverá Mesmo?'),(36,'É Esta Vida Tudo o Que Há?'),(37,'Decida-se Agora a Favor do Governo Divino'),(38,'Actue Sabiamente ao Passo que se Aproxima o Fim'),(39,'Confie na Vitória Divina!'),(40,'O Que o Futuro Próximo nos Reserva!'),(41,'Fique Pardo e Veja a Salvação da Parte de Jeová'),(42,'O Efeito do Reino de Deus Sobre Você'),(43,'Faz o Que Deus Requer de Você?'),(44,'Persista em Buscar o Reino de Deus'),(45,'Siga o Caminho da Vida'),(46,'Tenha Firme Confiança Até ao Fim'),(47,'Tenha Fé nas Boas Novas'),(48,'Como Passar Pela Prova da Lealdade Cristã'),(49,'Uma Terra Purificada: Viverá Para Vê-la?'),(50,'Como Tomará as Decisões com que se Confronta?'),(51,'Está a Verdade Transformando Sua Vida?'),(52,'Quem é o Seu Deus?'),(53,'Concorda Seu Modo de Pensar com o de Deus?'),(54,'Edifique Sua Fé no Criador do Homem'),(55,'Que Nome Está Fazendo Perante Deus?'),(56,'Entremos no Novo Mundo Sob a Liderança de Cristo'),(57,'Como Suportar Perseguição'),(58,'De que Modo Deve Servir a Deus?'),(59,'Ceifará o que Semear'),(60,'Quão Significativa é a Sua Vida?'),(61,'Nas Promessas de Quem Confia?'),(62,'A Única Cura Para a Humanidade Doentia'),(63,'Tem Você Espirito Evangelizador?'),(64,'Amantes de Prazeres ou Amantes de Deus?'),(65,'Sinta Honra e Alegria no Ministério de Deus'),(66,'Trabalhe como Escravo Para o Senhor da Colheita'),(67,'Tome Tempo Para Meditar em Coisas Espirituais'),(68,'Guarda Ressentimento ou é Perdoador?'),(69,'Renove o Espirito de Abnegação'),(70,'Faça de Jeová a Sua Confiança'),(71,'Como Manter-se Espiritualmente Desperto'),(72,'O Amor Identifica a Verdadeira Congregação Cristã'),(73,'Como Adquirir um Coração Sábio'),(74,'Os Olhos de Jeová nos Observam'),(75,'Reconhece a Soberania de Jeová em Sua Vida?'),(76,'Ciúme: O Correcto e o Incorrecto'),(77,'Siga o Proceder da Hospitalidade'),(78,'Sirva a Jeová com Coração Alegre'),(79,'Amizade com Deus ou com o Mundo: Qual Escolherá?'),(80,'Baseia a Sua Esperança na Ciência ou na Bíblia?'),(81,'Quem Está Habilitado para ser Ministro de Deus?'),(82,'Jeová e Cristo Fazem Parte de uma Trindade?'),(83,'Tempo de julgamento da Religião'),(84,'Escapará do Destino deste mundo?'),(85,'Boas Noticias num Mundo Violento'),(86,'Orações que São Ouvidas por Deus'),(87,'Qual é a Sua Relação com Deus?'),(88,'Por que Viver Segundo as Normas da Bíblia?'),(89,'Venham os que Tem Sede da Verdade!'),(90,'Procure Alcançar a Verdadeira Vida'),(91,'A presença do Messias e Seu Domínio'),(92,'O Papel da Religião nos Assuntos do Mundo'),(93,'Catástrofes Naturais - Como as Encara?'),(94,'A Religião Verdadeira Atende ás Necessidades da Soc Humana'),(95,'O Conceito da Bíblia Sobre Práticas Espiritas'),(96,'O Fim da Religião Falsa está Próximo'),(97,'Permaneçamos Inculpes em Meio a Uma Geração Pervertida'),(98,'Mantenha-se Limpo dos Aviltamentos do Mundo'),(99,'Porque Pode Confiar na Bíblia'),(100,'Amizade Verdadeira com Deus e o Próximo'),(101,'Jeová - O Grandioso Criador'),(102,'Preste Atenção á Palavra Profética'),(103,'Pode-se Encontrar Alegria em Servir a Deus'),(104,'Pais - Estão Construindo com Materiais á Prova de Fogo?'),(105,'Somos Consolados em Todas as Nossas Tribulações'),(106,'Arruinar a Terra Provocará Retribuição Divina'),(107,'Tenha Uma Boa Consciência Neste Mundo Pecaminoso'),(108,'Vença o Medo do Futuro'),(109,'O Reino de Deus Está Próximo'),(110,'Deus Vem Primeiro na Vida Familiar Bem Sucedida'),(111,'O Que é Realizado pela Cura das Nações?'),(112,'Como Expressar Amor num Mundo que Viola a Lei'),(113,'Como Podem os Jovens Enfrentar a Crise Actual?'),(114,'Apreço pelas Maravilhas da Criação de Deus'),(115,'Como Proteger-nos Contra os Laços de Satanás'),(116,'Escolha Sabiamente com Quem irá Associar-se!'),(117,'Como Vencer o Mal com o Bem'),(118,'Olhemos os Jovens do Ponto de Vista de Jeová'),(119,'Porque é Benéfico que os Cristãos Vivam Separados do Mundo'),(120,'Por Que Submeter-se á Regência de Deus Agora'),(121,'Uma Fraternidade Mundial que será Salva da Calamidade'),(122,'Paz Global - de Onde Virá?'),(123,'Por que os Cristãos Tem de Ser Diferentes'),(124,'Razões para Crer que a Bíblia é de Autoria Divina'),(125,'Por que a Humanidade Precisa de Um Resgate'),(126,'Quem se Salvará?'),(127,'O Que Acontece Quando Morremos?'),(128,'É o Inferno um Lugar de Tormento Ardente?'),(129,'É a Trindade um Ensino Divino?'),(130,'A Terra Permanecerá para Sempre'),(131,'Será que o Diabo Realmente Existe?'),(132,'Ressurreição - A Vitória sobre a Morte!'),(133,'Tem Importância o que Cremos sobre Nossa Origem'),(134,'Devem os Cristãos Guardar o Sábado?'),(135,'A Santidade da Vida e do Sangue'),(136,'Será que Deus aprova o uso de Imagens na Adoração?'),(137,'Ocorreram Realmente os Milagres da Bíblia?'),(138,'Viva com Bom Juízo num Mundo Depravado'),(139,'Sabedoria Divina num Mundo Cientifico'),(140,'Jesus Cristo, o Novo Governante da Terra'),(141,'Quando Terão fim os Gemidos da Criação Humana?'),(142,'Porque Refugiar-se em Jeová?'),(143,'Confie no Deus de Todo o Consolo'),(144,'Uma Congregação Leal sob a liderança de Cristo.'),(145,'Quem é Semelhante a Jeová, o Nosso Deus?'),(146,'Use a Educação para Louvar a Jeová'),(147,'Confie no Poder Salvador de Jeová'),(148,'Tem o mesmo Conceito de Deus sobre a Vida'),(149,'O que Significa \"Andar com Deus\"?'),(150,'Deus é Real para Si?'),(151,'Jeová é \"Uma Altura Protectora\" para Seu Povo'),(152,'O Verdadeiro Armagedom - Porquê e Quando?'),(153,'Tenha Bem em Mente o \"Atemorizante Dia\"'),(154,'O Governo Humano é Pesado na Balança'),(155,'Chegou a Hora do Julgamento de Babilónia?'),(156,'Dia do Juízo - Tempo de Temor ou de Esperança?'),(157,'Como os verdadeiros cristãos adornam o Ensino Divino'),(158,'Seja corajoso e confie em Jeová'),(159,'Como encontrar Segurança num mundo perigoso?'),(160,'Mantenha a identidade Cristã'),(161,'Porque Jesus sofreu a morte'),(162,'Seja liberto deste mundo de escuridão'),(163,'Porque Temer o Deus Verdadeiro'),(164,'Será que Deus Ainda está no controlo'),(165,'Os valores de quem preza?'),(166,'Como enfrentar o futuro com fé e coragem?'),(167,'Ajamos sabiamente num mundo insensato'),(168,'Pode sentir-se seguro neste mundo atribulado?'),(169,'Porque ser orientado pela Biblia?'),(170,'Quem está qualificado para governar a Humanidade?'),(171,'Poderá viver em paz agora e para sempre!'),(172,'Que reputação tem perante Deus?'),(173,'Existe  uma religiao verdadeira do ponto de vista  de Deus?'),(174,'Quem se qualificara  para  entrar no novo mundo de Deus?'),(175,'O que prova que a Bıblia e autentica?'),(176,'Quando havera  verdadeira paz e segurança?'),(177,'Onde encontrar ajuda em tempos  de aflic¸ao?'),(178,'Ande no caminho  da integridade'),(179,'Rejeite  as fantasias do mundo,  empenhe-se  pelas realidades do Reino'),(180,'A ressurreicão - por que essa esperança deve ser  real para  voce'),(181,'Ja e mais  tarde  do que voce imagina?'),(182,'O que o Reino de Deus esta  fazendo por nos agora?'),(183,'Desvie seus olhos do que e futil'),(184,'A morte é o fim de tudo?'),(185,'Será que a verdade influencia a sua vida? '),(186,'Sirva em união com o povo feliz de deus'),(187,'Por que um deus amoroso permite a maldade?'),(188,'Confia em Jeová?'),(189,'Andar com Deus traz bençãos agora e para sempre'),(190,' Como se cumprirá a promessa de perfeita felicidade familiar'),(191,'Como é Que o Amor e a Fé Vencem o Mundo'),(192,'\"Está a Caminhar na Estrada Para a Vida Eterna?\"'),(193,'Os Problemas de Hoje Logo Serão Coisa do Passado'),(194,'Como a Sabedoria de Deus nos Ajuda');
UNLOCK TABLES;
/*!40000 ALTER TABLE `dat_temas` ENABLE KEYS */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

