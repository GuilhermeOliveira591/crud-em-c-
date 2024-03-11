<h1>CRUD desenvolvido com C#</h1>

  <p>A aplicação é um CRUD (Create, Read, Update e Delete) que funciona pelo próprio CMD ou Terminal, onde a linguagem principal é o C#, realizando a gravação, leitura, criação e atualização dos dados em um banco de dados relacional, neste caso foi escolhido o SQL Server.</p>

  <p>Após as operações serem realizadas no banco de dados SQL Server, os valores são replicados no banco de dados Redis. Esta replicação é aplicada consequentemente após as operações no SQL Server.</p>

  <p>Quando é necessária a leitura dos dados, tudo é buscado no Redis, o que aumenta a performance e evita muitos acessos ao banco de dados principal SQL Server em grandes aplicações. Já as outras operações, como inclusão, atualização ou exclusão, são primeiramente realizadas no SQL Server e posteriormente no banco de dados Redis.</p>

  <p>Para este projeto, são necessárias as seguintes ferramentas:</p>

  <ul>
    <li>IDE de Banco de Dados (SSMS)</li>
    <li>IDE Visual Studio</li>
    <li>IDE Redinav</li>
      <li>Docker com Redis executando em um contâiner</li>
    <li>
      Instalação dos seguintes pacotes NuGet:
      <ul>
        <li>Install-Package Dapper</li>
        <li>Install-Package StackExchange.Redis</li>
        <li>Install-Package Newtonsoft.Json</li>
      </ul>
    </li>
  </ul>
