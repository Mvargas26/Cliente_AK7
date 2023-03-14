using Cliente_AK7.Models;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading;

class Program
{
    static HttpClient client = new HttpClient();
    static void Main()
    {

        MonitorRecursosSerividor();


        Console.ReadKey();
    }

    static void MonitorRecursosSerividor()
    {
        Process process = Process.GetCurrentProcess();

        Thread h1 = new Thread(() =>
        {
            while (true)
            {
                float monitorCPU = process.TotalProcessorTime.Ticks / (float)Stopwatch.Frequency / Environment.ProcessorCount;
                Console.WriteLine("CPU en uso: {0:F2}%", monitorCPU * 100);

                long monitorRAM = process.WorkingSet64;
                Console.WriteLine("RAM en uso: {0:N0} MB", monitorRAM);

                DriveInfo[] discos = DriveInfo.GetDrives();
                foreach (DriveInfo disco in discos)
                {
                    if (disco.IsReady)
                    {
                        long esapcioTotal = disco.TotalSize;
                        long espacioLibre = disco.TotalFreeSpace;
                        long espacioUsado = esapcioTotal - espacioLibre;
                        Console.WriteLine("{0} usado: {1:N0} bytes ({2:F2}%)", disco.Name, espacioUsado, (espacioUsado / (float)esapcioTotal) * 100);
                    }
                }
                RunAsync().GetAwaiter().GetResult();

                Thread.Sleep(10000);
            }
        });

        // Start the system resource monitoring thread
        h1.Start();
    }

    static async Task<UmbralCompServer> crearRegistro(UmbralCompServer c)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("paramSensibilidad", c);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<UmbralCompServer>();
    }
    static async Task RunAsync()
    {
        client.BaseAddress = new Uri("http://localhost:5021/");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        UmbralCompServer c = new UmbralCompServer()
        {
            CodServer = "S_W_1",
            CodComp = "CO2",
            CodUmbral = "normal",
            Porcentaje = usoCPUDevuelve()
        };
        c = await crearRegistro(c);
        Console.WriteLine($"Registrado");
        Console.ReadLine();
    }//fn
}