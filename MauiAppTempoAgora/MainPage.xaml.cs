using MauiAppTempoAgora.Models;
using MauiAppTempoAgora.Services;
using System.Globalization;

namespace MauiAppTempoAgora
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked(
            object sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_cidade.Text))
            {
                await DisplayAlertAsync(
                    "Atenção",
                    "Informe o nome de uma cidade.",
                    "OK");

                return;
            }

            try
            {
                loading.IsVisible = true;
                loading.IsRunning = true;

                lbl_res.Text = "Consultando previsão...";

                Tempo? t =
                    await DataService.GetPrevisao(txt_cidade.Text);

                if (t == null)
                {
                    lbl_res.Text =
                        "Não foi possível obter os dados da previsão.";

                    return;
                }

                string descricao =
                    CultureInfo.CurrentCulture.TextInfo
                        .ToTitleCase(
                            t.description ?? "Não informado");

                string nascerDoSol =
                    t.sunrise?.ToString(
                        "dd/MM/yyyy HH:mm:ss") ?? "Não informado";

                string porDoSol =
                    t.sunset?.ToString(
                        "dd/MM/yyyy HH:mm:ss") ?? "Não informado";

                string dadosPrevisao =
                    $"Latitude: {t.lat:F4}\n" +
                    $"Longitude: {t.lon:F4}\n" +
                    $"Descrição: {descricao}\n" +
                    $"Velocidade do vento: {t.speed:F2} m/s\n" +
                    $"Visibilidade: {FormatarVisibilidade(t.visibility)}\n" +
                    $"Nascer do Sol: {nascerDoSol}\n" +
                    $"Pôr do Sol: {porDoSol}\n" +
                    $"Temperatura Máxima: {t.temp_max:F2} °C\n" +
                    $"Temperatura Mínima: {t.temp_min:F2} °C";

                lbl_res.Text = dadosPrevisao;
            }
            catch (InvalidOperationException ex)
            {
                await DisplayAlertAsync(
                    "Aviso",
                    ex.Message,
                    "OK");

                lbl_res.Text = string.Empty;
            }
            catch (Exception)
            {
                await DisplayAlertAsync(
                    "Erro",
                    "Ocorreu um erro inesperado ao consultar " +
                    "a previsão do tempo.",
                    "OK");

                lbl_res.Text = string.Empty;
            }
            finally
            {
                loading.IsRunning = false;
                loading.IsVisible = false;
            }
        }

        private static string FormatarVisibilidade(int? visibilidade)
        {
            if (!visibilidade.HasValue)
                return "Não informado";

            return $"{visibilidade.Value / 1000.0:F2} km";
        }
    }
}