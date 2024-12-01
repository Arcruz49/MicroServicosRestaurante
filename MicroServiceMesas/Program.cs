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
                    .AddChoices(new[] { "Listar Mesas", "Pesquisar Mesa", "Criar uma Mesa", "Deletar uma Mesa", "Sair" }));

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

                case "Listar Mesas":
                    await ListMesas();
                    break;

                case "Pesquisar Mesa":
                    id = AnsiConsole.Ask<int>("[gray]Digite o id do Mesa: [/]");
                    await getMesaPorId(id);
                    break;

                case "Criar uma Mesa":
                    await CreateMesa();
                    break;

                case "Deletar uma Mesa":
                    id = AnsiConsole.Ask<int>("[gray]Digite o id do Mesa: [/]");
                    await DeletaMesa(id);
                    break;

                case "Sair":
                    AnsiConsole.MarkupLine("[red]Encerrando o programa...[/]");
                    return;
            }

            AnsiConsole.MarkupLine("[gray]Pressione qualquer tecla para voltar ao menu principal.[/]");
            Console.ReadKey(true);
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

    private static async Task getMesaPorId(int id)
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync("api/mesas/GetMesaPorId/?id=" + id);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();

                var mesa = JsonConvert.DeserializeObject<CadMesa>(data);

                var table = new Table();
                table.AddColumn("ID Mesa");
                table.AddColumn("Data Criação");


                table.AddRow(mesa.CdMesa.ToString(), mesa.DtCriacao?.ToString("dd/MM/yyyy"));


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

    private static async Task CreateMesa()
    {
        try
        {

            var mesa = new CadMesa();

            var content = new StringContent(
                JsonConvert.SerializeObject(mesa),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/mesas/CreateMesa", content);

            if (response.IsSuccessStatusCode)
            {
                AnsiConsole.MarkupLine("[green]Mesa criada com sucesso![/]");
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

    private static async Task DeletaMesa(int id)
    {
        try
        {

            var response = await client.DeleteAsync("/api/mesas/DeleteMesa/?id=" + id);

            if (response.IsSuccessStatusCode)
            {
                AnsiConsole.MarkupLine("[green]Mesa Deletada com sucesso![/]");
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
