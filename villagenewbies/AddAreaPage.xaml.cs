using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Diagnostics;
namespace VillageNewbies;

public partial class AddAreaPage : ContentPage

{
    private DatabaseAccess databaseAccess = new DatabaseAccess();
    public ObservableCollection<Alue> Alueet { get; private set; }
    public AddAreaPage()
	{
		InitializeComponent();
        Alueet = new ObservableCollection<Alue>();
        AreasCollectionView.ItemsSource = Alueet;
        LoadAlueetAsync();
    }

    private async Task LoadAlueetAsync()
    {
        var alueetAccess = new MokkiAccess();
        var alueetList = await alueetAccess.FetchAllAlueAsync();
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            Alueet.Clear();
            foreach (var alue in alueetList)
            {
                Alueet.Add(alue);
            }
            AreasCollectionView.ItemsSource = Alueet;
        });
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadAlueetAsync(); // P�ivit� m�kkilista t��ll�
    }


    private async void LisaaAlue_Clicked(object sender, EventArgs e)
    {
        // jos kent�t tyhj�t ja yritet��n tallentaa
        if (string.IsNullOrWhiteSpace(nimi.Text))
            
        {// N�yt� virheviesti tai k�sittele virhetilanne t�ss�
            await DisplayAlert("T�ytt�m�tt�m�t tiedot", "T�yt� kaikki tiedot ennen l�hett�mist�.", "Ok");
            return; 
        }

        var uusiAlue = new Alue
        {
            nimi = nimi.Text
        };

        var databaseAccess = new DatabaseAccess();
        await databaseAccess.LisaaAlueTietokantaan(uusiAlue);

        nimi.Text = "";
        //await Navigation.PopAsync();
        await LoadAlueetAsync(); // p�ivit� alueiden lista
    }
}

public partial class DatabaseAccess
{
    public async Task LisaaAlueTietokantaan(Alue uusiAlue)
    {
        string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
        var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

        DotNetEnv.Env.Load(projectRoot);
        var env = Environment.GetEnvironmentVariables();

        string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
        using (var connection = new MySqlConnection(connectionString))
        {
            try
            {
                await connection.OpenAsync();

                var query = "INSERT INTO alue (nimi)  VALUES (@Nimi)";

                using (var command = new MySqlCommand(query, connection))
                {
                    Debug.WriteLine(uusiAlue.nimi);
                   
                    command.Parameters.AddWithValue("@Nimi", uusiAlue.nimi);
                    
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                // K�sittely mahdollisille poikkeuksille
                Debug.WriteLine(ex.Message);
            }
        }
    }
}

    

