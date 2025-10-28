DROP table  ic_mobilidade_recomendacoes;
DROP table  ic_mobilidade_recomendacoes_linhas;
CREATE TABLE `ic_mobilidade_recomendacoes` (
                                   `IdIC` int(11) NOT NULL,
                                   `IdCongregacao` int(11) NOT NULL,
                                   `IdRecomendacao` varchar(50) NOT NULL,
                                   `Sequencia` int(11) NOT NULL,
                                   `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                   PRIMARY KEY (`IdIC`,`IdCongregacao`, `IdRecomendacao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `ic_mobilidade_recomendacoes_linhas` (
                                          `IdIC` int(11) NOT NULL,
                                          `IdRecomendacao` varchar(50) NOT NULL,
                                          `IdLinha` varchar(50) NOT NULL,
                                          `TipoTransporte` varchar(250) DEFAULT NULL,
                                          `Rota` varchar(250) DEFAULT NULL,
                                          `Viagem` varchar(250) DEFAULT NULL,
                                          `Paragem` varchar(250) DEFAULT NULL,
                                          `Sequencia` int(11) NOT NULL,
                                          `Manual` BOOLEAN DEFAULT NULL,
                                          `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                          PRIMARY KEY (`IdIC`,`IdRecomendacao`,`IdLinha`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

