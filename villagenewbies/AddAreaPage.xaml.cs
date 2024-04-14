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
    private Alue valittuAlue;

    public AddAreaPage(Alue alue) : this()
    {
        valittuAlue = alue;
        nimi.Text = alue.nimi;
        
    }


    private void AreasCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        valittuAlue = e.CurrentSelection.FirstOrDefault() as Alue;

        valittuAlue = (Alue)e.CurrentSelection.FirstOrDefault();
        if (valittuAlue != null)
        {
            // P�ivit� tekstikentt� valitun alueen nimell�
            nimi.Text = valittuAlue.nimi;
        }
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

    private async void TallennaAlue_Clicked(object sender, EventArgs e)
    {

        var muokattavaAlue = new Alue();

        if (!string.IsNullOrWhiteSpace(alue_id.Text) && int.TryParse(alue_id.Text, out int parsedAlueId))
        {
            muokattavaAlue.alue_id = parsedAlueId;
        }

        // P�ivit� aluenimi, jos se on annettu
        if (!string.IsNullOrWhiteSpace(nimi.Text))
        {
            muokattavaAlue.nimi = nimi.Text;
        }



        // P�ivitet��n _mokki-olion tiedot
        if (muokattavaAlue == null)
        {
            muokattavaAlue = new Alue();
        }

        // Kutsutaan DatabaseAccess-luokan p�ivitysmetodia
        var success = await databaseAccess.PaivitaAlueTietokantaan(muokattavaAlue);
        if (success)
        {
            await DisplayAlert("Onnistui", "Alueen tiedot tallennettu onnistuneesti.", "OK");
            // T�ss� kohtaa voit p�ivitt�� k�ytt�liittym�n tai navigoida takaisin p��sivulle
        }
        else
        {
            await DisplayAlert("Virhe", "Alueen tietojen tallentaminen ep�onnistui.", "OK");
        }

        // P�ivit� alueet n�yt�ll�
        await LoadAlueetAsync();

        // Tyhjenn� valittu alue
        valittuAlue = null;
        nimi.Text = "";
        alue_id.Text = "";


        ////Varmista, ett� k�ytt�j� on antanut nimen
        //if (string.IsNullOrWhiteSpace(nimi.Text))
        //{
        //    await DisplayAlert("Virhe", "Alueen nimi on tyhj�.", "OK");
        //    return;
        //}

        //// Jos alue on jo olemassa, p�ivit� sen tiedot
        //   if (valittuAlue != null)
        //   {
        //    valittuAlue.nimi = nimi.Text;
        //   }
        //    // Kutsu tietokantaa p�ivitt�m��n alue
        //    var success = await databaseAccess.PaivitaAlueTietokantaan(valittuAlue);
        //    if (success)
        //    {
        //        await DisplayAlert("Onnistui", "Alueen tiedot p�ivitetty.", "OK");
        //    }
        //    else
        //    {
        //        await DisplayAlert("Virhe", "Alueen tietojen p�ivitt�minen ep�onnistui.", "OK");
        //    }
    }


    private async void MuokkaaAlue_Clicked(object? sender, EventArgs e)
    {

        if (!(sender is Button button)) return;
        var alue = button.CommandParameter as Alue;
        if (alue != null)
        {
            alue_id.Text = alue.alue_id.ToString();
            nimi.Text = alue.nimi;

        }
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

                var query = "INSERT INTO alue (alue_id,nimi)  VALUES (@AlueId, @Nimi)";

                using (var command = new MySqlCommand(query, connection))
                {
                    Debug.WriteLine(uusiAlue.nimi);

                    command.Parameters.AddWithValue("@AlueId", uusiAlue.alue_id);
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
    public async Task<bool> PaivitaAlueTietokantaan(Alue paivitettyAlue)
    {
        string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
        var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

        DotNetEnv.Env.Load(projectRoot);
        var env = Environment.GetEnvironmentVariables();

        string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
        using (var connection = new MySqlConnection(connectionString))

        {
            await connection.OpenAsync();

            var query = @"UPDATE alue SET nimi = @Nimi WHERE alue_id = @AlueId";


            using (var command = new MySqlCommand(query, connection))
            {
                // Parametrien asettaminen
                command.Parameters.AddWithValue("@AlueId", paivitettyAlue.alue_id);
                command.Parameters.AddWithValue("@Nimi", paivitettyAlue.nimi);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0; // Onnistuiko p�ivitys
            }
        }
    }
}








//string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
//    var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

//    DotNetEnv.Env.Load(projectRoot);
//    var env = Environment.GetEnvironmentVariables();

//    string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
//    using (var connection = new MySqlConnection(connectionString))
//    {


//            await connection.OpenAsync();

//            var query = "UPDATE alue" +
//                "SET nimi = @Nimi" +
//                " WHERE alue_id = @AlueId";

//        using (var command = new MySqlCommand(query, connection))
//        {
//            command.Parameters.AddWithValue("@Nimi", paivitettyAlue.nimi);
//            command.Parameters.AddWithValue("@AlueId", paivitettyAlue.alue_id);

//            //int affectedRows = await command.ExecuteNonQueryAsync();
//            // return affectedRows > 0;

//            var result = await command.ExecuteNonQueryAsync();
//            return result > 0; // Onnistuiko p�ivitys

//            //   catch (Exception ex)
//            // {
//            //    // K�sittely mahdollisille poikkeuksille
//            //    Debug.WriteLine(ex.Message);
//            //    return false;
//            //}
//        }
//    }


