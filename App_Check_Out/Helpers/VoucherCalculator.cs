using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APP_CHECKOUT.Helpers
{
    public class ProductVoucherCalculatorModel
    {
        public string Name { get; set; }
        public decimal Price { get; set; }   // Giá 1 sp
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
        public decimal Discount { get; set; }
    }

    public static class VoucherCalculator
    {
        /// <summary>
        /// unit = "percent" hoặc "vnd"
        /// value = phần trăm (0.1 = 10%) hoặc số tiền (VND)
        /// maxDiscount = giới hạn giảm tối đa (chỉ áp dụng nếu unit = percent && is_limit = true)
        /// is_limit = true nếu có giới hạn, false nếu không giới hạn
        /// </summary>
        public static void ApplyVoucher(List<ProductVoucherCalculatorModel> products, decimal value, decimal? maxDiscount, string unit, bool is_limit)
        {
            decimal orderTotal = products.Sum(p => p.Total);
            decimal voucherDiscount = 0;

            if (unit == "percent")
            {
                decimal expectedDiscount = orderTotal * value; // ví dụ: 10% = 0.1
                if (is_limit)
                    voucherDiscount = Math.Min(expectedDiscount, maxDiscount ?? expectedDiscount);
                else
                    voucherDiscount = expectedDiscount;
            }
            else if (unit == "vnd")
            {
                voucherDiscount = Math.Min(value, orderTotal); // không được vượt quá tổng đơn
            }
            else
            {
                return;
            }

            // phân bổ theo tỷ lệ giá trị sản phẩm
            decimal allocated = 0;
            for (int i = 0; i < products.Count; i++)
            {
                if (i == products.Count - 1)
                {
                    products[i].Discount = voucherDiscount - allocated;
                }
                else
                {
                    decimal part = Math.Round((products[i].Total / orderTotal) * voucherDiscount, 0, MidpointRounding.AwayFromZero);
                    products[i].Discount = part;
                    allocated += part;
                }
            }
        }
    }
}
