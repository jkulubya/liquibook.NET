using Liquibook.NET.Book;
using Liquibook.NET.Types;

namespace Liquibook.NET.Simple
{
    public class SimpleOrder : IOrder
    {
        public bool IsLimit { get; set; }
        public bool IsBuy { get; set; }
        public Price Price { get; set; }
        public Price StopPrice { get; set; }
        public Quantity OrderQty { get; set; }
        public bool AllOrNone => ((Conditions & OrderConditions.AllOrNone) != 0);
        public bool ImmediateOrCancel => ((Conditions & OrderConditions.ImmediateOrCancel) != 0);
        public OrderConditions Conditions { get; set; }
        public Quantity FilledQuantity { get; set; } = 0;
        public int FilledCost { get; set; } = 0;
        public static int LastOrderId { get; set; }
        public int OrderId { get; set; }
        public OrderState State { get; private set; }

        public Quantity OpenQuantity
        {
            get
            {
                if (FilledQuantity < OrderQty)
                {
                    return OrderQty - FilledQuantity;
                }

                return 0;
            }
        }

        public SimpleOrder(bool isBuy, Price price, Quantity quantity)
        {
            IsBuy = isBuy;
            Price = price;
            OrderQty = quantity;
        }

        public SimpleOrder(bool isBuy, Price price, Quantity quantity, Price stopPrice, OrderConditions conditions)
        {
            IsBuy = isBuy;
            Price = price;
            OrderQty = quantity;
            StopPrice = stopPrice;
            Conditions = conditions;
            OrderId = ++LastOrderId;
            State = OrderState.New;
        }

        public void Fill(Quantity fillQuantity, int fillCost, int fillId)
        {
            FilledQuantity += fillQuantity;
            FilledCost += fillCost;
            if (OpenQuantity == 0)
            {
                State = OrderState.Complete;
            }
        }

        public void Accept()
        {
            if (State == OrderState.New)
            {
                State = OrderState.Accepted;
            }
        }

        public void Cancel()
        {
            if (State != OrderState.Complete)
            {
                State = OrderState.Cancelled;
            }
        }

        public void Replace(Quantity sizeDelta, Price newPrice)
        {
            if (State == OrderState.Accepted)
            {
                OrderQty += sizeDelta;
                Price = newPrice;
            }
        }
    }
}