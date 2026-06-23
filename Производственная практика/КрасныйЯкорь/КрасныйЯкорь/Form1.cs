using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace КрасныйЯкорь
{
    public partial class Form1 : Form
    {
        string connectionString = "Server=DESKTOP-VMAF0PK;Database=КрасныйЯкорь;Trusted_Connection=True;TrustServerCertificate=True;";
        SqlDataAdapter adapter;
        DataTable dataTable;

        public bool IsLoggedIn { get; private set; } = false;
        public string CurrentUserLogin { get; private set; }
        public string CurrentUserRole { get; private set; }
        public int CurrentUserId { get; private set; }

        public Form1()
        {
            InitializeComponent();
            BlockAllControls();
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ShowLoginForm();
        }

        private void ShowLoginForm()
        {
            LoginForm loginForm = new LoginForm();
            DialogResult result = loginForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                CurrentUserId = loginForm.UserId;
                CurrentUserLogin = loginForm.UserLogin;
                CurrentUserRole = loginForm.UserRole;
                IsLoggedIn = true;

                SetUserPermissions();
                dataGridView1.Enabled = true;
                MessageBox.Show($"Добро пожаловать, {CurrentUserLogin}!\nРоль: {CurrentUserRole}", "Успешный вход", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                Application.Exit();
            }
        }

        private void SetUserPermissions()
        {
            string role = CurrentUserRole.Trim().ToLower();

            button1.Enabled = true;

            if (role.Contains("администратор") || role.Contains("руководитель"))
            {
                button2.Enabled = true;
                button3.Enabled = true;
                dataGridView1.ReadOnly = false;
            }
            else if (role.Contains("менеджер"))
            {
                button2.Enabled = true;
                button3.Enabled = false;
                dataGridView1.ReadOnly = false;
            }
            else
            {
                button2.Enabled = false;
                button3.Enabled = false;
                dataGridView1.ReadOnly = true;
            }

            this.Text = $"Красный Якорь | {CurrentUserLogin} ({CurrentUserRole})";
        }

        private void BlockAllControls()
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            dataGridView1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!IsLoggedIn) return;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Товары";
                    adapter = new SqlDataAdapter(query, connection);
                    new SqlCommandBuilder(adapter);
                    dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
                }
                MessageBox.Show("Данные успешно загружены!", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!IsLoggedIn || dataTable == null) return;
            DataRow newRow = dataTable.NewRow();
            dataTable.Rows.Add(newRow);
            dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;
            dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1];
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!IsLoggedIn) return;
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите строку для удаления!");
                return;
            }

            if (MessageBox.Show("Удалить выбранный товар?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.No) return;

            try
            {
                int code = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["КодТовара"].Value);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Товары WHERE КодТовара = @Код";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Код", code);
                    cmd.ExecuteNonQuery();
                }
                dataGridView1.Rows.Remove(dataGridView1.SelectedRows[0]);
                MessageBox.Show("Товар удален!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Выйти из системы?", "Выход", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                IsLoggedIn = false;
                dataTable = null;
                dataGridView1.DataSource = null;
                BlockAllControls();
                this.Text = "Красный Якорь";
                ShowLoginForm();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (dataTable != null && dataTable.GetChanges() != null && IsLoggedIn)
            {
                try
                {
                    adapter.Update(dataTable);
                    MessageBox.Show("Все изменения сохранены в базу данных!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка сохранения: " + ex.Message);
                }
            }
        }
    }
}