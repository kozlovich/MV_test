namespace MV_test.Models;

public class BaseEntity 
{
    public int Id { get; set; }
}

public class Accomodation : BaseEntity
{
    public List<Facility> Facilities { get; set; } = new List<Facility>();
}

public class Facility : BaseEntity
{
    public int NumberOfBeds { get; set; }

    public List<PricePerPerson> PricesPerPerson { get;set; } = new List<PricePerPerson>();
    public List<BedDiscount> BedDiscounts { get; set; } = new List<BedDiscount>();
    public List<Promotion> Promotions { get; set; } = new List<Promotion>();
}

public class PricePerPerson : BaseEntity
{
    public float Price { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public DateTime Created { get; set; }
}

public class BedDiscount : BaseEntity
{
    public int Bed { get; set; }
    public int GuestAgeFrom { get; set; }
    public int GuestAgeTo { get; set; }
    public int DiscountInPercents { get; set; }
    public DateTime Created { get; set; }
}

public class Promotion : BaseEntity
{
    public int DiscountInPercents { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public DateTime Created { get; set; }
}
