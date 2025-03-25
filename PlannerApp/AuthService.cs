using MySql.Data.MySqlClient;
using PlannerApp;
using System;
using System.Windows;

using System.Windows.Controls;



public class AuthService
{
    public static bool RegisterUser(string firstName, string lastName, string username, string email, string password)
    {
        using (MySqlConnection conn = DatabaseHelper.GetConnection())
        {
            try
            {
                conn.Open();

                // Проверяем, есть ли уже пользователь с таким email или username
                string checkUserQuery = "SELECT COUNT(*) FROM users WHERE email = @Email OR username = @Username";
                MySqlCommand checkCmd = new MySqlCommand(checkUserQuery, conn);
                checkCmd.Parameters.AddWithValue("@Email", email);
                checkCmd.Parameters.AddWithValue("@Username", username);

                int userExists = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (userExists > 0)
                {
                    MessageBox.Show("Пользователь с таким email или username уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Хешируем пароль
                string hashedPassword = PasswordHelper.HashPassword(password);

                // Добавляем пользователя
                string insertQuery = "INSERT INTO users (first_name, last_name, username, email, password_hash) VALUES (@FirstName, @LastName, @Username, @Email, @PasswordHash)";
                MySqlCommand cmd = new MySqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);

                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
