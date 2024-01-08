using System.Diagnostics;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Firebase.Storage;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using project.Models;

namespace project.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    public static string? userMail;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Auth()
    {
        return View();
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
    public IActionResult Auth(string email, string password) {
        if (email == "admin@admin.com")
        {
            if (password == "adminadmin")
            {
                return Redirect("/Home/Admin");
            }
            else
            {
                string mess = "Пароль неверный!";
                ViewBag.Message = mess;
                return View();
            }
        }
        else
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
                        string query2 = $"select count(*) from users where email='{email}' and password='{hashPassword}'";
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
                            userMail = email;
                            connection.Close();
                            return RedirectToAction("Home_auth");
                        }
                    }
                }
            }
        
        return View();
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
        MySqlConnection connection = Connection();
        string query = $"SELECT * FROM lists where id_list='{id}';";
        MySqlCommand command = new MySqlCommand(query, connection);
        MySqlDataReader reader = command.ExecuteReader();
        if (reader.HasRows)
        {
            List<Lists> lists = new List<Lists>();
            while (reader.Read())
            {
                Lists model = new Lists();
                model.Id_list = reader.GetInt32(0);
                if (!reader.IsDBNull(2) || reader.GetInt32(1) == 0)
                {
                    model.Custom_string1_state = reader.GetInt32(1);
                    model.Custom_string1_name = reader.GetString(2);
                }
                else if (!reader.IsDBNull(4) && reader.GetInt32(3) == 0)
                {
                    model.Custom_string2_state = reader.GetInt32(3);
                    model.Custom_string2_name = reader.GetString(4);
                }
                else if (!reader.IsDBNull(6) && reader.GetInt32(5) == 0)
                {
                    model.Custom_string3_state = reader.GetInt32(5);
                    model.Custom_string3_name = reader.GetString(6);
                }
                else if (!reader.IsDBNull(8) && reader.GetInt32(7) == 0)
                {
                    model.Custom_multiline1_state = reader.GetInt32(7);
                    model.Custom_multiline1_name = reader.GetString(8);
                }
                else if (!reader.IsDBNull(10) && reader.GetInt32(9) == 0)
                {
                    model.Custom_multiline2_state = reader.GetInt32(9);
                    model.Custom_multiline2_name = reader.GetString(10);
                }
                else if (!reader.IsDBNull(12) && reader.GetInt32(11) == 0)
                {
                    model.Custom_multiline3_state = reader.GetInt32(11);
                    model.Custom_multiline3_name = reader.GetString(12);
                }
                else if (!reader.IsDBNull(14) && reader.GetInt32(13) == 0)
                {
                    model.Custom_int1_state = reader.GetInt32(13);
                    model.Custom_int1_name = reader.GetString(14);
                }
                else if (!reader.IsDBNull(16) && reader.GetInt32(15) == 0)
                {
                    model.Custom_int2_state = reader.GetInt32(15);
                    model.Custom_int2_name = reader.GetString(16);
                }
                else if (!reader.IsDBNull(18) && reader.GetInt32(17) == 0)
                {
                    model.Custom_int3_state = reader.GetInt32(17);
                    model.Custom_int3_name = reader.GetString(18);
                }
                else if (!reader.IsDBNull(20) && reader.GetInt32(19) == 0)
                {
                    model.Custom_checkbox1_state = reader.GetInt32(19);
                    model.Custom_checkbox1_name = reader.GetString(20);
                }
                else if (!reader.IsDBNull(22) && reader.GetInt32(21) == 0)
                {
                    model.Custom_checkbox2_state = reader.GetInt32(21);
                    model.Custom_checkbox2_name = reader.GetString(22);
                }
                else if (!reader.IsDBNull(24) && reader.GetInt32(23) == 0)
                {
                    model.Custom_checkbox3_state = reader.GetInt32(23);
                    model.Custom_checkbox3_name = reader.GetString(24);
                }
                else if (!reader.IsDBNull(26) && reader.GetInt32(25) == 0)
                {
                    model.Custom_data1_state = reader.GetInt32(25);
                    model.Custom_data1_name = reader.GetString(26);
                }
                else if (!reader.IsDBNull(28) && reader.GetInt32(27) == 0)
                {
                    model.Custom_data2_state = reader.GetInt32(27);
                    model.Custom_data2_name = reader.GetString(28);
                }
                else if (!reader.IsDBNull(30) && reader.GetInt32(29) == 0)
                {
                    model.Custom_data3_state = reader.GetInt32(29);
                    model.Custom_data3_name = reader.GetString(30);
                }
                lists.Add(model);
            }
            ViewBag.ListsModel = lists;
            return RedirectToAction("Items", lists);
        }
        reader.Close();
        return View();
    }


    //public IActionResult Items()
    //{
    //    MySqlConnection connection = Connection();
    //    string query = $"SELECT * FROM lists;";
    //    MySqlCommand command = new MySqlCommand(query, connection);
    //    MySqlDataReader reader = command.ExecuteReader();
    //    if (reader.HasRows)
    //    {
    //        List<Lists> lists = new List<Lists>();
    //        while (reader.Read())
    //        {
    //            Lists model = new Lists();
    //            model.Id_list = reader.GetInt32(0);
                
    //            lists.Add(model);
    //        }
    //        ViewBag.CollectionModels = lists;
    //        return View(lists);
    //    }
    //    reader.Close();
    //    return View();
    //}

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
        else {
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

