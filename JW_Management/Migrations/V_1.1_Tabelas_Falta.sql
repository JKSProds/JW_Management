CREATE TABLE IF NOT EXISTS `t_tipos` (
                                         `Id` int(11) NOT NULL AUTO_INCREMENT,
    `Descricao` varchar(50) DEFAULT NULL,
    `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Id`)
    ) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `r_designacoes` (
                                               `StampReuniao` varchar(50) NOT NULL,
    `Semana` varchar(50) DEFAULT NULL,
    `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`StampReuniao`)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `sys_discursos` (
                                               `NumDiscurso` int(11) NOT NULL AUTO_INCREMENT,
    `TemaDiscurso` varchar(50) DEFAULT NULL,
    `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`NumDiscurso`)
    ) ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `r_discursos` (
                                             `Stamp` varchar(50) NOT NULL,
    `IdPublicador` int(11) NOT NULL,
    `NomePublicador` varchar(50) DEFAULT NULL,
    `Congregacao` varchar(50) DEFAULT NULL,
    `Data` varchar(50) DEFAULT NULL,
    `NumDiscurso` int(11) NOT NULL,
    `TemaDiscurso` varchar(50) DEFAULT NULL,
    `timestamp` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`Stamp`)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
