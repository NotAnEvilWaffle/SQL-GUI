using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Data.SQLite;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace SQL_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static SQLiteConnection? sqlCon;
        public static SQLiteDataReader? sqlReader;
        public static SQLiteCommand? sqlCommand;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void FileChoose(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if(openFileDialog.ShowDialog() == true)
            {
                sqlCon = new SQLiteConnection("Data Source=" + openFileDialog.FileName + ";Version=3");
                sqlCon.Open();

                FillStackHeader(sqlCon);
            }

            
        }

        private void ExecQuery(object sender, RoutedEventArgs e)
        {

            if (sqlCon.State == System.Data.ConnectionState.Open)
            {
                //listBoxSQL.Items.Clear();
                sqlGridView.Columns.Clear();
                
                sqlCommand = sqlCon.CreateCommand();
                sqlCommand.CommandText = Query.Text;

                try
                {
                    sqlReader = sqlCommand.ExecuteReader();

                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show("Error in SQLite Query \n" + ex.Message);
                }

                for (int i = 0; i < sqlReader.FieldCount; i++)
                {
                    string header = sqlReader.GetName(i);
                    sqlGridView.Columns.Add(new GridViewColumn
                    {
                        Header = header,
                        Width = double.NaN,
                        DisplayMemberBinding = new Binding(String.Format("[{0}]", i))
                    });
                }

                listView.View = sqlGridView;

                //StringBuilder sb = new StringBuilder();
                ObservableCollection<List<string>> rows = new ObservableCollection<List<string>>();
                Binding binding = new Binding();
                binding.Source = rows;
                listView.SetBinding(ListView.ItemsSourceProperty, binding);

                rows.Clear();

                while (sqlReader.Read())
                {
                    //sb.Clear();
                    List<string> row = new List<string>();
                    for (int i = 0; i < sqlReader.FieldCount; i++)
                    {
                        string? field = sqlReader.GetValue(i).ToString();
                        row.Add(field);
                        //sb.Append(sqlReader.GetValue(i).ToString() + " ");
                    }
                    rows.Add(row);
                    //Trace.WriteLine(sb.ToString());
                    //listBoxSQL.Items.Add(sb.ToString());

                }
                //sqlCon.Close();
            }
        }

        private GridViewColumn CreateColumn(string header, string bindingName)
        {
            GridViewColumn column = new GridViewColumn();
            column.DisplayMemberBinding = new Binding(bindingName);
            column.Header = header;
            column.Width = double.NaN;

            return column;
        }

        private void FillStackHeader(SQLiteConnection connection)
        {
            sideBar.Children.Clear();

            var sql = connection.CreateCommand();
            sql.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY 1";
            sqlReader = sql.ExecuteReader();

            while (sqlReader.Read())
            {
               
                string? field = sqlReader.GetValue(0).ToString();
                var button = new Button();
                button.Name = field;
                button.Content = field;
                button.Click += ViewTableButton;
                DockPanel.SetDock(button, Dock.Top);
                sideBar.Children.Add(button);
            }
        }

        private void ViewTableButton(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button != null && sqlCon != null)
            {
                var sql = sqlCon.CreateCommand();
                sql.CommandText = String.Format("SELECT * FROM {0}", button.Name);

                if (sqlCon.State == System.Data.ConnectionState.Open)
                {
                    //listBoxSQL.Items.Clear();
                    sqlGridView.Columns.Clear();


                    try
                    {
                        sqlReader = sql.ExecuteReader();

                    }
                    catch (SQLiteException ex)
                    {
                        MessageBox.Show("Error in SQLite Query \n" + ex.Message);
                    }

                    for (int i = 0; i < sqlReader.FieldCount; i++)
                    {
                        string header = sqlReader.GetName(i);
                        sqlGridView.Columns.Add(new GridViewColumn
                        {
                            Header = header,
                            Width = double.NaN,
                            DisplayMemberBinding = new Binding(String.Format("[{0}]", i))
                        });
                    }

                    listView.View = sqlGridView;

                    ObservableCollection<List<string>> rows = new ObservableCollection<List<string>>();
                    Binding binding = new Binding();
                    binding.Source = rows;
                    listView.SetBinding(ListView.ItemsSourceProperty, binding);

                    rows.Clear();

                    while (sqlReader.Read())
                    {
                        List<string> row = new List<string>();
                        for (int i = 0; i < sqlReader.FieldCount; i++)
                        {
                            string? field = sqlReader.GetValue(i).ToString();
                            row.Add(field);
                        }
                        rows.Add(row);

                    }
                }
            }


        }
    }
}
