//using System;
//using StackExchange.Redis;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Security.Cryptography.X509Certificates;
//using System.Text;
//using System.Threading.Tasks;
//using Newtonsoft.Json;

//namespace CacheApplicationConsole
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {

           
//            static string ShowMenuAndGetChoice()
//            {
//                string[] options = { "0", "1", "2", "3", "4" };
//                string userChoice;

//                do
//                {
//                    Console.WriteLine("Escolha uma opção \n Tecle 0 - Para Sair" +
//                        "\n Tecle 1 - Para Inserção de dados " +
//                        "\n Tecle 2 - Para Atualizar dados " +
//                        "\n Tecle 3 - Para Deletar dados " +
//                        "\n Tecle 4 - Para Visualizar dados");

//                    userChoice = Console.ReadLine();

//                    if (Array.IndexOf(options, userChoice) == -1 && userChoice != "0")
//                    {
//                        Console.WriteLine("Opção Inválida\n");
//                    }

//                } while (Array.IndexOf(options, userChoice) == -1);

//                return userChoice;
//            }

//            static void ShowInsertMenu()
//            {
//                SqlConnection sqlConnection;
//                string connectionString = @"Data Source=NOTECOMP-0876\SQLSERVER;Initial Catalog=APPLICATION_CRUD;Integrated Security=True";
//                sqlConnection = new SqlConnection(connectionString);
//                sqlConnection.Open();

//                Console.WriteLine("Nome Completo: ");
//                string nomeCompleto = Console.ReadLine();

//                Console.WriteLine("Gênero: ");
//                string genero = Console.ReadLine();

//                Console.WriteLine("CPF sem traços e pontos: ");
//                string CPF = Console.ReadLine();

//                Console.WriteLine("Endereço Completo: ");
//                string enderecoCompleto = Console.ReadLine();

//                string insertQuery = "INSERT INTO Pessoa(nomeCompleto, genero, CPF, enderecoCompleto) values ('" + nomeCompleto + "', '" + genero + "', '" + CPF + "', '" + enderecoCompleto + "')";
//                SqlCommand insertCommand = new SqlCommand(insertQuery, sqlConnection);
//                insertCommand.ExecuteNonQuery();

//                Console.WriteLine("Dados Inseridos \n");
//                sqlConnection.Close();

//            }

//            static void ShowUpdateMenu()
//            {

//                SqlConnection sqlConnection;
//                string connectionString = @"Data Source=NOTECOMP-0876\SQLSERVER;Initial Catalog=APPLICATION_CRUD;Integrated Security=True";
//                sqlConnection = new SqlConnection(connectionString);
//                sqlConnection.Open();

//                Console.WriteLine("Qual o Id a ser atualizado?: ");
//                int id;
//                while (!int.TryParse(Console.ReadLine(), out id))
//                {
//                    Console.WriteLine("Insira um valor do tipo inteiro! \nQual o Id a ser deletado?: ");
//                }

//                string selectQuery = $"SELECT COUNT(*) FROM Pessoa WHERE Id = {id}";
//                SqlCommand selecter = new SqlCommand(selectQuery, sqlConnection);

//                int count = (int)selecter.ExecuteScalar();

//                if (count > 0)
//                {
//                    Console.WriteLine("Caso não quiser alterar uma determinada informação, tecle ENTER \n");

//                    Console.WriteLine("Atulizar -> Nome Completo: ");
//                    string nomeCompleto = Console.ReadLine();


//                    Console.WriteLine("Atulizar -> Gênero: ");
//                    string genero = Console.ReadLine();

//                    Console.WriteLine("Atulizar -> CPF sem traços e pontos: ");
//                    string CPF = Console.ReadLine();

//                    Console.WriteLine("Atulizar -> Endereço Completo: ");
//                    string enderecoCompleto = Console.ReadLine();

//                    string updateQuery = "UPDATE Pessoa SET";
//                    SqlCommand updateCommand = new SqlCommand(updateQuery, sqlConnection);

//                    if (!string.IsNullOrWhiteSpace(nomeCompleto))
//                    {
//                        updateQuery += " NomeCompleto = @NomeCompleto,";
//                        updateCommand.Parameters.AddWithValue("@NomeCompleto", nomeCompleto);
//                    }
//                    if (!string.IsNullOrWhiteSpace(genero))
//                    {
//                        updateQuery += " Genero = @Genero,";
//                        updateCommand.Parameters.AddWithValue("@Genero", genero);
//                    }
//                    if (!string.IsNullOrWhiteSpace(CPF))
//                    {
//                        updateQuery += " CPF = @CPF,";
//                        updateCommand.Parameters.AddWithValue("@CPF", CPF);
//                    }
//                    if (!string.IsNullOrWhiteSpace(enderecoCompleto))
//                    {
//                        updateQuery += " EnderecoCompleto = @EnderecoCompleto,";
//                        updateCommand.Parameters.AddWithValue("@EnderecoCompleto", enderecoCompleto);
//                    }

//                    updateQuery = updateQuery.TrimEnd(',');
//                    updateQuery += " WHERE Id = @Id";
//                    updateCommand.Parameters.AddWithValue("@Id", id);
//                    updateCommand.CommandText = updateQuery;
//                    updateCommand.ExecuteNonQuery();
//                    Console.WriteLine("\nRegistro atualizado com sucesso! \n");
//                }
//                else
//                {
//                    Console.WriteLine("\nId não encontrado \n");
//                }

//                sqlConnection.Close();

//            }

//            static void ShowDeleteMenu()
//            {
//                SqlConnection sqlConnection;
//                string connectionString = @"Data Source=NOTECOMP-0876\SQLSERVER;Initial Catalog=APPLICATION_CRUD;Integrated Security=True";
//                sqlConnection = new SqlConnection(connectionString);
//                sqlConnection.Open();

//                Console.WriteLine("Qual o Id a ser deletado?: ");
//                int id;
//                while (!int.TryParse(Console.ReadLine(), out id))
//                {
//                    Console.WriteLine("Insira um valor do tipo inteiro! \nQual o Id a ser deletado?: ");
//                }

//                string selectQuery = $"SELECT COUNT(*) FROM Pessoa WHERE Id = {id}";
//                SqlCommand selecter = new SqlCommand(selectQuery, sqlConnection);

//                int count = (int)selecter.ExecuteScalar();
                
//                if(count > 0)
//                {
//                    string deleteQuery = $"DELETE FROM Pessoa WHERE Id = {id}";
//                    SqlCommand deleter = new SqlCommand(deleteQuery, sqlConnection);
//                    deleter.ExecuteNonQuery();
//                    Console.WriteLine("\nRegistro excluído com sucesso! \n");
//                }
//                else
//                {
//                    Console.WriteLine("\nId não encontrado \n");
//                }

//                sqlConnection.Close();

//            }

//            static void ShowVisualizerMenu()
//            {
//                SqlConnection sqlConnection;
//                string connectionString = @"Data Source=NOTECOMP-0876\SQLSERVER;Initial Catalog=APPLICATION_CRUD;Integrated Security=True";
//                sqlConnection = new SqlConnection(connectionString);
//                sqlConnection.Open();

//                string selectQuery = "SELECT * FROM Pessoa";
//                SqlCommand display = new SqlCommand(selectQuery, sqlConnection);
//                SqlDataReader dataReader = display.ExecuteReader();

//                while (dataReader.Read())
//                {
//                    Console.WriteLine(
//                        "Id: " + dataReader.GetValue(0).ToString() +
//                        ", Nome: " + dataReader.GetValue(1).ToString() +
//                        ", Gênero - " + dataReader.GetValue(2).ToString() +
//                        ", CPF - " + dataReader.GetValue(3).ToString() +
//                        ", Endereço Completo - " + dataReader.GetValue(4).ToString()
//                    );
//                }

//                dataReader.Close();
//                sqlConnection.Close();

//                Console.WriteLine("\n");
//            }

//            while(true)
//            {
//                string choice = ShowMenuAndGetChoice();

//                if(choice == "0")
//                {
//                    Environment.Exit(0);
//                }
//                else if (choice == "1")
//                {
//                    ShowInsertMenu();
//                }
//                else if (choice == "2")
//                {
//                    ShowVisualizerMenu();
//                    ShowUpdateMenu();
//                }
//                else if (choice == "3")
//                {
//                    ShowVisualizerMenu();
//                    ShowDeleteMenu();
//                }
//                else if (choice == "4")
//                {
//                    ShowVisualizerMenu();
//                }
//                else
//                {

//                }
//            }


//        }
//    }
//}