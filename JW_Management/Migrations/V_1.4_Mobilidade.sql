CREATE TABLE `ic_mobilidade_paragens` (
                                   `IdIC` int(11) NOT NULL,
                                   `IdTipoTransporte` int(11) NOT NULL,
                                   `IdParagem` varchar(50) NOT NULL,
                                   `CodParagem` varchar(50) DEFAULT NULL,
                                   `NomeParagem` varchar(250) DEFAULT NULL,
                                   `Latitude` varchar(20) DEFAULT NULL,
                                   `Longitude` varchar(20) DEFAULT NULL,
                                   `IdZona` varchar(20) DEFAULT NULL,
                                   `UrlParagem` varchar(250) DEFAULT NULL,
                                   `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                   PRIMARY KEY (`IdIC`,`IdTipoTransporte`, `IdParagem`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `ic_mobilidade_tipos` (
                                          
                                          `Id` int(11) NOT NULL,
                                          `Nome` varchar(50) DEFAULT NULL,
                                          `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                          PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO ic_mobilidade_tipos (Id, Nome) VALUES (1, 'STCP'), (2, 'CP'), (3, 'Metro do Porto'), (4, 'Unir');
