using MauiAppTempoAgora.Models;
using Newtonsoft.Json.Linq;
using System.Net;

namespace MauiAppTempoAgora.Services
{
    public class DataService
    {
        public static async Task<Tempo?> GetPrevisao(string cidade)
        {
            if (string.IsNullOrWhiteSpace(cidade))
                return null;

            string chave = "8b7a647635618b915f016817fa0c8d67";

            string cidadeCodificada =
                Uri.EscapeDataString(cidade.Trim());

            string url =
                $"https://api.openweathermap.org/data/2.5/weather?" +
                $"q={cidadeCodificada}&units=metric&lang=pt_br&appid={chave}";

            using HttpClient client = new HttpClient();

            HttpResponseMessage resp;

            try
            {
                resp = await client.GetAsync(url);
            }
            catch (HttpRequestException)
            {
                throw new InvalidOperationException(
                    "Não foi possível acessar a internet. " +
                    "Verifique sua conexão e tente novamente.");
            }
            catch (TaskCanceledException)
            {
                throw new InvalidOperationException(
                    "A consulta demorou muito para responder. " +
                    "Verifique sua conexão e tente novamente.");
            }

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException(
                    "Cidade não encontrada. " +
                    "Verifique o nome informado e tente novamente.");
            }

            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new InvalidOperationException(
                    "A chave da API é inválida ou não está autorizada.");
            }

            if (!resp.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Erro ao consultar o serviço de clima. " +
                    $"Código: {(int)resp.StatusCode}.");
            }

            string json = await resp.Content.ReadAsStringAsync();

            JObject rascunho = JObject.Parse(json);

            long nascerUnix =
                (long)rascunho["sys"]!["sunrise"]!;

            long porDoSolUnix =
                (long)rascunho["sys"]!["sunset"]!;

            DateTime nascerDoSol =
                DateTimeOffset
                    .FromUnixTimeSeconds(nascerUnix)
                    .ToLocalTime()
                    .DateTime;

            DateTime porDoSol =
                DateTimeOffset
                    .FromUnixTimeSeconds(porDoSolUnix)
                    .ToLocalTime()
                    .DateTime;

            Tempo tempo = new Tempo
            {
                lat = (double?)rascunho["coord"]?["lat"],
                lon = (double?)rascunho["coord"]?["lon"],
                description = (string?)rascunho["weather"]?[0]?["description"],
                main = (string?)rascunho["weather"]?[0]?["main"],
                temp_min =(double?)rascunho["main"]?["temp_min"],
                temp_max =(double?)rascunho["main"]?["temp_max"],
                speed =(double?)rascunho["wind"]?["speed"],
                visibility =(int?)rascunho["visibility"],
                sunrise = nascerDoSol,
                sunset = porDoSol
            };

            return tempo;
        }
    }
}