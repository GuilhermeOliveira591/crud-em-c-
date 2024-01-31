using System;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;
using System.Data.SqlTypes;
using System.Diagnostics.Metrics;
using System.Collections;

namespace CacheApplicationConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = @"Data Source=NOTECOMP-0876\SQLSERVER;Initial Catalog=APPLICATION_CRUD;Integrated Security=True";
            string redisConnectionString = "localhost:6379,allowAdmin=true,password=123456";
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnectionString);
            IDatabase redisDatabase = redis.GetDatabase();

            static async Task ConsultarPessoaPorIdEAdicionarAoRedis( string connectionString, string redisConnectionString,int id, IDatabase redisDatabase)
            {
                using (var sqlDatabase = new SqlConnection(connectionString))
                {
                    await sqlDatabase.OpenAsync();
                    var query = $"SELECT * FROM Pessoa WHERE Id = {id}";
                    var pessoa = await sqlDatabase.QueryFirstOrDefaultAsync<Pessoa>(query);

                    if(pessoa != null)
                    {

                        var jsonObject = new
                        {
                            Id = pessoa.Id,
                            NomeCompleto = pessoa.NomeCompleto,
                            Genero = pessoa.Genero,
                            CPF = pessoa.CPF,
                            EnderecoCompleto = pessoa.EnderecoCompleto,
                            RedisIdentifier = pessoa.RedisIdentifier
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

            static int ProcurarPessoaParaObterId(string connectionString, string cpf)
            {
                using (var sqlDatabase = new SqlConnection(connectionString))
                {
                    sqlDatabase.Open();

                    // Usando parâmetros para evitar injeção de SQL
                    var query = "SELECT * FROM Pessoa WHERE CPF = @CPF";
                    var pessoa = sqlDatabase.QueryFirstOrDefault<Pessoa>(query, new { CPF = cpf });

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

            static int ProcurarPessoaParaObterRedisIdentifier(string connectionString, int id)
            {
                using (var sqlDatabase = new SqlConnection(connectionString))
                {
                    sqlDatabase.Open();

                    // Usando parâmetros para evitar injeção de SQL
                    var query = "SELECT * FROM Pessoa WHERE Id = @Id";
                    var pessoa = sqlDatabase.QueryFirstOrDefault<Pessoa>(query, new { Id = id });

                    if (pessoa.RedisIdentifier != 0 || pessoa.RedisIdentifier != null)
                    {
                        return pessoa.RedisIdentifier;
                    }

                    else
                    {
                        Console.WriteLine("Nada a ser exibido");
                        return 0; // Ou outro valor padrão, dependendo do seu contexto
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

            static async Task ShowInsertMenu()
            {
                string redisConnectionString = "localhost:6379,allowAdmin=true,password=123456";
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnectionString);
                IDatabase redisDatabase = redis.GetDatabase();

                SqlConnection sqlConnection;
                string connectionString = @"Data Source=NOTECOMP-0876\SQLSERVER;Initial Catalog=APPLICATION_CRUD;Integrated Security=True";
                sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();

                //Reading the data been inserted
                Console.WriteLine("Nome Completo: ");
                string nomeCompleto = Console.ReadLine();

                Console.WriteLine("Gênero: ");
                string genero = Console.ReadLine();

                Console.WriteLine("CPF sem traços e pontos: ");
                string CPF = Console.ReadLine();

                Console.WriteLine("Endereço Completo: ");
                string enderecoCompleto = Console.ReadLine();

                //Preparing the query
                string insertQuery = "INSERT INTO Pessoa(nomeCompleto, genero, CPF, enderecoCompleto, RedisIdentifier) VALUES (@NomeCompleto, @Genero, @CPF, @EnderecoCompleto, NEXT VALUE FOR RedisIndex)";
                SqlCommand insertCommand = new SqlCommand(insertQuery, sqlConnection);

                //Adding variable parameters
                insertCommand.Parameters.AddWithValue("@NomeCompleto", nomeCompleto);
                insertCommand.Parameters.AddWithValue("@Genero", genero);
                insertCommand.Parameters.AddWithValue("@CPF", CPF);
                insertCommand.Parameters.AddWithValue("@EnderecoCompleto", enderecoCompleto);

                insertCommand.ExecuteNonQuery();
                Console.WriteLine("Dados Inseridos \n");

                int id = ProcurarPessoaParaObterId(connectionString, CPF);
                await ConsultarPessoaPorIdEAdicionarAoRedis(connectionString,redisConnectionString,id,redisDatabase);
                
                sqlConnection.Close();
            }

            static void ShowUpdateMenu(IDatabase redisDatabase)
            {

                SqlConnection sqlConnection;
                string connectionString = @"Data Source=NOTECOMP-0876\SQLSERVER;Initial Catalog=APPLICATION_CRUD;Integrated Security=True";
                sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                string redisConnectionString = "localhost:6379,allowAdmin=true,password=123456";
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnectionString);
                IDatabase db = redis.GetDatabase();

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

                    int redisIdentifierParameter = ProcurarPessoaParaObterRedisIdentifier(connectionString, id);
                    RedisValue[] rangeList = db.ListRange("PessoaList", redisIdentifierParameter, redisIdentifierParameter);

                    foreach (var valor in rangeList)
                    {
                        string jsonString = valor.ToString();

                        Pessoa pessoa = JsonConvert.DeserializeObject<Pessoa>(jsonString);

                        if (nomeCompleto != "") { pessoa.NomeCompleto = nomeCompleto; }

                        if (genero != "") {  pessoa.Genero = genero; }

                        if (CPF != "") { pessoa.CPF = CPF; }

                        if (enderecoCompleto != "") { pessoa.EnderecoCompleto = enderecoCompleto; }

                        var jsonObject = new
                        {
                            Id = id,
                            NomeCompleto = pessoa.NomeCompleto,
                            Genero = pessoa.Genero,
                            CPF = pessoa.CPF,
                            EnderecoCompleto = pessoa.EnderecoCompleto,
                            RedisIdentifier = pessoa.RedisIdentifier
                        };

                        var jsonValue = JsonConvert.SerializeObject(jsonObject);


                        redisDatabase.ListSetByIndex($"PessoaList", pessoa.RedisIdentifier, jsonValue);
                        

                    }

                    Console.WriteLine("\nRegistro atualizado com sucesso! \n");
                }
                else
                {
                    Console.WriteLine("\nId não encontrado \n");
                }

                sqlConnection.Close();

            }

            static void UpdateRedisValue(string key, int? newValueInt = null, string newValueString = null)
            {
                string redisConnectionString = "localhost:6379,allowAdmin=true,password=123456";
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnectionString);
                IDatabase db = redis.GetDatabase();

                if (newValueInt != null)
                {
                    db.StringSet(key, newValueInt);
                }
                if (newValueString != null)
                {
                    db.StringSet(key, newValueString);
                }

            }

            static void ShowDeleteMenu()
            {
                SqlConnection sqlConnection;
                string connectionString = @"Data Source=NOTECOMP-0876\SQLSERVER;Initial Catalog=APPLICATION_CRUD;Integrated Security=True";
                sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();

                Console.WriteLine("Qual o Id a ser deletado?: ");
                int id;
                while (!int.TryParse(Console.ReadLine(), out id))
                {
                    Console.WriteLine("Insira um valor do tipo inteiro! \nQual o Id a ser deletado?: ");
                }

                string selectQuery = $"SELECT COUNT(*) FROM Pessoa WHERE Id = {id}";
                SqlCommand selecter = new SqlCommand(selectQuery, sqlConnection);

                int count = (int)selecter.ExecuteScalar();
                
                if(count > 0)
                {
                    string deleteQuery = $"DELETE FROM Pessoa WHERE Id = {id}";
                    SqlCommand deleter = new SqlCommand(deleteQuery, sqlConnection);
                    deleter.ExecuteNonQuery();
                    Console.WriteLine("\nRegistro excluído com sucesso! \n");
                }
                else
                {
                    Console.WriteLine("\nId não encontrado \n");
                }

                sqlConnection.Close();

            }

            static void ShowVisualizerMenu()
            {
                SqlConnection sqlConnection;
                string connectionString = @"Data Source=NOTECOMP-0876\SQLSERVER;Initial Catalog=APPLICATION_CRUD;Integrated Security=True";
                sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();

                string selectQuery = "SELECT * FROM Pessoa";
                SqlCommand display = new SqlCommand(selectQuery, sqlConnection);
                SqlDataReader dataReader = display.ExecuteReader();

                while (dataReader.Read())
                {
                    Console.WriteLine(
                        "Id: " + dataReader.GetValue(0).ToString() +
                        ", Nome: " + dataReader.GetValue(1).ToString() +
                        ", Gênero - " + dataReader.GetValue(2).ToString() +
                        ", CPF - " + dataReader.GetValue(3).ToString() +
                        ", Endereço Completo - " + dataReader.GetValue(4).ToString()
                    );
                }

                dataReader.Close();
                sqlConnection.Close();

                Console.WriteLine("\n");
            }

            while(true)
            {
                string choice = ShowMenuAndGetChoice();

                if(choice == "0")
                {
                    Environment.Exit(0);
                }
                else if (choice == "1")
                {
                    ShowInsertMenu();
                }
                else if (choice == "2")
                {
                    ShowVisualizerMenu();
                    ShowUpdateMenu(redisDatabase);
                }
                else if (choice == "3")
                {
                    ShowVisualizerMenu();
                    ShowDeleteMenu();
                }
                else if (choice == "4")
                {
                    ShowVisualizerMenu();
                }
                else
                {

                }
            }


        }
    }
}