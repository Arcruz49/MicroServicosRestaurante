
public class ResourcePedido
{
    public int CdPedido { get; set; }
    public int CdCliente { get; set; }
    public string NmCliente { get; set; }
    public DateTime? DtCriacao { get; set; }
    public List<ProdutoPedido> Produtos { get; set; }
}

public class ProdutoPedido
{
    public int CdPrato { get; set; }
    public string NmPrato { get; set; }
    public string DsPrato { get; set; }
    public decimal? Preco { get; set; }
    public int Quantidade { get; set; }
}

