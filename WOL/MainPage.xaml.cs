using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace WOL;

public partial class MainPage : ContentPage
{
    private System.Timers.Timer statusCheckTimer;
    private bool isStatusCheckRunning = false;
    private DateTime _startTime { get; set; }
    public MainPageViewModel Vm { get; set; } = new();

    public MainPage()
	{
		InitializeComponent();
        BindingContext = Vm;

        // Initialize the timer
        statusCheckTimer = new System.Timers.Timer
        {
            Interval = TimeSpan.FromSeconds(2).TotalMilliseconds,
            AutoReset = true
        };
        statusCheckTimer.Elapsed += OnStatusCheckTimerElapsed;
    }

    private async void OnStatusCheckTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (isStatusCheckRunning)
        {
            return;
        }
        TimeSpan elapsedTime = DateTime.Now - _startTime;
        Vm.CurrentStatus = elapsedTime.ToString();

        if (elapsedTime.TotalSeconds > 80)
        {
            statusCheckTimer.Stop();
            Vm.IsLoaderOn = false;
            Vm.CurrentStatus = "Device is offline!";
            await Task.Delay(1000);
            Vm.CurrentStatus = "Application is shutting down...";
            await Task.Delay(3000);
            Application.Current.Quit();
        }

        try
        {
            isStatusCheckRunning = true;

            Vm.CurrentStatus = "Checking Device Status.";
            await Task.Delay(400);

            Uri uri = new(userHost.Text);
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            using (var client = new HttpClient())
            {
                Vm.CurrentStatus = "Checking Device Status..";
                await Task.Delay(400)
                    ;
                var response = await client.GetAsync(uri, cancellationTokenSource.Token);
                var responseMessage = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode && responseMessage.Contains("Home server"))
                {
                    statusCheckTimer.Stop();
                    Vm.IsDeviceOn = true;
                    Vm.IsLoaderOn = false;
                    Vm.IsVisibleStatusLabel = false;
                }
                else
                {
                    isStatusCheckRunning = false;
                    Vm.CurrentStatus = "Checking Device Status...";
                    await Task.Delay(300);
                }

            }
        }
        catch (Exception)
        {
            statusCheckTimer.Stop();
            Vm.CurrentStatus = "Failed to check Device Status";
            Vm.IsLoaderOn = false;
            await Task.Delay(600);

            Vm.CurrentStatus = "Application is shutting down...";
            await Task.Delay(4000);
            Application.Current.Quit();
        }
    }

    public async Task<bool> IsServerReachable()
    {
        try
        {
            Vm.CurrentStatus = $"Connecting to server...";
            await Task.Delay(400);

            Uri uri = new Uri(userHost.Text);
            string domain = uri.Host;
            IPAddress[] ipAddresses = Dns.GetHostAddresses(domain);

            if (ipAddresses.Length == 0)
            {
                Vm.CurrentStatus = $"Invalid Server!";
                await Task.Delay(500);

                return false;
            }

            IPAddress ipAddress = ipAddresses[0];

            Vm.CurrentStatus = "Pinging to server...";
            await Task.Delay(400);

            using (Ping ping = new Ping())
            {
                PingReply reply = ping.Send(ipAddress);

                if (reply != null && reply.Status == IPStatus.Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        catch (PingException)
        {
            return false;
        }
    }

    private void OnBtnClicked(object sender, EventArgs e)
	{
        Application.Current.Quit();
	}

    private async void OnPowerButtonClicked(object sender, EventArgs e)
	{
        if (powerButton.Source.ToString() == "File: power_on.png")
        {
            var isServerReachable = await IsServerReachable();

            if (isServerReachable)
            {
                Vm.CurrentStatus = "Server is online";
                await Task.Delay(400);

                Vm.CurrentStatus = "Checking Device Status...";
                await Task.Delay(400);

                try
                {
                    Uri uri = new(userHost.Text);
                    var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));

                    using (var client = new HttpClient())
                    {

                        var response = await client.GetAsync(uri, cancellationTokenSource.Token);
                        var responseMessage = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode && responseMessage.Contains("Home server"))
                        {
                            statusCheckTimer.Stop();
                            Vm.IsDeviceOn = true;
                            Vm.IsLoaderOn = false;
                            Vm.IsVisibleStatusLabel = false;
                            powerButton.Source = "power_off.png";
                        }
                        else
                        {
                            Vm.CurrentStatus = "Device Offline";
                            await Task.Delay(400);

                            powerButton.IsVisible = false;
                            Vm.IsLoaderOn = true;
                            SendMagicBytes();
                        }

                    }
                }
                catch (Exception)
                {
                    Vm.CurrentStatus = "Device Offline";
                    await Task.Delay(400);

                    powerButton.IsVisible = false;
                    Vm.IsLoaderOn = true;
                    SendMagicBytes();
                }                
            }
            else
            {
                Vm.CurrentStatus = "Server is not reachable.";
            }

        }
        else if (powerButton.Source.ToString() == "File: power_off.png")
        {
            await IntiateShutdown();
        }
        else { }
    }

	private async void SendMagicBytes()
	{
        Vm.CurrentStatus = "Magic Byte Initiated...";
        await Task.Delay(400);

        // Convert the MAC address to bytes
        var macBytes = PhysicalAddress.Parse(userMAC.Text).GetAddressBytes();

        // Create a magic packet (6 bytes of 0xFF followed by 16 repetitions of the MAC address)
        var magicPacket = new byte[102];

        for (var i = 0; i < 6; i++)
        {
            magicPacket[i] = 0xFF;
        }

        for (var i = 6; i < 102; i += 6)
        {
            for (var j = 0; j < 6; j++)
            {
                magicPacket[i + j] = macBytes[j];
            }
        }

        // Send the magic packet to the target IP address and port (usually 9)

        try
        {
            Vm.CurrentStatus = "Sending Packet...";
            await Task.Delay(400);

            for (int i = 0; i < 5; i++)
            {
                using (var client = new UdpClient())
                {
                    Uri uri = new Uri(userHost.Text);
                    string domain = uri.Host;

                    client.Send(magicPacket, magicPacket.Length, domain, 9);
                }
                Vm.CurrentStatus = "Packet Sent Successful";
                await Task.Delay(400);
            }

            statusCheckTimer.Start();
            _startTime = DateTime.Now;
        }
        catch (Exception)
        {
            Vm.CurrentStatus = "Failed to send packet...";
            Vm.IsLoaderOn = false;
            powerButton.IsVisible = true;
        }
    }

    private async Task IntiateShutdown()
    {
        try
        {
            using (var client = new HttpClient())
            {
                Vm.IsLoaderOn = true;
                Vm.CurrentStatus = "Initiating shutdown request...";
                await Task.Delay(400);

                client.DefaultRequestHeaders.Add("key", "default");

                var response = await client.PostAsync(userHost.Text, new StringContent(""));

                if (response.IsSuccessStatusCode)
                {
                    Vm.IsLoaderOn = false;
                    Vm.CurrentStatus = "Shutdown request success.!";
                    await Task.Delay(5000);

                    Vm.CurrentStatus = "Application is shutting down...";
                    await Task.Delay(5000);
                    Application.Current.Quit();
                }
                else
                {
                    Vm.CurrentStatus = "Failed to Initiate Shutdown!";
                    await Task.Delay(800);
                }

            }
        }
        catch (Exception)
        {
            Vm.CurrentStatus = "Failed to Initiate Shutdown!";
            await Task.Delay(800);
        }
        
    }
}
