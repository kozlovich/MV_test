using MV_test.Models;

namespace MV_test.Services;

public interface ISeedService
{
    List<Accomodation> GetSeededData();
}

public class SeedService : ISeedService
{
    private readonly Lazy<List<Accomodation>> _accomodations;
    public SeedService()
    {
        _accomodations = new Lazy<List<Accomodation>>(Seed);
    }

    private List<Accomodation> Seed()
    {
        return SimpleSeed();

        Random random = new Random();

        var list = new List<Accomodation>();
        // 1. Generate 3 accomadations
        int count = 3;
        for (int i = 1; i <= count; i++)
        {
            var accomodation = new Accomodation { Id = i };
            var facilities = new List<Facility>();
            for (int j = 1; j <= 5; j++)
            {
                int numberOfBeds = random.Next(2, 4);
                Facility facility = new Facility
                {
                    Id = j,
                    NumberOfBeds = numberOfBeds
                };

                // Generate PricePerPerson
                List<PricePerPerson> prices = new();
                for (int k = 1; k <= 5; k++)
                {
                    var pricePerPerson = new PricePerPerson
                    {
                        Id = k,
                        Price = (float)random.Next(50, 150),
                        ValidFrom = DateTime.Today.AddDays(k),
                        ValidTo = DateTime.Today.AddDays(k + random.Next(0, 15)),
                        Created = DateTime.Today.AddMinutes(random.Next(100, 500)) // simulate different creation time
                    };
                    
                    prices.Add(pricePerPerson);
                }
                facility.PricesPerPerson.AddRange(prices);

                // Generate bedDiscounts
                List<BedDiscount> bedDiscounts = new();
                Dictionary<int, int> agePairs = new Dictionary<int, int>
                {
                    { 18, 99 },
                    { 0, 17 }
                }; 

                for (int k = 1; k <= numberOfBeds; k++)
                {
                    var bedDiscount = new BedDiscount
                    {
                        Id = k,
                        Bed = k,
                        GuestAgeFrom = agePairs.ElementAt(k - 1).Key,
                        GuestAgeTo = agePairs.ElementAt(k - 1).Value,
                        DiscountInPercents = random.Next(15, 25),
                        Created = DateTime.Today.AddMinutes(random.Next(50, 150)) // simulate different creation time
                    };
                    
                    bedDiscounts.Add(bedDiscount);
                }
                facility.BedDiscounts.AddRange(bedDiscounts);

                // SEED PROMOTIONS
                List<Promotion> promotions = new();
                for (int k = 1; k <= 1; k++)
                {
                    var promotion = new Promotion
                    {
                        Id = k,
                        ValidFrom = DateTime.Today.AddDays(k),
                        ValidTo = DateTime.Today.AddDays(k + random.Next(0, 15)),
                        DiscountInPercents = random.Next(5, 15),
                        Created = DateTime.Today.AddMinutes(random.Next(150, 350)) // simulate different creation time
                    };
                    
                    promotions.Add(promotion);
                }
                facility.Promotions.AddRange(promotions);

                facilities.Add(facility);
            }

            accomodation.Facilities.AddRange(facilities);
            list.Add(accomodation);
        }

        return list;
    }

    private List<Accomodation> SimpleSeed()
    {
        Accomodation accomodation = new Accomodation { Id = 1 };
        Facility facility = new Facility { Id = 1, NumberOfBeds = 3 };
        facility.BedDiscounts.Add(new BedDiscount { Bed = 1, GuestAgeFrom = 18, GuestAgeTo = 99, DiscountInPercents = 10, Created = DateTime.Today.AddDays(-1) });
        facility.BedDiscounts.Add(new BedDiscount { Bed = 2, GuestAgeFrom = 18, GuestAgeTo = 99, DiscountInPercents = 10, Created = DateTime.Today.AddDays(-1) });
        facility.BedDiscounts.Add(new BedDiscount { Bed = 3, GuestAgeFrom = 0, GuestAgeTo = 17, DiscountInPercents = 20, Created = DateTime.Today.AddDays(-1) });
        facility.PricesPerPerson.Add(new PricePerPerson { Price = 100, ValidFrom = DateTime.Today.AddDays(1), ValidTo = DateTime.Today.AddDays(10), Created = DateTime.Today.AddDays(-1) });
        facility.Promotions.Add(new Promotion {  ValidFrom = DateTime.Today.AddDays(1), ValidTo = DateTime.Today.AddDays(2), DiscountInPercents = 20, Created = DateTime.Today.AddDays(-1) });
        accomodation.Facilities.Add(facility);
        return new List<Accomodation> { accomodation };
    }

    public List<Accomodation> GetSeededData()
    {
        return _accomodations.Value;
    }

}