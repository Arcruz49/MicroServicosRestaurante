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
                    .AddChoices(new[] { "Listar Produtos", "Pesquisar Produto", "Criar um Produto", "Atualizar um Produto", "Deletar um Produto", "Sair" }));

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

                case "Listar Produtos":
                    await ListProdutos();
                    break;

                case "Pesquisar Produto":
                    id = AnsiConsole.Ask<int>("[gray]Digite o id do Produto: [/]");
                    await getProdutoPorId(id);
                    break;

                case "Criar um Produto":
                    await CreateProduto();
                    break;

                case "Atualizar um Produto":
                    id = AnsiConsole.Ask<int>("[gray]Digite o id do Produto: [/]");
                    await AtualizaProduto(id);
                    break;

                case "Deletar um Produto":
                    id = AnsiConsole.Ask<int>("[gray]Digite o id do Produto: [/]");
                    await DeletaProduto(id);
                    break;

                case "Sair":
                    AnsiConsole.MarkupLine("[red]Encerrando o programa...[/]");
                    return;
            }

            AnsiConsole.MarkupLine("[gray]Pressione qualquer tecla para voltar ao menu principal.[/]");
            Console.ReadKey(true);
        }
    }

    private static async Task ListProdutos()
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync("api/cardapio/GetProdutos");

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();

                var produtos = JsonConvert.DeserializeObject<List<CadCardapio>>(data);
                if (produtos != null)
                {
                    var table = new Table();
                    table.AddColumn("ID Produto");
                    table.AddColumn("Nome");
                    table.AddColumn("Data Criação");
                    table.AddColumn("Descrição");
                    table.AddColumn("Preço");

                    foreach (var produto in produtos)
                    {
                        table.AddRow(produto.CdPrato.ToString(), produto.NmPrato.ToString(), produto.DtCriacao?.ToString("dd/MM/yyyy"), produto.DsPrato.ToString(), produto.Preco.ToString());
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


    private static async Task getProdutoPorId(int id)
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync("api/cardapio/GetProdutoPorId/?id=" + id);

            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();

                var produto = JsonConvert.DeserializeObject<CadCardapio>(data);

                var table = new Table();
                table.AddColumn("ID Produto");
                table.AddColumn("Nome");
                table.AddColumn("Data Criação");
                table.AddColumn("Descrição");
                table.AddColumn("Preço");


                table.AddRow(produto.CdPrato.ToString(), produto.NmPrato.ToString(), produto.DtCriacao?.ToString("dd/MM/yyyy"), produto.DsPrato.ToString(), produto.Preco.ToString());


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

    private static async Task CreateProduto()
    {
        try
        {
            var nomeProduto = AnsiConsole.Ask<string>("Qual é o [green]nome do produto[/]?");
            var descricaoProduto = AnsiConsole.Ask<string>("Qual é a [green]descrição do produto[/]?");
            var precoProduto = AnsiConsole.Ask<double>("Qual é o [green]valor do produto[/]?");

            var novoProduto = new
            {
                NmPrato = nomeProduto,
                DsPrato = descricaoProduto,
                Preco = precoProduto,
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(novoProduto),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/cardapio/CreateProduto", content);

            if (response.IsSuccessStatusCode)
            {
                AnsiConsole.MarkupLine("[green]Produto criado com sucesso![/]");
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

    private static async Task AtualizaProduto(int id)
    {
        try
        {
            var nomeProduto = AnsiConsole.Ask<string>("Qual é o [green]nome do produto[/]?");
            var descricaoProduto = AnsiConsole.Ask<string>("Qual é a [green]descrição do produto[/]?");
            var precoProduto = AnsiConsole.Ask<double>("Qual é o [green]valor do produto[/]?");

            var novoProduto = new
            {
                NmPrato = nomeProduto,
                DsPrato = descricaoProduto,
                Preco = precoProduto,
            };


            var content = new StringContent(
                JsonConvert.SerializeObject(novoProduto),
                Encoding.UTF8,
                "application/json");

            var response = await client.PutAsync("/api/cardapio/AtualizaProduto/?id=" + id, content);

            if (response.IsSuccessStatusCode)
            {
                AnsiConsole.MarkupLine("[green]Produto Atualizado com sucesso![/]");
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

    private static async Task DeletaProduto(int id)
    {
        try
        {

            var response = await client.DeleteAsync("/api/cardapio/DeleteProduto/?id=" + id);

            if (response.IsSuccessStatusCode)
            {
                AnsiConsole.MarkupLine("[green]Produto Deletado com sucesso![/]");
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
