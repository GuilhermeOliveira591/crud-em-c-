﻿using System;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;
using System.Data.SqlTypes;
using System.Diagnostics.Metrics;
using System.Collections;
using System.Reflection;

namespace CacheApplicationConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            string sqlConnectionString = @"Data Source=NOTECOMP-0876\SQLSERVER;Initial Catalog=APPLICATION_CRUD;Integrated Security=True";
            string redisConnectionString = "localhost:6379,allowAdmin=true,password=123456";
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnectionString);
            IDatabase redisDatabase = redis.GetDatabase();

            static async Task ConsultarPessoaPorIdEAdicionarAoRedis(string sqlConnectionString, string redisConnectionString, int id, IDatabase redisDatabase)
            {
                using (var sqlDatabase = new SqlConnection(sqlConnectionString))
                {
                    await sqlDatabase.OpenAsync();
                    var query = $"SELECT * FROM Pessoa WHERE Id = {id} AND Ativo = 1";
                    var pessoa = await sqlDatabase.QueryFirstOrDefaultAsync<Pessoa>(query);

                    if (pessoa != null)
                    {

                        var jsonObject = new
                        {
                            Id = pessoa.Id,
                            NomeCompleto = pessoa.NomeCompleto,
                            Genero = pessoa.Genero,
                            CPF = pessoa.CPF,
                            EnderecoCompleto = pessoa.EnderecoCompleto
                        };

                        string jsonValue = JsonConvert.SerializeObject(jsonObject);

                        // Insert JSON data into Redis list
                        string redisListKey = $"PessoaList";
                        redisDatabase.ListRightPush(redisListKey, jsonValue);
                    }
                    else
                    {

                    }

                }
            }

            static int ProcurarPessoaParaObterId(string sqlConnectionString, string cpf, string nomeCompleto)
            {
                using (var sqlDatabase = new SqlConnection(sqlConnectionString))
                {
                    sqlDatabase.Open();

                    // Usando parâmetros para evitar injeção de SQL
                    var query =  $"SELECT * FROM Pessoa WHERE CPF = @CPF AND NomeCompleto = @NomeCompleto";
                    var pessoa = sqlDatabase.QueryFirstOrDefault<Pessoa>(query, new { CPF = cpf, NomeCompleto = nomeCompleto});

                    if (pessoa.Id != 0 || pessoa.Id != null)
                    {
                        return pessoa.Id;
                    }

                    else
                    {
                        Console.WriteLine("Nada a ser exibido");
                        return 0; // Ou outro valor padrão, dependendo do seu contexto
                    }
                }
            }

            static string ValidarCpfExistente(string sqlConnectionString, string cpf)
            {
                using (var sqlDatabase = new SqlConnection(sqlConnectionString))
                {
                    sqlDatabase.Open();

                    // Usando parâmetros para evitar injeção de SQL
                    var query = $"SELECT CPF FROM Pessoa WHERE CPF = @CPF AND Ativo = 1";
                    var pessoa = sqlDatabase.QueryFirstOrDefault<Pessoa>(query, new { CPF = cpf});


                    if (pessoa == null)
                    {
                        return cpf;
                    }
                    else
                    {
                        return "";
                    }
                }
            }

            static string ShowMenuAndGetChoice()
            {
                string[] options = { "0", "1", "2", "3", "4" };
                string userChoice;

                do
                {
                    Console.WriteLine("Escolha uma opção \n Tecle 0 - Para Sair" +
                        "\n Tecle 1 - Para Inserção de dados " +
                        "\n Tecle 2 - Para Atualizar dados " +
                        "\n Tecle 3 - Para Deletar dados " +
                        "\n Tecle 4 - Para Visualizar dados");

                    userChoice = Console.ReadLine();

                    if (Array.IndexOf(options, userChoice) == -1 && userChoice != "0")
                    {
                        Console.WriteLine("Opção Inválida\n");
                    }

                } while (Array.IndexOf(options, userChoice) == -1);

                return userChoice;
            }

            static async Task ShowInsertMenu(string sqlConnectionString, string redisConnectionString, IDatabase redisDatabase)
            {

                SqlConnection sqlConnection;
                sqlConnection = new SqlConnection(sqlConnectionString);
                sqlConnection.Open();

                //Reading the data been inserted
                Console.WriteLine("Nome Completo: ");
                string nomeCompleto = Console.ReadLine();

                Console.WriteLine("Gênero: ");
                string genero = Console.ReadLine();

                Console.WriteLine("CPF sem traços e pontos: ");
                string CPF = Console.ReadLine();
                string verificadorCpf = ValidarCpfExistente(sqlConnectionString, CPF);

                if(!string.IsNullOrWhiteSpace(verificadorCpf))
                {
                    Console.WriteLine("Endereço Completo: ");
                    string enderecoCompleto = Console.ReadLine();

                    //Preparing the query
                    string insertQuery = "INSERT INTO Pessoa(nomeCompleto, genero, CPF, enderecoCompleto, Ativo) VALUES (@NomeCompleto, @Genero, @CPF, @EnderecoCompleto, 1)";
                    SqlCommand insertCommand = new SqlCommand(insertQuery, sqlConnection);

                    //Adding variable parameters
                    insertCommand.Parameters.AddWithValue("@NomeCompleto", nomeCompleto);
                    insertCommand.Parameters.AddWithValue("@Genero", genero);
                    insertCommand.Parameters.AddWithValue("@CPF", CPF);
                    insertCommand.Parameters.AddWithValue("@EnderecoCompleto", enderecoCompleto);

                    insertCommand.ExecuteNonQuery();
                    Console.WriteLine("Dados Inseridos \n");

                    int id = ProcurarPessoaParaObterId(sqlConnectionString, CPF, nomeCompleto);
                    await ConsultarPessoaPorIdEAdicionarAoRedis(sqlConnectionString, redisConnectionString, id, redisDatabase);
                }
                else
                {
                    Console.WriteLine("CPF já existente no Banco de Dados \n");
                }                

                sqlConnection.Close();
            }

            static void ShowUpdateMenu(string sqlConnectionString, string redisConnectionString, IDatabase redisDatabase)
            {

                SqlConnection sqlConnection;
                sqlConnection = new SqlConnection(sqlConnectionString);
                sqlConnection.Open();
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnectionString);

                Console.WriteLine("Qual o Id a ser atualizado?: ");
                int id;
                while (!int.TryParse(Console.ReadLine(), out id))
                {
                    Console.WriteLine("Insira um valor do tipo inteiro! \nQual o Id a ser deletado?: ");
                }

                string selectQuery = $"SELECT COUNT(*) FROM Pessoa WHERE Id = {id}";
                SqlCommand selecter = new SqlCommand(selectQuery, sqlConnection);

                int count = (int)selecter.ExecuteScalar();

                if (count > 0)
                {
                    Console.WriteLine("Caso não quiser alterar uma determinada informação, tecle ENTER \n");

                    Console.WriteLine("Atualizar -> Nome Completo: ");
                    string nomeCompleto = Console.ReadLine();

                    Console.WriteLine("Atualizar -> Gênero: ");
                    string genero = Console.ReadLine();

                    Console.WriteLine("Atualizar -> CPF sem traços e pontos: ");
                    string CPF = Console.ReadLine();

                    Console.WriteLine("Atualizar -> Endereço Completo: ");
                    string enderecoCompleto = Console.ReadLine();

                    string updateQuery = "UPDATE Pessoa SET";
                    SqlCommand updateCommand = new SqlCommand(updateQuery, sqlConnection);

                    if (!string.IsNullOrWhiteSpace(nomeCompleto))
                    {
                        updateQuery += " NomeCompleto = @NomeCompleto,";
                        updateCommand.Parameters.AddWithValue("@NomeCompleto", nomeCompleto);
                    }
                    if (!string.IsNullOrWhiteSpace(genero))
                    {
                        updateQuery += " Genero = @Genero,";
                        updateCommand.Parameters.AddWithValue("@Genero", genero);
                    }
                    if (!string.IsNullOrWhiteSpace(CPF))
                    {
                        updateQuery += " CPF = @CPF,";
                        updateCommand.Parameters.AddWithValue("@CPF", CPF);
                    }
                    if (!string.IsNullOrWhiteSpace(enderecoCompleto))
                    {
                        updateQuery += " EnderecoCompleto = @EnderecoCompleto,";
                        updateCommand.Parameters.AddWithValue("@EnderecoCompleto", enderecoCompleto);
                    }

                    updateQuery = updateQuery.TrimEnd(',');
                    updateQuery += " WHERE Id = @Id";
                    updateCommand.Parameters.AddWithValue("@Id", id);
                    updateCommand.CommandText = updateQuery;
                    updateCommand.ExecuteNonQuery();

                    RedisValue[] rangeList = redisDatabase.ListRange("PessoaList", 0, -1);

                    for (long indice = 0; indice < rangeList.Length; indice++)
                    {
                        string valor = rangeList[indice];

                        // Verificar se o JSON contém o ID desejado
                        if (valor.Contains($"\"Id\":{id}"))
                        {
                            string jsonString = valor.ToString();

                            Pessoa pessoa = JsonConvert.DeserializeObject<Pessoa>(jsonString);

                            if (nomeCompleto != "") { pessoa.NomeCompleto = nomeCompleto; }

                            if (genero != "") { pessoa.Genero = genero; }

                            if (CPF != "") { pessoa.CPF = CPF; }

                            if (enderecoCompleto != "") { pessoa.EnderecoCompleto = enderecoCompleto; }

                            var jsonObject = new
                            {
                                Id = id,
                                NomeCompleto = pessoa.NomeCompleto,
                                Genero = pessoa.Genero,
                                CPF = pessoa.CPF,
                                EnderecoCompleto = pessoa.EnderecoCompleto
                            };

                            var jsonValue = JsonConvert.SerializeObject(jsonObject);


                            redisDatabase.ListSetByIndex($"PessoaList", indice, jsonValue);
                            break;
                        }
                    }

                    sqlConnection.Close();
                }
            }

            static void ShowDeleteMenu(string sqlConnectionString, string redisConnectionString, IDatabase redisDatabase)
            {
                SqlConnection sqlConnection;
                sqlConnection = new SqlConnection(sqlConnectionString);
                sqlConnection.Open();

                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnectionString);

                Console.WriteLine("Qual o Id a ser deletado?: ");
                int id;
                while (!int.TryParse(Console.ReadLine(), out id))
                {
                    Console.WriteLine("Insira um valor do tipo inteiro! \nQual o Id a ser deletado?: ");
                }

                string selectQuery = $"SELECT COUNT(*) FROM Pessoa WHERE Id = {id}";
                SqlCommand selecter = new SqlCommand(selectQuery, sqlConnection);

                int count = (int)selecter.ExecuteScalar();

                if (count > 0)
                {
                    string deleteQuery = $"UPDATE Pessoa SET Ativo = 0 WHERE Id = {id}";
                    SqlCommand deleter = new SqlCommand(deleteQuery, sqlConnection);

                    deleter.ExecuteNonQuery();
                    Console.WriteLine("\nRegistro excluído com sucesso! \n");

                    RedisValue[] rangeList = redisDatabase.ListRange("PessoaList", 0, -1);

                    // Iterar sobre os valores e procurar pelo ID desejado
                    for (long indice = 0; indice < rangeList.Length; indice++)
                    {
                        string valor = rangeList[indice];

                        // Verificar se o JSON contém o ID desejado
                        if (valor.Contains($"\"Id\":{id}"))
                        {
                            redisDatabase.ListRemove("PessoaList", rangeList[indice]);
                            break;
                        }
                    }

                }
                else
                {
                    Console.WriteLine("\nId não encontrado \n");
                }

                sqlConnection.Close();
            }

            static void ShowRedisData(string redisConnectionString, IDatabase redisDatabase)
            {
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnectionString);

                string listaKey = "PessoaList"; // Substitua pelo nome real da sua lista no Redis

                // Use LRange para obter todos os elementos na lista
                RedisValue[] lista = redisDatabase.ListRange(listaKey);

                foreach (var jsonValue in lista)
                {
                    string jsonString = jsonValue.ToString();

                    Pessoa pessoa = JsonConvert.DeserializeObject<Pessoa>(jsonString);

                    Console.WriteLine(
                        "Id: " + pessoa.Id +
                        ", Nome: " + pessoa.NomeCompleto +
                        ", Gênero: " + pessoa.Genero +
                        ", CPF: " + pessoa.CPF +
                        ", Endereço Completo: " + pessoa.EnderecoCompleto
                    );
                }

                Console.WriteLine("\n");
            }

            while (true)
            {
                string choice = ShowMenuAndGetChoice();

                if (choice == "0")
                {
                    Environment.Exit(0);
                }
                else if (choice == "1")
                {
                    ShowInsertMenu(sqlConnectionString, redisConnectionString, redisDatabase);
                }
                else if (choice == "2")
                {
                    ShowRedisData(redisConnectionString, redisDatabase);
                    ShowUpdateMenu(sqlConnectionString, redisConnectionString, redisDatabase);
                }
                else if (choice == "3")
                {
                    ShowRedisData(redisConnectionString, redisDatabase);
                    ShowDeleteMenu(sqlConnectionString, redisConnectionString, redisDatabase);
                }
                else if (choice == "4")
                {
                    ShowRedisData(redisConnectionString, redisDatabase);
                }
                else
                {
                
                }
            }

        }
    }
}
