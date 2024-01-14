using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Firebase.Storage;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using project.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace project.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    public static string? userMail;
    public static string? reuserMail;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Auth()
    {
        return View();
    }

    public IActionResult Admin()
    {
        List<MyViewModels> modelList = new List<MyViewModels>();
        MySqlConnection connection = Connection();
        string users_query = $"SELECT id_user, email, state, dostup FROM users;";
        MySqlCommand users_command = new MySqlCommand(users_query, connection);
        MySqlDataReader users_reader = users_command.ExecuteReader();
        while (users_reader.Read())
        {
            var users = new Users()
            {
                Id = Convert.ToInt32(users_reader["id_user"]),
                Email = users_reader["email"].ToString(),
                State = users_reader["state"].ToString(),
                Dostup = users_reader["dostup"].ToString()
            };
            var model = new MyViewModels()
            {
                Users = users
            };
            modelList.Add(model);
        }
        return View(modelList);
    }


    [HttpPost]
    public IActionResult Admin(string email, string state, int id, string action)
    {
        MySqlConnection connection = Connection();
        if (action == "delete")
        {
            string users_query = $"DELETE FROM users WHERE id_user='{id}';";
            MySqlCommand users_command = new MySqlCommand(users_query, connection);
            int amount = Convert.ToInt32(users_command.ExecuteScalar());
            if (amount > 1)
            {
                string message = "Запись успешно удалена!";
                ViewBag.Message = message;
            }
        }
        else if (action == "save")
        {
            string users_query = $"UPDATE users SET email='{email}', state='{state}' where id_user='{id}';";
            MySqlCommand users_command = new MySqlCommand(users_query, connection);
            int amount = Convert.ToInt32(users_command.ExecuteScalar());
            if (amount > 1)
            {
                string message = "Запись успешно обнавлена!";
                ViewBag.Message = message;
            }
        }
        return RedirectToAction("Admin");
    }

    [HttpPost]
    public IActionResult Block(string selectedBlock)
    {
        var indices = selectedBlock.Split(',');
        MySqlConnection connection = Connection();
        connection.Open();
        foreach (var index in indices)
        {
            int id = int.Parse(index);
            string query = $"UPDATE users SET dostup='blocked' WHERE id_user={id};";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
                Block_you(id);
            }
        }
        if (reuserMail == userMail)
        {
            return RedirectToAction("Auth");
        }
        else
        {
            return RedirectToAction("Admin");
        }

    }

    public void Block_you(int id)
    {
        MySqlConnection connection = Connection();
        connection.Open();
        string query = $"SELECT email FROM users WHERE dostup='blocked' and id_user='{id}';";
        using (MySqlCommand command = new MySqlCommand(query, connection))
        {
            command.ExecuteNonQuery();
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string value = reader.GetString(0);
                reuserMail = value;
            }
            connection.Close();
        }
    }

    [HttpPost]
    public IActionResult Unblock(string selectedUnblock)
    {
        var indices = selectedUnblock.Split(',');
        MySqlConnection connection = Connection();
        connection.Open();
        foreach (var index in indices)
        {
            int id = int.Parse(index);
            string query = $"UPDATE users SET dostup='active' WHERE id_user={id};";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        return RedirectToAction("Admin");
    }

    public IActionResult Login()
    {
        return View();
    }
    public IActionResult Home_auth()
    {
        MySqlConnection connection = Connection();
        string query = $"SELECT * FROM collections;";
        MySqlCommand command = new MySqlCommand(query, connection);
        MySqlDataReader reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            List<Collections> collections = new List<Collections>();
            while (reader.Read())
            {
                Collections model = new Collections();
                model.Id_collection = reader.GetInt32(0);
                model.Email = reader.GetString(1);
                model.Name = reader.GetString(2);
                model.Description = reader.GetString(3);
                model.ImageUrl = reader.GetString(4);
                model.Category = reader.GetString(5);
                model.Id_list = reader.GetInt32(6);
                collections.Add(model);
            }
            ViewBag.CollectionModels = collections;
            return View(collections);
        }
        reader.Close();
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
        string query = $"SELECT COUNT(*) FROM users WHERE email='{email}'";

        using (MySqlCommand command = new MySqlCommand(query, connection))
        {
            int orgCount = Convert.ToInt32(command.ExecuteScalar());
            if (orgCount <= 0)
            {
                string mess = "Пользователя c таким ником нет, зарегистрируйтесь и попробуйте еще раз!";
                ViewBag.Message = mess;
                return View();
            }
            else
            {
                string hashPassword = HashPassword(password);
                string query2 = $"select count(*) from users where email='{email}' and password='{hashPassword}';";
                MySqlCommand cmd2 = new MySqlCommand(query2, connection);
                int count = Convert.ToInt32(cmd2.ExecuteScalar());
                if (count == 0)
                {
                    string mess = "Пароль неверный!";
                    ViewBag.Message = mess;
                    return View();
                }
                else
                {
                    string query_dostup = $"select count(*) from users where email='{email}' and dostup='active';";
                    MySqlCommand cmd_dostup = new MySqlCommand(query_dostup, connection);
                    int count1 = Convert.ToInt32(cmd_dostup.ExecuteScalar());
                    if (count1 == 0)
                    {
                        string mess = "Ваш аккаунт заблокирован!";
                        ViewBag.Message = mess;
                        return View();
                    }
                    else
                    {
                        string query3 = $"select count(*) from users where email='{email}' and state='user';";
                        MySqlCommand cmd3 = new MySqlCommand(query3, connection);
                        int count3 = Convert.ToInt32(cmd3.ExecuteScalar());
                        if (count3 == 0)
                        {
                            connection.Close();
                            return RedirectToAction("Admin");
                        }
                        else
                        {
                            userMail = email;
                            connection.Close();
                            return RedirectToAction("Home_auth");
                        }
                    }
                }
            }
        }
    }

    public IActionResult Home()
    {
        MySqlConnection connection = Connection();
        string query = $"SELECT * FROM collections;";
        MySqlCommand command = new MySqlCommand(query, connection);
        MySqlDataReader reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            List<Collections> collections = new List<Collections>();
            while (reader.Read())
            {
                Collections model = new Collections();
                model.Id_collection = reader.GetInt32(0);
                model.Email = reader.GetString(1);
                model.Name = reader.GetString(2);
                model.Description = reader.GetString(3);
                model.ImageUrl = reader.GetString(4);
                model.Category = reader.GetString(5);
                model.Id_list = reader.GetInt32(6);
                collections.Add(model);
            }
            ViewBag.CollectionModels = collections;
            return View(collections);
        }
        reader.Close();
        return View();
    }

    public IActionResult Items()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Items(int id)
    {
        List<MyViewModels> modelList = new List<MyViewModels>();
        MySqlConnection connection = Connection();
        string query = $"SELECT items.name, items.tags, lists.string1_name, items.string1," +
            $"lists.string2_name, items.string2, lists.string3_name, items.string3, lists.multiline1_name, items.multiline1," +
            $"lists.multiline2_name, items.multiline2, lists.multiline3_name, items.multiline3, lists.int1_name, items.int_1, " +
            $"lists.int2_name, items.int_2, lists.int3_name, items.int_3, lists.checkbox1_name, items.checkbox1," +
            $"lists.checkbox2_name, items.checkbox2, lists.checkbox3_name, items.checkbox3, " +
            $"lists.data1_name, items.data1, lists.data2_name, items.data2, " +
            $"lists.data3_name, items.data3 FROM lists JOIN items ON lists.id_list = items.id_list WHERE id_collection = {id};";
        MySqlCommand command = new MySqlCommand(query, connection);
        MySqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            var lists = new Lists();
            var items = new Items();
            var model = new MyViewModels()
            {
                Lists = lists,
                Items = items
            };
            model.Items.Name = reader.GetString(0);
            model.Items.Tags = reader.GetString(1);
            if (!reader.IsDBNull(2) && !reader.IsDBNull(3))
            {
                model.Lists.String1_name = reader.GetString(2);
                model.Items.String1 = reader.GetString(3);
            }
            if (!reader.IsDBNull(4) && !reader.IsDBNull(5))
            {
                model.Lists.String2_name = reader.GetString(4);
                model.Items.String2 = reader.GetString(5);
            }
            if (!reader.IsDBNull(6) && !reader.IsDBNull(7))
            {
                model.Lists.String3_name = reader.GetString(6);
                model.Items.String3 = reader.GetString(7);
            }
            if (!reader.IsDBNull(8) && !reader.IsDBNull(9))
            {
                model.Lists.Multiline1_name = reader.GetString(8);
                model.Items.Multiline1 = reader.GetString(9);
            }
            if (!reader.IsDBNull(10) && !reader.IsDBNull(11))
            {
                model.Lists.Multiline2_name = reader.GetString(10);
                model.Items.Multiline2 = reader.GetString(11);
            }
            if (!reader.IsDBNull(12) && !reader.IsDBNull(13))
            {
                model.Lists.Multiline3_name = reader.GetString(12);
                model.Items.Multiline3 = reader.GetString(13);
            }
            if (!reader.IsDBNull(14) && reader.GetInt32(15) != 0)
            {
                model.Lists.Int1_name = reader.GetString(14);
                model.Items.Int1 = reader.GetInt32(15);
            }
            if (!reader.IsDBNull(16) && reader.GetInt32(17) != 0)
            {
                model.Lists.Int2_name = reader.GetString(16);
                model.Items.Int2 = reader.GetInt32(17);
            }
            if (!reader.IsDBNull(18) && reader.GetInt32(19) != 0)
            {
                model.Lists.Int3_name = reader.GetString(18);
                model.Items.Int3 = reader.GetInt32(19);
            }
            if (!reader.IsDBNull(20) && reader.GetInt32(21) != 0)
            {
                model.Lists.Checkbox1_name = reader.GetString(20);
                model.Items.Checkbox1 = (Checkbox)reader.GetInt32(21);
            }
            if (!reader.IsDBNull(22) && reader.GetInt32(23) != 0)
            {
                model.Lists.Checkbox2_name = reader.GetString(22);
                model.Items.Checkbox2 = (Checkbox)reader.GetInt32(23);
            }
            if (!reader.IsDBNull(24) && reader.GetInt32(25) != 0)
            {
                model.Lists.Checkbox3_name = reader.GetString(24);
                model.Items.Checkbox3 = (Checkbox)reader.GetInt32(25);
            }
            if (!reader.IsDBNull(26) && reader.GetInt32(27) != 0)
            {
                model.Lists.Data1_name = reader.GetString(26);
                model.Items.Data1 = reader.GetDateTime(27);
            }
            if (!reader.IsDBNull(28) && reader.GetInt32(29) != 0)
            {
                model.Lists.Data2_name = reader.GetString(28);
                model.Items.Data2 = reader.GetDateTime(29);
            }
            if (!reader.IsDBNull(30) && reader.GetInt32(31) != 0)
            {
                model.Lists.Data3_name = reader.GetString(30);
                model.Items.Data3 = reader.GetDateTime(31);
            }
            modelList.Add(model);
        }
        reader.Close();
        connection.Close();
        return View(modelList);
    }

    public IActionResult Add()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Add(string email, string name, string description, IFormFile image, string category,
        string string1, string string2, string string3, string multiline1, string multiline2, string multiline3,
        string int1, string int2, string int3, string checkbox1, string checkbox2, string checkbox3,
        string data1, string data2, string data3)
    {
        List<string> list = new List<string>();
        list.Clear();
        if (!string.IsNullOrEmpty(string1))
            list.Add(string1);
        if (!string.IsNullOrEmpty(string2))
            list.Add(string2);
        if (!string.IsNullOrEmpty(string3))
            list.Add(string3);
        if (!string.IsNullOrEmpty(multiline1))
            list.Add(multiline1);
        if (!string.IsNullOrEmpty(multiline2))
            list.Add(multiline2);
        if (!string.IsNullOrEmpty(multiline3))
            list.Add(multiline3);
        if (!string.IsNullOrEmpty(int1))
            list.Add(int1);
        if (!string.IsNullOrEmpty(int2))
            list.Add(int2);
        if (!string.IsNullOrEmpty(int3))
            list.Add(int3);
        if (!string.IsNullOrEmpty(checkbox1))
            list.Add(checkbox1);
        if (!string.IsNullOrEmpty(checkbox2))
            list.Add(checkbox2);
        if (!string.IsNullOrEmpty(checkbox3))
            list.Add(checkbox3);
        if (!string.IsNullOrEmpty(data1))
            list.Add(data1);
        if (!string.IsNullOrEmpty(data2))
            list.Add(data2);
        if (!string.IsNullOrEmpty(data3))
            list.Add(data3);
        if (list != null)
        {
            MySqlConnection connection = Connection();
            string query = $"INSERT INTO `lists`( `id_list`, `custom_string1_name`, `custom_string2_name`, `custom_string3_name`, " +
              $"`custom_multiline1_name`, `custom_multiline2_name`, `custom_multiline3_name`," +
              $" `custom_int1_name`, `custom_int2_name`, `custom_int3_name`," +
              $" `custom_checkbox1_name`, `custom_checkbox2_name`, `custom_checkbox3_name`, " +
              $"`custom_data1_name`, `custom_data2_name`, `custom_data3_name`) VALUES " +
              $"(default, @string1, @string2, @string3, @multiline1, @multiline2, @multiline3, @int1, @int2, @int3," +
              $"@checkbox1, @checkbox2, @checkbox3, @data1, @data2, @data3);";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@string1", string1);
            command.Parameters.AddWithValue("@string2", string2);
            command.Parameters.AddWithValue("@string3", string3);
            command.Parameters.AddWithValue("@multiline1", multiline1);
            command.Parameters.AddWithValue("@multiline2", multiline2);
            command.Parameters.AddWithValue("@multiline3", multiline3);
            command.Parameters.AddWithValue("@int1", int1);
            command.Parameters.AddWithValue("@int2", int2);
            command.Parameters.AddWithValue("@int3", int3);
            command.Parameters.AddWithValue("@checkbox1", checkbox1);
            command.Parameters.AddWithValue("@checkbox2", checkbox2);
            command.Parameters.AddWithValue("@checkbox3", checkbox3);
            command.Parameters.AddWithValue("@data1", data1);
            command.Parameters.AddWithValue("@data2", data2);
            command.Parameters.AddWithValue("@data3", data3);
            command.ExecuteScalar();
            string imageUrl = await Updownload(image);
            long lastInsertId = command.LastInsertedId;
            string query_collections = $"INSERT INTO `collections`(`id_collection`, `email`, `name`, `description`, `imageUrl`, `category`, `id_list`) " +
              $"VALUES (default,'{email}','{name}','{description}','{ViewBag.ImageUrl}','{category}', '{lastInsertId}');";
            MySqlCommand command_collections = new MySqlCommand(query_collections, connection);
            command_collections.ExecuteNonQuery();
            connection.Close();
        }
        else
        {
            MySqlConnection connection = Connection();
            string imageUrl = await Updownload(image);
            string query_collections = $"INSERT INTO `collections`(`id_collection`, `email`, `name`, `description`, `imageUrl`, `category`, `id_list`) " +
              $"VALUES (default,'{email}','{name}','{description}','{ViewBag.ImageUrl}','{category}', '0');";
            MySqlCommand command_collections = new MySqlCommand(query_collections, connection);
            command_collections.ExecuteNonQuery();
        }
        return View();
    }

    public async Task<string> Updownload(IFormFile photo)
    {
        if (photo != null && photo.Length > 0)
        {
            var storage = new FirebaseStorage("myapp-ca9c3.appspot.com");
            var fileStream = photo.OpenReadStream();
            var fileName = photo.FileName;
            var task = storage.Child("images").Child(fileName).PutAsync(fileStream);
            var downloadUrl = await task;
            var imageUrl = downloadUrl.ToString();
            ViewBag.ImageUrl = imageUrl;
        }
        return null;
    }

    public MySqlConnection Connection()
    {
        MySqlConnection connection = new MySqlConnection("Server=mysql6013.site4now.net;Database=db_aa373f_root;Uid=aa373f_root;Pwd=rootroot1;Charset=utf8;");
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