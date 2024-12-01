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
                    .AddChoices(new[] { "Listar Pedidos", "Pesquisar Pedido", "Criar um Pedido", "Deletar um Pedido", "Sair" }));

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

                case "Listar Pedidos":
                    await ListPedidos();
                    break;

                case "Pesquisar Pedido":
                    id = AnsiConsole.Ask<int>("[gray]Digite o id do Pedido: [/]");
                    await getPedidoPorId(id);
                    break;

                case "Criar um Pedido":
                    await CreatePedido();
                    break;

                case "Deletar um Pedido":
                    id = AnsiConsole.Ask<int>("[gray]Digite o id do Pedido: [/]");
                    await DeletaPedido(id);
                    break;

                case "Sair":
                    AnsiConsole.MarkupLine("[red]Encerrando o programa...[/]");
                    return;
            }

            AnsiConsole.MarkupLine("[gray]Pressione qualquer tecla para voltar ao menu principal.[/]");
            Console.ReadKey(true);
        }
    }

    private static async Task ListPedidos()
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync("api/pedidos/GetPedidos");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();

                var pedidos = JsonConvert.DeserializeObject<List<ResourcePedido>>(data);
                if (pedidos != null)
                {
                    var table = new Table()
                        .AddColumn("ID Pedido")
                        .AddColumn("Cliente")
                        .AddColumn("Data Criação")
                        .AddColumn("Pratos")
                        .AddColumn("Preço Total")
                        .Expand(); // Adiciona espaço proporcional entre as colunas

                    foreach (var pedido in pedidos)
                    {
                        var precoTotal = pedido.Produtos.Sum(p => (p.Preco ?? 0) * p.Quantidade);
                        var nomesPratos = string.Join(", ", pedido.Produtos.Select(p => p.NmPrato));

                        table.AddRow(
                            pedido.CdPedido.ToString(),
                            pedido.NmCliente.ToString(),
                            pedido.DtCriacao?.ToString("dd/MM/yyyy"),
                            nomesPratos,
                            precoTotal.ToString("C") 
                        );
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


    private static async Task getPedidoPorId(int id)
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync("api/pedidos/GetPedidoPorId/?id=" + id);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();

                var pedido = JsonConvert.DeserializeObject<ResourcePedido>(data);

                var table = new Table();
                table.AddColumn("ID Pedido");
                table.AddColumn("Cliente");
                table.AddColumn("Data Criação");
                table.AddColumn("Pratos");
                table.AddColumn("Preço Total");

                    var precoTotal = pedido.Produtos.Sum(p => (p.Preco ?? 0) * p.Quantidade);
                    var nomesPratos = string.Join(", ", pedido.Produtos.Select(p => p.NmPrato));

                    table.AddRow(pedido.CdPedido.ToString(), pedido.NmCliente.ToString(), pedido.DtCriacao?.ToString("dd/MM/yyyy"), nomesPratos, precoTotal.ToString());

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

    private static async Task CreatePedido()
    {
        try
        {
            var idCliente = AnsiConsole.Ask<int>("Qual é o [green]id do Cliente[/]?");
            var stringProdutos = AnsiConsole.Ask<string>("Quais os [green] ids dos Produtos[/]?");

            var listaProdutos = stringProdutos
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse) 
                .ToList();

            var content = new StringContent(
                JsonConvert.SerializeObject(listaProdutos),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/pedidos/CreatePedido/?cdCliente=" + idCliente, content);

            if (response.IsSuccessStatusCode)
            {
                AnsiConsole.MarkupLine("[green]Pedido criado com sucesso![/]");
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


    private static async Task DeletaPedido(int id)
    {
        try
        {

            var response = await client.DeleteAsync("/api/pedidos/DeletaPedido/?id=" + id);

            if (response.IsSuccessStatusCode)
            {
                AnsiConsole.MarkupLine("[green]Pedido Deletado com sucesso![/]");
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
