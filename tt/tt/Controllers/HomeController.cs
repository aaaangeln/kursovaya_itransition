using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Text;
using tt.Models;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Firebase.Storage;

namespace tt.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    public List<string> category = new List<string>();

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }


    [HttpPost]
    public async Task<ActionResult> Updownload(IFormFile photo)
    {
        if (photo != null && photo.Length > 0)
        {
            var storage = new FirebaseStorage("myapp-ca9c3.appspot.com");

            var fileStream = photo.OpenReadStream();
            var fileName = photo.FileName;
            var task = storage.Child("images").Child(fileName).PutAsync(fileStream); 
            task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"{e.Percentage}");
            var downloadUrl = await task;
            var imageUrl = downloadUrl.ToString();
            ViewBag.ImageUrl = imageUrl;
            MySqlConnection connection = Connection();
            string query = $"INSERT INTO collection (id, email, photo) VALUES (default, '1234@mtp.by', '{ViewBag.ImageUrl}');";
            MySqlCommand command = new MySqlCommand(query, connection);
            int amount = Convert.ToInt32(command.ExecuteScalar());
            if (amount < 0)
            {}
            return RedirectToAction("Add");
        }
        return View();
    }

    public IActionResult Add()
    {
        return View();
    }

    //public IActionResult Collections()
    //{
    //    MySqlConnection connection = Connection();
    //    string query_category = $"SHOW COLUMNS FROM collection LIKE 'category';";
    //    MySqlCommand command_category = new MySqlCommand(query_category, connection);
    //    MySqlDataReader reader = command_category.ExecuteReader();
    //    if (reader.Read())
    //    {
    //        string enumStr = reader["Type"].ToString();
    //        var regex = new Regex("^enum\\('(.*)'\\)$");
    //        var matches = regex.Match(enumStr);
    //        string enumValuesStr = matches.Groups[1].Value;
    //        string[] enumValues = enumValuesStr.Split("','");

    //        foreach (string value in enumValues)
    //        {
    //            category.Add(value);
    //        }
    //    }
    //    reader.Close();
    //    return View(category);
    //}

    public IActionResult Collections()
    {
        return View();
    }

    public IActionResult Collection(string alt)
    {
        MySqlConnection connection = Connection();
        string query = $"SELECT * FROM collection WHERE category='{alt}';";

        MySqlCommand command = new MySqlCommand(query, connection);
        MySqlDataReader reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            List<Collection> collections = new List<Collection>();
            while (reader.Read())
            {
                Collection model = new Collection();
                model.Id = reader.GetInt32(0);
                model.Email = reader.GetString(1);
                model.Category = reader.GetString(2);
                model.Name = reader.GetString(3);
                model.UrlImage = reader.GetString(4);
                model.Info = reader.GetString(5);
                collections.Add(model);
            }

            ViewBag.CollectionModels = collections;
            return View(collections);
        }
        reader.Close();
        return View();
    }

    public IActionResult Record(int id)
    {
        MySqlConnection connection = Connection();
        string query = $"SELECT * FROM collection WHERE id='{id}';";

        MySqlCommand command = new MySqlCommand(query, connection);
        MySqlDataReader reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            List<Collection> collections = new List<Collection>();
            while (reader.Read())
            {
                Collection model = new Collection();
                model.Id = reader.GetInt32(0);
                model.Email = reader.GetString(1);
                model.Category = reader.GetString(2);
                model.Name = reader.GetString(3);
                model.UrlImage = reader.GetString(4);
                model.Info = reader.GetString(5);
                collections.Add(model);
            }

            ViewBag.CollectionModels = collections;
            return View(collections);
        }
        reader.Close();
        return View();
    }

    public IActionResult Collections_auth()
    {
        return View();
    }

    public IActionResult Collection_auth()
    {
        return View();
    }

    public IActionResult Record_auth()
    {
        return View();
    }

    public IActionResult Auth()
    {
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string email, string password, string repassword)
    {
        MySqlConnection connection = Connection();
        string query = $"SELECT COUNT(email) FROM users WHERE email='{email}';";
        MySqlCommand command = new MySqlCommand(query, connection);
        int amount = Convert.ToInt32(command.ExecuteScalar());
        if (amount > 0)
        {
            string message = "Пользователь с таким email уже существует!";
            ViewBag.Message = message;
        }
        else
        {
            string pass = HashPassword(password);
            string repass = HashPassword(repassword);
            if (pass != repass)
            {
                string message = "Пароли не совпадают!";
                ViewBag.Message = message;
            }
            else
            {
                string query_insert = $"INSERT INTO users VALUES ('{email}','{pass}','user');";
                MySqlCommand command_insert = new MySqlCommand(query_insert, connection);
                int i = command_insert.ExecuteNonQuery();
                if (i > -1)
                {
                    string from = "aaaangeln01@mail.ru";
                    string to = email;
                    string passs = "MPZm9kUpiSbHcCkhxghx";
                    MailMessage mailMessage = new MailMessage(from, to);
                    mailMessage.Subject = "Collections";
                    mailMessage.Body = $"Добро пожаловать на сайт Collections.\nСпасибо {email} за регистрацию , мы очень рады, что Вы теперь с нами!";
                    SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587);
                    smtp.Credentials = new System.Net.NetworkCredential(from, passs);
                    smtp.EnableSsl = true;
                    smtp.Send(mailMessage);
                    return RedirectToAction("Auth");
                }
                else
                {
                    string message = "Вы не зарегистрированы, попробуте ещё раз!";
                    ViewBag.Message = message;
                    return View();
                }
            }
        }
        return View();
    }

    [HttpPost]
    public IActionResult Auth(string email, string password)
    {
        MySqlConnection connection = Connection();
        string query = $"SELECT COUNT(email) FROM users WHERE email='{email}';";
        MySqlCommand command = new MySqlCommand(query, connection);
        int amount = Convert.ToInt32(command.ExecuteScalar());
        if (amount < 1)
        {
            string message = "Пользователя с таким email нет!";
            ViewBag.Message = message;
        }
        else
        {
            string hashPassword = HashPassword(password);
            string query_password = $"SELECT COUNT(*) FROM users WHERE email='{email}' and password='{hashPassword}';";
            MySqlCommand command_password = new MySqlCommand(query_password, connection);
            int amount_password = Convert.ToInt32(command_password.ExecuteScalar());
            if (amount_password != 1)
            {
                string message = "Пароль неверный!";
                ViewBag.Message = message;
            }
            else
            {
                return RedirectToAction("Collections");
            }
        }
        return View();
    }

    public MySqlConnection Connection()
    {
        MySqlConnection connection = new MySqlConnection("Server=mysql6013.site4now.net;Database=db_aa373f_root;Uid=aa373f_root;Pwd=rootroot1;");
        connection.Open();
        return connection;
    }

    public string HashPassword(string pass)
    {
        SHA256 hash = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(pass);
        byte[] password = hash.ComputeHash(bytes);
        string hashPassword = Convert.ToBase64String(password);
        return hashPassword;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}