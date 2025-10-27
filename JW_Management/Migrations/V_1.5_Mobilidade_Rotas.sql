CREATE TABLE `ic_mobilidade_rotas` (
                                   `IdIC` int(11) NOT NULL,
                                   `IdTipoTransporte` int(11) NOT NULL,
                                   `IdRota` varchar(50) NOT NULL,
                                   `NomeRota` varchar(250) DEFAULT NULL,
                                   `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                   PRIMARY KEY (`IdIC`,`IdTipoTransporte`, `IdRota`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `ic_mobilidade_viagens` (
                                          `IdIC` int(11) NOT NULL,
                                          `IdRota` varchar(50) NOT NULL,
                                          `IdViagem` varchar(50) NOT NULL,
                                          `Destino` varchar(250) DEFAULT NULL,
                                          `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                          PRIMARY KEY (`IdIC`,`IdViagem`,`IdRota`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

DROP TABLE ic_mobilidade_viagens_paragens;
CREATE TABLE `ic_mobilidade_viagens_paragens` (
                                         `IdIC` int(11) NOT NULL,
                                         `IdViagem` varchar(50) NOT NULL,
                                         `IdParagem` varchar(50) NOT NULL,
                                         `HoraPartida` varchar(50) DEFAULT NULL,
                                         `Sequencia` int(11) DEFAULT NULL,
                                         `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                         PRIMARY KEY (`IdIC`,`IdViagem`,`IdParagem`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
