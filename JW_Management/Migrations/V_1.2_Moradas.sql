ALTER TABLE `sys_utilizadores`
    ADD COLUMN `Morada` varchar(300) NULL DEFAULT '' AFTER `Email`;