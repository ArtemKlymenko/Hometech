using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hometech.Models;
using Hometech.Models.OrderViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace Hometech.Controllers
{
    public class OrderController : Controller
    {
        private readonly UserManager<user_info> _userManager;
        private const string ConnectionString = "Server=localhost; Port=3306; Database=hometech; Uid=root; Pwd=kenowi36;";
        public OrderController(UserManager<user_info> userManager)
        {
            _userManager = userManager;
        }
        [HttpGet]
        [Authorize(Roles = "Клиент")]
        public async Task<IActionResult> Products()
        {
            List<ProductsInfo> stockProducts=new();
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = $"select id_client from hometech.client where client_login='{User.Identity.Name}'";
            var id = (int)cmd.ExecuteScalar(); 
            cmd.CommandText =$"SELECT category.title category, manufacturer.title manufacturer, vendor_code, price,amount from hometech.stock_products RIGHT JOIN hometech.cart USING(vendor_code) LEFT JOIN hometech.manufacturer USING (id_manufacturer) LEFT JOIN hometech.category USING (id_category) WHERE id_client='{id}';";
            MySqlDataReader reader =await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var category = reader.GetString(reader.GetOrdinal("category"));
                var manufacturer = reader.GetString(reader.GetOrdinal("manufacturer"));
                var vendorCode = reader.GetString(reader.GetOrdinal("vendor_code"));
                var price = reader.GetDecimal(reader.GetOrdinal("price"));
                var amount = reader.GetInt32(reader.GetOrdinal("amount"));
                var isAvailable = amount > 0;
                var product=new ProductsInfo(category,manufacturer, vendorCode,price,isAvailable,false,amount,1);
                stockProducts.Add(product);
            }
            await reader.CloseAsync();
            return View(stockProducts);
        }

        // [HttpPost]
        // [Authorize(Roles = "Клиент")]
        // public async Task<IActionResult> Products(ProductsViewModel model)
        // {
        //     
        // }
        [HttpGet]
        [Authorize(Roles = "Клиент")]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            await using var con =
                new MySqlConnection(ConnectionString);
            await using var cmd = con.CreateCommand();
            await con.OpenAsync();
            cmd.CommandText =
                $"select city,street,home_number from hometech.client where client_login='{user.login}'";
            var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            var city = reader.GetString(reader.GetOrdinal("city"));
            var street = reader.GetString(reader.GetOrdinal("street"));
            var homeNumber = reader.GetInt32(reader.GetOrdinal("home_number"));
            var model = new CheckoutViewModel
            {
                Name = user.name,
                Surname = user.surname,
                Email = user.email,
                PhoneNumber = user.phone_number,
                City = city,
                Street = street,
                HomeNumber = homeNumber,
                RememberAddress = false,
                RememberPersonal = false
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            await using var con =
                new MySqlConnection(ConnectionString);
            await using var cmd = con.CreateCommand();
            await con.OpenAsync();
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var email = user.email;
            if (model.Email != email && model.RememberPersonal)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.id_user}'.");
                }
            }
            var phoneNumber = user.phone_number;
            if (model.PhoneNumber != phoneNumber && model.RememberPersonal)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.id_user}'.");
                }
            } 
            var name = user.name;
            var surname = user.surname;
            if (model.Name != name || model.Surname != surname && model.RememberPersonal)
            {
                cmd.CommandText =
                    $"update hometech.user_info set name='{model.Name}', surname='{model.Surname}' where id_user={user.id_user}";
                await cmd.ExecuteNonQueryAsync();
            }
            cmd.CommandText =
                $"select city,street,home_number from hometech.client where client_login='{user.login}'";
            var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            var city = reader.GetString(reader.GetOrdinal("city"));
            var street = reader.GetString(reader.GetOrdinal("street"));
            var homeNumber = reader.GetInt32(reader.GetOrdinal("home_number"));
            reader.Close();
            if ((model.City != city || model.Street != street ||model.HomeNumber != homeNumber) && model.RememberAddress)
            {
                cmd.CommandText =
                    $"update hometech.client set city='{model.City}', street='{model.Street}', home_number={model.HomeNumber} where client_login='{user.login}'";
                await cmd.ExecuteNonQueryAsync();
            }
            return View();
        }
    }
}