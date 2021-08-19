using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hometech.Models;
using Microsoft.AspNetCore.Authorization;
using MySqlConnector;

namespace Hometech.Controllers
{
    public class ProductsInfo
    {
        public string category { get; set; }
        public string manufacturer { get; set; }
        public string vendor_code { get; set; }
        public decimal price { get; set; }
        public bool is_available { get; set; }
        public bool in_cart { get; set; }
        public int amount { get; set; }
        public int number { get; set; }
        //public static string StatusMessage { get; set; }
        public ProductsInfo(string c, string m, string vc, decimal p, bool ia,bool ic,int a,int n)
        {
            category = c;
            manufacturer = m;
            vendor_code = vc;
            price = p;
            is_available = ia;
            in_cart = ic;
            amount = a;
            number = n;
        }
    }
    public class HomeController : Controller
    {
        private const string ConnectionString = "Server=localhost; Port=3306; Database=hometech; Uid=root; Pwd=kenowi36;";
        
        public async Task<IActionResult> AllProducts()
        {
            List<ProductsInfo> stockProducts=new();
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT category.title category, manufacturer.title manufacturer, vendor_code, price, amount from hometech.stock_products LEFT JOIN hometech.manufacturer USING (id_manufacturer)LEFT JOIN hometech.category USING (id_category);";
            MySqlDataReader reader =await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var category = reader.GetString(reader.GetOrdinal("category"));
                var manufacturer = reader.GetString(reader.GetOrdinal("manufacturer"));
                var vendorCode = reader.GetString(reader.GetOrdinal("vendor_code"));
                var price = reader.GetDecimal(reader.GetOrdinal("price"));
                var amount = reader.GetInt32(reader.GetOrdinal("amount"));
                var isAvailable = amount > 0;
                var product=new ProductsInfo(category,manufacturer, vendorCode,price,isAvailable,false,amount,0);
                stockProducts.Add(product);
            }
            await reader.CloseAsync();
            foreach (var p in stockProducts)
            {
                if (!User.IsInRole("Клиент")) continue;
                cmd.CommandText =
                        $"select count(*) from hometech.cart where vendor_code='{p.vendor_code}' and id_client=(select id_client from hometech.client where client_login='{User.Identity.Name}')";
                var inCart = (long)cmd.ExecuteScalar() == 1;
                p.in_cart = inCart;
            }
            return View(stockProducts);
        }
        [Authorize(Roles="Клиент")]
        public async Task<IActionResult> Cart()
        {
            List<ProductsInfo> cart=new();
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            List<string> vendorCodes = new();
            cmd.CommandText = $"select id_client from hometech.client where client_login='{User.Identity.Name}'";
            var id = (int)cmd.ExecuteScalar(); 
            cmd.CommandText = $"select vendor_code from hometech.cart c left join hometech.stock_products using (vendor_code) where amount=0 and id_client='{id}'";
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                vendorCodes.Add(reader.GetString(reader.GetOrdinal("vendor_code")));
            }
            await reader.CloseAsync();
            if (vendorCodes.Count > 0)
            {
                cmd.CommandText =
                    $"delete from hometech.cart c where (select amount from hometech.stock_products sp where sp.vendor_code=c.vendor_code)=0 and id_client='{id}'";
                await cmd.ExecuteNonQueryAsync();
            }
            cmd.CommandText = $"select count(*) from hometech.cart where id_client='{id}'";
            var count = (long)cmd.ExecuteScalar();
            if (count==0) 
                return Redirect("/Home/EmptyCart");
            cmd.CommandText =$"SELECT category.title category, manufacturer.title manufacturer, vendor_code, price,amount from hometech.stock_products RIGHT JOIN hometech.cart USING(vendor_code) LEFT JOIN hometech.manufacturer USING (id_manufacturer) LEFT JOIN hometech.category USING (id_category) WHERE id_client='{id}';";
            reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var category= reader.GetString(reader.GetOrdinal("category"));
                var manufacturer= reader.GetString(reader.GetOrdinal("manufacturer"));
                var vendorCode= reader.GetString(reader.GetOrdinal("vendor_code"));
                var price = reader.GetDecimal(reader.GetOrdinal("price"));
                var amount = reader.GetInt32(reader.GetOrdinal("amount"));
                var isAvailable = amount > 0;
                var product=new ProductsInfo(category,manufacturer, vendorCode,price,isAvailable,true,amount,1);
                cart.Add(product);
            }
            return View(cart);
        }
        public IActionResult EmptyCart()
        {
            return View();
        }
        public async Task<IActionResult> ClearCart()
        {
            await using var con = new MySqlConnection("Server=localhost; Port=3306; Database=hometech; Uid=root; Pwd=kenowi36");
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = $"select id_client from hometech.client where client_login='{User.Identity.Name}'";
            var id = (int)cmd.ExecuteScalar(); 
            cmd.CommandText = $"delete from hometech.cart where id_client='{id}';";
            await cmd.ExecuteNonQueryAsync();
            return RedirectToAction(nameof(Cart));
        }
        public async Task<IActionResult> AddItem(string vendorCode)
        {
            await using var con = new MySqlConnection("Server=localhost; Port=3306; Database=hometech; Uid=root; Pwd=kenowi36");
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = $"select id_client from hometech.client where client_login='{User.Identity.Name}'";
            var id = (int)cmd.ExecuteScalar(); 
            cmd.CommandText = $"insert into hometech.cart (id_client, vendor_code) values ('{id}','{vendorCode}')";
            await cmd.ExecuteNonQueryAsync();
            return RedirectToAction(nameof(AllProducts));
        }
        public async Task<IActionResult> DeleteItem(string vendorCode)
        {
            await using var con = new MySqlConnection("Server=localhost; Port=3306; Database=hometech; Uid=root; Pwd=kenowi36");
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = $"select id_client from hometech.client where client_login='{User.Identity.Name}'";
            var id = (int)cmd.ExecuteScalar(); 
            cmd.CommandText = $"delete from hometech.cart where id_client='{id}' and vendor_code='{vendorCode}'";
            await cmd.ExecuteNonQueryAsync();
            return RedirectToAction(nameof(Cart));
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}