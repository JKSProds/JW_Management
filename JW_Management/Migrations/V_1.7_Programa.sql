-- Programa mensal (reuniões, serviço de campo, testemunho público) guardado como documentos JSON,
-- para gerar o PDF sem voltar a pedir os dados ao Hourglass.
CREATE TABLE IF NOT EXISTS `r_programa` (
    `Tipo` varchar(20) NOT NULL,
    `Chave` varchar(20) NOT NULL,
    `Dados` mediumtext NOT NULL,
    `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (`Tipo`, `Chave`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
