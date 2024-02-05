using AutoSavePrices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSendPrice.Helpers
{
    // Custom comparer for the Product class
    public class ClientComparer : IEqualityComparer<Client>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(Client x, Client y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.IdKontr == y.IdKontr && x.Category == y.Category && x.IdTerritory == y.IdTerritory;
        }

        // If Equals() returns true for a pair of objects
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(Client client)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(client, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashProductIdKontr = client.IdKontr.GetHashCode();

            //Get hash code for the Code field.
            int hashProductCategory = client.Category.GetHashCode();

            int hashProductTerritory = client.IdTerritory.GetHashCode();


            //Calculate the hash code for the product.
            return hashProductIdKontr ^ hashProductCategory ^ hashProductTerritory;
        }
    }
}
