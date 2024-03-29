using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace VillageNewbies;

public partial class AddCustomerPage : ContentPage
{
    private Asiakas _asiakas; // J�senmuuttuja lis�t��n t�ss�, t�m� lis�tty koodi

    public AddCustomerPage()
    {
        InitializeComponent();
    }
        //lis�tty koodi

    public AddCustomerPage(Asiakas asiakas) : this()
    {
            _asiakas = asiakas;
        // T�yt� kent�t asiakkaan tiedoilla

        if (_asiakas != null)
        {
            etunimi.Text = _asiakas.etunimi;
            sukunimi.Text = _asiakas.sukunimi;
            l�hiosoite.Text = _asiakas.lahiosoite;
            postinro.Text = _asiakas.postinro.ToString(); // Olettaen ett� postinro on tietotyyppi�, joka vaatii muunnoksen stringiksi
            puhelinnro.Text = _asiakas.puhelinnro;
            s�hk�posti.Text = _asiakas.email;
            // Huomioi, ett� 'toimipaikka' puuttuu Asiakas-luokasta t�ss� esimerkiss�
        }

    }

    // t�h�n lis�tty 3


    // t�h�n lis�tty 3 loppuu

        

    // asiakkaan vienti tietokantaan
    private async void LisaaAsiakas_Clicked(object sender, EventArgs e)
    {
        var puhelinnumero = puhelinnro.Text;
        //var puhelinnumeroRegex = new Regex(@"^\+?\d+$");
        var puhelinnumeroRegex = new Regex(@"^(\+358\d{9}|0\d{9})$");
        var puhelinnumeroOK = puhelinnumeroRegex.IsMatch(puhelinnumero);

        if (!puhelinnumeroOK)
        {
            // N�yt� virheilmoitus
            await DisplayAlert("Virheellinen puhelinnumero", "Sy�t� puhelinnumero muodossa +358451234567 tai 0451234567.", "OK");
            return; // Lopeta metodin suoritus t�h�n
        }

        // jos kent�t tyhj�t ja yritet��n tallentaa
        if (string.IsNullOrWhiteSpace(etunimi.Text) ||
        string.IsNullOrWhiteSpace(sukunimi.Text) ||
        string.IsNullOrWhiteSpace(l�hiosoite.Text) ||
        string.IsNullOrWhiteSpace(postinro.Text) ||
        string.IsNullOrWhiteSpace(s�hk�posti.Text) ||
        string.IsNullOrWhiteSpace(puhelinnro.Text))
        {
            // N�yt� varoitusikkuna
            await DisplayAlert("T�ytt�m�tt�m�t tiedot", "T�yt� kaikki asiakastiedot ennen l�hett�mist�.", "OK");
            return; // Lopeta metodin suoritus t�h�n
        }
             

        if (int.TryParse(postinro.Text, out _))
        {
            var uusiAsiakas = new Asiakas
            {
                etunimi = etunimi.Text,
                sukunimi = sukunimi.Text,
                lahiosoite = l�hiosoite.Text,
                postinro = postinro.Text,
                email = s�hk�posti.Text,
                puhelinnro = puhelinnro.Text
            };

            var databaseAccess = new DatabaseAccess();
            await databaseAccess.LisaaAsiakasTietokantaan(uusiAsiakas);
        }



        // lis�� t�h�n: palaa edelliselle sivulle tai anna k�ytt�j�lle palaute onnistuneesta lis�yksest�
        
        etunimi.Text = "";
        sukunimi.Text = "";
        l�hiosoite.Text = "";
        postinro.Text = "";
        toimipaikka.Text = "";
        s�hk�posti.Text = "";
        puhelinnro.Text = "";
    }

    private async void PoistaAsiakasTietokannasta_Clicked(object sender, EventArgs e)
    {
        var vastaus = await DisplayAlert("Vahvista poisto", "Haluatko varmasti poistaa t�m�n asiakkaan?", "Kyll�", "Ei");
        if (vastaus)
        {
            var databaseAccess = new DatabaseAccess();
            await databaseAccess.PoistaAsiakasTietokannasta(_asiakas.asiakas_id);
            await DisplayAlert("Poistettu", "Asiakas on poistettu onnistuneesti.", "OK");
            // Palaa tarvittaessa edelliselle sivulle
            await Navigation.PopAsync();
        }
    }


    public class DatabaseAccess
    {
        public async Task LisaaAsiakasTietokantaan(Asiakas uusiAsiakas)
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

                    var query = "INSERT INTO asiakas (postinro, etunimi, sukunimi, lahiosoite, email, puhelinnro) VALUES (@Postinro, @Etunimi, @Sukunimi, @Lahiosoite, @Email, @Puhelinnro)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        Debug.WriteLine(uusiAsiakas.postinro);
                        command.Parameters.AddWithValue("@Postinro", uusiAsiakas.postinro);
                        command.Parameters.AddWithValue("@Etunimi", uusiAsiakas.etunimi);
                        command.Parameters.AddWithValue("@Sukunimi", uusiAsiakas.sukunimi);
                        command.Parameters.AddWithValue("@Lahiosoite", uusiAsiakas.lahiosoite);
                        command.Parameters.AddWithValue("@Email", uusiAsiakas.email);
                        command.Parameters.AddWithValue("@Puhelinnro", uusiAsiakas.puhelinnro);

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



        //t�h�n lis�ys asiakkaan poistoon
        public async Task PoistaAsiakasTietokannasta(int asiakasId)
        {
            string connectionString = "server=localhost;database=vn;user=;password=;";
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var query = "DELETE FROM asiakas WHERE asiakas_id = @AsiakasId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AsiakasId", asiakasId);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    // K�sittely mahdollisille poikkeuksille
                    Console.WriteLine(ex.Message);
                }
            }
        }


        // ja t�h�n loppuu asiakkaan poist


    }

    
}


