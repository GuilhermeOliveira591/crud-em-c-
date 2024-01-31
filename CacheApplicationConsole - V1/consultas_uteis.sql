-- Truncar a tabela
TRUNCATE TABLE Pessoa;

-- Redefinir o identificador para 1
DBCC CHECKIDENT('Pessoa', RESEED, 1);

ALTER SEQUENCE RedisIndex
    RESTART WITH 0;
SELECT * FROM Pessoa

SELECT 
    name AS NomeDaSequencia,
    object_id AS IDDoObjeto,
    schema_id AS IDDoSchema,
    start_value AS ValorInicial,
    increment AS Incremento,
    current_value AS ValorAtual
FROM 
    sys.sequences;

UPDATE Pessoa
SET RedisIdentifier = RedisIdentifier - 1
WHERE RedisIdentifier >= 4;



--quando excluir vou ter que atualizar o identifier por conta da posição

