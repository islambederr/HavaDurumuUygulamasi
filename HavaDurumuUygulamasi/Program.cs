using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace HavaDurumuUygulamasi
{
    class Program
    {
        // OpenWeatherMap API Anahtarını buraya yapıştır
        private const string apiKey = "a795904d633e81365853939a4b2d3f11";
        private const string apiUrl = "http://api.openweathermap.org/data/2.5/weather";

        static List<string> sorguGecmisi = new List<string>(); // Sorgu geçmişi listesi

        static async Task Main(string[] args)
        {
            Console.WriteLine("🌤 Hava Durumu Uygulamasına Hoş Geldiniz! 🌦");

            while (true)
            {
                Console.Write("\nLütfen bir şehir adı girin (çıkış için 'çıkış' yazın): ");
                string sehir = Console.ReadLine();

                if (sehir.Equals("çıkış", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("\nProgramdan çıkılıyor...\n");
                    GecmisiGoster();
                    break;
                }

                await HavaDurumuGetir(sehir);
            }
        }

        static async Task HavaDurumuGetir(string sehir)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // API isteği için URL oluşturma
                    string requestUrl = $"{apiUrl}?q={sehir}&appid={apiKey}&units=metric&lang=tr";

                    // HTTP GET isteği gönderme
                    HttpResponseMessage response = await client.GetAsync(requestUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        // JSON verisini işleme
                        JObject veri = JObject.Parse(responseBody);
                        string sehirAdi = veri["name"].ToString();
                        double sicaklik = Convert.ToDouble(veri["main"]["temp"]);
                        double hissedilenSicaklik = Convert.ToDouble(veri["main"]["feels_like"]);
                        int nem = Convert.ToInt32(veri["main"]["humidity"]);
                        double ruzgarHizi = Convert.ToDouble(veri["wind"]["speed"]);
                        string durum = veri["weather"][0]["description"].ToString();
                        long gunDogumu = Convert.ToInt64(veri["sys"]["sunrise"]);
                        long gunBatimi = Convert.ToInt64(veri["sys"]["sunset"]);

                        // Unix zamanını DateTime'a çevirme
                        DateTime gunDogumuZamani = DateTimeOffset.FromUnixTimeSeconds(gunDogumu).LocalDateTime;
                        DateTime gunBatimiZamani = DateTimeOffset.FromUnixTimeSeconds(gunBatimi).LocalDateTime;

                        // Durum sembolü alma
                        string sembol = HavaDurumuSembol(durum);

                        // Sonucu ekrana yazdırma
                        Console.WriteLine($"\n{sehirAdi} için Hava Durumu:");
                        Console.WriteLine($"Sıcaklık: {sicaklik}°C");
                        Console.WriteLine($"Hissedilen Sıcaklık: {hissedilenSicaklik}°C");
                        Console.WriteLine($"Nem Oranı: %{nem}");
                        Console.WriteLine($"Rüzgar Hızı: {ruzgarHizi} m/s");
                        Console.WriteLine($"Gün Doğumu: {gunDogumuZamani:HH:mm}");
                        Console.WriteLine($"Gün Batımı: {gunBatimiZamani:HH:mm}");
                        Console.WriteLine($"Durum: {durum} {sembol}");
                        Console.WriteLine($"Sorgulama Zamanı: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");

                        // Sorgu geçmişine ekleme
                        sorguGecmisi.Add($"{DateTime.Now:dd.MM.yyyy HH:mm} - {sehirAdi}: {sicaklik}°C, {durum}");
                    }
                    else
                    {
                        Console.WriteLine("❗ Şehir bulunamadı. Lütfen doğru bir şehir adı girin.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Bir hata oluştu: {ex.Message}");
                }
            }
        }

        static string HavaDurumuSembol(string durum)
        {
            // Hava durumu açıklamasına göre sembol döndüren metod
            if (durum.IndexOf("güneş", StringComparison.OrdinalIgnoreCase) >= 0) return "☀️";
            if (durum.IndexOf("bulut", StringComparison.OrdinalIgnoreCase) >= 0) return "☁️";
            if (durum.IndexOf("yağmur", StringComparison.OrdinalIgnoreCase) >= 0) return "🌧";
            if (durum.IndexOf("kar", StringComparison.OrdinalIgnoreCase) >= 0) return "❄️";
            if (durum.IndexOf("fırtına", StringComparison.OrdinalIgnoreCase) >= 0) return "⛈";
            return "🌡"; // Varsayılan sembol

        }

        static void GecmisiGoster()
        {
            Console.WriteLine("\n📜 Sorgu Geçmişi:");
            if (sorguGecmisi.Count == 0)
            {
                Console.WriteLine("Henüz sorgu yapılmamış.");
            }
            else
            {
                foreach (var sorgu in sorguGecmisi)
                {
                    Console.WriteLine(sorgu);
                }
            }
        }
    }
}
