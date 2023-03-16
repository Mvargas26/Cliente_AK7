using Cliente_AK7.Models;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading;
using System.Data.SqlClient;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static HttpClient client = new HttpClient();

   
    static void Main()
    {
        client.BaseAddress = new Uri("http://apiprogra.somee.com/");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (systemWimdos())
        {
            Console.WriteLine("Esta en Sistema Wimdows");
            MonitorRecursosSerividorWin();
            MonitorServicioWin();

            Console.ReadKey();
        }  else
        {
            Console.WriteLine("Esta en Sistema Linux");
            Thread usageThread = new Thread(MonitorRecursosSerividor_Linux);
            usageThread.Start();
        }




        
    }
    #region Windows
   
    static async void MonitorRecursosSerividorWin()
    {
        Process process = Process.GetCurrentProcess();

        try
        {
            Thread h1 = new Thread(async () =>
            {
                while (true)
                {
                    int espacioC = 0;

                    float monitorCPU = process.TotalProcessorTime.Ticks / (float)Stopwatch.Frequency / Environment.ProcessorCount;
                    Console.WriteLine("CPU en uso: " + monitorCPU * 100);

                    long monitorRAM = process.WorkingSet64;
                    Console.WriteLine("RAM en uso: {0:N0} MB", monitorRAM / 1024);

                    DriveInfo[] discos = DriveInfo.GetDrives();
                    foreach (DriveInfo disco in discos)
                    {
                        if (disco.IsReady)
                        {
                            long esapcioTotal = disco.TotalSize;
                            long espacioLibre = disco.TotalFreeSpace;
                            long espacioUsado = esapcioTotal - espacioLibre;
                            Console.WriteLine("{0} usado: {1:N0} bytes ({2:F2}%)", disco.Name, espacioUsado, (espacioUsado / (float)esapcioTotal) * 100);
                            espacioC = Convert.ToInt32(espacioUsado / 1024 / 1024 / 1024);

                        }
                    }
                    var currentTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"); ;

                    MonitoreoServidor MS = new MonitoreoServidor()
                    {
                        IdMonitoreo = 0,
                        IdServer = "S_W_1",
                        UsoCpu = Convert.ToInt32(monitorCPU * 100),
                        UsoMemoria = (int)monitorRAM / 1024,
                        UsoEspacio = (int)espacioC,
                        EstadoServer = 1,
                        FechaMonitoreo = DateTime.Parse(currentTime),
                        TimeOut = 3,
                        estadoParam = "normal"
                    };
                    MS = await crearRegistroServidor(MS);
                    Console.WriteLine($"Registrado Server");

                    //hilo duerme 3 min
                    Thread.Sleep(180000);
                }
            });

            h1.Start();
        }
        catch (Exception ex)
        {

           Console.WriteLine("Error monitoreo Recursos W");
        }
    }
    static async void MonitorServicioWin()
    {
        Process process = Process.GetCurrentProcess();

        try
        {
            Thread h2 = new Thread(async () =>
            {
                while (true)
                {

                    MonitoreoServicio ms = new MonitoreoServicio()
                    {
                        IdMonitoreo = 0,
                        IdServicio = "SVC1",
                        EstadoServicio = 0,
                        FechaMoniServicio = DateTime.Now,
                        TimeOutServicio = 3,
                        estadoParam = "alert"

                    };

                    if (conexionBDNortwhind())
                    {
                        ms.EstadoServicio = 1;
                        ms.estadoParam = "normal";
                    };

                    ms = await crearRegistroServicio(ms);
                    Console.WriteLine($"Servicio Registrado");

                    //hilo duerme 3 min
                    Thread.Sleep(180000);
                }
            });

            h2.Start();
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error en Monitoreo Servicios....: ");
        }
    }

   static private bool conexionBDNortwhind()
    {
        SqlConnection conexion = new SqlConnection("server=MVARGASPC\\INSTA12019 ; database=Northwind ; integrated security = true");
        bool conecta = false;

        try
        {
            conexion.Open();
            if (conexion.State > 0 )
            {
                conecta = true;
            }
            conexion.Close();

            return conecta;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error conexion con Northwind ");
            return false;
        }finally { conexion.Close(); }
    }

    static private bool systemWimdos()
    {
         bool esWimdows = false;

        try
        {
            string sistemaOpera = "";

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                        sistemaOpera= "Windows";
                    break;
                case PlatformID.Unix:
                    sistemaOpera= "Linux";
                    break;
                default:
                    sistemaOpera = "Unknown";
                    break;
            }

            if (sistemaOpera.Equals("Windows"))
            {
                esWimdows = true;
            }
            return esWimdows;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al comprobar sistema....");
           return esWimdows;
        }
    }

    #endregion

    #region Linux

    static async void MonitorRecursosSerividor_Linux()
    {
        while (true)
        {
            float CPUusado = usoCPULinux();
            Console.WriteLine("uso CPU : " + CPUusado + "%");

            float RAMusada = usoRAMlinux();
            Console.WriteLine("uso RAM : " + RAMusada + "%");

            float DISCOusado = usoDISCOlinux();
            Console.WriteLine("uso Disk : " + DISCOusado + "%");

            MonitoreoServidor MS = new MonitoreoServidor()
            {
                IdMonitoreo = 0,
                IdServer = "S_L_1",
                UsoCpu = Convert.ToInt32(CPUusado),
                UsoMemoria = (int)RAMusada,
                UsoEspacio = (int)DISCOusado,
                EstadoServer = 1,
                FechaMonitoreo = DateTime.Now,
                TimeOut = 3,
                estadoParam = "normal"
            };

            await crearRegistroServidorLinux(MS);
            Console.WriteLine($"Registrado Server");
            Thread.Sleep(10000);
        }
    }

    static float usoCPULinux()
    {
        ProcessStartInfo psi = new ProcessStartInfo("bash", "-c \"top -b -n1 | grep 'Cpu(s)'\"");
        psi.RedirectStandardOutput = true;
        Process proc = Process.Start(psi);
        string output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();

        float usoCPU = float.Parse(output.Split(new char[] { ' ', '%' }, StringSplitOptions.RemoveEmptyEntries)[1]);

        return usoCPU;
    }

    static float usoRAMlinux()
    {
        ProcessStartInfo psi = new ProcessStartInfo("bash", "-c \"free | grep Mem\"");
        psi.RedirectStandardOutput = true;
        Process proc = Process.Start(psi);
        string output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();

        string[] values = output.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        float total = float.Parse(values[1]);
        float used = float.Parse(values[2]);
        float usoRAM = used / total * 100;

        return usoRAM;
    }

    static float usoDISCOlinux()
    {
        ProcessStartInfo psi = new ProcessStartInfo("bash", "-c \"df -h /\"");
        psi.RedirectStandardOutput = true;
        Process proc = Process.Start(psi);
        string output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();

        // parse the disk usage from the output
        string[] values = output.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        float total = float.Parse(values[8].Replace("G", ""));
        float used = float.Parse(values[9].Replace("G", ""));
        float usoDISco = used / total * 100;

        return usoDISco;
    }
    #endregion

    #region Compartidos
    static async Task<MonitoreoServidor> crearRegistroServidor(MonitoreoServidor c)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("Monitoreo", c);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<MonitoreoServidor>();
    }
    static async Task<MonitoreoServicio> crearRegistroServicio(MonitoreoServicio c)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("monitoreoServicio", c);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsAsync<MonitoreoServicio>();
    }
    static async Task crearRegistroServidorLinux(MonitoreoServidor c)
    {
        using (var httpClient = new HttpClient())
        {
            var requestUrl = "http://apiprogra.somee.com/Monitoreo";

            var requestParams = c;

            var requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestParams);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            requestMessage.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(requestMessage);

            var responseJson = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseJson);
        }
    }
    #endregion
}//fn class