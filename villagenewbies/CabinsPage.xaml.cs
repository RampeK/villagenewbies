using Google.Protobuf.WellKnownTypes;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;


namespace VillageNewbies.Views
{

    public partial class CabinsPage : ContentPage
    {
        public ICommand MakeReservationCommand { get; private set; }

        // Alueiden nimien mappaus
        private readonly Dictionary<int, string> _alueNimet = new Dictionary<int, string>
        {
            { 1, "Yll�s" },
            { 2, "Ruka" },
            { 3, "Pyh�" },
            { 4, "Levi" },
            { 5, "Sy�te" },
            { 6, "Vuokatti" },
            { 7, "Tahko" },
            { 8, "Himos" },
        };

        private readonly Dictionary<int, string> _hintaLuokat = new Dictionary<int, string>
        {
            {1, "Edullinen" },
            {2, "Keskiluokka" },
            {3, "Premium" }
        };
       

       

        public ObservableCollection<Mokki> Mokit { get; private set; }

        public CabinsPage()
        {
            InitializeComponent();
            Mokit = new ObservableCollection<Mokki>();
            AreaPicker.ItemsSource = _alueNimet.Values.ToList();
            HintaPicker.ItemsSource = _hintaLuokat.Values.ToList();
            LoadMokitAsync();
            LisaaMokki.Clicked += LisaaMokki_Clicked;

            MakeReservationCommand = new Command(ShowPopup);
        }
        private async void LisaaMokki_Clicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddCabinPage());
        }

        private async Task LoadMokitAsync()
        {
            var mokkiAccess = new MokkiAccess();
            var mokitList = await mokkiAccess.FetchAllMokitAsync();
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var mokki in mokitList)
                {
                    Mokit.Add(mokki);
                }
                CabinsCollectionView.ItemsSource = Mokit;
            });
        }
        // Popup kun painetaan Tee Varaus nappia
        // TODO Korjaa t�m� paska perkele vitun navigointi
        private async void ShowPopup()
        {
            var reservationPopup = new ReservationPopup();
            await this.ShowPopupAsync(reservationPopup);
        }

        // Kutsutaan, kun alue valitaan Pickerist�
        private void OnAreaSelected(object sender, EventArgs e)
        {
            if (AreaPicker.SelectedIndex == -1)
                return;

            var selectedAreaName = AreaPicker.SelectedItem.ToString();
            int selectedAreaId = _alueNimet.FirstOrDefault(x => x.Value == selectedAreaName).Key;

            var filteredMokit = Mokit.Where(m => m.alue_id == selectedAreaId).ToList();
            CabinsCollectionView.ItemsSource = filteredMokit;
        }


        private void onPriceSelected(object sender, EventArgs e)
        {
            if (HintaPicker.SelectedIndex == -1)
                return;

            var selectedPriceName = HintaPicker.SelectedItem.ToString();
            int selectedPriceId = _hintaLuokat.FirstOrDefault(x => x.Value == selectedPriceName).Key;
            Debug.WriteLine("Hinta id: {0}", selectedPriceId);

            if (selectedPriceId == 1)
            {
                var filteredHinta = Mokit.Where(m => m.hinta >= 70 && m.hinta <= 100).ToList();
                CabinsCollectionView.ItemsSource = filteredHinta;
            }

            else if (selectedPriceId == 2)
            {
                var filteredHinta = Mokit.Where(m => m.hinta >= 105 && m.hinta <= 120).ToList();
                CabinsCollectionView.ItemsSource = filteredHinta;
            }

            else if(selectedPriceId == 3)
            {
                var filteredHinta = Mokit.Where(m => m.hinta >= 130).ToList();
                CabinsCollectionView.ItemsSource = filteredHinta;


            }
        }
    }
}
    


