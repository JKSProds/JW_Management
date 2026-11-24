-- Os títulos das partes (tipos de designação) vinham cortados aos 50 caracteres.
ALTER TABLE `r_tipos` MODIFY `Descricao` varchar(255) DEFAULT NULL, MODIFY `DescricaoAdicional` varchar(255) DEFAULT NULL;
