-- MySQL dump 10.13  Distrib 5.7.23, for macos10.13 (x86_64)
--
-- Host: 10.0.0.244    Database: sys_jw_4143
-- ------------------------------------------------------
-- Server version	5.7.44

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
-- Table structure for table `l_estado_pedido`
--

DROP TABLE IF EXISTS `l_estado_pedido`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `l_estado_pedido` (
  `Id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Descricao` varchar(45) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `l_estado_pedido`
--

LOCK TABLES `l_estado_pedido` WRITE;
/*!40000 ALTER TABLE `l_estado_pedido` DISABLE KEYS */;
INSERT INTO `l_estado_pedido` VALUES (1,'Pendente'),(2,'Enviado Email'),(3,'Requisitado'),(4,'Fornecido');
/*!40000 ALTER TABLE `l_estado_pedido` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `l_grupos`
--

DROP TABLE IF EXISTS `l_grupos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `l_grupos` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Nome` varchar(50) DEFAULT NULL,
  `IdResponsavel` int(11) DEFAULT NULL,
  `IdAjudante` int(11) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `l_grupos`
--

LOCK TABLES `l_grupos` WRITE;
/*!40000 ALTER TABLE `l_grupos` DISABLE KEYS */;
/*!40000 ALTER TABLE `l_grupos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `l_img`
--

DROP TABLE IF EXISTS `l_img`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `l_img` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Referencia` varchar(45) NOT NULL,
  `Caminho` varchar(250) NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=69 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `l_img`
--

LOCK TABLES `l_img` WRITE;
/*!40000 ALTER TABLE `l_img` DISABLE KEYS */;
/*!40000 ALTER TABLE `l_img` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `l_movimentos`
--

DROP TABLE IF EXISTS `l_movimentos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `l_movimentos` (
  `Id` varchar(50) NOT NULL,
  `StampLiteratura` varchar(50) DEFAULT NULL,
  `Quantidade` decimal(10,0) DEFAULT NULL,
  `Data` datetime DEFAULT NULL,
  `IdPublicador` int(11) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `l_movimentos`
--

LOCK TABLES `l_movimentos` WRITE;
/*!40000 ALTER TABLE `l_movimentos` DISABLE KEYS */;
/*!40000 ALTER TABLE `l_movimentos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `l_pedidos_especiais`
--

DROP TABLE IF EXISTS `l_pedidos_especiais`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `l_pedidos_especiais` (
  `Id` varchar(50) NOT NULL,
  `StampLiteratura` varchar(50) DEFAULT NULL,
  `Quantidade` decimal(10,0) DEFAULT NULL,
  `IdPublicador` int(11) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Estado` varchar(45) NOT NULL,
  `Fornecido` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `l_pedidos_especiais`
--

LOCK TABLES `l_pedidos_especiais` WRITE;
/*!40000 ALTER TABLE `l_pedidos_especiais` DISABLE KEYS */;
/*!40000 ALTER TABLE `l_pedidos_especiais` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `l_pedidos_periodicos`
--

DROP TABLE IF EXISTS `l_pedidos_periodicos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `l_pedidos_periodicos` (
  `Id` varchar(25) NOT NULL,
  `Referencia` varchar(45) NOT NULL,
  `Quantidade` decimal(10,0) NOT NULL,
  `IdPublicador` int(10) unsigned NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `l_pedidos_periodicos`
--

LOCK TABLES `l_pedidos_periodicos` WRITE;
/*!40000 ALTER TABLE `l_pedidos_periodicos` DISABLE KEYS */;
/*!40000 ALTER TABLE `l_pedidos_periodicos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `l_periodicos`
--

DROP TABLE IF EXISTS `l_periodicos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `l_periodicos` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Referencia` varchar(10) DEFAULT NULL,
  `Descricao` varchar(100) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `l_periodicos`
--

LOCK TABLES `l_periodicos` WRITE;
/*!40000 ALTER TABLE `l_periodicos` DISABLE KEYS */;
INSERT INTO `l_periodicos` VALUES (1,'w','Sentinela de Estudo','1970-01-01 00:00:01'),(2,'wlp','Sentinela de Estudo - Letra Grande','1970-01-01 00:00:01'),(3,'mwb','Nossa Vida e Ministério Cristão - Apostila da Reunião','1970-01-01 00:00:01'),(4,'es','Examine as Escrituras Diariamente','1970-01-01 00:00:01'),(5,'eslp','Examine as Escrituras Diariamente - Letra Grande','1970-01-01 00:00:01'),(6,'wp','A Sentinela - Público','1970-01-01 00:00:01'),(7,'g','Despertai','1970-01-01 00:00:01'),(8,'dx','Indice','1970-01-01 00:00:01'),(9,'vol','Volume Encadernado','1970-01-01 00:00:01');
/*!40000 ALTER TABLE `l_periodicos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `l_pubs`
--

DROP TABLE IF EXISTS `l_pubs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `l_pubs` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `STAMP` varchar(50) DEFAULT NULL,
  `Referencia` varchar(10) DEFAULT NULL,
  `Descricao` varchar(100) DEFAULT NULL,
  `Data` varchar(100) DEFAULT NULL,
  `IdTipo` int(11) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=88625 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `l_pubs`
--

LOCK TABLES `l_pubs` WRITE;
/*!40000 ALTER TABLE `l_pubs` DISABLE KEYS */;
INSERT INTO `l_pubs` VALUES (3503,'43b5b7e9-73d7-11ed-940f-0242ac110004','pte','Envelope Plástico para Cartão de Território',NULL,5,'1970-01-01 00:00:01'),(3505,'43b5c4bc-73d7-11ed-940f-0242ac110004','bdg','Porta-cartão de lapela (Plástico)',NULL,5,'1970-01-01 00:00:01'),(3515,'43b5d188-73d7-11ed-940f-0242ac110004','ldcrt','Carrinho de publicações ',NULL,6,'1970-01-01 00:00:01'),(3520,'43b5ddeb-73d7-11ed-940f-0242ac110004','ldcrtadp','Carrinho - Peças de acrílico das prateleiras ',NULL,6,'1970-01-01 00:00:01'),(3521,'43b5ea67-73d7-11ed-940f-0242ac110004','ldcrtrcv','Carrinho - Capa para a chuva ',NULL,6,'1970-01-01 00:00:01'),(3522,'43b5fce4-73d7-11ed-940f-0242ac110004','ldcrtrkt','Carrinho - Toda a ferragem do carrinho ',NULL,6,'1970-01-01 00:00:01'),(3523,'43b60fc8-73d7-11ed-940f-0242ac110004','ldcrtwhl','Carrinho - Rodas ',NULL,6,'1970-01-01 00:00:01'),(3526,'43b61474-73d7-11ed-940f-0242ac110004','ldcrtmbd','Placa magnética ',NULL,6,'1970-01-01 00:00:01'),(5112,'43b622a1-73d7-11ed-940f-0242ac110004','nwtpkt','Tradução do Novo Mundo - Bolso',NULL,2,'1970-01-01 00:00:01'),(5117,'43b62742-73d7-11ed-940f-0242ac110004','nwtls','Tradução do Novo Mundo - Grande',NULL,2,'1970-01-01 00:00:01'),(5140,'43b62be4-73d7-11ed-940f-0242ac110004','nwt','Tradução do Novo Mundo',NULL,2,'1970-01-01 00:00:01'),(5326,'43b63d2a-73d7-11ed-940f-0242ac110004','sh','Busca de Deus ',NULL,3,'1970-01-01 00:00:01'),(5332,'43b641e5-73d7-11ed-940f-0242ac110004','od','Organizados ',NULL,3,'1970-01-01 00:00:01'),(5334,'43b64698-73d7-11ed-940f-0242ac110004','bh','O que a Bíblia Realmente Ensina?',NULL,3,'1970-01-01 00:00:01'),(5335,'43b64b43-73d7-11ed-940f-0242ac110004','lv','‘Amor de Deus ',NULL,3,'1970-01-01 00:00:01'),(5336,'43b64fea-73d7-11ed-940f-0242ac110004','yp2','Os Jovens Perguntam, Volume 2 ',NULL,3,'1970-01-01 00:00:01'),(5339,'43b654bc-73d7-11ed-940f-0242ac110004','yp1','Os Jovens Perguntam, Volume 1 ',NULL,3,'1970-01-01 00:00:01'),(5340,'43b65970-73d7-11ed-940f-0242ac110004','bhs',' O Que Nos Ensina a Bíblia?',NULL,3,'1970-01-01 00:00:01'),(5341,'43b65e20-73d7-11ed-940f-0242ac110004','sjj','Cantemos com Alegria a Jeová ',NULL,3,'1970-01-01 00:00:01'),(5343,'43b66306-73d7-11ed-940f-0242ac110004','lvs','Amar a Deus',NULL,3,'1970-01-01 00:00:01'),(5413,'43b667b4-73d7-11ed-940f-0242ac110004','jv','Proclamadores',NULL,3,'1970-01-01 00:00:01'),(5414,'43b66c4a-73d7-11ed-940f-0242ac110004','be','Escola do Ministério',NULL,3,'1970-01-01 00:00:01'),(5415,'43b670f4-73d7-11ed-940f-0242ac110004','lr','Instrutor ',NULL,3,'1970-01-01 00:00:01'),(5416,'43b674df-73d7-11ed-940f-0242ac110004','bt','Dê Testemunho Cabal sobre o Reino de Deus',NULL,3,'1970-01-01 00:00:01'),(5422,'43b678b5-73d7-11ed-940f-0242ac110004','kr','O Reino de Deus já Governa! ',NULL,3,'1970-01-01 00:00:01'),(5425,'43b68064-73d7-11ed-940f-0242ac110004','jy','Jesus - o Caminho, a Verdade e a Vida ',NULL,3,'1970-01-01 00:00:01'),(5427,'43b68432-73d7-11ed-940f-0242ac110004','lfb','Aprende com as Histórias da Bíblia ',NULL,3,'1970-01-01 00:00:01'),(5441,'43b68bcf-73d7-11ed-940f-0242ac110004','sjjls','Cantemos com Alegria a Jeová (Grande) ',NULL,3,'1970-01-01 00:00:01'),(5442,'43b690f7-73d7-11ed-940f-0242ac110004','sjjyls','Cantemos com Alegria a Jeová - S/Pauta',NULL,3,'1970-01-01 00:00:01'),(5445,'43b694cd-73d7-11ed-940f-0242ac110004','lff','Seja Feliz Para Sempre! (livro)',NULL,3,'1970-01-01 00:00:01'),(6219,'43b698a2-73d7-11ed-940f-0242ac110004','ialp','Imite Sua Fé',NULL,4,'1970-01-01 00:00:01'),(6630,'43b6a041-73d7-11ed-940f-0242ac110004','sp','Espíritos dos Mortos',NULL,4,'1970-01-01 00:00:01'),(6636,'43b6a41f-73d7-11ed-940f-0242ac110004','pr','Objetivo da Vida',NULL,4,'1970-01-01 00:00:01'),(6637,'43b6a7fb-73d7-11ed-940f-0242ac110004','we','Quando Morre Alguém',NULL,4,'1970-01-01 00:00:01'),(6642,'43b6afb4-73d7-11ed-940f-0242ac110004','ie','Quando Morremos',NULL,4,'1970-01-01 00:00:01'),(6645,'43b6b397-73d7-11ed-940f-0242ac110004','gf','Amigo de Deus ',NULL,4,'1970-01-01 00:00:01'),(6647,'43b6b759-73d7-11ed-940f-0242ac110004','la','Vida Satisfatória',NULL,4,'1970-01-01 00:00:01'),(6652,'43b6befc-73d7-11ed-940f-0242ac110004','bm','Mensagem da Bíblia',NULL,4,'1970-01-01 00:00:01'),(6654,'43b6c694-73d7-11ed-940f-0242ac110004','lc','A vida teve um Criador?',NULL,4,'1970-01-01 00:00:01'),(6655,'43b6ca5f-73d7-11ed-940f-0242ac110004','lf','A origem da vida ',NULL,4,'1970-01-01 00:00:01'),(6657,'43b6d2c9-73d7-11ed-940f-0242ac110004','ll','Escute e viva ',NULL,4,'1970-01-01 00:00:01'),(6658,'43b6d69a-73d7-11ed-940f-0242ac110004','ld','Escute a Deus',NULL,4,'1970-01-01 00:00:01'),(6659,'43b6da5a-73d7-11ed-940f-0242ac110004','fg','Boas Notícias de Deus Para Si!',NULL,4,'1970-01-01 00:00:01'),(6660,'43b6de40-73d7-11ed-940f-0242ac110004','jl','Vontade de Deus',NULL,4,'1970-01-01 00:00:01'),(6663,'43b6e5e0-73d7-11ed-940f-0242ac110004','mb','Minhas Primeiras Lições da Bíblia',NULL,4,'1970-01-01 00:00:01'),(6664,'43b6e9b6-73d7-11ed-940f-0242ac110004','yc','Ensine Seus Filhos',NULL,4,'1970-01-01 00:00:01'),(6665,'43b6edae-73d7-11ed-940f-0242ac110004','hf','Você Pode Ter uma Família Feliz!',NULL,4,'1970-01-01 00:00:01'),(6671,'43b6f1a7-73d7-11ed-940f-0242ac110004','rj','Volte para Jeová',NULL,4,'1970-01-01 00:00:01'),(6674,'43b6f576-73d7-11ed-940f-0242ac110004','np','Boas Novas para Pessoas de Todas as Nações',NULL,4,'1970-01-01 00:00:01'),(6684,'43b6f945-73d7-11ed-940f-0242ac110004','ypq','Respostas 10 Perguntas Que os Jovens Fazem',NULL,4,'1970-01-01 00:00:01'),(7074,'43b6fd2b-73d7-11ed-940f-0242ac110004','kt','Conhecer a Verdade ',NULL,1,'1970-01-01 00:00:01'),(7130,'43b700f9-73d7-11ed-940f-0242ac110004','T-30','O Que Acha da Bíblia?',NULL,1,'1970-01-01 00:00:01'),(7131,'43b704ff-73d7-11ed-940f-0242ac110004','T-31','O Que Espera do Futuro? ',NULL,1,'1970-01-01 00:00:01'),(7132,'43b708ca-73d7-11ed-940f-0242ac110004','T-32','Qual o Segredo para Ter uma Família Feliz? ',NULL,1,'1970-01-01 00:00:01'),(7133,'43b70c97-73d7-11ed-940f-0242ac110004','T-33','Quem controla o mundo?',NULL,1,'1970-01-01 00:00:01'),(7134,'43b71062-73d7-11ed-940f-0242ac110004','T-34','Sofrimento vai acabar?',NULL,1,'1970-01-01 00:00:01'),(7135,'43b71440-73d7-11ed-940f-0242ac110004','T-35','Será Que os Mortos Podem Voltar a Viver? ',NULL,1,'1970-01-01 00:00:01'),(7136,'43b7180d-73d7-11ed-940f-0242ac110004','T-36','O Que É o Reino de Deus?',NULL,1,'1970-01-01 00:00:01'),(7137,'43b71bd8-73d7-11ed-940f-0242ac110004','T-37','Respostas Importantes',NULL,1,'1970-01-01 00:00:01'),(7305,'43b71fa6-73d7-11ed-940f-0242ac110004','inv','Convite para reuniões congregacionais',NULL,1,'1970-01-01 00:00:01'),(8388,'43b7237e-73d7-11ed-940f-0242ac110004','ic','Cartão - Identificação (criança)',NULL,5,'1970-01-01 00:00:01'),(8410,'43b72748-73d7-11ed-940f-0242ac110004','jwcd1','Cartão de visita para jw.org',NULL,5,'1970-01-01 00:00:01'),(8422,'43b73514-73d7-11ed-940f-0242ac110004','jwcd3','Cartão de visita para jw.org',NULL,5,'1970-01-01 00:00:01'),(8703,'43b740e0-73d7-11ed-940f-0242ac110004','S-3','Relatório de Assistência às Reuniões',NULL,5,'1970-01-01 00:00:01'),(8704,'43b744b4-73d7-11ed-940f-0242ac110004','S-4','Relatório de Serviço de Campo',NULL,5,'1970-01-01 00:00:01'),(8712,'43b7488b-73d7-11ed-940f-0242ac110004','S-12','Cartão de Mapa de Território',NULL,5,'1970-01-01 00:00:01'),(8713,'43b74c89-73d7-11ed-940f-0242ac110004','S-13','Registo de Designação de Território',NULL,5,'1970-01-01 00:00:01'),(8721,'43b7507d-73d7-11ed-940f-0242ac110004','S-21','Cartão Registo de Publicador de Congregação',NULL,5,'1970-01-01 00:00:01'),(8724,'43b75452-73d7-11ed-940f-0242ac110004','S-24','Recibo (Maços de 100 unidades)',NULL,5,'1970-01-01 00:00:01'),(9172,'43b75bee-73d7-11ed-940f-0242ac110004','dpa','Cartão DPA',NULL,5,'1970-01-01 00:00:01'),(65423,'43b75fab-73d7-11ed-940f-0242ac110004','krlpi','O Reino de Deus já Governa! (L. Grande) ',NULL,3,'1970-01-01 00:00:01'),(65445,'43b7638e-73d7-11ed-940f-0242ac110004','lffi',' Seja Feliz Para Sempre! (brochura)',NULL,4,'1970-01-01 00:00:01'),(88521,'43b76b36-73d7-11ed-940f-0242ac110004','mvpnvt','Está Convidado',NULL,6,'1970-01-01 00:00:01'),(88541,'638168238467257092','na','O nome Divino Que Durará Para Sempre',NULL,4,'1970-01-01 00:00:01'),(88542,'638168239163785519','dg','Importa-se Deus realmente connosco? ',NULL,4,'1970-01-01 00:00:01'),(88543,'638168239663065153','gl','Veja a Boa Terra',NULL,4,'1970-01-01 00:00:01'),(88544,'638168240555919071','hb','Como pode o sangue salvar a sua vida? ',NULL,4,'1970-01-01 00:00:01'),(88546,'638168280121949712','jr','Jeremias',NULL,3,'1970-01-01 00:00:01'),(88547,'638168280443188172','wt','Adore a Deus',NULL,3,'1970-01-01 00:00:01'),(88548,'638168280971944694','rs','Raciocínios',NULL,3,'1970-01-01 00:00:01'),(88550,'638168281699794891','ct','Criador',NULL,3,'1970-01-01 00:00:01'),(88551,'638168281947220335','cf','Venha ser meu Seguidor',NULL,3,'1970-01-01 00:00:01'),(88553,'638168282501045326','sce','Criação ',NULL,3,'1970-01-01 00:00:01'),(88554,'638168282663386125','gt','O Maior Homem',NULL,3,'1970-01-01 00:00:01'),(88555,'638169105834782158','jwcd4','Cartão de visita para jw.org (apenas logótipo do jw.org)',NULL,5,'1970-01-01 00:00:01'),(88557,'638169375264747929','gm','Palavra de Deus?',NULL,3,'1970-01-01 00:00:01'),(88559,'638169829253506586','jd','Dia de Jeová',NULL,3,'1970-01-01 00:00:01'),(88560,'638169829536966738','rr','Adoração Pura',NULL,3,'1970-01-01 00:00:01'),(88561,'638169830127379441','th','Leitura e Ensino',NULL,4,'1970-01-01 00:00:01'),(88562,'638171618033205017','jwcd9','Cartão de visita para curso bíblico gratuito - Divulgar cursos bíblicos presenciais',NULL,6,'1970-01-01 00:00:01'),(88563,'638171618325045060','jwcd10','Cartão de visita para curso bíblico gratuito - Divulgar cursos bíblicos virtuais',NULL,6,'1970-01-01 00:00:01'),(88588,'638326489933628343','wfg','Aprenda com a Sabedoria de Jesus',NULL,4,'1970-01-01 00:00:01'),(88591,'638351556563009300','lmd','Ame as Pessoas – Faça Discípulos',NULL,4,'1970-01-01 00:00:01'),(88594,'638360675696950624','ed','As Testemunhas de Jeová e a Educação',NULL,4,'1970-01-01 00:00:01'),(88595,'638712338898954210','w','Sentinela de Estudo - Nº11 (2024)','202411',7,'1970-01-01 00:00:01'),(88596,'638712339025120230','wlp','Sentinela de Estudo - Letra Grande - Nº11 (2024)','202411',7,'1970-01-01 00:00:01'),(88597,'638712339096761626','w','Sentinela de Estudo - Nº12 (2024)','202412',7,'1970-01-01 00:00:01'),(88598,'638712339163758352','wlp','Sentinela de Estudo - Letra Grande - Nº12 (2024)','202412',7,'1970-01-01 00:00:01'),(88599,'638712339389007758','w','Sentinela de Estudo - Nº1 (2025)','202501',7,'1970-01-01 00:00:01'),(88600,'638712339453983960','wlp','Sentinela de Estudo - Letra Grande - Nº1 (2025)','202501',7,'1970-01-01 00:00:01'),(88601,'638712339622569204','mwb','Nossa Vida e Ministério Cristão - Apostila da Reunião - Nº3 (2025)','202503',7,'1970-01-01 00:00:01'),(88602,'638712341374134547','scl','Vida Cristã – Textos Bíblicos Úteis','',3,'1970-01-01 00:00:01'),(88603,'638712343543321103','jwcd9','Cartão de visita para curso bíblico','',5,'1970-01-01 00:00:01'),(88604,'638712343646094327','jwcd10','Cartão de visita para curso bíblico','',5,'1970-01-01 00:00:01'),(88605,'638722709426459346','w','Sentinela de Estudo - Nº2 (2025)','202502',7,'1970-01-01 00:00:01'),(88606,'638722709496304663','wlp','Sentinela de Estudo - Letra Grande - Nº2 (2025)','202502',7,'1970-01-01 00:00:01'),(88607,'638755906887102547','mwb','Nossa Vida e Ministério Cristão - Apostila da Reunião - Nº5 (2025)','202505',7,'1970-01-01 00:00:01'),(88608,'638755907029449487','w','Sentinela de Estudo - Nº3 (2025)','202503',7,'1970-01-01 00:00:01'),(88609,'638755907135303361','wlp','Sentinela de Estudo - Letra Grande - Nº3 (2025)','202503',7,'1970-01-01 00:00:01'),(88610,'638774053509339624','w','Sentinela de Estudo - Nº4 (2025)','202504',7,'1970-01-01 00:00:01'),(88611,'638774053673878765','wlp','Sentinela de Estudo - Letra Grande - Nº4 (2025)','202504',7,'1970-01-01 00:00:01'),(88612,'638786145745700014','mwb','Nossa Vida e Ministério Cristão - Apostila da Reunião - Nº7 (2025)','202507',7,'1970-01-01 00:00:01'),(88613,'638786145821702513','w','Sentinela de Estudo - Nº5 (2025)','202505',7,'1970-01-01 00:00:01'),(88614,'638786145929275710','wlp','Sentinela de Estudo - Letra Grande - Nº5 (2025)','202505',7,'1970-01-01 00:00:01'),(88615,'638855721299751849','w','Sentinela de Estudo - Nº6 (2025)','202506',7,'1970-01-01 00:00:01'),(88616,'638855721373087898','w','Sentinela de Estudo - Nº7 (2025)','202507',7,'1970-01-01 00:00:01'),(88617,'638855721477950525','wlp','Sentinela de Estudo - Letra Grande - Nº6 (2025)','202506',7,'1970-01-01 00:00:01'),(88618,'638855721572326563','wlp','Sentinela de Estudo - Letra Grande - Nº7 (2025)','202507',7,'1970-01-01 00:00:01'),(88619,'638855721726148854','mwb','Nossa Vida e Ministério Cristão - Apostila da Reunião - Nº9 (2025)','202509',7,'1970-01-01 00:00:01'),(88620,'638892031797587493','w','Sentinela de Estudo - Nº8 (2025)','202508',7,'1970-01-01 00:00:01'),(88621,'638892031889251764','w','Sentinela de Estudo - Nº9 (2025)','202509',7,'1970-01-01 00:00:01'),(88622,'638892032023493912','wlp','Sentinela de Estudo - Letra Grande - Nº8 (2025)','202508',7,'1970-01-01 00:00:01'),(88623,'638892032100569203','wlp','Sentinela de Estudo - Letra Grande - Nº9 (2025)','202509',7,'1970-01-01 00:00:01'),(88624,'638892032203286250','mwb','Nossa Vida e Ministério Cristão - Apostila da Reunião - Nº11 (2025)','202511',7,'1970-01-01 00:00:01');
/*!40000 ALTER TABLE `l_pubs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `l_tipos`
--

DROP TABLE IF EXISTS `l_tipos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `l_tipos` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Descricao` varchar(50) DEFAULT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `l_tipos`
--

LOCK TABLES `l_tipos` WRITE;
/*!40000 ALTER TABLE `l_tipos` DISABLE KEYS */;
INSERT INTO `l_tipos` VALUES (1,'Folhetos','1970-01-01 00:00:01'),(2,'Biblias','1970-01-01 00:00:01'),(3,'Livros','1970-01-01 00:00:01'),(4,'Brochuras','1970-01-01 00:00:01'),(5,'Formulários','1970-01-01 00:00:01'),(6,'Testemunho Público','1970-01-01 00:00:01'),(7,'Periódicos','1970-01-01 00:00:01');
/*!40000 ALTER TABLE `l_tipos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sys_utilizadores`
--

DROP TABLE IF EXISTS `sys_utilizadores`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `sys_utilizadores` (
  `IdUtilizador` int(11) NOT NULL AUTO_INCREMENT,
  `Username` varchar(50) DEFAULT NULL,
  `Password` varchar(200) DEFAULT NULL,
  `Nome` varchar(50) DEFAULT NULL,
  `Telemovel` varchar(20) DEFAULT NULL,
  `Email` varchar(50) DEFAULT NULL,
  `IdGrupo` int(11) DEFAULT NULL,
  `Tipo` int(10) unsigned NOT NULL DEFAULT '90',
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`IdUtilizador`)
) ENGINE=InnoDB AUTO_INCREMENT=1000021 DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sys_utilizadores`
--

LOCK TABLES `sys_utilizadores` WRITE;
/*!40000 ALTER TABLE `sys_utilizadores` DISABLE KEYS */;
INSERT INTO `sys_utilizadores` VALUES (0,NULL,NULL,'Salão do Reino',NULL,NULL,0,90,'1970-01-01 00:00:01'),(49,'jksprods','AQAAAAIAAYagAAAAEAy3pstxC0/EpklRqxeafo0m/iifKCNTBMBkwZDXpcYC7+qlGRy1/085f5iTPrgWCA==','Jorge Monteiro','912 321 280','jfmmonteiro1997@gmail.com',3,0,'1970-01-01 00:00:01'),(999999,NULL,NULL,'Outros',NULL,NULL,0,90,'1970-01-01 00:00:01');
/*!40000 ALTER TABLE `sys_utilizadores` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_anexos`
--

DROP TABLE IF EXISTS `t_anexos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `t_anexos` (
  `StampAnexo` varchar(45) NOT NULL,
  `StampTerritorio` varchar(45) NOT NULL,
  `Descricao` varchar(256) NOT NULL,
  `Ficheiro` varchar(256) NOT NULL,
  `Caminho` varchar(512) NOT NULL,
  `Extensao` varchar(10) NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`StampAnexo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_anexos`
--

LOCK TABLES `t_anexos` WRITE;
/*!40000 ALTER TABLE `t_anexos` DISABLE KEYS */;
/*!40000 ALTER TABLE `t_anexos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_movimentos`
--

DROP TABLE IF EXISTS `t_movimentos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `t_movimentos` (
  `StampMovimento` varchar(45) NOT NULL,
  `StampTerritorio` varchar(45) NOT NULL,
  `IdPublicador` int(10) unsigned NOT NULL,
  `DataMovimento` datetime NOT NULL,
  `TipoMovimento` int(10) unsigned NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`StampMovimento`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_movimentos`
--

LOCK TABLES `t_movimentos` WRITE;
/*!40000 ALTER TABLE `t_movimentos` DISABLE KEYS */;
/*!40000 ALTER TABLE `t_movimentos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_territorios`
--

DROP TABLE IF EXISTS `t_territorios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `t_territorios` (
  `Stamp` varchar(45) NOT NULL,
  `Id` varchar(45) NOT NULL,
  `Nome` varchar(120) NOT NULL,
  `Dificuldade` int(10) unsigned NOT NULL,
  `Descricao` varchar(1024) NOT NULL,
  `Url` varchar(256) NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Stamp`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_territorios`
--

LOCK TABLES `t_territorios` WRITE;
/*!40000 ALTER TABLE `t_territorios` DISABLE KEYS */;
/*!40000 ALTER TABLE `t_territorios` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'sys_jw_4143'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-09-09 12:21:56
