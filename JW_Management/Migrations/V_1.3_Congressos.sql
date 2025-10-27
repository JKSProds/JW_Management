CREATE TABLE `ic` (
                      `Id` int(11) NOT NULL AUTO_INCREMENT,
                      `Nome` varchar(50) DEFAULT NULL,
                      `Local` varchar(50) DEFAULT NULL,
                      `Latitude` varchar(50) DEFAULT NULL,
                      `Longitude` varchar(50) DEFAULT NULL,
                      `DataInicio` datetime DEFAULT NULL,
                      `DataFim` datetime DEFAULT NULL,
                      `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
                      PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4;

CREATE TABLE `ic_congregacoes` (
                                   `IdIC` int(11) NOT NULL,
                                   `IdCongregacao` int(11) NOT NULL,
                                   `Nome` varchar(50) DEFAULT NULL,
                                   `Morada` varchar(150) DEFAULT NULL,
                                   `Telemovel` varchar(150) DEFAULT NULL,
                                   `Latitude` varchar(20) DEFAULT NULL,
                                   `Longitude` varchar(20) DEFAULT NULL,
                                   `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                   PRIMARY KEY (`IdIC`,`IdCongregacao`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;