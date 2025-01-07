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
                Facility facility = new Facility
                {
                    Id = j,
                    NumberOfBeds = random.Next(2, 4)
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
                    { 0, 18 },
                    { 65, 99 }
                }; 

                for (int k = 1; k <= agePairs.Count; k++)
                {
                    var bedDiscount = new BedDiscount
                    {
                        Id = k,
                        Beds = random.Next(2, 4),
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

    public List<Accomodation> GetSeededData()
    {
        return _accomodations.Value;
    }

}