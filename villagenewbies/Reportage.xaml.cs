using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Data;

namespace VillageNewbies;

public partial class Reportage : ContentPage
{

    private DatabaseAccess databaseAccess;
    public ObservableCollection<VarausViewModel> Varaukset { get; } = new ObservableCollection<VarausViewModel>();
    private Dictionary<int, string> alueidenDictionary;
    public Reportage()
    {
        InitializeComponent();
        databaseAccess = new DatabaseAccess();
        BookinReportage.ItemsSource = Varaukset;
        alueidenDictionary = new Dictionary<int, string>(); 
        InitializePicker();

    }

    private async void InitializePicker()
    {
        // Aseta datepickerit n�ytt�m��n t�m�n p�iv�n p�iv�m��r�
        StartDatePicker.Date = DateTime.Today;
        EndDatePicker.Date = DateTime.Today.AddDays(1); // Oletus loppup�iv�m��r� on huomenna

        await LataaAlueet();
    }

    public async Task LataaAlueet()
    {
        var alueidenDictionary = await databaseAccess.HaeAlueet();
        AreaPicker.ItemsSource = alueidenDictionary.Values.ToList();
        //if (alueidenDictionary.Count > 0)
        //   AreaPicker.SelectedIndex = 0;
    }

    private async void Varaustenhaku_Clicked(object sender, EventArgs e)
    {
        
        if (AreaPicker.SelectedIndex != -1)
        {
            var selectedAreaName = (string)AreaPicker.SelectedItem;
            var selectedAreaId = alueidenDictionary.FirstOrDefault(x => x.Value == selectedAreaName).Key;
            var alkuPvm = StartDatePicker.Date;
            var loppuPvm = EndDatePicker.Date;
            var varauksetLista = await databaseAccess.HaeVaraukset(selectedAreaId, alkuPvm, loppuPvm);
            Varaukset.Clear();
            foreach (var varausViewModel in varauksetLista)
            {
                Varaukset.Add(varausViewModel);
            }
        }
       
        else
        {
            await DisplayAlert("Virhe", "Valitse ensin alue.", "OK");
        }
    }


    public class DatabaseAccess
    {
        private readonly string connectionString;

        public DatabaseAccess()
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));
            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
        }

        public async Task<Dictionary<int, string>> HaeAlueet()
        {
            Dictionary<int, string> alueet = new Dictionary<int, string>();
            using (var connection = new MySqlConnection(connectionString))

            {
                await connection.OpenAsync();
                string query = "SELECT alue_id, nimi FROM alue";
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            alueet.Add(reader.GetInt32("alue_id"), reader.GetString("nimi"));

                        }
                    }
                }
            }
            return alueet;
        }


        public async Task<List<VarausViewModel>> HaeVaraukset(int alueId, DateTime startDate, DateTime endDate)
        {
            List<VarausViewModel> varaukset = new List<VarausViewModel>();
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                SELECT v.varaus_id, m.mokkinimi, v.varattu_alkupvm, v.varattu_loppupvm
                FROM varaus v
                JOIN mokki m ON v.mokki_mokki_id = m.mokki_id
                WHERE m.alue_id = @AlueId
                AND v.varattu_loppupvm >= @StartDate
                AND v.varattu_alkupvm <= @EndDate;";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AlueId", alueId);
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var varausViewModel = new VarausViewModel
                            {
                                Varaus = new Varaus
                                {
                                    varaus_id = reader.GetInt32("varaus_id"),
                                    varattu_alkupvm = reader.GetDateTime("varattu_alkupvm"),
                                    varattu_loppupvm = reader.GetDateTime("varattu_loppupvm")
                                },
                                Mokkinimi = reader.GetString("mokkinimi")
                            };
                            varaukset.Add(varausViewModel);
                        }
                    }
                }
            }
            return varaukset;
        }


        
    }   
        private void Palveluhaku_Clicked(object sender, EventArgs e)
        {
             DisplayAlert("Virhe", "Palveluiden haku ei ole viel� toteutettu.", "OK");
        } 
    
}

