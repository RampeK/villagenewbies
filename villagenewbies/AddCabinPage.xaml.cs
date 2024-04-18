using Microsoft.Maui.Controls;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace VillageNewbies;

public partial class AddCabinPage : ContentPage
{
    private DatabaseAccess databaseAccess = new DatabaseAccess();
    public ObservableCollection<Mokki> Mokit { get; private set; }
    private Mokki _mokki;

    public AddCabinPage()
    {
        InitializeComponent();
        Mokit = new ObservableCollection<Mokki>();
    }

    public AddCabinPage(Mokki mokki) : this()
    {
        _mokki = mokki;
        // T�yt� kent�t m�kin tiedoilla

        if (_mokki != null)
        {
            alue_id.Text = _mokki.alue_id.ToString();
            mokkinimi.Text = _mokki.mokkinimi;
            katuosoite.Text = _mokki.katuosoite;
            postinro.Text = _mokki.postinro.ToString();
            hinta.Text = _mokki.hinta.ToString();
            kuvaus.Text = _mokki.kuvaus;
            henkilomaara.Text = _mokki.henkilomaara.ToString();
            varustelu.Text = _mokki.varustelu.ToString();
        }
    }

    private async void LisaaMokki_Clicked(object sender, EventArgs e)
    {
        // jos kent�t tyhj�t ja yritet��n tallentaa
        if 
            (
            string.IsNullOrWhiteSpace(alue_id.Text) ||
            !int.TryParse(alue_id.Text, out int parsedAlue_id) ||
            string.IsNullOrWhiteSpace(mokkinimi.Text) ||
            string.IsNullOrWhiteSpace(katuosoite.Text) ||
            string.IsNullOrWhiteSpace(postinro.Text) ||
            !int.TryParse(postinro.Text, out int parsedPostinro) ||
            string.IsNullOrWhiteSpace(hinta.Text) ||
            !double.TryParse(hinta.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedHinta) ||
            string.IsNullOrWhiteSpace(henkilomaara.Text) ||
            !int.TryParse(henkilomaara.Text, out int parsedHenkilomaara) ||
            string.IsNullOrWhiteSpace(kuvaus.Text) ||
            string.IsNullOrWhiteSpace(varustelu.Text))

        {// N�yt� virheviesti tai k�sittele virhetilanne t�ss�
            await DisplayAlert("T�ytt�m�tt�m�t tiedot", "T�yt� kaikki tiedot ennen l�hett�mist�.", "Ok");
            return; // Lopeta metodin suoritus t�h�n
        }

        var uusiMokki = new Mokki
        {

            alue_id = int.Parse(alue_id.Text),
            mokkinimi = mokkinimi.Text,
            katuosoite = katuosoite.Text,
            postinro = int.Parse(postinro.Text),
            hinta = double.Parse(hinta.Text),
            kuvaus = kuvaus.Text,
            henkilomaara = int.Parse(henkilomaara.Text),
            varustelu = varustelu.Text
        };

        var databaseAccess = new DatabaseAccess();

        alue_id.Text = "";
        mokkinimi.Text = "";
        katuosoite.Text = "";
        postinro.Text = "";
        hinta.Text = "";
        kuvaus.Text = "";
        henkilomaara.Text = "";
        varustelu.Text = "";

        await databaseAccess.LisaaMokkiTietokantaan(uusiMokki);
        await Navigation.PopAsync();
    }

    private async void TallennaMokki_Clicked(object sender, EventArgs e)
    {
        var muokattavaMokki = new Mokki();

        // Tarkista alue_id ja p�ivit�, jos se on annettu ja se on kokonaisluku
        if (!string.IsNullOrWhiteSpace(alue_id.Text) && int.TryParse(alue_id.Text, out int parsedAlueId))
        {
            muokattavaMokki.alue_id = parsedAlueId;
        }

        // P�ivit� mokkinimi, jos se on annettu
        if (!string.IsNullOrWhiteSpace(mokkinimi.Text))
        {
            muokattavaMokki.mokkinimi = mokkinimi.Text;
        }

        // P�ivit� katuosoite, jos se on annettu
        if (!string.IsNullOrWhiteSpace(katuosoite.Text))
        {
            muokattavaMokki.katuosoite = katuosoite.Text;
        }

        // Varmista, ett� postinro on kokonaisluku ja p�ivit�
        if (!string.IsNullOrWhiteSpace(postinro.Text) && int.TryParse(postinro.Text, out int parsedPostinro))
        {
            muokattavaMokki.postinro = parsedPostinro;
        }

        // Varmista, ett� hinta on desimaaliluku ja p�ivit�
        if (!string.IsNullOrWhiteSpace(hinta.Text) && double.TryParse(hinta.Text, out double parsedHinta))
        {
            muokattavaMokki.hinta = parsedHinta;
        }

        // Varmista, ett� henkilomaara on kokonaisluku ja p�ivit�
        if (!string.IsNullOrWhiteSpace(henkilomaara.Text) && int.TryParse(henkilomaara.Text, out int parsedHenkilomaara))
        {
            muokattavaMokki.henkilomaara = parsedHenkilomaara;
        }

        // P�ivit� kuvaus, jos se on annettu
        if (!string.IsNullOrWhiteSpace(kuvaus.Text))
        {
            muokattavaMokki.kuvaus = kuvaus.Text;
        }

        // P�ivit� varustelu, jos se on annettu
        if (!string.IsNullOrWhiteSpace(varustelu.Text))
        {
            muokattavaMokki.varustelu = varustelu.Text;
        }

        // P�ivitet��n _mokki-olion tiedot
        if (muokattavaMokki == null)
        {
            muokattavaMokki = new Mokki();
        }

        // Kutsutaan DatabaseAccess-luokan p�ivitysmetodia
        var success = await databaseAccess.PaivitaMokinTiedot(muokattavaMokki);
        if (success)
        {
            await DisplayAlert("Onnistui", "M�kin tiedot tallennettu onnistuneesti.", "OK");
            // T�ss� kohtaa voit p�ivitt�� k�ytt�liittym�n tai navigoida takaisin p��sivulle
        }
        else
        {
            await DisplayAlert("Virhe", "M�kin tietojen tallentaminen ep�onnistui.", "OK");
        }
        await Navigation.PopAsync();
    }

    private async void PoistaMokki_Clicked(object sender, EventArgs e)
    {
        var vastaus = await DisplayAlert("Vahvista poisto", "Haluatko varmasti poistaa t�m�n M�kin?", "Kyll�", "Ei");
        if (vastaus)
        {
            var databaseAccess = new DatabaseAccess();
            await databaseAccess.PoistaMokkiTietokannasta(_mokki.mokki_id);
            await DisplayAlert("Poistettu", "M�kki on poistettu onnistuneesti.", "OK");
            await Navigation.PopAsync();
        }
    }

    public partial class DatabaseAccess
    {
        public async Task LisaaMokkiTietokantaan(Mokki uusiMokki)
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

                    var query = "INSERT INTO mokki (mokki_id, alue_id, mokkinimi, katuosoite, postinro, hinta, kuvaus,henkilomaara, varustelu)  VALUES (@Mokki_id, @Alue_id, @Mokkinimi, @Katuosoite, @Postinro, @Hinta, @Kuvaus, @Henkilomaara, @Varustelu)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        Debug.WriteLine(uusiMokki.postinro);
                        command.Parameters.AddWithValue("@Mokki_id", uusiMokki.mokki_id);
                        command.Parameters.AddWithValue("@Alue_id", uusiMokki.alue_id);
                        command.Parameters.AddWithValue("@Mokkinimi", uusiMokki.mokkinimi);
                        command.Parameters.AddWithValue("@Katuosoite", uusiMokki.katuosoite);
                        command.Parameters.AddWithValue("@Postinro", uusiMokki.postinro);
                        command.Parameters.AddWithValue("@Hinta", uusiMokki.hinta);
                        command.Parameters.AddWithValue("@Henkilomaara", uusiMokki.henkilomaara);
                        command.Parameters.AddWithValue("@Kuvaus", uusiMokki.kuvaus);
                        command.Parameters.AddWithValue("@Varustelu", uusiMokki.varustelu);
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

        public async Task<bool> PoistaMokkiTietokannasta(int mokki_id)
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

                    var query = "DELETE FROM mokki WHERE mokki_id = @Mokki_id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Mokki_id", mokki_id);
                        var result = await command.ExecuteNonQueryAsync();
                        return result > 0; // palauttaa true jos yksi tai usempi rivi poistettiin
                    }
                }
                catch (Exception ex)
                {
                    // K�sittely mahdollisille poikkeuksille
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }

        public async Task<bool> PaivitaMokinTiedot(Mokki muokattuMokki)
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
            using (var connection = new MySqlConnection(connectionString))

            {
                await connection.OpenAsync();

                var query =
                    @"UPDATE mokki 
                SET alue_id = @AlueId,
                    mokkinimi = @Mokkinimi,
                    katuosoite = @Katuosoite,
                    postinro = @Postinro,
                    hinta = @Hinta,
                    kuvaus = @Kuvaus,
                    henkilomaara = @Henkilomaara,
                   varustelu = @Varustelu
                WHERE mokki_id = @MokkiId";

                using (var command = new MySqlCommand(query, connection))
                {
                    // Parametrien asettaminen
                    command.Parameters.AddWithValue("@MokkiId", muokattuMokki.mokki_id);
                    command.Parameters.AddWithValue("@AlueId", muokattuMokki.alue_id);
                    command.Parameters.AddWithValue("@Mokkinimi", muokattuMokki.mokkinimi);
                    command.Parameters.AddWithValue("@Katuosoite", muokattuMokki.katuosoite);
                    command.Parameters.AddWithValue("@Postinro", muokattuMokki.postinro);
                    command.Parameters.AddWithValue("@Hinta", muokattuMokki.hinta);
                    command.Parameters.AddWithValue("@Kuvaus", muokattuMokki.kuvaus);
                    command.Parameters.AddWithValue("@Henkilomaara", muokattuMokki.henkilomaara);
                    command.Parameters.AddWithValue("@Varustelu", muokattuMokki.varustelu);


                    var result = await command.ExecuteNonQueryAsync();
                    return result > 0; // Onnistuiko p�ivitys
                }
            }
        }
    }
}