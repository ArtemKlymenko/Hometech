using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Hometech.Models;
using Hometech.Models.ManageViewModels;
using MySqlConnector;

namespace Hometech.Controllers
{
    [Authorize(Roles = "Администратор,Клиент,Курьер")]
    [Route("[controller]/[action]")]
    public class ManageController : Controller
    {
        private const string ConnectionString = "Server=localhost; Port=3306; Database=hometech; Uid=root; Pwd=kenowi36;";
        private readonly UserManager<user_info> _userManager;
        private readonly SignInManager<user_info> _signInManager;
        private readonly ILogger _logger;

        public ManageController(
          UserManager<user_info> userManager,
          SignInManager<user_info> signInManager,
          ILogger<ManageController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [HttpGet]
        public async Task<IActionResult> EditPersonal()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new EditPersonalViewModel
            {
                Username = user.login,
                Name=user.name,
                Surname=user.surname,
                Email = user.email,
                PhoneNumber = user.phone_number,
                IsEmailConfirmed = user.email_confirm,
                StatusMessage = StatusMessage
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPersonal(EditPersonalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var email = user.email;
            if (model.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.id_user}'.");
                }
            }

            var phoneNumber = user.phone_number;
            if (model.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.id_user}'.");
                }
            } 
            var name = user.name;
            var surname = user.surname;
            if (model.Name != name || model.Surname != surname)
            {
                await using var con =
                    new MySqlConnection(ConnectionString);
                await using var cmd = con.CreateCommand();
                await con.OpenAsync();
                cmd.CommandText =
                    $"update hometech.user_info set name='{model.Name}', surname='{model.Surname}' where id_user={user.id_user}";
                await cmd.ExecuteNonQueryAsync();
            }
            StatusMessage = "Вы обновили свой профиль.";
            return RedirectToAction(nameof(EditPersonal));
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToAction(nameof(SetPassword));
            }

            var model = new ChangePasswordViewModel { StatusMessage = StatusMessage };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                AddErrors(changePasswordResult);
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("User changed their password successfully.");
            StatusMessage = "Ваш пароль изменён.";
            
            return View(model);
            // return RedirectToAction(nameof(ChangePassword));
        }

        [HttpGet]
        public async Task<IActionResult> SetPassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);

            if (hasPassword)
            {
                return RedirectToAction(nameof(ChangePassword));
            }

            var model = new SetPasswordViewModel { StatusMessage = StatusMessage };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                AddErrors(addPasswordResult);
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            StatusMessage = "Пароль установлен.";

            return RedirectToAction(nameof(SetPassword));
        }
        [HttpGet]
        [Authorize(Roles="Клиент")]
        public async Task<IActionResult> ChangeDeliveryAddress()
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
            var model = new ChangeDeliveryAddress
            {
                City=city,
                Street = street,
                HomeNumber = homeNumber,
                StatusMessage = StatusMessage
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Клиент")]
        public async Task<IActionResult> ChangeDeliveryAddress(ChangeDeliveryAddress model)
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
            await reader.CloseAsync();
            if (model.City != city || model.Street != street ||model.HomeNumber != homeNumber)
            {
                cmd.CommandText =
                    $"update hometech.client set city='{model.City}', street='{model.Street}', home_number={model.HomeNumber} where client_login='{user.login}'";
                await cmd.ExecuteNonQueryAsync();
                StatusMessage = "Ваш адрес доставки успешно изменён.";
            }
            return RedirectToAction(nameof(ChangeDeliveryAddress));
        }
        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        #endregion
    }
}