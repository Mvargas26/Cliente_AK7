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

    static async void MonitorRecursosSerividor()
    {
        Process process = Process.GetCurrentProcess();

        Thread h1 = new Thread(async () =>
        {
            while (true)
            {
                int espacioC = 0;
                float monitorCPU = process.TotalProcessorTime.Ticks / (float)Stopwatch.Frequency / Environment.ProcessorCount;
                Console.WriteLine("CPU en uso: "+monitorCPU * 100);

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
                        if (disco.Name == "C:")
                        {
                            espacioC = (int)espacioUsado;
                        }
                    }
                }

                client.BaseAddress = new Uri("http://localhost:5021/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                MonitoreoServidor MS = new MonitoreoServidor()
                {
                    IdMonitoreo = 0,
                    IdServer = "S_W_1",
                    UsoCpu = (int)monitorCPU,
                    UsoMemoria = (int)monitorRAM,
                    UsoEspacio = (int)espacioC,
                    EstadoServer=1,
                    FechaMonitoreo= DateTime.UtcNow,
                    TimeOut=1
                };

                MS = await crearRegistro(MS);
                Console.WriteLine($"Registrado");

                //hilo duerme 10 seg
                Thread.Sleep(10000);
            }
        });

        h1.Start();
    }

    static async Task<MonitoreoServidor> crearRegistro(MonitoreoServidor c)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("paramSensibilidad", c);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<MonitoreoServidor>();
    }
    
}//fn class