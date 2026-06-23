using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace КрасныйЯкорь
{
    public partial class LoginForm : Form
    {
        string connectionString = "Server=DESKTOP-VMAF0PK;Database=КрасныйЯкорь;Trusted_Connection=True;TrustServerCertificate=True;";

        // Свойства для передачи данных в главную форму
        public string UserLogin { get; private set; }
        public string UserRole { get; private set; }
        public int UserId { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            this.Text = "Вход в систему";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AcceptButton = btnLogin;   
            this.CancelButton = btnCancel;  
            btnLogin.Click += btnLogin_Click;
            btnCancel.Click += btnCancel_Click;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLogin.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT КодПользователя, Логин, Должность, Активен 
                                    FROM Пользователи 
                                    WHERE Логин = @Login AND Пароль = @Password AND Активен = 1";

                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Login", txtLogin.Text.Trim());
                    cmd.Parameters.AddWithValue("@Password", txtPassword.Text);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        UserId = Convert.ToInt32(reader["КодПользователя"]);
                        UserLogin = reader["Логин"].ToString();
                        UserRole = reader["Должность"].ToString();

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль, либо пользователь отключен!", "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к БД: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}