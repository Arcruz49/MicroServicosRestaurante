using Spectre.Console;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.Text;

class Program
{
    private static readonly HttpClient client = new HttpClient();

    static async Task Main(string[] args)
    {
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        client.BaseAddress = new Uri("https://localhost:44314/"); 
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        int id;
        while (true)
        {
            var option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("O que você gostaria de fazer?")
                    .PageSize(10)
                    .AddChoices(new[] { "Listar Clientes","Pesquisar Cliente","Criar um Cliente", "Atualizar um Cliente", "Deletar um cliente", "Sair" }));

            switch (option)
            {
                case "Exibir barra de progresso":
                    AnsiConsole.Progress()
                        .Start(ctx =>
                        {
                            var task = ctx.AddTask("[green]Processando dados...[/]");
                            while (!task.IsFinished)
                            {
                                Thread.Sleep(100); 
                                task.Increment(10); 
                            }
                        });
                    break;

                case "Listar Clientes":
                    await ListClientes();
                    break;

                case "Pesquisar Cliente":
                    id = AnsiConsole.Ask<int>("[gray]Digite o id do Cliente: [/]");
                    await getClientePorId(id);
                    break;

                case "Criar um Cliente":
                    await CreateCliente();
                    break;

                case "Atualizar um Cliente":
                    id = AnsiConsole.Ask<int>("[gray]Digite o id do Cliente: [/]");
                    await AtualizaCliente(id);
                    break;

                case "Deletar um cliente":
                    id = AnsiConsole.Ask<int>("[gray]Digite o id do Cliente: [/]");
                    await DeletaCliente(id);
                    break;

                case "Sair":
                    AnsiConsole.MarkupLine("[red]Encerrando o programa...[/]");
                    return; 
            }

            AnsiConsole.MarkupLine("[gray]Pressione qualquer tecla para voltar ao menu principal.[/]");
            Console.ReadKey(true);
        }
    }

    private static async Task ListClientes()
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync("api/clientes/GetClientes");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();

                var clientes = JsonConvert.DeserializeObject<List<CadCliente>>(data);
                if(clientes != null)
                {
                    var table = new Table();
                    table.AddColumn("ID Cliente");
                    table.AddColumn("Nome");
                    table.AddColumn("Data Criação");

                    foreach (var cliente in clientes)
                    {
                        table.AddRow(cliente.CdCliente.ToString(), cliente.NmCliente, cliente.DtCriacao?.ToString("dd/MM/yyyy"));
                    }

                    AnsiConsole.Write(table);
                }
                
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine($"[red]Erro ao obter dados da API. Código de status: {response.StatusCode}[/]");
                AnsiConsole.MarkupLine($"[red]Detalhes do erro: {errorContent}[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Erro: {ex.Message}[/]");
            AnsiConsole.MarkupLine($"[red]StackTrace: {ex.StackTrace}[/]");  
        }
    }


    private static async Task getClientePorId(int id)
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync("api/clientes/GetClientePorId/?id=" + id);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();

                var cliente = JsonConvert.DeserializeObject<CadCliente>(data);

                var table = new Table();
                table.AddColumn("ID Cliente");
                table.AddColumn("Nome");
                table.AddColumn("Data Criação");

                
                table.AddRow(cliente.CdCliente.ToString(), cliente.NmCliente, cliente.DtCriacao?.ToString("dd/MM/yyyy"));
                

                AnsiConsole.Write(table);
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine($"[red]Erro. Código de status: {response.StatusCode}[/]");
                AnsiConsole.MarkupLine($"[red]Detalhes do erro: {errorContent}[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Erro: {ex.Message}[/]");
            AnsiConsole.MarkupLine($"[red]StackTrace: {ex.StackTrace}[/]");
        }


    }

    private static async Task CreateCliente()
    {
        try
        {
            var nomeCliente = AnsiConsole.Ask<string>("Qual é o [green]nome do cliente[/]?");

            var novoCliente = new
            {
                nmCliente = nomeCliente,
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(novoCliente),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/clientes/CreateCliente", content);

            if (response.IsSuccessStatusCode)
            {
                AnsiConsole.MarkupLine("[green]Cliente criado com sucesso![/]");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine($"[red]Erro. Código de status: {response.StatusCode}[/]");
                AnsiConsole.MarkupLine($"[red]Detalhes do erro: {errorContent}[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Erro: {ex.Message}[/]");
            AnsiConsole.MarkupLine($"[red]StackTrace: {ex.StackTrace}[/]");
        }
    }

    private static async Task AtualizaCliente(int id)
    {
        try
        {
            var nomeCliente = AnsiConsole.Ask<string>("Qual é o [green]nome do cliente[/]?");
            var novoCliente = new
            {
                nmCliente = nomeCliente,
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(novoCliente),
                Encoding.UTF8,
                "application/json");

            var response = await client.PutAsync("/api/clientes/AtualizaCliente/?id=" + id, content);

            if (response.IsSuccessStatusCode)
            {
                AnsiConsole.MarkupLine("[green]Cliente Atualizado com sucesso![/]");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine($"[red]Erro. Código de status: {response.StatusCode}[/]");
                AnsiConsole.MarkupLine($"[red]Detalhes do erro: {errorContent}[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Erro: {ex.Message}[/]");
            AnsiConsole.MarkupLine($"[red]StackTrace: {ex.StackTrace}[/]");
        }
    }

    private static async Task DeletaCliente(int id)
    {
        try
        {

            var response = await client.DeleteAsync("/api/clientes/DeleteCliente/?id=" + id);

            if (response.IsSuccessStatusCode)
            {
                AnsiConsole.MarkupLine("[green]Cliente Deletado com sucesso![/]");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                AnsiConsole.MarkupLine($"[red]Erro. Código de status: {response.StatusCode}[/]");
                AnsiConsole.MarkupLine($"[red]Detalhes do erro: {errorContent}[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Erro: {ex.Message}[/]");
            AnsiConsole.MarkupLine($"[red]StackTrace: {ex.StackTrace}[/]");
        }
    }

}
