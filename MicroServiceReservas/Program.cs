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
                    .AddChoices(new[] { "Listar Reservas", "Listar Clientes", "Listar Mesas", "Pesquisar Reserva", "Criar uma Reserva", "Deletar uma Reserva", "Sair" }));

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

                case "Listar Reservas":
                    await ListReservas();
                    break;
                
                case "Listar Clientes":
                    await ListClientes();
                    break;

                case "Listar Mesas":
                    await ListMesas();
                    break;

                case "Pesquisar Reserva":
                    id = AnsiConsole.Ask<int>("[gray]Digite o id do Reserva: [/]");
                    await getReservaPorId(id);
                    break;

                case "Criar uma Reserva":
                    await CreateReserva();
                    break;

                case "Deletar uma Reserva":
                    id = AnsiConsole.Ask<int>("[gray]Digite o id do Reserva: [/]");
                    await DeletaReserva(id);
                    break;

                case "Sair":
                    AnsiConsole.MarkupLine("[red]Encerrando o programa...[/]");
                    return;
            }

            AnsiConsole.MarkupLine("[gray]Pressione qualquer tecla para voltar ao menu principal.[/]");
            Console.ReadKey(true);
        }
    }

    private static async Task ListReservas()
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync("api/reservas/GetReservas");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();

                var reservas = JsonConvert.DeserializeObject<List<CadReserva>>(data);
                if (reservas != null)
                {
                    var table = new Table();
                    table.AddColumn("ID Reserva");
                    table.AddColumn("ID do Cliente");
                    table.AddColumn("Nome do Cliente");
                    table.AddColumn("ID da Mesa");
                    table.AddColumn("Data Criação");

                    foreach (var reserva in reservas)
                    {
                        table.AddRow(reserva.CdReserva.ToString(), reserva.CdCliente.ToString(), reserva.NmCliente, reserva.CdMesa.ToString(), reserva.DtCriacao?.ToString("dd/MM/yyyy"));
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

    private static async Task getReservaPorId(int id)
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync("api/reservas/GetReservaPorId/?id=" + id);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();

                var reserva = JsonConvert.DeserializeObject<CadReserva>(data);

                var table = new Table();
                table.AddColumn("ID Reserva");
                table.AddColumn("ID do Cliente");
                table.AddColumn("Nome do Cliente");
                table.AddColumn("ID da Mesa");
                table.AddColumn("Data Criação");

                table.AddRow(reserva.CdReserva.ToString(), reserva.CdCliente.ToString(), reserva.NmCliente, reserva.CdMesa.ToString(), reserva.DtCriacao?.ToString("dd/MM/yyyy"));

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

    private static async Task CreateReserva()
    {
        try
        {
            var idCliente = AnsiConsole.Ask<string>("Qual é o [green]ID do cliente[/]?");
            var idMesa = AnsiConsole.Ask<string>("Qual é o [green]ID da mesa[/]?");

            var novaReserva = new
            {
                CdCliente = idCliente,
                CdMesa = idMesa
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(novaReserva),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/reservas/CreateReserva", content);

            if (response.IsSuccessStatusCode)
            {
                AnsiConsole.MarkupLine("[green]Reserva criada com sucesso![/]");
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

    private static async Task DeletaReserva(int id)
    {
        try
        {

            var response = await client.DeleteAsync("/api/reservas/DeleteReserva/?id=" + id);

            if (response.IsSuccessStatusCode)
            {
                AnsiConsole.MarkupLine("[green]Reserva Deletada com sucesso![/]");
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

    private static async Task ListClientes()
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync("api/clientes/GetClientes");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();

                var clientes = JsonConvert.DeserializeObject<List<CadCliente>>(data);
                if (clientes != null)
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

    private static async Task ListMesas()
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync("api/mesas/GetMesas");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();

                var mesas = JsonConvert.DeserializeObject<List<CadMesa>>(data);
                if (mesas != null)
                {
                    var table = new Table();
                    table.AddColumn("ID Mesa");
                    table.AddColumn("Data Criação");

                    foreach (var mesa in mesas)
                    {
                        table.AddRow(mesa.CdMesa.ToString(), mesa.DtCriacao?.ToString("dd/MM/yyyy"));
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

}
