public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int CategoryId { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }

    
    public override bool Equals(object obj)
    {
        return obj is Product p && Id == p.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}